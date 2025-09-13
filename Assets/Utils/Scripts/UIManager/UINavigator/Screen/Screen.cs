using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class Screen : ScreenView
    {
        [SerializeField] protected Button[] btnsBack;
        protected override void Awake()
        {
            base.Awake();
            if (btnsBack != null)
            {
                foreach (var item in btnsBack)
                    item.onClick.AddListener(OnBack);
            }
        }
        
        protected virtual void OnBack()
        {
            ScreenContainer.Pop().Forget();
        }
    }
}