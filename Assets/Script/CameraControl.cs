using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float inputSensitivity = 1f;
    [SerializeField] private InputAction cameraControl;
    private Vector3 input;

    private void OnEnable() => cameraControl.Enable();
    private void OnDisable() => cameraControl.Disable();

    private void Update()
    {
        ReadInput();
        MoveCamera();
    }

    private void ReadInput()
    {
        input = cameraControl.ReadValue<Vector3>() * inputSensitivity;
    }

    private void MoveCamera()
    {
        transform.position += input * Time.deltaTime;
    }
}
