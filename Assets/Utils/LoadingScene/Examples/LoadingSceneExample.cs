using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Examples
{
    /// <summary>
    /// Example script demonstrating basic usage of the Loading Scene system.
    /// Attach this to a GameObject in your scene to test the loading functionality.
    /// </summary>
    public class LoadingSceneExample : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private string targetSceneName = "GameScene";
        [SerializeField] private bool useStandardTransition = true;
        [SerializeField] private bool showUI = true;
        [SerializeField] private GameObject loadingUIPrefab;
        
        [Header("Custom Loading Steps")]
        [SerializeField] private bool includeDelayStep = true;
        [SerializeField] private float delayDuration = 2f;
        [SerializeField] private bool includeSystemInit = true;
        [SerializeField] private bool includeResourceLoading = false;
        [SerializeField] private string resourcePath = "TestAsset";
        
        private void Start()
        {
            // Setup UI Controller if prefab is provided
            if (loadingUIPrefab != null)
            {
                var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
                LoadingManager.Instance.SetUIController(uiController);
            }
            
            // Add a progress callback for debugging
            LoadingManager.Instance.AddProgressCallback(new DebugProgressCallback());
        }
        
        #region Public Methods (for UI buttons)
        
        /// <summary>
        /// Start standard scene transition
        /// </summary>
        public async void StartStandardSceneTransition()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("Target scene name is not set!");
                return;
            }
            
            var steps = LoadingStepFactory.CreateStandardSceneTransition(
                targetSceneName, includeSystemInit, includeResourceLoading);
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }
        
        /// <summary>
        /// Start simple scene loading
        /// </summary>
        public async void StartSimpleSceneLoad()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("Target scene name is not set!");
                return;
            }
            
            var steps = LoadingStepFactory.CreateSimpleSceneLoad(targetSceneName, 2f);
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }
        
        /// <summary>
        /// Start custom loading sequence
        /// </summary>
        public async void StartCustomLoading()
        {
            var steps = new List<ILoadingStep>();
            
            if (includeDelayStep)
            {
                steps.Add(LoadingStepFactory.CreateDelay(delayDuration, "Custom Delay", "Testing delay step..."));
            }
            
            if (includeSystemInit)
            {
                steps.Add(LoadingStepFactory.CreateSystemInit(
                    new[] { "Audio System", "Graphics System", "Input System" }, 0.5f));
            }
            
            if (includeResourceLoading && !string.IsNullOrEmpty(resourcePath))
            {
                steps.Add(LoadingStepFactory.CreateResourceLoad(resourcePath));
            }
            
            // Add custom step with lambda
            steps.Add(LoadingStepFactory.CreateCustom(async (step) => {
                step.UpdateProgress(0.3f);
                await Task.Delay(500);
                step.UpdateProgress(0.7f);
                await Task.Delay(300);
                step.UpdateProgress(1f);
            }, "Custom Step", "Doing custom work..."));
            
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                steps.Add(LoadingStepFactory.CreateSceneLoad(targetSceneName));
            }
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }
        
        /// <summary>
        /// Restart game to main menu
        /// </summary>
        public async void RestartGame()
        {
            var steps = LoadingStepFactory.CreateGameRestart("MainMenu");
            await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
        }
        
        /// <summary>
        /// Test loading with error handling
        /// </summary>
        public async void TestLoadingWithError()
        {
            var steps = new List<ILoadingStep>();
            
            steps.Add(LoadingStepFactory.CreateDelay(1f, "Normal Step", "This should work fine..."));
            
            // Add a step that will fail
            steps.Add(LoadingStepFactory.CreateCustom(async (step) => {
                step.UpdateProgress(0.5f);
                await Task.Delay(500);
                throw new System.Exception("Test error - this is intentional!");
            }, "Error Step", "This step will fail..."));
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Loading failed as expected: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test cancellation
        /// </summary>
        public async void TestCancellation()
        {
            var steps = new List<ILoadingStep>();
            
            steps.Add(LoadingStepFactory.CreateDelay(5f, "Long Step", "This is a long step that can be cancelled..."));
            steps.Add(LoadingStepFactory.CreateDelay(3f, "Another Step", "Another step..."));
            
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
                Debug.Log("Loading was cancelled successfully");
            }
        }
        
        #endregion
        
        #region Test Methods (for inspector buttons)
        
        [ContextMenu("Test Standard Scene Transition")]
        private void TestStandardTransition()
        {
            StartStandardSceneTransition();
        }
        
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
            TestLoadingWithError();
        }
        
        [ContextMenu("Test Cancellation")]
        private void TestCancel()
        {
            TestCancellation();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Example progress callback for debugging
    /// </summary>
    public class DebugProgressCallback : ILoadingProgressCallback
    {
        public void OnProgressUpdated(LoadingProgressData progressData)
        {
            Debug.Log($"[LoadingProgress] {progressData.TotalProgress:P1} - {progressData.CurrentStepName}: {progressData.CurrentStepDescription}");
        }
        
        public void OnStepStarted(ILoadingStep step, LoadingProgressData progressData)
        {
            Debug.Log($"[LoadingStep] Started: {step.StepName}");
        }
        
        public void OnStepCompleted(ILoadingStep step, LoadingProgressData progressData)
        {
            Debug.Log($"[LoadingStep] Completed: {step.StepName}");
        }
        
        public void OnLoadingCompleted(LoadingProgressData progressData)
        {
            Debug.Log($"[Loading] Completed in {progressData.ElapsedTime.TotalSeconds:F1}s");
        }
        
        public void OnLoadingError(ILoadingStep step, System.Exception exception, LoadingProgressData progressData)
        {
            Debug.LogError($"[Loading] Error in step '{step?.StepName}': {exception.Message}");
        }
    }
}