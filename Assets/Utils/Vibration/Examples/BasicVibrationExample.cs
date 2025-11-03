using UnityEngine;
using UnityEngine.UI;
using TirexGame.Utils.Vibration;

namespace TirexGame.Utils.Vibration.Examples
{
    /// <summary>
    /// Example demonstrating basic vibration usage
    /// </summary>
    public class BasicVibrationExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button vibrateButton;
        [SerializeField] private Button vibrateShortButton;
        [SerializeField] private Button vibrateLongButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text statusText;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;

        private void Start()
        {
            SetupButtons();
            UpdateStatusText();
        }

        private void SetupButtons()
        {
            if (vibrateButton != null)
                vibrateButton.onClick.AddListener(OnVibrateClicked);

            if (vibrateShortButton != null)
                vibrateShortButton.onClick.AddListener(OnVibrateShortClicked);

            if (vibrateLongButton != null)
                vibrateLongButton.onClick.AddListener(OnVibrateLongClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void UpdateStatusText()
        {
            if (statusText != null)
            {
                string status = $"Platform Support: {(VibrationManager.IsSupported ? "Yes" : "No")}\\n";
                status += $"Device Vibration: {(VibrationManager.IsEnabled ? "Enabled" : "Disabled")}";
                statusText.text = status;
            }
        }

        private void OnVibrateClicked()
        {
            LogAction("Default Vibrate");
            VibrationManager.Vibrate();
        }

        private void OnVibrateShortClicked()
        {
            LogAction("Short Vibrate (50ms)");
            VibrationManager.Vibrate(50);
        }

        private void OnVibrateLongClicked()
        {
            LogAction("Long Vibrate (500ms)");
            VibrationManager.Vibrate(500);
        }

        private void OnCancelClicked()
        {
            LogAction("Cancel Vibration");
            VibrationManager.Cancel();
        }

        private void LogAction(string action)
        {
            if (enableDebugLog)
            {
                Debug.Log($"Vibration Example: {action}");
            }
        }

        // Public methods for UnityEvents
        public void VibrateLight()
        {
            LogAction("Light Vibration");
            VibrationManager.Vibrate(VibrationIntensity.Light);
        }

        public void VibrateMedium()
        {
            LogAction("Medium Vibration");
            VibrationManager.Vibrate(VibrationIntensity.Medium);
        }

        public void VibrateHeavy()
        {
            LogAction("Heavy Vibration");
            VibrationManager.Vibrate(VibrationIntensity.Heavy);
        }
    }
}