# UINavigator

UINavigator là một package Unity mạnh mẽ để quản lý UI trong game, cung cấp hệ thống điều hướng cho các thành phần UI như Screen, Popup, Alert, Toast và Tooltip với animation transitions tùy chỉnh.

## 📋 Mục lục

- [Tính năng](#tính-năng)
- [Yêu cầu](#yêu-cầu)
- [Cài đặt](#cài-đặt)
- [Cấu trúc Package](#cấu-trúc-package)
- [Hướng dẫn sử dụng](#hướng-dẫn-sử-dụng)
  - [1. Screen Management](#1-screen-management)
  - [2. Alert System](#2-alert-system)
  - [3. Modal Popups](#3-modal-popups)
  - [4. Toast Notifications](#4-toast-notifications)
  - [5. Tooltip System](#5-tooltip-system)
  - [6. Animation Transitions](#6-animation-transitions)
- [API Reference](#api-reference)
- [Example](#example)

## ✨ Tính năng

- **Screen Management**: Hệ thống điều hướng màn hình với stack-based navigation
- **Alert System**: Hiển thị hộp thoại xác nhận với các nút tùy chỉnh
- **Modal Popups**: Quản lý popup với overlay blocking
- **Toast Notifications**: Thông báo tạm thời với thời gian hiển thị tùy chỉnh
- **Tooltip System**: Tooltip có thể định vị linh hoạt
- **Animation Transitions**: Hệ thống animation tùy chỉnh cho tất cả UI components
- **Addressable Assets**: Tích hợp với Unity Addressables để load UI prefabs

## 🔧 Yêu cầu

- Unity 2021.3 LTS hoặc cao hơn
- UniTask package
- DOTween package
- Unity Addressables package
- TextMeshPro package

## 📦 Cài đặt

1. Import package vào project Unity của bạn
2. Đảm bảo các dependencies đã được cài đặt:
   ```
   - UniTask
   - DOTween
   - Unity Addressables
   - TextMeshPro
   ```
3. Tạo các prefab UI sử dụng các component của UINavigator
4. Setup Addressable assets cho các UI prefabs

## 🏗️ Cấu trúc Package

```
UINavigator/
├── Scripts/
│   ├── Base/           # Base classes (UIView, EView, UIContainer)
│   ├── Screen/         # Screen management
│   ├── Alert/          # Alert dialog system
│   ├── Popup/Modal/    # Modal popup system
│   ├── Toast/          # Toast notification system
│   ├── Tooltip/        # Tooltip system
│   ├── Tab/            # Tab management
│   └── AnimationTransition/ # Animation system
├── Prefabs/            # Default UI prefabs
├── Config/             # Animation configuration assets
└── Example/            # Example implementation
```

## 📖 Hướng dẫn sử dụng

### 1. Screen Management

Screen system sử dụng stack-based navigation để quản lý các màn hình trong game.

#### Setup Screen Container

```csharp
// Tạo GameObject với ScreenContainer component
public class ScreenContainer : UIContainer
```

#### Tạo Screen

```csharp
using Utils.Scripts.UIManager.UINavigator;

public class MyScreen : Screen
{
    protected override void Awake()
    {
        base.Awake();
        // Custom initialization
    }
    
    // Override các method lifecycle nếu cần
    public override async UniTask OnPushEnter()
    {
        await base.OnPushEnter();
        // Custom logic khi screen được push
    }
}
```

#### Sử dụng Screen Navigation

```csharp
// Push screen mới
await ScreenContainer.Push("ScreenPrefabKey");

// Pop screen hiện tại
await ScreenContainer.Pop();

// Kiểm tra số lượng screen trong stack
int screenCount = ScreenContainer.Count;
```

### 2. Alert System

Alert system cung cấp hộp thoại xác nhận với các nút tùy chỉnh.

#### Setup Alert Container

```csharp
// Tạo GameObject với AlertContainer component
// Assign modal block overlay prefab trong Inspector
```

#### Hiển thị Alert

```csharp
using Utils.Scripts.UIManager.UINavigator.Alert;

// Alert đơn giản với callback
AlertContainer.Alert(
    "AlertPrefabKey",
    "Tiêu đề",
    "Nội dung thông báo",
    OnAlertCallback,
    buttonPositive: "OK",
    buttonNegative: "Cancel"
).Forget();

private void OnAlertCallback(CallbackStatus status)
{
    switch (status)
    {
        case CallbackStatus.Positive:
            Debug.Log("User clicked OK");
            break;
        case CallbackStatus.Negative:
            Debug.Log("User clicked Cancel");
            break;
    }
}

// Alert với Action callback
AlertContainer.Alert(
    "AlertPrefabKey",
    "Xác nhận",
    "Bạn có chắc chắn muốn thực hiện hành động này?",
    () => Debug.Log("Confirmed!"),
    buttonPositive: "Đồng ý",
    buttonNegative: "Hủy"
).Forget();
```

### 3. Modal Popups

Modal system để hiển thị popup với overlay blocking.

#### Hiển thị Modal

```csharp
using Utils.Scripts.UIManager.UINavigator.Popup.Modal;

// Hiển thị modal
var modal = await ModalContainer.Show("ModalPrefabKey");

// Ẩn modal
await ModalContainer.Hide();
```

#### Tạo Custom Modal

```csharp
public class MyModal : Modal
{
    [SerializeField] private Button closeButton;
    
    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(CloseModal);
    }
    
    private void CloseModal()
    {
        ModalContainer.Hide().Forget();
    }
}
```

### 4. Toast Notifications

Toast system cho thông báo tạm thời.

#### Hiển thị Toast

```csharp
using Utils.Scripts.UIManager.UINavigator.Toast;

// Toast đơn giản
await MonoToast.ShowToast(
    "ToastPrefabKey",
    "Thông báo thành công!",
    timeShow: 2f
);

// Toast với parent transform tùy chỉnh
await MonoToast.ShowToast(
    "ToastPrefabKey",
    "Thông báo",
    timeShow: 3f,
    parent: customParent
);
```

#### Tạo Custom Toast

```csharp
public class MyToast : Toast
{
    [SerializeField] private TextMeshProUGUI messageText;
    
    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
```

### 5. Tooltip System

Tooltip system với khả năng định vị linh hoạt.

#### Hiển thị Tooltip

```csharp
using Utils.Scripts.UIManager.UINavigator.Tooltip;

// Hiển thị tooltip
await TooltipContainer.Show(
    "TooltipPrefabKey",
    "Nội dung tooltip",
    targetTransform,
    TooltipPosition.Top
);

// Ẩn tooltip
await TooltipContainer.Hide();
```

#### Các vị trí Tooltip

```csharp
public enum TooltipPosition
{
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
```

### 6. Animation Transitions

Hệ thống animation cho các UI components.

#### Setup Animation

```csharp
[System.Serializable]
public class AnimationContainerEView
{
    public AnimationTransition ShowAnimation;
    public AnimationTransition HideAnimation;
}
```

#### Tạo Animation Assets

1. Tạo ScriptableObject Animation Transition
2. Configure animation parameters
3. Assign vào UI components trong Inspector

#### Sử dụng trong Code

```csharp
public class MyView : EView
{
    public override async UniTask OnShow()
    {
        await base.OnShow(); // Sử dụng animation được config
    }
    
    public override async UniTask OnShow(Vector2 position, bool isLocalPosition = false)
    {
        await base.OnShow(position, isLocalPosition);
    }
}
```

## 📚 API Reference

### Base Classes

#### UIView
- `bool IsInitialized`: Trạng thái khởi tạo
- `CanvasGroup ViewCanvasGroup`: Canvas group component

#### EView : UIView
- `OnShow()`: Hiển thị với animation
- `OnShow(Vector2 position, bool isLocalPosition)`: Hiển thị tại vị trí cụ thể
- `OnHide()`: Ẩn với animation
- `OnHide(Vector2 position, bool isLocalPosition)`: Ẩn tại vị trí cụ thể

#### UIContainer
- `Transform TransformContainer`: Container transform
- `SpawnUIGo(string key, Transform parent)`: Spawn UI GameObject

### Container Classes

#### ScreenContainer
- `Push(string keyAddressable)`: Push screen mới
- `Pop()`: Pop screen hiện tại
- `Count`: Số lượng screen trong stack

#### AlertContainer
- `Alert(...)`: Hiển thị alert với nhiều overload

#### ModalContainer
- `Show(string keyAddressable)`: Hiển thị modal
- `Hide()`: Ẩn modal

#### MonoToast
- `ShowToast(...)`: Hiển thị toast

#### TooltipContainer
- `Show(...)`: Hiển thị tooltip
- `Hide()`: Ẩn tooltip

## 🎯 Example

Xem thư mục `Example/` để có implementation đầy đủ:

- `MainScreen.cs`: Ví dụ sử dụng tất cả UI components
- `NavigatorStarter.cs`: Setup ban đầu
- Scene `UINavigator.unity`: Scene demo hoàn chỉnh

## 🔧 Best Practices

1. **Addressable Setup**: Đảm bảo tất cả UI prefabs được setup với Addressable keys
2. **Memory Management**: Sử dụng `.Forget()` cho các UniTask không cần await
3. **Animation Config**: Tạo animation assets để reuse across multiple UI elements
4. **Error Handling**: Luôn check null khi load Addressable assets
5. **Performance**: Sử dụng object pooling cho Toast và Tooltip nếu cần thiết

## 🐛 Troubleshooting

### Common Issues

1. **Animation không hoạt động**: Kiểm tra AnimationTransition assets đã được assign
2. **Addressable load failed**: Verify addressable keys và assets đã được built
3. **UI không hiển thị**: Kiểm tra Canvas setup và sorting order
4. **Memory leaks**: Đảm bảo release Addressable instances properly

### Debug Tips

- Enable Console logs để track UI operations
- Sử dụng Unity Profiler để monitor memory usage
- Check Addressable Event Viewer để debug asset loading

## 📄 License

This package is part of Unity-Utilities project.

---

*Để biết thêm chi tiết về implementation, xem source code trong thư mục Scripts/ và example trong thư mục Example/.*