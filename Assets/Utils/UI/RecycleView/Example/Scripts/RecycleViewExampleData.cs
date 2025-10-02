using UnityEngine;

namespace TirexGame.Utils.UI.Example
{
    #region Data Models

    // Data for a simple text message item (Type 0)
    public class TextMessageData : IRecycleViewData
    {
        public int ItemType => 0;
        public string Message { get; set; }
        public float CustomHeight { get; set; } = -1f; // Use default height
    }

    // Data for an item with an image and a caption (Type 1)
    public class ImageMessageData : IRecycleViewData
    {
        public int ItemType => 1;
        public Sprite Image { get; set; }
        public string Caption { get; set; }
        public float CustomHeight { get; set; } = -1f; // Use default height
    }

    // Data for a large text message item with custom height (Type 2)
    public class LargeTextMessageData : IRecycleViewData
    {
        public int ItemType => 2;
        public string Message { get; set; }
        public float CustomHeight { get; set; } = 150f; // Custom height
    }

    #endregion
}
