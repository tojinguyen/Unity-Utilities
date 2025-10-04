using UnityEngine;
using UnityEditor;
using TirexGame.Utils.Data;

namespace TirexGame.Utils.Data.Editor
{
    public static class DataEditorMenuItems
    {
        private const string MENU_ROOT = "TirexGame/Data/";
        
        [MenuItem(MENU_ROOT + "Data Manager Window", priority = 1)]
        public static void OpenDataManager()
        {
            DataManagerWindow.ShowWindow();
        }
        
        [MenuItem(MENU_ROOT + "Data Creation Wizard", priority = 2)]
        public static void OpenDataCreationWizard()
        {
            DataCreationWizard.CreateWizard();
        }
        
        [MenuItem(MENU_ROOT + "Open Data Folder", priority = 10)]
        public static void OpenDataFolder()
        {
            string dataPath = Application.persistentDataPath;
            
            if (System.IO.Directory.Exists(dataPath))
            {
                EditorUtility.RevealInFinder(dataPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Data folder does not exist: {dataPath}", "OK");
            }
        }
        
        [MenuItem(MENU_ROOT + "Clear All Data", priority = 11)]
        public static void ClearAllData()
        {
            if (EditorUtility.DisplayDialog("Clear All Data", 
                "Are you sure you want to delete all saved data? This action cannot be undone.", 
                "Delete All", "Cancel"))
            {
                try
                {
                    string dataPath = Application.persistentDataPath;
                    if (System.IO.Directory.Exists(dataPath))
                    {
                        var dataDirectories = System.IO.Directory.GetDirectories(dataPath);
                        foreach (var dir in dataDirectories)
                        {
                            System.IO.Directory.Delete(dir, true);
                        }
                        
                        EditorUtility.DisplayDialog("Success", "All data cleared successfully!", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to clear data: {ex.Message}");
                    EditorUtility.DisplayDialog("Error", $"Failed to clear data: {ex.Message}", "OK");
                }
            }
        }
        
        [MenuItem(MENU_ROOT + "Documentation", priority = 20)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/tojinguyen/Unity-Utilities/blob/main/Assets/Utils/Data/README.md");
        }
        
        [MenuItem(MENU_ROOT + "Report Issue", priority = 21)]
        public static void ReportIssue()
        {
            Application.OpenURL("https://github.com/tojinguyen/Unity-Utilities/issues");
        }
        
        // Preferences
        [SettingsProvider]
        public static SettingsProvider CreateDataManagerSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/TirexGame/Data Manager", SettingsScope.User)
            {
                label = "Data Manager",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Data Manager Preferences", EditorStyles.boldLabel);
                    EditorGUILayout.Space();
                    
                    var enableLogging = EditorPrefs.GetBool("DataManager.EnableLogging", true);
                    enableLogging = EditorGUILayout.Toggle("Enable Logging", enableLogging);
                    EditorPrefs.SetBool("DataManager.EnableLogging", enableLogging);
                    
                    var enableCaching = EditorPrefs.GetBool("DataManager.EnableCaching", true);
                    enableCaching = EditorGUILayout.Toggle("Enable Caching", enableCaching);
                    EditorPrefs.SetBool("DataManager.EnableCaching", enableCaching);
                    
                    var autoSaveInterval = EditorPrefs.GetFloat("DataManager.AutoSaveInterval", 300f);
                    autoSaveInterval = EditorGUILayout.FloatField("Auto Save Interval (seconds)", autoSaveInterval);
                    EditorPrefs.SetFloat("DataManager.AutoSaveInterval", autoSaveInterval);
                    
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("Reset to Defaults"))
                    {
                        EditorPrefs.DeleteKey("DataManager.EnableLogging");
                        EditorPrefs.DeleteKey("DataManager.EnableCaching");
                        EditorPrefs.DeleteKey("DataManager.AutoSaveInterval");
                    }
                },
                keywords = new[] { "Data", "Manager", "TirexGame", "Persistence" }
            };
            
            return provider;
        }
    }
}