using TirexGame.Utils.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    public struct StatModifier
    {
        public string StatName;
        public float Value;
        public bool IsPercent;
    }

    [Serializable]
    public class Equipment
    {
        public string SlotName;
        public string ItemId;
        public int Level;
        public List<string> Modifiers;

        public Equipment()
        {
            Modifiers = new List<string>();
        }
    }

    [Serializable]
    public class TestPlayerData : IDataModel<TestPlayerData>
    {
        public string PlayerName;
        public int Level;
        public float Health;
        public DateTime LastLogin;
        public PlayerClass PlayerClassType;
        public Vector2 LastPosition;
        public Vector3 SpawnPoint;
        
        public List<string> UnlockedAchievements;
        public List<InventoryItem> Inventory;
        public List<StatModifier> ActiveModifiers;
        
        public Equipment Weapon;
        public Equipment Armor;

        public void SetDefaultData()
        {
            PlayerName = "New Player";
            Level = 1;
            Health = 100f;
            LastLogin = DateTime.UtcNow;
            PlayerClassType = PlayerClass.Warrior;
            LastPosition = Vector2.zero;
            SpawnPoint = new Vector3(0, 5, 0);
            
            UnlockedAchievements = new List<string> { "First Login" };
            Inventory = new List<InventoryItem> { new InventoryItem { ItemId = "starter_sword", Quantity = 1 } };
            ActiveModifiers = new List<StatModifier>();
            
            Weapon = new Equipment { SlotName = "MainHand", ItemId = "wooden_sword", Level = 1 };
            Armor = null; // Test "Create" button in editor
        }
    }
}