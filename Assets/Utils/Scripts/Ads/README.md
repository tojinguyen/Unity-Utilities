# AdMob Integration for Unity Utilities

This package provides a complete AdMob integration solution for Unity projects with a clean, service-based architecture.

## Quick Start

### 🚀 Get Running in 5 Minutes

1. **Open Setup Window**: `TirexGame > Ads > Setup AdMob`
2. **Install SDK**: Click "Install Google Mobile Ads SDK"
3. **Create Config**: Click "Create AdMob Configuration"
4. **Add Manager**: Click "Add AdManager to Scene"
5. **Start Using**:
   ```csharp
   // In your game script
   await AdService.InitializeAsync();
   AdService.ShowBanner();
   ```

That's it! You now have working test ads in your game.

## Features

- **Banner Ads**: Customizable positioning (Top, Bottom, Center, etc.)
- **Interstitial Ads**: Full-screen ads with automatic retry logic
- **Rewarded Ads**: Video ads with reward callbacks
- **Async/Await Support**: Built with UniTask for modern async programming
- **Event System**: Comprehensive event callbacks for all ad actions
- **Configuration**: ScriptableObject-based configuration system
- **Test Mode**: Built-in test ad support for development
- **Auto-retry**: Automatic retry logic for failed ad loads
- **Platform Support**: Android and iOS with conditional compilation

## Installation

### 1. Install Google Mobile Ads SDK

You can install the Google Mobile Ads SDK in several ways:

#### Option A: Using the Setup Window (Recommended)
1. Open `TirexGame > Ads > Setup AdMob` from the Unity menu
2. Click "Install Google Mobile Ads SDK"

#### Option B: Manual Installation via Package Manager
1. Open `Window > Package Manager`
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/googleads/googleads-mobile-unity.git`

#### Option C: Manual Download
1. Download the latest release from [Google Mobile Ads Unity Plugin](https://github.com/googleads/googleads-mobile-unity/releases)
2. Import the `.unitypackage` file into your project

### 2. Create AdMob Configuration

1. Use the setup window: `TirexGame > Ads > Setup AdMob > Create AdMob Configuration`
2. Or manually: `Right-click in Project > Create > TirexGame > Ads > AdMob Config`
3. Configure your Ad Unit IDs in the inspector

### 3. Add AdManager to Scene

1. Use the setup window: `TirexGame > Ads > Setup AdMob > Add AdManager to Scene`
2. Or manually: Create an empty GameObject and add the `AdManager` component
3. Assign your `AdMobConfig` to the AdManager

## Configuration

### AdMob Config Settings

```
App IDs:
- Android App ID: Your AdMob Android app ID
- iOS App ID: Your AdMob iOS app ID

Ad Unit IDs:
- Banner ID: Banner ad unit ID
- Interstitial ID: Interstitial ad unit ID  
- Rewarded ID: Rewarded ad unit ID

Settings:
- Enable Test Mode: Use test ads during development
- Enable Logging: Show debug logs
- Retry Attempts: Number of retry attempts for failed loads
- Retry Delay: Delay between retry attempts
```

### Test Ad Unit IDs

The configuration comes pre-configured with Google's test ad unit IDs:

**Android:**
- Banner: `ca-app-pub-3940256099942544/6300978111`
- Interstitial: `ca-app-pub-3940256099942544/1033173712`
- Rewarded: `ca-app-pub-3940256099942544/5224354917`

**iOS:**
- Banner: `ca-app-pub-3940256099942544/2934735716`
- Interstitial: `ca-app-pub-3940256099942544/4411468910`
- Rewarded: `ca-app-pub-3940256099942544/1712485313`

## Usage

### Basic Usage

```csharp
using TirexGame.Utils.Ads;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        // Initialize the ad system
        await AdService.InitializeAsync();
        
        // Show a banner ad
        AdService.ShowBanner(AdPosition.Bottom);
    }
    
    public async void ShowInterstitialAd()
    {
        // Check if ad is ready
        if (!AdService.IsInterstitialReady())
        {
            // Load ad if not ready
            await AdService.LoadInterstitialAsync();
        }
        
        // Show the ad
        bool success = await AdService.ShowInterstitialAsync();
        Debug.Log($"Interstitial shown: {success}");
    }
    
    public async void ShowRewardedAd()
    {
        // Check if ad is ready
        if (!AdService.IsRewardedReady())
        {
            await AdService.LoadRewardedAsync();
        }
        
        // Show the ad and wait for result
        var result = await AdService.ShowRewardedAsync();
        
        if (result.Success)
        {
            Debug.Log($"Reward earned: {result.Reward.Type} x{result.Reward.Amount}");
            // Give reward to player
        }
        else
        {
            Debug.Log($"Reward failed: {result.ErrorMessage}");
        }
    }
}
```

### Event Handling

```csharp
public class AdEventHandler : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to ad events
        AdService.SubscribeToEvents(
            onInitialized: OnAdSystemInitialized,
            onAdLoaded: OnAdLoaded,
            onAdFailedToLoad: OnAdFailedToLoad,
            onRewardEarned: OnRewardEarned
        );
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        AdService.UnsubscribeFromEvents(
            onInitialized: OnAdSystemInitialized,
            onAdLoaded: OnAdLoaded,
            onAdFailedToLoad: OnAdFailedToLoad,
            onRewardEarned: OnRewardEarned
        );
    }
    
    private void OnAdSystemInitialized()
    {
        Debug.Log("Ad system ready!");
    }
    
    private void OnAdLoaded(AdType adType)
    {
        Debug.Log($"{adType} ad loaded");
    }
    
    private void OnAdFailedToLoad(AdType adType, string error)
    {
        Debug.LogError($"{adType} failed to load: {error}");
    }
    
    private void OnRewardEarned(AdReward reward)
    {
        Debug.Log($"Player earned: {reward.Type} x{reward.Amount}");
        // Give reward to player
    }
}
```

### Banner Ad Positioning

```csharp
// Show banner at different positions
AdService.ShowBanner(AdPosition.Top);
AdService.ShowBanner(AdPosition.Bottom);
AdService.ShowBanner(AdPosition.Center);
AdService.ShowBanner(AdPosition.TopLeft);
AdService.ShowBanner(AdPosition.TopRight);
AdService.ShowBanner(AdPosition.BottomLeft);
AdService.ShowBanner(AdPosition.BottomRight);

