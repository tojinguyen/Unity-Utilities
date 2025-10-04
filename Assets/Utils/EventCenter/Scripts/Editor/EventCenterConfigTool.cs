#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace TirexGame.Utils.EventCenter.Editor
{
    /// <summary>
    /// Unity Editor tool for creating and managing EventCenter configurations
    /// Provides GUI for creating custom EventCenterConfig assets
    /// </summary>
    public class EventCenterConfigTool : EditorWindow
    {
        #region Fields
        
        private EventCenterConfig _customConfig;
        private EventCenterConfig _defaultConfig;
        private Vector2 _scrollPosition;
        
        // Preview settings
        private bool _previewSettings = false;
        
        #endregion
        
        #region Menu Items
        
        [MenuItem("TirexGame/Event Center/Create Custom Config", priority = 100)]
        public static void CreateCustomConfig()
        {
            var window = GetWindow<EventCenterConfigTool>("EventCenter Config");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        [MenuItem("TirexGame/Event Center/Open Current Config", priority = 101)]
        public static void OpenCurrentConfig()
        {
            var config = EventCenterConfig.Instance;
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
            else
            {
                EditorUtility.DisplayDialog("No Config", "No EventCenter config found. Create one first.", "OK");
            }
        }
        
        [MenuItem("TirexGame/Event Center/Refresh Config Cache", priority = 102)]
        public static void RefreshConfigCache()
        {
            EventCenterConfig.RefreshConfiguration();
            EditorUtility.DisplayDialog("Cache Refreshed", "EventCenter configuration cache has been refreshed.", "OK");
        }
        
        [MenuItem("TirexGame/Event Center/Show Config Status", priority = 103)]
        public static void ShowConfigStatus()
        {
            var currentConfig = EventCenterConfig.Instance;
            var defaultConfig = EventCenterConfig.DefaultConfig;
            
            var message = "=== EventCenter Configuration Status ===\n\n";
            
            if (currentConfig != null)
            {
                message += $"✅ Active Config: {currentConfig.name}\n";
                message += $"📁 Source: {GetConfigSourceStatic(currentConfig)}\n";
                message += $"🚀 Auto Create: {currentConfig.autoCreateEventCenter}\n";
                message += $"🔒 Don't Destroy: {currentConfig.dontDestroyOnLoad}\n";
                message += $"⚡ Max Events/Frame: {currentConfig.maxEventsPerFrame}\n";
                message += $"📊 Debug Logging: {currentConfig.enableLogging}\n";
            }
            else
            {
                message += "❌ No config found\n";
            }
            
            if (defaultConfig != null)
            {
                message += $"\n📦 Package Default Available: {defaultConfig.name}";
            }
            
            EditorUtility.DisplayDialog("Config Status", message, "OK");
        }
        
        private static string GetConfigSourceStatic(EventCenterConfig config)
        {
            if (config == null) return "None";
            
            if (config.name.Contains("Runtime Default"))
                return "Runtime Default";
            else if (config.name.Contains("Default"))
                return "Package Default";
            else
                return "Custom Config";
        }
        
        #endregion
        
        #region Unity Editor Methods
        
        private void OnEnable()
        {
            _defaultConfig = EventCenterConfig.DefaultConfig;
            LoadOrCreateCustomConfig();
        }
        
        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            DrawConfigStatus();
            DrawConfigEditor();
            DrawActions();
            
            EditorGUILayout.EndScrollView();
        }
        
        #endregion
        
        #region GUI Drawing
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("🎯 EventCenter Configuration Tool", headerStyle);
            EditorGUILayout.Space(5);
            
            var descStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Create and customize your EventCenter configuration", descStyle);
            EditorGUILayout.Space(10);
        }
        
        private void DrawConfigStatus()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 Current Configuration Status", EditorStyles.boldLabel);
            
            var currentConfig = EventCenterConfig.Instance;
            var configName = currentConfig != null ? currentConfig.name : "None";
            var configSource = GetConfigSource(currentConfig);
            
            EditorGUILayout.LabelField("Active Config:", configName);
            EditorGUILayout.LabelField("Source:", configSource);
            
            if (currentConfig != null)
            {
                var statusColor = GetStatusColor(configSource);
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = statusColor;
                
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Status:", GetStatusText(configSource));
                EditorGUILayout.EndHorizontal();
                
                GUI.backgroundColor = oldColor;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawConfigEditor()
        {
            if (_customConfig == null)
            {
                LoadOrCreateCustomConfig();
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("⚙️ Custom Configuration Editor", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            // Auto Creation Settings
            EditorGUILayout.LabelField("Auto Creation Settings", EditorStyles.boldLabel);
            _customConfig.autoCreateEventCenter = EditorGUILayout.Toggle(
                new GUIContent("Auto Create EventCenter", "Automatically create EventCenter if none exists"), 
                _customConfig.autoCreateEventCenter);
            
            GUI.enabled = _customConfig.autoCreateEventCenter;
            _customConfig.dontDestroyOnLoad = EditorGUILayout.Toggle(
                new GUIContent("Don't Destroy On Load", "Make EventCenter persistent across scenes"), 
                _customConfig.dontDestroyOnLoad);
            
            _customConfig.autoCreatedName = EditorGUILayout.TextField(
                new GUIContent("Auto Created Name", "Name for auto-created EventCenter GameObject"), 
                _customConfig.autoCreatedName);
            GUI.enabled = true;
            
            EditorGUILayout.Space(5);
            
            // Performance Settings
            EditorGUILayout.LabelField("Performance Settings", EditorStyles.boldLabel);
            _customConfig.maxEventsPerFrame = EditorGUILayout.IntSlider(
                new GUIContent("Max Events Per Frame", "Maximum events processed per frame"), 
                _customConfig.maxEventsPerFrame, 100, 50000);
            
            _customConfig.maxBatchSize = EditorGUILayout.IntSlider(
                new GUIContent("Max Batch Size", "Maximum batch size for event processing"), 
                _customConfig.maxBatchSize, 10, 5000);
            
            EditorGUILayout.Space(5);
            
            // Debug Settings
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            _customConfig.enableLogging = EditorGUILayout.Toggle(
                new GUIContent("Enable Logging", "Enable debug logging for EventCenter"), 
                _customConfig.enableLogging);
            
            _customConfig.enableProfiling = EditorGUILayout.Toggle(
                new GUIContent("Enable Profiling", "Enable performance profiling"), 
                _customConfig.enableProfiling);
            
            _customConfig.showStats = EditorGUILayout.Toggle(
                new GUIContent("Show Stats", "Show runtime statistics"), 
                _customConfig.showStats);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_customConfig);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        private void DrawActions()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🔧 Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Save Custom Config
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("💾 Save Custom Config", GUILayout.Height(30)))
            {
                SaveCustomConfig();
            }
            
            // Reset to Default
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("🔄 Reset to Default", GUILayout.Height(30)))
            {
                ResetToDefault();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            // Copy from Default
            if (GUILayout.Button("📋 Copy from Default", GUILayout.Height(25)))
            {
                CopyFromDefault();
            }
            
            // Reveal in Project
            if (GUILayout.Button("📂 Reveal in Project", GUILayout.Height(25)))
            {
                RevealInProject();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadOrCreateCustomConfig()
        {
            // Try to load existing custom config
            var existingPath = "Assets/Resources/EventCenterConfig.asset";
            _customConfig = AssetDatabase.LoadAssetAtPath<EventCenterConfig>(existingPath);
            
            if (_customConfig == null)
            {
                // Create new custom config based on default
                _customConfig = CreateInstance<EventCenterConfig>();
                
                if (_defaultConfig != null)
                {
                    CopyConfigValues(_defaultConfig, _customConfig);
                }
                
                _customConfig.name = "Custom EventCenter Config";
            }
        }
        
        private void SaveCustomConfig()
        {
            // Ensure Resources directory exists
            var resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
                AssetDatabase.Refresh();
            }
            
            // Save the asset
            var assetPath = "Assets/Resources/EventCenterConfig.asset";
            var existingAsset = AssetDatabase.LoadAssetAtPath<EventCenterConfig>(assetPath);
            
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(_customConfig, assetPath);
            }
            else
            {
                EditorUtility.CopySerialized(_customConfig, existingAsset);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Refresh configuration cache
            EventCenterConfig.RefreshConfiguration();
            
            // Show success message
            EditorUtility.DisplayDialog("Config Saved", 
                $"Custom EventCenter configuration saved to {assetPath}", "OK");
            
            // Ping the asset
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<EventCenterConfig>(assetPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        private void ResetToDefault()
        {
            if (EditorUtility.DisplayDialog("Reset to Default", 
                "Are you sure you want to reset all settings to default values?", "Yes", "Cancel"))
            {
                if (_defaultConfig != null)
                {
                    CopyConfigValues(_defaultConfig, _customConfig);
                }
                EditorUtility.SetDirty(_customConfig);
            }
        }
        
        private void CopyFromDefault()
        {
            if (_defaultConfig != null)
            {
                CopyConfigValues(_defaultConfig, _customConfig);
                EditorUtility.SetDirty(_customConfig);
            }
        }
        
        private void RevealInProject()
        {
            var assetPath = "Assets/Resources/EventCenterConfig.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EventCenterConfig>(assetPath);
            
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                EditorUtility.DisplayDialog("Config Not Found", 
                    "Custom config not found. Save it first.", "OK");
            }
        }
        
        private void CopyConfigValues(EventCenterConfig source, EventCenterConfig target)
        {
            target.autoCreateEventCenter = source.autoCreateEventCenter;
            target.dontDestroyOnLoad = source.dontDestroyOnLoad;
            target.autoCreatedName = source.autoCreatedName;
            target.maxEventsPerFrame = source.maxEventsPerFrame;
            target.maxBatchSize = source.maxBatchSize;
            target.enableLogging = source.enableLogging;
            target.enableProfiling = source.enableProfiling;
            target.showStats = source.showStats;
        }
        
        private string GetConfigSource(EventCenterConfig config)
        {
            if (config == null) return "None";
            
            if (config.name.Contains("Runtime Default"))
                return "Runtime Default";
            else if (config.name.Contains("Default"))
                return "Package Default";
            else
                return "Custom Config";
        }
        
        private Color GetStatusColor(string source)
        {
            switch (source)
            {
                case "Custom Config": return Color.green;
                case "Package Default": return Color.cyan;
                case "Runtime Default": return Color.yellow;
                default: return Color.red;
            }
        }
        
        private string GetStatusText(string source)
        {
            switch (source)
            {
                case "Custom Config": return "✅ Using Custom Configuration";
                case "Package Default": return "📦 Using Package Default";
                case "Runtime Default": return "⚠️ Using Runtime Default";
                default: return "❌ No Configuration";
            }
        }
        
        #endregion
    }
}
#endif