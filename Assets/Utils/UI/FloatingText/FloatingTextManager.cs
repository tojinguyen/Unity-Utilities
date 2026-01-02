using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace TirexGame.Utils.UI
{
    /// <summary>
    /// Manager for creating and pooling floating text instances.
    /// Supports both world space (3D) and screen space (2D/UI) floating text.
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
        [SerializeField] private GameObject floatingText3DPrefab;
        [SerializeField] private GameObject floatingText2DPrefab;

        [Header("Pooling Settings")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int maxPoolSize = 100;
        [SerializeField] private bool autoExpand = true;

        [Header("Canvas References")]
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private Camera mainCamera;

        private Queue<FloatingTextBase> pool3D = new Queue<FloatingTextBase>();
        private Queue<FloatingTextBase> pool2D = new Queue<FloatingTextBase>();
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
            // Create default prefabs if not assigned
            if (floatingText3DPrefab == null)
            {
                floatingText3DPrefab = CreateDefault3DPrefab();
            }

            if (floatingText2DPrefab == null)
            {
                floatingText2DPrefab = CreateDefault2DPrefab();
            }

            // Pre-populate pools
            for (int i = 0; i < initialPoolSize / 2; i++)
            {
                CreateNewPooledObject(true);
                CreateNewPooledObject(false);
            }
        }

        /// <summary>
        /// Create a default 3D floating text prefab
        /// </summary>
        private GameObject CreateDefault3DPrefab()
        {
            GameObject prefab = new GameObject("FloatingText3D");
            FloatingText3D floatingText = prefab.AddComponent<FloatingText3D>();

            TextMeshPro tmp = prefab.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.color = Color.white;

            prefab.SetActive(false);
            return prefab;
        }

        /// <summary>
        /// Create a default 2D/UI floating text prefab
        /// </summary>
        private GameObject CreateDefault2DPrefab()
        {
            if (uiCanvas == null)
            {
                CreateUICanvas();
            }

            GameObject prefab = new GameObject("FloatingText2D");
            RectTransform rectTransform = prefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);

            FloatingText2D floatingText = prefab.AddComponent<FloatingText2D>();

            TextMeshProUGUI tmp = prefab.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.color = Color.white;

            prefab.SetActive(false);
            return prefab;
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
        /// Create a new pooled object
        /// </summary>
        private FloatingTextBase CreateNewPooledObject(bool is3D)
        {
            GameObject prefab = is3D ? floatingText3DPrefab : floatingText2DPrefab;
            GameObject instance = Instantiate(prefab);
            instance.SetActive(false);

            if (!is3D && uiCanvas != null)
            {
                instance.transform.SetParent(uiCanvas.transform, false);
            }
            else
            {
                instance.transform.SetParent(transform);
            }

            FloatingTextBase floatingText = instance.GetComponent<FloatingTextBase>();
            if (floatingText == null)
            {
                if (is3D)
                {
                    floatingText = instance.AddComponent<FloatingText3D>();
                }
                else
                {
                    floatingText = instance.AddComponent<FloatingText2D>();
                }
            }

            if (is3D)
            {
                pool3D.Enqueue(floatingText);
            }
            else
            {
                pool2D.Enqueue(floatingText);
            }

            return floatingText;
        }

        /// <summary>
        /// Get a floating text from the pool
        /// </summary>
        private FloatingTextBase GetFromPool(bool is3D)
        {
            Queue<FloatingTextBase> pool = is3D ? pool3D : pool2D;

            if (pool.Count == 0)
            {
                if (autoExpand && (pool3D.Count + pool2D.Count + activeTexts.Count) < maxPoolSize)
                {
                    return CreateNewPooledObject(is3D);
                }
                else
                {
                    Debug.LogWarning("FloatingTextManager: Pool is empty and auto-expand is disabled or max size reached.");
                    return null;
                }
            }

            FloatingTextBase floatingText = pool.Dequeue();
            activeTexts.Add(floatingText);
            return floatingText;
        }

        /// <summary>
        /// Return a floating text to the pool
        /// </summary>
        private void ReturnToPool(FloatingTextBase floatingText)
        {
            activeTexts.Remove(floatingText);

            if (floatingText is FloatingText2D || (floatingText is FloatingText legacy && legacy.IsUIMode))
            {
                pool2D.Enqueue(floatingText);
            }
            else
            {
                pool3D.Enqueue(floatingText);
            }
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
        public void SetPrefabs(GameObject prefab3D, GameObject prefab2D)
        {
            floatingText3DPrefab = prefab3D;
            floatingText2DPrefab = prefab2D;
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
