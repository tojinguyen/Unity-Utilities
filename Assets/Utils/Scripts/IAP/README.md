# IAP (In-App Purchase) Integration for Unity Utilities

This package provides a complete IAP integration solution for Unity projects with a clean, service-based architecture supporting Unity IAP.

## Quick Start

### 🚀 Get Running in 5 Minutes

1. **Open Setup Window**: `TirexGame > IAP > Setup IAP`
2. **Install Unity IAP**: Click "Install Unity IAP"
3. **Create Config**: Click "Create IAP Configuration"
4. **Add Manager**: Click "Add IAPManager to Scene"
5. **Configure Products**: Set up your products in the IAPConfig inspector
6. **Start Using**:
   ```csharp
   // In your game script
   await IAPService.InitializeAsync();
   var result = await IAPService.PurchaseAsync("your_product_id");
   ```

That's it! You now have working IAP in your game.

## Features

- **Multiple Product Types**: Consumable, Non-Consumable, and Subscription support
- **Cross-Platform**: Android (Google Play) and iOS (App Store) support
- **Async/Await Support**: Built with UniTask for modern async programming
- **Event System**: Comprehensive event callbacks for all IAP actions
- **Configuration**: ScriptableObject-based configuration system
- **Receipt Validation**: Client-side and server-side validation support
- **Restore Purchases**: Built-in purchase restoration for iOS
- **Test Mode**: Safe testing during development
- **Auto-retry**: Automatic retry logic for failed operations

## Installation

### 1. Install Unity IAP

You can install Unity IAP in several ways:

#### Option A: Using the Setup Window (Recommended)
1. Open `TirexGame > IAP > Setup IAP` from the Unity menu
2. Click "Install Unity IAP"

#### Option B: Manual Installation via Package Manager
1. Open `Window > Package Manager`
2. Select "Unity Registry" from the dropdown
3. Search for "In App Purchasing"
4. Click "Install"

### 2. Create IAP Configuration

1. Use the setup window: `TirexGame > IAP > Setup IAP > Create IAP Configuration`
2. Or manually: `Right-click in Project > Create > TirexGame > IAP > IAP Config`
3. Configure your products in the inspector

### 3. Add IAPManager to Scene

1. Use the setup window: `TirexGame > IAP > Setup IAP > Add IAPManager to Scene`
2. Or manually: Create an empty GameObject and add the `IAPManager` component
3. Assign your `IAPConfig` to the IAPManager

## Configuration

### IAP Config Settings

```
General Settings:
- Enable Test Mode: Use for development/testing
- Enable Logging: Show debug logs
- Enable Receipt Validation: Validate purchases

Validation Settings:
- Server Validation URL: For server-side validation
- Validation Timeout: Timeout for validation requests

Products:
- Product ID: Your product identifier
- Type: Consumable/NonConsumable/Subscription
- Store-specific IDs: Platform-specific identifiers
```

### Product Configuration

For each product, configure:
- **Product ID**: Universal identifier for your product
- **Type**: Choose the appropriate product type
- **Google Play ID**: (Optional) Google Play specific ID
- **Apple App Store ID**: (Optional) App Store specific ID
- **Enable Payout**: Whether this product should be processed

## Usage

### Basic Usage

```csharp
using TirexGame.Utils.IAP;
using Cysharp.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    private async void Start()
    {
        // Initialize the IAP system
        await IAPService.InitializeAsync();
    }
    
    public async void PurchaseProduct(string productId)
    {
        // Check if product is available
        if (!IAPService.IsProductAvailable(productId))
        {
            Debug.Log("Product not available");
            return;
        }
        
        // Purchase the product
        var result = await IAPService.PurchaseAsync(productId);
        
        if (result.Success)
        {
            Debug.Log($"Purchase successful: {productId}");
            // Process the purchase (give items, remove ads, etc.)
            ProcessPurchase(result);
        }
        else
        {
            Debug.Log($"Purchase failed: {result.ErrorMessage}");
        }
    }
    
    public async void RestorePurchases()
    {
        var result = await IAPService.RestorePurchasesAsync();
        
        if (result.Success)
        {
            Debug.Log($"Restored {result.RestoredPurchases.Length} purchases");
            foreach (var purchase in result.RestoredPurchases)
            {
                ProcessPurchase(purchase);
            }
        }
    }
    
    private void ProcessPurchase(PurchaseResult purchase)
    {
        switch (purchase.ProductId)
        {
            case "remove_ads":
                // Remove ads from the game
                break;
            case "premium_currency":
                // Give player premium currency
                break;
            // Handle other products...
        }
    }
}
```

