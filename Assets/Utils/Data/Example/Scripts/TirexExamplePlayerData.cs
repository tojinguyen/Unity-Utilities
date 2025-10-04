using TirexGame.Utils.Data;
using System;

namespace TirexGame.Utils.Data.Examples
{
    [Serializable]
    public class TirexExamplePlayerData : IDataModel<TirexExamplePlayerData>
    {
        public string PlayerName;
        public int Level;
        public float Health;
        public DateTime LastLogin;

        public void SetDefaultData()
        {
            PlayerName = "New Player";
            Level = 1;
            Health = 100f;
            LastLogin = DateTime.UtcNow;
        }
    }
}