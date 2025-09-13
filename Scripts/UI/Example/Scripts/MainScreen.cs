using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils.Scripts.UIManager.UINavigator;
using Utils.Scripts.UIManager.UINavigator.Alert;
using Utils.Scripts.UIManager.UINavigator.Popup.Modal;
using Utils.Scripts.UIManager.UINavigator.Toast;
using Utils.Scripts.UIManager.UINavigator.Tooltip;
using Screen = Utils.Scripts.UIManager.UINavigator.Screen;

namespace Utils.Scripts.UI.Example.Scripts
{
    public class MainScreen : Screen
    {
        [SerializeField] private Button pushBtn;
        [SerializeField] private Button alertBtn;
        [SerializeField] private Button modalBtn;
        [SerializeField] private Button toastBtn;
        [SerializeField] private Button tooltipBtn;
        [SerializeField] private Transform objectTooltipShow;

        [SerializeField] private TooltipPosition tooltipPosition;

        protected virtual void Start()
        {
            pushBtn.onClick.AddListener(Push);
            alertBtn.onClick.AddListener(Alert);
            modalBtn.onClick.AddListener(ShowModalAsync);
            toastBtn.onClick.AddListener(Toast);
            tooltipBtn.onClick.AddListener(TooltipShow);
        }


        private void Push()
        {
            ScreenContainer.Push("ScreenDefault").Forget();
        }

        private void Pop()
        {
            ScreenContainer.Pop().Forget();
        }

        private async void ShowModalAsync()
        {
            try
            {
                var modal = await ModalContainer.Show("ModalDefault");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Alert()
        {
            AlertContainer.Alert(
                "AlertDefault",
                "This is the title",
                "This is the content alert",
                CallBackAlert,
                buttonPositive: "OK", buttonNegative: "Cancel").Forget();
        }

        private void CallBackAlert(CallbackStatus callbackStatus)
        {
            if (callbackStatus == CallbackStatus.Positive)
                Alert();
        }

        private void Toast()
        {
            MonoToast.ShowToast("ToastDefault", "This is the toast", 2f).Forget();
        }

        private void TooltipShow()
        {
            TooltipContainer.Show("TooltipDefault", $"This is the tooltip {tooltipPosition.ToString()}",
                (RectTransform)objectTooltipShow, tooltipPosition).Forget();
        }
    }
}