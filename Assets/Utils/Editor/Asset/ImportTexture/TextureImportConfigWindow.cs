using UnityEngine;
using UnityEditor;
using System.IO;
/// <summary>
/// Editor Window for configuring texture import settings
/// </summary>
public class TextureImportConfigWindow : EditorWindow
{
    private bool autoConfigEnabled = true;
    private string folderFilter = "";
    private int maxTextureSize = 2048;
    
    private readonly int[] textureSizeOptions = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

    [MenuItem("TirexGame/Editor/Asset/Texture Import Config")]
    public static void ShowWindow()
    {
        TextureImportConfigWindow window = GetWindow<TextureImportConfigWindow>("Texture Import Config");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnEnable()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        autoConfigEnabled = EditorPrefs.GetBool("TextureAutoConfig_Enabled", true);
        folderFilter = EditorPrefs.GetString("TextureAutoConfig_FolderFilter", "");
        maxTextureSize = EditorPrefs.GetInt("TextureAutoConfig_MaxSize", 2048);
    }

    private void SaveSettings()
    {
        EditorPrefs.SetBool("TextureAutoConfig_Enabled", autoConfigEnabled);
        EditorPrefs.SetString("TextureAutoConfig_FolderFilter", folderFilter);
        EditorPrefs.SetInt("TextureAutoConfig_MaxSize", maxTextureSize);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        
        EditorGUILayout.LabelField("Auto Texture Import Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Automatically configure imported textures as Sprite (2D/UI) with Single mode.", MessageType.Info);
        
        GUILayout.Space(10);
        
        // Enable/Disable toggle
        autoConfigEnabled = EditorGUILayout.Toggle("Enable Auto Config", autoConfigEnabled);
        
        GUILayout.Space(10);
        
        // Folder filter
        EditorGUILayout.LabelField("Folder Filter (Optional)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Leave empty to apply to all textures, or specify folder paths separated by semicolons (;)\nExample: UI;Sprites;Textures/Icons", MessageType.None);
        folderFilter = EditorGUILayout.TextField("Filter:", folderFilter);
        
        GUILayout.Space(10);
        
        // Max texture size
        EditorGUILayout.LabelField("Max Texture Size", EditorStyles.boldLabel);
        int currentIndex = System.Array.IndexOf(textureSizeOptions, maxTextureSize);
        if (currentIndex == -1) currentIndex = 6; // Default to 2048
        
        currentIndex = EditorGUILayout.Popup("Max Size:", currentIndex, System.Array.ConvertAll(textureSizeOptions, x => x.ToString()));
        maxTextureSize = textureSizeOptions[currentIndex];
        
        GUILayout.Space(20);
        
        // Save button
        if (GUILayout.Button("Save Settings", GUILayout.Height(30)))
        {
            SaveSettings();
            EditorUtility.DisplayDialog("Success", "Settings saved successfully!", "OK");
        }
        
        GUILayout.Space(10);
        
        // Batch reimport button
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Reimport All Textures in Project", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm", "This will reimport all textures in the project. This may take some time. Continue?", "Yes", "Cancel"))
            {
                ReimportAllTextures();
            }
        }
        
        if (GUILayout.Button("Reimport Selected Textures", GUILayout.Height(30)))
        {
            ReimportSelectedTextures();
        }
        
        GUILayout.Space(10);
        
        // Current settings display
        EditorGUILayout.HelpBox($"Status: {(autoConfigEnabled ? "ENABLED" : "DISABLED")}\nMax Size: {maxTextureSize}\nFolder Filter: {(string.IsNullOrEmpty(folderFilter) ? "All folders" : folderFilter)}", MessageType.None);
    }

    private void ReimportAllTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            count++;
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Complete", $"Reimported {count} textures.", "OK");
        Debug.Log($"[TextureImportConfig] Reimported {count} textures");
    }

    private void ReimportSelectedTextures()
    {
        Object[] selectedObjects = Selection.objects;
        int count = 0;
        
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (obj is Texture2D)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
        }
        
        if (count > 0)
        {
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", $"Reimported {count} selected textures.", "OK");
            Debug.Log($"[TextureImportConfig] Reimported {count} selected textures");
        }
        else
        {
            EditorUtility.DisplayDialog("No Textures", "No textures selected.", "OK");
        }
    }
}