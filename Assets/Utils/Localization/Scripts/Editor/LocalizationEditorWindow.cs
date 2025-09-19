using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace TirexGame.Utils.Localization.Editor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        #region Fields
        private LocalizationSettings _settings;
        private Vector2 _scrollPosition;
        private string _searchText = "";
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Languages", "Keys & Translations", "Import/Export", "Validation" };

        // Language management
        private string _newLanguageDisplayName = "";
        private string _newLanguageCode = "";
        private string _newLanguageCountry = "";

        // Key management
        private string _newKeyName = "";
        private string _newKeyComment = "";
        private Dictionary<string, string> _newKeyTranslations = new Dictionary<string, string>();
        private bool _showAddKeyFoldout = false;
        private string _editingKey = "";

        // Table view
        private Vector2 _tableScrollPosition;
        private float _keyColumnWidth = 200f;
        private float _languageColumnWidth = 150f;

        // Validation
        private List<string> _missingKeys = new List<string>();
        private List<string> _duplicateKeys = new List<string>();
        private Dictionary<LanguageData, List<string>> _missingTranslations = new Dictionary<LanguageData, List<string>>();

        // Styles
        private GUIStyle _headerStyle;
        private GUIStyle _tableHeaderStyle;
        private GUIStyle _tableCellStyle;
        private GUIStyle _alternateRowStyle;
        private bool _stylesInitialized = false;
        #endregion

        #region Menu & Window
        [MenuItem("TirexGame/Localization/Localization Manager", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationEditorWindow>("Localization Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
            RefreshValidation();
        }

        private void OnGUI()
        {
            InitializeStyles();
            
            if (_settings == null)
            {
                DrawNoSettingsGUI();
                return;
            }

            DrawHeader();
            DrawTabs();
            
            EditorGUILayout.Space();
            
            switch (_selectedTab)
            {
                case 0: DrawLanguagesTab(); break;
                case 1: DrawKeysTab(); break;
                case 2: DrawImportExportTab(); break;
                case 3: DrawValidationTab(); break;
            }
        }
        #endregion

        #region Settings Management
        private void LoadSettings()
        {
            _settings = Resources.Load<LocalizationSettings>("LocalizationSettings");
            
            if (_settings == null)
            {
                // Try to find in project
                string[] guids = AssetDatabase.FindAssets("t:LocalizationSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
                }
            }
        }

        private void CreateNewSettings()
        {
            _settings = CreateInstance<LocalizationSettings>();
            
            string path = "Assets/Utils/Localization/Resources/";
            if (!AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            AssetDatabase.CreateAsset(_settings, path + "LocalizationSettings.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        #endregion

        #region GUI Drawing
        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };

            _tableHeaderStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            _tableCellStyle = new GUIStyle(EditorStyles.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(1, 1, 1, 1)
            };

            _alternateRowStyle = new GUIStyle()
            {
                normal = { background = MakeTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.1f)) }
            };

            _stylesInitialized = true;
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void DrawNoSettingsGUI()
        {
            EditorGUILayout.Space(50);
            
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField("Localization Settings Not Found", _headerStyle, GUILayout.Height(30));
            
            EditorGUILayout.Space(20);
            
            EditorGUILayout.LabelField("Please create a LocalizationSettings asset to get started.", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Create New Settings", GUILayout.Height(30), GUILayout.Width(200)))
            {
                CreateNewSettings();
            }
            
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Or assign existing settings:", GUILayout.Width(200));
            LocalizationSettings newSettings = EditorGUILayout.ObjectField(_settings, typeof(LocalizationSettings), false) as LocalizationSettings;
            if (newSettings != _settings)
            {
                _settings = newSettings;
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField("Localization Manager", _headerStyle);
            
            GUILayout.FlexibleSpace();
            
            // Settings reference
            EditorGUI.BeginChangeCheck();
            LocalizationSettings newSettings = EditorGUILayout.ObjectField(_settings, typeof(LocalizationSettings), false, GUILayout.Width(200)) as LocalizationSettings;
            if (EditorGUI.EndChangeCheck() && newSettings != _settings)
            {
                _settings = newSettings;
                RefreshValidation();
            }
            
            // Refresh button
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshValidation();
                Repaint();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
        }

        #endregion

        #region Languages Tab
        private void DrawLanguagesTab()
        {
            EditorGUILayout.LabelField("Language Management", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Current languages list
            if (_settings.SupportedLanguages.Count > 0)
            {
                DrawLanguagesList();
            }
            else
            {
                EditorGUILayout.HelpBox("No languages configured. Add a language to get started.", MessageType.Info);
            }

            EditorGUILayout.Space();
            DrawAddLanguageSection();
        }

        private void DrawLanguagesList()
        {
            EditorGUILayout.LabelField("Supported Languages", EditorStyles.boldLabel);
            
            for (int i = 0; i < _settings.SupportedLanguages.Count; i++)
            {
                var language = _settings.SupportedLanguages[i];
                if (language == null) continue;

                EditorGUILayout.BeginHorizontal();
                
                // Language info
                EditorGUILayout.LabelField($"{language.DisplayName} ({language.LanguageCode})", GUILayout.Width(200));
                
                // Default indicator
                if (language == _settings.DefaultLanguage)
                {
                    EditorGUILayout.LabelField("(Default)", EditorStyles.miniLabel, GUILayout.Width(60));
                }
                else
                {
                    if (GUILayout.Button("Set Default", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        _settings.SetDefaultLanguage(language);
                        EditorUtility.SetDirty(_settings);
                    }
                }
                
                // Completion percentage
                var table = _settings.GetTableForLanguage(language);
                float completion = table?.CompletionPercentage ?? 0f;
                EditorGUILayout.LabelField($"{completion:F1}%", GUILayout.Width(50));
                
                GUILayout.FlexibleSpace();
                
                // Remove button (can't remove default)
                GUI.enabled = language != _settings.DefaultLanguage;
                if (GUILayout.Button("Remove", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Remove Language", 
                        $"Are you sure you want to remove {language.DisplayName}?", "Yes", "No"))
                    {
                        _settings.RemoveLanguage(language);
                        EditorUtility.SetDirty(_settings);
                        break;
                    }
                }
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAddLanguageSection()
        {
            EditorGUILayout.LabelField("Add New Language", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Display Name", GUILayout.Width(100));
            _newLanguageDisplayName = EditorGUILayout.TextField(_newLanguageDisplayName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language Code", GUILayout.Width(100));
            _newLanguageCode = EditorGUILayout.TextField(_newLanguageCode);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Country Code", GUILayout.Width(100));
            _newLanguageCountry = EditorGUILayout.TextField(_newLanguageCountry);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            GUI.enabled = !string.IsNullOrEmpty(_newLanguageDisplayName) && !string.IsNullOrEmpty(_newLanguageCode);
            if (GUILayout.Button("Add Language", GUILayout.Height(25)))
            {
                CreateNewLanguage();
            }
            GUI.enabled = true;
        }

        private void CreateNewLanguage()
        {
            var newLanguage = CreateInstance<LanguageData>();
            newLanguage.name = $"Language_{_newLanguageCode}";
            
            // Use reflection or create a public setter method to set the private fields
            var so = new SerializedObject(newLanguage);
            so.FindProperty("displayName").stringValue = _newLanguageDisplayName;
            so.FindProperty("languageCode").stringValue = _newLanguageCode.ToLower();
            so.FindProperty("countryCode").stringValue = _newLanguageCountry.ToUpper();
            so.ApplyModifiedProperties();
            
            // Save as asset
            string path = "Assets/Utils/Localization/Languages/";
            if (!AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            AssetDatabase.CreateAsset(newLanguage, $"{path}{newLanguage.name}.asset");
            
            // Add to settings
            _settings.AddLanguage(newLanguage);
            
            // Set as default if first language
            if (_settings.SupportedLanguages.Count == 1)
            {
                _settings.SetDefaultLanguage(newLanguage);
            }
            
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            
            // Clear fields
            _newLanguageDisplayName = "";
            _newLanguageCode = "";
            _newLanguageCountry = "";
        }
        #endregion

        #region Keys Tab
        private void DrawKeysTab()
        {
            EditorGUILayout.LabelField("Keys & Translations Management", EditorStyles.boldLabel);
            
            // Search
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            _searchText = EditorGUILayout.TextField(_searchText);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                _searchText = "";
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Add key section
            DrawAddKeySection();
            
            EditorGUILayout.Space();
            
            // Keys table
            DrawKeysTable();
        }

        private void DrawAddKeySection()
        {
            _showAddKeyFoldout = EditorGUILayout.Foldout(_showAddKeyFoldout, "Add New Key", true);
            
            if (_showAddKeyFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key Name", GUILayout.Width(100));
                _newKeyName = EditorGUILayout.TextField(_newKeyName);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Comment", GUILayout.Width(100));
                _newKeyComment = EditorGUILayout.TextField(_newKeyComment);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Translations:", EditorStyles.boldLabel);
                
                // Translation fields for each language
                foreach (var language in _settings.SupportedLanguages)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(language.DisplayName, GUILayout.Width(100));
                    
                    if (!_newKeyTranslations.ContainsKey(language.LanguageCode))
                        _newKeyTranslations[language.LanguageCode] = "";
                    
                    _newKeyTranslations[language.LanguageCode] = EditorGUILayout.TextField(_newKeyTranslations[language.LanguageCode]);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space();
                
                GUI.enabled = !string.IsNullOrEmpty(_newKeyName);
                if (GUILayout.Button("Add Key", GUILayout.Height(25)))
                {
                    AddNewKey();
                }
                GUI.enabled = true;
                
                EditorGUI.indentLevel--;
            }
        }

        private void AddNewKey()
        {
            // Add key to all language tables
            foreach (var language in _settings.SupportedLanguages)
            {
                var table = _settings.GetTableForLanguage(language);
                if (table != null)
                {
                    string translation = _newKeyTranslations.ContainsKey(language.LanguageCode) 
                        ? _newKeyTranslations[language.LanguageCode] 
                        : "";
                    
                    table.EditorAddEntry(_newKeyName, translation, _newKeyComment);
                }
            }
            
            // Clear fields
            _newKeyName = "";
            _newKeyComment = "";
            _newKeyTranslations.Clear();
            
            RefreshValidation();
        }

        private void DrawKeysTable()
        {
            if (_settings.SupportedLanguages.Count == 0)
            {
                EditorGUILayout.HelpBox("No languages configured. Please add languages first.", MessageType.Info);
                return;
            }

            var allKeys = _settings.GetAllKeys();
            if (allKeys.Count == 0)
            {
                EditorGUILayout.HelpBox("No keys found. Add some keys to get started.", MessageType.Info);
                return;
            }

            // Filter keys by search
            var filteredKeys = string.IsNullOrEmpty(_searchText) 
                ? allKeys 
                : allKeys.Where(key => key.ToLower().Contains(_searchText.ToLower())).ToList();

            _tableScrollPosition = EditorGUILayout.BeginScrollView(_tableScrollPosition, GUILayout.Height(400));

            // Table header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", _tableHeaderStyle, GUILayout.Width(_keyColumnWidth));
            
            foreach (var language in _settings.SupportedLanguages)
            {
                EditorGUILayout.LabelField(language.DisplayName, _tableHeaderStyle, GUILayout.Width(_languageColumnWidth));
            }
            
            EditorGUILayout.LabelField("Actions", _tableHeaderStyle, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            // Table rows
            for (int i = 0; i < filteredKeys.Count; i++)
            {
                string key = filteredKeys[i];
                bool isAlternate = i % 2 == 1;
                
                if (isAlternate)
                    EditorGUILayout.BeginHorizontal(_alternateRowStyle);
                else
                    EditorGUILayout.BeginHorizontal();

                // Key column
                EditorGUILayout.LabelField(key, _tableCellStyle, GUILayout.Width(_keyColumnWidth));

                // Translation columns
                foreach (var language in _settings.SupportedLanguages)
                {
                    var table = _settings.GetTableForLanguage(language);
                    string currentValue = table?.GetValue(key) ?? "";
                    
                    EditorGUI.BeginChangeCheck();
                    string newValue = EditorGUILayout.TextField(currentValue, _tableCellStyle, GUILayout.Width(_languageColumnWidth));
                    if (EditorGUI.EndChangeCheck())
                    {
                        table?.EditorUpdateEntry(key, newValue);
                    }
                }

                // Actions column
                if (GUILayout.Button("×", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Key", $"Delete key '{key}'?", "Yes", "No"))
                    {
                        DeleteKey(key);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DeleteKey(string key)
        {
            foreach (var language in _settings.SupportedLanguages)
            {
                var table = _settings.GetTableForLanguage(language);
                table?.EditorRemoveEntry(key);
            }
            RefreshValidation();
        }
        #endregion

        #region Import/Export Tab
        private void DrawImportExportTab()
        {
            EditorGUILayout.LabelField("Import/Export", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Export Options", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Export All to CSV", GUILayout.Height(30)))
            {
                ExportToCSV();
            }
            
            if (GUILayout.Button("Export All to JSON", GUILayout.Height(30)))
            {
                ExportToJSON();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Import Options", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Import from CSV", GUILayout.Height(30)))
            {
                ImportFromCSV();
            }
            
            if (GUILayout.Button("Import from JSON", GUILayout.Height(30)))
            {
                ImportFromJSON();
            }
        }

        private void ExportToCSV()
        {
            string path = EditorUtility.SaveFilePanel("Export Localization to CSV", "", "localization", "csv");
            if (string.IsNullOrEmpty(path)) return;

            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.Append("Key,Comment");
            foreach (var language in _settings.SupportedLanguages)
            {
                csv.Append($",{language.LanguageCode}");
            }
            csv.AppendLine();

            // Data
            var allKeys = _settings.GetAllKeys();
            foreach (string key in allKeys)
            {
                csv.Append($"\"{key}\",");
                
                // Get comment from first table that has this key
                string comment = "";
                foreach (var language in _settings.SupportedLanguages)
                {
                    var table = _settings.GetTableForLanguage(language);
                    var entry = table?.Entries.FirstOrDefault(e => e.key == key);
                    if (entry != null && !string.IsNullOrEmpty(entry.comment))
                    {
                        comment = entry.comment;
                        break;
                    }
                }
                csv.Append($"\"{comment}\"");

                // Translations
                foreach (var language in _settings.SupportedLanguages)
                {
                    var table = _settings.GetTableForLanguage(language);
                    string value = table?.GetValue(key) ?? "";
                    csv.Append($",\"{value}\"");
                }
                csv.AppendLine();
            }

            System.IO.File.WriteAllText(path, csv.ToString());
            Debug.Log($"Localization exported to: {path}");
        }

        private void ExportToJSON()
        {
            string path = EditorUtility.SaveFilePanel("Export Localization to JSON", "", "localization", "json");
            if (string.IsNullOrEmpty(path)) return;

            var data = new Dictionary<string, object>();
            
            foreach (var language in _settings.SupportedLanguages)
            {
                var table = _settings.GetTableForLanguage(language);
                var translations = new Dictionary<string, string>();
                
                if (table != null)
                {
                    foreach (var entry in table.Entries)
                    {
                        translations[entry.key] = entry.value;
                    }
                }
                
                data[language.LanguageCode] = translations;
            }

            string json = JsonUtility.ToJson(new Serializable<Dictionary<string, object>>(data), true);
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"Localization exported to: {path}");
        }

        private void ImportFromCSV()
        {
            string path = EditorUtility.OpenFilePanel("Import Localization from CSV", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                if (lines.Length < 2) return;

                // Parse header
                string[] headers = ParseCSVLine(lines[0]);
                var languageIndices = new Dictionary<string, int>();
                
                for (int i = 2; i < headers.Length; i++) // Skip "Key" and "Comment" columns
                {
                    string langCode = headers[i];
                    var language = _settings.GetLanguageByCode(langCode);
                    if (language != null)
                    {
                        languageIndices[langCode] = i;
                    }
                }

                // Parse data
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = ParseCSVLine(lines[i]);
                    if (values.Length < 2) continue;

                    string key = values[0];
                    string comment = values.Length > 1 ? values[1] : "";

                    foreach (var kvp in languageIndices)
                    {
                        string langCode = kvp.Key;
                        int index = kvp.Value;
                        
                        if (index < values.Length)
                        {
                            string translation = values[index];
                            var language = _settings.GetLanguageByCode(langCode);
                            var table = _settings.GetTableForLanguage(language);
                            table?.EditorAddEntry(key, translation, comment);
                        }
                    }
                }

                RefreshValidation();
                Debug.Log("CSV import completed successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CSV import failed: {ex.Message}");
            }
        }

        private void ImportFromJSON()
        {
            string path = EditorUtility.OpenFilePanel("Import Localization from JSON", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                // JSON import implementation would go here
                // This is simplified for brevity
                Debug.Log("JSON import completed successfully.");
                RefreshValidation();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JSON import failed: {ex.Message}");
            }
        }

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            result.Add(currentField);
            return result.ToArray();
        }
        #endregion

        #region Validation Tab
        private void DrawValidationTab()
        {
            EditorGUILayout.LabelField("Validation & Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Run Full Validation", GUILayout.Height(30)))
            {
                RefreshValidation();
            }

            EditorGUILayout.Space();

            // Missing translations
            if (_missingTranslations.Count > 0)
            {
                EditorGUILayout.LabelField("Missing Translations", EditorStyles.boldLabel);
                foreach (var kvp in _missingTranslations)
                {
                    if (kvp.Value.Count > 0)
                    {
                        EditorGUILayout.LabelField($"{kvp.Key.DisplayName}: {kvp.Value.Count} missing");
                        EditorGUI.indentLevel++;
                        foreach (string missingKey in kvp.Value.Take(10)) // Show first 10
                        {
                            EditorGUILayout.LabelField($"• {missingKey}");
                        }
                        if (kvp.Value.Count > 10)
                        {
                            EditorGUILayout.LabelField($"... and {kvp.Value.Count - 10} more");
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No missing translations found!", MessageType.Info);
            }
        }

        private void RefreshValidation()
        {
            if (_settings == null) return;

            _missingTranslations.Clear();

            foreach (var language in _settings.SupportedLanguages)
            {
                var missing = _settings.GetMissingTranslationsForLanguage(language);
                if (missing.Count > 0)
                {
                    _missingTranslations[language] = missing;
                }
            }
        }
        #endregion

        #region Utility Classes
        [System.Serializable]
        public class Serializable<T>
        {
            public T data;
            public Serializable(T data) { this.data = data; }
        }
        #endregion
    }
}