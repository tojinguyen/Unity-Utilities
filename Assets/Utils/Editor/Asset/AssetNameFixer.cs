#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TirexGame.Utils.Editor.Asset
{
    /// <summary>
    /// Tool to fix asset file name mismatches with main object names
    /// Resolves the error: "The main object name 'A' should match the asset file name 'B'"
    /// </summary>
    public class AssetNameFixer : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<AssetMismatch> mismatches = new List<AssetMismatch>();
        private bool isScanning = false;
        private string searchFilter = "t:ScriptableObject";
        private bool includeAllAssets = false;
        
        private class AssetMismatch
        {
            public string assetPath;
            public string fileName;
            public string objectName;
            public Object assetObject;
            public bool selected = true;
        }

        [MenuItem("Tools/Asset/Fix Asset Name Mismatches")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetNameFixer>();
            window.titleContent = new GUIContent("Asset Name Fixer");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Asset Name Mismatch Fixer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "This tool finds and fixes assets where the main object name doesn't match the file name.\n" +
                "Example: File 'MyAsset.asset' contains object named 'OldName' → will rename to 'MyAsset'",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            // Search options
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Search Options", EditorStyles.boldLabel);
            
            includeAllAssets = EditorGUILayout.Toggle("Include All Asset Types", includeAllAssets);
            
            if (!includeAllAssets)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search Filter:", GUILayout.Width(100));
                searchFilter = EditorGUILayout.TextField(searchFilter);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Examples: t:ScriptableObject, t:Material, t:AnimationClip", MessageType.None);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Scan button
            EditorGUI.BeginDisabledGroup(isScanning);
            if (GUILayout.Button("Scan for Mismatches", GUILayout.Height(30)))
            {
                ScanForMismatches();
            }
            EditorGUI.EndDisabledGroup();

            if (isScanning)
            {
                EditorGUILayout.LabelField("Scanning...", EditorStyles.boldLabel);
                return;
            }

            if (mismatches.Count == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No mismatches found. Click 'Scan for Mismatches' to search.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Found {mismatches.Count} mismatch(es)", EditorStyles.boldLabel);
            
            // Select/Deselect all
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                foreach (var mismatch in mismatches)
                    mismatch.selected = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                foreach (var mismatch in mismatches)
                    mismatch.selected = false;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            // List of mismatches
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var mismatch in mismatches)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                
                mismatch.selected = EditorGUILayout.Toggle(mismatch.selected, GUILayout.Width(20));
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"File: {mismatch.fileName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Object Name: {mismatch.objectName}", new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } });
                EditorGUILayout.LabelField($"Path: {mismatch.assetPath}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = mismatch.assetObject;
                    EditorGUIUtility.PingObject(mismatch.assetObject);
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Fix button
            int selectedCount = mismatches.Count(m => m.selected);
            EditorGUI.BeginDisabledGroup(selectedCount == 0);
            
            if (GUILayout.Button($"Fix Selected ({selectedCount})", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Fix",
                    $"Are you sure you want to rename {selectedCount} asset(s)?\nThis will change the main object name to match the file name.",
                    "Yes, Fix Them",
                    "Cancel"))
                {
                    FixSelectedMismatches();
                }
            }
            
            EditorGUI.EndDisabledGroup();
        }

        private void ScanForMismatches()
        {
            isScanning = true;
            mismatches.Clear();
            
            try
            {
                string[] guids;
                
                if (includeAllAssets)
                {
                    // Search all assets
                    guids = AssetDatabase.FindAssets("");
                }
                else
                {
                    // Search with filter
                    guids = AssetDatabase.FindAssets(searchFilter);
                }

                int totalAssets = guids.Length;
                int processed = 0;

                foreach (string guid in guids)
                {
                    processed++;
                    if (processed % 100 == 0)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                            "Scanning Assets",
                            $"Processing {processed}/{totalAssets} assets...",
                            (float)processed / totalAssets))
                        {
                            break;
                        }
                    }

                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    
                    // Skip folders
                    if (AssetDatabase.IsValidFolder(assetPath))
                        continue;
                    
                    // Skip non-asset files
                    if (!assetPath.EndsWith(".asset") && 
                        !assetPath.EndsWith(".mat") && 
                        !assetPath.EndsWith(".anim") &&
                        !assetPath.EndsWith(".controller") &&
                        !assetPath.EndsWith(".prefab"))
                    {
                        if (!includeAllAssets)
                            continue;
                    }

                    // Load the asset
                    Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (asset == null)
                        continue;

                    // Get file name without extension
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    string objectName = asset.name;

                    // Check if names match
                    if (fileName != objectName)
                    {
                        mismatches.Add(new AssetMismatch
                        {
                            assetPath = assetPath,
                            fileName = fileName,
                            objectName = objectName,
                            assetObject = asset
                        });
                    }
                }

                EditorUtility.ClearProgressBar();
                
                Debug.Log($"Scan complete. Found {mismatches.Count} asset name mismatch(es).");
            }
            finally
            {
                isScanning = false;
                EditorUtility.ClearProgressBar();
            }
        }

        private void FixSelectedMismatches()
        {
            var selectedMismatches = mismatches.Where(m => m.selected).ToList();
            int fixedCount = 0;
            int failedCount = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < selectedMismatches.Count; i++)
                {
                    var mismatch = selectedMismatches[i];
                    
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Fixing Assets",
                        $"Processing {mismatch.fileName}... ({i + 1}/{selectedMismatches.Count})",
                        (float)i / selectedMismatches.Count))
                    {
                        break;
                    }

                    try
                    {
                        // Rename the asset object
                        mismatch.assetObject.name = mismatch.fileName;
                        
                        // Mark as dirty and save
                        EditorUtility.SetDirty(mismatch.assetObject);
                        
                        fixedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to fix {mismatch.assetPath}: {e.Message}");
                        failedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }

            // Remove fixed items from list
            mismatches.RemoveAll(m => m.selected && m.assetObject.name == m.fileName);

            // Show result
            string message = $"Fixed {fixedCount} asset(s).";
            if (failedCount > 0)
                message += $"\nFailed to fix {failedCount} asset(s). Check console for details.";

            EditorUtility.DisplayDialog("Fix Complete", message, "OK");
            
            Debug.Log($"Asset name fix complete: {fixedCount} fixed, {failedCount} failed.");
        }
    }
}
#endif
