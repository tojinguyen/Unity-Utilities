using System;

namespace Tirex.Game.Utils.Localization
{
    /// <summary>
    /// Enum representing supported language codes following ISO 639-1 standard
    /// </summary>
    [Serializable]
    public enum LanguageCode
    {
        EN = 0,  // English
        VI = 1,  // Vietnamese
        JP = 2,  // Japanese
        KO = 3,  // Korean
        ZH = 4,  // Chinese (Simplified)
        TH = 5,  // Thai
        ID = 6,  // Indonesian
        ES = 7,  // Spanish
        FR = 8,  // French
        DE = 9,  // German
        RU = 10, // Russian
        PT = 11, // Portuguese
        AR = 12, // Arabic
        HI = 13, // Hindi
        IT = 14, // Italian
    }

    /// <summary>
    /// Language information with display name and culture info
    /// </summary>
    [Serializable]
    public class LanguageInfo
    {
        public LanguageCode code;
        public string displayName;
        public string nativeName;
        public string cultureCode;
        public bool isRightToLeft;

        public LanguageInfo(LanguageCode code, string displayName, string nativeName, string cultureCode, bool isRightToLeft = false)
        {
            this.code = code;
            this.displayName = displayName;
            this.nativeName = nativeName;
            this.cultureCode = cultureCode;
            this.isRightToLeft = isRightToLeft;
        }
    }
}
