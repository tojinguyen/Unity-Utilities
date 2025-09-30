// NOTE: Editor-only utilities and models for the Event Visualizer tool
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventCenter.EditorTools
{
    [Serializable]
    public class SourceInfo
    {
        public string objectName;
        public string typeName;
        public int instanceId;
        public string hierarchyPath;
    }

    [Serializable]
    public class ListenerRecord
    {
        public string name; // method signature or handler name
        public SourceInfo targetInfo = new SourceInfo();
        public double durationMs;
        public string exception;
    }

    [Serializable]
    public class EventRecord
    {
        public string id = Guid.NewGuid().ToString("N");
        public double timeRealtime;
        public float gameTime;
        public string name;
        public string category;
        public string payloadPreview;
        public SourceInfo sourceInfo = new SourceInfo();
        public List<ListenerRecord> listeners = new List<ListenerRecord>();

        // Cached UI state (editor-only)
        [NonSerialized] public Rect lastDrawRect;
        [NonSerialized] public bool isSelected;
        [NonSerialized] public Color cachedColor = Color.white;
    }

    public interface IEventMetadataResolver
    {
        string ResolveCategory(string eventName);
    }

    public interface IPayloadFormatter
    {
        string ToPreviewString(object payload);
    }

    public interface IColorProvider
    {
        Color GetColorFor(string category, string eventName);
    }
}
#endif


