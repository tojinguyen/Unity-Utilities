using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Tirex.Utils.ObjectPooling
{
    public class TestObjectPooling : MonoBehaviour
    {
        [Header("Regular Object Pooling")]
        [SerializeField] private GameObject regularPrefab;
        [SerializeField] private int prewarmCount = 10;
        [SerializeField] private Transform spawnParent;
        
        [Header("Addressable Object Pooling")]
        [SerializeField] private AssetReference addressablePrefab;
        
        private readonly List<GameObject> _spawnedObjects = new();
        
        private void Start()
        {
            if (regularPrefab != null)
            {
                ObjectPooling.Prewarm(regularPrefab, prewarmCount);
                Debug.Log($"Prewarmed regular pool with {prewarmCount} objects. Pool size: {ObjectPooling.GetPoolSize(regularPrefab)}");
            }
            
            if (addressablePrefab != null)
            {
                PrewarmAddressablePool(prewarmCount).Forget();
            }
        }
        
        private async UniTaskVoid PrewarmAddressablePool(int count)
        {
            await AddressableObjectPooling.Prewarm(addressablePrefab, count);
            Debug.Log($"Prewarmed addressable pool with {count} objects. Pool size: {AddressableObjectPooling.GetPoolSize(addressablePrefab)}");
        }
        
        public void SpawnRegularObject()
        {
            if (regularPrefab == null) return;
            
            var position = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0);
            var obj = ObjectPooling.GetObject(regularPrefab, position);
            
            _spawnedObjects.Add(obj);
            Debug.Log($"Spawned regular object. Active objects: {_spawnedObjects.Count}, Pool size: {ObjectPooling.GetPoolSize(regularPrefab)}");
        }
        
        public void ReturnAllRegularObjects()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    ObjectPooling.ReturnObject(obj);
                }
            }
            
            Debug.Log($"Returned all objects to pool. Pool size: {ObjectPooling.GetPoolSize(regularPrefab)}");
            _spawnedObjects.Clear();
        }
        
        public async UniTaskVoid SpawnAddressableObject()
        {
            if (addressablePrefab == null) return;
            
            var position = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
            var obj = await AddressableObjectPooling.GetObject(addressablePrefab, position);
            
            _spawnedObjects.Add(obj);
            Debug.Log($"Spawned addressable object. Active objects: {_spawnedObjects.Count}, Pool size: {AddressableObjectPooling.GetPoolSize(addressablePrefab)}");
        }
        
        public void ReturnAllAddressableObjects()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    AddressableObjectPooling.ReturnObject(obj);
                }
            }
            
            Debug.Log($"Returned all addressable objects to pool. Pool size: {AddressableObjectPooling.GetPoolSize(addressablePrefab)}");
            _spawnedObjects.Clear();
        }
        
        public void ClearRegularPool()
        {
            if (regularPrefab == null) return;
            
            ObjectPooling.ClearPool(regularPrefab);
            Debug.Log("Regular object pool cleared");
        }
        
        public void ClearAddressablePool()
        {
            if (addressablePrefab == null) return;
            
            AddressableObjectPooling.ClearPool(addressablePrefab);
            Debug.Log("Addressable object pool cleared");
        }

        private void ClearAllPools()
        {
            ObjectPooling.ClearAllPools();
            AddressableObjectPooling.UnloadAllPools();
            Debug.Log("All pools cleared");
        }
        
        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}
