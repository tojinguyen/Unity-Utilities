using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Default implementation của ILoadingUIController.
    /// Sử dụng Strategy Pattern để cho phép tùy chỉnh UI.
    /// </summary>
    public class DefaultLoadingUIController : MonoBehaviour, ILoadingUIController
    {
        [Header("UI References")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _stepNameText;
        [SerializeField] private TextMeshProUGUI _stepDescriptionText;
        [SerializeField] private TextMeshProUGUI _progressPercentText;
        [SerializeField] private TextMeshProUGUI _progressInfoText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _errorPanel;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _errorCloseButton;
        
        [Header("Animation Settings")]
        [SerializeField] private bool _enableFadeAnimation = true;
        [SerializeField] private float _fadeAnimationDuration = 0.3f;
        [SerializeField] private bool _enableProgressAnimation = true;
        [SerializeField] private float _progressAnimationSpeed = 2f;
        
        [Header("Progress Bar Settings")]
        [SerializeField] private bool _showProgressPercentage = true;
        [SerializeField] private bool _showProgressInfo = true;
        [SerializeField] private bool _smoothProgressBar = true;
        
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private bool _isVisible;
        private float _targetProgress;
        private float _currentProgress;
        private Coroutine _fadeCoroutine;
        private Coroutine _progressCoroutine;
        
        #region ILoadingUIController Implementation
        
        public GameObject UIGameObject => gameObject;
        public bool IsVisible => _isVisible;
        
        public event Action OnCancelRequested;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            SetupEventHandlers();
            HideUI();
        }
        
        private void Start()
        {
            // Ensure UI is properly hidden on start
            if (_loadingPanel != null)
                _loadingPanel.SetActive(false);
                
            if (_errorPanel != null)
                _errorPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
        
        #region Public Methods
        
        public void ShowUI()
        {
            if (_isVisible) return;
            
            _isVisible = true;
            
            if (_loadingPanel != null)
                _loadingPanel.SetActive(true);
                
            HideError();
            
            if (_enableFadeAnimation && _canvasGroup != null)
            {
                if (_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeToAlpha(1f));
            }
            else if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
        
        public void HideUI()
        {
            if (!_isVisible) return;
            
            _isVisible = false;
            
            if (_enableFadeAnimation && _canvasGroup != null)
            {
                if (_fadeCoroutine != null)
                    StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeToAlpha(0f, () => {
                    if (_loadingPanel != null)
                        _loadingPanel.SetActive(false);
                }));
            }
            else
            {
                if (_canvasGroup != null)
                    _canvasGroup.alpha = 0f;
                if (_loadingPanel != null)
                    _loadingPanel.SetActive(false);
            }
        }
        
        public void UpdateProgress(LoadingProgressData progressData)
        {
            if (progressData == null) return;
            
            UpdateStepText(progressData.CurrentStepName, progressData.CurrentStepDescription);
            UpdateProgressBar(progressData.TotalProgress);
            
            if (_showProgressInfo && _progressInfoText != null)
            {
                string progressInfo = $"Step {progressData.CurrentStepIndex}/{progressData.TotalSteps}";
                
                if (progressData.ElapsedTime.TotalSeconds > 0)
                {
                    progressInfo += $" | {progressData.ElapsedTime:mm\\:ss}";
                }
                
                if (progressData.EstimatedRemainingTime.TotalSeconds > 0)
                {
                    progressInfo += $" | ETA: {progressData.EstimatedRemainingTime:mm\\:ss}";
                }
                
                _progressInfoText.text = progressInfo;
            }
        }
        
        public void UpdateStepText(string stepName, string description)
        {
            if (_stepNameText != null)
                _stepNameText.text = stepName ?? "";
                
            if (_stepDescriptionText != null)
                _stepDescriptionText.text = description ?? "";
        }
        
        public void UpdateProgressBar(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            
            if (_smoothProgressBar && _enableProgressAnimation)
            {
                if (_progressCoroutine != null)
                    StopCoroutine(_progressCoroutine);
                _progressCoroutine = StartCoroutine(AnimateProgressBar());
            }
            else
            {
                SetProgressBarValue(_targetProgress);
            }
        }
        
        public void ShowError(string errorMessage)
        {
            if (_errorPanel != null)
            {
                _errorPanel.SetActive(true);
                
                if (_errorText != null)
                    _errorText.text = errorMessage ?? "An error occurred";
            }
        }
        
        public void HideError()
        {
            if (_errorPanel != null)
                _errorPanel.SetActive(false);
        }
        
        public void SetCancelable(bool canCancel)
        {
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(canCancel);
        }
        
        public void Cleanup()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            
            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
            
            OnCancelRequested = null;
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeComponents()
        {
            // Get or add Canvas
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 1000; // High priority
            }
            
            // Get or add CanvasGroup for fading
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null && _enableFadeAnimation)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Auto-find components if not assigned
            if (_loadingPanel == null)
                _loadingPanel = transform.Find("LoadingPanel")?.gameObject;
                
            if (_progressBar == null)
                _progressBar = GetComponentInChildren<Slider>();
                
            if (_stepNameText == null)
            {
                var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
                if (textComponents.Length > 0)
                    _stepNameText = textComponents[0];
            }
            
            if (_cancelButton == null)
                _cancelButton = GetComponentInChildren<Button>();
        }
        
        private void SetupEventHandlers()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
            
            if (_errorCloseButton != null)
            {
                _errorCloseButton.onClick.RemoveAllListeners();
                _errorCloseButton.onClick.AddListener(HideError);
            }
        }
        
        private void SetProgressBarValue(float value)
        {
            _currentProgress = value;
            
            if (_progressBar != null)
                _progressBar.value = _currentProgress;
                
            if (_showProgressPercentage && _progressPercentText != null)
                _progressPercentText.text = $"{_currentProgress * 100:F0}%";
        }
        
        private void OnCancelButtonClicked()
        {
            OnCancelRequested?.Invoke();
        }
        
        #endregion
        
        #region Coroutines
        
        private System.Collections.IEnumerator FadeToAlpha(float targetAlpha, Action onComplete = null)
        {
            if (_canvasGroup == null) yield break;
            
            float startAlpha = _canvasGroup.alpha;
            float elapsedTime = 0f;
            
            while (elapsedTime < _fadeAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / _fadeAnimationDuration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            _canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }
        
        private System.Collections.IEnumerator AnimateProgressBar()
        {
            while (Mathf.Abs(_currentProgress - _targetProgress) > 0.001f)
            {
                _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress, 
                    _progressAnimationSpeed * Time.unscaledDeltaTime);
                SetProgressBarValue(_currentProgress);
                yield return null;
            }
            
            SetProgressBarValue(_targetProgress);
        }
        
        #endregion
        
        #region Static Factory Method
        
        /// <summary>
        /// Tạo UI Controller từ prefab
        /// </summary>
        /// <param name="prefab">Prefab chứa UI loading</param>
        /// <param name="parent">Parent transform (optional)</param>
        /// <returns>Instance của DefaultLoadingUIController</returns>
        public static DefaultLoadingUIController CreateFromPrefab(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("Loading UI prefab is null!");
                return null;
            }
            
            GameObject instance = Instantiate(prefab, parent);
            DefaultLoadingUIController controller = instance.GetComponent<DefaultLoadingUIController>();
            
            if (controller == null)
            {
                controller = instance.AddComponent<DefaultLoadingUIController>();
            }
            
            return controller;
        }
        
        /// <summary>
        /// Tạo UI Controller đơn giản bằng code
        /// </summary>
        /// <param name="parent">Parent transform (optional)</param>
        /// <returns>Instance của DefaultLoadingUIController</returns>
        public static DefaultLoadingUIController CreateSimpleUI(Transform parent = null)
        {
            GameObject uiObject = new GameObject("LoadingUI");
            if (parent != null)
                uiObject.transform.SetParent(parent);
                
            DefaultLoadingUIController controller = uiObject.AddComponent<DefaultLoadingUIController>();
            
            // TODO: Create simple UI elements programmatically
            // This would involve creating Canvas, Panel, Slider, Text components etc.
            
            return controller;
        }
        
        #endregion
    }
}