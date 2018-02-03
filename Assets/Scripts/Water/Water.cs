using UnityEngine;

public class Water : MonoBehaviour
{
    public static Material mat;

    public static bool isReflect = true;
    public static bool isRefract = true;

    protected virtual void Start()
    {
        var currMat = GetComponent<MeshRenderer>().sharedMaterial;
        if (mat == null || mat != currMat)
        {
            mat = currMat;
            if (isReflect)
            {
                mat.EnableKeyword("REFLECTION");
            }
            else
            {
                mat.DisableKeyword("REFLECTION");
            }
            if (isRefract)
            {
                mat.EnableKeyword("REFRACTION");
            }
            else
            {
                mat.DisableKeyword("REFRACTION");
            }
        }
    }

    void OnWillRenderObject()
    {
        if (isReflect)
        {
            mat.SetTexture("_ReflectTex", Reflection.Instance.Render());
        }

        if (isRefract)
        {
            mat.SetTexture("_RefractTex", Refraction.Instance.Render());
        }
    }

    public static void SetReflection(bool isOn)
    {
        isReflect = isOn;
        if (isReflect)
        {
            mat.EnableKeyword("REFLECTION");
        }
        else
        {
            mat.DisableKeyword("REFLECTION");
        }
    }

    public static void SetRefraction(bool isOn)
    {
        isRefract = isOn;
        if (isRefract)
        {
            mat.EnableKeyword("REFRACTION");
        }
        else
        {
            mat.DisableKeyword("REFRACTION");
        }
    }
}
