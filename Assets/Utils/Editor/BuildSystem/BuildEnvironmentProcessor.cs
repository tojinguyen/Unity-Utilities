#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;

namespace TirexGame.Utils.Editor.BuildSystem
{
    /// <summary>
    /// Build processor for build environment management
    /// </summary>
    public class BuildEnvironmentProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            var config = FindBuildEnvironmentConfig();
            if (config != null && config.AutoApplyOnBuild)
            {
                if (config.ShowBuildDialog)
                {
                    var proceed = EditorUtility.DisplayDialog(
                        "Build Environment", 
                        $"About to build with {config.SelectedEnvironment} environment.\n" +
                        $"Defines: {string.Join(", ", config.GetCurrentDefines())}\n\n" +
                        "Continue with build?",
                        "Yes", "Cancel"
                    );
                    
                    if (!proceed)
                    {
                        throw new BuildFailedException("Build cancelled by user.");
                    }
                }
                
                ApplyEnvironmentDefines(config, report);
            }
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            // Optional: Restore previous defines or perform cleanup
            Debug.Log($"Build completed for {report.summary.platform} with result: {report.summary.result}");
        }
        
        private BuildEnvironmentConfig FindBuildEnvironmentConfig()
        {
            var guids = AssetDatabase.FindAssets("t:BuildEnvironmentConfig");
            
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BuildEnvironmentConfig>(path);
            }
            
            return null;
        }
        
        private void ApplyEnvironmentDefines(BuildEnvironmentConfig config, BuildReport report)
        {
            var currentDefines = config.GetCurrentDefines();
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(report.summary.platform);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            
            // Get existing defines to preserve non-environment specific ones
            var existingDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var existingDefinesList = existingDefines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            
            // Remove old environment defines
            var allEnvironmentDefines = config.DevelopmentDefines
                .Concat(config.StagingDefines)
                .Concat(config.ProductionDefines)
                .Distinct()
                .ToList();
            
            var filteredDefines = existingDefinesList.Where(d => !allEnvironmentDefines.Contains(d)).ToList();
            
            // Add current environment defines
            var newDefines = filteredDefines.Concat(currentDefines).Distinct().ToList();
            var newDefinesString = string.Join(";", newDefines);
            
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefinesString);
            
            Debug.Log($"[BuildEnvironmentProcessor] Applied {config.SelectedEnvironment} environment defines for build: {string.Join(", ", currentDefines)}");
        }
    }
}
#endif