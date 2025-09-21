using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Examples
{
    public class LoadingSceneExample : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private string targetSceneName = "GameScene";
        [SerializeField] private bool showUI = true;
        [SerializeField] private GameObject loadingUIPrefab;
        
        private void Start()
        {
            if (loadingUIPrefab != null)
            {
                var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
                LoadingManager.Instance.SetUIController(uiController);
            }
            
            LoadingManager.Instance.AddProgressCallback(new ConsoleLoggerProgressCallback());
        }
        
        #region Public Methods (for UI buttons)
        
        /// <summary>
        /// Start simple scene loading
        /// </summary>
        public async void StartSimpleSceneLoad()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                ConsoleLogger.LogWarning("Target scene name is not set!");
                return;
            }
            
            var steps = new List<ILoadingStep>();
            steps.Add(LoadingStepFactory.CreateSceneLoad(targetSceneName));
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }
        
        /// <summary>
        /// Start custom loading sequence with user-defined steps
        /// </summary>
        public async void StartCustomLoading()
        {
            var steps = new List<ILoadingStep>();
            
            // Example: Add custom delay step implementation
            steps.Add(new CustomDelayStep(1f, "Custom Delay", "Example custom delay step"));
            
            // Example: Add custom work step
            steps.Add(new CustomWorkStep("Custom Work", "Doing some custom work..."));
            
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                steps.Add(LoadingStepFactory.CreateSceneLoad(targetSceneName));
            }
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }

        /// <summary>
        /// Test loading with error handling
        /// </summary>
        public async UniTaskVoid TestLoadingWithError()
        {
            var steps = new List<ILoadingStep>();
            
            steps.Add(new CustomDelayStep(1f, "Normal Step", "This should work fine..."));
            
            // Add a step that will fail
            steps.Add(new CustomErrorStep("Error Step", "This step will fail..."));
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"Loading failed as expected: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test cancellation
        /// </summary>
        public async void TestCancellation()
        {
            var steps = new List<ILoadingStep>();
            
            steps.Add(new CustomDelayStep(5f, "Long Step", "This is a long step that can be cancelled..."));
            steps.Add(new CustomDelayStep(3f, "Another Step", "Another step..."));
            
            // Start loading (this will run in background)
            var loadingTask = LoadingManager.Instance.StartLoadingAsync(steps, showUI);
            
            // Cancel after 2 seconds
            await Task.Delay(2000);
            LoadingManager.Instance.CancelLoading();
            
            try
            {
                await loadingTask;
            }
            catch (System.OperationCanceledException)
            {
                ConsoleLogger.Log("Loading was cancelled successfully");
            }
        }
        
        #endregion
        
        #region Test Methods (for inspector buttons)
        
        [ContextMenu("Test Simple Scene Load")]
        private void TestSimpleLoad()
        {
            StartSimpleSceneLoad();
        }
        
        [ContextMenu("Test Custom Loading")]
        private void TestCustom()
        {
            StartCustomLoading();
        }
        
        [ContextMenu("Test Error Handling")]
        private void TestError()
        {
            TestLoadingWithError().Forget();
        }
        
        [ContextMenu("Test Cancellation")]
        private void TestCancel()
        {
            TestCancellation();
        }
        
        #endregion
    }
    
    // Example implementation of custom loading steps
    // Users should create their own implementations based on their needs
    
    /// <summary>
    /// Example custom delay step - users can implement their own version
    /// </summary>
    public class CustomDelayStep : BaseLoadingStep
    {
        private readonly float _duration;
        
        public CustomDelayStep(float duration, string stepName, string description, float weight = 1f)
            : base(stepName, description, weight)
        {
            _duration = duration;
        }
        
        protected override async Task ExecuteStepAsync()
        {
            float elapsed = 0f;
            while (elapsed < _duration && !isCancelled)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateProgressInternal(elapsed / _duration);
                await Task.Yield();
                ThrowIfCancelled();
            }
        }
    }
    
    /// <summary>
    /// Example custom work step - users can implement their own version
    /// </summary>
    public class CustomWorkStep : BaseLoadingStep
    {
        public CustomWorkStep(string stepName, string description, float weight = 1f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            // Simulate some work
            for (int i = 0; i < 10; i++)
            {
                ThrowIfCancelled();
                UpdateProgressInternal((float)i / 10f);
                await Task.Delay(100);
            }
            UpdateProgressInternal(1f);
        }
    }
    
    /// <summary>
    /// Example error step for testing - users can implement their own version
    /// </summary>
    public class CustomErrorStep : BaseLoadingStep
    {
        public CustomErrorStep(string stepName, string description, float weight = 1f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            UpdateProgressInternal(0.5f);
            await Task.Delay(500);
            throw new System.Exception("Test error - this is intentional!");
        }
    }
    
    public class ConsoleLoggerProgressCallback : ILoadingProgressCallback
    {
        public void OnProgressUpdated(LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[LoadingProgress] {progressData.TotalProgress:P1} - {progressData.CurrentStepName}: {progressData.CurrentStepDescription}");
        }
        
        public void OnStepStarted(ILoadingStep step, LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[LoadingStep] Started: {step.StepName}");
        }
        
        public void OnStepCompleted(ILoadingStep step, LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[LoadingStep] Completed: {step.StepName}");
        }
        
        public void OnLoadingCompleted(LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[Loading] Completed in {progressData.ElapsedTime.TotalSeconds:F1}s");
        }
        
        public void OnLoadingError(ILoadingStep step, System.Exception exception, LoadingProgressData progressData)
        {
            ConsoleLogger.LogError($"[Loading] Error in step '{step?.StepName}': {exception.Message}");
        }
    }
}