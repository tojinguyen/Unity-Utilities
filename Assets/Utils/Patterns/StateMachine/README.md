# StateMachine Package

Package State Machine với hiệu năng cao, zero-allocation transitions, và hỗ trợ `CancellationToken` đầy đủ.

---

## Các Class chính

| Class | Mô tả |
|---|---|
| `StateMachine<T>` | State machine dựa trên class, type-safe, có context |
| `SimpleStateMachine<TEnum>` | State machine dựa trên enum, fluent builder pattern |
| `BaseState` | Base class cho state đơn giản |
| `BaseState<T>` | Base class cho state có context |
| `BaseTickableState` | Base class có `OnTick()` |
| `BaseCompositeState` | Base class cho state chứa sub-states |

---

## StateMachine\<T\> — Dùng khi state là class riêng biệt

### Tạo State

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;

public class IdleState : BaseState<PlayerContext>
{
    public override UniTask OnEnter(CancellationToken ct = default)
    {
        Debug.Log("Entered Idle");
        return base.OnEnter(ct);
    }
}

public class RunningState : BaseTickableState<PlayerContext>
{
    public override void OnTick()
    {
        // Gọi mỗi frame từ stateMachine.Tick()
    }
}
```

### Setup và chạy

```csharp
private StateMachine<PlayerContext> _sm;

private async void Start()
{
    var ctx = new PlayerContext();
    _sm = new StateMachine<PlayerContext>(ctx, enableDebugLogs: true);

    _sm.AddState(new IdleState());
    _sm.AddState(new RunningState());

    // Transitions tự động — dùng delegate, KHÔNG dùng Reflection
    _sm.AddTransition<IdleState, RunningState>(() => Input.GetKey(KeyCode.W));
    _sm.AddTransition<RunningState, IdleState>(() => !Input.GetKey(KeyCode.W));

    // Global transition — có thể fire từ BẤT KỲ state nào
    _sm.AddGlobalTransition<DeadState>(() => _hp <= 0);

    // Truyền destroyCancellationToken để tự dừng khi GameObject bị Destroy
    await _sm.StartAsync<IdleState>(destroyCancellationToken);
}

private async void Update()
{
    await _sm.CheckTransitionsAsync();
    _sm.Tick();
}

private async void OnDestroy()
{
    await _sm.StopAsync();
}
```

### Kiểm tra state hiện tại

```csharp
if (_sm.IsInState<IdleState>())
    Debug.Log("Đang ở Idle");

if (_sm.IsRunning)
    Debug.Log("State machine đang chạy");
```

---

## SimpleStateMachine\<TEnum\> — Dùng khi muốn fluent builder với enum

```csharp
public enum PlayerState { Idle, Moving, Dead }

private SimpleStateMachine<PlayerState> _sm;

private async void Start()
{
    _sm = new SimpleStateMachine<PlayerState>(enableLogs: true);

    _sm.State(PlayerState.Idle)
        .OnEnter(() => Debug.Log("Idle"))
        .OnTick(() => { /* check input */ })
        .TransitionTo(PlayerState.Moving, () => Input.GetKey(KeyCode.W))
        .TransitionTo(PlayerState.Dead, () => _hp <= 0);

    _sm.State(PlayerState.Moving)
        .OnEnter(async ct => { await SomethingAsync(ct); }) // Hỗ trợ CancellationToken
        .OnExit(() => Debug.Log("Stop moving"))
        .TransitionTo(PlayerState.Idle, () => !Input.GetKey(KeyCode.W));

    // Sub-states (Composite State Pattern)
    _sm.State(PlayerState.Moving)
        .WithSubStates<CombatSubState>(CombatSubState.Attacking)
            .SubState(CombatSubState.Attacking)
                .OnEnter(() => Debug.Log("Attacking"))
                .TransitionTo(CombatSubState.Defending, () => isDefending)
            .And()
            .SubState(CombatSubState.Defending)
                .OnEnter(() => Debug.Log("Defending"))
        .Done(); // hoặc .EndSubStates()

    // Kiểm tra state
    if (_sm.IsInState(PlayerState.Idle))
        Debug.Log("Đang Idle");

    await _sm.StartAsync(PlayerState.Idle, destroyCancellationToken);
}
```

---

## CancellationToken — Best Practices

### Dùng `destroyCancellationToken` (khuyến nghị)

```csharp
// Truyền vào StartAsync — state machine sẽ tự cancel khi GameObject bị Destroy
await _sm.StartAsync<IdleState>(destroyCancellationToken);
```

### Override OnEnter/OnExit với CT

```csharp
public class LoadingState : BaseState
{
    public override async UniTask OnEnter(CancellationToken ct = default)
    {
        // Truyền ct vào tất cả UniTask.Delay và async calls
        await UniTask.Delay(2000, cancellationToken: ct);
        await LoadAssetsAsync(ct);
    }
}
```

---

## So sánh cải tiến

| # | Vấn đề cũ | Sau refactor |
|---|---|---|
| 1 | Dùng Reflection trong `CheckTransitionsAsync` (GC alloc mỗi frame) | Delegate pre-built, zero allocation |
| 2 | Không có CancellationToken | CT đầy đủ ở tất cả async methods |
| 3 | `BaseState.OnEnter()` yield 1 frame vô ích | `UniTask.CompletedTask` — zero cost |
| 4 | Tên file có dấu cách (`ICompositeState .cs`) | Đã đổi thành `ICompositeState.cs` |
| 5 | `StateMachine<T>` thiếu `IsRunning` | Đã thêm `IsRunning`, `IsInState<T>()` |
| 6 | try/catch lồng 3–4 cấp | 1 cấp try/catch duy nhất |
| 7 | `BaseCompositeState` log không qua flag | `EnableDebugLogs` property |
| 8 | Không có Global Transition | `AddGlobalTransition<TTo>()` |
| 9 | `EndSubStates()` khó nhớ | Thêm alias `Done()` |

Xem `Example/` để biết thêm cách dùng đầy đủ.