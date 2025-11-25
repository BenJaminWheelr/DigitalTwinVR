using UnityEngine;
using System.Collections;

public class FireAlarmFlasher : MonoBehaviour
{
    public Light light1;
    public Light light2;

    public float flashIntensity = 6f;
    public float flashDuration = 0.08f;
    public float doubleFlashDelay = 0.12f;
    public float pauseBetweenBursts = 0.7f;

    private bool active = false;
    private Coroutine routine;

    void Start()
    {
        // Ensure lights start completely off
        SetLights(0f);
    }

    public void Activate()
    {
        if (active) return;
        active = true;

        // Random initial offset for this alarm (desync across building)
        float randomStartDelay = Random.Range(0f, 0.5f);

        routine = StartCoroutine(StrobeRoutine(randomStartDelay));
    }

    public void Deactivate()
    {
        active = false;
        if (routine != null) StopCoroutine(routine);
        SetLights(0f);
    }

    IEnumerator StrobeRoutine(float initialDelay = 0f)
    {
        // Wait for initial random offset
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (active)
        {
            // Flash 1
            SetLights(flashIntensity);
            yield return new WaitForSeconds(flashDuration);
            SetLights(0f);

            yield return new WaitForSeconds(doubleFlashDelay);

            // Flash 2
            SetLights(flashIntensity);
            yield return new WaitForSeconds(flashDuration);
            SetLights(0f);

            // Pause between bursts
            yield return new WaitForSeconds(pauseBetweenBursts);
        }
    }

    void SetLights(float intensity)
    {
        if (light1 != null) light1.intensity = intensity;
        if (light2 != null) light2.intensity = intensity;
    }
}
