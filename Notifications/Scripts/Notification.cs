
using System;

namespace Tirex.Game.Utils
{
    [Serializable]
    public class Notification
    {
        public int Id;
        public string Title;
        public string Text;
        public DateTime FireTime;
        public string ChannelId;
    }
}
