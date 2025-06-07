using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioManager : MonoSingleton<AudioManager>
{
    [Header("Configuration")]
    [SerializeField] private AudioDatabase audioDatabase;
    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
    
    [Header("Master Settings")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [SerializeField] private bool masterMuted = false;
    
    [Header("Runtime Info")]
    [SerializeField] private int activeAudioSourcesCounter = 0;
    [SerializeField] private int pooledAudioSources = 0;
    
    // Audio source pooling
    private readonly Queue<AudioSourceController> audioSourcePool = new Queue<AudioSourceController>();
    private readonly List<AudioSourceController> activeAudioSources = new List<AudioSourceController>();
    private readonly Dictionary<AudioType, List<AudioSourceController>> activeAudioByType = new Dictionary<AudioType, List<AudioSourceController>>();
    
    // Current music tracking
    private AudioSourceController currentMusicSource;
    private string currentMusicId;
    
    // Events
    public event Action<string, AudioType> OnAudioStarted;
    public event Action<string, AudioType> OnAudioStopped;
    public event Action<AudioType, float> OnCategoryVolumeChanged;
    public event Action<float> OnMasterVolumeChanged;
    
    // Properties
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyMasterVolumeToAll();
            OnMasterVolumeChanged?.Invoke(masterVolume);
        }
    }
    
    public bool MasterMuted
    {
        get => masterMuted;
        set
        {
            masterMuted = value;
            ApplyMasterVolumeToAll();
        }
    }
    
    public AudioDatabase Database => audioDatabase;
    public string CurrentMusicId => currentMusicId;
    public bool IsMusicPlaying => currentMusicSource != null && currentMusicSource.IsPlaying;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        if (audioDatabase != null)
        {
            audioDatabase.Initialize();
        }
        
        // Initialize audio type tracking
        foreach (AudioType audioType in Enum.GetValues(typeof(AudioType)))
        {
            activeAudioByType[audioType] = new List<AudioSourceController>();
        }
        
        // Pre-warm the pool
        PrewarmPool();
        
        ConsoleLogger.Log($"[AudioManager] Initialized with {initialPoolSize} audio sources");
    }
    
    private void PrewarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var audioSourceController = CreateAudioSourceController();
            audioSourceController.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSourceController);
        }
        pooledAudioSources = audioSourcePool.Count;
    }
    
    private AudioSourceController CreateAudioSourceController()
    {
        GameObject go;
        
        if (audioSourcePrefab != null)
        {
            go = Instantiate(audioSourcePrefab, transform);
        }
        else
        {
            go = new GameObject($"AudioSource_{DateTime.Now.Ticks}");
            go.transform.SetParent(transform);
            go.AddComponent<AudioSourceController>();
        }
        
        var controller = go.GetComponent<AudioSourceController>();
        if (controller == null)
        {
            controller = go.AddComponent<AudioSourceController>();
        }
        
        // Subscribe to events
        controller.OnAudioFinished += OnAudioSourceFinished;
        
        return controller;
    }
    
    private AudioSourceController GetAudioSourceFromPool()
    {
        AudioSourceController controller;
        
        if (audioSourcePool.Count > 0)
        {
            controller = audioSourcePool.Dequeue();
            pooledAudioSources = audioSourcePool.Count;
        }
        else if (activeAudioSources.Count < maxPoolSize)
        {
            controller = CreateAudioSourceController();
        }
        else
        {
            // Pool is full, stop the oldest non-music audio
            controller = GetOldestNonMusicAudioSource();
            if (controller != null)
            {
                controller.StopAsync(immediate: true).Forget();
            }
            else
            {
                ConsoleLogger.LogWarning("[AudioManager] No available audio sources and can't stop any");
                return null;
            }
        }
        
        controller.gameObject.SetActive(true);
        controller.Reset();
        
        return controller;
    }
    
    private void ReturnAudioSourceToPool(AudioSourceController controller)
    {
        if (controller == null) return;
        
        // Remove from active lists
        activeAudioSources.Remove(controller);
        foreach (var list in activeAudioByType.Values)
        {
            list.Remove(controller);
        }
        
        // Reset and return to pool
        controller.Reset();
        controller.gameObject.SetActive(false);
        audioSourcePool.Enqueue(controller);
        
        // Update counters
        activeAudioSourcesCounter = this.activeAudioSources.Count;
        pooledAudioSources = audioSourcePool.Count;
    }
    
    private AudioSourceController GetOldestNonMusicAudioSource()
    {
        return activeAudioSources.FirstOrDefault(a => a.CurrentAudioType != AudioType.Music);
    }
    
    private void OnAudioSourceFinished(AudioSourceController controller)
    {
        // If this is the current music, clear the reference
        if (controller == currentMusicSource)
        {
            currentMusicSource = null;
            currentMusicId = null;
        }
        
        OnAudioStopped?.Invoke(controller.CurrentClipId, controller.CurrentAudioType);
        ReturnAudioSourceToPool(controller);
    }
    
    #region Public API
    
    public async UniTask<bool> PlayAudioAsync(string audioId, Vector3? position = null, float volumeMultiplier = 1f)
    {
        if (audioDatabase == null)
        {
            ConsoleLogger.LogError("[AudioManager] Audio database is not assigned");
            return false;
        }
        
        var clipData = audioDatabase.GetAudioClip(audioId);
        if (clipData == null)
        {
            ConsoleLogger.LogError($"[AudioManager] Audio clip not found: {audioId}");
            return false;
        }
        
        // Check if audio type is muted
        if (audioDatabase.IsCategoryMuted(clipData.audioType) || masterMuted)
        {
            ConsoleLogger.Log($"[AudioManager] Audio category {clipData.audioType} is muted, skipping: {audioId}");
            return false;
        }
        
        // Check concurrent limit
        var activeOfType = activeAudioByType[clipData.audioType];
        var maxConcurrent = audioDatabase.GetMaxConcurrentSounds(clipData.audioType);
        
        if (activeOfType.Count >= maxConcurrent)
        {
            // Stop oldest sound of this type
            var oldest = activeOfType.FirstOrDefault();
            if (oldest != null)
            {
                oldest.StopAsync(immediate: true).Forget();
            }
        }
        
        // Check for duplicates
        if (!audioDatabase.AllowDuplicates(clipData.audioType))
        {
            var existing = activeOfType.FirstOrDefault(a => a.CurrentClipId == audioId);
            if (existing != null)
            {
                ConsoleLogger.Log($"[AudioManager] Duplicate audio not allowed: {audioId}");
                return false;
            }
        }
        
        // Handle music specifically
        if (clipData.audioType == AudioType.Music)
        {
            return await PlayMusicAsync(audioId, volumeMultiplier);
        }
        
        // Get audio source from pool
        var audioSource = GetAudioSourceFromPool();
        if (audioSource == null)
        {
            ConsoleLogger.LogError($"[AudioManager] Failed to get audio source for: {audioId}");
            return false;
        }
        
        // Apply volume multipliers
        var finalClipData = CloneClipData(clipData);
        finalClipData.volume *= volumeMultiplier * audioDatabase.GetCategoryVolume(clipData.audioType) * masterVolume;
        
        // Initialize and play
        audioSource.Initialize(finalClipData, position);
        
        // Add to tracking lists
        activeAudioSources.Add(audioSource);
        activeAudioByType[clipData.audioType].Add(audioSource);
        
        // Update counter
        activeAudioSourcesCounter = activeAudioSources.Count;
        
        // Play the audio
        await audioSource.PlayAsync();
        
        OnAudioStarted?.Invoke(audioId, clipData.audioType);
        ConsoleLogger.Log($"[AudioManager] Playing audio: {audioId}");
        
        return true;
    }
    
    public async UniTask<bool> PlayMusicAsync(string musicId, float volumeMultiplier = 1f, bool crossFade = true)
    {
        if (audioDatabase == null) return false;
        
        var clipData = audioDatabase.GetAudioClip(musicId);
        if (clipData == null || clipData.audioType != AudioType.Music)
        {
            ConsoleLogger.LogError($"[AudioManager] Music clip not found or not marked as music: {musicId}");
            return false;
        }
        
        // Check if already playing this music
        if (currentMusicId == musicId && currentMusicSource != null && currentMusicSource.IsPlaying)
        {
            ConsoleLogger.Log($"[AudioManager] Music already playing: {musicId}");
            return true;
        }
        
        var newMusicSource = GetAudioSourceFromPool();
        if (newMusicSource == null) return false;
        
        // Apply volume multipliers
        var finalClipData = CloneClipData(clipData);
        finalClipData.volume *= volumeMultiplier * audioDatabase.GetCategoryVolume(AudioType.Music) * masterVolume;
        
        // Handle crossfade
        if (currentMusicSource != null && currentMusicSource.IsPlaying && crossFade)
        {
            var fadeOutTask = currentMusicSource.StopAsync();
            
            // Initialize new music
            newMusicSource.Initialize(finalClipData);
            activeAudioSources.Add(newMusicSource);
            activeAudioByType[AudioType.Music].Add(newMusicSource);
            
            // Start new music
            var playTask = newMusicSource.PlayAsync();
            
            // Wait for both to complete
            await UniTask.WhenAll(fadeOutTask, playTask);
        }
        else
        {
            // Stop current music immediately
            if (currentMusicSource != null)
            {
                await currentMusicSource.StopAsync(immediate: true);
            }
            
            // Initialize and play new music
            newMusicSource.Initialize(finalClipData);
            activeAudioSources.Add(newMusicSource);
            activeAudioByType[AudioType.Music].Add(newMusicSource);
            await newMusicSource.PlayAsync();
        }
        
        // Update current music tracking
        currentMusicSource = newMusicSource;
        currentMusicId = musicId;
        activeAudioSourcesCounter = activeAudioSources.Count;
        
        OnAudioStarted?.Invoke(musicId, AudioType.Music);
        ConsoleLogger.Log($"[AudioManager] Playing music: {musicId}");
        
        return true;
    }
    
    public async UniTask StopAudioAsync(string audioId, bool immediate = false)
    {
        var audioSource = activeAudioSources.FirstOrDefault(a => a.CurrentClipId == audioId);
        if (audioSource != null)
        {
            await audioSource.StopAsync(immediate);
        }
    }
    
    public async UniTask StopAllAudioAsync(AudioType? audioType = null, bool immediate = false)
    {
        var audioSourcesToStop = audioType.HasValue 
            ? activeAudioByType[audioType.Value].ToList()
            : activeAudioSources.ToList();
        
        var stopTasks = audioSourcesToStop.Select(a => a.StopAsync(immediate));
        await UniTask.WhenAll(stopTasks);
    }
    
    public async UniTask StopMusicAsync(bool immediate = false)
    {
        if (currentMusicSource != null)
        {
            await currentMusicSource.StopAsync(immediate);
        }
    }
    
    public void PauseAudio(string audioId)
    {
        var audioSource = activeAudioSources.FirstOrDefault(a => a.CurrentClipId == audioId);
        audioSource?.Pause();
    }
    
    public void ResumeAudio(string audioId)
    {
        var audioSource = activeAudioSources.FirstOrDefault(a => a.CurrentClipId == audioId);
        audioSource?.Resume();
    }
    
    public void PauseAllAudio(AudioType? audioType = null)
    {
        var audioSources = audioType.HasValue 
            ? activeAudioByType[audioType.Value]
            : activeAudioSources;
        
        foreach (var audioSource in audioSources)
        {
            audioSource.Pause();
        }
    }
    
    public void ResumeAllAudio(AudioType? audioType = null)
    {
        var audioSources = audioType.HasValue 
            ? activeAudioByType[audioType.Value]
            : activeAudioSources;
        
        foreach (var audioSource in audioSources)
        {
            audioSource.Resume();
        }
    }
    
    public void SetCategoryVolume(AudioType audioType, float volume)
    {
        if (audioDatabase == null) return;
        
        audioDatabase.SetCategoryVolume(audioType, volume);
        
        // Apply to all active audio of this type
        foreach (var audioSource in activeAudioByType[audioType])
        {
            var categoryVolume = audioDatabase.GetCategoryVolume(audioType);
            audioSource.SetVolume(categoryVolume * masterVolume);
        }
        
        OnCategoryVolumeChanged?.Invoke(audioType, volume);
    }
    
    public void SetCategoryMuted(AudioType audioType, bool muted)
    {
        if (audioDatabase == null) return;
        
        audioDatabase.SetCategoryMuted(audioType, muted);
        
        // Apply to all active audio of this type
        foreach (var audioSource in activeAudioByType[audioType])
        {
            if (muted || masterMuted)
            {
                audioSource.SetVolume(0f);
            }
            else
            {
                var categoryVolume = audioDatabase.GetCategoryVolume(audioType);
                audioSource.SetVolume(categoryVolume * masterVolume);
            }
        }
    }
    
    public float GetCategoryVolume(AudioType audioType)
    {
        return audioDatabase?.GetCategoryVolume(audioType) ?? 1f;
    }
    
    public bool IsCategoryMuted(AudioType audioType)
    {
        return audioDatabase?.IsCategoryMuted(audioType) ?? false;
    }
    
    public bool IsAudioPlaying(string audioId)
    {
        return activeAudioSources.Any(a => a.CurrentClipId == audioId && a.IsPlaying);
    }
    
    public int GetActiveAudioCount(AudioType? audioType = null)
    {
        return audioType.HasValue 
            ? activeAudioByType[audioType.Value].Count
            : activeAudioSources.Count;
    }
    
    #endregion
    
    #region Helper Methods
    
    private AudioClipData CloneClipData(AudioClipData original)
    {
        return new AudioClipData
        {
            id = original.id,
            audioType = original.audioType,
            playMode = original.playMode,
            audioClip = original.audioClip,
            audioClipReference = original.audioClipReference,
            volume = original.volume,
            pitch = original.pitch,
            spatialBlend = original.spatialBlend,
            useFade = original.useFade,
            fadeInDuration = original.fadeInDuration,
            fadeOutDuration = original.fadeOutDuration,
            minDistance = original.minDistance,
            maxDistance = original.maxDistance,
            rolloffMode = original.rolloffMode
        };
    }
    
    private void ApplyMasterVolumeToAll()
    {
        foreach (var audioSource in activeAudioSources)
        {
            var audioType = audioSource.CurrentAudioType;
            var categoryVolume = audioDatabase?.GetCategoryVolume(audioType) ?? 1f;
            var isCategoryMuted = audioDatabase?.IsCategoryMuted(audioType) ?? false;
            
            if (masterMuted || isCategoryMuted)
            {
                audioSource.SetVolume(0f);
            }
            else
            {
                audioSource.SetVolume(categoryVolume * masterVolume);
            }
        }
    }
    
    #endregion
    
    #region Unity Events
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PauseAllAudio();
        }
        else
        {
            ResumeAllAudio();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PauseAllAudio();
        }
        else
        {
            ResumeAllAudio();
        }
    }
    
    protected override void OnDestroy()
    {
        // Stop all audio
        StopAllAudioAsync(immediate: true).Forget();
        
        // Clear events
        OnAudioStarted = null;
        OnAudioStopped = null;
        OnCategoryVolumeChanged = null;
        OnMasterVolumeChanged = null;
        
        base.OnDestroy();
    }
    
    #endregion
}
