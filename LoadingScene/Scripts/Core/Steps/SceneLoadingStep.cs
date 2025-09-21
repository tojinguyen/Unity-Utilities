using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TirexGame.Utils.LoadingScene
{
    public enum SceneLoadMode
    {
        Single,      
        Additive,   
        Replace     
    }

    public class SceneLoadingStep : BaseLoadingStep
    {
        private readonly string _sceneName;
        private readonly int _sceneBuildIndex;
        private readonly SceneLoadMode _loadMode;
        private readonly bool _allowSceneActivation;
        private readonly bool _unloadUnusedAssets;
        private readonly string _sceneToUnload;
        
        private AsyncOperation _loadOperation;
        private AsyncOperation _unloadOperation;
        private bool _useSceneName;
        
        #region Constructors
        
        /// <summary>
        /// Tạo SceneLoadingStep với tên scene
        /// </summary>
        /// <param name="sceneName">Tên scene cần load</param>
        /// <param name="loadMode">Chế độ load scene</param>
        /// <param name="allowSceneActivation">Cho phép scene được activate ngay lập tức</param>
        /// <param name="unloadUnusedAssets">Gọi Resources.UnloadUnusedAssets sau khi load</param>
        /// <param name="sceneToUnload">Tên scene cần unload (chỉ dùng với Replace mode)</param>
        /// <param name="weight">Trọng số của step</param>
        public SceneLoadingStep(string sceneName, SceneLoadMode loadMode = SceneLoadMode.Single, 
            bool allowSceneActivation = true, bool unloadUnusedAssets = true, 
            string sceneToUnload = null, float weight = 2f) 
            : base($"Loading Scene: {sceneName}", $"Loading scene '{sceneName}'...", weight)
        {
            _sceneName = sceneName ?? throw new ArgumentNullException(nameof(sceneName));
            _loadMode = loadMode;
            _allowSceneActivation = allowSceneActivation;
            _unloadUnusedAssets = unloadUnusedAssets;
            _sceneToUnload = sceneToUnload;
            _useSceneName = true;
        }
        
        /// <summary>
        /// Tạo SceneLoadingStep với build index
        /// </summary>
        /// <param name="sceneBuildIndex">Build index của scene</param>
        /// <param name="loadMode">Chế độ load scene</param>
        /// <param name="allowSceneActivation">Cho phép scene được activate ngay lập tức</param>
        /// <param name="unloadUnusedAssets">Gọi Resources.UnloadUnusedAssets sau khi load</param>
        /// <param name="sceneToUnload">Tên scene cần unload (chỉ dùng với Replace mode)</param>
        /// <param name="weight">Trọng số của step</param>
        public SceneLoadingStep(int sceneBuildIndex, SceneLoadMode loadMode = SceneLoadMode.Single, 
            bool allowSceneActivation = true, bool unloadUnusedAssets = true, 
            string sceneToUnload = null, float weight = 2f) 
            : base($"Loading Scene: {sceneBuildIndex}", $"Loading scene at index {sceneBuildIndex}...", weight)
        {
            _sceneBuildIndex = sceneBuildIndex;
            _loadMode = loadMode;
            _allowSceneActivation = allowSceneActivation;
            _unloadUnusedAssets = unloadUnusedAssets;
            _sceneToUnload = sceneToUnload;
            _useSceneName = false;
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// AsyncOperation của việc load scene (có thể null nếu chưa bắt đầu)
        /// </summary>
        public AsyncOperation LoadOperation => _loadOperation;
        
        /// <summary>
        /// AsyncOperation của việc unload scene (có thể null)
        /// </summary>
        public AsyncOperation UnloadOperation => _unloadOperation;
        
        #endregion
        
        #region Protected Methods Override
        
        protected override async Task ExecuteStepAsync()
        {
            try
            {
                // Bước 1: Unload scene cũ nếu cần (Replace mode)
                if (_loadMode == SceneLoadMode.Replace && !string.IsNullOrEmpty(_sceneToUnload))
                {
                    await UnloadSceneAsync();
                }
                
                // Bước 2: Load scene mới
                await LoadSceneAsync();
                
                // Bước 3: Unload unused assets nếu cần
                if (_unloadUnusedAssets)
                {
                    await UnloadUnusedAssetsAsync();
                }
                
                DebugLog($"Scene loading completed: {(_useSceneName ? _sceneName : _sceneBuildIndex.ToString())}");
            }
            catch (Exception ex)
            {
                DebugLog($"Scene loading failed: {ex.Message}");
                throw;
            }
        }
        
        public override void Cancel()
        {
            base.Cancel();
            
            // Cancel async operations if possible
            if (_loadOperation != null && !_loadOperation.isDone)
            {
                // Note: Unity's AsyncOperation cannot be cancelled directly
                // We can only prevent scene activation
                if (_loadOperation.allowSceneActivation)
                {
                    _loadOperation.allowSceneActivation = false;
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private async Task UnloadSceneAsync()
        {
            DebugLog($"Unloading scene: {_sceneToUnload}");
            UpdateProgressInternal(0.1f);
            
            ThrowIfCancelled();
            
            _unloadOperation = SceneManager.UnloadSceneAsync(_sceneToUnload);
            
            if (_unloadOperation == null)
            {
                DebugLog($"Scene '{_sceneToUnload}' not found or already unloaded");
                return;
            }
            
            while (!_unloadOperation.isDone && !isCancelled)
            {
                float unloadProgress = Mathf.Clamp01(_unloadOperation.progress / 0.9f);
                UpdateProgressInternal(0.1f + unloadProgress * 0.2f); // 10% to 30%
                await Task.Yield();
            }
            
            ThrowIfCancelled();
            DebugLog($"Scene unloaded: {_sceneToUnload}");
        }
        
        private async Task LoadSceneAsync()
        {
            DebugLog($"Starting scene load: {(_useSceneName ? _sceneName : _sceneBuildIndex.ToString())}");
            
            float startProgress = _loadMode == SceneLoadMode.Replace && !string.IsNullOrEmpty(_sceneToUnload) ? 0.3f : 0.1f;
            UpdateProgressInternal(startProgress);
            
            ThrowIfCancelled();
            
            // Determine Unity LoadSceneMode
            LoadSceneMode unityLoadMode = _loadMode == SceneLoadMode.Additive ? 
                LoadSceneMode.Additive : LoadSceneMode.Single;
            
            // Start async scene loading
            if (_useSceneName)
            {
                _loadOperation = SceneManager.LoadSceneAsync(_sceneName, unityLoadMode);
            }
            else
            {
                _loadOperation = SceneManager.LoadSceneAsync(_sceneBuildIndex, unityLoadMode);
            }
            
            if (_loadOperation == null)
            {
                throw new InvalidOperationException($"Failed to start loading scene: {(_useSceneName ? _sceneName : _sceneBuildIndex.ToString())}");
            }
            
            // Control scene activation
            _loadOperation.allowSceneActivation = _allowSceneActivation;
            
            // Wait for loading to complete
            while (!_loadOperation.isDone && !isCancelled)
            {
                // Unity reports progress from 0 to 0.9, then jumps to 1 when activation is allowed
                float rawProgress = _loadOperation.progress;
                float normalizedProgress = rawProgress < 0.9f ? rawProgress / 0.9f : 1f;
                
                float totalProgress = startProgress + normalizedProgress * (0.8f - startProgress); // startProgress to 80%
                UpdateProgressInternal(totalProgress);
                
                // If we're waiting for manual activation and loading is ready
                if (!_allowSceneActivation && rawProgress >= 0.9f)
                {
                    // Here you could add custom logic to decide when to activate
                    // For now, we'll activate immediately
                    _loadOperation.allowSceneActivation = true;
                }
                
                await Task.Yield();
            }
            
            ThrowIfCancelled();
            
            UpdateProgressInternal(0.8f);
            DebugLog($"Scene loaded: {(_useSceneName ? _sceneName : _sceneBuildIndex.ToString())}");
        }
        
        private async Task UnloadUnusedAssetsAsync()
        {
            DebugLog("Unloading unused assets...");
            UpdateProgressInternal(0.85f);
            
            ThrowIfCancelled();
            
            AsyncOperation unloadAssetsOperation = Resources.UnloadUnusedAssets();
            
            while (!unloadAssetsOperation.isDone && !isCancelled)
            {
                float unloadProgress = unloadAssetsOperation.progress;
                UpdateProgressInternal(0.85f + unloadProgress * 0.15f); // 85% to 100%
                await Task.Yield();
            }
            
            ThrowIfCancelled();
            DebugLog("Unused assets unloaded");
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Tạo step load scene đơn giản với tên scene
        /// </summary>
        /// <param name="sceneName">Tên scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep LoadScene(string sceneName, float weight = 2f)
        {
            return new SceneLoadingStep(sceneName, SceneLoadMode.Single, true, true, null, weight);
        }
        
        /// <summary>
        /// Tạo step load scene đơn giản với build index
        /// </summary>
        /// <param name="sceneBuildIndex">Build index</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep LoadScene(int sceneBuildIndex, float weight = 2f)
        {
            return new SceneLoadingStep(sceneBuildIndex, SceneLoadMode.Single, true, true, null, weight);
        }
        
        /// <summary>
        /// Tạo step load scene additive
        /// </summary>
        /// <param name="sceneName">Tên scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep LoadSceneAdditive(string sceneName, float weight = 2f)
        {
            return new SceneLoadingStep(sceneName, SceneLoadMode.Additive, true, false, null, weight);
        }
        
        /// <summary>
        /// Tạo step thay thế scene hiện tại
        /// </summary>
        /// <param name="newSceneName">Tên scene mới</param>
        /// <param name="oldSceneName">Tên scene cũ cần unload</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep ReplaceScene(string newSceneName, string oldSceneName, float weight = 2f)
        {
            return new SceneLoadingStep(newSceneName, SceneLoadMode.Replace, true, true, oldSceneName, weight);
        }
        
        #endregion
    }
    
    public static class SceneLoadingStepHelpers
    {
        public static SceneLoadingStep[] LoadScenesAdditive(string[] sceneNames, float weightPerScene = 1f)
        {
            if (sceneNames == null || sceneNames.Length == 0)
                return Array.Empty<SceneLoadingStep>();
            
            var steps = new SceneLoadingStep[sceneNames.Length];
            
            for (var i = 0; i < sceneNames.Length; i++)
            {
                steps[i] = SceneLoadingStep.LoadSceneAdditive(sceneNames[i], weightPerScene);
            }
            
            return steps;
        }
 
        public static ILoadingStep[] CreateSceneTransition(string targetScene, ILoadingStep[] preparationSteps = null)
        {
            var steps = new System.Collections.Generic.List<ILoadingStep>();
            
            if (preparationSteps is { Length: > 0 })
            {
                steps.AddRange(preparationSteps);
            }
            
            steps.Add(SceneLoadingStep.LoadScene(targetScene));
            
            // Note: Users can add their own finalization steps if needed
            // Example: steps.Add(new CustomDelayStep(0.5f, "Finalizing", "Finalizing scene transition..."));
            
            return steps.ToArray();
        }
    }
}