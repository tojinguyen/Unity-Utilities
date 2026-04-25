# StateMachine Package

Package State Machine hiệu năng cao cho Unity, hỗ trợ **zero-allocation transitions**, **CancellationToken**, và hai phong cách API khác nhau để phù hợp với mọi use case.

---

## Tổng quan nhanh

| Bạn muốn... | Dùng |
|---|---|
| State là class riêng, có context chung (DI-friendly) | `StateMachine<T>` |
| State đơn giản, cấu hình nhanh bằng fluent builder | `SimpleStateMachine<TEnum>` |
| State chứa sub-states phức tạp (class-based) | `BaseCompositeState` |

---

## Các Class & Interface

### Core

| Tên | Loại | Mô tả |
|---|---|---|
| `StateMachine<T>` | class | State machine type-safe, dùng class làm state key |
| `SimpleStateMachine<TEnum>` | class | State machine dùng enum + fluent builder |
| `IState` | interface | Giao diện cơ bản: `OnEnter(ct)`, `OnExit(ct)` |
| `IState<T>` | interface | Kế thừa `IState`, thêm `Initialize(T context)` |
| `ITickableState` | interface | Thêm `OnTick()` cho states cần update theo frame |
| `ICompositeState` | interface | State có thể chứa sub-states |
| `StateTransition` | internal | Lưu delegate thực thi — không dùng Reflection |

### Base Classes

| Tên | Kế thừa | Khi nào dùng |
|---|---|---|
| `BaseState` | `IState` | State đơn giản, không cần context |
| `BaseState<T>` | `IState<T>` | State có truy cập `Context` |
| `BaseTickableState` | `BaseState + ITickableState` | State cần `OnTick()` mỗi frame |
| `BaseTickableState<T>` | `BaseState<T> + ITickableState` | Tickable có context |
| `BaseCompositeState` | `BaseState + ICompositeState` | State phức tạp chứa sub-states |
| `BaseCompositeState<T>` | `BaseCompositeState + IState<T>` | Composite với context |
| `BaseTickableCompositeState` | `BaseCompositeState + ITickableState` | Composite + tick |
| `BaseTickableCompositeState<T>` | `BaseCompositeState<T> + ITickableState` | Composite + tick + context |

---

## StateMachine\<T\>

Phù hợp khi mỗi state là một class riêng biệt với logic phức tạp.

### 1. Định nghĩa Context và States

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using TirexGame.Utils.Patterns.StateMachine;

// Context — dữ liệu chia sẻ giữa các states
public class PlayerContext
{
    public int HP { get; set; } = 100;
    public Transform Transform { get; set; }
}

// State đơn giản
public class IdleState : BaseState<PlayerContext>
{
    public override UniTask OnEnter(CancellationToken ct = default)
    {
        Debug.Log("Idle: bắt đầu");
        return base.OnEnter(ct); // UniTask.CompletedTask — zero cost
    }

    public override UniTask OnExit(CancellationToken ct = default)
    {
        Debug.Log("Idle: kết thúc");
        return base.OnExit(ct);
    }
}

// State async (ví dụ: loading)
public class LoadingState : BaseState<PlayerContext>
{
    public override async UniTask OnEnter(CancellationToken ct = default)
    {
        Debug.Log("Loading...");
        await UniTask.Delay(2000, cancellationToken: ct); // CT được tôn trọng
        await LoadAssetsAsync(ct);
    }
}

// State cần update mỗi frame
public class RunningState : BaseTickableState<PlayerContext>
{
    public override void OnTick()
    {
        Context.Transform.Translate(Vector3.forward * Time.deltaTime * 5f);
    }
}
```

### 2. Khởi tạo và cấu hình

```csharp
public class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerContext> _sm;

    private async void Start()
    {
        var ctx = new PlayerContext { Transform = transform, HP = 100 };
        _sm = new StateMachine<PlayerContext>(ctx, enableDebugLogs: true);

        // Đăng ký states
        _sm.AddState(new IdleState());
        _sm.AddState(new RunningState());
        _sm.AddState(new DeadState());

        // Transitions tự động (zero-allocation — dùng delegate, KHÔNG dùng Reflection)
        _sm.AddTransition<IdleState, RunningState>(() => Input.GetKey(KeyCode.W));
        _sm.AddTransition<RunningState, IdleState>(() => !Input.GetKey(KeyCode.W));

        // Global transition — fire từ BẤT KỲ state nào
        _sm.AddGlobalTransition<DeadState>(() => ctx.HP <= 0);

        // destroyCancellationToken → tự dừng khi GameObject bị Destroy
        await _sm.StartAsync<IdleState>(destroyCancellationToken);
    }

    private async void Update()
    {
        // Kiểm tra transitions tự động
        await _sm.CheckTransitionsAsync();

        // Tick states cần update
        _sm.Tick();
    }

    private async void OnDestroy()
    {
        await _sm.StopAsync();
    }
}
```

### 3. Các property và method hữu ích

```csharp
// Kiểm tra state hiện tại (type-safe)
if (_sm.IsInState<IdleState>())
    Debug.Log("Đang Idle");

// Trạng thái máy
bool running     = _sm.IsRunning;
bool transitioning = _sm.IsTransitioning;
IState current   = _sm.CurrentState;
Type currentType = _sm.CurrentStateType;

// Chuyển state thủ công
await _sm.TransitionToAsync<RunningState>();

// Dừng machine
await _sm.StopAsync();

// Xóa state động
_sm.RemoveState<LoadingState>();
```

### 4. Events

```csharp
_sm.OnStateChanged    += (from, to) => Debug.Log($"{from} → {to}");
_sm.OnTransitionFailed += (type)     => Debug.LogError($"Transition đến {type} thất bại");
```

---

## SimpleStateMachine\<TEnum\>

Phù hợp khi muốn cấu hình nhanh bằng fluent builder, không cần tạo class riêng.

### 1. Setup cơ bản

```csharp
public enum PlayerState { Idle, Moving, Combat, Dead }

