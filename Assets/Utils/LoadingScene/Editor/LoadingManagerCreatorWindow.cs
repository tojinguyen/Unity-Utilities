using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TirexGame.Utils.LoadingScene.Editor
{
    public class LoadingManagerCreatorWindow : EditorWindow
    {
        // ---- Prefab Settings ----
        private string _prefabName = "LoadSceneManager";
        private string _savePath = "Assets/Prefabs";
        private int _canvasSortingOrder = 999;

        // ---- UI Options ----
        private bool _addProgressBar = true;
        private bool _addPercentText = true;
        private float _completionDelay = 0.8f;

        // ---- Animation ----
        private AnimationPreset _animPreset = AnimationPreset.Fade;

        // ---- Style ----
        private Color _panelColor = new Color(0f, 0f, 0f, 0.92f);
        private Color _barColor = new Color(0.2f, 0.7f, 1f, 1f);
        private Color _textColor = Color.white;

        // ---- Foldouts ----
        private bool _foldPrefab = true;
        private bool _foldUI = true;
        private bool _foldAnim = true;
        private bool _foldStyle = true;

        private Vector2 _scroll;
        private GUIStyle _headerStyle;

        private enum AnimationPreset { None, Fade, Slide, Scale, Spinner }

        [MenuItem("BombGuy/Utils/LoadingScene/Creator Window")]
        public static void Open()
        {
            var w = GetWindow<LoadingManagerCreatorWindow>("Loading Scene Creator");
            w.minSize = new Vector2(360, 480);
            w.maxSize = new Vector2(460, 680);
        }

        private void OnGUI()
        {
            InitStyles();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawHeader();

            DrawSection("Prefab Settings", ref _foldPrefab, DrawPrefabSettings);
            DrawSection("UI Options", ref _foldUI, DrawUIOptions);
            DrawSection("Animation Preset", ref _foldAnim, DrawAnimOptions);
            DrawSection("Style", ref _foldStyle, DrawStyleOptions);

            EditorGUILayout.Space(10);
            DrawButtons();

            EditorGUILayout.EndScrollView();
        }

        // ─────────────────────────────────────────────────────────
        // Draw Sections
        // ─────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Loading Scene Creator", _headerStyle);
            EditorGUILayout.LabelField("Tạo nhanh LoadSceneManager Prefab", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(4);
            DrawLine();
        }

        private void DrawPrefabSettings()
        {
            _prefabName = EditorGUILayout.TextField("Prefab Name", _prefabName);
            EditorGUILayout.BeginHorizontal();
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    _savePath = "Assets" + path.Substring(Application.dataPath.Length);
            }
            EditorGUILayout.EndHorizontal();
            _canvasSortingOrder = EditorGUILayout.IntField("Canvas Sorting Order", _canvasSortingOrder);
            _completionDelay = EditorGUILayout.FloatField("Completion Delay (s)", _completionDelay);
        }

        private void DrawUIOptions()
        {
            _addProgressBar = EditorGUILayout.Toggle("Progress Bar", _addProgressBar);
            _addPercentText = EditorGUILayout.Toggle("Progress % Text", _addPercentText);
        }

        private void DrawAnimOptions()
        {
            _animPreset = (AnimationPreset)EditorGUILayout.EnumPopup("Preset", _animPreset);
            EditorGUILayout.Space(2);
            string hint = _animPreset switch
            {
                AnimationPreset.None    => "Instant show/hide, không animation.",
                AnimationPreset.Fade    => "FadeAnimationStrategy — fade in/out.",
                AnimationPreset.Slide   => "SlideAnimationStrategy — slide từ dưới lên.",
                AnimationPreset.Scale   => "ScaleAnimationStrategy — zoom in/out.",
                AnimationPreset.Spinner => "SpinnerAnimationStrategy — xoay spinner.",
                _                       => ""
            };
            EditorGUILayout.HelpBox(hint, MessageType.Info);
        }

        private void DrawStyleOptions()
        {
            _panelColor = EditorGUILayout.ColorField("Panel Background", _panelColor);
            _barColor   = EditorGUILayout.ColorField("Progress Bar Color", _barColor);
            _textColor  = EditorGUILayout.ColorField("Text Color", _textColor);
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Add to Scene", GUILayout.Height(36)))
                AddToScene();

            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.3f);
            if (GUILayout.Button("Save as Prefab", GUILayout.Height(36)))
                SaveAsPrefab();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // ─────────────────────────────────────────────────────────
        // Creation
        // ─────────────────────────────────────────────────────────

        private void AddToScene()
        {
            var root = BuildHierarchy();
            Undo.RegisterCreatedObjectUndo(root, "Create LoadSceneManager");
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private void SaveAsPrefab()
        {
            if (string.IsNullOrWhiteSpace(_prefabName))
            {
                EditorUtility.DisplayDialog("Error", "Prefab name cannot be empty.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(_savePath))
            {
                System.IO.Directory.CreateDirectory(_savePath);
                AssetDatabase.Refresh();
            }

            var root = BuildHierarchy();
            string path = $"{_savePath}/{_prefabName}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);

            if (prefab != null)
            {
                AssetDatabase.Refresh();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log($"[LoadingScene] Prefab saved: {path}");
            }
        }

        private GameObject BuildHierarchy()
        {
            // Root — LoadSceneManager
            var root = new GameObject(_prefabName);
            var manager = root.AddComponent<LoadSceneManager>();

            // Canvas — Loading UI
            var canvasGo = new GameObject("LoadingUI");
            canvasGo.transform.SetParent(root.transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = _canvasSortingOrder;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var controller = canvasGo.AddComponent<DefaultLoadingUIController>();

            // Loading Panel
            var panel = MakeStretchedPanel("LoadingPanel", canvasGo.transform, _panelColor);

            Slider slider = null;
            if (_addProgressBar)
                slider = MakeProgressBar(panel.transform);

            TextMeshProUGUI percentTMP = null;
            if (_addPercentText)
                percentTMP = MakeText("ProgressPercentText", panel.transform, "0%", 56, new Vector2(0, 80));

            // Attach animation strategy
            if (_animPreset != AnimationPreset.None)
            {
                MonoBehaviour strat = _animPreset switch
                {
                    AnimationPreset.Fade    => canvasGo.AddComponent<FadeAnimationStrategy>(),
                    AnimationPreset.Slide   => canvasGo.AddComponent<SlideAnimationStrategy>(),
                    AnimationPreset.Scale   => canvasGo.AddComponent<ScaleAnimationStrategy>(),
                    AnimationPreset.Spinner => canvasGo.AddComponent<SpinnerAnimationStrategy>(),
                    _                       => null
                };

                if (strat != null)
                {
                    var soCtrl = new SerializedObject(controller);
                    soCtrl.FindProperty("_animationStrategyObject").objectReferenceValue = strat;
                    soCtrl.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Wire DefaultLoadingUIController fields
            var soController = new SerializedObject(controller);
            soController.FindProperty("_loadingPanel").objectReferenceValue = panel;
            SetProp(soController, "_progressBar", slider);
            SetProp(soController, "_progressPercentText", percentTMP);
            soController.ApplyModifiedPropertiesWithoutUndo();

            // Wire LoadSceneManager fields
            var soManager = new SerializedObject(manager);
            soManager.FindProperty("_ui").objectReferenceValue = controller;
            soManager.FindProperty("_completionDelay").floatValue = _completionDelay;
            soManager.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        // ─────────────────────────────────────────────────────────
        // UI Helpers
        // ─────────────────────────────────────────────────────────

        private static GameObject MakeStretchedPanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            Stretch(go.GetComponent<RectTransform>());
            return go;
        }

        private Slider MakeProgressBar(Transform parent)
        {
            var go = new GameObject("ProgressBar");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 24);
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);
            Stretch(bg.GetComponent<RectTransform>());

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0f, 0.25f);
            faRect.anchorMax = new Vector2(1f, 0.75f);
            faRect.sizeDelta = new Vector2(-10f, 0f);
            faRect.anchoredPosition = new Vector2(-5f, 0f);

            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            fill.AddComponent<Image>().color = _barColor;
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.sizeDelta = new Vector2(10f, 0f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.direction = Slider.Direction.LeftToRight;
            slider.interactable = false;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0;

            return slider;
        }

        private TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 80);
            rect.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = _textColor;
            return tmp;
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
        }

        private static void SetProp(SerializedObject so, string field, Object val)
        {
            var p = so.FindProperty(field);
            if (p != null) p.objectReferenceValue = val;
        }

        // ─────────────────────────────────────────────────────────
        // Style Helpers
        // ─────────────────────────────────────────────────────────

        private void InitStyles()
        {
            _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void DrawSection(string title, ref bool foldout, System.Action draw)
        {
            EditorGUILayout.Space(4);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, title);
            if (foldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                draw();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawLine()
        {
            var r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f));
            EditorGUILayout.Space(2);
        }
    }
}
