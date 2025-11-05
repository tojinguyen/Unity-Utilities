using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Spatial audio mode for determining 2D or 3D audio behavior
/// </summary>
public enum AudioSpatialMode
{
    Audio2D,    // spatialBlend = 0, no rolloff effects
    Audio3D     // spatialBlend = 1, with rolloff settings
}

/// <summary>
/// Attribute to conditionally show/hide fields in inspector based on another field's value
/// </summary>
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string FieldToCheck { get; }
    public object CompareValue { get; }
    public bool Inverse { get; }

    public ConditionalFieldAttribute(string fieldToCheck, object compareValue, bool inverse = false)
    {
        FieldToCheck = fieldToCheck;
        CompareValue = compareValue;
        Inverse = inverse;
    }
}

/// <summary>
/// Attribute to conditionally show/hide header in inspector based on another field's value
/// </summary>
public class ConditionalHeaderAttribute : PropertyAttribute
{
    public string Text { get; }
    public string FieldToCheck { get; }
    public object CompareValue { get; }
    public bool Inverse { get; }

    public ConditionalHeaderAttribute(string text, string fieldToCheck, object compareValue, bool inverse = false)
    {
        Text = text;
        FieldToCheck = fieldToCheck;
        CompareValue = compareValue;
        Inverse = inverse;
    }
}

/// <summary>
/// Data container for audio clip configuration.
/// Used by AudioDatabase to store audio clip settings and properties.
/// 
/// Usage:
/// - Set unique ID for each audio clip
/// - Choose appropriate AudioType for volume grouping
/// - Configure volume, pitch, and 3D audio settings
/// - Enable fade effects if needed
/// - Use either direct AudioClip or Addressable reference
/// </summary>
[Serializable]
public class AudioClipData
{
    [Header("Audio Settings")]
    [Tooltip("Unique identifier for this audio clip")]
    public string id;
    [Tooltip("Category type for volume and settings management")]
    public AudioType audioType = AudioType.SFX;
    [Tooltip("How this audio clip should be played")]
    public AudioPlayMode playMode = AudioPlayMode.Once;
    
    [Header("Clip Reference")]
    [Tooltip("Direct AudioClip reference (not addressable)")]
    public AudioClip audioClip;
    [Tooltip("Addressable AudioClip reference")]
    public AssetReference audioClipReference;
    
    [Header("Volume & Pitch")]
    [Range(0f, 1f)] [Tooltip("Volume multiplier for this clip")]
    public float volume = 1f;
    [Range(0.1f, 3f)] [Tooltip("Pitch multiplier for this clip")]
    public float pitch = 1f;
    
    [Header("Spatial Settings")]
    [Tooltip("Audio spatial mode - 2D or 3D")]
    public AudioSpatialMode spatialMode = AudioSpatialMode.Audio2D;
    
    [Header("Fade Settings")]
    [Tooltip("Enable fade in/out for this clip")]
    public bool useFade = false;
    [Tooltip("Duration of fade in effect")]
    public float fadeInDuration = 0.5f;
    [Tooltip("Duration of fade out effect")]
    public float fadeOutDuration = 0.5f;
    
    [ConditionalHeader("3D Audio Settings", "spatialMode", AudioSpatialMode.Audio3D)]
    [ConditionalField("spatialMode", AudioSpatialMode.Audio3D)]
    [Tooltip("Distance at which volume starts to fade")]
    public float minDistance = 1f;
    [ConditionalField("spatialMode", AudioSpatialMode.Audio3D)]
    [Tooltip("Distance at which volume reaches minimum")]
    public float maxDistance = 50f;
    [ConditionalField("spatialMode", AudioSpatialMode.Audio3D)]
    [Tooltip("How volume fades with distance")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    
    /// <summary>
    /// Get spatial blend value based on spatial mode
    /// </summary>
    public float SpatialBlend => spatialMode == AudioSpatialMode.Audio3D ? 1f : 0f;
    
    /// <summary>
    /// Check if this audio clip uses 3D spatial audio
    /// </summary>
    public bool Is3D => spatialMode == AudioSpatialMode.Audio3D;
    
    public bool IsValid => audioClip != null || audioClipReference != null;
    
    public bool UseAddressables => audioClipReference != null && audioClipReference.RuntimeKeyIsValid();
}
