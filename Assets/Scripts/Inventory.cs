using UnityEngine;

public class Inventory : MonoBehaviour
{
    public PopupController popupController;
    private static int flashlight = 1;
    private int batteries = 0;
    private int pills = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
    }

    public void AddBattery()
    {
        batteries += 1;
    }

    public void AddPill()
    {
        pills += 1;
    }
}
