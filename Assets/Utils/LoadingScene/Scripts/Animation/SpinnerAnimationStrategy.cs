using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.LoadingScene
{
    [AddComponentMenu("TirexGame/Loading/Animation/Spinner Animation")]
    public class SpinnerAnimationStrategy : MonoBehaviour, ILoadingAnimationStrategy
    {
        [Header("Spinner Settings")]
        [SerializeField] private Transform _spinnerTarget;
        [SerializeField] private float _rotationSpeed = -360f; // âm = ngược chiều kim đồng hồ
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Show/Hide Settings")]
        [SerializeField] [Range(0.05f, 2f)] private float _showDuration = 0.3f;
        [SerializeField] [Range(0.05f, 2f)] private float _hideDuration = 0.2f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private bool _isSpinning;
        private CancellationTokenSource _spinCts;

        private void OnDisable()
        {
            StopSpinner();
        }

        public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 0f;
            target.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _showDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                canvasGroup.alpha = _fadeCurve.Evaluate(Mathf.Clamp01(elapsed / _showDuration));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
                canvasGroup.alpha = 1f;
        }

        public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
        {
            StopSpinner();

            var canvasGroup = GetOrAddCanvasGroup(target);
            float startAlpha = canvasGroup.alpha;

            float elapsed = 0f;
            while (elapsed < _hideDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _hideDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                canvasGroup.alpha = 0f;
                target.SetActive(false);
            }
        }

        public void PlayIdleAnimation(GameObject target)
        {
            StartSpinner();
        }

        public void StopIdleAnimation(GameObject target)
        {
            StopSpinner();
        }

        private void StartSpinner()
        {
            if (_isSpinning) return;
            _isSpinning = true;
            _spinCts = new CancellationTokenSource();
            SpinLoop(_spinCts.Token).Forget();
        }

        private void StopSpinner()
        {
            _isSpinning = false;
            _spinCts?.Cancel();
            _spinCts?.Dispose();
            _spinCts = null;
        }

        private async UniTaskVoid SpinLoop(CancellationToken ct)
        {
            Transform target = _spinnerTarget != null ? _spinnerTarget : transform;

            while (!ct.IsCancellationRequested)
            {
                float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                target.Rotate(0f, 0f, _rotationSpeed * dt);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
