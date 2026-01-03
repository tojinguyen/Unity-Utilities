using UnityEngine;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Extension methods for easier FloatingText integration
    /// </summary>
    public static class FloatingTextExtensions
    {
        /// <summary>
        /// Show floating text at this transform's position
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this Transform transform, string text, FloatingTextData data, bool is3D = false)
        {
            return FloatingTextFactory.Create(text, transform.position, data, is3D);
        }

        /// <summary>
        /// Show floating text at GameObject's position
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this GameObject gameObject, string text, FloatingTextData data, bool is3D = false)
        {
            return FloatingTextFactory.Create(text, gameObject.transform.position, data, is3D);
        }

        /// <summary>
        /// Show floating text at Component's position
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this Component component, string text, FloatingTextData data, bool is3D = false)
        {
            return FloatingTextFactory.Create(text, component.transform.position, data, is3D);
        }

        /// <summary>
        /// Get builder for custom floating text at this transform's position
        /// </summary>
        public static FloatingTextBuilder GetFloatingTextBuilder(this Transform transform)
        {
            return FloatingTextFactory.Builder().SetPosition(transform.position);
        }

        /// <summary>
        /// Show damage with color based on type
        /// </summary>
        public static FloatingTextBase ShowColoredDamage(this Transform transform, float damage, DamageType damageType, bool is3D = false)
        {
            Color color = GetColorForDamageType(damageType);

            return FloatingTextFactory.Builder()
                .SetText(damage.ToString("0"))
                .SetPosition(transform.position)
                .WithColor(color)
                .WithFontSize(48)
                .WithSpeed(3f)
                .WithRandomDirection(20f)
                .Bold()
                .Show();
        }

        /// <summary>
        /// Damage type enum for colored damage
        /// </summary>
        public enum DamageType
        {
            Physical,
            Fire,
            Ice,
            Lightning,
            Poison,
            Holy,
            Dark
        }

        private static Color GetColorForDamageType(DamageType type)
        {
            switch (type)
            {
                case DamageType.Physical: return Color.white;
                case DamageType.Fire: return new Color(1f, 0.3f, 0f);
                case DamageType.Ice: return new Color(0.3f, 0.8f, 1f);
                case DamageType.Lightning: return new Color(1f, 1f, 0.3f);
                case DamageType.Poison: return new Color(0.5f, 1f, 0.3f);
                case DamageType.Holy: return new Color(1f, 1f, 0.7f);
                case DamageType.Dark: return new Color(0.5f, 0.3f, 0.8f);
                default: return Color.white;
            }
        }
    }
}
