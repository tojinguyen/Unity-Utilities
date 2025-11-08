using UnityEngine;
using UnityEngine.UI;
using TirexGame.Utils.Vibration;

namespace TirexGame.Utils.Vibration.Examples
{
    /// <summary>
    /// Example demonstrating custom vibration patterns
    /// </summary>
    public class VibrationPatternsExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button successPatternButton;
        [SerializeField] private Button warningPatternButton;
        [SerializeField] private Button errorPatternButton;
        [SerializeField] private Button doubleTapButton;
        [SerializeField] private Button longPressButton;
        [SerializeField] private Button customPatternButton;
        [SerializeField] private Text patternInfoText;

        [Header("Custom Pattern")]
        [SerializeField] private int[] customPattern = { 0, 100, 50, 100, 50, 100 };
        [SerializeField] private int customRepeat = -1;

        private void Start()
        {
            SetupButtons();
            UpdatePatternInfo();
        }

        private void SetupButtons()
        {
            if (successPatternButton != null)
                successPatternButton.onClick.AddListener(OnSuccessPattern);

            if (warningPatternButton != null)
                warningPatternButton.onClick.AddListener(OnWarningPattern);

            if (errorPatternButton != null)
                errorPatternButton.onClick.AddListener(OnErrorPattern);

            if (doubleTapButton != null)
                doubleTapButton.onClick.AddListener(OnDoubleTapPattern);

            if (longPressButton != null)
                longPressButton.onClick.AddListener(OnLongPressPattern);

            if (customPatternButton != null)
                customPatternButton.onClick.AddListener(OnCustomPattern);
        }

        private void UpdatePatternInfo()
        {
            if (patternInfoText != null)
            {
                string info = "Vibration Patterns:\\n";
                info += "• Success: Short double pulse\\n";
                info += "• Warning: Triple pulse with pauses\\n";
                info += "• Error: Strong single pulse\\n";
                info += "• Double Tap: Two quick pulses\\n";
                info += "• Long Press: Extended vibration\\n";
                info += $"• Custom: {string.Join(",", customPattern)}";
                patternInfoText.text = info;
            }
        }

        private void OnSuccessPattern()
        {
            VibrationManager.VibrateSuccess();
            Debug.Log("Pattern Example: Success pattern triggered");
        }

        private void OnWarningPattern()
        {
            VibrationManager.VibrateWarning();
            Debug.Log("Pattern Example: Warning pattern triggered");
        }

        private void OnErrorPattern()
        {
            VibrationManager.VibrateError();
            Debug.Log("Pattern Example: Error pattern triggered");
        }

        private void OnDoubleTapPattern()
        {
            VibrationManager.VibrateDoubleTap();
            Debug.Log("Pattern Example: Double tap pattern triggered");
        }

        private void OnLongPressPattern()
        {
            VibrationManager.VibrateLongPress();
            Debug.Log("Pattern Example: Long press pattern triggered");
        }

        private void OnCustomPattern()
        {
            VibrationManager.Vibrate(customPattern, customRepeat);
            Debug.Log($"Pattern Example: Custom pattern triggered - {string.Join(",", customPattern)}");
        }

        // Public methods for more pattern examples
        public void PlayHeartbeatPattern()
        {
            int[] heartbeat = { 0, 100, 100, 100, 200, 100, 200, 100 };
            VibrationManager.Vibrate(heartbeat);
            Debug.Log("Pattern Example: Heartbeat pattern");
        }

        public void PlayMorseCodeSOSPattern()
        {
            // SOS in Morse code: ... --- ...
            int[] sos = { 0, 100, 100, 100, 100, 100, 100, 300, 100, 300, 100, 300, 100, 100, 100, 100, 100, 100 };
            VibrationManager.Vibrate(sos);
            Debug.Log("Pattern Example: SOS Morse code pattern");
        }

        public void PlayNotificationPattern()
        {
            int[] notification = { 0, 50, 50, 150, 50, 50 };
            VibrationManager.Vibrate(notification);
            Debug.Log("Pattern Example: Notification pattern");
        }

        public void PlayGameOverPattern()
        {
            int[] gameOver = { 0, 200, 100, 200, 100, 200, 100, 500 };
            VibrationManager.Vibrate(gameOver);
            Debug.Log("Pattern Example: Game over pattern");
        }
    }
}