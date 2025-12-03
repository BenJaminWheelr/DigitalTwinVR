using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public SpawnManager spawnManager;
    public FireAlarmManager fireSystem;
    public FireCellManager fireCellManager;
    public float delay = 5f;
    private int spawnIndex;
    public void StartScenario()
    {
        spawnIndex = Random.Range(0, spawnManager.spawnPoints.Length);
        spawnManager.TeleportToSpawn(spawnIndex);
        StartCoroutine(DelayedFire());
    }

    IEnumerator DelayedFire()
    {
        yield return new WaitForSeconds(delay);
        fireSystem.ToggleAlarms();
        fireCellManager.StartFire();
    }

    public void CompleteScenario()
    {
        fireSystem.ToggleAlarms(); // Turns off the firealarms and stops timer
        fireCellManager.StartFire();
        float evacTime = fireSystem.GetTime();
        SaveTime(evacTime);
    }

    void SaveTime(float time)
    {
        if (spawnIndex < 0) return;

        string key = $"Spawn_{spawnIndex}_Time";
        PlayerPrefs.SetFloat(key, time);
        PlayerPrefs.Save();

        Debug.Log("Saved time for spawn " + spawnIndex + ": " + time);
    }

}
