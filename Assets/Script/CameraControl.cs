using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CameraControl : MonoBehaviour
{
    private CameraMovement cameraActions;
    private InputAction movement;
    private Transform cameraTransform;

    // horizontoal motion
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;
    private float speed;

    // zooming
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private float zoomDampening = 7.5f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private float zoomSpeed = 2f;

    // rotation
    [SerializeField] private float maxRotationSpeed = 1f;

    // values set in various functions
    private Vector3 targetPosition;

    private float zoomHeight;
    private float rotationInput;

    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    
}
