
using UnityEngine;

namespace Tirex.Game.Utils.Examples
{
    public class NotificationExample : MonoBehaviour
    {
        [SerializeField] private NotificationConfig config;

        private void Start()
        {
            NotificationManager.Initialize(config);
        }

        public void OnScheduleNotificationButtonClick()
        {
            Debug.Log("Scheduling notification...");
            NotificationManager.ScheduleNotification(new Notification
            {
                Id = 1,
                Title = "Hello from Gemini!",
                Text = "This is a test notification scheduled from the example script.",
                FireTime = System.DateTime.Now.AddSeconds(15),
                ChannelId = "default" // Make sure you have a channel with this ID in your config
            });
            Debug.Log("Notification scheduled!");
        }

        public void OnCancelNotificationButtonClick()
        {
            Debug.Log("Cancelling notification...");
            NotificationManager.CancelNotification(1);
            Debug.Log("Notification cancelled!");
        }
    }
}
