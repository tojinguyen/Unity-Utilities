using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TirexGame.Utils.Localization
{
    [System.Serializable]
    public class LocalizationParameters
    {
        [SerializeField] private string[] parameters = new string[0];
        
        public string[] Parameters => parameters;
        
        public void SetParameters(params string[] newParams)
        {
            parameters = newParams ?? new string[0];
        }
        
        public string GetParameter(int index)
        {
            return index >= 0 && index < parameters.Length ? parameters[index] : "";
        }
        
        public int Count => parameters.Length;
    }

    [AddComponentMenu("TirexGame/Localization/Localized Text Binder")]
    public class LocalizedTextBinder : MonoBehaviour
    {
        [Header("Localization")]
        [SerializeField] private string localizationKey = "";
        [SerializeField] private bool updateOnLanguageChange = true;
        [SerializeField] private bool updateOnStart = true;
        
        [Header("Parameters")]
        [SerializeField] private LocalizationParameters parameters = new LocalizationParameters();
        
        [Header("Fallback")]
        [SerializeField] private string fallbackText = "";
        [SerializeField] private bool useFallbackWhenMissing = true;

        [Header("Font Override")]
        [SerializeField] private bool useLanguageFont = true;
        [SerializeField] private Font customFont;
        [SerializeField] private TMP_FontAsset customTMPFont;

        // Components
        private Text _legacyText;
        private TextMeshProUGUI _tmpText;
        private TextMeshPro _tmp3DText;
        
        // Properties
        public string Key 
        { 
            get => localizationKey;
            set
            {
                if (localizationKey != value)
                {
                    localizationKey = value;
                    if (Application.isPlaying)
                        UpdateText();
                }
            }
        }
        
        public LocalizationParameters Parameters => parameters;
        public string FallbackText => fallbackText;

        // Events
        public event Action<string> OnTextUpdated;

        #region Unity Lifecycle
        private void Awake()
        {
            CacheTextComponents();
        }

        private void Start()
        {
            if (updateOnStart)
            {
                InitializeBinder();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying && updateOnLanguageChange)
            {
                RegisterForUpdates();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                UnregisterFromUpdates();
            }
        }

        private void OnDestroy()
        {
            UnregisterFromUpdates();
        }
        #endregion

        #region Initialization
        private void CacheTextComponents()
        {
            _legacyText = GetComponent<Text>();
            _tmpText = GetComponent<TextMeshProUGUI>();
            _tmp3DText = GetComponent<TextMeshPro>();

            // If no text component found, try to find in children
            if (_legacyText == null && _tmpText == null && _tmp3DText == null)
            {
                _legacyText = GetComponentInChildren<Text>();
                _tmpText = GetComponentInChildren<TextMeshProUGUI>();
                _tmp3DText = GetComponentInChildren<TextMeshPro>();
            }
        }

        private void InitializeBinder()
        {
            if (LocalizationManager.Instance.IsReady)
            {
                LocalizationManager.RegisterBinder(this);
                UpdateText();
                UpdateFont();
            }
            else
            {
                LocalizationManager.OnLocalizationReady += OnLocalizationReady;
            }
        }

        private void OnLocalizationReady()
        {
            LocalizationManager.OnLocalizationReady -= OnLocalizationReady;
            InitializeBinder();
        }

        private void RegisterForUpdates()
        {
            LocalizationManager.RegisterBinder(this);
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void UnregisterFromUpdates()
        {
            LocalizationManager.UnregisterBinder(this);
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        #endregion

        #region Text Update
        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                SetTextValue(fallbackText);
                return;
            }

            string localizedText = GetLocalizedText();
            SetTextValue(localizedText);
            OnTextUpdated?.Invoke(localizedText);
        }

        private string GetLocalizedText()
        {
            if (LocalizationManager.Instance == null || !LocalizationManager.Instance.IsReady)
            {
                return useFallbackWhenMissing ? fallbackText : localizationKey;
            }

            // Get base localized text
            string localizedText = LocalizationManager.Localize(localizationKey);

            // Apply parameters if any
            if (parameters.Count > 0)
            {
                try
                {
                    object[] paramObjects = new object[parameters.Count];
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        paramObjects[i] = parameters.GetParameter(i);
                    }
                    localizedText = string.Format(localizedText, paramObjects);
                }
                catch (FormatException ex)
                {
                    Debug.LogWarning($"[LocalizedTextBinder] Format error for key '{localizationKey}': {ex.Message}");
                }
            }

            // Use fallback if localized text is missing or same as key (indicating missing translation)
            if (useFallbackWhenMissing && (string.IsNullOrEmpty(localizedText) || localizedText.Contains($"[MISSING: {localizationKey}]")))
            {
                return !string.IsNullOrEmpty(fallbackText) ? fallbackText : localizationKey;
            }

            return localizedText;
        }

        private void SetTextValue(string text)
        {
            if (_legacyText != null)
                _legacyText.text = text;
            else if (_tmpText != null)
                _tmpText.text = text;
            else if (_tmp3DText != null)
                _tmp3DText.text = text;
        }

        public void UpdateFont()
        {
            if (!useLanguageFont) return;

            var currentLanguage = LocalizationManager.Instance?.CurrentLanguage;
            if (currentLanguage == null) return;

            // Use FontManager if available, otherwise fallback to language defaults
            var fontManager = LocalizationFontManager.Instance;
            
            if (fontManager != null)
            {
                // Apply font through FontManager
                if (_legacyText != null)
                    fontManager.ApplyFontToText(_legacyText, currentLanguage);
                else if (_tmpText != null)
                    fontManager.ApplyFontToText(_tmpText, currentLanguage);
                else if (_tmp3DText != null)
                    fontManager.ApplyFontToText(_tmp3DText, currentLanguage);
            }
            else
            {
                // Fallback to basic font application
                ApplyBasicFont(currentLanguage);
            }
        }

        private void ApplyBasicFont(LanguageData language)
        {
            // Use custom font if specified, otherwise use language default
            Font targetFont = customFont != null ? customFont : language.DefaultFont;
            TMP_FontAsset targetTMPFont = customTMPFont != null ? customTMPFont : language.DefaultTMPFont;

            // Apply fonts
            if (_legacyText != null && targetFont != null)
                _legacyText.font = targetFont;
            else if (_tmpText != null && targetTMPFont != null)
                _tmpText.font = targetTMPFont;
            else if (_tmp3DText != null && targetTMPFont != null)
                _tmp3DText.font = targetTMPFont;

            // Apply text direction for RTL languages
            ApplyBasicTextDirection(language);
        }

        private void ApplyBasicTextDirection(LanguageData language)
        {
            if (language.IsRightToLeft)
            {
                if (_legacyText != null)
                    _legacyText.alignment = TextAnchor.UpperRight;
                else if (_tmpText != null)
                {
                    _tmpText.alignment = TextAlignmentOptions.TopRight;
                    _tmpText.isRightToLeftText = true;
                }
                else if (_tmp3DText != null)
                {
                    _tmp3DText.alignment = TextAlignmentOptions.TopRight;
                    _tmp3DText.isRightToLeftText = true;
                }
            }
            else
            {
                if (_legacyText != null)
                    _legacyText.alignment = TextAnchor.UpperLeft;
                else if (_tmpText != null)
                {
                    _tmpText.alignment = TextAlignmentOptions.TopLeft;
                    _tmpText.isRightToLeftText = false;
                }
                else if (_tmp3DText != null)
                {
                    _tmp3DText.alignment = TextAlignmentOptions.TopLeft;
                    _tmp3DText.isRightToLeftText = false;
                }
            }
        }
        #endregion

        #region Event Handlers
        private void OnLanguageChanged(LanguageData newLanguage)
        {
            UpdateText();
            UpdateFont();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set localization key and update text immediately
        /// </summary>
        public void SetKey(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Set parameters for string formatting
        /// </summary>
        public void SetParameters(params string[] newParameters)
        {
            parameters.SetParameters(newParameters);
            if (Application.isPlaying)
                UpdateText();
        }

        /// <summary>
        /// Force refresh text and font
        /// </summary>
        public void Refresh()
        {
            UpdateText();
            UpdateFont();
        }

        /// <summary>
        /// Get current text value
        /// </summary>
        public string GetCurrentText()
        {
            if (_legacyText != null)
                return _legacyText.text;
            else if (_tmpText != null)
                return _tmpText.text;
            else if (_tmp3DText != null)
                return _tmp3DText.text;
            
            return "";
        }

        /// <summary>
        /// Check if this binder has a valid text component
        /// </summary>
        public bool HasTextComponent()
        {
            return _legacyText != null || _tmpText != null || _tmp3DText != null;
        }
        #endregion

        #region Editor Support
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            
            // Validate in play mode
            if (string.IsNullOrEmpty(localizationKey) && string.IsNullOrEmpty(fallbackText))
            {
                Debug.LogWarning($"[LocalizedTextBinder] Neither localization key nor fallback text is set on {gameObject.name}");
            }

            // Update text if parameters changed
            if (HasTextComponent())
            {
                UpdateText();
            }
        }

        /// <summary>
        /// Editor-only method to preview localization
        /// </summary>
        public void EditorPreviewText(string previewKey = null)
        {
            string keyToPreview = previewKey ?? localizationKey;
            if (string.IsNullOrEmpty(keyToPreview)) return;

            string previewText = LocalizationManager.Localize(keyToPreview);
            SetTextValue(previewText);
        }

        /// <summary>
        /// Editor-only method to reset to fallback
        /// </summary>
        public void EditorResetToFallback()
        {
            SetTextValue(fallbackText);
        }
        #endif
        #endregion
    }
}