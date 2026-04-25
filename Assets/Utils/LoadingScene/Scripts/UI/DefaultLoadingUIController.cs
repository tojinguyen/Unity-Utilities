using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Default implementation của ILoadingUIController.
    /// Dùng Strategy Pattern (ILoadingAnimationStrategy) để tùy chỉnh animation.
    /// 
    /// Cách dùng custom animation:
    /// 1. Tạo MonoBehaviour implement ILoadingAnimationStrategy trong project của bạn
    /// 2. Add component đó lên cùng GameObject hoặc kéo vào field _animationStrategy
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

        [Header("Animation Strategy")]
        [Tooltip("Kéo component ILoadingAnimationStrategy vào đây để custom animation.\n" +
                 "Nếu để trống, sẽ tự động tìm component trên cùng GameObject.\n" +
                 "Nếu không có component nào, dùng fallback instant show/hide.")]
        [SerializeField] private MonoBehaviour _animationStrategyObject;

        [Header("Progress Bar Settings")]
        [SerializeField] private bool _showProgressPercentage = true;
        [SerializeField] private bool _showProgressInfo = true;
        [SerializeField] private bool _smoothProgressBar = true;
        [SerializeField] [Range(0.5f, 10f)] private float _progressAnimationSpeed = 2f;

        // ---- Runtime ----
        private ILoadingAnimationStrategy _animationStrategy;
        private CancellationTokenSource _animCts;
        private bool _isVisible;
        private float _targetProgress;
        private float _currentProgress;
        private Coroutine _progressCoroutine;

        #region ILoadingUIController

        public GameObject UIGameObject => gameObject;
        public bool IsVisible => _isVisible;
        public event Action OnCancelRequested;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ResolveAnimationStrategy();
            InitializeComponents();
            SetupEventHandlers();

            // Bắt đầu ở trạng thái ẩn ngay lập tức (không animation)
            ForceHide();
        }

        private void Start()
        {
            if (_errorPanel != null)
                _errorPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region Public Methods (ILoadingUIController)

        public void ShowUI()
        {
            if (_isVisible) return;
            _isVisible = true;
            HideError();
            ShowAsync().Forget();
        }

        public void HideUI()
        {
            if (!_isVisible) return;
            _isVisible = false;
            HideAsync().Forget();
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
                    progressInfo += $" | {progressData.ElapsedTime:mm\\:ss}";

                if (progressData.EstimatedRemainingTime.TotalSeconds > 0)
                    progressInfo += $" | ETA: {progressData.EstimatedRemainingTime:mm\\:ss}";

                _progressInfoText.text = progressInfo;
            }
        }

        public void UpdateStepText(string stepName, string description)
        {
            if (_stepNameText != null)
                _stepNameText.text = stepName ?? string.Empty;

            if (_stepDescriptionText != null)
                _stepDescriptionText.text = description ?? string.Empty;
        }

        public void UpdateProgressBar(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);

            if (_smoothProgressBar)
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
            CancelCurrentAnimation();

            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }

            if (_animationStrategy != null)
                _animationStrategy.StopIdleAnimation(GetAnimationTarget());

            OnCancelRequested = null;
        }

        #endregion

        #region Private — Animation

        private async UniTaskVoid ShowAsync()
        {
            CancelCurrentAnimation();
            _animCts = new CancellationTokenSource();

            var target = GetAnimationTarget();
            target.SetActive(true);

            if (_animationStrategy != null)
            {
                try
                {
                    await _animationStrategy.PlayShowAnimation(target, _animCts.Token);
                    if (!_animCts.IsCancellationRequested)
                        _animationStrategy.PlayIdleAnimation(target);
                }
                catch (OperationCanceledException) { }
            }
        }

        private async UniTaskVoid HideAsync()
        {
            if (_animationStrategy != null)
                _animationStrategy.StopIdleAnimation(GetAnimationTarget());

            CancelCurrentAnimation();
            _animCts = new CancellationTokenSource();

            var target = GetAnimationTarget();

            if (_animationStrategy != null)
            {
                try
                {
                    await _animationStrategy.PlayHideAnimation(target, _animCts.Token);
                }
                catch (OperationCanceledException) { }
            }
            else
            {
                target.SetActive(false);
            }
        }

        private void ForceHide()
        {
            var target = GetAnimationTarget();
            if (target != null)
                target.SetActive(false);
        }

        private void CancelCurrentAnimation()
        {
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
        }

        private GameObject GetAnimationTarget()
        {
            return _loadingPanel != null ? _loadingPanel : gameObject;
        }

        #endregion

        #region Private — Initialization

        /// <summary>
        /// Tìm ILoadingAnimationStrategy từ field được assign hoặc auto-detect trên GameObject.
        /// </summary>
        private void ResolveAnimationStrategy()
        {
            // 1. Dùng field được assign trong Inspector
            if (_animationStrategyObject != null)
            {
                _animationStrategy = _animationStrategyObject as ILoadingAnimationStrategy;
                if (_animationStrategy == null)
                    ConsoleLogger.LogWarning("[DefaultLoadingUIController] _animationStrategyObject does not implement ILoadingAnimationStrategy!");
                return;
            }

            // 2. Auto-detect trên cùng GameObject
            _animationStrategy = GetComponent<ILoadingAnimationStrategy>();

            // 3. Fallback: null → instant show/hide (không animation)
            if (_animationStrategy == null)
                ConsoleLogger.Log("[DefaultLoadingUIController] No animation strategy found. Using instant show/hide.");
        }

        private void InitializeComponents()
        {
            // Auto-find nếu chưa assign
            if (_loadingPanel == null)
                _loadingPanel = transform.Find("LoadingPanel")?.gameObject;

            if (_progressBar == null)
                _progressBar = GetComponentInChildren<Slider>();

            if (_stepNameText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0)
                    _stepNameText = texts[0];
            }

            if (_cancelButton == null)
                _cancelButton = GetComponentInChildren<Button>();
        }

        private void SetupEventHandlers()
        {
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
            }

            if (_errorCloseButton != null)
            {
                _errorCloseButton.onClick.RemoveAllListeners();
                _errorCloseButton.onClick.AddListener(HideError);
            }
        }

        #endregion

        #region Private — Progress Bar

        private void SetProgressBarValue(float value)
        {
            _currentProgress = value;

            if (_progressBar != null)
                _progressBar.value = _currentProgress;

            if (_showProgressPercentage && _progressPercentText != null)
                _progressPercentText.text = $"{_currentProgress * 100:F0}%";
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

        #region Static Factory

        public static DefaultLoadingUIController CreateFromPrefab(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[DefaultLoadingUIController] Prefab is null!");
                return null;
            }

            GameObject instance = Instantiate(prefab, parent);
            var controller = instance.GetComponent<DefaultLoadingUIController>();
            if (controller == null)
                controller = instance.AddComponent<DefaultLoadingUIController>();

            return controller;
        }

        #endregion
    }
}