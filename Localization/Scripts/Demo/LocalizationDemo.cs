using UnityEngine;
using TMPro;
using TirexGame.Utils.Localization;
using UnityEditor;

namespace TirexGame.Utils.Localization.Demo
{
    /// <summary>
    /// Demo script to test localization system functionality
    /// </summary>
    public class LocalizationDemo : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI demoText;
        
        [Header("Test Keys")]
        public string[] testKeys = 
        {
            "DEMO_HELLO_WORLD",
            "DEMO_WELCOME_MESSAGE", 
            "DEMO_BUTTON_PLAY",
            "DEMO_BUTTON_SETTINGS"
        };

        private void Start()
        {
            // Wait for localization to be ready
            if (LocalizationManager.Instance.IsReady)
            {
                OnLocalizationReady();
            }
            else
            {
                LocalizationManager.OnLocalizationReady += OnLocalizationReady;
            }
            
            // Subscribe to language changes
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLocalizationReady -= OnLocalizationReady;
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLocalizationReady()
        {
            UpdateUI();
            LogSystemInfo();
        }

        private void OnLanguageChanged(LanguageData newLanguage)
        {
            UpdateUI();
            Debug.Log($"[LocalizationDemo] Language changed to: {newLanguage.DisplayName}");
        }

        private void UpdateUI()
        {
            if (statusText != null)
            {
                var currentLang = LocalizationManager.Instance.CurrentLanguage;
                statusText.text = $"Current Language: {currentLang?.DisplayName ?? "None"}";
            }

            if (demoText != null && testKeys.Length > 0)
            {
                string text = LocalizationManager.Localize(testKeys[0]);
                demoText.text = text;
            }
        }

        private void LogSystemInfo()
        {
            Debug.Log("=== Localization System Info ===");
            Debug.Log($"Current Language: {LocalizationManager.GetCurrentLanguageCode()}");
            Debug.Log($"Supported Languages: {LocalizationManager.GetSupportedLanguages().Count}");
            Debug.Log($"Total Keys: {LocalizationManager.GetAllKeys().Count}");
            Debug.Log($"Completion: {LocalizationManager.GetCurrentLanguageCompletionPercentage():F1}%");
            
            var missing = LocalizationManager.GetMissingTranslationsForCurrentLanguage();
            if (missing.Count > 0)
            {
                Debug.LogWarning($"Missing Translations: {missing.Count}");
            }
        }

        [ContextMenu("Test All Keys")]
        public void TestAllKeys()
        {
            Debug.Log("=== Testing All Localization Keys ===");
            
            foreach (string key in testKeys)
            {
                string localizedText = LocalizationManager.Localize(key);
                Debug.Log($"Key: {key} -> Value: {localizedText}");
            }
        }

        [ContextMenu("Switch to English")]
        public void SwitchToEnglish()
        {
            LocalizationManager.SetLanguage("en");
        }

        [ContextMenu("Switch to Vietnamese")]
        public void SwitchToVietnamese()
        {
            LocalizationManager.SetLanguage("vi");
        }

        [ContextMenu("Switch to Japanese")]
        public void SwitchToJapanese()
        {
            LocalizationManager.SetLanguage("ja");
        }

        [ContextMenu("Test System Language Detection")]
        public void TestSystemLanguageDetection()
        {
            bool success = LocalizationManager.TrySetSystemLanguage();
            Debug.Log($"System language detection: {(success ? "Success" : "Failed")}");
        }

        [ContextMenu("Run Full Validation")]
        public void RunValidation()
        {
            var settings = LocalizationManager.Instance.Settings;
            if (settings != null)
            {
                var result = LocalizationValidator.ValidateLocalizationSystem(settings);
                LocalizationValidator.LogValidationResults(result);
            }
            else
            {
                Debug.LogError("No LocalizationSettings found!");
            }
        }

        [ContextMenu("Create Demo Keys")]
        public void CreateDemoKeys()
        {
            Debug.Log("Demo keys should be created in Localization Manager:");
            Debug.Log("DEMO_HELLO_WORLD -> Hello World!");
            Debug.Log("DEMO_WELCOME_MESSAGE -> Welcome to our game!");
            Debug.Log("DEMO_BUTTON_PLAY -> Play");
            Debug.Log("DEMO_BUTTON_SETTINGS -> Settings");
        }

        private void OnGUI()
        {
            // Simple debug GUI
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Localization Demo", EditorGUIUtility.isProSkin ? GUI.skin.GetStyle("WhiteBoldLabel") : GUI.skin.label);
            
            if (LocalizationManager.Instance.IsReady)
            {
                var currentLang = LocalizationManager.Instance.CurrentLanguage;
                GUILayout.Label($"Language: {currentLang?.DisplayName ?? "None"}");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button("Switch to English"))
                    SwitchToEnglish();
                if (GUILayout.Button("Switch to Vietnamese"))
                    SwitchToVietnamese();
                if (GUILayout.Button("Test All Keys"))
                    TestAllKeys();
            }
            else
            {
                GUILayout.Label("Localization not ready...");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}