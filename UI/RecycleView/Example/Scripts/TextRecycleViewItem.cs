using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    // Item script for the TextMessageData prefab, now using the generic base class
    public class TextRecycleViewItem : RecycleViewItem<TextMessageData>
    {
        [SerializeField] private TextMeshProUGUI messageText;

        // The BindData method is now strongly-typed, no casting needed!
        public override void BindData(TextMessageData data, int index)
        {
            messageText.text = $"{index}: {data.Message}";
            
            // Adjust the RectTransform height to match custom height if specified
            var rectTransform = transform as RectTransform;
            if (rectTransform != null && data.CustomHeight > 0)
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.CustomHeight);
            }
        }
    }
}
