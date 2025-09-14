using Cysharp.Threading.Tasks;

public static class AudioService
{
    private static AudioManager audioManager;
    
    private static AudioManager AudioManager
    {
        get
        {
            if (audioManager == null)
                audioManager = AudioManager.Instance;
            return audioManager;
        }
    }
    
    #region Audio Playback
    
    /// <summary>
    /// Play an audio clip by ID
    /// </summary>
    public static UniTask<bool> PlayAsync(string audioId, float volumeMultiplier = 1f)
    {
        return AudioManager.PlayAudioAsync(audioId, null, volumeMultiplier);
    }
    
    /// <summary>
    /// Play an audio clip at a specific 3D position
    /// </summary>
    public static UniTask<bool> PlayAtPositionAsync(string audioId, UnityEngine.Vector3 position, float volumeMultiplier = 1f)
    {
        return AudioManager.PlayAudioAsync(audioId, position, volumeMultiplier);
    }
    
    /// <summary>
    /// Play music with optional crossfade
    /// </summary>
    public static UniTask<bool> PlayMusicAsync(string musicId, float volumeMultiplier = 1f, bool crossFade = true)
    {
        return AudioManager.PlayMusicAsync(musicId, volumeMultiplier, crossFade);
    }
    
    /// <summary>
    /// Stop a specific audio clip
    /// </summary>
    public static UniTask StopAsync(string audioId, bool immediate = false)
    {
        return AudioManager.StopAudioAsync(audioId, immediate);
    }
    
    /// <summary>
    /// Stop all audio of a specific type
    /// </summary>
    public static UniTask StopAllAsync(AudioType? audioType = null, bool immediate = false)
    {
        return AudioManager.StopAllAudioAsync(audioType, immediate);
    }
    
    /// <summary>
    /// Stop currently playing music
    /// </summary>
    public static UniTask StopMusicAsync(bool immediate = false)
    {
        return AudioManager.StopMusicAsync(immediate);
    }
    
    #endregion
    
    #region Audio Control
    
    /// <summary>
    /// Pause a specific audio clip
    /// </summary>
    public static void Pause(string audioId)
    {
        AudioManager.PauseAudio(audioId);
    }
    
    /// <summary>
    /// Resume a specific audio clip
    /// </summary>
    public static void Resume(string audioId)
    {
        AudioManager.ResumeAudio(audioId);
    }
    
    /// <summary>
    /// Pause all audio or audio of a specific type
    /// </summary>
    public static void PauseAll(AudioType? audioType = null)
    {
        AudioManager.PauseAllAudio(audioType);
    }
    
    /// <summary>
    /// Resume all audio or audio of a specific type
    /// </summary>
    public static void ResumeAll(AudioType? audioType = null)
    {
        AudioManager.ResumeAllAudio(audioType);
    }
    
    #endregion
    
    #region Volume Control
    
    /// <summary>
    /// Set master volume (0-1)
    /// </summary>
    public static void SetMasterVolume(float volume)
    {
        if (AudioManager != null)
            AudioManager.MasterVolume = volume;
    }
    
    /// <summary>
    /// Get master volume
    /// </summary>
    public static float GetMasterVolume()
    {
        return AudioManager?.MasterVolume ?? 1f;
    }
    
    /// <summary>
    /// Set master mute state
    /// </summary>
    public static void SetMasterMuted(bool muted)
    {
        if (AudioManager != null)
            AudioManager.MasterMuted = muted;
    }
    
    /// <summary>
    /// Get master mute state
    /// </summary>
    public static bool IsMasterMuted()
    {
        return AudioManager?.MasterMuted ?? false;
    }
    
    /// <summary>
    /// Set volume for a specific audio category
    /// </summary>
    public static void SetCategoryVolume(AudioType audioType, float volume)
    {
        AudioManager.SetCategoryVolume(audioType, volume);
    }
    
    /// <summary>
    /// Get volume for a specific audio category
    /// </summary>
    public static float GetCategoryVolume(AudioType audioType)
    {
        return AudioManager.GetCategoryVolume(audioType);
    }
    
    /// <summary>
    /// Set mute state for a specific audio category
    /// </summary>
    public static void SetCategoryMuted(AudioType audioType, bool muted)
    {
        AudioManager.SetCategoryMuted(audioType, muted);
    }
    
    /// <summary>
    /// Get mute state for a specific audio category
    /// </summary>
    public static bool IsCategoryMuted(AudioType audioType)
    {
        return AudioManager.IsCategoryMuted(audioType);
    }
    
    #endregion
    
    #region Audio Info
    
    /// <summary>
    /// Check if a specific audio is currently playing
    /// </summary>
    public static bool IsPlaying(string audioId)
    {
        return AudioManager.IsAudioPlaying(audioId);
    }
    
    /// <summary>
    /// Check if music is currently playing
    /// </summary>
    public static bool IsMusicPlaying()
    {
        return AudioManager.IsMusicPlaying;
    }
    
    /// <summary>
    /// Get the ID of currently playing music
    /// </summary>
    public static string GetCurrentMusicId()
    {
        return AudioManager.CurrentMusicId;
    }
    
    /// <summary>
    /// Get the number of active audio sources
    /// </summary>
    public static int GetActiveAudioCount(AudioType? audioType = null)
    {
        return AudioManager.GetActiveAudioCount(audioType);
    }
    
    #endregion
    
    #region Common Audio Operations
    
    /// <summary>
    /// Play a UI sound effect
    /// </summary>
    public static UniTask<bool> PlayUIAsync(string audioId, float volume = 1f)
    {
        return PlayAsync(audioId, volume);
    }
    
    /// <summary>
    /// Play a sound effect
    /// </summary>
    public static UniTask<bool> PlaySFXAsync(string audioId, float volume = 1f)
    {
        return PlayAsync(audioId, volume);
    }
    
    /// <summary>
    /// Play a sound effect at a 3D position
    /// </summary>
    public static UniTask<bool> PlaySFXAtAsync(string audioId, UnityEngine.Vector3 position, float volume = 1f)
    {
        return PlayAtPositionAsync(audioId, position, volume);
    }
    
    /// <summary>
    /// Play ambient sound
    /// </summary>
    public static UniTask<bool> PlayAmbientAsync(string audioId, float volume = 1f)
    {
        return PlayAsync(audioId, volume);
    }
    
    /// <summary>
    /// Play voice line
    /// </summary>
    public static UniTask<bool> PlayVoiceAsync(string audioId, float volume = 1f)
    {
        return PlayAsync(audioId, volume);
    }
    
    /// <summary>
    /// Stop all SFX sounds
    /// </summary>
    public static UniTask StopAllSFXAsync(bool immediate = false)
    {
        return StopAllAsync(AudioType.SFX, immediate);
    }
    
    /// <summary>
    /// Stop all UI sounds
    /// </summary>
    public static UniTask StopAllUIAsync(bool immediate = false)
    {
        return StopAllAsync(AudioType.UI, immediate);
    }
    
    /// <summary>
    /// Fade out and stop all audio
    /// </summary>
    public static UniTask FadeOutAllAsync()
    {
        return StopAllAsync(immediate: false);
    }
    
    #endregion
}
