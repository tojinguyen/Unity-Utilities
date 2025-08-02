using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tirex.Game.Utils.Localization.Components
{
    /// <summary>
    /// UI component for selecting language
    /// Can work with Dropdown, TMP_Dropdown, or custom button-based selection
    /// </summary>
    public class LanguageSelector : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private TMP_Dropdown tmpDropdown;
        [SerializeField] private Button[] languageButtons;

        [Header("Settings")]
        [SerializeField] private bool showNativeNames = true;
        [SerializeField] private bool showLanguageCodes = false;
        [SerializeField] private bool autoSetupOnStart = true;

        [Header("Button Mode Settings")]
        [SerializeField] private GameObject buttonTemplate;
        [SerializeField] private Transform buttonParent;

        // Private fields
        private List<LanguageCode> availableLanguages;
        private DropdownType dropdownType = DropdownType.None;

        private enum DropdownType
        {
            None,
            UnityDropdown,
            TMPDropdown,
            CustomButtons
        }

        private void Awake()
        {
            DetermineDropdownType();
        }

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupLanguageSelector();
            }
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        /// <summary>
        /// Determine which type of dropdown/selector to use
        /// </summary>
        private void DetermineDropdownType()
        {
            if (dropdown != null)
            {
                dropdownType = DropdownType.UnityDropdown;
            }
            else if (tmpDropdown != null)
            {
                dropdownType = DropdownType.TMPDropdown;
            }
            else if (languageButtons != null && languageButtons.Length > 0)
            {
                dropdownType = DropdownType.CustomButtons;
            }
            else if (buttonTemplate != null && buttonParent != null)
            {
                dropdownType = DropdownType.CustomButtons;
            }
        }

        /// <summary>
        /// Setup the language selector with available languages
        /// </summary>
        public void SetupLanguageSelector()
        {
            if (!LocalizationManager.Instance.IsInitialized)
            {
                Debug.LogWarning("LocalizationManager not initialized!");
                return;
            }

            availableLanguages = LocalizationManager.Instance.Config.SupportedLanguages;

            switch (dropdownType)
            {
                case DropdownType.UnityDropdown:
                    SetupUnityDropdown();
                    break;
                case DropdownType.TMPDropdown:
                    SetupTMPDropdown();
                    break;
                case DropdownType.CustomButtons:
                    SetupCustomButtons();
                    break;
            }

            UpdateCurrentSelection();
        }

        /// <summary>
        /// Setup Unity UI Dropdown
        /// </summary>
        private void SetupUnityDropdown()
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();

            foreach (var language in availableLanguages)
            {
                var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                string displayText = GetDisplayText(languageInfo);
                options.Add(new Dropdown.OptionData(displayText));
            }

            dropdown.AddOptions(options);
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        /// <summary>
        /// Setup TextMeshPro Dropdown
        /// </summary>
        private void SetupTMPDropdown()
        {
            if (tmpDropdown == null) return;

            tmpDropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var language in availableLanguages)
            {
                var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                string displayText = GetDisplayText(languageInfo);
                options.Add(new TMP_Dropdown.OptionData(displayText));
            }

            tmpDropdown.AddOptions(options);
            tmpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        /// <summary>
        /// Setup custom buttons for language selection
        /// </summary>
        private void SetupCustomButtons()
        {
            // Clear existing buttons if using template
            if (buttonTemplate != null && buttonParent != null)
            {
                // Clear existing generated buttons
                for (int i = buttonParent.childCount - 1; i >= 0; i--)
                {
                    var child = buttonParent.GetChild(i);
                    if (child != buttonTemplate.transform)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }

                buttonTemplate.SetActive(false);
                languageButtons = new Button[availableLanguages.Count];

                // Generate buttons for each language
                for (int i = 0; i < availableLanguages.Count; i++)
                {
                    var language = availableLanguages[i];
                    var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                    
                    var buttonObj = Instantiate(buttonTemplate, buttonParent);
                    buttonObj.SetActive(true);
                    
                    var button = buttonObj.GetComponent<Button>();
                    languageButtons[i] = button;
                    
                    // Setup button text
                    var textComponent = buttonObj.GetComponentInChildren<Text>();
                    var tmpTextComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                    
                    string displayText = GetDisplayText(languageInfo);
                    
                    if (textComponent != null)
                        textComponent.text = displayText;
                    else if (tmpTextComponent != null)
                        tmpTextComponent.text = displayText;
                    
                    // Add click listener
                    int languageIndex = i; // Capture for closure
                    button.onClick.AddListener(() => OnLanguageButtonClicked(languageIndex));
                }
            }
            else if (languageButtons != null)
            {
                // Use pre-assigned buttons
                for (int i = 0; i < languageButtons.Length && i < availableLanguages.Count; i++)
                {
                    int languageIndex = i; // Capture for closure
                    languageButtons[i].onClick.AddListener(() => OnLanguageButtonClicked(languageIndex));
                }
            }
        }

        /// <summary>
        /// Get display text for a language
        /// </summary>
        private string GetDisplayText(LanguageInfo languageInfo)
        {
            string displayText = showNativeNames ? languageInfo.nativeName : languageInfo.displayName;
            
            if (showLanguageCodes)
            {
                displayText += $" ({languageInfo.code})";
            }
            
            return displayText;
        }

        /// <summary>
        /// Update current selection to match current language
        /// </summary>
        private void UpdateCurrentSelection()
        {
            var currentLanguage = LocalizationManager.Instance.CurrentLanguage;
            int currentIndex = availableLanguages.IndexOf(currentLanguage);

            if (currentIndex >= 0)
            {
                switch (dropdownType)
                {
                    case DropdownType.UnityDropdown:
                        if (dropdown != null)
                            dropdown.value = currentIndex;
                        break;
                    case DropdownType.TMPDropdown:
                        if (tmpDropdown != null)
                            tmpDropdown.value = currentIndex;
                        break;
                    case DropdownType.CustomButtons:
                        UpdateButtonSelection(currentIndex);
                        break;
                }
            }
        }

        /// <summary>
        /// Update button visual states for selection
        /// </summary>
        private void UpdateButtonSelection(int selectedIndex)
        {
            if (languageButtons == null) return;

            for (int i = 0; i < languageButtons.Length; i++)
            {
                if (languageButtons[i] != null)
                {
                    // You can implement visual feedback here
                    // For example, change button colors, enable/disable, etc.
                    var colors = languageButtons[i].colors;
                    if (i == selectedIndex)
                    {
                        colors.normalColor = colors.selectedColor;
                    }
                    else
                    {
                        colors.normalColor = Color.white;
                    }
                    languageButtons[i].colors = colors;
                }
            }
        }

        /// <summary>
        /// Handle dropdown value changed
        /// </summary>
        private void OnDropdownValueChanged(int index)
        {
            if (index >= 0 && index < availableLanguages.Count)
            {
                LocalizationManager.Instance.SetLanguage(availableLanguages[index]);
            }
        }

        /// <summary>
        /// Handle language button clicked
        /// </summary>
        private void OnLanguageButtonClicked(int index)
        {
            if (index >= 0 && index < availableLanguages.Count)
            {
                LocalizationManager.Instance.SetLanguage(availableLanguages[index]);
            }
        }

        /// <summary>
        /// Called when language changes
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            UpdateCurrentSelection();
        }

        /// <summary>
        /// Manually set the selected language
        /// </summary>
        public void SetSelectedLanguage(LanguageCode language)
        {
            LocalizationManager.Instance.SetLanguage(language);
        }

        /// <summary>
        /// Refresh the language selector (useful after config changes)
        /// </summary>
        public void RefreshSelector()
        {
            SetupLanguageSelector();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Setup selector for preview
        /// </summary>
        [ContextMenu("Setup Language Selector")]
        public void EditorSetupSelector()
        {
            DetermineDropdownType();
            SetupLanguageSelector();
        }
#endif
    }
}
