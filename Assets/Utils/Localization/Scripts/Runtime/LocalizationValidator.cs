using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TirexGame.Utils.Localization
{
    [Serializable]
    public class ValidationResult
    {
        public List<string> missingKeys = new List<string>();
        public List<string> duplicateKeys = new List<string>();
        public Dictionary<string, List<string>> missingTranslations = new Dictionary<string, List<string>>();
        public List<string> unusedKeys = new List<string>();
        public List<ValidationIssue> issues = new List<ValidationIssue>();
        
        public bool HasErrors => issues.Any(i => i.severity == ValidationSeverity.Error);
        public bool HasWarnings => issues.Any(i => i.severity == ValidationSeverity.Warning);
        public int TotalIssues => issues.Count;
    }

    [Serializable]
    public class ValidationIssue
    {
        public ValidationSeverity severity;
        public string category;
        public string message;
        public string details;
        public UnityEngine.Object context;

        public ValidationIssue(ValidationSeverity severity, string category, string message, string details = "", UnityEngine.Object context = null)
        {
            this.severity = severity;
            this.category = category;
            this.message = message;
            this.details = details;
            this.context = context;
        }
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public static class LocalizationValidator
    {
        public static ValidationResult ValidateLocalizationSystem(LocalizationSettings settings)
        {
            var result = new ValidationResult();
            
            if (settings == null)
            {
                result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Settings", "LocalizationSettings is null"));
                return result;
            }

            ValidateSettings(settings, result);
            ValidateLanguages(settings, result);
            ValidateTables(settings, result);
            ValidateKeys(settings, result);
            ValidateUsage(settings, result);

            return result;
        }

        private static void ValidateSettings(LocalizationSettings settings, ValidationResult result)
        {
            // Check basic settings
            if (settings.SupportedLanguages.Count == 0)
            {
                result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Settings", "No supported languages configured", "", settings));
            }

            if (settings.DefaultLanguage == null)
            {
                result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Settings", "No default language set", "", settings));
            }
            else if (!settings.SupportedLanguages.Contains(settings.DefaultLanguage))
            {
                result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Settings", "Default language is not in supported languages list", "", settings));
            }

            if (string.IsNullOrEmpty(settings.PlayerPrefsKey))
            {
                result.issues.Add(new ValidationIssue(ValidationSeverity.Warning, "Settings", "PlayerPrefs key is empty", "", settings));
            }
        }

        private static void ValidateLanguages(LocalizationSettings settings, ValidationResult result)
        {
            var languageCodes = new HashSet<string>();
            
            foreach (var language in settings.SupportedLanguages)
            {
                if (language == null)
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Language", "Null language in supported languages list"));
                    continue;
                }

                // Check for duplicate language codes
                if (languageCodes.Contains(language.LanguageCode))
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Language", $"Duplicate language code: {language.LanguageCode}", "", language));
                }
                else
                {
                    languageCodes.Add(language.LanguageCode);
                }

                // Validate language data
                if (string.IsNullOrEmpty(language.LanguageCode))
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Language", "Language code is empty", "", language));
                }

                if (string.IsNullOrEmpty(language.DisplayName))
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Warning, "Language", $"Display name is empty for language: {language.LanguageCode}", "", language));
                }

                // Check if table exists for language
                var table = settings.GetTableForLanguage(language);
                if (table == null)
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Warning, "Language", $"No localization table found for language: {language.DisplayName}", "", language));
                }
            }
        }

        private static void ValidateTables(LocalizationSettings settings, ValidationResult result)
        {
            foreach (var table in settings.LocalizationTables)
            {
                if (table == null)
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Table", "Null table in localization tables list"));
                    continue;
                }

                if (table.Language == null)
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Table", $"Table {table.name} has no language assigned", "", table));
                    continue;
                }

                // Check for duplicate keys within table
                var keyCount = new Dictionary<string, int>();
                foreach (var entry in table.Entries)
                {
                    if (string.IsNullOrEmpty(entry.key)) continue;
                    
                    keyCount[entry.key] = keyCount.ContainsKey(entry.key) ? keyCount[entry.key] + 1 : 1;
                }

                foreach (var kvp in keyCount.Where(k => k.Value > 1))
                {
                    result.duplicateKeys.Add(kvp.Key);
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Table", $"Duplicate key '{kvp.Key}' found {kvp.Value} times in {table.Language.DisplayName}", "", table));
                }

                // Check completion percentage
                float completion = table.CompletionPercentage;
                if (completion < 50f)
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Warning, "Translation", $"Low completion for {table.Language.DisplayName}: {completion:F1}%", "", table));
                }
            }
        }

        private static void ValidateKeys(LocalizationSettings settings, ValidationResult result)
        {
            var allKeys = settings.GetAllKeys();
            var defaultTable = settings.GetTableForLanguage(settings.DefaultLanguage);

            foreach (string key in allKeys)
            {
                // Check if key exists in default language
                if (defaultTable == null || !defaultTable.HasKey(key))
                {
                    result.missingKeys.Add(key);
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Key", $"Key '{key}' missing in default language", "", defaultTable));
                }

                // Check missing translations for each language
                foreach (var language in settings.SupportedLanguages)
                {
                    var table = settings.GetTableForLanguage(language);
                    if (table == null) continue;

                    if (!table.HasKey(key) || string.IsNullOrEmpty(table.GetValue(key)))
                    {
                        if (!result.missingTranslations.ContainsKey(language.LanguageCode))
                        {
                            result.missingTranslations[language.LanguageCode] = new List<string>();
                        }
                        result.missingTranslations[language.LanguageCode].Add(key);
                    }
                }

                // Validate key naming convention
                if (!IsValidKeyName(key))
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Warning, "Key", $"Key '{key}' doesn't follow naming convention (should be UPPER_CASE_WITH_UNDERSCORES)"));
                }
            }
        }

        private static void ValidateUsage(LocalizationSettings settings, ValidationResult result)
        {
            var usedKeys = FindUsedKeys();
            var allKeys = settings.GetAllKeys();
            
            // Find unused keys
            foreach (string key in allKeys)
            {
                if (!usedKeys.Contains(key))
                {
                    result.unusedKeys.Add(key);
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Info, "Usage", $"Key '{key}' is not used in any scene or prefab"));
                }
            }

            // Find missing keys (used but not defined)
            foreach (string usedKey in usedKeys)
            {
                if (!allKeys.Contains(usedKey))
                {
                    result.issues.Add(new ValidationIssue(ValidationSeverity.Error, "Usage", $"Key '{usedKey}' is used but not defined in localization tables"));
                }
            }
        }

        private static HashSet<string> FindUsedKeys()
        {
            var usedKeys = new HashSet<string>();

            #if UNITY_EDITOR
            // Scan all scenes
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    var scene = UnityEditor.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                    ScanSceneForKeys(scene, usedKeys);
                    UnityEditor.EditorSceneManager.CloseScene(scene, true);
                }
            }

            // Scan current active scene
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.isLoaded)
            {
                ScanSceneForKeys(activeScene, usedKeys);
            }

            // Scan all prefabs
            string[] prefabGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGUIDs)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    ScanGameObjectForKeys(prefab, usedKeys);
                }
            }
            #endif

            return usedKeys;
        }

        private static void ScanSceneForKeys(Scene scene, HashSet<string> usedKeys)
        {
            var rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                ScanGameObjectForKeys(obj, usedKeys);
            }
        }

        private static void ScanGameObjectForKeys(GameObject obj, HashSet<string> usedKeys)
        {
            if (obj == null) return;

            // Check LocalizedTextBinder components
            var binders = obj.GetComponentsInChildren<LocalizedTextBinder>(true);
            foreach (var binder in binders)
            {
                if (!string.IsNullOrEmpty(binder.Key))
                {
                    usedKeys.Add(binder.Key);
                }
            }

            // TODO: Add more scanning for other components that might use localization keys
        }

        private static bool IsValidKeyName(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            
            // Check if key follows UPPER_CASE_WITH_UNDERSCORES convention
            foreach (char c in key)
            {
                if (!char.IsUpper(c) && !char.IsDigit(c) && c != '_')
                {
                    return false;
                }
            }
            
            return true;
        }

        public static void LogValidationResults(ValidationResult result)
        {
            if (result.TotalIssues == 0)
            {
                Debug.Log("[LocalizationValidator] ✓ No issues found! Localization system is valid.");
                return;
            }

            Debug.Log($"[LocalizationValidator] Found {result.TotalIssues} issues:");

            foreach (var issue in result.issues.OrderBy(i => i.severity))
            {
                string logMessage = $"[{issue.category}] {issue.message}";
                if (!string.IsNullOrEmpty(issue.details))
                {
                    logMessage += $" - {issue.details}";
                }

                switch (issue.severity)
                {
                    case ValidationSeverity.Error:
                        Debug.LogError(logMessage, issue.context);
                        break;
                    case ValidationSeverity.Warning:
                        Debug.LogWarning(logMessage, issue.context);
                        break;
                    case ValidationSeverity.Info:
                        Debug.Log(logMessage, issue.context);
                        break;
                }
            }

            // Summary
            int errors = result.issues.Count(i => i.severity == ValidationSeverity.Error);
            int warnings = result.issues.Count(i => i.severity == ValidationSeverity.Warning);
            int infos = result.issues.Count(i => i.severity == ValidationSeverity.Info);

            Debug.Log($"[LocalizationValidator] Summary: {errors} errors, {warnings} warnings, {infos} info messages");
        }
    }
}