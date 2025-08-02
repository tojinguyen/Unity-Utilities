using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Central manager for handling localization throughout the application
    /// Implements Singleton pattern for global access
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("LocalizationManager");
                    instance = go.AddComponent<LocalizationManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private LocalizationConfig config;
        [SerializeField] private bool initializeOnAwake = true;

        // Events
        public static event Action<LanguageCode> OnLanguageChanged;

        // Private fields
        private LanguageCode currentLanguage;
        private bool isInitialized = false;

        // Properties
        public LanguageCode CurrentLanguage => currentLanguage;
        public LocalizationConfig Config => config;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            // Implement singleton pattern
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (initializeOnAwake)
                {
                    Initialize();
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize the localization system
        /// </summary>
        /// <param name="localizationConfig">Optional config to use instead of the serialized one</param>
        public void Initialize(LocalizationConfig localizationConfig = null)
        {
            if (isInitialized) return;

            if (localizationConfig != null)
                config = localizationConfig;

            if (config == null)
            {
                Debug.LogError("LocalizationManager: No configuration provided!");
                return;
            }

            // Load saved language or detect system language
            LoadLanguagePreference();
            
            isInitialized = true;
            Debug.Log($"LocalizationManager initialized with language: {currentLanguage}");
        }

        /// <summary>
        /// Change the current language
        /// </summary>
        public void SetLanguage(LanguageCode language)
        {
            if (!config.IsLanguageSupported(language))
            {
                Debug.LogWarning($"Language {language} is not supported. Using default language {config.DefaultLanguage}");
                language = config.DefaultLanguage;
            }

            if (currentLanguage != language)
            {
                currentLanguage = language;
                
                // Save language preference
                if (config.SaveLanguagePreference)
                {
                    PlayerPrefs.SetString(config.LanguagePreferenceKey, language.ToString());
                    PlayerPrefs.Save();
                }

                // Notify all listeners
                OnLanguageChanged?.Invoke(currentLanguage);
                Debug.Log($"Language changed to: {language}");
            }
        }

        /// <summary>
        /// Get localized text by key
        /// </summary>
        public string GetText(string key)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized!");
                return key;
            }

            return config.GetText(key, currentLanguage);
        }

        /// <summary>
        /// Get localized text by key with string formatting
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            var text = GetText(key);
            try
            {
                return string.Format(text, args);
            }
            catch (FormatException)
            {
                Debug.LogWarning($"String format error for key: {key}");
                return text;
            }
        }

        /// <summary>
        /// Get localized sprite by key
        /// </summary>
        public Sprite GetSprite(string key)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized!");
                return null;
            }

            return config.GetSprite(key, currentLanguage);
        }

        /// <summary>
        /// Get language information for current language
        /// </summary>
        public LanguageInfo GetCurrentLanguageInfo()
        {
            return LocalizationConfig.GetLanguageInfo(currentLanguage);
        }

        /// <summary>
        /// Get all supported languages information
        /// </summary>
        public List<LanguageInfo> GetSupportedLanguagesInfo()
        {
            var languages = new List<LanguageInfo>();
            foreach (var lang in config.SupportedLanguages)
            {
                languages.Add(LocalizationConfig.GetLanguageInfo(lang));
            }
            return languages;
        }

        /// <summary>
        /// Check if a text key exists
        /// </summary>
        public bool HasTextKey(string key)
        {
            if (!isInitialized) return false;
            return config.GetAllTextKeys().Contains(key);
        }

        /// <summary>
        /// Check if a sprite key exists
        /// </summary>
        public bool HasSpriteKey(string key)
        {
            if (!isInitialized) return false;
            return config.GetAllSpriteKeys().Contains(key);
        }

        /// <summary>
        /// Load language preference from PlayerPrefs or detect system language
        /// </summary>
        private void LoadLanguagePreference()
        {
            LanguageCode targetLanguage = config.DefaultLanguage;

            // Try to load saved language preference
            if (config.SaveLanguagePreference && PlayerPrefs.HasKey(config.LanguagePreferenceKey))
            {
                string savedLanguage = PlayerPrefs.GetString(config.LanguagePreferenceKey);
                if (Enum.TryParse<LanguageCode>(savedLanguage, out var parsedLanguage) && 
                    config.IsLanguageSupported(parsedLanguage))
                {
                    targetLanguage = parsedLanguage;
                }
            }
            // Auto-detect system language if enabled
            else if (config.AutoDetectSystemLanguage)
            {
                var systemLanguage = DetectSystemLanguage();
                if (config.IsLanguageSupported(systemLanguage))
                {
                    targetLanguage = systemLanguage;
                }
            }

            currentLanguage = targetLanguage;
        }

        /// <summary>
        /// Detect system language and convert to LanguageCode
        /// </summary>
        private LanguageCode DetectSystemLanguage()
        {
            var systemLanguage = Application.systemLanguage;
            
            return systemLanguage switch
            {
                SystemLanguage.English => LanguageCode.EN,
                SystemLanguage.Vietnamese => LanguageCode.VI,
                SystemLanguage.Japanese => LanguageCode.JP,
                SystemLanguage.Korean => LanguageCode.KO,
                SystemLanguage.Chinese or SystemLanguage.ChineseSimplified => LanguageCode.ZH,
                SystemLanguage.Thai => LanguageCode.TH,
                SystemLanguage.Indonesian => LanguageCode.ID,
                SystemLanguage.Spanish => LanguageCode.ES,
                SystemLanguage.French => LanguageCode.FR,
                SystemLanguage.German => LanguageCode.DE,
                SystemLanguage.Russian => LanguageCode.RU,
                SystemLanguage.Portuguese => LanguageCode.PT,
                SystemLanguage.Arabic => LanguageCode.AR,
                SystemLanguage.Hindi => LanguageCode.HI,
                SystemLanguage.Italian => LanguageCode.IT,
                _ => LanguageCode.EN
            };
        }

        /// <summary>
        /// Static method for easy access to GetText
        /// </summary>
        public static string GetLocalizedText(string key)
        {
            return Instance.GetText(key);
        }

        /// <summary>
        /// Static method for easy access to GetText with formatting
        /// </summary>
        public static string GetLocalizedText(string key, params object[] args)
        {
            return Instance.GetText(key, args);
        }

        /// <summary>
        /// Static method for easy access to GetSprite
        /// </summary>
        public static Sprite GetLocalizedSprite(string key)
        {
            return Instance.GetSprite(key);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
