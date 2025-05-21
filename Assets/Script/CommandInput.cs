using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CommandInput : MonoBehaviour
{
    SelectCharacter selectedCharacter;
    [SerializeField] CommandType currentCommand;

    CommandManager commandManager;
    CursorData cursorData;
    MoveUnit moveUnit;
    CharacterAttack characterAttack;
    MouseInput mouseInput;


    private void Awake()
    {
        currentCommand = CommandType.Default;
        
        mouseInput = new MouseInput();
        
        commandManager = GetComponent<CommandManager>();
        cursorData = GetComponent<CursorData>();
        moveUnit = GetComponent<MoveUnit>();
        characterAttack = GetComponent<CharacterAttack>();
        selectedCharacter = GetComponent<SelectCharacter>();
    }

    private void OnEnable()
    {
        mouseInput.Enable();
        mouseInput.UnitCommand.ConfirmAction.performed += HandleLeftClick;
        mouseInput.UnitControl.Deselect.performed += HandleRightClick;
    }

    private void OnDisable()
    {
        mouseInput.Disable();
        mouseInput.UnitCommand.ConfirmAction.performed -= HandleLeftClick;
    }

    public void SetCommandType(CommandType commandType)
    {
        currentCommand = commandType;
    }

    public void InitCommand()
    {
        switch (currentCommand)
        {
            case CommandType.MoveTo:
                HighlightWalkableTerrain();
                break;
            case CommandType.Attack:
                characterAttack.CalculateAttackArea(
                    selectedCharacter.selected.GetComponent<GridObject>().positionOnGrid,
                    selectedCharacter.selected.atkRange);
                break;
        }
    }

    private void HandleLeftClick(InputAction.CallbackContext input)
    {
        switch (currentCommand)
        {
            case CommandType.Default:
                selectedCharacter.Select();
                break;
            case CommandType.MoveTo:
                MoveCommand();
                break;
            case CommandType.Attack:
                AttackCommand();
                break;
        }
    }

    private void HandleRightClick(InputAction.CallbackContext input)
    {
        selectedCharacter.Deselect();
    }

    public void HighlightWalkableTerrain()
    {
        moveUnit.CheckWalkableTerrain(selectedCharacter.selected);
    }

    private void AttackCommand()
    {
        GridObject gridObject = characterAttack.GetAttackTarget(cursorData.positionOnGrid);
        if (gridObject == null) { return; }
        commandManager.AddAttackCommand(selectedCharacter.selected, cursorData.positionOnGrid, gridObject);
        commandManager.ExecuteCommand();
    }

    private void MoveCommand()
    {
        List<PathNode> path = moveUnit.GetPath(cursorData.positionOnGrid);
        commandManager.AddMoveCommand(selectedCharacter.selected, cursorData.positionOnGrid, path);
        commandManager.ExecuteCommand();
    }
}
