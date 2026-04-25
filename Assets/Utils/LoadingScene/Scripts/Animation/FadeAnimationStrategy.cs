using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Animation strategy dùng CanvasGroup để fade in/out Loading UI.
    /// Đây là animation mặc định, đơn giản và phổ biến nhất.
    /// </summary>
    [AddComponentMenu("TirexGame/Loading/Animation/Fade Animation")]
    public class FadeAnimationStrategy : MonoBehaviour, ILoadingAnimationStrategy
    {
        [Header("Fade Settings")]
        [SerializeField] [Range(0.05f, 2f)] private float _showDuration = 0.3f;
        [SerializeField] [Range(0.05f, 2f)] private float _hideDuration = 0.3f;
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);
            canvasGroup.alpha = 0f;
            target.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _showDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _showDuration);
                canvasGroup.alpha = _showCurve.Evaluate(t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
                canvasGroup.alpha = 1f;
        }

        public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
        {
            var canvasGroup = GetOrAddCanvasGroup(target);

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < _hideDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _hideDuration);
                canvasGroup.alpha = startAlpha * _hideCurve.Evaluate(t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                canvasGroup.alpha = 0f;
                target.SetActive(false);
            }
        }

        public void PlayIdleAnimation(GameObject target) { }
        public void StopIdleAnimation(GameObject target) { }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = target.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
