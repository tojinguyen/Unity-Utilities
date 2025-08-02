# Strongly-Typed Localization Keys

## Vấn đề với Magic Strings

**Trước đây:**
```csharp
// ❌ Dễ sai, không có intellisense, runtime errors
string text = LocalizationManager.GetLocalizedText("ui_welcome_titl"); // Typo!
```

**Bây giờ:**
```csharp
// ✅ Strongly-typed, intellisense, compile-time errors
string text = LocalizationKeys.UiWelcomeTitle.GetText();
```

## 🚀 Tính năng Auto-Generate Keys

### 1. **Dropdown Selection trong Inspector**
- Sử dụng `[LocalizationKey]` attribute
- Dropdown hiển thị tất cả keys có sẵn
- Filter theo text-only hoặc sprite-only
- Tự động refresh khi có key mới

### 2. **Generated Enum/Constants**
- Tự động generate từ CSV import
- Strongly-typed access với intellisense
- Extension methods cho convenience
- Compile-time error checking

## 📝 Cách sử dụng:

### 1. Trong Components

```csharp
public class LocalizedText : MonoBehaviour
{
    [LocalizationKey(textKeysOnly: true)]  // ← Dropdown appears here!
    [SerializeField] private string localizationKey;
}

public class LocalizedImage : MonoBehaviour  
{
    [LocalizationKey(spriteKeysOnly: true)]  // ← Only sprite keys
    [SerializeField] private string localizationKey;
}
```

### 2. Trong Custom Scripts

```csharp
public class MyGameScript : MonoBehaviour
{
    [LocalizationKey]  // ← All keys available
    [SerializeField] private string welcomeKey;
    
    [LocalizationKey(textKeysOnly: true)]  // ← Text keys only
    [SerializeField] private string messageKey;
}
```

### 3. Generated Enum Usage

```csharp
// 🎯 Strongly-typed access
string welcome = LocalizationKeys.UiWelcomeTitle.GetText();
string score = LocalizationKeys.UiPlayerScore.GetText(playerName, points);
Sprite flag = LocalizationKeys.FlagIcon.GetSprite();

// 🔄 Convert enum to string key if needed
string keyString = LocalizationKeys.UiWelcomeTitle.ToKey(); // "ui_welcome_title"
```

## 🛠️ Auto-Generation Workflow:

### 1. **Import CSV** 
```
CSV → LocalizationConfig → Auto-generate prompt
```

### 2. **Manual Generation**
```
LocalizationConfig Inspector → Code Generation → Generate Enum Keys
```

### 3. **Auto-Detection**
- System tự động detect khi config thay đổi
- Prompt generate keys khi cần thiết
- Background validation

## 📊 Generated Code Examples:

### Enum Version (Recommended):
```csharp
public enum LocalizationKeys
{
    /// <summary>Key: ui_welcome_title</summary>
    UiWelcomeTitle,
    
    /// <summary>Key: ui_player_score</summary>
    UiPlayerScore,
}

// Extension methods
public static class LocalizationKeysExtensions
{
    public static string GetText(this LocalizationKeys key)
    {
        return LocalizationManager.GetLocalizedText(key.ToKey());
    }
    
    public static string GetText(this LocalizationKeys key, params object[] args)
    {
        return LocalizationManager.GetLocalizedText(key.ToKey(), args);
    }
}
```

### Constants Version:
```csharp
public static class LocalizationKeys
{
    /// <summary>Localization key: ui_welcome_title</summary>
    public const string UI_WELCOME_TITLE = "ui_welcome_title";
    
    /// <summary>Localization key: ui_player_score</summary>
    public const string UI_PLAYER_SCORE = "ui_player_score";
}
```

## 🎯 Inspector Features:

### LocalizationKey PropertyDrawer:
- **Dropdown**: Select from available keys
- **Text Field**: Manual key input for custom keys  
- **Refresh Button**: Update key list
- **Config Detection**: Auto-find LocalizationConfig
- **Filter Options**: Text-only, Sprite-only, All keys

### Visual Layout:
```
[Localization Key ▼] [custom_key] [↻]
```

## 🔧 Advanced Features:

### 1. **Key Validation**
```csharp
// Check if generated file is up-to-date
bool isUpToDate = LocalizationKeysGenerator.IsGeneratedFileUpToDate(config);

// Get all keys for custom dropdown
List<string> allKeys = LocalizationKeysGenerator.GetAllKeysFromConfig(config);
```

### 2. **Custom Key Conversion**
```csharp
// ui_welcome_title → UiWelcomeTitle (enum)
// ui_welcome_title → UI_WELCOME_TITLE (constant)
```

### 3. **Auto-Refresh**
- Editor detects config changes
- Auto-prompt for regeneration
- Cache management
- Performance optimization

## 🎊 Benefits:

✅ **No more typos** - Compile-time checking  
✅ **Intellisense support** - Full autocomplete  
✅ **Refactoring safe** - Find all references works  
✅ **Performance** - No runtime string lookups  
✅ **Backwards compatible** - String keys still work  
✅ **Easy migration** - Gradual adoption possible  

## 🚦 Migration Path:

### Phase 1: Add Attributes
```csharp
// Add to existing string fields
[LocalizationKey]
[SerializeField] private string myKey;
```

### Phase 2: Generate Enum
```
LocalizationConfig → Generate Enum Keys
```

### Phase 3: Use Enum
```csharp
// Replace string usage
string text = LocalizationKeys.MyKey.GetText();
```

### Phase 4: Remove String Keys (Optional)
```csharp
// Eventually remove string field entirely
// Use enum directly in code
```

## 💡 Best Practices:

1. **Generate after CSV import** - Always regenerate keys
2. **Use enum over constants** - Better intellisense
3. **Check "Keys Up to Date"** - In config inspector
4. **Commit generated files** - Include in version control
5. **Review key names** - Before generating final version

## 🔍 Troubleshooting:

**Q: Dropdown shows "(None)"**
- A: No LocalizationConfig found. Create or assign one.

**Q: Custom key not in dropdown**  
- A: Use text field next to dropdown for custom keys.

**Q: Generated enum has weird names**
- A: Use consistent key naming (lowercase, underscores).

**Q: Intellisense not working**
- A: Regenerate keys and restart IDE.

Hệ thống này làm cho localization trở nên cực kỳ developer-friendly và ít lỗi! 🎉
