# Localization System for Unity

Hệ thống đa ngôn ngữ (Localization) hoàn chỉnh cho Unity với đầy đủ tính năng Editor Tools và Runtime components.

## 🚀 Tính năng chính

### 🛠️ Editor Tools
- **Quản lý ngôn ngữ**: Tạo/xoá/sửa danh sách ngôn ngữ hỗ trợ
- **Quản lý key**: Thêm/xoá/sửa localization keys với UI dạng table
- **Import/Export**: Hỗ trợ CSV và JSON cho collaboration với translators
- **Preview**: Xem trước translations trực tiếp trong Editor
- **Validation**: Kiểm tra missing translations, duplicate keys, unused keys

### 🎮 Runtime Features
- **LocalizationManager**: Singleton quản lý trung tâm
- **LocalizedTextBinder**: Component tự động bind UI Text với localization keys
- **Auto Language Detection**: Tự động detect system language
- **Font Management**: Hỗ trợ multi-font và RTL text direction
- **Performance**: Optimized với Dictionary lookup và caching

## 📦 Cài đặt

### Yêu cầu
- Unity 2020.3 LTS trở lên
- TextMeshPro (khuyến nghị)

### Bước 1: Setup ban đầu
1. Mở `TirexGame > Localization > Setup Wizard`
2. Làm theo hướng dẫn trong wizard để tạo settings và ngôn ngữ cơ bản

### Bước 2: Cấu hình ngôn ngữ
1. Mở `TirexGame > Localization > Localization Manager`
2. Vào tab **Languages** để thêm/chỉnh sửa ngôn ngữ
3. Thiết lập ngôn ngữ mặc định

## 🎯 Sử dụng cơ bản

### 1. Thêm Localization Keys
```csharp
// Trong Localization Manager, tab "Keys & Translations"
// Thêm key: "MAIN_MENU_PLAY"
// English: "Play"
// Vietnamese: "Chơi"
```

### 2. Bind UI Text
```csharp
// Thêm LocalizedTextBinder component vào Text/TMP_Text
// Set localization key: "MAIN_MENU_PLAY"
// Component sẽ tự động update text khi đổi ngôn ngữ
```

### 3. Runtime API
```csharp
// Lấy text đã localize
string localizedText = LocalizationManager.Localize("MAIN_MENU_PLAY");

// Đổi ngôn ngữ
LocalizationManager.SetLanguage("vi");

// Lấy ngôn ngữ hiện tại
string currentLang = LocalizationManager.GetCurrentLanguageCode();

// Format với parameters
string formatted = LocalizationManager.Localize("WELCOME_MESSAGE", playerName);
```

## 🛠️ API Reference

### LocalizationManager (Singleton)
| Method | Mô tả |
|--------|-------|
| `Localize(key)` | Lấy text đã localize cho key |
| `Localize(key, params)` | Lấy text với string formatting |
| `SetLanguage(languageCode)` | Đổi ngôn ngữ hiện tại |
| `GetCurrentLanguageCode()` | Lấy mã ngôn ngữ hiện tại |
| `GetSupportedLanguages()` | Danh sách ngôn ngữ hỗ trợ |

### LocalizedTextBinder Component
| Property | Mô tả |
|----------|-------|
| `Key` | Localization key để bind |
| `Parameters` | Parameters cho string formatting |
| `FallbackText` | Text hiển thị khi key không tồn tại |
| `UseLanguageFont` | Tự động đổi font theo ngôn ngữ |

### Events
```csharp
// Đăng ký event khi đổi ngôn ngữ
LocalizationManager.OnLanguageChanged += OnLanguageChanged;

void OnLanguageChanged(LanguageData newLanguage)
{
    Debug.Log($"Language changed to: {newLanguage.DisplayName}");
}
```

## 📝 Import/Export

### Export CSV
```csharp
// Trong Localization Manager > Import/Export tab
// Click "Export All to CSV"
// File CSV sẽ có format:
// Key, Comment, en, vi, ja
// MAIN_MENU_PLAY, Main menu play button, Play, Chơi, プレイ
```

### Import CSV
```csharp
// Chuẩn bị file CSV với format tương tự
// Trong Localization Manager > Import/Export tab
// Click "Import from CSV"
// Chọn file CSV để import
```

## 🎨 Font Management

### Setup Fonts cho các ngôn ngữ
1. Tạo `LocalizationFontManager` asset
2. Assign fonts cho từng ngôn ngữ
3. Hệ thống sẽ tự động đổi font khi switch language

### RTL Support
```csharp
// Hệ thống tự động handle RTL languages (Arabic, Hebrew)
// Text alignment và direction sẽ được adjust tự động
```

## 🔍 Validation & Debug

### Validation Tools
- **Missing Translations**: Tìm keys thiếu translation
- **Duplicate Keys**: Phát hiện keys trùng lặp
- **Unused Keys**: Tìm keys không được sử dụng
- **Project Scan**: Scan toàn project tìm invalid references

### Debug Mode
```csharp
// Enable debug mode trong LocalizationSettings
// Missing keys sẽ hiển thị as "[MISSING: KEY_NAME]"
// Debug logs sẽ show localization activities
```

## 🎯 Best Practices

### Naming Convention
```csharp
// Sử dụng UPPER_CASE_WITH_UNDERSCORES
MAIN_MENU_PLAY
UI_BUTTON_CONFIRM
DIALOGUE_CHARACTER_GREETING
```

### Key Organization
```csharp
// Organize keys theo category
UI_MENU_*       // UI menu items
UI_BUTTON_*     // Buttons
DIALOGUE_*      // Dialogue text
TUTORIAL_*      // Tutorial text
ERROR_*         // Error messages
```

### Performance Tips
- Sử dụng caching trong LocalizationManager
- Avoid frequent language switching
- Preload fonts cho target languages

## 🐛 Troubleshooting

### Common Issues

**1. Text không update khi đổi ngôn ngữ**
- Kiểm tra LocalizedTextBinder component
- Đảm bảo "Update On Language Change" được enable
- Verify localization key tồn tại

**2. Font không đổi**
- Kiểm tra FontManager setup
- Assign font cho target language
- Enable "Use Language Font" trong TextBinder

**3. Import CSV failed**
- Kiểm tra CSV format
- Đảm bảo encoding UTF-8
- Check column headers match language codes

**4. Performance issues**
- Use object pooling cho UI elements
- Cache frequently used translations
- Optimize font atlas sizes

## 📁 File Structure
```
Assets/Utils/Localization/
├── Scripts/
│   ├── Data/                 # ScriptableObject classes
│   ├── Runtime/              # Core runtime classes
│   ├── Components/           # UI components
│   └── Editor/               # Editor tools
├── Resources/
│   ├── LocalizationSettings.asset
│   └── Tables/               # Localization tables
└── Languages/                # Language data assets
```

## 🤝 Contributing

### Adding new features
1. Follow existing code conventions
2. Add appropriate validation
3. Update documentation
4. Test với multiple languages

### Reporting bugs
1. Include Unity version
2. Describe reproduction steps
3. Attach sample project nếu có thể

## 📄 License

MIT License - See LICENSE file for details

## 🙏 Credits

Developed by TirexGame Utils
- Font handling inspired by Unity's localization package
- RTL support based on TextMeshPro features
- CSV parsing utilities optimized for Unity workflow

---

## 🆘 Support

Nếu có vấn đề hoặc cần support:
1. Check documentation và troubleshooting section
2. Search existing issues
3. Create new issue với detailed information

**Happy Localizing! 🌍**