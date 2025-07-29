using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public enum GameState
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    CheckWinCondition,
    CombatEnd,
    Awaiting,
    Busy
}

public class TurnManager : MonoBehaviour
{
    public List<Character> ActiveUnits = new List<Character>();
    public List<Character> PlayerTeam = new List<Character>();
    public List<Character> EnemyTeam = new List<Character>();

    public static TurnManager Instance { get; private set; }

    public Character currentUnit;
    public CommandMenu commandMenu;
    public AIManager AI;
    public GameState state;
    public int combatRound = 0;
    public int currentInitiativeIndex = 0;
    private bool isEndingTurn = false;
    AnimatorProxy proxy;

    private void Awake()
    {
        commandMenu = GetComponent<CommandMenu>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        state = GameState.Setup;
        new WaitForSeconds(3f);
        StartCombat();
    }

    public void StartCombat()
    {
        ScanForActiveUnits();
        AssignUnitsToTeams();
        RollInitiative();
        IncrementRounds();
    }

    void ScanForActiveUnits()
    {
        ActiveUnits = UnitRegistry.AllUnits.Where(unit => unit.IsAlive()).ToList();
        Debug.Log($"Found {ActiveUnits.Count} active units for combat");
    }

    void AssignUnitsToTeams()
    {
        PlayerTeam = ActiveUnits.Where(unit => unit.team == Team.Player).ToList();
        EnemyTeam = ActiveUnits.Where(unit => unit.team == Team.Enemy).ToList();
        Debug.Log($"Player Team: {PlayerTeam.Count} units, Enemy Team: {EnemyTeam.Count} units");
    }

    void RollInitiative()
    {
        foreach(var unit in ActiveUnits)
        {
            unit.Initiative = Random.Range(1, 21) + unit.InitiativeMod; // Roll 1d20 + Initiative Modifier
            Debug.Log($"{unit.Name} rolled initiative: {unit.Initiative}");
        }

        ActiveUnits = ActiveUnits.OrderByDescending(unit => unit.Initiative).ToList();

        Debug.Log("Initiative Order:");
        for(int i = 0; i < ActiveUnits.Count; i++)
        {
            Debug.Log($"{i + 1}. {ActiveUnits[i].Name} (Initiative: {ActiveUnits[i].Initiative})");
        }
    }

    void ProcessCurrentUnit()
    {
        Debug.Log($"Round {combatRound} - {currentUnit.name}'s turn (Initiative {currentInitiativeIndex + 1}/{ActiveUnits.Count}), belonging to team {currentUnit.team}");
        new WaitForSeconds(5f); // Simulate a delay for processing

        if (currentUnit.team == Team.Player)
        {
            state = GameState.PlayerTurn;
            HandlePlayerTurn();
        }
        else
        {
            state = GameState.EnemyTurn;
            HandleEnemyTurn();
        }
    }

    public static class UnitRegistry
    {
        public static readonly List<Character> AllUnits = new List<Character>();

        public static void Register(Character unit) => AllUnits.Add(unit);
        public static void Deregister(Character unit) => AllUnits.Remove(unit);
    }

    void IncrementRounds()
    {
        combatRound++;
        currentInitiativeIndex = 0;

        Debug.Log($"=== Starting Round {combatRound} ===");

        ActiveUnits.RemoveAll(unit => !unit.IsAlive());

        if (ActiveUnits.Count > 0)
        {
            currentUnit = ActiveUnits[currentInitiativeIndex];
            ProcessCurrentUnit();
        }
    }

    public void HandlePlayerTurn()
    {
        currentUnit.GetComponent<CharacterTurn>().GrantTurn();
        Debug.Log("Player's turn started");
    }

    public void HandleEnemyTurn()
    {
        if (currentUnit == null)
        {
            Debug.LogError("Current unit is null in HandleEnemyTurn");
            return;
        }
        var ai = currentUnit.GetComponent<AIManager>();
        if (ai == null)
        {
            Debug.LogError("AI is null in HandleEnemyTurn");
            return;
        }

        ai.GetComponent<CharacterTurn>().GrantTurn();
        ai.HandleAITurn(currentUnit);
    }

    public void EndCurrentUnitTurn()
    {
        if (isEndingTurn) return;
        isEndingTurn = true;

        proxy = new AnimatorProxy(currentUnit.GetComponentInChildren<Animator>(), this);
        proxy.WaitUntilAnimationStops(() =>
        {
            commandMenu.ClosePanel();
            state = GameState.Awaiting;
            Debug.Log($"{currentUnit.name}'s turn ended");

            currentInitiativeIndex++;
            if (currentInitiativeIndex >= ActiveUnits.Count)
            {
                isEndingTurn = false;
                IncrementRounds();
            }
            else
            {
                currentUnit = ActiveUnits[currentInitiativeIndex];
                isEndingTurn = false;
                ProcessCurrentUnit();
            }
        }
        );
    }
}
