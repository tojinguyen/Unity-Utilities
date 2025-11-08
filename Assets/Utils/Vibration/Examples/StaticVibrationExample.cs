using UnityEngine;
using TirexGame.Utils.Vibration;

namespace TirexGame.Utils.Vibration.Examples
{
    /// <summary>
    /// Example demonstrating static vibration calls without component references
    /// </summary>
    public class StaticVibrationExample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool enableDebugLog = true;

        private void Start()
        {
            LogAction("Static Vibration Example Started");
            LogAction($"Vibration Supported: {Vibration.IsSupported}");
            LogAction($"Vibration Enabled: {Vibration.IsEnabled}");
        }

        #region Simple Static Calls

        /// <summary>
        /// Example: Button click vibration
        /// Call this directly from button OnClick event or from code
        /// </summary>
        public void OnButtonClick()
        {
            LogAction("Button Click Vibration");
            Vibration.Click();  // Simple static call - no references needed!
        }

        /// <summary>
        /// Example: Success action
        /// </summary>
        public void OnSuccessAction()
        {
            LogAction("Success Vibration");
            Vibration.Success();  // Static call for success pattern
        }

        /// <summary>
        /// Example: Error occurred
        /// </summary>
        public void OnError()
        {
            LogAction("Error Vibration");
            Vibration.Error();  // Static call for error pattern
        }

        /// <summary>
        /// Example: Warning
        /// </summary>
        public void OnWarning()
        {
            LogAction("Warning Vibration");
            Vibration.Warning();  // Static call for warning pattern
        }

        #endregion

        #region Intensity Based Calls

        public void PlayLightVibration()
        {
            LogAction("Light Vibration");
            Vibration.Short();  // or Vibration.Play(50);
        }

        public void PlayMediumVibration()
        {
            LogAction("Medium Vibration");
            Vibration.Medium();  // or Vibration.Play(150);
        }

        public void PlayStrongVibration()
        {
            LogAction("Strong Vibration");
            Vibration.Strong();  // or Vibration.Play(300);
        }

        #endregion

        #region Custom Duration Calls

        public void PlayCustomDuration()
        {
            LogAction("Custom Duration (250ms)");
            Vibration.Play(250);  // Custom milliseconds
        }

        public void PlayCustomPattern()
        {
            LogAction("Custom Pattern");
            int[] pattern = { 0, 100, 50, 150, 50, 100 };  // off, on, off, on, off, on
            Vibration.Play(pattern);
        }

        #endregion

        #region Haptic Feedback Calls

        public void PlayLightHaptic()
        {
            LogAction("Light Haptic");
            Vibration.Haptic.Light();
        }

        public void PlayMediumHaptic()
        {
            LogAction("Medium Haptic");
            Vibration.Haptic.Medium();
        }

        public void PlayHeavyHaptic()
        {
            LogAction("Heavy Haptic");
            Vibration.Haptic.Heavy();
        }

        public void PlaySuccessHaptic()
        {
            LogAction("Success Haptic");
            Vibration.Haptic.Success();
        }

        public void PlayWarningHaptic()
        {
            LogAction("Warning Haptic");
            Vibration.Haptic.Warning();
        }

        public void PlayErrorHaptic()
        {
            LogAction("Error Haptic");
            Vibration.Haptic.Error();
        }

        #endregion

        #region Direct VibrationManager Calls (Advanced)

        public void PlayAdvancedVibration()
        {
            LogAction("Advanced Vibration Manager Call");
            
            // You can still use VibrationManager directly for more control
            VibrationManager.Vibrate(VibrationIntensity.Medium);
            
            // Or use advanced haptic feedback
            VibrationManager.TriggerImpactFeedback(ImpactFeedbackType.Medium);
        }

        #endregion

        #region Utility Methods

        public void StopAllVibrations()
        {
            LogAction("Stop All Vibrations");
            Vibration.Stop();
        }

        public void CheckVibrationStatus()
        {
            LogAction($"Vibration Status - Supported: {Vibration.IsSupported}, Enabled: {Vibration.IsEnabled}");
        }

        private void LogAction(string action)
        {
            if (enableDebugLog)
            {
                Debug.Log($"Static Vibration Example: {action}");
            }
        }

        #endregion

        #region Example Usage in Game Logic

        // Example: Call vibration from anywhere in your code without references
        private void OnDestroy()
        {
            // Example: Player dies
            Vibration.Error();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Example: Player collected item
                Vibration.Success();
            }
        }

        // Example: Using in coroutines or async methods
        private System.Collections.IEnumerator ExampleCoroutine()
        {
            LogAction("Starting coroutine with vibrations");
            
            Vibration.Click();  // Start vibration
            yield return new WaitForSeconds(1f);
            
            Vibration.Medium(); // Mid vibration
            yield return new WaitForSeconds(1f);
            
            Vibration.Success(); // End vibration
        }

        [ContextMenu("Test Coroutine")]
        public void TestCoroutine()
        {
            StartCoroutine(ExampleCoroutine());
        }

        #endregion
    }
}