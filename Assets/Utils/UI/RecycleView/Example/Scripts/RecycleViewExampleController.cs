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

        private List<IRecycleViewData> exampleDataList;

        private void Start()
        {
            if (recycleView == null)
            {
                Debug.LogError("RecycleView reference is not set!");
                return;
            }

            // Generate a large list of mixed data types with varying heights
            exampleDataList = new List<IRecycleViewData>();
            for (int i = 0; i < dataCount; i++)
            {
                if (i % 5 == 0) // Every 5th item is an image message
                {
                    exampleDataList.Add(new ImageMessageData
                    {
                        Image = sampleSprite,
                        Caption = $"Image item {i}",
                        CustomHeight = 100f + (i % 2) * 30f // Varying heights: 100, 130
                    });
                }
                else // Other items are text messages
                {
                    // Some text messages have custom heights for variety
                    string message = i % 3 == 0 ? "Short message" : $"This is a longer text message at index {i} with more content to display.";
                    exampleDataList.Add(new TextMessageData
                    {
                        Message = message,
                        CustomHeight = i % 4 == 0 ? (80f + (i % 3) * 20f) : -1f // Some items have custom height: 80, 100, 120
                    });
                }
            }

            // Set the data to the RecycleView
            recycleView.SetData(exampleDataList);

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
            Debug.Log($"Clicked on item at index: {item.CurrentDataIndex}");
            // You can access the specific data like this:
            var data = exampleDataList[item.CurrentDataIndex];
            
            if (data is TextMessageData textData)
            {
                Debug.Log($"Text Item Message: {textData.Message}");
            }
            else if (data is ImageMessageData imageData)
            {
                Debug.Log($"Image Item Caption: {imageData.Caption}");
            }
        }
    }
}
