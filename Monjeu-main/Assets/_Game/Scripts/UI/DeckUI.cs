using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Hotbar de sorts en bas d'écran.
/// Raccourcis : Q W E R A S D F (indices 0-7).
/// Réagit aux événements PA/PM du TacticalCharacter actif.
/// </summary>
public class DeckUI : MonoBehaviour
{
    // =========================================================
    // CONFIGURATION
    // =========================================================
    [Header("Slots (6 max, correspond aux touches 1 2 3 4 5 6)")]
    public List<SpellSlotUI> slots = new List<SpellSlotUI>();

    [Header("Tooltip")]
    public SpellTooltip tooltip;

    // =========================================================
    // ÉTAT
    // =========================================================
    private TacticalCharacter activeCharacter;
    private SpellCaster       activeCaster;
    private int               selectedSlotIndex = -1;

    private static readonly string[] Hotkeys = { "1", "2", "3", "4", "5", "6" };
    private static readonly KeyCode[] HotkeyCodes =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
    };

    // =========================================================
    // LIAISON AVEC UN PERSONNAGE
    // =========================================================
    public void BindCharacter(TacticalCharacter character)
    {
        // Désabonner l'ancien
        if (activeCharacter != null)
        {
            activeCharacter.OnPAChanged -= OnResourceChanged;
            activeCharacter.OnPMChanged -= OnResourceChanged;
        }

        activeCharacter = character;
        activeCaster    = character != null ? character.GetComponent<SpellCaster>() : null;

        if (activeCharacter != null)
        {
            activeCharacter.OnPAChanged += OnResourceChanged;
            activeCharacter.OnPMChanged += OnResourceChanged;
        }

        RebuildSlots();
    }

    public void UnbindCharacter()
    {
        BindCharacter(null);
        ClearSelection();
    }

    // =========================================================
    // CONSTRUCTION DE LA HOTBAR
    // =========================================================
    private void RebuildSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            SpellData spell = null;
            if (activeCharacter != null && activeCharacter.deck != null)
            {
                var spells = activeCharacter.deck.Spells;
                spell = i < spells.Count ? spells[i] : null;
            }

            string hotkey = i < Hotkeys.Length ? Hotkeys[i] : "";
            slots[i].Setup(spell, activeCharacter, this, i, hotkey);
            slots[i].gameObject.SetActive(activeCharacter != null);
        }

        ClearSelection();
    }

    // =========================================================
    // REFRESH — mis à jour à chaque changement de PA/PM
    // =========================================================
    private void OnResourceChanged(int current, int max) => RefreshAll();

    public void RefreshAll()
    {
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] != null) slots[i].Refresh();
    }

    // =========================================================
    // SÉLECTION
    // =========================================================
    public void SelectSlot(int index)
    {
        if (activeCaster == null || activeCharacter == null) return;
        if (index < 0 || index >= slots.Count) return;

        SpellSlotUI slot = slots[index];
        if (slot == null || !slot.HasSpell) return;

        // Désélectionner si on reclique sur le même
        if (selectedSlotIndex == index)
        {
            ClearSelection();
            activeCaster.CancelSpell();
            return;
        }

        // Tenter de sélectionner le sort
        bool ok = activeCaster.SelectSpell(slot.Spell);
        if (!ok) return;

        // Mettre à jour le visuel
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
            slots[selectedSlotIndex].SetSelected(false);

        selectedSlotIndex = index;
        slot.SetSelected(true);
    }

    public void ClearSelection()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
            slots[selectedSlotIndex]?.SetSelected(false);
        selectedSlotIndex = -1;
    }

    // =========================================================
    // RACCOURCIS CLAVIER
    // =========================================================
    void Update()
    {
        if (activeCharacter == null) return;

        for (int i = 0; i < HotkeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(HotkeyCodes[i]))
            {
                SelectSlot(i);
                break;
            }
        }

        // Echap = annuler le sort sélectionné
        if (Input.GetKeyDown(KeyCode.Escape) && selectedSlotIndex >= 0)
        {
            ClearSelection();
            activeCaster?.CancelSpell();
        }
    }

    // =========================================================
    // TOOLTIP
    // =========================================================
    public void ShowTooltip(SpellData spell, Vector3 anchorWorldPos)
    {
        if (tooltip != null) tooltip.Show(spell, anchorWorldPos);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.Hide();
    }
}
