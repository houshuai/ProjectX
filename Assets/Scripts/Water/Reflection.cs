using UnityEngine;

public class Reflection : MonoBehaviour
{
    private Camera mainCam;
    private Camera reflectCamera;
    private Matrix4x4 reflectMatrix;
    private Vector4 reflectClipPlane;

    private void Start()
    {
        mainCam = Camera.main;
        GenerateReflectCamera();
        GenerateReflectMatrix();
    }

    private void GenerateReflectCamera()
    {
        var texture = new RenderTexture(Mathf.FloorToInt(mainCam.pixelWidth), Mathf.FloorToInt(mainCam.pixelHeight), 24)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        var gameObject = new GameObject("ReflectCamera");
        reflectCamera = gameObject.AddComponent<Camera>();
        reflectCamera.CopyFrom(mainCam);
        reflectCamera.enabled = false;
        reflectCamera.clearFlags = CameraClearFlags.SolidColor;
        reflectCamera.backgroundColor = new Color(0, 0, 0, 0);
        reflectCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water"));
        reflectCamera.targetTexture = texture;
    }

    private void GenerateReflectMatrix()
    {
        var normal = new Vector3(0, 1, 0);
        float d = -Vector3.Dot(transform.position, normal);            //distance of mirror to origin(0,0,0)

        var matrix = new Matrix4x4
        {
            m00 = 1 - 2 * normal.x * normal.x,
            m01 = -2 * normal.x * normal.y,
            m02 = -2 * normal.x * normal.z,
            m03 = -2 * normal.x * d,

            m10 = -2 * normal.x * normal.y,
            m11 = 1 - 2 * normal.y * normal.y,
            m12 = -2 * normal.y * normal.z,
            m13 = -2 * normal.y * d,

            m20 = -2 * normal.x * normal.z,
            m21 = -2 * normal.y * normal.z,
            m22 = 1 - 2 * normal.z * normal.z,
            m23 = -2 * normal.z * d,

            m30 = 0,
            m31 = 0,
            m32 = 0,
            m33 = 1
        };

        reflectClipPlane = new Vector4(0, 1, 0, d);

        reflectMatrix = matrix;
    }

    public RenderTexture Render()
    {
        reflectCamera.worldToCameraMatrix = mainCam.worldToCameraMatrix * reflectMatrix;
        var matrix = reflectCamera.projectionMatrix;

        var clipPlane = reflectCamera.worldToCameraMatrix.inverse.transpose * reflectClipPlane;
        var q = new Vector4(                                                //corner point of frustum in camera space
            (Mathf.Sign(clipPlane.x) + matrix.m02) / matrix.m00,
            (Mathf.Sign(clipPlane.y) + matrix.m12) / matrix.m11,
            -1.0f,
            (1.0f + matrix.m22) / matrix.m23);
        var c = clipPlane * (2.0f / Vector4.Dot(clipPlane, q));

        matrix.m20 = c.x;
        matrix.m21 = c.y;
        matrix.m22 = c.z + 1.0f;
        matrix.m23 = c.w;

        reflectCamera.projectionMatrix = matrix;

        GL.invertCulling = true;
        reflectCamera.Render();
        GL.invertCulling = false;

        return reflectCamera.targetTexture;
    }
}
