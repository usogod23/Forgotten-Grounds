using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the existing transform-based door movement and optionally protects
/// the door with a key stored in the player's Inventory.
/// </summary>
public class DoorController : MonoBehaviour
{
    private const string KeyToken = "{KEY}";

    [Header("Movement")]
    [Tooltip("Local Y angle used while the door is open. Use -90 or 90 for opposite hinges.")]
    public float openAngle = -90f;

    [Tooltip("Local Y angle used while the door is closed, relative to its scene rotation.")]
    public float closeAngle = 0f;

    [Min(0f)]
    [Tooltip("How quickly the transform rotates toward its target angle.")]
    public float smoothSpeed = 3f;

    [Header("Lock")]
    [SerializeField]
    [Tooltip("When enabled, the player must carry Required Key before this door can move.")]
    private bool startsLocked;

    [SerializeField]
    [Tooltip("Key accepted by this door. Assign the same KeyDefinition asset to its pickup.")]
    private KeyDefinition requiredKey;

    [SerializeField]
    [Tooltip("When enabled, one matching key is removed after the door is unlocked.")]
    private bool consumeKeyOnUnlock;

    [Header("Messages")]
    [SerializeField]
    [Tooltip("Shown when the player has no matching key. Use {KEY} for its display name.")]
    private string lockedMessage = "Locked. You need {KEY}.";

    [SerializeField]
    [Tooltip("Shown after a successful unlock. Use {KEY} for its display name.")]
    private string unlockedMessage = "Unlocked with {KEY}.";

    [Header("Feedback Events")]
    [SerializeField]
    [Tooltip("Optional hook for a locked handle sound or animation.")]
    private UnityEvent onLockedInteraction = new UnityEvent();

    [SerializeField]
    [Tooltip("Optional hook invoked once when the correct key unlocks the door.")]
    private UnityEvent onUnlocked = new UnityEvent();

    [SerializeField]
    [Tooltip("Optional hook for opening audio, animation, or visual effects.")]
    private UnityEvent onOpened = new UnityEvent();

    [SerializeField]
    [Tooltip("Optional hook for closing audio, animation, or visual effects.")]
    private UnityEvent onClosed = new UnityEvent();

    private bool isOpen;
    private bool isLocked;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    /// <summary>True while the door's target state is open.</summary>
    public bool IsOpen => isOpen;

    /// <summary>True while interaction still requires the configured key.</summary>
    public bool IsLocked => isLocked;

    /// <summary>The key definition currently required by this door.</summary>
    public KeyDefinition RequiredKey => requiredKey;

    private void Awake()
    {
        // Preserve the authored scene orientation so one controller works on every
        // door root as well as on the separate DoorHinge used by the Barn1 demo.
        Quaternion initialRotation = transform.localRotation;
        closedRotation = initialRotation * Quaternion.Euler(0f, closeAngle, 0f);
        openRotation = initialRotation * Quaternion.Euler(0f, openAngle, 0f);
        isLocked = startsLocked;
    }

    private void Update()
    {
        // Input is handled by InteractRaycaster; this component only animates state.
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed);
    }

    /// <summary>
    /// Handles one player interaction. An unlocked door toggles immediately. A
    /// locked door first checks the supplied inventory, unlocks, then opens during
    /// the same interaction. Feedback is returned without coupling this class to UI.
    /// </summary>
    public bool Interact(Inventory inventory, out string feedbackMessage)
    {
        feedbackMessage = string.Empty;

        if (isLocked && !TryUnlockWithInventory(inventory, out feedbackMessage))
        {
            return false;
        }

        ToggleDoor();
        return true;
    }

    /// <summary>Locks the door without changing its open/closed state.</summary>
    public void Lock()
    {
        isLocked = true;
    }

    /// <summary>Unlocks the door without requiring or consuming a key.</summary>
    public void Unlock()
    {
        if (!isLocked)
        {
            return;
        }

        isLocked = false;
        onUnlocked.Invoke();
    }

    /// <summary>Opens the door without performing an inventory check.</summary>
    public void Open()
    {
        SetOpen(true);
    }

    /// <summary>Closes the door without performing an inventory check.</summary>
    public void Close()
    {
        SetOpen(false);
    }

    /// <summary>Toggles the current open/closed target state.</summary>
    public void ToggleDoor()
    {
        SetOpen(!isOpen);
    }

    private bool TryUnlockWithInventory(Inventory inventory, out string feedbackMessage)
    {
        string keyName = requiredKey != null ? requiredKey.DisplayName : "the correct key";

        if (requiredKey == null || inventory == null || !inventory.TryUseKey(requiredKey, consumeKeyOnUnlock))
        {
            feedbackMessage = FormatMessage(lockedMessage, keyName);
            onLockedInteraction.Invoke();

            if (requiredKey == null)
            {
                Debug.LogWarning($"Locked door '{name}' has no Required Key assigned.", this);
            }

            return false;
        }

        isLocked = false;
        feedbackMessage = FormatMessage(unlockedMessage, keyName);
        onUnlocked.Invoke();
        return true;
    }

    private void SetOpen(bool shouldOpen)
    {
        if (isOpen == shouldOpen)
        {
            return;
        }

        isOpen = shouldOpen;
        if (isOpen)
        {
            onOpened.Invoke();
        }
        else
        {
            onClosed.Invoke();
        }
    }

    private static string FormatMessage(string template, string keyName)
    {
        return string.IsNullOrWhiteSpace(template)
            ? string.Empty
            : template.Replace(KeyToken, keyName);
    }
}
