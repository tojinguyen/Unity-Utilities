using UnityEngine;
using UnityEngine.UI;
using TirexGame.Utils.Vibration;

namespace TirexGame.Utils.Vibration.Examples
{
    /// <summary>
    /// Example demonstrating haptic feedback usage
    /// </summary>
    public class HapticFeedbackExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button selectionButton;
        [SerializeField] private Button impactButton;
        [SerializeField] private Button notificationButton;
        [SerializeField] private Button successButton;
        [SerializeField] private Button warningButton;
        [SerializeField] private Button errorButton;
        [SerializeField] private Text feedbackText;

        [Header("Settings")]
        [SerializeField] private bool showFeedbackText = true;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (selectionButton != null)
                selectionButton.onClick.AddListener(OnSelectionFeedback);

            if (impactButton != null)
                impactButton.onClick.AddListener(OnImpactFeedback);

            if (notificationButton != null)
                notificationButton.onClick.AddListener(OnNotificationFeedback);

            if (successButton != null)
                successButton.onClick.AddListener(OnSuccessFeedback);

            if (warningButton != null)
                warningButton.onClick.AddListener(OnWarningFeedback);

            if (errorButton != null)
                errorButton.onClick.AddListener(OnErrorFeedback);
        }

        private void OnSelectionFeedback()
        {
            VibrationManager.TriggerHapticFeedback(HapticFeedbackType.Selection);
            ShowFeedback("Selection Haptic Feedback");
        }

        private void OnImpactFeedback()
        {
            VibrationManager.TriggerHapticFeedback(HapticFeedbackType.Impact);
            ShowFeedback("Impact Haptic Feedback");
        }

        private void OnNotificationFeedback()
        {
            VibrationManager.TriggerHapticFeedback(HapticFeedbackType.Notification);
            ShowFeedback("Notification Haptic Feedback");
        }

        private void OnSuccessFeedback()
        {
            VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Success);
            ShowFeedback("Success Notification Feedback");
        }

        private void OnWarningFeedback()
        {
            VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Warning);
            ShowFeedback("Warning Notification Feedback");
        }

        private void OnErrorFeedback()
        {
            VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Error);
            ShowFeedback("Error Notification Feedback");
        }

        private void ShowFeedback(string message)
        {
            if (showFeedbackText && feedbackText != null)
            {
                feedbackText.text = message;
                // Clear the text after 2 seconds
                Invoke(nameof(ClearFeedbackText), 2f);
            }
            Debug.Log($"Haptic Example: {message}");
        }

        private void ClearFeedbackText()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }

        // Public methods for different impact types
        public void TriggerLightImpact()
        {
            VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Light);
            ShowFeedback("Light Impact Feedback");
        }

        public void TriggerMediumImpact()
        {
            VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Medium);
            ShowFeedback("Medium Impact Feedback");
        }

        public void TriggerHeavyImpact()
        {
            VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Heavy);
            ShowFeedback("Heavy Impact Feedback");
        }
    }
}