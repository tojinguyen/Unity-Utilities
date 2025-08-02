# Auto-Update System for Localization

## Vấn đề đã giải quyết

Trước đây, mỗi UI element muốn cập nhật khi đổi ngôn ngữ phải:
1. Tự đăng ký event `LocalizationManager.OnLanguageChanged`
2. Tự hủy đăng ký trong OnDestroy
3. Viết code xử lý update trong từng script

→ **Rất bất tiện và dễ gây memory leaks!**

## Giải pháp mới: LocalizationAutoUpdater

### 🚀 Tính năng chính:
- **Tự động đăng ký/hủy đăng ký** events cho tất cả LocalizedText/LocalizedImage components
- **Centralized management** - Chỉ có 1 singleton xử lý tất cả updates
- **Zero boilerplate code** - Không cần viết event handling manually
- **Performance optimized** - Batch updates cho tất cả components cùng lúc

### 📝 Cách sử dụng:

#### 1. LocalizedText/LocalizedImage Components
```csharp
// Chỉ cần add component và set key - tất cả đều tự động!
[SerializeField] private bool useAutoUpdater = true; // Mặc định = true
```

#### 2. LocalizedTextFormatted (Mới)
Cho dynamic content với parameters:
```csharp
// Automatically updates score display when language changes
public LocalizedTextFormatted scoreText;

// Set parameters dynamically
scoreText.SetParameter("playerName", "John");
scoreText.SetParameter("score", 1500);
```

#### 3. Manual Control (nếu cần)
```csharp
// Disable auto-updater nếu muốn control manually
[SerializeField] private bool useAutoUpdater = false;
[SerializeField] private bool updateOnLanguageChange = true;
```

### ⚡ So sánh:

**TRƯỚC (Phức tạp):**
```csharp
private void OnEnable()
{
    LocalizationManager.OnLanguageChanged += OnLanguageChanged;
}

private void OnDisable()
{
    LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
}

private void OnLanguageChanged(LanguageCode newLanguage)
{
    titleText.text = LocalizationManager.GetLocalizedText("ui_title");
    scoreText.text = LocalizationManager.GetLocalizedText("ui_score", playerName, score);
    // ... repeat for every UI element
}
```

**SAU (Đơn giản):**
```csharp
// Chỉ cần add LocalizedText component với key = "ui_title"
// Chỉ cần add LocalizedTextFormatted component với key = "ui_score" và parameters
// Tất cả đều tự động cập nhật!
```

### 🔧 Components mới:

1. **LocalizedText** - Cho static text
2. **LocalizedImage** - Cho sprites theo ngôn ngữ  
3. **LocalizedTextFormatted** - Cho dynamic text với parameters
4. **LocalizationAutoUpdater** - Singleton quản lý auto-updates

### 📊 Ưu điểm:

✅ **Giảm 90% boilerplate code**  
✅ **Không còn memory leaks** từ events  
✅ **Performance tốt hơn** - batch updates  
✅ **Dễ maintain** - centralized logic  
✅ **Backward compatible** - vẫn support manual mode  

### 🎯 Khi nào dùng manual mode:

- Khi cần custom logic phức tạp
- Khi cần control timing của updates
- Khi làm việc với custom UI components

### 🛠️ Advanced Usage:

```csharp
// Check auto-updater statistics
var (textCount, formattedCount, imageCount) = LocalizationAutoUpdater.Instance.GetComponentCounts();

// Force update all components
LocalizationAutoUpdater.Instance.ForceUpdateAll();

// Clear registrations (useful for scene transitions)
LocalizationAutoUpdater.Instance.ClearAllRegistrations();
```

## Kết luận

Với hệ thống mới này, việc localization trở nên cực kỳ đơn giản:
1. Add component
2. Set key
3. Done! 🎉

Không cần lo về event management, memory leaks, hay performance issues nữa!
