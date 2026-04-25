using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TirexGame.Utils.LoadingScene.Editor
{
    /// <summary>
    /// Editor Window để tạo nhanh LoadingManager Prefab.
    /// Menu: Tools → TirexGame → Loading Manager Creator
    /// </summary>
    public class LoadingManagerCreatorWindow : EditorWindow
    {
        // ---- Prefab Settings ----
        private string _prefabName = "LoadingManager";
        private string _savePath = "Assets/Prefabs";

        // ---- Canvas Settings ----
        private int _canvasSortingOrder = 1000;

        // ---- UI Toggles ----
        private bool _addProgressBar = true;
        private bool _addStepNameText = true;
        private bool _addStepDescText = true;
        private bool _addPercentText = true;
        private bool _addProgressInfoText = false;
        private bool _addCancelButton = false;
        private bool _addErrorPanel = false;

        // ---- Animation Preset ----
        private AnimationPreset _animationPreset = AnimationPreset.Fade;
        private Color _panelBackgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.92f);
        private Color _progressBarColor = new Color(0.2f, 0.7f, 1f, 1f);
        private Color _textColor = Color.white;

        // ---- Foldouts ----
        private bool _showUIOptions = true;
        private bool _showAnimOptions = true;
        private bool _showStyleOptions = true;

        private Vector2 _scroll;
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;

        private enum AnimationPreset
        {
            None,
            Fade,
            Slide,
            Scale,
            SpinnerWithFade
        }

        [MenuItem("Tools/TirexGame/Loading Manager Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoadingManagerCreatorWindow>("Loading Manager Creator");
            window.minSize = new Vector2(380, 520);
            window.maxSize = new Vector2(480, 800);
        }

        private void OnGUI()
        {
            InitStyles();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawHeader();
            EditorGUILayout.Space(4);

            DrawSection("📁 Prefab Settings", ref _showUIOptions, DrawPrefabSettings);
            DrawSection("🖼 UI Components", ref _showUIOptions, DrawUIOptions);
            DrawSection("🎬 Animation Preset", ref _showAnimOptions, DrawAnimOptions);
            DrawSection("🎨 Style", ref _showStyleOptions, DrawStyleOptions);

            EditorGUILayout.Space(8);
            DrawCreateButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("⚡ Loading Manager Creator", _headerStyle);
            EditorGUILayout.LabelField("Tạo nhanh LoadingManager Prefab hoàn chỉnh", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(4);
            DrawHorizontalLine();
        }

        private void DrawPrefabSettings()
        {
            _prefabName = EditorGUILayout.TextField("Prefab Name", _prefabName);

            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path → relative
                    if (path.StartsWith(Application.dataPath))
                        _savePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            _canvasSortingOrder = EditorGUILayout.IntField("Canvas Sorting Order", _canvasSortingOrder);
        }

        private void DrawUIOptions()
        {
            _addProgressBar = EditorGUILayout.Toggle("Progress Bar (Slider)", _addProgressBar);
            _addStepNameText = EditorGUILayout.Toggle("Step Name Text", _addStepNameText);
            _addStepDescText = EditorGUILayout.Toggle("Step Description Text", _addStepDescText);
            _addPercentText = EditorGUILayout.Toggle("Progress % Text", _addPercentText);
            _addProgressInfoText = EditorGUILayout.Toggle("Progress Info Text", _addProgressInfoText);
            EditorGUILayout.Space(2);
            _addCancelButton = EditorGUILayout.Toggle("Cancel Button", _addCancelButton);
            _addErrorPanel = EditorGUILayout.Toggle("Error Panel", _addErrorPanel);
        }

        private void DrawAnimOptions()
        {
            _animationPreset = (AnimationPreset)EditorGUILayout.EnumPopup("Preset", _animationPreset);

            EditorGUILayout.Space(2);
            switch (_animationPreset)
            {
                case AnimationPreset.None:
                    EditorGUILayout.HelpBox("Không có animation. Show/Hide ngay lập tức.", MessageType.Info);
                    break;
                case AnimationPreset.Fade:
                    EditorGUILayout.HelpBox("Thêm FadeAnimationStrategy. Fade in khi show, fade out khi hide.", MessageType.Info);
                    break;
                case AnimationPreset.Slide:
                    EditorGUILayout.HelpBox("Thêm SlideAnimationStrategy. Slide từ dưới lên khi show.", MessageType.Info);
                    break;
                case AnimationPreset.Scale:
                    EditorGUILayout.HelpBox("Thêm ScaleAnimationStrategy. Zoom in khi show, zoom out khi hide.", MessageType.Info);
                    break;
                case AnimationPreset.SpinnerWithFade:
                    EditorGUILayout.HelpBox("Thêm SpinnerAnimationStrategy. Fade + spinner xoay khi loading.", MessageType.Info);
                    break;
            }
        }

        private void DrawStyleOptions()
        {
            _panelBackgroundColor = EditorGUILayout.ColorField("Panel Background", _panelBackgroundColor);
            _progressBarColor = EditorGUILayout.ColorField("Progress Bar Color", _progressBarColor);
            _textColor = EditorGUILayout.ColorField("Text Color", _textColor);
        }

        private void DrawCreateButton()
        {
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("✨ Create LoadingManager Prefab", GUILayout.Height(40)))
            {
                CreatePrefab();
            }
            GUI.backgroundColor = Color.white;
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Prefab Creation
        // ─────────────────────────────────────────────────────────────────────────────

        private void CreatePrefab()
        {
            if (string.IsNullOrEmpty(_prefabName))
            {
                EditorUtility.DisplayDialog("Error", "Prefab name cannot be empty!", "OK");
                return;
            }

            // Đảm bảo thư mục tồn tại
            if (!AssetDatabase.IsValidFolder(_savePath))
            {
                System.IO.Directory.CreateDirectory(_savePath);
                AssetDatabase.Refresh();
            }

            // ── Tạo root LoadingManager ──────────────────────────────────────────────
            var root = new GameObject(_prefabName);
            var loadingManager = root.AddComponent<LoadingManager>();

            // ── Tạo UI hierarchy dưới root ────────────────────────────────────────────
            var uiRoot = CreateUIRoot(root.transform);
            var controller = uiRoot.GetComponent<DefaultLoadingUIController>();

            // ── Thêm Animation Strategy ───────────────────────────────────────────────
            AttachAnimationStrategy(uiRoot, controller);

            // ── Lưu Prefab ────────────────────────────────────────────────────────────
            string assetPath = $"{_savePath}/{_prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            DestroyImmediate(root);

            if (prefab != null)
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("✅ Success",
                    $"Prefab created at:\n{assetPath}\n\nDrag nó vào scene và assign UIController vào LoadingManager nếu cần.",
                    "OK");
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                EditorUtility.DisplayDialog("❌ Error", "Failed to create prefab!", "OK");
            }
        }

        private GameObject CreateUIRoot(Transform parent)
        {
            // Canvas
            var canvasGo = new GameObject("LoadingUI");
            canvasGo.transform.SetParent(parent, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _canvasSortingOrder;

            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var controller = canvasGo.AddComponent<DefaultLoadingUIController>();

            // LoadingPanel
            var panelGo = CreatePanel(canvasGo.transform, "LoadingPanel", _panelBackgroundColor);

            // ── Progress Bar ──────────────────────────────────────────────────────────
            Slider progressBar = null;
            if (_addProgressBar)
            {
                progressBar = CreateProgressBar(panelGo.transform);
            }

            // ── Texts ─────────────────────────────────────────────────────────────────
            TextMeshProUGUI stepNameText = null;
            if (_addStepNameText)
                stepNameText = CreateTMProText(panelGo.transform, "StepNameText", "Loading...", 24, FontStyles.Bold, new Vector2(0, 60));

            TextMeshProUGUI stepDescText = null;
            if (_addStepDescText)
                stepDescText = CreateTMProText(panelGo.transform, "StepDescText", "Please wait...", 16, FontStyles.Normal, new Vector2(0, 30));

            TextMeshProUGUI percentText = null;
            if (_addPercentText)
                percentText = CreateTMProText(panelGo.transform, "PercentText", "0%", 20, FontStyles.Bold, new Vector2(0, -40));

            TextMeshProUGUI infoText = null;
            if (_addProgressInfoText)
                infoText = CreateTMProText(panelGo.transform, "ProgressInfoText", "Step 0/0", 14, FontStyles.Normal, new Vector2(0, -70));

            // ── Cancel Button ─────────────────────────────────────────────────────────
            Button cancelButton = null;
            if (_addCancelButton)
                cancelButton = CreateButton(panelGo.transform, "CancelButton", "Cancel", new Vector2(0, -110));

            // ── Error Panel ───────────────────────────────────────────────────────────
            GameObject errorPanel = null;
            TextMeshProUGUI errorText = null;
            Button errorCloseButton = null;
            if (_addErrorPanel)
                (errorPanel, errorText, errorCloseButton) = CreateErrorPanel(canvasGo.transform);

            // ── Wire references vào Controller ────────────────────────────────────────
            var so = new SerializedObject(controller);
            SetSerializedField(so, "_loadingPanel", panelGo);
            SetSerializedField(so, "_progressBar", progressBar);
            SetSerializedField(so, "_stepNameText", stepNameText);
            SetSerializedField(so, "_stepDescriptionText", stepDescText);
            SetSerializedField(so, "_progressPercentText", percentText);
            SetSerializedField(so, "_progressInfoText", infoText);
            SetSerializedField(so, "_cancelButton", cancelButton);
            SetSerializedField(so, "_errorPanel", errorPanel);
            SetSerializedField(so, "_errorText", errorText);
            SetSerializedField(so, "_errorCloseButton", errorCloseButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            return canvasGo;
        }

        private void AttachAnimationStrategy(GameObject uiRoot, DefaultLoadingUIController controller)
        {
            if (_animationPreset == AnimationPreset.None) return;

            MonoBehaviour strategy = _animationPreset switch
            {
                AnimationPreset.Fade => uiRoot.AddComponent<FadeAnimationStrategy>(),
                AnimationPreset.Slide => uiRoot.AddComponent<SlideAnimationStrategy>(),
                AnimationPreset.Scale => uiRoot.AddComponent<ScaleAnimationStrategy>(),
                AnimationPreset.SpinnerWithFade => uiRoot.AddComponent<SpinnerAnimationStrategy>(),
                _ => null
            };

            if (strategy != null)
            {
                var so = new SerializedObject(controller);
                SetSerializedField(so, "_animationStrategyObject", strategy);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // UI Helpers
        // ─────────────────────────────────────────────────────────────────────────────

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = color;

            return go;
        }

        private Slider CreateProgressBar(Transform parent)
        {
            var go = new GameObject("ProgressBar");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 20);
            rect.anchoredPosition = new Vector2(0, 0);

            var slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = _progressBarColor;

            slider.fillRect = fillRect;
            slider.targetGraphic = bgImg;

            return slider;
        }

        private TextMeshProUGUI CreateTMProText(Transform parent, string name, string defaultText, int fontSize,
            FontStyles style, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700, 40);
            rect.anchoredPosition = anchoredPosition;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = _textColor;

            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 44);
            rect.anchoredPosition = anchoredPosition;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            var btn = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 16;
            tmp.color = Color.white;

            return btn;
        }

        private static (GameObject panel, TextMeshProUGUI errorText, Button closeButton) CreateErrorPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "ErrorPanel", new Color(0.6f, 0.1f, 0.1f, 0.95f));

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.3f);
            rect.anchorMax = new Vector2(0.85f, 0.7f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panel.SetActive(false);

            var errorText = new GameObject("ErrorText");
            errorText.transform.SetParent(panel.transform, false);
            var etRect = errorText.AddComponent<RectTransform>();
            etRect.anchorMin = new Vector2(0.05f, 0.35f);
            etRect.anchorMax = new Vector2(0.95f, 0.95f);
            etRect.offsetMin = Vector2.zero;
            etRect.offsetMax = Vector2.zero;
            var etTmp = errorText.AddComponent<TextMeshProUGUI>();
            etTmp.text = "Error occurred";
            etTmp.alignment = TextAlignmentOptions.Center;
            etTmp.color = Color.white;

            var closeBtn = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(0, -30));

            return (panel, etTmp, closeBtn);
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Editor Style Helpers
        // ─────────────────────────────────────────────────────────────────────────────

        private void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 8, 8)
                };
            }
        }

        private void DrawSection(string title, ref bool foldout, System.Action content)
        {
            EditorGUILayout.Space(4);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
            if (foldout)
            {
                EditorGUILayout.BeginVertical(_boxStyle ?? EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                content();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawHorizontalLine()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1f));
            EditorGUILayout.Space(2);
        }

        private static void SetSerializedField(SerializedObject so, string fieldName, Object value)
        {
            var prop = so.FindProperty(fieldName);
            if (prop != null)
                prop.objectReferenceValue = value;
        }
    }
}