// Hide banner
AdService.HideBanner();

// Destroy banner (releases memory)
AdService.DestroyBanner();
```

## API Reference

### AdService Static Methods

#### Initialization
- `InitializeAsync()`: Initialize the ad system
- `IsInitialized`: Check if system is initialized

#### Banner Ads
- `ShowBanner(AdPosition)`: Show banner at position
- `HideBanner()`: Hide banner
- `DestroyBanner()`: Destroy banner

#### Interstitial Ads
- `LoadInterstitialAsync()`: Load interstitial ad
- `IsInterstitialReady()`: Check if ready to show
- `ShowInterstitialAsync()`: Show interstitial ad

#### Rewarded Ads
- `LoadRewardedAsync()`: Load rewarded ad
- `IsRewardedReady()`: Check if ready to show
- `ShowRewardedAsync()`: Show rewarded ad

#### Events
- `SubscribeToEvents(...)`: Subscribe to ad events
- `UnsubscribeFromEvents(...)`: Unsubscribe from events

### Event Types

- `OnInitialized`: Ad system initialized
- `OnAdLoaded(AdType)`: Ad loaded successfully
- `OnAdFailedToLoad(AdType, string)`: Ad failed to load
- `OnAdShown(AdType)`: Ad shown to user
- `OnAdClosed(AdType)`: Ad closed by user
- `OnAdFailedToShow(AdType, string)`: Ad failed to show
- `OnRewardEarned(AdReward)`: Reward earned from rewarded ad

## Testing

Use the included `AdTestUI` component for easy testing:

1. Create a Canvas with UI elements (buttons, text)
2. Add the `AdTestUI` component to a GameObject
3. Assign the UI references in the inspector
4. Test all ad types with the buttons

## Best Practices

### 1. Initialize Early
```csharp
// Initialize in your main scene or persistent object
await AdService.InitializeAsync();
```

### 2. Preload Ads
```csharp
// Preload ads for better user experience
await AdService.LoadInterstitialAsync();
await AdService.LoadRewardedAsync();
```

### 3. Check Readiness
```csharp
// Always check if ads are ready before showing
if (AdService.IsInterstitialReady())
{
    await AdService.ShowInterstitialAsync();
}
```

### 4. Handle Failures Gracefully
```csharp
var result = await AdService.ShowRewardedAsync();
if (!result.Success)
{
    // Provide alternative reward or try again later
    Debug.Log("Ad not available, try again later");
}
```

### 5. Respect User Experience
```csharp
// Don't show interstitials too frequently
// Consider user actions and game flow
// Always provide alternative ways to progress
```

## Troubleshooting

### Common Issues

1. **Ads not loading**: Check internet connection and ad unit IDs
2. **Test ads not showing**: Ensure test mode is enabled in config
3. **Build errors**: Make sure Google Mobile Ads SDK is properly installed
4. **iOS build issues**: Check iOS deployment target (minimum iOS 12.0)

### Debug Logging

Enable logging in the AdMobConfig to see detailed debug information:
```
Enable Logging: ✓
```

Check the Unity Console for detailed logs:
```
[AdManager] Initializing AdMob...
[AdManager] AdMob initialized successfully
[AdManager] Loading interstitial ad: ca-app-pub-xxxxx
[AdManager] Interstitial ad loaded successfully
```

### Platform Conditional Compilation

The code uses conditional compilation directives:
```csharp
#if UNITY_ANDROID || UNITY_IOS
// AdMob code here
#endif
```

This ensures the code only compiles on supported platforms.

### Testing with Test Ads

When `Enable Test Mode` is checked in the AdMobConfig:
- Test ads will be shown instead of real ads
- No real revenue will be generated
- Ads will always be available for testing
- Use test device IDs for more realistic testing

### Mediation Support

The AdMob implementation supports mediation networks:
- Configure mediation in your AdMob dashboard
- No code changes required
- The same API works with all mediated networks

### GDPR and Privacy Compliance

For GDPR compliance, you may need to:
```csharp
// Request consent before initializing ads
// Use Google UMP SDK or similar consent management
await RequestConsentAsync();
await AdService.InitializeAsync();
```

### Ad Frequency Management

Implement smart ad frequency controls:
```csharp
public class AdFrequencyManager
{
    private static float lastInterstitialTime;
    private static int sessionAdCount;
    private const float MIN_INTERVAL = 60f; // 1 minute minimum
    private const int MAX_SESSION_ADS = 10;
    
