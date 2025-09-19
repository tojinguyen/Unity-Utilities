using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        #region Singleton
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LocalizationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LocalizationManager");
                        _instance = go.AddComponent<LocalizationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        #endregion

        [Header("Settings")]
        [SerializeField] private LocalizationSettings settings;
        [SerializeField] private bool initializeOnAwake = true;

        // Events
        public static event Action<LanguageData> OnLanguageChanged;
        public static event Action OnLocalizationReady;

        // Properties
        public LocalizationSettings Settings => settings;
        public LanguageData CurrentLanguage => settings?.CurrentLanguage;
        public bool IsReady { get; private set; }

        // Cached data
        private Dictionary<string, LocalizedTextBinder> _registeredBinders = new Dictionary<string, LocalizedTextBinder>();

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (initializeOnAwake)
                {
                    Initialize();
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        #region Initialization
        public void Initialize()
        {
            if (IsReady) return;

            LoadSettings();
            
            if (settings != null)
            {
                settings.Initialize();
                SubscribeToEvents();
                IsReady = true;
                OnLocalizationReady?.Invoke();
                
                Debug.Log($"[LocalizationManager] Initialized with language: {CurrentLanguage?.DisplayName ?? "None"}");
            }
            else
            {
                Debug.LogError("[LocalizationManager] No LocalizationSettings found! Please create and assign settings.");
            }
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = Resources.Load<LocalizationSettings>("LocalizationSettings");
                if (settings == null)
                {
                    Debug.LogWarning("[LocalizationManager] LocalizationSettings not found in Resources folder. Please create one.");
                }
            }
        }

        private void SubscribeToEvents()
        {
            LocalizationSettings.OnLanguageChanged += HandleLanguageChanged;
        }

        private void UnsubscribeFromEvents()
        {
            LocalizationSettings.OnLanguageChanged -= HandleLanguageChanged;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Get localized text for a key
        /// </summary>
        public static string Localize(string key)
        {
            if (Instance.settings == null)
            {
                Debug.LogWarning($"[LocalizationManager] Settings not initialized. Returning key: {key}");
                return key;
            }

            return Instance.settings.GetLocalizedText(key);
        }

        /// <summary>
        /// Get localized text with string formatting
        /// </summary>
        public static string Localize(string key, params object[] args)
        {
            string localizedText = Localize(key);
            try
            {
                return string.Format(localizedText, args);
            }
            catch (FormatException)
            {
                Debug.LogWarning($"[LocalizationManager] Format error for key: {key}");
                return localizedText;
            }
        }

        /// <summary>
        /// Set current language
        /// </summary>
        public static bool SetLanguage(string languageCode)
        {
            if (Instance.settings == null) return false;

            var language = Instance.settings.GetLanguageByCode(languageCode);
            if (language != null)
            {
                Instance.settings.CurrentLanguage = language;
                return true;
            }

            Debug.LogWarning($"[LocalizationManager] Language not found: {languageCode}");
            return false;
        }

        /// <summary>
        /// Set current language
        /// </summary>
        public static bool SetLanguage(LanguageData language)
        {
            if (Instance.settings == null || language == null) return false;

            Instance.settings.CurrentLanguage = language;
            return true;
        }

        /// <summary>
        /// Get current language code
        /// </summary>
        public static string GetCurrentLanguageCode()
        {
            return Instance.CurrentLanguage?.LanguageCode ?? "";
        }

        /// <summary>
        /// Get all supported languages
        /// </summary>
        public static List<LanguageData> GetSupportedLanguages()
        {
            return Instance.settings?.SupportedLanguages ?? new List<LanguageData>();
        }

        /// <summary>
        /// Check if a key exists in current language
        /// </summary>
        public static bool HasKey(string key)
        {
            if (Instance.settings == null) return false;

            var table = Instance.settings.GetCurrentLanguageTable();
            return table?.HasKey(key) ?? false;
        }

        /// <summary>
        /// Get all keys for current language
        /// </summary>
        public static List<string> GetAllKeys()
        {
            return Instance.settings?.GetAllKeys() ?? new List<string>();
        }

        /// <summary>
        /// Register a text binder for automatic updates
        /// </summary>
        public static void RegisterBinder(LocalizedTextBinder binder)
        {
            if (binder != null && !string.IsNullOrEmpty(binder.Key))
            {
                string id = binder.GetInstanceID().ToString();
                Instance._registeredBinders[id] = binder;
            }
        }

        /// <summary>
        /// Unregister a text binder
        /// </summary>
        public static void UnregisterBinder(LocalizedTextBinder binder)
        {
            if (binder != null)
            {
                string id = binder.GetInstanceID().ToString();
                Instance._registeredBinders.Remove(id);
            }
        }

        /// <summary>
        /// Force refresh all registered binders
        /// </summary>
        public static void RefreshAllBinders()
        {
            foreach (var binder in Instance._registeredBinders.Values)
            {
                if (binder != null)
                {
                    binder.UpdateText();
                }
            }
        }
        #endregion

        #region Event Handlers
        private void HandleLanguageChanged(LanguageData newLanguage)
        {
            OnLanguageChanged?.Invoke(newLanguage);
            RefreshAllBinders();
            
            Debug.Log($"[LocalizationManager] Language changed to: {newLanguage.DisplayName}");
        }
        #endregion

        #region Editor Support
        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to set settings
        /// </summary>
        public void EditorSetSettings(LocalizationSettings newSettings)
        {
            settings = newSettings;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Editor-only method to force refresh
        /// </summary>
        public void EditorRefresh()
        {
            if (settings != null)
            {
                RefreshAllBinders();
            }
        }

        /// <summary>
        /// Editor-only method to validate setup
        /// </summary>
        public bool EditorValidateSetup()
        {
            if (settings == null)
            {
                Debug.LogError("LocalizationSettings is not assigned!");
                return false;
            }

            if (settings.SupportedLanguages.Count == 0)
            {
                Debug.LogError("No supported languages configured!");
                return false;
            }

            if (settings.DefaultLanguage == null)
            {
                Debug.LogError("No default language set!");
                return false;
            }

            return true;
        }
        #endif
        #endregion

        #region Utilities
        /// <summary>
        /// Get completion percentage for current language
        /// </summary>
        public static float GetCurrentLanguageCompletionPercentage()
        {
            if (Instance.settings == null) return 0f;

            var table = Instance.settings.GetCurrentLanguageTable();
            return table?.CompletionPercentage ?? 0f;
        }

        /// <summary>
        /// Get missing translations for current language
        /// </summary>
        public static List<string> GetMissingTranslationsForCurrentLanguage()
        {
            if (Instance.settings == null) return new List<string>();

            return Instance.settings.GetMissingTranslationsForLanguage(Instance.CurrentLanguage);
        }

        /// <summary>
        /// Auto-detect and set system language if supported
        /// </summary>
        public static bool TrySetSystemLanguage()
        {
            if (Instance.settings == null) return false;

            var systemLang = Application.systemLanguage;
            string systemCode = GetLanguageCodeFromSystemLanguage(systemLang);
            return SetLanguage(systemCode);
        }

        private static string GetLanguageCodeFromSystemLanguage(SystemLanguage systemLanguage)
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
        #endregion
    }
}