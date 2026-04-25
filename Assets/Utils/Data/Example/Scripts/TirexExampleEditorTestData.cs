using UnityEngine;
using System;
using System.Collections.Generic;
using TirexGame.Utils.Data;

namespace MyGame.Data.Examples
{
    public enum QuestStatus { NotStarted, InProgress, Completed, Failed }

    [Serializable]
    public struct RarityConfig
    {
        public string RarityName;
        public Color Color;
        public float DropChance;
    }

    [Serializable]
    public class QuestObjective
    {
        public string Description;
        public int RequiredAmount;
        public int CurrentAmount;
        public bool IsOptional;
    }

    [Serializable]
    public class QuestEntry
    {
        public string QuestId;
        public string Title;
        public QuestStatus Status;
        public List<QuestObjective> Objectives;

        public QuestEntry()
        {
            Objectives = new List<QuestObjective>();
        }
    }

    [Serializable]
    public class ShopItem
    {
        public string ItemId;
        public int Price;
        public bool IsLimited;
        public int Stock;
    }

    /// <summary>
    /// Complex model specifically designed to stress-test the Data Editor's rendering capabilities.
    /// Tests: Nested Lists, Structs in Lists, Class instances, Null references.
    /// </summary>
    [Serializable]
    public class TirexExampleEditorTestData : IDataModel<TirexExampleEditorTestData>
    {
        [Header("General Config")]
        public string ConfigName;
        public int Version;
        
        [Header("List of Structs")]
        public List<RarityConfig> Rarities;

        [Header("Nested Lists & Complex Objects")]
        public List<QuestEntry> ActiveQuests;
        
        [Header("Simple Class List")]
        public List<ShopItem> ShopInventory;

        [Header("Nested Metadata")]
        public ShopMetadata Metadata;

        [Serializable]
        public class ShopMetadata
        {
            public DateTime LastRestock;
            public string ShopKeeperName;
            public List<string> Tags;
        }

        public void SetDefaultData()
        {
            ConfigName = "Editor Test Config";
            Version = 1;

            Rarities = new List<RarityConfig>
            {
                new RarityConfig { RarityName = "Common", Color = Color.white, DropChance = 0.7f },
                new RarityConfig { RarityName = "Rare", Color = Color.blue, DropChance = 0.2f },
                new RarityConfig { RarityName = "Legendary", Color = Color.yellow, DropChance = 0.05f }
            };

            ActiveQuests = new List<QuestEntry>();
            ShopInventory = new List<ShopItem>();
            Metadata = new ShopMetadata
            {
                LastRestock = DateTime.Now,
                ShopKeeperName = "Merchant Bob",
                Tags = new List<string> { "Weapon", "Armor", "Potions" }
            };
        }
    }
}
