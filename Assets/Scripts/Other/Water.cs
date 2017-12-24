﻿using UnityEngine;

public class Water : MonoBehaviour
{
    public static bool isRef = true;        //is reflcet and refract
    public static Material mat;
    public static Shader refShader;
    public static Shader noRefShader;

    public Camera mainCam;
    public float vertexTick = 3;
    public int xCount = 5;
    public int zCount = 5;
    public float waveSpeed = 1f;
    public float viscosity = 0.2f;
    public int noiseFrequency = 2;
    public float noiseAmplitude = 0.2f;

    private Camera reflectCamera;
    private Matrix4x4 reflectMatrix;
    private Vector4 reflectClipPlane;
    private Camera refractCamera;
    private Vector4 refractClipPlane;

    private Mesh mesh;
    private float xmin, xmax, zmin, zmax;
    private float timeTick;
    private float f1, f2, k1, k2, k3;
    private Vector3[] prev;
    private Vector3[] curr;

    private Rigidbody player;
    private float noiseTime;
    private float noiseTimer;

    private void Start()
    {
        if (mat==null)
        {
            mat = GetComponent<MeshRenderer>().sharedMaterial;
            refShader = Shader.Find("Unlit/Water");
            noRefShader = Shader.Find("Unlit/WaterNoRef");
        }
        
        GenerateReflectCamera();
        GenerateRefractCamera();

        GenerateMesh();
        timeTick = Time.fixedDeltaTime;
        f1 = waveSpeed * waveSpeed * timeTick * timeTick / (vertexTick * vertexTick);
        f2 = 1.0f / (viscosity * timeTick + 2);
        k1 = (4.0f - 8.0f * f1) * f2;
        k2 = (viscosity * timeTick - 2.0f) * f2;
        k3 = 2.0f * f1 * f2;

        reflectMatrix = GenerateReflectMatrix();

        player = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Rigidbody>();
        noiseTime = 1.0f / noiseFrequency;
    }

    private void GenerateRefractCamera()
    {
        var texture = new RenderTexture(Mathf.FloorToInt(mainCam.pixelWidth), Mathf.FloorToInt(mainCam.pixelHeight), 24)
        {
            wrapMode = TextureWrapMode.Repeat
        };

        var gameObject = new GameObject("RefractCamera");
        refractCamera = gameObject.AddComponent<Camera>();
        refractCamera.CopyFrom(mainCam);
        refractCamera.enabled = false;
        refractCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water"));
        refractCamera.targetTexture = texture;
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

    private void GenerateMesh()
    {
        var xStart = -vertexTick * xCount / 2;
        var zStart = -vertexTick * zCount / 2;
        var y = transform.position.y;

        prev = new Vector3[xCount * zCount];
        curr = new Vector3[xCount * zCount];
        var indices = new int[(xCount - 1) * (zCount - 1) * 6];

        int index = 0;
        for (int j = 0; j < zCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                prev[index] = new Vector3(xStart + i * vertexTick, y, zStart + j * vertexTick);
                curr[index] = prev[index];
                index++;
            }
        }

        index = 0;
        for (int j = 0; j < zCount - 1; j++)
        {
            for (int i = 0; i < xCount - 1; i++)
            {
                indices[index++] = i + xCount * j;
                indices[index++] = i + xCount * (j + 1);
                indices[index++] = i + 1 + xCount * j;
                indices[index++] = i + 1 + xCount * j;
                indices[index++] = i + xCount * (j + 1);
                indices[index++] = i + 1 + xCount * (j + 1);
            }
        }

        mesh = new Mesh
        {
            vertices = curr
        };
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;

        xmin = transform.position.x + xStart + vertexTick;
        zmin = transform.position.z + zStart + vertexTick;
        xmax = xmin + vertexTick * (xCount - 1);
        zmax = zmin + vertexTick * (zCount - 1);
    }

    private Matrix4x4 GenerateReflectMatrix()
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
        refractClipPlane = new Vector4(0, -1, 0, -d);

        return matrix;
    }

    private void Update()
    {
        //interact with water
        var pos = player.position;
        if (pos.x > xmin && pos.z > zmin && pos.x < xmax && pos.z < zmax)
        {
            int x = Mathf.FloorToInt((pos.x - xmin) / vertexTick) + 1;
            int z = Mathf.FloorToInt((pos.z - zmin) / vertexTick) + 1;
            var p = prev[x + xCount * z];
            prev[x + xCount * z] = new Vector3(p.x, p.y - 0.002f * player.velocity.magnitude, p.z);
        }

        //generate random wave
        noiseTimer += Time.deltaTime;
        if (noiseTimer > noiseTime)
        {
            int x = Random.Range(1, xCount - 1);
            int z = Random.Range(1, zCount - 1);
            var p = prev[x + xCount * z];
            prev[x + xCount * z] = new Vector3(p.x, p.y - noiseAmplitude, p.z);
            noiseTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        var next = prev;
        for (int j = 1; j < zCount - 1; j++)
        {
            for (int i = 1; i < xCount - 1; i++)
            {
                int index = i + xCount * j;
                next[index] = k1 * curr[index] + k2 * prev[index] +
                    k3 * (curr[index + 1] + curr[index - 1] + curr[index - xCount] + curr[index + xCount]);
            }
        }

        mesh.vertices = next;
        mesh.RecalculateNormals();

        prev = curr;
        curr = next;
    }

    void OnWillRenderObject()
    {
        if (!isRef) return;

        //reflect
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

        mat.SetTexture("_ReflectTex", reflectCamera.targetTexture);

        //refract
        refractCamera.transform.position = mainCam.transform.position;
        refractCamera.transform.rotation = mainCam.transform.rotation;
        matrix = refractCamera.projectionMatrix;

        clipPlane = refractCamera.worldToCameraMatrix.inverse.transpose * refractClipPlane;
        q = new Vector4(                                                //corner point of frustuum in camera space
            (Mathf.Sign(clipPlane.x) + matrix.m02) / matrix.m00,
            (Mathf.Sign(clipPlane.y) + matrix.m12) / matrix.m11,
            -1.0f,
            (1.0f + matrix.m22) / matrix.m23);
        c = clipPlane * (2.0f / Vector4.Dot(clipPlane, q));

        matrix.m20 = c.x;
        matrix.m21 = c.y;
        matrix.m22 = c.z + 1.0f;
        matrix.m23 = c.w;

        refractCamera.projectionMatrix = matrix;

        refractCamera.Render();

        mat.SetTexture("_RefractTex", refractCamera.targetTexture);
    }

    public static void SetRef(bool isOn)
    {
        if (isRef != isOn)
        {
            isRef = isOn;
            if (isRef)
            {
                mat.shader = refShader;
            }
            else
            {
                mat.shader = noRefShader;
            }
        }

    }
}
