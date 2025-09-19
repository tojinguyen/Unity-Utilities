using UnityEngine;
using UnityEditor;
using System.IO;

namespace TirexGame.Utils.Localization.Editor
{
    public class LocalizationSetupWizard : EditorWindow
    {
        private int _currentStep = 0;
        private readonly string[] _stepTitles = 
        {
            "Welcome",
            "Create Settings",
            "Add Languages", 
            "Setup Complete"
        };

        // Step 1: Settings
        private LocalizationSettings _settings;
        private bool _createNewSettings = true;

        // Step 2: Languages
        private bool _addEnglish = true;
        private bool _addVietnamese = false;
        private bool _addJapanese = false;
        private bool _addChinese = false;
        private string _defaultLanguageCode = "en";

        private Vector2 _scrollPosition;

        [MenuItem("TirexGame/Localization/Setup Wizard", priority = 0)]
        public static void ShowWizard()
        {
            var window = GetWindow<LocalizationSetupWizard>("Localization Setup Wizard");
            window.minSize = new Vector2(500, 400);
            window.maxSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawProgressBar();
            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_currentStep)
            {
                case 0: DrawWelcomeStep(); break;
                case 1: DrawSettingsStep(); break;
                case 2: DrawLanguagesStep(); break;
                case 3: DrawCompleteStep(); break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawNavigationButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Localization Setup Wizard", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Step {_currentStep + 1} of {_stepTitles.Length}");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProgressBar()
        {
            float progress = (_currentStep + 1) / (float)_stepTitles.Length;
            Rect rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, progress, _stepTitles[_currentStep]);
        }

