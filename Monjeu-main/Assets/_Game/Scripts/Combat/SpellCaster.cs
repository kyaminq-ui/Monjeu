using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TacticalCharacter))]
public class SpellCaster : MonoBehaviour
{
    private TacticalCharacter character;
    private SpellData selectedSpell;
    private List<Cell> validTargetCells = new List<Cell>();
    private List<Cell> previewCells = new List<Cell>();

    public SpellData SelectedSpell => selectedSpell;
    public bool HasSpellSelected => selectedSpell != null;

    void Awake() => character = GetComponent<TacticalCharacter>();

    // =========================================================
    // SÉLECTION D'UN SORT
    // =========================================================
    public bool SelectSpell(SpellData spell)
    {
        CancelSpell();
        if (!character.CanCastSpell(spell)) return false;

        selectedSpell = spell;

        if (spell.zoneType == ZoneType.Self)
        {
            validTargetCells = new List<Cell> { character.CurrentCell };
        }
        else
        {
            validTargetCells = ComputeValidTargetCells(spell);
            GridManager.Instance.HighlightCells(validTargetCells, HighlightType.Attack);
        }
        return true;
    }

    public void CancelSpell()
    {
        if (selectedSpell == null) return;
        GridManager.Instance.ClearAllHighlights();
        previewCells.Clear();
        validTargetCells.Clear();
        selectedSpell = null;
    }

    // =========================================================
    // PREVIEW AoE AU SURVOL
    // =========================================================
    public void PreviewAoE(Cell hoveredCell)
    {
        if (selectedSpell == null || !validTargetCells.Contains(hoveredCell)) return;
        if (previewCells.Count > 0 && previewCells[0] == hoveredCell) return; // déjà affiché

        // Restaurer les highlights précédents
        GridManager.Instance.ClearAllHighlights();
        GridManager.Instance.HighlightCells(validTargetCells, HighlightType.Attack);

        previewCells = AoECalculator.GetAffectedCells(
            selectedSpell.zoneType,
            character.CurrentCell,
            hoveredCell,
            selectedSpell.aoeRadius
        );
        GridManager.Instance.HighlightCells(previewCells, HighlightType.AoE);
    }

    public void ClearPreview()
    {
        if (previewCells.Count == 0) return;
        GridManager.Instance.ClearAllHighlights();
        GridManager.Instance.HighlightCells(validTargetCells, HighlightType.Attack);
        previewCells.Clear();
    }

    // =========================================================
    // LANCER LE SORT
    // =========================================================
    public bool TryCast(Cell targetCell)
    {
        if (selectedSpell == null) return false;
        if (!validTargetCells.Contains(targetCell)) return false;

        List<Cell> affectedCells = AoECalculator.GetAffectedCells(
            selectedSpell.zoneType,
            character.CurrentCell,
            targetCell,
            selectedSpell.aoeRadius
        );

        character.SpendPA(selectedSpell.paCost);
        character.StartCooldown(selectedSpell);
        character.SetCastingState(true);

        SpellResolver.Resolve(selectedSpell, character, affectedCells);

        character.SetCastingState(false);

        // Briser l'invisibilité au cast
        character.RemoveStatusEffect(StatusEffectType.Invisible);

        CancelSpell();
        return true;
    }

    // =========================================================
    // CALCUL DES CASES VALIDES
    // =========================================================
    private List<Cell> ComputeValidTargetCells(SpellData spell)
    {
        var cells = new List<Cell>();
        Cell origin = character.CurrentCell;
        if (origin == null) return cells;

        int effectiveMax = spell.rangeMax + character.GetBonusRange();

        if (spell.isMeleeOnly)
            return GridManager.Instance.GetNeighbors(origin);

        for (int x = origin.GridX - effectiveMax; x <= origin.GridX + effectiveMax; x++)
            for (int y = origin.GridY - effectiveMax; y <= origin.GridY + effectiveMax; y++)
            {
                int dist = Mathf.Abs(x - origin.GridX) + Mathf.Abs(y - origin.GridY);
                if (dist < spell.rangeMin || dist > effectiveMax) continue;

                Cell cell = GridManager.Instance.GetCell(x, y);
                if (cell == null) continue;

                if (spell.zoneType == ZoneType.FreeCell && (cell.IsOccupied || !cell.IsWalkable)) continue;

                if (spell.requiresLineOfSight && !spell.ignoresLineOfSight)
                    if (!AoECalculator.HasLineOfSight(origin, cell)) continue;

                cells.Add(cell);
            }

        return cells;
    }
}
