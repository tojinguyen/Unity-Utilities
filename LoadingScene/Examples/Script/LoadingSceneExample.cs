using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Examples
{
    /// <summary>
    /// Example: Loading từ HOME scene sang INGAME scene với custom loading steps
    /// Mô phỏng quy trình loading thực tế trong game production
    /// </summary>
    public class LoadingSceneExample : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string ingameSceneName = "INGAME";
        [SerializeField] private string homeSceneName = "HOME";
        
        [Header("Loading Settings")]
        [SerializeField] private bool showUI = true;
        [SerializeField] private GameObject loadingUIPrefab;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Game Data Settings")]
        [SerializeField] private string[] gameDataPaths = { "PlayerData", "LevelData", "Settings" };
        [SerializeField] private string[] addressableKeys = { "PlayerPrefab", "UICanvas", "GameManager" };
        
        private void Start()
        {
            SetupLoadingManager();
        }
        
        private void SetupLoadingManager()
        {
            // Setup UI Controller
            if (loadingUIPrefab != null)
            {
                var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
                LoadingManager.Instance.SetUIController(uiController);
            }
            
            // Add progress callback if debug is enabled
            if (enableDebugLogs)
            {
                LoadingManager.Instance.AddProgressCallback(new GameLoadingProgressCallback());
            }
        }
        
        #region Public Methods (for UI buttons)
        
        /// <summary>
        /// Main method: Chuyển từ HOME scene sang INGAME scene với đầy đủ loading steps
        /// </summary>
        /// <param name="targetSceneName">Tên scene đích (mặc định là INGAME)</param>
        public async UniTaskVoid LoadIngameScene(string targetSceneName = null)
        {
            string sceneToLoad = string.IsNullOrEmpty(targetSceneName) ? ingameSceneName : targetSceneName;
            
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                ConsoleLogger.LogError("Target scene name is not set!");
                return;
            }
            
            ConsoleLogger.Log($"Starting transition from HOME to {sceneToLoad}...");
            
            var steps = CreateIngameLoadingSteps(sceneToLoad);
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
                ConsoleLogger.Log($"Successfully loaded {sceneToLoad} scene!");
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"Failed to load {sceneToLoad}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Quay về HOME scene
        /// </summary>
        public async UniTaskVoid ReturnToHome()
        {
            ConsoleLogger.Log("Returning to HOME scene...");
            
            var steps = CreateReturnHomeSteps();
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, showUI);
                ConsoleLogger.Log("Successfully returned to HOME!");
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"Failed to return to HOME: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Private Methods - Create Loading Steps
        
        /// <summary>
        /// Tạo chuỗi loading steps cho việc chuyển sang INGAME scene
        /// </summary>
        private List<ILoadingStep> CreateIngameLoadingSteps(string targetScene)
        {
            var steps = new List<ILoadingStep>();
            
            // Step 1: Preparation - Save current progress, cleanup
            steps.Add(new SaveGameDataStep("Saving Progress", "Saving your current progress..."));
            
            // Step 2: Load game data
            steps.Add(new LoadGameDataStep(gameDataPaths, "Loading Game Data", "Loading player data and settings..."));
            
            // Step 3: Load addressable assets
            steps.Add(new LoadAddressableAssetsStep(addressableKeys, "Loading Assets", "Loading game assets..."));
            
            // Step 4: Initialize game systems
            steps.Add(new InitializeGameSystemsStep("Initialize Systems", "Setting up game systems..."));
            
            // Step 5: Scene loading
            steps.Add(LoadingStepFactory.CreateSceneLoad(targetScene, 2f));
            
            // Step 6: Post-scene setup
            steps.Add(new PostSceneSetupStep("Post Setup", "Finalizing game setup..."));
            
            return steps;
        }
        
        /// <summary>
        /// Tạo chuỗi loading steps cho việc quay về HOME
        /// </summary>
        private List<ILoadingStep> CreateReturnHomeSteps()
        {
            var steps = new List<ILoadingStep>();
            
            // Step 1: Cleanup game data
            steps.Add(new CleanupGameDataStep("Cleanup", "Cleaning up game data..."));
            
            // Step 2: Return to home scene  
            steps.Add(LoadingStepFactory.CreateSceneLoad(homeSceneName, 1.5f));
            
            return steps;
        }
        
        #endregion
        
        #region Context Menu Methods (for testing in editor)
        
        [ContextMenu("Load INGAME Scene")]
        private void TestLoadIngame()
        {
            LoadIngameScene().Forget();
        }
        
        [ContextMenu("Load Custom Scene")]
        private void TestLoadCustomScene()
        {
            LoadIngameScene("TestScene").Forget();
        }
        
        [ContextMenu("Return to HOME")]
        private void TestReturnHome()
        {
            ReturnToHome().Forget();
        }
        
        #endregion
    }
    
    #region Custom Loading Steps - Production Example Implementation
    
    /// <summary>
    /// Step 1: Save game data trước khi chuyển scene
    /// </summary>
    public class SaveGameDataStep : BaseLoadingStep
    {
        public SaveGameDataStep(string stepName, string description, float weight = 0.5f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            UpdateProgressInternal(0.2f);
            
            // Simulate saving player progress
            await Task.Delay(300);
            ThrowIfCancelled();
            
            UpdateProgressInternal(0.6f);
            
            // Simulate saving game settings
            await Task.Delay(200);
            ThrowIfCancelled();
            
            UpdateProgressInternal(1f);
            ConsoleLogger.Log("Game data saved successfully");
        }
    }
    
    /// <summary>
    /// Step 2: Load game data cần thiết cho INGAME scene
    /// </summary>
    public class LoadGameDataStep : BaseLoadingStep
    {
        private readonly string[] _dataPaths;
        
        public LoadGameDataStep(string[] dataPaths, string stepName, string description, float weight = 1f)
            : base(stepName, description, weight)
        {
            _dataPaths = dataPaths ?? new string[0];
        }
        
        protected override async Task ExecuteStepAsync()
        {
            for (int i = 0; i < _dataPaths.Length; i++)
            {
                ThrowIfCancelled();
                
                // Simulate loading each data file
                ConsoleLogger.Log($"Loading data: {_dataPaths[i]}");
                await Task.Delay(400);
                
                UpdateProgressInternal((float)(i + 1) / _dataPaths.Length);
            }
            
            ConsoleLogger.Log($"Loaded {_dataPaths.Length} data files");
        }
    }
    
    /// <summary>
    /// Step 3: Load Addressable assets
    /// </summary>
    public class LoadAddressableAssetsStep : BaseLoadingStep
    {
        private readonly string[] _assetKeys;
        
        public LoadAddressableAssetsStep(string[] assetKeys, string stepName, string description, float weight = 1.5f)
            : base(stepName, description, weight)
        {
            _assetKeys = assetKeys ?? new string[0];
        }
        
        protected override async Task ExecuteStepAsync()
        {
            for (int i = 0; i < _assetKeys.Length; i++)
            {
                ThrowIfCancelled();
                
                // Simulate Addressable asset loading
                ConsoleLogger.Log($"Loading asset: {_assetKeys[i]}");
                
                // Simulate download progress (for remote assets)
                for (float progress = 0; progress < 1f; progress += 0.1f)
                {
                    ThrowIfCancelled();
                    await Task.Delay(50);
                    
                    float totalProgress = (i + progress) / _assetKeys.Length;
                    UpdateProgressInternal(totalProgress);
                }
            }
            
            ConsoleLogger.Log($"Loaded {_assetKeys.Length} addressable assets");
        }
    }
    
    /// <summary>
    /// Step 4: Initialize game systems
    /// </summary>
    public class InitializeGameSystemsStep : BaseLoadingStep
    {
        private readonly string[] _systemNames = { "Audio Manager", "Input System", "UI Manager", "Game Manager", "Network Manager" };
        
        public InitializeGameSystemsStep(string stepName, string description, float weight = 1f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            for (int i = 0; i < _systemNames.Length; i++)
            {
                ThrowIfCancelled();
                
                ConsoleLogger.Log($"Initializing: {_systemNames[i]}");
                
                // Simulate system initialization time
                await Task.Delay(300);
                
                UpdateProgressInternal((float)(i + 1) / _systemNames.Length);
            }
            
            ConsoleLogger.Log("All game systems initialized");
        }
    }
    
    /// <summary>
    /// Step 6: Post-scene setup sau khi scene đã load
    /// </summary>
    public class PostSceneSetupStep : BaseLoadingStep
    {
        public PostSceneSetupStep(string stepName, string description, float weight = 0.5f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            UpdateProgressInternal(0.3f);
            
            // Simulate spawning player
            ConsoleLogger.Log("Spawning player...");
            await Task.Delay(200);
            ThrowIfCancelled();
            
            UpdateProgressInternal(0.7f);
            
            // Simulate setting up game state
            ConsoleLogger.Log("Setting up game state...");
            await Task.Delay(300);
            ThrowIfCancelled();
            
            UpdateProgressInternal(1f);
            ConsoleLogger.Log("Game setup completed - Ready to play!");
        }
    }
    
    /// <summary>
    /// Cleanup step khi quay về HOME
    /// </summary>
    public class CleanupGameDataStep : BaseLoadingStep
    {
        public CleanupGameDataStep(string stepName, string description, float weight = 0.5f)
            : base(stepName, description, weight)
        {
        }
        
        protected override async Task ExecuteStepAsync()
        {
            UpdateProgressInternal(0.3f);
            
            // Simulate cleanup game objects
            ConsoleLogger.Log("Cleaning up game objects...");
            await Task.Delay(200);
            ThrowIfCancelled();
            
            UpdateProgressInternal(0.7f);
            
            // Simulate releasing resources
            ConsoleLogger.Log("Releasing resources...");
            await Task.Delay(200);
            ThrowIfCancelled();
            
            UpdateProgressInternal(1f);
            ConsoleLogger.Log("Cleanup completed");
        }
    }
    
    /// <summary>
    /// Custom progress callback cho game production
    /// </summary>
    public class GameLoadingProgressCallback : ILoadingProgressCallback
    {
        public void OnProgressUpdated(LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[LOADING] {progressData.TotalProgress:P1} - {progressData.CurrentStepName}: {progressData.CurrentStepDescription}");
        }
        
        public void OnStepStarted(ILoadingStep step, LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[STEP START] {step.StepName}");
        }
        
        public void OnStepCompleted(ILoadingStep step, LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[STEP DONE] {step.StepName} - Total elapsed: {progressData.ElapsedTime.TotalSeconds:F1}s");
        }
        
        public void OnLoadingCompleted(LoadingProgressData progressData)
        {
            ConsoleLogger.Log($"[LOADING COMPLETE] Total time: {progressData.ElapsedTime.TotalSeconds:F1}s");
        }
        
        public void OnLoadingError(ILoadingStep step, System.Exception exception, LoadingProgressData progressData)
        {
            ConsoleLogger.LogError($"[LOADING ERROR] Step '{step?.StepName}': {exception.Message}");
        }
    }
    
    #endregion
}