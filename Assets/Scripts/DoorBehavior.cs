using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform door;        // The part of the door to rotate
    public float openAngle = 90f; // How much the door opens (degrees)
    public float openSpeed = 2f;  // How fast the door opens/closes

    [Header("Player Detection")]
    public string playerTag = "Player"; // Tag of the XR rig or player collider

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen = false;
    private bool isOnFire = false;

    void Start()
    {
        // Store the initial rotation
        closedRotation = door.localRotation;
        // Open rotation around Z axis
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isOnFire)
        {
            if (other.CompareTag(playerTag))
            {
                isOpen = true;
            }
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isOpen = false;
        }
    }

    public bool getFireStatus()
    {
        return isOnFire;
    }

    public void setFireStatus(bool fireStatus)
    {
        isOnFire = fireStatus;
    }

    void Update()
    {
        // Smoothly rotate towards the target rotation
        door.localRotation = Quaternion.Slerp(
            door.localRotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * openSpeed
        );
    }
}
