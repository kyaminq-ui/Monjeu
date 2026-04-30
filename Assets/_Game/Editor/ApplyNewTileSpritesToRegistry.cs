#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Remplit TileSpriteRegistry à partir du dossier NewTile selon les conventions de nomage des PNG.
/// </summary>
public static class ApplyNewTileSpritesToRegistry
{
    const string NewTileFolder         = "Assets/_Game/Sprites/Tiles/NewTile";
    const string RegistryAssetPath      = "Assets/_Game/ScriptableObjects/TileSpriteRegistry.asset";

    static readonly Regex RxGroundDigits = new Regex(@"^GROUND(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RxBloodRust = new Regex(@"^GROUND_BLOOD_RUST_VARIANT(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RxCursed     = new Regex(@"^GROUND_CURSED_GLOW_VARIANT(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RxDecoration = new Regex(@"^GROUND_DECORATION(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RxEdge       = new Regex(@"^GROUND_EDGE(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    static readonly Regex RxObstacle   = new Regex(@"^GROUND_OBSTACLE(\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [MenuItem("Arena/Appliquer les tuiles NewTile au registre")]
    static void PopulateRegistry()
    {
        if (!AssetDatabase.IsValidFolder(NewTileFolder))
        {
            Debug.LogError($"[ApplyNewTileSpritesToRegistry] Dossier absent : {NewTileFolder}");
            return;
        }

        var registry = AssetDatabase.LoadAssetAtPath<TileSpriteRegistry>(RegistryAssetPath);
        if (registry == null)
        {
            Debug.LogError($"[ApplyNewTileSpritesToRegistry] TileSpriteRegistry introuvable à {RegistryAssetPath}");
            return;
        }

        registry.groundTiles    = OrderedSprites(NewTileFolder, RxGroundDigits);
        registry.bloodRustTiles = OrderedSprites(NewTileFolder, RxBloodRust);
        registry.cursedGlowTiles = OrderedSprites(NewTileFolder, RxCursed);
        registry.decorationTiles = OrderedSprites(NewTileFolder, RxDecoration);
        registry.edgeTiles       = OrderedSprites(NewTileFolder, RxEdge);
        registry.obstacleTiles   = OrderedSprites(NewTileFolder, RxObstacle);

        if (registry.bloodRustTiles != null && registry.bloodRustTiles.Length > 0)
            registry.groundBloodTile = registry.bloodRustTiles[0];
        if (registry.cursedGlowTiles != null && registry.cursedGlowTiles.Length > 0)
            registry.groundGrassTile = registry.cursedGlowTiles[0];
        if (registry.edgeTiles != null && registry.edgeTiles.Length >= 3)
        {
            registry.borderTopLeft   = registry.edgeTiles[0];
            registry.borderTopCenter = registry.edgeTiles[1];
            registry.borderTopRight  = registry.edgeTiles[2];
        }

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ApplyNewTileSpritesToRegistry] Registre mis à jour : sol {registry.groundTiles.Length}, " +
                  $"bloodRust {registry.bloodRustTiles.Length}, cursed {registry.cursedGlowTiles.Length}, " +
                  $"deco {registry.decorationTiles.Length}, edges {registry.edgeTiles.Length}, obstacles {registry.obstacleTiles.Length}");
    }

    static Sprite[] OrderedSprites(string folder, Regex namePattern)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        var        tmp = new List<(int index, Sprite spr)>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string baseName = Path.GetFileNameWithoutExtension(path);
            Match        m = namePattern.Match(baseName);
            if (!m.Success || m.Groups.Count < 2)
                continue;

            if (!int.TryParse(m.Groups[1].Value, out int n))
                continue;

            Sprite sprite = LoadFirstSprite(path);
            if (sprite != null)
                tmp.Add((n, sprite));
        }

        tmp.Sort((a, b) => a.index.CompareTo(b.index));
        var result = new Sprite[tmp.Count];
        for (int i = 0; i < tmp.Count; i++)
            result[i] = tmp[i].spr;
        return result;
    }

    static Sprite LoadFirstSprite(string assetPath)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (s != null)
            return s;

        Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object o in objs)
        {
            if (o is Sprite spr)
                return spr;
        }
        return null;
    }
}
#endif
