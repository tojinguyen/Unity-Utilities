using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Auto-initializer for the static EventSystem
    /// This ensures the EventSystem is initialized when Unity starts
    /// </summary>
    [System.Serializable]
    public class EventSystemInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeEventSystem()
        {
            try
            {
                EventSystem.Initialize();
                Debug.Log("[EventSystemInitializer] Static EventSystem auto-initialized");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventSystemInitializer] Failed to auto-initialize EventSystem: {ex.Message}");
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupEventSystemCleanup()
        {
            // Ensure cleanup when application quits
            Application.quitting += () =>
            {
                EventSystem.Shutdown();
                Debug.Log("[EventSystemInitializer] EventSystem shutdown on application quit");
            };
        }
    }
    
    /// <summary>
    /// MonoBehaviour helper for EventSystem management
    /// Add this to a GameObject if you need manual control over EventSystem lifecycle
    /// </summary>
    public class EventSystemManager : MonoBehaviour
    {
        [Header("EventSystem Settings")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool shutdownOnDestroy = true;
        [SerializeField] private bool logStatus = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (initializeOnAwake && !EventSystem.IsInitialized)
            {
                EventSystem.Initialize();
                Debug.Log("[EventSystemManager] EventSystem initialized in Awake");
            }
        }
        
        private void Start()
        {
            if (logStatus)
            {
                EventSystem.LogStatus();
            }
        }
        
        private void OnDestroy()
        {
            if (shutdownOnDestroy)
            {
                EventSystem.Shutdown();
                Debug.Log("[EventSystemManager] EventSystem shutdown in OnDestroy");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        [ContextMenu("Initialize EventSystem")]
        public void InitializeEventSystem()
        {
            EventSystem.Initialize();
        }
        
        [ContextMenu("Shutdown EventSystem")]
        public void ShutdownEventSystem()
        {
            EventSystem.Shutdown();
        }
        
        [ContextMenu("Clear All Events")]
        public void ClearAllEvents()
        {
            EventSystem.Clear();
        }
        
        [ContextMenu("Log EventSystem Status")]
        public void LogEventSystemStatus()
        {
            EventSystem.LogStatus();
        }
        
        #endregion
    }
}