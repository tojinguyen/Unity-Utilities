using UnityEngine;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Extension methods for easier FloatingText integration
    /// </summary>
    public static class FloatingTextExtensions
    {
        /// <summary>
        /// Show floating text at this transform's position using ScriptableObject config
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this Transform transform, string text, FloatingTextData data)
        {
            return FloatingTextFactory.Create(text, transform.position, data);
        }

        /// <summary>
        /// Show floating text at GameObject's position using ScriptableObject config
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this GameObject gameObject, string text, FloatingTextData data)
        {
            return FloatingTextFactory.Create(text, gameObject.transform.position, data);
        }

        /// <summary>
        /// Show floating text at Component's position using ScriptableObject config
        /// </summary>
        public static FloatingTextBase ShowFloatingText(this Component component, string text, FloatingTextData data)
        {
            return FloatingTextFactory.Create(text, component.transform.position, data);
        }
    }
}
