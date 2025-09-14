using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Scripts.UIManager.AnimationTransition;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class EView : UIView
    {
        [Header("Animation Transition")]
        [SerializeField] protected AnimationContainerEView animationTransition;
        
        public virtual async UniTask OnShow()
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.ShowAnimation)
                await animationTransition.ShowAnimation.Execute(ViewCanvasGroup);
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }
        
        public virtual async UniTask OnShow(Vector2 position, bool isLocalPosition = false)
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.ShowAnimation)
                await animationTransition.ShowAnimation.Execute(ViewCanvasGroup, position, isLocalPosition);
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }

            await UniTask.CompletedTask;
        }
        
        public virtual async UniTask OnHide()
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.HideAnimation)
                await animationTransition.HideAnimation.Execute(ViewCanvasGroup);
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnHide(Vector2 position, bool isLocalPosition = false)
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.HideAnimation)
                await animationTransition.HideAnimation.Execute(ViewCanvasGroup, position, isLocalPosition);
            else
            {
                transform.localScale = Vector3.zero;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }
    }
}