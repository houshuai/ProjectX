using UnityEngine;

public class Water : MonoBehaviour
{
    public static Material mat;

    public static bool isReflect = true;
    public static bool isRefract = true;

    private Reflection reflection;
    private Refraction refraction;

    private void Start()
    {
        Initial();
    }

    public void Initial()
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

        reflection = gameObject.AddComponent<Reflection>();
        refraction = gameObject.AddComponent<Refraction>();

    }

    void OnWillRenderObject()
    {
        if (isReflect)
        {
            mat.SetTexture("_ReflectTex", reflection.Render());
        }

        if (isRefract)
        {
            mat.SetTexture("_RefractTex", refraction.Render());
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
