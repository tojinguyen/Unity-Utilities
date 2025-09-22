
#if UNITY_ANDROID || UNITY_IOS
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
#endif
using UnityEngine;

namespace Tirex.Game.Utils
{
    public static class NotificationManager
    {
        public static void Initialize(NotificationConfig config)
        {
#if UNITY_ANDROID
            foreach (var channel in config.AndroidChannels)
            {
                var androidChannel = new AndroidNotificationChannel()
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Description = channel.Description,
                    Importance = Importance.Default,
                };
                AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
            }
#elif UNITY_IOS
            RequestAuthorization();
#endif
        }

        public static void ScheduleNotification(Notification notification)
        {
            if (notification.FireTime <= System.DateTime.Now)
            {
                Debug.LogWarning($"Notification with id {notification.Id} was scheduled for a past time.");
                return;
            }

#if UNITY_ANDROID
            var androidNotification = new AndroidNotification
            {
                Title = notification.Title,
                Text = notification.Text,
                FireTime = notification.FireTime,
                SmallIcon = "app_icon_small",
                LargeIcon = "app_icon_large"
            };

            AndroidNotificationCenter.SendNotificationWithExplicitID(androidNotification, notification.ChannelId, notification.Id);
#elif UNITY_IOS
            var timeTrigger = new iOSNotificationCalendarTrigger()
            {
                Year = notification.FireTime.Year,
                Month = notification.FireTime.Month,
                Day = notification.FireTime.Day,
                Hour = notification.FireTime.Hour,
                Minute = notification.FireTime.Minute,
                Second = notification.FireTime.Second,
                Repeats = false
            };

            var iosNotification = new iOSNotification()
            {
                Identifier = notification.Id.ToString(),
                Title = notification.Title,
                Body = notification.Text,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(iosNotification);
#endif
        }

        public static void CancelNotification(int id)
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(id);
#elif UNITY_IOS
            iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
#endif
        }

#if UNITY_IOS
        private static async void RequestAuthorization()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    await System.Threading.Tasks.Task.Yield();
                };
            }
        }
#endif
    }
}
