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

        private void Start()
        {
            if (recycleView == null)
            {
                Debug.LogError("RecycleView reference is not set!");
                return;
            }

            // Generate a large list of mixed data types
            var exampleDataList = new List<IRecycleViewData>();
            for (int i = 0; i < dataCount; i++)
            {
                if (i % 5 == 0) // Every 5th item is an image message
                {
                    exampleDataList.Add(new ImageMessageData
                    {
                        Image = sampleSprite,
                        Caption = "This is an image item"
                    });
                }
                else // Other items are text messages
                {
                    exampleDataList.Add(new TextMessageData
                    {
                        Message = "This is a simple text message."
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
            if (item is TextRecycleViewItem textItem)
            {
                // Handle text item click
            }
            else if (item is ImageRecycleViewItem imageItem)
            {
                // Handle image item click
            }
        }
    }
}
