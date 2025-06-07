# Unity-Utilities

Unity Utility Scripts are a collection of helpful scripts designed to streamline common tasks and enhance your workflow in Unity. These utility scripts are built to simplify various aspects of game development, from handling object pooling to managing scene transitions and improving performance optimization.

## ✨ Features Overview

### 🎵 Audio Management System
A comprehensive audio management solution with advanced features:
- **Music, SFX, UI, Voice, and Ambient audio support**
- **Audio source pooling** for optimal performance
- **Volume controls** per category and master volume
- **Fade effects** (fade in, fade out, crossfade)
- **3D spatial audio** support
- **Addressables integration** for efficient asset loading
- **UniTask async support** for non-blocking operations

#### Quick Start
```csharp
// Play background music with crossfade
AudioService.PlayMusic("background_music", fadeIn: true);

// Play UI sound effect
AudioService.PlayUI("button_click");

// Play 3D positioned sound
AudioService.PlaySFX("explosion", transform.position);

// Control volume
AudioService.SetMasterVolume(0.8f);
AudioService.SetCategoryVolume(AudioType.Music, 0.6f);
```

### 📱 AdMob Integration
Complete AdMob integration solution with a clean, service-based architecture:
- **Banner Ads** with customizable positioning (Top, Bottom, Center, etc.)
- **Interstitial Ads** with automatic retry logic and frequency management
- **Rewarded Ads** with reward callbacks and result handling
- **Event System** with comprehensive callbacks for all ad actions
- **Test Mode** with built-in test ad support for development
- **Setup Window** for easy configuration and SDK installation

#### Quick Start
```csharp
// Initialize ads
await AdService.InitializeAsync();

// Show banner ad
AdService.ShowBanner(AdPosition.Bottom);

// Show rewarded ad
var result = await AdService.ShowRewardedAsync();
if (result.Success)
{
    // Give reward to player
}
```

### 🎮 UI Management System
Streamlined UI handling with advanced features:
- **Screen/Popup/Toast management** with automatic lifecycle
- **Addressables integration** for dynamic UI loading
- **Service pattern** for easy static access
- **Async/await support** with UniTask
- **Stack management** for popups with backdrop support

#### Quick Start
```csharp
// Show a popup
var popup = await UIService.ShowUI<SettingsPopup>("settings_popup_address");

// Pop current popup
await UIService.Pop();
```

### 🔄 Object Pooling System
High-performance object pooling with advanced features:
- **Component-based pooling** for maximum flexibility
- **Addressables pooling** for dynamic asset loading
- **Performance tracking** with detailed statistics
- **Auto-scaling pools** with configurable limits
- **Memory optimization** with automatic cleanup

#### Quick Start
```csharp
// Prewarm pool
ObjectPooling.Prewarm(bulletPrefab, 50);

// Get from pool
var bullet = ObjectPooling.Get(bulletPrefab);

// Return to pool
ObjectPooling.Return(bullet);
```

### 💾 Advanced Data Management
Enterprise-grade data management with comprehensive features:
- **Multiple storage backends** (File, PlayerPrefs, Cloud)
- **Encryption & Compression** with multiple algorithms
- **Automatic backups** with versioning and restoration
- **Caching system** with expiration and statistics
- **Event-driven architecture** with data change notifications
- **Validation system** with custom rules
- **Auto-save functionality** with configurable intervals

#### Key Features
- **DataManager**: Centralized data access with caching and validation
- **DataRepository**: Generic CRUD operations with encryption/compression
- **DataBackupManager**: Versioned backups with metadata and restoration
- **DataCompressor**: Multiple compression algorithms (GZip, Deflate, Brotli)
- **DataEncryptor**: AES encryption for sensitive data
- **DataCacheManager**: High-performance caching with expiration

#### Quick Start
```csharp
// Save data with automatic backup and caching
await DataManager.Instance.SaveDataAsync(playerData, "player_save");

// Load data with caching
var data = await DataManager.Instance.GetDataAsync<PlayerData>("player_save");

// Create full backup
await DataManager.Instance.CreateFullBackupAsync();

// Restore from backup
await DataManager.Instance.RestoreFromBackupAsync("backup_id");
```

