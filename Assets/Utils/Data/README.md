# Unity Data Management Package

Một giải pháp quản lý dữ liệu toàn diện, hiệu suất cao và dễ sử dụng cho các dự án Unity. Package này cung cấp một kiến trúc linh hoạt, thread-safe và có khả năng mở rộng để lưu trữ, tải, mã hóa, nén và xác thực dữ liệu game một cách an toàn và hiệu quả.

## 🎯 Lý do nên sử dụng

- **🔒 Bảo mật cao**: Mã hóa AES 256-bit với device-specific keys
- **⚡ Hiệu suất tối ưu**: Cache thông minh và xử lý bất đồng bộ
- **🏗️ Kiến trúc linh hoạt**: Hỗ trợ multiple repositories và data models
- **🛡️ An toàn**: Thread-safe operations và data validation
- **📱 Đa nền tảng**: Hoạt động trên tất cả platforms Unity hỗ trợ
- **⚡ Dual API**: Cả async/await và synchronous operations

## ✨ Tính Năng Nổi Bật

### 🎮 Quản lý Dữ liệu Tập trung
- **`DataManager`**: Static singleton pattern với thread-safe operations
- **Khởi tạo linh hoạt**: Hỗ trợ cấu hình tùy chỉnh và lazy initialization
- **Multi-repository**: Quản lý nhiều loại dữ liệu với các strategies lưu trữ khác nhau
- **Dual API Support**: 
  - **Async API**: Sử dụng UniTask cho operations phức tạp và I/O intensive
  - **Sync API**: Cho các tác vụ nhẹ, không cần async/await overhead

### 💾 Hệ thống Repository Linh hoạt
- **`FileDataRepository`**: Lưu trữ bền vững với encryption và compression
- **`MemoryDataRepository`**: Lưu trữ tạm thời cho testing và session data
- **Interface-based**: Dễ dàng mở rộng với custom repositories (Cloud, Database...)
- **Sync Support**: Tất cả repositories đều hỗ trợ cả sync và async operations

### 🔐 Bảo mật & Tối ưu hóa
- **Mã hóa AES 256-bit**: 
  - Device-specific encryption keys
  - Random IV cho mỗi lần mã hóa
  - Chống reverse engineering và save file manipulation
- **Nén dữ liệu thông minh**:
  - Hỗ trợ GZip, Deflate, Brotli compression
  - Automatic compression detection
  - Entropy analysis để tối ưu hiệu suất
  - **Sync compression**: Phiên bản synchronous cho lightweight operations

### ⚡ Hiệu suất Cao
- **Zero Reflection**: Interface-based validation system
- **Dual Processing**: 
  - **Async/Await**: UniTask integration cho smooth gameplay
  - **Synchronous**: Direct processing cho tác vụ nhẹ
- **Smart Caching**:
  - LRU (Least Recently Used) eviction
  - Memory usage monitoring
  - Configurable expiration times
- **Thread Pool**: File I/O operations chạy trên background threads (async mode)

### ✅ Data Validation & Integrity
- **Type-safe validation**: `IValidatable` interface cho custom validation rules
- **Data corruption recovery**: Automatic fallback to default data
- **Detailed error reporting**: Comprehensive error messages và logging
- **Sync validation**: Immediate validation cho real-time feedback

### 🎯 Event System
- **Observer Pattern**: Subscribe/Unsubscribe to data events
- **Type-safe events**: Strongly typed callbacks cho từng data model
- **Lifecycle events**: OnSaved, OnLoaded, OnDeleted, OnError

### 🤖 Tự động hóa
- **Auto-Save**: Configurable periodic saving
- **Cache cleanup**: Automatic expired cache removal
- **Error recovery**: Graceful handling of corrupted data

## 📋 Yêu cầu Hệ thống

### Unity Version
- **Minimum**: Unity 2021.3 LTS
- **Recommended**: Unity 2022.3 LTS trở lên
- **Platforms**: Tất cả platforms Unity hỗ trợ (Windows, Mac, Linux, Android, iOS, WebGL...)

### Dependencies
```json
{
  "com.cysharp.unitask": "2.3.3",
  "com.unity.nuget.newtonsoft-json": "3.2.1"
}
```

### Packages cần thiết
1. **UniTask**: Async/await operations
   - Cài đặt: Window → Package Manager → Add package from git URL
   - URL: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

2. **Newtonsoft.Json**: JSON serialization
   - Cài đặt: Window → Package Manager → Unity Registry → "com.unity.nuget.newtonsoft-json"

## 🚀 Cài đặt

### Option 1: Unity Package Manager (Recommended)
1. Mở Unity Project
2. Window → Package Manager
3. Click "+" → "Add package from git URL"
4. Nhập: `https://github.com/tojinguyen/Unity-Utilities.git?path=/Assets/Utils/Data`

