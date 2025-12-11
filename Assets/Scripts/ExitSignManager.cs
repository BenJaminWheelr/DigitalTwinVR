using UnityEngine;
using UnityEngine.UI;

public class ExitSignManager : MonoBehaviour
{
    public GameObject exitSignParent;
    public Button exitButton; // drag your Button here in Inspector

    public Color greenColor = new Color(0f, 1f, 0f, 1f); // exit signs showing
    public Color redColor = new Color(1f, 0f, 0f, 1f);   // no exit signs
    [Range(0f, 1f)]
    public float highlightMultiplier = 0.8f; // darken by 20% when hovered

    public AudioSource clickAudioSource;
    public bool GetExitSignState()
    {
        return exitSignParent.activeSelf;
    }
    public void ToggleVisibility()
    {
        if (clickAudioSource != null) clickAudioSource.Play();
        bool isActive = !exitSignParent.activeSelf;
        exitSignParent.SetActive(isActive);

        UpdateButtonColors(isActive);
    }

    private void UpdateButtonColors(bool hasExitSigns)
    {
        Color normal = hasExitSigns ? greenColor : redColor;
        Color highlighted = normal * highlightMultiplier; // makes it darker
        Color pressed = normal * 0.6f; // optional: even darker when pressed

        ColorBlock cb = exitButton.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal; // stays same as normal
        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f; // ensures the color changes are applied
        exitButton.colors = cb;
    }

    private void Start()
    {
        // Make sure button reflects current state on scene start
        UpdateButtonColors(exitSignParent.activeSelf);
    }
}
