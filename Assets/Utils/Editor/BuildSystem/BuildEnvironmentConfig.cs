#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace TirexGame.Utils.Editor.BuildSystem
{
    /// <summary>
    /// Build environment configuration containing scripting defines for different environments
    /// </summary>
    [CreateAssetMenu(fileName = "BuildEnvironmentConfig", menuName = "TirexGame/Build/Environment Config", order = 1)]
    public class BuildEnvironmentConfig : ScriptableObject
    {
        [Header("Current Environment")]
        [SerializeField] private BuildEnvironment selectedEnvironment = BuildEnvironment.Development;
        
        [Header("Development Environment")]
        [SerializeField] private List<string> developmentDefines = new List<string>();
        
        [Header("Staging Environment")]  
        [SerializeField] private List<string> stagingDefines = new List<string>();
        
        [Header("Production Environment")]
        [SerializeField] private List<string> productionDefines = new List<string>();
        
        [Header("Build Settings")]
        [SerializeField] private bool autoApplyOnBuild = true;
        [SerializeField] private bool showBuildDialog = true;
        
        #region Properties
        
        /// <summary>
        /// Currently selected build environment
        /// </summary>
        public BuildEnvironment SelectedEnvironment 
        { 
            get => selectedEnvironment; 
            set => selectedEnvironment = value; 
        }
        
        /// <summary>
        /// Whether to automatically apply defines when building
        /// </summary>
        public bool AutoApplyOnBuild 
        { 
            get => autoApplyOnBuild; 
            set => autoApplyOnBuild = value; 
        }
        
        /// <summary>
        /// Whether to show build confirmation dialog
        /// </summary>
        public bool ShowBuildDialog 
        { 
            get => showBuildDialog; 
            set => showBuildDialog = value; 
        }
        
        /// <summary>
        /// Development scripting defines
        /// </summary>
        public List<string> DevelopmentDefines 
        { 
            get => developmentDefines; 
            set => developmentDefines = value; 
        }
        
        /// <summary>
        /// Staging scripting defines
        /// </summary>
        public List<string> StagingDefines 
        { 
            get => stagingDefines; 
            set => stagingDefines = value; 
        }
        
        /// <summary>
        /// Production scripting defines
        /// </summary>
        public List<string> ProductionDefines 
        { 
            get => productionDefines; 
            set => productionDefines = value; 
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get scripting defines for the currently selected environment
        /// </summary>
        /// <returns>List of scripting defines</returns>
        public List<string> GetCurrentDefines()
        {
            return selectedEnvironment switch
            {
                BuildEnvironment.Development => developmentDefines,
                BuildEnvironment.Staging => stagingDefines,
                BuildEnvironment.Production => productionDefines,
                _ => developmentDefines
            };
        }
        
        /// <summary>
        /// Get scripting defines for a specific environment
        /// </summary>
        /// <param name="environment">Target environment</param>
        /// <returns>List of scripting defines</returns>
        public List<string> GetDefinesForEnvironment(BuildEnvironment environment)
        {
            return environment switch
            {
                BuildEnvironment.Development => developmentDefines,
                BuildEnvironment.Staging => stagingDefines,
                BuildEnvironment.Production => productionDefines,
                _ => developmentDefines
            };
        }
        
        /// <summary>
        /// Get all scripting defines as a semicolon-separated string for Unity PlayerSettings
        /// </summary>
        /// <returns>Formatted string for PlayerSettings</returns>
        public string GetCurrentDefinesAsString()
        {
            var defines = GetCurrentDefines();
            return defines.Count > 0 ? string.Join(";", defines) : string.Empty;
        }
        
        /// <summary>
        /// Check if a specific define exists in the current environment
        /// </summary>
        /// <param name="define">Define to check</param>
        /// <returns>True if define exists</returns>
        public bool HasDefine(string define)
        {
            return GetCurrentDefines().Contains(define);
        }
        
        /// <summary>
        /// Add a define to the current environment if it doesn't exist
        /// </summary>
        /// <param name="define">Define to add</param>
        public void AddDefine(string define)
        {
            var currentDefines = GetCurrentDefines();
            if (!currentDefines.Contains(define))
            {
                currentDefines.Add(define);
            }
        }
        
        /// <summary>
        /// Remove a define from the current environment
        /// </summary>
        /// <param name="define">Define to remove</param>
        public void RemoveDefine(string define)
        {
            GetCurrentDefines().Remove(define);
        }
        
        #endregion
        
        #region Unity Events
        
        private void OnValidate()
        {
            // Clean up empty or null defines
            CleanDefinesList(developmentDefines);
            CleanDefinesList(stagingDefines);
            CleanDefinesList(productionDefines);
        }
        
        #endregion
        
        #region Private Methods
        
        private void CleanDefinesList(List<string> defines)
        {
            if (defines == null) return;
            
            for (int i = defines.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(defines[i]))
                {
                    defines.RemoveAt(i);
                }
                else
                {
                    // Trim whitespace and ensure valid define format
                    defines[i] = defines[i].Trim();
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Available build environments
    /// </summary>
    public enum BuildEnvironment
    {
        Development = 0,
        Staging = 1,
        Production = 2
    }
}
#endif