# ServiceLocator Pattern Implementation

A comprehensive Service Locator pattern implementation for Unity that provides dependency injection and service resolution capabilities with minimal coupling.

## Overview

The Service Locator pattern provides a centralized registry for services that can be accessed globally without tight coupling between components. This implementation includes:

- **Interface-based service registration and resolution**
- **Keyed service support** for multiple implementations
- **Lazy service loading** with factory functions
- **Automatic dependency injection** via attributes
- **Unity integration** with MonoBehaviour lifecycle
- **Thread-safe operations** and error handling
- **Comprehensive examples** and documentation

## Core Components

### 1. IServiceLocator Interface
The main contract defining service locator operations:
- Service registration (instance and lazy)
- Service resolution with optional keys
- Service existence checking
- Service unregistration and cleanup

### 2. ServiceLocator Class
The core implementation providing:
- Thread-safe service management
- Exception handling with custom exceptions
- Logging support for debugging
- Performance optimizations

### 3. ServiceLocatorManager MonoBehaviour
Unity integration layer providing:
- Singleton access pattern
- Unity lifecycle integration
- Global static access methods
- Editor debugging support

### 4. ServiceInjector
Automatic dependency injection system:
- Attribute-based field injection
- Optional and keyed service support
- Hierarchy-wide injection capabilities
- Base classes for automatic injection

## Quick Start

### Basic Service Registration and Resolution

```csharp
// Define service interface
public interface IPlayerService
{
    void MovePlayer(Vector3 direction);
    Vector3 GetPlayerPosition();
}

// Implement service
public class PlayerService : IPlayerService
{
    private Vector3 _position;
    
    public void MovePlayer(Vector3 direction)
    {
        _position += direction;
    }
    
    public Vector3 GetPlayerPosition()
    {
        return _position;
    }
}

// Register service
ServiceLocatorManager.Register<IPlayerService>(new PlayerService());

// Resolve and use service
var playerService = ServiceLocatorManager.Resolve<IPlayerService>();
playerService.MovePlayer(Vector3.forward);
```

### Automatic Dependency Injection

```csharp
public class GameController : InjectableMonoBehaviour
{
    [InjectService]
    private IPlayerService _playerService;
    
    [InjectService("main")]
    private IAudioService _audioService;
    
    [InjectService("analytics", optional: true)]
    private IAnalyticsService _analytics;
    
    // Services are automatically injected on Awake
    private void Start()
    {
        _playerService.MovePlayer(Vector3.up);
        _audioService.PlaySound("jump");
        
        // Optional service might be null
        _analytics?.TrackEvent("game_started");
    }
}
```

## Advanced Usage

### Keyed Services

Register multiple implementations of the same interface:

```csharp
// Register different audio services
ServiceLocatorManager.Register<IAudioService>(new SfxAudioService(), "sfx");
ServiceLocatorManager.Register<IAudioService>(new MusicAudioService(), "music");

// Resolve specific implementations
var sfxAudio = ServiceLocatorManager.Resolve<IAudioService>("sfx");
var musicAudio = ServiceLocatorManager.Resolve<IAudioService>("music");
```

### Lazy Services

Register services that are created only when first accessed:

```csharp
// Register a lazy service
ServiceLocatorManager.RegisterLazy<IExpensiveService>(() => 
{
    Debug.Log("Creating expensive service...");
    return new ExpensiveService();
});

// Service is created only when first resolved
var service = ServiceLocatorManager.Resolve<IExpensiveService>();
```

### Service Dependencies

Create services that depend on other services:

```csharp
ServiceLocatorManager.RegisterLazy<IGameService>(() =>
{
    var playerService = ServiceLocatorManager.Resolve<IPlayerService>();
    var scoreService = ServiceLocatorManager.Resolve<IScoreService>();
    var audioService = ServiceLocatorManager.Resolve<IAudioService>("main");
    
    return new GameService(playerService, scoreService, audioService);
});
```

### Safe Resolution

Use TryResolve for optional services:

```csharp
if (ServiceLocatorManager.TryResolve<IAnalyticsService>(out var analytics))
{
    analytics.TrackEvent("feature_used");
}
else
{
    Debug.Log("Analytics service not available");
}
```

## Best Practices

### 1. Use Interfaces
Always register and resolve services using interfaces, not concrete types:

```csharp
// Good
ServiceLocatorManager.Register<IPlayerService>(new PlayerService());

// Avoid
ServiceLocatorManager.Register<PlayerService>(new PlayerService());
```

### 2. Register Early
Register services during application startup, preferably in a dedicated bootstrapper:

```csharp
public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        RegisterCoreServices();
        RegisterGameServices();
    }
    
    private void RegisterCoreServices()
    {
        ServiceLocatorManager.Register<IPlayerService>(new PlayerService());
        ServiceLocatorManager.Register<IScoreService>(new ScoreService());
    }
}
```

### 3. Handle Missing Services
Always handle cases where services might not be available:

