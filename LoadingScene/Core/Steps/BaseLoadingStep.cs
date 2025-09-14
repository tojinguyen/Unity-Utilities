using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Base class cho tất cả các loading steps.
    /// Implement Command Pattern để đóng gói các hành động loading.
    /// </summary>
    public abstract class BaseLoadingStep : ILoadingStep
    {
        #region Protected Fields
        
        protected bool _isCompleted;
        protected bool _isCancelled;
        protected float _progress;
        
        #endregion

        #region ILoadingStep Implementation
        
        public virtual string StepName { get; protected set; } = "Loading Step";
        public virtual string Description { get; protected set; } = "Processing...";
        public virtual float Weight { get; protected set; } = 1f;
        public virtual bool CanSkip { get; protected set; } = false;
        
        public float Progress 
        { 
            get => _progress; 
            protected set 
            { 
                _progress = Mathf.Clamp01(value);
                OnProgressChanged?.Invoke(this);
            } 
        }
        
        public event Action<ILoadingStep> OnProgressChanged;
        public event Action<ILoadingStep> OnStepCompleted;
        public event Action<ILoadingStep, Exception> OnStepError;
        
        #endregion

        #region Constructor
        
        protected BaseLoadingStep(string stepName, string description = "", float weight = 1f, bool canSkip = false)
        {
            StepName = stepName;
            Description = !string.IsNullOrEmpty(description) ? description : stepName;
            Weight = Mathf.Max(0f, weight);
            CanSkip = canSkip;
            Reset();
        }
        
        #endregion

        #region Public Methods
        
        public async Task ExecuteAsync()
        {
            if (_isCompleted)
            {
                Debug.LogWarning($"Step '{StepName}' is already completed!");
                return;
            }
            
            if (_isCancelled)
            {
                Debug.LogWarning($"Step '{StepName}' was cancelled!");
                return;
            }
            
            try
            {
                Progress = 0f;
                await ExecuteStepAsync();
                
                if (!_isCancelled)
                {
                    Progress = 1f;
                    _isCompleted = true;
                    OnStepCompleted?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in step '{StepName}': {ex.Message}");
                OnStepError?.Invoke(this, ex);
                throw;
            }
        }
        
        public virtual void Cancel()
        {
            _isCancelled = true;
            Debug.Log($"Step '{StepName}' cancelled");
        }
        
        public virtual void Reset()
        {
            _progress = 0f;
            _isCompleted = false;
            _isCancelled = false;
        }
        
        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Override method này để implement logic cụ thể của step
        /// </summary>
        protected abstract Task ExecuteStepAsync();
        
        /// <summary>
        /// Cập nhật tiến độ của step này (Implementation của ILoadingStep)
        /// </summary>
        /// <param name="progress">Progress value (0-1)</param>
        public void UpdateProgress(float progress)
        {
            UpdateProgressInternal(progress);
        }
        
        /// <summary>
        /// Helper method để cập nhật progress an toàn
        /// </summary>
        /// <param name="progress">Progress value (0-1)</param>
        protected void UpdateProgressInternal(float progress)
        {
            if (!_isCancelled && !_isCompleted)
            {
                Progress = progress;
            }
        }
        
        /// <summary>
        /// Helper method để log debug information
        /// </summary>
        /// <param name="message">Debug message</param>
        protected void DebugLog(string message)
        {
            Debug.Log($"[{StepName}] {message}");
        }
        
        /// <summary>
        /// Kiểm tra xem step có bị cancel không, throw exception nếu có
        /// </summary>
        protected void ThrowIfCancelled()
        {
            if (_isCancelled)
            {
                throw new OperationCanceledException($"Step '{StepName}' was cancelled");
            }
        }
        
        #endregion
    }
}