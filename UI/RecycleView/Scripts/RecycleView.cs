
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class RecycleView : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Configuration")]
        [Tooltip("Mapping of item types to their prefabs. Each type must have a unique integer ID.")]
        [SerializeField] private List<RecycleViewItemType> itemTypeMappings;

        [Tooltip("Buffer of items to instantiate above and below the visible area to reduce pop-in.")]
        [SerializeField] private int viewBuffer = 2;

        [Header("Layout Settings")]
        [Tooltip("The layout mode for arranging items.")]
        [SerializeField] private LayoutMode layoutMode = LayoutMode.Vertical;
        
        [Tooltip("Padding between items in pixels.")]
        [SerializeField] private float itemPadding = 0f;

        [Header("References")]
        [Tooltip("The ScrollRect component.")]
        [SerializeField] private ScrollRect scrollRect;

        #endregion

        #region Public Events

        /// <summary>
        /// Callback invoked when an item in the view is clicked.
        /// </summary>
        public Action<RecycleViewItem> OnItemClicked;

        #endregion

        #region Private Fields

        private RectTransform _content;
        private readonly Dictionary<int, Queue<RecycleViewItem>> _itemPools = new Dictionary<int, Queue<RecycleViewItem>>();
        private readonly List<RecycleViewItem> _activeItems = new List<RecycleViewItem>();
        private List<IRecycleViewData> _dataList;
        private Dictionary<int, GameObject> _itemPrefabMap;
        private Dictionary<int, float> _itemTypeHeights = new Dictionary<int, float>();
        private Dictionary<int, float> _itemTypeWidths = new Dictionary<int, float>();
        private float[] _itemPositions; // Cache of item positions for performance
        private int _firstVisibleIndex = -1;
        private int _lastVisibleIndex = -1;
        private bool _isCalculatingPositions = false; // Flag to prevent multiple calculations
        private Coroutine _positionCalculationCoroutine;

        #endregion

        #region Enums & Serializable Classes

        public enum LayoutMode { Vertical, Horizontal }

        [Serializable]
        public class RecycleViewItemType
        {
            public int TypeId;
            public RecycleViewItem Prefab;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();

            _content = scrollRect.content;

            // Configure ScrollRect based on layout mode
            if (layoutMode == LayoutMode.Vertical)
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
            }
            else
            {
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
            }

            // Ensure content has correct anchor settings for consistent item positioning
            if (layoutMode == LayoutMode.Vertical)
            {
                // For vertical scrolling, anchor content to top
                _content.anchorMin = new Vector2(0, 1);
                _content.anchorMax = new Vector2(1, 1);
                _content.pivot = new Vector2(0.5f, 1);
            }
            else
            {
                // For horizontal scrolling, anchor content to left
                _content.anchorMin = new Vector2(0, 0);
                _content.anchorMax = new Vector2(0, 1);
                _content.pivot = new Vector2(0, 0.5f);
            }

            // Initialize prefab map and auto-detect sizes from prefabs
            _itemPrefabMap = new Dictionary<int, GameObject>();
            foreach (var mapping in itemTypeMappings)
            {
                if (mapping.Prefab == null)
                {
                    Debug.LogError($"RecycleView: Prefab for TypeId {mapping.TypeId} is not assigned.");
                    continue;
                }
                if (!_itemPrefabMap.ContainsKey(mapping.TypeId))
                {
                    _itemPrefabMap.Add(mapping.TypeId, mapping.Prefab.gameObject);
                    
                    // Auto-detect dimensions from prefab RectTransform
                    var prefabRect = mapping.Prefab.GetComponent<RectTransform>();
                    if (prefabRect != null)
                    {
                        float itemHeight = prefabRect.sizeDelta.y > 0 ? prefabRect.sizeDelta.y : 100f; // Use fixed fallback
                        float itemWidth = prefabRect.sizeDelta.x > 0 ? prefabRect.sizeDelta.x : 100f; // Use fixed fallback
                        
                        _itemTypeHeights[mapping.TypeId] = itemHeight;
                        _itemTypeWidths[mapping.TypeId] = itemWidth;
                    }
                    else
                    {
                        // Fallback to default dimensions
                        _itemTypeHeights[mapping.TypeId] = 100f; // Use fixed fallback
                        _itemTypeWidths[mapping.TypeId] = 100f; // Use fixed fallback
                    }
                }
                else
                {
                    Debug.LogWarning($"RecycleView: Duplicate TypeId {mapping.TypeId} found. Only the first one will be used.");
                }
            }
        }

        private void OnEnable()
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void OnDisable()
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the padding between items. Rebuilds the list when changed.
        /// </summary>
        public float ItemPadding
        {
            get => itemPadding;
            set
            {
                if (Math.Abs(itemPadding - value) > 0.001f)
                {
                    itemPadding = value;
                    if (_dataList != null && _dataList.Count > 0)
                    {
                        Rebuild();
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the data for the RecycleView and rebuilds the list.
        /// </summary>
        public void SetData(List<IRecycleViewData> data)
        {
            _dataList = data;
            Rebuild();
        }

        /// <summary>
        /// Refreshes the visible items with their current data. Does not rebuild the entire list.
        /// </summary>
        public void Refresh()
        {
            UpdateVisibleItems();
        }

        /// <summary>
        /// Sets the padding between items and rebuilds the list.
        /// </summary>
        /// <param name="padding">The new padding value in pixels.</param>
        public void SetItemPadding(float padding)
        {
            ItemPadding = padding;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the height of an item at the specified index.
        /// </summary>
        private float GetItemHeight(int index)
        {
            if (_dataList == null || index < 0 || index >= _dataList.Count)
                return 100f; // Use fixed fallback

            var data = _dataList[index];
                
            // Use item type height (auto-detected from prefab)
            if (_itemTypeHeights.ContainsKey(data.ItemType))
                return _itemTypeHeights[data.ItemType];
                
            return 100f; // Use fixed fallback
        }

        /// <summary>
        /// Get the width of an item at the specified index.
        /// </summary>
        private float GetItemWidth(int index)
        {
            if (_dataList == null || index < 0 || index >= _dataList.Count)
                return 100f; // Use fixed fallback

            var data = _dataList[index];
            
            // Use item type width
            if (_itemTypeWidths.ContainsKey(data.ItemType))
                return _itemTypeWidths[data.ItemType];
                
            return 100f; // Use fixed fallback
        }

        /// <summary>
        /// Pre-calculate all item positions for performance.
        /// Uses coroutine for large datasets to prevent frame drops.
        /// </summary>
        private void CalculateItemPositions()
        {
            if (_dataList == null || _dataList.Count == 0)
            {
                _itemPositions = new float[0];
                return;
            }

            // Prevent multiple simultaneous calculations
            if (_isCalculatingPositions)
            {
                if (_positionCalculationCoroutine != null)
                {
                    StopCoroutine(_positionCalculationCoroutine);
                }
            }

            // For large datasets, use coroutine to spread calculation across frames
            if (_dataList.Count > 1000)
            {
                _isCalculatingPositions = true;
                _positionCalculationCoroutine = StartCoroutine(CalculateItemPositionsCoroutine());
            }
            else
            {
                CalculateItemPositionsImmediate();
            }
        }

        /// <summary>
        /// Calculate positions immediately for small datasets.
        /// </summary>
        private void CalculateItemPositionsImmediate()
        {
            _itemPositions = new float[_dataList.Count];
            _itemPositions[0] = 0;

            for (int i = 1; i < _dataList.Count; i++)
            {
                if (layoutMode == LayoutMode.Vertical)
                {
                    _itemPositions[i] = _itemPositions[i - 1] + GetItemHeight(i - 1) + itemPadding;
                }
                else
                {
                    _itemPositions[i] = _itemPositions[i - 1] + GetItemWidth(i - 1) + itemPadding;
                }
            }

            // Debug log for position verification
            if (_dataList.Count <= 10) // Only log for small lists to avoid spam
            {
                LogItemPositions();
            }
        }

        /// <summary>
        /// Calculate positions using coroutine to spread work across frames for large datasets.
        /// </summary>
        private System.Collections.IEnumerator CalculateItemPositionsCoroutine()
        {
            _itemPositions = new float[_dataList.Count];
            _itemPositions[0] = 0;

            const int itemsPerFrame = 200; // Process 200 items per frame
            int processedCount = 1; // Start from 1 since position[0] = 0

            while (processedCount < _dataList.Count)
            {
                int endIndex = Mathf.Min(processedCount + itemsPerFrame, _dataList.Count);
                
                for (int i = processedCount; i < endIndex; i++)
                {
                    if (layoutMode == LayoutMode.Vertical)
                    {
                        _itemPositions[i] = _itemPositions[i - 1] + GetItemHeight(i - 1) + itemPadding;
                    }
                    else
                    {
                        _itemPositions[i] = _itemPositions[i - 1] + GetItemWidth(i - 1) + itemPadding;
                    }
                }

                processedCount = endIndex;
                
                // Yield control back to Unity for one frame
                yield return null;
            }

            // Mark calculation as complete
            _isCalculatingPositions = false;
            _positionCalculationCoroutine = null;

            // Update content size and visible items once calculation is complete
            float totalSize = CalculateTotalContentSize();
            if (layoutMode == LayoutMode.Vertical)
                _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalSize);
            else
                _content.sizeDelta = new Vector2(totalSize, _content.sizeDelta.y);

            OnScroll(Vector2.zero);
        }

        /// <summary>
        /// Helper method to log item positions for debugging.
        /// </summary>
        private void LogItemPositions()
        {
            for (int i = 0; i < _dataList.Count; i++)
            {
                if (layoutMode == LayoutMode.Vertical)
                {
                    Debug.Log($"RecycleView: Item {i} - Type: {_dataList[i].ItemType}, Height: {GetItemHeight(i)}, Position: {_itemPositions[i]}, Padding: {itemPadding}");
                }
                else
                {
                    Debug.Log($"RecycleView: Item {i} - Type: {_dataList[i].ItemType}, Width: {GetItemWidth(i)}, Position: {_itemPositions[i]}, Padding: {itemPadding}");
                }
            }
        }

        /// <summary>
        /// Get the position of an item at the specified index.
        /// </summary>
        private float GetItemPosition(int index)
        {
            if (_itemPositions == null || index < 0 || index >= _itemPositions.Length)
                return 0;
            return _itemPositions[index];
        }

        /// <summary>
        /// Calculate total content size.
        /// </summary>
        private float CalculateTotalContentSize()
        {
            if (_dataList == null || _dataList.Count == 0)
                return 0;

            if (layoutMode == LayoutMode.Vertical)
            {
                return GetItemPosition(_dataList.Count - 1) + GetItemHeight(_dataList.Count - 1);
            }
            else
            {
                return GetItemPosition(_dataList.Count - 1) + GetItemWidth(_dataList.Count - 1);
            }
        }

        #endregion

        #region Core Logic

        private void Rebuild()
        {
            // Return all active items to the pool
            foreach (var item in _activeItems)
            {
                ReturnItemToPool(item);
            }
            _activeItems.Clear();

            // Reset indices
            _firstVisibleIndex = -1;
            _lastVisibleIndex = -1;

            // Calculate item positions (may use coroutine for large datasets)
            CalculateItemPositions();

            // For small datasets, update content size and show items immediately
            if (_dataList != null && _dataList.Count <= 1000)
            {
                float totalSize = CalculateTotalContentSize();
                
                if (layoutMode == LayoutMode.Vertical)
                    _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalSize);
                else
                    _content.sizeDelta = new Vector2(totalSize, _content.sizeDelta.y);

                OnScroll(Vector2.zero);
            }
            // For large datasets, content size update and item display will be handled by coroutine
        }

        private void OnScroll(Vector2 scrollPos)
        {
            if (_dataList == null || _dataList.Count == 0)
                return;

            UpdateVisibleItems();
        }

        private void UpdateVisibleItems()
        {
            // Determine the range of data indices that should be visible
            int newFirstVisibleIndex, newLastVisibleIndex;
            if (layoutMode == LayoutMode.Vertical)
            {
                float contentY = _content.anchoredPosition.y;
                float viewportHeight = scrollRect.viewport.rect.height;
                
                // Find first visible index using binary search for performance
                newFirstVisibleIndex = FindFirstVisibleIndex(contentY) - viewBuffer;
                newFirstVisibleIndex = Mathf.Max(0, newFirstVisibleIndex);
                
                // Find last visible index
                newLastVisibleIndex = FindLastVisibleIndex(contentY + viewportHeight) + viewBuffer;
                newLastVisibleIndex = Mathf.Min(_dataList.Count - 1, newLastVisibleIndex);
            }
            else // Horizontal
            {
                float contentX = -_content.anchoredPosition.x;
                float viewportWidth = scrollRect.viewport.rect.width;
                
                // Find first visible index using binary search for performance
                newFirstVisibleIndex = FindFirstVisibleIndexHorizontal(contentX) - viewBuffer;
                newFirstVisibleIndex = Mathf.Max(0, newFirstVisibleIndex);
                
                // Find last visible index
                newLastVisibleIndex = FindLastVisibleIndexHorizontal(contentX + viewportWidth) + viewBuffer;
                newLastVisibleIndex = Mathf.Min(_dataList.Count - 1, newLastVisibleIndex);
            }

            // No change, no work
            if (newFirstVisibleIndex == _firstVisibleIndex && newLastVisibleIndex == _lastVisibleIndex)
                return;

            _firstVisibleIndex = newFirstVisibleIndex;
            _lastVisibleIndex = newLastVisibleIndex;

            // Recycle items that are no longer visible
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                var item = _activeItems[i];
                if (item.CurrentDataIndex < _firstVisibleIndex || item.CurrentDataIndex > _lastVisibleIndex)
                {
                    ReturnItemToPool(item);
                    _activeItems.RemoveAt(i);
                }
            }

            // Activate and position items that are now visible
            for (int i = _firstVisibleIndex; i <= _lastVisibleIndex; i++)
            {
                if (_activeItems.Exists(item => item.CurrentDataIndex == i))
                    continue; // Already active

                var data = _dataList[i];
                var item = GetItemFromPool(data.ItemType);

                // Ensure correct anchor and pivot settings for consistent positioning
                var itemRect = ((RectTransform)item.transform);
                
                // Position using calculated positions
                if (layoutMode == LayoutMode.Vertical)
                {
                    // Set anchor to top-left for vertical layout
                    itemRect.anchorMin = new Vector2(0, 1);
                    itemRect.anchorMax = new Vector2(1, 1);
                    itemRect.pivot = new Vector2(0.5f, 1);
                    
                    // Position from top, accounting for the item's height
                    itemRect.anchoredPosition = new Vector2(0, -GetItemPosition(i));
                    
                    // Ensure the item has the correct size
                    float itemHeight = GetItemHeight(i);
                    itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemHeight);
                }
                else
                {
                    // Set anchor to top-left for horizontal layout
                    itemRect.anchorMin = new Vector2(0, 0);
                    itemRect.anchorMax = new Vector2(0, 1);
                    itemRect.pivot = new Vector2(0, 0.5f);
                    
                    itemRect.anchoredPosition = new Vector2(GetItemPosition(i), 0);
                    
                    // Ensure the item has the correct size
                    float itemWidth = GetItemWidth(i);
                    itemRect.sizeDelta = new Vector2(itemWidth, itemRect.sizeDelta.y);
                }

                item.BindData(data, i);
                _activeItems.Add(item);
            }
        }

        /// <summary>
        /// Find the first visible item index using binary search for vertical layout.
        /// </summary>
        private int FindFirstVisibleIndex(float scrollPosition)
        {
            if (_itemPositions == null || _itemPositions.Length == 0)
                return 0;

            int left = 0;
            int right = _itemPositions.Length - 1;
            int result = 0;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                
                if (_itemPositions[mid] <= scrollPosition)
                {
                    result = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Find the last visible item index for vertical layout.
        /// </summary>
        private int FindLastVisibleIndex(float scrollEndPosition)
        {
            if (_itemPositions == null || _itemPositions.Length == 0)
                return 0;

            for (int i = 0; i < _itemPositions.Length; i++)
            {
                if (_itemPositions[i] > scrollEndPosition)
                {
                    return Mathf.Max(0, i - 1);
                }
            }

            return _itemPositions.Length - 1;
        }

        /// <summary>
        /// Find the first visible item index using binary search for horizontal layout.
        /// </summary>
        private int FindFirstVisibleIndexHorizontal(float scrollPosition)
        {
            return FindFirstVisibleIndex(scrollPosition); // Same logic for horizontal
        }

        /// <summary>
        /// Find the last visible item index for horizontal layout.
        /// </summary>
        private int FindLastVisibleIndexHorizontal(float scrollEndPosition)
        {
            return FindLastVisibleIndex(scrollEndPosition); // Same logic for horizontal
        }

        #endregion

        #region Pooling

        private RecycleViewItem GetItemFromPool(int itemType)
        {
            if (!_itemPools.ContainsKey(itemType))
            {
                _itemPools[itemType] = new Queue<RecycleViewItem>();
            }

            RecycleViewItem item;
            if (_itemPools[itemType].Count > 0)
            {
                item = _itemPools[itemType].Dequeue();
            }
            else
            {
                if (!_itemPrefabMap.ContainsKey(itemType))
                {
                    Debug.LogError($"RecycleView: No prefab registered for ItemType {itemType}. Cannot create new item.");
                    // Return a dummy or handle error appropriately
                    return null; // This will cause a NullReferenceException downstream, highlighting the configuration error.
                }
                var prefab = _itemPrefabMap[itemType];
                var instance = Instantiate(prefab, _content);
                item = instance.GetComponent<RecycleViewItem>();
                item.Initialize(this);
                
                // Ensure newly created items have consistent anchor settings
                var itemRect = instance.GetComponent<RectTransform>();
                if (layoutMode == LayoutMode.Vertical)
                {
                    itemRect.anchorMin = new Vector2(0, 1);
                    itemRect.anchorMax = new Vector2(1, 1);
                    itemRect.pivot = new Vector2(0.5f, 1);
                }
                else
                {
                    itemRect.anchorMin = new Vector2(0, 0);
                    itemRect.anchorMax = new Vector2(0, 1);
                    itemRect.pivot = new Vector2(0, 0.5f);
                }
            }

            item.gameObject.SetActive(true);
            return item;
        }

        private void ReturnItemToPool(RecycleViewItem item)
        {
            if (item == null)
                return;

            var data = _dataList[item.CurrentDataIndex];
            if (!_itemPools.ContainsKey(data.ItemType))
            {
                _itemPools[data.ItemType] = new Queue<RecycleViewItem>();
            }

            item.gameObject.SetActive(false);
            _itemPools[data.ItemType].Enqueue(item);
        }

        #endregion

        #region Public API Extensions

        /// <summary>
        /// Refreshes a specific item by index. This will update the visual representation
        /// of the item if it's currently visible.
        /// </summary>
        /// <param name="index">The index of the item to refresh</param>
        public void RefreshItem(int index)
        {
            if (_dataList == null || index < 0 || index >= _dataList.Count)
            {
                Debug.LogWarning($"Cannot refresh item at index {index}: index out of bounds");
                return;
            }

            // Find the active item with this index
            foreach (var activeItem in _activeItems)
            {
                if (activeItem.CurrentDataIndex == index)
                {
                    var data = _dataList[index];
                    activeItem.BindData(data, index);
                    break;
                }
            }
        }

        /// <summary>
        /// Refreshes all currently visible items. Useful when data has been modified
        /// and you want to ensure all visible items reflect the latest state.
        /// </summary>
        public void RefreshAllVisibleItems()
        {
            foreach (var activeItem in _activeItems)
            {
                if (activeItem.CurrentDataIndex >= 0 && activeItem.CurrentDataIndex < _dataList.Count)
                {
                    var data = _dataList[activeItem.CurrentDataIndex];
                    activeItem.BindData(data, activeItem.CurrentDataIndex);
                }
            }
        }

        #endregion
    }
}
