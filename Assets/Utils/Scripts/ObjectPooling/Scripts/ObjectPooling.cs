using System.Collections.Generic;
using UnityEngine;

public static class ObjectPooling
{
    private static readonly Dictionary<GameObject, Queue<Component>> Pools = new();
    private static readonly Dictionary<Component, GameObject> InstanceToPrefab = new();

#if OBJECT_POOLING_TRACK_PERFORMANCE
    // ======= PERFORMANCE TRACKING =======
    private static readonly Dictionary<GameObject, PoolStats> PoolStatistics = new();

    private struct PoolStats
    {
        public int TotalCreated;
        public int TotalReturned;
        public int TotalRequests;
        public int PoolHits;
        public int PoolMisses;
        public int PeakActiveCount;
        public int CurrentActiveCount;

        public float HitRate => TotalRequests > 0 ? (float)PoolHits / TotalRequests : 0f;
        public float MissRate => TotalRequests > 0 ? (float)PoolMisses / TotalRequests : 0f;
    }
#endif

    // ======= COMPONENT VERSIONS =======    
    public static void Prewarm<T>(T prefab, int count) where T : Component
    {
        var prefabGo = prefab.gameObject;

        if (!Pools.TryGetValue(prefabGo, out var pool))
        {
            pool = new Queue<Component>();
            Pools[prefabGo] = pool;
        }        for (var i = 0; i < count; i++)
        {
            var instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
            InstanceToPrefab[instance] = prefabGo;
        }

#if OBJECT_POOLING_TRACK_PERFORMANCE
        // ======= PERFORMANCE TRACKING =======
        if (!PoolStatistics.TryGetValue(prefabGo, out var stats))
        {
            stats = new PoolStats();
            PoolStatistics[prefabGo] = stats;
        }

        stats.TotalCreated += count;
        PoolStatistics[prefabGo] = stats;

        ConsoleLogger.Log(
            $"[ObjectPooling] Prewarmed pool for {prefabGo.name} with {count} objects. Pool size: {pool.Count}");
#endif
    }

    public static T GetObject<T>(T prefab) where T : Component
    {
        var prefabGo = prefab.gameObject;

        if (!Pools.TryGetValue(prefabGo, out var pool))
        {
            pool = new Queue<Component>();
            Pools[prefabGo] = pool;
        }

        var wasPoolHit = pool.Count > 0;
        var instance = wasPoolHit
            ? (T)pool.Dequeue()
            : Object.Instantiate(prefab);        instance.gameObject.SetActive(true);
        InstanceToPrefab[instance] = prefabGo;

#if OBJECT_POOLING_TRACK_PERFORMANCE
        // ======= PERFORMANCE TRACKING =======
        if (!PoolStatistics.TryGetValue(prefabGo, out var stats))
        {
            stats = new PoolStats();
            PoolStatistics[prefabGo] = stats;
        }

        stats.TotalRequests++;
        stats.CurrentActiveCount++;
        stats.PeakActiveCount = Mathf.Max(stats.PeakActiveCount, stats.CurrentActiveCount);

        if (wasPoolHit)
        {
            stats.PoolHits++;
            ConsoleLogger.Log(
                $"[ObjectPooling] Pool HIT for {prefabGo.name} - Pool size: {pool.Count}, Active: {stats.CurrentActiveCount}");
        }
        else
        {
            stats.PoolMisses++;
            stats.TotalCreated++;
            ConsoleLogger.LogWarning(
                $"[ObjectPooling] Pool MISS for {prefabGo.name} - Created new instance. Active: {stats.CurrentActiveCount}");
        }

        PoolStatistics[prefabGo] = stats;
#endif

        return instance;
    }


