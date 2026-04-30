using UnityEngine;

/// <summary>
/// Registre centralisé de tous les sprites de tiles.
/// Assigne ici tes sprites depuis Assets/_Game/Sprites/Tiles.
/// Créer via : Clic droit → Create → Arena → Tile Sprite Registry
///
/// IMPORTANT — Tuiles sous Assets/_Game/Sprites/Tiles/NewTile (*.png) :
/// Utilise le menu Unity <b>Arena → Appliquer les tuiles NewTile au registre</b>
/// pour remplir automatiquement ce ScriptableObject, ou assigne les sprites à la main.
///
/// Sinon (GIF / textures manuelles), dans l'Inspector Texture :
///   Texture Type → Sprite (2D and UI), Sprite Mode → Single,
///   Filter Mode → Point, Compression → None,
///   Pixels Per Unit → comme les autres tuiles du projet (ex. 64).
/// </summary>
[CreateAssetMenu(fileName = "TileSpriteRegistry", menuName = "Arena/Tile Sprite Registry")]
public class TileSpriteRegistry : ScriptableObject
{
    // =========================================================
    // SOLS
    // =========================================================

    [Header("=== TILES DE SOL (GROUND1 → GROUND12) ===")]
    [Tooltip("Glisse GROUND1 à GROUND12 dans ce tableau. " +
             "L'arène les pioche aléatoirement pour la diversité visuelle.")]
    public Sprite[] groundTiles;

    [Header("=== VARIANTES DÉCORATIVES ===")]
    [Tooltip("Fallback si bloodRustTiles est vide — ancien sprite GROUNDBLOOD unique.")]
    public Sprite groundBloodTile;

    [Tooltip("Fallback si cursedGlowTiles est vide — ancien sprite GROUNDGRASS unique.")]
    public Sprite groundGrassTile;

    [Header("=== PACK NEW TILE — RUST / MALÉDICTION ===")]
    [Tooltip("GROUND_BLOOD_RUST_VARIANT1..12 — variante sang/rouille (GroundBlood).")]
    public Sprite[] bloodRustTiles;

    [Tooltip("GROUND_CURSED_GLOW_VARIANT1..12 — aura maudite pour GroundGrass.")]
    public Sprite[] cursedGlowTiles;

    [Header("=== PACK NEW TILE — DÉCORS SUPPLÉMENTAIRES ===")]
    [Tooltip("GROUND_DECORATION1..12 — couche additive au sol (voir ArenaConfig.decorationTileChance).")]
    public Sprite[] decorationTiles;

    [Tooltip("GROUND_EDGE1..12 — périmètre de la grille (coins et bords de carte).")]
    public Sprite[] edgeTiles;

    // =========================================================
    // OBSTACLES
    // =========================================================

    [Header("=== OBSTACLES ===")]
    [Tooltip("Glisse GROUND_OBSTACLE1 à 12 dans ce tableau (ou OBSTACLE1..4 anciens sprites).")]
    public Sprite[] obstacleTiles;

    // =========================================================
    // BORDURES DÉCORATIVES (legacy — coins carte)
    // =========================================================

    [Header("=== BORDURES DÉCORATIVES (GIF legacy) ===")]
    [Tooltip("TOPLEFT — coin supérieur gauche de la carte")]
    public Sprite borderTopLeft;

    [Tooltip("TOPCENTER — bord supérieur central")]
    public Sprite borderTopCenter;

    [Tooltip("TOPRIGHT — coin supérieur droit de la carte")]
    public Sprite borderTopRight;

    // =========================================================
    // COULEURS DE SPAWN (overlay sur le sol)
    // =========================================================

    [Header("=== COULEURS DE SPAWN ===")]
    [Tooltip("Teinte de la zone de spawn Équipe 1 (gauche). " +
             "Appliquée en overlay sur le CellHighlight existant.")]
    public Color spawnTeam1Color = new Color(0.25f, 0.45f, 1f, 0.40f);

    [Tooltip("Teinte de la zone de spawn Équipe 2 (droite).")]
    public Color spawnTeam2Color = new Color(1f, 0.30f, 0.20f, 0.40f);

    // =========================================================
    // HELPERS — Accès aléatoire
    // =========================================================

    /// <summary>Retourne un tile de sol aléatoire depuis le tableau groundTiles.</summary>
    public Sprite GetRandomGroundTile(System.Random rng)
    {
        if (groundTiles == null || groundTiles.Length == 0) return null;
        return groundTiles[rng.Next(groundTiles.Length)];
    }

    /// <summary>Retourne un tile d'obstacle aléatoire depuis le tableau obstacleTiles.</summary>
    public Sprite GetRandomObstacleTile(System.Random rng)
    {
        if (obstacleTiles == null || obstacleTiles.Length == 0) return null;
        return obstacleTiles[rng.Next(obstacleTiles.Length)];
    }

    /// <summary>Variante sang/rouille (priorité au pack bloodRust).</summary>
    public Sprite GetRandomBloodRustTile(System.Random rng)
    {
        if (bloodRustTiles != null && bloodRustTiles.Length > 0)
            return bloodRustTiles[rng.Next(bloodRustTiles.Length)];
        return groundBloodTile;
    }

    /// <summary>Variante lueur maudite pour herbe/grass gameplay (priorité au pack).</summary>
    public Sprite GetRandomCursedGlowTile(System.Random rng)
    {
        if (cursedGlowTiles != null && cursedGlowTiles.Length > 0)
            return cursedGlowTiles[rng.Next(cursedGlowTiles.Length)];
        return groundGrassTile;
    }

    /// <summary>Détail décoratif placé au-dessus du sol (pas sur les obstacles).</summary>
    public Sprite GetRandomDecorationTile(System.Random rng)
    {
        if (decorationTiles == null || decorationTiles.Length == 0)
            return null;
        return decorationTiles[rng.Next(decorationTiles.Length)];
    }

    /// <summary>Bord externe de l'arène (alternative au sol générique).</summary>
    public Sprite GetRandomEdgeTile(System.Random rng)
    {
        if (edgeTiles == null || edgeTiles.Length == 0)
            return null;
        return edgeTiles[rng.Next(edgeTiles.Length)];
    }

    /// <summary>Retourne le sprite de sol correspondant au CellTileType donné.</summary>
    public Sprite GetGroundSpriteForType(CellTileType type, System.Random rng)
    {
        switch (type)
        {
            case CellTileType.GroundBlood:
                {
                    Sprite br = GetRandomBloodRustTile(rng);
                    return br != null ? br : GetRandomGroundTile(rng);
                }
            case CellTileType.GroundGrass:
                {
                    Sprite cg = GetRandomCursedGlowTile(rng);
                    return cg != null ? cg : GetRandomGroundTile(rng);
                }
            default:
                return GetRandomGroundTile(rng);
        }
    }
}
