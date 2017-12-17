using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenu : Menu<AudioMenu>
{
    public AudioMixer audioMixer;

    private Slider[] slideres;

    private void OnEnable()
    {
        slideres = GetComponentsInChildren<Slider>();

        float value;
        audioMixer.GetFloat("master",out value);
        slideres[0].value = value;
        audioMixer.GetFloat("SFX", out value);
        slideres[1].value = value;
        audioMixer.GetFloat("music", out value);
        slideres[2].value = value;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("master", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("music", volume);
    }
}
