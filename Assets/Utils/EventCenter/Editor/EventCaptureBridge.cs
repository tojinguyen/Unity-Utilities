// Optional bridge to hook into existing EventCenter runtime in Editor
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventCenter.EditorTools
{
    // This class exposes static methods that your runtime EventCenter can call when publishing
    // without adding hard Editor assembly references in runtime code. Use reflection/define symbols
    // or a compile-time #if UNITY_EDITOR guard around calls to these methods.
    public static class EventCaptureBridge
    {
        public static void Publish(string eventName, object payload, UnityEngine.Object source,
            string category, List<ListenerRecord> listeners)
        {
            EventCapture.OnPublish(eventName, payload, source, category, listeners);
        }
    }
}
#endif


