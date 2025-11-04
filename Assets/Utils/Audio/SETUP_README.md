# 🎵 AudioManager Setup Guide

## Quick Setup (3 steps)

### 1. Create AudioDatabase
```
1. Right-click in Project → Create → Audio → Audio Database
2. Name it (e.g., "MainAudioDatabase")
3. Add your audio clips and set IDs
```

### 2. Initialize AudioManager
```
1. Create empty GameObject in scene
2. Add "AudioManagerInitializer" component
3. Drag AudioDatabase to the field
```

### 3. Use in Code
```csharp
// Play BGM
await AudioManager.PlayBGM("menuTheme");

// Play SFX
await AudioManager.PlaySFX("buttonClick");

// Play 3D SFX
await AudioManager.PlaySFXAtPosition("explosion", transform.position);

// Volume control
AudioManager.MasterVolume = 0.8f;
AudioManager.SetCategoryVolume(AudioType.Music, 0.7f);
```

## Type-Safe API (Recommended)

### 1. Create Enums
```csharp
public enum BGMTracks
{
    MainMenu,
    Gameplay,
    BossTheme
}

public enum SFXSounds
{
    ButtonClick,
    CoinCollect,
    PlayerJump
}
```

### 2. Use Type-Safe API
```csharp
// Type-safe calls
await AudioManager.PlayBGM(BGMTracks.MainMenu);
await AudioManager.PlaySFX(SFXSounds.ButtonClick);
```

## Basic UI Integration

```csharp
public class AudioUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    
    void Start()
    {
        masterVolumeSlider.value = AudioManager.MasterVolume;
        muteToggle.isOn = AudioManager.MasterMuted;
        
        masterVolumeSlider.onValueChanged.AddListener(volume => {
            AudioManager.MasterVolume = volume;
        });
        
        muteToggle.onValueChanged.AddListener(muted => {
            AudioManager.MasterMuted = muted;
        });
    }
}
```

## Common Patterns

### Scene BGM
```csharp
public class SceneAudio : MonoBehaviour
{
    [SerializeField] private string sceneBGM = "menuTheme";
    
    async void Start()
    {
        await AudioManager.PlayBGM(sceneBGM);
    }
}
```

### Button with SFX
```csharp
public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private Button button;
    
    void Start()
    {
        button.onClick.AddListener(async () => {
            await AudioManager.PlaySFX("buttonClick");
        });
    }
}
```

## Troubleshooting

**Audio not playing?**
- Check if AudioManager is initialized
- Verify audio clip ID exists in AudioDatabase
- Check if audio type is muted

**Performance issues?**
- Reduce max pool size in Initialize()
- Stop unnecessary audio with immediate=true

```csharp
// Debug info
Debug.Log($"AudioManager initialized: {AudioManager.IsInitialized}");
Debug.Log($"Active audio count: {AudioManager.GetActiveAudioCount()}");
```

That's it! Simple and straightforward audio management.