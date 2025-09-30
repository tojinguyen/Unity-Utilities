## EventCenter Editor Tool — Yêu cầu chức năng

### Mục tiêu
- Cung cấp công cụ trong Unity Editor (dockable) để quan sát, lọc, phân tích và xuất luồng Event (publish/subscribe) của hệ thống `EventCenter` theo thời gian.
- Hỗ trợ realtime khi Play Mode, ghi log sự kiện, xem chi tiết, highlight chuỗi lan truyền (event → listeners), và xuất dữ liệu phục vụ debug/phân tích.

### Phạm vi
- Chỉ tập trung vào quan sát/ghi/chạy lại event; không thay đổi core `EventCenter` runtime ngoài việc thêm hook/bridge nhẹ cần thiết (non-invasive) để thu thập dữ liệu.

### Định nghĩa thuật ngữ
- Event: Thông điệp publish qua `EventCenter` (có tên, payload, nguồn phát).
- Source: Đối tượng/ hệ thống phát event (GameObject/Component/ScriptableObject/Service).
- Listener: Bên đăng ký nhận event và xử lý.
- Category/Channel: Nhóm phân loại event (Gameplay, UI, Network, Audio, ...).

## Tính năng cốt lõi

### Timeline Viewer
- Hiển thị tất cả event theo trục thời gian (trục X là thời gian, trục Y là các track theo Category/Channel).
- Zoom in/out timeline bằng scroll hoặc thanh zoom; pan bằng drag.
- Mỗi event là marker/box có label (tên event); tooltip hiện thời điểm, thời lượng handle, số listener.
- Hỗ trợ scale theo: Time.realtimeSinceStartup (mặc định), Time.time (optional).

### Event Stream per Category/Channel
- Mỗi Category/Channel là một track riêng nằm theo chiều dọc.
- Cho phép ẩn/hiện track; reorder track bằng drag-and-drop.
- Màu sắc track và màu marker theo cấu hình (per Category/Channel, per EventType).

### Event Details Panel
- Khi click event marker: panel bên phải hiển thị chi tiết:
  - Tên event (EventType / key).
  - Payload (dạng object → stringify an toàn; hỗ trợ Json preview nếu có thể).
  - Source (reference/identifiers: tên GameObject, path trong hierarchy, hoặc tên script/service; kèm InstanceID khi khả dụng).
  - Listeners: danh sách listener (tên method, component, object reference khi có thể), thứ tự gọi, thời gian xử lý, exception (nếu có).

### Search & Filter
- Bộ lọc theo: loại event, thời gian (range), nguồn phát (source), listener, Category/Channel.
- Ô search theo tên event (regex optional, case-insensitive toggle).
- Preset filters: Quick buttons (e.g., Only Errors, Only Gameplay, Only Selected GameObject).

### Pause & Record Mode
- Record: ghi event khi Play Mode đang chạy (default ON khi mở cửa sổ lần đầu có thể cấu hình).
- Pause: tạm dừng thu thập event (không ghi mới; timeline dừng cập nhật realtime).
- Nút Clear log: xóa toàn bộ buffer hiện tại.
- Export log: JSON (bắt buộc), CSV (tùy chọn) với metadata.

### Highlight Event Flow
- Khi chọn 1 event: highlight các listener đã được trigger từ event đó, vẽ line/arrow trực quan từ event → listener trên timeline/graph khu vực phụ.
- Cho phép follow-through: click listener node để nhảy đến các event tiếp theo mà listener đó publish (nếu có) → chain visualization.

## Tính năng nâng cao

### Real-time Mode
- Khi Play Mode, event hiển thị realtime (append vào timeline ngay khi nhận hook).
- Chỉ ghi/hiển thị khi cửa sổ mở hoặc có cấu hình background capture (optional, mặc định off để tiết kiệm chi phí).

