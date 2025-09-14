using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Utils.Scripts.UIManager.AnimationTransition
{
    [CreateAssetMenu(fileName = "New Animation Transition", menuName = "TirexGame/Core/UI Manager/Animation Transition")]
    public class AnimationTransition : ScriptableObject
    {
        [SerializeField] protected float duration = .15f;

        [Header("Before Properties")] [SerializeField]
        protected float alphaBefore = 0f;

        [SerializeField] protected Vector3 scaleBefore;
        [SerializeField] protected PositionTransitionEnum positionBefore;
        [SerializeField] protected Vector3 offsetBefore;

        [Header("After Properties")] [SerializeField]
        protected float alphaAfter = 1f;

        [SerializeField] protected Vector3 scaleAfter;
        [SerializeField] protected PositionTransitionEnum positionAfter;
        [SerializeField] protected Vector3 offsetAfter;

        [Header("Ease")] [SerializeField] protected Ease ease;

        public virtual async UniTask Execute(CanvasGroup target, RectTransform container = null)
        {
            var rectContainer = container ?? (RectTransform)target.transform.parent;
            // Set initial state
            SetInitialState(target, rectContainer);

            // Start parallel animations
            AnimateFade(target);
            AnimateScale(target);
    
            // Wait for position animation to complete
            await AnimatePosition(target, rectContainer);
        }

        public async UniTask Execute(CanvasGroup target, Vector2 position, bool isLocalPosition = false)
        {
            // Set initial state with custom position
            SetInitialState(target, position, isLocalPosition);

            // Start parallel animations (no position animation)
            AnimateFade(target);
    
            // Wait for scale animation to complete
            await AnimateScale(target).AsyncWaitForCompletion();
        }

        private Vector3 GetPosition(PositionTransitionEnum position, RectTransform target, RectTransform container, Vector3 offset)
        {
            var targetSize = target.rect;
            var containerSize = container.rect;

            var calculatedPosition = position switch
            {
                PositionTransitionEnum.Center => Vector3.zero,
                PositionTransitionEnum.Top => new Vector3(0, (targetSize.height + containerSize.height) / 2, 0),
                PositionTransitionEnum.Bottom => new Vector3(0, -(targetSize.height + containerSize.height) / 2, 0),
                PositionTransitionEnum.Left => new Vector3(-(targetSize.width + containerSize.width) / 2, 0, 0),
                PositionTransitionEnum.Right => new Vector3((targetSize.width + containerSize.width) / 2, 0, 0),
                _ => Vector3.zero
            };

            return calculatedPosition + offset;
        }

        private void SetInitialState(CanvasGroup target, RectTransform container)
        {
            target.alpha = alphaBefore;
            target.transform.localScale = scaleBefore;
            target.transform.localPosition = GetPosition(positionBefore, (RectTransform)target.transform, container, offsetBefore);
        }

        private void SetInitialState(CanvasGroup target, Vector2 position, bool isLocalPosition)
        {
            target.alpha = alphaBefore;
            target.transform.localScale = scaleBefore;

            if (isLocalPosition)
                target.transform.localPosition = position;
            else
                target.transform.position = position;
        }

        private Tween AnimateFade(CanvasGroup target)
        {
            return target.DOFade(alphaAfter, duration).SetEase(ease);
        }

        private Tween AnimateScale(CanvasGroup target)
        {
            return target.transform.DOScale(scaleAfter, duration).SetEase(ease);
        }

        private async UniTask AnimatePosition(CanvasGroup target, RectTransform container)
        {
            var endPosition = GetPosition(positionAfter, (RectTransform)target.transform, container, offsetAfter);
            await target.transform.DOLocalMove(endPosition, duration).SetEase(ease).AsyncWaitForCompletion();
        }

        protected enum PositionTransitionEnum
        {
            Center,
            Top,
            Bottom,
            Left,
            Right,
        }
    }
}