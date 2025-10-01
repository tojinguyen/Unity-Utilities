using System;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private string excludedFolder;

        public string ExcludedFolder
        {
            get => excludedFolder;
            set => excludedFolder = value;
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
            excludedFolder = "None";
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
            excludedFolder = "None";
        }
    }
}