```csharp
// Use TryResolve for optional services
if (ServiceLocatorManager.TryResolve<IOptionalService>(out var service))
{
    service.DoSomething();
}

// Or use exception handling for required services
try
{
    var requiredService = ServiceLocatorManager.Resolve<IRequiredService>();
    requiredService.DoSomething();
}
catch (ServiceNotFoundException)
{
    Debug.LogError("Required service not available!");
}
```

### 4. Clean Up on Scene Changes
Clear services when transitioning between major game states:

```csharp
private void OnDestroy()
{
    // Clear game-specific services but keep core services
    ServiceLocatorManager.Unregister<ILevelService>();
    ServiceLocatorManager.Unregister<IEnemyService>();
}
```

## Unity Integration

### Setting Up ServiceLocatorManager

1. Create an empty GameObject in your first scene
2. Add the `ServiceLocatorManager` component
3. Configure settings in the inspector:
   - **Enable Logging**: For debugging service operations
   - **Don't Destroy On Load**: To persist across scenes

### Using with Addressables

The ServiceLocator works well with Unity's Addressables system:

```csharp
public class AddressableServiceProvider : MonoBehaviour
{
    [SerializeField] private AssetReference audioServicePrefab;
    
    private async void Start()
    {
        var audioServiceGO = await AddressablesHelper.GetAssetAsync<GameObject>(
            audioServicePrefab, "ServiceProvider");
        var audioService = audioServiceGO.GetComponent<IAudioService>();
        
        ServiceLocatorManager.Register<IAudioService>(audioService);
    }
}
```

## Error Handling

The ServiceLocator provides comprehensive error handling:

### Custom Exceptions

- `ServiceNotFoundException`: Thrown when a requested service is not found
- `ServiceAlreadyRegisteredException`: Thrown when trying to register a service that's already registered

### Safe Resolution Methods

```csharp
// Throws exception if not found
var service = ServiceLocatorManager.Resolve<IService>();

// Returns false if not found
if (ServiceLocatorManager.TryResolve<IService>(out var service))
{
    // Use service safely
}

// Check existence without resolving
if (ServiceLocatorManager.IsRegistered<IService>())
{
    var service = ServiceLocatorManager.Resolve<IService>();
}
```

## Performance Considerations

### Lazy Loading
Use lazy registration for expensive services:

```csharp
// Service created only when needed
ServiceLocatorManager.RegisterLazy<IExpensiveService>(() => new ExpensiveService());
```

### Service Caching
Once a lazy service is resolved, it's cached for subsequent calls:

```csharp
// First call creates the service
var service1 = ServiceLocatorManager.Resolve<ILazyService>();

// Subsequent calls return the cached instance
var service2 = ServiceLocatorManager.Resolve<ILazyService>(); // Same instance
```

### Injection Performance
Service injection uses reflection and should be done during initialization, not in performance-critical loops:

```csharp
public class GameController : InjectableMonoBehaviour
{
    [InjectService] private IPlayerService _playerService;
    
    protected override void Awake()
    {
        base.Awake(); // Injection happens here
    }
    
    // Use injected services in Update without performance impact
    private void Update()
    {
        _playerService.UpdatePlayer();
    }
}
```

## Testing Support

The ServiceLocator pattern makes unit testing easier by allowing mock services:

```csharp
[Test]
public void TestPlayerMovement()
{
    // Create test service locator
    var testLocator = new ServiceLocator();
    
    // Register mock service
    var mockPlayerService = new Mock<IPlayerService>();
    testLocator.Register<IPlayerService>(mockPlayerService.Object);
    
    // Test with mock
    var controller = new GameController();
    ServiceInjector.Inject(controller, testLocator);
    
    // Verify behavior
    controller.MovePlayerUp();
    mockPlayerService.Verify(x => x.MovePlayer(Vector3.up), Times.Once);
}
```

## Migration Guide

### From Singleton Pattern

```csharp
// Old singleton approach
public class AudioManager : MonoSingleton<AudioManager>
{
    public void PlaySound(string sound) { }
}

// Usage
AudioManager.Instance.PlaySound("jump");

// New ServiceLocator approach
public interface IAudioService
{
    void PlaySound(string sound);
}

public class AudioService : IAudioService
{
    public void PlaySound(string sound) { }
}

// Registration
ServiceLocatorManager.Register<IAudioService>(new AudioService());

// Usage
var audio = ServiceLocatorManager.Resolve<IAudioService>();
audio.PlaySound("jump");
```

### From Direct References

```csharp
// Old direct reference approach
public class GameController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ScoreManager scoreManager;
    
    private void Start()
    {
        audioManager.PlaySound("start");
        scoreManager.AddScore(0);
    }
}

// New ServiceLocator approach
public class GameController : InjectableMonoBehaviour
{
    [InjectService] private IAudioService _audioService;
    [InjectService] private IScoreService _scoreService;
    
    private void Start()
    {
        _audioService.PlaySound("start");
        _scoreService.AddScore(0);
    }
}
```

## Examples

See `ServiceLocatorExamples.cs` for comprehensive examples including:

- Basic service registration and resolution
- Automatic dependency injection
- Keyed services
- Lazy services
- Service composition
- Error handling scenarios

The examples demonstrate real-world usage patterns and best practices for integrating the ServiceLocator pattern into your Unity projects.
