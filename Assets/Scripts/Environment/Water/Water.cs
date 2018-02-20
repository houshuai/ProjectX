using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public static List<Material> mats = new List<Material>();

    public static bool isReflect = true;
    public static bool isRefract = true;

    private Material currMat;

    protected virtual void Start()
    {
        currMat = GetComponent<MeshRenderer>().sharedMaterial;
        if (!mats.Contains(currMat))
        {
            if (isReflect)
            {
                currMat.EnableKeyword("REFLECTION");
            }
            else
            {
                currMat.DisableKeyword("REFLECTION");
            }
            if (isRefract)
            {
                currMat.EnableKeyword("REFRACTION");
            }
            else
            {
                currMat.DisableKeyword("REFRACTION");
            }
            mats.Add(currMat);
        }
    }

    void OnWillRenderObject()
    {
        if (isReflect)
        {
            currMat.SetTexture("_ReflectTex", Reflection.Instance.Render());
        }

        if (isRefract)
        {
            currMat.SetTexture("_RefractTex", Refraction.Instance.Render());
        }
    }

    public static void SetReflection(bool isOn)
    {
        isReflect = isOn;
        if (isReflect)
        {
            foreach (var mat in mats)
            {
                mat.EnableKeyword("REFLECTION");
            }
        }
        else
        {
            foreach (var mat in mats)
            {
                mat.DisableKeyword("REFLECTION");
            }
        }
    }

    public static void SetRefraction(bool isOn)
    {
        isRefract = isOn;
        if (isRefract)
        {
            foreach (var mat in mats)
            {
                mat.EnableKeyword("REFRACTION");
            }
        }
        else
        {
            foreach (var mat in mats)
            {
                mat.DisableKeyword("REFRACTION");
            }
        }
    }
}
