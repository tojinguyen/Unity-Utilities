using UnityEngine;
using System.Runtime.InteropServices;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// iOS implementation of vibration provider
    /// </summary>
    public class IOSVibrationProvider : IVibrationProvider
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _vibrate();
        
        [DllImport("__Internal")]
        private static extern void _vibrateWithIntensity(int intensity);
        
        [DllImport("__Internal")]
        private static extern void _triggerHapticFeedback(int feedbackType);
        
        [DllImport("__Internal")]
        private static extern void _triggerNotificationFeedback(int notificationType);
        
        [DllImport("__Internal")]
        private static extern void _triggerImpactFeedback(int impactType);
        
        [DllImport("__Internal")]
        private static extern bool _isHapticFeedbackSupported();
        
        [DllImport("__Internal")]
        private static extern bool _isVibrationEnabled();
#endif

        public bool IsSupported
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return _isHapticFeedbackSupported();
#else
                return false;
#endif
            }
        }

        public bool IsEnabled
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return _isVibrationEnabled();
#else
                return true;
#endif
            }
        }

        public void Vibrate()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;
            _vibrate();
#endif
        }

        public void Vibrate(int milliseconds)
        {
            // iOS doesn't support duration-based vibration, use intensity instead
            VibrationIntensity intensity = milliseconds switch
            {
                <= 50 => VibrationIntensity.Light,
                <= 150 => VibrationIntensity.Medium,
                _ => VibrationIntensity.Heavy
            };
            Vibrate(intensity);
        }

        public void Vibrate(VibrationIntensity intensity)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;
            _vibrateWithIntensity((int)intensity);
#endif
        }

        public void Vibrate(int[] pattern, int repeat = -1)
        {
            // iOS doesn't support pattern vibration, use simple vibration
            Vibrate();
        }

        public void Cancel()
        {
            // iOS doesn't support canceling vibration
        }

        public void TriggerHapticFeedback(HapticFeedbackType feedbackType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;
            _triggerHapticFeedback((int)feedbackType);
#endif
        }

        public void TriggerNotificationFeedback(NotificationFeedbackType notificationType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;
            _triggerNotificationFeedback((int)notificationType);
#endif
        }

        public void TriggerImpactFeedback(ImpactFeedbackType impactType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;
            _triggerImpactFeedback((int)impactType);
#endif
        }
    }
}