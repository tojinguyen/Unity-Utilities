// Editor config ScriptableObject for colors and settings
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EventCenter.EditorTools
{
    [Serializable]
    public class ChannelColor
    {
        public string channel;
        public Color color = Color.cyan;
    }

    public class EventVisualizerConfig : ScriptableObject
    {
        public int bufferSize = EventCapture.DefaultBufferSize;
        public List<ChannelColor> channelColors = new List<ChannelColor>();
        public bool backgroundCapture;
        public float defaultZoom = 100f;
        public bool showTooltips = true;

        public Color GetChannelColor(string channel)
        {
            var c = channelColors.Find(x => string.Equals(x.channel, channel, StringComparison.OrdinalIgnoreCase));
            if (c != null) return c.color;
            return Color.cyan;
        }

        public static EventVisualizerConfig LoadOrCreate()
        {
            const string resourcesPath = "EventCenterEditorConfig";
            var cfg = Resources.Load<EventVisualizerConfig>(resourcesPath);
            if (cfg != null) return cfg;
            cfg = CreateInstance<EventVisualizerConfig>();

            // Ensure folder exists
            const string assetPath = "Assets/Utils/EventCenter/Resources/EventCenterEditorConfig.asset";
            var dir = System.IO.Path.GetDirectoryName(assetPath).Replace('\\','/');
            if (!AssetDatabase.IsValidFolder("Assets/Utils")) AssetDatabase.CreateFolder("Assets", "Utils");
            if (!AssetDatabase.IsValidFolder("Assets/Utils/EventCenter")) AssetDatabase.CreateFolder("Assets/Utils", "EventCenter");
            if (!AssetDatabase.IsValidFolder("Assets/Utils/EventCenter/Resources")) AssetDatabase.CreateFolder("Assets/Utils/EventCenter", "Resources");

            AssetDatabase.CreateAsset(cfg, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return cfg;
        }
    }
}
#endif


