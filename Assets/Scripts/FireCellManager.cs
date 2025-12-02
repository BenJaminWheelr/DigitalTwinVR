using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireCellManager : MonoBehaviour
{
    [Header("All floors with VisualizeCells scripts")]
    public List<VisualizeCells> floors;

    [Header("Single doors to manage fire")]
    public List<DoorBehavior> singleDoors;

    [Header("Double doors to manage fire")]
    public List<GameObject> doubleDoorParents;

    [Header("Fire Settings")]
    public float fireCheckInterval = 10f; // seconds between fire checks
    [Range(0f, 1f)]
    public float fireChance = 0.25f; // 25% chance

    public Button cellButton;
    public Button startFireButton;
    public TMP_Text fireStatusText;
    public TMP_Text fogStatusText;

    public Color greenColor = new Color(0f, 1f, 0f, 1f);
    public Color redColor = new Color(1f, 0f, 0f, 1f);
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f;

    private bool isActive = false;
    private bool isFireStarted = false;
    private Coroutine fireRoutine;

    public AudioSource clickAudioSource;

    public Transform player; // assign VR camera or player transform here

    public void ToggleAllCells()
    {
        if (clickAudioSource != null) clickAudioSource.Play();
        isActive = !isActive;

        foreach (VisualizeCells floor in floors)
            floor?.ToggleCellsVisibility();

        UpdateButtonColors(isActive, cellButton);
    }

    public void StartFire()
    {
        if (isFireStarted) return;
        if (clickAudioSource != null) clickAudioSource.Play();

        isFireStarted = true;
        UpdateButtonColors(isFireStarted, startFireButton);

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

    private void UpdateButtonColors(bool isActive, Button buttonToUpdate)
    {
        Color normal = isActive ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier;
        Color pressed = normal * 0.6f;

        ColorBlock cb = buttonToUpdate.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f;
        buttonToUpdate.colors = cb;
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

        fireStatusText.text = $"Blocked Exits: {blockedDoors}/{singleDoors.Count + doubleDoorParents.Count}";
        if (floors.Count > 0)
            fogStatusText.text = floors[0].getFogStatus();
    }

    private void Start()
    {
        UpdateButtonColors(isActive, cellButton);
        UpdateButtonColors(isFireStarted, startFireButton);

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
}
