using System.Collections.Generic;
using UnityEngine;

public static class ObjectPooling
{
    private static readonly Dictionary<GameObject, Queue<Component>> Pools = new();
    private static readonly Dictionary<Component, GameObject> InstanceToPrefab = new();

    // ======= COMPONENT VERSIONS =======

    public static void Prewarm<T>(T prefab, int count) where T : Component
    {
        var prefabGo = prefab.gameObject;

        if (!Pools.TryGetValue(prefabGo, out var pool))
        {
            pool = new Queue<Component>();
            Pools[prefabGo] = pool;
        }

        for (var i = 0; i < count; i++)
        {
            var instance = Object.Instantiate(prefab);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
            InstanceToPrefab[instance] = prefabGo;
        }
    }

    public static T GetObject<T>(T prefab) where T : Component
    {
        var prefabGo = prefab.gameObject;

        if (!Pools.TryGetValue(prefabGo, out var pool))
        {
            pool = new Queue<Component>();
            Pools[prefabGo] = pool;
        }

        var instance = pool.Count > 0
            ? (T)pool.Dequeue()
            : Object.Instantiate(prefab);

        instance.gameObject.SetActive(true);
        InstanceToPrefab[instance] = prefabGo;

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

    // ======= RETURN & CLEAR =======

    public static void ReturnObject(Component instance)
    {
        if (!InstanceToPrefab.TryGetValue(instance, out var prefabGo))
        {
            ConsoleLogger.LogWarning($"[ObjectPooling] Tried to return unknown object: {instance.name}");
            Object.Destroy(instance.gameObject);
            return;
        }

        instance.gameObject.SetActive(false);
        Pools[prefabGo].Enqueue(instance);
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
        }

        Pools.Remove(prefabGo);
    }

    public static void ClearAllPools()
    {
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
    }

    public static int GetPoolSize<T>(T prefab) where T : Component
    {
        return Pools.TryGetValue(prefab.gameObject, out var pool) ? pool.Count : 0;
    }

    public static int GetPoolSize(GameObject prefab)
    {
        return Pools.TryGetValue(prefab, out var pool) ? pool.Count : 0;
    }
}