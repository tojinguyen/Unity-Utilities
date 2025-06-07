# Service Injection Performance Guide

## Performance Optimization

Việc sử dụng **Reflection** trong Unity có thể gây ra các vấn đề về performance nghiêm trọng, đặc biệt trong runtime. Hệ thống ServiceInjector đã được tối ưu hóa để loại bỏ hoàn toàn việc sử dụng Reflection.

## Các Phương Pháp Injection (Từ Nhanh Nhất đến Chậm Nhất)

### 1. Direct Service Resolution (Fastest ⚡)
Phương pháp nhanh nhất - trực tiếp resolve service khi cần:

```csharp
public class FastComponent : MonoBehaviour
{
    private IPlayerService _playerService;
    
    private void Start()
    {
        // Direct resolution - no reflection, no overhead
        _playerService = ServiceLocatorManager.Resolve<IPlayerService>();
    }
    
    private void Update()
    {
        // Use service directly
        _playerService.UpdatePlayer();
    }
}
```

**Ưu điểm:**
- Không có reflection overhead
- Không có attribute processing
- Kiểm soát hoàn toàn thời điểm resolve
- Performance tốt nhất

**Nhược điểm:**
- Cần viết thêm code
- Phải tự handle error cases

### 2. Interface-Based Injection (Fast ⚡)
Sử dụng interface `IServiceInjectable` để injection tự động:

```csharp
public class OptimizedComponent : InjectableMonoBehaviour
{
    private IPlayerService _playerService;
    private IScoreService _scoreService;
    
    public override void InjectServices(IServiceLocator serviceLocator)
    {
        // No reflection - direct method calls
        if (ServiceInjectionHelper.TryInject(serviceLocator, out _playerService))
        {
            Debug.Log("Player service injected");
        }
        
        ServiceInjectionHelper.TryInject(serviceLocator, out _scoreService, required: false);
    }
}
```

**Ưu điểm:**
- Không có reflection
- Tự động injection trong Awake
- Kiểm soát tốt error handling
- Code rõ ràng, dễ debug

**Nhược điểm:**
- Cần implement interface method
- Nhiều code hơn direct resolution một chút

### 3. ~~Attribute-Based Injection~~ (Deprecated - Slow 🐌)
**KHÔNG SỬ DỤNG** - Phương pháp cũ sử dụng Reflection:

```csharp
// DON'T USE - Poor performance due to reflection
[InjectService] private IPlayerService _playerService;
```

**Vấn đề:**
- Sử dụng reflection để scan fields
- Sử dụng reflection để call generic methods
- Overhead lớn trong runtime
- Khó debug và maintain

## Best Practices

### 1. Resolve Services Sớm
Resolve services trong initialization phase, không trong Update loop:

```csharp
// Good
private void Awake()
{
    _playerService = ServiceLocatorManager.Resolve<IPlayerService>();
}

private void Update()
{
    _playerService.UpdatePlayer(); // Fast - no resolution overhead
}

// Bad
private void Update()
{
    var playerService = ServiceLocatorManager.Resolve<IPlayerService>(); // Slow - resolution every frame
    playerService.UpdatePlayer();
}
```

### 2. Sử dụng TryResolve cho Optional Services
```csharp
// Safe resolution for optional services
if (ServiceLocatorManager.TryResolve<IAnalyticsService>(out var analytics))
{
    analytics.TrackEvent("feature_used");
}
```

### 3. Check Service Existence
```csharp
// Check before resolving expensive services
if (ServiceLocatorManager.IsRegistered<IExpensiveService>())
{
    var service = ServiceLocatorManager.Resolve<IExpensiveService>();
    service.DoExpensiveOperation();
}
```

### 4. Cache Services References
```csharp
public class ServiceUser : MonoBehaviour
{
    // Cache services as fields
    private IPlayerService _playerService;
    private IScoreService _scoreService;
    
    private void Awake()
    {
        // Resolve once during initialization
        _playerService = ServiceLocatorManager.Resolve<IPlayerService>();
        _scoreService = ServiceLocatorManager.Resolve<IScoreService>();
    }
    
    // Use cached references in runtime methods
    public void OnPlayerAction()
    {
        _playerService.MovePlayer(Vector3.forward);
        _scoreService.AddScore(10);
    }
}
```

## Performance Comparison

| Method | Performance | Flexibility | Ease of Use |
|--------|-------------|-------------|-------------|
| Direct Resolution | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| Interface Injection | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| ~~Reflection Injection~~ | ⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

## Migration từ Reflection-based Injection

### Trước (Reflection - Slow):
```csharp
public class OldComponent : InjectableMonoBehaviour
{
    [InjectService] private IPlayerService _playerService;
    [InjectService("main")] private IAudioService _audioService;
    [InjectService("optional", optional: true)] private IAnalyticsService _analytics;
    
    // Automatic injection via reflection - poor performance
}
```

### Sau (Interface-based - Fast):
```csharp
public class NewComponent : InjectableMonoBehaviour
{
    private IPlayerService _playerService;
    private IAudioService _audioService;
    private IAnalyticsService _analytics;
    
    public override void InjectServices(IServiceLocator serviceLocator)
    {
        // No reflection - direct method calls
        _playerService = ServiceInjectionHelper.Inject<IPlayerService>(serviceLocator);
        _audioService = ServiceInjectionHelper.Inject<IAudioService>(serviceLocator, "main");
        ServiceInjectionHelper.TryInject(serviceLocator, out _analytics, "optional", required: false);
    }
}
```

## Kết Luận

Việc loại bỏ Reflection khỏi service injection system mang lại:

1. **Performance cải thiện đáng kể** - Không có reflection overhead
2. **Code rõ ràng hơn** - Explicit service dependencies
3. **Dễ debug hơn** - Stack traces không bị obfuscate bởi reflection
4. **Build size nhỏ hơn** - Ít metadata cho reflection
5. **Better IL2CPP compatibility** - Reflection có thể gây issues với IL2CPP

Luôn ưu tiên **Direct Resolution** hoặc **Interface Injection** thay vì reflection-based approaches cho performance tốt nhất trong Unity projects.
