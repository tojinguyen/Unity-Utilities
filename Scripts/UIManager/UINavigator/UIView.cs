using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : MonoBehaviour
    {
        protected CanvasGroup ViewCanvasGroup;
        protected bool IsInitialized;

        protected void Awake()
        {
            IsInitialized = false;
            if (ViewCanvasGroup == null)
                ViewCanvasGroup = this.GetComponent<CanvasGroup>();
            ViewCanvasGroup.alpha = 0;
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
            ViewCanvasGroup.alpha = 0;
            IsInitialized = false;
        }
#endif
    }
}