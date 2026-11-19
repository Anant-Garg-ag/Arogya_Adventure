using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Look Settings")]
    public float mouseSensitivity = 0.1f;
    public Transform playerBody;

    private float xRotation = 0f;

void Start()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;

    if (playerBody == null && transform.parent != null)
    {
        playerBody = transform.parent;
    }

    // Rotate player 90 degrees to the left at startup
    if (playerBody != null)
    {
        playerBody.rotation = Quaternion.Euler(0f, -90f, 0f);
    }
}

    void Update()
    {
        if (Mouse.current == null) return;

        // Read delta from new Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // 1. Pitch (Look Up/Down) - Rotates only the Camera
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f); // Prevents flipping upside down
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. Yaw (Look Left/Right) - Rotates the Player Body
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // Press Escape to free cursor while testing in editor
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}