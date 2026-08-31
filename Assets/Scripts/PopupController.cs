using System.Collections;
using TMPro;
using UnityEngine;

public class PopupController : MonoBehaviour
{
    public TMP_Text displayText;
    public float displayTime = 2f;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        // 1. Activăm obiectul MAI ÎNTÂI pentru a-i permite să ruleze Coroutine-ul
        gameObject.SetActive(true);
        displayText.text = message;

        // 2. Oprim orice alt timer anterior
        StopAllCoroutines();

        // 3. Pornim temporizatorul asincron pentru ascundere
        StartCoroutine(AutoHidePopup());
    }

    private IEnumerator AutoHidePopup()
    {
        // Așteptăm timpul definit
        yield return new WaitForSeconds(displayTime);

        // Ascundem pergamentul automat
        gameObject.SetActive(false);
    }
}