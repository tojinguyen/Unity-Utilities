using System.Collections.Generic;
using UnityEngine;

public static class ObjectPooling
{
    private static Dictionary<GameObject, Queue<GameObject>> _pools = new();

    /// <summary>
    /// Preloads a specified number of objects into the pool for a given prefab.
    /// </summary>
    /// <param name="prefab">The prefab to preload.</param>
    /// <param name="count">The number of objects to preload.</param>
    public static void Prewarm(GameObject prefab, int count)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        for (var i = 0; i < count; i++)
        {
            var obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Retrieves an object from the pool or creates a new one if none are available.
    /// </summary>
    /// <param name="prefab">The prefab to use as a template.</param>
    /// <returns>A GameObject from the pool.</returns>
    public static GameObject GetObject(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Create a new object if the pool is empty
        var newObj = Object.Instantiate(prefab);
        return newObj;
    }

    /// <summary>
    /// Returns an object to the pool for reuse.
    /// </summary>
    /// <param name="prefab">The prefab associated with this object.</param>
    /// <param name="obj">The GameObject to return to the pool.</param>
    public static void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
    
    /// <summary>
    /// Clears all objects from the pool for the specified prefab.
    /// All pooled objects will be destroyed.
    /// </summary>
    /// <param name="prefab">The prefab whose pool should be cleared.</param>
    public static void ClearPool(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var pool))
            return;

        while (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            Object.Destroy(obj);
        }

        _pools.Remove(prefab);
    }


    /// <summary>
    /// Clears all pools and destroys all objects.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach (var pool in _pools.Values)
        {
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                Object.Destroy(obj);
            }
        }

        _pools.Clear();
    }
    
    /// <summary>
    /// Gets the current size of the pool for a specific prefab.
    /// </summary>
    /// <param name="prefab">The prefab whose pool size is queried.</param>
    /// <returns>The number of objects currently in the pool.</returns>
    public static int GetPoolSize(GameObject prefab)
    {
        return _pools.TryGetValue(prefab, out var pool) ? pool.Count : 0;
    }

}