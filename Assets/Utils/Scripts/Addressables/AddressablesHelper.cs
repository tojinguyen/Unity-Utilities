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
        public Dictionary<string, AsyncOperationHandle> LoadedAsset;
        public Dictionary<string, AsyncOperationHandle> LoadingAsset;
    }

    private const int DEFAULT_FEATURE_AMOUNT = 5;
    private const int DEFAULT_EXPECTED_LOAD_ASSETS = 20;
    private const string STR_FORMAT_GUID = "{0}{1}";
    private const string DEFAULT_FEATURE_NAME = "default_feature_cache";

    private static Dictionary<string, int> _refCountAssetRef = new(20);

    private static Dictionary<string, FeatureAddressableCache> _featureAddressableCaches;

    static AddressablesHelper()
    {
        _featureAddressableCaches = new Dictionary<string, FeatureAddressableCache>(DEFAULT_FEATURE_AMOUNT);
    }

    public static async UniTask<TObject> GetAssetAsync<TObject>(string assetName, string featureName = DEFAULT_FEATURE_NAME)
        where TObject : Object
    {
        try
        {
            var isFeatureExists = _featureAddressableCaches.TryGetValue(featureName, out var featureCache);
            if (!isFeatureExists)
            {
                featureCache = new FeatureAddressableCache
                {
                    LoadedAsset = new Dictionary<string, AsyncOperationHandle>(DEFAULT_EXPECTED_LOAD_ASSETS)
                };
                _featureAddressableCaches.Add(featureName, featureCache);
            }
            else
            {
                // Check loaded asset
                var isHandleExists = featureCache.LoadedAsset.TryGetValue(assetName, out var handle);
                if (isHandleExists)
                {
                    if (handle.IsValid())
                    {
                        _refCountAssetRef[assetName]++;
                        return handle.Result as TObject;
                    }

                    featureCache.LoadedAsset.Remove(assetName);
                }

                // Check loading asset
                var isHandleLoading = featureCache.LoadingAsset.TryGetValue(assetName, out handle);

                if (isHandleLoading)
                {
                    if (!handle.IsValid())
                        featureCache.LoadingAsset.Remove(assetName);
                    else
                    {
                        await handle;
                        if (_refCountAssetRef.TryGetValue(assetName, out var value))
                            _refCountAssetRef[assetName]++;
                        else
                            _refCountAssetRef.Add(assetName, 1);
                        return handle.Result as TObject;
                    }
                }
            }

            _refCountAssetRef.Add(assetName, 1);
            var handleAssetAsync = Addressables.LoadAssetAsync<TObject>(assetName);
            featureCache.LoadingAsset.Add(assetName, handleAssetAsync);
            await handleAssetAsync;
            featureCache.LoadedAsset.Add(assetName, handleAssetAsync);
            featureCache.LoadingAsset.Remove(assetName);
            if (handleAssetAsync.Status != AsyncOperationStatus.Succeeded)
                return null;
            var obj = handleAssetAsync.Result;
            return obj;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"GetAssetAsync with name {assetName} fail: {ex.Message}");
            return null;
        }
    }

    public static async UniTask<TObject> GetAssetAsync<TObject>(AssetReference assetRef, string featureName = DEFAULT_FEATURE_NAME) where TObject : Object
    {
        try
        {
            var assetName = GetGuidKeyFromAssetRef(assetRef);
            var obj = await GetAssetAsync<TObject>(assetName, featureName);
            return obj;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"GetAssetAsync asset {assetRef} with guid {GetGuidKeyFromAssetRef(assetRef)} fail: {ex.Message}");
            return null;
        }
    }

    public static void UnloadAssetHandle(AssetReference assetRef)
    {
        UnloadAssetHandle(GetGuidKeyFromAssetRef(assetRef));
    }
    
    public static void TryUnloadAssetHandle(AssetReference assetRef)
    {
        if (assetRef == null)
            return;

        var guid = GetGuidKeyFromAssetRef(assetRef);
        TryUnloadAssetHandle(guid);
    }
    
    public static void TryUnloadAssetHandle(string assetName)
    {
        if (_refCountAssetRef.TryGetValue(assetName, out var refCount))
            if (refCount > 1)
            {
                _refCountAssetRef[assetName] -= 1;
                return;
            }

        _refCountAssetRef.Remove(assetName);
        UnloadAssetHandle(assetName);
    }
    
    public static void UnloadAsyncAllAddressableInFeature(string featureName)
    {
        if (!_featureAddressableCaches.TryGetValue(featureName, out var featureCache))
            return;
        foreach (var element in featureCache.LoadingAsset)
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
    
    public static async UniTask UnloadAsyncAllAddressable()
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
    
    public static string GetGuidKeyFromAssetRef(AssetReference assetRef)
    {
        return string.Format(STR_FORMAT_GUID, assetRef.AssetGUID, assetRef.SubObjectName);
    }
    
    private static void UnloadAssetHandle(string guid)
    {
        if (!_featureAddressableCaches.TryGetValue(guid, out var featureCache))
            return;
        if (featureCache.LoadedAsset.TryGetValue(guid, out var handle))
        {
            Addressables.Release(handle);
            featureCache.LoadedAsset.Remove(guid);
        }
    }
}