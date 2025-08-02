using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Tirex.Game.Utils.Localization.Editor
{
    /// <summary>
    /// Property drawer for LocalizationKey attribute
    /// Shows dropdown with available localization keys
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
    public class LocalizationKeyPropertyDrawer : PropertyDrawer
    {
        private static Dictionary<LocalizationConfig, List<string>> cachedKeys = new Dictionary<LocalizationConfig, List<string>>();
        private static Dictionary<LocalizationConfig, List<string>> cachedTextKeys = new Dictionary<LocalizationConfig, List<string>>();
        private static Dictionary<LocalizationConfig, List<string>> cachedSpriteKeys = new Dictionary<LocalizationConfig, List<string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use LocalizationKey attribute only on string fields!");
                return;
            }

            var keyAttribute = (LocalizationKeyAttribute)attribute;
            
            // Find LocalizationConfig in the scene or project
            var config = FindLocalizationConfig();
            if (config == null)
            {
                // Fallback to regular string field with warning
                EditorGUI.BeginProperty(position, label, property);
                
                var rect = new Rect(position.x, position.y, position.width - 100, position.height);
                var buttonRect = new Rect(position.x + position.width - 95, position.y, 95, position.height);
                
                property.stringValue = EditorGUI.TextField(rect, label, property.stringValue);
                
                if (GUI.Button(buttonRect, "Find Config"))
                {
                    ShowConfigSelector(property);
                }
                
                EditorGUI.EndProperty();
                return;
            }

            // Get available keys based on attribute settings
            var availableKeys = GetAvailableKeys(config, keyAttribute);
            
            if (availableKeys.Count == 0)
            {
                EditorGUI.LabelField(position, label.text, "No localization keys found!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Calculate rects
            var dropdownRect = new Rect(position.x, position.y, position.width - 120, position.height);
            var textFieldRect = new Rect(position.x + position.width - 115, position.y, 70, position.height);
            var buttonRect = new Rect(position.x + position.width - 40, position.y, 40, position.height);

            // Find current selection
            int selectedIndex = availableKeys.IndexOf(property.stringValue);
            if (selectedIndex < 0) selectedIndex = 0;

            // Add "None" option
            var displayKeys = new List<string> { "(None)" };
            displayKeys.AddRange(availableKeys);
            
            if (!string.IsNullOrEmpty(property.stringValue) && !availableKeys.Contains(property.stringValue))
            {
                displayKeys.Add($"(Custom: {property.stringValue})");
                selectedIndex = displayKeys.Count - 1;
            }
            else
            {
                selectedIndex++; // Account for "None" option
            }

            // Dropdown selection
            int newSelectedIndex = EditorGUI.Popup(dropdownRect, label.text, selectedIndex, displayKeys.ToArray());
            
            if (newSelectedIndex != selectedIndex)
            {
                if (newSelectedIndex == 0)
                {
                    property.stringValue = "";
                }
                else if (newSelectedIndex <= availableKeys.Count)
                {
                    property.stringValue = availableKeys[newSelectedIndex - 1];
                }
            }

            // Manual text field for custom keys
            string newValue = EditorGUI.TextField(textFieldRect, property.stringValue);
            if (newValue != property.stringValue)
            {
                property.stringValue = newValue;
            }

            // Refresh button
            if (GUI.Button(buttonRect, "↻"))
            {
                RefreshCache();
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Get available keys based on attribute settings
        /// </summary>
        private List<string> GetAvailableKeys(LocalizationConfig config, LocalizationKeyAttribute keyAttribute)
        {
            if (keyAttribute.textKeysOnly)
            {
                return GetCachedTextKeys(config);
            }
            else if (keyAttribute.spriteKeysOnly)
            {
                return GetCachedSpriteKeys(config);
            }
            else
            {
                return GetCachedAllKeys(config);
            }
        }

        /// <summary>
        /// Get cached all keys
        /// </summary>
        private List<string> GetCachedAllKeys(LocalizationConfig config)
        {
            if (!cachedKeys.ContainsKey(config) || cachedKeys[config] == null)
            {
                cachedKeys[config] = LocalizationKeysGenerator.GetAllKeysFromConfig(config);
            }
            return cachedKeys[config];
        }

        /// <summary>
        /// Get cached text keys only
        /// </summary>
        private List<string> GetCachedTextKeys(LocalizationConfig config)
        {
            if (!cachedTextKeys.ContainsKey(config) || cachedTextKeys[config] == null)
            {
                var textKeys = config.GetAllTextKeys();
                textKeys.Sort();
                cachedTextKeys[config] = textKeys;
            }
            return cachedTextKeys[config];
        }

        /// <summary>
        /// Get cached sprite keys only
        /// </summary>
        private List<string> GetCachedSpriteKeys(LocalizationConfig config)
        {
            if (!cachedSpriteKeys.ContainsKey(config) || cachedSpriteKeys[config] == null)
            {
                var spriteKeys = config.GetAllSpriteKeys();
                spriteKeys.Sort();
                cachedSpriteKeys[config] = spriteKeys;
            }
            return cachedSpriteKeys[config];
        }

        /// <summary>
        /// Find localization config in scene or project
        /// </summary>
        private LocalizationConfig FindLocalizationConfig()
        {
            // First try to find in scene
            var manager = Object.FindObjectOfType<LocalizationManager>();
            if (manager != null && manager.Config != null)
            {
                return manager.Config;
            }

            // Try to find in project assets
            var guids = AssetDatabase.FindAssets("t:LocalizationConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LocalizationConfig>(path);
            }

            return null;
        }

        /// <summary>
        /// Show config selector popup
        /// </summary>
        private void ShowConfigSelector(SerializedProperty property)
        {
            var guids = AssetDatabase.FindAssets("t:LocalizationConfig");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("No Config Found", 
                    "No LocalizationConfig found in project. Please create one first.", "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(path);
                if (config != null)
                {
                    menu.AddItem(new GUIContent(config.name), false, () =>
                    {
                        Selection.activeObject = config;
                        EditorGUIUtility.PingObject(config);
                    });
                }
            }
            menu.ShowAsContext();
        }

        /// <summary>
        /// Refresh cached keys
        /// </summary>
        private void RefreshCache()
        {
            cachedKeys.Clear();
            cachedTextKeys.Clear();
            cachedSpriteKeys.Clear();
        }

        /// <summary>
        /// Clear cache when assets are refreshed
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ClearCacheOnLoad()
        {
            EditorApplication.projectChanged += () =>
            {
                cachedKeys.Clear();
                cachedTextKeys.Clear();
                cachedSpriteKeys.Clear();
            };
        }
    }
}
