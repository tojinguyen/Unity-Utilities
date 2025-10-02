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
                if (i % 10 == 0) // Every 10th item is a large text message
                {
                    exampleDataList.Add(new LargeTextMessageData
                    {
                        Message = $"This is a large text message with more content at index {i}. It should have a taller height to accommodate more text content.",
                        CustomHeight = 120f + (i % 3) * 20f // Varying heights: 120, 140, 160
                    });
                }
                else if (i % 5 == 0) // Every 5th item is an image message
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
                    exampleDataList.Add(new TextMessageData
                    {
                        Message = $"Text message {i}",
                        CustomHeight = i % 4 == 0 ? 80f : -1f // Some items have custom height, others use default
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
            else if (data is LargeTextMessageData largeTextData)
            {
                Debug.Log($"Large Text Item Message: {largeTextData.Message}");
            }
        }
    }
}
