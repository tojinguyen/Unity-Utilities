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
        
        /// <summary>
        /// Get the EventCenter configuration
        /// Loads from Resources folder or creates default if not found
        /// </summary>
        public static EventCenterConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EventCenterConfig>("EventCenterConfig");
                    if (_instance == null)
                    {
                        // Create default configuration
                        _instance = CreateInstance<EventCenterConfig>();
                        Debug.Log("[EventCenterConfig] Using default configuration. Create EventCenterConfig asset in Resources folder to customize.");
                    }
                }
                return _instance;
            }
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