using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public Material material;
    public Material waterMat;
    public GameObject dragon;
    public GameObject skeleton;
    public int seed = 152411;
    public float[] frequencys = new float[] { 0.0001f, 0.005f, 0.05f, 0.5f };
    public float[] amplitudes = new float[] { 100, 50, 5, 1 };
    public float firstHeight = 120;
    public float secondHeight = 75;
    public float thirdHeight = 30;
    public int plantScale = 3;
    public float tree0Random = 0.95f;
    public float tree1Random = 0.9f;
    public float bushRandom = 0.8f;
    public float dragonRandom = 0.9f;
    public int skeletonScale = 10;
    public float skeletonRandom = 0.5f;

    private int[][][] lodIndices;

    private int xCount, yCount;
    private int xGap, yGap;
    private SimplexNoise noise;
    private float[,] heights;
    private float[,] moisture;
    private PlantPool plantPool;
    private ChaseMesh chaseMesh;

    public void Initial(int xCount, int yCount, int lodCount)
    {
        this.xCount = xCount;
        this.yCount = yCount;
        xGap = xCount - 1;
        yGap = yCount - 1;
        lodIndices = new int[lodCount][][];
        for (int i = 0; i < lodCount; i++)
        {
            lodIndices[i] = InitialIndices(i);
        }

        noise = new SimplexNoise(seed, frequencys, amplitudes);
        heights = new float[xCount, yCount];
        moisture = new float[xCount, yCount];
        plantPool = GetComponent<PlantPool>();
        plantPool.InitialPool();
        chaseMesh = new ChaseMesh(xCount, yCount);
    }

    public async void Build(Terrain terrain)
    {
        var rect = terrain.rect;
        var computing = await ComputeAsync(rect);
        var nodes = computing.nodes;
        heights = computing.heights;
        moisture = computing.moisture;

        var mesh = new Mesh()
        {
            vertices = computing.vertices,
            triangles = lodIndices[terrain.lod][(int)terrain.type],
            uv = computing.uvs,
        };
        mesh.RecalculateNormals();

        var terrainObject = new GameObject("Terrain");
        terrainObject.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = terrainObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        var texture = GetMaskTexture();
        renderer.material.SetTexture("_Mask", texture);

        terrainObject.transform.position = new Vector3(rect.x, 0, rect.y);
        terrainObject.layer = LayerMask.NameToLayer("Terrain");

        SetWater(mesh, terrainObject.transform);
        terrain.plantObjects = SetPlant(rect, texture, terrainObject.transform, nodes);

        terrain.mesh = mesh;
        terrain.terrainObject = terrainObject;

        if (terrain.lod == 0)
        {
            terrainObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            await chaseMesh.AddDataAsync(rect, nodes);
            terrain.enemyObjects = SetEnemy(rect, nodes);
        }
    }

    private Task<AsyncComputing> ComputeAsync(Rect rect)
    {
        var asyncComputing = new AsyncComputing(rect, xCount, yCount, noise);
        return Task.Run(new System.Func<AsyncComputing>(asyncComputing.Compute));
    }

    public async void UpdateTerrain(Terrain terrain)
    {
        terrain.mesh.triangles = lodIndices[terrain.lod][(int)terrain.type];
        terrain.mesh.RecalculateNormals();

        var meshCollider = terrain.terrainObject.GetComponent<MeshCollider>();
        if (terrain.lod == 0)
        {
            if (meshCollider == null)
            {
                terrain.terrainObject.AddComponent<MeshCollider>().sharedMesh = terrain.mesh;
                var nodes = await chaseMesh.BuildMeshAsync(terrain.rect, terrain.terrainObject, terrain.plantObjects);
                terrain.enemyObjects = SetEnemy(terrain.rect, nodes);
            }
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

    private GameObject SetWater(Mesh terrainMesh, Transform terrainTransform)
    {
        var terrainVertices = terrainMesh.vertices;
        var terrainIndices = terrainMesh.triangles;

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

        if (vertexList.Count == 0)
        {
            return null;
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
        waterObject.layer = LayerMask.NameToLayer("Water");
        waterObject.transform.position = new Vector3(0, thirdHeight, 0);
        waterObject.transform.SetParent(terrainTransform, false);

        return waterObject;
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
                if (color == blue)
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
                else if (color == green)
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

    private List<GameObject> SetEnemy(Rect rect, Node[,] nodes)
    {
        var enemyList = new List<GameObject>();
        int i = xCount / 2;
        int j = yCount / 2;
        var xTick = rect.width / (xCount - 1);
        var yTick = rect.height / (yCount - 1);
        var height = nodes[i, j].center.y;
        if (height - Mathf.Floor(height) > dragonRandom)
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
                if (height - Mathf.Floor(height) < skeletonRandom)
                {
                    var sourcePosition = new Vector3(rect.x + i * xTick, height, rect.y + j * yTick);
                    enemyList.Add(Instantiate(skeleton, sourcePosition, Quaternion.identity));
                }
            }
        }

        return enemyList;
    }

    #region set lod indices
    private int[][] InitialIndices(int lod)
    {
        int scale = 1 << lod;
        int currXGap = xGap / scale;
        int currYGap = yGap / scale;
        if (currXGap < 4 || currYGap < 4)
        {
            return null;
        }
        int totalIndexCount = currXGap * currYGap * 6;
        int xdecrease = currXGap / 2 * 3;
        int ydecrease = currYGap / 2 * 3;
        var original = new int[totalIndexCount];
        SetCenterIndices(original, 0, 0, xGap, 0, yGap, 1 * scale);

        int index = 0;
        var leftTop = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(leftTop, index, scale, xGap, 0, yGap - scale, scale);
        index = SetLeftIndices(leftTop, index, 0, yGap - 2 * scale, scale);
        index = SetTopIndices(leftTop, index, 2 * scale, xGap, scale);
        index = SetLeftTopIndices(leftTop, index, scale);

        index = 0;
        var top = new int[totalIndexCount - xdecrease];
        index = SetCenterIndices(top, index, 0, xGap, 0, yGap - scale, scale);
        index = SetTopIndices(top, index, 0, xGap, scale);

        index = 0;
        var rightTop = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(rightTop, index, 0, xGap - scale, 0, yGap - scale, scale);
        index = SetTopIndices(rightTop, index, 0, xGap - 2 * scale, scale);
        index = SetRightTopIndices(rightTop, index, scale);
        index = SetRightIndices(rightTop, index, 0, yGap - 2 * scale, scale);

        index = 0;
        var left = new int[totalIndexCount - ydecrease];
        index = SetCenterIndices(left, index, scale, xGap, 0, yGap, scale);
        index = SetLeftIndices(left, index, 0, yGap, scale);

        index = 0;
        var center = new int[totalIndexCount - (currXGap + currYGap) * 3];
        index = SetCenterIndices(center, index, scale, xGap - scale, scale, yGap - scale, scale);
        index = SetLeftIndices(center, index, 2 * scale, yGap - 2 * scale, scale);
        index = SetTopIndices(center, index, 2 * scale, xGap - 2 * scale, scale);
        index = SetRightIndices(center, index, 2 * scale, yGap - 2 * scale, scale);
        index = SetBottomIndices(center, index, 2 * scale, xGap - 2 * scale, scale);
        index = SetLeftTopIndices(center, index, scale);
        index = SetRightTopIndices(center, index, scale);
        index = SetLeftBottomIndices(center, index, scale);
        index = SetRightBottomIndices(center, index, scale);

        index = 0;
        var right = new int[totalIndexCount - ydecrease];
        index = SetCenterIndices(right, index, 0, xGap - scale, 0, yGap, scale);
        index = SetRightIndices(right, index, 0, yGap, scale);

        index = 0;
        var leftBottom = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(leftBottom, index, scale, xGap, scale, yGap, scale);
        index = SetLeftIndices(leftBottom, index, 2 * scale, yGap, scale);
        index = SetLeftBottomIndices(leftBottom, index, scale);
        index = SetBottomIndices(leftBottom, index, 2 * scale, xGap, scale);

        index = 0;
        var bottom = new int[totalIndexCount - xdecrease];
        index = SetCenterIndices(bottom, index, 0, xGap, scale, yGap, scale);
        index = SetBottomIndices(bottom, index, 0, xGap, scale);

        index = 0;
        var rightBottom = new int[totalIndexCount - xdecrease - ydecrease];
        index = SetCenterIndices(rightBottom, index, 0, xGap - scale, scale, yGap, scale);
        index = SetBottomIndices(rightBottom, index, 0, xGap - 2 * scale, scale);
        index = SetRightBottomIndices(rightBottom, index, scale);
        index = SetRightIndices(rightBottom, index, 2 * scale, yGap, scale);

        var allIndices = new int[(int)MeshType.Count][];
        allIndices[(int)MeshType.Original] = original;
        allIndices[(int)MeshType.LeftTop] = leftTop;
        allIndices[(int)MeshType.Top] = top;
        allIndices[(int)MeshType.RightTop] = rightTop;
        allIndices[(int)MeshType.Left] = left;
        allIndices[(int)MeshType.Center] = center;
        allIndices[(int)MeshType.Right] = right;
        allIndices[(int)MeshType.LeftBottom] = leftBottom;
        allIndices[(int)MeshType.Bottom] = bottom;
        allIndices[(int)MeshType.RightBottom] = rightBottom;

        return allIndices;
    }

    private int SetCenterIndices(int[] indices, int startIndex, int xStart, int xEnd, int yStart, int yEnd, int offset)
    {
        for (int i = xStart; i < xEnd; i += offset)
        {
            for (int j = yStart; j < yEnd; j += offset)
            {
                int self = i + (j * xCount);
                int next = i + ((j + offset) * xCount);
                indices[startIndex++] = self;
                indices[startIndex++] = next;
                indices[startIndex++] = self + offset;
                indices[startIndex++] = self + offset;
                indices[startIndex++] = next;
                indices[startIndex++] = next + offset;
            }
        }
        return startIndex;
    }

    private int SetLeftIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int j = start; j < end; j += offset * 2)
        {
            int self = 0 + j * xCount;
            int next = 0 + (j + offset) * xCount;
            int nextNext = 0 + (j + 2 * offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = self;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = nextNext + offset;
        }
        return startIndex;
    }

    private int SetTopIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int i = start; i < end; i += offset * 2)
        {
            int self = i + (yGap - offset) * xCount;
            int next = i + (yGap - offset + offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = next + 2 * offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next + 2 * offset;
            indices[startIndex++] = self + 2 * offset;
        }
        return startIndex;
    }

    private int SetRightIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int j = start; j < end; j += offset * 2)
        {
            int self = xGap - offset + j * xCount;
            int next = xGap - offset + (j + offset) * xCount;
            int nextNext = xGap - offset + (j + 2 * offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = nextNext + offset;
            indices[startIndex++] = self + offset;
            indices[startIndex++] = next;
            indices[startIndex++] = nextNext;
            indices[startIndex++] = nextNext + offset;
        }
        return startIndex;
    }

    private int SetBottomIndices(int[] indices, int startIndex, int start, int end, int offset)
    {
        for (int i = start; i < end; i += offset * 2)
        {
            int self = i + 0 * xCount;
            int next = i + (0 + offset) * xCount;
            indices[startIndex++] = self;
            indices[startIndex++] = next;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = self + 2 * offset;
            indices[startIndex++] = self + 2 * offset;
            indices[startIndex++] = next + offset;
            indices[startIndex++] = next + 2 * offset;
        }
        return startIndex;
    }

    private int SetLeftTopIndices(int[] indices, int startIndex, int offset)
    {
        int self = 0 + (yGap - 2 * offset) * xCount;
        int next = 0 + (yGap - offset) * xCount;
        int nextNext = 0 + (yGap) * xCount;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + 2 * offset;

        return startIndex;
    }

    private int SetRightTopIndices(int[] indices, int startIndex, int offset)
    {
        int self = xGap - 2 * offset + (yGap - 2 * offset) * xCount;
        int next = xGap - 2 * offset + (yGap - offset) * xCount;
        int nextNext = xGap - 2 * offset + (yGap) * xCount;
        indices[startIndex++] = next;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        return startIndex;
    }

    private int SetLeftBottomIndices(int[] indices, int startIndex, int offset)
    {
        int self = 0 + 0 * xCount;
        int next = 0 + offset * xCount;
        int nextNext = 0 + 2 * offset * xCount;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = nextNext + offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = nextNext;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = next + 2 * offset;
        return startIndex;
    }

    private int SetRightBottomIndices(int[] indices, int startIndex, int offset)
    {
        int self = xGap - 2 * offset + 0 * xCount;
        int next = xGap - 2 * offset + offset * xCount;
        int nextNext = xGap - 2 * offset + 2 * offset * xCount;
        indices[startIndex++] = self;
        indices[startIndex++] = next;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = self + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        indices[startIndex++] = next + offset;
        indices[startIndex++] = nextNext + offset;
        indices[startIndex++] = nextNext + 2 * offset;
        return startIndex;
    }
    #endregion
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
    public readonly int lod;
    public readonly MeshType type;
    public Rect rect;
    public Mesh mesh;
    public GameObject terrainObject;
    public List<GameObject> plantObjects;
    public List<GameObject> enemyObjects;

    public Terrain(int lod, MeshType type)
    {
        this.lod = lod;
        this.type = type;
        plantObjects = new List<GameObject>();
        enemyObjects = new List<GameObject>();
    }
}

public class AsyncComputing
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public Node[,] nodes;
    public float[,] heights;
    public float[,] moisture;

    private Rect rect;
    private int xCount, yCount;
    private SimplexNoise noise;

    public AsyncComputing(Rect rect, int xCount, int yCount, SimplexNoise noise)
    {
        this.rect = rect;
        this.xCount = xCount;
        this.yCount = yCount;
        this.noise = noise;
    }

    public AsyncComputing Compute()
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