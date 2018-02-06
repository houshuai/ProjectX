using UnityEngine;

public class Ocean : MonoBehaviour
{
    public Material material;
    public float xLength = 100;
    public float yLength = 50;
    public int xCount = 20;
    public int yCount = 10;

    private Vector3[] vertices;
    private Mesh mesh;

    private void Start()
    {
        GenerateWater();
    }

    private void GenerateWater()
    {
        var vertexCount = xCount * yCount;
        var xTick = xLength / (xCount - 1);
        var yTick = yLength / (yCount - 1);
        var u = 1.0f / (xCount - 1);
        var v = 1.0f / (yCount - 1);

        vertices = new Vector3[vertexCount];
        var uv = new Vector2[vertexCount];

        int index = 0;
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                vertices[index] = new Vector3(i * xTick, 0, j * yTick);
                uv[index] = new Vector2(i * u, j * v);
                index++;
            }
        }

        index = 0;
        var indices = new int[(xCount - 1) * (yCount - 1) * 6];
        for (int j = 0; j < yCount - 1; j++)
        {
            for (int i = 0; i < xCount - 1; i++)
            {
                int self = i + (j * xCount);
                int next = i + ((j + 1) * xCount);
                indices[index++] = self;
                indices[index++] = next;
                indices[index++] = next + 1;
                indices[index++] = self;
                indices[index++] = next + 1;
                indices[index++] = self + 1;
            }
        }

        mesh = new Mesh()
        {
            vertices = vertices,
            uv = uv,
            triangles = indices,
        };
        mesh.RecalculateNormals();

        var water = new GameObject("water");
        water.AddComponent<MeshFilter>().mesh = mesh;
        water.AddComponent<MeshRenderer>().sharedMaterial = material;
        water.AddComponent<MeshCollider>().sharedMesh = mesh;
        //water.AddComponent<Reflection>().Initial(transform.position.y);
        //water.AddComponent<Refraction>().Initial(transform.position.y);
        //water.AddComponent<Water>();
        water.transform.SetParent(transform,false);
    }
}

