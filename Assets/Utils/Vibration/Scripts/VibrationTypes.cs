namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Defines the intensity level of vibration
    /// </summary>
    public enum VibrationIntensity
    {
        Light = 0,
        Medium = 1,
        Heavy = 2
    }

    /// <summary>
    /// Defines the type of haptic feedback
    /// </summary>
    public enum HapticFeedbackType
    {
        Selection,
        Impact,
        Notification
    }

    /// <summary>
    /// Defines the notification feedback type (iOS specific)
    /// </summary>
    public enum NotificationFeedbackType
    {
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Defines the impact feedback type (iOS specific)
    /// </summary>
    public enum ImpactFeedbackType
    {
        Light,
        Medium,
        Heavy
    }
}