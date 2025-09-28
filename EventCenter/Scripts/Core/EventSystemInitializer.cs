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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeEventSystem()
        {
            try
            {
                // Ensure EventCenter exists, create one if needed
                EnsureEventCenterExists();
                
                // Initialize EventSystem if EventCenter is available
                if (EventCenterService.IsAvailable)
                {
                    EventSystem.Initialize();
                    Debug.Log("[EventSystemInitializer] Static EventSystem auto-initialized");
                }
                else
                {
                    Debug.LogWarning("[EventSystemInitializer] Failed to create or find EventCenter");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventSystemInitializer] Failed to auto-initialize EventSystem: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensure an EventCenter exists in the application
        /// Creates one automatically if none is found
        /// </summary>
        private static void EnsureEventCenterExists()
        {
            // Check if EventCenter already exists
            var existingEventCenter = UnityEngine.Object.FindFirstObjectByType<EventCenter>();
            if (existingEventCenter != null)
            {
                Debug.Log("[EventSystemInitializer] EventCenter already exists, using existing one");
                return;
            }
            
            // Check if EventCenterSetup exists (it will create EventCenter)
            var existingSetup = UnityEngine.Object.FindFirstObjectByType<EventCenterSetup>();
            if (existingSetup != null)
            {
                Debug.Log("[EventSystemInitializer] EventCenterSetup found, it will handle EventCenter creation");
                return;
            }
            
            // Create EventCenter automatically
            CreateDefaultEventCenter();
        }
        
        /// <summary>
        /// Create a default EventCenter GameObject with optimal settings
        /// </summary>
        private static void CreateDefaultEventCenter()
        {
            // Create with default settings
            var eventCenter = EventCenterService.CreateAndSetCurrent("[EventCenter] - Auto Created", true);
            
            Debug.Log("[EventSystemInitializer] ✅ EventCenter created automatically and set to DontDestroyOnLoad");
            Debug.Log("[EventSystemInitializer] ℹ️ EventCenter will persist across all scenes");
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