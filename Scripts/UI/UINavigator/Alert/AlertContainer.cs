using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utils.Scripts.UIManager.UINavigator.Alert
{
    public class AlertContainer : UIContainer
    {
        [SerializeField] protected GameObject modalBlockOverlayOverride;
        [SerializeField] protected float durationBlockOverlay = .15f;
        
        private static CanvasGroup _modalBlockOverlay;
        private static float _durationBlockOverlay = .15f;
        private static UIContainer _container;
        
        protected override void Awake()
        {
            base.Awake();
            _durationBlockOverlay = durationBlockOverlay;
            _container = GetComponent<AlertContainer>();
            var modalOverlayPrefab = modalBlockOverlayOverride;
            if (!modalOverlayPrefab) 
                return;
            var modalSpawn = Instantiate(modalOverlayPrefab, TransformContainer);
            _modalBlockOverlay = modalSpawn.GetComponent<CanvasGroup>();
            _modalBlockOverlay.alpha = 0;
            _modalBlockOverlay.gameObject.SetActive(false);
        }
        
        public static async UniTaskVoid Alert(string keyModal, string title, string content, string buttonNeutral)
        {
            var res = await SpawnAndInitModal(keyModal, title, content, null, null, buttonNeutral);
        }

        public static async UniTaskVoid Alert(string keyModal, string title, string content, Action<CallbackStatus> callback, string buttonPositive = null, string buttonNegative = null, string buttonNeutral = null)
        {
            var res = await SpawnAndInitModal(keyModal, title, content, buttonPositive, buttonNegative, buttonNeutral);
            callback?.Invoke(res);
        }

        public static async UniTaskVoid Alert(string keyModal, string title, string content, Action callback = null, string buttonPositive = null, string buttonNegative = null, string buttonNeutral = null)
        {
            var res = await SpawnAndInitModal(keyModal, title, content, buttonPositive, buttonNegative, buttonNeutral);
            if (res == CallbackStatus.Positive)
                callback?.Invoke();
        }
        
        private static async UniTask<CallbackStatus> SpawnAndInitModal(string keyModal, string title, string content, string buttonPositive, string buttonNegative, string buttonNeutral)
        {
            _modalBlockOverlay.gameObject.SetActive(true);
            _modalBlockOverlay.DOFade(1f, _durationBlockOverlay);

            var modalSpawn = await Addressables.InstantiateAsync(keyModal, _container.TransformContainer).Task;
            var modal = modalSpawn.GetComponent<Alert>();
            if (modal is null) 
                return CallbackStatus.None;
            var res = await modal.Init(title, content, buttonPositive, buttonNegative, buttonNeutral);
            Addressables.ReleaseInstance(modalSpawn);
            await _modalBlockOverlay.DOFade(0f, _durationBlockOverlay).AsyncWaitForCompletion();
            _modalBlockOverlay.gameObject.SetActive(false);
            return res;
        }
    }
}