using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float runSpeed = 3f;
    public float rotationSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Animator animator;

    public Transform cameraTransform;

    private Vector3 startPosition;
    private Quaternion startRotation;

    [Header("UI Settings")]
    public GameObject menuPanel;
    public GameObject searchPanel;
    private bool isMenuOpen = false;

    public ThirdPersonMovement cameraScript;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (menuPanel != null) menuPanel.SetActive(false);
        LockCursor(true);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }

        if (!isMenuOpen)
        {
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0f, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && direction.magnitude > 0.1f;
        float currentTargetSpeed = isRunning ? runSpeed : moveSpeed;
        float animSpeed = isRunning ? direction.magnitude * 2f : direction.magnitude;

        animator.SetFloat("Speed", animSpeed);
        animator.SetBool("isGrounded", isGrounded);

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentTargetSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (cameraScript != null) cameraScript.isPaused = isMenuOpen;

        if (isMenuOpen)
        {
            Time.timeScale = 0f;
            LockCursor(false);

            if (menuPanel != null) menuPanel.SetActive(true);
            if (searchPanel != null) searchPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            LockCursor(true);

            if (menuPanel != null) menuPanel.SetActive(false);
            if (searchPanel != null) searchPanel.SetActive(false);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void ResetToStart()
    {
        //ovde gasim kontroller jer dolazi do poklapanja zahteva
        controller.enabled = false;

        //ovde vracam na pocetnu poziciju
        transform.position = startPosition;
        transform.rotation = startRotation;

        //ovde se gasi fizika kako se lik ne bi pomerao
        velocity = Vector3.zero;

        //ovde opet palim kontroler
        controller.enabled = true;

        Debug.Log("Vraceni ste na pocetnu poziciju.");
        ToggleMenu();
    }
    public void TeleportToStaraZgrada()
    {
        controller.enabled = false;
        transform.position = new Vector3(-4.5f, 2.0f, 18.0f);

        velocity = Vector3.zero;
        controller.enabled = true;

        Debug.Log("Stara zgrada.");

        ToggleMenu();
    }
    public void TeleportToNovaZgrada()
    {
        controller.enabled = false;
        transform.position = new Vector3(-0.5f, 0f, 36f);

        velocity = Vector3.zero;
        controller.enabled = true;

        Debug.Log("Nova zgrada.");

        ToggleMenu();
    }

    public void QuitGame()
    {
        Debug.Log("Game is exiting...");

        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
