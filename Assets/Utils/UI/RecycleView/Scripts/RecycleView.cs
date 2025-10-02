
using System;
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

        [Tooltip("The default height of a single item. Used when no custom height is specified.")]
        [SerializeField] private float defaultItemHeight = 100f;

        [Tooltip("The default width of a single item. Used when no custom width is specified.")]
        [SerializeField] private float defaultItemWidth = 100f;

        [Tooltip("Buffer of items to instantiate above and below the visible area to reduce pop-in.")]
        [SerializeField] private int viewBuffer = 2;

        [Header("Layout Settings")]
        [Tooltip("The layout mode for arranging items.")]
        [SerializeField] private LayoutMode layoutMode = LayoutMode.Vertical;

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

        #endregion

        #region Enums & Serializable Classes

        public enum LayoutMode { Vertical, Horizontal }

        [Serializable]
        public class RecycleViewItemType
        {
            public int TypeId;
            public RecycleViewItem Prefab;
            [Tooltip("Custom height for this item type. Use -1 to use default height.")]
            public float CustomHeight = -1f;
            [Tooltip("Custom width for this item type. Use -1 to use default width.")]
            public float CustomWidth = -1f;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();

            _content = scrollRect.content;

            // Initialize prefab map and size maps for quick lookups
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
                    
                    // Set item type dimensions
                    float itemHeight = mapping.CustomHeight > 0 ? mapping.CustomHeight : defaultItemHeight;
                    float itemWidth = mapping.CustomWidth > 0 ? mapping.CustomWidth : defaultItemWidth;
                    
                    _itemTypeHeights[mapping.TypeId] = itemHeight;
                    _itemTypeWidths[mapping.TypeId] = itemWidth;
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the height of an item at the specified index.
        /// </summary>
        private float GetItemHeight(int index)
        {
            if (_dataList == null || index < 0 || index >= _dataList.Count)
                return defaultItemHeight;

            var data = _dataList[index];
            
            // Check if data has custom height
            if (data.CustomHeight > 0)
                return data.CustomHeight;
                
            // Use item type height
            if (_itemTypeHeights.ContainsKey(data.ItemType))
                return _itemTypeHeights[data.ItemType];
                
            return defaultItemHeight;
        }

        /// <summary>
        /// Get the width of an item at the specified index.
        /// </summary>
        private float GetItemWidth(int index)
        {
            if (_dataList == null || index < 0 || index >= _dataList.Count)
                return defaultItemWidth;

            var data = _dataList[index];
            
            // Use item type width
            if (_itemTypeWidths.ContainsKey(data.ItemType))
                return _itemTypeWidths[data.ItemType];
                
            return defaultItemWidth;
        }

        /// <summary>
        /// Pre-calculate all item positions for performance.
        /// </summary>
        private void CalculateItemPositions()
        {
            if (_dataList == null || _dataList.Count == 0)
            {
                _itemPositions = new float[0];
                return;
            }

            _itemPositions = new float[_dataList.Count];
            _itemPositions[0] = 0;

            for (int i = 1; i < _dataList.Count; i++)
            {
                if (layoutMode == LayoutMode.Vertical)
                {
                    _itemPositions[i] = _itemPositions[i - 1] + GetItemHeight(i - 1);
                }
                else
                {
                    _itemPositions[i] = _itemPositions[i - 1] + GetItemWidth(i - 1);
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

            // Calculate item positions and total content size
            CalculateItemPositions();
            float totalSize = CalculateTotalContentSize();

            // Set content size
            if (layoutMode == LayoutMode.Vertical)
                _content.sizeDelta = new Vector2(_content.sizeDelta.x, totalSize);
            else
                _content.sizeDelta = new Vector2(totalSize, _content.sizeDelta.y);

            // Immediately update to show initial items
            OnScroll(Vector2.zero);
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

                // Position using calculated positions
                if (layoutMode == LayoutMode.Vertical)
                    ((RectTransform)item.transform).anchoredPosition = new Vector2(0, -GetItemPosition(i));
                else
                    ((RectTransform)item.transform).anchoredPosition = new Vector2(GetItemPosition(i), 0);

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
    }
}
