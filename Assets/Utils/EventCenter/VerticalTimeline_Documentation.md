# Vertical Timeline UX - Event Visualizer

## 📖 Tổng quan / Overview

Event Visualizer đã được cập nhật với UX timeline dọc hoàn toàn mới, thay thế cho timeline ngang cũ. Thiết kế mới này cung cấp trải nghiệm trực quan và dễ sử dụng hơn để theo dõi events theo thời gian thực.

The Event Visualizer has been updated with a completely new vertical timeline UX, replacing the old horizontal timeline. This new design provides a more intuitive and user-friendly experience for tracking events in real-time.

## ✨ Tính năng mới / New Features

### 🔸 Trục Timeline Dọc / Vertical Timeline Axis
- **Trục thẳng đứng** ở bên trái hiển thị timeline chạy theo thời gian
- **Time markers** với labels hiển thị thời gian chính xác
- **Grid lines** ngang để dễ đọc thời gian

### 🔸 Events Bên Phải Trục / Events on Right Side
- Events được hiển thị **bên phải** trục timeline
- Mỗi event có **màu sắc riêng** theo category
- **Time labels** hiển thị chính xác thời gian event

### 🔸 Collision Detection & Layering
- Events xảy ra **gần nhau** (trong vòng 50ms) được phát hiện tự động
- Tự động **sắp xếp theo layers** ngang để tránh đè lên nhau
- Events trong cùng time group được **căn giữa** theo chiều dọc

### 🔸 Connection Lines / Nét Đứt Kết Nối
- **Nét đứt** kết nối từ mỗi event đến timeline axis
- Màu sắc nét đứt **tương ứng** với màu event
- Hiển thị **thời gian chính xác** tại điểm kết nối

### 🔸 Scroll Dọc / Vertical Scrolling
- **Scroll dọc** để xem lại các events cũ
- Timeline **tự động mở rộng** theo thời gian
- **Smooth scrolling** experience

### 🔸 Watch List & Highlighting
- Events trong **watch list** được highlight đặc biệt
- **Selection** system với highlighting màu vàng
- **Category filtering** vẫn hoạt động bình thường

## 🎮 Cách sử dụng / How to Use

### 1. Mở Event Visualizer
```
Unity Menu > TirexGame > Event Center > Event Visualizer
```

### 2. Bắt đầu Recording
- Click nút **"Record"** trong toolbar
- Events sẽ xuất hiện trên timeline khi được publish

### 3. Navigation
- **Scroll dọc** để xem events cũ/mới
- **Zoom slider** để điều chỉnh tỷ lệ thời gian
- **Click event** để xem chi tiết bên panel phải

### 4. Testing với Demo Script
- Thêm `VerticalTimelineDemo` component vào GameObject
- Script sẽ tự động tạo các loại events khác nhau:
  - Timeline Test Events
  - Collision Test Events (burst)
  - Rapid Fire Events

## 🔧 Technical Implementation

### Core Components
- `DrawVerticalTimelineGrid()`: Vẽ trục timeline dọc và grid
- `CalculateEventLayout()`: Collision detection và layout system
- `DrawEventsWithConnections()`: Vẽ events và connection lines
- `DrawConnectionLine()`: Vẽ nét đứt kết nối
- `DrawVerticalPlayhead()`: Playhead cho replay mode

### Data Structures
```csharp
struct EventLayout
{
    public EventRecord eventRecord;
    public Rect rect;
    public int layer; // 0 = gần timeline nhất
    public Vector2 connectionPoint; // Điểm kết nối trên timeline
}
```

### Collision Detection Logic
- Events trong vòng **50ms** được coi là collision
- Tự động group events theo time proximity
- Layout theo layers với spacing 130px
- Vertical centering cho groups

## 🎯 Use Cases

### 1. Debug Performance Issues
- Xem events clustering để phát hiện performance spikes
- Timeline dọc giúp dễ thấy patterns theo thời gian

### 2. Event Flow Analysis
- Connection lines giúp trace event flow
- Multi-layer layout tránh confusion khi nhiều events

### 3. Real-time Monitoring
- Scroll để theo dõi events mới nhất
- Watch list để highlight events quan trọng

## 🚀 Demo & Testing

### Automatic Demo
```csharp
// Thêm vào GameObject
var demo = gameObject.AddComponent<VerticalTimelineDemo>();
demo.autoStart = true;
```

### Manual Testing
```csharp
// Tạo test events
EventSystem.Publish(new TimelineTestEvent("Test", 1, 0.5f));
EventSystem.Publish(new CollisionTestEvent("UI", Vector3.zero, true));
```

### Context Menu Controls
- Generate Timeline Events
- Generate Collision Burst
- Generate Rapid Fire
- Start/Stop Auto Generation

## 📊 Visual Improvements

### Before (Horizontal Timeline)
- Events xếp theo hàng ngang
- Khó theo dõi timeline dài
- Events đè lên nhau khi collision

### After (Vertical Timeline)
- Timeline dọc trực quan hơn
- Scroll dọc tự nhiên
- Smart collision handling
- Connection lines rõ ràng
- Time precision cao

## 🔮 Future Enhancements

- [ ] Timeline zoom to specific time range
- [ ] Event filtering by time range
- [ ] Export timeline as image
- [ ] Event annotation system
- [ ] Performance metrics overlay
- [ ] Multi-timeline comparison

---

## 📝 Notes

- Hệ thống cũ (`DrawTimeGrid`) vẫn được giữ lại để tương thích
- Có thể dễ dàng switch giữa horizontal/vertical mode nếu cần
- Performance được optimize cho thousands of events
- Compatible với existing EventCapture và EventModels

**Chúc bạn có trải nghiệm tuyệt vời với Vertical Timeline UX mới! 🎉**