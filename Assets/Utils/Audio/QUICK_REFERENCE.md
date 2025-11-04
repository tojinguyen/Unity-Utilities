# 🎵 Audio System - Quick Reference

## 🚀 Quick Start (3 bước nhanh)

### 1️⃣ Tạo AudioDatabase
```
Right-click → Create → Audio → Audio Database
Đặt tên và cấu hình audio clips
```

### 2️⃣ Add AudioSetupTemplate to Scene
```
1. Tạo Empty GameObject → đặt tên "AudioManager" 
2. Add component "AudioSetupTemplate"
3. Kéo AudioDatabase vào field "Audio Database"
4. Cấu hình Scene BGM và Ambient Sounds
```

### 3️⃣ Sử dụng trong Code
```csharp
// Phát BGM
await AudioManager.PlayBGM("menuTheme");

// Phát SFX
await AudioManager.PlaySFX("buttonClick");

// Phát SFX 3D
await AudioManager.PlaySFXAtPosition("explosion", transform.position);
```

---

## 📋 API Reference

### BGM (Background Music)
```csharp
// Phát BGM
await AudioManager.PlayBGM("musicId");
await AudioManager.PlayBGM("musicId", volume: 0.8f, crossFade: true);

// Dừng BGM
await AudioManager.StopBGM();
await AudioManager.StopBGM(immediate: true);

// Kiểm tra BGM
bool isPlaying = AudioManager.IsMusicPlaying;
string currentMusic = AudioManager.CurrentMusicId;
```

### SFX (Sound Effects)
```csharp
// Phát SFX 2D
await AudioManager.PlaySFX("sfxId");
await AudioManager.PlaySFX("sfxId", volume: 0.5f);

// Phát SFX 3D
Vector3 position = transform.position;
await AudioManager.PlaySFXAtPosition("sfxId", position);
await AudioManager.PlaySFXAtPosition("sfxId", position, volume: 0.7f);
```

### Volume Control
```csharp
// Master Volume
AudioManager.MasterVolume = 0.8f;
AudioManager.MasterMuted = true;

// Category Volume
AudioManager.SetCategoryVolume(AudioType.Music, 0.7f);
AudioManager.SetCategoryVolume(AudioType.SFX, 0.9f);

// Get Volume
float bgmVolume = AudioManager.GetCategoryVolume(AudioType.Music);
float sfxVolume = AudioManager.GetCategoryVolume(AudioType.SFX);
```

### Advanced Features
```csharp
// Stop tất cả audio
await AudioManager.StopAllAudio();
await AudioManager.StopAllSFX();

// Pause/Resume
AudioManager.PauseAllAudio();
AudioManager.ResumeAllAudio();

// Kiểm tra audio đang phát
bool isPlaying = AudioManager.IsAudioPlaying("audioId");

// Cleanup
AudioManager.Cleanup();
```

---

## 🎯 Using with Enums (Type-safe)

### Định nghĩa Enums
```csharp
public enum BGMTracks { MenuTheme, BattleTheme, VictoryTheme }
public enum SFXSounds { ButtonClick, Explosion, Jump }
```

### Sử dụng với Enums
```csharp
// Generic methods
await AudioManager.PlayBGM(BGMTracks.MenuTheme);
await AudioManager.PlaySFX(SFXSounds.ButtonClick);
await AudioManager.PlaySFXAtPosition(SFXSounds.Explosion, position);

// Extension methods (nếu có)
await BGMTracks.MenuTheme.Play();
await SFXSounds.ButtonClick.Play();
await SFXSounds.Explosion.PlayAt(position);
```

---

## 🎛️ UI Integration Examples

### Volume Slider
```csharp
public Slider masterVolumeSlider;

void Start()
{
    masterVolumeSlider.value = AudioManager.MasterVolume;
    masterVolumeSlider.onValueChanged.AddListener(volume => {
        AudioManager.MasterVolume = volume;
    });
}
```

### Mute Toggle
```csharp
public Toggle muteToggle;

void Start()
{
    muteToggle.isOn = AudioManager.MasterMuted;
    muteToggle.onValueChanged.AddListener(muted => {
        AudioManager.MasterMuted = muted;
    });
}
```

### Button with Sound
```csharp
public Button myButton;

void Start()
{
    myButton.onClick.AddListener(async () => {
        await AudioManager.PlaySFX("buttonClick");
        // Button logic here...
    });
}
```

---

## 🔧 Common Patterns

### Scene-based BGM
```csharp
public class SceneAudioController : MonoBehaviour
{
    public string sceneBGM = "menuTheme";
    
    async void Start()
    {
        await AudioManager.PlayBGM(sceneBGM);
    }
}
```

### 3D Audio Zone
```csharp
public class AudioZone : MonoBehaviour
{
    public string zoneAudio = "forestAmbient";
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.PlaySFXAtPosition(zoneAudio, transform.position);
        }
    }
}
```

### Settings Persistence
```csharp
// Save settings
PlayerPrefs.SetFloat("MasterVolume", AudioManager.MasterVolume);
PlayerPrefs.SetFloat("BGMVolume", AudioManager.GetCategoryVolume(AudioType.Music));
PlayerPrefs.Save();

// Load settings
AudioManager.MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
AudioManager.SetCategoryVolume(AudioType.Music, PlayerPrefs.GetFloat("BGMVolume", 1f));
```

---

## ⚡ Performance Tips

### Pool Management
```csharp
// AudioManager tự động quản lý pool, nhưng bạn có thể monitor:
int activeCount = AudioManager.GetActiveAudioSourcesCount();
if (activeCount > 20) {
    await AudioManager.StopAllSFX(); // Dọn dẹp nếu quá nhiều
}
```

### Memory Optimization
```csharp
// Sử dụng Addressables cho audio clips lớn
// Cleanup khi không cần thiết
AudioManager.Cleanup();
```

---

## 🐛 Troubleshooting

### Audio không phát
```csharp
// Check initialization
if (!AudioManager.IsInitialized) {
    Debug.LogError("AudioManager chưa được initialize!");
}

// Check audio clip exists
var clipData = AudioManager.GetAudioClip("audioId");
if (clipData == null) {
    Debug.LogError("Audio clip không tìm thấy!");
}
```

### Volume issues
```csharp
// Check volumes
Debug.Log($"Master: {AudioManager.MasterVolume}");
Debug.Log($"BGM: {AudioManager.GetCategoryVolume(AudioType.Music)}");
Debug.Log($"Muted: {AudioManager.MasterMuted}");
```

### Performance issues
```csharp
// Monitor active sources
Debug.Log($"Active sources: {AudioManager.GetActiveAudioSourcesCount()}");
```

---

## 📱 Mobile Considerations

### App Lifecycle
```csharp
void OnApplicationPause(bool pauseStatus)
{
    if (pauseStatus)
        AudioManager.PauseAllAudio();
    else
        AudioManager.ResumeAllAudio();
}
```

### Memory Management
```csharp
void OnApplicationFocus(bool hasFocus)
{
    if (!hasFocus)
        AudioManager.PauseAllAudio();
}

void OnDestroy()
{
    AudioManager.Cleanup();
}
```