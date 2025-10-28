using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum ColorLog
{
    BLUE,
    RED,
    GREEN,
    YELLOW,
    ORANGE,
    WHITE
}

public static class ConsoleLogger
{
    [Conditional("ENABLE_LOGS")]
    public static void LogColor(object message, ColorLog color = ColorLog.WHITE)
    {
        string colorStr = GetColorString(color);
#if ENABLE_LOGS
        UnityEngine.Debug.Log($"<color={colorStr}>{message.ToString()}</color>");
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void Log(object message)
    {
#if ENABLE_LOGS
        UnityEngine.Debug.Log(message.ToString());
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void LogError(object message)
    {
#if ENABLE_LOGS
        UnityEngine.Debug.LogError(message.ToString());
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void LogWarning(object message)
    {
#if ENABLE_LOGS
        UnityEngine.Debug.LogWarning(message.ToString());
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void DrawLine(Vector3 start, Vector3 end, ColorLog color = ColorLog.WHITE, float duration = 0)
    {
#if ENABLE_LOGS
        var unityColor = GetUnityColor(color);
        Debug.DrawLine(start, end, unityColor, duration);
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void DrawRay(Vector3 origin, Vector3 direction, ColorLog color = ColorLog.WHITE, float duration = 0)
    {
#if ENABLE_LOGS
        var unityColor = GetUnityColor(color);
        Debug.DrawRay(origin, direction, unityColor, duration);
#endif
    }

    [Conditional("ENABLE_LOGS")]
    public static void DrawBox(Vector3 center, Vector3 size, Quaternion rotation, ColorLog color = ColorLog.WHITE, float duration = 0)
    {
#if ENABLE_LOGS
        var unityColor = GetUnityColor(color);
        var halfSize = size * 0.5f;

        // Calculate box corners
        Vector3[] corners = new Vector3[8];
        corners[0] = center + rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z); // Bottom-left-back
        corners[1] = center + rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);  // Bottom-right-back
        corners[2] = center + rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);  // Bottom-left-front
        corners[3] = center + rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);   // Bottom-right-front
        corners[4] = center + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z); // Top-left-back
        corners[5] = center + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);  // Top-right-back
        corners[6] = center + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);  // Top-left-front
        corners[7] = center + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);   // Top-right-front

        // Draw box edges
        Debug.DrawLine(corners[0], corners[1], unityColor, duration); // Bottom-back
        Debug.DrawLine(corners[1], corners[3], unityColor, duration); // Bottom-right
        Debug.DrawLine(corners[3], corners[2], unityColor, duration); // Bottom-front
        Debug.DrawLine(corners[2], corners[0], unityColor, duration); // Bottom-left

        Debug.DrawLine(corners[4], corners[5], unityColor, duration); // Top-back
        Debug.DrawLine(corners[5], corners[7], unityColor, duration); // Top-right
        Debug.DrawLine(corners[7], corners[6], unityColor, duration); // Top-front
        Debug.DrawLine(corners[6], corners[4], unityColor, duration); // Top-left

        Debug.DrawLine(corners[0], corners[4], unityColor, duration); // Left-back
        Debug.DrawLine(corners[1], corners[5], unityColor, duration); // Right-back
        Debug.DrawLine(corners[2], corners[6], unityColor, duration); // Left-front
        Debug.DrawLine(corners[3], corners[7], unityColor, duration); // Right-front
#endif
    }
    
    /// <summary>
    /// Vẽ một tia (Ray) trong Scene View.
    /// </summary>
    [Conditional("ENABLE_LOGS")]
    public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 1f)
    {
        Debug.DrawRay(origin, direction, color, duration);
    }

    private static string GetColorString(ColorLog color)
    {
        return color switch
        {
            ColorLog.BLUE => "blue",
            ColorLog.RED => "red",
            ColorLog.GREEN => "green",
            ColorLog.YELLOW => "yellow",
            ColorLog.ORANGE => "orange",
            ColorLog.WHITE => "white",
            _ => "white"
        };
    }

    private static Color GetUnityColor(ColorLog color)
    {
        return color switch
        {
            ColorLog.BLUE => Color.blue,
            ColorLog.RED => Color.red,
            ColorLog.GREEN => Color.green,
            ColorLog.YELLOW => Color.yellow,
            ColorLog.ORANGE => new Color(1f, 0.5f, 0f) // Orange
            ,
            ColorLog.WHITE => Color.white,
            _ => Color.white
        };
    }
}
