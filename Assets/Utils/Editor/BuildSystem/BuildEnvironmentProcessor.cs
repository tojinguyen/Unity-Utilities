#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;

namespace TirexGame.Utils.Editor.BuildSystem
{
    /// <summary>
    /// Build processor that automatically applies scripting defines based on the build environment configuration
    /// </summary>
    public class BuildEnvironmentProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100; // Execute early in the build process
        
        private const string CONFIG_SEARCH_FILTER = "t:BuildEnvironmentConfig";
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[BuildEnvironmentProcessor] Starting build preprocessing...");
            
            try
            {
                // Find the build environment config
                var config = FindBuildEnvironmentConfig();
                if (config == null)
                {
                    Debug.LogWarning("[BuildEnvironmentProcessor] No BuildEnvironmentConfig found. Skipping scripting defines setup.");
                    return;
                }
                
                if (!config.AutoApplyOnBuild)
                {
                    Debug.Log("[BuildEnvironmentProcessor] Auto-apply is disabled. Skipping scripting defines setup.");
                    return;
                }
                
                // Show build dialog if enabled
                if (config.ShowBuildDialog)
                {
                    bool proceed = ShowBuildConfirmationDialog(config);
                    if (!proceed)
                    {
                        Debug.Log("[BuildEnvironmentProcessor] Build cancelled by user.");
                        throw new BuildFailedException("Build cancelled by user.");
                    }
                }
                
                // Apply scripting defines
                ApplyScriptingDefines(config, report.summary.platform);
                
                Debug.Log($"[BuildEnvironmentProcessor] Successfully applied {config.SelectedEnvironment} environment defines for {report.summary.platform}");
            }
            catch (BuildFailedException)
            {
                throw; // Re-throw build cancellation
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BuildEnvironmentProcessor] Error during build preprocessing: {ex.Message}");
                // Don't fail the build for this, just log the error
            }
        }
        
        /// <summary>
        /// Find the first BuildEnvironmentConfig in the project
        /// </summary>
        /// <returns>BuildEnvironmentConfig or null if not found</returns>
        private BuildEnvironmentConfig FindBuildEnvironmentConfig()
        {
            string[] guids = AssetDatabase.FindAssets(CONFIG_SEARCH_FILTER);
            
            if (guids.Length == 0)
            {
                return null;
            }
            
            if (guids.Length > 1)
            {
                Debug.LogWarning($"[BuildEnvironmentProcessor] Found {guids.Length} BuildEnvironmentConfig assets. Using the first one found.");
            }
            
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<BuildEnvironmentConfig>(assetPath);
        }
        
        /// <summary>
        /// Show build confirmation dialog
        /// </summary>
        /// <param name="config">Build environment config</param>
        /// <returns>True if user wants to proceed</returns>
        private bool ShowBuildConfirmationDialog(BuildEnvironmentConfig config)
        {
            var defines = config.GetCurrentDefines();
            string definesText = defines.Count > 0 ? string.Join(", ", defines) : "None";
            
            string message = $"Build Environment: {config.SelectedEnvironment}\n\n" +
                           $"Scripting Defines:\n{definesText}\n\n" +
                           "Do you want to proceed with the build?";
            
            return EditorUtility.DisplayDialog(
                "Build Environment Configuration",
                message,
                "Build",
                "Cancel"
            );
        }
        
        /// <summary>
        /// Apply scripting defines to the target platform
        /// </summary>
        /// <param name="config">Build environment config</param>
        /// <param name="buildTarget">Target build platform</param>
        private void ApplyScriptingDefines(BuildEnvironmentConfig config, BuildTarget buildTarget)
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));
            if (namedBuildTarget == null)
            {
                Debug.LogWarning($"[BuildEnvironmentProcessor] Unknown build target group for {buildTarget}");
                return;
            }
            
            // Get current defines
            var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var currentDefinesList = currentDefines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            
            // Get new defines from config
            var newDefines = config.GetCurrentDefines();
            
            // Remove old environment defines
            RemoveOldEnvironmentDefines(currentDefinesList, config);
            
            // Add new environment defines
            foreach (var define in newDefines)
            {
                if (!string.IsNullOrWhiteSpace(define) && !currentDefinesList.Contains(define))
                {
                    currentDefinesList.Add(define);
                }
            }
            
            // Apply the updated defines
            string newDefinesString = string.Join(";", currentDefinesList);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefinesString);
            
            Debug.Log($"[BuildEnvironmentProcessor] Applied scripting defines for {config.SelectedEnvironment}: {string.Join(", ", newDefines)}");
        }
        
        /// <summary>
        /// Remove defines from other environments to avoid conflicts
        /// </summary>
        /// <param name="currentDefines">Current defines list</param>
        /// <param name="config">Build environment config</param>
        private void RemoveOldEnvironmentDefines(System.Collections.Generic.List<string> currentDefines, BuildEnvironmentConfig config)
        {
            // Get all defines from other environments
            var allEnvironments = System.Enum.GetValues(typeof(BuildEnvironment)).Cast<BuildEnvironment>();
            
            foreach (var env in allEnvironments)
            {
                if (env == config.SelectedEnvironment) continue;
                
                var envDefines = config.GetDefinesForEnvironment(env);
                foreach (var define in envDefines)
                {
                    currentDefines.Remove(define);
                }
            }
        }
    }
}
#endif