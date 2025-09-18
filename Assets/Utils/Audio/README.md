# Unity Audio System

A powerful, performance-optimized audio system for Unity with static method access instead of Singleton pattern.

## Features

### 🎵 Basic Features (Implemented)
- ✅ **Static AudioManager** - No Singleton pattern, easy static method access
- ✅ **Volume Control** - Separate BGM/SFX channels with PlayerPrefs persistence
- ✅ **Loop & One-shot Sound** - BGM loops, SFX one-shot
- ✅ **AudioSource Pooling** - Efficient object pool for performance
- ✅ **Basic Fade In/Out** - Smooth music transitions

### 🎧 Advanced Features (Available)
- ✅ **2D/3D Sound Support** - UI sounds and positional audio
- ✅ **Dynamic Crossfade** - Smooth music transitions
- ✅ **Event System** - Audio events and callbacks
- ✅ **Performance Optimization** - Memory-efficient pooling

## Quick Start

### 1. Initialize AudioManager

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

### 2. Simple API Usage (Recommended)

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

### 3. Advanced Usage

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

### 4. Volume Control

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

### 5. Events

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

## Migration from Singleton

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
✅ **Easy API** - Simple `PlayBGM()`, `PlaySFX()`, `StopBGM()` methods  
✅ **Volume Control** - Separate BGM/SFX channels with PlayerPrefs  
✅ **Loop Support** - Music loops automatically  
✅ **Pooling System** - Efficient AudioSource management  
✅ **Fade Support** - Crossfade between music tracks  
✅ **3D Audio** - Positional sound support  
✅ **Performance** - Optimized for mobile and desktop  

## Setup Notes

1. Ensure you have an AudioDatabase ScriptableObject configured
2. AudioManager will auto-initialize a manager GameObject (persistent across scenes)
3. All volume settings are automatically saved to PlayerPrefs
4. The system handles pause/resume automatically for application focus changes

Enjoy your new static audio system! 🎵