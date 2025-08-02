// AUTO-GENERATED FILE - DO NOT EDIT MANUALLY
// Generated from LocalizationConfig: Example
// Generated at: 2025-08-02 12:00:00

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Auto-generated localization keys enum
    /// Use this instead of magic strings for better intellisense and error checking
    /// </summary>
    public enum LocalizationKeys
    {
        /// <summary>Key: ui_example_message</summary>
        UiExampleMessage,

        /// <summary>Key: ui_player_score</summary>
        UiPlayerScore,

        /// <summary>Key: ui_settings</summary>
        UiSettings,

        /// <summary>Key: ui_start_game</summary>
        UiStartGame,

        /// <summary>Key: ui_time_remaining</summary>
        UiTimeRemaining,

        /// <summary>Key: ui_welcome_title</summary>
        UiWelcomeTitle,
    }

    /// <summary>
    /// Extension methods for LocalizationKeys enum
    /// </summary>
    public static class LocalizationKeysExtensions
    {
        /// <summary>
        /// Convert enum to string key
        /// </summary>
        public static string ToKey(this LocalizationKeys key)
        {
            return key.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Get localized text using enum key
        /// </summary>
        public static string GetText(this LocalizationKeys key)
        {
            return LocalizationManager.GetLocalizedText(key.ToKey());
        }

        /// <summary>
        /// Get localized text with formatting using enum key
        /// </summary>
        public static string GetText(this LocalizationKeys key, params object[] args)
        {
            return LocalizationManager.GetLocalizedText(key.ToKey(), args);
        }

        /// <summary>
        /// Get localized sprite using enum key
        /// </summary>
        public static Sprite GetSprite(this LocalizationKeys key)
        {
            return LocalizationManager.GetLocalizedSprite(key.ToKey());
        }
    }
}
