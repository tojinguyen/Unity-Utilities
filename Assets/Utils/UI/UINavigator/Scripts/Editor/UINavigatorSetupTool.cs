using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils.Scripts.UIManager.UINavigator;
using Utils.Scripts.UIManager.UINavigator.Popup.Modal;
using Utils.Scripts.UIManager.UINavigator.Alert;
using Utils.Scripts.UIManager.UINavigator.Toast;
using Utils.Scripts.UIManager.UINavigator.Tooltip;

namespace Utils.Scripts.UIManager.UINavigator.Editor
{
    public class UINavigatorSetupTool
    {
        [MenuItem("BombGuy/Utils/UINavigator/Setup UI Hierarchy")]
        public static void SetupUIHierarchy()
        {
            // 1. Always create a new Canvas
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");

            // 2. Ensure EventSystem exists
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            // 3. Create LayerContainer
            GameObject layerContainerGo = new GameObject("LayerContainer", typeof(RectTransform));
            layerContainerGo.transform.SetParent(canvas.transform, false);
            SetupFullStretch(layerContainerGo.GetComponent<RectTransform>());
            Undo.RegisterCreatedObjectUndo(layerContainerGo, "Create LayerContainer");

            // 4. Create Sub Containers
            CreateContainer<ScreenContainer>(layerContainerGo.transform, "ScreenContainer");

            var modalContainer = CreateContainer<ModalContainer>(layerContainerGo.transform, "ModalViewContainer");
            AssignBlockOverlay<ModalContainer>(modalContainer, "modalBlockOverlay", "Assets/Utils/UI/UINavigator/Prefabs/ModalBlockOverlay.prefab");

            var alertContainer = CreateContainer<AlertContainer>(layerContainerGo.transform, "AlertViewContainer");
            AssignBlockOverlay<AlertContainer>(alertContainer, "modalBlockOverlayOverride", "Assets/Utils/UI/UINavigator/Prefabs/AlertBlockOverlay.prefab");

            CreateContainer<OverloadToastContainer>(layerContainerGo.transform, "ToastViewContainer");
            CreateContainer<TooltipContainer>(layerContainerGo.transform, "TooltipViewContainer");

            Selection.activeGameObject = layerContainerGo;
            Debug.Log("<b>[UINavigator]</b> UI Hierarchy setup completed successfully!");
        }

        private static GameObject CreateContainer<T>(Transform parent, string name) where T : Component
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(T));
            go.transform.SetParent(parent, false);
            SetupFullStretch(go.GetComponent<RectTransform>());
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        private static void AssignBlockOverlay<T>(GameObject container, string fieldName, string prefabPath) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"<b>[UINavigator]</b> BlockOverlay prefab not found at: {prefabPath}");
                return;
            }
            var comp = container.GetComponent<T>();
            if (comp == null) return;
            var so = new SerializedObject(comp);
            var prop = so.FindProperty(fieldName);
            if (prop == null) return;
            prop.objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
