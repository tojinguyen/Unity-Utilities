#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace TirexGame.Utils.Ads.Editor
{
    public class AdMobSetupWindow : EditorWindow
    {
        private string configPath = "Assets/Utils/Scripts/Ads/Resources";
        private string configName = "AdMobConfig";
        
        [MenuItem("TirexGame/Ads/Setup AdMob")]
        public static void ShowWindow()
        {
            GetWindow<AdMobSetupWindow>("AdMob Setup");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("AdMob Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This window helps you set up AdMob integration for your project.\n\n" +
                "Steps:\n" +
                "1. Install Google Mobile Ads SDK\n" +
                "2. Create AdMob Configuration\n" +
                "3. Configure your Ad Unit IDs\n" +
                "4. Add AdManager to your scene",
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            
            // Check if Google Mobile Ads SDK is installed
            if (IsGoogleMobileAdsInstalled())
            {
                EditorGUILayout.HelpBox("✓ Google Mobile Ads SDK is installed", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠ Google Mobile Ads SDK is not installed", MessageType.Warning);
                
                if (GUILayout.Button("Install Google Mobile Ads SDK"))
                {
                    InstallGoogleMobileAdsSDK();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Manual Installation:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Window > Package Manager > + > Add package from git URL", EditorStyles.miniLabel);
                EditorGUILayout.SelectableLabel("https://github.com/googleads/googleads-mobile-unity.git");
            }
            
            EditorGUILayout.Space(10);
            
            // Configuration creation
            EditorGUILayout.LabelField("Configuration Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            configPath = EditorGUILayout.TextField("Config Path:", configPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Config Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convert absolute path to relative path
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        configPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path", 
                            "Please select a folder within the Assets directory.", 
                            "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            configName = EditorGUILayout.TextField("Config Name:", configName);
            
            if (GUILayout.Button("Create AdMob Configuration"))
            {
                CreateAdMobConfig();
            }
            
            EditorGUILayout.Space(10);
            
            // Scene setup
            EditorGUILayout.LabelField("Scene Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add AdManager to Scene"))
            {
                AddAdManagerToScene();
            }
            
            EditorGUILayout.Space(10);
            
            // Documentation
            EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open AdMob Documentation"))
            {
                Application.OpenURL("https://developers.google.com/admob/unity/start");
            }
            
            if (GUILayout.Button("Open Google Mobile Ads Unity Plugin"))
            {
                Application.OpenURL("https://github.com/googleads/googleads-mobile-unity");
            }
        }
        
        private bool IsGoogleMobileAdsInstalled()
        {
            // Check if the Google Mobile Ads assembly exists
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name.Contains("GoogleMobileAds"))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void InstallGoogleMobileAdsSDK()
        {
            // This will open the Package Manager with the Git URL
            UnityEditor.PackageManager.Client.Add("https://github.com/googleads/googleads-mobile-unity.git");
            EditorUtility.DisplayDialog("Installing SDK", 
                "Installing Google Mobile Ads SDK via Package Manager. Please wait for the installation to complete.", 
                "OK");
        }
        
        private void CreateAdMobConfig()
        {
            // Create directory if it doesn't exist
            string fullPath = Path.Combine(Application.dataPath, configPath.Replace("Assets/", ""));
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            // Create the ScriptableObject
            var config = ScriptableObject.CreateInstance<AdMobConfig>();
            
            string assetPath = Path.Combine(configPath, configName + ".asset");
            
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            EditorUtility.DisplayDialog("Success", 
                $"AdMobConfig created at {assetPath}\n\nPlease configure your Ad Unit IDs in the inspector.", 
                "OK");
        }
        
        private void AddAdManagerToScene()
        {
            // Check if AdManager already exists in scene
            var existingAdManager = FindFirstObjectByType<AdManager>();
            if (existingAdManager != null)
            {
                EditorUtility.DisplayDialog("AdManager Exists", 
                    "AdManager already exists in the scene.", 
                    "OK");
                Selection.activeGameObject = existingAdManager.gameObject;
                return;
            }
            
            // Create new GameObject with AdManager
            var adManagerGO = new GameObject("AdManager");
            var adManager = adManagerGO.AddComponent<AdManager>();
            
            // Try to find the config asset and notify the user
            string[] guids = AssetDatabase.FindAssets("t:AdMobConfig");
            if (guids.Length > 0)
            {
                string configPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                ConsoleLogger.Log($"Found an AdMobConfig at {configPath}. Please assign it to the AdManager in the inspector.");
            }
            
            // Mark as dirty for saving
            EditorUtility.SetDirty(adManagerGO);
            
            // Select the created GameObject
            Selection.activeGameObject = adManagerGO;
            
            EditorUtility.DisplayDialog("Success", 
                "AdManager added to scene successfully!\n\nPlease find and assign the AdMobConfig in the inspector.", 
                "OK");
        }
    }
}
#endif