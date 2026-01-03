using UnityEngine;
using TMPro;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Floating text component for 2D screen space using TextMeshProUGUI
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FloatingText2D : FloatingTextBase
    {
        [HideInInspector][SerializeField] private TextMeshProUGUI textMeshProUGUI;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (textMeshProUGUI == null)
            {
                textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            }
        }

        protected override void SetPosition(Vector3 position)
        {
            if (rectTransform != null)
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
            if (rectTransform != null)
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
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }
        }

        protected override Color GetTextColor()
        {
            return textMeshProUGUI != null ? textMeshProUGUI.color : Color.white;
        }

        public override void SetColor(Color color)
        {
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.color = color;
            }
        }

        public override void SetFontSize(float size)
        {
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.fontSize = size;
            }
        }

        public override void SetFontStyle(FontStyles style)
        {
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.fontStyle = style;
            }
        }

        public override TMP_Text GetTextComponent()
        {
            return textMeshProUGUI;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (textMeshProUGUI == null)
            {
                textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            }
        }
#endif
    }
}
