using TirexGame.Utils.Data;
using System;

[Serializable]
public class PlayerData : IDataModel<PlayerData>
{
    [Required]
    [StringLength(16, minLength: 3)]
    public string PlayerName;

    [Range(1, 100)]
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