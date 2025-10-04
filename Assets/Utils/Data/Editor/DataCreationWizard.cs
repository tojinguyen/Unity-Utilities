using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using TirexGame.Utils.Data;

namespace TirexGame.Utils.Data.Editor
{
    public class DataCreationWizard : ScriptableWizard
    {
        [Header("Data Model Information")]
        public string dataModelName = "NewDataModel";
        public string namespaceName = "YourProject.Data";
        
        [Header("Fields Configuration")]
        public List<FieldDefinition> fields = new List<FieldDefinition>();
        
        [Header("Options")]
        public bool implementIValidatable = true;
        public bool generateDefaultValues = true;
        public bool createSampleData = true;
        
        private Vector2 _scrollPosition;
        private readonly string[] _supportedTypes = 
        {
            "string", "int", "float", "bool", "DateTime", "Vector3", "Vector2", "Color"
        };
        
        [System.Serializable]
        public class FieldDefinition
        {
            public string fieldName = "";
            public string fieldType = "string";
            public string defaultValue = "";
            public bool isPublic = true;
            public string description = "";
        }
        
        public static void CreateWizard()
        {
            var wizard = DisplayWizard<DataCreationWizard>("Data Model Creation Wizard", "Create", "Cancel");
            wizard.minSize = new Vector2(500, 600);
            
            // Add some default fields
            wizard.fields.Add(new FieldDefinition { fieldName = "id", fieldType = "string", defaultValue = "\"\"" });
            wizard.fields.Add(new FieldDefinition { fieldName = "name", fieldType = "string", defaultValue = "\"Default Name\"" });
        }
        
        protected override bool DrawWizardGUI()
        {
            bool changed = false;
            
            EditorGUILayout.LabelField("Create New Data Model", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            dataModelName = EditorGUILayout.TextField("Data Model Name", dataModelName);
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            if (EditorGUI.EndChangeCheck()) changed = true;
            
            EditorGUILayout.Space();
            
            // Options
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            implementIValidatable = EditorGUILayout.Toggle("Implement IValidatable", implementIValidatable);
            generateDefaultValues = EditorGUILayout.Toggle("Generate Default Values", generateDefaultValues);
            createSampleData = EditorGUILayout.Toggle("Create Sample Data", createSampleData);
            if (EditorGUI.EndChangeCheck()) changed = true;
            
            EditorGUILayout.Space();
            
            // Fields
            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));
            
            for (int i = 0; i < fields.Count; i++)
            {
                DrawFieldEditor(i);
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Field"))
            {
                fields.Add(new FieldDefinition());
                changed = true;
            }
            
            if (GUILayout.Button("Remove Last Field") && fields.Count > 0)
            {
                fields.RemoveAt(fields.Count - 1);
                changed = true;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Preview
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.TextArea(GeneratePreview(), GUILayout.Height(150));
            EditorGUILayout.EndVertical();
            
            return changed;
        }
        
        private void DrawFieldEditor(int index)
        {
            var field = fields[index];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Field {index + 1}", EditorStyles.boldLabel, GUILayout.Width(60));
            
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                fields.RemoveAt(index);
                return;
            }
            EditorGUILayout.EndHorizontal();
            
            field.fieldName = EditorGUILayout.TextField("Name", field.fieldName);
            
            // Type dropdown
            var typeIndex = Array.IndexOf(_supportedTypes, field.fieldType);
            if (typeIndex == -1) typeIndex = 0;
            
            typeIndex = EditorGUILayout.Popup("Type", typeIndex, _supportedTypes);
            field.fieldType = _supportedTypes[typeIndex];
            
            field.defaultValue = EditorGUILayout.TextField("Default Value", field.defaultValue);
            field.isPublic = EditorGUILayout.Toggle("Public", field.isPublic);
            field.description = EditorGUILayout.TextField("Description", field.description);
            
            EditorGUILayout.EndVertical();
        }
        
        private string GeneratePreview()
        {
            return GenerateDataModelCode().Substring(0, Math.Min(500, GenerateDataModelCode().Length)) + "...";
        }
        
