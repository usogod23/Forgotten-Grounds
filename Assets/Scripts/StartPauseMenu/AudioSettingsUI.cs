using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public sealed class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MasterParam = "MasterVolume";
    private const string MusicParam = "MusicVolume";
    private const string SfxParam = "SFXVolume";

    private const string MasterPref = "vol_master";
    private const string MusicPref = "vol_music";
    private const string SfxPref = "vol_sfx";

    private void OnEnable()
    {
        float master = PlayerPrefs.GetFloat(MasterPref, 1f);
        float music = PlayerPrefs.GetFloat(MusicPref, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxPref, 1f);

        if (masterSlider != null)
        {
            masterSlider.SetValueWithoutNotify(master);
            masterSlider.onValueChanged.RemoveListener(SetMaster);
            masterSlider.onValueChanged.AddListener(SetMaster);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(music);
            musicSlider.onValueChanged.RemoveListener(SetMusic);
            musicSlider.onValueChanged.AddListener(SetMusic);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfx);
            sfxSlider.onValueChanged.RemoveListener(SetSfx);
            sfxSlider.onValueChanged.AddListener(SetSfx);
        }

        ApplyToMixer(MasterParam, master);
        ApplyToMixer(MusicParam, music);
        ApplyToMixer(SfxParam, sfx);
    }

    private void OnDisable()
    {
        masterSlider?.onValueChanged.RemoveListener(SetMaster);
        musicSlider?.onValueChanged.RemoveListener(SetMusic);
        sfxSlider?.onValueChanged.RemoveListener(SetSfx);
    }

    public void SetMaster(float value)
    {
        ApplyToMixer(MasterParam, value);
        PlayerPrefs.SetFloat(MasterPref, value);
    }

    public void SetMusic(float value)
    {
        ApplyToMixer(MusicParam, value);
        PlayerPrefs.SetFloat(MusicPref, value);
    }

    public void SetSfx(float value)
    {
        ApplyToMixer(SfxParam, value);
        PlayerPrefs.SetFloat(SfxPref, value);
    }

    private void ApplyToMixer(string param, float sliderValue)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dB = sliderValue <= 0.0001f ? -80f : Mathf.Log10(sliderValue) * 20f;
        audioMixer.SetFloat(param, dB);
    }
}