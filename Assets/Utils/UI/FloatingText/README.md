# Floating Text System

Hệ thống hiển thị text bay cho Unity game, hỗ trợ cả 2D và 3D với TextMeshPro.

## Yêu cầu

- Unity 2020.3+
- TextMeshPro package

## Setup

### 1. Tạo FloatingTextData Asset

Tạo cấu hình cho các loại text trong game của bạn:

1. Right-click trong Project → **Create → TirexGame → UI → Floating Text Data**
2. Đặt tên (ví dụ: `DamageText`, `HealingText`, `GoldText`)
3. Cấu hình trong Inspector:
   - **Move Direction**: Hướng bay (VD: `0, 1, 0` để bay lên)
   - **Move Speed**: Tốc độ bay
   - **Lifetime**: Thời gian tồn tại
   - **Font Size**: Kích thước chữ
   - **Text Color**: Màu chữ
   - **Scale Curve**: Animation curve cho scale
   - **Alpha Curve**: Animation curve cho độ mờ

### 2. Thêm FloatingTextManager vào Scene

- FloatingTextManager sẽ tự động được tạo khi bạn gọi code lần đầu
- Hoặc tạo GameObject mới và add component `FloatingTextManager`

## Cách sử dụng

### Cách 1: Sử dụng FloatingTextData Asset (Khuyên dùng)

```csharp
using TirexGame.Utils.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private FloatingTextData damageData;
    [SerializeField] private FloatingTextData criticalData;

    public void TakeDamage(float damage, bool isCritical = false)
    {
        var data = isCritical ? criticalData : damageData;
        string text = isCritical ? $"{damage}!" : damage.ToString("0");

        FloatingTextFactory.Create(text, transform.position, data);
    }
}
```

### Cách 2: Tạo Data trong Code

```csharp
public class GameManager : MonoBehaviour
{
    private FloatingTextData damageData;

    void Start()
    {
        // Tạo config cho damage text
        damageData = ScriptableObject.CreateInstance<FloatingTextData>();
        damageData.MoveDirection = new Vector3(0, 1, 0);
        damageData.MoveSpeed = 3f;
        damageData.Lifetime = 1.2f;
        damageData.FontSize = 48f;
        damageData.TextColor = Color.red;
    }

    public void ShowDamage(float amount, Vector3 position)
    {
        FloatingTextFactory.Create(amount.ToString("0"), position, damageData);
    }
}
```

### Cách 3: Sử dụng Builder Pattern

```csharp
FloatingTextFactory.Builder()
    .SetText("Critical!")
    .SetPosition(transform.position)
    .WithColor(Color.red)
    .WithFontSize(60)
    .WithSpeed(4f)
    .WithLifetime(2f)
    .Bold()
    .Show();
```

## API Reference

### FloatingTextFactory

```csharp
// Tạo floating text
FloatingTextFactory.Create(string text, Vector3 position, FloatingTextData data, bool is3D = false)

// Tạo tại vị trí screen
FloatingTextFactory.CreateAtScreenPosition(string text, Vector3 screenPos, FloatingTextData data)

// Sử dụng Builder
FloatingTextFactory.Builder()
```

### FloatingTextManager

```csharp
// Show text ở world space (3D)
FloatingTextManager.Instance.ShowText3D(text, position, data)

// Show text ở screen space (2D UI)
FloatingTextManager.Instance.ShowText2D(text, screenPosition, data)

// Tự động convert world position sang screen
FloatingTextManager.Instance.ShowTextAtWorldPosition(text, worldPosition, data)

// Xóa tất cả floating text
FloatingTextManager.Instance.ClearAll()
```

## Ví dụ

### Hệ thống Damage

```csharp
public class CombatSystem : MonoBehaviour
{
    [SerializeField] private FloatingTextData damageData;
    [SerializeField] private FloatingTextData healingData;
    [SerializeField] private FloatingTextData criticalData;

    public void ShowDamage(float amount, Vector3 position)
    {
        FloatingTextFactory.Create(amount.ToString("0"), position, damageData);
    }

    public void ShowHealing(float amount, Vector3 position)
    {
        FloatingTextFactory.Create($"+{amount}", position, healingData);
    }

    public void ShowCritical(float amount, Vector3 position)
    {
        FloatingTextFactory.Create($"{amount}!", position, criticalData);
    }
}
```

### Hệ thống nhặt vật phẩm

```csharp
public class Pickup : MonoBehaviour
{
    [SerializeField] private FloatingTextData goldData;
    [SerializeField] private int goldAmount = 100;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FloatingTextFactory.Create($"+{goldAmount} Gold", transform.position, goldData);
            Destroy(gameObject);
        }
    }
}
```

## Tips

- **Tạo asset cho từng loại text**: Damage, Healing, Gold, XP, Miss, etc.
- **Reuse assets**: Sử dụng lại các asset đã tạo trong toàn bộ project
- **Object pooling**: Hệ thống tự động pool, không cần lo về performance
- **Animation curves**: Dùng curve để tạo hiệu ứng đẹp mắt (bounce, fade, etc.)

## Xem thêm

- `CustomTypesExample.cs` - Ví dụ định nghĩa text types riêng
- `CombatSystemExample.cs` - Ví dụ tích hợp vào combat system
- `ARCHITECTURE.md` - Chi tiết kiến trúc hệ thống
