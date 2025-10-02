# RecycleView for Unity

A high-performance UI component for Unity designed to display large, scrollable lists of data with multiple item types. It uses item recycling (pooling) to maintain a high and stable framerate by avoiding the constant creation and destruction of GameObjects during scrolling.

This package is ideal for leaderboards, inventory screens, chat logs, or any other UI that needs to display a potentially large number of items efficiently.

## Features

- **High Performance:** Smoothly scrolls through thousands of items.
- **Zero GC Allocation:** No garbage is generated during scrolling, preventing FPS spikes.
- **Multi-Item Type Support:** Display multiple different prefabs/layouts within the same list.
- **Efficient Pooling:** Each item type gets its own object pool for fast reuse.
- **Layouts:** Supports both Vertical and Horizontal linear layouts.
- **Item Padding:** Configurable spacing between items for better visual layout.
- **Simple API:** A clean interface for setting data and handling user interaction.
- **Inspector Integration:** Configure all prefabs and settings directly in the Unity Inspector.

## Requirements

- Unity Engine
- TextMeshPro (for text display)

## How to Use

Follow these steps to integrate the RecycleView into your project.

### 1. Create Your Data Classes

Your data objects must implement the `IRecycleViewData` interface. The `ItemType` property is crucial for telling the view which prefab to use.

**Example:**
```csharp
// A simple data class for a text message
public class TextMessageData : IRecycleViewData
{
    // This corresponds to the TypeId you set in the Inspector
    public int ItemType => 0; 
    public string Message { get; set; }
}

// A data class for an item with an image
public class ImageMessageData : IRecycleViewData
{
    public int ItemType => 1; 
    public Sprite Image { get; set; }
    public string Caption { get; set; }
}
```

### 2. Create Your Item Prefab Scripts

Create a script for each of your prefabs that inherits from the generic `RecycleViewItem<TData>` class, where `TData` is the specific data class for that item. This gives you a strongly-typed `BindData` method, which is cleaner and safer.

**Example:**
```csharp
using TMPro;

// Inherit from RecycleViewItem<TData> with your data class
public class TextRecycleViewItem : RecycleViewItem<TextMessageData>
{
    [SerializeField] private TextMeshProUGUI messageText;

    // The BindData method is now strongly-typed. No casting needed!
    public override void BindData(TextMessageData data, int index)
    {
        messageText.text = $"{index}: {data.Message}";
    }
}
```

### 3. Scene and Component Setup

1.  Create a `UI -> Scroll View` in your scene.
2.  Add the **RecycleView.cs** component to the `Scroll View` GameObject.
3.  Select the `Content` child object of the `Scroll View`. In its `Rect Transform`, set the anchors to **top-stretch**.
    > **Important:** Do NOT add a `Vertical Layout Group` or `Content Size Fitter` to the `Content` object. The `RecycleView` script controls the layout itself.
4.  On the `RecycleView` component in the Inspector:
    *   Set the `Item Height` (or `Item Width` for horizontal layouts).
    *   Under `Item Type Mappings`, set the `Size` to the number of different prefabs you have.
    *   For each element, provide a unique `TypeId` and drag the corresponding UI prefab into the `Prefab` field.

### 4. Populate the View from Code

Finally, get a reference to the `RecycleView` and call `SetData()` with your list of data objects.

**Example:**
```csharp
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RecycleView myRecycleView;

    void Start()
    {
        // 1. Create your data
        var dataList = new List<IRecycleViewData>();
        for (int i = 0; i < 1000; i++)
        {
            dataList.Add(new TextMessageData { Message = "Hello World!" });
        }

        // 2. Set the data
        myRecycleView.SetData(dataList);

        // 3. (Optional) Set padding between items
        myRecycleView.SetItemPadding(10f); // 10 pixels padding

        // 4. (Optional) Listen for clicks
        myRecycleView.OnItemClicked += OnListItemClicked;
    }

    private void OnListItemClicked(RecycleViewItem item)
    {
        Debug.Log($"You clicked item #{item.CurrentDataIndex}");
    }
}
```

## Public API Reference

- `void SetData(List<IRecycleViewData> data)`: Clears the list and rebuilds it with the new data.
- `void Refresh()`: Updates the data of currently visible items without a full rebuild.
- `void SetItemPadding(float padding)`: Sets the padding between items in pixels and rebuilds the list.
- `float ItemPadding { get; set; }`: Property to get or set the padding between items.
- `Action<RecycleViewItem> OnItemClicked`: An event that fires when an item's `NotifyItemClicked()` method is called (e.g., from a button's `OnClick` event).

## Configuration Options

### Inspector Settings

- **Item Type Mappings:** Configure which prefabs correspond to which item types.
- **View Buffer:** Number of extra items to instantiate outside the visible area to reduce pop-in.
- **Layout Mode:** Choose between Vertical or Horizontal scrolling.
- **Item Padding:** Set the spacing between items in pixels.

### Runtime Configuration

You can change the padding at runtime:

```csharp
// Change padding through property
myRecycleView.ItemPadding = 15f;

// Or through method
myRecycleView.SetItemPadding(15f);
```
