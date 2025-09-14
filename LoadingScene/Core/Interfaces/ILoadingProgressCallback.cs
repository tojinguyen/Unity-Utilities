using System;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Data class chứa thông tin về tiến độ loading
    /// </summary>
    [Serializable]
    public class LoadingProgressData
    {
        public string CurrentStepName { get; set; }
        public string CurrentStepDescription { get; set; }
        public float CurrentStepProgress { get; set; }
        public float TotalProgress { get; set; }
        public int CurrentStepIndex { get; set; }
        public int TotalSteps { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedRemainingTime { get; set; }

        public LoadingProgressData()
        {
            Reset();
        }

        public void Reset()
        {
            CurrentStepName = "";
            CurrentStepDescription = "";
            CurrentStepProgress = 0f;
            TotalProgress = 0f;
            CurrentStepIndex = 0;
            TotalSteps = 0;
            IsCompleted = false;
            HasError = false;
            ErrorMessage = "";
            ElapsedTime = TimeSpan.Zero;
            EstimatedRemainingTime = TimeSpan.Zero;
        }

        public override string ToString()
        {
            return $"Loading: {CurrentStepName} ({TotalProgress:P}) - {CurrentStepDescription}";
        }
    }

    /// <summary>
    /// Interface cho callback nhận thông báo về tiến độ loading.
    /// Sử dụng Observer Pattern.
    /// </summary>
    public interface ILoadingProgressCallback
    {
        /// <summary>
        /// Được gọi khi tiến độ loading thay đổi
        /// </summary>
        /// <param name="progressData">Dữ liệu tiến độ hiện tại</param>
        void OnProgressUpdated(LoadingProgressData progressData);
        
        /// <summary>
        /// Được gọi khi bắt đầu một bước loading mới
        /// </summary>
        /// <param name="step">Bước loading mới</param>
        /// <param name="progressData">Dữ liệu tiến độ</param>
        void OnStepStarted(ILoadingStep step, LoadingProgressData progressData);
        
        /// <summary>
        /// Được gọi khi một bước loading hoàn thành
        /// </summary>
        /// <param name="step">Bước loading đã hoàn thành</param>
        /// <param name="progressData">Dữ liệu tiến độ</param>
        void OnStepCompleted(ILoadingStep step, LoadingProgressData progressData);
        
        /// <summary>
        /// Được gọi khi toàn bộ quá trình loading hoàn thành
        /// </summary>
        /// <param name="progressData">Dữ liệu tiến độ cuối cùng</param>
        void OnLoadingCompleted(LoadingProgressData progressData);
        
        /// <summary>
        /// Được gọi khi có lỗi xảy ra trong quá trình loading
        /// </summary>
        /// <param name="step">Bước loading gặp lỗi</param>
        /// <param name="exception">Exception xảy ra</param>
        /// <param name="progressData">Dữ liệu tiến độ hiện tại</param>
        void OnLoadingError(ILoadingStep step, Exception exception, LoadingProgressData progressData);
    }
}