using UnityEngine;
using UnityEngine.UI;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// MonoBehaviour component for easy vibration integration in Unity scenes
    /// </summary>
    public class VibrationComponent : MonoBehaviour
    {
        [Header("Vibration Settings")]
        [SerializeField] private bool enableVibration = true;
        [SerializeField] private VibrationIntensity defaultIntensity = VibrationIntensity.Medium;

        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupButtons = true;
        [SerializeField] private HapticFeedbackType buttonFeedbackType = HapticFeedbackType.Selection;

        private void Start()
        {
            if (autoSetupButtons)
            {
                SetupButtonsInChildren();
            }
        }

        /// <summary>
        /// Automatically setup vibration for all buttons in children
        /// </summary>
        private void SetupButtonsInChildren()
        {
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                button.onClick.AddListener(() => VibrateOnButtonClick());
            }
        }

        #region Public Methods

        /// <summary>
        /// Vibrate with default settings
        /// </summary>
        public void Vibrate()
        {
            if (!enableVibration) return;
            VibrationManager.Vibrate(defaultIntensity);
        }

        /// <summary>
        /// Vibrate for button click
        /// </summary>
        public void VibrateOnButtonClick()
        {
            if (!enableVibration) return;
            VibrationManager.TriggerHapticFeedback(buttonFeedbackType);
        }

        /// <summary>
        /// Vibrate with specified intensity
        /// </summary>
        public void VibrateWithIntensity(int intensity)
        {
            if (!enableVibration) return;
            VibrationIntensity vibrationIntensity = (VibrationIntensity)Mathf.Clamp(intensity, 0, 2);
            VibrationManager.Vibrate(vibrationIntensity);
        }

        /// <summary>
        /// Vibrate for success action
        /// </summary>
        public void VibrateSuccess()
        {
            if (!enableVibration) return;
            VibrationManager.VibrateSuccess();
        }

        /// <summary>
        /// Vibrate for error action
        /// </summary>
        public void VibrateError()
        {
            if (!enableVibration) return;
            VibrationManager.VibrateError();
        }

        /// <summary>
        /// Vibrate for warning action
        /// </summary>
        public void VibrateWarning()
        {
            if (!enableVibration) return;
            VibrationManager.VibrateWarning();
        }

        /// <summary>
        /// Trigger haptic feedback
        /// </summary>
        public void TriggerHapticFeedback(int feedbackType)
        {
            if (!enableVibration) return;
            HapticFeedbackType hapticType = (HapticFeedbackType)Mathf.Clamp(feedbackType, 0, 2);
            VibrationManager.TriggerHapticFeedback(hapticType);
        }

        /// <summary>
        /// Trigger notification feedback
        /// </summary>
        public void TriggerNotificationFeedback(int notificationType)
        {
            if (!enableVibration) return;
            NotificationFeedbackType notifType = (NotificationFeedbackType)Mathf.Clamp(notificationType, 0, 2);
            VibrationManager.TriggerNotificationFeedback(notifType);
        }

        /// <summary>
        /// Trigger impact feedback
        /// </summary>
        public void TriggerImpactFeedback(int impactType)
        {
            if (!enableVibration) return;
            ImpactFeedbackType impact = (ImpactFeedbackType)Mathf.Clamp(impactType, 0, 2);
            VibrationManager.TriggerImpactFeedback(impact);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Enable or disable vibration for this component
        /// </summary>
        public bool EnableVibration
        {
            get => enableVibration;
            set => enableVibration = value;
        }

        /// <summary>
        /// Default vibration intensity
        /// </summary>
        public VibrationIntensity DefaultIntensity
        {
            get => defaultIntensity;
            set => defaultIntensity = value;
        }

        /// <summary>
        /// Check if vibration is supported
        /// </summary>
        public bool IsSupported => VibrationManager.IsSupported;

        /// <summary>
        /// Check if vibration is enabled in device settings
        /// </summary>
        public bool IsDeviceVibrationEnabled => VibrationManager.IsEnabled;

        #endregion
    }
}