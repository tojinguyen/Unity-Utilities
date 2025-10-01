# Notifications

This utility provides a cross-platform solution for scheduling and managing local notifications in Unity, with support for both Android and iOS. It uses Unity's built-in Notification package.

## Features

- **Cross-Platform Support**: Schedule notifications for both Android and iOS devices.
- **Configurable Channels**: Define notification channels for Android through a ScriptableObject.
- **Simple API**: Easy-to-use methods for initializing, scheduling, and canceling notifications.

## How to Use

1.  **Install Unity Notification Package**: Ensure you have the `Unity Notification` package installed in your project.
2.  **Create Notification Config**: Create a `NotificationConfig` ScriptableObject from the `Create > TirexGame > Notifications > Notification Config` menu. Define your Android notification channels here.
3.  **Initialize the Notification Manager**: Call `NotificationManager.Initialize()` with your `NotificationConfig` at the start of your application.
4.  **Schedule Notifications**: Use `NotificationManager.ScheduleNotification()` to send a notification.
5.  **Cancel Notifications**: Use `NotificationManager.CancelNotification()` to cancel a scheduled notification.

### Example Usage

```csharp
using Tirex.Game.Utils;
using System;
using UnityEngine;

public class NotificationExample : MonoBehaviour
{
    public NotificationConfig notificationConfig;

    void Start()
    {
        // Initialize the Notification Manager with your config
        NotificationManager.Initialize(notificationConfig);

        // Schedule a notification to fire in 10 seconds
        ScheduleTestNotification();
    }

    void ScheduleTestNotification()
    {
        var notification = new Notification
        {
            Id = 1,
            Title = "My Game",
            Text = "Come back and play!",
            FireTime = DateTime.Now.AddSeconds(10),
            ChannelId = "default_channel" // Must match a channel defined in NotificationConfig
        };

        NotificationManager.ScheduleNotification(notification);
        Debug.Log("Notification scheduled!");
    }

    void CancelTestNotification()
    {
        NotificationManager.CancelNotification(1);
        Debug.Log("Notification canceled!");
    }
}
```
