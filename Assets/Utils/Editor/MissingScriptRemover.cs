#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MissingScriptRemover : EditorWindow
{
    private static List<GameObject> prefabsWithMissingScripts = new List<GameObject>();
    private static List<string> prefabPaths = new List<string>();
    private Vector2 scrollPosition;
    private bool showOnlyWithMissing = true;
    private int totalPrefabsScanned = 0;
    private int prefabsWithMissing = 0;

    [MenuItem("TirexGame/Editor/Missing Script Remover")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptRemover>("Missing Script Remover");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Missing Script Remover", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan Project for Missing Scripts", GUILayout.Height(30)))
        {
            ScanProjectForMissingScripts();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (prefabsWithMissingScripts.Count > 0)
        {
            EditorGUILayout.LabelField($"Found {prefabsWithMissing} prefabs with missing scripts out of {totalPrefabsScanned} total prefabs", EditorStyles.helpBox);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Missing Scripts from All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Confirm Removal", 
                    $"Are you sure you want to remove missing scripts from {prefabsWithMissingScripts.Count} prefabs? This action cannot be undone.", 
                    "Yes", "Cancel"))
                {
                    RemoveMissingScriptsFromAll();
                }
            }
            
            if (GUILayout.Button("Clear List", GUILayout.Height(25)))
            {
                ClearList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            showOnlyWithMissing = EditorGUILayout.Toggle("Show only prefabs with missing scripts", showOnlyWithMissing);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < prefabsWithMissingScripts.Count; i++)
            {
                if (prefabsWithMissingScripts[i] == null) continue;

                bool hasMissingScript = HasMissingScript(prefabsWithMissingScripts[i]);
                
                if (showOnlyWithMissing && !hasMissingScript) continue;

                EditorGUILayout.BeginHorizontal();

                // Color code based on missing script status
                GUI.color = hasMissingScript ? Color.red : Color.green;
                EditorGUILayout.ObjectField(prefabsWithMissingScripts[i], typeof(GameObject), false);
                GUI.color = Color.white;

                if (hasMissingScript)
                {
                    if (GUILayout.Button("Remove Missing Scripts", GUILayout.Width(150)))
                    {
                        RemoveMissingScripts(prefabsWithMissingScripts[i], prefabPaths[i]);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Clean", GUILayout.Width(150));
                }

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = prefabsWithMissingScripts[i];
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        else if (totalPrefabsScanned > 0)
        {
            EditorGUILayout.HelpBox($"Great! No missing scripts found in {totalPrefabsScanned} prefabs.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Click 'Scan Project for Missing Scripts' to start scanning your project.", MessageType.Info);
        }
    }

    private void ScanProjectForMissingScripts()
    {
        prefabsWithMissingScripts.Clear();
        prefabPaths.Clear();
        totalPrefabsScanned = 0;
        prefabsWithMissing = 0;

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        totalPrefabsScanned = prefabGUIDs.Length;

        EditorUtility.DisplayProgressBar("Scanning Prefabs", "Scanning for missing scripts...", 0);

        try
        {
            for (int i = 0; i < prefabGUIDs.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    prefabsWithMissingScripts.Add(prefab);
                    prefabPaths.Add(prefabPath);

                    if (HasMissingScript(prefab))
                    {
                        prefabsWithMissing++;
                    }
                }

                float progress = (float)i / prefabGUIDs.Length;
                EditorUtility.DisplayProgressBar("Scanning Prefabs", $"Scanning {prefab?.name ?? "Unknown"} ({i + 1}/{prefabGUIDs.Length})", progress);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"Scan complete! Found {prefabsWithMissing} prefabs with missing scripts out of {totalPrefabsScanned} total prefabs.");
    }

    private static bool HasMissingScript(GameObject prefab)
    {
        if (prefab == null) return false;

        Component[] components = prefab.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null)
            {
                return true;
            }
        }
        return false;
    }

    private static void RemoveMissingScripts(GameObject prefab, string prefabPath)
    {
        if (prefab == null) return;

        bool wasModified = false;
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        try
        {
            Transform[] allTransforms = prefabInstance.GetComponentsInChildren<Transform>(true);
            
            foreach (Transform child in allTransforms)
            {
                Component[] components = child.GetComponents<Component>();
                for (int i = components.Length - 1; i >= 0; i--)
                {
                    if (components[i] == null)
                    {
                        SerializedObject serializedObject = new SerializedObject(child.gameObject);
                        SerializedProperty prop = serializedObject.FindProperty("m_Component");
                        
                        for (int j = prop.arraySize - 1; j >= 0; j--)
                        {
                            SerializedProperty componentProperty = prop.GetArrayElementAtIndex(j);
                            if (componentProperty.objectReferenceValue == null)
                            {
                                prop.DeleteArrayElementAtIndex(j);
                                wasModified = true;
                            }
                        }
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (wasModified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                Debug.Log($"Removed missing scripts from: {prefab.name}");
            }
        }
        finally
        {
            DestroyImmediate(prefabInstance);
        }
    }

    private void RemoveMissingScriptsFromAll()
    {
        int processedCount = 0;
        int cleanedCount = 0;

        EditorUtility.DisplayProgressBar("Removing Missing Scripts", "Processing prefabs...", 0);

        try
        {
            for (int i = 0; i < prefabsWithMissingScripts.Count; i++)
            {
                if (prefabsWithMissingScripts[i] != null && HasMissingScript(prefabsWithMissingScripts[i]))
                {
                    RemoveMissingScripts(prefabsWithMissingScripts[i], prefabPaths[i]);
                    cleanedCount++;
                }
                processedCount++;

                float progress = (float)i / prefabsWithMissingScripts.Count;
                EditorUtility.DisplayProgressBar("Removing Missing Scripts", 
                    $"Processing {prefabsWithMissingScripts[i]?.name ?? "Unknown"} ({i + 1}/{prefabsWithMissingScripts.Count})", 
                    progress);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Batch removal complete! Cleaned {cleanedCount} prefabs out of {processedCount} processed.");
        
        // Refresh the scan to update the UI
        ScanProjectForMissingScripts();
    }

    private void ClearList()
    {
        prefabsWithMissingScripts.Clear();
        prefabPaths.Clear();
        totalPrefabsScanned = 0;
        prefabsWithMissing = 0;
    }
}
#endif