using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utils.Scripts.UIManager.UINavigator.Toast
{
    public class OverloadToastContainer : UIContainer
    {
        public static UIContainer Container;
        
        protected override void Awake()
        {
            base.Awake();
            Container = this;
        }
        
        public async UniTask ShowToast(
            string keyAddressable,
            string content,
            float timeShow
        )
        {
            var parent = Container.TransformContainer;
            var parentSpawn = parent ? parent : ScreenContainer.Container.TransformContainer;

            var toastSpawn = await Addressables.InstantiateAsync(keyAddressable, parentSpawn).Task;

            var toast = toastSpawn.GetComponent<Toast>();
            if (toast is null) return;
            toast.Init(content);
            await toast.OnShow();
            await UniTask.Delay((int)(timeShow * 1000));
            await toast.OnHide();

            Addressables.ReleaseInstance(toast.gameObject);
        }
    }
}