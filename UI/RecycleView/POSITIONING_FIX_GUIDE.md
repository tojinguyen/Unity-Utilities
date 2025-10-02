# RecycleView Positioning Fix Guide

## Vấn đề hiện tại
Các item có height khác nhau bị lệch vị trí khi hiển thị trong RecycleView.

## Các thay đổi đã được thực hiện

### 1. Cập nhật logic định vị trong RecycleView.cs
- Thiết lập anchor và pivot settings nhất quán cho tất cả items
- Đảm bảo content có anchor settings đúng
- Thêm debug logs để kiểm tra vị trí được tính toán

### 2. Cài đặt cần kiểm tra trong Unity Inspector

#### A. Content RectTransform (ScrollRect Content)
- **Anchor Min**: (0, 1) - top-left
- **Anchor Max**: (1, 1) - top-right  
- **Pivot**: (0.5, 1) - center-top

#### B. Item Prefabs RectTransform
Đảm bảo tất cả item prefabs có cài đặt sau:
- **Anchor Min**: (0, 1) - top-left
- **Anchor Max**: (1, 1) - top-right
- **Pivot**: (0.5, 1) - center-top
- **Pos X**: 0
- **Pos Y**: 0

#### C. RecycleView Component Settings
- Kiểm tra `Default Item Height` đã được set đúng
- Đảm bảo mapping giữa item types và prefabs đúng
- Set `Layout Mode` = Vertical

## Cách kiểm tra và sửa lỗi

### 1. Kiểm tra Console Logs
Sau khi chạy, kiểm tra Unity Console để xem debug logs:
```
RecycleView: Item 0 - Type: 0, Height: 100, Position: 0
RecycleView: Item 1 - Type: 1, Height: 200, Position: 100
```

### 2. Kiểm tra Prefab Settings
- Mở từng item prefab
- Kiểm tra RectTransform settings như trên
- Đảm bảo sizeDelta.y = height mong muốn

### 3. Test với dữ liệu đơn giản
Tạo test data với chỉ 2-3 items để dễ debug:

```csharp
// In RecycleViewExampleController.cs
private void CreateTestData()
{
    exampleDataList = new List<IRecycleViewData>();
    
    // Item 1: Text (height 100)
    exampleDataList.Add(new TextMessageData { Message = "Test item 1" });
    
    // Item 2: Image (height 200) 
    exampleDataList.Add(new ImageMessageData { Image = sampleSprite, Caption = "Test item 2" });
    
    recycleView.SetData(exampleDataList);
}
```

## Dấu hiệu cho thấy fix đã hoạt động
- Các items được căn chỉnh đúng từ top xuống
- Không có khoảng trống hoặc overlap giữa các items
- Item thứ 2 nằm chính xác ở vị trí y = -100 (height của item đầu)

## Nếu vẫn còn vấn đề
1. Kiểm tra ScrollRect component có movement type = Clamped
2. Đảm bảo Viewport có mask component
3. Kiểm tra không có Layout Group components trên content
4. Xác nhận prefab sizes được detect đúng trong Awake()