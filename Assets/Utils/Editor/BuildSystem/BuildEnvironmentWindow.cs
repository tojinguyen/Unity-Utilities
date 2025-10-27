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
        
        private const string DEFAULT_CONFIG_PATH = "Assets/Utils/Editor/BuildSystem/Resources";
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
            EditorGUILayout.LabelField("Build Environment Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Manage scripting defines for different build environments", EditorStyles.helpBox);
        }
        
        private void DrawConfigSelection()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Config Asset:", GUILayout.Width(100));
            
            var newConfig = (BuildEnvironmentConfig)EditorGUILayout.ObjectField(currentConfig, typeof(BuildEnvironmentConfig), false);
            if (newConfig != currentConfig)
            {
                currentConfig = newConfig;
                if (currentConfig != null)
                {
                    EditorUtility.SetDirty(currentConfig);
                }
            }
            
            if (GUILayout.Button("Create New", GUILayout.Width(80)))
            {
                CreateDefaultConfig();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawCurrentEnvironment()
        {
            EditorGUILayout.LabelField("Current Environment", EditorStyles.boldLabel);
            
            var newEnvironment = (BuildEnvironment)EditorGUILayout.EnumPopup("Selected Environment", currentConfig.SelectedEnvironment);
            if (newEnvironment != currentConfig.SelectedEnvironment)
            {
                currentConfig.SelectedEnvironment = newEnvironment;
                EditorUtility.SetDirty(currentConfig);
            }
            
            // Show current defines count
            var currentDefines = currentConfig.GetCurrentDefines();
            EditorGUILayout.LabelField($"Active Defines: {currentDefines.Count}", EditorStyles.helpBox);
        }
        
        private void DrawEnvironmentDefines()
        {
            EditorGUILayout.LabelField("Environment Defines", EditorStyles.boldLabel);
            
            // Development
            DrawDefinesList("Development", currentConfig.DevelopmentDefines, currentConfig.SelectedEnvironment == BuildEnvironment.Development);
            EditorGUILayout.Space(5);
            
            // Staging
            DrawDefinesList("Staging", currentConfig.StagingDefines, currentConfig.SelectedEnvironment == BuildEnvironment.Staging);
            EditorGUILayout.Space(5);
            
            // Production
            DrawDefinesList("Production", currentConfig.ProductionDefines, currentConfig.SelectedEnvironment == BuildEnvironment.Production);
        }
        
        private void DrawDefinesList(string environmentName, System.Collections.Generic.List<string> defines, bool isActive)
        {
            var backgroundColor = isActive ? new Color(0.3f, 0.7f, 0.3f, 0.2f) : Color.clear;
            
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
                
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    indexToRemove = i;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Remove item after the loop to avoid GUI state issues
            if (indexToRemove >= 0)
            {
                defines.RemoveAt(indexToRemove);
                EditorUtility.SetDirty(currentConfig);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBuildSettings()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Build Settings", true);
            
            if (showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                var newAutoApply = EditorGUILayout.Toggle("Auto Apply On Build", currentConfig.AutoApplyOnBuild);
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
        }
        
        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply Current Environment"))
            {
                ApplyEnvironmentDefines(currentConfig);
            }
            
            if (GUILayout.Button("Clear All Defines"))
            {
                if (EditorUtility.DisplayDialog("Clear Defines", "Clear all scripting defines for current platform?", "Clear", "Cancel"))
                {
                    ClearAllDefines();
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
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
        
        private void DrawNoConfigFound()
        {
            EditorGUILayout.Space(50);
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("No Build Environment Config Found", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create a new configuration to get started:", EditorStyles.helpBox);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Create Build Environment Config", GUILayout.Height(30)))
            {
                CreateDefaultConfig();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Private Methods
        
        private void LoadExistingConfig()
        {
            currentConfig = FindExistingConfig();
        }
        
        private static BuildEnvironmentConfig FindExistingConfig()
        {
            string[] guids = AssetDatabase.FindAssets(CONFIG_SEARCH_FILTER);
            
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BuildEnvironmentConfig>(assetPath);
            }
            
            return null;
        }
        
        private static void CreateDefaultConfig()
        {
            // Create directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(DEFAULT_CONFIG_PATH))
            {
                string[] pathParts = DEFAULT_CONFIG_PATH.Split('/');
                string currentPath = pathParts[0];
                
                for (int i = 1; i < pathParts.Length; i++)
                {
                    string nextPath = currentPath + "/" + pathParts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                    }
                    currentPath = nextPath;
                }
            }
            
            // Create the config asset
            var config = CreateInstance<BuildEnvironmentConfig>();
            
            string fullPath = Path.Combine(DEFAULT_CONFIG_PATH, DEFAULT_CONFIG_NAME);
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
            
            Debug.Log($"Created BuildEnvironmentConfig at: {fullPath}");
            
            // Update window if open
            var window = GetWindow<BuildEnvironmentWindow>(false);
            if (window != null)
            {
                window.currentConfig = config;
                window.Repaint();
            }
        }
        
        private static void ApplyEnvironmentDefines(BuildEnvironmentConfig config)
        {
            if (config == null) return;
            
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var currentDefinesList = currentDefines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            
            // Remove old environment defines
            var allEnvironments = System.Enum.GetValues(typeof(BuildEnvironment)).Cast<BuildEnvironment>();
            foreach (var env in allEnvironments)
            {
                if (env == config.SelectedEnvironment) continue;
                
                var envDefines = config.GetDefinesForEnvironment(env);
                foreach (var define in envDefines)
                {
                    currentDefinesList.Remove(define);
                }
            }
            
            // Add new environment defines
            var newDefines = config.GetCurrentDefines();
            foreach (var define in newDefines)
            {
                if (!string.IsNullOrWhiteSpace(define) && !currentDefinesList.Contains(define))
                {
                    currentDefinesList.Add(define);
                }
            }
            
            // Apply the updated defines
            string newDefinesString = string.Join(";", currentDefinesList);
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
            string path = EditorUtility.SaveFilePanel("Export Build Environment Config", "", "BuildEnvironmentConfig.json", "json");
            
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
        
        #endregion
    }
}
#endif