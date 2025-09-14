using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.Data
{
    public class DataManager : MonoSingleton<DataManager>
    {
        [Header("Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool enableCaching = true;
        [SerializeField] private int defaultCacheExpirationMinutes = 30;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f; 
        
        private readonly Dictionary<Type, IDataRepository> _repositories = new();
        private readonly DataCacheManager _cacheManager = new();
        private readonly DataEventManager _eventManager = new();
        private readonly DataValidator _validator = new();
        
        public event Action<Type, object> OnDataSaved;
        public event Action<Type, object> OnDataLoaded;
        public event Action<Type, Exception> OnDataError;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            if (enableAutoSave)
            {
                StartAutoSave().Forget();
            }
            
            Log("DataManager initialized");
        }
        
        public void RegisterRepository<T>(IDataRepository<T> repository) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            _repositories[type] = repository;
            Log($"Repository registered for type: {type.Name}");
        }
        
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
                            // Trả về dữ liệu mặc định nếu dữ liệu đã lưu bị lỗi
                            var defaultDataOnFail = new T();
                            defaultDataOnFail.SetDefaultData();
                            return defaultDataOnFail;
                        }
                        
                        // Cache data
                        if (enableCaching)
                        {
                            _cacheManager.Cache(key, data, TimeSpan.FromMinutes(defaultCacheExpirationMinutes));
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
        
        public async UniTask<bool> DeleteDataAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
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

        public async UniTask<bool> ExistsAsync<T>(string key = null) where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            key ??= type.Name;
            
            if (enableCaching && _cacheManager.ContainsKey(key))
            {
                return true;
            }
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.ExistsAsync(key);
            }
            
            return false;
        }

        public async UniTask<IEnumerable<string>> GetAllKeysAsync<T>() where T : class, IDataModel<T>, new()
        {
            var type = typeof(T);
            
            if (_repositories.TryGetValue(type, out var repo) && repo is IDataRepository<T> typedRepo)
            {
                return await typedRepo.GetAllKeysAsync();
            }
            
            return Array.Empty<string>();
        }

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

        public async UniTask SaveAllAsync()
        {
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

        public void SubscribeToDataEvents<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null) where T : class
        {
            _eventManager.Subscribe(onSaved, onLoaded, onDeleted);
        }

        public void UnsubscribeFromDataEvents<T>(
            Action<T> onSaved = null,
            Action<T> onLoaded = null,
            Action<string> onDeleted = null) where T : class
        {
            _eventManager.Unsubscribe(onSaved, onLoaded, onDeleted);
        }

        public DataCacheStats GetCacheStats()
        {
            return _cacheManager.GetStats();
        }
        
        private async UniTaskVoid StartAutoSave()
        {
            while (this != null && gameObject.activeInHierarchy)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(autoSaveIntervalSeconds), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
                
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