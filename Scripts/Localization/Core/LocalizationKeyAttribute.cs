using System;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Attribute to mark string fields as localization keys
    /// Enables dropdown selection in inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LocalizationKeyAttribute : Attribute
    {
        public bool textKeysOnly;
        public bool spriteKeysOnly;

        /// <summary>
        /// Mark field as localization key selector
        /// </summary>
        /// <param name="textKeysOnly">Show only text keys</param>
        /// <param name="spriteKeysOnly">Show only sprite keys</param>
        public LocalizationKeyAttribute(bool textKeysOnly = false, bool spriteKeysOnly = false)
        {
            this.textKeysOnly = textKeysOnly;
            this.spriteKeysOnly = spriteKeysOnly;
        }
    }
}
