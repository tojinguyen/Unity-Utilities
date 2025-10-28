#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace TirexGame.Utils.Editor.BuildSystem
{
    /// <summary>
    /// Compatibility utility for handling editor interactions with different Unity versions and third-party tools
    /// </summary>
    public static class EditorCompatibility
    {
        /// <summary>
        /// Safely checks if Odin Inspector is available in the project
        /// </summary>
        public static bool IsOdinInspectorAvailable
        {
            get
            {
                try
                {
                    // Check if Sirenix assemblies are loaded
                    var odinAssembly = System.Reflection.Assembly.LoadFrom("Sirenix.OdinInspector.Editor");
                    return odinAssembly != null;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Safely creates Color32 from Color to avoid implicit operator issues
        /// </summary>
        public static Color32 SafeColorToColor32(Color color)
        {
            return new Color32(
                (byte)(color.r * 255f),
                (byte)(color.g * 255f), 
                (byte)(color.b * 255f),
                (byte)(color.a * 255f)
            );
        }
        
        /// <summary>
        /// Safely creates Color from Color32 to avoid implicit operator issues
        /// </summary>
        public static Color SafeColor32ToColor(Color32 color32)
        {
            return new Color(
                color32.r / 255f,
                color32.g / 255f,
                color32.b / 255f,
                color32.a / 255f
            );
        }
        
        /// <summary>
        /// Safe method to display dialog that works with or without Odin Inspector
        /// </summary>
        public static bool SafeDisplayDialog(string title, string message, string ok, string cancel = "")
        {
            try
            {
                if (string.IsNullOrEmpty(cancel))
                {
                    return EditorUtility.DisplayDialog(title, message, ok);
                }
                else
                {
                    return EditorUtility.DisplayDialog(title, message, ok, cancel);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EditorCompatibility] Dialog display failed: {ex.Message}");
                return true; // Default to proceed
            }
        }
        
        /// <summary>
        /// Gets Unity version compatibility info
        /// </summary>
        public static string GetUnityVersionInfo()
        {
            return $"Unity {Application.unityVersion}";
        }
        
        /// <summary>
        /// Checks if current Unity version supports modern scripting define symbols API
        /// </summary>
        public static bool SupportsNamedBuildTarget
        {
            get
            {
#if UNITY_2021_2_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }
    }
}
#endif