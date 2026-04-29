using UnityEngine;

/// <summary>
/// Registre centralisé de tous les sprites de tiles.
/// Assigne ici tes sprites depuis Assets/_Game/Sprites/Tiles.
/// Créer via : Clic droit → Create → Arena → Tile Sprite Registry
///
/// IMPORTANT — Import des GIF dans Unity :
/// Sélectionne chaque sprite dans le Project, puis dans l'Inspector :
///   Texture Type      → Sprite (2D and UI)
///   Sprite Mode       → Single
///   Filter Mode       → Point (no filter)   ← pixel art
///   Compression       → None
///   Pixels Per Unit   → correspond à ton GridConfig (32 ou 64)
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
    [Tooltip("GROUNDBLOOD — sol avec taches de sang")]
    public Sprite groundBloodTile;

    [Tooltip("GROUNDGRASS — sol avec herbe morte / mousse")]
    public Sprite groundGrassTile;

    // =========================================================
    // OBSTACLES
    // =========================================================

    [Header("=== OBSTACLES (OBSTACLE1 → OBSTACLE4) ===")]
    [Tooltip("Glisse OBSTACLE1 à OBSTACLE4 dans ce tableau.")]
    public Sprite[] obstacleTiles;

    // =========================================================
    // BORDURES DÉCORATIVES (optionnel pour le MVP)
    // =========================================================

    [Header("=== BORDURES DÉCORATIVES ===")]
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

    /// <summary>Retourne le sprite de sol correspondant au CellTileType donné.</summary>
    public Sprite GetGroundSpriteForType(CellTileType type, System.Random rng)
    {
        switch (type)
        {
            case CellTileType.GroundBlood:
                return groundBloodTile != null ? groundBloodTile : GetRandomGroundTile(rng);
            case CellTileType.GroundGrass:
                return groundGrassTile != null ? groundGrassTile : GetRandomGroundTile(rng);
            default:
                return GetRandomGroundTile(rng);
        }
    }
}
