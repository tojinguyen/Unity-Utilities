# Audio Package Requirements

## 🎵 Basic Features (Must-Have)

Đây là những tính năng cơ bản mà hầu như bất kỳ game nào cũng cần:

### 1. Static AudioManager System
- **Không sử dụng Singleton pattern** - thay vào đó sử dụng static methods
- Quản lý toàn bộ nhạc nền (BGM) và hiệu ứng âm thanh (SFX)
- API đơn giản và dễ sử dụng:
  - `AudioManager.PlayBGM("theme")`
  - `AudioManager.PlaySFX("jump")`
  - `AudioManager.StopBGM()`
  - `AudioManager.StopAllSFX()`

### 2. Volume Control
- Chia BGM và SFX thành 2 channel riêng biệt
- Có thể điều chỉnh âm lượng độc lập:
  - `AudioManager.SetVolumeBGM(float volume)`
  - `AudioManager.SetVolumeSFX(float volume)`
- Tự động lưu settings vào PlayerPrefs
- Hỗ trợ mute/unmute cho từng channel

### 3. Loop & One-shot Sound
- BGM có thể loop tự động
- SFX thường là one-shot (ví dụ: bắn súng, nhảy)
- Có thể config loop cho từng audio clip

### 4. Pooling System cho AudioSource
- Không tạo/destroy AudioSource liên tục
- Sử dụng object pool để tái sử dụng AudioSource
- Tối ưu hiệu suất và giảm GC allocation
- Có thể config số lượng AudioSource tối đa trong pool

### 5. Basic Fade In/Out
- Fade In/Out cho BGM để chuyển nhạc mượt mà
- Có thể config thời gian fade
- Hỗ trợ fade out trước khi chuyển BGM mới

## 🎧 Advanced Features (Pro-Level)

Những tính năng nâng cao giúp Audio system trở nên chuyên nghiệp:

### 1. Audio Mixer Integration
- Hỗ trợ Unity AudioMixer để mix và apply effects
- Có thể apply low-pass, high-pass, reverb, echo effects
- Expose parameters để dễ dàng chỉnh sửa runtime
- Hỗ trợ audio groups (BGM, SFX, Voice, UI)

### 2. Dynamic Crossfade
- Chuyển BGM mượt mà với crossfade effect
- Ví dụ: từ "menu theme" sang "battle theme" với fade dần
- Không bị cắt đột ngột (abrupt cut)
- Có thể config thời gian crossfade

### 3. 2D/3D Sound Support
- Hỗ trợ sound 2D (UI clicks, BGM)
- Hỗ trợ sound 3D positional (tiếng súng xa gần, bước chân)
- Tự động config AudioSource dựa trên loại sound
- Spatial blend và distance settings

### 4. Playlist System
- Tự động phát nhạc theo playlist
- Hỗ trợ shuffle, repeat modes
- Next/Previous track functionality
- Random playlist generation

### 5. Event-Driven SFX System
- Định nghĩa các sự kiện game trigger sound tự động
- Ví dụ: OnPlayerJump → tự động phát sound nhảy
- Sử dụng ScriptableObject hoặc event system
- Decouple audio logic khỏi gameplay code

### 6. Audio Ducking
- Tự động giảm volume BGM khi phát voice line hoặc cutscene
- Restore volume sau khi voice/cutscene kết thúc
- Có thể config ducking level và timing

### 7. Advanced Pooling & Performance
- Prefetch audio clips để giảm loading time
- Limit số lượng SFX cùng lúc để tránh overload
- Priority system cho SFX (SFX quan trọng có priority cao hơn)
- Memory management cho audio clips

### 8. Editor Tools & UX
- Custom Editor Window để config sound database
- Drag & drop audio clips, tự động generate ID/key
- Preview sound trực tiếp trong Inspector
- Batch import và organize audio assets
- Visual audio waveform preview

### 9. Async Loading Support
- Hỗ trợ Addressables/AssetBundle loading
- Load audio clips on demand
- Perfect cho mobile games cần tiết kiệm RAM
- Background loading với progress callback

### 10. Adaptive Music System
- Dynamic music thay đổi theo gameplay
- Ví dụ: 
  - Combat căng thẳng → nhạc tăng tempo
  - Boss gần chết → thêm layer nhạc trống dồn dập
  - Peaceful area → nhạc nhẹ nhàng
- Layer-based music system
- Real-time music adaptation

## 📋 Technical Requirements

### Performance
- Minimum GC allocation
- Efficient memory usage
- 60+ FPS performance target
- Mobile-friendly optimization

### Compatibility
- Unity 2022.3 LTS+
- Support Windows, macOS, iOS, Android
- WebGL compatible

### Code Quality
- Clean, readable code
- Comprehensive documentation
- Unit tests coverage
- Example scenes và tutorials

### Data Management
- ScriptableObject-based configuration
- JSON serialization support
- Runtime data persistence
- Easy migration between versions

## 🚀 Implementation Priority

### Phase 1: Core System (Week 1-2)
- Static AudioManager
- Basic PlayBGM/PlaySFX functions
- Volume control with PlayerPrefs
- AudioSource pooling

### Phase 2: Enhanced Features (Week 3-4)
- Fade In/Out system
- 2D/3D sound support
- Audio Mixer integration
- Event-driven SFX

### Phase 3: Advanced Features (Week 5-6)
- Dynamic crossfade
- Playlist system
- Audio ducking
- Editor tools

### Phase 4: Pro Features (Week 7-8)
- Async loading
- Adaptive music system
- Performance optimization
- Documentation và examples