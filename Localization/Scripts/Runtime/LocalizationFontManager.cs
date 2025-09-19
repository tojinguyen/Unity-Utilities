using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

namespace TirexGame.Utils.Localization
{
    [Serializable]
    public class FontMapping
    {
        public LanguageData language;
        public Font legacyFont;
        public TMP_FontAsset tmpFont;
        public float sizeMultiplier = 1f;
        public float lineSpacingMultiplier = 1f;
    }

    [CreateAssetMenu(fileName = "FontManager", menuName = "TirexGame/Localization/Font Manager")]
    public class LocalizationFontManager : ScriptableObject
    {
        [Header("Font Mappings")]
        [SerializeField] private List<FontMapping> fontMappings = new List<FontMapping>();
        
        [Header("Default Fonts")]
        [SerializeField] private Font defaultLegacyFont;
        [SerializeField] private TMP_FontAsset defaultTMPFont;
        
        [Header("RTL Support")]
        [SerializeField] private bool enableRTLSupport = true;
        [SerializeField] private TMP_FontAsset arabicFont;
        [SerializeField] private TMP_FontAsset hebrewFont;

        public static LocalizationFontManager Instance { get; private set; }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public FontMapping GetFontMapping(LanguageData language)
        {
            if (language == null) return null;
            
            return fontMappings.Find(mapping => mapping.language == language);
        }

        public Font GetLegacyFont(LanguageData language)
        {
            var mapping = GetFontMapping(language);
            return mapping?.legacyFont ?? defaultLegacyFont;
        }

        public TMP_FontAsset GetTMPFont(LanguageData language)
        {
            var mapping = GetFontMapping(language);
            TMP_FontAsset font = mapping?.tmpFont ?? defaultTMPFont;

            // Special handling for RTL languages
            if (enableRTLSupport && language != null && language.IsRightToLeft)
            {
                if (language.LanguageCode == "ar" && arabicFont != null)
                    return arabicFont;
                else if (language.LanguageCode == "he" && hebrewFont != null)
                    return hebrewFont;
            }

            return font;
        }

        public float GetSizeMultiplier(LanguageData language)
        {
            var mapping = GetFontMapping(language);
            return mapping?.sizeMultiplier ?? 1f;
        }

        public float GetLineSpacingMultiplier(LanguageData language)
        {
            var mapping = GetFontMapping(language);
            return mapping?.lineSpacingMultiplier ?? 1f;
        }

        public void ApplyFontToText(Text legacyText, LanguageData language)
        {
            if (legacyText == null || language == null) return;

            var font = GetLegacyFont(language);
            if (font != null)
            {
                legacyText.font = font;
            }

            ApplyTextProperties(legacyText, language);
        }

        public void ApplyFontToText(TextMeshProUGUI tmpText, LanguageData language)
        {
            if (tmpText == null || language == null) return;

            var font = GetTMPFont(language);
            if (font != null)
            {
                tmpText.font = font;
            }

            ApplyTextProperties(tmpText, language);
        }

        public void ApplyFontToText(TextMeshPro tmp3DText, LanguageData language)
        {
            if (tmp3DText == null || language == null) return;

            var font = GetTMPFont(language);
            if (font != null)
            {
                tmp3DText.font = font;
            }

            ApplyTextProperties(tmp3DText, language);
        }

        private void ApplyTextProperties(Text legacyText, LanguageData language)
        {
            float sizeMultiplier = GetSizeMultiplier(language);
            if (sizeMultiplier != 1f)
            {
                legacyText.fontSize = Mathf.RoundToInt(legacyText.fontSize * sizeMultiplier);
            }

            float lineSpacing = GetLineSpacingMultiplier(language);
            if (lineSpacing != 1f)
            {
                legacyText.lineSpacing = lineSpacing;
            }

            // Apply text direction
            if (language.IsRightToLeft)
            {
                legacyText.alignment = GetRTLAlignment(legacyText.alignment);
            }
        }

