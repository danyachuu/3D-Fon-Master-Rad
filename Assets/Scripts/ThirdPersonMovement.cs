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
    public float smoothSpeed = 15f;

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
        if (isPaused) return;

        yRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Vector3 targetPivotPoint = target.position + pivotOffset;
        smoothedPivotPoint = Vector3.Lerp(smoothedPivotPoint, targetPivotPoint, smoothSpeed * Time.deltaTime);
        Vector3 desiredCameraPos = smoothedPivotPoint - rotation * Vector3.forward * distance;

        RaycastHit hit;
        if (Physics.Linecast(smoothedPivotPoint, desiredCameraPos, out hit, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance - separationCushion, minDistance, distance);
        }
        else
        {
            currentDistance = distance;
        }

        Vector3 finalCameraPosition = smoothedPivotPoint - rotation * Vector3.forward * currentDistance;

        transform.position = finalCameraPosition;
        transform.LookAt(smoothedPivotPoint);
    }
}
