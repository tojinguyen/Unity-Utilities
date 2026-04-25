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
        [MenuItem("Tools/UINavigator/Setup UI Hierarchy")]
        public static void SetupUIHierarchy()
        {
            // 1. Find or create Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            }

            // Setup Canvas Scaler for Mobile
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920); // Default Portrait for mobile
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

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
            CreateContainer<ModalContainer>(layerContainerGo.transform, "ModalViewContainer");
            CreateContainer<AlertContainer>(layerContainerGo.transform, "AlertViewContainer");
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

        private static void SetupFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
