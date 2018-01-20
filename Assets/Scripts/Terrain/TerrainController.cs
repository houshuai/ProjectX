using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [Header("must be 2^n + 1")]
    public int xCount = 33;
    public int yCount = 33;
    public float xTick = 1.0f;
    public float yTick = 1.0f;
    public int lodCount = 3;

    private TerrainBuilder builder;
    private Transform player;
    private Terrain[][] allTerrain;
    private float xLength, yLength;

    private void Start()
    {
        xLength = (xCount - 1) * xTick;
        yLength = (yCount - 1) * yTick;
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        builder = GetComponent<TerrainBuilder>();
        builder.Initial(xCount, yCount, lodCount);
        allTerrain = new Terrain[][]
        {
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(1, MeshType.LeftTop),    new Terrain(1, MeshType.Top),        new Terrain(1, MeshType.Top),      new Terrain(1, MeshType.Top),         new Terrain(1, MeshType.RightTop),    new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.LeftTop),    new Terrain(0, MeshType.Top),      new Terrain(0, MeshType.RightTop),    new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.Left),       new Terrain(0, MeshType.Original), new Terrain(0, MeshType.Right),       new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.LeftBottom), new Terrain(0, MeshType.Bottom),   new Terrain(0, MeshType.RightBottom), new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(1, MeshType.LeftBottom), new Terrain(1, MeshType.Bottom),     new Terrain(1, MeshType.Bottom),   new Terrain(1, MeshType.Bottom),      new Terrain(1, MeshType.RightBottom), new Terrain(2, MeshType.Original)},
            new Terrain[]{ new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original)},
        };

        for (int i = 0; i < allTerrain.Length; i++)
        {
            var row = allTerrain[i];
            for (int j = 0; j < allTerrain[i].Length; j++)
            {
                row[j].rect = new Rect(-allTerrain[0].Length / 2 * xLength + j * xLength, 
                    allTerrain.Length / 2 * yLength - i * yLength, xLength, yLength);
                builder.Build(row[j]);
            }
        }

    }

    private void xxUpdate()
    {
        var pos = player.position;
        int xTileCount = allTerrain.Length;
        int yTileCount = allTerrain[0].Length;
        var center = allTerrain[allTerrain.Length / 2][allTerrain[0].Length / 2].rect;

        if (pos.x < center.xMin)
        {
            for (int i = 0; i < xTileCount; i++)
            {
                var row = allTerrain[i];
                builder.Delete(row[xTileCount - 1]);
                for (int j = xTileCount - 1; j >= 1; j--)
                {
                    row[j].rect = row[j - 1].rect;
                    row[j].mesh = row[j - 1].mesh;
                    row[j].terrainObject = row[j - 1].terrainObject;
                    row[j].plantObjects = row[j - 1].plantObjects;
                    row[j].enemyObjects = row[j - 1].enemyObjects;
                    builder.UpdateMesh(row[j]);
                }
                row[0].rect.x -= xLength;
                builder.Build(row[0]);
            }
        }
        else if (pos.z < center.yMin)
        {
            for (int j = 0; j < yTileCount; j++)
            {
                builder.Delete(allTerrain[0][j]);
                for (int i = 0; i < xTileCount - 1; i++)
                {
                    allTerrain[i][j].rect = allTerrain[i + 1][j].rect;
                    allTerrain[i][j].mesh = allTerrain[i + 1][j].mesh;
                    allTerrain[i][j].terrainObject = allTerrain[i + 1][j].terrainObject;
                    allTerrain[i][j].plantObjects = allTerrain[i + 1][j].plantObjects;
                    allTerrain[i][j].enemyObjects = allTerrain[i + 1][j].enemyObjects;
                    builder.UpdateMesh(allTerrain[i][j]);
                }
                allTerrain[xTileCount - 1][j].rect.y -= yLength;
                builder.Build(allTerrain[xTileCount - 1][j]);
            }
        }
        else if (pos.x > center.xMax)
        {
            for (int i = 0; i < xTileCount; i++)
            {
                var row = allTerrain[i];
                builder.Delete(row[0]);
                for (int j = 0; j < yTileCount - 1; j++)
                {
                    row[j].rect = row[j + 1].rect;
                    row[j].mesh = row[j + 1].mesh;
                    row[j].terrainObject = row[j + 1].terrainObject;
                    row[j].plantObjects = row[j + 1].plantObjects;
                    row[j].enemyObjects = row[j + 1].enemyObjects;
                    builder.UpdateMesh(row[j]);
                }
                row[yTileCount - 1].rect.x += xLength;
                builder.Build(row[yTileCount - 1]);
            }
        }
        else if (pos.z > center.yMax)
        {
            for (int j = 0; j < yTileCount; j++)
            {
                builder.Delete(allTerrain[xTileCount - 1][j]);
                for (int i = xTileCount - 1; i >= 1; i--)
                {
                    allTerrain[i][j].rect = allTerrain[i - 1][j].rect;
                    allTerrain[i][j].mesh = allTerrain[i - 1][j].mesh;
                    allTerrain[i][j].terrainObject = allTerrain[i - 1][j].terrainObject;
                    allTerrain[i][j].plantObjects = allTerrain[i - 1][j].plantObjects;
                    allTerrain[i][j].enemyObjects = allTerrain[i - 1][j].enemyObjects;
                    builder.UpdateMesh(allTerrain[i][j]);
                }
                allTerrain[0][j].rect.y += yLength;
                builder.Build(allTerrain[0][j]);
            }
        }


    }

}
