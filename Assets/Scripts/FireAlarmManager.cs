using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class FireAlarmManager : MonoBehaviour
{
    public Transform parentFolder; // Assign the folder containing all alarms
    public Button fireAlarmButton;
    public TMP_Text timerText;

    public Color greenColor = new Color(0f, 1f, 0f, 1f); // exit signs showing
    public Color redColor = new Color(1f, 0f, 0f, 1f);   // no exit signs
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f; // darken by 20% when hovered

    private List<FireAlarmFlasher> alarms = new List<FireAlarmFlasher>();

    private bool isActive = false;
    private float timer = 0f;
    private Coroutine timerRoutine;

    public AudioSource clickAudioSource;

    void Start()
    {
        // Automatically gather all alarms under the parent folder
        alarms.AddRange(parentFolder.GetComponentsInChildren<FireAlarmFlasher>());
        UpdateButtonColors(false);
        timerText.text = "Timer: 0:00";
    }

    public void ToggleAlarms()
    {
        if (clickAudioSource != null) clickAudioSource.Play();

        isActive = !isActive;
        UpdateButtonColors(isActive);

        foreach (var alarm in alarms)
        {
            if (isActive) alarm.Activate();
            else alarm.Deactivate();
        }

        if (isActive)
        {
            // Reset and start timer
            timer = 0f;
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = StartCoroutine(TimerRoutine());
        }
        else
        {
            // Stop timer
            if (timerRoutine != null) StopCoroutine(timerRoutine);
            timerRoutine = null;
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

    private void UpdateButtonColors(bool isActive)
    {
        Color normal = isActive ? greenColor : redColor;
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