        private void ApplyTextProperties(TextMeshProUGUI tmpText, LanguageData language)
        {
            float sizeMultiplier = GetSizeMultiplier(language);
            if (sizeMultiplier != 1f)
            {
                tmpText.fontSize = tmpText.fontSize * sizeMultiplier;
            }

            float lineSpacing = GetLineSpacingMultiplier(language);
            if (lineSpacing != 1f)
            {
                tmpText.lineSpacing = (lineSpacing - 1f) * 100f; // TMP uses percentage
            }

            // Apply text direction
            if (language.IsRightToLeft)
            {
                tmpText.alignment = GetRTLAlignment(tmpText.alignment);
                tmpText.isRightToLeftText = true;
            }
            else
            {
                tmpText.isRightToLeftText = false;
            }
        }

        private void ApplyTextProperties(TextMeshPro tmp3DText, LanguageData language)
        {
            float sizeMultiplier = GetSizeMultiplier(language);
            if (sizeMultiplier != 1f)
            {
                tmp3DText.fontSize = tmp3DText.fontSize * sizeMultiplier;
            }

            float lineSpacing = GetLineSpacingMultiplier(language);
            if (lineSpacing != 1f)
            {
                tmp3DText.lineSpacing = (lineSpacing - 1f) * 100f;
            }

            // Apply text direction
            if (language.IsRightToLeft)
            {
                tmp3DText.alignment = GetRTLAlignment(tmp3DText.alignment);
                tmp3DText.isRightToLeftText = true;
            }
            else
            {
                tmp3DText.isRightToLeftText = false;
            }
        }

        private TextAnchor GetRTLAlignment(TextAnchor originalAlignment)
        {
            // Convert left alignments to right alignments for RTL
            return originalAlignment switch
            {
                TextAnchor.UpperLeft => TextAnchor.UpperRight,
                TextAnchor.MiddleLeft => TextAnchor.MiddleRight,
                TextAnchor.LowerLeft => TextAnchor.LowerRight,
                TextAnchor.UpperRight => TextAnchor.UpperLeft,
                TextAnchor.MiddleRight => TextAnchor.MiddleLeft,
                TextAnchor.LowerRight => TextAnchor.LowerLeft,
                _ => originalAlignment
            };
        }

        private TextAlignmentOptions GetRTLAlignment(TextAlignmentOptions originalAlignment)
        {
            // Convert left alignments to right alignments for RTL
            return originalAlignment switch
            {
                TextAlignmentOptions.TopLeft => TextAlignmentOptions.TopRight,
                TextAlignmentOptions.Left => TextAlignmentOptions.Right,
                TextAlignmentOptions.BottomLeft => TextAlignmentOptions.BottomRight,
                TextAlignmentOptions.BaselineLeft => TextAlignmentOptions.BaselineRight,
                TextAlignmentOptions.MidlineLeft => TextAlignmentOptions.MidlineRight,
                TextAlignmentOptions.CaplineLeft => TextAlignmentOptions.CaplineRight,
                TextAlignmentOptions.TopRight => TextAlignmentOptions.TopLeft,
                TextAlignmentOptions.Right => TextAlignmentOptions.Left,
                TextAlignmentOptions.BottomRight => TextAlignmentOptions.BottomLeft,
                TextAlignmentOptions.BaselineRight => TextAlignmentOptions.BaselineLeft,
                TextAlignmentOptions.MidlineRight => TextAlignmentOptions.MidlineLeft,
                TextAlignmentOptions.CaplineRight => TextAlignmentOptions.CaplineLeft,
                _ => originalAlignment
            };
        }

        #if UNITY_EDITOR
        public void AddFontMapping(LanguageData language, Font legacyFont, TMP_FontAsset tmpFont)
        {
            var existingMapping = GetFontMapping(language);
            if (existingMapping != null)
            {
                existingMapping.legacyFont = legacyFont;
                existingMapping.tmpFont = tmpFont;
            }
            else
            {
                fontMappings.Add(new FontMapping
                {
                    language = language,
                    legacyFont = legacyFont,
                    tmpFont = tmpFont
                });
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void RemoveFontMapping(LanguageData language)
        {
            var mapping = GetFontMapping(language);
            if (mapping != null)
            {
                fontMappings.Remove(mapping);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        #endif
    }
}