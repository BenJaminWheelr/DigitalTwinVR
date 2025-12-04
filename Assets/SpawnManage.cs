using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using static UnityEngine.GraphicsBuffer;

public class SpawnManage : MonoBehaviour
{
    public Transform xrOrigin;       // XR Origin root
    private Transform cameraOffset;  // Will reference Camera Offset
    public List<Transform> spawnPoints;

    void Awake()
    {
        // Find the Camera Offset child at runtime
        cameraOffset = xrOrigin.Find("Camera Offset");
    }


    public void teleportPlayer(int spawnIndex)
    {
        Transform spawn = spawnPoints[spawnIndex];

        Vector3 headOffset = Camera.main.transform.position - xrOrigin.position;

        xrOrigin.position = spawn.position - headOffset;

        Physics.SyncTransforms();

        Debug.Log("FORCE TELEPORT TO " + spawn.position);
    }

}
