using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using TirexGame.Utils.Data;

namespace TirexGame.Utils.Data.Editor
{
    [CustomPropertyDrawer(typeof(IDataModel<>), true)]
    public class DataModelPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw a custom header
            var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, label.text, EditorStyles.boldLabel);
            
            position.y += EditorGUIUtility.singleLineHeight + 2;
            position.height -= EditorGUIUtility.singleLineHeight + 2;
            
            EditorGUI.indentLevel++;
            
            // Draw default property field
            EditorGUI.PropertyField(position, property, true);
            
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight + 2;
        }
    }
    
    [CanEditMultipleObjects]
    public class DataModelInspector : Editor
    {
        private bool _showValidation = true;
        private bool _showMetadata = false;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Data Model Inspector", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            // Validation section
            _showValidation = EditorGUILayout.Foldout(_showValidation, "Validation", true);
            if (_showValidation)
            {
                EditorGUI.indentLevel++;
                DrawValidationSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Metadata section
            _showMetadata = EditorGUILayout.Foldout(_showMetadata, "Metadata", true);
            if (_showMetadata)
            {
                EditorGUI.indentLevel++;
                DrawMetadataSection();
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Default inspector
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            DrawDefaultInspector();
            
            // Action buttons
            EditorGUILayout.Space();
            DrawActionButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (GUILayout.Button("Validate Data"))
            {
                ValidateData();
            }
            
            // Check if the target implements IValidatable
            if (target is IValidatable validatable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Validation Status", EditorStyles.miniBoldLabel);
                
                var validationResult = validatable.Validate();
                if (validationResult.IsValid)
                {
                    EditorGUILayout.HelpBox("✓ Data is valid", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"✗ Validation failed: {validationResult.ErrorMessage}", MessageType.Error);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMetadataSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var targetType = target.GetType();
            
            EditorGUILayout.LabelField("Type Information", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Full Name:", targetType.FullName);
            EditorGUILayout.LabelField("Assembly:", targetType.Assembly.GetName().Name);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Interfaces", EditorStyles.miniBoldLabel);
            foreach (var interfaceType in targetType.GetInterfaces())
            {
                EditorGUILayout.LabelField("• " + interfaceType.Name);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Fields & Properties", EditorStyles.miniBoldLabel);
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            EditorGUILayout.LabelField($"Fields: {fields.Length}");
            EditorGUILayout.LabelField($"Properties: {properties.Length}");
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Data", 
                    "Are you sure you want to reset this data to default values?", 
                    "Reset", "Cancel"))
                {
                    ResetToDefault();
                }
            }
            
            if (GUILayout.Button("Export JSON"))
            {
                ExportToJSON();
            }
            
            if (GUILayout.Button("Import JSON"))
            {
                ImportFromJSON();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void ValidateData()
        {
            try
            {
                var validator = new DataValidator();
                var isValid = validator.ValidateData(target);
                
                if (isValid)
                {
                    EditorUtility.DisplayDialog("Validation", "Data is valid!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation", "Data validation failed. Check console for details.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Validation error: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Validation error: {ex.Message}", "OK");
            }
        }
        
        private void ResetToDefault()
        {
            try
            {
                // Call SetDefaultData if available
                var setDefaultMethod = target.GetType().GetMethod("SetDefaultData");
                if (setDefaultMethod != null)
                {
                    Undo.RecordObject(target, "Reset to Default");
                    setDefaultMethod.Invoke(target, null);
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "SetDefaultData method not found", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Reset error: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Reset error: {ex.Message}", "OK");
            }
        }
        
        private void ExportToJSON()
        {
            try
            {
                string path = EditorUtility.SaveFilePanel("Export JSON", "", target.GetType().Name, "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(target, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Success", "Data exported successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Export error: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Export error: {ex.Message}", "OK");
            }
        }
        
        private void ImportFromJSON()
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Import JSON", "", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    var importedData = Newtonsoft.Json.JsonConvert.DeserializeObject(json, target.GetType());
                    
                    if (importedData != null)
                    {
                        Undo.RecordObject(target, "Import from JSON");
                        EditorUtility.CopySerialized(importedData as UnityEngine.Object, target as UnityEngine.Object);
                        EditorUtility.SetDirty(target);
                        EditorUtility.DisplayDialog("Success", "Data imported successfully!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Import error: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"Import error: {ex.Message}", "OK");
            }
        }
    }
}