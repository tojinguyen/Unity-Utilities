using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    public class DataManagerConfig
    {
        public bool EnableLogging { get; set; } = true;
        public bool EnableCaching { get; set; } = true;
        public int DefaultCacheExpirationMinutes { get; set; } = 30;
        public bool EnableAutoSave { get; set; } = true;
        public float AutoSaveIntervalSeconds { get; set; } = 300f;
    }

    public static class DataManager
    {
        private static DataManagerConfig _config = new();
        private static readonly Dictionary<Type, IDataRepository> _repositories = new();
        private static readonly DataCacheManager _cacheManager = new();
        private static readonly DataEventManager _eventManager = new();
        private static readonly DataValidator _validator = new();
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();
        
        public static event Action<Type, object> OnDataSaved;
        public static event Action<Type, object> OnDataLoaded;
        public static event Action<Type, Exception> OnDataError;
        
        public static void Initialize(DataManagerConfig config = null)
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    Log("DataManager already initialized - skipping duplicate initialization");
                    return;
                }

                _config = config ?? new DataManagerConfig();
                
                if (_config.EnableAutoSave)
                {
                    StartAutoSave().Forget();
                }
                
                _isInitialized = true;
                Log($"[DataManager] Successfully initialized at {DateTime.Now:HH:mm:ss.fff} - Priority execution enabled");
            }
        }
        
        public static void RegisterRepository<T>(IDataRepository<T> repository) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            _repositories[type] = repository;
            Log($"Repository registered for type: {type.Name}");
        }
        
        public static async UniTask<T> GetDataAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Check cache first
                if (_config.EnableCaching && _cacheManager.TryGetCached<T>(key, out var cachedData))
                {
                    Log($"Data retrieved from cache: {key}");
                    return cachedData;
                }
                
                // Get from repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
                {
                    var data = await typedRepo.LoadAsync(key);
                    
                    if (data != null)
                    {
                        // Validate data
                        var validationResult = await _validator.ValidateAsync(data);
                        if (!validationResult.IsValid)
                        {
                            LogError($"Data validation failed for {key}: {string.Join(", ", validationResult.Errors)}");
                            // Trả về dữ liệu mặc định nếu dữ liệu đã lưu bị lỗi
                            var defaultDataOnFail = new T();
                            defaultDataOnFail.SetDefaultData();
                            return defaultDataOnFail;
                        }
                        
                        // Cache data
                        if (_config.EnableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(_config.DefaultCacheExpirationMinutes));
                        }
                        
                        OnDataLoaded?.Invoke(type, data);
                        _eventManager.RaiseDataLoaded(type, data, key);
                        
                        Log($"Data loaded: {key}");
                        return data;
                    }
                }
                
                // Return default if not found
                Log($"No data found for {key}, creating default.");
                var defaultData = new T();
                defaultData.SetDefaultData();
                return defaultData;
            }
            catch (DataAccessException ex) // Bắt lỗi cụ thể từ repository
            {
                LogError($"Failed to get data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                // Nếu load lỗi, trả về dữ liệu mặc định để game không bị crash
                var defaultDataOnError = new T();
                defaultDataOnError.SetDefaultData();
                return defaultDataOnError;
            }
        }
        
        public static async UniTask<bool> SaveDataAsync<T>(T data, string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                if (data == null)
                {
                    LogError($"Cannot save null data for key: {key}");
                    return false;
                }
                
                // Validate data
                var validationResult = await _validator.ValidateAsync(data);
                if (!validationResult.IsValid)
                {
                    LogError($"Data validation failed for {key}: {string.Join(", ", validationResult.Errors)}");
                    return false;
                }
                
                // Save through repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
                {
                    var success = await typedRepo.SaveAsync(key, data);
                    
                    if (success)
                    {
                        // Update cache
                        if (_config.EnableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(_config.DefaultCacheExpirationMinutes));
                        }
                        
                        OnDataSaved?.Invoke(type, data);
                        _eventManager.RaiseDataSaved(type, data, key);
                        
                        Log($"Data saved: {key}");
                        return true;
                    }
                }
                
                LogError($"No repository found for type: {type.Name}");
                return false;
            }
            catch (DataAccessException ex)
            {
                LogError($"Failed to save data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                return false;
            }
        }
        
        public static async UniTask<bool> DeleteDataAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Delete from repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
                {
                    var success = await typedRepo.DeleteAsync(key);
                    
                    if (success)
                    {
                        // Remove from cache
                        _cacheManager.RemoveFromCache(key);
                        
                        _eventManager.RaiseDataDeleted(type, key);
                        Log($"Data deleted: {key}");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                return false;
            }
        }

        public static async UniTask<bool> ExistsAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            if (_config.EnableCaching && _cacheManager.ContainsKey(key))
            {
                return true;
            }
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.ExistsAsync(key);
            }
            
            return false;
        }

        public static async UniTask<IEnumerable<string>> GetAllKeysAsync<T>() where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.GetAllKeysAsync();
            }
            
            return Array.Empty<string>();
        }

        // Synchronous API Methods for lightweight operations
        
        /// <summary>
        /// Synchronously gets data for the specified type and key.
        /// This method blocks the current thread and should only be used for lightweight operations.
        /// </summary>
        public static T GetData<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Check cache first
                if (_config.EnableCaching && _cacheManager.TryGetCached<T>(key, out var cachedData))
                {
                    Log($"Data retrieved from cache: {key}");
                    return cachedData;
                }
                
                // Get from repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepositorySync<T> syncRepo)
                {
                    var data = syncRepo.Load(key);
                    
                    if (data != null)
                    {
                        // Validate data synchronously
                        var validationResult = _validator.Validate(data);
                        if (!validationResult.IsValid)
                        {
                            LogError($"Data validation failed for {key}: {string.Join(", ", validationResult.Errors)}");
                            // Return default data if saved data is corrupted
                            var defaultDataOnFail = new T();
                            defaultDataOnFail.SetDefaultData();
                            return defaultDataOnFail;
                        }
                        
                        // Cache data
                        if (_config.EnableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(_config.DefaultCacheExpirationMinutes));
                        }
                        
                        OnDataLoaded?.Invoke(type, data);
                        _eventManager.RaiseDataLoaded(type, data, key);
                        
                        Log($"Data loaded: {key}");
                        return data;
                    }
                }
                
                // Return default if not found
                Log($"No data found for {key}, creating default.");
                var defaultData = new T();
                defaultData.SetDefaultData();
                return defaultData;
            }
            catch (DataAccessException ex)
            {
                LogError($"Failed to get data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                // Return default data if load fails to prevent game crash
                var defaultDataOnError = new T();
                defaultDataOnError.SetDefaultData();
                return defaultDataOnError;
            }
        }
        
        /// <summary>
        /// Synchronously saves data for the specified type and key.
        /// This method blocks the current thread and should only be used for lightweight operations.
        /// </summary>
        public static bool SaveData<T>(T data, string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                if (data == null)
                {
                    LogError($"Cannot save null data for key: {key}");
                    return false;
                }
                
                // Validate data synchronously
                var validationResult = _validator.Validate(data);
                if (!validationResult.IsValid)
                {
                    LogError($"Data validation failed for {key}: {string.Join(", ", validationResult.Errors)}");
                    return false;
                }
                
                // Save through repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepositorySync<T> syncRepo)
                {
                    var success = syncRepo.Save(key, data);
                    
                    if (success)
                    {
                        // Update cache
                        if (_config.EnableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(_config.DefaultCacheExpirationMinutes));
                        }
                        
                        OnDataSaved?.Invoke(type, data);
                        _eventManager.RaiseDataSaved(type, data, key);
                        
                        Log($"Data saved: {key}");
                        return true;
                    }
                }
                
                LogError($"No sync repository found for type: {type.Name}");
                return false;
            }
            catch (DataAccessException ex)
            {
                LogError($"Failed to save data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Synchronously deletes data for the specified type and key.
        /// This method blocks the current thread and should only be used for lightweight operations.
        /// </summary>
        public static bool DeleteData<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Delete from repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepositorySync<T> syncRepo)
                {
                    var success = syncRepo.Delete(key);
                    
                    if (success)
                    {
                        // Remove from cache
                        _cacheManager.RemoveFromCache(key);
                        
                        _eventManager.RaiseDataDeleted(type, key);
                        Log($"Data deleted: {key}");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Failed to delete data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                return false;
            }
        }

        /// <summary>
        /// Synchronously checks if data exists for the specified type and key.
        /// This method blocks the current thread and should only be used for lightweight operations.
        /// </summary>
        public static bool Exists<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            key ??= type.Name;
            
            if (_config.EnableCaching && _cacheManager.ContainsKey(key))
            {
                return true;
            }
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepositorySync<T> syncRepo)
            {
                return syncRepo.Exists(key);
            }
            
            return false;
        }

        /// <summary>
        /// Synchronously gets all keys for the specified type.
        /// This method blocks the current thread and should only be used for lightweight operations.
        /// </summary>
        public static IEnumerable<string> GetAllKeys<T>() where T : class, IDataModel<T>, new()
        {
            EnsureInitialized();
            
            var type = typeof(T);
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepositorySync<T> syncRepo)
            {
                return syncRepo.GetAllKeys();
            }
            
            return Array.Empty<string>();
        }

        public static void ClearCache(string key = null)
        {
            EnsureInitialized();
            
            if (string.IsNullOrEmpty(key))
            {
                _cacheManager.ClearAll();
                Log("All cache cleared");
            }
            else
            {
                _cacheManager.RemoveFromCache(key);
                Log($"Cache cleared for key: {key}");
            }
        }

        public static async UniTask SaveAllAsync()
        {
            EnsureInitialized();
            
            Log("Auto-saving all pending data...");
            var tasks = new List<UniTask>();
            
            foreach (var kvp in _repositories)
            {
                var repository = kvp.Value;
                if (repository != null)
                {
                    tasks.Add(repository.SaveAllAsync());
                }
            }
            
            await UniTask.WhenAll(tasks);
            Log("Auto-save complete.");
        }

        public static void SubscribeToDataEvents<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null) where T : class
        {
            EnsureInitialized();
            _eventManager.Subscribe(onSaved, onLoaded, onDeleted);
        }

        public static void UnsubscribeFromDataEvents<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null) where T : class
        {
            EnsureInitialized();
            _eventManager.Unsubscribe(onSaved, onLoaded, onDeleted);
        }

        public static DataCacheStats GetCacheStats()
        {
            EnsureInitialized();
            return _cacheManager.GetStats();
        }
        
        private static async UniTaskVoid StartAutoSave()
        {
            while (_isInitialized)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_config.AutoSaveIntervalSeconds), ignoreTimeScale: true);
                
                if (_isInitialized)
                {
                    await SaveAllAsync();
                }
            }
        }
        
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[DataManager] Auto-initializing DataManager because it wasn't explicitly initialized. For better control, call DataManager.Initialize() manually.");
                Initialize();
            }
        }
        
        private static void Log(string message)
        {
            if (_config.EnableLogging)
            {
                Debug.Log($"[DataManager] {message}");
            }
        }
        
        private static void LogError(string message)
        {
            if (_config.EnableLogging)
            {
                Debug.LogError($"[DataManager] {message}");
            }
        }
        
        public static void Shutdown()
        {
            lock (_lockObject)
            {
                if (!_isInitialized) return;
                
                _cacheManager.ClearAll();
                _eventManager.ClearAll();
                _repositories.Clear();
                _isInitialized = false;
                
                Log("DataManager shutdown");
            }
        }
    }
}