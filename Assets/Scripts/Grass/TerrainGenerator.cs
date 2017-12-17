using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material grassMat;
    public Material terrainMat;

    public int row = 250;
    public int col = 250;

    [Range(0, 65000)] public int grassCount = 10000;
    public int rowCount = 5;
    public int colCount = 5;

    private void Start()
    {
        GenerateTerrain();
        GenerateGrass();
    }

    private void GenerateTerrain()
    {
        var vertices = new Vector3[row * col];
        var triangls = new int[2 * (row - 1) * (col - 1) * 3];
        var uv = new Vector2[row * col];
        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                vertices[vertexIndex] = new Vector3(i, 0, j);
                uv[vertexIndex++] = new Vector2(i, j);
                if (i == 0 || j == 0) continue;

                triangls[triangleIndex++] = col * i + j;
                triangls[triangleIndex++] = col * i + j - 1;
                triangls[triangleIndex++] = col * (i - 1) + j - 1;
                triangls[triangleIndex++] = col * (i - 1) + j - 1;
                triangls[triangleIndex++] = col * (i - 1) + j;
                triangls[triangleIndex++] = col * i + j;
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangls,
            uv = uv
        };
        mesh.RecalculateNormals();

        var terrain = new GameObject("Terrain");
        terrain.AddComponent<MeshFilter>().mesh = mesh;
        terrain.AddComponent<MeshRenderer>().material = terrainMat;
    }

    private void GenerateGrass()
    {
        var indices = new int[grassCount];
        for (int i = 0; i < grassCount; i++)
        {
            indices[i] = i;
        }

        Vector2 startPos = new Vector2(0, 0);
        float rowLength = row / rowCount;
        float colLength = col / colCount;

        for (int i = 0; i < rowCount; i++)
        {
            startPos.y = 0;
            for (int j = 0; j < colCount; j++)
            {
                Generate(startPos, rowLength, colLength, indices);
                startPos.y += colLength;
            }
            startPos.x += rowLength;
        }


    }

    private void Generate(Vector2 start, float rowLength, float colLength, int[] indices)
    {
        var vertices = new Vector3[grassCount];
        int vertexIndex = 0;

        for (int i = 0; i < grassCount; i++)
        {
            vertices[vertexIndex++] = new Vector3(start.x + Random.value * rowLength, 0, start.y + Random.value * colLength);
        }

        var mesh = new Mesh
        {
            vertices = vertices
        };
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        var grass = new GameObject("Grass");
        grass.AddComponent<MeshFilter>().mesh = mesh;
        grass.AddComponent<MeshRenderer>().material = grassMat;
    }
}
