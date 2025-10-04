// Data/Example/Scripts/SyncDataExample.cs

using UnityEngine;
using UnityEngine.UI;
using TirexGame.Utils.Data;
using TirexGame.Utils.Data.Examples;
using TMPro;
using System;

/// <summary>
/// Example demonstrating how to use the synchronous Data API for lightweight operations
/// </summary>
public class SyncDataExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField levelInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text infoText;

    [Header("Sync API Buttons")]
    [SerializeField] private Button syncLoadButton;
    [SerializeField] private Button syncSaveButton;
    [SerializeField] private Button syncDeleteButton;
    [SerializeField] private Button syncExistsButton;
    [SerializeField] private Button syncGetAllKeysButton;

    private TirexExamplePlayerData _playerData;
    private const string SyncPlayerKey = "SyncTirexExamplePlayerData";

    private void Start()
    {
        // Initialize DataManager with a lightweight repository
        InitializeDataManager();
        
        // Setup UI event handlers
        SetupUIEvents();
        
        // Load existing data on start
        LoadDataSync();
        
        UpdateInfo("Sync Data Example initialized. All operations use synchronous API.");
    }

    private void InitializeDataManager()
    {
        // Initialize with memory repository for fast sync operations
        DataManager.Initialize(new DataManagerConfig
        {
            EnableLogging = true,
            EnableCaching = true,
            DefaultCacheExpirationMinutes = 10
        });

        // Register a memory repository for fast sync operations
        var memoryRepo = new MemoryDataRepository<TirexExamplePlayerData>();
        DataManager.RegisterRepository(memoryRepo);
        
        UpdateStatus("DataManager initialized with Memory Repository");
    }

    private void SetupUIEvents()
    {
        syncLoadButton.onClick.AddListener(LoadDataSync);
        syncSaveButton.onClick.AddListener(SaveDataSync);
        syncDeleteButton.onClick.AddListener(DeleteDataSync);
        syncExistsButton.onClick.AddListener(CheckExistsSync);
        syncGetAllKeysButton.onClick.AddListener(GetAllKeysSync);
    }

    #region Synchronous Data Operations

    private void LoadDataSync()
    {
        try
        {
            UpdateStatus("Loading data synchronously...");
            
            // Use synchronous API - no await needed!
            _playerData = DataManager.GetData<TirexExamplePlayerData>(SyncPlayerKey);
            
            if (_playerData != null)
            {
                UpdateUIFromData();
                UpdateStatus($"Data loaded successfully: {_playerData.PlayerName}");
            }
            else
            {
                UpdateStatus("No data found, created default data");
                UpdateUIFromData();
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to load data: {ex.Message}");
        }
    }

    private void SaveDataSync()
    {
        try
        {
            UpdateStatus("Saving data synchronously...");
            
            UpdateDataFromUI();
            
            // Use synchronous API - no await needed!
            bool success = DataManager.SaveData(_playerData, SyncPlayerKey);
            
            if (success)
            {
                UpdateStatus($"Data saved successfully: {_playerData.PlayerName}");
            }
            else
            {
                UpdateStatus("Failed to save data");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to save data: {ex.Message}");
        }
    }

    private void DeleteDataSync()
    {
        try
        {
            UpdateStatus("Deleting data synchronously...");
            
            // Use synchronous API - no await needed!
            bool success = DataManager.DeleteData<TirexExamplePlayerData>(SyncPlayerKey);
            
            if (success)
            {
                _playerData = new TirexExamplePlayerData();
                _playerData.SetDefaultData();
                UpdateUIFromData();
                UpdateStatus("Data deleted successfully");
            }
            else
            {
                UpdateStatus("Failed to delete data or data doesn't exist");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to delete data: {ex.Message}");
        }
    }

    private void CheckExistsSync()
    {
        try
        {
            // Use synchronous API - no await needed!
            bool exists = DataManager.Exists<TirexExamplePlayerData>(SyncPlayerKey);
            
            UpdateStatus($"Data exists: {exists}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to check data existence: {ex.Message}");
        }
    }

    private void GetAllKeysSync()
    {
        try
        {
            // Use synchronous API - no await needed!
            var keys = DataManager.GetAllKeys<TirexExamplePlayerData>();
            
            var keyList = string.Join(", ", keys);
            UpdateStatus($"All keys: [{keyList}]");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to get all keys: {ex.Message}");
        }
    }

    #endregion

    #region UI Helper Methods

    private void UpdateUIFromData()
    {
        if (_playerData == null) return;

        playerNameInput.text = _playerData.PlayerName;
        levelInput.text = _playerData.Level.ToString();
    }

    private void UpdateDataFromUI()
    {
        if (_playerData == null)
        {
            _playerData = new TirexExamplePlayerData();
        }

        _playerData.PlayerName = string.IsNullOrEmpty(playerNameInput.text) ? "Unknown Player" : playerNameInput.text;
        
        if (int.TryParse(levelInput.text, out int level))
        {
            _playerData.Level = Mathf.Max(1, level);
        }
        
        _playerData.LastLogin = DateTime.Now;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"[SyncDataExample] {message}");
    }

    private void UpdateInfo(string message)
    {
        if (infoText != null)
        {
            infoText.text = message;
        }
    }

    #endregion

    #region Performance Comparison Example

    [ContextMenu("Performance Test: Sync vs Async")]
    private void PerformanceTestExample()
    {
        const int iterations = 100;
        
        // Test sync operations
        var syncStopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var testData = new TirexExamplePlayerData { PlayerName = $"TestPlayer{i}", Level = i };
            DataManager.SaveData(testData, $"TestSync_{i}");
            var loadedData = DataManager.GetData<TirexExamplePlayerData>($"TestSync_{i}");
            DataManager.DeleteData<TirexExamplePlayerData>($"TestSync_{i}");
        }
        syncStopwatch.Stop();
        
        Debug.Log($"Sync operations ({iterations} iterations): {syncStopwatch.ElapsedMilliseconds}ms");
        UpdateStatus($"Sync test completed: {syncStopwatch.ElapsedMilliseconds}ms for {iterations} operations");
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up event handlers
        if (syncLoadButton != null) syncLoadButton.onClick.RemoveAllListeners();
        if (syncSaveButton != null) syncSaveButton.onClick.RemoveAllListeners();
        if (syncDeleteButton != null) syncDeleteButton.onClick.RemoveAllListeners();
        if (syncExistsButton != null) syncExistsButton.onClick.RemoveAllListeners();
        if (syncGetAllKeysButton != null) syncGetAllKeysButton.onClick.RemoveAllListeners();
    }
}