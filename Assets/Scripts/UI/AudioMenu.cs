using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenu : Menu<AudioMenu>
{
    public AudioMixer audioMixer;

    private void OnEnable()
    {
        var slideres = GetComponentsInChildren<Slider>();

        //float value;
        //audioMixer.GetFloat("master",out value);
        slideres[0].value = Setting.instance.masterVolume;
        //audioMixer.GetFloat("SFX", out value);
        slideres[1].value = Setting.instance.sfxVolume;
        //audioMixer.GetFloat("music", out value);
        slideres[2].value = Setting.instance.musicVolume;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("master", volume);
        Setting.instance.masterVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", volume);
        Setting.instance.sfxVolume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("music", volume);
        Setting.instance.musicVolume = volume;
    }
}
