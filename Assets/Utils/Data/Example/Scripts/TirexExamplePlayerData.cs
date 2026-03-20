using TirexGame.Utils.Data;
using System;
using System.Collections.Generic;

namespace MyGame.Data
{
    public enum PlayerClass
    {
        Warrior,
        Mage,
        Archer,
        Thief
    }

    [Serializable]
    public class InventoryItem
    {
        public string ItemId = "item_001";
        public int Quantity = 1;
    }

    [Serializable]
    public class TestPlayerData : IDataModel<TestPlayerData>
    {
        public string PlayerName;
        public int Level;
        public float Health;
        public DateTime LastLogin;
        public PlayerClass PlayerClassType;
        public List<string> UnlockedAchievements;
        public List<InventoryItem> Inventory;

        public void SetDefaultData()
        {
            PlayerName = "New Player";
            Level = 1;
            Health = 100f;
            LastLogin = DateTime.UtcNow;
            PlayerClassType = PlayerClass.Warrior;
            UnlockedAchievements = new List<string>();
            Inventory = new List<InventoryItem>();
        }
    }
}