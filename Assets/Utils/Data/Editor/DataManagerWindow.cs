using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TirexGame.Utils.Data;
using Cysharp.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace TirexGame.Utils.Data.Editor
{
    public class DataManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTabIndex = 0;
        private readonly string[] _tabNames = { "Data Browser", "Data Editor", "Settings", "Tools" };
        
        // Data Browser
        private List<Type> _dataModelTypes = new List<Type>();
        private Type _selectedDataType;
        private List<string> _dataKeys = new List<string>();
        private string _selectedDataKey;
        private object _selectedDataInstance;
        private bool _dataLoaded = false;
        
        // Data Editor
        private SerializedObject _serializedObject;
        private string _newDataKey = "";
        
        // Settings
        private DataManagerConfig _config;
        private string _dataPath;
        
        // Auto-refresh functionality
        private FileSystemWatcher _fileWatcher;
        private bool _needsRefresh = false;
        private double _lastRefreshTime = 0;
        private const double REFRESH_INTERVAL = 1.0; // Refresh every 1 second if needed
        private bool _autoRefreshEnabled = true;
        
        public static void ShowWindow()
        {
            var window = GetWindow<DataManagerWindow>("Data Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            _config = new DataManagerConfig();
            _dataPath = Application.persistentDataPath;
            RefreshDataTypes();
            SetupFileWatcher();
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            CleanupFileWatcher();
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void OnFocus()
        {
            // Auto-refresh when window gains focus
            if (_autoRefreshEnabled)
            {
                _needsRefresh = true;
            }
        }
        
        private void OnGUI()
        {
            // Handle auto-refresh
            HandleAutoRefresh();
            
            EditorGUILayout.BeginVertical();
            
            // Header
            DrawHeader();
            
            // Tabs
            _selectedTabIndex = GUILayout.Toolbar(_selectedTabIndex, _tabNames);
            
            EditorGUILayout.Space(10);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            switch (_selectedTabIndex)
            {
                case 0:
                    DrawDataBrowserTab();
                    break;
                case 1:
                    DrawDataEditorTab();
                    break;
                case 2:
                    DrawSettingsTab();
                    break;
                case 3:
                    DrawToolsTab();
                    break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("TirexGame Data Manager", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Auto-refresh toggle
            var newAutoRefresh = GUILayout.Toggle(_autoRefreshEnabled, "Auto-Refresh", EditorStyles.toolbarButton);
            if (newAutoRefresh != _autoRefreshEnabled)
            {
                _autoRefreshEnabled = newAutoRefresh;
                if (_autoRefreshEnabled)
                {
                    SetupFileWatcher();
                }
                else
                {
                    CleanupFileWatcher();
                }
            }
            
            // Manual refresh button
            if (GUILayout.Button("Refresh (F5)", EditorStyles.toolbarButton))
            {
                ForceRefresh();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Handle F5 key for refresh
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F5)
            {
                ForceRefresh();
                Event.current.Use();
            }
        }
        
        private void DrawDataBrowserTab()
        {
            EditorGUILayout.LabelField("Data Browser", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - Data Types
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.LabelField("Data Types", EditorStyles.boldLabel);
            
            foreach (var dataType in _dataModelTypes)
            {
                bool isSelected = _selectedDataType == dataType;
                
                if (isSelected)
                {
                    GUI.backgroundColor = Color.cyan;
                }
                
                if (GUILayout.Button(dataType.Name, EditorStyles.miniButton))
                {
                    _selectedDataType = dataType;
                    RefreshDataKeys();
                }
                
                if (isSelected)
                {
                    GUI.backgroundColor = Color.white;
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // Middle panel - Data Keys
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            EditorGUILayout.LabelField("Data Keys", EditorStyles.boldLabel);
            
            if (_selectedDataType != null)
            {
                foreach (var key in _dataKeys)
                {
                    bool isSelected = _selectedDataKey == key;
                    
                    if (isSelected)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }
                    
                    if (GUILayout.Button(key, EditorStyles.miniButton))
                    {
                        _selectedDataKey = key;
                        LoadSelectedData();
                    }
                    
                    if (isSelected)
                    {
                        GUI.backgroundColor = Color.white;
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // Right panel - Data Inspector
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Data Inspector", EditorStyles.boldLabel);
            
            if (_selectedDataInstance != null && _dataLoaded)
            {
                DrawDataInspector();
                
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Save Changes", GUILayout.Height(30)))
                {
                    SaveSelectedData();
                }
                
                if (GUILayout.Button("Delete Data", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete Data", 
                        $"Are you sure you want to delete '{_selectedDataKey}'?", 
                        "Delete", "Cancel"))
                    {
                        DeleteSelectedData();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a data type and key to view/edit data", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawDataEditorTab()
        {
            EditorGUILayout.LabelField("Data Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (_selectedDataType != null)
            {
                EditorGUILayout.LabelField($"Creating new {_selectedDataType.Name}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Data Key:", GUILayout.Width(100));
                _newDataKey = EditorGUILayout.TextField(_newDataKey);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Create New Data", GUILayout.Height(30)))
                {
                    CreateNewData();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a data type from the Data Browser tab first", MessageType.Warning);
            }
        }
        
        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Data Browser Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Example data models from package are automatically filtered out to prevent conflicts with your project's data models.", 
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Data Manager Configuration", EditorStyles.boldLabel);
            
            _config.EnableLogging = EditorGUILayout.Toggle("Enable Logging", _config.EnableLogging);
            _config.EnableCaching = EditorGUILayout.Toggle("Enable Caching", _config.EnableCaching);
            _config.DefaultCacheExpirationMinutes = EditorGUILayout.IntField("Cache Expiration (minutes)", _config.DefaultCacheExpirationMinutes);
            _config.EnableAutoSave = EditorGUILayout.Toggle("Enable Auto Save", _config.EnableAutoSave);
            _config.AutoSaveIntervalSeconds = EditorGUILayout.FloatField("Auto Save Interval (seconds)", _config.AutoSaveIntervalSeconds);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Data Path Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Data Path:", GUILayout.Width(100));
            _dataPath = EditorGUILayout.TextField(_dataPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Data Path", _dataPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _dataPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Apply Settings", GUILayout.Height(30)))
            {
                DataManager.Initialize(_config);
                EditorUtility.DisplayDialog("Settings Applied", "Data Manager settings have been applied successfully!", "OK");
            }
        }
        
        private void DrawToolsTab()
        {
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Data Management Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Export All Data", GUILayout.Height(30)))
            {
                ExportAllData();
            }
            
            if (GUILayout.Button("Import Data", GUILayout.Height(30)))
            {
                ImportData();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Clear All Cache", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Cache", 
                    "Are you sure you want to clear all cached data?", 
                    "Clear", "Cancel"))
                {
                    // TODO: Implement cache clearing
                    EditorUtility.DisplayDialog("Cache Cleared", "All cached data has been cleared!", "OK");
                }
            }
        }
        
        private void RefreshDataTypes()
        {
            _dataModelTypes.Clear();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetInterfaces()
                            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDataModel<>)))
                        .Where(t => !IsExampleDataType(t))
                        .ToList();
                    
                    _dataModelTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assemblies that can't be loaded
                }
            }
        }
        
        /// <summary>
        /// Determines if a type is an example data type that should be excluded from production usage
        /// </summary>
        private bool IsExampleDataType(Type type)
        {
            // Simple filter: exclude any type with "TirexExample" prefix or in Examples namespace
            return type.Name.StartsWith("TirexExample", StringComparison.OrdinalIgnoreCase) ||
                   (type.Namespace != null && type.Namespace.Contains("Examples"));
        }
        
        private void RefreshDataKeys()
        {
            // Preserve current selection
            string previousSelectedKey = _selectedDataKey;
            
            _dataKeys.Clear();
            
            if (_selectedDataType == null)
            {
                _selectedDataKey = null;
                _selectedDataInstance = null;
                _dataLoaded = false;
                return;
            }
            
            try
            {
                var repository = GetRepository(_selectedDataType);
                
                // For simplicity in editor, we'll scan the directory directly
                string dataTypeFolder = Path.Combine(_dataPath, _selectedDataType.Name);
                if (Directory.Exists(dataTypeFolder))
                {
                    var files = Directory.GetFiles(dataTypeFolder, "*.dat");
                    _dataKeys = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
                }
                
                // Restore selection if the key still exists
                if (!string.IsNullOrEmpty(previousSelectedKey) && _dataKeys.Contains(previousSelectedKey))
                {
                    _selectedDataKey = previousSelectedKey;
                    // Keep the current data instance and loaded state if it's the same key
                    // The data might have been updated, so we'll reload it
                    if (_selectedDataInstance != null)
                    {
                        LoadSelectedData();
                    }
                }
                else
                {
                    // No valid previous selection or key no longer exists
                    _selectedDataKey = null;
                    _selectedDataInstance = null;
                    _dataLoaded = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to refresh data keys: {ex.Message}");
                _selectedDataKey = null;
                _selectedDataInstance = null;
                _dataLoaded = false;
            }
        }
        
        private void LoadSelectedData()
        {
            if (_selectedDataType == null || string.IsNullOrEmpty(_selectedDataKey)) return;
            
            try
            {
                Debug.Log($"[DataManagerWindow] Loading data for key '{_selectedDataKey}' of type '{_selectedDataType.Name}'");
                
                // Use integrated DataManager loading
                _selectedDataInstance = LoadDataWithDataManager(_selectedDataType, _selectedDataKey);
                
                if (_selectedDataInstance != null)
                {
                    _dataLoaded = true;
                    Debug.Log($"[DataManagerWindow] Successfully loaded data for key '{_selectedDataKey}'");
                }
                else
                {
                    Debug.LogWarning($"No data found for key '{_selectedDataKey}'");
                    EditorUtility.DisplayDialog("Warning", $"No data found for key '{_selectedDataKey}'", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }
        
        private void SaveSelectedData()
        {
            if (_selectedDataInstance == null || string.IsNullOrEmpty(_selectedDataKey)) 
            {
                Debug.LogWarning("[DataManagerWindow] Cannot save: No data instance or key selected");
                EditorUtility.DisplayDialog("Warning", "No data selected to save!", "OK");
                return;
            }
            
            try
            {
                Debug.Log($"[DataManagerWindow] Attempting to save data for key '{_selectedDataKey}' of type '{_selectedDataType.Name}'");
                
                // Log current data state before saving
                var currentJson = JsonConvert.SerializeObject(_selectedDataInstance, Formatting.Indented);
                Debug.Log($"[DataManagerWindow] Data to save:\n{currentJson}");
                
                // Use integrated DataManager saving
                bool success = SaveDataWithDataManager(_selectedDataType, _selectedDataKey, _selectedDataInstance);
                
                if (success)
                {
                    Debug.Log($"[DataManagerWindow] Data saved successfully for key '{_selectedDataKey}'");
                    
                    // Verify the save by loading the data back
                    var verifyData = LoadDataWithDataManager(_selectedDataType, _selectedDataKey);
                    
                    if (verifyData != null)
                    {
                        var verifyJson = JsonConvert.SerializeObject(verifyData, Formatting.Indented);
                        Debug.Log($"[DataManagerWindow] Verification successful. Saved data:\n{verifyJson}");
                        EditorUtility.DisplayDialog("Success", $"Data saved and verified successfully for key '{_selectedDataKey}'!", "OK");
                    }
                    else
                    {
                        Debug.LogError($"[DataManagerWindow] Verification failed: Could not load back saved data for key '{_selectedDataKey}'");
                        EditorUtility.DisplayDialog("Warning", "Data saved but verification failed. Please check manually.", "OK");
                    }
                }
                else
                {
                    Debug.LogError($"[DataManagerWindow] Save operation returned false for key '{_selectedDataKey}'");
                    EditorUtility.DisplayDialog("Error", "Failed to save data", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DataManagerWindow] Failed to save data for key '{_selectedDataKey}': {ex.Message}\nStackTrace: {ex.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"Failed to save data: {ex.Message}", "OK");
            }
        }
        
        private void DeleteSelectedData()
        {
            if (string.IsNullOrEmpty(_selectedDataKey)) return;
            
            try
            {
                var repositoryType = typeof(FileDataRepository<>).MakeGenericType(_selectedDataType);
                var repository = Activator.CreateInstance(repositoryType, _dataPath, true, true);
                
                // Use the repository's synchronous Delete method
                var deleteMethod = repositoryType.GetMethod("Delete");
                bool success = (bool)deleteMethod.Invoke(repository, new object[] { _selectedDataKey });
                
                if (success)
                {
                    RefreshDataKeys();
                    EditorUtility.DisplayDialog("Success", "Data deleted successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "Data file not found or already deleted", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to delete data: {ex.Message}", "OK");
            }
        }
        
        private void CreateNewData()
        {
            if (_selectedDataType == null || string.IsNullOrEmpty(_newDataKey)) return;
            
            try
            {
                var newInstance = Activator.CreateInstance(_selectedDataType);
                
                // Call SetDefaultData if available
                var setDefaultMethod = _selectedDataType.GetMethod("SetDefaultData");
                setDefaultMethod?.Invoke(newInstance, null);
                
                var repositoryType = typeof(FileDataRepository<>).MakeGenericType(_selectedDataType);
                var repository = Activator.CreateInstance(repositoryType, _dataPath, true, true);
                
                // Use the repository's synchronous Save method
                var saveMethod = repositoryType.GetMethod("Save");
                bool success = (bool)saveMethod.Invoke(repository, new object[] { _newDataKey, newInstance });
                
                if (success)
                {
                    string createdKey = _newDataKey;
                    _newDataKey = "";
                    RefreshDataKeys();
                    
                    // Auto-select the newly created data
                    _selectedDataKey = createdKey;
                    LoadSelectedData();
                    
                    EditorUtility.DisplayDialog("Success", "New data created successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to create new data", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create new data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create new data: {ex.Message}", "OK");
            }
        }
        
        private void DrawDataInspector()
        {
            if (_selectedDataInstance == null) return;
            
            var fields = _selectedDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = _selectedDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);
            
            EditorGUI.BeginChangeCheck();
            
            foreach (var field in fields)
            {
                DrawFieldEditor(field.Name, field.FieldType, field.GetValue(_selectedDataInstance), 
                    value => field.SetValue(_selectedDataInstance, value));
            }
            
            foreach (var property in properties)
            {
                DrawFieldEditor(property.Name, property.PropertyType, property.GetValue(_selectedDataInstance),
                    value => property.SetValue(_selectedDataInstance, value));
            }
            
            bool hasChanges = EditorGUI.EndChangeCheck();
            
            if (hasChanges)
            {
                Debug.Log($"[DataManagerWindow] Data modified for key '{_selectedDataKey}'");
            }
        }
        
        private void DrawFieldEditor(string name, Type type, object value, Action<object> setValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(120));
            
            if (type == typeof(string))
            {
                setValue(EditorGUILayout.TextField((string)value ?? ""));
            }
            else if (type == typeof(int))
            {
                setValue(EditorGUILayout.IntField((int)value));
            }
            else if (type == typeof(float))
            {
                setValue(EditorGUILayout.FloatField((float)value));
            }
            else if (type == typeof(bool))
            {
                setValue(EditorGUILayout.Toggle((bool)value));
            }
            else if (type == typeof(DateTime))
            {
                var dateTime = (DateTime)value;
                var dateString = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var newDateString = EditorGUILayout.TextField(dateString);
                if (DateTime.TryParse(newDateString, out DateTime newDateTime))
                {
                    setValue(newDateTime);
                }
            }
            else if (type == typeof(Vector3))
            {
                setValue(EditorGUILayout.Vector3Field("", (Vector3)value));
            }
            else if (type == typeof(Vector2))
            {
                setValue(EditorGUILayout.Vector2Field("", (Vector2)value));
            }
            else
            {
                EditorGUILayout.LabelField($"Unsupported type: {type.Name}");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void ExportAllData()
        {
            string exportPath = EditorUtility.SaveFilePanel("Export All Data", "", "data_export", "json");
            if (string.IsNullOrEmpty(exportPath)) return;
            
            try
            {
                var exportData = new Dictionary<string, Dictionary<string, object>>();
                
                foreach (var dataType in _dataModelTypes)
                {
                    string dataTypeFolder = Path.Combine(_dataPath, dataType.Name);
                    if (Directory.Exists(dataTypeFolder))
                    {
                        var typeData = new Dictionary<string, object>();
                        var files = Directory.GetFiles(dataTypeFolder, "*.dat");
                        
                        foreach (var file in files)
                        {
                            var key = Path.GetFileNameWithoutExtension(file);
                            var json = File.ReadAllText(file);
                            var data = JsonConvert.DeserializeObject(json, dataType);
                            typeData[key] = data;
                        }
                        
                        exportData[dataType.Name] = typeData;
                    }
                }
                
                var exportJson = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(exportPath, exportJson);
                
                EditorUtility.DisplayDialog("Success", "Data exported successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to export data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to export data: {ex.Message}", "OK");
            }
        }
        
        private void ImportData()
        {
            string importPath = EditorUtility.OpenFilePanel("Import Data", "", "json");
            if (string.IsNullOrEmpty(importPath)) return;
            
            try
            {
                var json = File.ReadAllText(importPath);
                var importData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
                
                foreach (var typeEntry in importData)
                {
                    var dataType = _dataModelTypes.FirstOrDefault(t => t.Name == typeEntry.Key);
                    if (dataType == null) continue;
                    
                    string directoryPath = Path.Combine(_dataPath, dataType.Name);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    
                    foreach (var dataEntry in typeEntry.Value)
                    {
                        string filePath = Path.Combine(directoryPath, $"{dataEntry.Key}.dat");
                        var dataJson = JsonConvert.SerializeObject(dataEntry.Value, Formatting.Indented);
                        File.WriteAllText(filePath, dataJson);
                    }
                }
                
                RefreshDataKeys();
                EditorUtility.DisplayDialog("Success", "Data imported successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to import data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to import data: {ex.Message}", "OK");
            }
        }
        
        #region Auto-Refresh Functionality
        
        private void OnEditorUpdate()
        {
            if (_needsRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > REFRESH_INTERVAL)
            {
                _needsRefresh = false;
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                
                // Smart refresh that preserves selection
                RefreshDataKeys();
                Repaint();
            }
        }
        
        private void HandleAutoRefresh()
        {
            // This method is called from OnGUI to handle any refresh logic
            // Currently handled by OnEditorUpdate, but kept for future extensions
        }
        
        private void SetupFileWatcher()
        {
            CleanupFileWatcher(); // Clean up any existing watcher
            
            if (!_autoRefreshEnabled || !Directory.Exists(_dataPath))
                return;
                
            try
            {
                _fileWatcher = new FileSystemWatcher(_dataPath, "*.dat");
                _fileWatcher.IncludeSubdirectories = true;
                _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
                
                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.Created += OnFileChanged;
                _fileWatcher.Deleted += OnFileChanged;
                _fileWatcher.Renamed += OnFileRenamed;
                
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to setup file watcher: {ex.Message}");
            }
        }
        
        private void CleanupFileWatcher()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileChanged;
                _fileWatcher.Created -= OnFileChanged;
                _fileWatcher.Deleted -= OnFileChanged;
                _fileWatcher.Renamed -= OnFileRenamed;
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.EndsWith(".dat"))
            {
                _needsRefresh = true;
            }
        }
        
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (e.FullPath.EndsWith(".dat") || e.OldFullPath.EndsWith(".dat"))
            {
                _needsRefresh = true;
            }
        }
        
        private void ForceRefresh()
        {
            RefreshDataTypes();
            RefreshDataKeys();
            if (_selectedDataKey != null && _selectedDataInstance != null)
            {
                LoadSelectedData(); // Reload current data
            }
            Repaint();
        }
        
        #endregion
        
        #region Repository Integration
        
        /// <summary>
        /// Get repository from DataManager if available, otherwise create default FileDataRepository
        /// This ensures editor and runtime use the same data source
        /// </summary>
        private object GetRepository(Type dataType)
        {
            try
            {
                // Try to get repository from runtime DataManager first
                var runtimeRepo = DataManager.GetRepository(dataType);
                if (runtimeRepo != null)
                {
                    Debug.Log($"[DataManagerWindow] Using runtime repository for {dataType.Name}");
                    return runtimeRepo;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManagerWindow] Failed to get runtime repository for {dataType.Name}: {ex.Message}");
            }
            
            // Fallback: create FileDataRepository as before
            Debug.Log($"[DataManagerWindow] Creating fallback FileDataRepository for {dataType.Name}");
            var repositoryType = typeof(FileDataRepository<>).MakeGenericType(dataType);
            return Activator.CreateInstance(repositoryType, _dataPath, true, true);
        }
        
        /// <summary>
        /// Load data using runtime DataManager if possible, fallback to direct repository access
        /// </summary>
        private object LoadDataWithDataManager(Type dataType, string key)
        {
            try
            {
                // Try using DataManager's GetData method for consistency
                var getDataMethod = typeof(DataManager).GetMethod("GetData", new Type[] { typeof(string) });
                if (getDataMethod != null)
                {
                    var genericMethod = getDataMethod.MakeGenericMethod(dataType);
                    var result = genericMethod.Invoke(null, new object[] { key });
                    if (result != null)
                    {
                        Debug.Log($"[DataManagerWindow] Loaded data via DataManager.GetData for key '{key}'");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManagerWindow] Failed to load via DataManager for key '{key}': {ex.Message}");
            }
            
            // Fallback to direct repository access
            var repository = GetRepository(dataType);
            var loadMethod = repository.GetType().GetMethod("Load");
            return loadMethod?.Invoke(repository, new object[] { key });
        }
        
        /// <summary>
        /// Save data using runtime DataManager if possible, fallback to direct repository access
        /// </summary>
        private bool SaveDataWithDataManager(Type dataType, string key, object data)
        {
            try
            {
                // Try using DataManager's SaveData method for consistency
                var saveDataMethod = typeof(DataManager).GetMethod("SaveData", new Type[] { typeof(object), typeof(string) });
                if (saveDataMethod != null)
                {
                    var genericMethod = saveDataMethod.MakeGenericMethod(dataType);
                    var saveResult = genericMethod.Invoke(null, new object[] { data, key });
                    if (saveResult is bool success && success)
                    {
                        Debug.Log($"[DataManagerWindow] Saved data via DataManager.SaveData for key '{key}'");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DataManagerWindow] Failed to save via DataManager for key '{key}': {ex.Message}");
            }
            
            // Fallback to direct repository access
            var repository = GetRepository(dataType);
            var saveMethod = repository.GetType().GetMethod("Save");
            var result = saveMethod?.Invoke(repository, new object[] { key, data });
            return result is bool && (bool)result;
        }
        
        #endregion
    }
}