
using System.Collections.Generic;
using UnityEngine;

namespace Tirex.Game.Utils
{
    [CreateAssetMenu(fileName = "NotificationConfig", menuName = "TirexGame/Notifications/Notification Config")]
    public class NotificationConfig : ScriptableObject
    {
        public List<NotificationChannel> AndroidChannels = new List<NotificationChannel>();
    }
}
