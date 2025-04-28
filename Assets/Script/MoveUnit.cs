using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    [SerializeField] GridMap grid; // Reference to the GridMap for position calculations
    [SerializeField] LayerMask terrain; // LayerMask to detect valid terrain clicks
    [SerializeField] GridObject targetCharacter; // The character that will be moved

    Pathfinding pathfinding; // Reference to the pathfinding system
    List<PathNode> fullPath; // Full path from current to target position
    List<PathNode> trimmedPath; // Trimmed path limited by character's move speed
    Camera cam; // Reference to the camera component

    private void Awake()
    {
        cam = GetComponent<Camera>(); // Get the Camera component attached to the same GameObject
    }

    private void Start()
    {
        pathfinding = grid.GetComponent<Pathfinding>(); // Get the Pathfinding component from the grid
    }

    private void Update()
    {
        UnitMovement unitMovement = targetCharacter.GetComponent<UnitMovement>(); // Get the UnitMovement script on the target character
        int maxMoveSpeed = targetCharacter.GetComponent<Character>().MaxMoveSpeed; // Get the character's maximum movement speed

        if (Input.GetMouseButtonDown(0)) // When left mouse button is clicked
        {
            if (unitMovement.IsMoving()) // Prevent issuing new orders while moving
            {
                Debug.Log("Can't issue new move order: unit is busy.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition); // Create a ray from the camera to the mouse position
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrain)) // If the ray hits valid terrain
            {
                Vector2Int gridPosition = grid.GetGridPosition(hit.point); // Convert hit point to grid coordinates

                fullPath = pathfinding.FindPath(targetCharacter.positionOnGrid.x, targetCharacter.positionOnGrid.y, gridPosition.x, gridPosition.y); // Find path from current to clicked position

                if (fullPath == null || fullPath.Count == 0) { return; } // If no path found, do nothing
                if (fullPath.Count <= maxMoveSpeed) // If path is within move speed
                {
                    trimmedPath = fullPath; // Use full path
                }
                else
                {
                    trimmedPath = fullPath.GetRange(0, maxMoveSpeed); // Otherwise, trim path to move speed
                }

                unitMovement.Move(trimmedPath); // Tell the unit to move along the trimmed path
            }
        }
    }
}