### Playback/Replay
- Cho phép replay sequence event đã ghi: play/pause/seek theo timeline.
- Tùy chọn: simulate-only (chỉ phát hình ảnh timeline), hoặc instrumentation replay (gọi lại listeners dưới sandbox OFF by default — cẩn trọng, chỉ phục vụ debug đặc biệt).

### Profiler Integration
- Hiển thị performance cost: thời lượng handle mỗi listener, tổng số listener, GC alloc (nếu đo được).
- Tích hợp marker với Unity Profiler Timeline bằng `ProfilerMarker` (optional).

### Breakpoints/Watch Events
- Đặt breakpoint theo điều kiện: khi event X (tên/regex) xuất hiện → pause game (EditorApplication.isPaused = true).
- Watch list: danh sách event quan trọng để ghim riêng (panel nhỏ hoặc track riêng).

### Collapse/Expand Hierarchy
- Gộp nhóm các event cùng loại trên track để giảm noise; hiển thị count + cluster range; expand để xem chi tiết.

### Export Visualization
- Export timeline thành ảnh PNG hoặc PDF (rasterize vùng timeline + legend + filters đang áp dụng).
- Export log: JSON (đầy đủ), CSV (bảng phẳng: time, event, category, source, listenersCount, duration, ...).

## Khả năng sử dụng trong Editor

### Dockable Window
- Dưới dạng `EditorWindow`, dockable như `Console`/`Inspector`.
- Tên cửa sổ: "EventCenter Timeline". Menu: `Tools/EventCenter/Timeline`.

### Customizable Colors
- Bảng cấu hình màu theo Category/Channel và theo EventType (override theo EventType > Channel > default).
- Lưu cấu hình dạng `ScriptableObject` tại `Assets/Utils/EventCenter/Resources/EventCenterEditorConfig.asset` (đường dẫn gợi ý).

### Shortcut & Quick Actions
- Nút Clear, Record toggle, Pause toggle ở toolbar.
- Hotkeys (configurable):
  - Toggle Record: Ctrl/Cmd+R
  - Toggle Pause: Ctrl/Cmd+P
  - Clear: Ctrl/Cmd+K
  - Zoom to Fit: F

## Kiến trúc & Tích hợp

### Hook thu thập dữ liệu (Instrumentation)
- Tại core `EventCenter` (hoặc adapter), thêm các callback/hook:
  - OnPublish(eventName, payload, source)
  - OnDispatchStart(eventName, listener, timestamp)
  - OnDispatchEnd(eventName, listener, durationMs, exception?)
- Bridge từ Runtime sang Editor:
  - Dùng `UnityEditor.EditorApplication.playModeStateChanged` để quản lý lifecycle buffer.
  - Sử dụng `PlayerLoop`/`EditorApplication.update` để push events vào buffer thread-safe.
  - Payload: chỉ serialize an toàn (try-catch), fallback sang `ToString()` nếu không serializable.

### Data Model (Editor-side)
- EventRecord
  - id (GUID)
  - timeRealtime (double)
  - gameTime (float, optional)
  - name (string)
  - category (enum/string)
  - payloadPreview (string/JSON snippet)
  - sourceInfo { objectName, typeName, instanceId }
  - listeners: ListenerRecord[]
- ListenerRecord
  - name (method signature)
  - targetInfo { objectName, typeName, instanceId }
  - durationMs (double)
  - exception (string, optional)

### Buffer & Hiệu năng
- Sử dụng ring-buffer có giới hạn (configurable, ví dụ 10k events) để tránh memory bloat.
- Chế độ sampling/throttling khi realtime dày đặc.
- Vẽ UI batching: chỉ render các item trong viewport; sử dụng culling/virtualization.
- Không tạo GC alloc trong loop vẽ; dùng static styles, pooled lists.

### UI Layout (gợi ý)
- Toolbar: Record, Pause, Clear, Export, Search, Filter presets, Zoom controls, Settings.
- Left Panel: Track list (channels), watch list.
- Center: Timeline (canvas) với marker và grid thời gian; zoom/pan; selection; context menu.
- Right Panel: Event Details; Profiler stats; Breakpoints.
- Status bar: tổng số event, FPS (editor UI), capture state, buffer usage.

