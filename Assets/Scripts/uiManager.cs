using UnityEngine;

public class uiManager : MonoBehaviour
{
    public GameObject menuPanel;
    private bool uiOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiOpen = !uiOpen;
            menuPanel.SetActive(uiOpen);

            if (uiOpen)
            {
                // Unlock cursor so UI can be clicked
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // Lock cursor again for camera movement
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Optional helper so other scripts can check if UI is open
    public bool IsUIOpen()
    {
        return uiOpen;
    }
}
