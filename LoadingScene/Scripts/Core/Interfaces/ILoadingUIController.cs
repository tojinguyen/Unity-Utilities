using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Interface cho controller quản lý UI loading.
    /// Sử dụng Strategy Pattern để cho phép tùy chỉnh UI.
    /// </summary>
    public interface ILoadingUIController
    {
        /// <summary>
        /// GameObject chứa UI loading
        /// </summary>
        GameObject UIGameObject { get; }
        
        /// <summary>
        /// Kiểm tra xem UI có đang hiển thị không
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// Hiển thị UI loading
        /// </summary>
        void ShowUI();
        
        /// <summary>
        /// Ẩn UI loading
        /// </summary>
        void HideUI();
        
        /// <summary>
        /// Cập nhật tiến độ loading trên UI
        /// </summary>
        /// <param name="progressData">Dữ liệu tiến độ</param>
        void UpdateProgress(LoadingProgressData progressData);
        
        /// <summary>
        /// Cập nhật text hiển thị bước hiện tại
        /// </summary>
        /// <param name="stepName">Tên bước</param>
        /// <param name="description">Mô tả bước</param>
        void UpdateStepText(string stepName, string description);
        
        /// <summary>
        /// Cập nhật progress bar
        /// </summary>
        /// <param name="progress">Tiến độ (0-1)</param>
        void UpdateProgressBar(float progress);
        
        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        /// <param name="errorMessage">Thông báo lỗi</param>
        void ShowError(string errorMessage);
        
        /// <summary>
        /// Ẩn thông báo lỗi
        /// </summary>
        void HideError();
        
        /// <summary>
        /// Thiết lập có cho phép người dùng hủy loading không
        /// </summary>
        /// <param name="canCancel">Có thể hủy không</param>
        void SetCancelable(bool canCancel);
        
        /// <summary>
        /// Sự kiện được kích hoạt khi người dùng nhấn nút hủy
        /// </summary>
        event System.Action OnCancelRequested;
        
        /// <summary>
        /// Cleanup resources khi UI bị destroy
        /// </summary>
        void Cleanup();
    }
}