# RecycleView Click On Item Guide

## Tổng quan

RecycleView hỗ trợ tính năng click item một cách tự động và linh hoạt. Bạn có thể xử lý click events ở nhiều cấp độ khác nhau tùy theo nhu cầu.

## Cách hoạt động

### 1. Button Reference Setup (Recommended for Performance)

Trong `RecycleViewItem`, bạn có thể kéo reference Button để tăng performance:

```csharp
public class MyRecycleViewItem : RecycleViewItem<MyData>
{
    // Kéo Button reference trong Inspector (recommended)
    // Nếu không assign, sẽ tự động tìm hoặc tạo Button component
}
```

### 2. Auto Click Handler Setup

Khi một item được khởi tạo, `RecycleViewItem.Initialize()` sẽ tự động:
- Sử dụng Button reference nếu có (tốt nhất cho performance)
- Nếu không có reference, sẽ tìm hoặc thêm `Button` component
- Thiết lập click listener để gọi `NotifyItemClicked()`
- Button sẽ invisible nhưng vẫn clickable (nếu tự tạo)

### 3. Click Event Flow

```
User clicks item → Button.onClick → NotifyItemClicked() → OnItemClicked() → ParentRecycleView.OnItemClicked event
```

## Cách sử dụng

### Cấp độ 1: Subscribe event ở Controller

```csharp
public class MyController : MonoBehaviour
{
    [SerializeField] private RecycleView recycleView;
    
    private void Start()
    {
        // Subscribe to click events
        recycleView.OnItemClicked += HandleItemClicked;
    }
    
    private void OnDestroy()
    {
        if (recycleView != null)
        {
            recycleView.OnItemClicked -= HandleItemClicked;
        }
    }
    
    private void HandleItemClicked(RecycleViewItem item)
    {
        Debug.Log($"Item clicked at index: {item.CurrentDataIndex}");
        
        // Access data
        var data = myDataList[item.CurrentDataIndex];
        
        // Handle different data types
        if (data is MyTextData textData)
        {
            // Handle text item click
        }
        else if (data is MyImageData imageData)
        {
            // Handle image item click
        }
    }
}
```

### Cấp độ 2: Override OnItemClicked trong Item Class

```csharp
public class MyRecycleViewItem : RecycleViewItem<MyData>
{
    [SerializeField] private Image backgroundImage;
    private MyData _currentData;
    
    public override void BindData(MyData data, int index)
    {
        _currentData = data;
        // Bind UI elements...
    }
    
    // Custom behavior cho item này
    protected override void OnItemClicked()
    {
        base.OnItemClicked(); // Vẫn trigger global event
        
        // Custom behavior
        backgroundImage.color = Color.yellow;
        Debug.Log($"Custom click behavior for: {_currentData.Name}");
    }
}
```

### Cấp độ 3: Conditional Click Handling

```csharp
public class ConditionalClickItem : RecycleViewItem<MyData>
{
    private bool _isClickable = true;
    
    protected override void OnItemClicked()
    {
        if (!_isClickable)
        {
            Debug.Log("This item is not clickable right now");
            return; // Không gọi base.OnItemClicked()
        }
        
        base.OnItemClicked();
        // Custom logic...
    }
    
    public void SetClickable(bool clickable)
    {
        _isClickable = clickable;
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = clickable;
        }
    }
}
```

## Advanced Features

### Refresh Items sau khi Click

```csharp
private void HandleItemClicked(RecycleViewItem item)
{
    var data = myDataList[item.CurrentDataIndex];
    
    // Modify data
    data.IsSelected = !data.IsSelected;
    
    // Refresh single item
    recycleView.RefreshItem(item.CurrentDataIndex);
    
    // Or refresh all visible items if needed
    // recycleView.RefreshAllVisibleItems();
}
```

### Multiple Click Types (Performance Optimized)

```csharp
public class AdvancedClickItem : RecycleViewItem<MyData>
{
    [Header("Button References - Kéo từ Inspector")]
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button shareButton;
    
    public override void Initialize(RecycleView parent)
    {
        base.Initialize(parent);
        
        // Setup additional buttons using references (no GetComponent needed!)
        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(OnEditClicked);
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
    }
    
    private void OnEditClicked()
    {
        // Handle edit - main click event won't trigger
        Debug.Log("Edit action");
    }
    
    private void OnDeleteClicked()
    {
        // Handle delete
        Debug.Log("Delete action");
    }
    
    protected override void OnItemClicked()
    {
        base.OnItemClicked(); // Main item click
        Debug.Log("Main item clicked");
    }
}
```

### Custom Click Events

```csharp
public class RecycleViewWithCustomEvents : RecycleView
{
    public Action<RecycleViewItem> OnItemDoubleClicked;
    public Action<RecycleViewItem, bool> OnItemLongPressed;
}

public class CustomClickItem : RecycleViewItem<MyData>
{
    private float _lastClickTime;
    private bool _isLongPressing;
    
    protected override void OnItemClicked()
    {
        float timeSinceLastClick = Time.time - _lastClickTime;
        
        if (timeSinceLastClick < 0.3f) // Double click
        {
            if (ParentRecycleView is RecycleViewWithCustomEvents customRV)
            {
                customRV.OnItemDoubleClicked?.Invoke(this);
            }
        }
        else
        {
            base.OnItemClicked(); // Single click
        }
        
        _lastClickTime = Time.time;
    }
}
```

## Best Practices

1. **Sử dụng Button references** thay vì GetComponent() để tăng performance
2. **Luôn unsubscribe events** trong `OnDestroy()` để tránh memory leaks
3. **Kiểm tra null references** khi access data
4. **Sử dụng RefreshItem()** thay vì SetData() khi chỉ update một vài items
5. **Override OnItemClicked()** cho custom behavior per item type
6. **Sử dụng Button.interactable** để enable/disable click temporarily
7. **Tránh heavy operations** trong click handlers để không lag UI
8. **Kéo Button references trong Inspector** thay vì để code tự tìm

## Troubleshooting

### Item không click được
- Kiểm tra có GameObject nào khác đang block raycast không
- Đảm bảo Canvas có GraphicRaycaster
- Kiểm tra Button.interactable = true

### Event không trigger
- Đảm bảo đã subscribe event trước khi SetData()
- Kiểm tra OnItemClicked() có gọi base.OnItemClicked() không

### Performance issues với nhiều items
- Sử dụng object pooling cho buttons
- Tránh tạo quá nhiều event listeners
- Sử dụng conditional click handling