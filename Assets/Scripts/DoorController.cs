using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openAngle = -90f; // Cât de mult se deschide ușa
    public float closeAngle = 0f; // Poziția închisă
    public float smoothSpeed = 3f; // Viteza de deschidere

    private bool isOpen = false;
    private bool playerIsNear = false;

    void Update()
    {
        // Verificăm dacă jucătorul e lângă ușă și apasă tasta E
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen; // Schimbă din deschis în închis și invers
        }

        // Calculăm cum trebuie să stea ușa acum
        float targetAngle = isOpen ? openAngle : closeAngle;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        // Rotim ușa ușor către poziția dorită
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    // Funcția asta se activează când jucătorul intră în zona invizibilă a ușii
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    // Funcția asta se activează când jucătorul pleacă de lângă ușă
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}