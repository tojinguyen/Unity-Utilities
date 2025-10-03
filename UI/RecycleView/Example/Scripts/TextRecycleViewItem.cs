using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    // Item script for the TextMessageData prefab, now using the generic base class
    public class TextRecycleViewItem : RecycleViewItem<TextMessageData>
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;

        private TextMessageData _currentData;

        // The BindData method is now strongly-typed, no casting needed!
        public override void BindData(TextMessageData data, int index)
        {
            _currentData = data;
            messageText.text = $"{index}: {data.Message}";
            
            // Reset background color
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white;
            }
            
            // Height is automatically managed by RecycleView using prefab's RectTransform size
        }

        // Override the click handler to provide custom behavior for text items
        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Example: Change background color on click
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.yellow;
            }
            
            // Example: Log specific information about this text item
            Debug.Log($"Text item clicked! Message: '{_currentData?.Message}' at index {CurrentDataIndex}");
        }
    }
}
