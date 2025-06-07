# Unity-Utilities

Unity Utility Scripts are a collection of helpful scripts designed to streamline common tasks and enhance your workflow in Unity. These utility scripts are built to simplify various aspects of game development, from handling object pooling to managing scene transitions and improving performance optimization.

## Features

### 🎵 Audio Management System
A comprehensive audio management solution with advanced features:
- **Music, SFX, UI, Voice, and Ambient audio support**
- **Audio source pooling** for optimal performance
- **Volume controls** per category and master volume
- **Fade effects** (fade in, fade out, crossfade)
- **3D spatial audio** support
- **Addressables integration** for efficient asset loading
- **UniTask async support** for non-blocking operations

#### Quick Start
```csharp
// Play background music with crossfade
AudioService.PlayMusic("background_music", fadeIn: true);

// Play UI sound effect
AudioService.PlayUI("button_click");

// Play 3D positioned sound
AudioService.PlaySFX("explosion", transform.position);

// Control volume
AudioService.SetMasterVolume(0.8f);
AudioService.SetCategoryVolume(AudioType.Music, 0.6f);
```

### 🎮 UI Management
Streamlined UI handling and management utilities.

### 🔄 Object Pooling
Efficient object pooling system to optimize performance and reduce garbage collection.

### 💾 Data Management
Robust data handling and persistence utilities.

### 📦 Addressables Helper
Simplified integration with Unity's Addressables system for efficient asset management.

### 🎯 Singleton Pattern
Easy-to-use singleton implementations for various use cases.

### 📝 Logging System
Advanced logging utilities for debugging and development.

### 🛠️ Editor Tools
Collection of custom editor tools to enhance the Unity development experience.

## Installation

1. Clone or download this repository
2. Import the package into your Unity project
3. Ensure you have the following dependencies:
   - UniTask
   - Unity Addressables (if using addressable audio clips)

## Dependencies

- **UniTask**: For async/await support
- **Unity.Addressables**: For addressable asset loading
- **Unity.ResourceManager**: For resource management

## Usage Examples

### Audio Management

#### Setting up Audio Database
1. Create an Audio Database asset: `Right-click → Create → TirexGame → Audio Database`
2. Configure your audio clips and categories
3. Assign the database to the AudioManager in your scene

#### Playing Audio
```csharp
// Simple audio playback
AudioService.PlaySFX("footstep");

// Advanced audio playback with options
var clipData = new AudioClipData
{
    clip = myAudioClip,
    volume = 0.8f,
    pitch = 1.2f,
    loop = true,
    fadeType = AudioFadeType.FadeIn,
    fadeDuration = 2f
};
AudioService.PlayAudio(clipData, AudioType.SFX);

// 3D positioned audio
AudioService.PlaySFX("explosion", worldPosition, AudioType.SFX);
```

#### Volume Control
```csharp
// Master volume control
AudioService.SetMasterVolume(0.7f);

// Category-specific volume
AudioService.SetMusicVolume(0.5f);
AudioService.SetSFXVolume(0.8f);
AudioService.SetUIVolume(1.0f);

// Mute/unmute categories
AudioService.SetMusicMuted(true);
AudioService.ToggleMusicMute();
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
