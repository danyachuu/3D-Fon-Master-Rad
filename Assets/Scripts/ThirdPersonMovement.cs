using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThirdPersonMovement : MonoBehaviour
{
    public Transform target;

    public Vector3 pivotOffset = new Vector3(0, 3.8f, 0);
    public float distance = 2f;
    public float mouseSensitivity = 3f;
    public float touchSensitivity = 0.15f;

    [Header("Mobile UI References")]
    [Tooltip("Drag your JoystickBackground RectTransform here!")]
    public RectTransform joystickRect;

    [Header("Collision Details")]
    public float minDistance = 0.5f;
    public LayerMask collisionLayers;
    public float separationCushion = 0.15f;
    public float cameraRadius = 0.2f;

    [Header("Smooth Damping")]
    public float smoothTime = 0.08f;
    public float distanceSmoothTime = 0.05f;
    private Vector3 currentVelocity;
    private float distanceVelocity;

    [HideInInspector] public bool isPaused = false;

    float xRotation = 10f;
    float yRotation = 0f;
    float currentDistance;
    private Vector3 smoothedPivotPoint;

    private int activeCameraFingerId = -1;
    private bool isMouseDragging = false;
    private Vector2 lastMousePosition;

    void Start()
    {
        currentDistance = distance;

#if !UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS && !UNITY_WEBGL
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif

        if (target != null)
        {
            smoothedPivotPoint = target.position + pivotOffset;
        }
    }

    void LateUpdate()
    {
        if (isPaused || target == null) return;

        HandleCameraRotationInput();

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Vector3 targetPivotPoint = target.position + pivotOffset;
        smoothedPivotPoint = Vector3.SmoothDamp(smoothedPivotPoint, targetPivotPoint, ref currentVelocity, smoothTime);
        Vector3 desiredCameraPos = smoothedPivotPoint - rotation * Vector3.forward * distance;

        Vector3 rayDirection = (desiredCameraPos - smoothedPivotPoint).normalized;
        float targetDistance = distance;

        RaycastHit hit;
        if (Physics.SphereCast(smoothedPivotPoint, cameraRadius, rayDirection, out hit, distance, collisionLayers))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                targetDistance = Mathf.Clamp(hit.distance - separationCushion, minDistance, distance);
            }
        }
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, distanceSmoothTime);

        Vector3 finalCameraPosition = smoothedPivotPoint - rotation * Vector3.forward * currentDistance;

        transform.position = finalCameraPosition;
        transform.LookAt(smoothedPivotPoint);
    }

    void HandleCameraRotationInput()
    {
        // 1. REAL TOUCH HARDWARE (iOS / Android)
        if (Input.touchCount > 0 && Input.touchSupported && !Application.isEditor)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverJoystick(touch.position) || IsPointerOverUI(touch.fingerId))
                    {
                        continue;
                    }
                    activeCameraFingerId = touch.fingerId;
                }

                if (touch.fingerId == activeCameraFingerId)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        yRotation -= touch.deltaPosition.x * touchSensitivity;
                        xRotation += touch.deltaPosition.y * touchSensitivity;
                        xRotation = Mathf.Clamp(xRotation, -30f, 60f);
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        activeCameraFingerId = -1;
                    }
                }
            }
            return;
        }

        // 2. EDITOR / SIMULATOR / WEBGL MOUSE & TOUCH DRAG
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            bool hitJoystick = IsPointerOverJoystick(mousePos);
            bool hitUI = IsPointerOverUI(-1);

            if (hitJoystick || hitUI)
            {
                isMouseDragging = false;
            }
            else
            {
                isMouseDragging = true;
                lastMousePosition = mousePos; // Store starting point of drag
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDragging = false;
        }

        if (isMouseDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMousePosition;

            // Update rotation using actual screen delta
            yRotation -= delta.x * (touchSensitivity * 0.5f);
            xRotation += delta.y * (touchSensitivity * 0.5f);
            xRotation = Mathf.Clamp(xRotation, -30f, 60f);

            lastMousePosition = currentMousePos;
        }
    }

    bool IsPointerOverJoystick(Vector2 screenPosition)
    {
        if (joystickRect == null) return false;

        Canvas parentCanvas = joystickRect.GetComponentInParent<Canvas>();
        Camera guiCamera = (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera) ? parentCanvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(joystickRect, screenPosition, guiCamera);
    }

    bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(pointerId) || EventSystem.current.IsPointerOverGameObject();
    }
}