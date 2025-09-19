using System;
using TMPro;
using UnityEngine;

namespace TirexGame.Utils.Localization
{
    [Serializable]
    public enum TextDirection
    {
        LeftToRight,
        RightToLeft
    }

    [CreateAssetMenu(fileName = "New Language", menuName = "TirexGame/Localization/Language Data")]
    public class LanguageData : ScriptableObject
    {
        [Header("Language Info")]
        [SerializeField] private string displayName = "English";
        [SerializeField] private string languageCode = "en";
        [SerializeField] private string countryCode = "US";
        
        [Header("Text Properties")]
        [SerializeField] private TextDirection textDirection = TextDirection.LeftToRight;
        [SerializeField] private Font defaultFont;
        [SerializeField] private TMP_FontAsset defaultTMPFont;
        
        [Header("Localization")]
        [SerializeField] private bool isDefault = false;
        [SerializeField] private bool isEnabled = true;

        // Properties
        public string DisplayName => displayName;
        public string LanguageCode => languageCode;
        public string CountryCode => countryCode;
        public string FullCode => $"{languageCode}-{countryCode}";
        public TextDirection Direction => textDirection;
        public Font DefaultFont => defaultFont;
        public TMP_FontAsset DefaultTMPFont => defaultTMPFont;
        public bool IsDefault => isDefault;
        public bool IsEnabled => isEnabled;
        public bool IsRightToLeft => textDirection == TextDirection.RightToLeft;

        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(languageCode))
                languageCode = "en";
            
            if (string.IsNullOrEmpty(countryCode))
                countryCode = "US";
            
            if (string.IsNullOrEmpty(displayName))
                displayName = languageCode.ToUpper();
        }

        public override string ToString()
        {
            return $"{displayName} ({FullCode})";
        }
    }
}