using UnityEngine;
using UnityEngine.UI;

public class VisualMenu : Menu<VisualMenu>
{
    private void OnEnable()
    {
        var dropdowns = GetComponentsInChildren<Dropdown>();
        var toggles = GetComponentsInChildren<Toggle>();

        //get resolution setting
        //if (Screen.width == 1366)
        //{
        //    dropdowns[0].value = 0;
        //}
        //else if (Screen.width == 1280)
        //{
        //    dropdowns[0].value = 1;
        //}
        //else if (Screen.width == 800)
        //{
        //    dropdowns[0].value = 2;
        //}
        dropdowns[0].value = Setting.instance.resolution;

        //get quality level setting
        //if (QualitySettings.GetQualityLevel() == 5)
        //{
        //    dropdowns[1].value = 0;
        //}
        //else if (QualitySettings.GetQualityLevel() == 3)
        //{
        //    dropdowns[1].value = 1;
        //}
        //else if (QualitySettings.GetQualityLevel() == 1)
        //{
        //    dropdowns[1].value = 2;
        //}
        dropdowns[1].value = Setting.instance.quality;

        //get water setting
        //toggles[0].isOn = Water.isReflect;
        //toggles[1].isOn = Water.isRefract;
        toggles[0].isOn = Setting.instance.reflection;
        toggles[1].isOn = Setting.instance.refraction;
    }

    public void SetResolution(int index)
    {
        if (index == 0)
        {
            Screen.SetResolution(1366, 768, true);
        }
        else if (index == 1)
        {
            Screen.SetResolution(1280, 720, true);
        }
        else if (index == 2)
        {
            Screen.SetResolution(800, 600, true);
        }
        Setting.instance.resolution = index;
    }

    public void SetVisualQuality(int index)
    {
        if (index == 0)
        {
            QualitySettings.SetQualityLevel(5, true);
        }
        else if (index == 1)
        {
            QualitySettings.SetQualityLevel(3, true);
        }
        else if (index == 2)
        {
            QualitySettings.SetQualityLevel(1, true);
        }
        Setting.instance.quality = index;
    }

    public void SetWaterReflection(bool isOn)
    {
        Water.SetReflection(isOn);
        Setting.instance.reflection = isOn;
    }

    public void SetWaterRefraction(bool isOn)
    {
        Water.SetRefraction(isOn);
        Setting.instance.refraction = isOn;
    }
}