### 📦 Addressables Helper
Simplified integration with Unity's Addressables system:
- **Feature-based asset caching** with automatic reference counting
- **Async/await support** with UniTask integration
- **Memory management** with automatic cleanup
- **Error handling** with graceful fallbacks

#### Quick Start
```csharp
// Load asset with caching
var asset = await AddressablesHelper.GetAssetAsync<GameObject>(assetReference, "MyFeature");

// Release when done
AddressablesHelper.ReleaseAsset(assetReference, "MyFeature");
```

### 🎯 Singleton Pattern
Thread-safe singleton implementations:
- **MonoSingleton**: MonoBehaviour-based singleton with automatic instantiation
- **Thread-safe access** with proper locking mechanisms
- **Lifecycle management** with initialization and cleanup hooks

### 📝 Enhanced Logging System
Advanced logging utilities with conditional compilation:
- **Colored console output** for better readability
- **Conditional compilation** with custom defines
- **Multiple log levels** with categorization
- **Performance optimized** for production builds

#### Quick Start
```csharp
// Colored logging
ConsoleLogger.LogColor("Important message", ColorLog.RED);

// Conditional logging (only in development)
ConsoleLogger.Log("Debug information");
```

### 🛠️ Editor Tools & Utilities
Collection of custom editor tools to enhance development:

#### Scene Toolbar Utilities
- **Quick scene switching** directly from toolbar
- **Build settings integration** with automatic scene detection
- **Customizable positioning** and sizing options

#### Sprite Import Settings
- **Automated sprite import configuration** with custom presets
- **Batch processing** for multiple sprites
- **Settings persistence** across project sessions

#### AdMob Setup Window
- **One-click SDK installation** with automatic configuration
- **Configuration management** with test/production modes
- **Asset creation helpers** for quick setup

## 🚀 Installation

### Option 1: Unity Package Manager (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` and select `Add package from git URL`
3. Enter: `https://github.com/your-repo/unity-utilities.git`

### Option 2: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Utils` folder into your Unity project
3. Ensure dependencies are installed (see Dependencies section)

### Option 3: Unity Package
1. Download the latest `.unitypackage` from releases
2. Import into your Unity project via `Assets > Import Package > Custom Package`

## 🔧 Dependencies

This package requires the following dependencies:

### Required Dependencies
- **UniTask** (`com.cysharp.unitask`): For async/await support
  ```
  https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  ```

### Optional Dependencies
- **Unity Addressables** (`com.unity.addressables`): For addressable asset loading
- **Google Mobile Ads SDK**: For AdMob integration
- **Newtonsoft.Json** (`com.unity.nuget.newtonsoft-json`): For data serialization

### Installing Dependencies

#### Via Package Manager:
1. Open `Window > Package Manager`
2. Click `+` and select `Add package from git URL`
3. Add each dependency URL

#### Via manifest.json:
Add to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unity.addressables": "1.21.19",
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  }
}
```

## 📚 Detailed Usage Examples

### Audio Management System

#### 1. Setting up Audio Database
1. In your Project window, right-click in a suitable folder
2. Go to `Create > TirexGame > Audio Database`
3. Configure audio clips with categories, volumes, and effects
4. Assign the database to AudioManager in your scene

#### 2. Advanced Audio Playback
```csharp
// Complex audio setup with custom parameters
var clipData = new AudioClipData
{
    clip = myAudioClip,
    volume = 0.8f,
    pitch = 1.2f,
    loop = true,
    fadeType = AudioFadeType.FadeIn,
    fadeDuration = 2f,
    spatialBlend = 1f, // 3D audio
    minDistance = 1f,
    maxDistance = 20f
};
AudioService.PlayAudio(clipData, AudioType.SFX);

// 3D positioned audio with falloff
AudioService.PlaySFX("explosion", worldPosition, AudioType.SFX);

