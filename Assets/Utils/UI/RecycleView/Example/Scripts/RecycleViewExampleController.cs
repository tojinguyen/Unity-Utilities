using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Drives the RecycleView example by generating dummy data.
    /// </summary>
    public class RecycleViewExampleController : MonoBehaviour
    {
        [SerializeField] private RecycleView recycleView;
        [SerializeField] private int dataCount = 1000;
        [SerializeField] private Sprite sampleSprite; // Assign a sample sprite in the inspector
        [SerializeField] private float itemPadding = 5f; // Padding between items

        private List<IRecycleViewData> exampleDataList;

        private void Start()
        {
            if (recycleView == null)
            {
                Debug.LogError("RecycleView reference is not set!");
                return;
            }

            // Generate a large list of mixed data types
            exampleDataList = new List<IRecycleViewData>();
            for (int i = 0; i < dataCount; i++)
            {
                if (i % 5 == 0) // Every 5th item is an image message
                {
                    exampleDataList.Add(new ImageMessageData
                    {
                        Image = sampleSprite,
                        Caption = $"Image item {i}"
                    });
                }
                else // Other items are text messages
                {
                    string message = i % 3 == 0 ? "Short message" : $"This is a longer text message at index {i} with more content to display.";
                    exampleDataList.Add(new TextMessageData
                    {
                        Message = message
                    });
                }
            }

            // Set the data to the RecycleView
            recycleView.SetData(exampleDataList);

            // Set the padding between items
            recycleView.SetItemPadding(itemPadding);

            // Subscribe to the item click event
            recycleView.OnItemClicked += HandleItemClicked;
        }

        private void OnDestroy()
        {
            if (recycleView != null)
            {
                recycleView.OnItemClicked -= HandleItemClicked;
            }
        }

        private void HandleItemClicked(RecycleViewItem item)
        {
            Debug.Log($"=== Item Clicked at Index: {item.CurrentDataIndex} ===");
            
            // You can access the specific data like this:
            var data = exampleDataList[item.CurrentDataIndex];
            
            if (data is TextMessageData textData)
            {
                Debug.Log($"Text Item Message: {textData.Message}");
                // Example: Could open an edit dialog, copy to clipboard, etc.
                HandleTextItemClick(textData, item.CurrentDataIndex);
            }
            else if (data is ImageMessageData imageData)
            {
                Debug.Log($"Image Item Caption: {imageData.Caption}");
                // Example: Could open image in fullscreen, show details, etc.
                HandleImageItemClick(imageData, item.CurrentDataIndex);
            }
        }
        
        private void HandleTextItemClick(TextMessageData textData, int index)
        {
            // Example specific handling for text items
            Debug.Log($"Handling text item click - could implement: edit text, copy message, etc.");
            
            // Example: Change the message text
            if (index % 10 == 0) // Every 10th item gets a special message
            {
                textData.Message = $"CLICKED! Original was at index {index}";
                // Refresh the item to show updated data
                recycleView.RefreshItem(index);
            }
        }
        
        private void HandleImageItemClick(ImageMessageData imageData, int index)
        {
            // Example specific handling for image items
            Debug.Log($"Handling image item click - could implement: fullscreen view, image editor, etc.");
            
            // Example: Update the caption
            imageData.Caption += " [CLICKED]";
            // Refresh the item to show updated data
            recycleView.RefreshItem(index);
        }

        /// <summary>
        /// Example method to demonstrate changing padding at runtime.
        /// Can be called from UI buttons or other events.
        /// </summary>
        public void IncreasePadding()
        {
            itemPadding += 5f;
            recycleView.SetItemPadding(itemPadding);
            Debug.Log($"Increased padding to: {itemPadding}");
        }

        /// <summary>
        /// Example method to demonstrate changing padding at runtime.
        /// Can be called from UI buttons or other events.
        /// </summary>
        public void DecreasePadding()
        {
            itemPadding = Mathf.Max(0f, itemPadding - 5f);
            recycleView.SetItemPadding(itemPadding);
            Debug.Log($"Decreased padding to: {itemPadding}");
        }

        /// <summary>
        /// Set a specific padding value.
        /// </summary>
        public void SetPadding(float padding)
        {
            itemPadding = Mathf.Max(0f, padding);
            recycleView.SetItemPadding(itemPadding);
            Debug.Log($"Set padding to: {itemPadding}");
        }
    }
}
