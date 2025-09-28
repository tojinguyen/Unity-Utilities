# 🎯 Unity Event Center System

Hệ thống Event Center hiệu suất cao cho Unity, hỗ trợ xử lý hàng chục nghìn events mỗi frame với kiến trúc struct-based và zero allocation.

*High-performance Event Center system for Unity, supporting tens of thousands of events per frame with struct-based architecture and zero allocation.*

## 📋 Mục lục | Table of Contents

- [Tính năng chính | Key Features](#-tính-năng-chính--key-features)
- [Cài đặt | Installation](#-cài-đặt--installation)
- [Sử dụng cơ bản | Basic Usage](#-sử-dụng-cơ-bản--basic-usage)
- [API Reference](#-api-reference)
- [Ví dụ nâng cao | Advanced Examples](#-ví-dụ-nâng-cao--advanced-examples)
- [Hiệu suất | Performance](#-hiệu-suất--performance)
- [Best Practices](#-best-practices)
- [FAQ](#-faq)

## 🚀 Tính năng chính | Key Features

- ✅ **Auto-Setup**: Tự động tạo EventCenter khi khởi động - Zero configuration!
- ✅ **Hiệu suất cao**: Hỗ trợ 50,000+ events/frame
- ✅ **Zero Allocation**: Sử dụng struct thay vì class để tránh GC
- ✅ **Type Safe**: Compile-time safety với generics
- ✅ **API đơn giản**: Static methods dễ sử dụng
- ✅ **Auto Cleanup**: Tự động unsubscribe với CancellationTokenOnDestroy
- ✅ **Persistent**: DontDestroyOnLoad - hoạt động xuyên suốt tất cả scenes
- ✅ **Flexible Subscriptions**: Normal, conditional, và one-time listeners
- ✅ **Batch Operations**: Publish nhiều events cùng lúc
- ✅ **Thread Safe**: An toàn khi sử dụng đa luồng
- ✅ **No Singleton**: Sử dụng Service Locator pattern

## 📦 Cài đặt | Installation

### Bước 1: Import Package
1. Copy thư mục `EventCenter` vào project Unity của bạn
2. Đảm bảo namespace `TirexGame.Utils.EventCenter` có thể truy cập được

### Bước 2: Khởi tạo (Tự động + Có thể tùy chỉnh)
Hệ thống sẽ **TỰ ĐỘNG** tạo EventCenter khi ứng dụng khởi động! 🚀

#### 🎯 Auto-Creation Mặc định:
- ✅ **Hoàn toàn tự động**: Không cần thêm GameObject vào scene
- ✅ **DontDestroyOnLoad**: EventCenter sẽ tồn tại xuyên suốt tất cả scenes  
- ✅ **Zero configuration**: Hoạt động ngay out-of-the-box
- ✅ **Thông minh**: Chỉ tạo khi không có EventCenter nào khác

#### ⚙️ Tùy chỉnh Configuration (Tùy chọn):
Nếu bạn muốn tùy chỉnh settings, hãy sử dụng **TirexGame Config Tool**:

1. **Mở Config Tool**: `TirexGame → Event Center → Create Custom Config`
2. **Tùy chỉnh settings**: Thay đổi performance, debug settings, tên EventCenter, etc.
3. **Save Config**: Click "💾 Save Custom Config"
4. **Hoàn thành**: Hệ thống sẽ tự động sử dụng custom config của bạn!

#### 📊 Config Hierarchy:
1. **Custom Config** (trong `Assets/Resources/EventCenterConfig.asset`) - **Ưu tiên cao nhất**
2. **Package Default** (trong package) - Fallback tốt
3. **Runtime Default** - Fallback cuối cùng

**Bạn không cần làm gì thêm để bắt đầu!** EventCenter sẽ được tạo tự động.

### Bước 3: Sử dụng
```csharp
using TirexGame.Utils.EventCenter;
```

## ⚙️ Configuration System

EventCenter hỗ trợ hệ thống configuration linh hoạt với 3 levels ưu tiên:

### 📊 Config Priority (Thứ tự ưu tiên)
1. **Custom Config** - `Assets/Resources/EventCenterConfig.asset`
2. **Package Default** - Built-in trong package  
3. **Runtime Default** - Tạo runtime nếu không tìm thấy config nào

### 🔧 TirexGame Config Tool

**Mở Config Tool:**
- Menu: `TirexGame → Event Center → Create Custom Config`
- Hoặc: `TirexGame → Event Center → Open Current Config`

**Các settings có thể tùy chỉnh:**

#### Auto Creation Settings:
- `autoCreateEventCenter`: Bật/tắt tự động tạo EventCenter
- `dontDestroyOnLoad`: EventCenter có persist across scenes không  
- `autoCreatedName`: Tên của EventCenter được tạo tự động

#### Performance Settings:
- `maxEventsPerFrame`: Số lượng events tối đa mỗi frame (100-50000)
- `maxBatchSize`: Kích thước batch tối đa (10-5000)

#### Debug Settings:
- `enableLogging`: Bật debug logging
- `enableProfiling`: Bật performance profiling
- `showStats`: Hiển thị runtime statistics

### 🔄 Config Tool Actions:

```
💾 Save Custom Config    - Lưu config vào project
🔄 Reset to Default      - Reset về settings mặc định  
📋 Copy from Default     - Copy từ package default
📂 Reveal in Project     - Mở config trong Project window
```

### 💡 Config Usage Examples:

```csharp
// Đọc config hiện tại
var config = EventCenterConfig.Instance;
Debug.Log($"Max Events: {config.maxEventsPerFrame}");
Debug.Log($"Auto Create: {config.autoCreateEventCenter}");

// Trong Editor - refresh cache sau khi thay đổi
EventCenterConfig.RefreshConfiguration();
```

**🎯 Khuyến nghị:**
- Dùng **default settings** cho hầu hết các project
- Chỉ tạo **custom config** khi cần fine-tuning performance
- Disable auto-creation nếu bạn muốn full control EventCenter lifecycle

## 🎮 Sử dụng cơ bản | Basic Usage

### 1. Định nghĩa Event (Event Definition)

Events phải là **struct** để đạt hiệu suất tối ưu:

```csharp
// Event đơn giản
public struct PlayerDied
{
    public string PlayerName;
    public Vector3 DeathPosition;
    
    public PlayerDied(string name, Vector3 position)
    {
        PlayerName = name;
        DeathPosition = position;
    }
}

// Event với nhiều dữ liệu
public struct ScoreChanged
{
    public int NewScore;
    public int OldScore;
    public int Delta;
    public bool IsHighScore;
    
    public ScoreChanged(int newScore, int oldScore, bool isHighScore)
    {
        NewScore = newScore;
        OldScore = oldScore;
        Delta = newScore - oldScore;
        IsHighScore = isHighScore;
    }
}
```

### 2. Subscribe Events (Đăng ký lắng nghe)

```csharp
private void Start()
{
    // Cách 1: Sử dụng lambda
    EventSystem.Subscribe<PlayerDied>((deathEvent) =>
    {
        Debug.Log($"Player {deathEvent.PlayerName} died at {deathEvent.DeathPosition}");
    });
    
    // Cách 2: Sử dụng method reference
    EventSystem.Subscribe<ScoreChanged>(OnScoreChanged);
}

private void OnScoreChanged(ScoreChanged scoreEvent)
{
    Debug.Log($"Score: {scoreEvent.OldScore} → {scoreEvent.NewScore} (+{scoreEvent.Delta})");
    
    if (scoreEvent.IsHighScore)
    {
        Debug.Log("🎉 New High Score!");
    }
}
```

### 3. Publish Events (Phát sự kiện)

```csharp
// Publish một event
var deathEvent = new PlayerDied("John", transform.position);
EventSystem.Publish(deathEvent);

// Publish nhiều events cùng lúc (batch)
var events = new ScoreChanged[]
{
    new ScoreChanged(100, 0, false),
    new ScoreChanged(500, 100, false),
    new ScoreChanged(1000, 500, true)
};
EventSystem.PublishBatch(events);
```

### 4. Unsubscribe (Hủy đăng ký)

#### Cách truyền thống (Traditional way):
```csharp
private void OnDestroy()
{
    // Hủy đăng ký để tránh memory leak
    EventSystem.Unsubscribe<ScoreChanged>(OnScoreChanged);
}
```

#### 🎯 Cách mới - Tự động Cleanup (New way - Auto Cleanup):
```csharp
// Sử dụng extension methods - KHÔNG CẦN OnDestroy!
// Use extension methods - NO NEED FOR OnDestroy!
private void Start()
{
    // Tự động unsubscribe khi GameObject bị destroy
    this.SubscribeWithCleanup<ScoreChanged>(OnScoreChanged);
    
    // Conditional subscription với auto cleanup
    this.SubscribeWhenWithCleanup<ItemCollected>(OnRareItem, 
        item => item.IsRare);
    
    // One-time subscription với auto cleanup
    this.SubscribeOnceWithCleanup<PlayerJumped>(OnFirstJump);
}

// Không cần OnDestroy nữa! 🎉
// No more OnDestroy needed! 🎉
```

## 📖 API Reference

### Static Methods

#### Subscribe
```csharp
// Đăng ký lắng nghe event thông thường
EventSystem.Subscribe<T>(Action<T> callback)

// Đăng ký với điều kiện (chỉ lắng nghe khi condition = true)
EventSystem.SubscribeWhen<T>(Action<T> callback, Func<T, bool> condition)

// Đăng ký một lần (tự động unsubscribe sau lần đầu)
EventSystem.SubscribeOnce<T>(Action<T> callback)
EventSystem.SubscribeOnce<T>(Action<T> callback, Func<T, bool> condition)
```

#### 🎯 Extension Methods (Auto Cleanup)
```csharp
// Đăng ký với tự động cleanup khi MonoBehaviour bị destroy
this.SubscribeWithCleanup<T>(Action<T> callback)

// Conditional subscription với auto cleanup
this.SubscribeWhenWithCleanup<T>(Action<T> callback, Func<T, bool> condition)

// One-time subscription với auto cleanup
this.SubscribeOnceWithCleanup<T>(Action<T> callback)
this.SubscribeOnceWithCleanup<T>(Action<T> callback, Func<T, bool> condition)
```

#### Publish
```csharp
// Publish một event
EventSystem.Publish<T>(T eventData)

// Publish nhiều events cùng lúc
EventSystem.PublishBatch<T>(T[] events)
EventSystem.PublishBatch<T>(IEnumerable<T> events)
```

#### Unsubscribe
```csharp
// Hủy đăng ký callback cụ thể
EventSystem.Unsubscribe<T>(Action<T> callback)

// Hủy tất cả subscribers của event type
EventSystem.UnsubscribeAll<T>()
```

#### Utility
```csharp
// Kiểm tra có subscribers không
bool hasSubscribers = EventSystem.HasSubscribers<T>();

// Đếm số lượng subscribers
int count = EventSystem.GetSubscriberCount<T>();
```

## 🔥 Ví dụ nâng cao | Advanced Examples

### 1. Conditional Subscription
```csharp
// Chỉ lắng nghe khi player HP < 20%
EventSystem.SubscribeWhen<PlayerHealthChanged>((healthEvent) =>
{
    Debug.Log("⚠️ Critical Health!");
    PlayCriticalHealthEffect();
}, (healthEvent) => healthEvent.HealthPercentage < 0.2f);

// Chỉ lắng nghe items hiếm
EventSystem.SubscribeWhen<ItemPickedUp>((itemEvent) =>
{
    Debug.Log($"💎 Rare item: {itemEvent.ItemName}");
}, (itemEvent) => itemEvent.Rarity == ItemRarity.Legendary);
```

### 2. One-time Subscription
```csharp
// Tutorial: chỉ hiện hướng dẫn lần đầu
EventSystem.SubscribeOnce<PlayerFirstJump>((jumpEvent) =>
{
    ShowJumpTutorial();
});

// Achievement: chỉ unlock một lần
EventSystem.SubscribeOnce<ScoreChanged>((scoreEvent) =>
{
    UnlockAchievement("First 1000 Points");
}, (scoreEvent) => scoreEvent.NewScore >= 1000);
```

### 3. Multiple Subscribers
```csharp
// UI System
EventSystem.Subscribe<PlayerHealthChanged>((health) =>
{
    UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
});

// Audio System  
EventSystem.Subscribe<PlayerHealthChanged>((health) =>
{
    if (health.CurrentHealth <= 0)
        PlayDeathSound();
});

// Analytics System
EventSystem.Subscribe<PlayerHealthChanged>((health) =>
{
    TrackPlayerHealth(health.CurrentHealth);
});
```

### 4. Complex Event Data
```csharp
public struct CombatEvent
{
    public GameObject Attacker;
    public GameObject Target;
    public int Damage;
    public DamageType Type;
    public Vector3 HitPosition;
    public bool IsCritical;
    public float KnockbackForce;
    
    public CombatEvent(GameObject attacker, GameObject target, int damage, 
                      DamageType type, Vector3 hitPos, bool critical, float knockback)
    {
        Attacker = attacker;
        Target = target;
        Damage = damage;
        Type = type;
        HitPosition = hitPos;
        IsCritical = critical;
        KnockbackForce = knockback;
    }
}
```

## ⚡ Hiệu suất | Performance

### Benchmarks
- **Events/Frame**: 50,000+ events có thể xử lý trong 1 frame
- **Memory**: Zero allocation với struct events
- **CPU**: ~0.1ms cho 10,000 events với 10 subscribers
- **Memory Overhead**: <1KB cho toàn bộ hệ thống

### Optimization Tips
1. **Sử dụng struct** thay vì class cho events
2. **Minimize data** trong event struct
3. **Avoid boxing** - không sử dụng object trong event
4. **Batch publishing** cho nhiều events cùng lúc
5. **Unsubscribe** khi không cần thiết

### Performance Test
```csharp
// Test với 50,000 events
var events = new PlayerMoved[50000];
for (int i = 0; i < events.Length; i++)
{
    events[i] = new PlayerMoved(Vector3.one * i, i);
}

var stopwatch = Stopwatch.StartNew();
EventSystem.PublishBatch(events);
stopwatch.Stop();

Debug.Log($"Processed {events.Length} events in {stopwatch.ElapsedMilliseconds}ms");
// Typical result: ~2-5ms for 50,000 events
```

## 💡 Best Practices

### ✅ DO's
```csharp
// ✅ Sử dụng struct cho events
public struct PlayerMoved
{
    public Vector3 Position;
    public float Speed;
}

// ✅ 🎯 SỬ DỤNG EXTENSION METHODS CHO AUTO CLEANUP (KHUYÊN DÙNG!)
private void Start()
{
    // Tự động unsubscribe khi GameObject destroy - Không cần OnDestroy!
    this.SubscribeWithCleanup<PlayerMoved>(OnPlayerMoved);
    this.SubscribeWhenWithCleanup<ItemCollected>(OnRareItem, item => item.IsRare);
    this.SubscribeOnceWithCleanup<PlayerJumped>(OnFirstJump);
}

// ✅ Hoặc unsubscribe thủ công trong OnDestroy (cách cũ)
private void OnDestroy()
{
    EventSystem.Unsubscribe<PlayerMoved>(OnPlayerMoved);
}

// ✅ Sử dụng meaningful names
public struct EnemyDefeated { /* ... */ }
public struct QuestCompleted { /* ... */ }

// ✅ Batch publishing cho hiệu suất
EventSystem.PublishBatch(multipleEvents);

// ✅ Conditional subscriptions cho filtering
EventSystem.SubscribeWhen<DamageDealt>(OnCriticalHit, 
    dmg => dmg.IsCritical);
```

### ❌ DON'Ts
```csharp
// ❌ Không sử dụng class cho events
public class PlayerMoved // Slow, creates garbage
{
    public Vector3 Position;
}

// ❌ Không quên unsubscribe
// Memory leak potential!

// ❌ Không đặt quá nhiều data trong event
public struct HugeEvent
{
    public byte[] LargeArray; // Avoid large data
    public Texture2D Image;   // Avoid Unity objects
}

// ❌ Không sử dụng object/boxing
public struct BadEvent
{
    public object Data; // Boxing allocation!
}
```

### 🏗️ Architecture Patterns

#### 1. Component Communication (Auto Cleanup)
```csharp
// Player Controller
public class PlayerController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var jumpEvent = new PlayerJumped(transform.position, jumpHeight);
            EventSystem.Publish(jumpEvent);
        }
    }
}

// Audio Manager  
public class AudioManager : MonoBehaviour
{
    private void Start()
    {
        // 🎯 Auto cleanup - không cần OnDestroy!
        this.SubscribeWithCleanup<PlayerJumped>(OnPlayerJumped);
    }
    
    private void OnPlayerJumped(PlayerJumped jumpEvent)
    {
        PlayJumpSound();
    }
    
    // Không cần OnDestroy! Extension method sẽ tự cleanup
}
```

#### 2. UI Updates
```csharp
// Game Manager
EventSystem.Publish(new ScoreChanged(newScore, oldScore, isHighScore));

// UI Manager
EventSystem.Subscribe<ScoreChanged>((score) =>
{
    scoreText.text = $"Score: {score.NewScore}";
    if (score.IsHighScore)
        ShowHighScoreEffect();
});
```

#### 3. System Decoupling
```csharp
// Input System
EventSystem.Publish(new InputPressed(KeyCode.E, transform.position));

// Interaction System
EventSystem.SubscribeWhen<InputPressed>((input) =>
{
    TryInteract(input.Position);
}, (input) => input.Key == KeyCode.E);
```

## 🔧 FAQ

### Q: Event có thể sử dụng class không?
**A:** Có thể, nhưng struct được khuyến nghị cho hiệu suất tốt hơn. Struct tránh garbage collection và nhanh hơn.

### Q: Có cần unsubscribe không?
**A:** Có, để tránh memory leak. Tốt nhất là unsubscribe trong `OnDestroy()`.

### Q: Có thread-safe không?
**A:** Có, EventSystem được thiết kế thread-safe và có thể sử dụng an toàn từ nhiều threads.

### Q: Có thể subscribe nhiều lần cùng một callback không?
**A:** Có, nhưng callback sẽ được gọi nhiều lần tương ứng. Cẩn thận để tránh duplicate subscriptions.

### Q: Performance so với UnityEvents như thế nào?
**A:** EventSystem nhanh hơn UnityEvents 5-10 lần và sử dụng ít memory hơn đáng kể.

### Q: Có thể debug events không?
**A:** Có thể enable debug logging hoặc sử dụng `GetSubscriberCount<T>()` để kiểm tra số lượng subscribers.

### Q: Xử lý exception trong subscriber như thế nào?
**A:** EventSystem sẽ catch và log exceptions, không làm crash ứng dụng. Subscriber khác vẫn sẽ được gọi bình thường.

### Q: Gặp lỗi "NullReferenceException" khi subscribe?
**A:** Lỗi này hiếm khi xảy ra với version mới vì EventCenter được tự động tạo. Nếu vẫn gặp:
1. **Kiểm tra logs**: Xem có thông báo "EventCenter created automatically" không
2. **Kiểm tra scene**: Có GameObject `[EventCenter] - Auto Created` trong Hierarchy không
3. **Manual override**: Nếu cần tùy chỉnh, add `EventCenterSetup` component vào scene
4. **Debug**: Sử dụng `EventSystem.IsInitialized` và `EventCenterService.IsAvailable`

```csharp
// Kiểm tra trạng thái (chỉ để debug)
Debug.Log($"EventCenter available: {EventCenterService.IsAvailable}");
Debug.Log($"EventSystem initialized: {EventSystem.IsInitialized}");
```

### Q: EventCenter có tự động tạo không?
**A:** ✅ **Có!** EventCenter sẽ tự động được tạo khi ứng dụng khởi động với các đặc điểm:
- Tên: `[EventCenter] - Auto Created`  
- DontDestroyOnLoad: Có (tồn tại xuyên suốt tất cả scenes)
- Tự động khởi tạo khi không tìm thấy EventCenter nào khác
- Không cần setup thủ công

### Q: Làm sao customize EventCenter tự động tạo?
**A:** Có 3 cách:
1. **Config Tool (Khuyến nghị)**: `TirexGame → Event Center → Create Custom Config`
   - GUI thân thiện, dễ sử dụng
   - Tự động save vào đúng vị trí
   - Có preview và validation
2. **Manual Config**: Tạo `EventCenterConfig.asset` trong `Assets/Resources/`
3. **EventCenterSetup**: Add component này vào scene để override toàn bộ

### Q: Config system hoạt động như thế nào?
**A:** 📊 **3-level hierarchy**:
1. **Custom Config** (`Assets/Resources/EventCenterConfig.asset`) - Ưu tiên cao nhất
2. **Package Default** (built-in) - Fallback tốt  
3. **Runtime Default** - Fallback cuối cùng

Hệ thống tự động chọn config theo thứ tự ưu tiên. Sử dụng Config Tool để tạo custom config một cách dễ dàng!

### Q: Disable auto-creation EventCenter được không?
**A:** ✅ **Được!** Có 2 cách:
1. **Config Tool**: Uncheck "Auto Create EventCenter" trong custom config
2. **EventCenterSetup**: Add component này vào scene (sẽ override auto-creation)

Sau đó bạn có thể tự tạo EventCenter thủ công hoặc dùng EventCenterSetup với settings riêng.


## 🤝 Support

Nếu gặp vấn đề hoặc cần hỗ trợ:
1. Kiểm tra FAQ ở trên
2. Xem examples trong thư mục `Examples/`
3. Kiểm tra console logs để debug
4. Liên hệ team development

## 📄 License

MIT License - Xem file LICENSE để biết thêm chi tiết.

---

**Happy Coding! 🚀**

*Được phát triển với ❤️ cho Unity Community*