using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Tirex.Game.Utils.Localization.Editor
{
    /// <summary>
    /// Editor window for managing localization across the project
    /// Provides tools for finding localized components, batch operations, and project-wide validation
    /// </summary>
    public class LocalizationEditorWindow : EditorWindow
    {
        private LocalizationConfig selectedConfig;
        private Vector2 scrollPosition;
        private int selectedTab = 0;
        private string[] tabNames = { "Overview", "Components", "Validation", "Tools" };

        // Component search
        private List<LocalizedText> localizedTexts = new List<LocalizedText>();
        private List<LocalizedImage> localizedImages = new List<LocalizedImage>();
        private bool autoRefreshComponents = true;

        // Validation
        private List<string> validationResults = new List<string>();
        private bool showOnlyIssues = true;

        [MenuItem("Tools/Tirex Game Utils/Localization Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationEditorWindow>("Localization Manager");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            RefreshComponents();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            // Header
            DrawHeader();
            
            EditorGUILayout.Space(5);

            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Draw tab content
            switch (selectedTab)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawComponentsTab(); break;
                case 2: DrawValidationTab(); break;
                case 3: DrawToolsTab(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw the header with config selection
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("box");
            
            EditorGUILayout.LabelField("Localization Config:", GUILayout.Width(120));
            
            var newConfig = EditorGUILayout.ObjectField(selectedConfig, typeof(LocalizationConfig), false) as LocalizationConfig;
            if (newConfig != selectedConfig)
            {
                selectedConfig = newConfig;
                OnConfigChanged();
            }

            if (GUILayout.Button("Create New", GUILayout.Width(80)))
            {
                CreateNewConfig();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw overview tab with general information
        /// </summary>
        private void DrawOverviewTab()
        {
            if (selectedConfig == null)
            {
                EditorGUILayout.HelpBox("Please select or create a LocalizationConfig to get started.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Configuration Overview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Default Language: {selectedConfig.DefaultLanguage}");
            EditorGUILayout.LabelField($"Supported Languages: {string.Join(", ", selectedConfig.SupportedLanguages)}");
            EditorGUILayout.LabelField($"Text Entries: {selectedConfig.GetAllTextKeys().Count}");
            EditorGUILayout.LabelField($"Sprite Entries: {selectedConfig.GetAllSpriteKeys().Count}");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Project Statistics", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"LocalizedText Components: {localizedTexts.Count}");
            EditorGUILayout.LabelField($"LocalizedImage Components: {localizedImages.Count}");

            if (GUILayout.Button("Refresh Component Count"))
            {
                RefreshComponents();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Initialize Manager"))
            {
                InitializeManager();
            }
            if (GUILayout.Button("Validate All"))
            {
                ValidateAll();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update All Components"))
            {
                UpdateAllComponents();
            }
            if (GUILayout.Button("Open Config Inspector"))
            {
                Selection.activeObject = selectedConfig;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw components tab with component management
        /// </summary>
        private void DrawComponentsTab()
        {
            EditorGUILayout.BeginHorizontal();
            autoRefreshComponents = EditorGUILayout.Toggle("Auto Refresh", autoRefreshComponents);
            if (GUILayout.Button("Manual Refresh", GUILayout.Width(100)))
            {
                RefreshComponents();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // LocalizedText components
            EditorGUILayout.LabelField($"LocalizedText Components ({localizedTexts.Count})", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (localizedTexts.Count == 0)
            {
                EditorGUILayout.LabelField("No LocalizedText components found in scene.");
            }
            else
            {
                foreach (var localizedText in localizedTexts)
                {
                    if (localizedText == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.ObjectField(localizedText, typeof(LocalizedText), true, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Key: {localizedText.LocalizationKey}", GUILayout.MinWidth(150));
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = localizedText.gameObject;
                    }
                    
                    if (GUILayout.Button("Update", GUILayout.Width(60)))
                    {
                        localizedText.UpdateText();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // LocalizedImage components
            EditorGUILayout.LabelField($"LocalizedImage Components ({localizedImages.Count})", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (localizedImages.Count == 0)
            {
                EditorGUILayout.LabelField("No LocalizedImage components found in scene.");
            }
            else
            {
                foreach (var localizedImage in localizedImages)
                {
                    if (localizedImage == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUILayout.ObjectField(localizedImage, typeof(LocalizedImage), true, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Key: {localizedImage.LocalizationKey}", GUILayout.MinWidth(150));
                    
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = localizedImage.gameObject;
                    }
                    
                    if (GUILayout.Button("Update", GUILayout.Width(60)))
                    {
                        localizedImage.UpdateSprite();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw validation tab with project validation tools
        /// </summary>
        private void DrawValidationTab()
        {
            EditorGUILayout.BeginHorizontal();
            showOnlyIssues = EditorGUILayout.Toggle("Show Only Issues", showOnlyIssues);
            if (GUILayout.Button("Run Validation", GUILayout.Width(120)))
            {
                RunProjectValidation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (validationResults.Count == 0)
            {
                EditorGUILayout.LabelField("No validation results. Click 'Run Validation' to check project.");
            }
            else
            {
                foreach (var result in validationResults)
                {
                    if (result.Contains("✓"))
                    {
                        if (!showOnlyIssues)
                        {
                            EditorGUILayout.LabelField(result, EditorStyles.label);
                        }
                    }
                    else if (result.Contains("⚠"))
                    {
                        EditorGUILayout.LabelField(result, EditorStyles.helpBox);
                    }
                    else if (result.Contains("✗"))
                    {
                        EditorGUILayout.LabelField(result, EditorStyles.helpBox);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(result, EditorStyles.boldLabel);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw tools tab with utility functions
        /// </summary>
        private void DrawToolsTab()
        {
            EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Add LocalizedText to Selected UI Text"))
            {
                AddLocalizedTextToSelection();
            }

            if (GUILayout.Button("Add LocalizedImage to Selected UI Image"))
            {
                AddLocalizedImageToSelection();
            }

            if (GUILayout.Button("Remove All Localized Components"))
            {
                if (EditorUtility.DisplayDialog("Remove Components", 
                    "Remove all LocalizedText and LocalizedImage components from scene?", "Yes", "No"))
                {
                    RemoveAllLocalizedComponents();
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Key Management", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button("Generate Keys from Text Components"))
            {
                GenerateKeysFromTextComponents();
            }

            if (GUILayout.Button("Find Unused Keys"))
            {
                FindUnusedKeys();
            }

            if (GUILayout.Button("Find Missing Keys"))
            {
                FindMissingKeys();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Refresh component lists
        /// </summary>
        private void RefreshComponents()
        {
            localizedTexts = FindObjectsOfType<LocalizedText>().ToList();
            localizedImages = FindObjectsOfType<LocalizedImage>().ToList();
        }

        /// <summary>
        /// Called when config changes
        /// </summary>
        private void OnConfigChanged()
        {
            if (autoRefreshComponents)
            {
                RefreshComponents();
            }
        }

        /// <summary>
        /// Create new localization config
        /// </summary>
        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Localization Config",
                "LocalizationConfig",
                "asset",
                "Choose location for new Localization Config");

            if (!string.IsNullOrEmpty(path))
            {
                var config = CreateInstance<LocalizationConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                selectedConfig = config;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
            }
        }

        /// <summary>
        /// Initialize localization manager in scene
        /// </summary>
        private void InitializeManager()
        {
            var manager = FindObjectOfType<LocalizationManager>();
            if (manager == null)
            {
                var go = new GameObject("LocalizationManager");
                manager = go.AddComponent<LocalizationManager>();
            }

            if (selectedConfig != null)
            {
                manager.Initialize(selectedConfig);
                EditorUtility.DisplayDialog("Manager Initialized", 
                    "LocalizationManager has been initialized with the selected config.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Config Selected", 
                    "Please select a LocalizationConfig first.", "OK");
            }
        }

        /// <summary>
        /// Validate all localization in project
        /// </summary>
        private void ValidateAll()
        {
            RunProjectValidation();
            selectedTab = 2; // Switch to validation tab
        }

        /// <summary>
        /// Update all localized components
        /// </summary>
        private void UpdateAllComponents()
        {
            foreach (var localizedText in localizedTexts)
            {
                if (localizedText != null)
                    localizedText.UpdateText();
            }

            foreach (var localizedImage in localizedImages)
            {
                if (localizedImage != null)
                    localizedImage.UpdateSprite();
            }

            Debug.Log($"Updated {localizedTexts.Count + localizedImages.Count} localized components.");
        }

        /// <summary>
        /// Run comprehensive project validation
        /// </summary>
        private void RunProjectValidation()
        {
            validationResults.Clear();
            validationResults.Add("=== Project Validation Results ===");

            // Validate config
            if (selectedConfig == null)
            {
                validationResults.Add("✗ No LocalizationConfig selected");
                return;
            }

            validationResults.Add("✓ LocalizationConfig found");

            // Validate manager
            var manager = FindObjectOfType<LocalizationManager>();
            if (manager == null)
            {
                validationResults.Add("⚠ No LocalizationManager found in scene");
            }
            else
            {
                validationResults.Add("✓ LocalizationManager found in scene");
            }

            // Validate components
            RefreshComponents();
            validationResults.Add($"✓ Found {localizedTexts.Count} LocalizedText components");
            validationResults.Add($"✓ Found {localizedImages.Count} LocalizedImage components");

            // Validate component keys
            int invalidComponents = 0;
            foreach (var localizedText in localizedTexts)
            {
                if (string.IsNullOrEmpty(localizedText.LocalizationKey))
                {
                    invalidComponents++;
                    validationResults.Add($"⚠ LocalizedText on {localizedText.gameObject.name} has empty key");
                }
                else if (!selectedConfig.HasTextKey(localizedText.LocalizationKey))
                {
                    invalidComponents++;
                    validationResults.Add($"⚠ LocalizedText key '{localizedText.LocalizationKey}' not found in config");
                }
            }

            foreach (var localizedImage in localizedImages)
            {
                if (string.IsNullOrEmpty(localizedImage.LocalizationKey))
                {
                    invalidComponents++;
                    validationResults.Add($"⚠ LocalizedImage on {localizedImage.gameObject.name} has empty key");
                }
                else if (!selectedConfig.HasSpriteKey(localizedImage.LocalizationKey))
                {
                    invalidComponents++;
                    validationResults.Add($"⚠ LocalizedImage key '{localizedImage.LocalizationKey}' not found in config");
                }
            }

            if (invalidComponents == 0)
            {
                validationResults.Add("✓ All component keys are valid");
            }

            validationResults.Add("=== Validation Complete ===");
        }

        /// <summary>
        /// Add LocalizedText to selected UI Text components
        /// </summary>
        private void AddLocalizedTextToSelection()
        {
            int addedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                var textComponent = obj.GetComponent<UnityEngine.UI.Text>();
                var tmpComponent = obj.GetComponent<TMPro.TextMeshProUGUI>();
                
                if ((textComponent != null || tmpComponent != null) && obj.GetComponent<LocalizedText>() == null)
                {
                    obj.AddComponent<LocalizedText>();
                    addedCount++;
                }
            }

            Debug.Log($"Added LocalizedText to {addedCount} objects.");
            RefreshComponents();
        }

        /// <summary>
        /// Add LocalizedImage to selected UI Image components
        /// </summary>
        private void AddLocalizedImageToSelection()
        {
            int addedCount = 0;
            foreach (var obj in Selection.gameObjects)
            {
                var imageComponent = obj.GetComponent<UnityEngine.UI.Image>();
                
                if (imageComponent != null && obj.GetComponent<LocalizedImage>() == null)
                {
                    obj.AddComponent<LocalizedImage>();
                    addedCount++;
                }
            }

            Debug.Log($"Added LocalizedImage to {addedCount} objects.");
            RefreshComponents();
        }

        /// <summary>
        /// Remove all localized components from scene
        /// </summary>
        private void RemoveAllLocalizedComponents()
        {
            int removedCount = 0;
            
            foreach (var component in localizedTexts)
            {
                if (component != null)
                {
                    DestroyImmediate(component);
                    removedCount++;
                }
            }

            foreach (var component in localizedImages)
            {
                if (component != null)
                {
                    DestroyImmediate(component);
                    removedCount++;
                }
            }

            Debug.Log($"Removed {removedCount} localized components.");
            RefreshComponents();
        }

        /// <summary>
        /// Generate localization keys from existing text components
        /// </summary>
        private void GenerateKeysFromTextComponents()
        {
            // This is a placeholder - implement based on your specific needs
            Debug.Log("Generate keys feature would analyze existing UI text and suggest/create localization keys.");
        }

        /// <summary>
        /// Find unused localization keys
        /// </summary>
        private void FindUnusedKeys()
        {
            if (selectedConfig == null) return;

            var usedKeys = new HashSet<string>();
            
            foreach (var localizedText in localizedTexts)
            {
                if (!string.IsNullOrEmpty(localizedText.LocalizationKey))
                {
                    usedKeys.Add(localizedText.LocalizationKey);
                }
            }

            foreach (var localizedImage in localizedImages)
            {
                if (!string.IsNullOrEmpty(localizedImage.LocalizationKey))
                {
                    usedKeys.Add(localizedImage.LocalizationKey);
                }
            }

            var allTextKeys = selectedConfig.GetAllTextKeys();
            var allSpriteKeys = selectedConfig.GetAllSpriteKeys();
            
            var unusedTextKeys = allTextKeys.Where(key => !usedKeys.Contains(key));
            var unusedSpriteKeys = allSpriteKeys.Where(key => !usedKeys.Contains(key));

            Debug.Log($"Unused text keys: {string.Join(", ", unusedTextKeys)}");
            Debug.Log($"Unused sprite keys: {string.Join(", ", unusedSpriteKeys)}");
        }

        /// <summary>
        /// Find missing localization keys
        /// </summary>
        private void FindMissingKeys()
        {
            if (selectedConfig == null) return;

            var missingKeys = new List<string>();

            foreach (var localizedText in localizedTexts)
            {
                if (!string.IsNullOrEmpty(localizedText.LocalizationKey) && 
                    !selectedConfig.HasTextKey(localizedText.LocalizationKey))
                {
                    missingKeys.Add($"Text key: {localizedText.LocalizationKey}");
                }
            }

            foreach (var localizedImage in localizedImages)
            {
                if (!string.IsNullOrEmpty(localizedImage.LocalizationKey) && 
                    !selectedConfig.HasSpriteKey(localizedImage.LocalizationKey))
                {
                    missingKeys.Add($"Sprite key: {localizedImage.LocalizationKey}");
                }
            }

            if (missingKeys.Count > 0)
            {
                Debug.Log($"Missing keys:\n{string.Join("\n", missingKeys)}");
            }
            else
            {
                Debug.Log("No missing keys found!");
            }
        }

        private void Update()
        {
            if (autoRefreshComponents)
            {
                RefreshComponents();
            }
        }
    }
}