// Crossfade between music tracks
AudioService.PlayMusic("new_track", fadeIn: true, crossfade: true);
```

#### 3. Volume and Settings Management
```csharp
// Master volume control
AudioService.SetMasterVolume(0.7f);

// Category-specific volumes
AudioService.SetMusicVolume(0.5f);
AudioService.SetSFXVolume(0.8f);
AudioService.SetUIVolume(1.0f);
AudioService.SetVoiceVolume(0.9f);
AudioService.SetAmbientVolume(0.6f);

// Mute/unmute functionality
AudioService.SetMusicMuted(true);
AudioService.ToggleMusicMute();

// Check current settings
bool isMuted = AudioService.IsMusicMuted();
float currentVolume = AudioService.GetMusicVolume();
```

### Data Management System

#### 1. Basic Data Operations
```csharp
// Define your data model
[Serializable]
public class PlayerData : IDataModel<PlayerData>
{
    public string playerName;
    public int level;
    public float experience;
    
    public PlayerData Clone() => JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(this));
    public void CopyFrom(PlayerData other) 
    {
        playerName = other.playerName;
        level = other.level;
        experience = other.experience;
    }
}

// Save player data
var playerData = new PlayerData { playerName = "John", level = 5, experience = 1250f };
await DataManager.Instance.SaveDataAsync(playerData, "player_profile");

// Load player data
var loadedData = await DataManager.Instance.GetDataAsync<PlayerData>("player_profile");

// Check if data exists
bool exists = await DataManager.Instance.HasDataAsync<PlayerData>("player_profile");

// Delete data
await DataManager.Instance.DeleteDataAsync<PlayerData>("player_profile");
```

#### 2. Advanced Data Features
```csharp
// Repository with encryption and compression
var repository = new FileDataRepository<PlayerData>(
    basePath: Application.persistentDataPath + "/secure_data",
    useEncryption: true,
    useCompression: true,
    encryptionKey: "your_32_char_encryption_key_here",
    encryptionIv: "your_16_char_iv_here"
);

// Register custom repository
DataManager.Instance.RegisterRepository<PlayerData>(repository);

// Create manual backup with description
string backupId = await DataManager.Instance.CreateBackupAsync(
    "player_profile", 
    playerData, 
    "Before major update"
);

// List available backups
var backups = await DataManager.Instance.GetAvailableBackupsAsync();

// Restore from backup
await DataManager.Instance.RestoreFromBackupAsync(backupId);

// Subscribe to data events
DataManager.Instance.SubscribeToDataEvents<PlayerData>(
    onSaved: data => Debug.Log($"Player data saved: {data.playerName}"),
    onLoaded: data => Debug.Log($"Player data loaded: {data.playerName}"),
    onDeleted: key => Debug.Log($"Player data deleted: {key}")
);

// Get cache performance statistics
var stats = DataManager.Instance.GetCacheStats();
Debug.Log($"Cache hit rate: {stats.HitRate:P2}");
```

#### 3. Data Compression Examples
```csharp
// Compress JSON data
string jsonData = JsonConvert.SerializeObject(largeDataObject);
var compressionResult = await DataCompressor.CompressJsonAsync(jsonData);

if (compressionResult.Success)
{
    Debug.Log($"Compressed from {compressionResult.Stats.OriginalSize} to {compressionResult.Stats.CompressedSize} bytes");
    Debug.Log($"Space saved: {compressionResult.Stats.SpaceSavedPercent:F1}%");
    
    // Save compressed data
    File.WriteAllBytes("compressed_data.gz", compressionResult.Data);
}

// Test different compression algorithms
var testData = Encoding.UTF8.GetBytes(jsonData);
var efficiencyResults = await DataCompressor.TestCompressionEfficiencyAsync(testData);

foreach (var result in efficiencyResults)
{
    Debug.Log($"{result.Key}: {result.Value.SpaceSavedPercent:F1}% space saved");
}

