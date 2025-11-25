using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FireCellManager : MonoBehaviour
{
    [Header("All floors with VisualizeCells scripts")]
    public List<VisualizeCells> floors;

    public Button cellButton; // drag your Button here in Inspector
    public Button startFireButton;
    public TMP_Text fireStatusText;
    public TMP_Text fogStatusText; 

    public Color greenColor = new Color(0f, 1f, 0f, 1f); // exit signs showing
    public Color redColor = new Color(1f, 0f, 0f, 1f);   // no exit signs
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f; // darken by 20% when hovered

    private bool isActive = false;
    private bool isFireStarted = false;
    // Call this from your button's OnClick

    public AudioSource clickAudioSource;
    public void ToggleAllCells()
    {
        if (clickAudioSource != null) clickAudioSource.Play();
        isActive = !isActive;

        foreach (VisualizeCells floor in floors)
        {
            if (floor != null)
                floor.ToggleCellsVisibility();
        }
        UpdateButtonColors(isActive, cellButton);

    }

    public void StartFire()
    {
        if (isFireStarted) { return; }
        if (clickAudioSource != null) clickAudioSource.Play();
        foreach (VisualizeCells floor in floors)
        {
            StartCoroutine(floor.StartFireRoutine());
        }
        isFireStarted = true;
        UpdateButtonColors(isFireStarted, startFireButton);
    }

    private void UpdateButtonColors(bool isActive, Button buttonToUpdate)
    {
        Color normal = isActive ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier; // makes it darker
        Color pressed = normal * 0.6f; // optional: even darker when pressed

        ColorBlock cb = buttonToUpdate.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal; // stays same as normal
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f; // ensures the color changes are applied
        buttonToUpdate.colors = cb;
    }

    void Update()
    {
        fireStatusText.text = floors[0].getFireStatus();
        fogStatusText.text = floors[0].getFogStatus();
    }

    private void Start()
    {
        // Make sure button reflects current state on scene start
        UpdateButtonColors(isActive, cellButton);
        UpdateButtonColors(isFireStarted, startFireButton);
    }
}
