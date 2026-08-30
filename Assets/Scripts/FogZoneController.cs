using System.Collections;
using UnityEngine;

public class FogZoneController : MonoBehaviour
{
    [Header("Fog Settings")]
    // cat de deasă devine ceata inauntru
    public float targetFogDensity = 0.05f;
    // in cate secunde se face tranzitia (fade-in / fade-out)
    public float transitionDuration = 1f; 

    private float originalFogDensity;
    private Coroutine currentTransition;

    void Start()
    {
        // salvez setarea initiala a cetii din afara zonei
        originalFogDensity = RenderSettings.fogDensity;
    }

    void OnTriggerEnter(Collider other)
    {
        // verific daca cel care a intrat este jucatorul
        if (other.CompareTag("Player"))
        {
            if (currentTransition != null) StopCoroutine(currentTransition);
            currentTransition = StartCoroutine(FadeFog(RenderSettings.fogDensity, targetFogDensity));
        }
    }

    void OnTriggerExit(Collider other)
    {
        // verific daca jucatorul a iesit din zona
        if (other.CompareTag("Player"))
        {
            if (currentTransition != null) StopCoroutine(currentTransition);
            currentTransition = StartCoroutine(FadeFog(RenderSettings.fogDensity, originalFogDensity));
        }
    }

    private IEnumerator FadeFog(float startDensity, float endDensity)
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            // trecere fina intre densitatea de pornire și cea dorita
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, endDensity, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // asigur valoarea finala exacta
        RenderSettings.fogDensity = endDensity;
    }
}