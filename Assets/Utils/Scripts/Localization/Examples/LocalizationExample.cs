using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tirex.Game.Utils.Localization.Examples
{
    /// <summary>
    /// Example demonstrating how to use the localization system
    /// Shows different ways to interact with localized content
    /// </summary>
    public class LocalizationExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LocalizationConfig localizationConfig;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI dynamicText;
        [SerializeField] private Button englishButton;
        [SerializeField] private Button vietnameseButton;
        [SerializeField] private Button japaneseButton;

        [Header("Dynamic Content")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private int score = 1000;

        private void Start()
        {
            // Initialize the localization system
            if (localizationConfig != null)
            {
                LocalizationManager.Instance.Initialize(localizationConfig);
            }

            SetupButtons();
            UpdateDynamicText();

            // Subscribe to language change events
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        /// <summary>
        /// Setup button click events
        /// </summary>
        private void SetupButtons()
        {
            if (englishButton != null)
                englishButton.onClick.AddListener(() => ChangeLanguage(LanguageCode.EN));

            if (vietnameseButton != null)
                vietnameseButton.onClick.AddListener(() => ChangeLanguage(LanguageCode.VI));

            if (japaneseButton != null)
                japaneseButton.onClick.AddListener(() => ChangeLanguage(LanguageCode.JP));
        }

        /// <summary>
        /// Change the current language
        /// </summary>
        private void ChangeLanguage(LanguageCode language)
        {
            LocalizationManager.Instance.SetLanguage(language);
            Debug.Log($"Language changed to: {language}");
        }

        /// <summary>
        /// Update dynamic text with formatted localization
        /// </summary>
        private void UpdateDynamicText()
        {
            if (dynamicText != null)
            {
                // Example of using formatted text
                string localizedText = LocalizationManager.GetLocalizedText("ui_player_score", playerName, score);
                dynamicText.text = localizedText;
            }
        }

        /// <summary>
        /// Called when language changes
        /// </summary>
        private void OnLanguageChanged(LanguageCode newLanguage)
        {
            Debug.Log($"Language changed to: {newLanguage}");
            
            // Update dynamic content when language changes
            UpdateDynamicText();

            // You can also manually update specific UI elements here
            if (titleText != null)
            {
                titleText.text = LocalizationManager.GetLocalizedText("ui_welcome_title");
            }
        }

        /// <summary>
        /// Example of programmatically getting localized text
        /// </summary>
        public void ShowLocalizedMessage()
        {
            string message = LocalizationManager.GetLocalizedText("ui_example_message");
            Debug.Log($"Localized message: {message}");
        }

        /// <summary>
        /// Example of getting localized text with parameters
        /// </summary>
        public void ShowFormattedMessage()
        {
            string message = LocalizationManager.GetLocalizedText("ui_time_remaining", "5", "30");
            Debug.Log($"Formatted message: {message}");
        }

        /// <summary>
        /// Example of checking if a key exists
        /// </summary>
        public void CheckKeyExists()
        {
            bool exists = LocalizationManager.Instance.HasTextKey("ui_welcome_title");
            Debug.Log($"Key 'ui_welcome_title' exists: {exists}");
        }

        /// <summary>
        /// Get current language information
        /// </summary>
        public void ShowCurrentLanguageInfo()
        {
            var languageInfo = LocalizationManager.Instance.GetCurrentLanguageInfo();
            Debug.Log($"Current language: {languageInfo.displayName} ({languageInfo.nativeName})");
        }

        /// <summary>
        /// Example of creating sample localization data
        /// </summary>
        [ContextMenu("Create Sample Data")]
        private void CreateSampleData()
        {
            if (localizationConfig == null)
            {
                Debug.LogError("No localization config assigned!");
                return;
            }

            // Add sample text entries
            var sampleEntries = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<LanguageCode, string>>
            {
                {
                    "ui_welcome_title",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "Welcome to the Game!" },
                        { LanguageCode.VI, "Chào mừng đến với Game!" },
                        { LanguageCode.JP, "ゲームへようこそ！" }
                    }
                },
                {
                    "ui_start_game",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "Start Game" },
                        { LanguageCode.VI, "Bắt đầu Game" },
                        { LanguageCode.JP, "ゲーム開始" }
                    }
                },
                {
                    "ui_settings",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "Settings" },
                        { LanguageCode.VI, "Cài đặt" },
                        { LanguageCode.JP, "設定" }
                    }
                },
                {
                    "ui_player_score",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "Player {0}: {1} points" },
                        { LanguageCode.VI, "Người chơi {0}: {1} điểm" },
                        { LanguageCode.JP, "プレイヤー {0}: {1} ポイント" }
                    }
                },
                {
                    "ui_time_remaining",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "Time remaining: {0}:{1}" },
                        { LanguageCode.VI, "Thời gian còn lại: {0}:{1}" },
                        { LanguageCode.JP, "残り時間: {0}:{1}" }
                    }
                },
                {
                    "ui_example_message",
                    new System.Collections.Generic.Dictionary<LanguageCode, string>
                    {
                        { LanguageCode.EN, "This is an example localized message!" },
                        { LanguageCode.VI, "Đây là một tin nhắn bản địa hóa ví dụ!" },
                        { LanguageCode.JP, "これはローカライズされたメッセージの例です！" }
                    }
                }
            };

            // Add entries to config
            foreach (var entry in sampleEntries)
            {
                foreach (var languageValue in entry.Value)
                {
                    localizationConfig.SetText(entry.Key, languageValue.Key, languageValue.Value);
                }
            }

            Debug.Log("Sample localization data created!");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(localizationConfig);
#endif
        }
    }
}
