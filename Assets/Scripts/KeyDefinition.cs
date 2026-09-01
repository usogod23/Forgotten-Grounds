using System;
using UnityEngine;

/// <summary>
/// Defines one type of key that can be shared by pickups and locked doors.
/// Using the same asset on both objects creates a type-safe key-to-door link.
/// </summary>
[CreateAssetMenu(
    fileName = "KeyDefinition",
    menuName = "Horror Game/Inventory/Key Definition")]
public sealed class KeyDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    [Tooltip("Stable, unique ID. Keep it unchanged if this key is later added to save data.")]
    private string keyId = string.Empty;

    [SerializeField]
    [Tooltip("Player-facing name used in inventory and door feedback messages.")]
    private string displayName = "Key";

    [SerializeField]
    [Tooltip("Optional icon reserved for inventory UI.")]
    private Sprite icon;

    /// <summary>Stable identifier suitable for save data.</summary>
    public string KeyId => keyId;

    /// <summary>Name shown to the player, with the asset name as a safe fallback.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    /// <summary>Optional icon for a future visual inventory.</summary>
    public Sprite Icon => icon;

    /// <summary>
    /// Compares key types. A direct asset reference is preferred; matching non-empty
    /// IDs also allows the same logical key to be represented by another asset later.
    /// </summary>
    public bool Matches(KeyDefinition other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(keyId)
            && !string.IsNullOrWhiteSpace(other.keyId)
            && string.Equals(keyId, other.keyId, StringComparison.Ordinal);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Trimming prevents invisible whitespace from producing different key IDs.
        keyId = keyId == null ? string.Empty : keyId.Trim();
        displayName = displayName == null ? string.Empty : displayName.Trim();
    }
#endif
}
