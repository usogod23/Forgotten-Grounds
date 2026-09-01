using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VoiceOverTrigger : MonoBehaviour
{
    [Header("Save System")]
    public string uniqueTriggerID;

    [Header("Voice Over Settings")]
    public AudioClip voiceClip;

    [TextArea]
    public string subtitleText;

    [Header("Subtitle Settings")]
    public Color subtitleColor = Color.white;
    public TMP_FontAsset subtitleFont;
    public float fadeDuration = 0.3f;

    [Header("UI Reference")]
    public TMP_Text subtitleUI;

    [Header("Clue Integration (Optional)")]
    public ClueInfo clueData;
    public ClueManager clueManager;

    public PopupController popupController;

    // pentru a fi derulata o singura data interactiunea (folosit doar in cazuri necesare)
    private bool hasPlayed = false;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // vocea sa se auda mereu clar indiferent de orientarea camerei
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // ma asigur ca player-ul este cel care trece prin collider si ma asigur ca nu s-a mai derulat interactiunea o alta data
        if (other.CompareTag("Player"))
        {

            bool isPlayed = false;

            if (PlayerPositionSaveSystem.Instance != null)
            {
                isPlayed = PlayerPositionSaveSystem.Instance.IsTriggerPlayed(uniqueTriggerID);
            }
            else
            {
                isPlayed = hasPlayed;
            }

            // daca a fost deja rulata salvarea curenta opresc
            if (isPlayed)
            {
                return;
            }

            if (PlayerPositionSaveSystem.Instance != null)
            {
                PlayerPositionSaveSystem.Instance.MarkTriggerPlayed(uniqueTriggerID);
            }
            else
            {
                hasPlayed = true;
            }

            if (clueData != null && clueManager != null)
            {
                clueManager.AddClue(clueData);

                // arat popup pe ecran
                if (popupController != null)
                {
                    popupController.Show("Clue added");
                }
            }

            StartCoroutine(PlayVoiceOver());
        }
    }

    private IEnumerator PlayVoiceOver()
    {
        // activez textul pe ecran
        if (subtitleUI != null)
        {
            subtitleUI.text = subtitleText;

            subtitleUI.color = subtitleColor;
            if (subtitleFont != null)
            {
                subtitleUI.font = subtitleFont;
            }
            subtitleUI.alpha = 0f;
            subtitleUI.gameObject.SetActive(true);
        }

        // redau sunetul
        if (voiceClip != null)
        {
            audioSource.clip = voiceClip;
            audioSource.Play();
        }

        if (subtitleUI != null)
        {
            yield return StartCoroutine(FadeTextAlpha(0f, 1f, fadeDuration));
        }

        // calculez cat timp textul sta vizibil pe ecran
        float totalDuration = (voiceClip != null) ? voiceClip.length : 3f;
        float waitTime = totalDuration - (fadeDuration * 2);

        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }

        // fade out
        if (subtitleUI != null)
        {
            yield return StartCoroutine(FadeTextAlpha(1f, 0f, fadeDuration));
            subtitleUI.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeTextAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            subtitleUI.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        subtitleUI.alpha = endAlpha;
    }
}
