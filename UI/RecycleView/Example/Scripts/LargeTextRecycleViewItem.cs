using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Implementation of RecycleViewItem for displaying large text messages with custom heights.
    /// </summary>
    public class LargeTextRecycleViewItem : RecycleViewItem<LargeTextMessageData>
    {
        [Header("UI References")]
        [SerializeField] private Text messageText;
        [SerializeField] private Text indexText;
        [SerializeField] private Image backgroundImage;

        public override void BindData(LargeTextMessageData data, int index)
        {
            if (data == null)
            {
                Debug.LogError("LargeTextRecycleViewItem received null data");
                return;
            }

            // Update UI elements
            if (messageText != null)
                messageText.text = data.Message;

            if (indexText != null)
                indexText.text = $"Index: {index}";

            // Optional: Change background color based on index for visual variety
            if (backgroundImage != null)
            {
                Color backgroundColor = index % 2 == 0 ? new Color(0.9f, 0.9f, 1f, 1f) : new Color(1f, 0.9f, 0.9f, 1f);
                backgroundImage.color = backgroundColor;
            }

            // Adjust the RectTransform height to match custom height
            var rectTransform = transform as RectTransform;
            if (rectTransform != null && data.CustomHeight > 0)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.CustomHeight);
            }
        }

        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            // We can access the data through the RecycleView if needed
            if (ParentRecycleView != null)
            {
                Debug.Log($"Large Text Item clicked at index: {CurrentDataIndex}");
            }
        }
    }
}