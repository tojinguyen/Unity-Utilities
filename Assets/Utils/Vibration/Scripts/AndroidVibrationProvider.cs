using UnityEngine;

namespace TirexGame.Utils.Vibration
{
    /// <summary>
    /// Android implementation of vibration provider
    /// </summary>
    public class AndroidVibrationProvider : IVibrationProvider
    {
        private AndroidJavaObject vibrator;
        private AndroidJavaClass unityPlayer;
        private AndroidJavaObject currentActivity;
        private AndroidJavaClass vibrationEffectClass;
        private bool hasVibrationEffect;

        public AndroidVibrationProvider()
        {
            InitializeAndroid();
        }

        private void InitializeAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                
                // Check if VibrationEffect is available (Android 8.0+)
                try
                {
                    vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    hasVibrationEffect = true;
                }
                catch
                {
                    hasVibrationEffect = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize Android vibration: {e.Message}");
            }
#endif
        }

        public bool IsSupported
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return vibrator != null && vibrator.Call<bool>("hasVibrator");
#else
                return false;
#endif
            }
        }

        public bool IsEnabled
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Check system settings for vibration
                try
                {
                    AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaClass settingsSystem = new AndroidJavaClass("android.provider.Settings$System");
                    int hapticFeedback = settingsSystem.CallStatic<int>("getInt", contentResolver, "haptic_feedback_enabled", 0);
                    return hapticFeedback == 1;
                }
                catch
                {
                    return true; // Default to enabled if can't check
                }
#else
                return true;
#endif
            }
        }

        public void Vibrate()
        {
            Vibrate(100); // Default 100ms vibration
        }

        public void Vibrate(long milliseconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;

            try
            {
                if (hasVibrationEffect)
                {
                    // Use VibrationEffect for Android 8.0+
                    AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createOneShot", milliseconds, -1); // -1 for default amplitude
                    vibrator.Call("vibrate", vibrationEffect);
                }
                else
                {
                    // Use deprecated method for older Android versions
                    vibrator.Call("vibrate", milliseconds);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android vibration error: {e.Message}");
            }
#endif
        }

        public void Vibrate(VibrationIntensity intensity)
        {
            long duration = intensity switch
            {
                VibrationIntensity.Light => 50,
                VibrationIntensity.Medium => 100,
                VibrationIntensity.Heavy => 200,
                _ => 100
            };
            Vibrate(duration);
        }

        public void Vibrate(long[] pattern, int repeat = -1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;

            try
            {
                if (hasVibrationEffect)
                {
                    // Use VibrationEffect for Android 8.0+
                    AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                        "createWaveform", pattern, repeat);
                    vibrator.Call("vibrate", vibrationEffect);
                }
                else
                {
                    // Use deprecated method for older Android versions
                    vibrator.Call("vibrate", pattern, repeat);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android pattern vibration error: {e.Message}");
            }
#endif
        }

        public void Cancel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSupported) return;

            try
            {
                vibrator.Call("cancel");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android vibration cancel error: {e.Message}");
            }
#endif
        }

        public void TriggerHapticFeedback(HapticFeedbackType feedbackType)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!IsSupported || !IsEnabled) return;

            try
            {
                AndroidJavaClass hapticFeedbackConstants = new AndroidJavaClass("android.view.HapticFeedbackConstants");
                int feedbackConstant = feedbackType switch
                {
                    HapticFeedbackType.Selection => hapticFeedbackConstants.GetStatic<int>("VIRTUAL_KEY"),
                    HapticFeedbackType.Impact => hapticFeedbackConstants.GetStatic<int>("LONG_PRESS"),
                    HapticFeedbackType.Notification => hapticFeedbackConstants.GetStatic<int>("VIRTUAL_KEY"),
                    _ => hapticFeedbackConstants.GetStatic<int>("VIRTUAL_KEY")
                };

                AndroidJavaObject view = currentActivity.Call<AndroidJavaObject>("findViewById", 
                    new AndroidJavaClass("android.R$id").GetStatic<int>("content"));
                view.Call<bool>("performHapticFeedback", feedbackConstant);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Android haptic feedback error: {e.Message}");
                // Fallback to simple vibration
                Vibrate(50);
            }
#endif
        }

        public void TriggerNotificationFeedback(NotificationFeedbackType notificationType)
        {
            // Android doesn't have specific notification feedback, use haptic feedback
            TriggerHapticFeedback(HapticFeedbackType.Notification);
        }

        public void TriggerImpactFeedback(ImpactFeedbackType impactType)
        {
            // Android doesn't have specific impact feedback, use vibration with different intensities
            VibrationIntensity intensity = impactType switch
            {
                ImpactFeedbackType.Light => VibrationIntensity.Light,
                ImpactFeedbackType.Medium => VibrationIntensity.Medium,
                ImpactFeedbackType.Heavy => VibrationIntensity.Heavy,
                _ => VibrationIntensity.Medium
            };
            Vibrate(intensity);
        }
    }
}