### Event Handling

```csharp
public class IAPEventHandler : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to IAP events
        IAPService.SubscribeToEvents(
            onInitialized: OnIAPInitialized,
            onPurchaseCompleted: OnPurchaseCompleted,
            onPurchaseFailed: OnPurchaseFailed,
            onPurchasesRestored: OnPurchasesRestored
        );
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        IAPService.UnsubscribeFromEvents(
            onInitialized: OnIAPInitialized,
            onPurchaseCompleted: OnPurchaseCompleted,
            onPurchaseFailed: OnPurchaseFailed,
            onPurchasesRestored: OnPurchasesRestored
        );
    }
    
    private void OnIAPInitialized()
    {
        Debug.Log("IAP system ready!");
    }
    
    private void OnPurchaseCompleted(PurchaseResult result)
    {
        Debug.Log($"Purchase completed: {result.ProductId}");
    }
    
    private void OnPurchaseFailed(string productId, string error)
    {
        Debug.Log($"Purchase failed: {productId} - {error}");
    }
    
    private void OnPurchasesRestored(RestoreResult result)
    {
        Debug.Log($"Purchases restored: {result.RestoredPurchases.Length} items");
    }
}
```

### Product Information

```csharp
public class ProductCatalog : MonoBehaviour
{
    public void DisplayProducts()
    {
        var products = IAPService.GetAllProducts();
        
        foreach (var product in products)
        {
            Debug.Log($"Product: {product.Title}");
            Debug.Log($"Description: {product.Description}");
            Debug.Log($"Price: {product.Price}");
            Debug.Log($"Available: {product.IsAvailable}");
        }
    }
    
    public void CheckSpecificProduct(string productId)
    {
        var product = IAPService.GetProductInfo(productId);
        bool isAvailable = IAPService.IsProductAvailable(productId);
        
        Debug.Log($"{productId}: {product.Title} - {product.Price} (Available: {isAvailable})");
    }
}
```

### Receipt Validation

```csharp
public class PurchaseValidator : MonoBehaviour
{
    public async void ValidatePurchase(string receipt, string productId)
    {
        var validation = await IAPService.ValidatePurchaseAsync(receipt, productId);
        
        if (validation.IsValid)
        {
            Debug.Log("Purchase is valid");
            // Process the validated purchase
        }
        else
        {
            Debug.Log($"Purchase validation failed: {validation.ErrorMessage}");
            // Handle invalid purchase
        }
    }
}
```

## API Reference

### IAPService Static Methods

#### Initialization
- `InitializeAsync()`: Initialize the IAP system
- `IsInitialized`: Check if system is initialized

#### Purchase Methods
- `PurchaseAsync(productId)`: Purchase a product
- `RestorePurchasesAsync()`: Restore previous purchases

#### Product Information
- `IsProductAvailable(productId)`: Check if product is available
- `GetProductInfo(productId)`: Get detailed product information
- `GetAllProducts()`: Get information for all configured products

#### Validation
- `ValidatePurchaseAsync(receipt, productId)`: Validate a purchase receipt

#### Events
- `SubscribeToEvents(...)`: Subscribe to IAP events
- `UnsubscribeFromEvents(...)`: Unsubscribe from events

### Data Structures

#### PurchaseResult
```csharp
public struct PurchaseResult
{
    public bool Success;
    public string ProductId;
    public string TransactionId;
    public string Receipt;
    public PurchaseState State;
    public string ErrorMessage;
}
```

