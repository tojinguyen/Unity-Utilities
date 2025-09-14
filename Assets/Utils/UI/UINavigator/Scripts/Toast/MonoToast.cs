using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utils.Scripts.UIManager.UINavigator.Toast
{
    public class MonoToast : UIContainer
    {
        private static Toast _currentToast = null;
        private static CancellationTokenSource _cancelToken;
        private static bool _isCurrentToastShowed = false;
        private static UIContainer _container;
        
        protected override void Awake()
        {
            base.Awake();
            _container = this;
        }
        
        public static async UniTask ShowToast(
            string keyAddressable,
            string content,
            float timeShow,
            Transform parent = null
        )
        {
            if (_currentToast)
            {
                if (!_isCurrentToastShowed) return;
                _cancelToken?.Cancel();
                await _currentToast.OnHide();
                Addressables.ReleaseInstance(_currentToast.gameObject);
                _currentToast = null;
            }
            _isCurrentToastShowed = false;
            _cancelToken = new CancellationTokenSource();
            var parentSpawn = parent ? parent : _container.TransformContainer;
            var toastPrefab = await AddressableHelper.GetAssetAsync<GameObject>(keyAddressable);
            if (toastPrefab is null)
            {
                Debug.LogError($"[Toast] Can't load toast prefab with key addressable: {keyAddressable}");
                return;
            }
            var toastSpawn = Instantiate(toastPrefab, parentSpawn);

            var toast = toastSpawn.GetComponent<Toast>();
            if (toast is null) 
                return;
            _currentToast = toast;
            _currentToast.Init(content);
            await _currentToast.OnShow();
            _isCurrentToastShowed = true;
            await UniTask.Delay((int)(timeShow * 1000), cancellationToken: _cancelToken.Token);
            await _currentToast.OnHide();

            if (_currentToast)
            {
                Addressables.ReleaseInstance(_currentToast.gameObject);
                _currentToast = null;
            }
        }
    }
}