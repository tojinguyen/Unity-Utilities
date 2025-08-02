using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// ScriptableObject containing all localization data and settings
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Tirex Game Utils/Localization/Localization Config", order = 1)]
    public class LocalizationConfig : ScriptableObject
    {
        [Header("Language Settings")]
        [SerializeField] private LanguageCode defaultLanguage = LanguageCode.EN;
        [SerializeField] private List<LanguageCode> supportedLanguages = new List<LanguageCode> { LanguageCode.EN, LanguageCode.VI };
        
        [Header("Localization Data")]
        [SerializeField] private List<LocalizedTextEntry> textEntries = new List<LocalizedTextEntry>();
        [SerializeField] private List<LocalizedSpriteEntry> spriteEntries = new List<LocalizedSpriteEntry>();
        
        [Header("Settings")]
        [SerializeField] private bool autoDetectSystemLanguage = true;
        [SerializeField] private bool saveLanguagePreference = true;
        [SerializeField] private string languagePreferenceKey = "selected_language";

        // Properties
        public LanguageCode DefaultLanguage => defaultLanguage;
        public List<LanguageCode> SupportedLanguages => supportedLanguages;
        public bool AutoDetectSystemLanguage => autoDetectSystemLanguage;
        public bool SaveLanguagePreference => saveLanguagePreference;
        public string LanguagePreferenceKey => languagePreferenceKey;

        // Language Information
        private static readonly Dictionary<LanguageCode, LanguageInfo> LanguageInfos = new Dictionary<LanguageCode, LanguageInfo>
        {
            { LanguageCode.EN, new LanguageInfo(LanguageCode.EN, "English", "English", "en-US") },
            { LanguageCode.VI, new LanguageInfo(LanguageCode.VI, "Vietnamese", "Tiếng Việt", "vi-VN") },
            { LanguageCode.JP, new LanguageInfo(LanguageCode.JP, "Japanese", "日本語", "ja-JP") },
            { LanguageCode.KO, new LanguageInfo(LanguageCode.KO, "Korean", "한국어", "ko-KR") },
            { LanguageCode.ZH, new LanguageInfo(LanguageCode.ZH, "Chinese (Simplified)", "简体中文", "zh-CN") },
            { LanguageCode.TH, new LanguageInfo(LanguageCode.TH, "Thai", "ไทย", "th-TH") },
            { LanguageCode.ID, new LanguageInfo(LanguageCode.ID, "Indonesian", "Bahasa Indonesia", "id-ID") },
            { LanguageCode.ES, new LanguageInfo(LanguageCode.ES, "Spanish", "Español", "es-ES") },
            { LanguageCode.FR, new LanguageInfo(LanguageCode.FR, "French", "Français", "fr-FR") },
            { LanguageCode.DE, new LanguageInfo(LanguageCode.DE, "German", "Deutsch", "de-DE") },
            { LanguageCode.RU, new LanguageInfo(LanguageCode.RU, "Russian", "Русский", "ru-RU") },
            { LanguageCode.PT, new LanguageInfo(LanguageCode.PT, "Portuguese", "Português", "pt-PT") },
            { LanguageCode.AR, new LanguageInfo(LanguageCode.AR, "Arabic", "العربية", "ar-SA", true) },
            { LanguageCode.HI, new LanguageInfo(LanguageCode.HI, "Hindi", "हिन्दी", "hi-IN") },
            { LanguageCode.IT, new LanguageInfo(LanguageCode.IT, "Italian", "Italiano", "it-IT") },
        };

        /// <summary>
        /// Get language information by language code
        /// </summary>
        public static LanguageInfo GetLanguageInfo(LanguageCode languageCode)
        {
            return LanguageInfos.TryGetValue(languageCode, out var info) ? info : LanguageInfos[LanguageCode.EN];
        }

        /// <summary>
        /// Get localized text by key
        /// </summary>
        public string GetText(string key, LanguageCode language)
        {
            var entry = textEntries.Find(e => e.Key == key);
            return entry?.GetValue(language) ?? key;
        }

        /// <summary>
        /// Get localized sprite by key
        /// </summary>
        public Sprite GetSprite(string key, LanguageCode language)
        {
            var entry = spriteEntries.Find(e => e.Key == key);
            return entry?.GetSprite(language);
        }

        /// <summary>
        /// Add or update text entry
        /// </summary>
        public void SetText(string key, LanguageCode language, string text)
        {
            var entry = textEntries.Find(e => e.Key == key);
            if (entry == null)
            {
                entry = new LocalizedTextEntry(key);
                textEntries.Add(entry);
            }
            entry.SetValue(language, text);
        }

        /// <summary>
        /// Add or update sprite entry
        /// </summary>
        public void SetSprite(string key, LanguageCode language, Sprite sprite)
        {
            var entry = spriteEntries.Find(e => e.Key == key);
            if (entry == null)
            {
                entry = new LocalizedSpriteEntry(key);
                spriteEntries.Add(entry);
            }
            entry.SetSprite(language, sprite);
        }

        /// <summary>
        /// Remove text entry by key
        /// </summary>
        public void RemoveTextEntry(string key)
        {
            textEntries.RemoveAll(e => e.Key == key);
        }

        /// <summary>
        /// Remove sprite entry by key
        /// </summary>
        public void RemoveSpriteEntry(string key)
        {
            spriteEntries.RemoveAll(e => e.Key == key);
        }

        /// <summary>
        /// Get all text keys
        /// </summary>
        public List<string> GetAllTextKeys()
        {
            return textEntries.Select(e => e.Key).ToList();
        }

        /// <summary>
        /// Get all sprite keys
        /// </summary>
        public List<string> GetAllSpriteKeys()
        {
            return spriteEntries.Select(e => e.Key).ToList();
        }

        /// <summary>
        /// Clear all localization data
        /// </summary>
        public void ClearAllData()
        {
            textEntries.Clear();
            spriteEntries.Clear();
        }

        /// <summary>
        /// Validate if language is supported
        /// </summary>
        public bool IsLanguageSupported(LanguageCode language)
        {
            return supportedLanguages.Contains(language);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor only: Get text entries for CSV export
        /// </summary>
        public List<LocalizedTextEntry> GetTextEntries()
        {
            return textEntries;
        }

        /// <summary>
        /// Editor only: Set text entries from CSV import
        /// </summary>
        public void SetTextEntries(List<LocalizedTextEntry> entries)
        {
            textEntries = entries;
        }

        /// <summary>
        /// Editor only: Get sprite entries
        /// </summary>
        public List<LocalizedSpriteEntry> GetSpriteEntries()
        {
            return spriteEntries;
        }

        /// <summary>
        /// Editor only: Set sprite entries
        /// </summary>
        public void SetSpriteEntries(List<LocalizedSpriteEntry> entries)
        {
            spriteEntries = entries;
        }
#endif
    }
}