private SimpleStateMachine<PlayerState> _sm;

private async void Start()
{
    _sm = new SimpleStateMachine<PlayerState>(enableLogs: true);

    _sm.State(PlayerState.Idle)
        .OnEnter(() => Debug.Log("Idle"))
        .OnTick(() => CheckInput())
        .TransitionTo(PlayerState.Moving, () => Input.GetKey(KeyCode.W))
        .TransitionTo(PlayerState.Dead,   () => _hp <= 0);

    _sm.State(PlayerState.Moving)
        .OnEnter(async ct => await PlayAnimationAsync(ct)) // Hỗ trợ CancellationToken
        .OnExit(() => StopMoving())
        .OnTick(() => MovePlayer())
        .TransitionTo(PlayerState.Idle, () => !Input.GetKey(KeyCode.W));

    _sm.State(PlayerState.Dead)
        .OnEnter(() => Debug.Log("Bạn đã chết!"));

    await _sm.StartAsync(PlayerState.Idle, destroyCancellationToken);
}

private async void Update()
{
    await _sm.CheckTransitionsAsync();
    _sm.Tick();
}
```

### 2. Sub-States (Composite State)

```csharp
public enum CombatSubState { Attacking, Defending, Casting }

_sm.State(PlayerState.Combat)
    .OnEnter(() => Debug.Log("Bước vào combat"))
    .WithSubStates<CombatSubState>(CombatSubState.Attacking) // initial sub-state
        .SubState(CombatSubState.Attacking)
            .OnEnter(() => Debug.Log("Tấn công!"))
            .OnTick(() => HandleAttackInput())
            .TransitionTo(CombatSubState.Defending, () => Input.GetKey(KeyCode.LeftControl))
            .TransitionTo(CombatSubState.Casting,   () => Input.GetKeyDown(KeyCode.E))
        .And()
        .SubState(CombatSubState.Defending)
            .OnEnter(() => Debug.Log("Phòng thủ"))
            .TransitionTo(CombatSubState.Attacking, () => !Input.GetKey(KeyCode.LeftControl))
        .And()
        .SubState(CombatSubState.Casting)
            .OnEnter(async ct =>
            {
                Debug.Log("Đang cast...");
                await UniTask.Delay(1500, cancellationToken: ct);
                Debug.Log("Spell hoàn thành!");
            })
            .TransitionTo(CombatSubState.Attacking, () => Input.GetKeyDown(KeyCode.Q))
        .Done(); // alias của .EndSubStates()
```

### 3. Kiểm tra trạng thái

```csharp
// Kiểm tra enum state
if (_sm.IsInState(PlayerState.Idle))
    Debug.Log("Đang Idle");

bool running = _sm.IsRunning;
PlayerState current = _sm.CurrentState;
```

---

## CancellationToken — Best Practices

State machine tích hợp `CancellationToken` đầy đủ để tránh memory leak khi GameObject bị destroy.

### Quy tắc

```csharp
// ✅ ĐÚNG — Luôn truyền destroyCancellationToken
await _sm.StartAsync<IdleState>(destroyCancellationToken);

// ✅ ĐÚNG — Luôn truyền ct vào UniTask.Delay và async calls
public override async UniTask OnEnter(CancellationToken ct = default)
{
    await UniTask.Delay(2000, cancellationToken: ct);
    await SomeAsyncOperation(ct);
}

// ❌ SAI — Không truyền CT → memory leak khi Destroy
await UniTask.Delay(2000); // không biết khi nào hủy
```

### Khi nào CT bị hủy?

| Trigger | Hành động |
|---|---|
| `StopAsync()` gọi thủ công | Hủy internal CTS, exit current state |
| `destroyCancellationToken` được trigger | Hủy tất cả, clean up tự động |
| `OperationCanceledException` | Được catch riêng, không log error |

---

## BaseCompositeState — Class-based Sub-States

Dùng khi sub-states cần logic phức tạp dưới dạng class riêng.

```csharp
public class CombatState : BaseTickableCompositeState<GameContext>
{
    // Bật log cho state này
    public CombatState() => EnableDebugLogs = true;

    public override async UniTask OnEnter(CancellationToken ct = default)
    {
        // Đăng ký sub-states
        AddSubState(new AttackSubState(), isInitial: true);
        AddSubState(new DefendSubState());

        // Định nghĩa transitions giữa sub-states
        AddSubTransition<AttackSubState, DefendSubState>(() => isDefending);
        AddSubTransition<DefendSubState, AttackSubState>(() => !isDefending);

        await base.OnEnter(ct); // Tự động enter sub-state đầu tiên
    }

    public override void OnTick()
    {
        base.OnTick(); // Tự động tick current sub-state
        // Logic riêng của CombatState
    }
}
```

---

## Lựa chọn API

```
Cần state phức tạp, DI, testable?
    └─► StateMachine<T> + BaseState<T>

Cần cấu hình nhanh, enum-based?
    └─► SimpleStateMachine<TEnum>

State cần chứa sub-states?
    ├─► SimpleStateMachine → .WithSubStates<TSubEnum>()
    └─► StateMachine<T>   → BaseCompositeState
```

---

## Xem thêm

- [`Example/StateMachineExample.cs`](Example/StateMachineExample.cs) — Demo cơ bản với `StateMachine<T>`
- [`Example/SimpleStateMachineExample.cs`](Example/SimpleStateMachineExample.cs) — Demo player + traffic light
- [`Example/GameStateExample.cs`](Example/GameStateExample.cs) — Demo game phức tạp với composite states