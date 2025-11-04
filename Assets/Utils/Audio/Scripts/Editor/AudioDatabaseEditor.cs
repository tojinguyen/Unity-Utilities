using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AudioDatabase))]
public class AudioDatabaseEditor : Editor
{
    private SerializedProperty audioClipsProperty;
    private SerializedProperty categorySettingsProperty;
    private bool showAudioClips = true;
    private bool showCategorySettings = true;

    private void OnEnable()
    {
        audioClipsProperty = serializedObject.FindProperty("audioClips");
        categorySettingsProperty = serializedObject.FindProperty("categorySettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        
        // Audio Clips Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showAudioClips = EditorGUILayout.Foldout(showAudioClips, $"Audio Clips ({audioClipsProperty.arraySize})", true, EditorStyles.foldoutHeader);
        
        if (showAudioClips)
        {
            EditorGUI.indentLevel++;
            
            // Add/Remove buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Audio Clip", GUILayout.Height(25)))
            {
                audioClipsProperty.arraySize++;
                var newElement = audioClipsProperty.GetArrayElementAtIndex(audioClipsProperty.arraySize - 1);
                ResetAudioClipData(newElement);
            }
            
            GUI.enabled = audioClipsProperty.arraySize > 0;
            if (GUILayout.Button("Remove Last", GUILayout.Height(25)))
            {
                audioClipsProperty.arraySize--;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Draw audio clips list
            for (int i = 0; i < audioClipsProperty.arraySize; i++)
            {
                var element = audioClipsProperty.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Header with index and delete button
                EditorGUILayout.BeginHorizontal();
                var idProperty = element.FindPropertyRelative("id");
                string displayName = string.IsNullOrEmpty(idProperty.stringValue) 
                    ? $"Audio Clip {i}" 
                    : $"{i}: {idProperty.stringValue}";
                
                EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
                
                if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(16)))
                {
                    audioClipsProperty.DeleteArrayElementAtIndex(i);
                    break; // Exit loop since array changed
                }
                EditorGUILayout.EndHorizontal();

                // Draw the AudioClipData using PropertyDrawer
                EditorGUILayout.PropertyField(element, GUIContent.none);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Category Settings Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        showCategorySettings = EditorGUILayout.Foldout(showCategorySettings, $"Category Settings ({categorySettingsProperty.arraySize})", true, EditorStyles.foldoutHeader);
        
        if (showCategorySettings)
        {
            EditorGUI.indentLevel++;
            
            // Add/Remove buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Category Setting", GUILayout.Height(25)))
            {
                categorySettingsProperty.arraySize++;
                var newElement = categorySettingsProperty.GetArrayElementAtIndex(categorySettingsProperty.arraySize - 1);
                ResetCategorySettings(newElement);
            }
            
            GUI.enabled = categorySettingsProperty.arraySize > 0;
            if (GUILayout.Button("Remove Last", GUILayout.Height(25)))
            {
                categorySettingsProperty.arraySize--;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Draw category settings list
            for (int i = 0; i < categorySettingsProperty.arraySize; i++)
            {
                var element = categorySettingsProperty.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Header with delete button
                EditorGUILayout.BeginHorizontal();
                var audioTypeProperty = element.FindPropertyRelative("audioType");
                EditorGUILayout.LabelField($"Category {i}: {audioTypeProperty.enumDisplayNames[audioTypeProperty.enumValueIndex]}", EditorStyles.boldLabel);
                
                if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(16)))
                {
                    categorySettingsProperty.DeleteArrayElementAtIndex(i);
                    break; // Exit loop since array changed
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(element, GUIContent.none);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Utility buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rebuild Lookup Tables", GUILayout.Height(30)))
        {
            AudioDatabase database = (AudioDatabase)target;
            database.Initialize();
            EditorUtility.SetDirty(database);
        }
        
        if (GUILayout.Button("Validate Data", GUILayout.Height(30)))
        {
            ValidateDatabase();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void ResetAudioClipData(SerializedProperty element)
    {
        var idProperty = element.FindPropertyRelative("id");
        var audioTypeProperty = element.FindPropertyRelative("audioType");
        var playModeProperty = element.FindPropertyRelative("playMode");
        var volumeProperty = element.FindPropertyRelative("volume");
        var pitchProperty = element.FindPropertyRelative("pitch");
        var spatialBlendProperty = element.FindPropertyRelative("spatialBlend");
        var useFadeProperty = element.FindPropertyRelative("useFade");
        var fadeInDurationProperty = element.FindPropertyRelative("fadeInDuration");
        var fadeOutDurationProperty = element.FindPropertyRelative("fadeOutDuration");
        var minDistanceProperty = element.FindPropertyRelative("minDistance");
        var maxDistanceProperty = element.FindPropertyRelative("maxDistance");

        if (idProperty != null) idProperty.stringValue = "";
        if (audioTypeProperty != null) audioTypeProperty.enumValueIndex = 0;
        if (playModeProperty != null) playModeProperty.enumValueIndex = 0;
        if (volumeProperty != null) volumeProperty.floatValue = 1f;
        if (pitchProperty != null) pitchProperty.floatValue = 1f;
        if (spatialBlendProperty != null) spatialBlendProperty.floatValue = 0f;
        if (useFadeProperty != null) useFadeProperty.boolValue = false;
        if (fadeInDurationProperty != null) fadeInDurationProperty.floatValue = 0.5f;
        if (fadeOutDurationProperty != null) fadeOutDurationProperty.floatValue = 0.5f;
        if (minDistanceProperty != null) minDistanceProperty.floatValue = 1f;
        if (maxDistanceProperty != null) maxDistanceProperty.floatValue = 50f;
    }

    private void ResetCategorySettings(SerializedProperty element)
    {
        var audioTypeProperty = element.FindPropertyRelative("audioType");
        var masterVolumeProperty = element.FindPropertyRelative("masterVolume");
        var mutedProperty = element.FindPropertyRelative("muted");
        var maxConcurrentSoundsProperty = element.FindPropertyRelative("maxConcurrentSounds");
        var allowDuplicatesProperty = element.FindPropertyRelative("allowDuplicates");

        if (audioTypeProperty != null) audioTypeProperty.enumValueIndex = 0;
        if (masterVolumeProperty != null) masterVolumeProperty.floatValue = 1f;
        if (mutedProperty != null) mutedProperty.boolValue = false;
        if (maxConcurrentSoundsProperty != null) maxConcurrentSoundsProperty.intValue = 10;
        if (allowDuplicatesProperty != null) allowDuplicatesProperty.boolValue = true;
    }

    private void ValidateDatabase()
    {
        AudioDatabase database = (AudioDatabase)target;
        HashSet<string> usedIds = new HashSet<string>();
        List<string> issues = new List<string>();

        // Check for duplicate IDs and empty AudioClips
        for (int i = 0; i < audioClipsProperty.arraySize; i++)
        {
            var element = audioClipsProperty.GetArrayElementAtIndex(i);
            var idProperty = element.FindPropertyRelative("id");
            var audioClipProperty = element.FindPropertyRelative("audioClip");
            var audioClipReferenceProperty = element.FindPropertyRelative("audioClipReference");

            string id = idProperty.stringValue;
            
            // Check for empty ID
            if (string.IsNullOrEmpty(id))
            {
                issues.Add($"Audio Clip at index {i} has empty ID");
                continue;
            }

            // Check for duplicate ID
            if (usedIds.Contains(id))
            {
                issues.Add($"Duplicate ID found: '{id}' at index {i}");
            }
            else
            {
                usedIds.Add(id);
            }

            // Check if both AudioClip and AudioClipReference are null
            if (audioClipProperty.objectReferenceValue == null && 
                audioClipReferenceProperty.FindPropertyRelative("m_AssetGUID").stringValue == "")
            {
                issues.Add($"Audio Clip '{id}' at index {i} has no AudioClip or AudioClipReference assigned");
            }
        }

        // Display results
        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog("Validation Complete", "No issues found in the AudioDatabase!", "OK");
        }
        else
        {
            string message = "Found the following issues:\n\n" + string.Join("\n", issues);
            EditorUtility.DisplayDialog("Validation Issues", message, "OK");
        }
    }
}