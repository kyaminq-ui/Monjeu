using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================
    public static TurnManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // EVENTS
    // =========================================================
    public event System.Action<TacticalCharacter> OnTurnStart;
    public event System.Action<TacticalCharacter> OnTurnEnd;
    public event System.Action<int> OnRoundStart;       // numéro du round
    public event System.Action<int> OnCombatEnd;        // teamId gagnante (-1 = égalité)
    public event System.Action OnTimeOut;

    // =========================================================
    // CONFIGURATION
    // =========================================================
    [Header("Configuration")]
    public float turnDuration = 15f;

    // =========================================================
    // ÉTAT
    // =========================================================
    private List<TacticalCharacter> turnOrder = new List<TacticalCharacter>();
    private Dictionary<TacticalCharacter, int> characterTeams = new Dictionary<TacticalCharacter, int>();
    private int currentIndex = 0;
    private int roundNumber = 0;
    private float timeRemaining = 0f;
    private bool combatActive = false;
    private bool turnActive = false;

    public TacticalCharacter CurrentCharacter =>
        (currentIndex < turnOrder.Count) ? turnOrder[currentIndex] : null;
    public float TimeRemaining => timeRemaining;
    public int RoundNumber => roundNumber;
    public bool IsCombatActive => combatActive;

    // =========================================================
    // ENREGISTREMENT
    // =========================================================
    public void RegisterCharacter(TacticalCharacter character, int teamId)
    {
        if (characterTeams.ContainsKey(character)) return;
        characterTeams[character] = teamId;
        character.OnDeath += () => OnCharacterDied(character);
    }

    // =========================================================
    // DÉMARRAGE DU COMBAT
    // =========================================================
    public void StartCombat()
    {
        // Ordre tiré aléatoirement une seule fois, fixe pour tout le combat
        turnOrder.Clear();
        foreach (var kvp in characterTeams)
            turnOrder.Add(kvp.Key);
        Shuffle(turnOrder);

        roundNumber = 0;
        currentIndex = 0;
        combatActive = true;
        StartNewRound();
    }

    // =========================================================
    // GESTION DES ROUNDS
    // =========================================================
    private void StartNewRound()
    {
        roundNumber++;
        currentIndex = 0;
        OnRoundStart?.Invoke(roundNumber);
        AdvanceToNextTurn();
    }

    private void AdvanceToNextTurn()
    {
        // Passer les personnages morts
        while (currentIndex < turnOrder.Count && !turnOrder[currentIndex].IsAlive)
            currentIndex++;

        // Tous ont joué ce round → nouveau round
        if (currentIndex >= turnOrder.Count)
        {
            StartNewRound();
            return;
        }

        TacticalCharacter current = turnOrder[currentIndex];
        timeRemaining = turnDuration;
        turnActive = true;

        current.OnTurnStart();
        OnTurnStart?.Invoke(current);
    }

    // =========================================================
    // FIN DE TOUR
    // =========================================================
    public void EndTurn()
    {
        if (!turnActive) return;
        turnActive = false;

        TacticalCharacter current = CurrentCharacter;
        current?.OnTurnEnd();
        OnTurnEnd?.Invoke(current);

        currentIndex++;
        AdvanceToNextTurn();
    }

    // =========================================================
    // TIMER
    // =========================================================
    void Update()
    {
        if (!combatActive || !turnActive) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            OnTimeOut?.Invoke();
            EndTurn();
        }
    }

    // =========================================================
    // VICTOIRE
    // =========================================================
    private void OnCharacterDied(TacticalCharacter character)
    {
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        Dictionary<int, int> aliveByTeam = new Dictionary<int, int>();
        foreach (var kvp in characterTeams)
        {
            if (!kvp.Key.IsAlive) continue;
            int team = kvp.Value;
            if (!aliveByTeam.ContainsKey(team)) aliveByTeam[team] = 0;
            aliveByTeam[team]++;
        }

        if (aliveByTeam.Count == 1)
        {
            combatActive = false;
            turnActive = false;
            foreach (var kvp in aliveByTeam)
                OnCombatEnd?.Invoke(kvp.Key);
        }
        else if (aliveByTeam.Count == 0)
        {
            combatActive = false;
            turnActive = false;
            OnCombatEnd?.Invoke(-1);
        }
    }

    // =========================================================
    // UTILITAIRES
    // =========================================================
    private void Shuffle(List<TacticalCharacter> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
