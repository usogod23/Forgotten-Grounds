using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Adds a configured key to the player's existing Inventory when the raycaster
/// interacts with this object. No special Unity tag is required.
/// </summary>
public sealed class KeyPickup : MonoBehaviour
{
    private const string KeyToken = "{KEY}";

    [Header("Key")]
    [SerializeField]
    [Tooltip("Key type collected by this pickup. Assign the same asset to its matching door.")]
    private KeyDefinition key;

    [Header("Feedback")]
    [SerializeField]
    [Tooltip("Message shown after pickup. Use {KEY} where the configured key name should appear.")]
    private string pickupMessage = "Picked up {KEY}.";

    [SerializeField]
    [Tooltip("Optional hook for pickup audio, particles, objectives, or other presentation logic.")]
    private UnityEvent onPickedUp = new UnityEvent();

    /// <summary>The key definition assigned to this pickup.</summary>
    public KeyDefinition Key => key;

    /// <summary>
    /// Tries to add the key to an inventory, invokes feedback hooks, and hides the
    /// world object on success. The returned message can be displayed by any UI.
    /// </summary>
    public bool TryCollect(Inventory inventory, out string feedbackMessage)
    {
        feedbackMessage = string.Empty;

        if (key == null)
        {
            Debug.LogWarning($"Key pickup '{name}' has no KeyDefinition assigned.", this);
            return false;
        }

        if (inventory == null)
        {
            Debug.LogWarning($"Key pickup '{name}' could not find the player's Inventory.", this);
            return false;
        }

        if (!inventory.AddKey(key))
        {
            return false;
        }

        feedbackMessage = FormatMessage(pickupMessage, key.DisplayName);
        onPickedUp.Invoke();

        // This mirrors how the existing clue, battery, and pill pickups are collected.
        gameObject.SetActive(false);
        return true;
    }

    private static string FormatMessage(string template, string keyName)
    {
        return string.IsNullOrWhiteSpace(template)
            ? string.Empty
            : template.Replace(KeyToken, keyName);
    }
}
