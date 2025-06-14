using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utilities.Firebase
{
    /// <summary>
    /// Main Firebase Manager that handles initialization and access to Firebase services.
    /// Use FIREBASE_TOOL scripting define to enable/disable Firebase functionality.
    /// </summary>
    public class FirebaseManager : MonoSingleton<FirebaseManager>
    {
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableRemoteConfig = true;

        // Reference to ensure the singletons are instantiated
        private FirebaseAnalyticsManager _analyticsManager;
        private FirebaseRemoteConfigManager _remoteConfigManager;

        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        protected override void Initialize()
        {
            base.Initialize();
            
            // Force instantiation of the singleton instances
            _analyticsManager = FirebaseAnalyticsManager.Instance;
            _remoteConfigManager = FirebaseRemoteConfigManager.Instance;
            
            if (initializeOnAwake)
            {
                InitializeFirebase();
            }
        }

        /// <summary>
        /// Initializes Firebase services based on settings.
        /// </summary>
        public void InitializeFirebase()
        {
#if FIREBASE_TOOL
            try
            {
                ConsoleLogger.LogColor("[Firebase] Initializing Firebase...", ColorLog.BLUE);
                
                // Initialize Firebase core
                // Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                //     var dependencyStatus = task.Result;
                //     if (dependencyStatus == Firebase.DependencyStatus.Available)
                //     {
                //         InitializeServices();
                //     }
                //     else
                //     {
                //         ConsoleLogger.LogError($"[Firebase] Could not resolve all Firebase dependencies: {dependencyStatus}");
                //         _isInitialized = false;
                //     }
                // });

                // For placeholder code, just directly initialize services
                InitializeServices();
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[Firebase] Failed to initialize Firebase: {ex.Message}");
                _isInitialized = false;
            }
#else
            ConsoleLogger.LogColor("[Firebase] Firebase is disabled. Enable with FIREBASE_TOOL scripting define.", ColorLog.YELLOW);
            _isInitialized = false;
#endif
        }

        /// <summary>
        /// Initializes the individual Firebase services after Firebase core is initialized.
        /// </summary>
        private void InitializeServices()
        {
#if FIREBASE_TOOL
            try
            {
                if (enableAnalytics)
                {
                    FirebaseAnalyticsManager.Instance.Initialize();
                }

                if (enableRemoteConfig)
                {
                    // You can set default values for Remote Config here
                    Dictionary<string, object> defaults = new Dictionary<string, object>
                    {
                        // Example default values
                        { "welcome_message", "Welcome to our game!" },
                        { "difficulty_level", 1 },
                        { "show_ads", true },
                        { "coins_multiplier", 1.0 }
                    };
                    
                    FirebaseRemoteConfigManager.Instance.Initialize(defaults);
                }

                _isInitialized = true;
                ConsoleLogger.LogColor("[Firebase] Firebase services initialized successfully.", ColorLog.GREEN);
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[Firebase] Failed to initialize Firebase services: {ex.Message}");
                _isInitialized = false;
            }
#endif
        }

        /// <summary>
        /// Fetches the latest Remote Config values.
        /// </summary>
        /// <param name="cacheExpiryInSeconds">How long to cache values before fetching again (in seconds)</param>
        /// <returns>True if fetch was successful, false otherwise</returns>
        public async Task<bool> FetchRemoteConfigAsync(long cacheExpiryInSeconds = 3600)
        {
#if FIREBASE_TOOL
            if (!_isInitialized || !enableRemoteConfig)
            {
                ConsoleLogger.LogWarning("[Firebase] Cannot fetch Remote Config: Firebase not initialized or Remote Config not enabled.");
                return false;
            }

            return await FirebaseRemoteConfigManager.Instance.FetchDataAsync(cacheExpiryInSeconds);
#else
            ConsoleLogger.LogColor("[Firebase] (Disabled) Would fetch Remote Config values", ColorLog.YELLOW);
            return false;
#endif
        }

        /// <summary>
        /// Logs an event to Firebase Analytics.
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="parameters">Optional parameters for the event</param>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
#if FIREBASE_TOOL
            if (!_isInitialized || !enableAnalytics)
            {
                ConsoleLogger.LogWarning("[Firebase] Cannot log event: Firebase not initialized or Analytics not enabled.");
                return;
            }

            FirebaseAnalyticsManager.Instance.LogEvent(eventName, parameters);
#else
            ConsoleLogger.LogColor($"[Firebase] (Disabled) Would log event: {eventName}", ColorLog.YELLOW);
#endif
        }

        /// <summary>
        /// Gets the Analytics Manager instance.
        /// </summary>
        /// <returns>The Firebase Analytics Manager instance</returns>
        public FirebaseAnalyticsManager GetAnalytics()
        {
            return FirebaseAnalyticsManager.Instance;
        }

        /// <summary>
        /// Gets the Remote Config Manager instance.
        /// </summary>
        /// <returns>The Firebase Remote Config Manager instance</returns>
        public FirebaseRemoteConfigManager GetRemoteConfig()
        {
            return FirebaseRemoteConfigManager.Instance;
        }
    }
}
