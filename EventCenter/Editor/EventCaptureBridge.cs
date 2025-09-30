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
                        try
                        {
                            // Extract data from anonymous object using reflection
                            var listenerType = listener.GetType();
                            var nameProperty = listenerType.GetProperty("name");
                            var targetInfoProperty = listenerType.GetProperty("targetInfo");
                            var durationProperty = listenerType.GetProperty("durationMs");
                            var exceptionProperty = listenerType.GetProperty("exception");
                            
                            var name = nameProperty?.GetValue(listener)?.ToString() ?? "Unknown";
                            var targetInfo = targetInfoProperty?.GetValue(listener);
                            var duration = (float)(durationProperty?.GetValue(listener) ?? 0f);
                            var exception = exceptionProperty?.GetValue(listener)?.ToString();
                            
                            // Extract target info if available
                            SourceInfo sourceInfo = new SourceInfo { objectName = "Unknown", typeName = "Unknown", instanceId = 0 };
                            if (targetInfo != null)
                            {
                                var targetType = targetInfo.GetType();
                                var objectNameProp = targetType.GetProperty("objectName");
                                var typeNameProp = targetType.GetProperty("typeName");
                                var instanceIdProp = targetType.GetProperty("instanceId");
                                
                                sourceInfo.objectName = objectNameProp?.GetValue(targetInfo)?.ToString() ?? "Unknown";
                                sourceInfo.typeName = typeNameProp?.GetValue(targetInfo)?.ToString() ?? "Unknown";
                                sourceInfo.instanceId = (int)(instanceIdProp?.GetValue(targetInfo) ?? 0);
                            }
                            
                            listenerRecords.Add(new ListenerRecord
                            {
                                name = name,
                                targetInfo = sourceInfo,
                                durationMs = duration,
                                exception = exception
                            });
                            
                            Debug.Log($"[EventCaptureBridge] Converted listener: {name} on {sourceInfo.typeName}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[EventCaptureBridge] Failed to convert listener: {ex.Message}");
                            // Fallback for failed conversion
                            listenerRecords.Add(new ListenerRecord
                            {
                                name = "Failed to parse",
                                targetInfo = new SourceInfo
                                {
                                    objectName = listener.ToString(),
                                    typeName = listener.GetType().Name,
                                    instanceId = listener.GetHashCode()
                                },
                                durationMs = 0.0,
                                exception = $"Conversion error: {ex.Message}"
                            });
                        }
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


