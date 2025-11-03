#import <UIKit/UIKit.h>
#import <AudioToolbox/AudioToolbox.h>

// Haptic feedback generators (iOS 10+)
static UIImpactFeedbackGenerator *lightImpactGenerator;
static UIImpactFeedbackGenerator *mediumImpactGenerator;
static UIImpactFeedbackGenerator *heavyImpactGenerator;
static UINotificationFeedbackGenerator *notificationGenerator;
static UISelectionFeedbackGenerator *selectionGenerator;

// Initialize haptic feedback generators
void initializeHapticGenerators() {
    if (@available(iOS 10.0, *)) {
        if (!lightImpactGenerator) {
            lightImpactGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [lightImpactGenerator prepare];
        }
        if (!mediumImpactGenerator) {
            mediumImpactGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            [mediumImpactGenerator prepare];
        }
        if (!heavyImpactGenerator) {
            heavyImpactGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            [heavyImpactGenerator prepare];
        }
        if (!notificationGenerator) {
            notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
            [notificationGenerator prepare];
        }
        if (!selectionGenerator) {
            selectionGenerator = [[UISelectionFeedbackGenerator alloc] init];
            [selectionGenerator prepare];
        }
    }
}

extern "C" {
    
    // Basic vibration
    void _vibrate() {
        AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
    }
    
    // Vibration with intensity mapping
    void _vibrateWithIntensity(int intensity) {
        if (@available(iOS 10.0, *)) {
            initializeHapticGenerators();
            
            switch (intensity) {
                case 0: // Light
                    [lightImpactGenerator impactOccurred];
                    break;
                case 1: // Medium
                    [mediumImpactGenerator impactOccurred];
                    break;
                case 2: // Heavy
                    [heavyImpactGenerator impactOccurred];
                    break;
                default:
                    [mediumImpactGenerator impactOccurred];
                    break;
            }
        } else {
            // Fallback to basic vibration for iOS < 10
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // Generic haptic feedback
    void _triggerHapticFeedback(int feedbackType) {
        if (@available(iOS 10.0, *)) {
            initializeHapticGenerators();
            
            switch (feedbackType) {
                case 0: // Selection
                    [selectionGenerator selectionChanged];
                    break;
                case 1: // Impact
                    [mediumImpactGenerator impactOccurred];
                    break;
                case 2: // Notification
                    [notificationGenerator notificationOccurred:UINotificationFeedbackTypeSuccess];
                    break;
                default:
                    [selectionGenerator selectionChanged];
                    break;
            }
        } else {
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // Notification feedback
    void _triggerNotificationFeedback(int notificationType) {
        if (@available(iOS 10.0, *)) {
            initializeHapticGenerators();
            
            UINotificationFeedbackType feedbackType;
            switch (notificationType) {
                case 0: // Success
                    feedbackType = UINotificationFeedbackTypeSuccess;
                    break;
                case 1: // Warning
                    feedbackType = UINotificationFeedbackTypeWarning;
                    break;
                case 2: // Error
                    feedbackType = UINotificationFeedbackTypeError;
                    break;
                default:
                    feedbackType = UINotificationFeedbackTypeSuccess;
                    break;
            }
            
            [notificationGenerator notificationOccurred:feedbackType];
        } else {
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // Impact feedback
    void _triggerImpactFeedback(int impactType) {
        if (@available(iOS 10.0, *)) {
            initializeHapticGenerators();
            
            switch (impactType) {
                case 0: // Light
                    [lightImpactGenerator impactOccurred];
                    break;
                case 1: // Medium
                    [mediumImpactGenerator impactOccurred];
                    break;
                case 2: // Heavy
                    [heavyImpactGenerator impactOccurred];
                    break;
                default:
                    [mediumImpactGenerator impactOccurred];
                    break;
            }
        } else {
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // Check if haptic feedback is supported
    bool _isHapticFeedbackSupported() {
        if (@available(iOS 10.0, *)) {
            return true;
        }
        return false;
    }
    
    // Check if vibration is enabled (simplified check)
    bool _isVibrationEnabled() {
        // iOS doesn't provide a direct way to check vibration settings
        // Return true by default, the system will handle it
        return true;
    }
}