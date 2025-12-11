using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireCellManager : MonoBehaviour
{
    [Header("Single doors to manage fire")]
    public List<DoorBehavior> singleDoors;

    [Header("Double doors to manage fire")]
    public List<GameObject> doubleDoorParents;

    [Header("Fire Settings")]
    public float fireCheckInterval = 10f; // seconds between fire checks
    [Range(0f, 1f)]
    public float fireChance = 0.25f; // 25% chance

    [Header("Fog Control")]
    public float fogIncreasePerSecond = 0.01f;
    public float maxFogDensity = 0.15f;
    public Color fogColor = new Color(0.15f, 0.15f, 0.15f, 1f); // Dark gray default

    private Coroutine fogRoutine;


    public TMP_Text fireStatusText;
    public TMP_Text fogStatusText;
    public TMP_Text fogIncreaseText;
    public TMP_Text fireSpreadText;
    public TMP_Text fireChanceText;
    public Slider fireSpreadSlider;
    public Slider fireChanceSlider;
    public Slider fogSlider;
    public Slider fogIncreaseSlider;

    public Color greenColor = new Color(0f, 1f, 0f, 1f);
    public Color redColor = new Color(1f, 0f, 0f, 1f);
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f;

    private bool isActive = false;
    private bool isFireStarted = false;
    private Coroutine fireRoutine;

    public AudioSource clickAudioSource;

    public Transform player; // assign VR camera or player transform here

    private IEnumerator IncreaseFogOverTime()
    {
        while (true)
        {
            if (!isFireStarted)
                yield break;

            RenderSettings.fog = true;

            RenderSettings.fogDensity = Mathf.Min(
                RenderSettings.fogDensity + fogIncreasePerSecond,
                maxFogDensity
            );

            yield return new WaitForSeconds(1f);
        }
    }

    public void SetMaxFogDensity(float value)
    {
        float rounded = Mathf.Round(value * 100f) / 100f;
        maxFogDensity = rounded;
    }

    public void SetFireCheckInterval(float value)
    {
        fireCheckInterval = value;
    }

    public void SetFireChance(float value)
    {
        value = value / 100f;
        fireChance = value;
    }


    public void SetFogIncreaseRate(float value)
    {
        float rounded = Mathf.Round(value * 100f) / 100f;
        fogIncreasePerSecond = rounded;
    }

    public void StartFire()
    {
        if (isFireStarted) return;

        RenderSettings.fogDensity = 0f;
        isFireStarted = true;
        RenderSettings.fogColor = fogColor;


        fogRoutine = StartCoroutine(IncreaseFogOverTime());
        fireRoutine = StartCoroutine(ManageDoorFires());
    }



    private IEnumerator ManageDoorFires()
    {
        List<DoorBehavior> availableSingles = new List<DoorBehavior>(singleDoors);
        List<GameObject> availableDoubles = new List<GameObject>(doubleDoorParents);

        while (availableSingles.Count + availableDoubles.Count > 2)
        {
            yield return new WaitForSeconds(fireCheckInterval);

            if (Random.value > fireChance)
                continue; // skip this interval based on chance

            // find closest single door
            DoorBehavior closestSingle = null;
            float minDistSingle = float.MaxValue;
            foreach (var door in availableSingles)
            {
                float dist = Vector3.Distance(player.position, door.transform.position);
                if (dist < minDistSingle)
                {
                    minDistSingle = dist;
                    closestSingle = door;
                }
            }

            // find closest double door
            GameObject closestDouble = null;
            float minDistDouble = float.MaxValue;
            foreach (var parent in availableDoubles)
            {
                float dist = Vector3.Distance(player.position, parent.transform.position);
                if (dist < minDistDouble)
                {
                    minDistDouble = dist;
                    closestDouble = parent;
                }
            }

            // choose the overall closest
            if (closestSingle != null && (closestDouble == null || minDistSingle <= minDistDouble))
            {
                // block single door
                closestSingle.setFireStatus(true);
                Transform fireChild = closestSingle.transform.Find("Fire");
                if (fireChild != null)
                    fireChild.gameObject.SetActive(true);
                availableSingles.Remove(closestSingle);
            }
            else if (closestDouble != null)
            {
                DoorBehavior[] doors = closestDouble.GetComponentsInChildren<DoorBehavior>();
                foreach (var door in doors)
                    door.setFireStatus(true);

                Transform fireChild = closestDouble.transform.Find("Fire");
                if (fireChild != null)
                    fireChild.gameObject.SetActive(true);

                availableDoubles.Remove(closestDouble);
            }
        }
    }


    void Update()
    {
        int blockedDoors = 0;

        foreach (var door in singleDoors)
            if (door.getFireStatus()) blockedDoors++;

        foreach (var parent in doubleDoorParents)
        {
            DoorBehavior[] doors = parent.GetComponentsInChildren<DoorBehavior>();
            if (doors.Length > 0 && doors[0].getFireStatus())
                blockedDoors++;
        }

        if (fireStatusText != null)
            fireStatusText.text = $"Blocked Exits: {blockedDoors}/{singleDoors.Count + doubleDoorParents.Count}";

        if (fogStatusText != null)
            fogStatusText.text = $"Fog Density: {RenderSettings.fogDensity:F2} / {maxFogDensity:F2}";

        if (fogIncreaseText != null)
            fogIncreaseText.text = $"Increase Rate: {fogIncreasePerSecond:F2} /s";

        if (fireSpreadText != null)
            fireSpreadText.text = $"Fire Spread Rate: {fireCheckInterval:F2} /s";

        if (fireChanceText != null)
            fireChanceText.text = $"Fire Spread Chance: {fireChance * 100}%";

    }

    private void refreshUI()
    {
        fogSlider.value = maxFogDensity;        // sync UI to variable
        SetMaxFogDensity(fogSlider.value);      // enforce rounding + refresh

        fogIncreaseSlider.value = fogIncreasePerSecond;
        SetFogIncreaseRate(fogIncreaseSlider.value);

        fireSpreadSlider.value = fireCheckInterval;
        SetFireCheckInterval(fireSpreadSlider.value);

        fireChanceSlider.value = fireChance;
        SetFireChance(fireChanceSlider.value);
    }

    private void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0f;

        refreshUI();

        // deactivate all single doors
        foreach (var door in singleDoors)
        {
            if (door == null) continue;
            Transform fireChild = door.transform.Find("Fire");
            if (fireChild != null) fireChild.gameObject.SetActive(false);
            door.setFireStatus(false);
        }

        // deactivate all double doors
        foreach (var parent in doubleDoorParents)
        {
            if (parent == null) continue;
            Transform fireChild = parent.transform.Find("Fire");
            if (fireChild != null)
                fireChild.gameObject.SetActive(false);

            DoorBehavior[] doors = parent.GetComponentsInChildren<DoorBehavior>();
            foreach (var door in doors)
                door.setFireStatus(false);
        }
    }

    public void ResetEnvironment()
    {
        Debug.Log("RESET ENVIRONMENT");

        // STOP FIRE
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }

        // STOP FOG
        if (fogRoutine != null)
        {
            StopCoroutine(fogRoutine);
            fogRoutine = null;
        }

        isFireStarted = false;

        // UNBLOCK SINGLE
        foreach (var door in singleDoors)
        {
            if (door == null) continue;

            door.setFireStatus(false);

            Transform fireChild = door.transform.Find("Fire");
            if (fireChild != null)
                fireChild.gameObject.SetActive(false);
        }

        // UNBLOCK DOUBLE
        foreach (var parent in doubleDoorParents)
        {
            if (parent == null) continue;

            DoorBehavior[] doors = parent.GetComponentsInChildren<DoorBehavior>();
            foreach (var door in doors)
                door.setFireStatus(false);

            Transform fireChild = parent.transform.Find("Fire");
            if (fireChild != null)
                fireChild.gameObject.SetActive(false);
        }

        // CLEAR FOG
        RenderSettings.fogDensity = 0f;
        RenderSettings.fog = true;

        Debug.Log("ENVIRONMENT RESET COMPLETE");
    }


}
