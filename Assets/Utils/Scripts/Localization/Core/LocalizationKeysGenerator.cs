using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Utility for generating strongly-typed localization keys
    /// Auto-generates enums and constants from localization data
    /// </summary>
    public static class LocalizationKeysGenerator
    {
        private const string KEYS_ENUM_TEMPLATE = @"// AUTO-GENERATED FILE - DO NOT EDIT MANUALLY
// Generated from LocalizationConfig: {0}
// Generated at: {1}

namespace Tirex.Game.Utils.Localization
{{
    /// <summary>
    /// Auto-generated localization keys enum
    /// Use this instead of magic strings for better intellisense and error checking
    /// </summary>
    public enum LocalizationKeys
    {{
{2}
    }}

    /// <summary>
    /// Extension methods for LocalizationKeys enum
    /// </summary>
    public static class LocalizationKeysExtensions
    {{
        /// <summary>
        /// Convert enum to string key
        /// </summary>
        public static string ToKey(this LocalizationKeys key)
        {{
            return key.ToString().ToLowerInvariant();
        }}

        /// <summary>
        /// Get localized text using enum key
        /// </summary>
        public static string GetText(this LocalizationKeys key)
        {{
            return LocalizationManager.GetLocalizedText(key.ToKey());
        }}

        /// <summary>
        /// Get localized text with formatting using enum key
        /// </summary>
        public static string GetText(this LocalizationKeys key, params object[] args)
        {{
            return LocalizationManager.GetLocalizedText(key.ToKey(), args);
        }}

        /// <summary>
        /// Get localized sprite using enum key
        /// </summary>
        public static Sprite GetSprite(this LocalizationKeys key)
        {{
            return LocalizationManager.GetLocalizedSprite(key.ToKey());
        }}
    }}
}}";

        private const string KEYS_CONSTANTS_TEMPLATE = @"// AUTO-GENERATED FILE - DO NOT EDIT MANUALLY
// Generated from LocalizationConfig: {0}
// Generated at: {1}

namespace Tirex.Game.Utils.Localization
{{
    /// <summary>
    /// Auto-generated localization keys constants
    /// Use these constants instead of magic strings
    /// </summary>
    public static class LocalizationKeys
    {{
{2}
    }}
}}";

        /// <summary>
        /// Generate LocalizationKeys enum from config
        /// </summary>
        /// <param name="config">Localization configuration</param>
        /// <param name="outputPath">Output path for generated file</param>
        /// <param name="useEnum">Generate enum (true) or constants (false)</param>
        /// <returns>Success status</returns>
        public static bool GenerateKeys(LocalizationConfig config, string outputPath = null, bool useEnum = true)
        {
            if (config == null)
            {
                Debug.LogError("LocalizationConfig is null!");
                return false;
            }

            // Get all text keys
            var textKeys = config.GetAllTextKeys();
            var spriteKeys = config.GetAllSpriteKeys();
            
            // Combine and sort all keys
            var allKeys = new HashSet<string>(textKeys);
            foreach (var spriteKey in spriteKeys)
            {
                allKeys.Add(spriteKey);
            }

            if (allKeys.Count == 0)
            {
                Debug.LogWarning("No localization keys found to generate!");
                return false;
            }

            // Default output path
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(Application.dataPath, "Utils", "Scripts", "Localization", "Generated", "LocalizationKeys.cs");
            }

            // Ensure directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                string generatedCode;
                if (useEnum)
                {
                    generatedCode = GenerateEnumCode(config, allKeys);
                }
                else
                {
                    generatedCode = GenerateConstantsCode(config, allKeys);
                }

                File.WriteAllText(outputPath, generatedCode, Encoding.UTF8);
                Debug.Log($"Successfully generated localization keys at: {outputPath}");

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to generate localization keys: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate enum-based code
        /// </summary>
        private static string GenerateEnumCode(LocalizationConfig config, HashSet<string> keys)
        {
            var enumEntries = new StringBuilder();
            var sortedKeys = new List<string>(keys);
            sortedKeys.Sort();

            foreach (var key in sortedKeys)
            {
                string enumName = ConvertKeyToEnumName(key);
                string comment = $"        /// <summary>Key: {key}</summary>";
                string enumEntry = $"        {enumName},";
                
                enumEntries.AppendLine(comment);
                enumEntries.AppendLine(enumEntry);
                enumEntries.AppendLine();
            }

            return string.Format(KEYS_ENUM_TEMPLATE,
                config.name,
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                enumEntries.ToString().TrimEnd());
        }

        /// <summary>
        /// Generate constants-based code
        /// </summary>
        private static string GenerateConstantsCode(LocalizationConfig config, HashSet<string> keys)
        {
            var constantEntries = new StringBuilder();
            var sortedKeys = new List<string>(keys);
            sortedKeys.Sort();

            foreach (var key in sortedKeys)
            {
                string constantName = ConvertKeyToConstantName(key);
                string comment = $"        /// <summary>Localization key: {key}</summary>";
                string constantEntry = $"        public const string {constantName} = \"{key}\";";
                
                constantEntries.AppendLine(comment);
                constantEntries.AppendLine(constantEntry);
                constantEntries.AppendLine();
            }

            return string.Format(KEYS_CONSTANTS_TEMPLATE,
                config.name,
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                constantEntries.ToString().TrimEnd());
        }

        /// <summary>
        /// Convert localization key to valid enum name
        /// </summary>
        private static string ConvertKeyToEnumName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "INVALID_KEY";

            // Convert to PascalCase and remove invalid characters
            var parts = key.Split('_', '-', '.', ' ');
            var enumName = new StringBuilder();

            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    string cleanPart = System.Text.RegularExpressions.Regex.Replace(part, @"[^a-zA-Z0-9]", "");
                    if (!string.IsNullOrEmpty(cleanPart))
                    {
                        enumName.Append(char.ToUpper(cleanPart[0]));
                        if (cleanPart.Length > 1)
                        {
                            enumName.Append(cleanPart.Substring(1).ToLower());
                        }
                    }
                }
            }

            string result = enumName.ToString();
            
            // Ensure it starts with a letter
            if (string.IsNullOrEmpty(result) || !char.IsLetter(result[0]))
            {
                result = "Key" + result;
            }

            // Handle common cases
            if (string.IsNullOrEmpty(result))
                result = "InvalidKey";

            return result;
        }

        /// <summary>
        /// Convert localization key to valid constant name
        /// </summary>
        private static string ConvertKeyToConstantName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "INVALID_KEY";

            // Convert to UPPER_CASE
            string constantName = key.ToUpper().Replace('-', '_').Replace('.', '_').Replace(' ', '_');
            
            // Remove invalid characters
            constantName = System.Text.RegularExpressions.Regex.Replace(constantName, @"[^A-Z0-9_]", "");
            
            // Ensure it starts with a letter
            if (!string.IsNullOrEmpty(constantName) && !char.IsLetter(constantName[0]))
            {
                constantName = "KEY_" + constantName;
            }

            if (string.IsNullOrEmpty(constantName))
                constantName = "INVALID_KEY";

            return constantName;
        }

        /// <summary>
        /// Get list of all keys from config for dropdown
        /// </summary>
        public static List<string> GetAllKeysFromConfig(LocalizationConfig config)
        {
            if (config == null)
                return new List<string>();

            var allKeys = new HashSet<string>(config.GetAllTextKeys());
            foreach (var spriteKey in config.GetAllSpriteKeys())
            {
                allKeys.Add(spriteKey);
            }

            var sortedKeys = new List<string>(allKeys);
            sortedKeys.Sort();
            return sortedKeys;
        }

        /// <summary>
        /// Check if generated keys file exists and is up to date
        /// </summary>
        public static bool IsGeneratedFileUpToDate(LocalizationConfig config, string outputPath = null)
        {
            if (config == null)
                return false;

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.Combine(Application.dataPath, "Utils", "Scripts", "Localization", "Generated", "LocalizationKeys.cs");
            }

            if (!File.Exists(outputPath))
                return false;

            // Check if file contains all current keys
            string fileContent = File.ReadAllText(outputPath);
            var currentKeys = GetAllKeysFromConfig(config);

            foreach (var key in currentKeys)
            {
                string enumName = ConvertKeyToEnumName(key);
                if (!fileContent.Contains(enumName))
                {
                    return false;
                }
            }

            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor only: Auto-generate keys when config changes
        /// </summary>
        [UnityEditor.InitializeOnLoadMethod]
        private static void AutoGenerateOnLoad()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                // Find all LocalizationConfig assets
                var guids = UnityEditor.AssetDatabase.FindAssets("t:LocalizationConfig");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var config = UnityEditor.AssetDatabase.LoadAssetAtPath<LocalizationConfig>(path);
                    
                    if (config != null && !IsGeneratedFileUpToDate(config))
                    {
                        Debug.Log($"Auto-generating localization keys for {config.name}");
                        GenerateKeys(config);
                    }
                }
            };
        }
#endif
    }
}
