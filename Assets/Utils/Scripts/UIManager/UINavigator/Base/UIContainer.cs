using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator
{
    public class UIContainer : MonoBehaviour
    {
        private Transform _transformContainer;
        private RectTransform _rectContainer;

        public Transform TransformContainer => _transformContainer;
        public RectTransform RectContainer => _rectContainer;

        protected virtual void Awake()
        {
            _transformContainer = transform;
            _rectContainer = (RectTransform)transform;
        }

        internal static async UniTask<GameObject> SpawnUIGo(string key, Transform parent)
        {
            var res = await AddressableHelper.GetAssetAsync<GameObject>(key);
            return Instantiate(res, parent);
        }
    }
}