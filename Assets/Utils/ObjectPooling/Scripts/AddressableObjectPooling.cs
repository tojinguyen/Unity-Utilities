using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Tirex.Utils.ObjectPooling
{
    public static class AddressableObjectPooling
    {
        private const string ADDRESSABLES_OBJECT_POOLING_NAME_FORMAT = "addressables_object_pooling_{0}";
        private static readonly Dictionary<AssetReference, Queue<GameObject>> _pools = new(20);
        private static readonly Dictionary<GameObject, AssetReference> _objectToKeyMap = new(20);

        /// <summary>
        /// Prewarm the pool by instantiating a number of objects in advance.
        /// </summary>
        public static async UniTask Prewarm(AssetReference assetRef, int count)
        {
            if (!_pools.ContainsKey(assetRef))
            {
                _pools[assetRef] = new Queue<GameObject>();
            }

            var pool = _pools[assetRef];

            var tasks = new List<UniTask<GameObject>>();
            for (var i = 0; i < count; i++)
            {
                try
                {
                    var task = AddressableHelper.GetAssetAsync<GameObject>(assetRef, GetFeatureName(assetRef));
                    tasks.Add(task);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[AddressableObjectPooling] Prewarm failed to start task for {assetRef}: {e.Message}");
                }
            }

            GameObject[] results = null;
            try
            {
                // Wait for all tasks to complete simultaneously.
                results = await UniTask.WhenAll(tasks);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableObjectPooling] Prewarm encountered an error while loading {assetRef}: {e.Message}");
                return;
            }

            // After all tasks are completed, process the loaded objects.
            if (results != null)
            {
                foreach (var obj in results)
                {
                // If the object is not null, deactivate it and add it to the pool.
                if (obj == null)
                    continue;
                obj.SetActive(false);
                pool.Enqueue(obj);
                _objectToKeyMap[obj] = assetRef;
            }
            }
        }

        /// <summary>
        /// Get an object from the pool or instantiate a new one if the pool is empty.
        /// </summary>
        public static async UniTask<GameObject> GetObject(AssetReference assetRef)
        {
            if (!_pools.ContainsKey(assetRef))
            {
                _pools[assetRef] = new Queue<GameObject>();
            }

            var pool = _pools[assetRef];

            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            GameObject newObject = null;
            try
            {
                newObject = await AddressableHelper.GetAssetAsync<GameObject>(assetRef, GetFeatureName(assetRef));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableObjectPooling] GetObject failed to load {assetRef}: {e.Message}");
            }
            
            if (newObject != null)
            {
                _objectToKeyMap[newObject] = assetRef;
            }
            return newObject;
        }

        /// <summary>
        /// Get an object from the pool with Vector2 position.
        /// </summary>
        public static async UniTask<GameObject> GetObject(AssetReference assetRef, Vector2 position)
        {
            var instance = await GetObject(assetRef);
            if (instance != null)
            {
                instance.transform.position = new Vector3(position.x, position.y, instance.transform.position.z);
            }

            return instance;
        }

        /// <summary>
        /// Return an object back to the pool.
        /// </summary>
        public static void ReturnObject(GameObject obj)
        {
            if (obj == null) return;
            
            if (!_objectToKeyMap.TryGetValue(obj, out var key))
            {
                Debug.LogError($"Object {obj.name} does not belong to any pool.");
                return;
            }

            obj.SetActive(false);

            if (!_pools.TryGetValue(key, out var pool))
            {
                pool = new Queue<GameObject>();
                _pools[key] = pool;
            }

            pool.Enqueue(obj);
        }


        /// <summary>
        /// Get the current size of the pool for a specific asset.
        /// </summary>
        public static int GetPoolSize(AssetReference assetRef)
        {
            return _pools.TryGetValue(assetRef, out var pool) ? pool.Count : 0;
        }

        /// <summary>
        /// Clear all objects in the pool for a specific asset.
        /// </summary>
        public static void ClearPool(AssetReference assetRef)
        {
            if (!_pools.TryGetValue(assetRef, out var pool)) return;
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                // Optionally destroy the object if you want to free memory.
                Object.Destroy(obj);
            }

            _pools.Remove(assetRef);
            try
            {
                AddressableHelper.UnloadAsyncAllAddressableInFeature(GetFeatureName(assetRef)).Forget();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AddressableObjectPooling] Failed to unload {assetRef}: {e.Message}");
            }
        }

        /// <summary>
        /// Unload all pools and release their objects.
        /// </summary>
        public static void UnloadAllPools()
        {
            foreach (var element in _pools)
            {
                var assetRef = element.Key;
                var pool = element.Value;
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    // Optionally destroy the object to free memory.
                    Object.Destroy(obj);
                }

                try
                {
                    AddressableHelper.UnloadAsyncAllAddressableInFeature(GetFeatureName(assetRef)).Forget();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AddressableObjectPooling] Failed to unload {assetRef}: {e.Message}");
                }
            }

            _pools.Clear();
            _objectToKeyMap.Clear();
        }

        private static string GetFeatureName(AssetReference assetRef)
        {
            return string.Format(ADDRESSABLES_OBJECT_POOLING_NAME_FORMAT, assetRef.AssetGUID);
        }

#if UNITY_EDITOR
        public static IEnumerable<AssetReference> GetAllPoolKeys() => _pools.Keys;
#endif
    }
}