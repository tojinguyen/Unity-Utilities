using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class AudioClipData
{
    [Header("Audio Settings")]
    public string id;
    public AudioType audioType = AudioType.SFX;
    public AudioPlayMode playMode = AudioPlayMode.Once;
    
    [Header("Clip Reference")]
    public AudioClip audioClip;
    public AssetReference audioClipReference;
    
    [Header("Volume & Pitch")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    
    [Header("Loop Settings")]
    [SerializeField] private float minRandomDelay = 1f;
    [SerializeField] private float maxRandomDelay = 5f;
    
    [Header("Fade Settings")]
    public bool useFade = false;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    
    [Header("3D Audio Settings")]
    public float minDistance = 1f;
    public float maxDistance = 50f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    
    public float GetRandomDelay() => UnityEngine.Random.Range(minRandomDelay, maxRandomDelay);
    
    public bool IsValid => audioClip != null || audioClipReference != null;
    
    public bool UseAddressables => audioClipReference != null && audioClipReference.RuntimeKeyIsValid();
}
