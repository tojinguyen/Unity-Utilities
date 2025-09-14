using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Examples
{
    /// <summary>
    /// Simple scene loader script để đặt trên các GameObject trong scene.
    /// Cung cấp interface đơn giản để load scene từ UI buttons hoặc triggers.
    /// </summary>
    public class SimpleSceneLoader : MonoBehaviour
    {
        [Header("Scene Loading Settings")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private int targetSceneBuildIndex = -1;
        [SerializeField] private bool useSceneName = true;
        
        [Header("Loading Options")]
        [SerializeField] private bool useStandardTransition = true;
        [SerializeField] private bool showLoadingUI = true;
        [SerializeField] private float minimumLoadingTime = 1f;
        
        [Header("Custom Steps")]
        [SerializeField] private bool includeSystemInit = true;
        [SerializeField] private bool includeAssetPreload = false;
        [SerializeField] private string[] customSystemNames = { "Audio", "Input", "UI" };
        
        /// <summary>
        /// Load scene được chỉ định trong inspector
        /// </summary>
        public async void LoadTargetScene()
        {
            if (useSceneName)
            {
                if (string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.LogWarning("Target scene name is not set!");
                    return;
                }
                await LoadSceneByName(targetSceneName);
            }
            else
            {
                if (targetSceneBuildIndex < 0)
                {
                    Debug.LogWarning("Target scene build index is not set!");
                    return;
                }
                await LoadSceneByIndex(targetSceneBuildIndex);
            }
        }
        
        /// <summary>
        /// Load scene theo tên
        /// </summary>
        /// <param name="sceneName">Tên scene cần load</param>
        public async UniTask LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Scene name is empty!");
                return;
            }
            
            var steps = useStandardTransition ? 
                LoadingStepFactory.CreateStandardSceneTransition(sceneName, includeSystemInit, includeAssetPreload) :
                LoadingStepFactory.CreateSimpleSceneLoad(sceneName, minimumLoadingTime);
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showLoadingUI);
        }
        
        /// <summary>
        /// Load scene theo build index
        /// </summary>
        /// <param name="sceneIndex">Build index của scene</param>
        public async UniTask LoadSceneByIndex(int sceneIndex)
        {
            if (sceneIndex < 0)
            {
                Debug.LogWarning("Invalid scene build index!");
                return;
            }
            
            var steps = new System.Collections.Generic.List<ILoadingStep>();
            
            if (useStandardTransition)
            {
                steps.Add(LoadingStepFactory.CreateDelay(0.5f, "Preparing", "Preparing to load scene..."));
                
                if (includeSystemInit)
                {
                    steps.Add(LoadingStepFactory.CreateSystemInit(customSystemNames, 0.3f));
                }
                
                if (includeAssetPreload)
                {
                    steps.Add(LoadingStepFactory.CreateDelay(1f, "Preloading", "Preloading assets..."));
                }
            }
            else if (minimumLoadingTime > 0f)
            {
                steps.Add(LoadingStepFactory.CreateDelay(minimumLoadingTime * 0.3f, "Preparing", "Preparing..."));
            }
            
            steps.Add(LoadingStepFactory.CreateSceneLoad(sceneIndex));
            
            if (!useStandardTransition && minimumLoadingTime > 0f)
            {
                steps.Add(LoadingStepFactory.CreateDelay(minimumLoadingTime * 0.2f, "Finishing", "Finishing..."));
            }
            else if (useStandardTransition)
            {
                steps.Add(LoadingStepFactory.CreateDelay(0.3f, "Finalizing", "Finalizing..."));
            }
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showLoadingUI);
        }
        
        /// <summary>
        /// Restart game về main menu
        /// </summary>
        public async void RestartToMainMenu()
        {
            await LoadSceneByName("MainMenu");
        }
        
        /// <summary>
        /// Restart game về scene đầu tiên (build index 0)
        /// </summary>
        public async void RestartToFirstScene()
        {
            await LoadSceneByIndex(0);
        }
        
        /// <summary>
        /// Load scene tiếp theo trong build settings
        /// </summary>
        public async void LoadNextScene()
        {
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            
            if (nextSceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
            {
                await LoadSceneByIndex(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("No next scene available!");
            }
        }
        
        /// <summary>
        /// Load scene trước đó trong build settings
        /// </summary>
        public async void LoadPreviousScene()
        {
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            int previousSceneIndex = currentSceneIndex - 1;
            
            if (previousSceneIndex >= 0)
            {
                await LoadSceneByIndex(previousSceneIndex);
            }
            else
            {
                Debug.LogWarning("No previous scene available!");
            }
        }
        
        /// <summary>
        /// Reload scene hiện tại
        /// </summary>
        public async void ReloadCurrentScene()
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            await LoadSceneByName(currentSceneName);
        }
        
        #region Inspector Context Menu
        
        [ContextMenu("Load Target Scene")]
        private void LoadTargetSceneContext()
        {
            LoadTargetScene();
        }
        
        [ContextMenu("Reload Current Scene")]
        private void ReloadCurrentSceneContext()
        {
            ReloadCurrentScene();
        }
        
        #endregion
    }
}