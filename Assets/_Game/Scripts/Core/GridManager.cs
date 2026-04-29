using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire principal de la grille isométrique
/// Singleton : une seule instance accessible partout via GridManager.Instance
/// </summary>
public class GridManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    /// <summary>Instance unique accessible globalement</summary>
    public static GridManager Instance { get; private set; }

    void Awake()
    {
        // Pattern Singleton : s'assurer qu'il n'existe qu'une instance
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("⚠️ GridManager : Instance dupliquée détectée, destruction.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad : survit au changement de scène
        // À retirer si tu veux une grille par scène
        // DontDestroyOnLoad(gameObject);

        InitializeGrid();
    }

    // =========================================================
    // CONFIGURATION
    // =========================================================

    [Header("=== CONFIGURATION ===")]
    [Tooltip("Glisser ici le GridConfig créé dans les assets")]
    public GridConfig config;

    [Header("=== PREFABS ===")]
    [Tooltip("Prefab utilisé pour afficher chaque cellule (optionnel)")]
    public GameObject cellPrefab;

    // =========================================================
    // DONNÉES PRIVÉES
    // =========================================================

    /// <summary>Tableau 2D de toutes les cellules</summary>
    private Cell[,] grid;

    /// <summary>Parent GameObject pour organiser la Hierarchy</summary>
    private Transform gridContainer;

    /// <summary>Liste des cellules actuellement highlighted</summary>
    private List<Cell> highlightedCells = new List<Cell>();

    /// <summary>Cellule actuellement sélectionnée</summary>
    private Cell selectedCell = null;

    /// <summary>Cellule actuellement survolée</summary>
    private Cell hoveredCell = null;

    // =========================================================
    // INITIALISATION
    // =========================================================

    /// <summary>
    /// Créer toute la grille au démarrage
    /// </summary>
    void InitializeGrid()
    {
        // Vérification de sécurité
        if (config == null)
        {
            Debug.LogError("❌ GridManager : Aucun GridConfig assigné ! " +
                          "Crée un GridConfig et assigne-le dans l'Inspector.");
            return;
        }

        // Créer un conteneur pour garder la Hierarchy propre
        gridContainer = new GameObject("=== GRID ===").transform;
        gridContainer.SetParent(transform);

        // Initialiser le tableau
        grid = new Cell[config.width, config.height];

        // Créer chaque cellule
        for (int x = 0; x < config.width; x++)
        {
            for (int y = 0; y < config.height; y++)
            {
                CreateCell(x, y);
            }
        }

        if (config.debugMode)
            Debug.Log($"✅ GridManager : Grille {config.width}x{config.height} créée " +
                     $"({config.width * config.height} cellules)");
    }

    /// <summary>
    /// Créer une cellule individuelle
    /// </summary>
    void CreateCell(int x, int y)
    {
        // Calculer la position monde avec les formules isométriques
        Vector3 worldPos = GridToWorld(x, y);

        // Créer l'objet Cell (données)
        Cell cell = new Cell(x, y, worldPos);
        grid[x, y] = cell;

        // Créer l'objet visuel
        CreateCellVisual(cell, worldPos);
    }

    /// <summary>
    /// Créer le GameObject visuel d'une cellule
    /// </summary>
    void CreateCellVisual(Cell cell, Vector3 worldPos)
    {
        GameObject cellObject;

        // Utiliser le prefab si disponible, sinon créer un objet vide
        if (cellPrefab != null)
        {
            cellObject = Instantiate(cellPrefab, worldPos, Quaternion.identity, gridContainer);
        }
        else
        {
            cellObject = new GameObject();
            cellObject.transform.position = worldPos;
            cellObject.transform.SetParent(gridContainer);
        }

        // Attacher le composant CellHighlight
        CellHighlight highlight = cellObject.GetComponent<CellHighlight>();
        if (highlight == null)
            highlight = cellObject.AddComponent<CellHighlight>();

        // Initialiser le highlight avec la cellule et la config
        highlight.Initialize(cell, config);

        // Lier le visuel à la cellule
        cell.VisualObject = cellObject;

        // Visibilité initiale
        highlight.SetVisible(config.showGridOnStart);
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — ACCÈS AUX CELLULES
    // =========================================================

    /// <summary>
    /// Obtenir une cellule par ses coordonnées de grille
    /// Retourne null si hors limites
    /// </summary>
    public Cell GetCell(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            if (config.debugMode)
                Debug.LogWarning($"⚠️ GetCell({x},{y}) : Hors limites !");
            return null;
        }
        return grid[x, y];
    }

    /// <summary>
    /// Vérifier si des coordonnées sont dans la grille
    /// </summary>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < config.width && y >= 0 && y < config.height;
    }

    /// <summary>
    /// Obtenir toutes les cellules voisines d'une cellule
    /// </summary>
    /// <param name="includeDiagonals">Inclure les diagonales ?</param>
    public List<Cell> GetNeighbors(int x, int y, bool includeDiagonals = false)
    {
        List<Cell> neighbors = new List<Cell>();

        // Les 4 directions cardinales
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        // Les 4 diagonales
        int[] dxDiag = { 1, 1, -1, -1 };
        int[] dyDiag = { 1, -1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            Cell neighbor = GetCell(x + dx[i], y + dy[i]);
            if (neighbor != null) neighbors.Add(neighbor);
        }

        if (includeDiagonals)
        {
            for (int i = 0; i < 4; i++)
            {
                Cell neighbor = GetCell(x + dxDiag[i], y + dyDiag[i]);
                if (neighbor != null) neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — CONVERSIONS
    // =========================================================

    /// <summary>
    /// Convertit des coordonnées de grille en position monde (isométrique)
    /// Formule : worldX = (gridX - gridY) * (tileWidth / 2)
    ///           worldY = (gridX + gridY) * (tileHeight / 4)  (divisé par 4, pas 2)
    /// </summary>
    public Vector3 GridToWorld(int gridX, int gridY)
    {
        float worldX = (gridX - gridY) * (config.tileWidth / 2f);
        float worldY = (gridX + gridY) * (config.tileHeight / 2f);

        // Ajouter l'origine de la grille (décalage global)
        return new Vector3(
            config.gridOrigin.x + worldX,
            config.gridOrigin.y + worldY,
            config.gridOrigin.z
        );
    }

    /// <summary>
    /// Convertit une position monde en coordonnées de grille (isométrique)
    /// Formule inverse de GridToWorld
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // Enlever l'origine pour travailler en local
        float localX = worldPosition.x - config.gridOrigin.x;
        float localY = worldPosition.y - config.gridOrigin.y;

        // Formule inverse isométrique
        float gridXf = (localX / config.tileWidth + localY / config.tileHeight);
        float gridYf = (localY / config.tileHeight - localX / config.tileWidth);

        // Arrondir à l'entier le plus proche
        int gridX = Mathf.RoundToInt(gridXf);
        int gridY = Mathf.RoundToInt(gridYf);

        return new Vector2Int(gridX, gridY);
    }

    /// <summary>
    /// Obtenir la cellule la plus proche d'une position monde
    /// Retourne null si hors grille
    /// </summary>
    public Cell GetCellFromWorldPosition(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGrid(worldPosition);
        return GetCell(gridPos.x, gridPos.y);
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — HIGHLIGHTS
    // =========================================================

    /// <summary>
    /// Mettre en évidence une liste de cellules
    /// </summary>
    public void HighlightCells(List<Cell> cells, HighlightType type)
    {
        foreach (Cell cell in cells)
        {
            if (cell == null) continue;

            cell.SetHighlight(type);
            highlightedCells.Add(cell);

            // Appliquer visuellement
            if (cell.VisualObject != null)
            {
                CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
                highlight?.ApplyHighlight(type);
            }
        }
    }

    /// <summary>
    /// Mettre en évidence une seule cellule
    /// </summary>
    public void HighlightCell(int x, int y, HighlightType type)
    {
        Cell cell = GetCell(x, y);
        if (cell == null) return;

        cell.SetHighlight(type);
        if (!highlightedCells.Contains(cell))
            highlightedCells.Add(cell);

        if (cell.VisualObject != null)
        {
            CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
            highlight?.ApplyHighlight(type);
        }
    }

    /// <summary>
    /// Effacer TOUS les highlights actifs
    /// </summary>
    public void ClearAllHighlights()
    {
        foreach (Cell cell in highlightedCells)
        {
            if (cell == null) continue;

            cell.ClearHighlight();

            if (cell.VisualObject != null)
            {
                CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
                highlight?.ResetColor();
            }
        }

        highlightedCells.Clear();
    }

    /// <summary>
    /// Mettre en évidence une zone carrée autour d'un point
    /// </summary>
    public void HighlightRange(int centerX, int centerY, int range, HighlightType type)
    {
        List<Cell> cellsInRange = new List<Cell>();

        for (int x = centerX - range; x <= centerX + range; x++)
        {
            for (int y = centerY - range; y <= centerY + range; y++)
            {
                // Exclure le centre
                if (x == centerX && y == centerY) continue;

                Cell cell = GetCell(x, y);
                if (cell != null && cell.IsWalkable)
                    cellsInRange.Add(cell);
            }
        }

        HighlightCells(cellsInRange, type);
    }

    /// <summary>
    /// Mettre en évidence une zone en diamant (distance de Manhattan)
    /// Meilleure pour l'isométrique
    /// </summary>
    public void HighlightDiamond(int centerX, int centerY, int range, HighlightType type)
    {
        List<Cell> cellsInRange = new List<Cell>();

        for (int x = centerX - range; x <= centerX + range; x++)
        {
            for (int y = centerY - range; y <= centerY + range; y++)
            {
                // Distance de Manhattan
                int distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);

                if (distance <= range && !(x == centerX && y == centerY))
                {
                    Cell cell = GetCell(x, y);
                    if (cell != null)
                        cellsInRange.Add(cell);
                }
            }
        }

        HighlightCells(cellsInRange, type);
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — SÉLECTION
    // =========================================================

    /// <summary>
    /// Sélectionner une cellule
    /// </summary>
    public void SelectCell(int x, int y)
    {
        // Désélectionner la précédente
        if (selectedCell != null)
        {
            selectedCell.IsSelected = false;
            if (selectedCell.VisualObject != null)
            {
                CellHighlight h = selectedCell.VisualObject.GetComponent<CellHighlight>();
                h?.ApplyHighlight(selectedCell.CurrentHighlight);
            }
        }

        Cell cell = GetCell(x, y);
        if (cell == null) return;

        selectedCell = cell;
        cell.IsSelected = true;

        if (cell.VisualObject != null)
        {
            CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
            highlight?.ApplyHighlight(HighlightType.Selected);
        }
    }

    /// <summary>
    /// Gérer le survol de la souris
    /// </summary>
    public void SetHoveredCell(int x, int y)
    {
        // Retirer le hover précédent
        if (hoveredCell != null && !hoveredCell.IsSelected)
        {
            hoveredCell.IsHovered = false;
            if (hoveredCell.VisualObject != null)
            {
                CellHighlight h = hoveredCell.VisualObject.GetComponent<CellHighlight>();
                h?.ApplyHighlight(hoveredCell.CurrentHighlight);
            }
        }

        Cell cell = GetCell(x, y);
        if (cell == null) { hoveredCell = null; return; }

        hoveredCell = cell;
        cell.IsHovered = true;

        // Ne pas overrider le Selected
        if (!cell.IsSelected)
        {
            if (cell.VisualObject != null)
            {
                CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
                highlight?.ApplyHighlight(HighlightType.Hover);
            }
        }
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — GESTION DES OCCUPANTS
    // =========================================================

    /// <summary>
    /// Placer un objet sur une cellule
    /// </summary>
    public bool PlaceObject(GameObject obj, int x, int y)
    {
        Cell cell = GetCell(x, y);

        if (cell == null)
        {
            Debug.LogWarning($"⚠️ PlaceObject : Cellule ({x},{y}) introuvable.");
            return false;
        }

        if (cell.IsOccupied)
        {
            Debug.LogWarning($"⚠️ PlaceObject : Cellule ({x},{y}) déjà occupée par {cell.Occupant.name}.");
            return false;
        }

        if (!cell.IsWalkable)
        {
            Debug.LogWarning($"⚠️ PlaceObject : Cellule ({x},{y}) non praticable.");
            return false;
        }

        cell.SetOccupant(obj);

        // Déplacer l'objet à la position monde de la cellule
        obj.transform.position = cell.WorldPosition;

        if (config.debugMode)
            Debug.Log($"✅ {obj.name} placé en ({x},{y})");

        return true;
    }

    /// <summary>
    /// Retirer un objet d'une cellule
    /// </summary>
    public void RemoveObject(int x, int y)
    {
        Cell cell = GetCell(x, y);
        if (cell != null)
            cell.ClearOccupant();
    }

    /// <summary>
    /// Déplacer un objet d'une cellule à une autre
    /// </summary>
    public bool MoveObject(int fromX, int fromY, int toX, int toY)
    {
        Cell fromCell = GetCell(fromX, fromY);
        Cell toCell = GetCell(toX, toY);

        if (fromCell == null || toCell == null) return false;
        if (!fromCell.IsOccupied) return false;
        if (toCell.IsOccupied) return false;
        if (!toCell.IsWalkable) return false;

        GameObject obj = fromCell.Occupant;
        fromCell.ClearOccupant();
        toCell.SetOccupant(obj);
        obj.transform.position = toCell.WorldPosition;

        return true;
    }

    // =========================================================
    // MÉTHODES PUBLIQUES — VISIBILITÉ
    // =========================================================

    /// <summary>
    /// Afficher ou cacher toute la grille
    /// </summary>
    public void SetGridVisible(bool visible)
    {
        for (int x = 0; x < config.width; x++)
        {
            for (int y = 0; y < config.height; y++)
            {
                Cell cell = grid[x, y];
                if (cell?.VisualObject != null)
                {
                    CellHighlight highlight = cell.VisualObject.GetComponent<CellHighlight>();
                    highlight?.SetVisible(visible);
                }
            }
        }
    }

    // =========================================================
    // GIZMOS — Debug visuel dans l'éditeur
    // =========================================================

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (config == null) return;

        for (int x = 0; x < config.width; x++)
        {
            for (int y = 0; y < config.height; y++)
            {
                Vector3 pos = GridToWorld(x, y);

                // Dessiner un losange pour chaque cellule
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);

                float hw = config.tileWidth / 2f;   // half width
                float hh = config.tileHeight / 2f;  // half height

                Vector3 top = pos + new Vector3(0, hh, 0);
                Vector3 bottom = pos + new Vector3(0, -hh, 0);
                Vector3 left = pos + new Vector3(-hw, 0, 0);
                Vector3 right = pos + new Vector3(hw, 0, 0);

                Gizmos.DrawLine(top, right);
                Gizmos.DrawLine(right, bottom);
                Gizmos.DrawLine(bottom, left);
                Gizmos.DrawLine(left, top);

                // Numéroter les cellules (seulement si grille petite)
                if (config.width <= 10 && config.height <= 10)
                {
                    UnityEditor.Handles.Label(pos, $"{x},{y}");
                }
            }
        }
    }
#endif
}
