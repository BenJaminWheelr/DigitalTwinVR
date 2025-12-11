using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public SpawnManage spawnManager;
    public FireAlarmManager fireSystem;
    public FireCellManager fireCellManager;
    public float delay = 5f;
    private int spawnIndex;
    private bool scenarioStarted = false;

    public Button startScenarioButton;
    public AudioSource clickAudioSource;

    public Color greenColor = new Color(0f, 1f, 0f, 1f);
    public Color redColor = new Color(1f, 0f, 0f, 1f);
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f;

    private void UpdateButtonColors(bool active)
    {
        Color normal = active ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier;
        Color pressed = normal * 0.6f;

        ColorBlock cb = startScenarioButton.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f;
        startScenarioButton.colors = cb;
    }
    public void Start()
    {
        UpdateButtonColors(scenarioStarted);

    }
    public void StartScenario()
    {
        // RESET FIRST
        if (clickAudioSource != null) clickAudioSource.Play();
        if (scenarioStarted)
        {
            StopScenario();
            return;
        }
        scenarioStarted = true;
        UpdateButtonColors(scenarioStarted);
        fireCellManager.ResetEnvironment();

        // Teleport player
        spawnIndex = Random.Range(0, spawnManager.spawnPoints.Count);
        spawnManager.teleportPlayer(spawnIndex);

        scenarioStarted = true;

        // Delay fire
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
        if (!scenarioStarted) { return; }
        Debug.Log("SCENARIO HAS BEEN COMPLETED, TOGGLING ALARMS");
        fireSystem.ToggleAlarms(); // Turns off the firealarms and stops timer
        fireCellManager.ResetEnvironment();
        float evacTime = fireSystem.GetTime();
        SaveTime(evacTime);
        scenarioStarted = false;
        UpdateButtonColors(scenarioStarted);


    }

    public void StopScenario()
    {
        fireSystem.ToggleAlarms(); // Turns off the firealarms and stops timer
        fireCellManager.ResetEnvironment();
        scenarioStarted = false;
        UpdateButtonColors(scenarioStarted);
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
