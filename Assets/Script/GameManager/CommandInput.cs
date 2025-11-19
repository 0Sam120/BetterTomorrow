using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CommandInput : MonoBehaviour
{
    [SerializeField] CommandType currentCommand;
    [SerializeField] GridRenderer gridRenderer;

    // References to other components
    SelectCharacter selectedCharacter;

    CommandManager commandManager;
    CursorData cursorData;
    MoveUnit moveUnit;
    CharacterAttack characterAttack;
    MouseInput mouseInput;

    public static CommandInput Instance { get; private set; }
    public GridRenderer GetRenderer() => gridRenderer;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentCommand = CommandType.Default;

        // Initialize scripts
        mouseInput = new MouseInput();
        commandManager = new CommandManager();

        // Get component references
        cursorData = GetComponent<CursorData>();
        selectedCharacter = GetComponent<SelectCharacter>();
        characterAttack = GetComponent<CharacterAttack>();
        moveUnit = GetComponent<MoveUnit>();
    }

    private void OnEnable()
    {
        // Enable input
        mouseInput.Enable();

        // Register input callbacks
        mouseInput.UnitCommand.ConfirmAction.performed += HandleLeftClick;
        mouseInput.UnitControl.Deselect.performed += HandleRightClick;
    }

    private void OnDisable()
    {
        // Disable input
        mouseInput.Disable();

        // Unregister input callbacks
        mouseInput.UnitCommand.ConfirmAction.performed -= HandleLeftClick;
    }

    // Sets the current command type
    public void SetCommandType(CommandType commandType)
    {
        currentCommand = commandType;
    }

    // Initializes the command by highlighting valid areas or targets
    public void InitCommand()
    {
        switch (currentCommand)
        {
            case CommandType.MoveTo:
                HighlightWalkableTerrain();
                break;
            case CommandType.Attack:
                characterAttack.CalculateAttackTargets(
                    selectedCharacter.selected.GetComponent<GridObject>().positionOnGrid,
                    selectedCharacter.selected.atkRange, selectedCharacter.selected.GetComponent<CameraHelper>(),
                    selectedCharacter.selected.team);
                break;
        }
    }

    // Handles left-click input
    private void HandleLeftClick(InputAction.CallbackContext input)
    {
        switch (currentCommand)
        {
            case CommandType.UseAbility:
                AbilityCommand();
                break;
        }
    }

    // Handles right-click input
    private void HandleRightClick(InputAction.CallbackContext input)
    {
        switch (currentCommand)
        {
            case CommandType.MoveTo:
                MoveCommand();
                break;
        }
    }

    // Highlights tiles that are walkable for the selected character
    public void HighlightWalkableTerrain()
    {
        moveUnit.CheckWalkableTerrain(selectedCharacter.selected, selectedCharacter.selected.GetComponent<Character>().MaxMoveSpeed);
    }

    // Processes the attack command
    public void AttackCommand()
    {
        var gridObject = characterAttack.GetCurrentTarget();
        if (gridObject == null) return;

        commandManager.AddAttackCommand(
            selectedCharacter.selected,
            gridObject.positionOnGrid,
            gridObject
        );

        commandManager.ExecuteCommand();
        currentCommand = CommandType.Default;
    }

    // Processes the move command
    private void MoveCommand()
    {
        Vector2Int startPos;
        if (selectedCharacter == null)
        {
            Debug.LogError("No character selected for movement.");
            return;
        }

        startPos = selectedCharacter.selected.GetComponent<GridObject>().positionOnGrid;

        List<Vector2Int> path = moveUnit.GetPath(startPos, cursorData.positionOnGrid);

        commandManager.AddMoveCommand(selectedCharacter.selected, cursorData.positionOnGrid, path);
        commandManager.ExecuteCommand();
        currentCommand = CommandType.Default; // Reset command after execution
    }

    private void AbilityCommand()
    {
        List<Character> validTargets;
        validTargets = GetComponent<SkillResolution>().GetValidSkillTargets();

        commandManager.AddAbilityCommand(selectedCharacter.selected, cursorData.positionOnGrid, validTargets);
        commandManager.ExecuteCommand();
        currentCommand = CommandType.Default;
    }
}
