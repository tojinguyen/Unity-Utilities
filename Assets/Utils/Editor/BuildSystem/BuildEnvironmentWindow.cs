#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.IO;
using System.Linq;

namespace TirexGame.Utils.Editor.BuildSystem
{
    /// <summary>
    /// Editor window for managing build environment configurations
    /// </summary>
    public class BuildEnvironmentWindow : EditorWindow
    {
        private BuildEnvironmentConfig currentConfig;
        private Vector2 scrollPosition;
        private bool showAdvancedSettings = false;
        
        private const string DEFAULT_CONFIG_PATH = "Assets/Editor/BuildSystem/Resources";
        private const string DEFAULT_CONFIG_NAME = "BuildEnvironmentConfig.asset";
        private const string CONFIG_SEARCH_FILTER = "t:BuildEnvironmentConfig";
        
        #region Menu Items
        
        [MenuItem("TirexGame/Build/Environment Manager", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildEnvironmentWindow>("Build Environment");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        [MenuItem("TirexGame/Build/Create Environment Config", priority = 101)]
        public static void CreateEnvironmentConfig()
        {
            CreateDefaultConfig();
        }
        
        [MenuItem("TirexGame/Build/Apply Current Environment", priority = 102)]
        public static void ApplyCurrentEnvironment()
        {
            var config = FindExistingConfig();
            if (config != null)
            {
                ApplyEnvironmentDefines(config);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No BuildEnvironmentConfig found in project.", "OK");
            }
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnEnable()
        {
            LoadExistingConfig();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            DrawHeader();
            EditorGUILayout.Space(10);
            
            DrawConfigSelection();
            EditorGUILayout.Space(10);
            
            if (currentConfig != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                DrawCurrentEnvironment();
                EditorGUILayout.Space(10);
                
                DrawEnvironmentDefines();
                EditorGUILayout.Space(10);
                
                DrawBuildSettings();
                EditorGUILayout.Space(10);
                
                DrawActions();
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                DrawNoConfigFound();
            }
        }
        
        #endregion
        
        #region GUI Drawing
        
        private void DrawHeader()
        {
            var titleStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            
            EditorGUILayout.LabelField("Build Environment Manager (Standalone)", titleStyle);
            EditorGUILayout.LabelField("Manage scripting defines for different build environments", EditorStyles.centeredGreyMiniLabel);
        }
        
        private void DrawConfigSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            var newConfig = (BuildEnvironmentConfig)EditorGUILayout.ObjectField(
                "Config Asset", 
                currentConfig, 
                typeof(BuildEnvironmentConfig), 
                false
            );
            
            if (newConfig != currentConfig)
            {
                currentConfig = newConfig;
            }
            
            if (GUILayout.Button("Create New", GUILayout.Width(80)))
            {
                CreateDefaultConfig();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (currentConfig != null)
            {
                EditorGUILayout.LabelField("Path: " + AssetDatabase.GetAssetPath(currentConfig), EditorStyles.miniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCurrentEnvironment()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Current Environment", EditorStyles.boldLabel);
            
            var newEnvironment = (TirexGame.Utils.Editor.BuildSystem.BuildEnvironment)EditorGUILayout.EnumPopup("Selected Environment", currentConfig.SelectedEnvironment);
            if (newEnvironment != currentConfig.SelectedEnvironment)
            {
                currentConfig.SelectedEnvironment = newEnvironment;
                EditorUtility.SetDirty(currentConfig);
            }
            
            EditorGUILayout.LabelField($"Active Defines ({currentConfig.GetCurrentDefines().Count}):", EditorStyles.boldLabel);
            var currentDefines = currentConfig.GetCurrentDefines();
            if (currentDefines.Count > 0)
            {
                var definesText = string.Join(", ", currentDefines);
                EditorGUILayout.LabelField(definesText, EditorStyles.wordWrappedLabel);
            }
            else
            {
                EditorGUILayout.LabelField("No defines set for current environment", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEnvironmentDefines()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Environment Scripting Defines", EditorStyles.boldLabel);
            
            // Development
            DrawDefinesList("Development", currentConfig.DevelopmentDefines, currentConfig.SelectedEnvironment == TirexGame.Utils.Editor.BuildSystem.BuildEnvironment.Development);
            EditorGUILayout.Space(5);
            
            // Staging
            DrawDefinesList("Staging", currentConfig.StagingDefines, currentConfig.SelectedEnvironment == TirexGame.Utils.Editor.BuildSystem.BuildEnvironment.Staging);
            EditorGUILayout.Space(5);
            
            // Production
            DrawDefinesList("Production", currentConfig.ProductionDefines, currentConfig.SelectedEnvironment == TirexGame.Utils.Editor.BuildSystem.BuildEnvironment.Production);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDefinesList(string environmentName, System.Collections.Generic.List<string> defines, bool isActive)
        {
            if (isActive)
            {
                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f, 1f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.backgroundColor = originalColor;
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{environmentName} ({defines.Count})", EditorStyles.boldLabel);
            
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                defines.Add("");
                EditorUtility.SetDirty(currentConfig);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Track which item to remove to avoid modifying collection during iteration
            int indexToRemove = -1;
            
            for (int i = 0; i < defines.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                var newDefine = EditorGUILayout.TextField(defines[i]);
                if (newDefine != defines[i])
                {
                    defines[i] = newDefine;
                    EditorUtility.SetDirty(currentConfig);
                }
                
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    indexToRemove = i;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Remove item if requested
            if (indexToRemove >= 0)
            {
                defines.RemoveAt(indexToRemove);
                EditorUtility.SetDirty(currentConfig);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBuildSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
            
            var newAutoApply = EditorGUILayout.Toggle("Auto Apply on Build", currentConfig.AutoApplyOnBuild);
            if (newAutoApply != currentConfig.AutoApplyOnBuild)
            {
                currentConfig.AutoApplyOnBuild = newAutoApply;
                EditorUtility.SetDirty(currentConfig);
            }
            
            var newShowDialog = EditorGUILayout.Toggle("Show Build Dialog", currentConfig.ShowBuildDialog);
            if (newShowDialog != currentConfig.ShowBuildDialog)
            {
                currentConfig.ShowBuildDialog = newShowDialog;
                EditorUtility.SetDirty(currentConfig);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply Current Environment"))
            {
                ApplyEnvironmentDefines(currentConfig);
            }
            
            if (GUILayout.Button("Clear All Defines"))
            {
                if (EditorUtility.DisplayDialog("Clear All Defines", 
                    "This will remove all scripting defines from the current build target. Continue?", 
                    "Yes", "Cancel"))
                {
                    ClearAllDefines();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced", true);
            if (showAdvancedSettings)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Export Config"))
                {
                    ExportConfig();
                }
                
                if (GUILayout.Button("Import Config"))
                {
                    ImportConfig();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawNoConfigFound()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("No Configuration Found", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Create a new BuildEnvironmentConfigStandalone asset to get started.", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create Default Config"))
            {
                CreateDefaultConfig();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Config Management
        
        private void LoadExistingConfig()
        {
            if (currentConfig == null)
            {
                currentConfig = FindExistingConfig();
            }
        }
        
        private static BuildEnvironmentConfig FindExistingConfig()
        {
            var guids = AssetDatabase.FindAssets(CONFIG_SEARCH_FILTER);
            
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BuildEnvironmentConfig>(path);
            }
            
            return null;
        }
        
        private static void CreateDefaultConfig()
        {
            // Ensure directory exists
            if (!Directory.Exists(DEFAULT_CONFIG_PATH))
            {
                Directory.CreateDirectory(DEFAULT_CONFIG_PATH);
            }
            
            // Create the config asset
            var config = CreateInstance<BuildEnvironmentConfig>();
            
            // Set default values
            config.SelectedEnvironment = TirexGame.Utils.Editor.BuildSystem.BuildEnvironment.Development;
            config.DevelopmentDefines.AddRange(new[] { "DEVELOPMENT", "DEBUG_MODE", "ENABLE_LOGS" });
            config.StagingDefines.AddRange(new[] { "STAGING", "ENABLE_LOGS" });
            config.ProductionDefines.AddRange(new[] { "PRODUCTION", "RELEASE" });
            
            var fullPath = Path.Combine(DEFAULT_CONFIG_PATH, DEFAULT_CONFIG_NAME);
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created BuildEnvironmentConfigStandalone at: {fullPath}");
            
            // Select and focus the new asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
        
        #endregion
        
        #region Environment Management
        
        private static void ApplyEnvironmentDefines(BuildEnvironmentConfig config)
        {
            if (config == null)
            {
                Debug.LogError("Cannot apply environment defines: config is null");
                return;
            }
            
            var currentDefines = config.GetCurrentDefines();
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            // Get existing defines to preserve non-environment specific ones
            var existingDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var existingDefinesList = existingDefines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            
            // Remove old environment defines
            var allEnvironmentDefines = config.DevelopmentDefines
                .Concat(config.StagingDefines)
                .Concat(config.ProductionDefines)
                .Distinct()
                .ToList();
            
            var filteredDefines = existingDefinesList.Where(d => !allEnvironmentDefines.Contains(d)).ToList();
            
            // Add current environment defines
            var newDefines = filteredDefines.Concat(currentDefines).Distinct().ToList();
            var newDefinesString = string.Join(";", newDefines);
            
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefinesString);
            
            Debug.Log($"Applied {config.SelectedEnvironment} environment defines: {string.Join(", ", newDefines)}");
            
            EditorUtility.DisplayDialog(
                "Environment Applied", 
                $"Applied {config.SelectedEnvironment} environment defines to {EditorUserBuildSettings.selectedBuildTargetGroup}", 
                "OK"
            );
        }
        
        private void ClearAllDefines()
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, "");
            Debug.Log($"Cleared all scripting defines for {EditorUserBuildSettings.selectedBuildTargetGroup}");
        }
        
        private void ExportConfig()
        {
            if (currentConfig == null) return;
            
            string json = JsonUtility.ToJson(currentConfig, true);
            string path = EditorUtility.SaveFilePanel("Export Build Environment Config", "", "BuildEnvironmentConfigStandalone.json", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                Debug.Log($"Exported config to: {path}");
            }
        }
        
        private void ImportConfig()
        {
            string path = EditorUtility.OpenFilePanel("Import Build Environment Config", "", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    JsonUtility.FromJsonOverwrite(json, currentConfig);
                    EditorUtility.SetDirty(currentConfig);
                    Debug.Log($"Imported config from: {path}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to import config: {ex.Message}");
                    EditorUtility.DisplayDialog("Import Error", $"Failed to import config:\n{ex.Message}", "OK");
                }
            }
        }
        
        private void ConvertFromOriginalConfig()
        {
            EditorUtility.DisplayDialog("Feature Removed", "Convert from original config feature has been removed as this is now the main version.", "OK");
        }
        
        #endregion
    }
}
#endif