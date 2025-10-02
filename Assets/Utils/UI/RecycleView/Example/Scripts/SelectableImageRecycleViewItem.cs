using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Image item with selection support and visual feedback
    /// </summary>
    public class SelectableImageRecycleViewItem : RecycleViewItem<ImageMessageData>
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject selectionOverlay;
        [SerializeField] private GameObject selectionCheckmark;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;

        private ImageMessageData _currentData;
        private bool _isSelected = false;

        public override void BindData(ImageMessageData data, int index)
        {
            _currentData = data;
            itemImage.sprite = data.Image;
            captionText.text = $"{index}: {data.Caption}";
            itemImage.enabled = data.Image != null;
            
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

            // Update selection overlay
            if (selectionOverlay != null)
            {
                selectionOverlay.SetActive(_isSelected);
            }

            // Update checkmark
            if (selectionCheckmark != null)
            {
                selectionCheckmark.SetActive(_isSelected);
            }

            // Dim/brighten image based on selection
            if (itemImage != null)
            {
                var color = itemImage.color;
                color.a = _isSelected ? 0.8f : 1f;
                itemImage.color = color;
            }
        }

        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Toggle selection state
            _isSelected = !_isSelected;
            UpdateSelectionVisual();
            
            // Add pulse effect on click
            if (selectionCheckmark != null && _isSelected)
            {
                StartCoroutine(PulseEffect());
            }
        }

        private System.Collections.IEnumerator PulseEffect()
        {
            var originalScale = selectionCheckmark.transform.localScale;
            var targetScale = originalScale * 1.3f;
            
            float duration = 0.15f;
            float elapsed = 0f;
            
            // Scale up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                selectionCheckmark.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // Scale back down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                selectionCheckmark.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            selectionCheckmark.transform.localScale = originalScale;
        }
    }
}