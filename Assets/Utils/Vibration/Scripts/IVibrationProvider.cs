using UnityEngine;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Interface for platform-specific vibration implementations
    /// </summary>
    public interface IVibrationProvider
    {
        /// <summary>
        /// Check if vibration is supported on the current platform
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Check if vibration is enabled in device settings
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Vibrate with default pattern
        /// </summary>
        void Vibrate();

        /// <summary>
        /// Vibrate for specified duration in milliseconds
        /// </summary>
        /// <param name="milliseconds">Duration in milliseconds</param>
        void Vibrate(int milliseconds);

        /// <summary>
        /// Vibrate with specified intensity
        /// </summary>
        /// <param name="intensity">Vibration intensity</param>
        void Vibrate(VibrationIntensity intensity);

        /// <summary>
        /// Vibrate with pattern and repeat
        /// </summary>
        /// <param name="pattern">Array of durations: off, on, off, on...</param>
        /// <param name="repeat">Repeat pattern (-1 for no repeat)</param>
        void Vibrate(int[] pattern, int repeat = -1);

        /// <summary>
        /// Cancel ongoing vibration
        /// </summary>
        void Cancel();

        /// <summary>
        /// Trigger haptic feedback
        /// </summary>
        /// <param name="feedbackType">Type of haptic feedback</param>
        void TriggerHapticFeedback(HapticFeedbackType feedbackType);

        /// <summary>
        /// Trigger notification feedback (iOS specific)
        /// </summary>
        /// <param name="notificationType">Type of notification feedback</param>
        void TriggerNotificationFeedback(NotificationFeedbackType notificationType);

        /// <summary>
        /// Trigger impact feedback (iOS specific)
        /// </summary>
        /// <param name="impactType">Type of impact feedback</param>
        void TriggerImpactFeedback(ImpactFeedbackType impactType);
    }
}