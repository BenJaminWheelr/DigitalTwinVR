using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
    private float sped = 20.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float rotSens = 3.0f;

    private Vector3 room1TeleportLocation = new Vector3(0, 3, 0);
    private Vector3 room2TeleportLocation = new Vector3(0, 3, -25.22f);


    // Update is called once per frame
    void Update()
    {
        Vector3 position = new Vector3(0, 0 ,0);
        yaw += Input.GetAxis("Mouse X") * rotSens;
        pitch -= Input.GetAxis("Mouse Y") * rotSens;
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // --- WASD Movement ---
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // --- Teleport when pressing 1 ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            position.y += sped * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            position.y -= sped * Time.deltaTime;
        }
        transform.position += position + move * sped * Time.deltaTime;
        

    }
}
