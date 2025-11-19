using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Defines the types of commands that can be executed
public enum CommandType
{
    Default,
    MoveTo,
    Attack,
    UseAbility
}

public class SkillCommandData
{
    public int Range;
    public int Area;
    public TargetType Target;
    public List<SkillEffect> Effects;
}

// Represents a command to be executed by a character
public class Command
{
    public Character character;              // The character performing the command
    public Vector2Int selectedGrid;          // Target grid position for the command
    public CommandType type;                 // Type of command (Move or Attack)
    public SkillCommandData skillData;

    public Command(Character character, Vector2Int selectedGrid, CommandType type, SkillCommandData data = null)
    {
        this.character = character;
        this.selectedGrid = selectedGrid;
        this.type = type;
    }

    public List<Vector2Int> path;              // Path to follow (for MoveTo)
    public GridObject target;                // Target object (for Attack)
    public List<Character> targetCharacters; // Target object (for Skill)
}

// Handles execution of move and attack commands
public class CommandManager
{
    public Command currentCommand;           // Currently active command


    // Executes the current command based on its type
    public void ExecuteCommand()
    {
        switch (currentCommand.type)
        {
            case CommandType.MoveTo:
                ExecuteMoveCommand();
                break;
            case CommandType.Attack:
                ExecuteAttackCommand();
                break;
            case CommandType.UseAbility:
                ExecuteAbilityCommand();
                break;
        }
    }

    // Executes an attack command
    public void ExecuteAttackCommand()
    {
        Debug.Log("Shoot");
        Character receiver = currentCommand.character;

        if (!receiver.GetComponent<CharacterTurn>().SpendMomentum(2))
        {
            return;
        }

        ClearUtility.Instance.FullClear();
        int total = receiver.RollToHit();
        var attackComponent = receiver.GetComponent<AttackComponent>();
        attackComponent.AttackPosition(currentCommand.target, total);
        currentCommand = null;
    }

    // Executes a move command
    public void ExecuteMoveCommand()
    {
        Character receiver = currentCommand.character;
        var characterTurn = receiver.GetComponent<CharacterTurn>();

        // Check both requirements BEFORE executing anything
        bool canSpend = characterTurn.CanSpendMomentum(2);
        bool pathIsValid = receiver.GetComponent<UnitMovement>().PathIsValid(currentCommand.path);

        if (!canSpend || !pathIsValid)
        {
            return; // Abort if either fails
        }

        // Now we can spend momentum and move
        characterTurn.SpendMomentum(2);
        ClearUtility.Instance.FullClear(); // Clear any previous highlights
        receiver.GetComponent<UnitMovement>().Move(currentCommand.path);

        currentCommand = null;
    }

    public void ExecuteAbilityCommand()
    {
        Debug.Log("Using ability");
        
        Character user = currentCommand.character;
        var characterTurn = user.GetComponent<CharacterTurn>();

        // Check both requirements BEFORE executing anything
        bool canSpend = characterTurn.CanSpendMomentum(2);

        if (!canSpend)
        {
            Debug.Log("Not enough momentum");
            return; // Abort if either fails
        }

        // Now we can spend momentum and move
        characterTurn.SpendMomentum(2);
        ClearUtility.Instance.FullClear(); // Clear any previous highlights
        user.GetComponent<SkillComponent>().UseSkill(TurnManager.Instance.currentSkill, currentCommand.targetCharacters);

        TurnManager.Instance.currentSkill = null;
        currentCommand = null;
    }

    // Sets up a move command with path information
    public void AddMoveCommand(Character character, Vector2Int selectedGrid, List<Vector2Int> path)
    {
        currentCommand = new Command(character, selectedGrid, CommandType.MoveTo);
        currentCommand.path = path;
    }

    // Sets up an attack command with a target
    public void AddAttackCommand(Character attacker, Vector2Int selectGrid, GridObject target)
    {
        currentCommand = new Command(attacker, selectGrid, CommandType.Attack);
        if (target == null) { return; }
        currentCommand.target = target;
    }

    public void AddAbilityCommand(Character user, Vector2Int selectGrid, List<Character> validTargets)
    {
        var skill = TurnManager.Instance.currentSkill;
        
        currentCommand = new Command(user, selectGrid, CommandType.UseAbility, new SkillCommandData
        {
            Range = skill.areaOfEffect,
            Area = skill.range,
            Target = skill.targeting

        });

        if(validTargets != null)
        {
            currentCommand.targetCharacters = validTargets;
        }
    }
}
