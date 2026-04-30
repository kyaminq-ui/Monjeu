using UnityEngine;

public class CellHighlight : MonoBehaviour
{
    static Material CachedUnlitSpriteMaterial;

    // =========================================================
    // RùFùRENCES PRIVùES
    // =========================================================

    private SpriteRenderer spriteRenderer;
    private GridConfig     config;

    private bool   isPulsing = false;
    private float  pulseTimer = 0f;
    private Color  baseColor;

    [Header("Rendu (URP / grilles procùdurales)")]
    [Tooltip("Rùglùs au-dessus des sprites de sol crùùs par ArenaGenerator (tri isomùtrique trùs nùgatif).")]
    [SerializeField] int overlaySortingOrder = 420;

    [Tooltip("Sprite-Lit-Default multiplie la teinte sprite par les lumiùres 2D : en scùne sombre la surbrillance " +
             "semble absente mùme si une couleur forte est rùglùe. Unlit garantit Move/Attack lisibles.")]
    [SerializeField] bool useUnlitHighlightMaterial = true;

    [Header("Animation")]
    [Range(0f, 5f)]
    public float pulseSpeed = 2f;

    [Range(0f, 1f)]
    public float pulseIntensity = 0.3f;

    // =========================================================
    // INITIALISATION ù Appelùe par GridManager
    // =========================================================

    public void Initialize(Cell cell, GridConfig gridConfig)
    {
        config           = gridConfig;
        spriteRenderer   = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (config.cellSprite != null)
            spriteRenderer.sprite = config.cellSprite;

        spriteRenderer.sortingOrder = overlaySortingOrder;
        ApplySharedUnlitMaterialIfConfigured();

        gameObject.name = $"Cell_{cell.GridX}_{cell.GridY}";
        RestoreBaselineVisual();
    }

    // =========================================================
    // UPDATE ù Animation pulsation
    // =========================================================

    void Update()
    {
        if (!isPulsing) return;

        pulseTimer += Time.deltaTime * pulseSpeed;

        float sinValue = (Mathf.Sin(pulseTimer) + 1f) / 2f;

        Color pulseColor = Color.Lerp(baseColor, Color.white, sinValue * pulseIntensity);
        spriteRenderer.color = pulseColor;
    }

    // =========================================================
    // MùTHODES PUBLIQUES
    // =========================================================

    /// <summary>Applique un highlight selon le type</summary>
    public void ApplyHighlight(HighlightType type)
    {
        if (spriteRenderer == null || config == null) return;

        isPulsing   = false;
        pulseTimer  = 0f;

        if (type == HighlightType.None)
        {
            RestoreBaselineVisual();
            return;
        }

        // Les cases dùsactivùes (ex. Show Grid off) seraient encore invisibles sans highlight
        spriteRenderer.enabled       = true;
        spriteRenderer.sortingOrder = overlaySortingOrder;
        ApplySharedUnlitMaterialIfConfigured();

        switch (type)
        {
            case HighlightType.Move:
                SetColor(config.moveColor, true);
                break;
            case HighlightType.Attack:
                SetColor(config.attackColor, true);
                break;
            case HighlightType.AoE:
                SetColor(config.aoeColor, true);
                break;
            case HighlightType.Selected:
                SetColor(config.selectedColor, false);
                break;
            case HighlightType.Hover:
                SetColor(config.hoverColor, false);
                break;
            default:
                RestoreBaselineVisual();
                break;
        }
    }

    /// <summary>Remet la couleur par dùfaut et stoppe la pulsation</summary>
    public void ResetColor()
    {
        RestoreBaselineVisual();
    }

    /// <summary>Affiche ou cache ce visuel</summary>
    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }

    // =========================================================
    // UTILITAIRES INTERNES
    // =========================================================

    /// <summary>ùtat repos : grille optionnellement visible + couleur dùfaut GridConfig.</summary>
    void RestoreBaselineVisual()
    {
        isPulsing  = false;
        pulseTimer = 0f;
        baseColor  = config != null ? config.defaultCellColor : Color.white;
        spriteRenderer.color = baseColor;

        if (config != null)
            spriteRenderer.enabled = config.showGridOnStart;
        else
            spriteRenderer.enabled = false;
    }

    void SetColor(Color color, bool pulse)
    {
        baseColor    = color;
        spriteRenderer.color = color;
        isPulsing    = pulse;
    }

    void ApplySharedUnlitMaterialIfConfigured()
    {
        if (!useUnlitHighlightMaterial || spriteRenderer == null)
            return;

        if (CachedUnlitSpriteMaterial == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (s == null)
                s = Shader.Find("Sprites/Default");
            if (s != null)
                CachedUnlitSpriteMaterial = new Material(s);
        }

        if (CachedUnlitSpriteMaterial != null)
            spriteRenderer.sharedMaterial = CachedUnlitSpriteMaterial;
    }
}
