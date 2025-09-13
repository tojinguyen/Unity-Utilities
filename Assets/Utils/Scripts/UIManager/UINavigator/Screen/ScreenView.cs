using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Scripts.UIManager.AnimationTransition;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class ScreenView : UIView
    {
        [Header("Animation Transition")]
        [SerializeField] protected AnimationContainerCView animationTransition;
        
        public virtual async UniTask OnPushEnter()
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.PushEnterAnimation)
                await animationTransition.PushEnterAnimation.Execute(ViewCanvasGroup, ScreenContainer.Container.RectContainer);
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnPushEnter(Vector2 position, bool isLocalPosition = false)
        {
            await UniTask.WaitUntil(() => IsInitialized);
            this.gameObject.SetActive(true);
            if (animationTransition.PushEnterAnimation)
                await animationTransition.PushEnterAnimation.Execute(ViewCanvasGroup, position, isLocalPosition);
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnPushExit(Screen screen)
        {
            await UniTask.WaitUntil(() => screen.IsInitialized);
            if (animationTransition.PushExitAnimation)
                animationTransition.PushExitAnimation.Execute(ViewCanvasGroup, ScreenContainer.Container.RectContainer).Forget();
            await UniTask.CompletedTask;
            this.gameObject.SetActive(false);
        }

        public virtual async UniTask OnPopEnter()
        {
            await UniTask.WaitUntil(() => IsInitialized);
            this.gameObject.SetActive(true);
            if (animationTransition.PopEnterAnimation)
                animationTransition.PopEnterAnimation.Execute(ViewCanvasGroup, ScreenContainer.Container.RectContainer).Forget();
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }
        public virtual async UniTask OnPopEnter(Vector2 position, bool isLocalPosition = false)
        {
            await UniTask.WaitUntil(() => IsInitialized);
            this.gameObject.SetActive(true);
            if (animationTransition.PopEnterAnimation)
                await animationTransition.PopEnterAnimation.Execute(ViewCanvasGroup, position, isLocalPosition);
            else
            {
                transform.localScale = Vector3.one;
                ViewCanvasGroup.alpha = 1;
            }
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnPopExit()
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.PopExitAnimation)
                await animationTransition.PopExitAnimation.Execute(ViewCanvasGroup, ScreenContainer.Container.RectContainer);
            this.gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnPopExit(Vector2 position, bool isLocalPosition = false)
        {
            await UniTask.WaitUntil(() => IsInitialized);
            if (animationTransition.PopExitAnimation)
                await animationTransition.PopExitAnimation.Execute(ViewCanvasGroup, position, isLocalPosition);
            else
            {
                transform.localScale = Vector3.zero;
                ViewCanvasGroup.alpha = 1;
            }
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }
    }
}