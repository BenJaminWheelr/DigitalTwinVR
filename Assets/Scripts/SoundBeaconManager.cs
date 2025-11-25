using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    [Header("Sound Beacon GameObjects")]
    public List<SoundBeacon> beacons;

    public Button soundBeaconButton;

    public Color greenColor = new Color(0f, 1f, 0f, 1f); // active
    public Color redColor = new Color(1f, 0f, 0f, 1f);   // inactive
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f; // darken by 20% when hovered

    public AudioSource clickAudioSource;

    private SoundBeacon activeBeacon = null;

    private void Start()
    {
        UpdateButtonColors(false);
    }

    public void ToggleSound()
    {
        // Play click
        if (clickAudioSource != null)
            clickAudioSource.Play();

        // If a beacon is currently active, deactivate it
        if (activeBeacon != null)
        {
            activeBeacon.SetActive(false);
            activeBeacon = null;
            UpdateButtonColors(false);
            return;
        }

        // Pick a random beacon to activate
        if (beacons != null && beacons.Count > 0)
        {
            int index = Random.Range(0, beacons.Count);
            activeBeacon = beacons[index];
            activeBeacon.SetActive(true);
            UpdateButtonColors(true);
        }
    }

    private void UpdateButtonColors(bool isActive)
    {
        Color normal = isActive ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier;
        Color pressed = normal * 0.6f;

        ColorBlock cb = soundBeaconButton.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f;
        soundBeaconButton.colors = cb;
    }
}
