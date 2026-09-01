using System.Collections;
using System;
using UnityEngine;

public class BloodLightZoneController : MonoBehaviour
{
    [Header("Light References")]
    public Light targetLight;

    [Header("Blood Settings (Sun & Sky)")]
    public Color bloodColor = new Color(0.6f, 0f, 0f);
    public float targetIntensity = 1f;

    [Header("Environment Settings")]
    [Tooltip("Cât de întunecat devine mediul (Intensity Multiplier)")]
    public float targetAmbientIntensity = 0.1f;
    public float transitionDuration = 3f;

    // stari initiale
    private Color originalLightColor;
    private float originalIntensity;
    private float originalAmbientIntensity;

    // skybox
    private Material originalSkybox;
    private Material clonedSkybox;
    private Color originalSkyColor;
    private string tintProperty = "";

    private Coroutine currentTransition;

    void Start()
    {
        if (targetLight != null)
        {
            originalLightColor = targetLight.color;
            originalIntensity = targetLight.intensity;
        }

        originalAmbientIntensity = RenderSettings.ambientIntensity;

        originalSkybox = RenderSettings.skybox;
        if (originalSkybox != null)
        {
            // verific tipul de skybox
            if (originalSkybox.HasProperty("_Tint")) tintProperty = "_Tint";
            else if (originalSkybox.HasProperty("_SkyTint")) tintProperty = "_SkyTint";

            if (!string.IsNullOrEmpty(tintProperty))
            {
                // creez clona si ii salvez culoarea
                clonedSkybox = new Material(originalSkybox);
                originalSkyColor = clonedSkybox.GetColor(tintProperty);

                // aplic clona in scena
                RenderSettings.skybox = clonedSkybox;
            }
        }
    }

    // resetare la load
    void OnEnable()
    {
        PlayerPositionSaveSystem.OnSaveLoaded += ResetLightOnLoad;
    }

    void OnDisable()
    {
        PlayerPositionSaveSystem.OnSaveLoaded -= ResetLightOnLoad;

        if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
        }
    }

    private void ResetLightOnLoad()
    {
        if (currentTransition != null) StopCoroutine(currentTransition);

        if (targetLight != null)
        {
            targetLight.color = originalLightColor;
            targetLight.intensity = originalIntensity;
        }

        RenderSettings.ambientIntensity = originalAmbientIntensity;

        if (clonedSkybox != null && !string.IsNullOrEmpty(tintProperty))
        {
            clonedSkybox.SetColor(tintProperty, originalSkyColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentTransition != null) StopCoroutine(currentTransition);

            Color startLightCol = targetLight != null ? targetLight.color : originalLightColor;
            float startInt = targetLight != null ? targetLight.intensity : originalIntensity;

            Color startSkyCol = originalSkyColor;
            if (clonedSkybox != null && !string.IsNullOrEmpty(tintProperty))
            {
                startSkyCol = clonedSkybox.GetColor(tintProperty);
            }

            currentTransition = StartCoroutine(FadeLight(
                startLightCol, bloodColor,
                startInt, targetIntensity,
                RenderSettings.ambientIntensity, targetAmbientIntensity,
                startSkyCol, bloodColor));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentTransition != null) StopCoroutine(currentTransition);

            Color startLightCol = targetLight != null ? targetLight.color : originalLightColor;
            float startInt = targetLight != null ? targetLight.intensity : originalIntensity;

            Color startSkyCol = bloodColor;
            if (clonedSkybox != null && !string.IsNullOrEmpty(tintProperty))
            {
                startSkyCol = clonedSkybox.GetColor(tintProperty);
            }

            currentTransition = StartCoroutine(FadeLight(
                startLightCol, originalLightColor,
                startInt, originalIntensity,
                RenderSettings.ambientIntensity, originalAmbientIntensity,
                startSkyCol, originalSkyColor));
        }
    }

    private IEnumerator FadeLight(Color startLightCol, Color endLightCol, float startSunInt, float endSunInt, float startAmb, float endAmb, Color startSkyCol, Color endSkyCol)
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;

            if (targetLight != null)
            {
                targetLight.color = Color.Lerp(startLightCol, endLightCol, t);
                targetLight.intensity = Mathf.Lerp(startSunInt, endSunInt, t);
            }

            RenderSettings.ambientIntensity = Mathf.Lerp(startAmb, endAmb, t);

            if (clonedSkybox != null && !string.IsNullOrEmpty(tintProperty))
            {
                clonedSkybox.SetColor(tintProperty, Color.Lerp(startSkyCol, endSkyCol, t));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (targetLight != null)
        {
            targetLight.color = endLightCol;
            targetLight.intensity = endSunInt;
        }
        RenderSettings.ambientIntensity = endAmb;

        if (clonedSkybox != null && !string.IsNullOrEmpty(tintProperty))
        {
            clonedSkybox.SetColor(tintProperty, endSkyCol);
        }
    }
}