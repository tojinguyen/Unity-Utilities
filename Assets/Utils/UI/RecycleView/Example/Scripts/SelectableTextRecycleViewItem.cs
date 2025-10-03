using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Text item with selection support and visual feedback
    /// </summary>
    public class SelectableTextRecycleViewItem : RecycleViewItem<TextMessageData>
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private GameObject selectionCheckmark;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.cyan;
        [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray

        private TextMessageData _currentData;
        private bool _isSelected = false;

        public override void Initialize(RecycleView parent)
        {
            base.Initialize(parent);
            
            // Setup button color transitions using the reference
            if (clickButton != null)
            {
                var colors = clickButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = hoverColor;
                colors.pressedColor = selectedColor;
                clickButton.colors = colors;
            }
        }

        public override void BindData(TextMessageData data, int index)
        {
            _currentData = data;
            messageText.text = $"{index}: {data.Message}";
            
            UpdateSelectionVisual();
        }

        private void UpdateSelectionVisual()
        {
            if (_currentData == null) return;

            // Update background color
            if (backgroundImage != null)
            {
                backgroundImage.color = _isSelected ? selectedColor : normalColor;
            }

            // Update selection border
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(_isSelected);
            }

            // Update checkmark
            if (selectionCheckmark != null)
            {
                selectionCheckmark.SetActive(_isSelected);
            }
        }

        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Toggle selection state
            _isSelected = !_isSelected;
            UpdateSelectionVisual();
            
            // Add click animation
            if (clickButton != null)
            {
                StartCoroutine(ClickAnimation());
            }
        }

        private System.Collections.IEnumerator ClickAnimation()
        {
            // Quick scale animation
            var originalScale = transform.localScale;
            var targetScale = originalScale * 0.95f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            // Scale down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale back up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
    }
}