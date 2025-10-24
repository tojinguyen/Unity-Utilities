#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.Editor.AddressableImporter
{
    [CreateAssetMenu(fileName = "AddressableImporterConfig", menuName = "TirexGame/Addressable Importer Config")]
    [Serializable]
    public class AddressableImporterConfig : ScriptableObject
    {
        [SerializeField] private List<FolderData> folderConfigurations = new List<FolderData>();
        [SerializeField] private bool autoImportOnAssetChange = true;
        [SerializeField] private bool logImportResults = true;
        [SerializeField] private bool removeFromAddressablesOnDelete = true;

        public List<FolderData> FolderConfigurations
        {
            get => folderConfigurations;
            set => folderConfigurations = value;
        }

        public bool AutoImportOnAssetChange
        {
            get => autoImportOnAssetChange;
            set => autoImportOnAssetChange = value;
        }

        public bool LogImportResults
        {
            get => logImportResults;
            set => logImportResults = value;
        }

        public bool RemoveFromAddressablesOnDelete
        {
            get => removeFromAddressablesOnDelete;
            set => removeFromAddressablesOnDelete = value;
        }

        public void AddFolderConfiguration(FolderData folderData)
        {
            if (folderData != null && !folderConfigurations.Contains(folderData))
            {
                folderConfigurations.Add(folderData);
            }
        }

        public void RemoveFolderConfiguration(FolderData folderData)
        {
            if (folderData != null)
            {
                folderConfigurations.Remove(folderData);
            }
        }

        public void RemoveFolderConfigurationAt(int index)
        {
            if (index >= 0 && index < folderConfigurations.Count)
            {
                folderConfigurations.RemoveAt(index);
            }
        }

        public FolderData GetFolderConfigurationForPath(string assetPath)
        {
            foreach (var config in folderConfigurations)
            {
                if (config.IsEnabled && !string.IsNullOrEmpty(config.FolderPath))
                {
                    if (assetPath.StartsWith(config.FolderPath))
                    {
                        return config;
                    }
                }
            }
            return null;
        }

        private void OnValidate()
        {
            // Ensure we have at least one folder configuration
            if (folderConfigurations.Count == 0)
            {
                folderConfigurations.Add(new FolderData());
            }
        }
    }
}
#endif