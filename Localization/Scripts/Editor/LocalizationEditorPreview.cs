using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

namespace TirexGame.Utils.Localization.Editor
{
    [InitializeOnLoad]
    public static class LocalizationEditorPreview
    {
        private static LocalizationSettings _settings;
        private static LanguageData _previewLanguage;
        private static bool _previewMode = false;

        // Events
        public static System.Action<LanguageData> OnPreviewLanguageChanged;

        static LocalizationEditorPreview()
        {
            LoadSettings();
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        public static LocalizationSettings Settings
        {
            get
            {
                if (_settings == null)
                    LoadSettings();
                return _settings;
            }
        }

        public static LanguageData PreviewLanguage
        {
            get => _previewLanguage;
            set
            {
                if (_previewLanguage != value)
                {
                    _previewLanguage = value;
                    OnPreviewLanguageChanged?.Invoke(_previewLanguage);
                    
                    if (_previewMode)
                    {
                        RefreshAllPreviews();
                    }
                }
            }
        }

        public static bool PreviewMode
        {
            get => _previewMode;
            set
            {
                if (_previewMode != value)
                {
                    _previewMode = value;
                    
                    if (_previewMode)
                    {
                        RefreshAllPreviews();
                    }
                    else
                    {
                        RestoreOriginalTexts();
                    }
                }
            }
        }

        private static void LoadSettings()
        {
            _settings = Resources.Load<LocalizationSettings>("LocalizationSettings");
            
            if (_settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:LocalizationSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
                }
            }

            if (_settings != null && _previewLanguage == null)
            {
                _previewLanguage = _settings.DefaultLanguage;
            }
        }

        private static void OnEditorUpdate()
        {
            // Check if settings changed
            if (_settings == null)
            {
                LoadSettings();
            }
        }

        private static void OnHierarchyChanged()
        {
            if (_previewMode)
            {
                // Refresh previews when hierarchy changes
                EditorApplication.delayCall += RefreshAllPreviews;
            }
        }

        public static void RefreshAllPreviews()
        {
            if (!_previewMode || _settings == null || _previewLanguage == null) return;

            var allBinders = Object.FindObjectsByType<LocalizedTextBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var binder in allBinders)
            {
                PreviewBinderText(binder);
            }
        }

        public static void PreviewBinderText(LocalizedTextBinder binder)
        {
            if (binder == null || _previewLanguage == null || string.IsNullOrEmpty(binder.Key)) return;

            var table = _settings.GetTableForLanguage(_previewLanguage);
            if (table != null)
            {
                string previewText = table.GetValue(binder.Key);
                if (!string.IsNullOrEmpty(previewText))
                {
                    binder.EditorPreviewText(binder.Key);
                }
                else
                {
                    binder.EditorResetToFallback();
                }
            }
        }

        private static void RestoreOriginalTexts()
        {
            var allBinders = Object.FindObjectsByType<LocalizedTextBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (var binder in allBinders)
            {
                binder.EditorResetToFallback();
            }
        }

        public static List<LanguageData> GetAvailableLanguages()
        {
            return _settings?.SupportedLanguages ?? new List<LanguageData>();
        }

        public static string GetPreviewText(string key)
        {
            if (_settings == null || _previewLanguage == null) return key;

            var table = _settings.GetTableForLanguage(_previewLanguage);
            return table?.GetValue(key) ?? key;
        }
    }

    // Toolbar GUI
    [InitializeOnLoad]
    public static class LocalizationToolbar
    {
        static LocalizationToolbar()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            var settings = LocalizationEditorPreview.Settings;
            if (settings == null) return;

            GUILayout.FlexibleSpace();

            // Preview toggle
            EditorGUI.BeginChangeCheck();
            bool newPreviewMode = GUILayout.Toggle(LocalizationEditorPreview.PreviewMode, "Preview", EditorStyles.toolbarButton, GUILayout.Width(60));
            if (EditorGUI.EndChangeCheck())
            {
                LocalizationEditorPreview.PreviewMode = newPreviewMode;
            }

            // Language dropdown
            if (LocalizationEditorPreview.PreviewMode)
            {
                var languages = LocalizationEditorPreview.GetAvailableLanguages();
                if (languages.Count > 0)
                {
                    var languageNames = languages.Select(l => l.DisplayName).ToArray();
                    int currentIndex = languages.IndexOf(LocalizationEditorPreview.PreviewLanguage);
                    
                    EditorGUI.BeginChangeCheck();
                    int newIndex = EditorGUILayout.Popup(currentIndex, languageNames, EditorStyles.toolbarPopup, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < languages.Count)
                    {
                        LocalizationEditorPreview.PreviewLanguage = languages[newIndex];
                    }
                }
            }

            // Localization Manager button
            if (GUILayout.Button("Loc Manager", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LocalizationEditorWindow.ShowWindow();
            }
        }
    }

    // Custom Scene View GUI
    [InitializeOnLoad]
    public static class LocalizationSceneGUI
    {
        static LocalizationSceneGUI()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!LocalizationEditorPreview.PreviewMode) return;

            Handles.BeginGUI();
            
            var rect = new Rect(10, 10, 200, 60);
            GUI.Box(rect, "");
            
            GUILayout.BeginArea(new Rect(15, 15, 190, 50));
            
            GUILayout.Label("Localization Preview", EditorStyles.boldLabel);
            
            var currentLang = LocalizationEditorPreview.PreviewLanguage;
            GUILayout.Label($"Language: {currentLang?.DisplayName ?? "None"}");
            
            GUILayout.EndArea();
            
            Handles.EndGUI();
        }
    }
}

