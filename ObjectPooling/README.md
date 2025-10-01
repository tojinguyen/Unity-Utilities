# Object Pooling System

This utility provides a robust and efficient object pooling system for Unity, designed to improve performance by reusing `GameObjects` and `Components` instead of frequently instantiating and destroying them. It includes support for both standard prefabs and Addressable Assets.

## Features

### Standard Object Pooling (`ObjectPooling.cs`)

-   **Generic Pooling**: Supports pooling of any `Component` or `GameObject`.
-   **Prewarming**: Initialize the pool with a specified number of instances in advance.
-   **Flexible `GetObject` Methods**: Retrieve objects with various position, rotation, and parent options.
-   **Return to Pool**: Easily return objects to the pool for reuse.
-   **Clear Pool**: Clear all instances of a specific prefab from the pool or clear all pools.
-   **Performance Tracking (Optional)**: Conditional compilation (`OBJECT_POOLING_TRACK_PERFORMANCE`) for detailed statistics on pool hits, misses, and active counts.

### Addressable Object Pooling (`AddressableObjectPooling.cs`)

-   **Addressable Asset Support**: Seamlessly integrates with Unity's Addressable Asset System.
-   **Async Operations**: All Addressable-related operations are asynchronous using `UniTask`.
-   **Prewarming**: Preload and pool Addressable Assets.
-   **Automatic Unloading**: Handles unloading of Addressable Assets when pools are cleared.

## How to Use

### Standard Object Pooling

```csharp
using UnityEngine;
using Tirex.Utils.ObjectPooling;

public class StandardPoolingExample : MonoBehaviour
{
    public GameObject bulletPrefab;

    void Start()
    {
        // Prewarm the pool with 10 bullets
        ObjectPooling.Prewarm(bulletPrefab, 10);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get a bullet from the pool
            GameObject bullet = ObjectPooling.GetObject(bulletPrefab, transform.position, Quaternion.identity);
            // Do something with the bullet
            // ...
            // After use, return it to the pool
            // ObjectPooling.ReturnObject(bullet);
        }
    }

    void OnDestroy()
    {
        // Clear the pool when no longer needed
        ObjectPooling.ClearPool(bulletPrefab);
    }
}
```

### Addressable Object Pooling

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Tirex.Utils.ObjectPooling;

public class AddressablePoolingExample : MonoBehaviour
{
    public AssetReference bulletAssetReference;

    async UniTaskVoid Start()
    {
        // Prewarm the pool with 10 bullets from Addressables
        await AddressableObjectPooling.Prewarm(bulletAssetReference, 10);
    }

    async UniTaskVoid ShootBullet()
    {
        // Get a bullet from the Addressable pool
        GameObject bullet = await AddressableObjectPooling.GetObject(bulletAssetReference, transform.position);
        // Do something with the bullet
        // ...
        // After use, return it to the pool
        AddressableObjectPooling.ReturnObject(bullet);
    }

    void OnDestroy()
    {
        // Clear the pool when no longer needed
        AddressableObjectPooling.ClearPool(bulletAssetReference);
    }
}
```
