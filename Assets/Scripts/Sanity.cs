using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Sanity : MonoBehaviour
{
    public GameObject Flashlight;
    public Volume sanityVolume; // drag your global Volume here

    private Inventory inventory;
    private ColorAdjustments colorAdjustments;
    public Camera camera;

    public float fov1 = 50;
    public float fov2 = 40;
    public float fov3 = 30;

    public float sanityDecrease = 0.05f;
    public float speed = 15;
    public float sanity;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        sanity = 100;

        sanityVolume.profile = Instantiate(sanityVolume.profile);
        bool found = sanityVolume.profile.TryGet(out colorAdjustments);
        colorAdjustments.saturation.overrideState = true;
    }

    void Update()
    {
        if (!Flashlight.activeSelf && sanity > 0)
        {
            sanity -= sanityDecrease;
        }

        if (Input.GetKeyDown(KeyCode.Q) && inventory.GetPill() > 0)
        {
            sanity = 100;
            inventory.UsePill();
        }

        // FOV
        if (sanity < 25)
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, fov3, Time.deltaTime * speed);
        else if (sanity < 50)
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, fov2, Time.deltaTime * speed);
        else if (sanity < 75)
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, fov1, Time.deltaTime * speed);
        else
            camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, 60, Time.deltaTime * speed);

        // --- Grayscale based on sanity ---
        // saturation: 0 = normal color, -100 = full grayscale
        float targetSaturation = Mathf.Lerp(-100f, 0f, sanity / 100f);
        colorAdjustments.saturation.value = Mathf.MoveTowards(
            colorAdjustments.saturation.value,
            targetSaturation,
            Time.deltaTime * speed
        );
    }
}