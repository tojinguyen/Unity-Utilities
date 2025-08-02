using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Data structure for localized text entry
    /// </summary>
    [Serializable]
    public class LocalizedTextEntry
    {
        [SerializeField] private string key;
        [SerializeField] private List<LocalizedValue> values = new List<LocalizedValue>();

        public string Key => key;
        public List<LocalizedValue> Values => values;

        public LocalizedTextEntry(string key)
        {
            this.key = key;
        }

        public string GetValue(LanguageCode language)
        {
            var value = values.Find(v => v.language == language);
            return value?.text ?? key; // Return key as fallback if translation not found
        }

        public void SetValue(LanguageCode language, string text)
        {
            var existingValue = values.Find(v => v.language == language);
            if (existingValue != null)
            {
                existingValue.text = text;
            }
            else
            {
                values.Add(new LocalizedValue(language, text));
            }
        }
    }

    /// <summary>
    /// Individual localized value for a specific language
    /// </summary>
    [Serializable]
    public class LocalizedValue
    {
        public LanguageCode language;
        public string text;

        public LocalizedValue(LanguageCode language, string text)
        {
            this.language = language;
            this.text = text;
        }
    }

    /// <summary>
    /// Data structure for localized sprite/image entry
    /// </summary>
    [Serializable]
    public class LocalizedSpriteEntry
    {
        [SerializeField] private string key;
        [SerializeField] private List<LocalizedSpriteValue> values = new List<LocalizedSpriteValue>();

        public string Key => key;
        public List<LocalizedSpriteValue> Values => values;

        public LocalizedSpriteEntry(string key)
        {
            this.key = key;
        }

        public Sprite GetSprite(LanguageCode language)
        {
            var value = values.Find(v => v.language == language);
            return value?.sprite;
        }

        public void SetSprite(LanguageCode language, Sprite sprite)
        {
            var existingValue = values.Find(v => v.language == language);
            if (existingValue != null)
            {
                existingValue.sprite = sprite;
            }
            else
            {
                values.Add(new LocalizedSpriteValue(language, sprite));
            }
        }
    }

    /// <summary>
    /// Individual localized sprite value for a specific language
    /// </summary>
    [Serializable]
    public class LocalizedSpriteValue
    {
        public LanguageCode language;
        public Sprite sprite;

        public LocalizedSpriteValue(LanguageCode language, Sprite sprite)
        {
            this.language = language;
            this.sprite = sprite;
        }
    }
}
