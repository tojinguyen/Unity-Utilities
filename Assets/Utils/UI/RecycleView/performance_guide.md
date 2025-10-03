# RecycleView Performance Guide

## Tổng quan về Performance

RecycleView được thiết kế để xử lý hiệu quả các danh sách lớn với hàng ngàn phần tử. Dưới đây là các thông tin về performance và tối ưu hóa.

## Performance Characteristics

### ✅ Điểm mạnh (Good Performance)
- **Virtualization**: Chỉ render các items trong viewport + buffer
- **Object Pooling**: Tái sử dụng GameObjects thay vì tạo/hủy liên tục
- **Binary Search**: Tìm kiếm first visible index với O(log n)
- **Cached Positions**: Pre-calculate và cache vị trí của tất cả items

### ⚠️ Điểm cần lưu ý (Performance Considerations)

#### 1. Initialization Performance
- **Với datasets nhỏ (< 1000 items)**: Initialization ngay lập tức
- **Với datasets lớn (≥ 1000 items)**: Sử dụng Coroutine để spread calculation across frames

#### 2. Memory Usage
```csharp
// Memory estimates for different dataset sizes:
// 1,000 items   ≈ 4KB   positions array + ~32KB data objects
// 10,000 items  ≈ 40KB  positions array + ~320KB data objects  
// 100,000 items ≈ 400KB positions array + ~3.2MB data objects
```

## Optimization Features

### 1. Coroutine-based Position Calculation
```csharp
// Automatically triggered for large datasets
if (_dataList.Count > 1000)
{
    // Uses coroutine to prevent frame drops
    // Processes 200 items per frame
    StartCoroutine(CalculateItemPositionsCoroutine());
}
```

### 2. Intelligent Content Sizing
- Small datasets: Immediate content size update
- Large datasets: Deferred update after position calculation completes

### 3. Optimized Scroll Detection
- Binary search for first visible item: O(log n)
- Buffer system to reduce item creation/destruction
- Early return if visible range hasn't changed

## Performance Testing

### Sử dụng PerformanceTestController

```csharp
// Test với các kích thước khác nhau
[ContextMenu("Run Performance Test (1K items)")]    // ~1-5ms
[ContextMenu("Run Performance Test (10K items)")]   // ~10-50ms spread across frames
[ContextMenu("Run Performance Test (50K items)")]   // ~100-300ms spread across frames
```

### Expected Performance Metrics

| Dataset Size | Initialization Time | Memory Usage | Frame Impact |
|--------------|-------------------|--------------|--------------|
| 1,000 items  | 1-5ms            | ~36KB        | No frame drop |
| 10,000 items | 10-50ms (spread) | ~360KB       | Smooth (coroutine) |
| 50,000 items | 100-300ms (spread)| ~1.8MB      | Smooth (coroutine) |

## Best Practices

### 1. Data Structure Optimization
```csharp
// ✅ Good: Implement IRecycleViewData efficiently
public class MyData : IRecycleViewData
{
    public int ItemType { get; set; }
    // Keep data minimal and cache expensive calculations
}

// ❌ Avoid: Heavy objects in data list
public class HeavyData : IRecycleViewData
{
    public Texture2D heavyTexture; // This will consume lots of memory
    public string[] massiveArray;  // Avoid large arrays in data objects
}
```

### 2. Prefab Optimization
```csharp
// ✅ Set proper sizeDelta on prefabs for auto-detection
// ✅ Use consistent anchor settings
// ✅ Minimize component count on item prefabs
```

### 3. Runtime Usage
```csharp
// ✅ Good: Batch data updates
var newData = GenerateLargeDataset();
recycleView.SetData(newData); // Single rebuild

// ❌ Avoid: Frequent rebuilds
foreach(var item in smallUpdates) {
    recycleView.SetData(currentData); // Multiple rebuilds = poor performance
}

// ✅ Good: Use Refresh() for data updates without structure changes
recycleView.Refresh(); // Only updates visible items
```

## Monitoring Performance

### 1. Built-in Logging
```csharp
// Automatic performance logging for large datasets
"RecycleView setup time: 45ms"
"Total initialization time: 50ms"
```

### 2. Unity Profiler Integration
- Monitor `CalculateItemPositionsCoroutine()` in Profiler
- Check memory allocations during `SetData()`
- Watch frame time during scrolling

### 3. Custom Performance Testing
```csharp
// Use PerformanceTestController for benchmarking
var controller = GetComponent<PerformanceTestController>();
controller.RunPerformanceTestWithSize(yourDataSize);
```

## Troubleshooting Performance Issues

### Frame Drops During Initialization
```csharp
// Problem: Large dataset causing frame drops
// Solution: Coroutine threshold is automatically applied at 1000+ items
// Manual override:
private const int COROUTINE_THRESHOLD = 500; // Lower threshold for slower devices
```

### High Memory Usage
```csharp
// Problem: Memory usage too high
// Solutions:
1. Implement IRecycleViewData with minimal data
2. Use object pooling for expensive data objects
3. Load data on-demand in BindData()
```

### Slow Scrolling
```csharp
// Problem: Scrolling performance issues
// Check:
1. Item prefab complexity (too many components?)
2. BindData() method efficiency in items
3. Buffer size (higher buffer = smoother but more memory)
```

## Configuration Recommendations

### Buffer Size
```csharp
// Low-end devices
viewBuffer = 1;  // Minimal memory usage

// Mid-range devices  
viewBuffer = 2;  // Default, good balance

// High-end devices
viewBuffer = 3-5; // Smoother scrolling, higher memory
```

### Coroutine Batch Size
```csharp
// In CalculateItemPositionsCoroutine():
const int itemsPerFrame = 200; // Default

// For slower devices:
const int itemsPerFrame = 100; // More frames, less work per frame

// For faster devices:
const int itemsPerFrame = 500; // Fewer frames, more work per frame
```