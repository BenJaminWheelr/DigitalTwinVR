using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManage : MonoBehaviour
{
    public List<Transform> spawnPoints = new List<Transform>();
    public Transform xrOrigin;
    public float fireAlarmDelay = 10f;

    public void teleportPlayer(int spawnIndex)
    {
        int index = Random.Range(0, spawnPoints.Count);
        Transform spawn = spawnPoints[index];

        xrOrigin.position = spawn.position;
        xrOrigin.rotation = spawn.rotation;
    }
}
