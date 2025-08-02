using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tirex.Game.Utils.Localization.Components
{
    /// <summary>
    /// Specialized LocalizedText component for dynamic/formatted content
    /// Automatically handles parameter updates and language changes
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizedTextFormatted : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool useAutoUpdater = true;

        [Header("Format Parameters")]
        [SerializeField] private LocalizedParameter[] parameters;

        [Header("Auto-detect Components")]
        [SerializeField] private bool autoDetectTextComponent = true;

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

        public LocalizedParameter[] Parameters
        {
            get => parameters;
            set
            {
                parameters = value;
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
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.RegisterLocalizedTextFormatted(this);
            }
            else
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
            if (useAutoUpdater)
            {
                LocalizationAutoUpdater.UnregisterLocalizedTextFormatted(this);
            }
            else
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
                Debug.LogWarning($"LocalizedTextFormatted on {gameObject.name}: No valid text component found!", this);
            }
        }

        /// <summary>
        /// Update the text content with current parameters
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

            // Get parameter values
            object[] paramValues = GetParameterValues();
            
            string localizedText;
            if (paramValues.Length > 0)
            {
                localizedText = LocalizationManager.Instance.GetText(localizationKey, paramValues);
            }
            else
            {
                localizedText = LocalizationManager.Instance.GetText(localizationKey);
            }

            SetText(localizedText);
        }

        /// <summary>
        /// Get current parameter values
        /// </summary>
        private object[] GetParameterValues()
        {
            if (parameters == null || parameters.Length == 0)
                return new object[0];

            object[] values = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                values[i] = parameters[i].GetValue();
            }
            return values;
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
        /// Set a specific parameter value
        /// </summary>
        public void SetParameter(int index, object value)
        {
            if (parameters != null && index >= 0 && index < parameters.Length)
            {
                parameters[index].SetValue(value);
                UpdateText();
            }
        }

        /// <summary>
        /// Set parameter by name
        /// </summary>
        public void SetParameter(string parameterName, object value)
        {
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].name == parameterName)
                    {
                        parameters[i].SetValue(value);
                        UpdateText();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called when language changes
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            UpdateText();
        }

        /// <summary>
        /// Force refresh all parameters and update text
        /// </summary>
        public void ForceRefresh()
        {
            UpdateText();
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
                
                if (hasValidTextComponent && !string.IsNullOrEmpty(localizationKey))
                {
                    SetText($"[{localizationKey}]");
                }
            }
        }
#endif
    }

    /// <summary>
    /// Parameter for formatted localized text
    /// </summary>
    [System.Serializable]
    public class LocalizedParameter
    {
        public string name;
        public ParameterType type;
        
        [SerializeField] private string stringValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private bool boolValue;
        
        // For dynamic values from other components
        [SerializeField] private Component sourceComponent;
        [SerializeField] private string sourceProperty;

        public enum ParameterType
        {
            String,
            Integer,
            Float,
            Boolean,
            ComponentProperty
        }

        public object GetValue()
        {
            switch (type)
            {
                case ParameterType.String:
                    return stringValue;
                case ParameterType.Integer:
                    return intValue;
                case ParameterType.Float:
                    return floatValue;
                case ParameterType.Boolean:
                    return boolValue;
                case ParameterType.ComponentProperty:
                    return GetComponentPropertyValue();
                default:
                    return stringValue;
            }
        }

        public void SetValue(object value)
        {
            switch (type)
            {
                case ParameterType.String:
                    stringValue = value?.ToString() ?? "";
                    break;
                case ParameterType.Integer:
                    if (int.TryParse(value?.ToString(), out int intVal))
                        intValue = intVal;
                    break;
                case ParameterType.Float:
                    if (float.TryParse(value?.ToString(), out float floatVal))
                        floatValue = floatVal;
                    break;
                case ParameterType.Boolean:
                    if (bool.TryParse(value?.ToString(), out bool boolVal))
                        boolValue = boolVal;
                    break;
            }
        }

        private object GetComponentPropertyValue()
        {
            if (sourceComponent == null || string.IsNullOrEmpty(sourceProperty))
                return "";

            try
            {
                var field = sourceComponent.GetType().GetField(sourceProperty);
                if (field != null)
                {
                    return field.GetValue(sourceComponent);
                }

                var property = sourceComponent.GetType().GetProperty(sourceProperty);
                if (property != null)
                {
                    return property.GetValue(sourceComponent);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get property {sourceProperty} from {sourceComponent.name}: {e.Message}");
            }

            return "";
        }
    }
}
