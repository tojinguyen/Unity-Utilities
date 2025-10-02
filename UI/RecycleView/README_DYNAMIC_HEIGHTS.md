# RecycleView with Dynamic Heights

A high-performance, recyclable UI list component for Unity that supports **dynamic item heights** and multiple item types. Perfect for creating efficient scrollable lists with thousands of items without performance degradation.

## Key Features ✨

- **Performance Optimized**: Only instantiates visible items + buffer
- **Multiple Item Types**: Support for different item prefabs in the same list
- **Dynamic Heights**: Support for items with different heights in vertical layout ⭐
- **Custom Heights**: Both per-item-type and per-item custom heights ⭐
- **Memory Efficient**: Automatic object pooling and recycling
- **Easy Integration**: Simple data binding with strongly-typed item classes
- **Flexible Layout**: Support for both vertical and horizontal layouts

## What's New in this Version 🆕

- **Dynamic Height Support**: Items can now have different heights within the same list
- **Custom Height API**: Specify custom heights per item type or per individual item
- **Optimized Performance**: Binary search for visible range calculation with varying heights
- **Position Caching**: Pre-calculated item positions for smooth scrolling
- **Backward Compatible**: Existing code continues to work without changes

## Quick Start

### 1. Setup Data Classes

Implement `IRecycleViewData` for your data:

```csharp
public class MyMessageData : IRecycleViewData
{
    public int ItemType => 0; // Unique ID for this item type
    public string Message { get; set; }
    
    // Optional: Custom height for this specific item
    public float CustomHeight => Message.Length > 50 ? 120f : -1f; // Tall for long messages
}

public class MyImageData : IRecycleViewData
{
    public int ItemType => 1;
    public Sprite Image { get; set; }
    public string Caption { get; set; }
    public float CustomHeight => 150f; // Always 150px tall
}
```

### 2. Create Item Prefabs & Scripts

Create item scripts inheriting from `RecycleViewItem<TData>`:

```csharp
public class MyMessageItem : RecycleViewItem<MyMessageData>
{
    [SerializeField] private Text messageText;
    
    public override void BindData(MyMessageData data, int index)
    {
        messageText.text = data.Message;
        
        // Automatically resize item to match custom height
        var rectTransform = transform as RectTransform;
        if (rectTransform != null && data.CustomHeight > 0)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.CustomHeight);
        }
    }
}
```

### 3. Configure RecycleView

1. Add `RecycleView` component to a GameObject with a `ScrollRect`
2. Configure item type mappings in the inspector:
   - **Type ID**: Unique integer for each item type
   - **Prefab**: The prefab for this item type
   - **Custom Height**: Default height for this item type (-1 to use defaultItemHeight) ⭐
   - **Custom Width**: Default width for this item type (-1 to use defaultItemWidth) ⭐
3. Set `defaultItemHeight` and `defaultItemWidth` for fallback dimensions

### 4. Populate with Data

```csharp
public class MyController : MonoBehaviour
{
    [SerializeField] private RecycleView recycleView;
    
    void Start()
    {
        var dataList = new List<IRecycleViewData>();
        
        for (int i = 0; i < 1000; i++)
        {
            if (i % 5 == 0)
            {
                // Every 5th item is an image with custom height
                dataList.Add(new MyImageData 
                { 
                    Image = someSprite, 
                    Caption = $"Image {i}",
                    CustomHeight = 120f + (i % 3) * 20f // Varying heights: 120, 140, 160
                });
            }
            else
            {
                // Text messages with dynamic height based on content
                string message = i % 3 == 0 ? "Short message" : "This is a much longer message that requires more space to display properly";
                dataList.Add(new MyMessageData 
                { 
                    Message = message,
                    CustomHeight = message.Length > 30 ? 100f : -1f // Custom height for long messages
                });
            }
        }
        
        recycleView.SetData(dataList);
    }
}
```

## Dynamic Heights Support 📏

### Three Ways to Set Item Heights

#### 1. Default Height (Fallback)
Set in RecycleView inspector for all items without custom heights:
```csharp
// In inspector: Default Item Height = 80
```

#### 2. Per-Item-Type Heights
Set custom heights for entire item types in the RecycleView inspector:
```csharp
// In RecycleView inspector item type mappings:
// Type 0 (Text): Custom Height = 80
// Type 1 (Image): Custom Height = 120  
// Type 2 (Large Text): Custom Height = 150
```