        private void DrawWelcomeStep()
        {
            EditorGUILayout.LabelField("Welcome to Localization Setup", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("This wizard will help you set up the localization system for your project.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("The wizard will:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Create LocalizationSettings asset", EditorStyles.label);
            EditorGUILayout.LabelField("• Set up supported languages", EditorStyles.label);
            EditorGUILayout.LabelField("• Create localization tables", EditorStyles.label);
            EditorGUILayout.LabelField("• Add LocalizationManager to scene", EditorStyles.label);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Make sure you have TextMeshPro imported if you plan to use TMP components.", MessageType.Info);
        }

        private void DrawSettingsStep()
        {
            EditorGUILayout.LabelField("Localization Settings", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            _createNewSettings = EditorGUILayout.Toggle("Create New Settings", _createNewSettings);

            if (_createNewSettings)
            {
                EditorGUILayout.HelpBox("A new LocalizationSettings asset will be created in Assets/Utils/Localization/Resources/", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("Select Existing Settings:");
                _settings = EditorGUILayout.ObjectField(_settings, typeof(LocalizationSettings), false) as LocalizationSettings;
                
                if (_settings == null)
                {
                    EditorGUILayout.HelpBox("Please select existing settings or choose to create new ones.", MessageType.Warning);
                }
            }
        }

        private void DrawLanguagesStep()
        {
            EditorGUILayout.LabelField("Language Configuration", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Select languages to add:", EditorStyles.boldLabel);
            
            _addEnglish = EditorGUILayout.Toggle("English (en)", _addEnglish);
            _addVietnamese = EditorGUILayout.Toggle("Vietnamese (vi)", _addVietnamese);
            _addJapanese = EditorGUILayout.Toggle("Japanese (ja)", _addJapanese);
            _addChinese = EditorGUILayout.Toggle("Chinese (zh)", _addChinese);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Default Language:", EditorStyles.boldLabel);
            
            string[] availableDefaults = GetSelectedLanguageCodes();
            if (availableDefaults.Length > 0)
            {
                int currentIndex = System.Array.IndexOf(availableDefaults, _defaultLanguageCode);
                if (currentIndex == -1) currentIndex = 0;
                
                int newIndex = EditorGUILayout.Popup("Default Language", currentIndex, availableDefaults);
                _defaultLanguageCode = availableDefaults[newIndex];
            }
            else
            {
                EditorGUILayout.HelpBox("Please select at least one language.", MessageType.Warning);
            }
        }

        private void DrawCompleteStep()
        {
            EditorGUILayout.LabelField("Setup Complete!", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Your localization system has been set up successfully!", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Next Steps:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Open the Localization Manager from TirexGame/Localization menu", EditorStyles.label);
            EditorGUILayout.LabelField("• Add localization keys and translations", EditorStyles.label);
            EditorGUILayout.LabelField("• Add LocalizedTextBinder components to your UI texts", EditorStyles.label);
            EditorGUILayout.LabelField("• Test language switching in Play mode", EditorStyles.label);

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Localization Manager", GUILayout.Height(30)))
            {
                LocalizationEditorWindow.ShowWindow();
                Close();
            }
        }

        private void DrawNavigationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _currentStep > 0;
            if (GUILayout.Button("Back", GUILayout.Width(100)))
            {
                _currentStep--;
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            if (_currentStep < _stepTitles.Length - 1)
            {
                GUI.enabled = CanProceedToNextStep();
                if (GUILayout.Button("Next", GUILayout.Width(100)))
                {
                    if (_currentStep == 1 && _createNewSettings)
                    {
                        CreateSettings();
                    }
                    else if (_currentStep == 2)
                    {
                        SetupLanguages();
                    }
                    
                    _currentStep++;
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("Finish", GUILayout.Width(100)))
                {
                    Close();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool CanProceedToNextStep()
        {
            switch (_currentStep)
            {
                case 0: return true;
                case 1: return _createNewSettings || _settings != null;
                case 2: return GetSelectedLanguageCodes().Length > 0;
                case 3: return true;
                default: return false;
            }
        }

        private string[] GetSelectedLanguageCodes()
        {
            var codes = new System.Collections.Generic.List<string>();
            if (_addEnglish) codes.Add("en");
            if (_addVietnamese) codes.Add("vi");
            if (_addJapanese) codes.Add("ja");
            if (_addChinese) codes.Add("zh");
            return codes.ToArray();
        }

        private void CreateSettings()
        {
            // Create directory structure
            string resourcesPath = "Assets/Utils/Localization/Resources";
            string languagesPath = "Assets/Utils/Localization/Languages";
            string tablesPath = "Assets/Utils/Localization/Resources/Tables";

            Directory.CreateDirectory(resourcesPath);
            Directory.CreateDirectory(languagesPath);
            Directory.CreateDirectory(tablesPath);

            // Create settings asset
            _settings = CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(_settings, resourcesPath + "/LocalizationSettings.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void SetupLanguages()
        {
            if (_settings == null) return;

            var languageConfigs = new System.Collections.Generic.List<(string code, string name, string country)>();
            
            if (_addEnglish) languageConfigs.Add(("en", "English", "US"));
            if (_addVietnamese) languageConfigs.Add(("vi", "Vietnamese", "VN"));
            if (_addJapanese) languageConfigs.Add(("ja", "Japanese", "JP"));
            if (_addChinese) languageConfigs.Add(("zh", "Chinese", "CN"));

            foreach (var config in languageConfigs)
            {
                // Create language asset
                var language = CreateInstance<LanguageData>();
                var so = new SerializedObject(language);
                so.FindProperty("displayName").stringValue = config.name;
                so.FindProperty("languageCode").stringValue = config.code;
                so.FindProperty("countryCode").stringValue = config.country;
                so.FindProperty("isDefault").boolValue = config.code == _defaultLanguageCode;
                so.ApplyModifiedProperties();

                string languagePath = $"Assets/Utils/Localization/Languages/Language_{config.code}.asset";
                AssetDatabase.CreateAsset(language, languagePath);

                // Add to settings
                _settings.AddLanguage(language);

                if (config.code == _defaultLanguageCode)
                {
                    _settings.SetDefaultLanguage(language);
                }
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();

            // Create LocalizationManager in scene if it doesn't exist
            var existingManager = FindObjectOfType<LocalizationManager>();
            if (existingManager == null)
            {
                var managerGO = new GameObject("LocalizationManager");
                var manager = managerGO.AddComponent<LocalizationManager>();
                manager.EditorSetSettings(_settings);
                
                EditorUtility.SetDirty(managerGO);
            }
        }
    }
}