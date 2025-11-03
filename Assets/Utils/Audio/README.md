# Unity Audio System

A powerful, performance-optimized audio system for Unity with static method access and **type-safe Generic Enum API**.

## Features

### 🎵 Basic Features (Implemented)
- ✅ **Static AudioManager** - No Singleton pattern, easy static method access
- ✅ **Generic Enum API** - Type-safe audio playback with compile-time checking
- ✅ **Volume Control** - Separate BGM/SFX channels with PlayerPrefs persistence
- ✅ **Loop & One-shot Sound** - BGM loops, SFX one-shot
- ✅ **AudioSource Pooling** - Efficient object pool for performance
- ✅ **Basic Fade In/Out** - Smooth music transitions

### 🎧 Advanced Features (Available)
- ✅ **2D/3D Sound Support** - UI sounds and positional audio
- ✅ **Dynamic Crossfade** - Smooth music transitions
- ✅ **Event System** - Audio events and callbacks
- ✅ **Performance Optimization** - Memory-efficient pooling
- ✅ **Backward Compatibility** - String API still works alongside enum API

## Quick Start

### 1. Define Audio Enums (Recommended - New!)

```csharp
// Define type-safe audio enums
public enum BGMTracks
{
    MainMenu,
    Gameplay, 
    Victory,
    Defeat
}

public enum SFXSounds
{
    ButtonClick,
    CoinCollect,
    PlayerJump,
    Explosion
}

public enum UISounds
{
    MenuOpen,
    MenuClose,
    ButtonHover
}
```

### 2. Initialize AudioManager

```csharp
void Start()
{
    // Initialize with audio database
    AudioManager.Initialize(audioDatabase);
    
    // Or with custom settings
    AudioManager.Initialize(
        database: audioDatabase,
        audioSourcePrefab: myAudioSourcePrefab,
        initialPool: 10,
        maxPool: 50
    );
}
```

### 3. Generic Enum API Usage (Recommended ⭐)

```csharp
// Type-safe BGM playback
await AudioManager.PlayBGM(BGMTracks.MainMenu, volume: 0.8f, crossFade: true);
await AudioManager.PlayBGM(BGMTracks.Gameplay, volume: 1f, crossFade: true);

// Type-safe SFX playback
await AudioManager.PlaySFX(SFXSounds.ButtonClick, volume: 1f);
await AudioManager.PlaySFX(SFXSounds.CoinCollect, volume: 0.8f);

// 3D positioned SFX with enum
Vector3 position = transform.position;
await AudioManager.PlaySFXAtPosition(SFXSounds.Explosion, position, volume: 0.9f);

// Generic audio playback
await AudioManager.PlayAudio(UISounds.MenuOpen, volume: 0.7f);

// Stop specific audio with enum
await AudioManager.StopAudio(BGMTracks.Gameplay, immediate: false);

// Check audio state with enum
bool isPlaying = AudioManager.IsAudioPlaying(BGMTracks.MainMenu);

// Get audio clip data with enum
AudioClipData clipData = AudioManager.GetAudioClip(SFXSounds.ButtonClick);
```

### 4. Legacy String API Usage (Still Supported)

```csharp
// Play background music
await AudioManager.PlayBGM("menuTheme");

// Play sound effects
await AudioManager.PlaySFX("buttonClick");
await AudioManager.PlaySFX("gunshot", volume: 0.8f);

// Stop music
await AudioManager.StopBGM();

// Volume control
AudioManager.SetVolumeBGM(0.7f);  // Set BGM volume
AudioManager.SetVolumeSFX(0.9f);  // Set SFX volume
AudioManager.MasterVolume = 0.8f;  // Set master volume

// Volume gets automatically saved to PlayerPrefs
```

### 5. Advanced Usage

```csharp
// 3D positional audio
await AudioManager.PlaySFXAtPosition("explosion", transform.position);

// Play any audio type
await AudioManager.PlayAudioAsync("audioId", position: null, volume: 1f);

// Music with crossfade
await AudioManager.PlayMusicAsync("battleTheme", volume: 1f, crossFade: true);

// Stop specific audio
await AudioManager.StopAudioAsync("audioId");

// Pause/Resume
AudioManager.PauseAllAudio(AudioType.SFX);
AudioManager.ResumeAllAudio(AudioType.SFX);

// Check status
bool isPlaying = AudioManager.IsAudioPlaying("audioId");
bool musicPlaying = AudioManager.IsMusicPlaying;
string currentMusic = AudioManager.CurrentMusicId;
```

### 6. Volume Control

```csharp
// Master volume (affects all audio)
AudioManager.MasterVolume = 0.8f;
AudioManager.MasterMuted = true;

// Category volumes (BGM, SFX, UI, Voice, Ambient)
AudioManager.SetCategoryVolume(AudioType.Music, 0.7f);
AudioManager.SetCategoryVolume(AudioType.SFX, 0.9f);
AudioManager.SetCategoryMuted(AudioType.Voice, false);

// Get volumes
float bgmVolume = AudioManager.GetCategoryVolume(AudioType.Music);
bool sfxMuted = AudioManager.IsCategoryMuted(AudioType.SFX);
```

