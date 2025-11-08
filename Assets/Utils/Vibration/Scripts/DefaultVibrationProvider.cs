using UnityEngine;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Default vibration provider for unsupported platforms
    /// </summary>
    public class DefaultVibrationProvider : IVibrationProvider
    {
        public bool IsSupported => false;
        public bool IsEnabled => false;

        public void Vibrate()
        {
            Debug.LogWarning("Vibration is not supported on this platform");
        }

        public void Vibrate(int milliseconds)
        {
            Debug.LogWarning("Vibration is not supported on this platform");
        }

        public void Vibrate(VibrationIntensity intensity)
        {
            Debug.LogWarning("Vibration is not supported on this platform");
        }

        public void Vibrate(int[] pattern, int repeat = -1)
        {
            Debug.LogWarning("Vibration is not supported on this platform");
        }

        public void Cancel()
        {
            Debug.LogWarning("Vibration is not supported on this platform");
        }

        public void TriggerHapticFeedback(HapticFeedbackType feedbackType)
        {
            Debug.LogWarning("Haptic feedback is not supported on this platform");
        }

        public void TriggerNotificationFeedback(NotificationFeedbackType notificationType)
        {
            Debug.LogWarning("Notification feedback is not supported on this platform");
        }

        public void TriggerImpactFeedback(ImpactFeedbackType impactType)
        {
            Debug.LogWarning("Impact feedback is not supported on this platform");
        }
    }
}