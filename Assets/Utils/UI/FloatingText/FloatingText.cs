using UnityEngine;
using TMPro;
using System;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Floating text component that displays animated text in both 2D and 3D space.
    /// Supports TextMeshPro and TextMeshProUGUI.
    /// LEGACY: Use FloatingText3D or FloatingText2D instead.
    /// </summary>
    [System.Obsolete("Use FloatingText3D or FloatingText2D instead for better type safety.", false)]
    public class FloatingText : FloatingTextBase
    {
        [Header("Text Components")]
        [SerializeField] private TextMeshPro textMeshPro;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;

        private RectTransform rectTransform;
        private bool isUIMode;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            // Auto-detect mode based on available components
            if (textMeshProUGUI != null)
            {
                isUIMode = true;
            }
            else if (textMeshPro != null)
            {
                isUIMode = false;
            }
        }

        protected override void SetPosition(Vector3 position)
        {
            if (isUIMode && rectTransform != null)
            {
                rectTransform.position = position;
            }
            else
            {
                transform.position = position;
            }
        }

        protected override void UpdatePosition(float deltaMovement)
        {
            Vector3 movement = moveDirection * deltaMovement;
            if (isUIMode && rectTransform != null)
            {
                rectTransform.position += movement;
            }
            else
            {
                transform.position += movement;
            }
        }

        protected override void SetText(string text)
        {
            if (isUIMode && textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }
            else if (textMeshPro != null)
            {
                textMeshPro.text = text;
            }
        }

        protected override Color GetTextColor()
        {
            if (isUIMode && textMeshProUGUI != null)
            {
                return textMeshProUGUI.color;
            }
            else if (textMeshPro != null)
            {
                return textMeshPro.color;
            }
            return Color.white;
        }

        public override void SetColor(Color color)
        {
            if (isUIMode && textMeshProUGUI != null)
            {
                textMeshProUGUI.color = color;
            }
            else if (textMeshPro != null)
            {
                textMeshPro.color = color;
            }
        }

        public override void SetFontSize(float size)
        {
            if (isUIMode && textMeshProUGUI != null)
            {
                textMeshProUGUI.fontSize = size;
            }
            else if (textMeshPro != null)
            {
                textMeshPro.fontSize = size;
            }
        }

        public override void SetFontStyle(FontStyles style)
        {
            if (isUIMode && textMeshProUGUI != null)
            {
                textMeshProUGUI.fontStyle = style;
            }
            else if (textMeshPro != null)
            {
                textMeshPro.fontStyle = style;
            }
        }

        public override TMP_Text GetTextComponent()
        {
            return isUIMode ? (TMP_Text)textMeshProUGUI : (TMP_Text)textMeshPro;
        }

        /// <summary>
        /// Check if this floating text is in UI mode (Canvas)
        /// </summary>
        public bool IsUIMode => isUIMode;

        /// <summary>
        /// Force set the mode (useful when creating instances)
        /// </summary>
        public void SetMode(bool uiMode)
        {
            isUIMode = uiMode;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-assign components if not set
            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TextMeshPro>();
            }
            if (textMeshProUGUI == null)
            {
                textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            }
        }
#endif
    }
}
