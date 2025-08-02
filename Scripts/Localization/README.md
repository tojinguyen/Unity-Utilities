# Tirex Game Utils - Localization System

A comprehensive, flexible localization system for Unity games that supports multiple languages, CSV import/export, and automatic UI component updates.

## Features

- ✅ **ScriptableObject-based Configuration** - Easy to manage and modify
- ✅ **Multiple Language Support** - Supports 15+ languages with ISO 639-1 codes
- ✅ **CSV Import/Export** - Easy translation workflow with Excel/Google Sheets
- ✅ **Auto-updating UI Components** - LocalizedText and LocalizedImage components
- ✅ **Language Selector Component** - Ready-to-use language switching UI
- ✅ **Formatted Text Support** - String formatting with parameters
- ✅ **Editor Tools** - Comprehensive editor window and inspectors
- ✅ **Runtime Language Switching** - Change language during gameplay
- ✅ **Auto Language Detection** - Automatically detect system language
- ✅ **Persistent Language Settings** - Save user's language preference

## Quick Start

### 1. Create Localization Config

1. Right-click in Project window
2. Go to `Create > Tirex Game Utils > Localization > Localization Config`
3. Name it (e.g., "GameLocalizationConfig")

### 2. Setup Localization Manager

```csharp
using Tirex.Game.Utils.Localization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LocalizationConfig localizationConfig;
    
    private void Start()
    {
        // Initialize the localization system
        LocalizationManager.Instance.Initialize(localizationConfig);
    }
}
```

### 3. Add Localized Components

#### For Text Components:
1. Add `LocalizedText` component to any GameObject with Text or TextMeshPro
2. Set the `Localization Key` (e.g., "ui_welcome")
3. The text will automatically update when language changes

#### For Image Components:
1. Add `LocalizedImage` component to any GameObject with Image component
2. Set the `Localization Key` (e.g., "flag_icon")
3. The sprite will automatically update when language changes

### 4. Add Language Selector

1. Create a Dropdown or custom buttons
2. Add `LanguageSelector` component
3. Assign the dropdown/buttons in the inspector
4. It will automatically populate with supported languages

## Usage Examples

### Basic Text Localization

```csharp
// Get localized text
string welcomeText = LocalizationManager.GetLocalizedText("ui_welcome");

// Get formatted text with parameters
string scoreText = LocalizationManager.GetLocalizedText("ui_player_score", playerName, score);
```

### Change Language Programmatically

```csharp
// Change to Vietnamese
LocalizationManager.Instance.SetLanguage(LanguageCode.VI);

// Listen for language changes
LocalizationManager.OnLanguageChanged += OnLanguageChanged;

private void OnLanguageChanged(LanguageCode newLanguage)
{
    Debug.Log($"Language changed to: {newLanguage}");
}
```

### Check Language Information

```csharp
// Get current language info
LanguageInfo currentLang = LocalizationManager.Instance.GetCurrentLanguageInfo();
Debug.Log($"Current: {currentLang.displayName} ({currentLang.nativeName})");

// Get all supported languages
List<LanguageInfo> supportedLangs = LocalizationManager.Instance.GetSupportedLanguagesInfo();
```

## CSV Workflow

### Exporting to CSV

1. Select your LocalizationConfig in the inspector
2. Click "Export to CSV" in the CSV Tools section
3. Choose save location
4. Edit the CSV file in Excel, Google Sheets, or any text editor

### Importing from CSV

1. Prepare your CSV file with this format:
   ```csv
   Key,English,Vietnamese,Japanese
   ui_welcome,"Welcome!","Chào mừng!","ようこそ！"
   ui_start,"Start Game","Bắt đầu","ゲーム開始"
   ```

2. In LocalizationConfig inspector, click "Import from CSV"
3. Select your CSV file
4. Data will be imported and replace existing entries

### CSV Template

Use "Create Template" to generate a CSV file with your supported languages and sample keys.

## Editor Tools

### Localization Manager Window

Access via `Tools > Tirex Game Utils > Localization Manager`

Features:
- **Overview Tab**: Project statistics and quick actions
- **Components Tab**: Manage LocalizedText and LocalizedImage components in scene
- **Validation Tab**: Validate localization data and find issues
- **Tools Tab**: Batch operations and key management

### Custom Inspector

The LocalizationConfig has a custom inspector with:
- CSV import/export tools
- Data validation
- Quick actions for testing
- Data browser for viewing entries

## Supported Languages

| Code | Language | Native Name |
|------|----------|-------------|
| EN | English | English |
| VI | Vietnamese | Tiếng Việt |
| JP | Japanese | 日本語 |
| KO | Korean | 한국어 |
| ZH | Chinese (Simplified) | 简体中文 |
| TH | Thai | ไทย |
| ID | Indonesian | Bahasa Indonesia |
| ES | Spanish | Español |
| FR | French | Français |
| DE | German | Deutsch |
| RU | Russian | Русский |
| PT | Portuguese | Português |
| AR | Arabic | العربية |
| HI | Hindi | हिन्दी |
| IT | Italian | Italiano |

