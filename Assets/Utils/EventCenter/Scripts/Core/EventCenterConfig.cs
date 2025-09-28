using UnityEngine;

namespace TirexGame.Utils.EventCenter
{
    /// <summary>
    /// Configuration asset for EventCenter auto-initialization
    /// Create this asset in Resources folder to customize EventCenter behavior
    /// </summary>
    [CreateAssetMenu(fileName = "EventCenterConfig", menuName = "TirexGame/Event Center/Config", order = 100)]
    public class EventCenterConfig : ScriptableObject
    {
        [Header("Auto Creation Settings")]
        [Tooltip("Automatically create EventCenter if none exists in scene")]
        public bool autoCreateEventCenter = true;
        
        [Tooltip("Make auto-created EventCenter persistent across scenes")]
        public bool dontDestroyOnLoad = true;
        
        [Tooltip("Name for auto-created EventCenter GameObject")]
        public string autoCreatedName = "[EventCenter] - Auto Created";
        
        [Header("Performance Settings")]
        [Tooltip("Maximum events processed per frame")]
        public int maxEventsPerFrame = 10000;
        
        [Tooltip("Maximum batch size for event processing")]
        public int maxBatchSize = 1000;
        
        [Header("Debug Settings")]
        [Tooltip("Enable debug logging for EventCenter")]
        public bool enableLogging = false;
        
        [Tooltip("Enable performance profiling")]
        public bool enableProfiling = false;
        
        [Tooltip("Show runtime statistics")]
        public bool showStats = false;
        
        #region Static Access
        
        private static EventCenterConfig _instance;
        private static EventCenterConfig _defaultInstance;
        
        // Paths for different config locations
        private const string CUSTOM_CONFIG_PATH = "EventCenterConfig";
        private const string DEFAULT_CONFIG_PATH = "EventCenter/DefaultEventCenterConfig";
        
        /// <summary>
        /// Get the EventCenter configuration
        /// Priority: Custom config in Resources root -> Default config in package -> Runtime default
        /// </summary>
        public static EventCenterConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadConfiguration();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Get the default configuration from package
        /// </summary>
        public static EventCenterConfig DefaultConfig
        {
            get
            {
                if (_defaultInstance == null)
                {
                    _defaultInstance = Resources.Load<EventCenterConfig>(DEFAULT_CONFIG_PATH);
                    if (_defaultInstance == null)
                    {
                        Debug.LogWarning("[EventCenterConfig] Default config not found in package, creating runtime default");
                        _defaultInstance = CreateRuntimeDefault();
                    }
                }
                return _defaultInstance;
            }
        }
        
        /// <summary>
        /// Load configuration with proper hierarchy
        /// </summary>
        private static void LoadConfiguration()
        {
            // 1. Try to load custom config from project Resources folder
            _instance = Resources.Load<EventCenterConfig>(CUSTOM_CONFIG_PATH);
            if (_instance != null)
            {
                Debug.Log("[EventCenterConfig] Using custom configuration from project Resources folder");
                return;
            }
            
            // 2. Try to load default config from package
            _instance = Resources.Load<EventCenterConfig>(DEFAULT_CONFIG_PATH);
            if (_instance != null)
            {
                Debug.Log("[EventCenterConfig] Using default configuration from package");
                return;
            }
            
            // 3. Create runtime default as fallback
            Debug.LogWarning("[EventCenterConfig] No config found, using runtime default. Consider creating a custom config.");
            _instance = CreateRuntimeDefault();
        }
        
        /// <summary>
        /// Create a runtime default configuration
        /// </summary>
        private static EventCenterConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<EventCenterConfig>();
            config.name = "Runtime Default EventCenter Config";
            
            // Set optimal default values
            config.autoCreateEventCenter = true;
            config.dontDestroyOnLoad = true;
            config.autoCreatedName = "[EventCenter] - Auto Created";
            config.maxEventsPerFrame = 10000;
            config.maxBatchSize = 1000;
            config.enableLogging = false;
            config.enableProfiling = false;
            config.showStats = false;
            
            return config;
        }
        
        /// <summary>
        /// Reset the configuration cache (useful for editor tools)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RefreshConfiguration()
        {
            _instance = null;
            _defaultInstance = null;
            Debug.Log("[EventCenterConfig] Configuration cache refreshed");
        }
        
        #endregion
        
        #region Validation
        
        private void OnValidate()
        {
            // Ensure reasonable values
            maxEventsPerFrame = Mathf.Max(100, maxEventsPerFrame);
            maxBatchSize = Mathf.Max(10, maxBatchSize);
            
            if (string.IsNullOrEmpty(autoCreatedName))
            {
                autoCreatedName = "[EventCenter] - Auto Created";
            }
        }
        
        #endregion
    }
}