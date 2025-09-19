using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TirexGame.Utils.Localization
{
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "TirexGame/Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [Header("General Settings")]
        [SerializeField] private bool autoDetectSystemLanguage = true;
        [SerializeField] private bool savePrefernceToPlayerPrefs = true;
        [SerializeField] private string playerPrefsKey = "SelectedLanguage";

        [Header("Languages")]
        [SerializeField] private List<LanguageData> supportedLanguages = new List<LanguageData>();
        [SerializeField] private LanguageData defaultLanguage;

        [Header("Tables")]
        [SerializeField] private List<LocalizationTable> localizationTables = new List<LocalizationTable>();

        [Header("Debug")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private bool showMissingKeyWarnings = true;
        [SerializeField] private string missingKeyFormat = "[MISSING: {0}]";

        // Events
        public static event System.Action<LanguageData> OnLanguageChanged;

        // Properties
        public bool AutoDetectSystemLanguage => autoDetectSystemLanguage;
        public bool SavePreferenceToPlayerPrefs => savePrefernceToPlayerPrefs;
        public string PlayerPrefsKey => playerPrefsKey;
        public List<LanguageData> SupportedLanguages => supportedLanguages;
        public LanguageData DefaultLanguage => defaultLanguage;
        public List<LocalizationTable> LocalizationTables => localizationTables;
        public bool EnableDebugMode => enableDebugMode;
        public bool ShowMissingKeyWarnings => showMissingKeyWarnings;
        public string MissingKeyFormat => missingKeyFormat;

        // Runtime Properties
        private LanguageData _currentLanguage;
        public LanguageData CurrentLanguage 
        { 
            get => _currentLanguage ?? defaultLanguage;
            set
            {
                if (_currentLanguage != value && value != null)
                {
                    _currentLanguage = value;
                    OnLanguageChanged?.Invoke(_currentLanguage);
                    
                    if (savePrefernceToPlayerPrefs)
                    {
                        PlayerPrefs.SetString(playerPrefsKey, _currentLanguage.LanguageCode);
                    }
                }
            }
        }

        // Methods
        public void Initialize()
        {
            if (CurrentLanguage == null)
            {
                LoadSavedLanguage();
            }

            if (CurrentLanguage == null && autoDetectSystemLanguage)
            {
                DetectSystemLanguage();
            }

            if (CurrentLanguage == null)
            {
                CurrentLanguage = defaultLanguage;
            }
        }

        public LanguageData GetLanguageByCode(string languageCode)
        {
            return supportedLanguages.FirstOrDefault(lang => 
                lang.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
        }

        public LocalizationTable GetTableForLanguage(LanguageData language)
        {
            return localizationTables.FirstOrDefault(table => table.Language == language);
        }

        public LocalizationTable GetCurrentLanguageTable()
        {
            return GetTableForLanguage(CurrentLanguage);
        }

        public string GetLocalizedText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (showMissingKeyWarnings)
                    Debug.LogWarning("[LocalizationSettings] Attempted to get localized text with null or empty key");
                return string.Format(missingKeyFormat, "NULL_KEY");
            }

            var currentTable = GetCurrentLanguageTable();
            if (currentTable != null)
            {
                string value = currentTable.GetValue(key);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            // Fallback to default language
            if (CurrentLanguage != defaultLanguage && defaultLanguage != null)
            {
                var defaultTable = GetTableForLanguage(defaultLanguage);
                if (defaultTable != null)
                {
                    string value = defaultTable.GetValue(key);
                    if (!string.IsNullOrEmpty(value))
                        return value;
                }
            }

            // Return missing key format
            if (showMissingKeyWarnings)
            {
                Debug.LogWarning($"[LocalizationSettings] Missing localization key: {key}");
            }

            return string.Format(missingKeyFormat, key);
        }

        public void AddLanguage(LanguageData language)
        {
            if (language != null && !supportedLanguages.Contains(language))
            {
                supportedLanguages.Add(language);
                
                // Create a table for this language if it doesn't exist
                if (!localizationTables.Any(table => table.Language == language))
                {
                    CreateTableForLanguage(language);
                }

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }

        public bool RemoveLanguage(LanguageData language)
        {
            if (language == defaultLanguage)
            {
                Debug.LogError("Cannot remove default language");
                return false;
            }

            bool removed = supportedLanguages.Remove(language);
            if (removed)
            {
                // Remove corresponding table
                var table = GetTableForLanguage(language);
                if (table != null)
                {
                    localizationTables.Remove(table);
                }

                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }

            return removed;
        }

        public void SetDefaultLanguage(LanguageData language)
        {
            if (supportedLanguages.Contains(language))
            {
                defaultLanguage = language;
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }

        public List<string> GetAllKeys()
        {
            var allKeys = new HashSet<string>();
            foreach (var table in localizationTables)
            {
                foreach (var key in table.GetAllKeys())
                {
                    allKeys.Add(key);
                }
            }
            return allKeys.ToList();
        }

        public List<string> GetMissingTranslationsForLanguage(LanguageData language)
        {
            var table = GetTableForLanguage(language);
            return table?.GetMissingTranslations() ?? new List<string>();
        }

        private void LoadSavedLanguage()
        {
            if (savePrefernceToPlayerPrefs && PlayerPrefs.HasKey(playerPrefsKey))
            {
                string savedCode = PlayerPrefs.GetString(playerPrefsKey);
                var savedLanguage = GetLanguageByCode(savedCode);
                if (savedLanguage != null)
                {
                    _currentLanguage = savedLanguage;
                }
            }
        }

        private void DetectSystemLanguage()
        {
            var systemLang = Application.systemLanguage;
            string systemCode = GetLanguageCodeFromSystemLanguage(systemLang);
            var detectedLanguage = GetLanguageByCode(systemCode);
            
            if (detectedLanguage != null)
            {
                _currentLanguage = detectedLanguage;
            }
        }

        private string GetLanguageCodeFromSystemLanguage(SystemLanguage systemLanguage)
        {
            return systemLanguage switch
            {
                SystemLanguage.English => "en",
                SystemLanguage.Vietnamese => "vi",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Chinese => "zh",
                SystemLanguage.ChineseSimplified => "zh",
                SystemLanguage.ChineseTraditional => "zh",
                SystemLanguage.Korean => "ko",
                SystemLanguage.Spanish => "es",
                SystemLanguage.French => "fr",
                SystemLanguage.German => "de",
                SystemLanguage.Italian => "it",
                SystemLanguage.Portuguese => "pt",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Arabic => "ar",
                SystemLanguage.Hindi => "hi",
                SystemLanguage.Thai => "th",
                _ => "en"
            };
        }

        private void CreateTableForLanguage(LanguageData language)
        {
            #if UNITY_EDITOR
            var table = CreateInstance<LocalizationTable>();
            table.SetLanguage(language);
            table.name = $"LocalizationTable_{language.LanguageCode}";
            
            string path = $"Assets/Utils/Localization/Resources/Tables/";
            if (!UnityEditor.AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            
            UnityEditor.AssetDatabase.CreateAsset(table, $"{path}{table.name}.asset");
            localizationTables.Add(table);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }

        private void OnValidate()
        {
            // Ensure default language is in supported languages
            if (defaultLanguage != null && !supportedLanguages.Contains(defaultLanguage))
            {
                supportedLanguages.Insert(0, defaultLanguage);
            }

            // Set first language as default if none selected
            if (defaultLanguage == null && supportedLanguages.Count > 0)
            {
                defaultLanguage = supportedLanguages[0];
            }
        }
    }
}