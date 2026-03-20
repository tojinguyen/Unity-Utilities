using System.Collections.Generic;
using UnityEngine;

namespace Tirex.Utils.ObjectPooling
{
    public class TestObjectPooling : MonoBehaviour
    {
        [Header("Regular Object Pooling")]
        [SerializeField] private GameObject regularPrefab;
        [SerializeField] private int prewarmCount = 10;
        
        private readonly List<GameObject> _spawnedObjects = new();
        
        private void Start()
        {
            if (regularPrefab != null)
            {
                ObjectPooling.Prewarm(regularPrefab, prewarmCount);
                Debug.Log($"Prewarmed regular pool with {prewarmCount} objects. Pool size: {ObjectPooling.GetPoolSize(regularPrefab)}");
            }
        }
        

        
        [ContextMenu("Spawn Regular Object")]
        public void SpawnRegularObject()
        {
            if (regularPrefab == null) return;
            
            var position = new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 3f), 0);
            var obj = ObjectPooling.GetObject(regularPrefab, position);
            
            _spawnedObjects.Add(obj);
            Debug.Log($"Spawned regular object. Active objects: {_spawnedObjects.Count}, Pool size: {ObjectPooling.GetPoolSize(regularPrefab)}");
        }
        
        [ContextMenu("Return All Regular Objects")]
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
        
        [ContextMenu("Clear Regular Pool")]
        public void ClearRegularPool()
        {
            if (regularPrefab == null) return;
            
            ObjectPooling.ClearPool(regularPrefab);
            Debug.Log("Regular object pool cleared");
        }
        

        [ContextMenu("Clear All Pools")]
        public void ClearAllPools()
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
