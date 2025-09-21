# 🎮 Loading Scene System - Unity Package

**Hệ thống quản lý loading scene gọn nhẹ và dễ mở rộng cho Unity**

[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## ✨ Tính năng chính

- 🔄 **Quản lý loading steps**: Framework cơ bản để tạo và thực thi các bước loading tuần tự
- 📊 **Progress tracking**: Theo dõi và hiển thị tiến độ loading real-time
- 🎨 **Custom UI**: Tùy chỉnh giao diện loading hoàn toàn hoặc sử dụng UI có sẵn
- ⚡ **Async operations**: Hỗ trợ loading bất đồng bộ không block UI thread
- 🛡️ **Error handling**: Xử lý lỗi và cancellation gracefully
- 🎬 **Scene management**: Chuyển đổi scene với nhiều chế độ (Single, Additive, Replace)
- 👀 **Observer Pattern**: Theo dõi tiến độ với callbacks
- 🎯 **Extensible**: Framework mở cho phép tạo custom loading steps dễ dàng

## 🏗️ Kiến trúc hệ thống

### 🎯 Design Patterns:
- **🔄 Singleton Pattern**: `LoadingManager` - quản lý toàn bộ hệ thống
- **⚡ Command Pattern**: `ILoadingStep` - đóng gói mỗi bước loading  
- **👀 Observer Pattern**: `ILoadingProgressCallback` - thông báo tiến độ
- **🎨 Strategy Pattern**: `ILoadingUIController` - tùy chỉnh UI
- **🏭 Factory Pattern**: `LoadingStepFactory` - tạo scene loading steps

### 📁 Cấu trúc thư mục:
```
📦 LoadingScene/
├── 🔧 Core/
│   ├── 📋 Interfaces/
│   │   ├── ILoadingStep.cs
│   │   ├── ILoadingProgressCallback.cs
│   │   └── ILoadingUIController.cs
│   ├── 🔨 Steps/
│   │   ├── BaseLoadingStep.cs
│   │   └── SceneLoadingStep.cs
│   ├── LoadingManager.cs
│   └── LoadingStepFactory.cs
├── 🎨 UI/
│   └── DefaultLoadingUIController.cs
├── 📚 Examples/
│   ├── LoadingSceneExample.cs
│   └── Demo/ (Example scenes)
├── LoadingManagerHelper.cs
└── README.md
```

## 🚀 Quick Start

### 1️⃣ Load scene đơn giản:

```csharp
// Load scene với minimum setup
var steps = new List<ILoadingStep>
{
    LoadingStepFactory.CreateSceneLoad("GameScene")
};
await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
```

### 2️⃣ Load scene với UI button:

```csharp
using UnityEngine;
using TirexGame.Utils.LoadingScene;

public class MenuController : MonoBehaviour
{
    public string targetScene = "GameScene";

    public async void OnPlayButtonClicked()
    {
        var steps = new List<ILoadingStep>
        {
            LoadingStepFactory.CreateSceneLoad(targetScene)
        };
        await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
    }
}
```

### 3️⃣ Custom loading steps:

```csharp
public class CustomDelayStep : BaseLoadingStep
{
    private readonly float _duration;
    
    public CustomDelayStep(float duration, string stepName, string description)
        : base(stepName, description)
    {
        _duration = duration;
    }
    
    protected override async Task ExecuteStepAsync()
    {
        float elapsed = 0f;
        while (elapsed < _duration && !isCancelled)
        {
            elapsed += Time.unscaledDeltaTime;
            UpdateProgressInternal(elapsed / _duration);
            await Task.Yield();
            ThrowIfCancelled();
        }
    }
}

// Sử dụng:
var steps = new List<ILoadingStep>
{
    new CustomDelayStep(1f, "Preparing", "Getting ready..."),
    new CustomWorkStep("Initialize", "Setting up game..."),
    LoadingStepFactory.CreateSceneLoad("GameScene")
};

await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
```

## 📚 Example Scenes

Package bao gồm 3 example scenes hoàn chỉnh:

### 🏠 MainMenu Scene
- Demo tất cả các loại loading
- UI buttons cho các test cases
- Progress tracking và error handling

### ⏳ LoadingScene  
- Custom Loading UI với animations
- Progress bar mượt mà
- Cancel functionality
- Time tracking (elapsed/estimated)

### 🎮 GameScene
- Scene đích sau khi loading
- Demo quay lại menu
- Animation objects

## 🔧 Loading Steps Framework

### 🎬 SceneLoadingStep (Built-in)
```csharp
// Load scene thường
LoadingStepFactory.CreateSceneLoad("GameScene");

// Load scene với build index
LoadingStepFactory.CreateSceneLoad(1);

// Hoặc sử dụng trực tiếp
SceneLoadingStep.LoadScene("GameScene");
SceneLoadingStep.LoadSceneAdditive("UIScene");
SceneLoadingStep.ReplaceScene("NewScene", "OldScene");
```

### 🔧 Custom Loading Steps (User Implementation)

Framework cung cấp `BaseLoadingStep` class để bạn tạo custom steps:

#### ⏱️ Custom Delay Step
```csharp
public class CustomDelayStep : BaseLoadingStep
{
    private readonly float _duration;
    
    public CustomDelayStep(float duration, string stepName, string description)
        : base(stepName, description)
    {
        _duration = duration;
    }
    
    protected override async Task ExecuteStepAsync()
    {
        float elapsed = 0f;
        while (elapsed < _duration && !isCancelled)
        {
            elapsed += Time.unscaledDeltaTime;
            UpdateProgressInternal(elapsed / _duration);
            await Task.Yield();
            ThrowIfCancelled();
        }
    }
}
```

#### � Custom Resource Loading Step
```csharp
public class CustomResourceStep : BaseLoadingStep
{
    private readonly string _resourcePath;
    
    public CustomResourceStep(string resourcePath)
        : base("Loading Resource", $"Loading {resourcePath}")
    {
        _resourcePath = resourcePath;
    }
    
    protected override async Task ExecuteStepAsync()
    {
        UpdateProgressInternal(0.1f);
        
        var request = Resources.LoadAsync(_resourcePath);
        while (!request.isDone && !isCancelled)
        {
            UpdateProgressInternal(0.1f + request.progress * 0.9f);
            await Task.Yield();
            ThrowIfCancelled();
        }
        
        if (request.asset == null)
            throw new System.Exception($"Failed to load: {_resourcePath}");
    }
}
```

#### 🏗️ Custom System Initialization Step
```csharp
public class CustomSystemInitStep : BaseLoadingStep
{
    private readonly string[] _systemNames;
    
    public CustomSystemInitStep(string[] systemNames)
        : base("Initialize Systems", "Initializing game systems...")
    {
        _systemNames = systemNames;
    }
    
    protected override async Task ExecuteStepAsync()
    {
        for (int i = 0; i < _systemNames.Length; i++)
        {
            ThrowIfCancelled();
            
            // Simulate system initialization
            await Task.Delay(500);
            
            UpdateProgressInternal((float)(i + 1) / _systemNames.Length);
        }
    }
}
```

## 🎨 Custom UI Controller

```csharp
public class MyLoadingUI : MonoBehaviour, ILoadingUIController
{
    public GameObject UIGameObject => gameObject;
    public bool IsVisible { get; private set; }

    public event Action OnCancelRequested;

    public void ShowUI() { /* Your implementation */ }
    public void HideUI() { /* Your implementation */ }
    public void UpdateProgress(LoadingProgressData data) { /* Your implementation */ }
    // ... other methods
}

// Register your custom UI
LoadingManager.Instance.SetUIController(myCustomUI);
```

## 📊 Progress Tracking

```csharp
public class MyProgressCallback : ILoadingProgressCallback
{
    public void OnProgressUpdated(LoadingProgressData data)
    {
        Debug.Log($"Loading: {data.TotalProgress:P} - {data.CurrentStepName}");
    }

    public void OnStepStarted(ILoadingStep step, LoadingProgressData data)
    {
        Debug.Log($"Started: {step.StepName}");
    }

    public void OnLoadingCompleted(LoadingProgressData data)
    {
        Debug.Log($"Completed in {data.ElapsedTime.TotalSeconds:F1}s");
    }

    // ... other callback methods
}

// Register callback
LoadingManager.Instance.AddProgressCallback(new MyProgressCallback());
```

## 🎯 Common Use Cases

### 🎮 Simple Scene Loading
```csharp
var steps = new List<ILoadingStep>
{
    LoadingStepFactory.CreateSceneLoad("GameScene")
};
await LoadingManager.Instance.StartLoadingAsync(steps);
```

### 🔄 Level Transition with Custom Steps
```csharp
var steps = new List<ILoadingStep>
{
    new CustomDelayStep(0.5f, "Saving Progress", "Saving your progress..."),
    new CustomSaveStep(), // Your custom save implementation
    LoadingStepFactory.CreateSceneLoad($"Level{nextLevelIndex}")
};
```

### 🎯 Game Initialization
```csharp
var steps = new List<ILoadingStep>
{
    new CustomSystemInitStep(new[] { "Audio", "Input", "Save System" }),
    new CustomResourceStep("GameData/Settings"),
    LoadingStepFactory.CreateSceneLoad("MainMenu")
};
```

## ⚡ Advanced Features

### 🛡️ Error Handling
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

### ❌ Cancellation
```csharp
// Cancel from code
LoadingManager.Instance.CancelLoading();

// Cancel from UI (automatic with cancel button)
uiController.SetCancelable(true);
```

### ⚙️ Custom Loading Step
```csharp
public class NetworkLoadingStep : BaseLoadingStep
{
    public NetworkLoadingStep() : base("Network", "Connecting to server...") { }

    protected override async Task ExecuteStepAsync()
    {
        UpdateProgressInternal(0.2f);
        await ConnectToServer();
        
        UpdateProgressInternal(0.6f); 
        await AuthenticateUser();
        
        UpdateProgressInternal(1f);
    }
}
```

## 📋 Best Practices

✅ **Extends BaseLoadingStep**: Kế thừa từ `BaseLoadingStep` cho tất cả custom steps  
✅ **Implement Progress Updates**: Luôn gọi `UpdateProgressInternal()` để cập nhật tiến độ  
✅ **Handle Cancellation**: Check `isCancelled` và gọi `ThrowIfCancelled()` thường xuyên  
✅ **Error Handling**: Luôn wrap loading calls trong try-catch  
✅ **UI Responsiveness**: Sử dụng async/await để không block UI  
✅ **Memory Management**: Cleanup resources trong UI Controller  
✅ **Testing**: Sử dụng example scenes để test scenarios khác nhau  
✅ **Minimal Framework**: Chỉ implement những steps bạn thực sự cần

## 🔄 Package Philosophy

Package này được thiết kế theo nguyên tắc **"Minimal and Extensible"**:

- 🎯 **Core Framework Only**: Chỉ cung cấp interfaces và base classes cần thiết
- 🔧 **User Implementation**: Người dùng tự implement các steps cụ thể theo nhu cầu
- 📦 **Lightweight**: Không có code thừa hoặc dependencies không cần thiết
- 🚀 **Performance**: Framework tối ưu, hiệu suất cao
- 🎨 **Flexible**: Có thể customize mọi thứ theo ý muốn

## 📝 Requirements

- Unity 2020.3 LTS hoặc mới hơn
- .NET Standard 2.1
- TextMeshPro (cho UI examples)

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch
3. Commit changes  
4. Push to branch
5. Create Pull Request

## 📄 License

MIT License - xem file [LICENSE](LICENSE) để biết thêm chi tiết.

## 🙏 Credits

Được phát triển bởi TirexGame Team với mục đích cung cấp giải pháp loading scene chuyên nghiệp cho Unity developers.

---

**Happy Loading! 🚀**