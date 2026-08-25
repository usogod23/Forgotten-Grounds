using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openAngle = -90f; // Cât de mult se deschide ușa
    public float closeAngle = 0f; // Poziția închisă
    public float smoothSpeed = 3f; // Viteza de deschidere

    private bool isOpen = false;
    private bool playerIsNear = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Awake()
    {
        // Păstrăm orientarea din scenă, ca același controller să funcționeze
        // pe uși amplasate în direcții diferite.
        Quaternion initialRotation = transform.localRotation;
        closedRotation = initialRotation * Quaternion.Euler(0f, closeAngle, 0f);
        openRotation = initialRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        // Verificăm dacă jucătorul e lângă ușă și apasă tasta E
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen; // Schimbă din deschis în închis și invers
        }

        // Calculăm cum trebuie să stea ușa acum, relativ la rotația ei inițială.
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

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
