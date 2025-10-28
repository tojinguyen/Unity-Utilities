# Build Environment System

## Giới thiệu

Build Environment System cho phép quản lý scripting defines cho các môi trường build khác nhau (Development, Staging, Production) một cách dễ dàng.

## Tính năng

- ✅ Quản lý scripting defines cho 3 môi trường: Development, Staging, Production
- ✅ Auto-apply defines khi build
- ✅ GUI thân thiện trong Unity Editor
- ✅ Export/Import cấu hình dưới dạng JSON
- ✅ Tương thích với mọi Unity version từ 2021.3+
- ✅ Không phụ thuộc vào package bên ngoài (Odin Inspector, etc.)

## Cách sử dụng

### 1. Tạo Config

1. Vào menu `TirexGame > Build > Environment Manager`
2. Click "Create New" để tạo BuildEnvironmentConfig mới
3. Hoặc sử dụng menu `TirexGame > Build > Create Environment Config`

### 2. Cấu hình Environments

**Development Environment:**
- Dành cho development build
- Thường chứa: `DEVELOPMENT`, `DEBUG_MODE`, `ENABLE_LOGS`

**Staging Environment:**
- Dành cho testing/staging build  
- Thường chứa: `STAGING`, `ENABLE_LOGS`

**Production Environment:**
- Dành cho production build
- Thường chứa: `PRODUCTION`, `RELEASE`

### 3. Apply Defines

- Click "Apply Current Environment" để áp dụng defines
- Hoặc sử dụng menu `TirexGame > Build > Apply Current Environment`
- Hoặc enable "Auto Apply on Build" để tự động áp dụng khi build

## Menu Items

- `TirexGame > Build > Environment Manager` - Mở window quản lý
- `TirexGame > Build > Create Environment Config` - Tạo config mới
- `TirexGame > Build > Apply Current Environment` - Áp dụng environment hiện tại

## API Reference

### BuildEnvironmentConfig

```csharp
// Get current environment defines
List<string> defines = config.GetCurrentDefines();

// Apply specific environment  
config.SelectedEnvironment = BuildEnvironment.Production;

// Add/remove defines
config.AddDefine("CUSTOM_DEFINE");
config.RemoveDefine("OLD_DEFINE");

// Get defines as string for PlayerSettings
string definesString = config.GetCurrentDefinesAsString();
```

### Programmatic Usage

```csharp
// Find config
var config = AssetDatabase.FindAssets("t:BuildEnvironmentConfig")
    .Select(guid => AssetDatabase.LoadAssetAtPath<BuildEnvironmentConfig>(
        AssetDatabase.GUIDToAssetPath(guid)))
    .FirstOrDefault();

// Apply environment
if (config != null)
{
    var defines = config.GetCurrentDefines();
    var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
        EditorUserBuildSettings.selectedBuildTargetGroup);
    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, 
        string.Join(";", defines));
}
```

## Build Integration

System tự động tích hợp với Unity Build Pipeline thông qua `BuildEnvironmentProcessor`:

- **IPreprocessBuildWithReport**: Tự động apply defines trước khi build
- **IPostprocessBuildWithReport**: Log kết quả build

### Settings

- **Auto Apply on Build**: Tự động apply defines khi build
- **Show Build Dialog**: Hiển thị dialog xác nhận trước khi build

## File Structure

```
BuildSystem/
├── BuildEnvironmentConfig.cs       # ScriptableObject config
├── BuildEnvironmentWindow.cs       # Editor window
├── BuildEnvironmentProcessor.cs    # Build pipeline integration
└── Resources/
    └── BuildEnvironmentConfig.asset # Default config asset
```

## Best Practices

### 1. Naming Convention

```csharp
// Environment prefixes
DEVELOPMENT_*
STAGING_*
PRODUCTION_*

// Feature flags
ENABLE_*
DISABLE_*

// Platform specific
MOBILE_*
DESKTOP_*
```

### 2. Common Defines

```csharp
// Development
DEVELOPMENT
DEBUG_MODE
ENABLE_LOGS
ENABLE_CHEATS
UNITY_ASSERTIONS

// Staging
STAGING
ENABLE_LOGS
ENABLE_ANALYTICS

// Production
PRODUCTION
RELEASE
ENABLE_ANALYTICS
DISABLE_CONSOLE
```

### 3. Conditional Compilation

```csharp
#if DEVELOPMENT
    // Development only code
    Debug.Log("Development build");
#elif STAGING
    // Staging only code
    Analytics.Enable();
#elif PRODUCTION
    // Production only code
    // No debug logs
#endif
```

## Troubleshooting

### Config không được apply:
- Kiểm tra "Auto Apply on Build" setting
- Verify `BuildEnvironmentProcessor` đang hoạt động
- Check Console để xem log messages

### Thiếu defines sau build:
- Check Build Target Group settings
- Verify config được load đúng
- Kiểm tra tên defines có hợp lệ không

### Lỗi import vào project khác:
- System không phụ thuộc package bên ngoài
- Chỉ cần copy toàn bộ BuildSystem folder
- Tương thích với mọi Unity version từ 2021.3+

## Migration

Nếu đang sử dụng phiên bản cũ có Odin Inspector dependencies:

1. **Backup** cấu hình hiện tại bằng Export Config
2. **Replace** với phiên bản mới
3. **Import** lại cấu hình đã backup

## Support

- Unity 2021.3 LTS và mới hơn
- Tất cả build platforms
- Windows, Mac, Linux Editor