#### ProductInfo
```csharp
public struct ProductInfo
{
    public string ProductId;
    public string Title;
    public string Description;
    public string Price;
    public string PriceCurrencyCode;
    public decimal PriceDecimal;
    public ProductType Type;
    public bool IsAvailable;
}
```

## Testing

Use the included `IAPTestUI` component for easy testing:

1. Create a Canvas with UI elements (buttons, text)
2. Add the `IAPTestUI` component to a GameObject
3. Assign the UI references in the inspector
4. Test all IAP functionality with the buttons

## Best Practices

### 1. Initialize Early
```csharp
// Initialize in your main scene or persistent object
await IAPService.InitializeAsync();
```

### 2. Check Availability
```csharp
// Always check if products are available before purchasing
if (IAPService.IsProductAvailable(productId))
{
    await IAPService.PurchaseAsync(productId);
}
```

### 3. Handle Failures Gracefully
```csharp
var result = await IAPService.PurchaseAsync(productId);
if (!result.Success)
{
    // Show user-friendly error message
    Debug.Log("Purchase not available, try again later");
}
```

### 4. Validate Important Purchases
```csharp
// Validate high-value purchases
if (isHighValuePurchase)
{
    var validation = await IAPService.ValidatePurchaseAsync(receipt, productId);
    if (!validation.IsValid)
    {
        // Handle validation failure
        return;
    }
}
```

### 5. Provide Restore Functionality
```csharp
// Always provide a way for users to restore purchases
public async void OnRestoreButtonClicked()
{
    await IAPService.RestorePurchasesAsync();
}
```

## Store Setup

### Google Play Console

1. Upload your APK/AAB to Google Play Console
2. Navigate to "Monetization" > "Products" > "In-app products"
3. Create your products with the same IDs as configured
4. Activate the products

### Apple App Store Connect

1. Create your app in App Store Connect
2. Navigate to "Features" > "In-App Purchases"
3. Create your products with the same IDs as configured
4. Submit for review

## Troubleshooting

### Common Issues

1. **Products not loading**: Check internet connection and store setup
2. **Test purchases not working**: Ensure test mode is enabled
3. **Build errors**: Make sure Unity IAP is properly installed
4. **iOS restore issues**: Check that you're calling restore on iOS only

### Debug Logging

Enable logging in the IAPConfig to see detailed debug information:
```
Enable Logging: ✓
```

Check the Unity Console for detailed logs:
```
[IAPManager] Initializing IAP...
[IAPManager] IAP initialized successfully
[IAPManager] Product loaded: remove_ads - Remove Ads ($2.99)
[IAPManager] Purchase completed: remove_ads
```

### Platform Conditional Compilation

The code uses conditional compilation directives:
```csharp
#if UNITY_PURCHASING && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS)
// Unity IAP code here
#endif
```

This ensures the code only compiles on supported platforms.

## Security Considerations

### Receipt Validation

- **Client-side**: Built-in validation using Unity's security features
- **Server-side**: Implement server validation for high-value purchases
- **Hybrid**: Use client validation for small purchases, server for large ones

### Best Practices

1. **Never trust the client**: Validate important purchases server-side
2. **Store purchase state**: Save purchase information securely
3. **Handle edge cases**: Account for network failures, interrupted purchases
4. **Test thoroughly**: Test on real devices with real store accounts

## Dependencies

- Unity 2021.3 or later
- UniTask package (`com.cysharp.unitask`)
- Unity IAP package (`com.unity.purchasing`)

## Support

For questions and support:
- Check Unity IAP documentation
- Review the example scripts
- Use the test UI for debugging

## Version History

### Version 1.0.0
- Initial IAP integration
- Support for Consumable, Non-Consumable, and Subscription products
- UniTask async/await support
- Event system implementation
- Receipt validation (client-side)
- Purchase restoration
- Cross-platform support (Android/iOS)

## License

This package follows the same license as the Unity Utilities package (MIT License).
