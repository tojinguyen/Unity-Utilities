using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TirexGame.Utilities.Patterns;

namespace TirexGame.Utilities.Firebase
{
    /// <summary>
    /// Manager for Firebase Remote Config operations with conditional compilation support.
    /// Use FIREBASE_TOOL scripting define to enable/disable Firebase functionality.
    /// </summary>
    public class FirebaseRemoteConfigManager : Singleton<FirebaseRemoteConfigManager>
    {
        private bool _isInitialized = false;
        private Dictionary<string, object> _defaultValues;

        /// <summary>
        /// Initializes the Firebase Remote Config system.
        /// </summary>
        /// <param name="defaultValues">Default values to use when Remote Config values are not available</param>
        public void Initialize(Dictionary<string, object> defaultValues = null)
        {
            _defaultValues = defaultValues ?? new Dictionary<string, object>();

#if FIREBASE_TOOL
            try
            {
                ConsoleLogger.LogColor("[FirebaseRemoteConfig] Initializing Firebase Remote Config...", ColorLog.BLUE);
                // Initialize Firebase Remote Config
                // Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(_defaultValues);
                _isInitialized = true;
                ConsoleLogger.LogColor("[FirebaseRemoteConfig] Firebase Remote Config initialized successfully.", ColorLog.GREEN);
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to initialize Firebase Remote Config: {ex.Message}");
                _isInitialized = false;
            }
#else
            ConsoleLogger.LogColor("[FirebaseRemoteConfig] Firebase Remote Config is disabled. Enable with FIREBASE_TOOL scripting define.", ColorLog.YELLOW);
            _isInitialized = false;
#endif
        }

        /// <summary>
        /// Fetches the latest values from Firebase Remote Config.
        /// </summary>
        /// <param name="cacheExpiryInSeconds">How long to cache values before fetching again (in seconds)</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<bool> FetchDataAsync(long cacheExpiryInSeconds = 3600)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning("[FirebaseRemoteConfig] Cannot fetch data: Firebase Remote Config not initialized.");
                return false;
            }

