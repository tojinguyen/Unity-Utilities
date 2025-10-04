// Data/Example/Scripts/SampleUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TirexGame.Utils.Data;
using TirexGame.Utils.Data.Examples;
using TMPro; 
using System;

public class SampleUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField levelInput;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text lastLoginText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button takeDamageButton;
    [SerializeField] private Button saveInvalidButton;

    private TirexExamplePlayerData _playerData;
    private const string DefaultPlayerKey = "TirexExamplePlayerData";

    private void Start()
    {
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        takeDamageButton.onClick.AddListener(OnTakeDamageButtonClicked);
        saveInvalidButton.onClick.AddListener(OnSaveInvalidDataClicked);

        DataManager.SubscribeToDataEvents<TirexExamplePlayerData>(
            onSaved: OnPlayerDataSaved,
            onLoaded: OnPlayerDataLoaded,
            onDeleted: OnPlayerDataDeleted
        );
        
        OnLoadButtonClicked();
    }

    private void OnDestroy()
    {
        DataManager.UnsubscribeFromDataEvents<TirexExamplePlayerData>(
            onSaved: OnPlayerDataSaved,
            onLoaded: OnPlayerDataLoaded,
            onDeleted: OnPlayerDataDeleted
        );
    }

    private async void OnLoadButtonClicked()
    {
        SetStatus("Loading data...", Color.yellow);
        
        _playerData = await DataManager.GetDataAsync<TirexExamplePlayerData>(DefaultPlayerKey);
        
        UpdateUI();
        
        SetStatus("Data loaded successfully!", Color.green);
    }

    private async void OnSaveButtonClicked()
    {
        SetStatus("Saving data...", Color.yellow);
        
        UpdateDataFromUI();

        var success = await DataManager.SaveDataAsync(_playerData, DefaultPlayerKey);

        if (success)
        {
            SetStatus("Data saved successfully!", Color.green);
        }
        else
        {
            SetStatus("Failed to save data. Check console for errors.", Color.red);
        }
    }

    private async void OnDeleteButtonClicked()
    {
        SetStatus("Deleting data...", Color.yellow);
        var success = await DataManager.DeleteDataAsync<TirexExamplePlayerData>(DefaultPlayerKey);

        if (success)
        {
            SetStatus("Data deleted. Loading default data.", new Color(1, 0.5f, 0)); // Orange
            OnLoadButtonClicked();
        }
        else
        {
            SetStatus("Failed to delete data.", Color.red);
        }
    }
    
    private void OnLevelUpButtonClicked()
    {
        if (_playerData == null) return;
        _playerData.Level++;
        UpdateUI();
        SetStatus("Level increased. Click Save to persist changes.", Color.cyan);
    }
    
    private void OnTakeDamageButtonClicked()
    {
        if (_playerData == null) return;
        _playerData.Health = Mathf.Max(0, _playerData.Health - 10);
        UpdateUI();
        SetStatus("Took damage. Click Save to persist changes.", Color.cyan);
    }
    
    private void OnSaveInvalidDataClicked()
    {
        if (_playerData == null) return;
        
        playerNameInput.text = "A"; 
        levelInput.text = "101";
        
        Debug.LogWarning("Attempting to save invalid data to demonstrate validation...");
        OnSaveButtonClicked();
    }

    private void UpdateUI()
    {
        if (_playerData == null) return;

        playerNameInput.text = _playerData.PlayerName;
        levelInput.text = _playerData.Level.ToString();
        healthSlider.value = _playerData.Health / 100f;
        healthText.text = $"{_playerData.Health:F0}/100";
        lastLoginText.text = $"Last Login: {_playerData.LastLogin:yyyy-MM-dd HH:mm:ss} UTC";
    }

    private void UpdateDataFromUI()
    {
        if (_playerData == null) return;

        _playerData.PlayerName = playerNameInput.text;
        
        if (int.TryParse(levelInput.text, out int level))
        {
            _playerData.Level = level;
        }

        _playerData.Health = healthSlider.value * 100f;
        
        _playerData.LastLogin = DateTime.UtcNow;
    }
    
    private void SetStatus(string message, Color color)
    {
        statusText.text = $"Status: {message}";
        statusText.color = color;
    }

    #region Event Handlers

    private void OnPlayerDataSaved(TirexExamplePlayerData data)
    {
        Debug.Log($"[SampleUIController-Event] PlayerData Saved! Name: {data.PlayerName}");
    }

    private void OnPlayerDataLoaded(TirexExamplePlayerData data)
    {
        Debug.Log($"[SampleUIController-Event] PlayerData Loaded! Name: {data.PlayerName}");
    }

    private void OnPlayerDataDeleted(string key)
    {
        Debug.Log($"[SampleUIController-Event] PlayerData Deleted for key: {key}");
    }

    #endregion
}