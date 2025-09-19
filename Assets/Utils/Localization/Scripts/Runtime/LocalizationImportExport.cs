using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TirexGame.Utils.Localization
{
    [Serializable]
    public class LocalizationExportData
    {
        public List<LanguageExportData> languages = new List<LanguageExportData>();
        public List<KeyExportData> keys = new List<KeyExportData>();
        public string exportedAt;
        public string version = "1.0";
    }

    [Serializable]
    public class LanguageExportData
    {
        public string displayName;
        public string languageCode;
        public string countryCode;
        public bool isDefault;
    }

    [Serializable]
    public class KeyExportData
    {
        public string key;
        public string comment;
        public List<TranslationData> translations = new List<TranslationData>();
    }

    [Serializable]
    public class TranslationData
    {
        public string languageCode;
        public string value;
        public bool isTranslated;
    }

    public static class LocalizationImportExport
    {
        public static string ExportToCSV(LocalizationSettings settings)
        {
            if (settings == null) return null;

            var csv = new StringBuilder();
            
            // Header row
            csv.Append("Key,Comment");
            foreach (var language in settings.SupportedLanguages)
            {
                csv.Append($",{language.LanguageCode}");
            }
            csv.AppendLine();

            // Data rows
            var allKeys = settings.GetAllKeys();
            foreach (string key in allKeys)
            {
                csv.Append($"\"{EscapeCSV(key)}\",");
                
                // Get comment from any table that has this key
                string comment = GetCommentForKey(settings, key);
                csv.Append($"\"{EscapeCSV(comment)}\"");

                // Add translations for each language
                foreach (var language in settings.SupportedLanguages)
                {
                    var table = settings.GetTableForLanguage(language);
                    string value = table?.GetValue(key) ?? "";
                    csv.Append($",\"{EscapeCSV(value)}\"");
                }
                csv.AppendLine();
            }

            return csv.ToString();
        }

        public static string ExportToJSON(LocalizationSettings settings)
        {
            if (settings == null) return null;

            var exportData = new LocalizationExportData
            {
                exportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Export language data
            foreach (var language in settings.SupportedLanguages)
            {
                exportData.languages.Add(new LanguageExportData
                {
                    displayName = language.DisplayName,
                    languageCode = language.LanguageCode,
                    countryCode = language.CountryCode,
                    isDefault = language == settings.DefaultLanguage
                });
            }

            // Export keys and translations
            var allKeys = settings.GetAllKeys();
            foreach (string key in allKeys)
            {
                var keyData = new KeyExportData
                {
                    key = key,
                    comment = GetCommentForKey(settings, key)
                };

                foreach (var language in settings.SupportedLanguages)
                {
                    var table = settings.GetTableForLanguage(language);
                    string value = table?.GetValue(key) ?? "";
                    
                    keyData.translations.Add(new TranslationData
                    {
                        languageCode = language.LanguageCode,
                        value = value,
                        isTranslated = !string.IsNullOrEmpty(value)
                    });
                }

                exportData.keys.Add(keyData);
            }

            return JsonUtility.ToJson(exportData, true);
        }

        public static bool ImportFromCSV(LocalizationSettings settings, string csvContent)
        {
            if (settings == null || string.IsNullOrEmpty(csvContent)) return false;

            try
            {
                string[] lines = csvContent.Split('\n');
                if (lines.Length < 2) return false;

                // Parse header
                string[] headers = ParseCSVLine(lines[0]);
                if (headers.Length < 3) return false; // At least Key, Comment, and one language

                var languageMap = new Dictionary<int, LanguageData>();
                
                // Map header columns to languages
                for (int i = 2; i < headers.Length; i++) // Skip Key(0) and Comment(1)
                {
                    string langCode = headers[i].Trim();
                    var language = settings.GetLanguageByCode(langCode);
                    if (language != null)
                    {
                        languageMap[i] = language;
                    }
                }

                if (languageMap.Count == 0)
                {
                    Debug.LogWarning("No matching languages found in CSV headers");
                    return false;
                }

                // Process data rows
                int importedCount = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] values = ParseCSVLine(line);
                    if (values.Length < 2) continue;

                    string key = UnescapeCSV(values[0]);
                    string comment = values.Length > 1 ? UnescapeCSV(values[1]) : "";

                    if (string.IsNullOrEmpty(key)) continue;

                    // Import translations for each language
                    foreach (var kvp in languageMap)
                    {
                        int columnIndex = kvp.Key;
                        var language = kvp.Value;
                        
                        string translation = "";
                        if (columnIndex < values.Length)
                        {
                            translation = UnescapeCSV(values[columnIndex]);
                        }

                        var table = settings.GetTableForLanguage(language);
                        table?.AddEntry(key, translation, comment);
                    }

                    importedCount++;
                }

                Debug.Log($"Successfully imported {importedCount} keys from CSV");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"CSV import failed: {ex.Message}");
                return false;
            }
        }

        public static bool ImportFromJSON(LocalizationSettings settings, string jsonContent)
        {
            if (settings == null || string.IsNullOrEmpty(jsonContent)) return false;

            try
            {
                var importData = JsonUtility.FromJson<LocalizationExportData>(jsonContent);
                if (importData == null) return false;

                int importedKeys = 0;

                // Import keys and translations
                foreach (var keyData in importData.keys)
                {
                    if (string.IsNullOrEmpty(keyData.key)) continue;

                    foreach (var translation in keyData.translations)
                    {
                        var language = settings.GetLanguageByCode(translation.languageCode);
                        if (language != null)
                        {
                            var table = settings.GetTableForLanguage(language);
                            table?.AddEntry(keyData.key, translation.value, keyData.comment);
                        }
                    }

                    importedKeys++;
                }

                Debug.Log($"Successfully imported {importedKeys} keys from JSON");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON import failed: {ex.Message}");
                return false;
            }
        }

        public static void SaveToFile(string content, string defaultName, string extension, string title)
        {
            #if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel(title, "", defaultName, extension);
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, content, Encoding.UTF8);
                Debug.Log($"File saved to: {path}");
            }
            #endif
        }

        public static string LoadFromFile(string title, string extension)
        {
            #if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel(title, "", extension);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            #endif
            return null;
        }

        private static string GetCommentForKey(LocalizationSettings settings, string key)
        {
            foreach (var language in settings.SupportedLanguages)
            {
                var table = settings.GetTableForLanguage(language);
                if (table != null)
                {
                    var entry = table.Entries.FirstOrDefault(e => e.key == key);
                    if (entry != null && !string.IsNullOrEmpty(entry.comment))
                    {
                        return entry.comment;
                    }
                }
            }
            return "";
        }

        private static string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            // Escape quotes by doubling them
            if (value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
            }
            
            return value;
        }

        private static string UnescapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            // Remove surrounding quotes if present
            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 1)
            {
                value = value.Substring(1, value.Length - 2);
            }
            
            // Unescape doubled quotes
            if (value.Contains("\"\""))
            {
                value = value.Replace("\"\"", "\"");
            }
            
            return value;
        }

        private static string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            bool quoteStarted = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteStarted = true;
                    }
                    else
                    {
                        // Check if this is an escaped quote
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip the next quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                    quoteStarted = false;
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field
            result.Add(currentField.ToString());

            return result.ToArray();
        }
    }
}