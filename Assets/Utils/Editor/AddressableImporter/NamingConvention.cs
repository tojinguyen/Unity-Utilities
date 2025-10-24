#if UNITY_EDITOR
using System;

namespace TirexGame.Utils.Editor.AddressableImporter
{
    [Serializable]
    public enum NamingConvention
    {
        /// <summary>
        /// Use the filename as the addressable key (e.g., "texture.png" -> "texture")
        /// </summary>
        FileName,
        
        /// <summary>
        /// Use the full path relative to Assets folder (e.g., "Textures/UI/texture.png" -> "Textures/UI/texture")
        /// </summary>
        FullPath,
        
        /// <summary>
        /// Use the path relative to the specified folder (e.g., "UI/texture.png" -> "UI/texture")
        /// </summary>
        RelativeToFolder,
        
        /// <summary>
        /// Use a custom prefix + filename (e.g., "ui_texture")
        /// </summary>
        PrefixedFileName
    }
}
#endif