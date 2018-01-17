using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public float xLength = 100;
    public float yLength = 100;
    [Header("must be 2^n + 1")]
    public int xCount = 33;
    public int yCount = 33;
    public int lodCount = 2;

    private TerrainBuilder builder;
    private Transform player;
    private Terrain[][] allTerrain;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        builder = GetComponent<TerrainBuilder>();
        builder.Initial(xLength, yLength, xCount, yCount, lodCount);
        allTerrain = new Terrain[][]
        {
            new Terrain[]{ new Terrain(1, MeshType.Original), new Terrain(1, MeshType.Original),   new Terrain(1, MeshType.Original), new Terrain(1, MeshType.Original),    new Terrain(1, MeshType.Original)},
            new Terrain[]{ new Terrain(1, MeshType.Original), new Terrain(0, MeshType.LeftTop),    new Terrain(0, MeshType.Top),      new Terrain(0, MeshType.RightTop),    new Terrain(1, MeshType.Original)},
            new Terrain[]{ new Terrain(1, MeshType.Original), new Terrain(0, MeshType.Left),       new Terrain(0, MeshType.Original), new Terrain(0, MeshType.Right),       new Terrain(1, MeshType.Original)},
            new Terrain[]{ new Terrain(1, MeshType.Original), new Terrain(0, MeshType.LeftBottom), new Terrain(0, MeshType.Bottom),   new Terrain(0, MeshType.RightBottom), new Terrain(1, MeshType.Original)},
            new Terrain[]{ new Terrain(1, MeshType.Original), new Terrain(1, MeshType.Original),   new Terrain(1, MeshType.Original), new Terrain(1, MeshType.Original),    new Terrain(1, MeshType.Original)},
        };

        for (int i = 0; i < 5; i++)
        {
            var row = allTerrain[i];
            for (int j = 0; j < 5; j++)
            {
                row[j].rect = new Rect(-2 * xLength + j * xLength, 2 * yLength - i * yLength, xLength, yLength);
                builder.Build(row[j]);
            }
        }
        
    }

    private void Update()
    {
        var pos = player.position;
        var center = allTerrain[2][2].rect;

        if (pos.x < center.xMin)
        {
            for (int i = 0; i < 5; i++)
            {
                var row = allTerrain[i];
                //delete row[4]
                Delete(row[4]);
                for (int j = 4; j >= 1; j--)
                {
                    row[j].rect = row[j - 1].rect;
                    row[j].mesh = row[j - 1].mesh;
                    row[j].terrainObject = row[j - 1].terrainObject;
                    row[j].plantObjects = row[j - 1].plantObjects;
                    row[j].dragonObject = row[j - 1].dragonObject;
                    builder.UpdateMesh(row[j]);
                }
                row[0].rect.x -= xLength;
                builder.Build(row[0]);
            }
        }
        else if (pos.z < center.yMin)
        {
            for (int j = 0; j < 5; j++)
            {
                //delete allTerrain[0][j]
                Delete(allTerrain[0][j]);
                for (int i = 0; i < 4; i++)
                {
                    allTerrain[i][j].rect = allTerrain[i+1][j].rect;
                    allTerrain[i][j].mesh = allTerrain[i+1][j].mesh;
                    allTerrain[i][j].terrainObject = allTerrain[i + 1][j].terrainObject;
                    allTerrain[i][j].plantObjects = allTerrain[i + 1][j].plantObjects;
                    allTerrain[i][j].dragonObject = allTerrain[i + 1][j].dragonObject;
                    builder.UpdateMesh(allTerrain[i][j]);
                }
                allTerrain[4][j].rect.y -= yLength;
                builder.Build(allTerrain[4][j]);
            }
        }
        else if (pos.x > center.xMax)
        {
            for (int i = 0; i < 5; i++)
            {
                var row = allTerrain[i];
                //delete row[0]
                Delete(row[0]);
                for (int j = 0; j < 4; j++)
                {
                    row[j].rect = row[j + 1].rect;
                    row[j].mesh = row[j + 1].mesh;
                    row[j].terrainObject = row[j + 1].terrainObject;
                    row[j].plantObjects = row[j + 1].plantObjects;
                    row[j].dragonObject = row[j + 1].dragonObject;
                    builder.UpdateMesh(row[j]);
                }
                row[4].rect.x += xLength;
                builder.Build(row[4]);
            }
        }
        else if (pos.z > center.yMax)
        {
            for (int j = 0; j < 5; j++)
            {
                //delete allTerrain[4][j]
                Delete(allTerrain[4][j]);
                for (int i = 4; i >= 1; i--)
                {
                    allTerrain[i][j].rect = allTerrain[i - 1][j].rect;
                    allTerrain[i][j].mesh = allTerrain[i - 1][j].mesh;
                    allTerrain[i][j].terrainObject = allTerrain[i - 1][j].terrainObject;
                    allTerrain[i][j].plantObjects = allTerrain[i - 1][j].plantObjects;
                    allTerrain[i][j].dragonObject = allTerrain[i - 1][j].dragonObject;
                    builder.UpdateMesh(allTerrain[i][j]);
                }
                allTerrain[0][j].rect.y += yLength;
                builder.Build(allTerrain[0][j]);
            }
        }


    }

    private void Delete(Terrain terrain)
    {
        Destroy(terrain.terrainObject);
        Destroy(terrain.dragonObject);
        for (int i = 0; i < terrain.plantObjects.Count; i++)
        {
            terrain.plantObjects[i].transform.SetParent(transform);
            terrain.plantObjects[i].SetActive(false);
        }
    }

}
