# Addressables Helper

This utility provides a helper class for managing Addressable Assets in Unity. It adds a layer of caching and reference counting to the standard Addressables API, making it easier to manage memory and avoid unnecessary loading/unloading of assets.

## Features

- **Caching:** Loaded assets are cached to prevent redundant loading.
- **Feature-based Caching:** Group assets into "features" for collective management. This is useful for unloading all assets related to a specific feature at once.
- **Reference Counting:** Automatically tracks asset usage and unloads them when they are no longer needed.
- **Asynchronous Operations:** Leverages `UniTask` for efficient, asynchronous asset loading.
- **Flexible Loading:** Load assets using either `AssetReference` or a string key.
- **Flexible Unloading:** Unload assets individually, by feature, or all at once.

## How to Use

The `AddressableHelper` is a static class, so you can call its methods from anywhere in your code.

### Loading Assets

To load an asset, use the `GetAssetAsync<TObject>()` method. You can provide either an `AssetReference` or a string key.

```csharp
// Load by AssetReference
public AssetReference monsterPrefab;
var monster = await AddressableHelper.GetAssetAsync<GameObject>(monsterPrefab, "gameplay");

// Load by string key
var playerPrefab = await AddressableHelper.GetAssetAsync<GameObject>("player", "gameplay");
```

### Unloading Assets

To unload an asset, use the `TryUnloadAssetHandle()` method.

```csharp
// Unload by AssetReference
AddressableHelper.TryUnloadAssetHandle(monsterPrefab);

// Unload by string key
AddressableHelper.TryUnloadAssetHandle("player");
```

### Feature Management

You can group assets by providing a feature name when loading them. This allows you to unload all assets associated with a feature at once.

```csharp
// Load assets into a "gameplay" feature
var monster = await AddressableHelper.GetAssetAsync<GameObject>(monsterPrefab, "gameplay");
var playerPrefab = await AddressableHelper.GetAssetAsync<GameObject>("player", "gameplay");

// Unload all assets in the "gameplay" feature
AddressableHelper.UnloadAsyncAllAddressableInFeature("gameplay");
```

### Unload All Assets

To unload all loaded assets, use the `UnloadAsyncAllAddressable()` method.

```csharp
AddressableHelper.UnloadAsyncAllAddressable();
```
