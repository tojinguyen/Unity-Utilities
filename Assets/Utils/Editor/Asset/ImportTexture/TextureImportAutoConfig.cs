#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Auto-configure texture import settings to Sprite (UI) with Single mode
/// </summary>
public class TextureImportAutoConfig : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Get settings from EditorPrefs
        bool autoConfigEnabled = EditorPrefs.GetBool("TextureAutoConfig_Enabled", true);
        
        if (!autoConfigEnabled)
            return;

        // Get folder filters
        string folderFilter = EditorPrefs.GetString("TextureAutoConfig_FolderFilter", "");
        
        // Check if path matches filter (if set)
        if (!string.IsNullOrEmpty(folderFilter))
        {
            string[] folders = folderFilter.Split(';');
            bool matchesFilter = false;
            
            foreach (string folder in folders)
            {
                if (!string.IsNullOrEmpty(folder.Trim()) && assetPath.Contains(folder.Trim()))
                {
                    matchesFilter = true;
                    break;
                }
            }
            
            if (!matchesFilter)
                return;
        }

        TextureImporter importer = assetImporter as TextureImporter;
        
        // Get texture type from settings
        int textureTypeIndex = EditorPrefs.GetInt("TextureAutoConfig_TextureType", 8); // Default: Sprite (2D and UI)
        importer.textureType = (TextureImporterType)textureTypeIndex;
        
        // Get sprite mode from settings (only applies if texture type is Sprite)
        if (importer.textureType == TextureImporterType.Sprite)
        {
            int spriteModeIndex = EditorPrefs.GetInt("TextureAutoConfig_SpriteMode", 1); // Default: Single
            importer.spriteImportMode = (SpriteImportMode)spriteModeIndex;
        }
        
        // Additional settings
        bool enableMipmap = EditorPrefs.GetBool("TextureAutoConfig_Mipmap", false);
        importer.mipmapEnabled = enableMipmap;
        
        int filterModeIndex = EditorPrefs.GetInt("TextureAutoConfig_FilterMode", 1); // Default: Bilinear
        importer.filterMode = (FilterMode)filterModeIndex;
        
        int compressionIndex = EditorPrefs.GetInt("TextureAutoConfig_Compression", 1); // Default: Compressed
        importer.textureCompression = (TextureImporterCompression)compressionIndex;
        
        // Apply max size from settings
        int maxSize = EditorPrefs.GetInt("TextureAutoConfig_MaxSize", 2048);
        importer.maxTextureSize = maxSize;
        
        Debug.Log($"[TextureAutoConfig] Configured: {assetPath} as {importer.textureType}" + 
                  (importer.textureType == TextureImporterType.Sprite ? $" ({importer.spriteImportMode})" : ""));
    }
}
#endif