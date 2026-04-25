# 🎯 Unity Event Center System

Hệ thống Event System hiệu suất cao cho Unity, sử dụng **Generic Static Cache (`EventHub<T>`)** để đạt tốc độ O(1) và **Zero-Allocation** hoàn toàn.

> ⚡ **~15× faster** than Dictionary-based event systems · Zero GC · Type-safe · Auto cleanup

---

## 📋 Table of Contents

- [Key Features](#-key-features)
- [Setup](#-setup)
- [Quick Start](#-quick-start)
- [API Reference](#-api-reference)
  - [EventSystem (Static API)](#eventsystem--static-api)
  - [Extension Methods (MonoBehaviour)](#extension-methods--monobehaviour)
  - [EventSubscriptionGroup](#eventsubscriptiongroup)
  - [IEventSubscription & AddTo()](#ieventsubscription--addto)
- [Advanced Examples](#-advanced-examples)
- [Best Practices](#-best-practices)
- [Architecture](#-architecture)
- [FAQ](#-faq)

---

## 🚀 Key Features

| Feature | Detail |
|---------|--------|
| **Zero Allocation** | Struct events + Static Generic Cache → No GC pressure |
| **O(1) Dispatch** | No Dictionary lookup per publish |
| **Auto Cleanup** | Extension methods tie subscription to MonoBehaviour lifetime |
| **Group Unsubscribe** | `EventSubscriptionGroup` — dispose all subs in one line |
| **Fluent API** | `.AddTo(group)` chaining |
| **Priority Support** | Listeners ordered by priority |
| **Conditional** | `SubscribeWhen<T>` — filter before invoke |
| **One-shot** | `SubscribeOnce<T>` — auto-unsubscribe after first match |
| **Legacy Support** | `BaseEvent` (class) path kept for backward compatibility |

---

## 📦 Setup

### Option A — Auto (Recommended)
`EventCenter` MonoBehaviour tự khởi động khi có trong Scene. Hệ thống tự `Initialize` qua `Awake`.

### Option B — Manual với `EventCenterSetup`
1. Tạo một **empty GameObject** trong Scene đầu tiên.
2. Add component **`EventCenterSetup`**.
3. ✅ Tick `Auto Initialize` và `Dont Destroy On Load`.

```csharp
// Không cần code thêm gì — hệ thống tự wire.
using TirexGame.Utils.EventCenter;
```

---

## ⚡ Quick Start

### 1. Định nghĩa Event (phải là `struct`)

```csharp
public struct PlayerDied
{
    public string PlayerName;
    public Vector3 DeathPosition;
}

public struct ScoreChanged
{
    public int OldScore;
    public int NewScore;
    public bool IsHighScore;
}
```

### 2. Publish

```csharp
EventSystem.Publish(new PlayerDied { PlayerName = "Alice", DeathPosition = transform.position });
```

### 3. Subscribe — 3 cách chọn 1

```csharp
// ── Cách 1: Static, tự quản lý lifetime ───────────────────────────────
IEventSubscription sub = EventSystem.Subscribe<PlayerDied>(OnPlayerDied);
// ...sau đó hủy:
sub.Dispose();

// ── Cách 2: Extension method — auto cleanup khi MonoBehaviour bị destroy ─
private void Start()
{
    this.Subscribe<PlayerDied>(OnPlayerDied);       // tự hủy khi destroy
    this.SubscribeWhen<ScoreChanged>(             // chỉ gọi khi IsHighScore
        OnHighScore,
        e => e.IsHighScore);
    this.SubscribeOnce<PlayerDied>(OnFirstDeath); // chỉ gọi 1 lần
}

// ── Cách 3: EventSubscriptionGroup — gom nhiều sub, dispose 1 lần ────────
private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();

void Init()
{
    EventSystem.Subscribe<PlayerDied>(OnPlayerDied).AddTo(_subs);
    EventSystem.Subscribe<ScoreChanged>(OnScore).AddTo(_subs);
    this.Subscribe<LevelStarted>(OnLevel).AddTo(_subs); // MonoBehaviour
}

void Cleanup()
{
    _subs.Dispose(); // hủy tất cả cùng lúc
}
```

---

## 📖 API Reference

### `EventSystem` — Static API

#### Publish

```csharp
// Publish một struct event (synchronous, zero-alloc)
EventSystem.Publish<T>(T payload)                    where T : struct

// Publish nhiều events liên tiếp
EventSystem.PublishBatch<T>(T[] events)              where T : struct
EventSystem.PublishBatch<T>(IEnumerable<T> events)  where T : struct
```

#### Subscribe

```csharp
// Subscribe với callback
IEventSubscription EventSystem.Subscribe<T>(Action<T> callback, int priority = 0)
    where T : struct

// Subscribe với IEventListener<T> interface
IEventSubscription EventSystem.Subscribe<T>(IEventListener<T> listener)
    where T : struct

// Subscribe chỉ khi condition == true
IEventSubscription EventSystem.SubscribeWhen<T>(
    Action<T> callback,
    Func<T, bool> condition,
    int priority = 0)
    where T : struct

// Subscribe một lần (tự unsubscribe sau khi fired)
IEventSubscription EventSystem.SubscribeOnce<T>(Action<T> callback, int priority = 0)
    where T : struct

// Subscribe một lần với condition
IEventSubscription EventSystem.SubscribeOnce<T>(
    Action<T> callback,
    Func<T, bool> condition,
    int priority = 0)
    where T : struct
```

#### Unsubscribe

```csharp
// Unsubscribe listener interface
EventSystem.Unsubscribe<T>(IEventListener<T> listener) where T : struct
EventSystem.Unsubscribe(IEventListener listener)

// Hoặc đơn giản hơn — dispose subscription object:
sub.Dispose();
```

#### Utility

```csharp
bool EventSystem.HasSubscribers<T>()          where T : struct
int  EventSystem.GetSubscriberCount<T>()      where T : struct
bool EventSystem.IsInitialized                // readonly property
void EventSystem.Clear()                      // xóa tất cả subscriptions
void EventSystem.LogStatus()                  // log trạng thái hệ thống
```

---

### Extension Methods — MonoBehaviour

> `using TirexGame.Utils.EventCenter;` là đủ — không cần import thêm.

Tất cả extension methods đều **tự động unsubscribe** khi `MonoBehaviour` bị `Destroy`.

```csharp
// Subscribe thông thường
IEventSubscription this.Subscribe<T>(Action<T> callback, int priority = 0)
    where T : struct

// Subscribe có filter
IEventSubscription this.SubscribeWhen<T>(
    Action<T> callback,
    Func<T, bool> condition,
    int priority = 0)
    where T : struct

// Subscribe một lần
IEventSubscription this.SubscribeOnce<T>(Action<T> callback, int priority = 0)
    where T : struct
```

> ⚠️ **Obsolete names** (vẫn hoạt động nhưng nên chuyển sang tên mới):
> `SubscribeWithCleanup` → `Subscribe`
> `SubscribeWhenWithCleanup` → `SubscribeWhen`
> `SubscribeOnceWithCleanup` → `SubscribeOnce`

---

### `EventSubscriptionGroup`

Dùng khi class **không phải MonoBehaviour** (ví dụ: plain C# service, presenter, ViewModel).

```csharp
public class UIPresenter
{
    private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();

    public void Initialize()
    {
        EventSystem.Subscribe<PlayerDied>(OnPlayerDied).AddTo(_subs);
        EventSystem.Subscribe<ScoreChanged>(OnScore).AddTo(_subs);
        EventSystem.SubscribeWhen<LevelLoaded>(
            OnBossLevel,
            e => e.IsBossLevel).AddTo(_subs);
    }

    public void Dispose()
    {
        _subs.Dispose(); // hủy tất cả bằng 1 dòng
    }
}
```

---

### `IEventSubscription` & `AddTo()`

`Subscribe` trả về `IEventSubscription`. Bạn có thể:

```csharp
// Dispose trực tiếp
IEventSubscription sub = EventSystem.Subscribe<PlayerDied>(OnPlayerDied);
sub.Dispose();

// Gắn vào EventSubscriptionGroup
sub.AddTo(myGroup);

// Gắn vào MonoBehaviour lifetime
sub.AddTo(this); // tự hủy khi MonoBehaviour bị destroy
```

---

## 🔥 Advanced Examples

### Conditional Subscription

```csharp
// Chỉ xử lý khi HP < 20%
EventSystem.SubscribeWhen<HealthChanged>(
    e => ShowLowHPWarning(),
    e => e.NewHP / (float)e.MaxHP < 0.2f
);

// Chỉ xử lý legendary items
this.SubscribeWhen<ItemPickedUp>(
    OnLegendaryItem,
    e => e.Rarity == Rarity.Legendary
);
```

### One-shot (Tutorial, Achievement)

```csharp
// Hiện tutorial chỉ lần đầu jump
EventSystem.SubscribeOnce<PlayerJumped>(_ => ShowJumpTutorial());

// Unlock achievement khi lần đầu đạt 1000 điểm
EventSystem.SubscribeOnce<ScoreChanged>(
    _ => UnlockAchievement("First Thousand"),
    e => e.NewScore >= 1000
);
```

### Group trên MonoBehaviour

```csharp
public class GameManager : MonoBehaviour
{
    private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();

    private void OnEnable()
    {
        EventSystem.Subscribe<PlayerDied>(OnPlayerDied).AddTo(_subs);
        EventSystem.Subscribe<ScoreChanged>(OnScore).AddTo(_subs);
        this.Subscribe<LevelLoaded>(OnLevel).AddTo(_subs);
    }

    private void OnDisable()
    {
        _subs.Dispose();
    }
}
```

### Batch Publish

```csharp
var events = new DamageEvent[]
{
    new DamageEvent { Target = enemy1, Damage = 50 },
    new DamageEvent { Target = enemy2, Damage = 75 },
    new DamageEvent { Target = boss,   Damage = 200 },
};
EventSystem.PublishBatch(events);
```

### IEventListener\<T\> Interface

```csharp
public class EnemyAI : MonoBehaviour, IEventListener<PlayerDied>
{
    private IEventSubscription _sub;

    private void OnEnable()  => _sub = EventSystem.Subscribe<PlayerDied>(this);
    private void OnDisable() => _sub?.Dispose();

    public void HandleEvent(PlayerDied e)
    {
        // React to player death
        CelebratVictory();
    }
}
```

---

## 💡 Best Practices

### ✅ DO

```csharp
// ✅ Events là struct
public struct EnemyKilled { public int EnemyId; public Vector3 Position; }

// ✅ Dùng extension methods — không cần OnDestroy
private void Start()
{
    this.Subscribe<EnemyKilled>(OnEnemyKilled);
}

// ✅ Dùng EventSubscriptionGroup cho non-MonoBehaviour
private readonly EventSubscriptionGroup _subs = new EventSubscriptionGroup();

// ✅ Filter tại source với SubscribeWhen
this.SubscribeWhen<DamageDealt>(OnCrit, e => e.IsCritical);

// ✅ Dispose subscription khi không dùng nữa
sub.Dispose();
```

### ❌ DON'T

```csharp
// ❌ Không dùng class cho event — GC allocation
public class BadEvent { public int Value; }

// ❌ Không subscribe rồi quên dispose
EventSystem.Subscribe<PlayerDied>(OnDied); // memory leak!

// ❌ Không nhét Unity objects nặng vào event struct
public struct HeavyEvent
{
    public Texture2D Texture; // ❌ reference type, tránh nếu được
    public byte[]    Data;    // ❌ array allocation
}

// ❌ Không dùng object/boxing
public struct BoxingEvent { public object Payload; } // ❌
```

---

## 🏗️ Architecture

```
EventSystem.Publish<T>(payload)
        │
        ▼
  EventHub<T>            ← Static Generic Cache per type T
  (static field)         ← Zero Dictionary lookup, O(1)
        │
        ▼
  HandlerEntry[]         ← Sorted by priority, soft-delete pattern
        │
        ▼
  Action<T> callbacks    ← Direct delegate invoke, Zero-Alloc
```

**Luồng Subscribe:**
```
EventSystem.Subscribe<T>(callback)
    → EventHub<T>.Subscribe(callback)
    → Returns IEventSubscription token
    → Token → AddTo(group) or AddTo(monoBehaviour) or manual Dispose()
```

**Legacy BaseEvent path** (backward compat):
```
EventSystem.PublishLegacy(baseEvent)
    → EventCenter.PublishEvent(baseEvent)
    → EventDispatcher.Dispatch(baseEvent)   ← synchronous, no queue
```

---

## 🔧 FAQ

**Q: Tại sao phải dùng `struct` cho event?**
> Struct không tạo GC allocation, không bị boxing khi dùng với `EventHub<T>`. Class event vẫn được hỗ trợ qua legacy path nhưng sẽ tạo GC.

**Q: `SubscribeWithCleanup` cũ của tôi có bị lỗi không?**
> Không. Các tên cũ được giữ lại với `[Obsolete]` attribute — code vẫn compile và chạy bình thường, chỉ hiện warning. Hãy chuyển dần sang `Subscribe` / `SubscribeWhen` / `SubscribeOnce`.

**Q: Có thread-safe không?**
> `EventHub<T>` không lock — phù hợp với Unity main thread. Nếu cần publish từ background thread, hãy marshal về main thread trước (ví dụ: `UniTask.SwitchToMainThread()`).

**Q: `EventSubscriptionGroup` khác gì extension method?**
> Extension method (`this.Subscribe<T>`) dùng cho MonoBehaviour, tự hủy khi component bị destroy.
> `EventSubscriptionGroup` dùng cho bất kỳ class nào, bạn tự gọi `Dispose()`.

**Q: Có thể dùng Priority không?**
> Có. Tất cả `Subscribe` overloads đều nhận `int priority = 0`. Giá trị cao hơn = được gọi trước.

**Q: Làm sao check hệ thống đang hoạt động?**
> ```csharp
> Debug.Log($"Initialized: {EventSystem.IsInitialized}");
> Debug.Log($"Subscribers for PlayerDied: {EventSystem.GetSubscriberCount<PlayerDied>()}");
> EventSystem.LogStatus(); // log toàn bộ trạng thái
> ```

**Q: Có batch publish không?**
> Có: `EventSystem.PublishBatch<T>(T[] events)` hoặc `EventSystem.PublishBatch<T>(IEnumerable<T>)`.

---

## 📄 License

MIT License — Xem file `LICENSE` để biết thêm chi tiết.

---

**Happy Coding! 🚀**
*Developed with ❤️ for Unity Community — TirexGame*