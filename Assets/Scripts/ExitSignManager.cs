using UnityEngine;

public class ExitSignManager : MonoBehaviour
{
    public GameObject exitSignParent;

    public void toggleVisibility()
    {
        exitSignParent.SetActive(!exitSignParent.activeSelf);
    }
}
