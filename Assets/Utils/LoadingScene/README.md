# Loading Scene

Load scene bằng Addressables với fake progress bar. Không cần config gì ngoài Inspector.

## Setup

**1. Tạo prefab `LoadSceneManager`**

- Tạo GameObject, gắn component `LoadSceneManager`
- Assign `DefaultLoadingUIController` vào field **UI**
- Chỉnh **Completion Delay** (mặc định `0.8s`) — thời gian dừng tại 100% trước khi ẩn màn hình

**2. Tạo prefab `DefaultLoadingUIController`**

- Gắn component `DefaultLoadingUIController` lên Canvas/Panel
- Assign:
  - **Loading Panel** — panel chứa UI loading (sẽ được show/hide)
  - **Progress Bar** — `Slider` hiển thị tiến độ
  - **Progress Percent Text** — `TextMeshProUGUI` hiển thị `%`
  - **Animation Strategy** *(tuỳ chọn)* — kéo một trong các component animation vào

**3. Đặt prefab `LoadSceneManager` vào scene đầu tiên (Bootstrap/Init)**

`DontDestroyOnLoad` sẽ giữ nó xuyên suốt các scene.

---

## Sử dụng

```csharp
await LoadSceneManager.LoadSceneAsync("GameplayScene");
```

Chỉ vậy thôi.

---

## Animation Strategy *(tuỳ chọn)*

Gắn một trong các component sau lên cùng GameObject với `DefaultLoadingUIController` (hoặc kéo vào field **Animation Strategy**):

| Component | Hiệu ứng |
|---|---|
| `FadeAnimationStrategy` | Fade in / out |
| `ScaleAnimationStrategy` | Pop scale |
| `SlideAnimationStrategy` | Slide từ 4 hướng |
| `SpinnerAnimationStrategy` | Xoay spinner |

Không assign → instant show/hide, không animation.

---

## Custom UI

Implement interface `ILoadingUI` để tự viết UI controller:

```csharp
public class MyLoadingUI : MonoBehaviour, ILoadingUI
{
    public void ShowUI() { ... }
    public void HideUI() { ... }
    public void SetProgress(float progress) { ... } // 0 → 1
}
```

Sau đó assign vào field **UI** của `LoadSceneManager` prefab.
