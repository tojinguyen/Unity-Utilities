using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// MonoBehaviour wrapper cho LoadingManager để dễ dàng access từ inspector.
    /// Cung cấp interface đơn giản cho việc setup và test loading system.
    /// </summary>
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
                LoadingManager.Instance.AddProgressCallback(new Examples.DebugProgressCallback());
            }
        }
        
        /// <summary>
        /// Setup UI Controller từ prefab
        /// </summary>
        public void SetupUIController()
        {
            if (loadingUIPrefab != null)
            {
                var uiController = DefaultLoadingUIController.CreateFromPrefab(loadingUIPrefab);
                LoadingManager.Instance.SetUIController(uiController);
                Debug.Log("Loading UI Controller setup completed");
            }
            else if (autoSetupUI)
            {
                Debug.LogWarning("Loading UI Prefab is not assigned but autoSetupUI is enabled!");
            }
        }
        
        /// <summary>
        /// Quick test method để load scene từ inspector
        /// </summary>
        [ContextMenu("Test Load Scene")]
        public async void TestLoadScene()
        {
            var steps = LoadingStepFactory.CreateSimpleSceneLoad("TestScene", 2f);
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Test loading failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test standard scene transition
        /// </summary>
        [ContextMenu("Test Standard Transition")]
        public async void TestStandardTransition()
        {
            var steps = LoadingStepFactory.CreateStandardSceneTransition("TestScene", true, true);
            
            try
            {
                await LoadingManager.Instance.StartLoadingAsync(steps, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Test transition failed: {ex.Message}");
            }
        }
    }
}