    public static T GetObject<T>(T prefab, Transform parent) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        return instance;
    }

    public static T GetObject<T>(T prefab, Vector3 position) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.position = position;
        return instance;
    }

    public static T GetObject<T>(T prefab, Quaternion rotation) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.rotation = rotation;
        return instance;
    }

    public static T GetObject<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        return instance;
    }

    public static T GetObject<T>(T prefab, Vector2 position) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.position = new Vector3(position.x, position.y, instance.transform.position.z);
        return instance;
    }

    public static T GetObject<T>(T prefab, Transform parent, Vector3 localPosition) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        return instance;
    }

    public static T GetObject<T>(T prefab, Transform parent, Vector3 localPosition, Quaternion localRotation)
        where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = localRotation;
        return instance;
    }

    public static T GetObject<T>(T prefab, Transform parent, Vector2 localPosition) where T : Component
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        return instance;
    }

    // ======= GAME OBJECT VERSIONS =======

    public static void Prewarm(GameObject prefab, int count)
    {
        var component = prefab.GetComponent<Component>();
        if (component == null)
        {
            ConsoleLogger.LogError("[ObjectPooling] GameObject has no Component to pool.");
            return;
        }

        Prewarm(component, count);
    }

    public static GameObject GetObject(GameObject prefab)
    {
        var component = prefab.GetComponent<Component>();
        if (component == null)
        {
            ConsoleLogger.LogError("[ObjectPooling] GameObject has no Component to pool.");
            return null;
        }

        var instance = GetObject(component);
        return instance.gameObject;
    }

    public static GameObject GetObject(GameObject prefab, Transform parent)
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Vector3 position)
    {
        var instance = GetObject(prefab);
        instance.transform.position = position;
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Quaternion rotation)
    {
        var instance = GetObject(prefab);
        instance.transform.rotation = rotation;
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var instance = GetObject(prefab);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Vector2 position)
    {
        var instance = GetObject(prefab);
        instance.transform.position = new Vector3(position.x, position.y, instance.transform.position.z);
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Transform parent, Vector3 localPosition)
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Transform parent, Vector3 localPosition,
        Quaternion localRotation)
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = localRotation;
        return instance;
    }

    public static GameObject GetObject(GameObject prefab, Transform parent, Vector2 localPosition)
    {
        var instance = GetObject(prefab);
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        return instance;
    }

    // ======= RETURN & CLEAR =======
    public static void ReturnObject(Component instance)
    {
        if (!InstanceToPrefab.TryGetValue(instance, out var prefabGo))
        {
            ConsoleLogger.LogWarning($"[ObjectPooling] Tried to return unknown object: {instance.name}");
            Object.Destroy(instance.gameObject);
            return;
        }        instance.gameObject.SetActive(false);
        Pools[prefabGo].Enqueue(instance);

#if OBJECT_POOLING_TRACK_PERFORMANCE
        // ======= PERFORMANCE TRACKING =======
        if (PoolStatistics.TryGetValue(prefabGo, out var stats))
        {
            stats.TotalReturned++;
            stats.CurrentActiveCount--;
            PoolStatistics[prefabGo] = stats;

            ConsoleLogger.Log(
                $"[ObjectPooling] Object returned to pool for {prefabGo.name} - Pool size: {Pools[prefabGo].Count}, Active: {stats.CurrentActiveCount}");
        }
#endif
    }

    public static void ReturnObject(GameObject obj)
    {
        var component = obj.GetComponent<Component>();
        if (component == null)
        {
            ConsoleLogger.LogWarning("[ObjectPooling] Tried to return a GameObject with no Component.");
            Object.Destroy(obj);
            return;
        }

        ReturnObject(component);
    }

    public static void ClearPool<T>(T prefab) where T : Component
    {
        var prefabGo = prefab.gameObject;

        if (!Pools.TryGetValue(prefabGo, out var pool))
            return;

        while (pool.Count > 0)
        {
            var instance = pool.Dequeue();
            InstanceToPrefab.Remove(instance);
            Object.Destroy(instance.gameObject);
        }        Pools.Remove(prefabGo);

#if OBJECT_POOLING_TRACK_PERFORMANCE
        // ======= PERFORMANCE TRACKING =======
        if (PoolStatistics.TryGetValue(prefabGo, out var stats))
        {
            ConsoleLogger.Log(
                $"[ObjectPooling] Pool cleared for {prefabGo.name} - Final stats: Hit Rate: {stats.HitRate:P2}, Total Created: {stats.TotalCreated}");
            PoolStatistics.Remove(prefabGo);
        }
#endif
    }    public static void ClearAllPools()
    {
#if OBJECT_POOLING_TRACK_PERFORMANCE
        // ======= PERFORMANCE TRACKING =======
        ConsoleLogger.Log("[ObjectPooling] Clearing all pools - Final summary:");
        foreach (var kvp in PoolStatistics)
        {
            var prefabGo = kvp.Key;
            var stats = kvp.Value;
            ConsoleLogger.Log(
                $"  {prefabGo.name}: Hit Rate: {stats.HitRate:P2}, Total Created: {stats.TotalCreated}, Peak Active: {stats.PeakActiveCount}");
        }
#endif

        foreach (var pool in Pools.Values)
        {
            while (pool.Count > 0)
            {
                var instance = pool.Dequeue();
                InstanceToPrefab.Remove(instance);
                Object.Destroy(instance.gameObject);
            }
        }

        Pools.Clear();
        InstanceToPrefab.Clear();
#if OBJECT_POOLING_TRACK_PERFORMANCE
        PoolStatistics.Clear();
#endif
    }

    public static int GetPoolSize<T>(T prefab) where T : Component
    {
        return Pools.TryGetValue(prefab.gameObject, out var pool) ? pool.Count : 0;
    }

    public static int GetPoolSize(GameObject prefab)
    {
        return Pools.TryGetValue(prefab, out var pool) ? pool.Count : 0;
    }    // ======= DEBUG & PERFORMANCE TRACKING =======

