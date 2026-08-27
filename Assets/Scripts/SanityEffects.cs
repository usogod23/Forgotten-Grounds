using UnityEngine;

public sealed class SanityEffects : MonoBehaviour
{
    [Header("Referinte")]
    [SerializeField] private Sanity sanity;
    [SerializeField] private Transform cameraTransform;

    [Header("Heartbeat")]
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private float heartbeatThreshold = 50f;
    [SerializeField] private float maxHeartbeatVolume = 1f;
    [SerializeField] private float minPitch = 1f;
    [SerializeField] private float maxPitch = 1.5f;

    [Header("Camera Sway (la sanity 0)")]
    [SerializeField] private float swayStartThreshold = 1f;
    [SerializeField] private float swayPositionAmount = 0.03f;
    [SerializeField] private float swayRotationAmount = 1.5f;
    [SerializeField] private float swaySpeed = 1.2f;

    private Vector3 baseLocalPosition;

    private void Awake()
    {
        if (cameraTransform != null)
        {
            baseLocalPosition = cameraTransform.localPosition;
        }

        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.volume = 0f;
            if (!heartbeatSource.isPlaying)
            {
                heartbeatSource.Play();
            }
        }
    }

    private void Update()
    {
        if (sanity == null)
        {
            return;
        }

        UpdateHeartbeat();
    }

    private void LateUpdate()
    {
        if (sanity == null || cameraTransform == null)
        {
            return;
        }

        UpdateCameraSway();
    }

    private void UpdateHeartbeat()
    {
        if (heartbeatSource == null)
        {
            return;
        }

        float currentSanity = sanity.sanity;

        if (currentSanity >= heartbeatThreshold)
        {
            heartbeatSource.volume = 0f;
            heartbeatSource.pitch = minPitch;
            return;
        }

        float intensity = Mathf.InverseLerp(heartbeatThreshold, 0f, currentSanity);

        heartbeatSource.volume = Mathf.Lerp(0f, maxHeartbeatVolume, intensity);
        heartbeatSource.pitch = Mathf.Lerp(minPitch, maxPitch, intensity);
    }

    private void UpdateCameraSway()
    {

        if (sanity.sanity > swayStartThreshold)
        {
            baseLocalPosition = cameraTransform.localPosition;
            return;
        }

        float sway = Mathf.Sin(Time.time * swaySpeed);

        float offsetX = sway * swayPositionAmount;
        float rollAngle = sway * swayRotationAmount;

        cameraTransform.localPosition = baseLocalPosition + new Vector3(offsetX, 0f, 0f);

        Vector3 currentEuler = cameraTransform.localEulerAngles;
        cameraTransform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, rollAngle);
    }
}