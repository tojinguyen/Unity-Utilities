using System;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioSourceController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Runtime Info")]
    [SerializeField] private string currentClipId;
    [SerializeField] private AudioType currentAudioType;
    [SerializeField] private bool isPlaying;
    [SerializeField] private bool isFading;
    [SerializeField] private float targetVolume;
    [SerializeField] private float originalVolume;
    
    // Events
    public event Action<AudioSourceController> OnAudioFinished;
    public event Action<AudioSourceController> OnFadeComplete;
    
    // Properties
    public AudioSource AudioSource => audioSource;
    public string CurrentClipId => currentClipId;
    public AudioType CurrentAudioType => currentAudioType;
    public bool IsPlaying => isPlaying && audioSource.isPlaying;
    public bool IsFading => isFading;
    public float Volume => audioSource.volume;
    public float OriginalVolume => originalVolume;
    
    private Coroutine fadeCoroutine;
    private Coroutine playCoroutine;
    private AudioClipData currentClipData;
    
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        Reset();
    }
    
    private void Update()
    {
        // Check if audio finished playing
        if (isPlaying && !audioSource.isPlaying && !audioSource.loop)
        {
            OnAudioComplete();
        }
    }
    
    public void Initialize(AudioClipData clipData, Vector3? position = null)
    {
        if (clipData == null || !clipData.IsValid)
        {
            ConsoleLogger.LogError($"[AudioSourceController] Invalid audio clip data");
            return;
        }
        
        currentClipData = clipData;
        currentClipId = clipData.id;
        currentAudioType = clipData.audioType;
        
        // Configure AudioSource
        audioSource.clip = clipData.audioClip;
        audioSource.volume = clipData.volume;
        audioSource.pitch = clipData.pitch;
        audioSource.spatialBlend = clipData.SpatialBlend;
        audioSource.minDistance = clipData.minDistance;
        audioSource.maxDistance = clipData.maxDistance;
        audioSource.rolloffMode = clipData.rolloffMode;
        audioSource.loop = clipData.playMode == AudioPlayMode.Loop;
        
        originalVolume = clipData.volume;
        targetVolume = clipData.volume;
        
        // Set position for 3D audio
        if (position.HasValue && clipData.Is3D)
        {
            transform.position = position.Value;
        }
        
        if (AudioManager.EnableAudioLogs)
        {
            ConsoleLogger.Log($"[AudioSourceController] Initialized audio: {clipData.id}");
        }
    }
    
    public async UniTask PlayAsync()
    {
        if (currentClipData == null)
        {
            ConsoleLogger.LogError($"[AudioSourceController] No clip data available for playback");
            return;
        }
        
        // Load clip from Addressables if needed
        if (currentClipData.UseAddressables && audioSource.clip == null)
        {
            try
            {
                var clip = await AddressableHelper.GetAssetAsync<AudioClip>(
                    currentClipData.audioClipReference, 
                    $"AudioSystem_{currentClipData.audioType}"
                );
                audioSource.clip = clip;
            }
            catch (Exception e)
            {
                ConsoleLogger.LogError($"[AudioSourceController] Failed to load audio clip: {e.Message}");
                return;
            }
        }
        
        if (audioSource.clip == null)
        {
            ConsoleLogger.LogError($"[AudioSourceController] No audio clip available");
            return;
        }
        
        isPlaying = true;
        
        // Apply fade in if needed
        if (currentClipData.useFade && currentClipData.fadeInDuration > 0f)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            await FadeToAsync(originalVolume, currentClipData.fadeInDuration);
        }
        else
        {
            audioSource.Play();
        }
        
        if (AudioManager.EnableAudioLogs)
        {
            ConsoleLogger.Log($"[AudioSourceController] Started playing: {currentClipId}");
        }
    }
    
    public async UniTask StopAsync(bool immediate = false)
    {
        if (!isPlaying) return;
        
        // Stop any ongoing coroutines
        StopAllCoroutines();
        
        if (immediate || !currentClipData.useFade || currentClipData.fadeOutDuration <= 0f)
        {
            audioSource.Stop();
            OnAudioComplete();
        }
        else
        {
            await FadeToAsync(0f, currentClipData.fadeOutDuration);
            audioSource.Stop();
            OnAudioComplete();
        }
        
        if (AudioManager.EnableAudioLogs)
        {
            ConsoleLogger.Log($"[AudioSourceController] Stopped playing: {currentClipId}");
        }
    }
    
    public async UniTask FadeToAsync(float targetVol, float duration)
    {
        if (duration <= 0f)
        {
            audioSource.volume = targetVol;
            targetVolume = targetVol;
            return;
        }
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
            
        fadeCoroutine = StartCoroutine(FadeCoroutine(targetVol, duration));
        
        // Wait for fade to complete
        while (isFading)
        {
            await UniTask.Yield();
        }
    }
    
    private IEnumerator FadeCoroutine(float targetVol, float duration)
    {
        isFading = true;
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVol, progress);
            yield return null;
        }
        
        audioSource.volume = targetVol;
        targetVolume = targetVol;
        isFading = false;
        fadeCoroutine = null;
        
        OnFadeComplete?.Invoke(this);
    }
    
    public void Pause()
    {
        if (isPlaying && audioSource.isPlaying)
        {
            audioSource.Pause();
            if (AudioManager.EnableAudioLogs)
            {
                ConsoleLogger.Log($"[AudioSourceController] Paused: {currentClipId}");
            }
        }
    }
    
    public void Resume()
    {
        if (isPlaying && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            if (AudioManager.EnableAudioLogs)
            {
                ConsoleLogger.Log($"[AudioSourceController] Resumed: {currentClipId}");
            }
        }
    }
    
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (!isFading)
        {
            audioSource.volume = volume * originalVolume;
            targetVolume = volume * originalVolume;
        }
    }
    
    public void SetPitch(float pitch)
    {
        audioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    }
    
    private void OnAudioComplete()
    {
        isPlaying = false;
        
        // Stop coroutines
        StopAllCoroutines();
        fadeCoroutine = null;
        playCoroutine = null;
        
        OnAudioFinished?.Invoke(this);
        
        if (AudioManager.EnableAudioLogs)
        {
            ConsoleLogger.Log($"[AudioSourceController] Audio finished: {currentClipId}");
        }
    }
    
    public void Reset()
    {
        // Stop everything
        StopAllCoroutines();
        
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
        }
        
        // Reset state
        currentClipId = string.Empty;
        currentAudioType = AudioType.SFX;
        isPlaying = false;
        isFading = false;
        targetVolume = 1f;
        originalVolume = 1f;
        currentClipData = null;
        fadeCoroutine = null;
        playCoroutine = null;
        
        // Don't clear event handlers - they are managed by AudioManager
        // OnAudioFinished and OnFadeComplete should remain subscribed
    }
    
    /// <summary>
    /// Cleanup all event handlers (called when destroying object)
    /// </summary>
    public void Cleanup()
    {
        Reset();
        
        // Clear all event handlers
        OnAudioFinished = null;
        OnFadeComplete = null;
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
}