// Toolbar extender utility
namespace TirexGame.Utils.Localization.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;

    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        static int m_toolCount;
        static GUIStyle m_commandStyle = null;

        public static readonly List<Action> LeftToolbarGUI = new List<Action>();
        public static readonly List<Action> RightToolbarGUI = new List<Action>();

        static ToolbarExtender()
        {
            Type toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

            #if UNITY_2019_1_OR_NEWER
            string fieldName = "k_ToolCount";
            #else
            string fieldName = "s_ShownToolIcons";
            #endif

            FieldInfo toolIcons = toolbarType.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            #if UNITY_2019_3_OR_NEWER
            m_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 8;
            #elif UNITY_2019_1_OR_NEWER
            m_toolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 7;
            #elif UNITY_2018_2_OR_NEWER
            m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 6;
            #else
            m_toolCount = toolIcons != null ? ((Array) toolIcons.GetValue(null)).Length : 5;
            #endif

            ToolbarCallback.OnToolbarGUI = OnGUI;
            ToolbarCallback.OnToolbarGUILeft = GUILeft;
            ToolbarCallback.OnToolbarGUIRight = GUIRight;
        }

        public static void OnGUI()
        {
            // Create two containers, left and right
            // Screen is whole toolbar

            if (m_commandStyle == null)
            {
                m_commandStyle = new GUIStyle("CommandLeft");
            }

            var screenWidth = EditorGUIUtility.currentViewWidth;

            // Following calculations match code reflected from Toolbar.OldOnGUI()
            float playButtonsPosition = Mathf.RoundToInt ((screenWidth - 140) / 2);

            Rect leftRect = new Rect(0, 0, playButtonsPosition, Screen.height);
            Rect rightRect = new Rect(playButtonsPosition + 140, 0, screenWidth - playButtonsPosition - 140, Screen.height);

            GUILayout.BeginArea(leftRect);
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.BeginArea(rightRect);
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }

        public static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler();
            }
            GUILayout.EndHorizontal();
        }
    }

    public static class ToolbarCallback
    {
        static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        static Type m_iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
        static PropertyInfo m_windowBackend = m_guiViewType.GetProperty("windowBackend",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static PropertyInfo m_viewVisualTree = m_iWindowBackendType.GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo m_imguiContainerOnGUI = typeof(Editor).Assembly.GetType("UnityEngine.UIElements.IMGUIContainer")?.GetField("m_OnGUIHandler",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static ScriptableObject m_currentToolbar;

        /// <summary>
        /// Callback for toolbar OnGUI method.
        /// </summary>
        public static Action OnToolbarGUI;
        public static Action OnToolbarGUILeft;
        public static Action OnToolbarGUIRight;

        static ToolbarCallback()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            // Check if required reflection types are available
            if (m_toolbarType == null || m_windowBackend == null || m_viewVisualTree == null)
                return;

            // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
            if (m_currentToolbar == null)
            {
                // Find toolbar
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
                if (m_currentToolbar != null)
                {
                    try
                    {
                        #if UNITY_2021_1_OR_NEWER
                        var windowBackendValue = m_windowBackend.GetValue(m_currentToolbar, null);
                        if (windowBackendValue == null)
                            return;

                        var root = m_viewVisualTree.GetValue(windowBackendValue, null);
                        if (root == null)
                            return;

                        var queryProperty = root.GetType().GetProperty("Query");
                        if (queryProperty == null)
                            return;

                        var toolbarZone = queryProperty.GetValue(root, null);
                        if (toolbarZone == null)
                            return;

                        var toolbarZoneCall = toolbarZone.GetType().GetMethod("Call", new Type[] { typeof(string) });
                        if (toolbarZoneCall == null)
                            return;

                        var container = toolbarZoneCall.Invoke(toolbarZone, new object[] { "ToolbarZoneLeftAlign" });
                        if (container != null)
                        {
                            var field = container.GetType().GetField("m_OnGUIHandler",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (field != null)
                            {
                                field.SetValue(container, (System.Action) OnToolbarGUILeft);
                            }
                        }

                        container = toolbarZoneCall.Invoke(toolbarZone, new object[] { "ToolbarZoneRightAlign" });
                        if (container != null)
                        {
                            var field = container.GetType().GetField("m_OnGUIHandler",
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (field != null)
                            {
                                field.SetValue(container, (System.Action) OnToolbarGUIRight);
                            }
                        }
                        #else
                        // Check if m_imguiContainerOnGUI is available for older Unity versions
                        if (m_imguiContainerOnGUI == null)
                            return;

                        var windowBackendValue = m_windowBackend.GetValue(m_currentToolbar, null);
                        if (windowBackendValue == null)
                            return;

                        // Get it's visual tree
                        var visualTree = m_viewVisualTree.GetValue(windowBackendValue, null);
                        if (visualTree == null)
                            return;

                        var itemProperty = visualTree.GetType().GetProperty("Item");
                        if (itemProperty == null)
                            return;

                        // Get first child which 'happens' to be toolbar IMGUIContainer
                        var toolbarContainer = itemProperty.GetValue(visualTree, new object[] { 0 });
                        if (toolbarContainer == null)
                            return;

                        // (Re)attach handler
                        var handler = m_imguiContainerOnGUI.GetValue(toolbarContainer);
                        m_imguiContainerOnGUI.SetValue(toolbarContainer, (Action) OnGUI);
                        #endif
                    }
                    catch (System.Exception)
                    {
                        // Silently ignore reflection errors that can occur during Unity refresh
                        // This prevents the NullReferenceException from being thrown
                    }
                }
            }
        }

        static void OnGUI()
        {
            var handler = OnToolbarGUI;
            if (handler != null) handler();
        }
    }
}