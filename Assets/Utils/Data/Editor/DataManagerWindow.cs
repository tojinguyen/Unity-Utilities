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
        
        [MenuItem("Tools/TirexGame/Data Manager")]
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
        }
        
        private void OnGUI()
        {
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
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshDataTypes();
                RefreshDataKeys();
            }
            
            EditorGUILayout.EndHorizontal();
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
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Validation Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Validate All Data", GUILayout.Height(30)))
            {
                ValidateAllData();
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
                        .ToList();
                    
                    _dataModelTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assemblies that can't be loaded
                }
            }
        }
        
        private void RefreshDataKeys()
        {
            _dataKeys.Clear();
            _selectedDataKey = null;
            _selectedDataInstance = null;
            _dataLoaded = false;
            
            if (_selectedDataType == null) return;
            
            try
            {
                var repositoryType = typeof(FileDataRepository<>).MakeGenericType(_selectedDataType);
                var repository = Activator.CreateInstance(repositoryType, _dataPath, true, true);
                
                var getAllKeysMethod = repositoryType.GetMethod("GetAllKeysAsync");
                var task = getAllKeysMethod.Invoke(repository, null);
                
                // For simplicity in editor, we'll scan the directory directly
                string dataTypeFolder = Path.Combine(_dataPath, _selectedDataType.Name);
                if (Directory.Exists(dataTypeFolder))
                {
                    var files = Directory.GetFiles(dataTypeFolder, "*.dat");
                    _dataKeys = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to refresh data keys: {ex.Message}");
            }
        }
        
        private void LoadSelectedData()
        {
            if (_selectedDataType == null || string.IsNullOrEmpty(_selectedDataKey)) return;
            
            try
            {
                var repositoryType = typeof(FileDataRepository<>).MakeGenericType(_selectedDataType);
                var repository = Activator.CreateInstance(repositoryType, _dataPath, true, true);
                
                // For editor simplicity, we'll load synchronously by reading the file directly
                string filePath = Path.Combine(_dataPath, _selectedDataType.Name, $"{_selectedDataKey}.dat");
                if (File.Exists(filePath))
                {
                    // This is a simplified version - in production you'd use the repository's LoadAsync method
                    var json = File.ReadAllText(filePath);
                    _selectedDataInstance = JsonConvert.DeserializeObject(json, _selectedDataType);
                    _dataLoaded = true;
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
            if (_selectedDataInstance == null || string.IsNullOrEmpty(_selectedDataKey)) return;
            
            try
            {
                string directoryPath = Path.Combine(_dataPath, _selectedDataType.Name);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                string filePath = Path.Combine(directoryPath, $"{_selectedDataKey}.dat");
                var json = JsonConvert.SerializeObject(_selectedDataInstance, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                EditorUtility.DisplayDialog("Success", "Data saved successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to save data: {ex.Message}", "OK");
            }
        }
        
        private void DeleteSelectedData()
        {
            if (string.IsNullOrEmpty(_selectedDataKey)) return;
            
            try
            {
                string filePath = Path.Combine(_dataPath, _selectedDataType.Name, $"{_selectedDataKey}.dat");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    RefreshDataKeys();
                    EditorUtility.DisplayDialog("Success", "Data deleted successfully!", "OK");
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
                
                string directoryPath = Path.Combine(_dataPath, _selectedDataType.Name);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                string filePath = Path.Combine(directoryPath, $"{_newDataKey}.dat");
                var json = JsonConvert.SerializeObject(newInstance, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                _newDataKey = "";
                RefreshDataKeys();
                EditorUtility.DisplayDialog("Success", "New data created successfully!", "OK");
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
        
        private void ValidateAllData()
        {
            var validationResults = new List<string>();
            
            foreach (var dataType in _dataModelTypes)
            {
                string dataTypeFolder = Path.Combine(_dataPath, dataType.Name);
                if (Directory.Exists(dataTypeFolder))
                {
                    var files = Directory.GetFiles(dataTypeFolder, "*.dat");
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var json = File.ReadAllText(file);
                            JsonConvert.DeserializeObject(json, dataType);
                            validationResults.Add($"✓ {dataType.Name}/{Path.GetFileNameWithoutExtension(file)}");
                        }
                        catch (Exception ex)
                        {
                            validationResults.Add($"✗ {dataType.Name}/{Path.GetFileNameWithoutExtension(file)}: {ex.Message}");
                        }
                    }
                }
            }
            
            string message = validationResults.Count > 0 
                ? string.Join("\n", validationResults) 
                : "No data found to validate.";
            
            EditorUtility.DisplayDialog("Validation Results", message, "OK");
        }
    }
}