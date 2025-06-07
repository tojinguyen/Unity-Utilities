using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Centralized data management service that provides unified access to all data operations
    /// </summary>
    public class DataManager : MonoSingleton<DataManager>
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int defaultCacheExpirationMinutes = 30;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes
        
        private readonly Dictionary<Type, IDataRepository> _repositories = new();
        private readonly DataCacheManager _cacheManager = new();
        private readonly DataEventManager _eventManager = new();
        private readonly DataValidator _validator = new();
        private readonly DataBackupManager _backupManager = new();
        
        // Events
        public event Action<Type, object> OnDataSaved;
        public event Action<Type, object> OnDataLoaded;
        public event Action<Type, Exception> OnDataError;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            if (enableAutoSave)
            {
                StartAutoSave();
            }
            
            Log("DataManager initialized");
        }
        
        /// <summary>
        /// Register a data repository for a specific data type
        /// </summary>
        public void RegisterRepository<T>(IDataRepository<T> repository) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            _repositories[type] = repository;
            Log($"Repository registered for type: {type.Name}");
        }
        
        /// <summary>
        /// Get data with caching support
        /// </summary>
        public async UniTask<T> GetDataAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Check cache first
                if (enableCaching && _cacheManager.TryGetCached<T>(key, out var cachedData))
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
                            return null;
                        }
                        
                        // Cache data
                        if (enableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(defaultCacheExpirationMinutes));
                        }
                        
                        OnDataLoaded?.Invoke(type, data);
                        _eventManager.RaiseDataLoaded(type, data);
                        
                        Log($"Data loaded: {key}");
                        return data;
                    }
                }
                
                // Return default if not found
                var defaultData = new T();
                defaultData.SetDefaultData();
                return defaultData;
            }
            catch (Exception ex)
            {
                LogError($"Failed to get data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                throw;
            }
        }
        
        /// <summary>
        /// Save data with validation and caching
        /// </summary>
        public async UniTask<bool> SaveDataAsync<T>(T data, string key = null) where T : class, IDataModel<T>, new()
        {
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
                
                // Create backup before saving
                await _backupManager.CreateBackupAsync(key, data);
                
                // Save through repository
                if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
                {
                    var success = await typedRepo.SaveAsync(key, data);
                    
                    if (success)
                    {
                        // Update cache
                        if (enableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(defaultCacheExpirationMinutes));
                        }
                        
                        OnDataSaved?.Invoke(type, data);
                        _eventManager.RaiseDataSaved(type, data);
                        
                        Log($"Data saved: {key}");
                        return true;
                    }
                }
                
                LogError($"No repository found for type: {type.Name}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save data {key}: {ex.Message}");
                OnDataError?.Invoke(type, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Delete data
        /// </summary>
        public async UniTask<bool> DeleteDataAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            key ??= type.Name;
            
            try
            {
                // Create backup before deletion
                var existingData = await GetDataAsync<T>(key);
                if (existingData != null)
                {
                    await _backupManager.CreateBackupAsync(key, existingData);
                }
                
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
        
        /// <summary>
        /// Check if data exists
        /// </summary>
        public async UniTask<bool> ExistsAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            key ??= type.Name;
            
            // Check cache first
            if (enableCaching && _cacheManager.ContainsKey(key))
            {
                return true;
            }
            
            // Check repository
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.ExistsAsync(key);
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all data keys for a type
        /// </summary>
        public async UniTask<IEnumerable<string>> GetAllKeysAsync<T>() where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.GetAllKeysAsync();
            }
            
            return new string[0];
        }
        
        /// <summary>
        /// Clear cache for a specific key or all cache
        /// </summary>
        public void ClearCache(string key = null)
        {
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
        
        /// <summary>
        /// Save all cached data
        /// </summary>
        public async UniTask SaveAllAsync()
        {
            var tasks = new List<UniTask>();
            
            foreach (var kvp in _repositories)
            {
                if (kvp.Value is IDataRepository repository)
                {
                    tasks.Add(repository.SaveAllAsync());
                }
            }
            
            await UniTask.WhenAll(tasks);
            Log("All data saved");
        }
        
        /// <summary>
        /// Subscribe to data events
        /// </summary>
        public void SubscribeToDataEvents<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null) where T : class
        {
            _eventManager.Subscribe<T>(onSaved, onLoaded, onDeleted);
        }
        
        /// <summary>
        /// Unsubscribe from data events
        /// </summary>
        public void UnsubscribeFromDataEvents<T>() where T : class
        {
            _eventManager.Unsubscribe<T>();
        }
        
        /// <summary>
        /// Get cache statistics
        /// </summary>
        public DataCacheStats GetCacheStats()
        {
            return _cacheManager.GetStats();
        }
        
        /// <summary>
        /// Create a backup of all data
        /// </summary>
        public async UniTask CreateFullBackupAsync()
        {
            await _backupManager.CreateFullBackupAsync(_repositories);
            Log("Full backup created");
        }
        
        /// <summary>
        /// Restore data from backup
        /// </summary>
        public async UniTask<bool> RestoreFromBackupAsync(string backupId)
        {
            var success = await _backupManager.RestoreFromBackupAsync(backupId, _repositories);
            if (success)
            {
                ClearCache(); // Clear cache after restore
                Log($"Data restored from backup: {backupId}");
            }
            return success;
        }
        
        private async void StartAutoSave()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(autoSaveIntervalSeconds));
                
                if (this != null)
                {
                    await SaveAllAsync();
                }
            }
        }
        
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[DataManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            if (enableLogging)
            {
                Debug.LogError($"[DataManager] {message}");
            }
        }
        
        protected override void OnDestroy()
        {
            _cacheManager.ClearAll();
            _eventManager.ClearAll();
            base.OnDestroy();
        }
    }
}
