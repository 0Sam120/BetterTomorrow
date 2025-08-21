using UnityEngine;
using System.Collections;

public class CameraHelper : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Camera cinematicCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);

    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool smoothTransitions = true;

    [SerializeField] private bool showDebugGizmos = false;

    public Transform attachedUnit;

    public static CameraHelper Instance { get; private set; }

    // Store original camera state
    private struct CameraState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float fieldOfView;

        public CameraState(Transform transform, float fov)
        {
            position = transform.position;
            rotation = transform.rotation;
            fieldOfView = fov;
        }
    }

    private CameraState originalCameraState;
    private CameraState lastCameraState;
    private bool isTransitioning = false;
    private Coroutine currentTransition;

    // Camera control reference (assuming it exists somewhere)
    private CameraControl cameraControl => CameraControl.Instance;

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
    }

    public void SetCameraControl(bool enabled)
    {
        Debug.Log("SetCameraControl was called!");
        if (cameraControl != null)
        {
            Debug.Log($"Before: {cameraControl.enabled}");
            cameraControl.enabled = enabled;
            Debug.Log($"After: {cameraControl.enabled}");

            // Check again next frame
            StartCoroutine(CheckNextFrame());
        }
    }

    IEnumerator CheckNextFrame()
    {
        yield return null; // Wait one frame
        Debug.Log($"Next frame: {cameraControl.enabled}");
    }

    public void ToggleCinematicCamera()
    {
        cinematicCamera.transform.position = gameCamera.transform.position;
        cinematicCamera.transform.rotation = gameCamera.transform.rotation;
        cinematicCamera.fieldOfView = gameCamera.fieldOfView;

        gameCamera.enabled = !gameCamera.enabled;
    }

    public void ToggleGameCamera()
    {
        gameCamera.enabled = enabled;
    }

    public void SetCameraForAttack(GridObject target, bool useSmoothing = true)
    {
        if (gameCamera == null || target == null) return;

        // Store current state before changing
        lastCameraState = new CameraState(cinematicCamera.transform, cinematicCamera.fieldOfView);

        Vector3 targetPosition = attachedUnit.transform.position + offset;
        Quaternion targetRotation = Quaternion.LookRotation((target.transform.position - targetPosition).normalized);

        if (useSmoothing && smoothTransitions)
        {
            StartCameraTransition(targetPosition, targetRotation, cinematicCamera.fieldOfView);
        }
        else
        {
            SetCameraImmediate(targetPosition, targetRotation);
        }
    }

    public void SetCameraForAttack(Vector3 targetWorldPosition, bool useSmoothing = true)
    {
        if (gameCamera == null) return;

        lastCameraState = new CameraState(gameCamera.transform, gameCamera.fieldOfView);

        Vector3 cameraPosition = attachedUnit.transform.position + offset;
        Quaternion targetRotation = Quaternion.LookRotation((targetWorldPosition - cameraPosition).normalized);

        if (useSmoothing && smoothTransitions)
        {
            StartCameraTransition(cameraPosition, targetRotation, gameCamera.fieldOfView);
        }
        else
        {
            SetCameraImmediate(cameraPosition, targetRotation);
        }
    }

    public void ResetCameraPosition(bool useSmoothing = true)
    {
        if (gameCamera == null) return;

        if (useSmoothing && smoothTransitions)
        {
            StartCameraTransition(lastCameraState.position, lastCameraState.rotation, lastCameraState.fieldOfView);
        }
        else
        {
            SetCameraImmediate(lastCameraState.position, lastCameraState.rotation);
            gameCamera.fieldOfView = lastCameraState.fieldOfView;
        }
    }

    public void ResetToOriginal(bool useSmoothing = true)
    {
        if (gameCamera == null) return;

        if (useSmoothing && smoothTransitions)
        {
            StartCameraTransition(originalCameraState.position, originalCameraState.rotation, originalCameraState.fieldOfView);
        }
        else
        {
            SetCameraImmediate(originalCameraState.position, originalCameraState.rotation);
            gameCamera.fieldOfView = originalCameraState.fieldOfView;
        }
    }

    private void SetCameraImmediate(Vector3 position, Quaternion rotation)
    {
        gameCamera.transform.position = position;
        gameCamera.transform.rotation = rotation;
    }

    private void StartCameraTransition(Vector3 targetPosition, Quaternion targetRotation, float targetFOV)
    {
        // Stop any existing transition
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(AnimateCamera(targetPosition, targetRotation, targetFOV));
    }

    private IEnumerator AnimateCamera(Vector3 targetPosition, Quaternion targetRotation, float targetFOV)
    {
        isTransitioning = true;

        Vector3 startPosition = cinematicCamera.transform.position;
        Quaternion startRotation = cinematicCamera.transform.rotation;
        float startFOV = cinematicCamera.fieldOfView;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);

            cinematicCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            cinematicCamera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);
            cinematicCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curveValue);

            yield return null;
        }

        // Ensure final values are exact
        cinematicCamera.transform.position = targetPosition;
        cinematicCamera.transform.rotation = targetRotation;
        cinematicCamera.fieldOfView = targetFOV;

        isTransitioning = false;
        currentTransition = null;
    }

    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration);
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    // Properties
    public bool IsTransitioning => isTransitioning;
    public float TransitionDuration => transitionDuration;

    // Event for when transitions complete
    public System.Action OnTransitionComplete;

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || gameCamera == null) return;

        // Draw camera frustum
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(gameCamera.transform.position, cinematicCamera.transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, gameCamera.fieldOfView, gameCamera.farClipPlane, gameCamera.nearClipPlane, gameCamera.aspect);

        // Draw offset visualization if target is set
        if (attachedUnit != null)
        {
            Gizmos.color = Color.green;
            Vector3 targetPos = attachedUnit.position + offset;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            Gizmos.DrawLine(attachedUnit.position, targetPos);
        }
    }
}

