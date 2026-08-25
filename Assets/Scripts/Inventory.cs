using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public PopupController popupController;
    private static int flashlight = 1;
    private int batteries = 0;
    private int pills = 0;

    public TMP_Text batteryText;
    public TMP_Text pillText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        batteryText.text = "0";
        pillText.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            string s = string.Format("Flashlight = 1\nBatteries = {0}\nPills = {1}\n", batteries, pills);
            Debug.Log(s);
            popupController.Show(s);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            UseBattery();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UsePill();
        }
    }

    public void AddBattery()
    {
        batteries += 1;
        batteryText.text = batteries.ToString();
    }

    public int GetBattery()
    {
        return batteries;
    }

    public void UseBattery()
    {
        if (batteries > 0)
        {
            batteries -= 1;
            batteryText.text = batteries.ToString();
        }
    }

    public void AddPill()
    {
        pills += 1;
        pillText.text = pills.ToString();
    }

    public int GetPill()
    {
        return pills;
    }

    public void UsePill()
    {
        if (pills > 0)
        {
            pills -= 1;
            pillText.text = pills.ToString();
        }
    }
}
