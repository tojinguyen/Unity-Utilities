using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Tirex.Utils.ObjectPooling;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Manager for creating and pooling floating text instances.
    /// Supports both world space (3D) and screen space (2D/UI) floating text.
    /// Uses Tirex ObjectPooling system for efficient object management.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        private static FloatingTextManager instance;
        public static FloatingTextManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("FloatingTextManager");
                    instance = go.AddComponent<FloatingTextManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Prefabs")]
        [SerializeField] private FloatingText3D floatingText3DPrefab;
        [SerializeField] private FloatingText2D floatingText2DPrefab;

        [Header("Pooling Settings")]
        [SerializeField] private int initialPoolSize = 20;

        [Header("Canvas References")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private Camera mainCamera;

        private List<FloatingTextBase> activeTexts = new List<FloatingTextBase>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
        }

        /// <summary>
        /// Initialize object pools
        /// </summary>
        private void InitializePool()
        {
            // Prewarm pools using ObjectPooling system
            if (floatingText3DPrefab != null)
            {
                ObjectPooling.Prewarm(floatingText3DPrefab, initialPoolSize);
            }

            if (floatingText2DPrefab != null)
            {
                ObjectPooling.Prewarm(floatingText2DPrefab, initialPoolSize);
            }
        }

        /// <summary>
        /// Create a default 3D floating text prefab
        /// </summary>
        private FloatingText3D CreateDefault3DPrefab()
        {
            GameObject prefab = new GameObject("FloatingText3D_Default");
            FloatingText3D floatingText = prefab.AddComponent<FloatingText3D>();

            TextMeshPro tmp = prefab.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.color = Color.white;

            prefab.SetActive(false);
            return floatingText;
        }

        /// <summary>
        /// Create a default 2D/UI floating text prefab
        /// </summary>
        private FloatingText2D CreateDefault2DPrefab()
        {
            if (uiCanvas == null)
            {
                CreateUICanvas();
            }

            GameObject prefab = new GameObject("FloatingText2D_Default");
            RectTransform rectTransform = prefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);

            FloatingText2D floatingText = prefab.AddComponent<FloatingText2D>();

            TextMeshProUGUI tmp = prefab.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.color = Color.white;

            prefab.SetActive(false);
            return floatingText;
        }

        /// <summary>
        /// Create UI canvas if it doesn't exist
        /// </summary>
        private void CreateUICanvas()
        {
            GameObject canvasObj = new GameObject("FloatingTextCanvas");
            canvasObj.transform.SetParent(transform);

            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 100;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        /// <summary>
        /// Get a floating text from the pool using ObjectPooling system
        /// </summary>
        private FloatingTextBase GetFromPool(bool is3D)
        {
            FloatingTextBase instance;

            if (is3D)
            {
                if (floatingText3DPrefab == null)
                {
                    Debug.LogWarning("FloatingTextManager: 3D prefab is not assigned. Creating default prefab.");
                    floatingText3DPrefab = CreateDefault3DPrefab();
                    ObjectPooling.Prewarm(floatingText3DPrefab, initialPoolSize);
                }

                instance = ObjectPooling.GetObject(floatingText3DPrefab);
            }
            else
            {
                if (floatingText2DPrefab == null)
                {
                    Debug.LogWarning("FloatingTextManager: 2D prefab is not assigned. Creating default prefab.");
                    floatingText2DPrefab = CreateDefault2DPrefab();
                    ObjectPooling.Prewarm(floatingText2DPrefab, initialPoolSize);
                }

                instance = ObjectPooling.GetObject(floatingText2DPrefab, uiCanvas != null ? uiCanvas.transform : null);
            }

            if (instance != null)
            {
                activeTexts.Add(instance);
            }

            return instance;
        }

        /// <summary>
        /// Return a floating text to the pool using ObjectPooling system
        /// </summary>
        private void ReturnToPool(FloatingTextBase floatingText)
        {
            if (floatingText == null) return;

            activeTexts.Remove(floatingText);
            ObjectPooling.ReturnObject(floatingText);
        }

        /// <summary>
        /// Show floating text in world space (3D)
        /// </summary>
        public FloatingTextBase ShowText3D(string text, Vector3 worldPosition, FloatingTextData data = null)
        {
            FloatingTextBase floatingText = GetFromPool(true);
            if (floatingText == null) return null;

            floatingText.Show(text, worldPosition, data, ReturnToPool);
            return floatingText;
        }

        /// <summary>
        /// Show floating text in screen space (2D/UI)
        /// </summary>
        public FloatingTextBase ShowText2D(string text, Vector3 screenPosition, FloatingTextData data = null)
        {
            FloatingTextBase floatingText = GetFromPool(false);
            if (floatingText == null) return null;

            floatingText.Show(text, screenPosition, data, ReturnToPool);
            return floatingText;
        }

        /// <summary>
        /// Show floating text at a world position but display it in screen space
        /// </summary>
        public FloatingTextBase ShowTextAtWorldPosition(string text, Vector3 worldPosition, FloatingTextData data = null)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                Debug.LogError("FloatingTextManager: No camera found!");
                return null;
            }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            return ShowText2D(text, screenPosition, data);
        }

        /// <summary>
        /// Show damage text (convenience method)
        /// </summary>
        public FloatingTextBase ShowDamage(float damage, Vector3 position, bool is3D = false)
        {
            string text = damage.ToString("0");
            FloatingTextData data = FloatingTextData.CreateDamageDefault();

            if (is3D)
            {
                return ShowText3D(text, position, data);
            }
            else
            {
                return ShowTextAtWorldPosition(text, position, data);
            }
        }

        /// <summary>
        /// Show healing text (convenience method)
        /// </summary>
        public FloatingTextBase ShowHealing(float amount, Vector3 position, bool is3D = false)
        {
            string text = "+" + amount.ToString("0");
            FloatingTextData data = FloatingTextData.CreateHealingDefault();

            if (is3D)
            {
                return ShowText3D(text, position, data);
            }
            else
            {
                return ShowTextAtWorldPosition(text, position, data);
            }
        }

        /// <summary>
        /// Show critical hit text (convenience method)
        /// </summary>
        public FloatingTextBase ShowCritical(float damage, Vector3 position, bool is3D = false)
        {
            string text = damage.ToString("0") + "!";
            FloatingTextData data = FloatingTextData.CreateCriticalDefault();

            if (is3D)
            {
                return ShowText3D(text, position, data);
            }
            else
            {
                return ShowTextAtWorldPosition(text, position, data);
            }
        }

        /// <summary>
        /// Set the UI canvas reference
        /// </summary>
        public void SetCanvas(Canvas canvas)
        {
            uiCanvas = canvas;
        }

        /// <summary>
        /// Set the main camera reference
        /// </summary>
        public void SetCamera(Camera camera)
        {
            mainCamera = camera;
        }

        /// <summary>
        /// Set custom prefabs
        /// </summary>
        public void SetPrefabs(FloatingText3D prefab3D, FloatingText2D prefab2D)
        {
            floatingText3DPrefab = prefab3D;
            floatingText2DPrefab = prefab2D;

            // Re-initialize pools with new prefabs
            InitializePool();
        }

        /// <summary>
        /// Clear all active floating texts
        /// </summary>
        public void ClearAll()
        {
            foreach (var text in activeTexts.ToArray())
            {
                text.gameObject.SetActive(false);
                ReturnToPool(text);
            }
            activeTexts.Clear();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
