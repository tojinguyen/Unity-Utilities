using UnityEngine;
using TMPro;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Floating text component for 3D world space using TextMeshPro
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText3D : FloatingTextBase
    {
        [Header("3D Text Component")]
        [SerializeField] private TextMeshPro textMeshPro;

        private void Awake()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TextMeshPro>();
            }
        }

        protected override void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        protected override void UpdatePosition(float deltaMovement)
        {
            Vector3 movement = moveDirection * deltaMovement;
            transform.position += movement;
        }

        protected override void SetText(string text)
        {
            if (textMeshPro != null)
            {
                textMeshPro.text = text;
            }
        }

        protected override Color GetTextColor()
        {
            return textMeshPro != null ? textMeshPro.color : Color.white;
        }

        public override void SetColor(Color color)
        {
            if (textMeshPro != null)
            {
                textMeshPro.color = color;
            }
        }

        public override void SetFontSize(float size)
        {
            if (textMeshPro != null)
            {
                textMeshPro.fontSize = size;
            }
        }

        public override void SetFontStyle(FontStyles style)
        {
            if (textMeshPro != null)
            {
                textMeshPro.fontStyle = style;
            }
        }

        public override TMP_Text GetTextComponent()
        {
            return textMeshPro;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TextMeshPro>();
            }
        }
#endif
    }
}
