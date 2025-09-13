using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : MonoBehaviour
    {
        protected CanvasGroup ViewCanvasGroup;
        public bool IsInitialized;

        protected virtual void Awake()
        {
            IsInitialized = false;
            if (ViewCanvasGroup == null)
                ViewCanvasGroup = this.GetComponent<CanvasGroup>();
            ViewCanvasGroup.alpha = 0;
        }

        protected virtual void Start()
        {
            Initialize().Forget();
        }

        protected async virtual UniTask Initialize()
        {
            IsInitialized = true;
            await UniTask.CompletedTask;
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (ViewCanvasGroup == null)
                ViewCanvasGroup = this.GetComponent<CanvasGroup>();
        }
#endif
    }
}