## Xuất/Nhập

### Export Log
- JSON format (đề xuất):
  - version, sessionId, unityVersion, projectName, captureStartTimeUtc
  - records: EventRecord[] (đầy đủ fields)
- CSV format (tuỳ chọn): Các cột phẳng: time, name, category, source, listenersCount, durationAvg, exceptionsCount, ...

### Export Visualization
- PNG: xuất khu vực timeline hiện tại (viewport) + legend + filters
- PDF (optional): 1 trang A4/A3, scale-to-fit timeline + summary table

## Playback/Replay (chi tiết)
- Chế độ visualize-only: thay đổi con trỏ thời gian (playhead) để scrub qua buffer, highlight events đến thời điểm đó.
- Chế độ instrumentation replay (optional, disabled by default): gọi lại listeners bằng mock publish theo timestamp, cảnh báo rủi ro side-effect.

## Breakpoints & Watch
- Breakpoints theo tên event hoặc điều kiện (predicate payload nếu có thể bằng string filter).
- Khi match → `EditorApplication.isPaused = true` và highlight event.
- Watch list: hiển thị track riêng hoặc pin ở top; không bị clear bởi Clear Log nếu chọn "Preserve Watches".

## Cấu hình & Lưu trữ
- `EventCenterEditorConfig` (ScriptableObject):
  - colors: per-channel, per-event overrides
  - bufferSize, backgroundCapture, defaultZoom, showTooltips
  - hotkeys mapping
  - export default paths
- Lưu tại `Resources` để có `Resources.Load` trong Editor (editor-only usage).

## API mở rộng (Editor)
- `IEventMetadataResolver`: cho phép resolve Category/Channel từ eventName.
- `IPayloadFormatter`: tuỳ biến stringify/pretty-print payload theo loại.
- `IColorProvider`: tuỳ biến màu sắc theo logic riêng.

## Tiêu chí chấp nhận (Acceptance Criteria)
- Mở `Tools/EventCenter/Timeline` hiển thị cửa sổ dockable.
- Khi Play Mode và Record ON, timeline cập nhật realtime các event publish từ `EventCenter`.
- Click vào marker hiển thị details: tên, payload preview, source, danh sách listeners, thời lượng.
- Tìm kiếm theo tên event hoạt động; filter theo channel, time range, source, listener hoạt động.
- Toggle Pause dừng cập nhật; Clear xoá buffer; Export JSON tạo file hợp lệ.
- Highlight flow từ event đến tất cả listener với đường nối; follow-through được chain.
- Màu sắc theo cấu hình và có thể đổi; shortcut hoạt động.

## Ràng buộc/Kỹ thuật
- Editor-only code phải nằm trong thư mục `Editor` hoặc dùng `#if UNITY_EDITOR`.
- Không làm chậm Play Mode quá 2ms/frame khi ~200 events/giây (mục tiêu; có thể điều chỉnh thông qua sampling).
- Không throw exception trong UI loop; xử lý lỗi serialize payload an toàn.

## Lộ trình triển khai (đề xuất)
1) MVP: Record/Pause/Clear, Timeline basic + Details, Search/Filter, Export JSON.
2) Realtime tối ưu + Highlight flow + Colors config.
3) Profiler stats + Breakpoints/Watch + CSV export.
4) Replay visualize-only + Export PNG/PDF.
5) Extensibility APIs + background capture option.

## Ghi chú tích hợp với EventCenter hiện có
- Cần điểm móc từ `EventCenter` để bắn thông tin publish/dispatch (không đổi API public hiện tại nếu có thể).
- Nếu `EventCenter` chưa có categories, cho phép map tên event → category qua `IEventMetadataResolver` hoặc bảng cấu hình.