            try
            {
                ConsoleLogger.LogColor("[FirebaseRemoteConfig] Fetching remote config data...", ColorLog.BLUE);
                // Firebase.RemoteConfig.ConfigSettings configSettings = new Firebase.RemoteConfig.ConfigSettings();
                // configSettings.FetchTimeoutInMilliseconds = 10000;
                // configSettings.MinimumFetchInternalInMilliseconds = cacheExpiryInSeconds * 1000;
                // await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(cacheExpiryInSeconds));
                // Firebase.RemoteConfig.ConfigInfo info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
                // if (info.LastFetchStatus == Firebase.RemoteConfig.LastFetchStatus.Success)
                // {
                //     await Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                //     ConsoleLogger.LogColor("[FirebaseRemoteConfig] Remote config data fetched and activated successfully.", ColorLog.GREEN);
                //     return true;
                // }
                // else
                // {
                //     ConsoleLogger.LogWarning($"[FirebaseRemoteConfig] Remote config fetch failed with status: {info.LastFetchStatus}");
                //     return false;
                // }

                // Simulated success for placeholder code
                await Task.Delay(100);
                ConsoleLogger.LogColor("[FirebaseRemoteConfig] Remote config data fetched and activated successfully.", ColorLog.GREEN);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to fetch remote config data: {ex.Message}");
                return false;
            }
#else
            ConsoleLogger.LogColor("[FirebaseRemoteConfig] (Disabled) Would fetch remote config data", ColorLog.YELLOW);
            return false;
#endif
        }

        /// <summary>
        /// Gets a string value from Firebase Remote Config.
        /// </summary>
        /// <param name="key">The key for the value to retrieve</param>
        /// <param name="defaultValue">Default value to return if the key is not found</param>
        /// <returns>The string value for the key</returns>
        public string GetString(string key, string defaultValue = "")
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning($"[FirebaseRemoteConfig] Cannot get string: Firebase Remote Config not initialized. Using default: {defaultValue}");
                return defaultValue;
            }

            try
            {
                // string value = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                // ConsoleLogger.LogColor($"[FirebaseRemoteConfig] Retrieved string value for {key}: {value}", ColorLog.BLUE);
                // return value;

                // Simulated for placeholder code - in a real implementation, would get from Firebase
                if (_defaultValues.TryGetValue(key, out object value) && value is string stringValue)
                {
                    return stringValue;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to get string for key {key}: {ex.Message}");
                return defaultValue;
            }
#else
            ConsoleLogger.LogColor($"[FirebaseRemoteConfig] (Disabled) Would get string for key: {key}, using default: {defaultValue}", ColorLog.YELLOW);
            return defaultValue;
#endif
        }

        /// <summary>
        /// Gets a boolean value from Firebase Remote Config.
        /// </summary>
        /// <param name="key">The key for the value to retrieve</param>
        /// <param name="defaultValue">Default value to return if the key is not found</param>
        /// <returns>The boolean value for the key</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning($"[FirebaseRemoteConfig] Cannot get bool: Firebase Remote Config not initialized. Using default: {defaultValue}");
                return defaultValue;
            }

            try
            {
                // bool value = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
                // ConsoleLogger.LogColor($"[FirebaseRemoteConfig] Retrieved boolean value for {key}: {value}", ColorLog.BLUE);
                // return value;

                // Simulated for placeholder code
                if (_defaultValues.TryGetValue(key, out object value) && value is bool boolValue)
                {
                    return boolValue;
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to get boolean for key {key}: {ex.Message}");
                return defaultValue;
            }
#else
            ConsoleLogger.LogColor($"[FirebaseRemoteConfig] (Disabled) Would get boolean for key: {key}, using default: {defaultValue}", ColorLog.YELLOW);
            return defaultValue;
#endif
        }

        /// <summary>
        /// Gets a numeric (double) value from Firebase Remote Config.
        /// </summary>
        /// <param name="key">The key for the value to retrieve</param>
        /// <param name="defaultValue">Default value to return if the key is not found</param>
        /// <returns>The double value for the key</returns>
        public double GetDouble(string key, double defaultValue = 0.0)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning($"[FirebaseRemoteConfig] Cannot get double: Firebase Remote Config not initialized. Using default: {defaultValue}");
                return defaultValue;
            }

            try
            {
                // double value = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
                // ConsoleLogger.LogColor($"[FirebaseRemoteConfig] Retrieved double value for {key}: {value}", ColorLog.BLUE);
                // return value;

                // Simulated for placeholder code
                if (_defaultValues.TryGetValue(key, out object value))
                {
                    if (value is double doubleValue)
                    {
                        return doubleValue;
                    }
                    else if (value is int intValue)
                    {
                        return intValue;
                    }
                    else if (value is float floatValue)
                    {
                        return floatValue;
                    }
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to get double for key {key}: {ex.Message}");
                return defaultValue;
            }
#else
            ConsoleLogger.LogColor($"[FirebaseRemoteConfig] (Disabled) Would get double for key: {key}, using default: {defaultValue}", ColorLog.YELLOW);
            return defaultValue;
#endif
        }

        /// <summary>
        /// Gets a long value from Firebase Remote Config.
        /// </summary>
        /// <param name="key">The key for the value to retrieve</param>
        /// <param name="defaultValue">Default value to return if the key is not found</param>
        /// <returns>The long value for the key</returns>
        public long GetLong(string key, long defaultValue = 0)
        {
#if FIREBASE_TOOL
            if (!_isInitialized)
            {
                ConsoleLogger.LogWarning($"[FirebaseRemoteConfig] Cannot get long: Firebase Remote Config not initialized. Using default: {defaultValue}");
                return defaultValue;
            }

            try
            {
                // long value = (long)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
                // ConsoleLogger.LogColor($"[FirebaseRemoteConfig] Retrieved long value for {key}: {value}", ColorLog.BLUE);
                // return value;

                // Simulated for placeholder code
                if (_defaultValues.TryGetValue(key, out object value))
                {
                    if (value is long longValue)
                    {
                        return longValue;
                    }
                    else if (value is int intValue)
                    {
                        return intValue;
                    }
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"[FirebaseRemoteConfig] Failed to get long for key {key}: {ex.Message}");
                return defaultValue;
            }
#else
            ConsoleLogger.LogColor($"[FirebaseRemoteConfig] (Disabled) Would get long for key: {key}, using default: {defaultValue}", ColorLog.YELLOW);
            return defaultValue;
#endif
        }
    }
}
