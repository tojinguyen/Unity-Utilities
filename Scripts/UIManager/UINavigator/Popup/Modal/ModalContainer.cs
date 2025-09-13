using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utils.Scripts.UIManager.UINavigator.Popup.Modal
{
    public class ModalContainer : UIContainer
    {
        [SerializeField] private GameObject modalBlockOverlay;
        [SerializeField] private float durationBlockOverlay = .15f;
        
        private static Stack<Modal> _stackModal = new();
        private static CanvasGroup _modalBlockOverlay;
        private static float _durationBlockOverlay = .15f;
        private const string _keyModalBlockOverlay = "ModalBlockOverlay";
        internal static UIContainer Container;
        
        protected override void Awake()
        {
            base.Awake();
            _stackModal = new();
            _durationBlockOverlay = durationBlockOverlay;
            Container = GetComponent<ModalContainer>();
            var modalOverlayPrefab = modalBlockOverlay;

            if (!modalOverlayPrefab) return;
            var modalSpawn = Instantiate(modalOverlayPrefab, TransformContainer);
            _modalBlockOverlay = modalSpawn.GetComponent<CanvasGroup>();
            _modalBlockOverlay.alpha = 0;
            _modalBlockOverlay.gameObject.SetActive(false);
        }
        
        public static async UniTask<Modal> Show(string keyModal)
        {
            return await SpawnAndInitModal(keyModal);
        }

        private static async UniTask<Modal> SpawnAndInitModal(string keyModal)
        {
            _modalBlockOverlay.gameObject.SetActive(true);
            _modalBlockOverlay.DOFade(1f, _durationBlockOverlay);

            var modalSpawn = await Addressables.InstantiateAsync(keyModal, Container.TransformContainer).Task;

            var modal = modalSpawn.GetComponent<Modal>();
            modal.Init().Forget();
            _stackModal.Push(modal);
            return modal;
        }

        public static async UniTask Pop()
        {
            if (_stackModal.TryPop(out var modal))
            {
                await modal.OnHide();
                Addressables.ReleaseInstance(modal.gameObject);
 
            }

            if (_stackModal.Count <= 0)
            {
                await _modalBlockOverlay.DOFade(0f, _durationBlockOverlay).AsyncWaitForCompletion();
                _modalBlockOverlay.gameObject.SetActive(false);
            }
        }
    }
}