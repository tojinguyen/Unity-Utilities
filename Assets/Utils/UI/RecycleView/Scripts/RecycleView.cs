
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

        [Tooltip("The height of a single item. Required for vertical linear layout.")]
        [SerializeField] private float itemHeight = 100f;

        [Tooltip("The width of a single item. Required for horizontal linear layout.")]
        [SerializeField] private float itemWidth = 100f;

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
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (scrollRect == null)
                scrollRect = GetComponent<ScrollRect>();

            _content = scrollRect.content;

            // Initialize prefab map for quick lookups
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

            // Set content size
            float totalSize = 0;
            if (_dataList != null)
            {
                totalSize = layoutMode == LayoutMode.Vertical ? _dataList.Count * itemHeight : _dataList.Count * itemWidth;
            }

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
                newFirstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(contentY / itemHeight) - viewBuffer);
                float viewportHeight = scrollRect.viewport.rect.height;
                newLastVisibleIndex = Mathf.Min(_dataList.Count - 1, Mathf.CeilToInt((contentY + viewportHeight) / itemHeight) + viewBuffer -1);
            }
            else // Horizontal
            {
                float contentX = -_content.anchoredPosition.x;
                newFirstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(contentX / itemWidth) - viewBuffer);
                float viewportWidth = scrollRect.viewport.rect.width;
                newLastVisibleIndex = Mathf.Min(_dataList.Count - 1, Mathf.CeilToInt((contentX + viewportWidth) / itemWidth) + viewBuffer -1);
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

                // Position and bind data
                if (layoutMode == LayoutMode.Vertical)
                    ((RectTransform)item.transform).anchoredPosition = new Vector2(0, -i * itemHeight);
                else
                    ((RectTransform)item.transform).anchoredPosition = new Vector2(i * itemWidth, 0);

                item.BindData(data, i);
                _activeItems.Add(item);
            }
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
