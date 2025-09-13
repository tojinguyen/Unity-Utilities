using System;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

public static class AddressableHelper
{
    private class FeatureAddressableCache
    {
        public Dictionary<AssetReference, AsyncOperationHandle> LoadedAsset;
        public Dictionary<AssetReference, AsyncOperationHandle> LoadingAsset;
        public Dictionary<string, AsyncOperationHandle> LoadedAssetByKey;
        public Dictionary<string, AsyncOperationHandle> LoadingAssetByKey;
    }

    private const int DefaultFeatureAmount = 5;
    private const int DefaultExpectedLoadAssets = 20;
    private const string DefaultFeatureName = "default_feature_cache";

    private static readonly Dictionary<AssetReference, int> RefCountAssetRef = new(20);
    private static readonly Dictionary<string, int> RefCountStringKey = new(20);

    private static readonly Dictionary<string, FeatureAddressableCache> FeatureAddressableCaches;

    static AddressableHelper()
    {
        FeatureAddressableCaches = new Dictionary<string, FeatureAddressableCache>(DefaultFeatureAmount);
    }

    /// <summary>
    /// Unload asset handle by asset reference
    /// </summary>
    /// <param name="assetRef">Asset Reference</param>
    public static void TryUnloadAssetHandle(AssetReference assetRef)
    {
        if (assetRef == null)
            return;

        if (RefCountAssetRef.TryGetValue(assetRef, out var refCount))
            if (refCount > 1)
            {
                RefCountAssetRef[assetRef] -= 1;
                return;
            }
        RefCountAssetRef.Remove(assetRef);
        // Find feature cache
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            if (!featureCache.LoadedAsset.TryGetValue(assetRef, out var handle)) 
                continue;
            Addressables.Release(handle);
            featureCache.LoadedAsset.Remove(assetRef);
            return;
        }
    }
    
    /// <summary>
    /// Unload asset handle by string key
    /// </summary>
    /// <param name="key">Asset Key</param>
    public static void TryUnloadAssetHandle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (RefCountStringKey.TryGetValue(key, out var refCount))
            if (refCount > 1)
            {
                RefCountStringKey[key] -= 1;
                return;
            }
        RefCountStringKey.Remove(key);
        // Find feature cache
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            if (!featureCache.LoadedAssetByKey.TryGetValue(key, out var handle)) 
                continue;
            Addressables.Release(handle);
            featureCache.LoadedAssetByKey.Remove(key);
            return;
        }
    }

    /// <summary>
    /// Remove all asset in feature cache
    /// </summary>
    /// <param name="featureName">Feature Name</param>
    public static async UniTaskVoid UnloadAsyncAllAddressableInFeature(string featureName)
    {
        if (!FeatureAddressableCaches.TryGetValue(featureName, out var featureCache))
            return;
            
        // Handle AssetReference loading assets
        foreach (var element in featureCache.LoadingAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            await handle;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountAssetRef.ContainsKey(asset))
            {
                RefCountAssetRef[asset] -= 1;
                if (RefCountAssetRef[asset] <= 0)
                    RefCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadingAsset.Clear();
        
        // Handle AssetReference loaded assets
        foreach (var element in featureCache.LoadedAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountAssetRef.ContainsKey(asset))
            {
                RefCountAssetRef[asset] -= 1;
                if (RefCountAssetRef[asset] <= 0)
                    RefCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadedAsset.Clear();
        
        // Handle string key loading assets
        foreach (var element in featureCache.LoadingAssetByKey)
        {
            var key = element.Key;
            var handle = element.Value;
            await handle;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountStringKey.ContainsKey(key))
            {
                RefCountStringKey[key] -= 1;
                if (RefCountStringKey[key] <= 0)
                    RefCountStringKey.Remove(key);
            }
        }
        featureCache.LoadingAssetByKey.Clear();
        
        // Handle string key loaded assets
        foreach (var element in featureCache.LoadedAssetByKey)
        {
            var key = element.Key;
            var handle = element.Value;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountStringKey.ContainsKey(key))
            {
                RefCountStringKey[key] -= 1;
                if (RefCountStringKey[key] <= 0)
                    RefCountStringKey.Remove(key);
            }
        }
        featureCache.LoadedAssetByKey.Clear();
    }
    
    /// <summary>
    /// Remove all asset
    /// </summary>
    public static async UniTaskVoid UnloadAsyncAllAddressable()
    {
        // Handle AssetReference loading assets
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadingAsset.Values)
            {
                await handle;
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadingAsset.Clear();
        }
        
        // Handle AssetReference loaded assets
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadedAsset.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadedAsset.Clear();
        }
        
        // Handle string key loading assets
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadingAssetByKey.Values)
            {
                await handle;
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadingAssetByKey.Clear();
        }
        
        // Handle string key loaded assets
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadedAssetByKey.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadedAssetByKey.Clear();
        }
        
        FeatureAddressableCaches.Clear();
        RefCountAssetRef.Clear();
        RefCountStringKey.Clear();
    }
    
    /// <summary>
    /// Load asset async by asset reference
    /// </summary>
    /// <param name="assetRef">Asset Reference</param>
    /// <param name="featureName">Feature Name</param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static async UniTask<TObject> GetAssetAsync<TObject>(AssetReference assetRef, string featureName = DefaultFeatureName)
        where TObject : Object
    {
        try
        {
            var isFeatureExists = FeatureAddressableCaches.TryGetValue(featureName, out var featureCache);
            if (!isFeatureExists)
            {
                featureCache = new FeatureAddressableCache
                {
                    LoadedAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadingAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadedAssetByKey = new Dictionary<string, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadingAssetByKey = new Dictionary<string, AsyncOperationHandle>(DefaultExpectedLoadAssets)
                };
                FeatureAddressableCaches.Add(featureName, featureCache);
            }
            else
            {
                // Check loaded asset
                var isHandleExists = featureCache.LoadedAsset.TryGetValue(assetRef, out var handle);
                if (isHandleExists)
                {
                    if (handle.IsValid())
                    {
                        RefCountAssetRef[assetRef]++;
                        return handle.Result as TObject;
                    }

                    featureCache.LoadedAsset.Remove(assetRef);
                }

                // Check loading asset
                var isHandleLoading = featureCache.LoadingAsset.TryGetValue(assetRef, out handle);

                if (isHandleLoading)
                {
                    if (!handle.IsValid())
                        featureCache.LoadingAsset.Remove(assetRef);
                    else
                    {
                        await handle;
                        if (RefCountAssetRef.ContainsKey(assetRef))
                            RefCountAssetRef[assetRef]++;
                        else
                            RefCountAssetRef.Add(assetRef, 1);
                        return handle.Result as TObject;
                    }
                }
            }

            RefCountAssetRef.Add(assetRef, 1);
            var handleAssetAsync = Addressables.LoadAssetAsync<TObject>(assetRef);
            featureCache.LoadingAsset.Add(assetRef, handleAssetAsync);
            await handleAssetAsync;
            featureCache.LoadedAsset.Add(assetRef, handleAssetAsync);
            featureCache.LoadingAsset.Remove(assetRef);
            if (handleAssetAsync.Status != AsyncOperationStatus.Succeeded)
                return null;
            var obj = handleAssetAsync.Result;
            return obj;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"GetAssetAsync with name {assetRef.AssetGUID} fail: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Load asset async by string key
    /// </summary>
    /// <param name="key">Asset Key</param>
    /// <param name="featureName">Feature Name</param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static async UniTask<TObject> GetAssetAsync<TObject>(string key, string featureName = DefaultFeatureName)
        where TObject : Object
    {
        try
        {
            var isFeatureExists = FeatureAddressableCaches.TryGetValue(featureName, out var featureCache);
            if (!isFeatureExists)
            {
                featureCache = new FeatureAddressableCache
                {
                    LoadedAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadingAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadedAssetByKey = new Dictionary<string, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadingAssetByKey = new Dictionary<string, AsyncOperationHandle>(DefaultExpectedLoadAssets)
                };
                FeatureAddressableCaches.Add(featureName, featureCache);
            }
            else
            {
                // Check loaded asset by key
                var isHandleExists = featureCache.LoadedAssetByKey.TryGetValue(key, out var handle);
                if (isHandleExists)
                {
                    if (handle.IsValid())
                    {
                        RefCountStringKey[key]++;
                        return handle.Result as TObject;
                    }

                    featureCache.LoadedAssetByKey.Remove(key);
                }

                // Check loading asset by key
                var isHandleLoading = featureCache.LoadingAssetByKey.TryGetValue(key, out handle);

                if (isHandleLoading)
                {
                    if (!handle.IsValid())
                        featureCache.LoadingAssetByKey.Remove(key);
                    else
                    {
                        await handle;
                        if (RefCountStringKey.ContainsKey(key))
                            RefCountStringKey[key]++;
                        else
                            RefCountStringKey.Add(key, 1);
                        return handle.Result as TObject;
                    }
                }
            }

            RefCountStringKey.Add(key, 1);
            var handleAssetAsync = Addressables.LoadAssetAsync<TObject>(key);
            featureCache.LoadingAssetByKey.Add(key, handleAssetAsync);
            await handleAssetAsync;
            featureCache.LoadedAssetByKey.Add(key, handleAssetAsync);
            featureCache.LoadingAssetByKey.Remove(key);
            if (handleAssetAsync.Status != AsyncOperationStatus.Succeeded)
                return null;
            var obj = handleAssetAsync.Result;
            return obj;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"GetAssetAsync with key {key} fail: {ex.Message}");
            return null;
        }
    }
}
