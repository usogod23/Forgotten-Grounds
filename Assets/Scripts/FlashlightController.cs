using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public GameObject Flashlight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Flashlight.SetActive(!Flashlight.activeSelf);
        }
    }
}
