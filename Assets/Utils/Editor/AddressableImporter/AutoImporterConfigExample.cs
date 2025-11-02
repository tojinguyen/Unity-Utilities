#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace TirexGame.Utils.Editor.AddressableImporter.Examples
{
    /// <summary>
    /// Example script demonstrating how to programmatically configure Addressable Auto-Importer
    /// </summary>
    public static class AutoImporterConfigExample
    {
        [MenuItem("TirexGame/Examples/Setup Auto-Import Example")]
        public static void SetupExampleConfiguration()
        {
            // Find or create config
            var config = GetOrCreateConfig();
            
            // Clear existing configurations for clean setup
            config.FolderConfigurations.Clear();
            
            // Enable auto-import features
            config.AutoImportOnAssetChange = true;
            config.LogImportResults = true;
            config.RemoveFromAddressablesOnDelete = true;
            
            // Example 1: UI Textures - simple configuration
            var uiTexturesConfig = new FolderData
            {
                FolderPath = "Assets/Textures/UI",
                GroupName = "UI_Textures",
                NamingConvention = NamingConvention.FileName,
                IncludeSubfolders = true,
                GroupSubfoldersSeparately = false,
                ExcludedFileExtensions = new[] { ".cs", ".meta", ".txt" },
                IsEnabled = true,
                
                // Schema configuration
                BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether,
                CompressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4,
                IncludeInBuild = true,
                StaticContent = true
            };
            config.AddFolderConfiguration(uiTexturesConfig);
            
            // Example 2: Audio with separate subfolders
            var audioConfig = new FolderData
            {
                FolderPath = "Assets/Audio",
                GroupName = "Audio",
                NamingConvention = NamingConvention.RelativeToFolder,
                IncludeSubfolders = true,
                GroupSubfoldersSeparately = true, // Each subfolder gets its own group
                ExcludedFileExtensions = new[] { ".cs", ".meta" },
                IsEnabled = true,
                
                // Schema configuration  
                BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately,
                CompressionType = BundledAssetGroupSchema.BundleCompressionMode.LZMA,
                IncludeInBuild = true,
                StaticContent = false
            };
            config.AddFolderConfiguration(audioConfig);
            
            // Example 3: Prefabs with custom prefix
            var prefabsConfig = new FolderData
            {
                FolderPath = "Assets/Prefabs/Characters",
                GroupName = "Characters",
                NamingConvention = NamingConvention.PrefixedFileName,
                CustomPrefix = "char",
                IncludeSubfolders = false, // Only top level
                GroupSubfoldersSeparately = false,
                ExcludedFileExtensions = new[] { ".cs", ".meta", ".asset" },
                ExcludedSubfolder = "Editor", // Exclude Editor folder
                IsEnabled = true,
                
                // Schema configuration
                BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately,
                CompressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4,
                IncludeInBuild = true,
                StaticContent = true
            };
            config.AddFolderConfiguration(prefabsConfig);
            
            // Example 4: Sprites with full path naming
            var spritesConfig = new FolderData
            {
                FolderPath = "Assets/Sprites",
                GroupName = "Sprites",
                NamingConvention = NamingConvention.FullPath,
                IncludeSubfolders = true,
                GroupSubfoldersSeparately = false,
                ExcludedFileExtensions = new[] { ".cs", ".meta" },
                IsEnabled = true,
                
                // Schema configuration
                BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether,
                CompressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4,
                IncludeInBuild = true,
                StaticContent = true
            };
            config.AddFolderConfiguration(spritesConfig);
            
            // Save the configuration
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Auto-Importer example configuration created successfully!");
            Debug.Log("Configured folders:");
            Debug.Log("1. Assets/Textures/UI -> UI_Textures group (filename addressing)");
            Debug.Log("2. Assets/Audio -> Audio groups (separate groups per subfolder)");  
            Debug.Log("3. Assets/Prefabs/Characters -> Characters group (prefixed addressing)");
            Debug.Log("4. Assets/Sprites -> Sprites group (full path addressing)");
            Debug.Log("\nTry adding assets to these folders to see auto-import in action!");
        }
        
        [MenuItem("TirexGame/Examples/Create Test Folders")]
        public static void CreateTestFolders()
        {
            // Create the example folder structure
            CreateFolderIfNotExists("Assets/Textures");
            CreateFolderIfNotExists("Assets/Textures/UI");
            CreateFolderIfNotExists("Assets/Textures/UI/Icons");
            CreateFolderIfNotExists("Assets/Textures/UI/Backgrounds");
            
            CreateFolderIfNotExists("Assets/Audio");
            CreateFolderIfNotExists("Assets/Audio/Music");
            CreateFolderIfNotExists("Assets/Audio/SFX");
            CreateFolderIfNotExists("Assets/Audio/Voice");
            
            CreateFolderIfNotExists("Assets/Prefabs");
            CreateFolderIfNotExists("Assets/Prefabs/Characters");
            CreateFolderIfNotExists("Assets/Prefabs/Characters/Editor");
            
            CreateFolderIfNotExists("Assets/Sprites");
            CreateFolderIfNotExists("Assets/Sprites/UI");
            CreateFolderIfNotExists("Assets/Sprites/Environment");
            
            AssetDatabase.Refresh();
            Debug.Log("Test folder structure created! You can now test the auto-import functionality by adding assets to these folders.");
        }
        
        private static void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                string folderName = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
        
        private static AddressableImporterConfig GetOrCreateConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:AddressableImporterConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<AddressableImporterConfig>(path);
            }
            
            // Create new config
            var config = ScriptableObject.CreateInstance<AddressableImporterConfig>();
            
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            
            string configPath = "Assets/Settings/AddressableImporterConfig.asset";
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            
            return config;
        }
    }
}
#endif