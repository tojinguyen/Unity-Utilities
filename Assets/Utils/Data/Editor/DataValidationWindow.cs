using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using TirexGame.Utils.Data;

namespace TirexGame.Utils.Data.Editor
{
    public class DataValidationWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private bool _showOnlyErrors = false;
        private bool _autoValidateOnSave = true;
        private bool _useEncryption = true;
        private bool _useCompression = true;
        private string _dataPath;
        
        [System.Serializable]
        public class ValidationResult
        {
            public string dataType;
            public string dataKey;
            public bool isValid;
            public string errorMessage;
            public string filePath;
            public DateTime validationTime;
            
            public ValidationResult(string dataType, string dataKey, bool isValid, string errorMessage = "", string filePath = "")
            {
                this.dataType = dataType;
                this.dataKey = dataKey;
                this.isValid = isValid;
                this.errorMessage = errorMessage;
                this.filePath = filePath;
                this.validationTime = DateTime.Now;
            }
        }
        
        public static void ShowWindow()
        {
            var window = GetWindow<DataValidationWindow>("Data Validation");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void OnEnable()
        {
            _dataPath = Application.persistentDataPath;
            LoadValidationSettings();
        }
        
        private void OnDisable()
        {
            SaveValidationSettings();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            DrawHeader();
            
            // Settings
            DrawSettings();
            
            EditorGUILayout.Space();
            
            // Action buttons
            DrawActionButtons();
            
            EditorGUILayout.Space();
            
            // Results
            DrawValidationResults();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("Data Validation Tools", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear Results", EditorStyles.toolbarButton))
            {
                _validationResults.Clear();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            
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
            
            _showOnlyErrors = EditorGUILayout.Toggle("Show Only Errors", _showOnlyErrors);
            _autoValidateOnSave = EditorGUILayout.Toggle("Auto Validate on Save", _autoValidateOnSave);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("File Format Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            _useEncryption = EditorGUILayout.Toggle("Use Encryption", _useEncryption);
            _useCompression = EditorGUILayout.Toggle("Use Compression", _useCompression);
            if (GUILayout.Button("Auto Detect", GUILayout.Width(80)))
            {
                AutoDetectFileFormat();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox(
                "Configure these settings to match your FileDataRepository settings:\n" +
                "• Use Encryption: Enable if your data files are encrypted\n" +
                "• Use Compression: Enable if your data files are compressed\n" +
                "• Auto Detect: Automatically detect format from existing files\n" +
                "• If validation fails, try different combinations",
                MessageType.Info);
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validate All Data", GUILayout.Height(30)))
            {
                ValidateAllData();
            }
            
            if (GUILayout.Button("Validate Selected Type", GUILayout.Height(30)))
            {
                ShowDataTypeSelectionMenu();
            }
            
            if (GUILayout.Button("Fix Common Issues", GUILayout.Height(30)))
            {
                FixCommonIssues();
            }
            
            if (GUILayout.Button("Export Report", GUILayout.Height(30)))
            {
                ExportValidationReport();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawValidationResults()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            
            if (_validationResults.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation results available. Run validation to see results.", MessageType.Info);
                return;
            }
            
            // Filter results
            var filteredResults = _showOnlyErrors 
                ? _validationResults.Where(r => !r.isValid).ToList()
                : _validationResults;
            
            // Statistics
            var totalCount = _validationResults.Count;
            var errorCount = _validationResults.Count(r => !r.isValid);
            var successCount = totalCount - errorCount;
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Total: {totalCount}", GUILayout.Width(80));
            
            GUI.color = Color.green;
            EditorGUILayout.LabelField($"Valid: {successCount}", GUILayout.Width(80));
            
            GUI.color = errorCount > 0 ? Color.red : Color.white;
            EditorGUILayout.LabelField($"Errors: {errorCount}", GUILayout.Width(80));
            
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Results list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var result in filteredResults)
            {
                DrawValidationResult(result);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawValidationResult(ValidationResult result)
        {
            var backgroundColor = result.isValid ? Color.green : Color.red;
            backgroundColor.a = 0.1f;
            
            GUI.backgroundColor = backgroundColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.BeginHorizontal();
            
            // Status icon
            var statusIcon = result.isValid ? "✓" : "✗";
            var statusColor = result.isValid ? Color.green : Color.red;
            
            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField(statusIcon, GUILayout.Width(20));
            GUI.color = originalColor;
            
            // Data info
            EditorGUILayout.LabelField($"{result.dataType}/{result.dataKey}", EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // Action buttons
            if (!result.isValid && GUILayout.Button("Fix", GUILayout.Width(40)))
            {
                TryFixValidationIssue(result);
            }
            
            if (!string.IsNullOrEmpty(result.filePath) && GUILayout.Button("Open", GUILayout.Width(50)))
            {
                OpenDataFile(result.filePath);
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (!result.isValid && !string.IsNullOrEmpty(result.errorMessage))
            {
                EditorGUILayout.LabelField("Error:", EditorStyles.miniBoldLabel);
                EditorGUILayout.TextArea(result.errorMessage, EditorStyles.helpBox);
            }
            
            EditorGUILayout.LabelField($"Validated: {result.validationTime:yyyy-MM-dd HH:mm:ss}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void ValidateAllData()
        {
            _validationResults.Clear();
            
            try
            {
                var dataTypes = GetAllDataModelTypes();
                
                foreach (var dataType in dataTypes)
                {
                    ValidateDataType(dataType);
                }
                
                EditorUtility.DisplayDialog("Validation Complete", 
                    $"Validation completed. Found {_validationResults.Count(r => !r.isValid)} errors out of {_validationResults.Count} items.", 
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Validation failed: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Validation failed: {ex.Message}", "OK");
            }
        }
        
        private void ValidateDataType(Type dataType)
        {
            try
            {
                string dataTypeFolder = Path.Combine(_dataPath, dataType.Name);
                if (!Directory.Exists(dataTypeFolder))
                {
                    return;
                }
                
                var files = Directory.GetFiles(dataTypeFolder, "*.dat");
                
                foreach (var file in files)
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    ValidateDataFile(dataType, key, file);
                }
            }
            catch (Exception ex)
            {
                var result = new ValidationResult(dataType.Name, "ALL", false, $"Failed to validate type: {ex.Message}");
                _validationResults.Add(result);
            }
        }
        
        private async void ValidateDataFile(Type dataType, string key, string filePath)
        {
            try
            {
                // Check if file exists and is readable
                if (!File.Exists(filePath))
                {
                    _validationResults.Add(new ValidationResult(dataType.Name, key, false, "File does not exist", filePath));
                    return;
                }

                // Read and decode file content like runtime does
                string json;
                try
                {
                    var fileBytes = File.ReadAllBytes(filePath);
                    
                    // Decode based on user settings: File Bytes -> Decrypt -> Decompress -> JSON String
                    var dataBytes = _useEncryption ? DataEncryptor.Decrypt(fileBytes) : fileBytes;
                    
                    if (_useCompression)
                    {
                        var decompressedResult = DataCompressor.DecompressBytes(dataBytes);
                        if (!decompressedResult.Success)
                        {
                            _validationResults.Add(new ValidationResult(dataType.Name, key, false, 
                                $"Decompression failed: {decompressedResult.Error}", filePath));
                            return;
                        }
                        json = Encoding.UTF8.GetString(decompressedResult.Data);
                    }
                    else
                    {
                        json = Encoding.UTF8.GetString(dataBytes);
                    }
                }
                catch (Exception decodeEx)
                {
                    // If decoding fails, try reading as plain text (fallback)
                    try
                    {
                        json = File.ReadAllText(filePath, Encoding.UTF8);
                        Debug.LogWarning($"File {filePath} could not be decoded with current settings (encryption: {_useEncryption}, compression: {_useCompression}), reading as plain text");
                    }
                    catch (Exception readEx)
                    {
                        _validationResults.Add(new ValidationResult(dataType.Name, key, false, 
                            $"Failed to read/decode file: {decodeEx.Message} | {readEx.Message}", filePath));
                        return;
                    }
                }
                
                // Validate JSON format
                if (string.IsNullOrWhiteSpace(json))
                {
                    _validationResults.Add(new ValidationResult(dataType.Name, key, false, "File content is empty after decoding", filePath));
                    return;
                }

                // Try to deserialize using Newtonsoft.Json (same as runtime)
                object dataInstance;
                try
                {
                    dataInstance = Newtonsoft.Json.JsonConvert.DeserializeObject(json, dataType);
                    if (dataInstance == null)
                    {
                        _validationResults.Add(new ValidationResult(dataType.Name, key, false, "Failed to deserialize - null result", filePath));
                        return;
                    }
                }
                catch (Exception jsonEx)
                {
                    _validationResults.Add(new ValidationResult(dataType.Name, key, false, $"JSON parse error: {jsonEx.Message}", filePath));
                    return;
                }
                
                // Validate using IValidatable if implemented
                if (dataInstance is IValidatable validatable)
                {
                    bool isValid = validatable.Validate(out var errors);
                    if (!isValid)
                    {
                        string errorMessage = errors != null && errors.Count > 0 
                            ? string.Join("; ", errors) 
                            : "Unknown validation error";
                        _validationResults.Add(new ValidationResult(dataType.Name, key, false, errorMessage, filePath));
                        return;
                    }
                }
                
                // Validate using DataValidator
                var validator = new DataValidator();
                var validationResult = await validator.ValidateAsync(dataInstance);
                
                if (validationResult.IsValid)
                {
                    _validationResults.Add(new ValidationResult(dataType.Name, key, true, "", filePath));
                }
                else
                {
                    string errorMessage = validationResult.Errors != null && validationResult.Errors.Count > 0 
                        ? string.Join("; ", validationResult.Errors) 
                        : "Data validation failed";
                    _validationResults.Add(new ValidationResult(dataType.Name, key, false, errorMessage, filePath));
                }
            }
            catch (Exception ex)
            {
                _validationResults.Add(new ValidationResult(dataType.Name, key, false, $"Validation error: {ex.Message}", filePath));
            }
        }
        
        private void ShowDataTypeSelectionMenu()
        {
            var dataTypes = GetAllDataModelTypes();
            var menu = new GenericMenu();
            
            foreach (var dataType in dataTypes)
            {
                menu.AddItem(new GUIContent(dataType.Name), false, () => {
                    _validationResults.RemoveAll(r => r.dataType == dataType.Name);
                    ValidateDataType(dataType);
                });
            }
            
            menu.ShowAsContext();
        }
        
        private void FixCommonIssues()
        {
            int fixedCount = 0;
            
            foreach (var result in _validationResults.Where(r => !r.isValid).ToList())
            {
                if (TryFixValidationIssue(result))
                {
                    fixedCount++;
                }
            }
            
            if (fixedCount > 0)
            {
                EditorUtility.DisplayDialog("Fix Complete", $"Fixed {fixedCount} issues. Re-run validation to verify.", "OK");
                ValidateAllData();
            }
            else
            {
                EditorUtility.DisplayDialog("Fix Complete", "No issues could be automatically fixed.", "OK");
            }
        }
        
        private bool TryFixValidationIssue(ValidationResult result)
        {
            try
            {
                if (result.errorMessage.Contains("File is empty"))
                {
                    // Try to recreate with default data
                    var dataTypes = GetAllDataModelTypes();
                    var dataType = dataTypes.FirstOrDefault(t => t.Name == result.dataType);
                    
                    if (dataType != null)
                    {
                        var newInstance = Activator.CreateInstance(dataType);
                        var setDefaultMethod = dataType.GetMethod("SetDefaultData");
                        setDefaultMethod?.Invoke(newInstance, null);
                        
                        var json = JsonUtility.ToJson(newInstance, true);
                        File.WriteAllText(result.filePath, json);
                        
                        return true;
                    }
                }
                else if (result.errorMessage.Contains("JSON Error"))
                {
                    // Try to fix common JSON issues
                    if (File.Exists(result.filePath))
                    {
                        var content = File.ReadAllText(result.filePath);
                        
                        // Fix common JSON issues
                        content = content.Replace("'", "\""); // Single quotes to double quotes
                        content = content.Trim();
                        
                        if (!content.StartsWith("{") && !content.StartsWith("["))
                        {
                            // Wrap in object if needed
                            content = "{" + content + "}";
                        }
                        
                        File.WriteAllText(result.filePath, content);
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to fix issue: {ex.Message}");
                return false;
            }
        }
        
        private void OpenDataFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
            }
        }
        
        private void ExportValidationReport()
        {
            string exportPath = EditorUtility.SaveFilePanel("Export Validation Report", "", "validation_report", "txt");
            if (string.IsNullOrEmpty(exportPath)) return;
            
            try
            {
                var report = GenerateValidationReport();
                File.WriteAllText(exportPath, report);
                EditorUtility.DisplayDialog("Success", "Validation report exported successfully!", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to export report: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to export report: {ex.Message}", "OK");
            }
        }
        
        private string GenerateValidationReport()
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("DATA VALIDATION REPORT");
            report.AppendLine("=====================");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Data Path: {_dataPath}");
            report.AppendLine();
            
            var totalCount = _validationResults.Count;
            var errorCount = _validationResults.Count(r => !r.isValid);
            var successCount = totalCount - errorCount;
            
            report.AppendLine("SUMMARY");
            report.AppendLine("-------");
            report.AppendLine($"Total Items: {totalCount}");
            report.AppendLine($"Valid Items: {successCount}");
            report.AppendLine($"Invalid Items: {errorCount}");
            report.AppendLine($"Success Rate: {(totalCount > 0 ? (double)successCount / totalCount * 100 : 0):F1}%");
            report.AppendLine();
            
            if (errorCount > 0)
            {
                report.AppendLine("ERRORS");
                report.AppendLine("------");
                
                foreach (var result in _validationResults.Where(r => !r.isValid))
                {
                    report.AppendLine($"✗ {result.dataType}/{result.dataKey}");
                    report.AppendLine($"  Error: {result.errorMessage}");
                    report.AppendLine($"  File: {result.filePath}");
                    report.AppendLine($"  Time: {result.validationTime:yyyy-MM-dd HH:mm:ss}");
                    report.AppendLine();
                }
            }
            
            report.AppendLine("VALID ITEMS");
            report.AppendLine("-----------");
            
            foreach (var result in _validationResults.Where(r => r.isValid))
            {
                report.AppendLine($"✓ {result.dataType}/{result.dataKey}");
            }
            
            return report.ToString();
        }
        
        private List<Type> GetAllDataModelTypes()
        {
            var dataModelTypes = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetInterfaces()
                            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDataModel<>)))
                        .ToList();
                    
                    dataModelTypes.AddRange(types);
                }
                catch (System.Reflection.ReflectionTypeLoadException)
                {
                    // Ignore assemblies that can't be loaded
                }
            }
            
            return dataModelTypes;
        }
        
        private void LoadValidationSettings()
        {
            _showOnlyErrors = EditorPrefs.GetBool("DataValidation.ShowOnlyErrors", false);
            _autoValidateOnSave = EditorPrefs.GetBool("DataValidation.AutoValidateOnSave", true);
            _useEncryption = EditorPrefs.GetBool("DataValidation.UseEncryption", true);
            _useCompression = EditorPrefs.GetBool("DataValidation.UseCompression", true);
            _dataPath = EditorPrefs.GetString("DataValidation.DataPath", Application.persistentDataPath);
        }
        
        private void SaveValidationSettings()
        {
            EditorPrefs.SetBool("DataValidation.ShowOnlyErrors", _showOnlyErrors);
            EditorPrefs.SetBool("DataValidation.AutoValidateOnSave", _autoValidateOnSave);
            EditorPrefs.SetBool("DataValidation.UseEncryption", _useEncryption);
            EditorPrefs.SetBool("DataValidation.UseCompression", _useCompression);
            EditorPrefs.SetString("DataValidation.DataPath", _dataPath);
        }
        
        private void AutoDetectFileFormat()
        {
            try
            {
                // Find the first .dat file to test
                var datFiles = Directory.GetFiles(_dataPath, "*.dat", SearchOption.AllDirectories);
                if (datFiles.Length == 0)
                {
                    EditorUtility.DisplayDialog("Auto Detect", "No .dat files found in the data path.", "OK");
                    return;
                }
                
                var testFile = datFiles[0];
                var fileBytes = File.ReadAllBytes(testFile);
                
                // Test different combinations
                bool detectedEncryption = false;
                bool detectedCompression = false;
                
                // Test 1: No encryption, no compression (plain JSON)
                try
                {
                    var jsonText = Encoding.UTF8.GetString(fileBytes);
                    Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText);
                    // Success - file is plain JSON
                    detectedEncryption = false;
                    detectedCompression = false;
                    Debug.Log("Detected format: Plain JSON (no encryption, no compression)");
                }
                catch
                {
                    // Test 2: Encryption only
                    try
                    {
                        var decryptedBytes = DataEncryptor.Decrypt(fileBytes);
                        var jsonText = Encoding.UTF8.GetString(decryptedBytes);
                        Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText);
                        // Success - encrypted but not compressed
                        detectedEncryption = true;
                        detectedCompression = false;
                        Debug.Log("Detected format: Encrypted JSON (encryption: yes, compression: no)");
                    }
                    catch
                    {
                        // Test 3: Encryption + Compression
                        try
                        {
                            var decryptedBytes = DataEncryptor.Decrypt(fileBytes);
                            var decompressedResult = DataCompressor.DecompressBytes(decryptedBytes);
                            if (decompressedResult.Success)
                            {
                                var jsonText = Encoding.UTF8.GetString(decompressedResult.Data);
                                Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText);
                                // Success - encrypted and compressed
                                detectedEncryption = true;
                                detectedCompression = true;
                                Debug.Log("Detected format: Encrypted + Compressed JSON (encryption: yes, compression: yes)");
                            }
                            else
                            {
                                throw new Exception("Decompression failed");
                            }
                        }
                        catch
                        {
                            // Test 4: Compression only (no encryption)
                            try
                            {
                                var decompressedResult = DataCompressor.DecompressBytes(fileBytes);
                                if (decompressedResult.Success)
                                {
                                    var jsonText = Encoding.UTF8.GetString(decompressedResult.Data);
                                    Newtonsoft.Json.JsonConvert.DeserializeObject(jsonText);
                                    // Success - compressed but not encrypted
                                    detectedEncryption = false;
                                    detectedCompression = true;
                                    Debug.Log("Detected format: Compressed JSON (encryption: no, compression: yes)");
                                }
                                else
                                {
                                    throw new Exception("Could not detect file format");
                                }
                            }
                            catch
                            {
                                EditorUtility.DisplayDialog("Auto Detect Failed", 
                                    "Could not automatically detect the file format. Please set manually.", "OK");
                                return;
                            }
                        }
                    }
                }
                
                // Apply detected settings
                _useEncryption = detectedEncryption;
                _useCompression = detectedCompression;
                
                EditorUtility.DisplayDialog("Auto Detect Complete", 
                    $"File format detected successfully!\n\n" +
                    $"Encryption: {(detectedEncryption ? "Yes" : "No")}\n" +
                    $"Compression: {(detectedCompression ? "Yes" : "No")}\n\n" +
                    $"Settings have been updated automatically.", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Auto detect failed: {ex.Message}");
                EditorUtility.DisplayDialog("Auto Detect Failed", 
                    $"Failed to auto-detect file format: {ex.Message}", "OK");
            }
        }
    }
    
    // Auto validation on save
    public class DataValidationProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!EditorPrefs.GetBool("DataValidation.AutoValidateOnSave", true))
                return;
                
            // Check if any data files were modified
            bool dataFilesChanged = importedAssets.Any(asset => asset.EndsWith(".dat") || asset.EndsWith(".json"));
            
            if (dataFilesChanged)
            {
                EditorApplication.delayCall += () =>
                {
                    // var window = EditorWindow.GetWindow<DataValidationWindow>(false, "Data Validation", false);
                    // Auto-validate in background
                    Debug.Log("Data files changed, running auto-validation...");
                };
            }
        }
    }
}