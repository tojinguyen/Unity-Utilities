using UnityEngine;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Simplified static class for easy vibration access
    /// This provides the most commonly used vibration functions in a simple static interface
    /// </summary>
    public static class Vibration
    {
        /// <summary>
        /// Play a quick vibration (perfect for button clicks)
        /// </summary>
        public static void Click()
        {
            VibrationManager.VibrateClick();
        }

        /// <summary>
        /// Play a short vibration
        /// </summary>
        public static void Short()
        {
            VibrationManager.Vibrate(VibrationIntensity.Light);
        }

        /// <summary>
        /// Play a medium vibration
        /// </summary>
        public static void Medium()
        {
            VibrationManager.Vibrate(VibrationIntensity.Medium);
        }

        /// <summary>
        /// Play a strong vibration
        /// </summary>
        public static void Strong()
        {
            VibrationManager.Vibrate(VibrationIntensity.Heavy);
        }

        /// <summary>
        /// Play default vibration
        /// </summary>
        public static void Play()
        {
            VibrationManager.Vibrate();
        }

        /// <summary>
        /// Play vibration for specified milliseconds
        /// </summary>
        /// <param name="milliseconds">Duration in milliseconds</param>
        public static void Play(long milliseconds)
        {
            VibrationManager.Vibrate(milliseconds);
        }

        /// <summary>
        /// Play vibration with custom pattern
        /// </summary>
        /// <param name="pattern">Pattern array: off, on, off, on...</param>
        public static void Play(long[] pattern)
        {
            VibrationManager.Vibrate(pattern);
        }

        /// <summary>
        /// Play success vibration pattern
        /// </summary>
        public static void Success()
        {
            VibrationManager.VibrateSuccess();
        }

        /// <summary>
        /// Play error vibration pattern
        /// </summary>
        public static void Error()
        {
            VibrationManager.VibrateError();
        }

        /// <summary>
        /// Play warning vibration pattern
        /// </summary>
        public static void Warning()
        {
            VibrationManager.VibrateWarning();
        }

        /// <summary>
        /// Stop all vibrations
        /// </summary>
        public static void Stop()
        {
            VibrationManager.Cancel();
        }

        /// <summary>
        /// Check if vibration is supported on this device
        /// </summary>
        public static bool IsSupported => VibrationManager.IsSupported;

        /// <summary>
        /// Check if vibration is enabled in device settings
        /// </summary>
        public static bool IsEnabled => VibrationManager.IsEnabled;

        // Haptic feedback shortcuts
        public static class Haptic
        {
            /// <summary>
            /// Light haptic feedback
            /// </summary>
            public static void Light()
            {
                VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Light);
            }

            /// <summary>
            /// Medium haptic feedback
            /// </summary>
            public static void Medium()
            {
                VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Medium);
            }

            /// <summary>
            /// Heavy haptic feedback
            /// </summary>
            public static void Heavy()
            {
                VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Heavy);
            }

            /// <summary>
            /// Success haptic feedback
            /// </summary>
            public static void Success()
            {
                VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Success);
            }

            /// <summary>
            /// Warning haptic feedback
            /// </summary>
            public static void Warning()
            {
                VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Warning);
            }

            /// <summary>
            /// Error haptic feedback
            /// </summary>
            public static void Error()
            {
                VibrationManager.TriggerNotificationFeedback(NotificationFeedbackType.Error);
            }
        }
    }
}