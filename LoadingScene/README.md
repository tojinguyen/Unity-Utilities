# Loading Scene System - Unity Package

Hệ thống quản lý loading scene với khả năng tùy chỉnh cao cho Unity.

## Tính năng chính

- **Quản lý loading steps**: Tạo và thực thi các bước loading tuần tự
- **Progress tracking**: Theo dõi và hiển thị tiến độ loading
- **Custom UI**: Tùy chỉnh giao diện loading
- **Async operations**: Hỗ trợ loading bất đồng bộ
- **Error handling**: Xử lý lỗi và cancellation
- **Scene management**: Chuyển đổi scene với nhiều chế độ khác nhau

## Kiến trúc hệ thống

### Design Patterns được sử dụng:
- **Singleton Pattern**: `LoadingManager` - quản lý toàn bộ hệ thống
- **Command Pattern**: `ILoadingStep` - đóng gói mỗi bước loading
- **Observer Pattern**: `ILoadingProgressCallback` - thông báo tiến độ
- **Strategy Pattern**: `ILoadingUIController` - tùy chỉnh UI
- **Factory Pattern**: `LoadingStepFactory` - tạo các loại step

### Cấu trúc thư mục:
```
LoadingScene/
├── Core/
│   ├── Interfaces/
│   │   ├── ILoadingStep.cs
│   │   ├── ILoadingProgressCallback.cs
│   │   └── ILoadingUIController.cs
│   ├── Steps/
│   │   ├── BaseLoadingStep.cs
│   │   ├── CommonLoadingSteps.cs
│   │   └── SceneLoadingStep.cs
│   ├── LoadingManager.cs
│   └── LoadingStepFactory.cs
├── UI/
│   └── DefaultLoadingUIController.cs
└── Examples/
    ├── LoadingSceneExample.cs
    └── SimpleSceneLoader.cs
```

## Cách sử dụng cơ bản

### 1. Load scene đơn giản:

```csharp
// Load scene với minimum setup
var steps = LoadingStepFactory.CreateSimpleSceneLoad("GameScene", 2f);
await LoadingManager.Instance.StartLoadingAsync(steps);
```

### 2. Load scene với các bước chuẩn:

```csharp
// Load scene với system initialization và asset preloading
var steps = LoadingStepFactory.CreateStandardSceneTransition("GameScene", true, true);
await LoadingManager.Instance.StartLoadingAsync(steps);
```

### 3. Tạo custom loading steps:

```csharp
var steps = new List<ILoadingStep>();

// Thêm delay step
steps.Add(LoadingStepFactory.CreateDelay(1f, "Preparing", "Preparing game..."));

// Thêm system initialization
steps.Add(LoadingStepFactory.CreateSystemInit(new[] { "Audio", "Input", "UI" }));

// Thêm custom step
steps.Add(LoadingStepFactory.CreateCustom(async (step) => {
    // Your custom loading logic here
    step.UpdateProgress(0.5f);
    await SomeAsyncOperation();
    step.UpdateProgress(1f);
}, "Custom Step", "Doing custom work..."));

// Load scene
steps.Add(LoadingStepFactory.CreateSceneLoad("GameScene"));

await LoadingManager.Instance.StartLoadingAsync(steps);
```

### 4. Thiết lập UI Controller:

```csharp
// Từ prefab
var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
LoadingManager.Instance.SetUIController(uiController);

// Hoặc tạo UI đơn giản
var simpleUI = DefaultLoadingUIController.CreateSimpleUI();
LoadingManager.Instance.SetUIController(simpleUI);
```

### 5. Theo dõi progress:

```csharp
// Tạo custom progress callback
public class MyProgressCallback : ILoadingProgressCallback
{
    public void OnProgressUpdated(LoadingProgressData progressData)
    {
        Debug.Log($"Loading: {progressData.TotalProgress:P}");
    }
    
    public void OnStepStarted(ILoadingStep step, LoadingProgressData progressData)
    {
        Debug.Log($"Started: {step.StepName}");
    }
    
    public void OnStepCompleted(ILoadingStep step, LoadingProgressData progressData)
    {
        Debug.Log($"Completed: {step.StepName}");
    }
    
    public void OnLoadingCompleted(LoadingProgressData progressData)
    {
        Debug.Log("Loading completed!");
    }
    
    public void OnLoadingError(ILoadingStep step, Exception exception, LoadingProgressData progressData)
    {
        Debug.LogError($"Error: {exception.Message}");
    }
}

// Đăng ký callback
LoadingManager.Instance.AddProgressCallback(new MyProgressCallback());
```

