using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace TirexGame.Utils.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizationParameters))]
    public class LocalizationParametersDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var parametersProperty = property.FindPropertyRelative("parameters");
            
            // Header
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, parametersProperty, new GUIContent("Parameters"), true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var parametersProperty = property.FindPropertyRelative("parameters");
            return EditorGUI.GetPropertyHeight(parametersProperty, true);
        }
    }

    [CustomEditor(typeof(LocalizedTextBinder))]
    public class LocalizedTextBinderEditor : Editor
    {
        private SerializedProperty _keyProperty;
        private SerializedProperty _updateOnLanguageChangeProperty;
        private SerializedProperty _updateOnStartProperty;
        private SerializedProperty _parametersProperty;
        private SerializedProperty _fallbackTextProperty;
        private SerializedProperty _useFallbackWhenMissingProperty;
        private SerializedProperty _useLanguageFontProperty;
        private SerializedProperty _customFontProperty;
        private SerializedProperty _customTMPFontProperty;

        private string[] _availableKeys = new string[0];
        private int _selectedKeyIndex = -1;
        private bool _showPreview = false;

        private void OnEnable()
        {
            _keyProperty = serializedObject.FindProperty("localizationKey");
            _updateOnLanguageChangeProperty = serializedObject.FindProperty("updateOnLanguageChange");
            _updateOnStartProperty = serializedObject.FindProperty("updateOnStart");
            _parametersProperty = serializedObject.FindProperty("parameters");
            _fallbackTextProperty = serializedObject.FindProperty("fallbackText");
            _useFallbackWhenMissingProperty = serializedObject.FindProperty("useFallbackWhenMissing");
            _useLanguageFontProperty = serializedObject.FindProperty("useLanguageFont");
            _customFontProperty = serializedObject.FindProperty("customFont");
            _customTMPFontProperty = serializedObject.FindProperty("customTMPFont");

            LoadAvailableKeys();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var binder = target as LocalizedTextBinder;

            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Localized Text Binder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Localization Key Section
            EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);
            DrawKeySelector();
            EditorGUILayout.PropertyField(_updateOnLanguageChangeProperty);
            EditorGUILayout.PropertyField(_updateOnStartProperty);

            EditorGUILayout.Space();

            // Parameters Section
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_parametersProperty);

            EditorGUILayout.Space();

            // Fallback Section
            EditorGUILayout.LabelField("Fallback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_fallbackTextProperty);
            EditorGUILayout.PropertyField(_useFallbackWhenMissingProperty);

            EditorGUILayout.Space();

            // Font Override Section
            EditorGUILayout.LabelField("Font Override", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useLanguageFontProperty);
            
            if (!_useLanguageFontProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_customFontProperty);
                EditorGUILayout.PropertyField(_customTMPFontProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Preview Section
            DrawPreviewSection(binder);

            // Buttons
            EditorGUILayout.Space();
            DrawActionButtons(binder);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKeySelector()
        {
            EditorGUILayout.BeginHorizontal();

            // Key dropdown
            if (_availableKeys.Length > 0)
            {
                UpdateSelectedKeyIndex();
                
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Localization Key", _selectedKeyIndex, _availableKeys);
                if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < _availableKeys.Length)
                {
                    _keyProperty.stringValue = _availableKeys[newIndex];
                    _selectedKeyIndex = newIndex;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_keyProperty, new GUIContent("Localization Key"));
            }

            // Refresh button
            if (GUILayout.Button("↻", GUILayout.Width(25)))
            {
                LoadAvailableKeys();
            }

            EditorGUILayout.EndHorizontal();

            // Show current key if not in dropdown
            if (_selectedKeyIndex == -1 && !string.IsNullOrEmpty(_keyProperty.stringValue))
            {
                EditorGUILayout.HelpBox($"Current key: {_keyProperty.stringValue} (not found in available keys)", MessageType.Warning);
            }
        }

        private void DrawPreviewSection(LocalizedTextBinder binder)
        {
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Preview", true);
            
            if (_showPreview)
            {
                EditorGUI.indentLevel++;

                if (Application.isPlaying && LocalizationManager.Instance != null && LocalizationManager.Instance.IsReady)
                {
                    // Show current language
                    var currentLanguage = LocalizationManager.Instance.CurrentLanguage;
                    EditorGUILayout.LabelField("Current Language", currentLanguage?.DisplayName ?? "None");

                    // Show localized text
                    if (!string.IsNullOrEmpty(_keyProperty.stringValue))
                    {
                        string localizedText = LocalizationManager.Localize(_keyProperty.stringValue);
                        EditorGUILayout.LabelField("Localized Text", localizedText);
                        
                        // Show completion percentage
                        float completion = LocalizationManager.GetCurrentLanguageCompletionPercentage();
                        EditorGUILayout.LabelField("Completion", $"{completion:F1}%");
                    }

                    // Language selector for testing
                    var languages = LocalizationManager.GetSupportedLanguages();
                    if (languages.Count > 1)
                    {
                        var languageNames = languages.Select(l => l.DisplayName).ToArray();
                        int currentIndex = languages.FindIndex(l => l == currentLanguage);
                        
                        EditorGUI.BeginChangeCheck();
                        int newIndex = EditorGUILayout.Popup("Test Language", currentIndex, languageNames);
                        if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < languages.Count)
                        {
                            LocalizationManager.SetLanguage(languages[newIndex]);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Preview is available only in Play Mode when LocalizationManager is ready.", MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawActionButtons(LocalizedTextBinder binder)
        {
            EditorGUILayout.BeginHorizontal();

            // Refresh button
            if (GUILayout.Button("Refresh Text"))
            {
                if (Application.isPlaying)
                {
                    binder.Refresh();
                }
                else
                {
                    EditorGUILayout.HelpBox("Refresh is available only in Play Mode.", MessageType.Info);
                }
            }

            // Test with fallback
            if (GUILayout.Button("Test Fallback"))
            {
                if (Application.isPlaying)
                {
                    binder.EditorResetToFallback();
                }
                else
                {
                    Debug.Log($"Fallback text: {_fallbackTextProperty.stringValue}");
                }
            }

            EditorGUILayout.EndHorizontal();

            // Validation
            if (!binder.HasTextComponent())
            {
                EditorGUILayout.HelpBox("No Text component found! Please add a Text, TextMeshPro, or TextMeshProUGUI component.", MessageType.Warning);
            }

            if (string.IsNullOrEmpty(_keyProperty.stringValue) && string.IsNullOrEmpty(_fallbackTextProperty.stringValue))
            {
                EditorGUILayout.HelpBox("Both localization key and fallback text are empty. Please set at least one.", MessageType.Warning);
            }
        }

        private void LoadAvailableKeys()
        {
            var settings = Resources.Load<LocalizationSettings>("LocalizationSettings");
            if (settings != null)
            {
                _availableKeys = settings.GetAllKeys().ToArray();
            }
            else
            {
                _availableKeys = new string[0];
            }

            UpdateSelectedKeyIndex();
        }

        private void UpdateSelectedKeyIndex()
        {
            string currentKey = _keyProperty.stringValue;
            _selectedKeyIndex = System.Array.IndexOf(_availableKeys, currentKey);
        }
    }
}