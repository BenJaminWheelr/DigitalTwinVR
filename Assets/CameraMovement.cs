using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float rotSens = 3.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Rotationwwwww
        yaw += Input.GetAxis("Mouse X") * rotSens;
        pitch -= Input.GetAxis("Mouse Y") * rotSens;
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // Movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Vertical (jump/fly) movement
        if (Input.GetKey(KeyCode.Space))
            move.y += 1;
        if (Input.GetKey(KeyCode.LeftControl))
            move.y -= 1;

        // Apply movement with collision
        controller.Move(move * speed * Time.deltaTime);
    }
}
