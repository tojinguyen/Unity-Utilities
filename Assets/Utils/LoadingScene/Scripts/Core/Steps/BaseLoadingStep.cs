using System;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
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
                ConsoleLogger.LogWarning($"Step '{StepName}' is already completed!");
                return;
            }
            
            if (_isCancelled)
            {
                ConsoleLogger.LogWarning($"Step '{StepName}' was cancelled!");
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
                ConsoleLogger.LogError($"Error in step '{StepName}': {ex.Message}");
                OnStepError?.Invoke(this, ex);
                throw;
            }
        }
        
        public virtual void Cancel()
        {
            _isCancelled = true;
            ConsoleLogger.Log($"Step '{StepName}' cancelled");
        }
        
        public virtual void Reset()
        {
            _progress = 0f;
            _isCompleted = false;
            _isCancelled = false;
        }
        
        #endregion

        #region Protected Methods

        protected abstract Task ExecuteStepAsync();
  
        public void UpdateProgress(float progress)
        {
            UpdateProgressInternal(progress);
        }

        protected void UpdateProgressInternal(float progress)
        {
            if (!_isCancelled && !_isCompleted)
            {
                Progress = progress;
            }
        }
   
        protected void DebugLog(string message)
        {
            ConsoleLogger.Log($"[{StepName}] {message}");
        }

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