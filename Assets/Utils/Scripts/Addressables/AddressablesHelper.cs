using System;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

public static class AddressablesHelper
{
    private class _featureAddressableCache
    {
        public Dictionary<AssetReference, AsyncOperationHandle> LoadedAsset;
        public Dictionary<AssetReference, AsyncOperationHandle> LoadingAsset;
    }

    private const int DefaultFeatureAmount = 5;
    private const int DefaultExpectedLoadAssets = 20;
    private const string DefaultFeatureName = "default_feature_cache";

    private static readonly Dictionary<AssetReference, int> RefCountAssetRef = new(20);

    private static readonly Dictionary<string, _featureAddressableCache> FeatureAddressableCaches;

    static AddressablesHelper()
    {
        FeatureAddressableCaches = new Dictionary<string, _featureAddressableCache>(DefaultFeatureAmount);
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
    /// Remove all asset in feature cache
    /// </summary>
    /// <param name="featureName">Feature Name</param>
    public static async UniTaskVoid UnloadAsyncAllAddressableInFeature(string featureName)
    {
        if (!FeatureAddressableCaches.TryGetValue(featureName, out var featureCache))
            return;
        foreach (var element in featureCache.LoadingAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            await handle;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountAssetRef.TryGetValue(asset, out var refCount))
            {
                RefCountAssetRef[asset] -= 1;
                if (RefCountAssetRef[asset] <= 0)
                    RefCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadingAsset.Clear();
        
        foreach (var element in featureCache.LoadedAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (RefCountAssetRef.TryGetValue(asset, out var refCount))
            {
                RefCountAssetRef[asset] -= 1;
                if (RefCountAssetRef[asset] <= 0)
                    RefCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadedAsset.Clear();
    }
    
    /// <summary>
    /// Remove all asset
    /// </summary>
    public static async UniTaskVoid UnloadAsyncAllAddressable()
    {
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
        
        foreach (var featureCache in FeatureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadedAsset.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadedAsset.Clear();
        }
        
        FeatureAddressableCaches.Clear();
        RefCountAssetRef.Clear();
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
                featureCache = new _featureAddressableCache
                {
                    LoadedAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets),
                    LoadingAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DefaultExpectedLoadAssets)
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
                        if (RefCountAssetRef.TryGetValue(assetRef, out var value))
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
}