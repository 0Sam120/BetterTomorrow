using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class HighlightTest : MonoBehaviour
{
    [SerializeField] GridRenderer gridRenderer;

    SelectCharacter selectCharacter;
    HighlightTestInput testInput;
    InputAction moveAction;

    private void Awake()
    {
        testInput = new HighlightTestInput();
    }

    void Start()
    {
        gridRenderer.currentTargetType = TargetType.Ally; 
        selectCharacter = GetComponent<SelectCharacter>();
        selectCharacter.MoveCommandSelected();
    }

    public void OnEnable()
    {
        testInput.Enable();
        testInput.HighlightSelector.Single.performed += HighlightSingle;
        testInput.HighlightSelector.Square.performed += HighlightSquare;
        testInput.HighlightSelector.Circle.performed += HighlightCircle;
        testInput.HighlightSelector.Line.performed +=   HighlightLine;
        testInput.HighlightSelector.Cone.performed += HighlightCone;
    }

    public void OnDisable()
    {
        testInput.Disable();
        testInput.HighlightSelector.Single.performed -= HighlightSingle;
        testInput.HighlightSelector.Square.performed -= HighlightSquare;
        testInput.HighlightSelector.Circle.performed -= HighlightCircle;
        testInput.HighlightSelector.Line.performed -= HighlightLine;
        testInput.HighlightSelector.Cone.performed -= HighlightCone;
    }

    void HighlightSingle(InputAction.CallbackContext inputValue)
    {
        gridRenderer.currentTargetType = TargetType.Ally;
    }

    void HighlightSquare(InputAction.CallbackContext inputValue)
    {
        gridRenderer.currentTargetType = TargetType.Area;
    }

    void HighlightCircle(InputAction.CallbackContext inputValue)
    {
        gridRenderer.currentTargetType = TargetType.Circle;
    }

    void HighlightLine(InputAction.CallbackContext inputValue)
    {
        gridRenderer.currentTargetType = TargetType.Line;
    }

    void HighlightCone(InputAction.CallbackContext inputValue)
    {
        gridRenderer.currentTargetType = TargetType.Cone;
    }
}
