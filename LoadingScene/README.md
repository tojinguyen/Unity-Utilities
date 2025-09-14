# 🎮 Loading Scene System - Unity Package

**Hệ thống quản lý loading scene chuyên nghiệp với khả năng tùy chỉnh cao cho Unity**

[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## ✨ Tính năng chính

- 🔄 **Quản lý loading steps**: Tạo và thực thi các bước loading tuần tự với Command Pattern
- 📊 **Progress tracking**: Theo dõi và hiển thị tiến độ loading real-time với smooth animation
- 🎨 **Custom UI**: Tùy chỉnh giao diện loading hoàn toàn hoặc sử dụng UI có sẵn
- ⚡ **Async operations**: Hỗ trợ loading bất đồng bộ không block UI thread
- 🛡️ **Error handling**: Xử lý lỗi và cancellation gracefully
- 🎬 **Scene management**: Chuyển đổi scene với nhiều chế độ (Single, Additive, Replace)
- 🏭 **Factory Pattern**: Tạo loading steps dễ dàng với LoadingStepFactory
- 👀 **Observer Pattern**: Theo dõi tiến độ với callbacks
- 🎯 **Extensible**: Dễ dàng mở rộng với custom loading steps

## 🏗️ Kiến trúc hệ thống

### 🎯 Design Patterns:
- **🔄 Singleton Pattern**: `LoadingManager` - quản lý toàn bộ hệ thống
- **⚡ Command Pattern**: `ILoadingStep` - đóng gói mỗi bước loading  
- **👀 Observer Pattern**: `ILoadingProgressCallback` - thông báo tiến độ
- **🎨 Strategy Pattern**: `ILoadingUIController` - tùy chỉnh UI
- **🏭 Factory Pattern**: `LoadingStepFactory` - tạo steps dễ dàng

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
│   │   ├── CommonLoadingSteps.cs
│   │   └── SceneLoadingStep.cs
│   ├── LoadingManager.cs
│   └── LoadingStepFactory.cs
├── 🎨 UI/
│   └── DefaultLoadingUIController.cs
├── 📚 Examples/
│   ├── LoadingSceneExample.cs
│   ├── SimpleSceneLoader.cs
│   └── Demo/ (Example scenes)
├── LoadingManagerHelper.cs
└── README.md
```

## 🚀 Quick Start

### 1️⃣ Load scene đơn giản:

```csharp
// Load scene với minimum setup
var steps = LoadingStepFactory.CreateSimpleSceneLoad("GameScene");
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
        var steps = LoadingStepFactory.CreateStandardSceneTransition(targetScene);
        await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
    }
}
```

### 3️⃣ Custom loading steps:

```csharp
var steps = new List<ILoadingStep>
{
    LoadingStepFactory.CreateDelay(1f, "Preparing", "Getting ready..."),
    LoadingStepFactory.CreateCustom(async (step) => {
        step.UpdateProgress(0.5f);
        await InitializeGameSystems();
        step.UpdateProgress(1f);
    }, "Initialize", "Setting up game..."),
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

## 🔧 Các loại Loading Steps

### 🎬 SceneLoadingStep
```csharp
// Load scene thường
SceneLoadingStep.LoadScene("GameScene");

// Load scene additive  
SceneLoadingStep.LoadSceneAdditive("UIScene");

// Replace scene
SceneLoadingStep.ReplaceScene("NewScene", "OldScene");
```

### ⏱️ DelayLoadingStep
```csharp
// Delay với progress tracking
new DelayLoadingStep(2f, "Loading", "Please wait...");
```

### 📦 ResourceLoadingStep
```csharp
// Load assets từ Resources
new ResourceLoadingStep("Prefabs/PlayerPrefab", typeof(GameObject));
```

### 🔧 CustomLoadingStep
```csharp
// Thực thi custom logic
LoadingStepFactory.CreateCustom(async (step) => {
    step.UpdateProgress(0.3f);
    await YourCustomOperation();
    step.UpdateProgress(1f);
}, "Custom Step", "Processing...");
```

### 🏗️ GameSystemInitializationStep
```csharp
// Khởi tạo hệ thống game
new GameSystemInitializationStep(new[] { "Audio", "Input", "Save System" });
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

### 🎮 Game Menu → Game Scene
```csharp
var steps = LoadingStepFactory.CreateStandardSceneTransition("GameScene");
await LoadingManager.Instance.StartLoadingAsync(steps);
```

### 🔄 Level Transition  
```csharp
var steps = new List<ILoadingStep>
{
    LoadingStepFactory.CreateDelay(0.5f, "Saving Progress"),
    LoadingStepFactory.CreateCustom(async (step) => await SaveGameData(), "Save Game"),
    LoadingStepFactory.CreateSceneLoad($"Level{nextLevelIndex}")
};
```

### 🔧 Game Restart
```csharp
var steps = LoadingStepFactory.CreateGameRestart("MainMenu");
await LoadingManager.Instance.StartLoadingAsync(steps);
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

✅ **Sử dụng Factory Pattern**: Dùng `LoadingStepFactory` thay vì new trực tiếp  
✅ **Cân bằng Weight**: Cân bằng trọng số của steps để progress bar chính xác  
✅ **Error Handling**: Luôn wrap loading calls trong try-catch  
✅ **UI Responsiveness**: Sử dụng async/await để không block UI  
✅ **Memory Management**: Cleanup resources trong UI Controller  
✅ **Testing**: Sử dụng example scenes để test scenarios khác nhau

## 🔄 Migration & Updates

### Từ version cũ:
- `UpdateProgress()` method được thêm vào `ILoadingStep` interface
- `CreateCustomSync()` method để phân biệt sync/async operations
- Improved error handling và cancellation support

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