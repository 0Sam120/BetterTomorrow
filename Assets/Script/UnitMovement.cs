using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1000f;
    GridObject m_GridObject;
    List<Vector3> pathWorldPosition;
    AnimationControlller m_AnimationControlller;
    public bool isMoving = false;

    private void Awake()
    {
        m_GridObject = GetComponent<GridObject>();
        m_AnimationControlller = GetComponentInChildren<AnimationControlller>();
    }
    internal void Move(List<PathNode> path)
    {
        if (isMoving) return;

        pathWorldPosition = m_GridObject.targetGrid.ConvertPathNodesToWorldPosition(path);

        Debug.Log("Character is moving");
        m_GridObject.positionOnGrid.x = path[path.Count-1].pos_x;
        m_GridObject.positionOnGrid.y = path[path.Count-1].pos_y;

        RotateTowards();
        m_AnimationControlller.StartMoving();
        isMoving = true;
    }

    private void Update()
    {
        if(pathWorldPosition == null) { return; }
        if(pathWorldPosition.Count == 0) { return; }

        transform.position = Vector3.MoveTowards(transform.position, pathWorldPosition[0], moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, pathWorldPosition[0]) < 0.05f)
        {
            pathWorldPosition.RemoveAt(0);
            if(pathWorldPosition.Count == 0)
            {
                m_AnimationControlller.StopMoving();
                isMoving = false;
            }
            else
            {
                RotateTowards();
            }
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private void RotateTowards()
    {
        Vector3 direction = (pathWorldPosition[0] - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
