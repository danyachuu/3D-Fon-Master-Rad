using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public Transform target;

    public Vector3 pivotOffset = new Vector3(0, 1.6f, 0); // height of head
    public float distance = 2f;
    public float mouseSensitivity = 3f;

    [Header("Collision Detalji")]
    public float minDistance = 0.5f;
    public LayerMask collisionLayers;

    [HideInInspector] public bool isPaused = false;

    float xRotation = 10f;
    float yRotation = 0f;
    float currentDistance;

    void Start()
    {
        currentDistance = distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (isPaused) return;

        yRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -30f, 60f);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);

        Vector3 pivotPoint = target.position + pivotOffset;

        Vector3 desiredCameraPos = pivotPoint - rotation * Vector3.forward * distance;
        RaycastHit hit;
        if (Physics.Linecast(pivotPoint, desiredCameraPos, out hit, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance * 0.9f, minDistance, distance);
        }
        else
        {
            currentDistance = distance;
        }

        Vector3 finalCameraPosition = pivotPoint - rotation * Vector3.forward * currentDistance;

        transform.position = finalCameraPosition;
        transform.LookAt(pivotPoint);
    }
}
