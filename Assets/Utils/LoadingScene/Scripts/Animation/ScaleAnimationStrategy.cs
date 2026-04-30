using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    [AddComponentMenu("TirexGame/Loading/Animation/Scale Animation")]
    public class ScaleAnimationStrategy : MonoBehaviour, ILoadingAnimationStrategy
    {
        [Header("Scale Settings")]
        [SerializeField] [Range(0.05f, 2f)] private float _showDuration = 0.35f;
        [SerializeField] [Range(0.05f, 2f)] private float _hideDuration = 0.25f;
        [SerializeField] private Vector3 _startScale = new Vector3(0.85f, 0.85f, 1f);
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Fade Combination")]
        [SerializeField] private bool _combineFade = true;

        public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
        {
            var rect = target.GetComponent<RectTransform>() ?? target.transform as RectTransform;
            if (rect == null) { target.SetActive(true); return; }

            var canvasGroup = _combineFade ? GetOrAddCanvasGroup(target) : null;

            rect.localScale = _startScale;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            target.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _showDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = _showCurve.Evaluate(Mathf.Clamp01(elapsed / _showDuration));
                rect.localScale = Vector3.LerpUnclamped(_startScale, Vector3.one, t);
                if (canvasGroup != null) canvasGroup.alpha = t;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                rect.localScale = Vector3.one;
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
        }

        public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
        {
            var rect = target.GetComponent<RectTransform>() ?? target.transform as RectTransform;
            if (rect == null) { target.SetActive(false); return; }

            var canvasGroup = _combineFade ? GetOrAddCanvasGroup(target) : null;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            float elapsed = 0f;
            while (elapsed < _hideDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = _hideCurve.Evaluate(Mathf.Clamp01(elapsed / _hideDuration));
                float invT = 1f - t;
                rect.localScale = Vector3.LerpUnclamped(Vector3.one, _startScale, invT);
                if (canvasGroup != null) canvasGroup.alpha = startAlpha * t;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                rect.localScale = _startScale;
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                target.SetActive(false);
            }
        }

        public void PlayIdleAnimation(GameObject target) { }
        public void StopIdleAnimation(GameObject target) { }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
