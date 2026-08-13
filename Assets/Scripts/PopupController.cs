using TMPro;
using UnityEngine;

public class PopupController : MonoBehaviour
{
    public TMP_Text displayText;

    public void Show(string message)
    {
        gameObject.SetActive(true);
        displayText.text = message;
    }
}
