using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Header("Audio Clips")] [SerializeField]
    private List<AudioClipData> audioClips = new List<AudioClipData>();

    [Header("Category Settings")] [SerializeField]
    private List<AudioCategorySettings> categorySettings = new List<AudioCategorySettings>();

    private Dictionary<string, AudioClipData> clipLookup;
    private Dictionary<AudioType, AudioCategorySettings> categoryLookup;

    [Serializable]
    public class AudioCategorySettings
    {
        public AudioType audioType;
        [Range(0f, 1f)] public float masterVolume = 1f;
        public bool muted = false;
        public int maxConcurrentSounds = 10;
        public bool allowDuplicates = true;
    }

    public void Initialize()
    {
        BuildLookupTables();
    }

    private void BuildLookupTables()
    {
        // Build clip lookup
        clipLookup = new Dictionary<string, AudioClipData>();
        foreach (var clip in audioClips)
        {
            if (!string.IsNullOrEmpty(clip.id))
            {
                if (clipLookup.ContainsKey(clip.id))
                {
                    ConsoleLogger.LogWarning($"[AudioDatabase] Duplicate audio clip ID: {clip.id}");
                }
                else
                {
                    clipLookup[clip.id] = clip;
                }
            }
        }

        // Build category lookup
        categoryLookup = new Dictionary<AudioType, AudioCategorySettings>();
        foreach (var setting in categorySettings)
        {
            categoryLookup[setting.audioType] = setting;
        }

        // Ensure all audio types have settings
        foreach (AudioType audioType in Enum.GetValues(typeof(AudioType)))
        {
            if (!categoryLookup.ContainsKey(audioType))
            {
                var defaultSettings = new AudioCategorySettings
                {
                    audioType = audioType,
                    masterVolume = 1f,
                    muted = false,
                    maxConcurrentSounds = 10,
                    allowDuplicates = true
                };
                categorySettings.Add(defaultSettings);
                categoryLookup[audioType] = defaultSettings;
            }
        }
    }

    public AudioClipData GetAudioClip(string id)
    {
        if (clipLookup == null)
            BuildLookupTables();

        return clipLookup.TryGetValue(id, out var clip) ? clip : null;
    }

    public AudioCategorySettings GetCategorySettings(AudioType audioType)
    {
        if (categoryLookup == null)
            BuildLookupTables();

        return categoryLookup.TryGetValue(audioType, out var settings) ? settings : null;
    }

    public List<AudioClipData> GetAudioClipsByType(AudioType audioType)
    {
        var result = new List<AudioClipData>();
        foreach (var clip in audioClips)
        {
            if (clip.audioType == audioType)
                result.Add(clip);
        }

        return result;
    }

    public bool HasAudioClip(string id)
    {
        if (clipLookup == null)
            BuildLookupTables();

        return clipLookup.ContainsKey(id);
    }

    public void AddAudioClip(AudioClipData clipData)
    {
        if (clipData == null || string.IsNullOrEmpty(clipData.id))
            return;

        // Check for duplicates
        for (int i = 0; i < audioClips.Count; i++)
        {
            if (audioClips[i].id == clipData.id)
            {
                audioClips[i] = clipData;
                BuildLookupTables();
                return;
            }
        }

        audioClips.Add(clipData);
        BuildLookupTables();
    }

    public bool RemoveAudioClip(string id)
    {
        for (int i = 0; i < audioClips.Count; i++)
        {
            if (audioClips[i].id == id)
            {
                audioClips.RemoveAt(i);
                BuildLookupTables();
                return true;
            }
        }

        return false;
    }

    public void SetCategoryVolume(AudioType audioType, float volume)
    {
        var settings = GetCategorySettings(audioType);
        if (settings != null)
        {
            settings.masterVolume = Mathf.Clamp01(volume);
        }
    }

    public void SetCategoryMuted(AudioType audioType, bool muted)
    {
        var settings = GetCategorySettings(audioType);
        if (settings != null)
        {
            settings.muted = muted;
        }
    }

    public float GetCategoryVolume(AudioType audioType)
    {
        var settings = GetCategorySettings(audioType);
        return settings?.masterVolume ?? 1f;
    }

    public bool IsCategoryMuted(AudioType audioType)
    {
        var settings = GetCategorySettings(audioType);
        return settings?.muted ?? false;
    }

    public int GetMaxConcurrentSounds(AudioType audioType)
    {
        var settings = GetCategorySettings(audioType);
        return settings?.maxConcurrentSounds ?? 10;
    }

    public bool AllowDuplicates(AudioType audioType)
    {
        var settings = GetCategorySettings(audioType);
        return settings?.allowDuplicates ?? true;
    }

    private void OnValidate()
    {
        // Validate IDs are unique
        var usedIds = new HashSet<string>();
        foreach (var clip in audioClips)
        {
            if (!string.IsNullOrEmpty(clip.id))
            {
                if (usedIds.Contains(clip.id))
                {
                    ConsoleLogger.LogWarning($"[AudioDatabase] Duplicate ID found: {clip.id}");
                }
                else
                {
                    usedIds.Add(clip.id);
                }
            }
        }
    }
}