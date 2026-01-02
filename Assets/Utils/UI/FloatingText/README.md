# Floating Text System

A powerful and flexible floating text system for Unity games that supports both 2D and 3D environments with TextMeshPro integration.

## Features

✨ **Dual Mode Support**

- World Space (3D) floating text
- Screen Space (2D/UI) floating text
- Automatic mode detection

🎨 **Highly Customizable**

- Animation curves for scale and alpha
- Direction and speed control
- Randomization options
- Custom colors and fonts

⚡ **Performance Optimized**

- Built-in object pooling
- Automatic pool expansion
- Memory efficient

🛠️ **Easy to Use**

- Factory pattern for quick creation
- Builder pattern for custom configurations
- Predefined presets (Damage, Healing, Critical, etc.)
- Editor tools for testing

## Quick Start

### Basic Usage

```csharp
using TirexGame.Utils.UI;

// Show damage text
FloatingTextFactory.Damage(100, enemyPosition);

// Show healing text
FloatingTextFactory.Healing(50, playerPosition);

// Show critical hit
FloatingTextFactory.Critical(200, enemyPosition);

// Show custom text
FloatingTextFactory.Create("Level Up!", playerPosition, FloatingTextFactory.FloatingTextType.Experience);
```

### Using the Builder Pattern

```csharp
FloatingTextFactory.Builder()
    .SetText("Epic!")
    .SetPosition(transform.position)
    .WithColor(Color.yellow)
    .WithFontSize(60)
    .WithSpeed(3f)
    .WithLifetime(2f)
    .Bold()
    .Show();
```

### Using Custom Data

```csharp
// Create a custom configuration
FloatingTextData customData = ScriptableObject.CreateInstance<FloatingTextData>();
customData.MoveDirection = new Vector3(0, 1, 0);
customData.MoveSpeed = 2.5f;
customData.Lifetime = 1.5f;
customData.TextColor = Color.cyan;
customData.FontSize = 48f;

// Use it
FloatingTextManager.Instance.ShowText3D("Custom!", position, customData);
```

## Components

### FloatingText

The core component that handles animation and lifecycle of individual floating text instances.

**Key Properties:**

- `moveDirection`: Direction the text moves
- `moveSpeed`: Speed of movement
- `lifetime`: Duration before disappearing
- `scaleCurve`: Animation curve for scaling
- `alphaCurve`: Animation curve for fading

### FloatingTextManager

Singleton manager that handles pooling and creation of floating text instances.

**Key Methods:**

- `ShowText3D(text, position, data)`: Show text in world space
- `ShowText2D(text, screenPosition, data)`: Show text in screen space
- `ShowTextAtWorldPosition(text, worldPos, data)`: Convert world to screen position
- `ShowDamage/ShowHealing/ShowCritical`: Convenience methods

### FloatingTextData

ScriptableObject for storing reusable configurations.

**How to Create:**

1. Right-click in Project window
2. Create → TirexGame → UI → Floating Text Data
3. Configure properties in Inspector
4. Use in code or assign to prefabs

### FloatingTextFactory

Static factory for creating floating text with predefined presets.

**Available Presets:**

- `Damage`: Red, bold, with random direction
- `Healing`: Green, upward movement
- `Critical`: Orange, large, dramatic
- `Miss`: Gray, italic
- `Experience`: Blue, smooth fade
- `Gold`: Golden color

## Advanced Usage

### Creating Custom Prefabs

```csharp
// Set custom prefabs at runtime
FloatingTextManager.Instance.SetPrefabs(my3DPrefab, my2DPrefab);
```

### Working with Camera

```csharp
// Set custom camera for world-to-screen conversion
FloatingTextManager.Instance.SetCamera(myCamera);
```

### Working with Canvas

```csharp
// Set custom canvas for UI floating text
FloatingTextManager.Instance.SetCanvas(myCanvas);
```

### Pooling Configuration

In the Inspector:

- **Initial Pool Size**: Number of pre-created instances
- **Max Pool Size**: Maximum pool size limit
- **Auto Expand**: Automatically create new instances when pool is empty

## Examples

### Damage Number System

```csharp
public class Enemy : MonoBehaviour
{
    public void TakeDamage(float damage, bool isCritical = false)
    {
        health -= damage;

        if (isCritical)
        {
            FloatingTextFactory.Critical(damage, transform.position);
        }
        else
        {
            FloatingTextFactory.Damage(damage, transform.position);
        }
    }
}
```

### Pickup System

```csharp
public class Pickup : MonoBehaviour
{
    public int goldAmount = 100;

    void OnPickup()
    {
        FloatingTextFactory.Gold(goldAmount, transform.position);
        Destroy(gameObject);
    }
}
```

### Custom Animation

```csharp
// Create data with custom animation curves
FloatingTextData data = ScriptableObject.CreateInstance<FloatingTextData>();

// Bounce effect
data.ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
data.ScaleCurve.AddKey(0.5f, 1.3f); // Peak at middle

// Quick fade at end
data.AlphaCurve = new AnimationCurve(
    new Keyframe(0, 1),
    new Keyframe(0.8f, 1),
    new Keyframe(1, 0)
);

FloatingTextManager.Instance.ShowText3D("Bounce!", position, data);
```

### Multiple Texts at Once

```csharp
// Show multiple floating texts
for (int i = 0; i < 10; i++)
{
    Vector3 randomPos = transform.position + Random.insideUnitSphere * 2f;
    FloatingTextFactory.Damage(Random.Range(10, 100), randomPos);
}
```

## Best Practices

1. **Use Factory Methods**: Prefer `FloatingTextFactory` over direct manager access for common cases
2. **Cache Data Objects**: Create `FloatingTextData` assets and reuse them
3. **Configure Pool Size**: Set appropriate pool size based on your game's needs
4. **Clear When Done**: Call `ClearAll()` when changing scenes or during cleanup
5. **3D vs 2D**: Use 3D mode for world objects, 2D for HUD elements

## Performance Tips

- The system uses object pooling by default
- Pool automatically expands up to `maxPoolSize`
- Returned texts are reused, not destroyed
- Use `ClearAll()` to reset all active texts immediately

## Requirements

- Unity 2020.3 or higher
- TextMeshPro package
- .NET Standard 2.0+

## Integration with Other Systems

### UniTask Integration

```csharp
using Cysharp.Threading.Tasks;

async UniTask ShowDamageSequence()
{
    FloatingTextFactory.Damage(50, position);
    await UniTask.Delay(100);
    FloatingTextFactory.Damage(30, position + Vector3.up);
    await UniTask.Delay(100);
    FloatingTextFactory.Damage(20, position + Vector3.up * 2);
}
```

### DOTween Integration

```csharp
using DG.Tweening;

void ShowWithExtraAnimation()
{
    var floatingText = FloatingTextFactory.Damage(100, position);
    floatingText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
}
```

## Troubleshooting

**Text not showing:**

- Ensure FloatingTextManager exists in scene
- Check camera reference for 2D mode
- Verify canvas exists for UI mode
- Check layer and sorting order

**Performance issues:**

- Increase pool size to reduce instantiation
- Use ClearAll() periodically
- Reduce maxPoolSize if memory is limited

**Text appears at wrong position:**

- For 2D mode, use ShowTextAtWorldPosition()
- Check camera reference
- Verify canvas render mode

## License

Part of TirexGame Utilities package.

---

For more information and updates, visit the project repository.
