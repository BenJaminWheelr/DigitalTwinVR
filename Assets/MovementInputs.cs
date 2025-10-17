using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementInputs : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouse look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate whole camera for yaw (left/right)
        transform.Rotate(Vector3.up * mouseX);

        // Apply pitch (up/down) directly to local rotation
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        // Overwrite local rotation (up/down) while keeping yaw
        Quaternion yaw = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = yaw * Quaternion.Euler(pitch, 0, 0);

        // --- Movement (WASD) ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized;

        controller.Move(move * speed * Time.deltaTime);
    }
}
