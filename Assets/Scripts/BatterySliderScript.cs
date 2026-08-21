using UnityEngine;
using UnityEngine.UI;

public class BatterySliderScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    public Slider batterySlider;

    public float max_battery = 100;
    public float current_battery;
    void Start()
    {
        current_battery = player.GetComponent<FlashlightController>().battery;
    }

    // Update is called once per frame
    void Update()
    {
        current_battery = player.GetComponent<FlashlightController>().battery;
        batterySlider.value = current_battery;
        batterySlider.maxValue = 100;
    }
}
