using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Stores the player's existing consumables and the new data-driven key items.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Existing popup used when the inventory is inspected with Tab.")]
    public PopupController popupController;

    [SerializeField]
    [Tooltip("Text displaying the current battery count.")]
    private TMP_Text batteryText;

    [SerializeField]
    [Tooltip("Text displaying the current pill count.")]
    private TMP_Text pillText;

    [Header("Keys")]
    [SerializeField]
    [Tooltip("Keys currently carried by the player. Duplicate entries represent multiple copies.")]
    private List<KeyDefinition> keys = new List<KeyDefinition>();

    private static int flashlight = 1;
    private int batteries;
    private int pills;

    private void Start()
    {
        RefreshConsumableUi();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            string inventorySummary = BuildInventorySummary();
            Debug.Log(inventorySummary);

            if (popupController != null)
            {
                popupController.Show(inventorySummary);
            }
        }
    }

    /// <summary>Adds one battery and refreshes the existing HUD counter.</summary>
    public void AddBattery()
    {
        batteries += 1;
        RefreshBatteryUi();
    }

    /// <summary>Returns the number of batteries currently carried.</summary>
    public int GetBattery()
    {
        return batteries;
    }

    /// <summary>Consumes one battery when one is available.</summary>
    public void UseBattery()
    {
        if (batteries > 0)
        {
            batteries -= 1;
            RefreshBatteryUi();
        }
    }

    /// <summary>Restores the battery count from save data.</summary>
    public void SetBatteries(int value)
    {
        batteries = Mathf.Max(0, value);
        RefreshBatteryUi();
    }

    /// <summary>Restores the pill count from save data.</summary>
    public void SetPills(int value)
    {
        pills = Mathf.Max(0, value);
        RefreshPillUi();
    }

    /// <summary>Adds one pill and refreshes the existing HUD counter.</summary>
    public void AddPill()
    {
        pills += 1;
        RefreshPillUi();
    }

    /// <summary>Returns the number of pills currently carried.</summary>
    public int GetPill()
    {
        return pills;
    }

    /// <summary>Consumes one pill when one is available.</summary>
    public void UsePill()
    {
        if (pills > 0)
        {
            pills -= 1;
            RefreshPillUi();
        }
    }

    /// <summary>
    /// Adds one key to the inventory. Returns false only for an invalid definition.
    /// </summary>
    public bool AddKey(KeyDefinition key)
    {
        if (key == null)
        {
            Debug.LogWarning("Cannot add a null KeyDefinition to the inventory.", this);
            return false;
        }

        keys.Add(key);
        return true;
    }

    /// <summary>Checks whether at least one matching key is available.</summary>
    public bool HasKey(KeyDefinition requiredKey)
    {
        return FindKeyIndex(requiredKey) >= 0;
    }

    /// <summary>Returns how many copies of a matching key are available.</summary>
    public int GetKeyCount(KeyDefinition requiredKey)
    {
        if (requiredKey == null)
        {
            return 0;
        }

        int count = 0;
        foreach (KeyDefinition ownedKey in keys)
        {
            if (requiredKey.Matches(ownedKey))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Removes one matching key, if present.</summary>
    public bool RemoveKey(KeyDefinition requiredKey)
    {
        int keyIndex = FindKeyIndex(requiredKey);
        if (keyIndex < 0)
        {
            return false;
        }

        keys.RemoveAt(keyIndex);
        return true;
    }

    /// <summary>
    /// Verifies that a matching key exists and optionally consumes one copy.
    /// This is the single inventory operation used by locked doors.
    /// </summary>
    public bool TryUseKey(KeyDefinition requiredKey, bool consumeKey)
    {
        int keyIndex = FindKeyIndex(requiredKey);
        if (keyIndex < 0)
        {
            return false;
        }

        if (consumeKey)
        {
            keys.RemoveAt(keyIndex);
        }

        return true;
    }

    private int FindKeyIndex(KeyDefinition requiredKey)
    {
        if (requiredKey == null)
        {
            return -1;
        }

        for (int index = 0; index < keys.Count; index++)
        {
            if (requiredKey.Matches(keys[index]))
            {
                return index;
            }
        }

        return -1;
    }

    private string BuildInventorySummary()
    {
        var summary = new StringBuilder();
        summary.AppendLine($"Flashlight = {flashlight}");
        summary.AppendLine($"Batteries = {batteries}");
        summary.AppendLine($"Pills = {pills}");
        summary.Append("Keys = ");

        if (keys.Count == 0)
        {
            summary.Append("None");
            return summary.ToString();
        }

        for (int index = 0; index < keys.Count; index++)
        {
            if (index > 0)
            {
                summary.Append(", ");
            }

            KeyDefinition key = keys[index];
            summary.Append(key != null ? key.DisplayName : "Missing Key");
        }

        return summary.ToString();
    }

    private void RefreshConsumableUi()
    {
        RefreshBatteryUi();
        RefreshPillUi();
    }

    private void RefreshBatteryUi()
    {
        if (batteryText != null)
        {
            batteryText.text = batteries.ToString();
        }
    }

    private void RefreshPillUi()
    {
        if (pillText != null)
        {
            pillText.text = pills.ToString();
        }
    }
}