// Get recommended algorithm for data size
var algorithm = DataCompressor.GetRecommendedAlgorithm(testData.Length);
```

### AdMob Integration

#### 1. Setup and Configuration
```csharp
public class AdController : MonoBehaviour
{
    private async void Start()
    {
        // Initialize the ad system
        await AdService.InitializeAsync();
        
        // Subscribe to ad events for analytics/rewards
        AdService.SubscribeToEvents(
            onInitialized: OnAdSystemReady,
            onAdLoaded: OnAdLoaded,
            onAdFailedToLoad: OnAdFailedToLoad,
            onAdShown: OnAdShown,
            onAdClosed: OnAdClosed,
            onRewardEarned: OnRewardEarned
        );
        
        // Preload ads for better UX
        await PreloadAds();
    }
    
    private async UniTask PreloadAds()
    {
        var loadTasks = new[]
        {
            AdService.LoadInterstitialAsync(),
            AdService.LoadRewardedAsync()
        };
        
        await UniTask.WhenAll(loadTasks);
    }
    
    private void OnAdSystemReady()
    {
        Debug.Log("Ad system ready - showing banner");
        AdService.ShowBanner(AdPosition.Bottom);
    }
}
```

#### 2. Smart Ad Display Logic
```csharp
public class GameManager : MonoBehaviour
{
    private int gamesPlayed = 0;
    private float lastInterstitialTime = 0f;
    private const float MIN_INTERSTITIAL_INTERVAL = 60f; // 1 minute
    
    public async void OnGameOver()
    {
        gamesPlayed++;
        
        // Show interstitial every 3 games, but not too frequently
        if (gamesPlayed % 3 == 0 && CanShowInterstitial())
        {
            await ShowInterstitialWithFallback();
        }
    }
    
    private bool CanShowInterstitial()
    {
        return Time.time - lastInterstitialTime > MIN_INTERSTITIAL_INTERVAL;
    }
    
    private async UniTask ShowInterstitialWithFallback()
    {
        if (!AdService.IsInterstitialReady())
        {
            Debug.Log("Interstitial not ready, loading...");
            bool loaded = await AdService.LoadInterstitialAsync();
            
            if (!loaded)
            {
                Debug.Log("Failed to load interstitial");
                return;
            }
        }
        
        bool shown = await AdService.ShowInterstitialAsync();
        if (shown)
        {
            lastInterstitialTime = Time.time;
        }
    }
    
    public async void OnWatchRewardedAdButton()
    {
        if (!AdService.IsRewardedReady())
        {
            ShowLoadingIndicator(true);
            bool loaded = await AdService.LoadRewardedAsync();
            ShowLoadingIndicator(false);
            
            if (!loaded)
            {
                ShowMessage("Ad not available. Try again later.");
                return;
            }
        }
        
        var result = await AdService.ShowRewardedAsync();
        
        if (result.Success)
        {
            // Reward earned in OnRewardEarned callback
            ShowMessage("Reward earned!");
        }
        else
        {
            ShowMessage("Ad was closed before completion.");
        }
    }
}
```

### UI Management System

#### 1. UI Architecture Setup
```csharp
// Base classes for different UI types
public class MainMenuScreen : ScreenBase
{
    public override async UniTask OnBeforeShow()
    {
        // Setup animations, load data, etc.
        await base.OnBeforeShow();
    }
    
    public override async UniTask OnDoneShow()
    {
        // UI is fully visible and ready
        await base.OnDoneShow();
    }
}

public class SettingsPopup : PopupBase
{
    [SerializeField] private Button closeButton;
    
    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(() => UIService.Pop().Forget());
    }
}

public class NotificationToast : ToastBase
{
    [SerializeField] private Text messageText;
    
    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
```

#### 2. Dynamic UI Loading
```csharp
public class UIController : MonoBehaviour
{
    private async void Start()
    {
        // Show main menu screen
        var mainMenu = await UIService.ShowUI<MainMenuScreen>("main_menu_address");
        
        // Setup main menu events
        mainMenu.onSettingsClicked += ShowSettings;
    }
    
