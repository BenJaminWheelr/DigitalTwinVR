using UnityEngine;

public class uiManager : MonoBehaviour
{
    public GameObject menuPanel;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }
}
