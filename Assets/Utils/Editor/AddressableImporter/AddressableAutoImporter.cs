#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace TirexGame.Utils.Editor.AddressableImporter
{
    /// <summary>
    /// Automatically imports assets into Addressables based on folder configurations when assets are added, moved, or renamed.
    /// </summary>
    public class AddressableAutoImporter : AssetPostprocessor
    {
        private static AddressableImporterConfig GetConfig()
        {
            string[] configGuids = AssetDatabase.FindAssets("t:AddressableImporterConfig");
            if (configGuids.Length == 0)
            {
                return null;
            }

            string configPath = AssetDatabase.GUIDToAssetPath(configGuids[0]);
            return AssetDatabase.LoadAssetAtPath<AddressableImporterConfig>(configPath);
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var config = GetConfig();
            if (config == null || !config.AutoImportOnAssetChange)
            {
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                if (config.LogImportResults)
                {
                    Debug.LogWarning("Addressable Asset Settings not found. Cannot auto-import assets.");
                }
                return;
            }

            // Process imported assets (newly added or reimported)
            foreach (string assetPath in importedAssets)
            {
                ProcessAssetForAutoImport(assetPath, config, settings);
            }

            // Process moved assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string newPath = movedAssets[i];
                string oldPath = movedFromAssetPaths[i];
                
                // Remove from old location if it was in addressables
                RemoveAssetFromAddressables(oldPath, config, settings);
                
                // Add to new location if it matches a rule
                ProcessAssetForAutoImport(newPath, config, settings);
            }

            // Process deleted assets
            if (config.RemoveFromAddressablesOnDelete)
            {
                foreach (string assetPath in deletedAssets)
                {
                    RemoveAssetFromAddressables(assetPath, config, settings);
                }
            }
        }

        private static void ProcessAssetForAutoImport(string assetPath, AddressableImporterConfig config, AddressableAssetSettings settings)
        {
            // Skip meta files and directories
            if (assetPath.EndsWith(".meta") || AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            // Find matching folder configuration
            FolderData matchingConfig = config.GetFolderConfigurationForPath(assetPath);
            if (matchingConfig == null || !matchingConfig.IsEnabled)
            {
                return;
            }

            // Check if the file extension is excluded
            string extension = Path.GetExtension(assetPath);
            if (matchingConfig.ExcludedFileExtensions != null && 
                matchingConfig.ExcludedFileExtensions.Any(ext => extension.Equals(ext, System.StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            // Check if the asset is in an excluded subfolder
            if (!string.IsNullOrEmpty(matchingConfig.ExcludedSubfolder))
            {
                string relativePath = assetPath.Substring(matchingConfig.FolderPath.Length + 1);
                if (relativePath.StartsWith(matchingConfig.ExcludedSubfolder))
                {
                    return;
                }
            }

            // Get or create the appropriate group
            string groupName = GetGroupNameForAsset(assetPath, matchingConfig);
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null);
                SetupGroupSchemas(group, matchingConfig, config);
            }

            // Get asset GUID
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                if (config.LogImportResults)
                {
                    Debug.LogWarning($"Could not get GUID for asset: {assetPath}");
                }
                return;
            }

            // Create or move entry to the group
            var entry = settings.CreateOrMoveEntry(guid, group);
            if (entry == null)
            {
                if (config.LogImportResults)
                {
                    Debug.LogWarning($"Could not create addressable entry for asset: {assetPath}");
                }
                return;
            }

            // Set the address name
            string addressableName = GenerateAddressableName(assetPath, matchingConfig);
            entry.address = addressableName;

            if (config.LogImportResults)
            {
                Debug.Log($"Auto-imported asset '{assetPath}' to addressable group '{groupName}' with address '{addressableName}'");
            }
        }

        private static void RemoveAssetFromAddressables(string assetPath, AddressableImporterConfig config, AddressableAssetSettings settings)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                string groupName = entry.parentGroup.Name;
                string address = entry.address;
                
                settings.RemoveAssetEntry(guid);
                
                if (config.LogImportResults)
                {
                    Debug.Log($"Removed asset '{assetPath}' (address: '{address}') from addressable group '{groupName}'");
                }
            }
        }

        private static string GetGroupNameForAsset(string assetPath, FolderData folderConfig)
        {
            if (!folderConfig.GroupSubfoldersSeparately)
            {
                return folderConfig.GroupName;
            }

            // Get the immediate subdirectory within the configured folder
            string relativePath = assetPath.Substring(folderConfig.FolderPath.Length + 1);
            string[] pathParts = relativePath.Split('/');
            
            if (pathParts.Length > 1)
            {
                // Asset is in a subdirectory
                string subdirectory = pathParts[0];
                return $"{folderConfig.GroupName}_{subdirectory}";
            }
            else
            {
                // Asset is directly in the configured folder
                return folderConfig.GroupName;
            }
        }

        private static string GenerateAddressableName(string assetPath, FolderData folderConfig)
        {
            string name = Path.GetFileNameWithoutExtension(assetPath);

            switch (folderConfig.NamingConvention)
            {
                case NamingConvention.FileName:
                    return name;

                case NamingConvention.FullPath:
                    return assetPath.Substring("Assets/".Length).Replace(Path.GetExtension(assetPath), "");

                case NamingConvention.RelativeToFolder:
                    string relativePath = assetPath.Substring(folderConfig.FolderPath.Length + 1);
                    return relativePath.Replace(Path.GetExtension(relativePath), "");

                case NamingConvention.PrefixedFileName:
                    return $"{folderConfig.CustomPrefix}_{name}";

                default:
                    return name;
            }
        }

        private static void SetupGroupSchemas(AddressableAssetGroup group, FolderData folderConfig, AddressableImporterConfig config)
        {
            // Add BundledAssetGroupSchema
            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema == null)
            {
                bundledSchema = group.AddSchema<BundledAssetGroupSchema>();
            }

            // Configure BundledAssetGroupSchema
            bundledSchema.IncludeInBuild = folderConfig.IncludeInBuild;
            bundledSchema.BundleMode = folderConfig.BundleMode;
            bundledSchema.Compression = folderConfig.CompressionType;
            bundledSchema.BuildPath.SetVariableByName(group.Settings, folderConfig.BuildPath);
            bundledSchema.LoadPath.SetVariableByName(group.Settings, folderConfig.LoadPath);

            // Add ContentUpdateGroupSchema
            var contentUpdateSchema = group.GetSchema<ContentUpdateGroupSchema>();
            if (contentUpdateSchema == null)
            {
                contentUpdateSchema = group.AddSchema<ContentUpdateGroupSchema>();
            }

            // Configure ContentUpdateGroupSchema
            contentUpdateSchema.StaticContent = folderConfig.StaticContent;

            // Mark the group as dirty to save changes
            EditorUtility.SetDirty(group);
        }
    }
}
#endif