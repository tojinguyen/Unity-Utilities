using UnityEngine;
using UnityEditor;
using System.IO;

namespace Tirex.Game.Utils.Localization.Editor
{
    /// <summary>
    /// Custom editor for LocalizationConfig ScriptableObject
    /// Provides CSV import/export functionality and localization management tools
    /// </summary>
    [CustomEditor(typeof(LocalizationConfig))]
    public class LocalizationConfigEditor : UnityEditor.Editor
    {
        private LocalizationConfig config;
        private string csvExportPath = "";
        private string csvImportPath = "";
        private bool showCSVTools = true;
        private bool showDataInspector = false;
        private Vector2 textEntriesScrollPosition;
        private Vector2 spriteEntriesScrollPosition;

        private void OnEnable()
        {
            config = (LocalizationConfig)target;
            
            // Set default paths
            if (string.IsNullOrEmpty(csvExportPath))
            {
                csvExportPath = Path.Combine(Application.dataPath, "Localization_Export.csv");
            }
            if (string.IsNullOrEmpty(csvImportPath))
            {
                csvImportPath = Path.Combine(Application.dataPath, "Localization_Import.csv");
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector first
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Localization Tools", EditorStyles.boldLabel);

            // CSV Tools Section
            showCSVTools = EditorGUILayout.Foldout(showCSVTools, "CSV Import/Export Tools", true);
            if (showCSVTools)
            {
                EditorGUILayout.BeginVertical("box");
                DrawCSVTools();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(5);

            // Data Inspector Section
            showDataInspector = EditorGUILayout.Foldout(showDataInspector, "Data Inspector", true);
            if (showDataInspector)
            {
                EditorGUILayout.BeginVertical("box");
                DrawDataInspector();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(5);

            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            DrawQuickActions();
            EditorGUILayout.EndVertical();

            // Apply changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }
        }

        /// <summary>
        /// Draw CSV import/export tools
        /// </summary>
        private void DrawCSVTools()
        {
            EditorGUILayout.LabelField("CSV Export", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            csvExportPath = EditorGUILayout.TextField("Export Path:", csvExportPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.SaveFilePanel("Export CSV", Path.GetDirectoryName(csvExportPath), 
                    Path.GetFileNameWithoutExtension(csvExportPath), "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    csvExportPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export to CSV"))
            {
                ExportToCSV();
            }
            if (GUILayout.Button("Create Template"))
            {
                CreateCSVTemplate();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("CSV Import", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            csvImportPath = EditorGUILayout.TextField("Import Path:", csvImportPath);
            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string path = EditorUtility.OpenFilePanel("Import CSV", Path.GetDirectoryName(csvImportPath), "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    csvImportPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import from CSV"))
            {
                ImportFromCSV();
            }
            if (GUILayout.Button("Import (Merge)"))
            {
                ImportFromCSV(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Open CSV Folder"))
            {
                string folderPath = Path.GetDirectoryName(csvExportPath);
                if (Directory.Exists(folderPath))
                {
                    EditorUtility.RevealInFinder(folderPath);
                }
            }
        }

        /// <summary>
        /// Draw data inspector with current entries
        /// </summary>
        private void DrawDataInspector()
        {
            var textEntries = config.GetTextEntries();
            var spriteEntries = config.GetSpriteEntries();

            EditorGUILayout.LabelField($"Text Entries: {textEntries.Count}", EditorStyles.boldLabel);
            
            if (textEntries.Count > 0)
            {
                textEntriesScrollPosition = EditorGUILayout.BeginScrollView(textEntriesScrollPosition, 
                    GUILayout.Height(150));
                
                foreach (var entry in textEntries)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(entry.Key, GUILayout.Width(150));
                    
                    foreach (var language in config.SupportedLanguages)
                    {
                        string value = entry.GetValue(language);
                        EditorGUILayout.LabelField($"{language}: {(string.IsNullOrEmpty(value) ? "[Empty]" : value)}", 
                            GUILayout.MinWidth(100));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Sprite Entries: {spriteEntries.Count}", EditorStyles.boldLabel);
            
            if (spriteEntries.Count > 0)
            {
                spriteEntriesScrollPosition = EditorGUILayout.BeginScrollView(spriteEntriesScrollPosition, 
                    GUILayout.Height(100));
                
                foreach (var entry in spriteEntries)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(entry.Key, GUILayout.Width(150));
                    
                    foreach (var language in config.SupportedLanguages)
                    {
                        var sprite = entry.GetSprite(language);
                        EditorGUILayout.LabelField($"{language}: {(sprite != null ? sprite.name : "[None]")}", 
                            GUILayout.MinWidth(100));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// Draw quick action buttons
        /// </summary>
        private void DrawQuickActions()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All Data"))
            {
                if (EditorUtility.DisplayDialog("Clear All Data", 
                    "Are you sure you want to clear all localization data? This cannot be undone.", 
                    "Yes", "No"))
                {
                    config.ClearAllData();
                    EditorUtility.SetDirty(config);
                }
            }
            
            if (GUILayout.Button("Validate Data"))
            {
                ValidateData();
            }
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Find Missing Translations"))
            {
                FindMissingTranslations();
            }

            if (GUILayout.Button("Create Test Manager"))
            {
                CreateTestManager();
            }
        }

        /// <summary>
        /// Export localization data to CSV
        /// </summary>
        private void ExportToCSV()
        {
            if (LocalizationCSVUtility.ExportToCSV(config, csvExportPath))
            {
                EditorUtility.DisplayDialog("Export Successful", 
                    $"Localization data exported to:\n{csvExportPath}", "OK");
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("Export Failed", 
                    "Failed to export localization data. Check the console for details.", "OK");
            }
        }

        /// <summary>
        /// Import localization data from CSV
        /// </summary>
        private void ImportFromCSV(bool clearExisting = true)
        {
            if (!File.Exists(csvImportPath))
            {
                EditorUtility.DisplayDialog("File Not Found", 
                    $"CSV file not found:\n{csvImportPath}", "OK");
                return;
            }

            string action = clearExisting ? "replace" : "merge with";
            if (EditorUtility.DisplayDialog("Import CSV", 
                $"This will {action} the current localization data. Continue?", 
                "Yes", "No"))
            {
                if (LocalizationCSVUtility.ImportFromCSV(config, csvImportPath, clearExisting))
                {
                    EditorUtility.DisplayDialog("Import Successful", 
                        "Localization data imported successfully!", "OK");
                    EditorUtility.SetDirty(config);
                }
                else
                {
                    EditorUtility.DisplayDialog("Import Failed", 
                        "Failed to import localization data. Check the console for details.", "OK");
                }
            }
        }

        /// <summary>
        /// Create CSV template file
        /// </summary>
        private void CreateCSVTemplate()
        {
            string[] sampleKeys = { "ui_welcome", "ui_start_game", "ui_settings", "ui_quit" };
            
            if (LocalizationCSVUtility.CreateTemplate(config, csvExportPath, sampleKeys))
            {
                EditorUtility.DisplayDialog("Template Created", 
                    $"CSV template created at:\n{csvExportPath}", "OK");
                AssetDatabase.Refresh();
            }
            else
            {
                EditorUtility.DisplayDialog("Template Creation Failed", 
                    "Failed to create CSV template. Check the console for details.", "OK");
            }
        }

        /// <summary>
        /// Validate localization data for missing translations
        /// </summary>
        private void ValidateData()
        {
            var textEntries = config.GetTextEntries();
            int missingCount = 0;
            string missingDetails = "";

            foreach (var entry in textEntries)
            {
                foreach (var language in config.SupportedLanguages)
                {
                    string value = entry.GetValue(language);
                    if (string.IsNullOrEmpty(value))
                    {
                        missingCount++;
                        missingDetails += $"Key: {entry.Key}, Language: {language}\n";
                    }
                }
            }

            if (missingCount > 0)
            {
                EditorUtility.DisplayDialog("Validation Results", 
                    $"Found {missingCount} missing translations:\n\n{missingDetails}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Results", 
                    "All translations are complete!", "OK");
            }
        }

        /// <summary>
        /// Find and display missing translations
        /// </summary>
        private void FindMissingTranslations()
        {
            var textEntries = config.GetTextEntries();
            var missingTranslations = new System.Text.StringBuilder();

            foreach (var entry in textEntries)
            {
                var missingLanguages = new System.Collections.Generic.List<string>();
                
                foreach (var language in config.SupportedLanguages)
                {
                    string value = entry.GetValue(language);
                    if (string.IsNullOrEmpty(value))
                    {
                        missingLanguages.Add(language.ToString());
                    }
                }

                if (missingLanguages.Count > 0)
                {
                    missingTranslations.AppendLine($"{entry.Key}: {string.Join(", ", missingLanguages)}");
                }
            }

            if (missingTranslations.Length > 0)
            {
                Debug.Log($"Missing translations:\n{missingTranslations.ToString()}");
            }
            else
            {
                Debug.Log("No missing translations found!");
            }
        }

        /// <summary>
        /// Create a test LocalizationManager in the scene
        /// </summary>
        private void CreateTestManager()
        {
            var existing = FindObjectOfType<LocalizationManager>();
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Manager Exists", 
                    "LocalizationManager already exists in scene. Replace it?", "Yes", "No"))
                {
                    return;
                }
                DestroyImmediate(existing.gameObject);
            }

            var go = new GameObject("LocalizationManager");
            var manager = go.AddComponent<LocalizationManager>();
            
            // Use reflection to set the config field
            var configField = typeof(LocalizationManager).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(manager, config);

            EditorUtility.DisplayDialog("Manager Created", 
                "LocalizationManager created in scene with this config assigned.", "OK");
        }
    }
}
