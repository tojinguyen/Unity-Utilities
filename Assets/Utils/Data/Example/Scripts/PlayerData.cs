using TirexGame.Utils.Data;
using System;

[Serializable]
public class PlayerData : IDataModel<PlayerData>
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