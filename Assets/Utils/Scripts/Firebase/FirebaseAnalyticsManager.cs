using System;
using System.Collections.Generic;
using TirexGame.Utilities.Patterns;

namespace TirexGame.Utilities.Firebase
{
    /// <summary>
    /// Manager for Firebase Analytics operations with conditional compilation support.
    /// Use FIREBASE_TOOL scripting define to enable/disable Firebase functionality.
    /// </summary>
    public class FirebaseAnalyticsManager : Singleton<FirebaseAnalyticsManager>
    {
        private bool _isInitialized = false;

        /// <summary>
        /// Initializes the Firebase Analytics system.
        /// </summary>
        public void Initialize()
        {
#if FIREBASE_TOOL
            try
            {
                // Firebase initialization code
                ConsoleLogger.LogColor("[FirebaseAnalytics] Initializing Firebase Analytics...", ColorLog.BLUE);
                // Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {...});
                _isInitialized = true;
                ConsoleLogger.LogColor("[FirebaseAnalytics] Firebase Analytics initialized successfully.", ColorLog.GREEN);
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseAnalytics] Failed to initialize Firebase Analytics: {ex.Message}");
                _isInitialized = false;
            }
#else
            ConsoleLogger.LogColor("[FirebaseAnalytics] Firebase Analytics is disabled. Enable with FIREBASE_TOOL scripting define.", ColorLog.YELLOW);
            _isInitialized = false;
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
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning("[FirebaseAnalytics] Cannot log event: Firebase Analytics not initialized.");
                return;
            }

            try
            {
                if (parameters == null)
                {
                    // Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
                    ConsoleLogger.LogColor($"[FirebaseAnalytics] Logged event: {eventName}", ColorLog.BLUE);
                }
                else
                {
                    // Convert Dictionary<string, object> to Firebase.Analytics.Parameter[]
                    // Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
                    ConsoleLogger.LogColor($"[FirebaseAnalytics] Logged event: {eventName} with {parameters.Count} parameters", ColorLog.BLUE);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseAnalytics] Failed to log event {eventName}: {ex.Message}");
            }
#else
            ConsoleLogger.LogColor($"[FirebaseAnalytics] (Disabled) Event would be logged: {eventName}", ColorLog.YELLOW);
#endif
        }

        /// <summary>
        /// Sets a user property in Firebase Analytics.
        /// </summary>
        /// <param name="name">Name of the user property</param>
        /// <param name="value">Value of the user property</param>
        public void SetUserProperty(string name, string value)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning("[FirebaseAnalytics] Cannot set user property: Firebase Analytics not initialized.");
                return;
            }

            try
            {
                // Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value);
                ConsoleLogger.LogColor($"[FirebaseAnalytics] Set user property: {name} = {value}", ColorLog.BLUE);
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseAnalytics] Failed to set user property {name}: {ex.Message}");
            }
#else
            ConsoleLogger.LogColor($"[FirebaseAnalytics] (Disabled) User property would be set: {name} = {value}", ColorLog.YELLOW);
#endif
        }

        /// <summary>
        /// Sets the user ID for Firebase Analytics.
        /// </summary>
        /// <param name="userId">The user ID to set</param>
        public void SetUserId(string userId)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning("[FirebaseAnalytics] Cannot set user ID: Firebase Analytics not initialized.");
                return;
            }

            try
            {
                // Firebase.Analytics.FirebaseAnalytics.SetUserId(userId);
                ConsoleLogger.LogColor($"[FirebaseAnalytics] Set user ID: {userId}", ColorLog.BLUE);
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseAnalytics] Failed to set user ID: {ex.Message}");
            }
#else
            ConsoleLogger.LogColor($"[FirebaseAnalytics] (Disabled) User ID would be set: {userId}", ColorLog.YELLOW);
#endif
        }
    }
}
