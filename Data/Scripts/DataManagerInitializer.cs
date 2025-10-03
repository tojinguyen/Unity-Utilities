using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Unity component helper to automatically initialize DataManager.
    /// Add this component to a GameObject in your scene to auto-initialize DataManager.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class DataManagerInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int defaultCacheExpirationMinutes = 30;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f;
        
        [Header("Auto Initialize")]
        [SerializeField] private bool initializeOnAwake = true;
        
        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeDataManager();
            }
        }
        
        [ContextMenu("Initialize DataManager")]
        public void InitializeDataManager()
        {
            var config = new DataManagerConfig
            {
                EnableLogging = enableLogging,
                EnableCaching = enableCaching,
                DefaultCacheExpirationMinutes = defaultCacheExpirationMinutes,
                EnableAutoSave = enableAutoSave,
                AutoSaveIntervalSeconds = autoSaveIntervalSeconds
            };
            
            DataManager.Initialize(config);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && enableAutoSave)
            {
                // Save all data when app is paused
                DataManager.SaveAllAsync().Forget();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && enableAutoSave)
            {
                // Save all data when app loses focus
                DataManager.SaveAllAsync().Forget();
            }
        }
        
        private void OnDestroy()
        {
            // Optional: Shutdown DataManager when this component is destroyed
            // DataManager.Shutdown();
        }
        
        /// <summary>
        /// Automatically initialize DataManager at runtime startup
        /// This ensures DataManager is always initialized even without GameObject
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitializeDataManager()
        {
            var config = new DataManagerConfig
            {
                EnableLogging = true,
                EnableCaching = true,
                DefaultCacheExpirationMinutes = 30,
                EnableAutoSave = true,
                AutoSaveIntervalSeconds = 300f
            };
            
            DataManager.Initialize(config);
            Debug.Log("[DataManager] Auto-initialized at runtime startup");
        }
    }
}