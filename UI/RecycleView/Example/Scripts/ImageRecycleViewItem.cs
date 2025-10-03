using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    // Item script for the ImageMessageData prefab, now using the generic base class
    public class ImageRecycleViewItem : RecycleViewItem<ImageMessageData>
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private GameObject selectionHighlight; // Optional highlight GameObject

        private ImageMessageData _currentData;

        // The BindData method is now strongly-typed, no casting needed!
        public override void BindData(ImageMessageData data, int index)
        {
            _currentData = data;
            itemImage.sprite = data.Image;
            captionText.text = $"{index}: {data.Caption}";
            itemImage.enabled = data.Image != null;
            
            // Hide selection highlight by default
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(false);
            }
            
            // Height is automatically managed by RecycleView using prefab's RectTransform size
        }

        // Override the click handler to provide custom behavior for image items
        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Example: Show/hide selection highlight
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(!selectionHighlight.activeSelf);
            }
            
            // Example: Log specific information about this image item
            Debug.Log($"Image item clicked! Caption: '{_currentData?.Caption}' at index {CurrentDataIndex}");
            
            // Example: Could open a larger view of the image, show context menu, etc.
        }
    }
}