    private async void ShowSettings()
    {
        // Show settings popup (automatically manages backdrop and stack)
        var settingsPopup = await UIService.ShowUI<SettingsPopup>("settings_popup_address");
        
        // Configure settings
        settingsPopup.Initialize(currentSettings);
    }
    
    public async void ShowNotification(string message)
    {
        // Show toast notification
        var toast = await UIService.ShowUI<NotificationToast>("notification_toast_address");
        toast.SetMessage(message);
        
        // Auto-hide after 3 seconds
        await UniTask.Delay(3000);
        await UIService.Pop();
    }
}
```

### Object Pooling System

#### 1. Advanced Pooling Setup
```csharp
public class BulletManager : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private AssetReference bulletAssetRef;
    
    private void Start()
    {
        // Prewarm pool for immediate availability
        ObjectPooling.Prewarm(bulletPrefab, initialPoolSize);
        
        // Setup addressable pooling for dynamic loading
        AddressablesObjectPooling.Prewarm(bulletAssetRef, initialPoolSize, "BulletFeature");
    }
    
    public void FireBullet(Vector3 position, Vector3 direction)
    {
        // Get bullet from pool
        var bullet = ObjectPooling.Get(bulletPrefab, position, Quaternion.LookRotation(direction));
        
        // Configure bullet
        bullet.Initialize(direction, bulletSpeed);
        
        // Bullet will return itself to pool when destroyed/deactivated
    }
    
    private void OnDestroy()
    {
        // Clean up addressable pools
        AddressablesObjectPooling.ClearFeature("BulletFeature");
    }
}

public class Bullet : MonoBehaviour
{
    private float speed;
    private Vector3 direction;
    private float lifetime = 5f;
    
    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
        