#if OBJECT_POOLING_TRACK_PERFORMANCE
    public static void LogPoolStats(GameObject prefab)
    {
        if (PoolStatistics.TryGetValue(prefab, out var stats))
        {
            ConsoleLogger.Log($"[ObjectPooling] Pool stats for {prefab.name}:\n" +
                              $"  Total Created: {stats.TotalCreated}\n" +
                              $"  Total Returned: {stats.TotalReturned}\n" +
                              $"  Total Requests: {stats.TotalRequests}\n" +
                              $"  Pool Hits: {stats.PoolHits}\n" +
                              $"  Pool Misses: {stats.PoolMisses}\n" +
                              $"  Peak Active Count: {stats.PeakActiveCount}\n" +
                              $"  Current Active Count: {stats.CurrentActiveCount}\n" +
                              $"  Hit Rate: {stats.HitRate:P2}\n" +
                              $"  Miss Rate: {stats.MissRate:P2}");
        }
        else
        {
            ConsoleLogger.LogWarning($"[ObjectPooling] No stats found for {prefab.name}.");
        }
    }

    public static void LogPoolStats<T>(T prefab) where T : Component
    {
        LogPoolStats(prefab.gameObject);
    }

    public static void LogAllPoolStats()
    {
        ConsoleLogger.Log("[ObjectPooling] === ALL POOL STATISTICS ===");
        foreach (var kvp in PoolStatistics)
        {
            var prefabGo = kvp.Key;
            var stats = kvp.Value;
            var poolSize = Pools.TryGetValue(prefabGo, out var pool) ? pool.Count : 0;

            ConsoleLogger.Log($"{prefabGo.name}: Pool Size: {poolSize}, Active: {stats.CurrentActiveCount}, " +
                              $"Hit Rate: {stats.HitRate:P2}, Total Created: {stats.TotalCreated}");
        }
    }

    public static int GetActiveCount(GameObject prefab)
    {
        return PoolStatistics.TryGetValue(prefab, out var stats) ? stats.CurrentActiveCount : 0;
    }

    public static int GetActiveCount<T>(T prefab) where T : Component
    {
        return GetActiveCount(prefab.gameObject);
    }

    public static float GetHitRate(GameObject prefab)
    {
        return PoolStatistics.TryGetValue(prefab, out var stats) ? stats.HitRate : 0f;
    }

    public static float GetHitRate<T>(T prefab) where T : Component
    {
        return GetHitRate(prefab.gameObject);
    }
#else
    // Stub methods when performance tracking is disabled
    public static void LogPoolStats(GameObject prefab) { }
    public static void LogPoolStats<T>(T prefab) where T : Component { }
    public static void LogAllPoolStats() { }
    public static int GetActiveCount(GameObject prefab) { return 0; }
    public static int GetActiveCount<T>(T prefab) where T : Component { return 0; }
    public static float GetHitRate(GameObject prefab) { return 0f; }
    public static float GetHitRate<T>(T prefab) where T : Component { return 0f; }
#endif
}