# 📏 Horizontal Scrolling - Multi-Layer Timeline Support

## 🎯 Vấn đề (Problem)

### ❌ **Trước khi có horizontal scrolling:**
- Events xếp nhiều layers có thể tràn ra ngoài vùng xem
- Không thể see events ở layers xa
- Timeline bị cut off khi có quá nhiều collision events
- User không thể access events ở layers ngoài

### ✅ **Sau khi có horizontal scrolling:**
- **Auto-detect layers** và adjust width accordingly
- **Horizontal scrolling** khi cần thiết
- **Layer counter** trong toolbar cho feedback
- **Scroll hints** khi có nhiều layers

## 🛠️ Technical Implementation

### **1. Dynamic Width Calculation**

```csharp
// Pre-calculate event layouts to determine required width
var eventLayouts = CalculateEventLayout(eventsList, tempRect, minT, maxT);

// Calculate required width based on maximum layer
int maxLayer = eventLayouts.Any() ? eventLayouts.Max(l => l.layer) : 0;
const float timelineAxisX = 60f;
const float layerOffset = 135f;
const float eventWidth = 120f;
const float padding = 50f;

float requiredWidth = timelineAxisX + 20f + (maxLayer + 1) * layerOffset + eventWidth + padding;
float actualWidth = Math.Max(rect.width, requiredWidth);
```

### **2. Dual-Direction ScrollView**

```csharp
// Start scroll view with both horizontal and vertical scrolling
_timelineScroll = GUILayout.BeginScrollView(_timelineScroll, 
    GUILayout.ExpandHeight(true), 
    GUILayout.ExpandWidth(true));

var timelineRect = GUILayoutUtility.GetRect(actualWidth, totalTimelineHeight);
```

### **3. Layer Information Display**

```csharp
// Layer information for horizontal scrolling
int maxLayer = tempLayouts.Any() ? tempLayouts.Max(l => l.layer) + 1 : 0;

GUILayout.Label($"Layers: {maxLayer}", GUILayout.Width(60));
if (maxLayer > 3) // Show scroll hint when many layers
{
    GUILayout.Label("→ Scroll →", EditorStyles.miniLabel, GUILayout.Width(50));
}
```

## 📐 Width Calculation Formula

### **Components:**
- **Timeline Axis**: 60px from left
- **Initial Offset**: 20px from axis to first layer
- **Layer Spacing**: 135px between layers
- **Event Width**: 120px for each event box
- **Padding**: 50px extra space on right

### **Formula:**
```
requiredWidth = 60 + 20 + (maxLayer + 1) × 135 + 120 + 50
```

### **Example Calculations:**

| Layers | Calculation | Total Width |
|--------|-------------|-------------|
| **1 layer** | 60 + 20 + (1 × 135) + 120 + 50 | **385px** |
| **3 layers** | 60 + 20 + (3 × 135) + 120 + 50 | **655px** |
| **5 layers** | 60 + 20 + (5 × 135) + 120 + 50 | **925px** |
| **10 layers** | 60 + 20 + (10 × 135) + 120 + 50 | **1600px** |

## 🎨 Visual Layout

### **Timeline Layout with Layers:**
```
┌─Timeline Axis─┬─Layer 0─┬─Layer 1─┬─Layer 2─┬─Layer 3─┐
│   12.34s ●    │ [Event1]│         │ [Event3]│         │
│   12.35s ●    │         │ [Event2]│         │ [Event4]│
│   12.36s ●    │ [Event5]│         │         │ [Event6]│
└───────────────┴─────────┴─────────┴─────────┴─────────┘
                 ←──────── Horizontal Scroll ────────→
```

### **Scrolling Behavior:**
- **Auto-fit**: Nếu events fit trong view → no scroll
- **Horizontal scroll**: Khi events tràn ra → scroll bar xuất hiện
- **Vertical scroll**: Luôn có cho timeline dọc
- **Combined**: Both directions scroll independently

## 🧪 Testing Features

### **New Test Function:**
```csharp
[ContextMenu("Test Horizontal Scrolling")]
private void MenuTestHorizontalScrolling()
{
    // Generates 15 events at same time
    // Forces multiple layers (collision detection)
    // Tests horizontal scrolling capability
}
```

### **GUI Button:**
- **📏 Test Horizontal Scroll**: Generate nhiều collision events
- **Layer Counter**: Shows số layers hiện tại
- **Scroll Hint**: "→ Scroll →" khi cần scroll

### **Debug Information in Toolbar:**
```
Color: 5/20 | Layers: 8 | → Scroll →
```

## 🎯 User Experience

### **Automatic Behavior:**
- ✅ **No config needed** - auto-detect layers
- ✅ **Smart width calculation** - chỉ scroll khi cần
- ✅ **Visual feedback** - layer counter + hints
- ✅ **Smooth scrolling** - both directions work independently

### **Edge Cases Handled:**
- ✅ **No events**: No scrolling needed
- ✅ **Few events**: Auto-fit without scroll
- ✅ **Many layers**: Horizontal scroll appears
- ✅ **Long timeline**: Vertical scroll still works
- ✅ **Combination**: Both scrolls work together

### **Visual Cues:**
- **Layer Counter**: `Layers: 5` shows current complexity
- **Scroll Hint**: `→ Scroll →` appears when layers > 3
- **Scroll Bars**: Auto-appear when content overflows
- **Dynamic Width**: Timeline expands as needed

## 🚀 Usage Instructions

### **To Test Horizontal Scrolling:**
1. **Open Event Visualizer**
2. **Click Record**
3. **Use "📏 Test Horizontal Scroll"** button
4. **Watch layer counter** in toolbar
5. **Scroll horizontally** to see all events

### **Expected Behavior:**
```
With few events: No horizontal scroll
With collision events: Horizontal scroll appears
Layer counter shows: "Layers: 8"
Scroll hint shows: "→ Scroll →"
```

### **Manual Testing:**
- Generate rapid fire events (collision)
- Use mixed burst generation
- Check layer counter increases
- Verify horizontal scroll works

---

**Result**: Timeline giờ đây support horizontal scrolling cho multi-layer events! 📏✨