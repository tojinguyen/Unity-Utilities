using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Scripts.UIManager.UINavigator.Popup.Modal
{
    public class Modal : EView
    {
        [SerializeField] protected Button closeBtn;
        
        protected virtual void Start()
        {
            if (closeBtn)
                closeBtn.onClick.AddListener(OnClose);
        }
        
        public async UniTask Init()
        {
            await UniTask.NextFrame();
            await OnShow();
        }
        
        protected virtual void OnClose()
        {
            ModalContainer.Pop().Forget();
        }
    }
}