## Advanced Usage

### Custom Language Detection

```csharp
// Override auto-detection
LocalizationManager.Instance.SetLanguage(LanguageCode.EN);

// Check if language is supported
bool isSupported = LocalizationManager.Instance.Config.IsLanguageSupported(LanguageCode.ZH);
```

### Formatted Text with Multiple Parameters

```csharp
// In your localization config:
// "ui_battle_result": "Player {0} defeated {1} enemies in {2} seconds!"

string battleResult = LocalizationManager.GetLocalizedText("ui_battle_result", 
    playerName, enemyCount, timeInSeconds);
```

### Right-to-Left Language Support

```csharp
// Check if current language is RTL
LanguageInfo langInfo = LocalizationManager.Instance.GetCurrentLanguageInfo();
if (langInfo.isRightToLeft)
{
    // Adjust UI layout for RTL languages like Arabic
}
```

### Runtime Language Management

```csharp
// Get available languages
var availableLanguages = LocalizationManager.Instance.Config.SupportedLanguages;

// Create custom language selector
foreach (var lang in availableLanguages)
{
    LanguageInfo info = LocalizationConfig.GetLanguageInfo(lang);
    // Create button with info.nativeName
}
```

## Best Practices

### Key Naming Convention

Use a consistent naming convention for keys:
- `ui_` prefix for UI elements
- `gameplay_` prefix for gameplay messages
- `menu_` prefix for menu items
- Use lowercase with underscores

Examples:
- `ui_welcome_title`
- `ui_start_game_button`
- `gameplay_level_complete`
- `menu_settings_audio`

### Organizing Translations

1. **Group by Feature**: Keep related translations together
2. **Use Descriptive Keys**: Make keys self-explanatory
3. **Consistent Formatting**: Use the same style across all keys
4. **Version Control**: Track changes in your localization files

### Performance Considerations

1. **Initialize Early**: Initialize LocalizationManager in your first scene
2. **Cache References**: Cache LocalizationManager.Instance if used frequently
3. **Batch Updates**: Update multiple UI elements together when changing language

### Working with Translators

1. **Export to CSV**: Provide translators with clean CSV files
2. **Context Information**: Include context or screenshots for better translations
3. **Validation**: Always validate imported translations for completeness
4. **Testing**: Test with actual content, not Lorem Ipsum

## Troubleshooting

### Common Issues

**LocalizationManager not initialized**
- Make sure to call `LocalizationManager.Instance.Initialize(config)` early in your game

**Text not updating**
- Check if LocalizedText component has the correct key
- Verify the key exists in your LocalizationConfig
- Ensure LocalizationManager is initialized

**CSV import fails**
- Check CSV format (UTF-8 encoding recommended)
- Ensure first row contains proper headers
- Verify language names match supported languages

**Missing translations**
- Use the Validation tab in Localization Manager window
- Run "Find Missing Translations" to identify gaps

### Debug Information

Enable debug logging to see localization system activity:

```csharp
// Check if key exists
bool hasKey = LocalizationManager.Instance.HasTextKey("your_key");
Debug.Log($"Key exists: {hasKey}");

// Get current language
LanguageCode currentLang = LocalizationManager.Instance.CurrentLanguage;
Debug.Log($"Current language: {currentLang}");
```

## Integration with Other Systems

### With Unity Analytics

```csharp
// Track language usage
LocalizationManager.OnLanguageChanged += (language) => {
    Analytics.CustomEvent("language_changed", new Dictionary<string, object> {
        { "language", language.ToString() }
    });
};
```

### With Save System

```csharp
[System.Serializable]
public class GameSettings
{
    public LanguageCode preferredLanguage = LanguageCode.EN;
    
    public void Apply()
    {
        LocalizationManager.Instance.SetLanguage(preferredLanguage);
    }
}
```

## API Reference

### LocalizationManager

| Method | Description |
|--------|-------------|
| `Initialize(config)` | Initialize with LocalizationConfig |
| `SetLanguage(language)` | Change current language |
| `GetText(key)` | Get localized text by key |
| `GetText(key, args)` | Get formatted localized text |
| `GetSprite(key)` | Get localized sprite by key |
| `HasTextKey(key)` | Check if text key exists |
| `HasSpriteKey(key)` | Check if sprite key exists |

### Events

| Event | Description |
|-------|-------------|
| `OnLanguageChanged` | Fired when language changes |

### Components

| Component | Purpose |
|-----------|---------|
| `LocalizedText` | Auto-updating text component |
| `LocalizedImage` | Auto-updating image component |
| `LanguageSelector` | Language selection UI |

## License

This localization system is part of the Tirex Game Utils package. See the main package license for details.

## Support

For support and questions, please refer to the main Tirex Game Utils documentation or contact the development team.
