using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    private CharacterController controller;
    private Animator animator; // 1. Animator reference
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // 2. Get the Animator component
        // Use GetComponentInChildren<Animator>() if your 3D model is nested under this object
        animator = GetComponent<Animator>(); 
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;

        // Ground Check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Read Input
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
        }

        // Move relative to Player's local rotation
        Vector3 moveInput = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(moveInput * moveSpeed * Time.deltaTime);

        // 3. Trigger Walk Animation
        bool isMoving = (moveX != 0 || moveZ != 0);
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }

        // Jump
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}