# Unity Data Management Package

Một giải pháp quản lý dữ liệu toàn diện, hiệu suất cao và dễ sử dụng cho các dự án Unity. Package này cung cấp một kiến trúc linh hoạt để lưu, tải, mã hóa, nén, và xác thực dữ liệu game một cách an toàn và hiệu quả.

## ✨ Tính Năng Nổi Bật

*   **Quản lý tập trung**: `DataManager` (Singleton) là điểm truy cập duy nhất cho mọi hoạt động dữ liệu.
*   **Lưu trữ linh hoạt**: Hỗ trợ nhiều loại `Repository` (lưu vào file, bộ nhớ,...) thông qua interface.
    *   `FileDataRepository`: Lưu dữ liệu an toàn vào file trên thiết bị.
    *   `MemoryDataRepository`: Lưu dữ liệu tạm thời trong RAM, lý tưởng cho testing.
*   **An toàn và Bảo mật**:
    *   **Mã hóa AES**: Tự động mã hóa dữ liệu trước khi lưu để chống gian lận. Key được tạo dựa trên `SystemInfo.deviceUniqueIdentifier` để tăng cường bảo mật.
    *   **Nén dữ liệu**: Sử dụng các thuật toán GZip, Deflate, Brotli để giảm kích thước file save, tiết kiệm dung lượng lưu trữ.
*   **Hiệu suất cao**:
    *   **Không Reflection**: Hệ thống Validation sử dụng interface (`IValidatable`) thay vì reflection, đảm bảo tốc độ tối đa tại runtime.
    *   **Lập trình bất đồng bộ**: Sử dụng `UniTask` cho các hoạt động I/O (đọc/ghi file), tránh làm đóng băng game.
    *   **Caching thông minh**: `DataCacheManager` tích hợp sẵn giúp giảm thiểu truy cập ổ đĩa bằng cách lưu trữ dữ liệu thường dùng trong bộ nhớ.
*   **Validation Mạnh mẽ**: Các `IDataModel` có thể tự định nghĩa logic validation của riêng mình, đảm bảo tính toàn vẹn của dữ liệu trước khi lưu.
*   **Hệ thống Sự kiện (Events)**: `DataEventManager` cho phép các hệ thống khác trong game lắng nghe các sự kiện như `OnDataSaved`, `OnDataLoaded`, `OnDataDeleted`.
*   **Tự động lưu (Auto-Save)**: Cấu hình `DataManager` để tự động lưu tất cả dữ liệu định kỳ.

## 🚀 Bắt Đầu Nhanh

### 1. Định nghĩa Data Model

Tạo một class chứa dữ liệu của bạn và implement interface `IDataModel<T>`. Interface này yêu cầu bạn định nghĩa dữ liệu mặc định và logic validation.

```csharp
// Ví dụ: PlayerData.cs
using TirexGame.Utils.Data;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData : IDataModel<PlayerData>
{
    public string PlayerName;
    public int Level;
    public float Health;
    public DateTime LastLogin;

    // Cung cấp dữ liệu mặc định khi chưa có file save
    public void SetDefaultData()
    {
        PlayerName = "New Player";
        Level = 1;
        Health = 100f;
        LastLogin = DateTime.UtcNow;
    }

    // Tự định nghĩa logic validation hiệu suất cao
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            errors.Add("PlayerName is required.");
        }
        else if (PlayerName.Length < 3 || PlayerName.Length > 16)
        {
            errors.Add("PlayerName length must be between 3 and 16.");
        }

        if (Level < 1 || Level > 100)
        {
            errors.Add("Level must be between 1 and 100.");
        }

        return errors.Count == 0;
    }
}
```

### 2. Khởi tạo DataManager

Tạo một GameObject trong Scene khởi đầu của bạn (ví dụ: `GameInitializer`) và đăng ký các `Repository` cần thiết.

```csharp
// Ví dụ: GameInitializer.cs
using UnityEngine;
using TirexGame.Utils.Data;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        var dataManager = DataManager.Instance;

        // Đăng ký một repository để xử lý PlayerData
        // Bật mã hóa và nén dữ liệu để tăng cường bảo mật và tối ưu dung lượng
        var playerDataRepository = new FileDataRepository<PlayerData>(
            useEncryption: true, 
            useCompression: true
        );
        
        dataManager.RegisterRepository(playerDataRepository);
        
        Debug.Log("Game Initialized with PlayerData Repository");
    }
}
```

### 3. Sử dụng DataManager

Từ bất kỳ đâu trong code, bạn có thể truy cập `DataManager` để thực hiện các thao tác dữ liệu.

#### Tải Dữ liệu

```csharp
private async void LoadPlayer()
{
    // Tự động đọc từ cache, nếu không có sẽ đọc từ file.
    // Nếu file không tồn tại, sẽ tạo và trả về dữ liệu mặc định.
    PlayerData playerData = await DataManager.Instance.GetDataAsync<PlayerData>();
    Debug.Log($"Player Name: {playerData.PlayerName}, Level: {playerData.Level}");
}
```

#### Lưu Dữ liệu

```csharp
private async void SavePlayer(PlayerData playerData)
{
    // Dữ liệu sẽ được validate trước khi lưu.
    // Nếu không hợp lệ, thao tác sẽ thất bại và log lỗi.
    bool success = await DataManager.Instance.SaveDataAsync(playerData);
    if (success)
    {
        Debug.Log("Player data saved!");
    }
}
```

#### Xóa Dữ liệu

```csharp
private async void DeletePlayer()
{
    bool success = await DataManager.Instance.DeleteDataAsync<PlayerData>();
    if (success)
    {
        Debug.Log("Player data deleted!");
    }
}
```

### 4. Lắng nghe Sự kiện Dữ liệu

Bạn có thể đăng ký để nhận thông báo khi có thay đổi về dữ liệu.

```csharp
private void Start()
{
    DataManager.Instance.SubscribeToDataEvents<PlayerData>(
        onSaved: OnPlayerDataSaved,
        onLoaded: OnPlayerDataLoaded
    );
}

private void OnDestroy()
{
    // Luôn hủy đăng ký để tránh memory leak
    if (DataManager.Instance != null)
    {
        DataManager.Instance.UnsubscribeFromDataEvents<PlayerData>(
            onSaved: OnPlayerDataSaved,
            onLoaded: OnPlayerDataLoaded
        );
    }
}

private void OnPlayerDataSaved(PlayerData data)
{
    Debug.Log($"Event: PlayerData was saved! New level: {data.Level}");
}

private void OnPlayerDataLoaded(PlayerData data)
{
    Debug.Log($"Event: PlayerData was loaded! Player: {data.PlayerName}");
}
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
