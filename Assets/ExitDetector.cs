using UnityEngine;

public class ExitDetector : MonoBehaviour
{
    public GameManager gm;

    private void OnTriggerEnter(Collider other)
    {
        if (gm == null) return;

        // Only trigger when the XR player enters
        if (other.CompareTag("Player"))
        {
            gm.CompleteScenario();
        }
    }
}
