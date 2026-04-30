using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils.Scripts.UIManager.UINavigator;
using AnimTrans = Utils.Scripts.UIManager.AnimationTransition.AnimationTransition;
using ModalView = Utils.Scripts.UIManager.UINavigator.Popup.Modal.Modal;
using AlertView = Utils.Scripts.UIManager.UINavigator.Alert.Alert;
using ToastView = Utils.Scripts.UIManager.UINavigator.Toast.Toast;
using TooltipView = Utils.Scripts.UIManager.UINavigator.Tooltip.Tooltip;
using TabView = Utils.Scripts.UIManager.UINavigator.Tab.Tab;

namespace Utils.Scripts.UIManager.UINavigator.Editor
{
    public static class UINavigatorMenuItems
    {
        private const string AnimationTransitionPath = "Assets/Utils/UI/UINavigator/Config/AnimationTransition";

        // ── Canvas creation ──────────────────────────────────────────────

        [MenuItem("BombGuy/Utils/UINavigator/Canvas", false, 0)]
        public static void CreateCanvas()
        {
            GameObject canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create UINavigator Canvas");

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            Selection.activeGameObject = canvasGo;
        }

        // ── View objects (each gets its own new Canvas) ───────────────────

        [MenuItem("BombGuy/Utils/UINavigator/ScreenView", false, 20)]
        public static void CreateScreenView()
        {
            Canvas canvas = CreateFreshCanvas("ScreenCanvas");
            GameObject go = CreateUIObject("ScreenView", canvas.transform);
            ScreenView sv = Undo.AddComponent<ScreenView>(go);
            AutoAssignScreenViewAnimations(sv);
            Selection.activeGameObject = go;
        }

        [MenuItem("BombGuy/Utils/UINavigator/Modal", false, 21)]
        public static void CreateModal()
        {
            Canvas canvas = CreateFreshCanvas("ModalCanvas");
            GameObject go = CreateUIObject("Modal", canvas.transform);
            ModalView modal = Undo.AddComponent<ModalView>(go);
            AutoAssignEViewAnimations(modal);
            Selection.activeGameObject = go;
        }

        [MenuItem("BombGuy/Utils/UINavigator/Alert", false, 22)]
        public static void CreateAlert()
        {
            Canvas canvas = CreateFreshCanvas("AlertCanvas");
            GameObject go = CreateUIObject("Alert", canvas.transform);
            AlertView alert = Undo.AddComponent<AlertView>(go);
            AutoAssignEViewAnimations(alert);
            Selection.activeGameObject = go;
        }

        [MenuItem("BombGuy/Utils/UINavigator/Toast", false, 23)]
        public static void CreateToast()
        {
            Canvas canvas = CreateFreshCanvas("ToastCanvas");
            GameObject go = CreateUIObject("Toast", canvas.transform);
            ToastView toast = Undo.AddComponent<ToastView>(go);
            AutoAssignEViewAnimations(toast, toastMode: true);
            Selection.activeGameObject = go;
        }

        [MenuItem("BombGuy/Utils/UINavigator/Tooltip", false, 24)]
        public static void CreateTooltip()
        {
            Canvas canvas = CreateFreshCanvas("TooltipCanvas");
            GameObject go = CreateUIObject("Tooltip", canvas.transform);
            TooltipView tooltip = Undo.AddComponent<TooltipView>(go);
            AutoAssignEViewAnimations(tooltip);
            Selection.activeGameObject = go;
        }

        [MenuItem("BombGuy/Utils/UINavigator/Tab", false, 25)]
        public static void CreateTab()
        {
            Canvas canvas = CreateFreshCanvas("TabCanvas");
            GameObject go = CreateUIObject("Tab", canvas.transform);
            TabView tab = Undo.AddComponent<TabView>(go);
            AutoAssignEViewAnimations(tab);
            Selection.activeGameObject = go;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static Canvas CreateFreshCanvas(string name)
        {
            GameObject canvasGo = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(canvasGo, $"Create {name}");

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            SetupFullStretch(go.GetComponent<RectTransform>());
            return go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        private static void SetupFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        // ── Animation auto-assign ─────────────────────────────────────────

        internal static void AutoAssignScreenViewAnimations(ScreenView view)
        {
            if (view == null) return;
            SerializedObject so = new SerializedObject(view);
            SerializedProperty container = so.FindProperty("animationTransition");
            if (container == null) return;

            AssignAsset(container, "PushEnterAnimation", "PushEnter");
            AssignAsset(container, "PushExitAnimation", "PushExit");
            AssignAsset(container, "PopEnterAnimation", "PopEnter");
            AssignAsset(container, "PopExitAnimation", "PopExit");

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void AutoAssignEViewAnimations(EView view, bool toastMode = false)
        {
            if (view == null) return;
            SerializedObject so = new SerializedObject(view);
            SerializedProperty container = so.FindProperty("animationTransition");
            if (container == null) return;

            if (toastMode)
            {
                AssignAsset(container, "ShowAnimation", "ToastShow");
                AssignAsset(container, "HideAnimation", "ToastHide");
            }
            else
            {
                AssignAsset(container, "ShowAnimation", "PushEnter");
                AssignAsset(container, "HideAnimation", "PushExit");
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignAsset(SerializedProperty container, string propertyName, string assetFileName)
        {
            SerializedProperty prop = container.FindPropertyRelative(propertyName);
            if (prop == null || prop.objectReferenceValue != null) return;

            string[] guids = AssetDatabase.FindAssets($"{assetFileName} t:AnimationTransition", new[] { AnimationTransitionPath });
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AnimTrans asset = AssetDatabase.LoadAssetAtPath<AnimTrans>(path);
            if (asset != null)
                prop.objectReferenceValue = asset;
        }
    }
}
