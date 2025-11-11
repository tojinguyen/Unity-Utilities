# Ads Monetization

This utility provides a comprehensive solution for integrating and managing mobile ads in your Unity project. It supports banner, interstitial, and rewarded ads using the Google Mobile Ads (AdMob) SDK.

## Features

- **Easy Integration:** Quickly integrate banner, interstitial, and rewarded ads into your game.
- **AdMob Support:** Built on top of the official Google Mobile Ads SDK.
- **Simplified API:** A simple, static `AdService` class for easy access to all ad functionalities.
- **Automatic Ad Loading:** Ads are automatically pre-loaded to ensure they are ready when you need them.
- **Retry Logic:** Automatically retries loading ads if they fail.
- **Test UI:** A built-in UI for testing your ad placements and logic.
- **Events:** Subscribe to a wide range of ad events to track the ad lifecycle.

## How to Use

### Setup

1. **Use the Setup Window:** Go to `TirexGame > Ads > Setup AdMob` to use the automated setup window.
   - Install Google Mobile Ads SDK automatically
   - Browse and select the folder for your config file
   - Create AdMobConfig with one click
   - Add AdManager to your scene automatically

2. **Manual Configuration:**
   - Create an `AdMobConfig` ScriptableObject by right-clicking in the Project window and selecting `Create > TirexGame > Ads > AdMob Config`.
   - Enter your AdMob ad unit IDs for Android and iOS.
   - Assign the `AdMobConfig` to the `AdManager` component in your scene.

### Managing Ad Unit IDs

The `AdMobConfig` inspector now provides convenient buttons to:
- **Clear Android IDs:** Remove all Android ad unit IDs
- **Clear iOS IDs:** Remove all iOS ad unit IDs  
- **Set Test IDs:** Automatically set Google's test ad unit IDs for testing
- **Clear All:** Remove all ad unit IDs for both platforms

This makes it easy to switch between test and production ad units, or clear unwanted IDs.

### Displaying Ads

The `AdService` class provides a simple, static API for showing ads.

#### Banner Ads

```csharp
// Show a banner ad at the bottom of the screen
AdService.ShowBanner(AdPosition.Bottom);

// Hide the banner ad
AdService.HideBanner();
```

#### Interstitial Ads

```csharp
// Show an interstitial ad
if (AdService.IsInterstitialReady())
{
    await AdService.ShowInterstitialAsync();
}
```

#### Rewarded Ads

```csharp
// Show a rewarded ad and get the result
if (AdService.IsRewardedReady())
{
    AdResult result = await AdService.ShowRewardedAsync();
    if (result.Success)
    {
        Debug.Log("Player earned reward: " + result.Reward.Amount + " " + result.Reward.Type);
    }
}
```

### Events

You can subscribe to various ad events to track the ad lifecycle.

```csharp
private void OnEnable()
{
    AdService.SubscribeToEvents(
        onAdLoaded: (adType) => Debug.Log(adType + " ad loaded"),
        onRewardEarned: (reward) => Debug.Log("Reward earned: " + reward.Amount)
    );
}

private void OnDisable()
{
    AdService.UnsubscribeFromEvents(
        onAdLoaded: (adType) => Debug.Log(adType + " ad loaded"),
        onRewardEarned: (reward) => Debug.Log("Reward earned: " + reward.Amount)
    );
}
```

### Test UI

The `AdTestUI` prefab can be added to your scene to provide a simple UI for testing your ad integrations. It includes buttons for showing each ad type.
