using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public Transform target;

    public Vector3 pivotOffset = new Vector3(0, 3.8f, 0); // height of head
    public float distance = 2f;
    public float mouseSensitivity = 3f;

    [Header("Collision Detalji")]
    public float minDistance = 0.5f;
    public LayerMask collisionLayers;
    public float separationCushion = 0.15f;
    public float cameraRadius = 0.2f;

    [Header("Smooth Damping (Fixes Jitter)")]
    public float smoothTime = 0.08f;
    public float distanceSmoothTime = 0.05f;
    private Vector3 currentVelocity;
    private float distanceVelocity;

    [HideInInspector] public bool isPaused = false;

    float xRotation = 10f;
    float yRotation = 0f;
    float currentDistance;
    private Vector3 smoothedPivotPoint;

    void Start()
    {
        currentDistance = distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            smoothedPivotPoint = target.position + pivotOffset;
        }
    }

    void LateUpdate()
    {
        if (isPaused || target == null) return;


        yRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);

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
}
