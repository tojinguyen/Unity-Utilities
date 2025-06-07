# Audio Management System Setup Guide

This guide will help you set up and configure the Audio Management System in your Unity project.

## Quick Setup (5 minutes)

### 1. Create Audio Database
1. In your Project window, right-click in a suitable folder
2. Go to `Create > TirexGame > Audio Database`
3. Name it something like "GameAudioDatabase"

### 2. Configure Audio Database
1. Select your newly created Audio Database
2. In the Inspector, configure the following:

#### Category Settings
- **Music**: Volume 0.7, Enabled ✓
- **SFX**: Volume 0.8, Enabled ✓  
- **UI**: Volume 1.0, Enabled ✓
- **Voice**: Volume 0.9, Enabled ✓
- **Ambient**: Volume 0.6, Enabled ✓

#### Add Audio Clips
1. Click "Add Clip" to add new audio entries
2. For each clip, configure:
   - **ID**: Unique identifier (e.g., "background_music", "button_click")
   - **Audio Type**: Music/SFX/UI/Voice/Ambient
   - **Clip**: Drag your AudioClip from the project
   - **Volume**: Base volume (0-1)
   - **Pitch**: Pitch multiplier (usually 1.0)
   - **Loop**: Enable for background music/ambient sounds
   - **3D Settings**: Configure if you need spatial audio

### 3. Setup Audio Manager in Scene
1. Create an empty GameObject in your scene
2. Name it "AudioManager"
3. Add the `AudioManager` component
4. In the Inspector, assign your Audio Database to the "Audio Database" field
5. Configure pooling settings:
   - **Initial Pool Size**: 10 (good starting point)
   - **Max Concurrent Audio**: 20 (adjust based on your needs)

### 4. Test the System
Add the `AudioExample` script to a GameObject in your scene to test basic functionality:

```csharp
// Play background music
AudioService.PlayMusic("background_music");

// Play sound effect
AudioService.PlaySFX("button_click");

// Control volume
AudioService.SetMasterVolume(0.8f);
```

## Advanced Configuration

### Audio Clip Data Settings

#### Basic Settings
- **ID**: Unique string identifier for the audio clip
- **Audio Type**: Category (Music, SFX, UI, Voice, Ambient)
- **Clip**: Direct AudioClip reference
- **Addressable Key**: Alternative to direct clip reference for Addressables

#### Volume & Pitch
- **Volume**: Base volume (0-1, default: 1)
- **Volume Range**: Random variation (±range)
- **Pitch**: Base pitch (0.1-3, default: 1)
- **Pitch Range**: Random variation (±range)

#### Playback Settings
- **Loop**: Whether the audio should loop
- **Loop Delay Min/Max**: Random delay between loop iterations
- **Play Mode**: Once, Loop, or LoopWithRandomDelay

#### Fade Settings
- **Fade Type**: None, FadeIn, FadeOut, or CrossFade
- **Fade Duration**: Duration of fade effect in seconds

#### 3D Audio Settings
- **Spatial Blend**: 0 = 2D, 1 = 3D
- **Min/Max Distance**: 3D audio distance settings
- **Rolloff Mode**: How volume changes with distance

### AudioManager Configuration

#### Pooling Settings
- **Initial Pool Size**: Number of AudioSources created at start
- **Max Concurrent Audio**: Maximum simultaneous audio clips
- **Auto Expand Pool**: Whether to create more sources when needed

#### Master Settings
- **Master Volume**: Global volume multiplier
- **Master Muted**: Global mute state
- **Persist Across Scenes**: Keep AudioManager when loading new scenes

## Best Practices

### Performance
1. **Use Object Pooling**: The system automatically pools AudioSources
2. **Limit Concurrent Audio**: Set reasonable limits to avoid audio chaos
3. **Use Addressables**: For large audio files, use Addressable assets
4. **Preload Important Audio**: Load critical audio clips at game start

### Organization
1. **Consistent Naming**: Use descriptive, consistent IDs ("ui_button_click", "sfx_explosion")
2. **Category Usage**: Properly categorize audio for better volume control
3. **Database Management**: Keep audio databases organized by scene/feature

### Audio Design
1. **Volume Balance**: Set appropriate base volumes for each category
2. **3D Audio**: Use spatial audio for immersive environmental sounds
3. **Fade Effects**: Use fades for smooth music transitions
4. **Random Variation**: Add pitch/volume variation to avoid repetitive sounds

## Troubleshooting

### Common Issues

#### "Audio clip not found"
- Check that the clip ID matches exactly (case-sensitive)
- Ensure the clip is added to the Audio Database
- Verify the Audio Database is assigned to AudioManager

#### "No AudioManager instance"
- Make sure AudioManager component is in the scene
- Check that AudioManager.Instance is not null before calling AudioService methods

#### "Audio not playing"
- Check if the category is muted
- Verify master volume is not 0
- Ensure the AudioSource pool isn't exhausted

#### "Addressable audio not loading"
- Verify Addressable assets are properly configured
- Check that the Addressable key matches the asset address
- Ensure Addressables package is installed

### Debug Tips
1. Enable debug logging in AudioManager inspector
2. Check the Console for audio system messages
3. Use the AudioExample script to test basic functionality
4. Verify audio file formats are supported by Unity

## API Reference

### AudioService Static Methods

#### Playback
```csharp
AudioService.PlayMusic(string id, bool fadeIn = false)
AudioService.PlaySFX(string id, Vector3? position = null)
AudioService.PlayUI(string id)
AudioService.PlayVoice(string id)
AudioService.PlayAmbient(string id)
```

#### Control
```csharp
AudioService.StopMusic(bool fadeOut = false)
AudioService.StopAllSFX()
AudioService.StopAllAudio()
```

#### Volume
```csharp
AudioService.SetMasterVolume(float volume)
AudioService.SetCategoryVolume(AudioType type, float volume)
AudioService.SetMusicVolume(float volume)
AudioService.SetSFXVolume(float volume)
```

#### Mute
```csharp
AudioService.SetMasterMuted(bool muted)
AudioService.SetCategoryMuted(AudioType type, bool muted)
AudioService.ToggleMusicMute()
```
