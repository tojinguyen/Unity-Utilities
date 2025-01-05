using System;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

public static class AddressablesHelper
{
    private class FeatureAddressableCache
    {
        public Dictionary<AssetReference, AsyncOperationHandle> LoadedAsset;
        public Dictionary<AssetReference, AsyncOperationHandle> LoadingAsset;
    }

    private const int DEFAULT_FEATURE_AMOUNT = 5;
    private const int DEFAULT_EXPECTED_LOAD_ASSETS = 20;
    private const string DEFAULT_FEATURE_NAME = "default_feature_cache";

    private static Dictionary<AssetReference, int> _refCountAssetRef = new(20);

    private static Dictionary<string, FeatureAddressableCache> _featureAddressableCaches;

    static AddressablesHelper()
    {
        _featureAddressableCaches = new Dictionary<string, FeatureAddressableCache>(DEFAULT_FEATURE_AMOUNT);
    }

    /// <summary>
    /// Unload asset handle by asset reference
    /// </summary>
    /// <param name="assetRef">Asset Reference</param>
    public static void TryUnloadAssetHandle(AssetReference assetRef)
    {
        if (assetRef == null)
            return;

        if (_refCountAssetRef.TryGetValue(assetRef, out var refCount))
            if (refCount > 1)
            {
                _refCountAssetRef[assetRef] -= 1;
                return;
            }
        _refCountAssetRef.Remove(assetRef);
        // Find feature cache
        foreach (var featureCache in _featureAddressableCaches.Values)
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
        if (!_featureAddressableCaches.TryGetValue(featureName, out var featureCache))
            return;
        foreach (var element in featureCache.LoadingAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            await handle;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (_refCountAssetRef.TryGetValue(asset, out var refCount))
            {
                _refCountAssetRef[asset] -= 1;
                if (_refCountAssetRef[asset] <= 0)
                    _refCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadingAsset.Clear();
        
        foreach (var element in featureCache.LoadedAsset)
        {
            var asset = element.Key;
            var handle = element.Value;
            if (handle.IsValid())
                Addressables.Release(handle);

            if (_refCountAssetRef.TryGetValue(asset, out var refCount))
            {
                _refCountAssetRef[asset] -= 1;
                if (_refCountAssetRef[asset] <= 0)
                    _refCountAssetRef.Remove(asset);
            }
        }
        featureCache.LoadedAsset.Clear();
    }
    
    /// <summary>
    /// Remove all asset
    /// </summary>
    public static async UniTaskVoid UnloadAsyncAllAddressable()
    {
        foreach (var featureCache in _featureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadingAsset.Values)
            {
                await handle;
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadingAsset.Clear();
        }
        
        foreach (var featureCache in _featureAddressableCaches.Values)
        {
            foreach (var handle in featureCache.LoadedAsset.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            featureCache.LoadedAsset.Clear();
        }
        
        _featureAddressableCaches.Clear();
        _refCountAssetRef.Clear();
    }
    
    /// <summary>
    /// Load asset async by asset reference
    /// </summary>
    /// <param name="assetRef">Asset Reference</param>
    /// <param name="featureName">Feature Name</param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static async UniTask<TObject> GetAssetAsync<TObject>(AssetReference assetRef, string featureName = DEFAULT_FEATURE_NAME)
        where TObject : Object
    {
        try
        {
            var isFeatureExists = _featureAddressableCaches.TryGetValue(featureName, out var featureCache);
            if (!isFeatureExists)
            {
                featureCache = new FeatureAddressableCache
                {
                    LoadedAsset = new Dictionary<AssetReference, AsyncOperationHandle>(DEFAULT_EXPECTED_LOAD_ASSETS)
                };
                _featureAddressableCaches.Add(featureName, featureCache);
            }
            else
            {
                // Check loaded asset
                var isHandleExists = featureCache.LoadedAsset.TryGetValue(assetRef, out var handle);
                if (isHandleExists)
                {
                    if (handle.IsValid())
                    {
                        _refCountAssetRef[assetRef]++;
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
                        if (_refCountAssetRef.TryGetValue(assetRef, out var value))
                            _refCountAssetRef[assetRef]++;
                        else
                            _refCountAssetRef.Add(assetRef, 1);
                        return handle.Result as TObject;
                    }
                }
            }

            _refCountAssetRef.Add(assetRef, 1);
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