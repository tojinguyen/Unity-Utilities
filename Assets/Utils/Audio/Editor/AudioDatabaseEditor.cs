using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

/// <summary>
/// Custom editor for AudioDatabase that automatically assigns IDs based on audio file names.
/// When you drag audio files into the audioClips list, it will automatically:
/// - Set the ID to the filename without extension
/// - Convert to PascalCase (e.g., "jump_sound" becomes "JumpSound")
/// - Assign the audio clip reference
/// </summary>
[CustomEditor(typeof(AudioDatabase))]
public class AudioDatabaseEditor : Editor
{
    private SerializedProperty audioClipsProperty;
    private SerializedProperty categorySettingsProperty;

    private void OnEnable()
    {
        audioClipsProperty = serializedObject.FindProperty("audioClips");
        categorySettingsProperty = serializedObject.FindProperty("categorySettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio Database", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag audio files into the list below. IDs will be automatically assigned based on filenames.", MessageType.Info);
        
        EditorGUILayout.Space();

        // Audio Clips section
        EditorGUILayout.LabelField("Audio Clips", EditorStyles.boldLabel);
        
        // Auto-assign button
        if (GUILayout.Button("Auto-Assign IDs from Filenames"))
        {
            AutoAssignIDs();
        }
        
        // Clear all button
        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Clear All Audio Clips", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear All Audio Clips", 
                "Are you sure you want to clear all audio clips? This action cannot be undone.", 
                "Yes, Clear All", "Cancel"))
            {
                audioClipsProperty.ClearArray();
            }
        }
        
        EditorGUILayout.Space();

        // Draw the audio clips list
        EditorGUILayout.PropertyField(audioClipsProperty, new GUIContent("Audio Clips"), true);

        EditorGUILayout.Space();

        // Category Settings section
        EditorGUILayout.LabelField("Category Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(categorySettingsProperty, new GUIContent("Category Settings"), true);

        // Check for changes and auto-assign IDs
        if (EditorGUI.EndChangeCheck())
        {
            AutoAssignIDsForNewEntries();
            serializedObject.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Automatically assigns IDs to all audio clips based on their filenames
    /// </summary>
    private void AutoAssignIDs()
    {
        AudioDatabase database = target as AudioDatabase;
        
        for (int i = 0; i < audioClipsProperty.arraySize; i++)
        {
            var audioClipData = audioClipsProperty.GetArrayElementAtIndex(i);
            var audioClipProperty = audioClipData.FindPropertyRelative("audioClip");
            var idProperty = audioClipData.FindPropertyRelative("id");

            if (audioClipProperty.objectReferenceValue is AudioClip audioClip)
            {
                string fileName = audioClip.name;
                string baseId = GenerateIdFromFileName(fileName);
                string uniqueId = database.GenerateUniqueId(baseId);
                idProperty.stringValue = uniqueId;
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// Auto-assigns IDs only for new entries that have audio clips but no ID
    /// </summary>
    private void AutoAssignIDsForNewEntries()
    {
        AudioDatabase database = target as AudioDatabase;
        
        for (int i = 0; i < audioClipsProperty.arraySize; i++)
        {
            var audioClipData = audioClipsProperty.GetArrayElementAtIndex(i);
            var audioClipProperty = audioClipData.FindPropertyRelative("audioClip");
            var idProperty = audioClipData.FindPropertyRelative("id");

            // Only assign ID if it's empty and we have an audio clip
            if (string.IsNullOrEmpty(idProperty.stringValue) && 
                audioClipProperty.objectReferenceValue is AudioClip audioClip)
            {
                string fileName = audioClip.name;
                string baseId = GenerateIdFromFileName(fileName);
                string uniqueId = database.GenerateUniqueId(baseId);
                idProperty.stringValue = uniqueId;
            }
        }
    }

    /// <summary>
    /// Generates a proper ID from a filename by converting to PascalCase
    /// Examples:
    /// - "jump_sound" -> "JumpSound"
    /// - "background-music" -> "BackgroundMusic"
    /// - "ui click" -> "UiClick"
    /// - "SFX_Explosion_01" -> "SfxExplosion01"
    /// </summary>
    private string GenerateIdFromFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "";

        // Remove common audio file extensions if present
        string[] audioExtensions = { ".wav", ".mp3", ".ogg", ".aiff", ".flac" };
        foreach (string ext in audioExtensions)
        {
            if (fileName.EndsWith(ext, System.StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - ext.Length);
                break;
            }
        }

        // Split by common separators and convert to PascalCase
        string[] separators = { "_", "-", " ", "." };
        string[] words = fileName.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
        
        string result = "";
        foreach (string word in words)
        {
            if (!string.IsNullOrEmpty(word))
            {
                // Capitalize first letter, lowercase the rest
                result += char.ToUpper(word[0]) + word.Substring(1).ToLower();
            }
        }

        // Ensure it starts with a letter (add prefix if starts with number)
        if (!string.IsNullOrEmpty(result) && char.IsDigit(result[0]))
        {
            result = "Audio" + result;
        }

        return result;
    }
}