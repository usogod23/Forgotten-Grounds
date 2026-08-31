using UnityEngine;

public class CrosshairScript : MonoBehaviour
{
    public GameObject Popup;
    public GameObject SaveMenu;
    public GameObject ClueMenu;
    public GameObject PauseMenu;
    public GameObject StartMenu;

    public GameObject CrosshairVisual;

    void Update()
    {
        bool anyMenuOpen = Popup.activeSelf || SaveMenu.activeSelf || ClueMenu.activeSelf || PauseMenu.activeSelf || StartMenu.activeSelf;
        CrosshairVisual.SetActive(!anyMenuOpen);
    }
}