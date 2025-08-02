using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Utility class for importing and exporting localization data to/from CSV files
    /// </summary>
    public static class LocalizationCSVUtility
    {
        private const string CSV_SEPARATOR = ",";
        private const string CSV_QUOTE = "\"";
        private const string CSV_ESCAPED_QUOTE = "\"\"";

        /// <summary>
        /// Export localization data to CSV format
        /// </summary>
        /// <param name="config">Localization configuration to export</param>
        /// <param name="filePath">Path to save the CSV file</param>
        /// <param name="includeKeys">Include key column in export</param>
        /// <returns>Success status</returns>
        public static bool ExportToCSV(LocalizationConfig config, string filePath, bool includeKeys = true)
        {
            try
            {
                var supportedLanguages = config.SupportedLanguages;
                var textEntries = config.GetTextEntries();

                if (textEntries == null || textEntries.Count == 0)
                {
                    Debug.LogWarning("No text entries to export!");
                    return false;
                }

                var csvBuilder = new StringBuilder();

                // Create header row
                var headerRow = new List<string>();
                if (includeKeys)
                {
                    headerRow.Add("Key");
                }

                foreach (var language in supportedLanguages)
                {
                    var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                    headerRow.Add(languageInfo.displayName);
                }

                csvBuilder.AppendLine(string.Join(CSV_SEPARATOR, headerRow));

                // Add data rows
                foreach (var entry in textEntries)
                {
                    var dataRow = new List<string>();
                    
                    if (includeKeys)
                    {
                        dataRow.Add(EscapeCSVField(entry.Key));
                    }

                    foreach (var language in supportedLanguages)
                    {
                        string value = entry.GetValue(language);
                        dataRow.Add(EscapeCSVField(value));
                    }

                    csvBuilder.AppendLine(string.Join(CSV_SEPARATOR, dataRow));
                }

                // Write to file
                File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);
                Debug.Log($"Successfully exported localization data to: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to export CSV: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Import localization data from CSV format
        /// </summary>
        /// <param name="config">Localization configuration to import into</param>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="clearExisting">Clear existing data before import</param>
        /// <param name="keyColumnIndex">Index of the key column (0-based, -1 if no key column)</param>
        /// <returns>Success status</returns>
        public static bool ImportFromCSV(LocalizationConfig config, string filePath, bool clearExisting = true, int keyColumnIndex = 0)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"CSV file not found: {filePath}");
                    return false;
                }

                string csvContent = File.ReadAllText(filePath, Encoding.UTF8);
                var lines = ParseCSVLines(csvContent);

                if (lines.Count < 2) // At least header + 1 data row
                {
                    Debug.LogError("CSV file must contain at least a header row and one data row!");
                    return false;
                }

                var supportedLanguages = config.SupportedLanguages;
                var headerColumns = lines[0];

                // Parse header to determine language column mapping
                var languageColumnMap = new Dictionary<int, LanguageCode>();
                
                for (int i = 0; i < headerColumns.Count; i++)
                {
                    if (i == keyColumnIndex) continue; // Skip key column

                    string columnName = headerColumns[i].Trim();
                    
                    // Try to find matching language by display name
                    foreach (var language in supportedLanguages)
                    {
                        var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                        if (string.Equals(columnName, languageInfo.displayName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(columnName, languageInfo.nativeName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(columnName, language.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            languageColumnMap[i] = language;
                            break;
                        }
                    }
                }

                if (languageColumnMap.Count == 0)
                {
                    Debug.LogError("No matching language columns found in CSV!");
                    return false;
                }

                // Clear existing data if requested
                if (clearExisting)
                {
                    config.ClearAllData();
                }

                // Process data rows
                int importedCount = 0;
                for (int lineIndex = 1; lineIndex < lines.Count; lineIndex++)
                {
                    var dataColumns = lines[lineIndex];
                    
                    if (dataColumns.Count == 0) continue; // Skip empty lines

                    string key;
                    if (keyColumnIndex >= 0 && keyColumnIndex < dataColumns.Count)
                    {
                        key = dataColumns[keyColumnIndex].Trim();
                    }
                    else
                    {
                        key = $"imported_key_{lineIndex}"; // Generate key if not provided
                    }

                    if (string.IsNullOrEmpty(key)) continue;

                    // Import values for each language
                    foreach (var kvp in languageColumnMap)
                    {
                        int columnIndex = kvp.Key;
                        var language = kvp.Value;

                        if (columnIndex < dataColumns.Count)
                        {
                            string value = dataColumns[columnIndex].Trim();
                            if (!string.IsNullOrEmpty(value))
                            {
                                config.SetText(key, language, value);
                            }
                        }
                    }

                    importedCount++;
                }

                Debug.Log($"Successfully imported {importedCount} entries from CSV: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to import CSV: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parse CSV content into lines and columns
        /// </summary>
        private static List<List<string>> ParseCSVLines(string csvContent)
        {
            var lines = new List<List<string>>();
            var currentLine = new List<string>();
            var currentField = new StringBuilder();
            
            bool inQuotes = false;
            bool expectingDelimiterOrNewline = false;

            for (int i = 0; i < csvContent.Length; i++)
            {
                char c = csvContent[i];
                char nextChar = (i + 1 < csvContent.Length) ? csvContent[i + 1] : '\0';

                if (expectingDelimiterOrNewline)
                {
                    if (c == ',')
                    {
                        expectingDelimiterOrNewline = false;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        expectingDelimiterOrNewline = false;
                        if (c == '\r' && nextChar == '\n')
                        {
                            i++; // Skip the \n
                        }
                        currentLine.Add(currentField.ToString());
                        lines.Add(currentLine);
                        currentLine = new List<string>();
                        currentField.Clear();
                        continue;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        throw new FormatException($"Unexpected character '{c}' at position {i}");
                    }
                }

                if (c == '"')
                {
                    if (inQuotes)
                    {
                        if (nextChar == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip the next quote
                        }
                        else
                        {
                            inQuotes = false;
                            expectingDelimiterOrNewline = true;
                        }
                    }
                    else
                    {
                        inQuotes = true;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    currentLine.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    if (c == '\r' && nextChar == '\n')
                    {
                        i++; // Skip the \n
                    }
                    currentLine.Add(currentField.ToString());
                    lines.Add(currentLine);
                    currentLine = new List<string>();
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field and line if not empty
            if (currentField.Length > 0 || currentLine.Count > 0)
            {
                currentLine.Add(currentField.ToString());
                lines.Add(currentLine);
            }

            return lines;
        }

        /// <summary>
        /// Escape CSV field by wrapping in quotes and escaping internal quotes
        /// </summary>
        private static string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            bool needsQuoting = field.Contains(CSV_SEPARATOR) || 
                               field.Contains(CSV_QUOTE) || 
                               field.Contains("\n") || 
                               field.Contains("\r");

            if (needsQuoting)
            {
                return CSV_QUOTE + field.Replace(CSV_QUOTE, CSV_ESCAPED_QUOTE) + CSV_QUOTE;
            }

            return field;
        }

        /// <summary>
        /// Create a template CSV file with supported languages
        /// </summary>
        /// <param name="config">Localization configuration</param>
        /// <param name="filePath">Path to save the template CSV</param>
        /// <param name="sampleKeys">Sample keys to include in template</param>
        /// <returns>Success status</returns>
        public static bool CreateTemplate(LocalizationConfig config, string filePath, string[] sampleKeys = null)
        {
            try
            {
                var supportedLanguages = config.SupportedLanguages;
                var csvBuilder = new StringBuilder();

                // Create header row
                var headerRow = new List<string> { "Key" };
                foreach (var language in supportedLanguages)
                {
                    var languageInfo = LocalizationConfig.GetLanguageInfo(language);
                    headerRow.Add(languageInfo.displayName);
                }
                csvBuilder.AppendLine(string.Join(CSV_SEPARATOR, headerRow));

                // Add sample keys if provided
                if (sampleKeys != null)
                {
                    foreach (var key in sampleKeys)
                    {
                        var dataRow = new List<string> { EscapeCSVField(key) };
                        
                        // Add empty cells for each language
                        for (int i = 0; i < supportedLanguages.Count; i++)
                        {
                            dataRow.Add("");
                        }
                        
                        csvBuilder.AppendLine(string.Join(CSV_SEPARATOR, dataRow));
                    }
                }

                File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);
                Debug.Log($"Successfully created CSV template: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create CSV template: {e.Message}");
                return false;
            }
        }
    }
}
