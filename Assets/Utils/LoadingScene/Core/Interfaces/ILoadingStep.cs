using System;
using System.Threading.Tasks;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Interface định nghĩa một bước loading trong quá trình chuyển scene.
    /// Sử dụng Command Pattern để đóng gói mỗi hành động loading.
    /// </summary>
    public interface ILoadingStep
    {
        /// <summary>
        /// Tên hiển thị của bước loading này
        /// </summary>
        string StepName { get; }
        
        /// <summary>
        /// Mô tả chi tiết của bước loading
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Trọng số của bước này trong tổng tiến độ (0-1)
        /// </summary>
        float Weight { get; }
        
        /// <summary>
        /// Tiến độ hiện tại của bước này (0-1)
        /// </summary>
        float Progress { get; }
        
        /// <summary>
        /// Kiểm tra xem bước này có thể được bỏ qua không
        /// </summary>
        bool CanSkip { get; }
        
        /// <summary>
        /// Sự kiện được kích hoạt khi tiến độ thay đổi
        /// </summary>
        event Action<ILoadingStep> OnProgressChanged;
        
        /// <summary>
        /// Sự kiện được kích hoạt khi bước hoàn thành
        /// </summary>
        event Action<ILoadingStep> OnStepCompleted;
        
        /// <summary>
        /// Sự kiện được kích hoạt khi có lỗi xảy ra
        /// </summary>
        event Action<ILoadingStep, Exception> OnStepError;
        
        /// <summary>
        /// Thực thi bước loading này
        /// </summary>
        /// <returns>Task hoàn thành khi bước loading kết thúc</returns>
        Task ExecuteAsync();
        
        /// <summary>
        /// Hủy bỏ bước loading (nếu có thể)
        /// </summary>
        void Cancel();
        
        /// <summary>
        /// Reset bước loading về trạng thái ban đầu
        /// </summary>
        void Reset();
    }
}