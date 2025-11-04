using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    [Range(0f, 1f)] [Tooltip("0 = 2D, 1 = 3D audio")]
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    
    [Header("Loop Settings")]
    [SerializeField] [Tooltip("Minimum delay between loop iterations")]
    private float minRandomDelay = 1f;
    [SerializeField] [Tooltip("Maximum delay between loop iterations")]
    private float maxRandomDelay = 5f;
    
    [Header("Fade Settings")]
    [Tooltip("Enable fade in/out for this clip")]
    public bool useFade = false;
    [Tooltip("Duration of fade in effect")]
    public float fadeInDuration = 0.5f;
    [Tooltip("Duration of fade out effect")]
    public float fadeOutDuration = 0.5f;
    
    [Header("3D Audio Settings")]
    [Tooltip("Distance at which volume starts to fade")]
    public float minDistance = 1f;
    [Tooltip("Distance at which volume reaches minimum")]
    public float maxDistance = 50f;
    [Tooltip("How volume fades with distance")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    
    public float GetRandomDelay() => UnityEngine.Random.Range(minRandomDelay, maxRandomDelay);
    
    public bool IsValid => audioClip != null || audioClipReference != null;
    
    public bool UseAddressables => audioClipReference != null && audioClipReference.RuntimeKeyIsValid();
}
