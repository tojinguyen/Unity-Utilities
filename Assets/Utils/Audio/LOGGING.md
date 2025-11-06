# Audio Package Logging Configuration

## Overview
The Audio package now supports configurable logging to reduce console noise during production while maintaining essential error reporting.

## Logging Categories

### Always Logged (Cannot be disabled)
- **Errors**: Missing audio clips, initialization failures, invalid configurations
- **Warnings**: Duplicate IDs, resource exhaustion, already initialized warnings

### Optional Logging (Can be disabled)
- Audio playback start/stop notifications
- Audio initialization confirmations
- Pool statistics and debug information
- Music playback notifications
- Pause/resume notifications

## Usage

### Enable/Disable Audio Logs
```csharp
// Enable audio logging (default: false)
AudioManager.EnableAudioLogs = true;

// Disable audio logging
AudioManager.EnableAudioLogs = false;
```

### Example Usage
```csharp
// During development - enable all logs
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    AudioManager.EnableAudioLogs = true;
#else
    AudioManager.EnableAudioLogs = false;
#endif

// Initialize audio system
AudioManager.Initialize(audioDatabase);

// Play audio (will log only if EnableAudioLogs is true)
await AudioManager.PlayAudioAsync("button_click");
```

## Benefits
- **Production**: Clean console output with only essential error information
- **Development**: Full logging for debugging audio issues
- **Performance**: Reduced string allocations when logging is disabled
- **Flexibility**: Runtime toggle without code changes

## Migration
This change is backward compatible. All existing audio functionality works without changes. The default logging behavior is now **disabled** - you need to explicitly enable it if you want the previous verbose logging.