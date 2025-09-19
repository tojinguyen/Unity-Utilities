using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene.Examples
{
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

        public async UniTaskVoid LoadTargetScene()
        {
            if (useSceneName)
            {
                if (string.IsNullOrEmpty(targetSceneName))
                {
                    ConsoleLogger.LogWarning("Target scene name is not set!");
                    return;
                }
                await LoadSceneByName(targetSceneName);
            }
            else
            {
                if (targetSceneBuildIndex < 0)
                {
                    ConsoleLogger.LogWarning("Target scene build index is not set!");
                    return;
                }
                await LoadSceneByIndex(targetSceneBuildIndex);
            }
        }

        private async UniTask LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                ConsoleLogger.LogWarning("Scene name is empty!");
                return;
            }
            
            var steps = useStandardTransition ? 
                LoadingStepFactory.CreateStandardSceneTransition(sceneName, includeSystemInit, includeAssetPreload) :
                LoadingStepFactory.CreateSimpleSceneLoad(sceneName, minimumLoadingTime);
            
            await LoadingManager.Instance.StartLoadingAsync(steps, showLoadingUI);
        }

        private async UniTask LoadSceneByIndex(int sceneIndex)
        {
            if (sceneIndex < 0)
            {
                ConsoleLogger.LogWarning("Invalid scene build index!");
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

        private async UniTaskVoid ReloadCurrentScene()
        {
            var currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            await LoadSceneByName(currentSceneName);
        }
        
        #region Inspector Context Menu
        
        [ContextMenu("Load Target Scene")]
        public void LoadTargetSceneContext()
        {
            LoadTargetScene().Forget();
        }
        
        [ContextMenu("Reload Current Scene")]
        public void ReloadCurrentSceneContext()
        {
            ReloadCurrentScene().Forget();
        }
        
        #endregion
    }
}