#### 3. Per-Item Custom Heights (Most Flexible)
Individual items can override all other heights:
```csharp
public class DynamicTextData : IRecycleViewData
{
    public int ItemType => 0;
    public string Text { get; set; }
    
    // Dynamic height based on text length
    public float CustomHeight => Text.Length > 100 ? 150f : Text.Length > 50 ? 100f : -1f;
}
```

### Height Priority System
The system uses this priority order:
1. **Item CustomHeight** (highest priority)
2. **Item Type CustomHeight** 
3. **Default Item Height** (fallback)

### Automatic Layout Calculation
The RecycleView automatically:
- Pre-calculates item positions for smooth scrolling
- Uses binary search for efficient visible range detection
- Handles content size calculation with varying heights
- Maintains proper item positioning during scroll

## Performance Features 🚀

- **Object Pooling**: Automatically pools and reuses item instances
- **Culling**: Only renders visible items + configurable buffer
- **Position Caching**: Pre-calculates item positions for optimal performance ⭐
- **Binary Search**: Efficient visible range calculation for large lists ⭐
- **Memory Management**: Automatic cleanup and item recycling

## Configuration Options

### RecycleView Inspector
- **Item Type Mappings**: Map item types to prefabs and default sizes ⭐
- **Default Item Height/Width**: Fallback dimensions ⭐
- **View Buffer**: Extra items to render outside visible area
- **Layout Mode**: Vertical or Horizontal scrolling
- **Scroll Rect**: Reference to the ScrollRect component

### Layout Modes
- **Vertical**: Items arranged vertically (supports dynamic heights) ⭐
- **Horizontal**: Items arranged horizontally (supports dynamic widths) ⭐

## Advanced Usage

### Custom Click Handling
```csharp
recycleView.OnItemClicked += (item) => {
    Debug.Log($"Clicked item at index: {item.CurrentDataIndex}");
    
    // Access typed data through the data list
    var data = myDataList[item.CurrentDataIndex];
    if (data is MyMessageData messageData)
    {
        Debug.Log($"Message: {messageData.Message}");
    }
};
```

### Refreshing Data
```csharp
// Refresh visible items without rebuilding
recycleView.Refresh();

// Set new data and rebuild
recycleView.SetData(newDataList);
```

### Performance Tips
1. Use reasonable buffer sizes (2-5 items)
2. Pre-calculate custom heights when possible
3. Avoid frequent data changes during scrolling
4. Use object pooling for expensive item setup

## Migration Guide

### From Previous Version
Existing code will continue to work without changes. To add dynamic heights:

1. **Update Data Classes** (Optional):
   ```csharp
   // Before
   public class MyData : IRecycleViewData
   {
       public int ItemType => 0;
   }
   
   // After (optional)
   public class MyData : IRecycleViewData
   {
       public int ItemType => 0;
       public float CustomHeight => 120f; // Add custom height
   }
   ```

2. **Update Item Scripts** (Optional):
   ```csharp
   public override void BindData(MyData data, int index)
   {
       // Your existing code...
       
       // Add height adjustment (optional)
       var rectTransform = transform as RectTransform;
       if (rectTransform != null && data.CustomHeight > 0)
       {
           rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, data.CustomHeight);
       }
   }
   ```

3. **Update Inspector Settings**:
   - Rename `itemHeight` to `defaultItemHeight` in your scripts
   - Configure custom heights in item type mappings

## Requirements

- Unity 2019.4 or later
- UI system (Canvas, ScrollRect)

## Examples

Check the `Example` folder for:
- `RecycleViewExampleController`: Complete implementation with multiple item types and varying heights
- `TextRecycleViewItem`: Simple text item with dynamic height support
- `ImageRecycleViewItem`: Image item with custom height
- `LargeTextRecycleViewItem`: Large text item with automatic height adjustment

## Troubleshooting

### Common Issues

**Q: Items are not displaying with correct heights**
A: Make sure to adjust the RectTransform size in your item's BindData method when using custom heights.

**Q: Scrolling performance is poor with many different heights**
A: Consider grouping items with similar heights or using fewer unique height values.

**Q: Items are flickering during scroll**
A: Increase the view buffer size in the RecycleView inspector.

**Q: Content size is incorrect**
A: Ensure all your IRecycleViewData implementations return proper CustomHeight values.