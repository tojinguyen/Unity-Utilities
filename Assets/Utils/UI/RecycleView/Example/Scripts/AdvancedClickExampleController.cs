using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Advanced example showing different ways to handle item clicks
    /// </summary>
    public class AdvancedClickExampleController : MonoBehaviour
    {
        [SerializeField] private RecycleView recycleView;
        [SerializeField] private Button clearSelectionsButton;
        [SerializeField] private Button selectAllButton;
        [SerializeField] private Button deleteSelectedButton;
        [SerializeField] private Text selectionCountText;
        [SerializeField] private Sprite sampleSprite;

        private List<IRecycleViewData> exampleDataList;
        private HashSet<int> selectedIndices = new HashSet<int>();

        private void Start()
        {
            SetupUI();
            GenerateData();
            recycleView.SetData(exampleDataList);
            recycleView.OnItemClicked += HandleItemClicked;
            UpdateSelectionUI();
        }

        private void OnDestroy()
        {
            if (recycleView != null)
            {
                recycleView.OnItemClicked -= HandleItemClicked;
            }
        }

        private void SetupUI()
        {
            clearSelectionsButton.onClick.AddListener(ClearAllSelections);
            selectAllButton.onClick.AddListener(SelectAllItems);
            deleteSelectedButton.onClick.AddListener(DeleteSelectedItems);
        }

        private void GenerateData()
        {
            exampleDataList = new List<IRecycleViewData>();
            
            for (int i = 0; i < 100; i++)
            {
                if (i % 4 == 0)
                {
                    exampleDataList.Add(new SelectableImageData
                    {
                        Image = sampleSprite,
                        Caption = $"Image {i}",
                        IsSelected = false,
                        ItemId = i
                    });
                }
                else
                {
                    exampleDataList.Add(new SelectableTextData
                    {
                        Message = $"Text message {i}",
                        IsSelected = false,
                        ItemId = i
                    });
                }
            }
        }

        private void HandleItemClicked(RecycleViewItem item)
        {
            var index = item.CurrentDataIndex;
            var data = exampleDataList[index];

            // Toggle selection
            if (data is ISelectableData selectableData)
            {
                selectableData.IsSelected = !selectableData.IsSelected;
                
                if (selectableData.IsSelected)
                {
                    selectedIndices.Add(index);
                }
                else
                {
                    selectedIndices.Remove(index);
                }

                // Refresh the clicked item to show selection state
                recycleView.RefreshItem(index);
                UpdateSelectionUI();

                Debug.Log($"Item {index} {(selectableData.IsSelected ? "selected" : "deselected")}. Total selected: {selectedIndices.Count}");
            }
        }

        private void ClearAllSelections()
        {
            selectedIndices.Clear();
            
            foreach (var data in exampleDataList)
            {
                if (data is ISelectableData selectableData)
                {
                    selectableData.IsSelected = false;
                }
            }
            
            recycleView.RefreshAllVisibleItems();
            UpdateSelectionUI();
        }

        private void SelectAllItems()
        {
            selectedIndices.Clear();
            
            for (int i = 0; i < exampleDataList.Count; i++)
            {
                if (exampleDataList[i] is ISelectableData selectableData)
                {
                    selectableData.IsSelected = true;
                    selectedIndices.Add(i);
                }
            }
            
            recycleView.RefreshAllVisibleItems();
            UpdateSelectionUI();
        }

        private void DeleteSelectedItems()
        {
            if (selectedIndices.Count == 0)
            {
                Debug.Log("No items selected for deletion");
                return;
            }

            // Sort indices in descending order to avoid index shifting issues
            var sortedIndices = new List<int>(selectedIndices);
            sortedIndices.Sort((a, b) => b.CompareTo(a));

            foreach (var index in sortedIndices)
            {
                exampleDataList.RemoveAt(index);
            }

            selectedIndices.Clear();
            
            // Update data and refresh view
            recycleView.SetData(exampleDataList);
            UpdateSelectionUI();
            
            Debug.Log($"Deleted {sortedIndices.Count} items");
        }

        private void UpdateSelectionUI()
        {
            selectionCountText.text = $"Selected: {selectedIndices.Count}/{exampleDataList.Count}";
            deleteSelectedButton.interactable = selectedIndices.Count > 0;
        }
    }

    // Interface for selectable items
    public interface ISelectableData
    {
        bool IsSelected { get; set; }
        int ItemId { get; set; }
    }

    // Extended text data with selection
    [System.Serializable]
    public class SelectableTextData : TextMessageData, ISelectableData
    {
        public bool IsSelected { get; set; }
        public int ItemId { get; set; }
    }

    // Extended image data with selection
    [System.Serializable]
    public class SelectableImageData : ImageMessageData, ISelectableData
    {
        public bool IsSelected { get; set; }
        public int ItemId { get; set; }
    }
}