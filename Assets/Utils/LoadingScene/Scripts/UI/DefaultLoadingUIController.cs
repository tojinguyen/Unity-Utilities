using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TirexGame.Utils.LoadingScene
{
    public class DefaultLoadingUIController : MonoBehaviour, ILoadingUI
    {
        [Header("UI References")]
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressPercentText;

        [Header("Animation Strategy")]
        [Tooltip("Kéo component ILoadingAnimationStrategy vào đây để custom animation.\n" +
                 "Nếu để trống, tự tìm trên cùng GameObject.\n" +
                 "Nếu không có, dùng instant show/hide.")]
        [SerializeField] private MonoBehaviour _animationStrategyObject;

        private ILoadingAnimationStrategy _animationStrategy;
        private CancellationTokenSource _animCts;
        private bool _isVisible;

        public bool IsVisible => _isVisible;

        private void Awake()
        {
            ResolveAnimationStrategy();
            AutoFindComponents();
            ForceHide();
        }

        private void OnDestroy()
        {
            CancelAnimation();
            _animationStrategy?.StopIdleAnimation(GetTarget());
        }

        public void ShowUI()
        {
            if (_isVisible) return;
            _isVisible = true;
            ShowAsync().Forget();
        }

        public void HideUI()
        {
            if (!_isVisible) return;
            _isVisible = false;
            HideAsync().Forget();
        }

        public void SetProgress(float progress)
        {
            float v = Mathf.Clamp01(progress);
            if (_progressBar != null) _progressBar.value = v;
            if (_progressPercentText != null) _progressPercentText.text = $"{v * 100:F0}%";
        }

        private async UniTaskVoid ShowAsync()
        {
            CancelAnimation();
            _animCts = new CancellationTokenSource();

            var target = GetTarget();
            target.SetActive(true);

            if (_animationStrategy != null)
            {
                try
                {
                    await _animationStrategy.PlayShowAnimation(target, _animCts.Token);
                    if (!_animCts.IsCancellationRequested)
                        _animationStrategy.PlayIdleAnimation(target);
                }
                catch (System.OperationCanceledException) { }
            }
        }

        private async UniTaskVoid HideAsync()
        {
            _animationStrategy?.StopIdleAnimation(GetTarget());
            CancelAnimation();
            _animCts = new CancellationTokenSource();

            var target = GetTarget();

            if (_animationStrategy != null)
            {
                try { await _animationStrategy.PlayHideAnimation(target, _animCts.Token); }
                catch (System.OperationCanceledException) { }
            }
            else
            {
                target.SetActive(false);
            }
        }

        private void ForceHide()
        {
            var t = GetTarget();
            if (t != null) t.SetActive(false);
        }

        private void CancelAnimation()
        {
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
        }

        private GameObject GetTarget() => _loadingPanel != null ? _loadingPanel : gameObject;

        private void ResolveAnimationStrategy()
        {
            if (_animationStrategyObject != null)
            {
                _animationStrategy = _animationStrategyObject as ILoadingAnimationStrategy;
                return;
            }
            _animationStrategy = GetComponent<ILoadingAnimationStrategy>();
        }

        private void AutoFindComponents()
        {
            if (_loadingPanel == null)
                _loadingPanel = transform.Find("LoadingPanel")?.gameObject;

            if (_progressBar == null)
                _progressBar = GetComponentInChildren<Slider>();

            if (_progressPercentText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) _progressPercentText = texts[0];
            }
        }

        public static DefaultLoadingUIController CreateFromPrefab(GameObject prefab, Transform parent = null)
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, parent);
            return go.GetComponent<DefaultLoadingUIController>() ?? go.AddComponent<DefaultLoadingUIController>();
        }
    }
}
