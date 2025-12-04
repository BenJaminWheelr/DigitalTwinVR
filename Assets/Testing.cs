using UnityEngine;
using UnityEngine.XR;

public class Testing : MonoBehaviour
{
    public Transform xrRig;          // XR Origin ROOT
    public Transform cameraOffset;   // Camera Offset object
    public Transform target;         // Spawn point

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Teleport();
        }
    }

    public void Teleport()
    {
        // calculate offset between head and rig
        Vector3 headOffset = Camera.main.transform.position - xrRig.position;

        // move rig so head lands exactly on target
        xrRig.position = target.position - headOffset;

        // force physics reset (prevents snap-back)
        Physics.SyncTransforms();

        Debug.Log("FORCE TELEPORT TO " + target.position);
    }
}
