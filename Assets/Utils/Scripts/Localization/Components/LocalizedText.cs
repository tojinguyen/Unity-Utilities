using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tirex.Game.Utils.Localization.Components
{
    /// <summary>
    /// Component that automatically updates text based on current language
    /// Supports both Unity Text and TextMeshPro components
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnLanguageChange = true;
        [SerializeField] private bool useFormattedText = false;
        [SerializeField] private string[] formatArgs;

        [Header("Auto-detect Components")]
        [SerializeField] private bool autoDetectTextComponent = true;
        [SerializeField] private bool useAutoUpdater = true;

        // Component references
        private Text unityText;
        private TextMeshProUGUI tmpText;
        private TextMeshPro tmp3DText;
        private bool hasValidTextComponent;

        // Properties
        public string LocalizationKey
        {
            get => localizationKey;
            set
            {
                localizationKey = value;
                UpdateText();
            }
        }

        public string[] FormatArgs
        {
            get => formatArgs;
            set
            {
                formatArgs = value;
                if (useFormattedText)
                    UpdateText();
            }
        }

        private void Awake()
        {
            CacheTextComponents();
        }

        private void Start()
        {
            UpdateText();
        }

        private void OnEnable()
        {
            // Choose between manual event handling or auto-updater
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.RegisterLocalizedText(this);
            }
            else if (updateOnLanguageChange)
            {
                LocalizationManager.OnLanguageChanged += OnLanguageChanged;
            }
            
            // Auto-update when enabled if manager is ready
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsInitialized)
            {
                UpdateText();
            }
        }

        private void OnDisable()
        {
            // Unregister from appropriate system
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.UnregisterLocalizedText(this);
            }
            else if (updateOnLanguageChange)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        /// <summary>
        /// Cache text component references
        /// </summary>
        private void CacheTextComponents()
        {
            if (autoDetectTextComponent)
            {
                unityText = GetComponent<Text>();
                tmpText = GetComponent<TextMeshProUGUI>();
                tmp3DText = GetComponent<TextMeshPro>();
            }

            hasValidTextComponent = unityText != null || tmpText != null || tmp3DText != null;

            if (!hasValidTextComponent)
            {
                Debug.LogWarning($"LocalizedText on {gameObject.name}: No valid text component found!", this);
            }
        }

        /// <summary>
        /// Update the text content based on current language
        /// </summary>
        public void UpdateText()
        {
            if (!hasValidTextComponent || string.IsNullOrEmpty(localizationKey))
                return;

            if (!LocalizationManager.Instance.IsInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized yet!");
                return;
            }

            string localizedText;

            if (useFormattedText && formatArgs != null && formatArgs.Length > 0)
            {
                // Convert string format args to objects for proper formatting
                object[] objArgs = new object[formatArgs.Length];
                for (int i = 0; i < formatArgs.Length; i++)
                {
                    objArgs[i] = formatArgs[i];
                }
                localizedText = LocalizationManager.Instance.GetText(localizationKey, objArgs);
            }
            else
            {
                localizedText = LocalizationManager.Instance.GetText(localizationKey);
            }

            SetText(localizedText);
        }

        /// <summary>
        /// Set text on the appropriate component
        /// </summary>
        private void SetText(string text)
        {
            if (unityText != null)
                unityText.text = text;
            else if (tmpText != null)
                tmpText.text = text;
            else if (tmp3DText != null)
                tmp3DText.text = text;
        }

        /// <summary>
        /// Get current text from the component
        /// </summary>
        public string GetCurrentText()
        {
            if (unityText != null)
                return unityText.text;
            else if (tmpText != null)
                return tmpText.text;
            else if (tmp3DText != null)
                return tmp3DText.text;
            
            return string.Empty;
        }

        /// <summary>
        /// Set localization key and update text immediately
        /// </summary>
        public void SetLocalizationKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// Set format arguments and update text if using formatted text
        /// </summary>
        public void SetFormatArgs(params string[] args)
        {
            formatArgs = args;
            if (useFormattedText)
                UpdateText();
        }

        /// <summary>
        /// Enable or disable formatted text
        /// </summary>
        public void SetUseFormattedText(bool useFormatted)
        {
            useFormattedText = useFormatted;
            UpdateText();
        }

        /// <summary>
        /// Called when language changes
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            UpdateText();
        }

        /// <summary>
        /// Validate the component setup
        /// </summary>
        public bool IsValid()
        {
            return hasValidTextComponent && !string.IsNullOrEmpty(localizationKey);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Update text in edit mode for preview
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                CacheTextComponents();
                
                // Show localization key as preview in edit mode
                if (hasValidTextComponent && !string.IsNullOrEmpty(localizationKey))
                {
                    SetText($"[{localizationKey}]");
                }
            }
        }

        /// <summary>
        /// Editor-only: Force update text
        /// </summary>
        [ContextMenu("Update Text")]
        public void EditorUpdateText()
        {
            CacheTextComponents();
            UpdateText();
        }

        /// <summary>
        /// Editor-only: Set localization key from current text
        /// </summary>
        [ContextMenu("Set Key From Current Text")]
        public void SetKeyFromCurrentText()
        {
            if (hasValidTextComponent)
            {
                string currentText = GetCurrentText();
                if (!string.IsNullOrEmpty(currentText))
                {
                    localizationKey = currentText.ToLowerInvariant().Replace(" ", "_");
                    Debug.Log($"Set localization key to: {localizationKey}");
                }
            }
        }
#endif
    }
}
