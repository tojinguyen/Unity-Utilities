using System;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

public class AddressablesHelper
{
    private const string STR_FORMAT_GUID = "{0}{1}";

    private Dictionary<string, int> _refCountAssetRef = new(20);

    private Dictionary<string, AsyncOperationHandle> _loadingAssets;
    private Dictionary<string, AsyncOperationHandle> _loadedAssets;
    
    public AddressablesHelper(int expectedLoadingAssets = 5, int expectedLoadedAssets = 20)
    {
        _loadingAssets = new Dictionary<string, AsyncOperationHandle>(expectedLoadingAssets);
        _loadedAssets = new Dictionary<string, AsyncOperationHandle>(expectedLoadedAssets);
    }

    public AddressablesHelper()
    {
        _loadingAssets = new Dictionary<string, AsyncOperationHandle>(5);
        _loadedAssets = new Dictionary<string, AsyncOperationHandle>(20);
    }

    public async UniTask<TObject> GetAssetAsync<TObject>(string name) where TObject : Object
    {
        try
        {
            if (_loadedAssets.TryGetValue(name, out var handle))
            {
                _refCountAssetRef[name]++;
                if (handle.IsValid())
                {
                    return handle.Result as TObject;
                }

                _loadedAssets.Remove(name);
            }

            if (_loadingAssets.TryGetValue(name, out handle))
            {
                if (_refCountAssetRef.TryGetValue(name, out var value))
                {
                    _refCountAssetRef[name]++;
                }
                else
                {
                    _refCountAssetRef.Add(name, 1);
                }

                if (!handle.IsValid())
                    _loadingAssets.Remove(name);
                else
                {
                    await handle;
                    return handle.Result as TObject;
                }
            }
            _refCountAssetRef.Add(name, 1);
            handle = Addressables.LoadAssetAsync<TObject>(name);
            _loadingAssets.Add(name, handle);
            await handle;
            _loadedAssets.Add(name, handle);
            _loadingAssets.Remove(name);
            if (handle.Status != AsyncOperationStatus.Succeeded) 
                return null;
            var go = handle.Result;

            return go as TObject;

        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError("GetAssetAsync with name fail: " + ex.Message);
            return null;
        }
    }

    public async UniTaskVoid GetAssetAsync<TObject>(string name, Action<TObject> result) where TObject : Object
    {
        var resource = await GetAssetAsync<TObject>(name);

        result?.Invoke(resource);
    }
    
    public async UniTask<TObject> GetAssetAsync<TObject>(AssetReference assetRef) where TObject : Object
    {
        try
        {
            var guid = GetGuidKeyFromAssetRef(assetRef);
            if (_loadedAssets.TryGetValue(guid, out var handle))
            {
                _refCountAssetRef[guid]++;
                if (handle.IsValid())
                    return handle.Result as TObject;
                _loadedAssets.Remove(guid);
            }
            if (_loadingAssets.TryGetValue(guid, out handle))
            {
                if (_refCountAssetRef.TryGetValue(guid, out var value))
                {
                    _refCountAssetRef[guid]++;
                }
                else
                {
                    _refCountAssetRef.Add(guid, 1);
                }

                if (!handle.IsValid())
                    _loadingAssets.Remove(guid);
                else
                {
                    await handle.Task;
                    return handle.Result as TObject;
                }
            }
            _refCountAssetRef.Add(guid, 1);
            handle = Addressables.LoadAssetAsync<TObject>(assetRef);
            _loadingAssets.Add(guid, handle);
            await handle;
            _loadedAssets.Add(guid, handle);
            _loadingAssets.Remove(guid);
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var go = handle.Result;
                return go as TObject;
            }

            return null;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError("GetAssetAsync fail: " + ex.Message);
            return null;
        }

    }

    public async UniTaskVoid GetAssetAsync<TObject>(AssetReference assetRef, Action<TObject> resultCallback) where TObject : Object
    {
        var result = await GetAssetAsync<TObject>(assetRef);

        resultCallback?.Invoke(result);
    }

    public void UnloadAssetHandle(AssetReference assetRef)
    {
        UnloadAssetHandle(GetGuidKeyFromAssetRef(assetRef));
    }

    public void UnloadAssetHandle(string guild)
    {
        if (!_loadedAssets.TryGetValue(guild, out var handle))
            return;
        Addressables.Release(handle);
        _loadedAssets.Remove(guild);
    }

    public void TryUnloadAssetHandle(AssetReference assetRef)
    {
        if (assetRef == null)
            return;

        var guid = GetGuidKeyFromAssetRef(assetRef);
        if (_refCountAssetRef.TryGetValue(guid, out var refCount))
            if (refCount > 1)
            {
                _refCountAssetRef[guid] -= 1;
                return;
            }
        _refCountAssetRef.Remove(guid);
        UnloadAssetHandle(guid);
    }

    protected string GetGuidKeyFromAssetRef(AssetReference assetRef)
    {
        return string.Format(STR_FORMAT_GUID, assetRef.AssetGUID, assetRef.SubObjectName);
    }
    
    public async UniTask UnloadAsyncAllAddressable()
    {
        foreach (var handle in _loadingAssets.Values)
        {
            await handle.Task;
            Addressables.Release(handle);
        }
        _loadingAssets.Clear();

        foreach (var handle in _loadedAssets.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _loadedAssets.Clear();
        _refCountAssetRef.Clear();
    }
}

