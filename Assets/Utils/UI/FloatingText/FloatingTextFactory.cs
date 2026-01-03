using UnityEngine;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Factory for creating floating texts with ScriptableObject configuration
    /// </summary>
    public static class FloatingTextFactory
    {
        /// <summary>
        /// Create floating text with ScriptableObject data configuration.
        /// Automatically converts world position to screen position for 2D mode.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="worldPosition">Position in world space</param>
        /// <param name="data">FloatingTextData ScriptableObject configuration</param>
        public static FloatingTextBase Create(string text, Vector3 worldPosition, FloatingTextData data)
        {
            if (data == null)
            {
                Debug.LogWarning("FloatingTextFactory: FloatingTextData is null. Using default settings.");
            }

            // Use 3D mode if specified in data, otherwise use 2D
            if (data != null && data.UseWorldSpace)
            {
                return FloatingTextManager.Instance.ShowText3D(text, worldPosition, data);
            }
            else
            {
                return FloatingTextManager.Instance.ShowTextAtWorldPosition(text, worldPosition, data);
            }
        }

        /// <summary>
        /// Create floating text at screen position (2D UI mode)
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="screenPosition">Position in screen space</param>
        /// <param name="data">FloatingTextData ScriptableObject configuration</param>
        public static FloatingTextBase CreateAtScreenPosition(string text, Vector3 screenPosition, FloatingTextData data)
        {
            return FloatingTextManager.Instance.ShowText2D(text, screenPosition, data);
        }

        /// <summary>
        /// Create floating text in 3D world space
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="worldPosition">Position in world space</param>
        /// <param name="data">FloatingTextData ScriptableObject configuration</param>
        public static FloatingTextBase Create3D(string text, Vector3 worldPosition, FloatingTextData data)
        {
            return FloatingTextManager.Instance.ShowText3D(text, worldPosition, data);
        }
    }
}
