using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material diffuseMap;
    public float length = 100;
    [Header("vertex count = (2^n + 1)^2")]
    public int n = 4;
    public float smooth = 0.8f;
    public float minHeight = -5;
    public float maxHeight = 5;
    public float dis = 10;

    private Transform player;
    private int count;
    private Rect currRect;
    private List<Terrain> allTerrain;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        count = (1 << n) + 1;

        allTerrain = new List<Terrain>();
        currRect = new Rect(0, 0, length, length);
        var mesh = GenerateTerrain(currRect);
        var terrain = new Terrain()
        {
            rect = currRect,
            mesh = mesh
        };
        allTerrain.Add(terrain);
    }

    /// <summary>
    /// generate terrain in position vector3(rect.x,0,rect.y)
    /// </summary>
    private Mesh GenerateTerrain(Rect rect)
    {
        int vertexCount = count * count;

        var uvs = new Vector2[vertexCount];
        float u = 1.0F / (count - 1);
        float v = 1.0F / (count - 1);
        uint index = 0;
        for (int j = 0; j < count; j++)
        {
            for (int i = 0; i < count; i++)
            {
                uvs[index] = new Vector2(i * u, j * v);
                index++;
            }
        }

        index = 0;
        var vertices = new Vector3[vertexCount];
        float tick = length / (count - 1);
        float range = maxHeight - minHeight;
        var heights = Fractal.RandomMap(n, smooth);
        EdgeJoint(heights, rect, range);
        for (int j = 0; j < count; j++)
        {
            for (int i = 0; i < count; i++)
            {
                float height = heights[i, j] * range;
                vertices[index] = new Vector3(i * tick, height, j * tick);
                index++;
            }
        }

        index = 0;
        var triangles = new int[(count - 1) * (count - 1) * 6];
        for (int j = 0; j < count - 1; j++)
        {
            for (int i = 0; i < count - 1; i++)
            {
                int self = i + (j * count);
                int next = i + ((j + 1) * count);
                triangles[index++] = self;
                triangles[index++] = next;
                triangles[index++] = next + 1;
                triangles[index++] = self;
                triangles[index++] = next + 1;
                triangles[index++] = self + 1;
            }
        }

        var mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();

        var terrain = new GameObject("Terrain");
        terrain.AddComponent<MeshFilter>().mesh = mesh;
        terrain.AddComponent<MeshRenderer>().sharedMaterial = diffuseMap;
        terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
        terrain.transform.position = new Vector3(rect.x, 0, rect.y);

        return mesh;
    }

    private void EdgeJoint(float[,] heightMap, Rect rect, float range)
    {

        var testRect = new Rect(rect.x - length, rect.y, rect.width, rect.height);
        var terrain = FindTerrain(allTerrain, testRect);
        if (terrain != null)
        {
            var vertices = terrain.mesh.vertices;
            for (int i = 0; i < count; i++)
            {
                heightMap[0, i] = vertices[(i + 1) * count - 1].y / range;
            }
        }

        testRect = new Rect(rect.x, rect.y - length, rect.width, rect.height);
        terrain = FindTerrain(allTerrain, testRect);
        if (terrain != null)
        {
            int min = (count - 1) * count;
            var vertices = terrain.mesh.vertices;
            for (int i = 0; i < count; i++)
            {
                heightMap[i, 0] = vertices[min + i].y / range;
            }
        }

        testRect = new Rect(rect.x + length, rect.y, rect.width, rect.height);
        terrain = FindTerrain(allTerrain, testRect);
        if (terrain != null)
        {
            var vertices = terrain.mesh.vertices;
            for (int i = 0; i < count; i++)
            {
                heightMap[count - 1, i] = vertices[i * count].y / range;
            }
        }

        testRect = new Rect(rect.x, rect.y + length, rect.width, rect.height);
        terrain = FindTerrain(allTerrain, testRect);
        if (terrain != null)
        {
            var vertices = terrain.mesh.vertices;
            for (int i = 0; i < count; i++)
            {
                heightMap[i, count - 1] = vertices[i].y / range;
            }
        }
    }

    private void Update()
    {
        var pos = player.position;

        if (pos.x < currRect.xMin)
        {
            currRect.x -= length;
        }
        else if (pos.z < currRect.yMin)
        {
            currRect.y -= length;
        }
        else if (pos.x > currRect.xMax)
        {
            currRect.x += length;
        }
        else if (pos.z > currRect.yMax)
        {
            currRect.y += length;
        }

        Rect newRect = currRect;
        if (pos.x - currRect.xMin < dis)
        {
            newRect.x -= length;
        }
        else if (pos.x - currRect.xMax > -dis)
        {
            newRect.x += length;
        }
        else if (pos.z - currRect.yMin < dis)
        {
            newRect.y -= length;
        }
        else if (pos.z - currRect.yMax > -dis)
        {
            newRect.y += length;
        }

        if (FindTerrain(allTerrain, newRect) == null)
        {
            var newTerrain = new Terrain()
            {
                rect = newRect,
                mesh = GenerateTerrain(newRect)
            };
            allTerrain.Add(newTerrain);
        }
    }

    private Terrain FindTerrain(List<Terrain> terrainList, Rect rect)
    {
        foreach (var terrain in terrainList)
        {
            if (terrain.rect == rect)
            {
                return terrain;
            }
        }

        return null;
    }
}

class Terrain
{
    public Rect rect;
    public Mesh mesh;

}