### 7. Events

```csharp
// Subscribe to events
AudioManager.OnAudioStarted += (audioId, audioType) => 
{
    Debug.Log($"Started playing: {audioId} ({audioType})");
};

AudioManager.OnAudioStopped += (audioId, audioType) => 
{
    Debug.Log($"Stopped playing: {audioId} ({audioType})");
};

AudioManager.OnMasterVolumeChanged += (volume) => 
{
    Debug.Log($"Master volume changed to: {volume}");
};
```

## 🆕 Generic Enum API Benefits

### ✅ **Type Safety**
```csharp
// ❌ String API - Runtime errors possible
AudioManager.PlaySFX("button_clik", 1f); // Typo! Runtime error

// ✅ Enum API - Compile-time safety
AudioManager.PlaySFX(SFXSounds.ButtonClick, 1f); // Compiler catches errors
```

### ✅ **IntelliSense Support**
IDE automatically suggests available enum values, improving developer experience.

### ✅ **Refactoring Safe**
Renaming enum values automatically updates all references in the codebase.

### ✅ **Performance**
Enum to string conversion is efficient and only happens once.

### ✅ **Migration Friendly**
Both enum and string APIs work together - migrate at your own pace.

## Legacy AudioService

The old AudioService is still available as a wrapper, but using AudioManager directly is recommended:

```csharp
// These still work (legacy)
await AudioService.PlayAsync("audioId");
await AudioService.PlayMusicAsync("musicId");
AudioService.SetMasterVolume(0.8f);
```

## Audio Types

- **Music** - Background music, usually looped
- **SFX** - Sound effects, one-shot
- **UI** - User interface sounds
- **Voice** - Character voices, dialogue
- **Ambient** - Environmental sounds

## Performance Features

- **Object Pooling** - AudioSources are pooled and reused
- **Automatic Cleanup** - Old sounds are stopped when pool is full
- **Memory Efficient** - No unnecessary GameObject creation/destruction
- **PlayerPrefs Integration** - Settings automatically saved

## Migration Guides

### From String API to Enum API

```csharp
// 1. Define your audio enums
public enum BGMTracks { MainMenu, Gameplay, Victory }
public enum SFXSounds { ButtonClick, CoinCollect, Explosion }

// 2. Replace string calls with enum calls
// Before
await AudioManager.PlayBGM("MainMenuMusic", 0.8f);
await AudioManager.PlaySFX("ButtonClickSound", 1f);

// After  
await AudioManager.PlayBGM(BGMTracks.MainMenu, 0.8f);
await AudioManager.PlaySFX(SFXSounds.ButtonClick, 1f);

// 3. Update AudioDatabase IDs to match enum names
// Ensure your audio IDs match: "MainMenu", "ButtonClick", etc.
```

### From Singleton AudioManager

If you were using the old Singleton AudioManager:

```csharp
// Old way (Singleton)
AudioManager.Instance.PlayMusicAsync("theme");
AudioManager.Instance.MasterVolume = 0.8f;

// New way (Static)
AudioManager.PlayMusicAsync("theme");
AudioManager.MasterVolume = 0.8f;

// Or use simple API
AudioManager.PlayBGM("theme");
AudioManager.SetVolumeBGM(0.8f);
```

## Requirements Met

✅ **No Singleton** - Uses static methods as requested  
✅ **Type-Safe API** - Generic enum API with compile-time checking  
✅ **Easy API** - Simple `PlayBGM()`, `PlaySFX()`, `StopBGM()` methods  
✅ **Volume Control** - Separate BGM/SFX channels with PlayerPrefs  
✅ **Loop Support** - Music loops automatically  
✅ **Pooling System** - Efficient AudioSource management  
✅ **Fade Support** - Crossfade between music tracks  
✅ **3D Audio** - Positional sound support  
✅ **Performance** - Optimized for mobile and desktop  
✅ **IntelliSense** - Full IDE support with enum suggestions  
✅ **Backward Compatibility** - String API still works alongside enum API  

## Examples & Documentation

- **`AudioEnumExamples.cs`** - Complete usage examples and best practices
- **`ENUM_API_DOCUMENTATION.md`** - Detailed API documentation
- **Game Integration Examples** - UI buttons, game events, state management

## Setup Notes

1. **Define Audio Enums** - Create enum definitions for type-safe audio IDs
2. **Configure AudioDatabase** - Ensure audio IDs match your enum value names
3. **AudioManager Initialization** - Auto-initializes a manager GameObject (persistent across scenes)
4. **Volume Settings** - All volume settings are automatically saved to PlayerPrefs
5. **Pause/Resume** - The system handles pause/resume automatically for application focus changes

## Quick Start Checklist

1. ✅ Create audio enums (`BGMTracks`, `SFXSounds`, etc.)
2. ✅ Initialize AudioManager with your AudioDatabase
3. ✅ Replace string audio calls with type-safe enum calls
4. ✅ Enjoy compile-time safety and better IntelliSense!

Enjoy your new type-safe static audio system! 🎵