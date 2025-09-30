# StateMachine - Type-Safe Implementation

Một implementation của State Machine pattern được cải tiến không phụ thuộc vào MonoBehaviour và sử dụng Type làm key thay vì string để đảm bảo type safety.

## Tính năng chính

### ✅ Đã cải tiến:
- **Không phụ thuộc MonoBehaviour**: StateMachine giờ đây là pure C# class
- **Type-safe**: Sử dụng generic types thay vì string làm key
- **Loại bỏ Update/FixedUpdate**: Không còn frame-based updates tự động
- **Manual Tick**: Cung cấp cơ chế tick thủ công cho states cần updates

### 🔧 Tính năng mới:
- **ITickableState**: Interface cho states cần periodic updates
- **BaseTickableState**: Base class cho tickable states
- **Generic transitions**: Type-safe state transitions
- **Flexible initialization**: Constructor với tùy chọn debug logging

## Cách sử dụng

### 1. Tạo StateMachine

```csharp
// Tạo StateMachine với debug logging
var stateMachine = new StateMachine(enableDebugLogs: true);
```

### 2. Tạo States

```csharp
// Tạo states kế thừa từ BaseState hoặc BaseTickableState
public class IdleState : BaseState
{
    public override async UniTask OnEnter()
    {
        Debug.Log("Entered Idle State");
        await base.OnEnter();
    }
    
    public override async UniTask OnExit()
    {
        Debug.Log("Exited Idle State");
        await base.OnExit();
    }
}

// Cho states cần updates
public class MovingState : BaseTickableState
{
    public override void OnTick()
    {
        // Logic update ở đây
        UpdateMovement();
    }
}
```

### 3. Đăng ký States

```csharp
// Sử dụng generic methods để đảm bảo type safety
stateMachine.AddState(new IdleState());
stateMachine.AddState(new WalkingState());
stateMachine.AddState(new RunningState());
```

### 4. Thiết lập Transitions

```csharp
// Type-safe transitions với generic types
stateMachine.AddTransition<IdleState, WalkingState>(() => Input.GetKey(KeyCode.W));
stateMachine.AddTransition<WalkingState, RunningState>(() => Input.GetKey(KeyCode.LeftShift));
stateMachine.AddTransition<RunningState, IdleState>(() => !Input.GetKey(KeyCode.W));
```

### 5. Khởi động và quản lý

```csharp
// Khởi động với state ban đầu
await stateMachine.StartAsync<IdleState>();

// Trong Update loop (nếu cần)
private async void Update()
{
    // Kiểm tra transitions tự động
    await stateMachine.CheckTransitionsAsync();
    
    // Manual tick cho tickable states
    stateMachine.Tick();
}

// Chuyển state thủ công
await stateMachine.TransitionToAsync<RunningState>();

// Dừng StateMachine
await stateMachine.StopAsync();
```

## Interfaces và Base Classes

### IState
```csharp
public interface IState
{
    UniTask OnEnter();
    UniTask OnExit();
}
```

### ITickableState
```csharp
public interface ITickableState
{
    void OnTick();
}
```

### BaseState
- Basic implementation của IState
- Phù hợp cho states đơn giản không cần updates

### BaseTickableState
- Kế thừa BaseState và implement ITickableState
- Sử dụng cho states cần periodic updates

### BaseState<T> và BaseTickableState<T>
- Generic versions với context type
- Sử dụng Initialize(T context) để thiết lập context

## So sánh với version cũ

| Tính năng | Version cũ | Version mới |
|-----------|------------|-------------|
| Dependency | MonoBehaviour | Pure C# class |
| State Key | string | Type (type-safe) |
| Updates | Automatic Update/FixedUpdate | Manual Tick() |
| Transitions | AddTransition(string, string) | AddTransition<TFrom, TTo>() |
| State Registration | AddState(IState) | AddState<T>(T state) |
| State Starting | StartAsync(string) | StartAsync<T>() |

## Lợi ích

1. **Type Safety**: Compile-time checking thay vì runtime errors
2. **Performance**: Không phụ thuộc MonoBehaviour lifecycle
3. **Flexibility**: Manual control over updates
4. **Maintainability**: Easier to refactor với strongly-typed states
5. **Testability**: Pure C# class dễ unit test hơn

## Ví dụ hoàn chỉnh

Xem file `StateMachineExample.cs` để xem implementation example đầy đủ.