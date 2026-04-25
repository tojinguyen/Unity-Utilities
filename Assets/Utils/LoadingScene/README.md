# 🎮 Loading Scene System - Unity Package

**Hệ thống quản lý loading scene gọn nhẹ, dễ mở rộng và hỗ trợ custom animation cho Unity**

[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![UniTask](https://img.shields.io/badge/UniTask-Required-orange.svg)](https://github.com/Cysharp/UniTask)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## ✨ Tính năng chính

- 🔄 **Quản lý loading steps**: Framework tạo và thực thi các bước loading tuần tự
- 📊 **Progress tracking**: Theo dõi và hiển thị tiến độ loading real-time
- 🎬 **Custom Animation System**: Strategy Pattern cho phép thay thế animation bất kỳ — Fade, Slide, Scale, Spinner hoặc tự viết
- ⚡ **Async operations (UniTask)**: Loading bất đồng bộ hiệu năng cao, không allocation
- 🛡️ **Error handling**: Xử lý lỗi và cancellation gracefully
- 🎯 **Scene management**: Chuyển đổi scene với nhiều chế độ (Single, Additive, Replace)
- 👀 **Observer Pattern**: Theo dõi tiến độ với callbacks
- 🏭 **Editor Tool**: Tạo nhanh LoadingManager prefab hoàn chỉnh trong vài giây

---

## 🏗️ Kiến trúc hệ thống

### 🎯 Design Patterns

| Pattern | Class | Mục đích |
|---|---|---|
| **Singleton** | `LoadingManager` | Quản lý toàn bộ hệ thống |
| **Command** | `ILoadingStep` | Đóng gói mỗi bước loading |
| **Observer** | `ILoadingProgressCallback` | Thông báo tiến độ |
| **Strategy** | `ILoadingUIController` | Tùy chỉnh UI |
| **Strategy** | `ILoadingAnimationStrategy` | **Tùy chỉnh animation** ← Mới |
| **Factory** | `LoadingStepFactory` | Tạo scene loading steps |

### 📁 Cấu trúc thư mục

```
📦 LoadingScene/
├── 🔧 Scripts/
│   ├── Core/
│   │   ├── Interfaces/
│   │   │   ├── ILoadingStep.cs
│   │   │   ├── ILoadingProgressCallback.cs
│   │   │   └── ILoadingUIController.cs
│   │   ├── Steps/
│   │   │   ├── BaseLoadingStep.cs
│   │   │   └── SceneLoadingStep.cs
│   │   ├── LoadingManager.cs
│   │   └── LoadingStepFactory.cs
│   ├── Animation/                          ← Mới
│   │   ├── ILoadingAnimationStrategy.cs   ← Interface để custom
│   │   ├── FadeAnimationStrategy.cs
│   │   ├── SlideAnimationStrategy.cs
│   │   ├── ScaleAnimationStrategy.cs
│   │   └── SpinnerAnimationStrategy.cs
│   └── UI/
│       └── DefaultLoadingUIController.cs
├── 🛠️ Editor/
│   └── LoadingManagerCreatorWindow.cs     ← Mới
├── 📚 Examples/
│   └── Script/
│       └── LoadingSceneExample.cs
└── README.md
```

---

## 🚀 Quick Start

### 1️⃣ Tạo prefab bằng Editor Tool (khuyến nghị)

Mở menu: **Tools → TirexGame → Loading Manager Creator**

Chọn:
- UI components cần thiết (ProgressBar, Text, Cancel Button...)
- Animation preset (Fade / Slide / Scale / Spinner)
- Màu sắc cho panel và progress bar

Nhấn **✨ Create LoadingManager Prefab** → kéo prefab vào scene là xong.

---

### 2️⃣ Load scene đơn giản

```csharp
var steps = new List<ILoadingStep>
{
    LoadingStepFactory.CreateSceneLoad("GameScene")
};
await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
```

### 3️⃣ Load scene với UI button

```csharp
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

### 4️⃣ Custom loading steps

```csharp
public class InitSystemsStep : BaseLoadingStep
{
    public InitSystemsStep() : base("Initialize", "Setting up game systems...") { }

    protected override async UniTask ExecuteStepAsync()
    {
        UpdateProgressInternal(0.3f);
        await SomeAsyncWork();
        ThrowIfCancelled();

        UpdateProgressInternal(0.7f);
        await AnotherAsyncWork();

        UpdateProgressInternal(1f);
    }
}

// Sử dụng:
var steps = new List<ILoadingStep>
{
    new InitSystemsStep(),
    LoadingStepFactory.CreateSceneLoad("GameScene")
};
await LoadingManager.Instance.StartLoadingAsync(steps, showUI: true);
```

---

## 🎬 Custom Animation System

### Built-in Animation Strategies

| Strategy | Component | Mô tả |
|---|---|---|
| `FadeAnimationStrategy` | `MonoBehaviour` | Fade in/out qua CanvasGroup alpha |
| `SlideAnimationStrategy` | `MonoBehaviour` | Slide từ 4 hướng (Bottom/Top/Left/Right) |
| `ScaleAnimationStrategy` | `MonoBehaviour` | Zoom in/out, có thể kết hợp fade |
| `SpinnerAnimationStrategy` | `MonoBehaviour` | Fade + spinner xoay khi loading đang chạy |

**Cách dùng:** Add component strategy lên cùng GameObject với `DefaultLoadingUIController`, hoặc kéo vào field **Animation Strategy Object** trong Inspector.

---

### ✍️ Tự viết Custom Animation (trong project của bạn)

Implement `ILoadingAnimationStrategy` — **không cần sửa package**:

```csharp
// File này nằm trong PROJECT của bạn, không phải package
using TirexGame.Utils.LoadingScene;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class MyCustomAnimation : MonoBehaviour, ILoadingAnimationStrategy
{
    [SerializeField] private Animator _animator;

    public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
    {
        _animator.Play("LoadingShow");
        // Đợi animation xong
        await UniTask.Delay(500, cancellationToken: ct);
    }

    public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
    {
        _animator.Play("LoadingHide");
        await UniTask.Delay(300, cancellationToken: ct);
        target.SetActive(false);
    }

    public void PlayIdleAnimation(GameObject target)
    {
        _animator.Play("LoadingIdle"); // Chạy vòng lặp khi loading đang chạy
    }

    public void StopIdleAnimation(GameObject target)
    {
        _animator.Play("Idle");
    }
}
```

**Kết hợp DOTween (ví dụ):**

```csharp
public class DOTweenLoadingAnimation : MonoBehaviour, ILoadingAnimationStrategy
{
    public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
    {
        target.SetActive(true);
        var cg = target.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        await cg.DOFade(1f, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .ToUniTask(cancellationToken: ct);
    }

    public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
    {
        var cg = target.GetComponent<CanvasGroup>();
        await cg.DOFade(0f, 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .ToUniTask(cancellationToken: ct);
        target.SetActive(false);
    }

    public void PlayIdleAnimation(GameObject target) { }
    public void StopIdleAnimation(GameObject target) { }
}
```

**Sau đó** kéo component `MyCustomAnimation` vào field **Animation Strategy Object** của `DefaultLoadingUIController` trong Inspector — xong.

---

## 🛠️ Editor Tool — Loading Manager Creator

Mở qua menu: **Tools → TirexGame → Loading Manager Creator**

| Tùy chọn | Mô tả |
|---|---|
| Prefab Name | Tên prefab sẽ được tạo |
| Save Path | Thư mục lưu prefab |
| Canvas Sorting Order | Độ ưu tiên hiển thị canvas |
| UI Components | Chọn các thành phần UI cần có |
| Animation Preset | Chọn built-in animation hoặc None |
| Style | Màu nền panel, progress bar, text |

Sau khi tạo, prefab được chọn tự động trong Project window. Kéo vào scene và gọi:

```csharp
LoadingManager.Instance.SetUIController(
    FindObjectOfType<DefaultLoadingUIController>()
);
```

---

## 🔧 Loading Steps Framework

### Built-in Steps

```csharp
// Load scene thường
LoadingStepFactory.CreateSceneLoad("GameScene");

// Load scene với build index
LoadingStepFactory.CreateSceneLoad(1);

// Load additive
SceneLoadingStep.LoadSceneAdditive("UIScene");

// Replace scene
SceneLoadingStep.ReplaceScene("NewScene", "OldScene");
```

### Custom Step Template

```csharp
public class MyLoadingStep : BaseLoadingStep
{
    public MyLoadingStep(string name, string description, float weight = 1f)
        : base(name, description, weight) { }

    protected override async UniTask ExecuteStepAsync()
    {
        // Bước 1
        UpdateProgressInternal(0.33f);
        await DoWork1();
        ThrowIfCancelled(); // Kiểm tra cancel

        // Bước 2
        UpdateProgressInternal(0.66f);
        await DoWork2();
        ThrowIfCancelled();

        // Hoàn thành
        UpdateProgressInternal(1f);
    }
}
```

**Lưu ý quan trọng:**
- Luôn gọi `ThrowIfCancelled()` sau mỗi `await` để hỗ trợ cancel
- Gọi `UpdateProgressInternal(0..1)` để UI cập nhật progress
- `weight` ảnh hưởng đến tỷ lệ progress tổng thể (step nặng hơn chiếm tỷ lệ lớn hơn)

---

## 🎨 Custom UI Controller

Nếu muốn thay thế hoàn toàn UI, implement `ILoadingUIController`:

```csharp
public class MyLoadingUI : MonoBehaviour, ILoadingUIController
{
    public GameObject UIGameObject => gameObject;
    public bool IsVisible { get; private set; }
    public event Action OnCancelRequested;

    public void ShowUI() { /* ... */ }
    public void HideUI() { /* ... */ }
    public void UpdateProgress(LoadingProgressData data) { /* ... */ }
    public void UpdateStepText(string stepName, string desc) { /* ... */ }
    public void UpdateProgressBar(float progress) { /* ... */ }
    public void ShowError(string msg) { /* ... */ }
    public void HideError() { /* ... */ }
    public void SetCancelable(bool canCancel) { /* ... */ }
    public void Cleanup() { /* ... */ }
}

// Đăng ký
LoadingManager.Instance.SetUIController(myCustomUI);
```

---

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

    public void OnStepCompleted(ILoadingStep step, LoadingProgressData data)
    {
        Debug.Log($"Done: {step.StepName}");
    }

    public void OnLoadingCompleted(LoadingProgressData data)
    {
        Debug.Log($"Completed in {data.ElapsedTime.TotalSeconds:F1}s");
    }

    public void OnLoadingError(ILoadingStep step, Exception ex, LoadingProgressData data)
    {
        Debug.LogError($"Error in '{step?.StepName}': {ex.Message}");
    }
}

// Đăng ký
LoadingManager.Instance.AddProgressCallback(new MyProgressCallback());
```

---

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
// Cancel từ code
LoadingManager.Instance.CancelLoading();

// Cancel từ UI (tự động khi nhấn Cancel button)
LoadingManager.Instance.SetUIController(uiController);
// uiController.SetCancelable(true) được gọi tự động khi StartLoadingAsync
```

---

## 📋 Best Practices

| ✅ Nên làm | ❌ Tránh |
|---|---|
| Kế thừa `BaseLoadingStep` | Tạo step không kế thừa base |
| Gọi `UpdateProgressInternal()` đều đặn | Không cập nhật progress |
| `ThrowIfCancelled()` sau mỗi `await` | Bỏ qua cancellation check |
| Wrap `StartLoadingAsync` trong try-catch | Để exception không được xử lý |
| Dùng `UniTask` cho async operations | Dùng `Task` hoặc Coroutine |
| Cleanup resources trong `Cleanup()` | Để lại memory leak |

---

## 📝 Requirements

- **Unity** 2020.3 LTS hoặc mới hơn
- **UniTask** (bắt buộc) — [GitHub](https://github.com/Cysharp/UniTask)
- **TextMeshPro** — có trong Unity Package Manager
- **.NET Standard 2.1**

---

## 📄 License

MIT License — xem file [LICENSE](LICENSE) để biết thêm chi tiết.

---

## 🙏 Credits

Được phát triển bởi **TirexGame Team** với mục đích cung cấp giải pháp loading scene chuyên nghiệp cho Unity developers.

---

**Happy Loading! 🚀**