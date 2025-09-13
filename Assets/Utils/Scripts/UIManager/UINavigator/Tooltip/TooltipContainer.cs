using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Utils.Scripts.UIManager.UINavigator.Tooltip
{
    public class TooltipContainer : UIContainer
    {
        public static UIContainer Container;
        protected const string KeyTooltipBlockOverlay = "TooltipBlockOverlay";
        private static Tooltip _currentTooltip = null;
        private static CancellationTokenSource _cancelToken;
        
        protected override void Awake()
        {
            base.Awake();
            Container = this;
            _currentTooltip = null;
        }
        
        public static async UniTask Show(string keyAddressable, string content, RectTransform parent, TooltipPosition tooltipPosition, bool isCloseAnyClick = false)
        {
            await Show(keyAddressable, content, parent, tooltipPosition, Vector2.zero, isCloseAnyClick);
            await UniTask.CompletedTask;
        }

        public static async UniTask Show(string keyAddressable, string content, RectTransform parent, TooltipPosition tooltipPosition, Vector2 offset, bool isCloseAnyClick = false)
        {
            await UniTask.WaitUntil(() => _currentTooltip == null);
            var tooltipSpawn = await Addressables.InstantiateAsync(keyAddressable, parent).Task;

            var tooltip = tooltipSpawn.GetComponent<Tooltip>();
            _currentTooltip = tooltip;
            var tooltipRect = tooltipSpawn.GetComponent<RectTransform>();
            if (tooltip)
            {
                var hPos = 0f;
                var wPos = 0f;
                switch (tooltipPosition)
                {
                    case TooltipPosition.Top:
                        hPos = parent.rect.height / 2 + tooltipRect.rect.height / 2;
                        break;
                    case TooltipPosition.Bottom:
                        hPos = -parent.rect.height / 2 - tooltipRect.rect.height / 2;
                        break;
                    case TooltipPosition.Left:
                        wPos = -parent.rect.width / 2 - tooltipRect.rect.width / 2;
                        break;
                    case TooltipPosition.Right:
                        wPos = parent.rect.width / 2 + tooltipRect.rect.width / 2;
                        break;
                    case TooltipPosition.TopLeft:
                        hPos = parent.rect.height / 2 + tooltipRect.rect.height / 2;
                        wPos = -parent.rect.width / 2 - tooltipRect.rect.width / 2;
                        break;
                    case TooltipPosition.TopRight:
                        hPos = parent.rect.height / 2 + tooltipRect.rect.height / 2;
                        wPos = parent.rect.width / 2 + tooltipRect.rect.width / 2;
                        break;
                    case TooltipPosition.BottomLeft:
                        hPos = -parent.rect.height / 2 - tooltipRect.rect.height / 2;
                        wPos = -parent.rect.width / 2 - tooltipRect.rect.width / 2;
                        break;
                    case TooltipPosition.BottomRight:
                        hPos = -parent.rect.height / 2 - tooltipRect.rect.height / 2;
                        wPos = parent.rect.width / 2 + tooltipRect.rect.width / 2;
                        break;
                }
                var positionShow = new Vector2(wPos, hPos) + offset;
                tooltip.Init(content, positionShow, true).Forget();
            }
            var mousePressStream = InputSystem.onEvent.Where(_ => Input.GetMouseButtonDown(0)).Call(_ => CloseTooltipAsync().Forget());

            await UniTask.WaitUntil(() => _currentTooltip == null);
            mousePressStream.Dispose();
            await UniTask.CompletedTask;
        }

        private static async UniTaskVoid CloseTooltipAsync()
        {
            if (!_currentTooltip)
                if (!_currentTooltip.gameObject) return;
            await _currentTooltip.OnHide(_currentTooltip.transform.position);
            try
            {
                Addressables.ReleaseInstance(_currentTooltip.gameObject);
            }
            catch
            {
                Debug.LogError($"Can't release instance of {_currentTooltip.gameObject.name}");
            }
            _currentTooltip = null;
        }
    }

    public enum TooltipPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}