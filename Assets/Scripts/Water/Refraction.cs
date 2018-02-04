using UnityEngine;

public class Refraction : MonoBehaviour
{
    private static Refraction instance;

    public static Refraction Instance
    {
        get { return instance; }
    }
    
    private Camera mainCam;
    private Camera refractCamera;
    private Vector4 refractClipPlane;
    private RenderTexture targetTexture;
    private bool isRendered;

    public void Initial(float waterHeight)
    {
        mainCam = Camera.main;
        GenerateRefractCamera();
        refractClipPlane = new Vector4(0, -1, 0, waterHeight);
        instance = this;
    }

    private void GenerateRefractCamera()
    {
        targetTexture = new RenderTexture(Mathf.FloorToInt(mainCam.pixelWidth), Mathf.FloorToInt(mainCam.pixelHeight), 24)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        var gameObject = new GameObject("RefractCamera");
        refractCamera = gameObject.AddComponent<Camera>();
        refractCamera.CopyFrom(mainCam);
        refractCamera.enabled = false;
        refractCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water"));
        refractCamera.targetTexture = targetTexture;
    }

    public RenderTexture Render()
    {
        if (isRendered)
        {
            return targetTexture;
        }

        refractCamera.transform.position = mainCam.transform.position;
        refractCamera.transform.rotation = mainCam.transform.rotation;
        var matrix = refractCamera.projectionMatrix;

        var clipPlane = refractCamera.worldToCameraMatrix.inverse.transpose * refractClipPlane;
        var q = new Vector4(                                                //corner point of frustuum in camera space
            (Mathf.Sign(clipPlane.x) + matrix.m02) / matrix.m00,
            (Mathf.Sign(clipPlane.y) + matrix.m12) / matrix.m11,
            -1.0f,
            (1.0f + matrix.m22) / matrix.m23);
        var c = clipPlane * (2.0f / Vector4.Dot(clipPlane, q));

        matrix.m20 = c.x;
        matrix.m21 = c.y;
        matrix.m22 = c.z + 1.0f;
        matrix.m23 = c.w;

        refractCamera.projectionMatrix = matrix;
        
        refractCamera.Render();

        isRendered = true;

        return refractCamera.targetTexture;
    }

    private void LateUpdate()
    {
        isRendered = false;
    }
}
