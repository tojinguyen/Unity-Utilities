using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Class chính quản lý toàn bộ hệ thống loading scene.
    /// Sử dụng Singleton Pattern để đảm bảo chỉ có một instance duy nhất.
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        #region Singleton Implementation
        
        private static LoadingManager _instance;
        private static readonly object _lock = new object();
        
        public static LoadingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<LoadingManager>();
                            if (_instance == null)
                            {
                                GameObject go = new GameObject("LoadingManager");
                                _instance = go.AddComponent<LoadingManager>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        
        #endregion

        #region Events
        
        /// <summary>
        /// Sự kiện được kích hoạt khi bắt đầu loading
        /// </summary>
        public static event Action<LoadingProgressData> OnLoadingStarted;
        
        /// <summary>
        /// Sự kiện được kích hoạt khi tiến độ loading thay đổi
        /// </summary>
        public static event Action<LoadingProgressData> OnLoadingProgressChanged;
        
        /// <summary>
        /// Sự kiện được kích hoạt khi loading hoàn thành
        /// </summary>
        public static event Action<LoadingProgressData> OnLoadingCompleted;
        
        /// <summary>
        /// Sự kiện được kích hoạt khi có lỗi xảy ra
        /// </summary>
        public static event Action<ILoadingStep, Exception, LoadingProgressData> OnLoadingError;
        
        #endregion

        #region Private Fields
        
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private float _minimumLoadingTime = 1f;
        [SerializeField] private int _maxConcurrentSteps = 1;
        
        private readonly List<ILoadingStep> _loadingSteps = new List<ILoadingStep>();
        private readonly List<ILoadingProgressCallback> _progressCallbacks = new List<ILoadingProgressCallback>();
        private readonly LoadingProgressData _progressData = new LoadingProgressData();
        
        private ILoadingUIController _uiController;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isLoading;
        private System.Diagnostics.Stopwatch _loadingStopwatch;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Kiểm tra xem có đang loading không
        /// </summary>
        public bool IsLoading => _isLoading;
        
        /// <summary>
        /// Dữ liệu tiến độ hiện tại
        /// </summary>
        public LoadingProgressData CurrentProgress => _progressData;
        
        /// <summary>
        /// Controller UI hiện tại
        /// </summary>
        public ILoadingUIController UIController => _uiController;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _loadingSteps.Clear();
                _progressCallbacks.Clear();
                
                if (_uiController != null)
                {
                    _uiController.Cleanup();
                    _uiController = null;
                }
            }
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Thiết lập UI Controller
        /// </summary>
        /// <param name="uiController">UI Controller mới</param>
        public void SetUIController(ILoadingUIController uiController)
        {
            if (_uiController != null)
            {
                _uiController.OnCancelRequested -= OnCancelRequested;
                _uiController.Cleanup();
            }
            
            _uiController = uiController;
            
            if (_uiController != null)
            {
                _uiController.OnCancelRequested += OnCancelRequested;
            }
            
            DebugLog($"UI Controller set: {uiController?.GetType().Name ?? "null"}");
        }
        
        /// <summary>
        /// Thêm callback nhận thông báo tiến độ
        /// </summary>
        /// <param name="callback">Callback object</param>
        public void AddProgressCallback(ILoadingProgressCallback callback)
        {
            if (callback != null && !_progressCallbacks.Contains(callback))
            {
                _progressCallbacks.Add(callback);
                DebugLog($"Progress callback added: {callback.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Xóa callback
        /// </summary>
        /// <param name="callback">Callback object</param>
        public void RemoveProgressCallback(ILoadingProgressCallback callback)
        {
            if (callback != null && _progressCallbacks.Contains(callback))
            {
                _progressCallbacks.Remove(callback);
                DebugLog($"Progress callback removed: {callback.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Bắt đầu quá trình loading với danh sách các steps
        /// </summary>
        /// <param name="steps">Danh sách các bước loading</param>
        /// <param name="showUI">Có hiển thị UI không</param>
        /// <returns>Task hoàn thành khi loading xong</returns>
        public async Task StartLoadingAsync(IEnumerable<ILoadingStep> steps, bool showUI = true)
        {
            if (_isLoading)
            {
                DebugLogWarning("Loading is already in progress!");
                return;
            }
            
            await StartLoadingInternal(steps.ToList(), showUI);
        }
        
        /// <summary>
        /// Bắt đầu quá trình loading với một step duy nhất
        /// </summary>
        /// <param name="step">Bước loading</param>
        /// <param name="showUI">Có hiển thị UI không</param>
        /// <returns>Task hoàn thành khi loading xong</returns>
        public async Task StartLoadingAsync(ILoadingStep step, bool showUI = true)
        {
            await StartLoadingAsync(new[] { step }, showUI);
        }
        
        /// <summary>
        /// Hủy quá trình loading hiện tại
        /// </summary>
        public void CancelLoading()
        {
            if (_isLoading && _cancellationTokenSource != null)
            {
                DebugLog("Cancelling loading...");
                _cancellationTokenSource.Cancel();
            }
        }
        
        #endregion

        #region Private Methods
        
        private void Initialize()
        {
            _loadingStopwatch = new System.Diagnostics.Stopwatch();
            DebugLog("LoadingManager initialized");
        }
        
        private async Task StartLoadingInternal(List<ILoadingStep> steps, bool showUI)
        {
            if (steps == null || steps.Count == 0)
            {
                DebugLogWarning("No loading steps provided!");
                return;
            }
            
            _isLoading = true;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            _loadingSteps.Clear();
            _loadingSteps.AddRange(steps);
            
            // Reset progress data
            _progressData.Reset();
            _progressData.TotalSteps = _loadingSteps.Count;
            
            _loadingStopwatch.Restart();
            
            try
            {
                // Hiển thị UI nếu cần
                if (showUI && _uiController != null)
                {
                    _uiController.ShowUI();
                    _uiController.SetCancelable(true);
                }
                
                // Thông báo bắt đầu loading
                NotifyLoadingStarted();
                
                // Thực thi các steps
                await ExecuteStepsAsync(_cancellationTokenSource.Token);
                
                // Đảm bảo thời gian loading tối thiểu
                await EnsureMinimumLoadingTime();
                
                // Hoàn thành loading
                CompleteLoading();
            }
            catch (OperationCanceledException)
            {
                DebugLog("Loading was cancelled");
                HandleLoadingCancellation();
            }
            catch (Exception ex)
            {
                DebugLogError($"Loading failed: {ex.Message}");
                HandleLoadingError(null, ex);
            }
            finally
            {
                _isLoading = false;
                _loadingStopwatch.Stop();
                
                // Ẩn UI
                if (_uiController != null)
                {
                    _uiController.HideUI();
                }
            }
        }
        
        private async Task ExecuteStepsAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < _loadingSteps.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var step = _loadingSteps[i];
                _progressData.CurrentStepIndex = i + 1;
                _progressData.CurrentStepName = step.StepName;
                _progressData.CurrentStepDescription = step.Description;
                
                DebugLog($"Starting step {i + 1}/{_loadingSteps.Count}: {step.StepName}");
                
                // Thông báo bắt đầu step
                NotifyStepStarted(step);
                
                // Subscribe to step progress events
                step.OnProgressChanged += OnStepProgressChanged;
                step.OnStepCompleted += OnStepCompleted;
                step.OnStepError += OnStepError;
                
                try
                {
                    // Thực thi step
                    await step.ExecuteAsync();
                    
                    // Cập nhật tiến độ tổng thể
                    UpdateTotalProgress();
                    
                    DebugLog($"Completed step: {step.StepName}");
                    NotifyStepCompleted(step);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Step failed: {step.StepName} - {ex.Message}");
                    throw;
                }
                finally
                {
                    // Unsubscribe from step events
                    step.OnProgressChanged -= OnStepProgressChanged;
                    step.OnStepCompleted -= OnStepCompleted;
                    step.OnStepError -= OnStepError;
                }
            }
        }
        
        private async Task EnsureMinimumLoadingTime()
        {
            var elapsedTime = _loadingStopwatch.Elapsed;
            var minimumTime = TimeSpan.FromSeconds(_minimumLoadingTime);
            
            if (elapsedTime < minimumTime)
            {
                var remainingTime = minimumTime - elapsedTime;
                DebugLog($"Waiting additional {remainingTime.TotalSeconds:F1}s to meet minimum loading time");
                await Task.Delay(remainingTime);
            }
        }
        
        private void UpdateTotalProgress()
        {
            if (_loadingSteps.Count == 0) return;
            
            float totalWeight = _loadingSteps.Sum(s => s.Weight);
            float completedWeight = 0f;
            
            for (int i = 0; i < _progressData.CurrentStepIndex - 1; i++)
            {
                completedWeight += _loadingSteps[i].Weight;
            }
            
            // Thêm tiến độ của step hiện tại
            if (_progressData.CurrentStepIndex > 0 && _progressData.CurrentStepIndex <= _loadingSteps.Count)
            {
                var currentStep = _loadingSteps[_progressData.CurrentStepIndex - 1];
                completedWeight += currentStep.Weight * currentStep.Progress;
            }
            
            _progressData.TotalProgress = totalWeight > 0 ? completedWeight / totalWeight : 0f;
            _progressData.ElapsedTime = _loadingStopwatch.Elapsed;
            
            // Tính toán thời gian ước tính còn lại
            if (_progressData.TotalProgress > 0f)
            {
                var estimatedTotalTime = _progressData.ElapsedTime.TotalSeconds / _progressData.TotalProgress;
                var estimatedRemainingSeconds = estimatedTotalTime - _progressData.ElapsedTime.TotalSeconds;
                _progressData.EstimatedRemainingTime = TimeSpan.FromSeconds(Math.Max(0, estimatedRemainingSeconds));
            }
        }
        
        private void CompleteLoading()
        {
            _progressData.TotalProgress = 1f;
            _progressData.IsCompleted = true;
            _progressData.ElapsedTime = _loadingStopwatch.Elapsed;
            _progressData.EstimatedRemainingTime = TimeSpan.Zero;
            
            DebugLog($"Loading completed in {_progressData.ElapsedTime.TotalSeconds:F1}s");
            NotifyLoadingCompleted();
        }
        
        private void HandleLoadingCancellation()
        {
            _progressData.HasError = true;
            _progressData.ErrorMessage = "Loading was cancelled by user";
            
            if (_uiController != null)
            {
                _uiController.ShowError(_progressData.ErrorMessage);
            }
        }
        
        private void HandleLoadingError(ILoadingStep step, Exception exception)
        {
            _progressData.HasError = true;
            _progressData.ErrorMessage = exception.Message;
            
            if (_uiController != null)
            {
                _uiController.ShowError(_progressData.ErrorMessage);
            }
            
            NotifyLoadingError(step, exception);
        }
        
        #region Event Handlers
        
        private void OnStepProgressChanged(ILoadingStep step)
        {
            _progressData.CurrentStepProgress = step.Progress;
            UpdateTotalProgress();
            NotifyProgressChanged();
        }
        
        private void OnStepCompleted(ILoadingStep step)
        {
            UpdateTotalProgress();
            NotifyProgressChanged();
        }
        
        private void OnStepError(ILoadingStep step, Exception exception)
        {
            HandleLoadingError(step, exception);
        }
        
        private void OnCancelRequested()
        {
            CancelLoading();
        }
        
        #endregion
        
        #region Notification Methods
        
        private void NotifyLoadingStarted()
        {
            OnLoadingStarted?.Invoke(_progressData);
            
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnProgressUpdated(_progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
            
            UpdateUI();
        }
        
        private void NotifyProgressChanged()
        {
            OnLoadingProgressChanged?.Invoke(_progressData);
            
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnProgressUpdated(_progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
            
            UpdateUI();
        }
        
        private void NotifyStepStarted(ILoadingStep step)
        {
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnStepStarted(step, _progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
            
            UpdateUI();
        }
        
        private void NotifyStepCompleted(ILoadingStep step)
        {
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnStepCompleted(step, _progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
        }
        
        private void NotifyLoadingCompleted()
        {
            OnLoadingCompleted?.Invoke(_progressData);
            
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnLoadingCompleted(_progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
            
            UpdateUI();
        }
        
        private void NotifyLoadingError(ILoadingStep step, Exception exception)
        {
            OnLoadingError?.Invoke(step, exception, _progressData);
            
            foreach (var callback in _progressCallbacks.ToList())
            {
                try
                {
                    callback.OnLoadingError(step, exception, _progressData);
                }
                catch (Exception ex)
                {
                    DebugLogError($"Error in progress callback: {ex.Message}");
                }
            }
        }
        
        private void UpdateUI()
        {
            if (_uiController != null)
            {
                _uiController.UpdateProgress(_progressData);
                _uiController.UpdateStepText(_progressData.CurrentStepName, _progressData.CurrentStepDescription);
                _uiController.UpdateProgressBar(_progressData.TotalProgress);
            }
        }
        
        #endregion
        
        #region Debug Methods
        
        private void DebugLog(string message)
        {
            if (_enableDebugLogs)
            {
                ConsoleLogger.Log($"[LoadingManager] {message}");
            }
        }
        
        private void DebugLogWarning(string message)
        {
            if (_enableDebugLogs)
            {
                ConsoleLogger.LogWarning($"[LoadingManager] {message}");
            }
        }
        
        private void DebugLogError(string message)
        {
            if (_enableDebugLogs)
            {
                ConsoleLogger.LogError($"[LoadingManager] {message}");
            }
        }
        
        #endregion
        
        #endregion
    }
}