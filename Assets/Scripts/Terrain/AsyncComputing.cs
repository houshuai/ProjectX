using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AsyncComputing<T>
{
    public abstract T Compute();

    public virtual Task<T> ComputeAsync()
    {
        return Task.Run(new Func<T>(Compute));
    }
}

public class TerrainComputing : AsyncComputing<TerrainComputing>
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public Node[,] nodes;
    public float[,] heights;
    public float[,] moisture;

    private Rect rect;
    private int xCount, yCount;
    private SimplexNoise noise;

    public TerrainComputing(Rect rect, int xCount, int yCount, SimplexNoise noise)
    {
        this.rect = rect;
        this.xCount = xCount;
        this.yCount = yCount;
        this.noise = noise;
    }

    public override TerrainComputing Compute()
    {
        int vertexCount = xCount * yCount;
        float xTick = rect.width / (xCount - 1);
        float yTick = rect.height / (yCount - 1);
        float u = 1.0f / (xCount - 1);
        float v = 1.0f / (yCount - 1);

        nodes = new Node[xCount, yCount];
        heights = new float[xCount, yCount];
        moisture = new float[xCount, yCount];

        int index = 0;
        vertices = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                var x = i * xTick;
                var y = j * yTick;
                var height = noise.GetOctave(rect.x + x, rect.y + y);
                heights[i, j] = height;
                moisture[i, j] = noise.GetSingle(rect.x + x, rect.y + y);
                vertices[index] = new Vector3(x, height, y);
                nodes[i, j] = new Node(new Vector3(rect.x + x, height, rect.y + y), xTick, yTick, true);
                uvs[index] = new Vector2(i * u, j * v);
                index++;
            }
        }

        return this;
    }
}

public class WaterComputing : AsyncComputing<WaterComputing>
{
    public Vector3[] vertices;
    public int[] indices;

    private Vector3[] terrainVertices;
    private int[] terrainIndices;
    private float height;

    public WaterComputing(Vector3[] terrainVertices, int[] terrainIndices, float waterHeight)
    {
        this.terrainVertices = terrainVertices;
        this.terrainIndices = terrainIndices;
        height = waterHeight;
    }

    public override WaterComputing Compute()
    {
        var vertexList = new List<Vector3>();
        var indexList = new List<int>();

        int index = 0;
        for (int i = 0; i < terrainIndices.Length; i += 3)
        {
            int index1 = terrainIndices[i];
            int index2 = terrainIndices[i + 1];
            int index3 = terrainIndices[i + 2];
            var vertex1 = terrainVertices[index1];
            var vertex2 = terrainVertices[index2];
            var vertex3 = terrainVertices[index3];
            if (vertex1.y < height ||
                vertex2.y < height ||
                vertex3.y < height)
            {
                vertexList.Add(new Vector3(vertex1.x, 0, vertex1.z));
                vertexList.Add(new Vector3(vertex2.x, 0, vertex2.z));
                vertexList.Add(new Vector3(vertex3.x, 0, vertex3.z));
                indexList.Add(index++);
                indexList.Add(index++);
                indexList.Add(index++);
            }
        }

        if (vertexList.Count == 0)
        {
            return null;
        }

        vertices = vertexList.ToArray();
        indices = indexList.ToArray();

        return this;
    }
}
