using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Defines the types of commands that can be executed
public enum CommandType
{
    Default,
    MoveTo,
    Attack
}

// Represents a command to be executed by a character
public class Command
{
    public Character character;              // The character performing the command
    public Vector2Int selectedGrid;          // Target grid position for the command
    public CommandType type;                 // Type of command (Move or Attack)

    public Command(Character character, Vector2Int selectedGrid, CommandType type)
    {
        this.character = character;
        this.selectedGrid = selectedGrid;
        this.type = type;
    }

    public List<PathNode> path;              // Path to follow (for MoveTo)
    public GridObject target;                // Target object (for Attack)
}

// Handles execution of move and attack commands
public class CommandManager : MonoBehaviour
{
    public Command currentCommand;           // Currently active command
    ClearUtility clearUtility;              // Utility to clear grid highlights and paths

    private void Awake()
    {
        clearUtility = GetComponent<ClearUtility>();
    }

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
        }
    }

    // Executes an attack command
    public void ExecuteAttackCommand()
    {
        Debug.Log("Shoot");
        Character receiver = currentCommand.character;
        receiver.GetComponent<AttackComponent>().AttackPosition(currentCommand.target);
        currentCommand = null;
        clearUtility.ClearGridHighlightAttack();
    }

    // Executes a move command
    public void ExecuteMoveCommand()
    {
        Character receiver = currentCommand.character;
        receiver.GetComponent<UnitMovement>().Move(currentCommand.path);

        if (receiver.GetComponent<UnitMovement>().isMoving)
        {
            clearUtility.ClearPathfinding();
            clearUtility.ClearGridHighlightMove();
            currentCommand = null;
        }
        else
        {
            return;
        }
    }

    // Sets up a move command with path information
    public void AddMoveCommand(Character character, Vector2Int selectedGrid, List<PathNode> path)
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
}
