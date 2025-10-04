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
                    ConsoleLogger.Log("[EventSystemInitializer] Static EventSystem auto-initialized");
                }
                else
                {
                    ConsoleLogger.LogWarning("[EventSystemInitializer] Failed to create or find EventCenter");
                }
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"[EventSystemInitializer] Failed to auto-initialize EventSystem: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Ensure an EventCenter exists in the application
        /// Creates one automatically if none is found
        /// </summary>
        private static void EnsureEventCenterExists()
        {
            var config = EventCenterConfig.Instance;
            
            // Check if auto-creation is disabled in config
            if (!config.autoCreateEventCenter)
            {
                ConsoleLogger.Log("[EventSystemInitializer] Auto-creation disabled in configuration");
                return;
            }
            
            // Check if EventCenter already exists
            var existingEventCenter = UnityEngine.Object.FindFirstObjectByType<EventCenter>();
            if (existingEventCenter != null)
            {
                ConsoleLogger.Log("[EventSystemInitializer] EventCenter already exists, using existing one");
                return;
            }
            
            // Check if EventCenterSetup exists (it will create EventCenter)
            var existingSetup = UnityEngine.Object.FindFirstObjectByType<EventCenterSetup>();
            if (existingSetup != null)
            {
                ConsoleLogger.Log("[EventSystemInitializer] EventCenterSetup found, it will handle EventCenter creation");
                return;
            }
            
            // Create EventCenter automatically using config settings
            CreateDefaultEventCenter();
        }
        
        /// <summary>
        /// Create a default EventCenter GameObject with settings from configuration
        /// </summary>
        private static void CreateDefaultEventCenter()
        {
            var config = EventCenterConfig.Instance;
            
            // Create with settings from config
            var eventCenter = EventCenterService.CreateAndSetCurrent(config.autoCreatedName, config.dontDestroyOnLoad);
            
            ConsoleLogger.Log($"[EventSystemInitializer] ✅ EventCenter created: '{config.autoCreatedName}'");
            ConsoleLogger.Log($"[EventSystemInitializer] ✅ DontDestroyOnLoad: {config.dontDestroyOnLoad}");
            ConsoleLogger.Log($"[EventSystemInitializer] ℹ️ Max Events/Frame: {config.maxEventsPerFrame}");
            ConsoleLogger.Log($"[EventSystemInitializer] ℹ️ Max Batch Size: {config.maxBatchSize}");
            
            if (config.enableLogging)
            {
                ConsoleLogger.Log("[EventSystemInitializer] ✅ ConsoleLogger logging enabled");
            }
            
            if (config.enableProfiling)
            {
                ConsoleLogger.Log("[EventSystemInitializer] ✅ Performance profiling enabled");
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupEventSystemCleanup()
        {
            // Ensure cleanup when application quits
            Application.quitting += () =>
            {
                EventSystem.Shutdown();
                ConsoleLogger.Log("[EventSystemInitializer] EventSystem shutdown on application quit");
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
                ConsoleLogger.Log("[EventSystemManager] EventSystem initialized in Awake");
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
                ConsoleLogger.Log("[EventSystemManager] EventSystem shutdown in OnDestroy");
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