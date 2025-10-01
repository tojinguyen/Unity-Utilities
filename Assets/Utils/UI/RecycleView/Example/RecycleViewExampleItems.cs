using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    #region Data Models

    // Data for a simple text message item (Type 0)
    public class TextMessageData : IRecycleViewData
    {
        public int ItemType => 0;
        public string Message { get; set; }
    }

    // Data for an item with an image and a caption (Type 1)
    public class ImageMessageData : IRecycleViewData
    {
        public int ItemType => 1;
        public Sprite Image { get; set; }
        public string Caption { get; set; }
    }

    #endregion

    #region Item Implementations

    // Item script for the TextMessageData prefab
    public class TextRecycleViewItem : RecycleViewItem
    {
        [SerializeField] private TextMeshProUGUI messageText;

        public override void BindData(IRecycleViewData data, int index)
        {
            base.BindData(data, index); // Important to call the base method
            var textData = data as TextMessageData;
            if (textData != null)
            {
                messageText.text = $"{index}: {textData.Message}";
            }
        }
    }

    // Item script for the ImageMessageData prefab
    public class ImageRecycleViewItem : RecycleViewItem
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI captionText;

        public override void BindData(IRecycleViewData data, int index)
        {
            base.BindData(data, index); // Important to call the base method
            var imageData = data as ImageMessageData;
            if (imageData != null)
            {
                itemImage.sprite = imageData.Image;
                captionText.text = $"{index}: {imageData.Caption}";
                // You might want to handle null sprites, e.g., by hiding the image
                itemImage.enabled = imageData.Image != null;
            }
        }
    }

    #endregion
}
