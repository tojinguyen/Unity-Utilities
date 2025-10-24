#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace TirexGame.Utils.Editor.AddressableImporter
{
    public class AddressableImporterWindow : EditorWindow
    {
        private AddressableImporterConfig config;
        private Vector2 scrollPosition;
        private int m_IndexToRemove = -1;
        private string m_SearchQuery = "";

        [MenuItem("TirexGame/Editor/Addressable/Addressable Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<AddressableImporterWindow>("Addressable Importer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
        }

        private void OnGUI()
        {
            if (config == null)
            {
                LoadOrCreateConfig();
                return;
            }

            DrawHeader();
            DrawConfigSection();
            DrawFolderConfigurations();
            DrawActionButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Addressable Asset Importer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Automatically manage addressable assets based on folder configurations", EditorStyles.helpBox);
            EditorGUILayout.Space();
        }

        private void DrawConfigSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            config.AutoImportOnAssetChange = EditorGUILayout.Toggle("Auto Import on Asset Change", config.AutoImportOnAssetChange);
            config.LogImportResults = EditorGUILayout.Toggle("Log Import Results", config.LogImportResults);
            config.RemoveFromAddressablesOnDelete = EditorGUILayout.Toggle("Remove from Addressables on Delete", config.RemoveFromAddressablesOnDelete);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawFolderConfigurations()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Folder Configurations", EditorStyles.boldLabel);

            m_SearchQuery = EditorGUILayout.TextField("Search", m_SearchQuery);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < config.FolderConfigurations.Count; i++)
            {
                var folderData = config.FolderConfigurations[i];

                // Filtering logic
                if (!string.IsNullOrEmpty(m_SearchQuery) &&
                    !folderData.GroupName.ToLowerInvariant().Contains(m_SearchQuery.ToLowerInvariant()) &&
                    !folderData.FolderPath.ToLowerInvariant().Contains(m_SearchQuery.ToLowerInvariant()))
                {
                    continue; // Skip if it doesn't match
                }

                DrawFolderConfiguration(i);
            }

            if (m_IndexToRemove != -1)
            {
                config.RemoveFolderConfigurationAt(m_IndexToRemove);
                EditorUtility.SetDirty(config);
                m_IndexToRemove = -1;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Folder Configuration"))
            {
                config.AddFolderConfiguration(new FolderData());
                EditorUtility.SetDirty(config);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawFolderConfiguration(int index)
        {
            var folderData = config.FolderConfigurations[index];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            folderData.IsEnabled = EditorGUILayout.Toggle(folderData.IsEnabled, GUILayout.Width(20));
            string headerLabel = string.IsNullOrEmpty(folderData.GroupName) 
                ? $"Configuration {index + 1}" 
                : folderData.GroupName;
            EditorGUILayout.LabelField(headerLabel, EditorStyles.boldLabel);

            if (GUILayout.Button("Reimport", GUILayout.Width(80)))
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings != null) ProcessFolder(folderData, settings);
            }
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Clear Addressables for Configuration",
                    $"This will remove all addressable entries from the groups managed by this configuration ({folderData.GroupName}). Are you sure?", "Yes", "Cancel"))
                {
                    ClearGroupForConfiguration(folderData);
                }
            }
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                m_IndexToRemove = index;
            }

            EditorGUILayout.EndHorizontal();

            if (folderData.IsEnabled)
            {
                // Folder Path
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Folder Path:", GUILayout.Width(100));
                folderData.FolderPath = EditorGUILayout.TextField(folderData.FolderPath);
                if (GUILayout.Button("Browse", GUILayout.Width(60)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    {
                        folderData.FolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                        EditorUtility.SetDirty(config);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Group Name
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Group Name:", GUILayout.Width(100));
                folderData.GroupName = EditorGUILayout.TextField(folderData.GroupName);
                EditorGUILayout.EndHorizontal();

                // Naming Convention
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Naming:", GUILayout.Width(100));
                folderData.NamingConvention = (NamingConvention)EditorGUILayout.EnumPopup(folderData.NamingConvention);
                EditorGUILayout.EndHorizontal();

                if (folderData.NamingConvention == NamingConvention.PrefixedFileName)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Custom Prefix:", GUILayout.Width(100));
                    folderData.CustomPrefix = EditorGUILayout.TextField(folderData.CustomPrefix);
                    EditorGUILayout.EndHorizontal();
                }

                // Include Subfolders
                folderData.IncludeSubfolders = EditorGUILayout.Toggle("Include Subfolders", folderData.IncludeSubfolders);

                // Group Subfolders Separately
                if (folderData.IncludeSubfolders)
                {
                    folderData.GroupSubfoldersSeparately = EditorGUILayout.Toggle("Group Subfolders Separately", folderData.GroupSubfoldersSeparately);
                }

                // Excluded Folder
                if (folderData.IncludeSubfolders)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Exclude Subfolder:", GUILayout.Width(120));

                    if (Directory.Exists(folderData.FolderPath))
                    {
                        var subdirectories = Directory.GetDirectories(folderData.FolderPath, "*", SearchOption.TopDirectoryOnly)
                            .Select(Path.GetFileName).ToList();
                        subdirectories.Insert(0, "None");

                        int selectedIndex = subdirectories.IndexOf(folderData.ExcludedSubfolder);
                        if (selectedIndex == -1) selectedIndex = 0;

                        int newIndex = EditorGUILayout.Popup(selectedIndex, subdirectories.ToArray());

                        if (newIndex != selectedIndex)
                        {
                            folderData.ExcludedSubfolder = subdirectories[newIndex];
                            EditorUtility.SetDirty(config);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Path is not valid");
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // Excluded File Extensions
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Excluded Extensions:", GUILayout.Width(120));

                if (Directory.Exists(folderData.FolderPath))
                {
                    var availableExtensions = ScanExtensions(folderData.FolderPath, folderData.IncludeSubfolders);
                    int mask = 0;
                    for (int i = 0; i < availableExtensions.Length; i++)
                    {
                        if (Array.Exists(folderData.ExcludedFileExtensions, ext => ext == availableExtensions[i]))
                        {
                            mask |= 1 << i;
                        }
                    }

                    int newMask = EditorGUILayout.MaskField(mask, availableExtensions);

                    if (newMask != mask)
                    {
                        var newExcluded = new List<string>();
                        for (int i = 0; i < availableExtensions.Length; i++)
                        {
                            if ((newMask & (1 << i)) != 0)
                            {
                                newExcluded.Add(availableExtensions[i]);
                            }
                        }
                        folderData.ExcludedFileExtensions = newExcluded.ToArray();
                        EditorUtility.SetDirty(config);
                    }
                }
                else
                {
                    // Fallback to text field if path is invalid
                    string extensionsString = string.Join(", ", folderData.ExcludedFileExtensions);
                    string newExtensionsString = EditorGUILayout.TextField(extensionsString);
                    if (newExtensionsString != extensionsString)
                    {
                        folderData.ExcludedFileExtensions = newExtensionsString.Split(',').Select(s => s.Trim()).ToArray();
                        EditorUtility.SetDirty(config);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import All Assets"))
            {
                ImportAllAssets();
            }

            if (GUILayout.Button("Clear All Addressables"))
            {
                if (EditorUtility.DisplayDialog("Clear All Addressables",
                    "This will remove ALL assets from addressable groups. Are you sure?", "Yes", "Cancel"))
                {
                    ClearAllAddressables();
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save Configuration"))
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                Debug.Log("Addressable Importer configuration saved.");
            }
        }

        /// <summary>
        /// Loads or creates the AddressableImporterConfig.
        /// Config is stored in Assets/Settings/ to ensure it persists per-project
        /// and is included in version control while avoiding conflicts in common Editor folders.
        /// </summary>
        private void LoadOrCreateConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:AddressableImporterConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                config = AssetDatabase.LoadAssetAtPath<AddressableImporterConfig>(path);
            }

            if (config == null)
            {
                config = CreateInstance<AddressableImporterConfig>();

                // Create config in Assets/Settings/ folder to avoid conflicts with common Editor folders
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }

                string configPath = "Assets/Settings/AddressableImporterConfig.asset";
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created new AddressableImporterConfig at {configPath}");
            }
        }

        private void ImportAllAssets()
        {
            if (config == null) return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings not found. Please set up Addressables first.");
                return;
            }

            int importedCount = 0;
            foreach (var folderConfig in config.FolderConfigurations)
            {
                if (!folderConfig.IsEnabled || string.IsNullOrEmpty(folderConfig.FolderPath))
                    continue;

                importedCount += ProcessFolder(folderConfig, settings);
            }

            if (config.LogImportResults)
            {
                Debug.Log($"Addressable Importer: Processed {importedCount} assets.");
            }
        }

        private int ProcessFolder(FolderData folderConfig, AddressableAssetSettings settings)
        {
            if (!Directory.Exists(folderConfig.FolderPath))
            {
                Debug.LogWarning($"Folder not found: {folderConfig.FolderPath}");
                return 0;
            }

            int processedCount = 0;

            // Process files in the root folder first, regardless of subfolder settings
            processedCount += ProcessFilesInDirectory(folderConfig.FolderPath, folderConfig.GroupName, folderConfig, settings, SearchOption.TopDirectoryOnly);

            // If including subfolders, iterate through them and handle exclusions
            if (folderConfig.IncludeSubfolders)
            {
                var subdirectories = Directory.GetDirectories(folderConfig.FolderPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var subDir in subdirectories)
                {
                    string subDirName = Path.GetFileName(subDir);

                    // Check if the subdirectory should be excluded
                    if (!string.IsNullOrEmpty(folderConfig.ExcludedSubfolder) && folderConfig.ExcludedSubfolder != "None" && subDirName == folderConfig.ExcludedSubfolder)
                    {
                        continue; // Skip this folder
                    }

                    // Determine the group name based on whether we are grouping separately
                    string groupName = folderConfig.GroupSubfoldersSeparately
                        ? $"{folderConfig.GroupName}_{subDirName}"
                        : folderConfig.GroupName;

                    // Process files in the subdirectory (recursively)
                    processedCount += ProcessFilesInDirectory(subDir, groupName, folderConfig, settings, SearchOption.AllDirectories);
                }
            }

            return processedCount;
        }

        private int ProcessFilesInDirectory(string path, string groupName, FolderData folderConfig, AddressableAssetSettings settings, SearchOption searchOption)
        {
            var files = Directory.GetFiles(path, "*.*", searchOption)
                .Where(f => !folderConfig.ExcludedFileExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .Where(f => !f.EndsWith(".meta"));

            if (!files.Any()) return 0;

            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null);
            }

            int processedCount = 0;
            foreach (var file in files)
            {
                string assetPath = file.Replace('\\', '/');
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid)) continue;

                string addressableName = GenerateAddressableName(assetPath, folderConfig);
                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = addressableName;

                processedCount++;
            }
            return processedCount;
        }

        private string GenerateAddressableName(string assetPath, FolderData folderConfig)
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

        private void ClearAllAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            foreach (var group in settings.groups.ToArray())
            {
                if (group.ReadOnly) continue;
                settings.RemoveGroup(group);
            }

            Debug.Log("All addressable groups cleared.");
        }

        private void ClearGroupForConfiguration(FolderData folderData)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            List<AddressableAssetGroup> groupsToRemove = new List<AddressableAssetGroup>();

            if (!folderData.GroupSubfoldersSeparately)
            {
                var group = settings.FindGroup(folderData.GroupName);
                if (group != null && !group.ReadOnly)
                {
                    groupsToRemove.Add(group);
                }
            }
            else
            {
                // Find all groups that could have been generated by this config
                // We iterate over a copy (.ToList()) because settings.groups will be modified.
                foreach (var group in settings.groups.ToList())
                {
                    if (!group.ReadOnly && (group.Name == folderData.GroupName || group.Name.StartsWith(folderData.GroupName + "_")))
                    {
                        groupsToRemove.Add(group);
                    }
                }
            }

            int removedCount = 0;
            foreach (var group in groupsToRemove)
            {
                settings.RemoveGroup(group);
                removedCount++;
            }

            if (config.LogImportResults)
            {
                Debug.Log($"Removed {removedCount} addressable group(s) for configuration '{folderData.FolderPath}'.");
            }
        }

        private string[] ScanExtensions(string path, bool includeSubfolders)
        {
            if (!Directory.Exists(path)) return new string[0];

            var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(path, "*.*", searchOption)
                .Where(f => !f.EndsWith(".meta"))
                .Select(Path.GetExtension)
                .Distinct()
                .OrderBy(ext => ext)
                .ToArray();
        }
    }
}
#endif