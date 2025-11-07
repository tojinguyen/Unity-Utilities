# Vibration Package

A comprehensive vibration and haptic feedback package for Unity mobile games, supporting both Android and iOS platforms.

## Features

- ✅ **Cross-platform support** (Android & iOS)
- ✅ **Multiple vibration types** (duration-based, intensity-based, pattern-based)
- ✅ **Haptic feedback** (iOS native haptic feedback, Android haptic constants)
- ✅ **Predefined patterns** (success, warning, error, etc.)
- ✅ **Easy-to-use API** with static methods
- ✅ **Unity component** for visual setup
- ✅ **Editor tools** for testing
- ✅ **Example scripts** and documentation

## Platform Support

### Android
- Basic vibration with duration and patterns
- Haptic feedback using Android haptic constants
- VibrationEffect API support (Android 8.0+)
- Legacy vibration API fallback
- Vibration permission handling

### iOS
- Native haptic feedback (UIImpactFeedbackGenerator, UINotificationFeedbackGenerator, UISelectionFeedbackGenerator)
- Impact feedback with different intensities
- Notification feedback for success/warning/error
- Selection feedback for UI interactions

## Quick Start

### 1. Simplified Static Usage (Recommended)

```csharp
using TirexGame.Utils.Vibration;

// Simple static calls - NO COMPONENT REFERENCES NEEDED!
Vibration.Click();      // Perfect for button clicks
Vibration.Success();    // Success feedback
Vibration.Error();      // Error feedback
Vibration.Warning();    // Warning feedback

// Intensity-based vibrations
Vibration.Short();      // Light vibration
Vibration.Medium();     // Medium vibration  
Vibration.Strong();     // Heavy vibration

// Custom duration
Vibration.Play(150);    // Vibrate for 150ms

// Custom pattern
long[] pattern = { 0, 100, 50, 200 };
Vibration.Play(pattern);

// Haptic feedback shortcuts
Vibration.Haptic.Light();
Vibration.Haptic.Success();
Vibration.Haptic.Error();

// Utility
Vibration.Stop();       // Stop all vibrations
bool supported = Vibration.IsSupported;
bool enabled = Vibration.IsEnabled;
```

### 2. Advanced Usage (VibrationManager)

```csharp
using TirexGame.Utils.Vibration;

// Full control with VibrationManager
VibrationManager.Vibrate();
VibrationManager.Vibrate(100); // 100ms
VibrationManager.Vibrate(VibrationIntensity.Medium);

// Predefined patterns
VibrationManager.VibrateSuccess();
VibrationManager.VibrateError();
VibrationManager.VibrateWarning();
```

### 3. Haptic Feedback

```csharp
// Simplified haptic feedback
Vibration.Haptic.Light();    // Light haptic
Vibration.Haptic.Medium();   // Medium haptic  
Vibration.Haptic.Heavy();    // Heavy haptic
Vibration.Haptic.Success();  // Success haptic
Vibration.Haptic.Warning();  // Warning haptic
Vibration.Haptic.Error();    // Error haptic

// Advanced haptic feedback (VibrationManager)
VibrationManager.TriggerHapticFeedback(HapticFeedbackType.Selection);
VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Success);
VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Heavy);
```

### 4. Custom Patterns

```csharp
// Simplified custom patterns
long[] pattern = { 0, 100, 50, 200, 50, 100 }; // off, on, off, on...
Vibration.Play(pattern);

// Advanced patterns with repeat (VibrationManager)
VibrationManager.Vibrate(pattern, 0); // Repeat from index 0
```

### 5. Using VibrationComponent

Add the `VibrationComponent` to any GameObject for easy setup:

```csharp
public class MyScript : MonoBehaviour
{
    public VibrationComponent vibrationComponent;
    
    private void Start()
    {
        // The component can auto-setup button vibrations
        vibrationComponent.EnableVibration = true;
    }
    
    public void OnButtonClick()
    {
        vibrationComponent.VibrateOnButtonClick();
    }
}
```

## API Reference

### Vibration (Simplified Static Class)

The `Vibration` class provides the easiest way to add vibration to your game without any component references:

#### Basic Methods
- `Click()` - Quick vibration for button clicks
- `Short()` - Light vibration
- `Medium()` - Medium vibration
- `Strong()` - Heavy vibration
- `Play()` - Default vibration
- `Play(long milliseconds)` - Custom duration vibration
- `Play(long[] pattern)` - Custom pattern vibration
- `Stop()` - Cancel all vibrations

#### Feedback Methods
- `Success()` - Success pattern
- `Error()` - Error pattern  
- `Warning()` - Warning pattern

#### Properties
- `bool IsSupported` - Platform support check
- `bool IsEnabled` - Device settings check

