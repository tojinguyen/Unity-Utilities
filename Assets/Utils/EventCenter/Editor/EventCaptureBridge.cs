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
            string category, List<object> listeners)
        {
            Debug.Log($"[EventCaptureBridge] Publish called - Event: {eventName}, Category: {category}, Source: {source}");
            
            // Convert listeners to proper format
            var listenerRecords = new List<ListenerRecord>();
            if (listeners != null)
            {
                Debug.Log($"[EventCaptureBridge] Converting {listeners.Count} listeners");
                foreach (var listener in listeners)
                {
                    if (listener != null)
                    {
                        listenerRecords.Add(new ListenerRecord
                        {
                            name = listener.ToString(),
                            targetInfo = new SourceInfo
                            {
                                objectName = listener.GetType().Name,
                                typeName = listener.GetType().FullName,
                                instanceId = listener.GetHashCode()
                            },
                            durationMs = 0.0, // Will be measured during dispatch
                            exception = null
                        });
                    }
                }
            }
            
            Debug.Log($"[EventCaptureBridge] Calling EventCapture.OnPublish with {listenerRecords.Count} listener records");
            EventCapture.OnPublish(eventName, payload, source, category, listenerRecords);
            Debug.Log($"[EventCaptureBridge] EventCapture.OnPublish completed");
        }
    }
}
#endif


