#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Imports PNG/GIF placés sous Sprites/Tiles/NewTile comme sprites pixel-art (filtre Point, PPU aligné jeu).
/// </summary>
public sealed class NewTileTextureImportSettings : AssetPostprocessor
{
    const string FolderMarker = "/Sprites/Tiles/NewTile";

    void OnPreprocessTexture()
    {
        if (assetPath.IndexOf(FolderMarker) < 0)
            return;

        var ti                   = (TextureImporter)assetImporter;
        ti.textureType           = TextureImporterType.Sprite;
        ti.spriteImportMode      = SpriteImportMode.Single;
        ti.spritePixelsPerUnit   = 64f;
        ti.filterMode            = FilterMode.Point;
        ti.mipmapEnabled         = false;
        ti.alphaIsTransparency   = true;
        ti.textureCompression    = TextureImporterCompression.Uncompressed;

        TextureImporterPlatformSettings defs = ti.GetDefaultPlatformTextureSettings();
        defs.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SetPlatformTextureSettings(defs);
    }
}
#endif
