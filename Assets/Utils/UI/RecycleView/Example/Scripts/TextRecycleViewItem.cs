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
        }
    }
}