    public static bool CanShowInterstitial()
    {
        return Time.time - lastInterstitialTime > MIN_INTERVAL &&
               sessionAdCount < MAX_SESSION_ADS;
    }
    
    public static void OnInterstitialShown()
    {
        lastInterstitialTime = Time.time;
        sessionAdCount++;
    }
}
```

## Dependencies

- Unity 2021.3 or later
- UniTask package (`com.cysharp.unitask`)
- Google Mobile Ads SDK

## Advanced Configuration

### Custom Ad Loading Strategy

Create a custom ad preloading strategy:
```csharp
public class SmartAdLoader : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(PreloadAdsRoutine());
    }
    
    private IEnumerator PreloadAdsRoutine()
    {
        // Wait for initialization
        yield return new WaitUntil(() => AdService.IsInitialized);
        
        // Preload interstitial
        if (!AdService.IsInterstitialReady())
        {
            yield return AdService.LoadInterstitialAsync().ToCoroutine();
        }
        
        // Wait a bit before loading rewarded to avoid rate limiting
        yield return new WaitForSeconds(2f);
        
        // Preload rewarded
        if (!AdService.IsRewardedReady())
        {
            yield return AdService.LoadRewardedAsync().ToCoroutine();
        }
    }
}
```

### Performance Optimization

1. **Preload Strategy**: Load ads during natural breaks (menu screens, loading)
2. **Memory Management**: Destroy banners when not needed
3. **Background Loading**: Load ads while player is engaged
4. **Retry Logic**: Implement exponential backoff for failed loads

### Integration with Analytics

Track ad performance with your analytics system:
```csharp
public class AdAnalytics : MonoBehaviour
{
    private void Start()
    {
        AdService.SubscribeToEvents(
            onAdShown: (adType) => TrackAdShown(adType),
            onRewardEarned: (reward) => TrackRewardEarned(reward),
            onAdFailedToShow: (adType, error) => TrackAdError(adType, error)
        );
    }
    
    private void TrackAdShown(AdType adType)
    {
        // Analytics.LogEvent("ad_shown", "ad_type", adType.ToString());
    }
    
    private void TrackRewardEarned(AdReward reward)
    {
        // Analytics.LogEvent("reward_earned", "type", reward.Type, "amount", reward.Amount);
    }
    
    private void TrackAdError(AdType adType, string error)
    {
        // Analytics.LogEvent("ad_error", "ad_type", adType.ToString(), "error", error);
    }
}
```

## Support

For issues and questions:
1. Check the Unity Console for error messages
2. Verify your AdMob configuration
3. Test with Google's test ad unit IDs
4. Consult the [Google AdMob documentation](https://developers.google.com/admob/unity)

## Migration Guide

### From Legacy AdMob Integration

If you're migrating from an older AdMob setup:

1. **Remove old AdMob scripts** and dependencies
2. **Install the new Google Mobile Ads SDK** via Package Manager
3. **Create AdMobConfig** with your existing ad unit IDs
4. **Replace old ad calls** with new AdService calls:

```csharp
// Old way
AdMobManager.Instance.ShowInterstitial();

// New way
await AdService.ShowInterstitialAsync();
```

### From Other Ad Networks

When migrating from other ad networks:

1. **Keep your existing ad unit IDs** from AdMob dashboard
2. **Replace network-specific calls** with AdService calls
3. **Update event handling** to use the new event system
4. **Test thoroughly** with test ads before going live

## Version History

### Version 1.0.0
- Initial AdMob integration
- Banner, Interstitial, and Rewarded ad support
- UniTask async/await support
- Event system implementation
- Test mode support
- Automatic retry logic

## Roadmap

### Planned Features
- **App Open Ads**: Support for app open ad format
- **Native Ads**: Native ad integration
- **Mediation Helpers**: Built-in mediation network helpers
- **A/B Testing**: Ad placement and timing optimization
- **Revenue Analytics**: Built-in revenue tracking

## License

This package follows the same license as the Unity Utilities package (MIT License).