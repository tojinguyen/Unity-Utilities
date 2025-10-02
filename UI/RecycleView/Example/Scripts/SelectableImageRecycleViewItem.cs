using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.UI.Example
{
    /// <summary>
    /// Image item with selection support and visual feedback
    /// </summary>
    public class SelectableImageRecycleViewItem : RecycleViewItem<SelectableImageData>
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject selectionOverlay;
        [SerializeField] private GameObject selectionCheckmark;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;

        private SelectableImageData _currentData;

        public override void BindData(SelectableImageData data, int index)
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
                backgroundImage.color = _currentData.IsSelected ? selectedColor : normalColor;
            }

            // Update selection overlay
            if (selectionOverlay != null)
            {
                selectionOverlay.SetActive(_currentData.IsSelected);
            }

            // Update checkmark
            if (selectionCheckmark != null)
            {
                selectionCheckmark.SetActive(_currentData.IsSelected);
            }

            // Dim/brighten image based on selection
            if (itemImage != null)
            {
                var color = itemImage.color;
                color.a = _currentData.IsSelected ? 0.8f : 1f;
                itemImage.color = color;
            }
        }

        protected override void OnItemClicked()
        {
            base.OnItemClicked();
            
            // Add pulse effect on click
            if (selectionCheckmark != null && _currentData.IsSelected)
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