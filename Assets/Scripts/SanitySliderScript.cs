using UnityEngine;
using UnityEngine.UI;

public class SanitySliderScript : MonoBehaviour
{
    public GameObject player;
    public Slider sanitySlider;

    public float max_sanity = 100;
    public float current_sanity;
    void Start()
    {
        current_sanity = player.GetComponent<Sanity>().sanity;
    }

    // Update is called once per frame
    void Update()
    {
        current_sanity = player.GetComponent<Sanity>().sanity;
        sanitySlider.value = current_sanity;
        sanitySlider.maxValue = 100;
    }
}
