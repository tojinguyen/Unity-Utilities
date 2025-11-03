using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class AudioExampleController : MonoBehaviour
{
    [Header("Audio Database")]
    public AudioDatabase audioDatabase;
    
    [Header("BGM Controls")]
    public Button playMenuThemeButton;
    public Button playBattleThemeButton;
    public Button stopBGMButton;
    public Slider bgmVolumeSlider;
    public TextMeshProUGUI bgmVolumeText;
    
    [Header("SFX Controls")]
    public Button buttonClickButton;
    public Button explosionButton;
    public Button jumpButton;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;
    
    [Header("Advanced Controls")]
    public Button crossfadeButton;
    public Button sound3DButton;
    public Button stopAllButton;
    public Toggle masterMuteToggle;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    
    [Header("Status Display")]
    public TextMeshProUGUI statusText;
    
    private void Start()
    {
        InitializeAudioManager();
        SetupUI();
        SubscribeToEvents();
    }
    
    private void InitializeAudioManager()
    {
        // Initialize AudioManager with database
        AudioManager.Initialize(audioDatabase);
        
        // Load saved volumes
        bgmVolumeSlider.value = AudioManager.GetCategoryVolume(AudioType.Music);
        sfxVolumeSlider.value = AudioManager.GetCategoryVolume(AudioType.SFX);
        masterVolumeSlider.value = AudioManager.MasterVolume;
        masterMuteToggle.isOn = AudioManager.MasterMuted;
        
        UpdateVolumeTexts();
        UpdateStatus("Audio Manager Initialized");
    }
    
    private void SetupUI()
    {
        // BGM Buttons
        playMenuThemeButton.onClick.AddListener(() => PlayBGM("menuTheme"));
        playBattleThemeButton.onClick.AddListener(() => PlayBGM("battleTheme"));
        stopBGMButton.onClick.AddListener(StopBGM);
        
        // SFX Buttons
        buttonClickButton.onClick.AddListener(() => PlaySFX("buttonClick"));
        explosionButton.onClick.AddListener(() => PlaySFX("explosion"));
        jumpButton.onClick.AddListener(() => PlaySFX("jump"));
        
        // Advanced Buttons
        crossfadeButton.onClick.AddListener(CrossfadeToBattle);
        sound3DButton.onClick.AddListener(Play3DSound);
        stopAllButton.onClick.AddListener(StopAllAudio);
        
        // Volume Controls
        bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        masterMuteToggle.onValueChanged.AddListener(SetMasterMute);
    }
    
    private async void PlayBGM(string audioId)
    {
        await AudioManager.PlayBGM(audioId);
        UpdateStatus($"Playing BGM: {audioId}");
    }
    
    private async void PlaySFX(string audioId)
    {
        await AudioManager.PlaySFX(audioId);
        UpdateStatus($"Played SFX: {audioId}");
    }
    
    private async void StopBGM()
    {
        await AudioManager.StopBGM();
        UpdateStatus("Stopped BGM");
    }
    
    private async void CrossfadeToBattle()
    {
        await AudioManager.PlayMusicAsync("battleTheme", crossFade: true);
        UpdateStatus("Crossfaded to Battle Theme");
    }
    
    private async void Play3DSound()
    {
        Vector3 randomPos = Random.insideUnitSphere * 5f;
        await AudioManager.PlaySFXAtPosition("explosion", randomPos);
        UpdateStatus($"3D Sound at position: {randomPos}");
    }
    
    private void StopAllAudio()
    {
        AudioManager.StopAllAudio();
        UpdateStatus("Stopped all audio");
    }
    
    private void SetBGMVolume(float volume)
    {
        AudioManager.SetCategoryVolume(AudioType.Music, volume);
        UpdateVolumeTexts();
    }
    
    private void SetSFXVolume(float volume)
    {
        AudioManager.SetCategoryVolume(AudioType.SFX, volume);
        UpdateVolumeTexts();
    }
    
    private void SetMasterVolume(float volume)
    {
        AudioManager.MasterVolume = volume;
        UpdateVolumeTexts();
    }
    
    private void SetMasterMute(bool muted)
    {
        AudioManager.MasterMuted = muted;
        UpdateStatus($"Master Mute: {muted}");
    }
    
    private void UpdateVolumeTexts()
    {
        bgmVolumeText.text = $"BGM: {bgmVolumeSlider.value:F2}";
        sfxVolumeText.text = $"SFX: {sfxVolumeSlider.value:F2}";
        masterVolumeText.text = $"Master: {masterVolumeSlider.value:F2}";
    }
    
    private void UpdateStatus(string message)
    {
        statusText.text = $"Status: {message}";
        Debug.Log($"[AudioExample] {message}");
    }
    
    private void SubscribeToEvents()
    {
        AudioManager.OnAudioStarted += OnAudioStarted;
        AudioManager.OnAudioStopped += OnAudioStopped;
    }
    
    private void OnAudioStarted(string audioId, AudioType audioType)
    {
        UpdateStatus($"Started: {audioId} ({audioType})");
    }
    
    private void OnAudioStopped(string audioId, AudioType audioType)
    {
        UpdateStatus($"Stopped: {audioId} ({audioType})");
    }
    
    private void OnDestroy()
    {
        AudioManager.OnAudioStarted -= OnAudioStarted;
        AudioManager.OnAudioStopped -= OnAudioStopped;
    }
}