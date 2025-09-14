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

Đây là tài liệu tham khảo nhanh các phương thức public chính bạn sẽ sử dụng từ `DataManager.Instance`.

#### `RegisterRepository<T>(IDataRepository<T> repository)`
-   **Mô tả**: Đăng ký một repository để xử lý một loại dữ liệu cụ thể `T`. Phải được gọi trước khi bạn cố gắng lưu/tải loại dữ liệu đó.
-   **Tham số**:
    -   `repository`: Một instance của repository (ví dụ `new FileDataRepository<PlayerData>()`).
-   **Ví dụ**:
    ```csharp
    // Trong file khởi tạo game
    var repo = new FileDataRepository<PlayerData>(useEncryption: true, useCompression: true);
    DataManager.Instance.RegisterRepository(repo);
    ```

#### `UniTask<T> GetDataAsync<T>(string key = null)`
-   **Mô tả**: Lấy dữ liệu một cách bất đồng bộ. Sẽ thử lấy từ cache trước, sau đó từ repository. Nếu không có, sẽ tạo và trả về dữ liệu mặc định.
-   **Tham số**:
    -   `key` (tùy chọn): Định danh duy nhất cho dữ liệu. Nếu `null`, tên của class `T` sẽ được sử dụng.
-   **Trả về**: Một `UniTask` chứa object dữ liệu `T`.
-   **Ví dụ**:
    ```csharp
    PlayerData playerData = await DataManager.Instance.GetDataAsync<PlayerData>();
    ```

#### `UniTask<bool> SaveDataAsync<T>(T data, string key = null)`
-   **Mô tả**: Lưu dữ liệu một cách bất đồng bộ. Dữ liệu sẽ được xác thực trước khi lưu.
-   **Tham số**:
    -   `data`: Object dữ liệu cần lưu.
    -   `key` (tùy chọn): Định danh duy nhất cho dữ liệu. Nếu `null`, tên của class `T` sẽ được sử dụng.
-   **Trả về**: Một `UniTask<bool>` cho biết thao tác có thành công hay không.
-   **Ví dụ**:
    ```csharp
    playerData.Level++;
    bool success = await DataManager.Instance.SaveDataAsync(playerData);
    ```

#### `UniTask<bool> DeleteDataAsync<T>(string key = null)`
-   **Mô tả**: Xóa dữ liệu từ repository và cache.
-   **Tham số**:
    -   `key` (tùy chọn): Định danh của dữ liệu cần xóa.
-   **Trả về**: Một `UniTask<bool>` cho biết thao tác xóa có thành công hay không.
-   **Ví dụ**:
    ```csharp
    await DataManager.Instance.DeleteDataAsync<PlayerData>();
    ```

#### `SubscribeToDataEvents<T>(Action<T> onSaved, Action<T> onLoaded, Action<string> onDeleted)`
-   **Mô tả**: Đăng ký lắng nghe các sự kiện cho một loại dữ liệu cụ thể.
-   **Tham số**:
    -   `onSaved` (tùy chọn): Callback được gọi khi dữ liệu `T` được lưu.
    -   `onLoaded` (tùy chọn): Callback được gọi khi dữ liệu `T` được tải.
    -   `onDeleted` (tùy chọn): Callback được gọi khi dữ liệu `T` bị xóa.
-   **Ví dụ**:
    ```csharp
    DataManager.Instance.SubscribeToDataEvents<PlayerData>(onSaved: OnPlayerSaved);

    private void OnPlayerSaved(PlayerData data) {
        Debug.Log($"Player data was saved! New level: {data.Level}");
    }
    ```

#### `UnsubscribeFromDataEvents<T>(...)`
-   **Mô tả**: Hủy đăng ký các callback sự kiện để tránh memory leak. Phải được gọi với cùng một tham chiếu phương thức đã dùng để đăng ký.
-   **Ví dụ**:
    ```csharp
    void OnDestroy() {
        if (DataManager.Instance != null) {
            DataManager.Instance.UnsubscribeFromDataEvents<PlayerData>(onSaved: OnPlayerSaved);
        }
    }
    ```

#### `ClearCache(string key = null)`
-   **Mô tả**: Xóa dữ liệu khỏi bộ nhớ đệm.
-   **Tham số**:
    -   `key` (tùy chọn): Nếu được cung cấp, chỉ xóa entry có key này. Nếu `null`, toàn bộ cache sẽ bị xóa.
-   **Ví dụ**:
    ```csharp
    // Xóa cache cho PlayerData
    DataManager.Instance.ClearCache(nameof(PlayerData));
    // Xóa toàn bộ cache
    DataManager.Instance.ClearCache();
    ```