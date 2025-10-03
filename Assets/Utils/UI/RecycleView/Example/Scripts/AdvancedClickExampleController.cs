using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Simple example showing basic item click handling
    /// </summary>
    public class AdvancedClickExampleController : MonoBehaviour
    {
        [SerializeField] private RecycleView recycleView;
        [SerializeField] private Sprite sampleSprite;

        private List<IRecycleViewData> exampleDataList;

        private void Start()
        {
            GenerateData();
            recycleView.SetData(exampleDataList);
            recycleView.OnItemClicked += HandleItemClicked;
        }

        private void OnDestroy()
        {
            if (recycleView != null)
            {
                recycleView.OnItemClicked -= HandleItemClicked;
            }
        }

        private void GenerateData()
        {
            exampleDataList = new List<IRecycleViewData>();
            
            for (int i = 0; i < 100; i++)
            {
                if (i % 4 == 0)
                {
                    exampleDataList.Add(new ImageMessageData
                    {
                        Image = sampleSprite,
                        Caption = $"Image {i}"
                    });
                }
                else
                {
                    exampleDataList.Add(new TextMessageData
                    {
                        Message = $"Text message {i}"
                    });
                }
            }
        }

        private void HandleItemClicked(RecycleViewItem item)
        {
            var index = item.CurrentDataIndex;
            var data = exampleDataList[index];

            // Simple click handling - just log the click
            if (data is TextMessageData textData)
            {
                Debug.Log($"Text item clicked at index {index}: {textData.Message}");
            }
            else if (data is ImageMessageData imageData)
            {
                Debug.Log($"Image item clicked at index {index}: {imageData.Caption}");
            }
        }
    }
}