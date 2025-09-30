# EventCenter Editor Tool - Hướng dẫn sử dụng

## Tổng quan
EventCenter Editor Tool cung cấp một cửa sổ Timeline dockable để quan sát, lọc, phân tích và xuất luồng Event của hệ thống EventCenter theo thời gian trong Unity Editor.

## Cách mở cửa sổ
- Menu: `TirexGame/Event Center/Event Visualizer`
- Cửa sổ sẽ hiện ra dưới dạng dockable window có thể dock như Console hoặc Inspector

## Giao diện chính

### 1. Toolbar (Thanh công cụ trên cùng)
- **Record**: Bật/tắt ghi lại events (mặc định ON khi vào Play Mode)
- **Pause**: Tạm dừng thu thập events (timeline dừng cập nhật realtime)  
- **Clear**: Xóa toàn bộ buffer hiện tại
- **Search Box**: Tìm kiếm events theo tên (hỗ trợ case-insensitive)
- **Filter fields**: 
  - Cat: Lọc theo Category
  - Src: Lọc theo Source (object name/type)
  - Lst: Lọc theo Listener
- **Time Range**: Slider để lọc theo khoảng thời gian
- **Preset Filters**: 
  - Errors: Hiện events có exception
  - Player: Hiện events category Player
  - UI: Hiện events category UI
  - Clear: Xóa tất cả filters
- **Zoom**: Slider điều chỉnh zoom timeline
- **Fit**: Auto-zoom để fit tất cả events
- **Replay Mode**: Chế độ phát lại timeline
  - Play/Pause: Điều khiển phát lại
  - Reset: Reset playhead về đầu
- **Export**: Xuất dữ liệu JSON hoặc CSV

### 2. Left Panel (Bảng bên trái)
#### Channels List
- Hiển thị danh sách các Category/Channel events
- Có thể ẩn/hiện từng channel bằng checkbox
- Hiển thị số lượng events trong mỗi channel

#### Watch List & Breakpoints
- **Watch List**: Danh sách events quan trọng để highlight
  - Thêm pattern event name để watch
  - Events match sẽ được highlight đỏ trên timeline
- **Breakpoints**: 
  - Pause on match: Tự động pause game khi match pattern
  - Event Name Contains: Pattern để match

### 3. Center Panel (Timeline chính)
- **Timeline Grid**: Lưới thời gian với các mốc giây
- **Event Tracks**: Mỗi category là một track ngang
- **Event Markers**: Hộp màu đại diện cho từng event
  - Click để select và xem details
  - Màu sắc theo category (có thể config)
  - Events trong watch list được highlight đỏ
- **Listener Links**: Khi select event, hiển thị đường nối đến listeners
- **Playhead**: Đường đỏ dọc trong Replay Mode (có thể drag để scrub)

### 4. Right Panel (Chi tiết Event)
Khi click chọn một event, hiển thị:
- **Basic Info**: Tên, category, thời gian realtime/game time
- **Source Info**: Object name, type, hierarchy path
- **Payload**: Nội dung data của event (JSON format)
- **Listeners**: Danh sách các listener đã xử lý event
  - Tên method, target object, thời gian xử lý
  - Exception nếu có lỗi xảy ra

## Hotkeys
- **Ctrl/Cmd + R**: Toggle Record
- **Ctrl/Cmd + P**: Toggle Pause  
- **Ctrl/Cmd + K**: Clear log
- **F**: Zoom to Fit

## Tính năng chính

### Real-time Monitoring
- Khi Play Mode và Record ON, events được hiển thị realtime trên timeline
- Timeline tự động scroll và cập nhật khi có event mới

### Filtering & Search
- Tìm kiếm theo tên event (text search)
- Lọc theo category, source, listener
- Lọc theo khoảng thời gian
- Preset filters cho các trường hợp thường dùng

### Event Flow Visualization  
- Khi select event, hiển thị visual links đến tất cả listeners
- Có thể follow chain events (listener publish event khác)

### Replay & Playback
- Replay Mode: Phát lại timeline đã ghi
- Playhead có thể drag để scrub đến thời điểm bất kỳ
- Play/Pause controls
- Auto-pause khi đến cuối timeline

### Export & Analysis
- **JSON Export**: Full data với metadata (version, session info, tất cả events)
- **CSV Export**: Dạng bảng phẳng dễ phân tích (time, name, category, source, listeners count, duration, exceptions)

### Breakpoints & Debugging
- Set breakpoint pattern: Game tự động pause khi có event match
- Watch list: Highlight events quan trọng
- Exception tracking: Dễ dàng spot events có lỗi

## Cấu hình

### EventVisualizerConfig
File config tại `Assets/Utils/EventCenter/Resources/EventCenterEditorConfig.asset`:
- Buffer size: Số events tối đa trong buffer
- Channel colors: Màu sắc cho từng category
- Default zoom level
- Background capture option

### Tích hợp với EventCenter
Tool tự động hook vào EventCenter runtime thông qua reflection để capture:
- BaseEvent publish (cả regular và immediate)
- Struct event publish  
- Listener dispatch với timing và exception info

## Performance
- Ring buffer với giới hạn cấu hình được (mặc định 10k events)
- UI virtualization: chỉ render events trong viewport
- Sampling/throttling khi events quá dày đặc
- Non-invasive capture: không ảnh hưởng performance runtime

## Troubleshooting

### Events không hiển thị
- Kiểm tra Record button có ON không
- Kiểm tra Pause button có OFF không  
- Đảm bảo EventCenter đang hoạt động trong scene
- Check filters có quá strict không

### Performance issue
- Giảm buffer size trong config
- Clear log thường xuyên
- Tắt background capture khi không cần

### Export không hoạt động
- Đảm bảo có quyền ghi file ở thư mục chọn
- Check log Console có lỗi gì không

## Mở rộng

### Custom Category Resolver
Implement `IEventMetadataResolver` để custom cách phân loại events

### Custom Payload Formatter  
Implement `IPayloadFormatter` để custom cách hiển thị payload

### Custom Colors
Implement `IColorProvider` để custom logic màu sắc

---

*Tool này tuân theo requirements trong EventCenter_EditorTool_Requirements.md và được thiết kế để tối ưu workflow debug và phân tích event system.*