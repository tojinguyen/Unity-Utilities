# In-App Purchasing (IAP)

This utility provides a comprehensive and easy-to-use solution for handling in-app purchases in your Unity project. It is built on top of Unity's IAP system and provides a simplified, async-based workflow.

## Features

- **Configuration-based Setup**: Define all your IAP products in a `ScriptableObject` for easy management.
- **Async-based Workflow**: All purchase and restore operations are asynchronous, using `UniTask` for modern C# async/await support.
- **Service-oriented Architecture**: A static `IAPService` class provides a simple and clean API for interacting with the IAP system.
- **Receipt Validation**: Built-in support for local receipt validation and a placeholder for server-side validation.
- **Test UI**: A sample UI scene is included to demonstrate how to use the IAP system and test your IAP setup.

## How to Use

1.  **Create IAP Config**: Create an `IAPConfig` ScriptableObject from the `Create > TirexGame > IAP > IAP Config` menu. Configure your products and settings in this asset.
2.  **Initialize the IAP Manager**: The `IAPManager` can be set to initialize automatically on awake. You can also initialize it manually by calling `IAPService.InitializeAsync()`.
3.  **Purchase Products**: Use the `IAPService.PurchaseAsync(productId)` method to initiate a purchase.
4.  **Restore Purchases**: Use the `IAPService.RestorePurchasesAsync()` method to restore non-consumable purchases.

### Example Usage

```csharp
// Initialize IAP
await IAPService.InitializeAsync();

// Purchase a product
var purchaseResult = await IAPService.PurchaseAsync("my_product_id");
if (purchaseResult.Success)
{
    Debug.Log("Purchase successful!");
}
else
{
    Debug.LogError($"Purchase failed: {purchaseResult.ErrorMessage}");
}

// Restore purchases
var restoreResult = await IAPService.RestorePurchasesAsync();
if (restoreResult.Success)
{
    Debug.Log($"Restored {restoreResult.RestoredPurchases.Length} purchases.");
}
else
{
    Debug.LogError($"Restore failed: {restoreResult.ErrorMessage}");
}
```