## Các loại Loading Steps có sẵn

### 1. SceneLoadingStep
Load scene với các chế độ khác nhau:
```csharp
// Load scene thường
SceneLoadingStep.LoadScene("GameScene");

// Load scene additive
SceneLoadingStep.LoadSceneAdditive("UIScene");

// Replace scene
SceneLoadingStep.ReplaceScene("NewScene", "OldScene");
```

### 2. DelayLoadingStep
Tạo delay với progress tracking:
```csharp
new DelayLoadingStep(2f, "Loading", "Please wait...");
```

### 3. ResourceLoadingStep
Load assets từ Resources folder:
```csharp
new ResourceLoadingStep("Prefabs/PlayerPrefab", typeof(GameObject));
```

### 4. CustomLoadingStep
Thực thi custom logic:
```csharp
new CustomLoadingStep(async (step) => {
    // Your async logic
}, "Custom Step", "Processing...");
```

### 5. GameSystemInitializationStep
Khởi tạo các hệ thống game:
```csharp
new GameSystemInitializationStep(new[] { "Audio", "Input", "Save System" });
```

## Tùy chỉnh UI Loading

### Tạo custom UI Controller:

```csharp
public class MyLoadingUIController : MonoBehaviour, ILoadingUIController
{
    public GameObject UIGameObject => gameObject;
    public bool IsVisible { get; private set; }
    
    public event Action OnCancelRequested;
    
    public void ShowUI()
    {
        // Show your loading UI
        IsVisible = true;
    }
    
    public void HideUI()
    {
        // Hide your loading UI
        IsVisible = false;
    }
    
    public void UpdateProgress(LoadingProgressData progressData)
    {
        // Update your UI with progress data
    }
    
    public void UpdateStepText(string stepName, string description)
    {
        // Update step text
    }
    
    public void UpdateProgressBar(float progress)
    {
        // Update progress bar (0-1)
    }
    
    public void ShowError(string errorMessage)
    {
        // Show error message
    }
    
    public void HideError()
    {
        // Hide error message
    }
    
    public void SetCancelable(bool canCancel)
    {
        // Enable/disable cancel button
    }
    
    public void Cleanup()
    {
        // Cleanup resources
    }
}
```

## Error Handling

```csharp
try
{
    await LoadingManager.Instance.StartLoadingAsync(steps);
}
catch (OperationCanceledException)
{
    Debug.Log("Loading was cancelled");
}
catch (Exception ex)
{
    Debug.LogError($"Loading failed: {ex.Message}");
}
```

## Cancellation

```csharp
// Cancel loading
LoadingManager.Instance.CancelLoading();

// Hoặc từ UI Controller
// User clicks cancel button -> OnCancelRequested event -> LoadingManager.CancelLoading()
```

## Best Practices

1. **Sử dụng Factory Pattern**: Dùng `LoadingStepFactory` để tạo steps thay vì new trực tiếp
2. **Weight balancing**: Cân bằng trọng số của các steps để progress bar chính xác
3. **Error handling**: Luôn wrap loading calls trong try-catch
4. **UI responsiveness**: Sử dụng async/await để không block UI thread
5. **Memory management**: Cleanup resources trong UI Controller
6. **Testing**: Sử dụng example scripts để test các scenarios khác nhau

## Examples Scripts

- `LoadingSceneExample.cs`: Demo đầy đủ các tính năng
- `SimpleSceneLoader.cs`: Script đơn giản để đặt trên UI buttons

## Extensibility

Hệ thống được thiết kế để dễ dàng mở rộng:
- Tạo custom loading steps bằng cách inherit từ `BaseLoadingStep`
- Tạo custom UI bằng cách implement `ILoadingUIController`
- Tạo custom progress callbacks bằng cách implement `ILoadingProgressCallback`
- Sử dụng factory pattern để tạo các helper methods mới