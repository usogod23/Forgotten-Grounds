using System.Collections;
using UnityEngine;

public sealed class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioSource gameMusicSource;
    [SerializeField] private float fadeDuration = 1.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (menuMusicSource != null)
        {
            menuMusicSource.loop = true;
            menuMusicSource.volume = 1f;
            if (!menuMusicSource.isPlaying)
            {
                menuMusicSource.Play();
            }
        }

        if (gameMusicSource != null)
        {
            gameMusicSource.loop = true;
            gameMusicSource.volume = 0f;
        }
    }

    // Apeleaza asta din butonul de Start (OnClick, in Inspector)
    public void TransitionToGameplay()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (gameMusicSource != null && !gameMusicSource.isPlaying)
        {
            gameMusicSource.Play();
        }

        fadeRoutine = StartCoroutine(CrossfadeRoutine());
    }

    private IEnumerator CrossfadeRoutine()
    {
        float elapsed = 0f;
        float startMenuVolume = menuMusicSource != null ? menuMusicSource.volume : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            if (menuMusicSource != null)
            {
                menuMusicSource.volume = Mathf.Lerp(startMenuVolume, 0f, t);
            }

            if (gameMusicSource != null)
            {
                gameMusicSource.volume = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        if (menuMusicSource != null)
        {
            menuMusicSource.volume = 0f;
            menuMusicSource.Stop();
        }

        if (gameMusicSource != null)
        {
            gameMusicSource.volume = 1f;
        }
    }
}