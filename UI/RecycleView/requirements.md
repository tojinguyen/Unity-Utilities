# RecycleView Requirement – Multi-Item Type

## 1. Mục tiêu tổng thể

- Cung cấp một giải pháp UI component trong Unity cho phép hiển thị số lượng lớn phần tử trong ScrollView (hàng trăm, hàng nghìn item) với hiệu năng ổn định.
- Hỗ trợ tái sử dụng item (item recycling) để tránh tạo/destroy liên tục, giảm thiểu GC Alloc và giữ FPS ổn định khi scroll.
- Mở rộng tính năng để trong cùng một ScrollView có thể hiển thị nhiều loại item khác nhau (multi-item type) mà vẫn đảm bảo cơ chế pooling, reusability và performance.

## 2. Chức năng chính

### 2.1. Quản lý dữ liệu

- Thành phần RecycleView phải nhận vào một danh sách dữ liệu tổng (DataList).
- Mỗi phần tử trong danh sách dữ liệu phải xác định được loại item (ItemType) mà nó đại diện.
- RecycleView cần có khả năng đọc thông tin ItemType từ dữ liệu để quyết định sẽ sử dụng prefab nào khi render.

### 2.2. Đăng ký prefab cho từng loại item

- RecycleView cần cung cấp cơ chế cho phép developer đăng ký nhiều loại item.
- Mỗi loại item tương ứng với một prefab UI cụ thể.
- Có thể có một hoặc nhiều loại item trong cùng một RecycleView.
- Hệ thống phải map được ItemType → Prefab.

### 2.3. Cơ chế Pooling theo loại

- Mỗi loại item cần có một object pool riêng.
- Khi một item loại A ra khỏi viewport, nó được trả về pool của loại A, không được trả sang loại B.
- Khi cần hiển thị item loại A ở vị trí mới, RecycleView sẽ lấy prefab từ pool của loại A thay vì tạo mới.

### 2.4. Khởi tạo và hiển thị

- Khi khởi tạo, RecycleView chỉ instantiate số lượng item vừa đủ để phủ kín viewport (cộng thêm buffer).
- Khi scroll, RecycleView:
    - Xác định phần tử mới cần hiển thị.
    - Kiểm tra loại item tương ứng.
    - Lấy một instance từ pool đúng loại.
    - Gán dữ liệu mới vào instance.
- RecycleView không được phép destroy các item khi scroll, chỉ tái sử dụng.

### 2.5. Render dữ liệu

- Mỗi item UI phải có khả năng nhận dữ liệu mới để render nội dung.
- RecycleView gọi hàm BindData (hoặc tương đương) trên item mỗi khi dữ liệu thay đổi.
- Developer phải định nghĩa cách hiển thị dữ liệu cho từng loại item.

## 3. Performance Requirement

- Hệ thống phải đảm bảo khả năng hiển thị ít nhất 1000 phần tử trong danh sách với trải nghiệm scroll mượt mà.
- FPS phải ổn định ở mức 60 FPS trên thiết bị mobile tầm trung.
- Không phát sinh GC Alloc trong quá trình scroll (chỉ phát sinh khi khởi tạo).
- Thời gian khởi tạo ban đầu không vượt quá 200ms cho viewport chứa khoảng 20 item.

## 4. Khả năng mở rộng

- Hỗ trợ cả Scroll theo chiều dọc và Scroll theo chiều ngang.
- Cho phép hiển thị item có kích thước khác nhau (dynamic height/width), không bắt buộc item đồng nhất kích thước.
- Hỗ trợ cả Linear layout (1 cột/1 hàng) và Grid layout (nhiều cột/nhiều hàng).
- Cho phép thêm hoặc xóa loại item mới mà không cần thay đổi logic lõi của RecycleView.

## 5. API Requirement (mô tả hành vi, không code)

- **Đăng ký loại item**: Developer phải có cách đăng ký prefab cho từng loại item.
- **Cập nhật dữ liệu**: Developer có thể gọi SetData(List<Data>) để truyền dữ liệu mới.
- **Refresh/Rebuild**: Cho phép refresh danh sách khi dữ liệu thay đổi.
- **Event callback**: RecycleView phải cung cấp cơ chế callback cho developer, ví dụ: khi item được click hoặc khi item được tạo/reuse.

## 6. Debug & Tooling

- Inspector phải hiển thị danh sách các loại item đã được đăng ký.
- Hiển thị số lượng item đang active và số lượng item trong pool.
- Log cảnh báo khi dữ liệu yêu cầu một loại item chưa được đăng ký.
