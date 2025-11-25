using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundBeacon : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Visual Indicator")]
    public GameObject visualPrefab; // assign a prefab in inspector
    private GameObject visualInstance;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (visualPrefab != null)
        {
            // Instantiate but disable initially
            visualInstance = Instantiate(visualPrefab, transform);
            visualInstance.SetActive(false);
        }
    }

    // Call this to turn the beacon on/off
    public void SetActive(bool active)
    {
        if (active)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
            if (visualInstance != null)
                visualInstance.SetActive(true);
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            if (visualInstance != null)
                visualInstance.SetActive(false);
        }
    }
}