#### Haptic Subclass
- `Haptic.Light()` - Light haptic feedback
- `Haptic.Medium()` - Medium haptic feedback
- `Haptic.Heavy()` - Heavy haptic feedback
- `Haptic.Success()` - Success haptic feedback
- `Haptic.Warning()` - Warning haptic feedback
- `Haptic.Error()` - Error haptic feedback

### VibrationManager (Advanced Static Class)

#### Properties
- `bool IsSupported` - Check if vibration is supported on current platform
- `bool IsEnabled` - Check if vibration is enabled in device settings

#### Basic Vibration Methods
- `Vibrate()` - Default vibration
- `Vibrate(long milliseconds)` - Duration-based vibration
- `Vibrate(VibrationIntensity intensity)` - Intensity-based vibration
- `Vibrate(long[] pattern, int repeat = -1)` - Pattern-based vibration
- `Cancel()` - Cancel ongoing vibration

#### Haptic Feedback Methods
- `TriggerHapticFeedback(HapticFeedbackType type)`
- `TriggerNotificationFeedback(NotificationFeedbackType type)`
- `TriggerImpactFeedback(ImpactFeedbackType type)`

#### Predefined Pattern Methods
- `VibrateClick()` - Short vibration for button clicks
- `VibrateConfirm()` - Medium vibration for confirmations
- `VibrateError()` - Strong vibration for errors
- `VibrateSuccess()` - Success pattern
- `VibrateWarning()` - Warning pattern
- `VibrateDoubleTap()` - Double tap pattern
- `VibrateLongPress()` - Long press vibration

### Enums

#### VibrationIntensity
- `Light` - Light vibration
- `Medium` - Medium vibration
- `Heavy` - Heavy vibration

#### HapticFeedbackType
- `Selection` - For UI selection feedback
- `Impact` - For impact-based feedback
- `Notification` - For notification feedback

#### NotificationFeedbackType (iOS)
- `Success` - Success notification
- `Warning` - Warning notification
- `Error` - Error notification

#### ImpactFeedbackType (iOS)
- `Light` - Light impact
- `Medium` - Medium impact
- `Heavy` - Heavy impact

## Editor Tools

### Vibration Test Window
Access via `Tools > Tirex Game > Vibration Test Window`

Features:
- Platform information display
- Test all vibration types
- Test haptic feedback
- Test custom patterns
- Real-time testing in Play Mode

### VibrationComponent Inspector
The custom inspector provides:
- Platform support information
- Runtime testing buttons
- Easy configuration options

## Setup Requirements

### Android
Add the following permission to your `AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.VIBRATE" />
```

### iOS
No additional setup required. Haptic feedback will automatically fallback gracefully on unsupported devices.

## Examples

The package includes several example scripts:

### BasicVibrationExample
Demonstrates basic vibration functionality with UI integration.

### StaticVibrationExample
Demonstrates the new simplified static API without component references.

### HapticFeedbackExample
Shows different types of haptic feedback and their usage.

### VibrationPatternsExample
Illustrates custom vibration patterns and predefined patterns.

## Best Practices

1. **Check Support**: Always check `VibrationManager.IsSupported` before using vibration features
2. **Respect Settings**: Check `VibrationManager.IsEnabled` to respect user device settings
3. **Use Appropriate Feedback**: Choose the right vibration type for different UI interactions
4. **Don't Overuse**: Use vibration sparingly to avoid annoying users
5. **Test on Device**: Always test vibration on actual devices, not just in editor

## Integration with Other Packages

This package integrates well with:
- UI systems (buttons, toggles, sliders)
- Game events (score, achievements, game over)
- Audio systems (sound + vibration feedback)

## Troubleshooting

### Common Issues

1. **Vibration not working on Android**
   - Check VIBRATE permission in AndroidManifest.xml
   - Verify device vibration settings
   - Test on physical device (not emulator)

2. **Haptic feedback not working on iOS**
   - Test on physical device with iOS 10+
   - Check device haptic settings
   - Some devices don't support all haptic types

3. **Editor testing limitations**
   - Vibration only works in Play Mode
   - Some features only work on actual devices
   - Use the test window for development

### Debug Tips

- Enable debug logging in example scripts
- Use the Vibration Test Window for systematic testing
- Check Unity console for error messages
- Test on multiple devices and OS versions

## Version History

### v1.0.0
- Initial release
- Android vibration support
- iOS haptic feedback support
- Basic API implementation
- Editor tools
- Example scripts

## License

This package is part of the TirexGame Unity Utilities and follows the same license terms.

## Support

For issues, feature requests, or questions, please refer to the main Unity Utilities repository.