# Firebase Integration

This utility provides a simplified interface for integrating Firebase services into your Unity project. It includes support for Firebase Analytics and Remote Config.

## Features

- **Firebase Manager**: A central manager to initialize and access Firebase services.
- **Firebase Analytics Manager**: A dedicated manager for handling Firebase Analytics events and user properties.
- **Firebase Remote Config Manager**: A manager for fetching and using Remote Config values.
- **Conditional Compilation**: The entire Firebase integration can be enabled or disabled using the `FIREBASE_TOOL` scripting define.

## How to Use

1.  **Enable Firebase**: Add the `FIREBASE_TOOL` scripting define to your project's player settings to enable the Firebase integration.
2.  **Initialization**: The `FirebaseManager` can be configured to initialize on awake. You can also manually initialize it by calling the `InitializeFirebase()` method.
3.  **Accessing Services**: Use the `FirebaseManager` to access the `FirebaseAnalyticsManager` and `FirebaseRemoteConfigManager` instances.

### Logging Analytics Events

```csharp
// Get the Analytics Manager
var analyticsManager = FirebaseManager.Instance.GetAnalytics();

// Log a simple event
analyticsManager.LogEvent("level_start");

// Log an event with parameters
var parameters = new Dictionary<string, object>
{
    { "level_name", "Level 1" },
    { "difficulty", "easy" }
};
analyticsManager.LogEvent("level_complete", parameters);
```

### Using Remote Config

```csharp
// Get the Remote Config Manager
var remoteConfigManager = FirebaseManager.Instance.GetRemoteConfig();

// Fetch the latest config values
await remoteConfigManager.FetchDataAsync();

// Get a string value
string welcomeMessage = remoteConfigManager.GetString("welcome_message", "Welcome!");

// Get a boolean value
bool showAds = remoteConfigManager.GetBool("show_ads", true);

// Get a double value
double coinsMultiplier = remoteConfigManager.GetDouble("coins_multiplier", 1.0);
```
