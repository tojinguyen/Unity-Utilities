using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class Screen : ScreenView
    {
        [SerializeField] protected Button[] buttonsBack;
        
        protected override void Awake()
        {
            base.Awake();
            if (buttonsBack != null)
            {
                foreach (var item in buttonsBack)
                    item.onClick.AddListener(OnBack);
            }
        }
        
        protected virtual void OnBack()
        {
            ScreenContainer.Pop().Forget();
        }
    }
}