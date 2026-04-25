using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Animation strategy dùng RectTransform để slide Loading UI vào/ra từ các hướng.
    /// </summary>
    [AddComponentMenu("TirexGame/Loading/Animation/Slide Animation")]
    public class SlideAnimationStrategy : MonoBehaviour, ILoadingAnimationStrategy
    {
        public enum SlideDirection
        {
            FromBottom,
            FromTop,
            FromLeft,
            FromRight
        }

        [Header("Slide Settings")]
        [SerializeField] private SlideDirection _showFromDirection = SlideDirection.FromBottom;
        [SerializeField] [Range(0.05f, 2f)] private float _showDuration = 0.4f;
        [SerializeField] [Range(0.05f, 2f)] private float _hideDuration = 0.3f;
        [SerializeField] private AnimationCurve _showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float _slideOffset = 300f;

        public async UniTask PlayShowAnimation(GameObject target, CancellationToken ct)
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                target.SetActive(true);
                return;
            }

            Vector2 startPos = GetOffscreenPosition(_showFromDirection, _slideOffset);
            Vector2 endPos = Vector2.zero;

            rect.anchoredPosition = startPos;
            target.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _showDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = _showCurve.Evaluate(Mathf.Clamp01(elapsed / _showDuration));
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
                rect.anchoredPosition = endPos;
        }

        public async UniTask PlayHideAnimation(GameObject target, CancellationToken ct)
        {
            var rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                target.SetActive(false);
                return;
            }

            // Hide vào hướng ngược lại với lúc show
            SlideDirection hideDirection = GetOppositeDirection(_showFromDirection);
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = GetOffscreenPosition(hideDirection, _slideOffset);

            float elapsed = 0f;
            while (elapsed < _hideDuration)
            {
                if (ct.IsCancellationRequested) break;
                elapsed += Time.unscaledDeltaTime;
                float t = _hideCurve.Evaluate(Mathf.Clamp01(elapsed / _hideDuration));
                rect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, 1f - t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                rect.anchoredPosition = endPos;
                target.SetActive(false);
            }
        }

        public void PlayIdleAnimation(GameObject target) { }
        public void StopIdleAnimation(GameObject target) { }

        private static Vector2 GetOffscreenPosition(SlideDirection direction, float offset)
        {
            return direction switch
            {
                SlideDirection.FromBottom => new Vector2(0, -offset),
                SlideDirection.FromTop => new Vector2(0, offset),
                SlideDirection.FromLeft => new Vector2(-offset, 0),
                SlideDirection.FromRight => new Vector2(offset, 0),
                _ => new Vector2(0, -offset)
            };
        }

        private static SlideDirection GetOppositeDirection(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.FromBottom => SlideDirection.FromTop,
                SlideDirection.FromTop => SlideDirection.FromBottom,
                SlideDirection.FromLeft => SlideDirection.FromRight,
                SlideDirection.FromRight => SlideDirection.FromLeft,
                _ => SlideDirection.FromTop
            };
        }
    }
}
