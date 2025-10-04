using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Setup component for EventCenter system - ensures proper initialization
    /// Use this instead of singleton pattern for better dependency management
    /// </summary>
    public class EventCenterSetup : MonoBehaviour
    {
        [Header("EventCenter Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool createEventPool = true;
        [SerializeField] private bool enableLogging = false;
        
        [Header("Performance Settings")]
        [SerializeField] private int maxEventsPerFrame = 10000;
        [SerializeField] private int maxBatchSize = 1000;
        [SerializeField] private int initialPoolSize = 100;
        [SerializeField] private int maxPoolSize = 1000;
        
        private EventCenter _eventCenter;
        private EventPool _eventPool;
        
        private void Awake()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Initialize the EventCenter system
        /// </summary>
        public void Initialize()
        {
            // Create EventCenter if not already present
            if (_eventCenter == null)
            {
                _eventCenter = GetComponent<EventCenter>();
                if (_eventCenter == null)
                {
                    _eventCenter = gameObject.AddComponent<EventCenter>();
                }
            }
            
            // Configure EventCenter
            ConfigureEventCenter();
            
            // Create EventPool if needed
            if (createEventPool && _eventPool == null)
            {
                var poolGO = new GameObject("EventPool");
                poolGO.transform.SetParent(transform);
                _eventPool = poolGO.AddComponent<EventPool>();
                
                ConfigureEventPool();
            }
            
            // Register with services
            EventCenterService.SetCurrent(_eventCenter);
            if (_eventPool != null)
            {
                EventPoolService.SetCurrent(_eventPool);
            }
            
            // Set persistence
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            Log("EventCenter system initialized successfully");
        }
        
        private void ConfigureEventCenter()
        {
            // Access serialized fields through reflection or make EventCenter configurable
            // For now, log the configuration
            Log($"EventCenter configured - MaxEvents: {maxEventsPerFrame}, BatchSize: {maxBatchSize}");
        }
        
        private void ConfigureEventPool()
        {
            // Configure EventPool settings
            Log($"EventPool configured - InitialSize: {initialPoolSize}, MaxSize: {maxPoolSize}");
        }
        
        private void OnDestroy()
        {
            // Clean up service references
            if (ReferenceEquals(EventCenterService.Current, _eventCenter))
            {
                EventCenterService.ClearCurrent();
            }
            
            if (ReferenceEquals(EventPoolService.Current, _eventPool))
            {
                EventPoolService.ClearCurrent();
            }
            
            Log("EventCenter system cleaned up");
        }
        
        private void Log(string message)
        {
            // Logging disabled
        }
        
        #region Public API
        
        /// <summary>
        /// Get the EventCenter instance
        /// </summary>
        public EventCenter GetEventCenter()
        {
            return _eventCenter;
        }
        
        /// <summary>
        /// Get the EventPool instance
        /// </summary>
        public EventPool GetEventPool()
        {
            return _eventPool;
        }
        
        /// <summary>
        /// Check if the system is properly initialized
        /// </summary>
        public bool IsInitialized()
        {
            return _eventCenter != null && ReferenceEquals(EventCenterService.Current, _eventCenter);
        }
        
        #endregion
    }
}