﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material material;
    public Material waterMat;
    public float xLength = 100;
    public float yLength = 50;
    public int xCount = 20;
    public int yCount = 10;
    public float minHeight = -5;
    public float maxHeight = 5;
    public int seed = 152411;
    public float scale = 40;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public int octave = 4;
    public float pow = 1.6f;
    public float dis = 10;
    public float firstHeight = 30;
    public float secondHeight = 20;
    public float thirdHeight = 10;

    private SimplexNoise noise;
    private float[,] heights;                 //0 1 2
    private float[,] moisture;                //3 4 5
    private Transform player;                 //6 7 8
    private Rect[] rects = new Rect[9];       //0-8 is leftTop top rightTop left center right leftBottom bottom rightBottom
    private List<Terrain> allTerrain;

    private void Start()
    {
        noise = new SimplexNoise(scale, persistence, octave, pow, seed);
        heights = new float[xCount, yCount];
        moisture = new float[xCount, yCount];
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        player.position = new Vector3(player.position.x,
            noise.Get2DPow(player.position.x, player.position.z) * (maxHeight - minHeight), player.position.z);

        allTerrain = new List<Terrain>();
        rects[0] = new Rect(-xLength, yLength, xLength, yLength);
        rects[1] = new Rect(0, yLength, xLength, yLength);
        rects[2] = new Rect(xLength, yLength, xLength, yLength);
        rects[3] = new Rect(-xLength, 0, xLength, yLength);
        rects[4] = new Rect(0, 0, xLength, yLength);
        rects[5] = new Rect(xLength, 0, xLength, yLength);
        rects[6] = new Rect(-xLength, -yLength, xLength, yLength);
        rects[7] = new Rect(0, -yLength, xLength, yLength);
        rects[8] = new Rect(xLength, -yLength, xLength, yLength);
        foreach (var rect in rects)
        {
            GenerateTerrain(rect);
        }

        var center = FindTerrain(rects[4]);
        GenerateWater(center.terrainObject.GetComponent<MeshFilter>().mesh);
    }

    private void Update()
    {
        var pos = player.position;
        var center = rects[4];

        if (pos.x < center.xMin)
        {
            for (int i = 0; i < 9; i++)
            {
                rects[i].x -= xLength;
                FindTerrain(rects[i]);
            }
            DeleteTerrain();
        }
        else if (pos.z < center.yMin)
        {
            for (int i = 0; i < 9; i++)
            {
                rects[i].y -= yLength;
                FindTerrain(rects[i]);
            }
            DeleteTerrain();
        }
        else if (pos.x > center.xMax)
        {
            for (int i = 0; i < 9; i++)
            {
                rects[i].x += xLength;
                FindTerrain(rects[i]);
            }
            DeleteTerrain();
        }
        else if (pos.z > center.yMax)
        {
            for (int i = 0; i < 9; i++)
            {
                rects[i].y += yLength;
                FindTerrain(rects[i]);
            }
            DeleteTerrain();
        }


    }

    /// <summary>
    /// generate terrain in position vector3(rect.x,0,rect.y)
    /// </summary>
    private Terrain GenerateTerrain(Rect rect)
    {
        int vertexCount = xCount * yCount;
        float xTick = xLength / (xCount - 1);
        float yTick = yLength / (yCount - 1);
        float u = 1.0f / (xCount - 1);
        float v = 1.0f / (yCount - 1);
        var uvs = new Vector2[vertexCount];
        int index = 0;

        index = 0;
        var vertices = new Vector3[vertexCount];
        float range = maxHeight - minHeight;
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                var x = i * xTick;
                var y = j * yTick;
                var height = noise.Get2DPow(rect.x + x, rect.y + y) * range;
                heights[i, j] = height;
                moisture[i, j] = noise.Get2D(rect.x + x, rect.y + y);
                vertices[index] = new Vector3(x, height, y);
                uvs[index] = new Vector2(i * u, j * v);
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

        var mesh = new Mesh()
        {
            vertices = vertices,
            triangles = indices,
            uv = uvs
        };
        mesh.RecalculateNormals();

        var terrainObject = new GameObject("Terrain");
        terrainObject.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = terrainObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        renderer.material.SetTexture("_Mask", GetMaskTexture(heights));
        terrainObject.AddComponent<MeshCollider>().sharedMesh = mesh;
        terrainObject.transform.position = new Vector3(rect.x, 0, rect.y);

        var terrain = new Terrain()
        {
            rect = rect,
            terrainObject = terrainObject
        };
        allTerrain.Add(terrain);

        return terrain;
    }

    //private void EdgeJoint(float[,] heightMap, Rect rect, float range)
    //{

    //    var testRect = new Rect(rect.x - length, rect.y, rect.width, rect.height);
    //    var terrain = FindTerrain(allTerrain, testRect);
    //    if (terrain != null)
    //    {
    //        var vertices = terrain.mesh.vertices;
    //        for (int i = 0; i < count; i++)
    //        {
    //            heightMap[0, i] = vertices[(i + 1) * count - 1].y / range;
    //        }
    //        int width = count / 4;
    //        for (int i = 1; i < count - 1; i++)
    //        {
    //            for (int j = 1; j < width; j++)
    //            {
    //                heightMap[j, i] = (heightMap[j - 1, i] + heightMap[j + 1, i]) / 2;
    //            }
    //        }
    //    }

    //    testRect = new Rect(rect.x, rect.y - length, rect.width, rect.height);
    //    terrain = FindTerrain(allTerrain, testRect);
    //    if (terrain != null)
    //    {
    //        int min = (count - 1) * count;
    //        var vertices = terrain.mesh.vertices;
    //        for (int i = 0; i < count; i++)
    //        {
    //            heightMap[i, 0] = vertices[min + i].y / range;
    //        }
    //        int width = count / 4;
    //        for (int i = 1; i < count - 1; i++)
    //        {
    //            for (int j = 1; j < width; j++)
    //            {
    //                heightMap[i, j] = (heightMap[i, j - 1] + heightMap[i, j + 1]) / 2;
    //            }
    //        }
    //    }

    //    testRect = new Rect(rect.x + length, rect.y, rect.width, rect.height);
    //    terrain = FindTerrain(allTerrain, testRect);
    //    if (terrain != null)
    //    {
    //        var vertices = terrain.mesh.vertices;
    //        for (int i = 0; i < count; i++)
    //        {
    //            heightMap[count - 1, i] = vertices[i * count].y / range;
    //        }
    //        int width = count / 4;
    //        for (int i = 1; i < count - 1; i++)
    //        {
    //            for (int j = count - 2; j >= count - width; j--)
    //            {
    //                heightMap[j, i] = (heightMap[j - 1, i] + heightMap[j + 1, i]) / 2;
    //            }
    //        }
    //    }

    //    testRect = new Rect(rect.x, rect.y + length, rect.width, rect.height);
    //    terrain = FindTerrain(allTerrain, testRect);
    //    if (terrain != null)
    //    {
    //        var vertices = terrain.mesh.vertices;
    //        for (int i = 0; i < count; i++)
    //        {
    //            heightMap[i, count - 1] = vertices[i].y / range;
    //        }
    //        int width = count / 4;
    //        for (int i = 1; i < count - 1; i++)
    //        {
    //            for (int j = count - 2; j >= count - width; j--)
    //            {
    //                heightMap[i, j] = (heightMap[i, j - 1] + heightMap[i, j + 1]) / 2;
    //            }
    //        }
    //    }
    //}

    private Texture2D GetMaskTexture(float[,] heightMap)
    {
        var texture = new Texture2D(xCount, yCount, TextureFormat.ARGB32, false);
        var red = new Color32(255, 0, 0, 0);
        var green = new Color32(0, 255, 0, 0);
        var blue = new Color32(0, 0, 255, 0);
        var alpha = new Color32(0, 0, 0, 255);
        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                if (heightMap[i, j] > firstHeight)
                {
                    texture.SetPixel(i, j, red);
                }
                else if (heightMap[i, j] > secondHeight)
                {
                    if (moisture[i, j] > 0.7f)
                    {
                        texture.SetPixel(i, j, blue);
                    }
                    else
                    {
                        texture.SetPixel(i, j, green);
                    }

                }
                else if (heightMap[i, j] > thirdHeight)
                {
                    if (moisture[i, j] > 0.4f)
                    {
                        texture.SetPixel(i, j, blue);
                    }
                    else
                    {
                        texture.SetPixel(i, j, green);
                    }
                }
                else
                {
                    texture.SetPixel(i, j, alpha);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    private Terrain FindTerrain(Rect rect)
    {
        foreach (var terrain in allTerrain)
        {
            if (terrain.rect == rect)
            {
                return terrain;
            }
        }

        return GenerateTerrain(rect);
    }

    private void DeleteTerrain()
    {
        for (int i = allTerrain.Count - 1; i >= 0; i--)
        {
            bool exist = false;
            foreach (var rect in rects)
            {
                if (allTerrain[i].rect == rect)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                Destroy(allTerrain[i].terrainObject);
                allTerrain.RemoveAt(i);
            }

        }
    }
    
    private void GenerateWater(Mesh terrainMesh)
    {
        var terrainVertices = terrainMesh.vertices;
        var terrainIndices = terrainMesh.triangles;
        int vertexCount = (xCount - 1) * (yCount - 1) * 6;

        var vertexList = new List<Vector3>();
        var indexList = new List<int>();
        
        int index = 0;
        for (int i = 0; i < vertexCount; i += 3)
        {
            int index1 = terrainIndices[i];
            int index2 = terrainIndices[i + 1];
            int index3 = terrainIndices[i + 2];
            var vertex1 = terrainVertices[index1];
            var vertex2 = terrainVertices[index2];
            var vertex3 = terrainVertices[index3];
            if (vertex1.y < thirdHeight ||
                vertex2.y < thirdHeight ||
                vertex3.y < thirdHeight)
            {
                vertexList.Add(new Vector3(vertex1.x, 0, vertex1.z));
                vertexList.Add(new Vector3(vertex2.x, 0, vertex2.z));
                vertexList.Add(new Vector3(vertex3.x, 0, vertex3.z));
                indexList.Add(index++);
                indexList.Add(index++);
                indexList.Add(index++);
            }
        }

        var vertices = vertexList.ToArray();
        var indices = indexList.ToArray();

        var mesh = new Mesh()
        {
            vertices = vertices,
            triangles = indices,
        };

        var waterObject = new GameObject("Water");
        waterObject.AddComponent<MeshFilter>().mesh = mesh;
        waterObject.AddComponent<MeshRenderer>().sharedMaterial = waterMat;
        waterObject.transform.position = new Vector3(0, thirdHeight,0);
    }
    
}

class Terrain
{
    public Rect rect;
    public GameObject terrainObject;
}

internal struct Point2Int
{
    public int x, y;

    public Point2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static int SqrDistance(Point2Int p1, Point2Int p2)
    {
        var x = p1.x - p2.x;
        var y = p1.y - p2.y;
        return x * x + y * y;
    }

    public override bool Equals(object obj)
    {
        return this == (Point2Int)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Point2Int left, Point2Int right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return true;
        }

        return false;
    }

    public static bool operator !=(Point2Int left, Point2Int right)
    {
        if (left.x != right.x || left.y != right.y)
        {
            return true;
        }

        return false;
    }
}
