using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public SpawnManage spawnManager;
    public FireAlarmManager fireSystem;
    public FireCellManager fireCellManager;
    public float delay = 5f;
    private int spawnIndex;
    private bool scenarioStarted;
    public void StartScenario()
    {
        spawnIndex = Random.Range(0, spawnManager.spawnPoints.Count);
        spawnManager.teleportPlayer(spawnIndex);
        scenarioStarted = true;
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
        //if (!scenarioStarted) { return; }
        fireSystem.ToggleAlarms(); // Turns off the firealarms and stops timer
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
