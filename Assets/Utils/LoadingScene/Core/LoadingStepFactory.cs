using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Factory class để tạo ra các loại loading steps khác nhau.
    /// Sử dụng Factory Pattern để đơn giản hóa việc tạo steps.
    /// </summary>
    public static class LoadingStepFactory
    {
        /// <summary>
        /// Tạo delay step đơn giản
        /// </summary>
        /// <param name="duration">Thời gian delay (giây)</param>
        /// <param name="stepName">Tên step</param>
        /// <param name="description">Mô tả step</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>DelayLoadingStep instance</returns>
        public static DelayLoadingStep CreateDelay(float duration, string stepName = "Loading", string description = "", float weight = 1f)
        {
            return new DelayLoadingStep(duration, stepName, description, false, weight);
        }
        
        /// <summary>
        /// Tạo scene loading step
        /// </summary>
        /// <param name="sceneName">Tên scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep CreateSceneLoad(string sceneName, float weight = 2f)
        {
            return SceneLoadingStep.LoadScene(sceneName, weight);
        }
        
        /// <summary>
        /// Tạo scene loading step với build index
        /// </summary>
        /// <param name="sceneIndex">Build index của scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep CreateSceneLoad(int sceneIndex, float weight = 2f)
        {
            return SceneLoadingStep.LoadScene(sceneIndex, weight);
        }
        
        /// <summary>
        /// Tạo resource loading step
        /// </summary>
        /// <param name="resourcePath">Đường dẫn resource</param>
        /// <param name="assetType">Loại asset (optional)</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>ResourceLoadingStep instance</returns>
        public static ResourceLoadingStep CreateResourceLoad(string resourcePath, System.Type assetType = null, float weight = 1f)
        {
            return new ResourceLoadingStep(resourcePath, assetType, "", weight);
        }
        
        /// <summary>
        /// Tạo custom loading step với action
        /// </summary>
        /// <param name="action">Async action để thực thi</param>
        /// <param name="stepName">Tên step</param>
        /// <param name="description">Mô tả step</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>CustomLoadingStep instance</returns>
        public static CustomLoadingStep CreateCustom(System.Func<ILoadingStep, System.Threading.Tasks.Task> action, 
            string stepName, string description = "", float weight = 1f)
        {
            return new CustomLoadingStep(action, stepName, description, weight);
        }
        
        /// <summary>
        /// Tạo custom loading step với sync action
        /// </summary>
        /// <param name="action">Sync action để thực thi</param>
        /// <param name="stepName">Tên step</param>
        /// <param name="description">Mô tả step</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>CustomLoadingStep instance</returns>
        public static CustomLoadingStep CreateCustom(System.Action<ILoadingStep> action, 
            string stepName, string description = "", float weight = 1f)
        {
            return new CustomLoadingStep(action, stepName, description, weight);
        }
        
        /// <summary>
        /// Tạo game system initialization step
        /// </summary>
        /// <param name="systemNames">Danh sách tên hệ thống</param>
        /// <param name="timePerSystem">Thời gian cho mỗi hệ thống</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>GameSystemInitializationStep instance</returns>
        public static GameSystemInitializationStep CreateSystemInit(string[] systemNames = null, 
            float timePerSystem = 0.5f, float weight = 1f)
        {
            return new GameSystemInitializationStep(systemNames, timePerSystem, weight);
        }
        
        /// <summary>
        /// Tạo một chuỗi loading steps chuẩn cho việc chuyển scene
        /// </summary>
        /// <param name="targetScene">Scene đích</param>
        /// <param name="includeSystemInit">Có bao gồm system initialization không</param>
        /// <param name="includeAssetPreload">Có bao gồm asset preloading không</param>
        /// <param name="customSteps">Các custom steps bổ sung</param>
        /// <returns>Danh sách ILoadingStep</returns>
        public static List<ILoadingStep> CreateStandardSceneTransition(string targetScene, 
            bool includeSystemInit = true, bool includeAssetPreload = true, ILoadingStep[] customSteps = null)
        {
            var steps = new List<ILoadingStep>();
            
            // Preparation phase
            steps.Add(CreateDelay(0.5f, "Preparing", "Preparing to load scene...", 0.3f));
            
            if (includeSystemInit)
            {
                steps.Add(CreateSystemInit(new[] { "Audio", "Input", "UI" }, 0.3f, 0.8f));
            }
            
            // Add custom preparation steps
            if (customSteps != null)
            {
                steps.AddRange(customSteps);
            }
            
            if (includeAssetPreload)
            {
                steps.Add(CreateDelay(1f, "Preloading Assets", "Preloading essential assets...", 1f));
            }
            
            // Main scene loading
            steps.Add(CreateSceneLoad(targetScene, 3f));
            
            // Finalization
            steps.Add(CreateDelay(0.3f, "Finalizing", "Finalizing scene setup...", 0.2f));
            
            return steps;
        }
        
        /// <summary>
        /// Tạo loading steps đơn giản chỉ có load scene
        /// </summary>
        /// <param name="targetScene">Scene đích</param>
        /// <param name="minimumLoadTime">Thời gian loading tối thiểu</param>
        /// <returns>Danh sách ILoadingStep</returns>
        public static List<ILoadingStep> CreateSimpleSceneLoad(string targetScene, float minimumLoadTime = 1f)
        {
            var steps = new List<ILoadingStep>();
            
            if (minimumLoadTime > 0f)
            {
                steps.Add(CreateDelay(minimumLoadTime * 0.3f, "Preparing", "Preparing...", 0.5f));
            }
            
            steps.Add(CreateSceneLoad(targetScene, 2f));
            
            if (minimumLoadTime > 0f)
            {
                steps.Add(CreateDelay(minimumLoadTime * 0.2f, "Finishing", "Finishing...", 0.3f));
            }
            
            return steps;
        }
        
        /// <summary>
        /// Tạo loading steps cho việc restart game
        /// </summary>
        /// <param name="firstSceneName">Tên scene đầu tiên của game</param>
        /// <returns>Danh sách ILoadingStep</returns>
        public static List<ILoadingStep> CreateGameRestart(string firstSceneName = "MainMenu")
        {
            var steps = new List<ILoadingStep>();
            
            steps.Add(CreateDelay(0.5f, "Cleaning Up", "Cleaning up resources...", 0.5f));
            steps.Add(CreateSystemInit(new[] { "Save System", "Audio", "Input", "UI" }, 0.4f, 1f));
            steps.Add(CreateSceneLoad(firstSceneName, 2f));
            steps.Add(CreateDelay(0.3f, "Ready", "Game is ready!", 0.2f));
            
            return steps;
        }
    }
}