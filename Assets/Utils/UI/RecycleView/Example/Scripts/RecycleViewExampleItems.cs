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

    // Item script for the TextMessageData prefab, now using the generic base class
    public class TextRecycleViewItem : RecycleViewItem<TextMessageData>
    {
        [SerializeField] private TextMeshProUGUI messageText;

        // The BindData method is now strongly-typed, no casting needed!
        public override void BindData(TextMessageData data, int index)
        {
            messageText.text = $"{index}: {data.Message}";
        }
    }

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
        }
    }

    #endregion
}
