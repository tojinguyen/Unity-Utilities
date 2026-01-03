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
   - **Font Style**: Bold, Italic, Normal
   - **Use World Space**: Tick nếu muốn dùng 3D mode
   - **Randomize Direction**: Thêm biến thiên ngẫu nhiên
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

        // Tự động chuyển đổi world position sang screen position
        FloatingTextFactory.Create(text, transform.position, data);
    }
}
```

### Cách 2: Sử dụng Extension Methods

```csharp
public class Player : MonoBehaviour
{
    [SerializeField] private FloatingTextData healingData;

    public void Heal(float amount)
    {
        // Sử dụng extension method
        this.ShowFloatingText($"+{amount}", healingData);
    }
}
```

### Cách 3: Sử dụng FloatingTextManager trực tiếp

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private FloatingTextData xpData;

    public void ShowXP(float amount, Vector3 position)
    {
        FloatingTextManager.Instance.ShowTextAtWorldPosition(
            $"+{amount} XP",
            position,
            xpData
        );
    }
}
```

## API Reference

### FloatingTextFactory (Khuyên dùng)

```csharp
// Tạo floating text tại world position (tự động convert sang screen)
FloatingTextFactory.Create(string text, Vector3 worldPosition, FloatingTextData data)

// Tạo tại vị trí screen
FloatingTextFactory.CreateAtScreenPosition(string text, Vector3 screenPos, FloatingTextData data)

// Tạo ở 3D world space
FloatingTextFactory.Create3D(string text, Vector3 worldPosition, FloatingTextData data)
```

### Extension Methods

```csharp
// Hiển thị floating text tại vị trí của object
transform.ShowFloatingText(text, data)
gameObject.ShowFloatingText(text, data)
component.ShowFloatingText(text, data)
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

### Convenience Methods

```csharp
// Hiển thị damage (sử dụng preset có sẵn)
FloatingTextManager.Instance.ShowDamage(damage, position)

// Hiển thị healing (sử dụng preset có sẵn)
FloatingTextManager.Instance.ShowHealing(amount, position)

// Hiển thị critical hit (sử dụng preset có sẵn)
FloatingTextManager.Instance.ShowCritical(damage, position)
```

## Ví dụ

### Hệ thống Damage đơn giản

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
            // Sử dụng extension method
            transform.ShowFloatingText($"+{goldAmount} Gold", goldData);
            Destroy(gameObject);
        }
    }
}
```

### Sử dụng với Health System

```csharp
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private FloatingTextData damageData;
    [SerializeField] private FloatingTextData healData;

    private float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        this.ShowFloatingText(damage.ToString("0"), damageData);
    }

    public void Heal(float amount)
    {
        health += amount;
        this.ShowFloatingText($"+{amount}", healData);
    }
}
```

## Quick Presets

FloatingTextData có sẵn các preset trong Editor:

1. Mở FloatingTextData asset trong Inspector
2. Cuộn xuống phần **Quick Presets**
3. Click:
   - **Damage**: Text đỏ, bold, bay lên
   - **Healing**: Text xanh, bold, bay lên
   - **Critical**: Text vàng, lớn hơn, bay nhanh hơn

Hoặc sử dụng trong code:

```csharp
// Sử dụng preset mặc định (không cần tạo asset)
FloatingTextManager.Instance.ShowDamage(100, transform.position);
FloatingTextManager.Instance.ShowHealing(50, transform.position);
FloatingTextManager.Instance.ShowCritical(200, transform.position);
```

## Tips

- **Tạo asset cho từng loại text**: Damage, Healing, Gold, XP, Miss, etc.
- **Reuse assets**: Sử dụng lại các asset đã tạo trong toàn bộ project
- **Object pooling**: Hệ thống tự động pool, không cần lo về performance
- **Animation curves**: Dùng curve để tạo hiệu ứng đẹp mắt (bounce, fade, etc.)
- **Use World Space**: Tick option này trong FloatingTextData nếu muốn text ở 3D world space
- **Extension methods**: Sử dụng `transform.ShowFloatingText()` để code ngắn gọn hơn
