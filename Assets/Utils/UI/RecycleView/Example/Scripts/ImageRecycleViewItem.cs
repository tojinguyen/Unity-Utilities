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

        // The BindData method is now strongly-typed, no casting needed!
        public override void BindData(ImageMessageData data, int index)
        {
            itemImage.sprite = data.Image;
            captionText.text = $"{index}: {data.Caption}";
            itemImage.enabled = data.Image != null;
            
            // Adjust the RectTransform height to match custom height if specified
            var rectTransform = transform as RectTransform;
            if (rectTransform != null && data.CustomHeight > 0)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.CustomHeight);
            }
        }
    }
}
