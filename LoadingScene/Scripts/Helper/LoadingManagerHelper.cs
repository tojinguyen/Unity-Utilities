using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    public class LoadingManagerHelper : MonoBehaviour
    {
        [Header("UI Setup")]
        [SerializeField] private GameObject loadingUIPrefab;
        [SerializeField] private bool autoSetupUI = true;
        
        [Header("Default Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float minimumLoadingTime = 1f;
        
        [Header("Progress Callback")]
        [SerializeField] private bool addDebugProgressCallback = true;
        
        private void Start()
        {
            if (autoSetupUI)
            {
                SetupUIController();
            }
            
            if (addDebugProgressCallback)
            {
                LoadingManager.Instance.AddProgressCallback(new Examples.ConsoleLoggerProgressCallback());
            }
        }
        
        public void SetupUIController()
        {
            if (loadingUIPrefab != null)
            {
                var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
                LoadingManager.Instance.SetUIController(uiController);
                ConsoleLogger.Log("Loading UI Controller setup completed");
            }
            else if (autoSetupUI)
            {
                ConsoleLogger.LogWarning("Loading UI Prefab is not assigned but autoSetupUI is enabled!");
            }
        }
        
        [ContextMenu("Test Load Scene")]
        public async UniTaskVoid TestLoadScene()
        {
            var steps = LoadingStepFactory.CreateSimpleSceneLoad("TestScene", 2f);
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, true);
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"Test loading failed: {ex.Message}");
            }
        }
        
        [ContextMenu("Test Standard Transition")]
        public async UniTaskVoid TestStandardTransition()
        {
            var steps = LoadingStepFactory.CreateStandardSceneTransition("TestScene", true, true);
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, true);
            }
            catch (System.Exception ex)
            {
                ConsoleLogger.LogError($"Test transition failed: {ex.Message}");
            }
        }
    }
}