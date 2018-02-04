using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public Material material;
    public Material waterMat;
    public GameObject dragon;
    public GameObject skeleton;

    private int seed = 152411;
    private float[] frequencys;
    private float[] amplitudes;
    private float firstHeight = 120;
    private float secondHeight = 75;
    private float thirdHeight = 30;
    private int plantScale = 3;
    private float tree0Random = 0.95f;
    private float tree1Random = 0.9f;
    private float bushRandom = 0.8f;
    private float dragonRandom = 0.9f;
    private int skeletonScale = 10;
    private float skeletonRandom = 0.5f;

    private int xCount, yCount;
    private Indices indices;
    private int[][][] lodIndices;
    private SimplexNoise noise;
    private float[,] heights;
    private float[,] moisture;
    private PlantPool plantPool;
    private ChaseMesh chaseMesh;

    public void Initial(int xCount, int yCount, int lodCount)
    {
        this.xCount = xCount;
        this.yCount = yCount;
        lodIndices = new int[lodCount][][];
        indices = new Indices(xCount, yCount);
        for (int i = 0; i < lodCount; i++)
        {
            lodIndices[i] = indices.InitialIndices(i);
        }

        LoadParameter();

        noise = new SimplexNoise(seed, frequencys, amplitudes);
        heights = new float[xCount, yCount];
        moisture = new float[xCount, yCount];
        plantPool = GetComponent<PlantPool>();
        plantPool.Initial();
        chaseMesh = new ChaseMesh(xCount, yCount);

        gameObject.AddComponent<Reflection>().Initial(thirdHeight);
        gameObject.AddComponent<Refraction>().Initial(thirdHeight);
    }

    private void LoadParameter()
    {
        using (var fs = File.OpenRead(Application.streamingAssetsPath + "/terrainBuilder.txt"))
        {
            char c = ' ';
            var sr = new StreamReader(fs);
            seed = Convert.ToInt32(sr.ReadLine().Split(c)[1]);
            var str = sr.ReadLine().Split(c);
            frequencys = new float[str.Length - 1];
            for (int i = 0; i < str.Length - 1; i++)
            {
                frequencys[i] = Convert.ToSingle(str[i + 1]);
            }
            str = sr.ReadLine().Split(c);
            amplitudes = new float[str.Length - 1];
            for (int i = 0; i < str.Length - 1; i++)
            {
                amplitudes[i] = Convert.ToSingle(str[i + 1]);
            }
            firstHeight = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            secondHeight = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            thirdHeight = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            plantScale = Convert.ToInt32(sr.ReadLine().Split(c)[1]);
            tree0Random = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            tree1Random = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            bushRandom = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            dragonRandom = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
            skeletonScale = Convert.ToInt32(sr.ReadLine().Split(c)[1]);
            skeletonRandom = Convert.ToSingle(sr.ReadLine().Split(c)[1]);
        }
    }

    public async void Build(Terrain terrain)
    {
        var rect = terrain.rect;
        var terrainComputing = new TerrainComputing(rect, xCount, yCount, noise);
        var terrainResult = await terrainComputing.ComputeAsync();
        var nodes = terrainResult.nodes;
        heights = terrainResult.heights;
        moisture = terrainResult.moisture;
        terrain.vertices = terrainResult.vertices;

        var mesh = new Mesh()
        {
            vertices = terrainResult.vertices,
            triangles = lodIndices[terrain.lod][(int)terrain.type],
            uv = terrainResult.uvs,
        };
        mesh.RecalculateNormals();
        terrain.mesh = mesh;

        var terrainObject = new GameObject("Terrain");
        terrainObject.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = terrainObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        var texture = GetMaskTexture();
        renderer.material.SetTexture("_Mask", texture);
        terrain.shader = renderer.material.shader;

        terrainObject.transform.position = new Vector3(rect.x, 0, rect.y);
        terrainObject.layer = LayerMask.NameToLayer("Terrain");

        SetWater(mesh, terrainObject.transform);
        terrain.plantObjects = SetPlant(rect, texture, terrainObject.transform, nodes);
        terrain.plantObjects.AddRange(SetGrass(rect, texture, terrainObject.transform, mesh, nodes));

        terrain.terrainObject = terrainObject;

        if (terrain.lod == 0)
        {
            terrainObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            await chaseMesh.AddDataAsync(rect, nodes);
            terrain.enemyObjects = SetEnemy(rect, nodes);
        }
        else
        {
            terrain.shader.maximumLOD = 110;
        }
    }

    public void UpdateTerrain(Terrain terrain)
    {
        terrain.mesh.triangles = lodIndices[terrain.lod][(int)terrain.type];
        terrain.mesh.RecalculateNormals();

        var meshCollider = terrain.terrainObject.GetComponent<MeshCollider>();
        if (terrain.lod == 0)
        {
            var nodes = chaseMesh.BuildMesh(terrain.rect, terrain.vertices, terrain.plantObjects);
            if (nodes != null) 
            {
                terrain.enemyObjects = SetEnemy(terrain.rect, nodes);
            }

            if (meshCollider == null)
            {
                terrain.terrainObject.AddComponent<MeshCollider>().sharedMesh = terrain.mesh;
            }
            terrain.shader.maximumLOD = 220;
        }
        else
        {
            if (meshCollider != null)
            {
                Destroy(meshCollider);
            }
            terrain.shader.maximumLOD = 110;
        }
    }

    public void Delete(Terrain terrain)
    {
        for (int i = 0; i < terrain.plantObjects.Count; i++)
        {
            plantPool.ReuseInCache(terrain.plantObjects[i]);
        }
        for (int i = 0; i < terrain.enemyObjects.Count; i++)
        {
            Destroy(terrain.enemyObjects[i]);
        }
        chaseMesh.RemoveData(terrain.rect);
        Destroy(terrain.terrainObject);
    }

    private Texture2D GetMaskTexture()
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
                if (heights[i, j] > firstHeight)
                {
                    texture.SetPixel(i, j, red);
                }
                else if (heights[i, j] > secondHeight)
                {
                    if (moisture[i, j] > 0.5f)
                    {
                        texture.SetPixel(i, j, blue);
                    }
                    else
                    {
                        texture.SetPixel(i, j, green);
                    }

                }
                else if (heights[i, j] > thirdHeight)
                {
                    if (moisture[i, j] > 0.2f)
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

    private async void SetWater(Mesh terrainMesh, Transform terrainTransform)
    {
        var terrainVertices = terrainMesh.vertices;
        var terrainIndices = terrainMesh.triangles;

        var waterComputing = new WaterComputing(terrainVertices, terrainIndices, thirdHeight);
        var water = await waterComputing.ComputeAsync();

        if (water == null)
        {
            return;
        }

        var mesh = new Mesh()
        {
            vertices = water.vertices,
            triangles = water.indices,
        };

        var waterObject = new GameObject("Water");
        waterObject.AddComponent<MeshFilter>().mesh = mesh;
        waterObject.AddComponent<MeshRenderer>().sharedMaterial = waterMat;
        waterObject.AddComponent<Water>();
        waterObject.layer = LayerMask.NameToLayer("Water");
        waterObject.transform.position = new Vector3(0, thirdHeight, 0);
        waterObject.transform.SetParent(terrainTransform, false);

    }

    private List<GameObject> SetPlant(Rect rect, Texture2D texture, Transform terrainTransform, Node[,] nodes)
    {
        var plantList = new List<GameObject>();
        var green = new Color32(0, 255, 0, 0);
        var blue = new Color32(0, 0, 255, 0);
        float xTick = rect.width / (xCount - 1);
        float yTick = rect.height / (yCount - 1);

        for (int j = 1; j < yCount - 1; j += plantScale)//不在边界上种树，node的walkable不方便设置
        {
            for (int i = 1; i < xCount - 1; i += plantScale)
            {
                var height = heights[i, j];
                var random = height - Mathf.Floor(height);
                var color = texture.GetPixel(i, j);
                GameObject plant = null;
                if (color == green)
                {
                    if (random > tree0Random)
                    {
                        plant = plantPool.GetTree(0);
                    }
                    else if (random > bushRandom)
                    {   //the range of random is (bushRandom, tree0Random)  
                        plant = plantPool.GetBush((random - bushRandom) / (tree0Random - bushRandom));
                    }
                }
                else if (color == blue)
                {
                    if (random > tree1Random)
                    {
                        plant = plantPool.GetTree(1);
                    }
                    else if (random > bushRandom)
                    {
                        plant = plantPool.GetBush((random - bushRandom) / (tree1Random - bushRandom));
                    }
                }

                if (plant != null)
                {
                    plant.transform.position = new Vector3(rect.x + i * xTick, height, rect.y + j * yTick);
                    plant.transform.SetParent(terrainTransform);
                    plantList.Add(plant);
                    nodes[i, j].isWalkable = false;
                    nodes[i - 1, j].isWalkable = false;
                    nodes[i + 1, j].isWalkable = false;
                    nodes[i, j - 1].isWalkable = false;
                    nodes[i, j + 1].isWalkable = false;
                }
            }
        }
        return plantList;
    }

    private List<GameObject> SetGrass(Rect rect, Texture2D texture, Transform terrainTransform, Mesh mesh, Node[,] nodes)
    {
        var plantList = new List<GameObject>();
        var blue = new Color32(0, 0, 255, 0);
        float xTick = rect.width / (xCount - 1);
        float yTick = rect.height / (yCount - 1);
        var normals = mesh.normals;

        int index = 0;
        for (int j = 0; j < yCount; j++)
        {
            for (int i = 0; i < xCount; i++)
            {
                if (nodes[i, j].isWalkable && texture.GetPixel(i, j) == blue)
                {
                    var height = heights[i, j];
                    var normal = normals[index++];
                    var plant = plantPool.GetGrass();
                    plant.transform.position = new Vector3(rect.x + i * xTick, height, rect.y + j * yTick);
                    var rotation = Quaternion.FromToRotation(Vector3.up, normal);
                    plant.transform.rotation *= rotation;
                    plant.transform.SetParent(terrainTransform);
                    plantList.Add(plant);
                }
            }
        }
        return plantList;
    }

    private List<GameObject> SetEnemy(Rect rect, Node[,] nodes)
    {
        var enemyList = new List<GameObject>();
        int i = xCount / 2;
        int j = yCount / 2;
        var xTick = rect.width / (xCount - 1);
        var yTick = rect.height / (yCount - 1);
        var height = nodes[i, j].center.y;
        if (height - Mathf.Floor(height) > dragonRandom && nodes[i, j].isWalkable)
        {
            var sourcePosition = new Vector3(rect.x + i * xTick, height, rect.y + j * yTick);
            enemyList.Add(Instantiate(dragon, sourcePosition, Quaternion.identity));
        }
        var ss = skeletonScale / 2;
        for (i = ss; i < xCount - ss; i += skeletonScale)
        {
            for (j = ss; j < yCount - ss; j += skeletonScale)
            {
                height = nodes[i, j].center.y;
                if (height - Mathf.Floor(height) < skeletonRandom && nodes[i, j].isWalkable)
                {
                    var sourcePosition = new Vector3(rect.x + i * xTick, height, rect.y + j * yTick);
                    enemyList.Add(Instantiate(skeleton, sourcePosition, Quaternion.identity));
                }
            }
        }
        
        return enemyList;
    }

}

public enum MeshType
{
    Original,
    LeftTop,
    Top,
    RightTop,
    Left,
    Center,
    Right,
    LeftBottom,
    Bottom,
    RightBottom,
    Count
}

public class Terrain
{
    public int lod;
    public MeshType type;
    public Rect rect;
    public Mesh mesh;
    public Shader shader;
    public GameObject terrainObject;
    public List<GameObject> plantObjects;
    public List<GameObject> enemyObjects;
    public Vector3[] vertices;

    public Terrain(int lod, MeshType type)
    {
        this.lod = lod;
        this.type = type;
        plantObjects = new List<GameObject>();
        enemyObjects = new List<GameObject>();
    }
}