### Option 2: Download và Import
1. Download source code từ [GitHub repository](https://github.com/tojinguyen/Unity-Utilities)
2. Copy thư mục `Assets/Utils/Data` vào project của bạn
3. Unity sẽ tự động compile và import package

### Option 3: UnityPackage
1. Download file `.unitypackage` từ [Releases page](https://github.com/tojinguyen/Unity-Utilities/releases)
2. Double-click để import vào Unity project
3. Chọn các files cần thiết và click "Import"

### Verification
Để kiểm tra package đã được cài đặt thành công:
```csharp
// Thêm dòng này vào một script bất kỳ
using TirexGame.Utils.Data;

// Nếu không có error, package đã ready!
Debug.Log("Data Package is ready!");
```

## 🚀 Bắt Đầu Nhanh

### Bước 1: Tạo Data Model

Tạo một class chứa dữ liệu của bạn và implement interface `IDataModel<T>`. Interface này yêu cầu bạn định nghĩa dữ liệu mặc định và logic validation (tùy chọn).

```csharp
// File: PlayerData.cs
using TirexGame.Utils.Data;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData : IDataModel<PlayerData>, IValidatable
{
    [Header("Basic Info")]
    public string PlayerName = "New Player";
    public int Level = 1;
    public float Health = 100f;
    public float Experience = 0f;
    
    [Header("Progress")]
    public DateTime LastLogin;
    public int HighScore = 0;
    public List<string> UnlockedAchievements = new();
    
    [Header("Settings")]
    public float MasterVolume = 1f;
    public bool NotificationsEnabled = true;

    /// <summary>
    /// Được gọi khi không có file save hoặc cần reset data
    /// </summary>
    public void SetDefaultData()
    {
        PlayerName = "New Player";
        Level = 1;
        Health = 100f;
        Experience = 0f;
        LastLogin = DateTime.UtcNow;
        HighScore = 0;
        UnlockedAchievements.Clear();
        MasterVolume = 1f;
        NotificationsEnabled = true;
    }

    /// <summary>
    /// Validation logic để đảm bảo tính toàn vẹn của dữ liệu
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        // Validate player name
        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            errors.Add("PlayerName cannot be empty");
        }
        else if (PlayerName.Length < 3 || PlayerName.Length > 20)
        {
            errors.Add("PlayerName must be between 3-20 characters");
        }

        // Validate game stats
        if (Level < 1 || Level > 100)
        {
            errors.Add("Level must be between 1-100");
        }

        if (Health < 0 || Health > 100)
        {
            errors.Add("Health must be between 0-100");
        }

        if (Experience < 0)
        {
            errors.Add("Experience cannot be negative");
        }

        // Validate settings
        if (MasterVolume < 0 || MasterVolume > 1)
        {
            errors.Add("MasterVolume must be between 0-1");
        }

        return errors.Count == 0;
    }
}
```

### Bước 2: Khởi tạo DataManager

**⚡ Auto-Initialization (Khuyến khích)**: DataManager được tự động khởi tạo khi runtime bắt đầu với cấu hình mặc định. Bạn không cần làm gì thêm!

**🔧 Manual Setup**: Nếu muốn tùy chỉnh cấu hình, sử dụng component `DataManagerInitializer` hoặc khởi tạo thủ công.

#### Option A: Sử dụng DataManagerInitializer Component (Đơn giản)

1. Tạo một GameObject trong Scene đầu tiên của game
2. Attach component `DataManagerInitializer`
3. Cấu hình settings trong Inspector
4. Component sẽ tự động khởi tạo DataManager với `DefaultExecutionOrder(-1000)` để chạy trước tất cả script khác

#### Option B: Khởi tạo thủ công (Kiểm soát tốt hơn)

Tạo một GameObject với script khởi tạo trong Scene đầu tiên của game (ví dụ: MainMenu, Startup Scene).

```csharp
// File: GameInitializer.cs
using UnityEngine;
using TirexGame.Utils.Data;

[DefaultExecutionOrder(-500)] // Đảm bảo chạy sớm, nhưng sau DataManagerInitializer
public class GameInitializer : MonoBehaviour
{
    [Header("Data Manager Settings")]
    [SerializeField] private bool enableEncryption = true;
    [SerializeField] private bool enableCompression = true;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

    private void Awake()
    {
        // Cấu hình DataManager (sẽ skip nếu đã được auto-initialize)
        var config = new DataManagerConfig
        {
            EnableLogging = true,
            EnableCaching = true,
            DefaultCacheExpirationMinutes = 30,
            EnableAutoSave = enableAutoSave,
            AutoSaveIntervalSeconds = autoSaveInterval
        };

        // Khởi tạo DataManager với config
        DataManager.Initialize(config);

        // Đăng ký repositories cho các loại dữ liệu
        RegisterRepositories();
        
        Debug.Log("🎮 Game Data System Initialized!");
    }

    private void RegisterRepositories()
    {
        // Repository cho PlayerData với encryption và compression
        var playerRepo = new FileDataRepository<PlayerData>(
            useEncryption: enableEncryption,
            useCompression: enableCompression
        );
        DataManager.RegisterRepository<PlayerData>(playerRepo);

        // Có thể đăng ký thêm repositories cho các data models khác
        // var settingsRepo = new FileDataRepository<GameSettings>(false, true);
        // DataManager.RegisterRepository<GameSettings>(settingsRepo);
    }

    private void OnDestroy()
    {
        // Cleanup khi object bị destroy
        DataManager.Shutdown();
    }
}
```

### Bước 3: Sử dụng DataManager trong Game

Bây giờ bạn có thể sử dụng DataManager từ bất kỳ đâu trong code để thao tác với dữ liệu.

#### Load Data khi bắt đầu game

```csharp
// File: GameController.cs hoặc PlayerController.cs
using UnityEngine;
using TirexGame.Utils.Data;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI playerNameText;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private UnityEngine.UI.Slider healthSlider;

    private PlayerData _playerData;

    private async void Start()
    {
        await LoadPlayerData();
        UpdateUI();
        
        // Subscribe to data events
        DataManager.SubscribeToDataEvents<PlayerData>(
            onSaved: OnPlayerDataSaved,
            onLoaded: OnPlayerDataLoaded
        );
    }

    private async UniTask LoadPlayerData()
    {
        try
        {
            // Tự động load từ cache hoặc file, tạo default nếu chưa có
            _playerData = await DataManager.GetDataAsync<PlayerData>();
            Debug.Log($"✅ Loaded player: {_playerData.PlayerName}, Level: {_playerData.Level}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Failed to load player data: {ex.Message}");
            // Fallback to default data
            _playerData = new PlayerData();
            _playerData.SetDefaultData();
        }
    }

    private void UpdateUI()
    {
        if (_playerData != null)
        {
            playerNameText.text = _playerData.PlayerName;
            levelText.text = $"Level {_playerData.Level}";
            healthSlider.value = _playerData.Health / 100f;
        }
    }

    // Event callbacks
    private void OnPlayerDataSaved(PlayerData data)
    {
        Debug.Log($"💾 Player data saved! Level: {data.Level}");
    }

    private void OnPlayerDataLoaded(PlayerData data)
    {
        Debug.Log($"📂 Player data loaded! Welcome back, {data.PlayerName}!");
    }

    private void OnDestroy()
    {
        // Cleanup để tránh memory leaks
        if (DataManager.IsInitialized)
        {
            DataManager.UnsubscribeFromDataEvents<PlayerData>(
                onSaved: OnPlayerDataSaved,
                onLoaded: OnPlayerDataLoaded
            );
        }
    }
}
```

#### Save Data khi có thay đổi

```csharp
// Ví dụ: Player level up
public async void LevelUp()
{
    _playerData.Level++;
    _playerData.Experience = 0;
    _playerData.Health = 100f; // Full heal on level up

    // Save changes
    bool success = await DataManager.SaveDataAsync(_playerData);
    if (success)
    {
        Debug.Log($"🎉 Level up! Now level {_playerData.Level}");
        UpdateUI();
    }
    else
    {
        Debug.LogError("❌ Failed to save level up data!");
    }
}

// Ví dụ: Player takes damage
public async void TakeDamage(float damage)
{
    _playerData.Health = Mathf.Max(0, _playerData.Health - damage);
    
    // Auto-save critical data like health
    await DataManager.SaveDataAsync(_playerData);
    UpdateUI();
    
    if (_playerData.Health <= 0)
    {
        HandlePlayerDeath();
    }
}

// Ví dụ: Update player settings
public async void UpdatePlayerName(string newName)
{
    if (string.IsNullOrWhiteSpace(newName))
    {
        Debug.LogWarning("⚠️ Invalid player name!");
        return;
    }

    _playerData.PlayerName = newName;
    _playerData.LastLogin = System.DateTime.UtcNow;

    bool success = await DataManager.SaveDataAsync(_playerData);
    if (success)
    {
        Debug.Log($"✅ Player name updated to: {newName}");
        UpdateUI();
    }
}
```

### Bước 4: Advanced Usage - Events và Caching

```csharp
// File: DataEventHandler.cs
using UnityEngine;
using TirexGame.Utils.Data;

public class DataEventHandler : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to multiple data types
        DataManager.SubscribeToDataEvents<PlayerData>(
            onSaved: OnPlayerSaved,
            onLoaded: OnPlayerLoaded,
            onDeleted: OnPlayerDeleted
        );

        // Subscribe to global events
        DataManager.OnDataError += OnDataError;
    }

    private void OnPlayerSaved(PlayerData data)
    {
        // Update UI, show save confirmation, trigger achievements, etc.
        ShowNotification($"Game saved! Level {data.Level}");
    }

    private void OnPlayerLoaded(PlayerData data)
    {
        // Initialize game state, update UI, log analytics
        Debug.Log($"Welcome back, {data.PlayerName}! Last seen: {data.LastLogin}");
    }

    private void OnPlayerDeleted(string key)
    {
        // Handle data deletion, reset UI, show confirmation
        ShowNotification("Player data deleted!");
    }

    private void OnDataError(System.Type dataType, System.Exception error)
    {
        Debug.LogError($"Data error for {dataType.Name}: {error.Message}");
        // Handle error: show error message, attempt recovery, etc.
    }

    private void ShowNotification(string message)
    {
        // Implement your notification system here
        Debug.Log($"🔔 {message}");
    }

    // Cache management examples
    public void ClearPlayerCache()
    {
        DataManager.ClearCache(nameof(PlayerData));
        Debug.Log("Player cache cleared");
    }

    public void ClearAllCache()
    {
        DataManager.ClearCache();
        Debug.Log("All cache cleared");
    }

    private void OnDestroy()
    {
        // Always cleanup subscriptions
        if (DataManager.IsInitialized)
        {
            DataManager.UnsubscribeFromDataEvents<PlayerData>(
                onSaved: OnPlayerSaved,
                onLoaded: OnPlayerLoaded,
                onDeleted: OnPlayerDeleted
            );
            DataManager.OnDataError -= OnDataError;
        }
    }
}
```

## ⚡ Synchronous API - Cho Tác Vụ Nhẹ

**Từ phiên bản mới**, package hỗ trợ **Synchronous API** cho các tác vụ nhẹ mà không cần overhead của async/await. API này phù hợp cho:

- Lưu/tải dữ liệu nhẹ (< 1MB)
- Operations trong game loop
- Quick access cho cached data
- Testing và debugging

### 🔄 So Sánh Async vs Sync

| **Tính năng** | **Async API** | **Sync API** |
|-------------|-------------|-------------|
| **Performance** | Tối ưu cho I/O heavy | Tối ưu cho lightweight operations |
| **Thread Safety** | Thread pool + await | Main thread execution |
| **Best For** | Large files, complex operations | Small data, quick access |
| **Error Handling** | Full async exception handling | Direct exception handling |
| **Memory Usage** | Slightly higher (async state machine) | Lower memory footprint |

### 📖 Sync API Usage Examples

#### Basic Load/Save Operations

```csharp
using TirexGame.Utils.Data;
using UnityEngine;

public class SyncDataExample : MonoBehaviour
{
    private PlayerData _playerData;

    private void Start()
    {
        // Initialize với MemoryRepository cho sync operations nhanh
        DataManager.Initialize();
        DataManager.RegisterRepository(new MemoryDataRepository<PlayerData>());
        
        // Load data đồng bộ - không cần await!
        LoadPlayerDataSync();
    }

    /// <summary>
    /// Load dữ liệu đồng bộ - phù hợp cho tác vụ nhẹ
    /// </summary>
    private void LoadPlayerDataSync()
    {
        try
        {
            // Sử dụng sync API - không cần await hoặc UniTask!
            _playerData = DataManager.GetData<PlayerData>();
            
            Debug.Log($"✅ Sync loaded: {_playerData.PlayerName}, Level: {_playerData.Level}");
            UpdateUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Sync load failed: {ex.Message}");
            _playerData = new PlayerData();
            _playerData.SetDefaultData();
        }
    }

    /// <summary>
    /// Save dữ liệu đồng bộ - lý tưởng cho game loop
    /// </summary>
    private void SavePlayerDataSync()
    {
        try
        {
            // Immediate save operation
            bool success = DataManager.SaveData(_playerData);
            
            if (success)
            {
                Debug.Log($"💾 Sync saved successfully!");
            }
            else
            {
                Debug.LogError("❌ Sync save failed!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Sync save error: {ex.Message}");
        }
    }

    /// <summary>
    /// Quick data operations trong Update loop
    /// </summary>
    private void Update()
    {
        // Example: Quick save when player moves (lightweight)
        if (Input.GetKeyDown(KeyCode.S))
        {
            _playerData.LastLogin = System.DateTime.Now;
            DataManager.SaveData(_playerData); // Instant save
        }
        
        // Example: Quick check if data exists
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool exists = DataManager.Exists<PlayerData>();
            Debug.Log($"Data exists: {exists}");
        }
        
        // Example: Get all available keys
        if (Input.GetKeyDown(KeyCode.K))
        {
            var keys = DataManager.GetAllKeys<PlayerData>();
            Debug.Log($"Available keys: [{string.Join(", ", keys)}]");
        }
    }
    
    private void UpdateUI()
    {
        // Update UI immediately after sync load
        // No need for async/await pattern
    }
}
```

#### Complete Sync API Reference

```csharp
// 1. Load Data Synchronously
T data = DataManager.GetData<T>(key);

// 2. Save Data Synchronously  
bool success = DataManager.SaveData<T>(data, key);

// 3. Delete Data Synchronously
bool deleted = DataManager.DeleteData<T>(key);

// 4. Check Existence Synchronously
bool exists = DataManager.Exists<T>(key);

// 5. Get All Keys Synchronously
IEnumerable<string> keys = DataManager.GetAllKeys<T>();
```

### ⚠️ Sync API Best Practices

#### ✅ Khi nên sử dụng Sync API:
- **Settings/Preferences**: Lưu các cài đặt game (volume, graphics, controls)
- **Quick save states**: Lưu checkpoint nhỏ trong game loop
- **UI state management**: Lưu trạng thái UI, selected items
- **Cache access**: Truy cập dữ liệu đã được cache
- **Testing**: Unit tests và debugging

#### ❌ Khi KHÔNG nên sử dụng Sync API:
- **Large files**: Files > 1MB (dùng async)
- **Network operations**: Luôn dùng async cho network I/O
- **Complex processing**: Encryption/compression heavy data
- **Mobile performance**: Trên mobile, prefer async để tránh ANR

#### Performance Optimization

```csharp
public class PerformanceOptimizedSync : MonoBehaviour
{
    private void OptimizedDataOperations()
    {
        // ✅ Good: Use MemoryRepository for sync operations
        DataManager.RegisterRepository(new MemoryDataRepository<PlayerData>());
        
        // ✅ Good: Batch operations where possible
        var data1 = DataManager.GetData<PlayerData>("player1");
        var data2 = DataManager.GetData<PlayerData>("player2");
        var data3 = DataManager.GetData<PlayerData>("player3");
        
        // Process all data...
        
        // Save all at once
        DataManager.SaveData(data1, "player1");
        DataManager.SaveData(data2, "player2"); 
        DataManager.SaveData(data3, "player3");
        
        // ❌ Bad: Don't use FileRepository with encryption for frequent sync ops
        // var fileRepo = new FileDataRepository<PlayerData>(useEncryption: true);
        // This will block main thread during encryption
    }
    
    [ContextMenu("Performance Test")]
    private void PerformanceTest()
    {
        // Test sync vs async performance
        const int iterations = 1000;
        
        // Sync test
        var syncStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var data = new PlayerData { PlayerName = $"Player{i}" };
            DataManager.SaveData(data, $"sync_test_{i}");
            var loaded = DataManager.GetData<PlayerData>($"sync_test_{i}");
        }
        syncStopwatch.Stop();
        
        Debug.Log($"Sync: {syncStopwatch.ElapsedMilliseconds}ms for {iterations} operations");
        Debug.Log($"Sync: {(float)syncStopwatch.ElapsedMilliseconds / iterations:F2}ms per operation");
    }
}
```

### 🏗️ Sync API Architecture

Sync API được implement ở các levels:

1. **DataManager**: Sync methods như `GetData<T>()`, `SaveData<T>()`
2. **DataRepository**: `IDataRepositorySync<T>` interface
3. **Supporting Classes**: 
   - `DataValidator.Validate()` - sync validation
   - `DataCompressor.CompressBytes()` - sync compression
   - Cache access - always synchronous

```csharp
// Repository level sync implementation
public class CustomSyncRepository<T> : IDataRepositorySync<T> where T : class, IDataModel<T>
{
    public T Load(string key)
    {
        // Your sync load implementation
        return data;
    }
    
    public bool Save(string key, T data)
    {
        // Your sync save implementation
        return true;
    }
    
    // Implement other sync methods...
}
```
```

## 🏗️ Cấu Trúc Package

*   **`DataManager.cs`**: Class chính, quản lý toàn bộ hệ thống.
*   **`DataRepository.cs`**: Định nghĩa các interface `IDataRepository` và các implement mẫu (`FileDataRepository`, `MemoryDataRepository`).
*   **`IDataModel.cs`**: Interface cơ bản cho tất cả các class dữ liệu.
*   **`IValidatable.cs`**: Interface cho logic validation hiệu suất cao.
*   **`DataValidator.cs`**: Class thực thi việc kiểm tra dữ liệu qua `IValidatable`.
*   **`DataEncryptor.cs`**: Xử lý mã hóa và giải mã dữ liệu (AES).
*   **`DataCompressor.cs`**: Xử lý nén và giải nén dữ liệu.
*   **`DataCacheManager.cs`**: Quản lý bộ nhớ đệm (cache).
*   **`DataEventManager.cs`**: Quản lý và phát các sự kiện liên quan đến dữ liệu.

## 🔧 Tùy Chỉnh

*   **Tạo Repository mới**: Bạn có thể dễ dàng tạo Repository riêng (ví dụ: `CloudDataRepository`, `DatabaseRepository`) bằng cách implement `IDataRepository<T>`.
*   **Thay đổi thuật toán mã hóa**: Chỉnh sửa `DataEncryptor.cs` để sử dụng thuật toán khác nếu cần.
*   **Cấu hình `DataManager`**: Các tùy chọn như auto-save, caching, logging có thể được điều chỉnh trực tiếp trên `DataManager` component trong Unity Editor.


---

```

## 🛠️ Troubleshooting

### Common Issues và Solutions

#### ❌ "DataManager not initialized" Error

**Vấn đề**: Lỗi xuất hiện khi gọi DataManager trước khi khởi tạo.

**Giải pháp**:
```csharp
// Đảm bảo gọi Initialize trước khi sử dụng
DataManager.Initialize();

// Hoặc kiểm tra trạng thái
if (!DataManager.IsInitialized)
{
    DataManager.Initialize();
}
```

#### ❌ "Repository not found for type" Error

**Vấn đề**: Không tìm thấy repository cho data type.

**Giải pháp**:
```csharp
// Đăng ký repository trước khi sử dụng
var repo = new FileDataRepository<PlayerData>(true, true);
DataManager.RegisterRepository<PlayerData>(repo);

// Sau đó mới load/save data
var data = await DataManager.GetDataAsync<PlayerData>();
```

#### ❌ Data Validation Fails

**Vấn đề**: Data không pass validation và không được lưu.

**Giải pháp**:
```csharp
// Kiểm tra validation trước khi save
if (data.Validate(out List<string> errors))
{
    await DataManager.SaveDataAsync(data);
}
else
{
    Debug.LogError($"Validation failed: {string.Join(", ", errors)}");
    // Fix data hoặc sử dụng default values
}
```

#### ❌ File Access/Permission Issues

**Vấn đề**: Không thể đọc/ghi file do quyền truy cập.

**Giải pháp**:
```csharp
// Sử dụng try-catch để handle exceptions
try
{
    var data = await DataManager.GetDataAsync<PlayerData>();
}
catch (UnauthorizedAccessException ex)
{
    Debug.LogError($"File access denied: {ex.Message}");
    // Fallback to memory repository hoặc request permissions
}
catch (DirectoryNotFoundException ex)
{
    Debug.LogError($"Directory not found: {ex.Message}");
    // Directory sẽ được tạo tự động trong lần save tiếp theo
}
```

#### ⚠️ Memory Leaks với Events

**Vấn đề**: Không unsubscribe events dẫn đến memory leaks.

**Giải pháp**:
```csharp
public class GameController : MonoBehaviour
{
    private void Start()
    {
        DataManager.SubscribeToDataEvents<PlayerData>(onSaved: OnPlayerSaved);
    }

    private void OnDestroy()
    {
        // QUAN TRỌNG: Luôn unsubscribe
        if (DataManager.IsInitialized)
        {
            DataManager.UnsubscribeFromDataEvents<PlayerData>(onSaved: OnPlayerSaved);
        }
    }
}
```

#### 🐛 Corrupted Save Files

**Vấn đề**: File save bị hỏng không load được.

**Giải pháp**:
```csharp
// DataManager tự động handle corrupted files
try
{
    var data = await DataManager.GetDataAsync<PlayerData>();
    // Nếu file corrupt, data sẽ là default values
}
catch (Exception ex)
{
    Debug.LogWarning($"Save file corrupted, using defaults: {ex.Message}");
    // Data vẫn được trả về với default values
}

// Hoặc manually delete corrupted files
public async void ResetPlayerData()
{
    await DataManager.DeleteDataAsync<PlayerData>();
    var freshData = await DataManager.GetDataAsync<PlayerData>();
    // freshData sẽ có default values
}
```

#### ⚡ Performance Issues

**Vấn đề**: Slow performance khi load/save data.

**Giải pháp**:

1. **Enable Caching**:
```csharp
var config = new DataManagerConfig
{
    EnableCaching = true,
    DefaultCacheExpirationMinutes = 30
};
DataManager.Initialize(config);
```

2. **Optimize Compression**:
```csharp
// Chỉ enable compression cho data lớn
var repo = new FileDataRepository<PlayerData>(
    useEncryption: true,
    useCompression: data => data.Length > 1024 // Chỉ nén nếu > 1KB
);
```

3. **Batch Operations**:
```csharp
// Thay vì save nhiều lần
await DataManager.SaveDataAsync(playerData);
await DataManager.SaveDataAsync(settingsData);

// Save một lần với auto-save
DataManager.EnableAutoSave(intervalSeconds: 60); // Auto-save mỗi phút
```

#### 🔐 Encryption Key Issues

**Vấn đề**: Data không thể decrypt sau khi chuyển device.

**Lý do**: Encryption key dựa trên `SystemInfo.deviceUniqueIdentifier`.

**Giải pháp**:
```csharp
// Option 1: Backup/Restore system
public async void BackupPlayerData()
{
    var data = await DataManager.GetDataAsync<PlayerData>();
    var json = JsonConvert.SerializeObject(data);
    // Upload to cloud hoặc export to file
}

// Option 2: Custom encryption key
public class CustomDataRepository<T> : FileDataRepository<T>
{
    public CustomDataRepository(string customKey) 
        : base(true, true, customKey)
    {
    }
}
```

#### 📱 Platform-Specific Issues

**WebGL**: File system limitations
```csharp
#if UNITY_WEBGL
// Sử dụng PlayerPrefs hoặc IndexedDB repository
var repo = new PlayerPrefsRepository<PlayerData>();
#else
var repo = new FileDataRepository<PlayerData>(true, true);
#endif
DataManager.RegisterRepository<PlayerData>(repo);
```

**Mobile**: Storage permissions
```csharp
// Kiểm tra permission trước khi save
if (Application.platform == RuntimePlatform.Android)
{
    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
    {
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }
}
```

### Debug Tips

#### Enable Detailed Logging
```csharp
var config = new DataManagerConfig
{
    EnableLogging = true,
    LogLevel = LogLevel.Verbose // Sẽ log mọi operations
};
```

#### Inspect Cache State
```csharp
// Check cache statistics
var cacheInfo = DataManager.GetCacheInfo();
Debug.Log($"Cache entries: {cacheInfo.EntryCount}, Memory usage: {cacheInfo.MemoryUsage}");
```

#### Validate Installation
```csharp
[MenuItem("Tools/Data Package/Validate Installation")]
public static void ValidateInstallation()
{
    try
    {
        DataManager.Initialize();
        var repo = new FileDataRepository<PlayerData>(false, false);
        DataManager.RegisterRepository<PlayerData>(repo);
        Debug.Log("✅ Data Package installation is valid!");
    }
    catch (Exception ex)
    {
        Debug.LogError($"❌ Installation issue: {ex.Message}");
    }
}
```

---

## ⚙️ How It Works: A Deep Dive

Phần này phân tích sâu về kiến trúc và luồng dữ liệu bên trong package, chi tiết đến từng class và cấu trúc dữ liệu cốt lõi.

### Luồng Tải Dữ Liệu (`GetDataAsync<T>`)

Đây là chuỗi sự kiện chi tiết khi `DataManager.Instance.GetDataAsync<T>()` được gọi:


1.  **[DataManager] Bắt đầu**: `DataManager` nhận yêu cầu. Nó xác định `key` (mặc định là tên của Type `T`) và `Type` của dữ liệu cần tải.


2.  **[DataManager → DataCacheManager] Kiểm tra Cache**: `DataManager` gọi `_cacheManager.TryGetCached<T>(key, out T data)`.
    *   **Cache Hit**: Nếu `_cacheManager` tìm thấy một `CacheEntry` hợp lệ (chưa hết hạn) trong `Dictionary<string, CacheEntry>`, nó sẽ trả về dữ liệu ngay lập tức. Luồng xử lý kết thúc tại đây, mang lại hiệu suất cao nhất.
    *   **Cache Miss**: Nếu không tìm thấy hoặc entry đã hết hạn, luồng xử lý tiếp tục.


3.  **[DataManager] Tìm kiếm Repository**: `DataManager` sử dụng `Dictionary<Type, IDataRepository>` của nó để tìm repository đã được đăng ký cho `Type T`. Nếu không tìm thấy, quá trình thất bại.


4.  **[DataManager → FileDataRepository] Ủy quyền Tải**: `DataManager` gọi `repository.LoadAsync(key)`.
    *   **Xác định đường dẫn**: `FileDataRepository` tạo đường dẫn file đầy đủ: `Application.persistentDataPath/T.Name/{key}.dat`.
    *   **Đọc File Bất đồng bộ**: Đọc toàn bộ nội dung file thành một mảng `byte[]` bằng `System.IO.File.ReadAllBytesAsync()`. Nếu file không tồn tại, trả về `null`.
    *   **Pipeline Giải mã**: Dữ liệu `byte[]` đi qua một pipeline ngược:
        a.  **Giải mã (Decryption)**: Nếu `_useEncryption` là `true`, `DataEncryptor.Decrypt(fileBytes)` được gọi. Nó tách 16 byte IV đầu tiên và dùng nó cùng với key (lấy từ `SystemInfo.deviceUniqueIdentifier`) để giải mã phần còn lại bằng thuật toán AES.
        b.  **Giải nén (Decompression)**: Nếu `_useCompression` là `true`, `DataCompressor.DecompressBytesAsync(decryptedBytes)` được gọi. Nó sử dụng stream tương ứng (GZip, Deflate, Brotli) để giải nén dữ liệu.
        c.  **Decode & Deserialize**: Mảng `byte[]` cuối cùng được chuyển thành chuỗi JSON (`Encoding.UTF8.GetString()`), sau đó `Newtonsoft.Json.JsonConvert.DeserializeObject<T>()` chuyển chuỗi này thành object `T`.

5.  **[DataManager] Xử lý Kết quả**: `DataManager` nhận lại object `T` (hoặc `null`) từ repository.
    *   **Trường hợp không có file**: Nếu kết quả là `null`, `DataManager` tạo một instance mới của `T`, gọi `instance.SetDefaultData()`, và coi đây là dữ liệu để xử lý tiếp.
    *   **[DataManager → DataValidator] Xác thực Dữ liệu**: Dữ liệu (tải được hoặc mặc định) được đưa cho `_validator.ValidateAsync(data)`. `DataValidator` sẽ gọi phương thức `data.Validate()` từ interface `IValidatable`.
        *   Nếu không hợp lệ (ví dụ, file save bị hỏng), `DataManager` sẽ log lỗi và trả về dữ liệu mặc định (`new T().SetDefaultData()`) để đảm bảo game không bị crash.
    *   **[DataManager → DataCacheManager] Lưu vào Cache**: Nếu dữ liệu hợp lệ, `DataManager` gọi `_cacheManager.Cache(key, data)` để lưu trữ nó cho những lần truy cập sau.
    *   **[DataManager → DataEventManager] Phát Sự kiện**: `_eventManager.RaiseDataLoaded(typeof(T), data, key)` được gọi để thông báo cho các hệ thống khác rằng dữ liệu đã được tải thành công.

6.  **[DataManager] Hoàn tất**: Trả về object dữ liệu cuối cùng cho nơi đã gọi nó.

### Luồng Lưu Dữ Liệu (`SaveDataAsync<T>`)

1.  **[DataManager] Bắt đầu**: Nhận object `data` và `key`.

2.  **[DataManager → DataValidator] Xác thực Dữ liệu**: Gọi `_validator.ValidateAsync(data)` để đảm bảo dữ liệu toàn vẹn trước khi lưu. Nếu không hợp lệ, quá trình dừng lại và trả về `false`.

3.  **[DataManager] Tìm kiếm Repository**: Tương tự như luồng tải, tìm repository phù hợp.

4.  **[DataManager → FileDataRepository] Ủy quyền Lưu**: Gọi `repository.SaveAsync(key, data)`.
    *   **Pipeline Mã hóa**:
        a.  **Serialize**: `Newtonsoft.Json.JsonConvert.SerializeObject(data)` chuyển object thành chuỗi JSON.
        b.  **Encode**: `Encoding.UTF8.GetBytes(json)` chuyển chuỗi thành `byte[]`.
        c.  **Nén (Compression)**: Nếu `_useCompression` là `true`, `DataCompressor.CompressBytesAsync()` được gọi để giảm kích thước `byte[]`.
        d.  **Mã hóa (Encryption)**: Nếu `_useEncryption` là `true`, `DataEncryptor.Encrypt()` được gọi. Nó tạo ra một IV (Initialization Vector) ngẫu nhiên 16 byte, gắn vào đầu dữ liệu đã được mã hóa bằng AES, và trả về một `byte[]` duy nhất.
    *   **Ghi File Bất đồng bộ**: Mảng `byte[]` cuối cùng được ghi vào file bằng `System.IO.File.WriteAllBytesAsync()`.

5.  **[DataManager] Xử lý sau khi lưu**:
    *   **[DataManager → DataCacheManager] Cập nhật Cache**: Gọi `_cacheManager.Cache(key, data)` để đảm bảo cache luôn chứa phiên bản mới nhất.
    *   **[DataManager → DataEventManager] Phát Sự kiện**: Gọi `_eventManager.RaiseDataSaved(typeof(T), data, key)`.

6.  **[DataManager] Hoàn tất**: Trả về `true` để báo hiệu lưu thành công.

### Phân Tích Chi Tiết Các Thành Phần Cốt Lõi

-   **`DataManager`**:
    -   **Vai trò**: Singleton, Facade, Orchestrator. Là điểm truy cập duy nhất cho mọi hoạt động dữ liệu.
    -   **Cấu trúc dữ liệu chính**: `private readonly Dictionary<Type, IDataRepository> _repositories`. Một dictionary dùng `Type` của data model làm key để tra cứu nhanh repository tương ứng. Điều này cho phép quản lý nhiều loại dữ liệu với các cách lưu trữ khác nhau (ví dụ `PlayerData` lưu vào file, `SessionData` lưu vào memory).
    -   **Hoạt động**: Điều phối các service con (`_cacheManager`, `_validator`, `_eventManager`). Chạy một `UniTaskVoid` lặp vô hạn (`StartAutoSave`) để gọi `SaveAllAsync` định kỳ.


-   **`IDataRepository<T>` / `FileDataRepository<T>`**:
    -   **Vai trò**: Lớp trừu tượng hóa truy cập dữ liệu (Data Access Layer). `FileDataRepository` là một implementation cụ thể để làm việc với file system.
    -   **Hoạt động**: Chứa logic pipeline mã hóa/giải mã, nén/giải nén, và serialize/deserialize. Sử dụng `UniTask.RunOnThreadPool` cho các tác vụ file I/O để không block main thread của Unity. Tên file được tạo theo format `{basePath}/{TypeName}/{key}.dat`.


-   **`DataCacheManager`**:
    -   **Vai trò**: Cung cấp một lớp cache in-memory để giảm thiểu đọc/ghi ổ đĩa.
    -   **Cấu trúc dữ liệu chính**: `private readonly Dictionary<string, CacheEntry> _cache`.
    -   **`CacheEntry` Class**: Một object chứa:
        -   `object Data`: Dữ liệu thực tế.
        -   `DateTime CachedAt`, `DateTime ExpiresAt`: Quản lý thời gian sống của cache.
        -   `DateTime LastAccessed`, `int AccessCount`: Dùng cho các chiến lược eviction như LRU (Least Recently Used) hoặc LFU (Least Frequently Used).
        -   `long SizeBytes`: Ước tính dung lượng bộ nhớ của object.
    -   **Hoạt động**: Cung cấp các phương thức `Cache`, `TryGetCached`, `RemoveFromCache`. Tự động chạy `Cleanup()` để xóa các entry hết hạn. Khi bộ nhớ cache đầy (`_maxMemoryBytes`), nó sẽ loại bỏ entry ít được sử dụng gần đây nhất (`EvictLeastRecentlyUsed`).


-   **`DataEncryptor`**:
    -   **Vai trò**: Đảm bảo tính bảo mật và chống chỉnh sửa file save.
    -   **Hoạt động**: Sử dụng `System.Security.Cryptography.Aes`.
        -   **Key Derivation**: Key AES 256-bit được tạo ra một lần duy nhất bằng `Rfc2898DeriveBytes`. Nó kết hợp `SystemInfo.deviceUniqueIdentifier` với một chuỗi `salt` cố định. Điều này làm cho key là duy nhất cho mỗi thiết bị, ngăn chặn việc chia sẻ file save.
        -   **Encryption**: Tạo một IV (Initialization Vector) 16-byte ngẫu nhiên cho mỗi lần mã hóa. Dữ liệu trả về có cấu trúc: `[16 bytes IV][Dữ liệu đã mã hóa]`.
        -   **Decryption**: Đọc 16 byte đầu làm IV, phần còn lại là dữ liệu mã hóa để giải mã.


-   **`DataCompressor`**:
    -   **Vai trò**: Giảm dung lượng file lưu trữ.
    -   **Hoạt động**: Sử dụng các class `GZipStream`, `DeflateStream`, `BrotliStream` từ `System.IO.Compression`. Cung cấp các hàm tiện ích như `ShouldCompress` (dựa trên tính toán entropy của một mẫu dữ liệu) để quyết định có nên nén hay không, tránh lãng phí CPU cho dữ liệu đã được nén hoặc dữ liệu ngẫu nhiên.


-   **`DataValidator` / `IValidatable`**:
    -   **Vai trò**: Đảm bảo tính toàn vẹn của dữ liệu.
    -   **Hoạt động**: Thay vì dùng reflection (chậm), nó yêu cầu data model implement interface `IValidatable`. `DataValidator` chỉ cần ép kiểu và gọi phương thức `Validate()`. Cách tiếp cận này nhanh, an toàn về kiểu và dễ dàng cho việc unit test.


-   **`DataEventManager`**:
    -   **Vai trò**: Triển khai mẫu Observer (Pub/Sub) để các hệ thống có thể phản ứng với thay đổi dữ liệu mà không cần liên kết trực tiếp với `DataManager`.
    -   **Cấu trúc dữ liệu chính**: `private readonly Dictionary<Type, object> _subscriptions`. `object` ở đây thực chất là một instance của `DataEventSubscription<T>`, một class nội bộ chứa các `Action<T>` cho `OnSaved`, `OnLoaded`, v.v.
    -   **Hoạt động**: Cung cấp các phương thức `Subscribe` và `Unsubscribe` an toàn với thread-safe (`lock`). Khi một sự kiện xảy ra, nó tìm các `Action` tương ứng trong dictionary và `Invoke()` chúng.

> 💡 **Sơ đồ Pipeline Dữ liệu với `FileDataRepository`**:
> ```
> Save:  Object → (Serialize) → JSON → (Compress) → byte[] → (Encrypt) → Encrypted byte[] → File
> Load:  File → Encrypted byte[] → (Decrypt) → byte[] → (Decompress) → JSON → (Deserialize) → Object
> ```

---

## 📚 API Reference

Đây là tài liệu tham khảo chi tiết các API công khai của `DataManager`.

### Core Methods

#### `Initialize(DataManagerConfig config = null)`
-   **Mô tả**: Khởi tạo DataManager với cấu hình tùy chỉnh. Phải được gọi trước khi sử dụng bất kỳ API nào.
-   **Tham số**:
    -   `config` (tùy chọn): Cấu hình cho DataManager. Nếu `null`, sẽ sử dụng config mặc định.
-   **Thread-safe**: ✅ Có
-   **Ví dụ**:
    ```csharp
    var config = new DataManagerConfig
    {
        EnableLogging = true,
        EnableCaching = true,
        EnableAutoSave = true,
        AutoSaveIntervalSeconds = 300f
    };
    DataManager.Initialize(config);
    ```

#### `RegisterRepository<T>(IDataRepository<T> repository)`
-   **Mô tả**: Đăng ký repository để xử lý một loại dữ liệu cụ thể. Mỗi Type chỉ có thể có một repository.
-   **Tham số**:
    -   `repository`: Instance của repository (FileDataRepository, MemoryDataRepository, hoặc custom).
-   **Exceptions**: `ArgumentNullException`, `InvalidOperationException`
-   **Ví dụ**:
    ```csharp
    var repo = new FileDataRepository<PlayerData>(
        useEncryption: true, 
        useCompression: true
    );
    DataManager.RegisterRepository<PlayerData>(repo);
    ```

#### `async UniTask<T> GetDataAsync<T>(string key = null) where T : IDataModel<T>, new()`
-   **Mô tả**: Load dữ liệu bất đồng bộ. Tự động fallback về default data nếu không tìm thấy hoặc data corrupt.
-   **Tham số**:
    -   `key` (tùy chọn): Unique identifier. Mặc định là `typeof(T).Name`.
-   **Trả về**: `UniTask<T>` chứa data object
-   **Cache behavior**: Tự động cache kết quả để tăng tốc lần truy cập sau
-   **Error handling**: Tự động handle corrupted files và trả về default data
-   **Ví dụ**:
    ```csharp
    // Load with default key
    PlayerData player = await DataManager.GetDataAsync<PlayerData>();
    
    // Load with custom key
    PlayerData backup = await DataManager.GetDataAsync<PlayerData>("backup_save");
    ```

#### `async UniTask<bool> SaveDataAsync<T>(T data, string key = null)`
-   **Mô tả**: Lưu dữ liệu bất đồng bộ với validation và error handling.
-   **Tham số**:
    -   `data`: Object cần lưu
    -   `key` (tùy chọn): Unique identifier
-   **Trả về**: `true` nếu thành công, `false` nếu thất bại
-   **Validation**: Tự động validate data trước khi lưu (nếu implement `IValidatable`)
-   **Side effects**: Update cache, trigger events
-   **Ví dụ**:
    ```csharp
    playerData.Level++;
    bool success = await DataManager.SaveDataAsync(playerData);
    if (!success)
    {
        Debug.LogError("Failed to save player data!");
    }
    ```

#### `async UniTask<bool> DeleteDataAsync<T>(string key = null)`
-   **Mô tả**: Xóa dữ liệu từ storage và cache.
-   **Tham số**:
    -   `key` (tùy chọn): Identifier của data cần xóa
-   **Trả về**: `true` nếu thành công
-   **Side effects**: Remove from cache, trigger OnDeleted event
-   **Ví dụ**:
    ```csharp
    // Delete default save
    await DataManager.DeleteDataAsync<PlayerData>();
    
    // Delete specific save slot
    await DataManager.DeleteDataAsync<PlayerData>("save_slot_2");
    ```

### Repository Management

#### `bool IsRepositoryRegistered<T>()`
-   **Mô tả**: Kiểm tra xem repository cho type T đã được đăng ký chưa.
-   **Trả về**: `true` nếu đã có repository

#### `void UnregisterRepository<T>()`
-   **Mô tả**: Hủy đăng ký repository cho type T.
-   **Side effects**: Clear cache cho type này

### Event System

#### `SubscribeToDataEvents<T>(Action<T> onSaved = null, Action<T> onLoaded = null, Action<string> onDeleted = null)`
-   **Mô tả**: Đăng ký event callbacks cho một data type.
-   **Tham số**:
    -   `onSaved`: Được gọi sau khi save thành công
    -   `onLoaded`: Được gọi sau khi load thành công  
    -   `onDeleted`: Được gọi sau khi delete thành công (parameter là key)
-   **Thread-safe**: ✅ Có
-   **Ví dụ**:
    ```csharp
    DataManager.SubscribeToDataEvents<PlayerData>(
        onSaved: (data) => Debug.Log($"Saved level {data.Level}"),
        onLoaded: (data) => Debug.Log($"Loaded {data.PlayerName}"),
        onDeleted: (key) => Debug.Log($"Deleted save {key}")
    );
    ```

#### `UnsubscribeFromDataEvents<T>(...)`
-   **Mô tả**: Hủy đăng ký event callbacks. **Quan trọng**: Phải sử dụng cùng method reference.
-   **Memory leak prevention**: Luôn gọi trong `OnDestroy()`

### Global Events

#### `static event Action<Type, object> OnDataSaved`
-   **Mô tả**: Global event được trigger khi bất kỳ data nào được save

#### `static event Action<Type, object> OnDataLoaded`
-   **Mô tả**: Global event được trigger khi bất kỳ data nào được load

#### `static event Action<Type, Exception> OnDataError`
-   **Mô tả**: Global event được trigger khi có lỗi xảy ra

### Cache Management

#### `ClearCache(string key = null)`
-   **Mô tả**: Xóa cache entries.
-   **Tham số**:
    -   `key` (tùy chọn): Nếu null, xóa toàn bộ cache
-   **Performance**: Không ảnh hưởng đến saved data

#### `GetCacheInfo()`
-   **Mô tả**: Lấy thông tin về cache hiện tại.
-   **Trả về**: Object chứa statistics như entry count, memory usage

### Utility Methods

#### `bool IsInitialized { get; }`
-   **Mô tả**: Property kiểm tra DataManager đã được khởi tạo chưa

#### `void Shutdown()`
-   **Mô tả**: Cleanup DataManager, stop auto-save, clear cache
-   **Best practice**: Gọi trong `OnApplicationQuit()` hoặc `OnDestroy()`

#### `async UniTask SaveAllAsync()`
-   **Mô tả**: Lưu tất cả data đang có trong cache
-   **Use case**: Manual save-all, emergency backup

### Configuration Class

#### `DataManagerConfig`

```csharp
public class DataManagerConfig
{
    public bool EnableLogging { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int DefaultCacheExpirationMinutes { get; set; } = 30;
    public bool EnableAutoSave { get; set; } = true;
    public float AutoSaveIntervalSeconds { get; set; } = 300f; // 5 minutes
}
```

### Repository Types

#### `FileDataRepository<T>`
```csharp
// Constructor
public FileDataRepository(
    bool useEncryption = false,
    bool useCompression = false,
    CompressionType compressionType = CompressionType.GZip,
    string customEncryptionKey = null
)
```

#### `MemoryDataRepository<T>`
```csharp
// Constructor - Lưu data trong RAM, mất khi restart
public MemoryDataRepository()
```

### Interface Requirements

#### `IDataModel<T>`
```csharp
public interface IDataModel<T> where T : class
{
    void SetDefaultData();
}
```

#### `IValidatable` (Optional)
```csharp
public interface IValidatable
{
    bool Validate(out List<string> errors);
}
```

### Best Practices

1. **Initialization**: Luôn gọi `Initialize()` trước khi sử dụng
2. **Registration**: Đăng ký repositories trong `Awake()` hoặc initialization script
3. **Event Cleanup**: Luôn unsubscribe events trong `OnDestroy()`
4. **Error Handling**: Sử dụng try-catch cho async operations
5. **Performance**: Enable caching cho data thường xuyên truy cập
6. **Security**: Enable encryption cho sensitive data

### Thread Safety

- ✅ **Thread-safe**: `Initialize()`, `RegisterRepository()`, Event subscriptions
- ⚠️ **Main thread only**: File I/O operations (handled internally by UniTask)
- ✅ **Async-safe**: Tất cả `*Async()` methods có thể gọi từ nhiều threads

---

## 🎯 Advanced Use Cases

### Multiple Save Slots
```csharp
// Save to different slots
await DataManager.SaveDataAsync(playerData, "slot_1");
await DataManager.SaveDataAsync(playerData, "slot_2");
await DataManager.SaveDataAsync(playerData, "autosave");

// Load from specific slot
var slot1Data = await DataManager.GetDataAsync<PlayerData>("slot_1");
var slot2Data = await DataManager.GetDataAsync<PlayerData>("slot_2");
```

### Cloud Save Integration
```csharp
public class CloudDataRepository<T> : IDataRepository<T> where T : IDataModel<T>, new()
{
    public async UniTask<T> LoadAsync(string key)
    {
        // Implement cloud loading logic
        var cloudData = await CloudSaveService.LoadAsync(key);
        return JsonConvert.DeserializeObject<T>(cloudData);
    }

    public async UniTask<bool> SaveAsync(string key, T data)
    {
        // Implement cloud saving logic
        var json = JsonConvert.SerializeObject(data);
        return await CloudSaveService.SaveAsync(key, json);
    }
    
    // ... implement other methods
}

// Register cloud repository
DataManager.RegisterRepository<PlayerData>(new CloudDataRepository<PlayerData>());
```

### Data Migration
```csharp
[Serializable]
public class PlayerDataV2 : IDataModel<PlayerDataV2>
{
    public int dataVersion = 2;
    // ... new fields
    
    public void SetDefaultData()
    {
        if (dataVersion < 2)
        {
            // Migrate from v1 to v2
            MigrateFromV1();
        }
    }
    
    private void MigrateFromV1()
    {
        // Migration logic here
        dataVersion = 2;
    }
}
```

## 🤝 Contributing

Chúng tôi rất hoan nghênh mọi đóng góp! Dưới đây là các cách bạn có thể hỗ trợ:

### 🐛 Báo cáo Bugs
- Sử dụng [GitHub Issues](https://github.com/tojinguyen/Unity-Utilities/issues)
- Mô tả chi tiết bước reproduce
- Kèm theo Unity version và platform info
- Include relevant error logs

### 💡 Feature Requests
- Tạo issue với label "enhancement"
- Mô tả use case cụ thể
- Giải thích tại sao feature này hữu ích

### 🔧 Code Contributions
1. Fork repository
2. Tạo feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push branch: `git push origin feature/amazing-feature`
5. Tạo Pull Request

### 📝 Documentation
- Cải thiện README
- Thêm code examples
- Dịch documentation

## 📄 License

Package này được phát hành dưới [MIT License](LICENSE).

```
MIT License

Copyright (c) 2024 Tojin Nguyen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 📞 Support

- **Documentation**: [Wiki](https://github.com/tojinguyen/Unity-Utilities/wiki)
- **Issues**: [GitHub Issues](https://github.com/tojinguyen/Unity-Utilities/issues)
- **Discussions**: [GitHub Discussions](https://github.com/tojinguyen/Unity-Utilities/discussions)
- **Email**: tojin.nguyen@gmail.com

## 🙏 Acknowledgments

- [UniTask](https://github.com/Cysharp/UniTask) - Async/await support for Unity
- [Newtonsoft.Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html) - JSON serialization
- Unity Technologies - Excellent game engine
- Community contributors và testers

---

## 📊 Changelog

### v2.1.0 (Current)
- ✨ Added static DataManager implementation
- ✨ Improved thread safety
- ✨ Enhanced error handling và logging
- ✨ Better memory management for cache
- 🐛 Fixed encryption key issues on some platforms
- 📚 Comprehensive documentation update

### v2.0.0
- 🔥 Major architecture refactor
- ✨ Added caching system
- ✨ Event system implementation
- ✨ Multiple repository support
- ⚡ Performance improvements

### v1.5.0
- ✨ Added compression support
- ✨ Encryption with device-specific keys
- 🐛 Fixed data corruption issues

### v1.0.0
- 🎉 Initial release
- 📁 Basic file-based data management
- 🔒 AES encryption support

---

<div align="center">

**⭐ Nếu package này hữu ích, hãy cho chúng tôi một star trên GitHub! ⭐**

[🌟 Star on GitHub](https://github.com/tojinguyen/Unity-Utilities) | [📖 Documentation](https://github.com/tojinguyen/Unity-Utilities/wiki) | [🐛 Report Issues](https://github.com/tojinguyen/Unity-Utilities/issues)

</div>