        // Auto-return to pool after lifetime
        this.DelayedCall(lifetime, () => ObjectPooling.Return(this));
    }
    
    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Handle collision
            other.GetComponent<Enemy>().TakeDamage(damage);
            
            // Return to pool
            ObjectPooling.Return(this);
        }
    }
}
```

#### 2. Performance Monitoring
```csharp
public class PoolMonitor : MonoBehaviour
{
    [SerializeField] private Text statsText;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowPoolStats();
        }
    }
    
    private void ShowPoolStats()
    {
        var stats = ObjectPooling.GetPoolStats();
        var statsDisplay = new StringBuilder();
        
        foreach (var stat in stats)
        {
            var prefabName = stat.Key.name;
            var poolStat = stat.Value;
            
            statsDisplay.AppendLine($"{prefabName}:");
            statsDisplay.AppendLine($"  Hit Rate: {poolStat.HitRate:P2}");
            statsDisplay.AppendLine($"  Active: {poolStat.CurrentActiveCount}");
            statsDisplay.AppendLine($"  Peak: {poolStat.PeakActiveCount}");
            statsDisplay.AppendLine($"  Total Created: {poolStat.TotalCreated}");
        }
        
        statsText.text = statsDisplay.ToString();
    }
}
```

## 🎯 Best Practices

### Audio System
1. **Preload Critical Audio**: Load frequently used audio clips at startup
2. **Use Categories**: Organize audio by type for better volume control
3. **3D Audio Setup**: Configure spatial settings for immersive audio
4. **Memory Management**: Use Addressables for large audio files

### Data Management
1. **Backup Strategy**: Enable auto-backups for critical data
2. **Encryption**: Use encryption for sensitive player data
3. **Compression**: Enable compression for large data sets
4. **Validation**: Implement custom validators for data integrity

### AdMob Integration
1. **Frequency Control**: Don't show ads too frequently
2. **User Experience**: Always provide alternatives to ads
3. **Testing**: Use test ads during development
4. **Analytics**: Track ad performance for optimization

### UI Management
1. **Async Loading**: Use async patterns for smooth UI transitions
2. **Memory Management**: Properly dispose of UI elements
3. **State Management**: Maintain UI state across scenes
4. **Responsive Design**: Design for multiple screen sizes

### Object Pooling
1. **Pool Size**: Start with conservative estimates and monitor usage
2. **Cleanup**: Return objects to pools promptly
3. **Performance**: Monitor pool statistics to optimize sizes
4. **Memory**: Use pooling for frequently instantiated objects

## 🔧 Configuration

### Conditional Compilation Symbols
Add these symbols to your project for enhanced functionality:

- `ALL_LOG`: Enable all logging (development only)
- `DATA_LOG`: Enable data system logging
- `OBJECT_POOLING_TRACK_PERFORMANCE`: Enable pool performance tracking
- `AUTO_APPLY_SPRITE_SETTINGS`: Enable automatic sprite import settings

### Project Settings
1. **Scripting Runtime Version**: .NET Standard 2.1 (recommended)
2. **API Compatibility Level**: .NET Standard 2.1
3. **Managed Stripping Level**: Medium (for mobile optimization)

## 🐛 Troubleshooting

### Common Issues

1. **UniTask not found**: Install UniTask dependency
2. **Addressables errors**: Ensure Addressables package is installed
3. **Audio not playing**: Check AudioManager setup and database assignment
4. **AdMob compilation errors**: Install Google Mobile Ads SDK
5. **Data encryption errors**: Verify encryption keys are 16/32 characters

### Debug Mode
Enable debug logging by adding `ALL_LOG` to your scripting define symbols:
1. Go to `Edit > Project Settings > Player`
2. Under `Other Settings > Scripting Define Symbols`
3. Add `ALL_LOG`

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

### Development Setup
1. Clone the repository
2. Open in Unity 2022.3 or later
3. Install required dependencies
4. Add conditional compilation symbols for development

### Pull Request Guidelines
1. Follow existing code style and conventions
2. Add unit tests for new features
3. Update documentation as needed
4. Test thoroughly across different platforms

## 📞 Support

For issues, questions, and feature requests:
1. Check the documentation and examples
2. Search existing issues on GitHub
3. Create a new issue with detailed information
4. Join our Discord community for real-time help

## 🗺️ Roadmap

### Planned Features
- **Networking utilities** with Mirror/Netcode integration
- **Analytics integration** with multiple providers
- **Cloud save** with multiple backend support
- **Localization helpers** with CSV/JSON support
- **Performance profiling** tools and utilities
- **Build automation** scripts and CI/CD integration

### Version History
- **v1.2.0** (June 2025): Added AdMob integration and enhanced data management
- **v1.1.0** (May 2025): Added UI management system and object pooling improvements
- **v1.0.0** (April 2025): Initial release with audio, data, and core utilities

## Installation

1. Clone or download this repository
2. Import the package into your Unity project
3. Ensure you have the following dependencies:
   - UniTask
   - Unity Addressables (if using addressable audio clips)

## Dependencies

- **UniTask**: For async/await support
- **Unity.Addressables**: For addressable asset loading
- **Unity.ResourceManager**: For resource management

## Usage Examples

### Audio Management

#### Setting up Audio Database
1. Create an Audio Database asset: `Right-click → Create → TirexGame → Audio Database`
2. Configure your audio clips and categories
3. Assign the database to the AudioManager in your scene

#### Playing Audio
```csharp
// Simple audio playback
AudioService.PlaySFX("footstep");

// Advanced audio playback with options
var clipData = new AudioClipData
{
    clip = myAudioClip,
    volume = 0.8f,
    pitch = 1.2f,
    loop = true,
    fadeType = AudioFadeType.FadeIn,
    fadeDuration = 2f
};
AudioService.PlayAudio(clipData, AudioType.SFX);

// 3D positioned audio
AudioService.PlaySFX("explosion", worldPosition, AudioType.SFX);
```

#### Volume Control
```csharp
// Master volume control
AudioService.SetMasterVolume(0.7f);

// Category-specific volume
AudioService.SetMusicVolume(0.5f);
AudioService.SetSFXVolume(0.8f);
AudioService.SetUIVolume(1.0f);

// Mute/unmute categories
AudioService.SetMusicMuted(true);
AudioService.ToggleMusicMute();
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
