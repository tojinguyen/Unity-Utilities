using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TirexGame.Utils.Localization
{
    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
        public string comment;
        public bool isTranslated;

        public LocalizationEntry()
        {
            key = "";
            value = "";
            comment = "";
            isTranslated = false;
        }

        public LocalizationEntry(string key, string value, string comment = "")
        {
            this.key = key;
            this.value = value;
            this.comment = comment;
            this.isTranslated = !string.IsNullOrEmpty(value);
        }
    }

    [CreateAssetMenu(fileName = "New Localization Table", menuName = "TirexGame/Localization/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [Header("Table Info")]
        [SerializeField] private LanguageData language;
        [SerializeField] private string tableName;
        [SerializeField] private string description;

        [Header("Entries")]
        [SerializeField] private List<LocalizationEntry> entries = new List<LocalizationEntry>();

        // Dictionary for fast lookup
        private Dictionary<string, string> _lookupCache;
        private bool _cacheValid = false;

        // Properties
        public LanguageData Language => language;
        public string TableName => tableName;
        public string Description => description;
        public List<LocalizationEntry> Entries => entries;
        public int EntryCount => entries.Count;
        public int TranslatedCount => entries.Count(e => e.isTranslated);
        public float CompletionPercentage => EntryCount > 0 ? (float)TranslatedCount / EntryCount * 100f : 0f;

        // Methods
        public void SetLanguage(LanguageData languageData)
        {
            language = languageData;
            if (language != null && string.IsNullOrEmpty(tableName))
            {
                tableName = language.LanguageCode;
            }
            InvalidateCache();
        }

        public string GetValue(string key)
        {
            if (!_cacheValid)
                RebuildCache();

            return _lookupCache.TryGetValue(key, out string value) ? value : null;
        }

        public bool HasKey(string key)
        {
            if (!_cacheValid)
                RebuildCache();

            return _lookupCache.ContainsKey(key);
        }

        public void AddEntry(string key, string value, string comment = "")
        {
            var existingEntry = entries.FirstOrDefault(e => e.key == key);
            if (existingEntry != null)
            {
                existingEntry.value = value;
                existingEntry.comment = comment;
                existingEntry.isTranslated = !string.IsNullOrEmpty(value);
            }
            else
            {
                entries.Add(new LocalizationEntry(key, value, comment));
            }
            InvalidateCache();
        }

        public bool RemoveEntry(string key)
        {
            var entryToRemove = entries.FirstOrDefault(e => e.key == key);
            if (entryToRemove != null)
            {
                entries.Remove(entryToRemove);
                InvalidateCache();
                return true;
            }
            return false;
        }

        public void UpdateEntry(string key, string newValue, string newComment = null)
        {
            var entry = entries.FirstOrDefault(e => e.key == key);
            if (entry != null)
            {
                entry.value = newValue;
                entry.isTranslated = !string.IsNullOrEmpty(newValue);
                if (newComment != null)
                    entry.comment = newComment;
                InvalidateCache();
            }
        }

        public List<string> GetAllKeys()
        {
            return entries.Select(e => e.key).ToList();
        }

        public List<string> GetMissingTranslations()
        {
            return entries.Where(e => !e.isTranslated).Select(e => e.key).ToList();
        }

        public void ClearAll()
        {
            entries.Clear();
            InvalidateCache();
        }

        public void SortByKey()
        {
            entries = entries.OrderBy(e => e.key).ToList();
            InvalidateCache();
        }

        private void RebuildCache()
        {
            _lookupCache = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key) && !_lookupCache.ContainsKey(entry.key))
                {
                    _lookupCache[entry.key] = entry.value;
                }
            }
            _cacheValid = true;
        }

        private void InvalidateCache()
        {
            _cacheValid = false;
        }

        private void OnValidate()
        {
            InvalidateCache();
            
            // Update isTranslated flags
            foreach (var entry in entries)
            {
                entry.isTranslated = !string.IsNullOrEmpty(entry.value);
            }
        }

        // Editor utility methods
        #if UNITY_EDITOR
        public void EditorAddEntry(string key, string value, string comment = "")
        {
            AddEntry(key, value, comment);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void EditorRemoveEntry(string key)
        {
            if (RemoveEntry(key))
                UnityEditor.EditorUtility.SetDirty(this);
        }

        public void EditorUpdateEntry(string key, string newValue, string newComment = null)
        {
            UpdateEntry(key, newValue, newComment);
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}