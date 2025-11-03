using UnityEngine;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Main vibration manager that handles all vibration functionality across platforms
    /// </summary>
    public static class VibrationManager
    {
        private static IVibrationProvider provider;
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize the vibration manager
        /// </summary>
        static VibrationManager()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the appropriate vibration provider based on platform
        /// </summary>
        private static void Initialize()
        {
            if (isInitialized) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            provider = new AndroidVibrationProvider();
#elif UNITY_IOS && !UNITY_EDITOR
            provider = new IOSVibrationProvider();
#else
            provider = new DefaultVibrationProvider();
#endif

            isInitialized = true;
            Debug.Log($"VibrationManager initialized with provider: {provider.GetType().Name}");
        }

        /// <summary>
        /// Check if vibration is supported on the current platform
        /// </summary>
        public static bool IsSupported => provider?.IsSupported ?? false;

        /// <summary>
        /// Check if vibration is enabled in device settings
        /// </summary>
        public static bool IsEnabled => provider?.IsEnabled ?? false;

        /// <summary>
        /// Vibrate with default pattern (100ms)
        /// </summary>
        public static void Vibrate()
        {
            provider?.Vibrate();
        }

        /// <summary>
        /// Vibrate for specified duration in milliseconds
        /// </summary>
        /// <param name="milliseconds">Duration in milliseconds</param>
        public static void Vibrate(long milliseconds)
        {
            provider?.Vibrate(milliseconds);
        }

        /// <summary>
        /// Vibrate with specified intensity
        /// </summary>
        /// <param name="intensity">Vibration intensity</param>
        public static void Vibrate(VibrationIntensity intensity)
        {
            provider?.Vibrate(intensity);
        }

        /// <summary>
        /// Vibrate with pattern and repeat
        /// </summary>
        /// <param name="pattern">Array of durations: off, on, off, on...</param>
        /// <param name="repeat">Repeat pattern (-1 for no repeat)</param>
        public static void Vibrate(long[] pattern, int repeat = -1)
        {
            provider?.Vibrate(pattern, repeat);
        }

        /// <summary>
        /// Cancel ongoing vibration
        /// </summary>
        public static void Cancel()
        {
            provider?.Cancel();
        }

        /// <summary>
        /// Trigger haptic feedback
        /// </summary>
        /// <param name="feedbackType">Type of haptic feedback</param>
        public static void TriggerHapticFeedback(HapticFeedbackType feedbackType)
        {
            provider?.TriggerHapticFeedback(feedbackType);
        }

        /// <summary>
        /// Trigger notification feedback (iOS specific, fallback on Android)
        /// </summary>
        /// <param name="notificationType">Type of notification feedback</param>
        public static void TriggerNotificationFeedback(NotificationFeedbackType notificationType)
        {
            provider?.TriggerNotificationFeedback(notificationType);
        }

        /// <summary>
        /// Trigger impact feedback (iOS specific, fallback on Android)
        /// </summary>
        /// <param name="impactType">Type of impact feedback</param>
        public static void TriggerImpactFeedback(ImpactFeedbackType impactType)
        {
            provider?.TriggerImpactFeedback(impactType);
        }

        // Predefined vibration patterns for common use cases

        /// <summary>
        /// Short vibration for button clicks
        /// </summary>
        public static void VibrateClick()
        {
            Vibrate(VibrationIntensity.Light);
        }

        /// <summary>
        /// Medium vibration for confirmations
        /// </summary>
        public static void VibrateConfirm()
        {
            Vibrate(VibrationIntensity.Medium);
        }

        /// <summary>
        /// Strong vibration for errors or warnings
        /// </summary>
        public static void VibrateError()
        {
            Vibrate(VibrationIntensity.Heavy);
        }

        /// <summary>
        /// Success vibration pattern
        /// </summary>
        public static void VibrateSuccess()
        {
            long[] pattern = { 0, 100, 50, 100 };
            Vibrate(pattern);
        }

        /// <summary>
        /// Warning vibration pattern
        /// </summary>
        public static void VibrateWarning()
        {
            long[] pattern = { 0, 200, 100, 200, 100, 200 };
            Vibrate(pattern);
        }

        /// <summary>
        /// Double tap vibration pattern
        /// </summary>
        public static void VibrateDoubleTap()
        {
            long[] pattern = { 0, 50, 50, 50 };
            Vibrate(pattern);
        }

        /// <summary>
        /// Long press vibration pattern
        /// </summary>
        public static void VibrateLongPress()
        {
            Vibrate(300);
        }
    }
}