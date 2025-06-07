#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace TirexGame.Utils.IAP.Editor
{
    public class IAPSetupWindow : EditorWindow
    {
        private string configPath = "Assets/Utils/Configs/";
        private string configName = "IAPConfig";
        
        [MenuItem("TirexGame/IAP/Setup IAP")]
        public static void ShowWindow()
        {
            GetWindow<IAPSetupWindow>("IAP Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("IAP Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            // Unity IAP installation check
            EditorGUILayout.LabelField("Unity IAP SDK", EditorStyles.boldLabel);
            
            if (!IsUnityPurchasingInstalled())
            {
                EditorGUILayout.HelpBox("⚠ Unity IAP is not installed", MessageType.Warning);
                
                if (GUILayout.Button("Install Unity IAP"))
                {
                    InstallUnityIAP();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Manual Installation:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Window > Package Manager > Unity Registry > In App Purchasing", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Unity IAP is installed", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            // Configuration creation
            EditorGUILayout.LabelField("Configuration Setup", EditorStyles.boldLabel);
            
            configPath = EditorGUILayout.TextField("Config Path:", configPath);
            configName = EditorGUILayout.TextField("Config Name:", configName);
            
            if (GUILayout.Button("Create IAP Configuration"))
            {
                CreateIAPConfig();
            }
            
            EditorGUILayout.Space(10);
            
            // Scene setup
            EditorGUILayout.LabelField("Scene Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add IAPManager to Scene"))
            {
                AddIAPManagerToScene();
            }
            
            EditorGUILayout.Space(10);
            
            // Documentation
            EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open Unity IAP Documentation"))
            {
                Application.OpenURL("https://docs.unity3d.com/Manual/UnityIAP.html");
            }
            
            if (GUILayout.Button("Open Store Setup Guides"))
            {
                Application.OpenURL("https://docs.unity3d.com/Manual/UnityIAPSettingUp.html");
            }
        }
        
        private bool IsUnityPurchasingInstalled()
        {
            // Check if Unity Purchasing is installed
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name.Contains("UnityEngine.Purchasing"))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void InstallUnityIAP()
        {
            // Install Unity IAP via Package Manager
            UnityEditor.PackageManager.Client.Add("com.unity.purchasing");
            EditorUtility.DisplayDialog("Installing Unity IAP", 
                "Installing Unity IAP via Package Manager. Please wait for the installation to complete.", 
                "OK");
        }
        
        private void CreateIAPConfig()
        {
            // Ensure directory exists
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
            
            // Create config asset
            var config = CreateInstance<IAPConfig>();
            var fullPath = Path.Combine(configPath, $"{configName}.asset");
            
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            Selection.activeObject = config;
            
            EditorUtility.DisplayDialog("Success", 
                $"IAP Configuration created at {fullPath}\n\nPlease configure your products in the inspector.", 
                "OK");
        }
        
        private void AddIAPManagerToScene()
        {
            // Check if IAPManager already exists in scene
            var existingIAPManager = FindObjectOfType<IAPManager>();
            if (existingIAPManager != null)
            {
                EditorUtility.DisplayDialog("IAPManager Exists", 
                    "IAPManager already exists in the scene.", 
                    "OK");
                Selection.activeGameObject = existingIAPManager.gameObject;
                return;
            }
            
            // Create new GameObject with IAPManager
            var iapManagerGO = new GameObject("IAPManager");
            var iapManager = iapManagerGO.AddComponent<IAPManager>();
            
            // Try to find the config asset
            string[] guids = AssetDatabase.FindAssets("t:IAPConfig");
            if (guids.Length > 0)
            {
                string configAssetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var config = AssetDatabase.LoadAssetAtPath<IAPConfig>(configAssetPath);
                
                // Use reflection to set the config field
                var configField = typeof(IAPManager).GetField("config", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (configField != null)
                {
                    configField.SetValue(iapManager, config);
                }
            }
            
            // Mark as dirty for saving
            EditorUtility.SetDirty(iapManagerGO);
            
            // Select the created GameObject
            Selection.activeGameObject = iapManagerGO;
            
            EditorUtility.DisplayDialog("Success", 
                "IAPManager added to scene successfully!\n\nDon't forget to assign the IAPConfig in the inspector.", 
                "OK");
        }
    }
}
#endif
