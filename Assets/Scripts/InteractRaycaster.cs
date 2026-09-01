using UnityEngine;

/// <summary>
/// Handles the player's existing centre-screen interaction ray. Pickups and doors
/// are resolved from the collider or one of its parents, so nested prefabs work too.
/// </summary>
public class InteractRaycaster : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Existing popup used for pickup and locked-door feedback messages.")]
    public PopupController popupController;

    [Tooltip("Existing interaction prompt shown while looking at a pickup.")]
    public GameObject pickupText;

    [Tooltip("Existing interaction prompt shown while looking at a door.")]
    public GameObject doorText;

    [Header("Raycast")]
    [Min(0f)]
    [Tooltip("Maximum distance from the camera at which an object can be used.")]
    public float rayLength = 4f;

    private Inventory inventory;
    private ClueManager clueManager;

    private void Awake()
    {
        inventory = GetComponentInParent<Inventory>();
        clueManager = GetComponentInParent<ClueManager>();
    }

    private void Update()
    {
        bool hasHit = Physics.Raycast(
            transform.position,
            transform.forward,
            out RaycastHit hitInfo,
            rayLength);

        UpdateInteractionPrompts(hasHit, hitInfo);

        if (hasHit && Input.GetKeyDown(KeyCode.E))
        {
            InteractWith(hitInfo.collider);
        }
    }

    private void UpdateInteractionPrompts(bool hasHit, RaycastHit hitInfo)
    {
        bool isPickup = false;
        bool isDoor = false;

        if (hasHit)
        {
            Collider hitCollider = hitInfo.collider;
            isPickup = hitCollider.GetComponentInParent<KeyPickup>() != null
                || hitCollider.CompareTag("Clue")
                || hitCollider.CompareTag("Battery")
                || hitCollider.CompareTag("Pill");

            isDoor = hitCollider.GetComponentInParent<DoorController>() != null
                || hitCollider.CompareTag("Door");
        }

        if (pickupText != null)
        {
            pickupText.SetActive(isPickup);
        }

        if (doorText != null)
        {
            doorText.SetActive(isDoor);
        }
    }

    private void InteractWith(Collider hitCollider)
    {
        // Component-driven key pickup detection avoids adding another project tag.
        KeyPickup keyPickup = hitCollider.GetComponentInParent<KeyPickup>();
        if (keyPickup != null)
        {
            if (keyPickup.TryCollect(inventory, out string pickupMessage))
            {
                ShowPopup(pickupMessage);
            }

            return;
        }

        // The existing door prefabs place DoorController on the root or DoorHinge.
        DoorController door = hitCollider.GetComponentInParent<DoorController>();
        if (door != null)
        {
            door.Interact(inventory, out string doorMessage);
            ShowPopup(doorMessage);
            return;
        }

        if (hitCollider.CompareTag("Clue"))
        {
            CollectClue(hitCollider);
        }
        else if (hitCollider.CompareTag("Battery"))
        {
            CollectBattery(hitCollider);
        }
        else if (hitCollider.CompareTag("Pill"))
        {
            CollectPill(hitCollider);
        }
    }

    private void CollectClue(Collider hitCollider)
    {
        ClueInfo clueInfo = hitCollider.GetComponent<ClueInfo>();
        if (clueInfo != null)
        {
            if (clueManager != null)
            {
                clueManager.AddClue(clueInfo);
            }

            string message = "We found Clue: " + clueInfo.ClueName;
            Debug.Log(message);
            ShowPopup(message);
        }

        hitCollider.gameObject.SetActive(false);
    }

    private void CollectBattery(Collider hitCollider)
    {
        if (inventory == null)
        {
            Debug.LogWarning("The player has no Inventory component.", this);
            return;
        }

        inventory.AddBattery();
        hitCollider.gameObject.SetActive(false);
        ShowPopup("Added battery to inventory");
    }

    private void CollectPill(Collider hitCollider)
    {
        if (inventory == null)
        {
            Debug.LogWarning("The player has no Inventory component.", this);
            return;
        }

        inventory.AddPill();
        hitCollider.gameObject.SetActive(false);
        ShowPopup("Added pill to inventory");
    }

    private void ShowPopup(string message)
    {
        if (popupController != null && !string.IsNullOrWhiteSpace(message))
        {
            popupController.Show(message);
        }
    }
}
