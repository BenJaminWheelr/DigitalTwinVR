using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class FireAlarmManager : MonoBehaviour
{
    [Header("Alarm Setup")]
    public Transform parentFolder;         // folder containing all alarms
    public Transform player;               // VR camera / player position
    public float maxAlarmDistance = 10f;   // only alarms within this distance flash
    public float distanceCheckInterval = 0.25f; // how often to check player distance

    [Header("UI")]
    public Button fireAlarmButton;
    public TMP_Text timerText;

    [Header("Fog Settings")]
    [Range(0f, 1f)]
    public float maxFogDensity = 0.05f;
    public float fogPercentPerSecond = 0.01f;

    [Header("Button Colors")]
    public Color greenColor = new Color(0f, 1f, 0f, 1f);
    public Color redColor = new Color(1f, 0f, 0f, 1f);
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f;

    [Header("Audio")]
    public AudioSource clickAudioSource;

    private List<FireAlarmFlasher> alarms = new List<FireAlarmFlasher>();
    private bool isActive = false;
    private float timer = 0f;
    private Coroutine timerRoutine;
    private Coroutine fogRoutine;
    private Coroutine distanceRoutine;

    void Start()
    {
        // Gather all alarms under parent
        alarms.AddRange(parentFolder.GetComponentsInChildren<FireAlarmFlasher>());

        // UI initial state
        UpdateButtonColors(false);
        timerText.text = "Timer: 0:00";

        // Initialize fog
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0f;
    }

    public void ToggleAlarms()
    {
        if (clickAudioSource != null) clickAudioSource.Play();

        isActive = !isActive;
        UpdateButtonColors(isActive);

        if (isActive)
        {
            timer = 0f;
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(TimerRoutine());

            if (fogRoutine != null) StopCoroutine(fogRoutine);
            fogRoutine = StartCoroutine(FogRoutine(true));

            if (distanceRoutine != null) StopCoroutine(distanceRoutine);
            distanceRoutine = StartCoroutine(DistanceCheckRoutine());
        }
        else
        {
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = null;

            if (fogRoutine != null) StopCoroutine(fogRoutine);
            fogRoutine = StartCoroutine(FogRoutine(false));

            if (distanceRoutine != null) StopCoroutine(distanceRoutine);
            distanceRoutine = null;

            // Deactivate all alarms
            foreach (var alarm in alarms)
                if (alarm != null) alarm.Deactivate();
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (true)
        {
            timer += Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"Timer: {minutes}:{seconds:00}";
            yield return null;
        }
    }

    public float GetTime()
    {
        return timer;
    }

    private IEnumerator FogRoutine(bool increasing)
    {
        float target = increasing ? maxFogDensity : 0f;

        while (increasing ? RenderSettings.fogDensity < target : RenderSettings.fogDensity > target)
        {
            float delta = fogPercentPerSecond * Time.deltaTime;
            if (increasing)
                RenderSettings.fogDensity = Mathf.Min(RenderSettings.fogDensity + delta, target);
            else
                RenderSettings.fogDensity = Mathf.Max(RenderSettings.fogDensity - delta, target);

            yield return null;
        }
    }

    private IEnumerator DistanceCheckRoutine()
    {
        while (isActive)
        {
            foreach (var alarm in alarms)
            {
                if (alarm == null) continue;

                float distance = Vector3.Distance(player.position, alarm.transform.position);

                if (distance <= maxAlarmDistance)
                {
                    alarm.Activate();
                }
                else
                {
                    alarm.Deactivate();
                }
            }
            yield return new WaitForSeconds(distanceCheckInterval);
        }
    }

    private void UpdateButtonColors(bool active)
    {
        Color normal = active ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier;
        Color pressed = normal * 0.6f;

        ColorBlock cb = fireAlarmButton.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f;
        fireAlarmButton.colors = cb;
    }
}
