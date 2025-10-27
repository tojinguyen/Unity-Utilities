#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace TirexGame.Utils.Editor.AddressableImporter
{
    [Serializable]
    public class FolderData
    {
        [SerializeField] private string folderPath;
        [SerializeField] private string groupName;
        [SerializeField] private NamingConvention namingConvention;
        [SerializeField] private string customPrefix;
        [SerializeField] private bool includeSubfolders;
        [SerializeField] private bool groupSubfoldersSeparately;
        [SerializeField] private string[] excludedFileExtensions;
        [SerializeField] private bool isEnabled;
        [SerializeField] private string excludedSubfolder;
        
        // Schema Configuration
        [SerializeField] private BundledAssetGroupSchema.BundlePackingMode bundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        [SerializeField] private BundledAssetGroupSchema.BundleCompressionMode compressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        [SerializeField] private string buildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
        [SerializeField] private string loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";
        [SerializeField] private bool includeInBuild = true;
        [SerializeField] private bool staticContent = false;

        public string ExcludedSubfolder
        {
            get => excludedSubfolder;
            set => excludedSubfolder = value;
        }

        public string FolderPath
        {
            get => folderPath;
            set => folderPath = value;
        }

        public string GroupName
        {
            get => groupName;
            set => groupName = value;
        }

        public NamingConvention NamingConvention
        {
            get => namingConvention;
            set => namingConvention = value;
        }

        public string CustomPrefix
        {
            get => customPrefix;
            set => customPrefix = value;
        }

        public bool IncludeSubfolders
        {
            get => includeSubfolders;
            set => includeSubfolders = value;
        }

        public bool GroupSubfoldersSeparately
        {
            get => groupSubfoldersSeparately;
            set => groupSubfoldersSeparately = value;
        }

        public string[] ExcludedFileExtensions
        {
            get => excludedFileExtensions;
            set => excludedFileExtensions = value;
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        // Schema Properties
        public BundledAssetGroupSchema.BundlePackingMode BundleMode
        {
            get => bundleMode;
            set => bundleMode = value;
        }

        public BundledAssetGroupSchema.BundleCompressionMode CompressionType
        {
            get => compressionType;
            set => compressionType = value;
        }

        public string BuildPath
        {
            get => buildPath;
            set => buildPath = value;
        }

        public string LoadPath
        {
            get => loadPath;
            set => loadPath = value;
        }

        public bool IncludeInBuild
        {
            get => includeInBuild;
            set => includeInBuild = value;
        }

        public bool StaticContent
        {
            get => staticContent;
            set => staticContent = value;
        }

        public FolderData()
        {
            folderPath = "";
            groupName = "Default";
            namingConvention = NamingConvention.FileName;
            customPrefix = "";
            includeSubfolders = true;
            groupSubfoldersSeparately = false;
            excludedFileExtensions = new[] { ".cs", ".js", ".dll" };
            isEnabled = true;
            excludedSubfolder = "None";
            
            // Schema defaults
            bundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            compressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            buildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
            loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";
            includeInBuild = true;
            staticContent = false;
        }

        public FolderData(string path, string group = "Default")
        {
            folderPath = path;
            groupName = group;
            namingConvention = NamingConvention.FileName;
            customPrefix = "";
            includeSubfolders = true;
            groupSubfoldersSeparately = false;
            excludedFileExtensions = new[] { ".cs", ".js", ".dll" };
            isEnabled = true;
            excludedSubfolder = "None";
            
            // Schema defaults
            bundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            compressionType = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            buildPath = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
            loadPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";
            includeInBuild = true;
            staticContent = false;
        }
    }
}
#endif