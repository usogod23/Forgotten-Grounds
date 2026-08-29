using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public GameObject Flashlight;
    private Inventory inventory;
    public float battery;
    public float batteryDecrease = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        battery = 100;
        Flashlight.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Flashlight.activeSelf)
        {
            battery -= batteryDecrease * Time.deltaTime;
            if (battery <= 0f)
            {
                Flashlight.SetActive(false);
            }
        }

        if(Input.GetKeyDown(KeyCode.F) && battery > 0f)
        {
            Flashlight.SetActive(!Flashlight.activeSelf);
        }

        if(Input.GetKeyDown(KeyCode.R) && inventory.GetBattery() > 0)
        {
            battery = 100;
            inventory.UseBattery();
        }
    }
}