        void OnWizardCreate()
        {
            try
            {
                CreateDataModel();
                EditorUtility.DisplayDialog("Success", "Data model created successfully!", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create data model: {ex.Message}", "OK");
                Debug.LogError($"Data model creation failed: {ex.Message}");
            }
        }
        
        private void CreateDataModel()
        {
            // Validate input
            if (string.IsNullOrEmpty(dataModelName))
            {
                throw new ArgumentException("Data model name cannot be empty");
            }
            
            if (fields.Any(f => string.IsNullOrEmpty(f.fieldName)))
            {
                throw new ArgumentException("All fields must have names");
            }
            
            // Create directory structure
            string scriptsPath = "Assets/Scripts/Data";
            if (!Directory.Exists(scriptsPath))
            {
                Directory.CreateDirectory(scriptsPath);
            }
            
            // Generate and save the data model
            string dataModelCode = GenerateDataModelCode();
            string filePath = Path.Combine(scriptsPath, $"{dataModelName}.cs");
            File.WriteAllText(filePath, dataModelCode);
            
            // Create sample data if requested
            if (createSampleData)
            {
                CreateSampleDataFile();
            }
            
            // Refresh the asset database
            AssetDatabase.Refresh();
            
            // Select the created file
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
        
        private string GenerateDataModelCode()
        {
            var code = new System.Text.StringBuilder();
            
            // Usings
            code.AppendLine("using System;");
            code.AppendLine("using UnityEngine;");
            code.AppendLine("using TirexGame.Utils.Data;");
            
            if (implementIValidatable)
            {
                code.AppendLine("using System.Text;");
            }
            
            code.AppendLine();
            
            // Namespace
            if (!string.IsNullOrEmpty(namespaceName))
            {
                code.AppendLine($"namespace {namespaceName}");
                code.AppendLine("{");
            }
            
            // Class declaration
            var indent = string.IsNullOrEmpty(namespaceName) ? "" : "    ";
            code.AppendLine($"{indent}[Serializable]");
            
            var interfaces = $"IDataModel<{dataModelName}>";
            if (implementIValidatable)
            {
                interfaces += ", IValidatable";
            }
            
            code.AppendLine($"{indent}public class {dataModelName} : {interfaces}");
            code.AppendLine($"{indent}{{");
            
            // Fields
            foreach (var field in fields)
            {
                if (!string.IsNullOrEmpty(field.description))
                {
                    code.AppendLine($"{indent}    /// <summary>");
                    code.AppendLine($"{indent}    /// {field.description}");
                    code.AppendLine($"{indent}    /// </summary>");
                }
                
                var accessibility = field.isPublic ? "public" : "private";
                var typeName = GetTypeName(field.fieldType);
                code.AppendLine($"{indent}    {accessibility} {typeName} {field.fieldName};");
            }
            
            code.AppendLine();
            
            // SetDefaultData method
            code.AppendLine($"{indent}    public void SetDefaultData()");
            code.AppendLine($"{indent}    {{");
            
            if (generateDefaultValues)
            {
                foreach (var field in fields)
                {
                    var defaultValue = GetDefaultValue(field.fieldType, field.defaultValue);
                    code.AppendLine($"{indent}        {field.fieldName} = {defaultValue};");
                }
            }
            
            code.AppendLine($"{indent}    }}");
            
            // IValidatable implementation
            if (implementIValidatable)
            {
                code.AppendLine();
                code.AppendLine($"{indent}    public ValidationResult Validate()");
                code.AppendLine($"{indent}    {{");
                code.AppendLine($"{indent}        var errors = new StringBuilder();");
                code.AppendLine();
                
                // Generate basic validation for common types
                foreach (var field in fields)
                {
                    if (field.fieldType == "string")
                    {
                        code.AppendLine($"{indent}        if (string.IsNullOrEmpty({field.fieldName}))");
                        code.AppendLine($"{indent}            errors.AppendLine(\"{field.fieldName} cannot be empty\");");
                    }
                    else if (field.fieldType == "int" || field.fieldType == "float")
                    {
                        code.AppendLine($"{indent}        if ({field.fieldName} < 0)");
                        code.AppendLine($"{indent}            errors.AppendLine(\"{field.fieldName} cannot be negative\");");
                    }
                }
                
                code.AppendLine();
                code.AppendLine($"{indent}        return errors.Length == 0 ");
                code.AppendLine($"{indent}            ? ValidationResult.Valid()");
                code.AppendLine($"{indent}            : ValidationResult.Invalid(errors.ToString());");
                code.AppendLine($"{indent}    }}");
            }
            
            // Close class
            code.AppendLine($"{indent}}}");
            
            // Close namespace
            if (!string.IsNullOrEmpty(namespaceName))
            {
                code.AppendLine("}");
            }
            
            return code.ToString();
        }
        
        private string GetTypeName(string fieldType)
        {
            return fieldType switch
            {
                "string" => "string",
                "int" => "int",
                "float" => "float",
                "bool" => "bool",
                "DateTime" => "DateTime",
                "Vector3" => "Vector3",
                "Vector2" => "Vector2",
                "Color" => "Color",
                _ => "object"
            };
        }
        
        private string GetDefaultValue(string fieldType, string customDefault)
        {
            if (!string.IsNullOrEmpty(customDefault))
            {
                return customDefault;
            }
            
            return fieldType switch
            {
                "string" => "\"\"",
                "int" => "0",
                "float" => "0f",
                "bool" => "false",
                "DateTime" => "DateTime.UtcNow",
                "Vector3" => "Vector3.zero",
                "Vector2" => "Vector2.zero",
                "Color" => "Color.white",
                _ => "null"
            };
        }
        
        private void CreateSampleDataFile()
        {
            try
            {
                string samplePath = "Assets/Scripts/Data/Samples";
                if (!Directory.Exists(samplePath))
                {
                    Directory.CreateDirectory(samplePath);
                }
                
                var code = new System.Text.StringBuilder();
                
                code.AppendLine("using UnityEngine;");
                code.AppendLine($"using {namespaceName};");
                code.AppendLine("using TirexGame.Utils.Data;");
                code.AppendLine();
                code.AppendLine($"namespace {namespaceName}.Samples");
                code.AppendLine("{");
                code.AppendLine($"    public class {dataModelName}Sample : MonoBehaviour");
                code.AppendLine("    {");
                code.AppendLine("        [SerializeField]");
                code.AppendLine($"        private {dataModelName} sampleData = new {dataModelName}();");
                code.AppendLine();
                code.AppendLine("        private void Start()");
                code.AppendLine("        {");
                code.AppendLine("            // Initialize with default data");
                code.AppendLine("            sampleData.SetDefaultData();");
                code.AppendLine();
                code.AppendLine("            // You can modify the data here");
                code.AppendLine("            // sampleData.someField = someValue;");
                code.AppendLine();
                code.AppendLine("            // Example: Save the data");
                code.AppendLine("            // DataManager.SaveAsync(\"sample\", sampleData);");
                code.AppendLine("        }");
                code.AppendLine("    }");
                code.AppendLine("}");
                
                string sampleFilePath = Path.Combine(samplePath, $"{dataModelName}Sample.cs");
                File.WriteAllText(sampleFilePath, code.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create sample file: {ex.Message}");
            }
        }
        
        void OnWizardUpdate()
        {
            helpString = "Create a new data model with the specified fields and options.";
            
            // Validate wizard state
            isValid = !string.IsNullOrEmpty(dataModelName) && 
                     fields.All(f => !string.IsNullOrEmpty(f.fieldName));
            
            if (!isValid)
            {
                if (string.IsNullOrEmpty(dataModelName))
                {
                    errorString = "Data model name is required.";
                }
                else if (fields.Any(f => string.IsNullOrEmpty(f.fieldName)))
                {
                    errorString = "All fields must have names.";
                }
            }
            else
            {
                errorString = "";
            }
        }
    }
}