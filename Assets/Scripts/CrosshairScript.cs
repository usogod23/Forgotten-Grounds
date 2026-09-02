using UnityEngine;

public class CrosshairScript : MonoBehaviour
{
    public GameObject SaveMenu;
    public GameObject ClueMenu;
    public GameObject PauseMenu;
    public GameObject StartMenu;

    public GameObject CrosshairVisual;

    void Update()
    {
        bool anyMenuOpen = SaveMenu.activeSelf || ClueMenu.activeSelf || PauseMenu.activeSelf || StartMenu.activeSelf;
        CrosshairVisual.SetActive(!anyMenuOpen);
    }
}