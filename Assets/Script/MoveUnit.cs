using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    [SerializeField] GridMap grid;
    [SerializeField] LayerMask terrain;
    [SerializeField] GridObject targetCharacter;

    Pathfinding pathfinding;
    List<PathNode> fullPath;
    List<PathNode> trimmedPath;
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        pathfinding = grid.GetComponent<Pathfinding>();
    }

    private void Update()
    {
        UnitMovement unitMovement = targetCharacter.GetComponent<UnitMovement>();
        int maxMoveSpeed = targetCharacter.GetComponent<Character>().MaxMoveSpeed;

        if (Input.GetMouseButtonDown(0))
        {
            if (targetCharacter.GetComponent<UnitMovement>().IsMoving())
            {
                Debug.Log("Can't issue new move order: unit is busy.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrain))
            {
                Vector2Int gridPosition = grid.GetGridPosition(hit.point);

                fullPath = pathfinding.FindPath(targetCharacter.positionOnGrid.x, targetCharacter.positionOnGrid.y, gridPosition.x, gridPosition.y);

                if(fullPath == null || fullPath.Count == 0) { return; }
                if(fullPath.Count <= maxMoveSpeed)
                {
                    trimmedPath = fullPath;
                }
                else
                {
                    trimmedPath = fullPath.GetRange(0, maxMoveSpeed);
                }

                unitMovement.Move(trimmedPath);
            }
        }
    }
}
