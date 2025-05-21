using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public enum CommandType
{
    Default,
    MoveTo,
    Attack
}

public class Command
{
    public Character character;
    public Vector2Int selectedGrid;
    public CommandType type;

    public Command(Character character, Vector2Int selectedGrid, CommandType type)
    {
        this.character = character;
        this.selectedGrid = selectedGrid;
        this.type = type;
    }

    public List<PathNode> path;
    public GridObject target;
}

public class CommandManager : MonoBehaviour
{
    public Command currentCommand;
    ClearUtility clearUtility;

    private void Awake()
    {
        clearUtility = GetComponent<ClearUtility>();
    }

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

    public void ExecuteAttackCommand()
    {
        Debug.Log("Shoot");
        Character receiver = currentCommand.character;
        receiver.GetComponent<AttackComponent>().AttackPosition(currentCommand.target);
        currentCommand = null;
        clearUtility.ClearGridHighlightAttack();
    }
    
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

    public void AddMoveCommand(Character character, Vector2Int selectedGrid, List<PathNode> path)
    {
        currentCommand = new Command(character, selectedGrid, CommandType.MoveTo);
        currentCommand.path = path;
    }

    public void AddAttackCommand(Character attacker, Vector2Int selectGrid, GridObject target)
    {
        currentCommand = new Command(attacker, selectGrid, CommandType.Attack);
        if (target == null) { return; }
        currentCommand.target = target;
    }
}
