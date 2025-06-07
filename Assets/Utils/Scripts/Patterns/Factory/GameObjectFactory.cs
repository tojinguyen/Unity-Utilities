using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace TirexGame.Utils.Patterns.Factory
{
    /// <summary>
    /// A factory for creating GameObjects from prefabs with pooling support
    /// </summary>
    public class GameObjectFactory : MonoBehaviour, IManagedFactory<GameObject>, IAsyncFactory<GameObject>
    {
        [Header("Configuration")]
        [SerializeField] private bool enablePooling = true;
        [SerializeField] private bool enableLogging = false;
        [SerializeField] private Transform defaultParent;
        
        [Header("Prefab Registry")]
        [SerializeField] private List<PrefabEntry> prefabEntries = new();
        
        private readonly Dictionary<string, GameObject> _prefabRegistry = new();
        private readonly Dictionary<string, AssetReference> _addressableRegistry = new();
        private readonly Dictionary<GameObject, string> _instanceToId = new();
        private readonly HashSet<GameObject> _activeInstances = new();
        
        public int ActiveCount => _activeInstances.Count;
        
        private void Awake()
        {
            InitializeRegistry();
        }
        
        private void InitializeRegistry()
        {
            foreach (var entry in prefabEntries)
            {
                if (!string.IsNullOrEmpty(entry.id))
                {
                    if (entry.prefab != null)
                    {
                        _prefabRegistry[entry.id] = entry.prefab;
                        Log($"Registered prefab: {entry.id}");
                    }
                    
                    if (entry.addressableReference != null && entry.addressableReference.RuntimeKeyIsValid())
                    {
                        _addressableRegistry[entry.id] = entry.addressableReference;
                        Log($"Registered addressable: {entry.id}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Register a prefab with an ID
        /// </summary>
        public void RegisterPrefab(string id, GameObject prefab)
        {
            if (string.IsNullOrEmpty(id) || prefab == null)
            {
                LogError("Invalid id or prefab for registration");
                return;
            }
            
            _prefabRegistry[id] = prefab;
            Log($"Registered prefab: {id}");
        }
        
        /// <summary>
        /// Register an addressable reference with an ID
        /// </summary>
        public void RegisterAddressable(string id, AssetReference assetReference)
        {
            if (string.IsNullOrEmpty(id) || assetReference == null)
            {
                LogError("Invalid id or asset reference for registration");
                return;
            }
            
            _addressableRegistry[id] = assetReference;
            Log($"Registered addressable: {id}");
        }
        
        public GameObject Create()
        {
            LogError("Create() without ID not supported. Use Create(string id) instead.");
            return null;
        }
        
        public GameObject Create(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                LogError("Cannot create object with null or empty ID");
                return null;
            }
            
            GameObject instance = null;
            
            // Try pooling first if enabled
            if (enablePooling && _prefabRegistry.ContainsKey(id))
            {
                instance = ObjectPooling.GetObject(_prefabRegistry[id]);
            }
            else if (_prefabRegistry.ContainsKey(id))
            {
                // Direct instantiation
                var prefab = _prefabRegistry[id];
                instance = Instantiate(prefab, defaultParent);
            }
            
            if (instance != null)
            {
                _instanceToId[instance] = id;
                _activeInstances.Add(instance);
                Log($"Created object: {id}");
                
                // Initialize if it has IFactoryCreated interface
                var factoryCreated = instance.GetComponent<IFactoryCreated>();
                factoryCreated?.OnFactoryCreated(id);
            }
            else
            {
                LogError($"Failed to create object with ID: {id}");
            }
            
            return instance;
        }
        
        public async UniTask<GameObject> CreateAsync()
        {
            LogError("CreateAsync() without ID not supported. Use CreateAsync(string id) instead.");
            return null;
        }
        
        public async UniTask<GameObject> CreateAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                LogError("Cannot create object with null or empty ID");
                return null;
            }
            
            GameObject instance = null;
            
            // Try addressables first
            if (_addressableRegistry.ContainsKey(id))
            {
                try
                {
                    var assetRef = _addressableRegistry[id];
                    var prefab = await AddressablesHelper.GetAssetAsync<GameObject>(assetRef, "GameObjectFactory");
                    
                    if (enablePooling)
                    {
                        instance = ObjectPooling.GetObject(prefab);
                    }
                    else
                    {
                        instance = Instantiate(prefab, defaultParent);
                    }
                }
                catch (Exception e)
                {
                    LogError($"Failed to load addressable {id}: {e.Message}");
                }
            }
            
            // Fallback to direct prefab creation
            if (instance == null)
            {
                instance = Create(id);
            }
            
            if (instance != null)
            {
                Log($"Created object async: {id}");
            }
            
            return instance;
        }
        
        public void Destroy(GameObject obj)
        {
            if (obj == null)
            {
                LogError("Cannot destroy null object");
                return;
            }
            
            if (!_activeInstances.Contains(obj))
            {
                LogError("Object not created by this factory");
                return;
            }
            
            var id = _instanceToId.ContainsKey(obj) ? _instanceToId[obj] : "Unknown";
            
            // Notify if it has IFactoryCreated interface
            var factoryCreated = obj.GetComponent<IFactoryCreated>();
            factoryCreated?.OnFactoryDestroyed();
            
            if (enablePooling)
            {
                ObjectPooling.ReturnObject(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
            }
            
            _instanceToId.Remove(obj);
            _activeInstances.Remove(obj);
            
            Log($"Destroyed object: {id}");
        }
        
        /// <summary>
        /// Destroy all objects created by this factory
        /// </summary>
        public void DestroyAll()
        {
            var instances = new GameObject[_activeInstances.Count];
            _activeInstances.CopyTo(instances);
            
            foreach (var instance in instances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            
            Log("Destroyed all factory objects");
        }
        
        /// <summary>
        /// Get all active instances of a specific type
        /// </summary>
        public List<GameObject> GetActiveInstances(string id)
        {
            var result = new List<GameObject>();
            
            foreach (var kvp in _instanceToId)
            {
                if (kvp.Value == id && _activeInstances.Contains(kvp.Key))
                {
                    result.Add(kvp.Key);
                }
            }
            
            return result;
        }
        
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[GameObjectFactory] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GameObjectFactory] {message}");
        }
        
        private void OnDestroy()
        {
            DestroyAll();
        }
        
        [System.Serializable]
        private class PrefabEntry
        {
            public string id;
            public GameObject prefab;
            public AssetReference addressableReference;
        }
    }
    
    /// <summary>
    /// Interface for objects that need to know when they're created/destroyed by factory
    /// </summary>
    public interface IFactoryCreated
    {
        void OnFactoryCreated(string id);
        void OnFactoryDestroyed();
    }
}
