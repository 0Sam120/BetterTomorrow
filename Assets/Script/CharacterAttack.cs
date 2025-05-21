using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack : MonoBehaviour
{
    [SerializeField] GridMap targetGrid;
    [SerializeField] GridRenderer highlight;

    List<Vector2Int> attackPosition;

    private MouseInput mouseInput;
    private Camera cam;

    private void Awake()
    {
        mouseInput = new MouseInput();
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        //CalculateAttackArea();
    }

    private void OnEnable()
    {
        mouseInput.UnitCommand.Enable();
        mouseInput.UnitCommand.ConfirmAction.performed += Attack;
    }

    private void OnDisable()
    {
        mouseInput.UnitCommand.Disable();
        mouseInput.UnitCommand.ConfirmAction.performed -= Attack;
    }

    public void CalculateAttackArea(Vector2Int characterPositionOnGrid, int attackRange)
    {
        if(attackPosition == null)
        {
            attackPosition = new List<Vector2Int>();
        }
        else
        {
            attackPosition.Clear();
        }

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > attackRange || (x == 0 && y == 0)) { continue; }
                if (targetGrid.CheckBoundry(characterPositionOnGrid.x + x, characterPositionOnGrid.y + y))
                {
                    attackPosition.Add(new Vector2Int(characterPositionOnGrid.x + x,
                        characterPositionOnGrid.y + y));
                }
            }
        }

        highlight.fieldHighlight(attackPosition);
    }

    internal GridObject GetAttackTarget(Vector2Int positionOnGrid)
    {
        GridObject target = targetGrid.GetPlacedObject(positionOnGrid);
        return target;
    }

    internal bool Check(Vector2Int positionOnGrid)
    {
        return attackPosition.Contains(positionOnGrid);
    }

    private void Attack(InputAction.CallbackContext context)
    {
        //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, float.MaxValue, terrainMask))
        //{
        //    Vector2Int gridPosition = targetGrid.GetGridPosition(hit.point);
        //    Debug.Log("Shot");

        //    if (attackPosition.Contains(gridPosition))
        //    {
        //        GridObject gridObject = targetGrid.GetPlacedObject(gridPosition);
        //        if (gridObject == null) { return; }
        //        selectedCharacter.GetComponent<AttackComponent>().AttackPosition(gridObject);
        //        Debug.Log("Found target");
        //    }
        //}
    }
}
