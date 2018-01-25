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
    private Terrain[,] allTerrain;
    private float xLength, yLength;

    private void Start()
    {
        xLength = (xCount - 1) * xTick;
        yLength = (yCount - 1) * yTick;
        player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        builder = GetComponent<TerrainBuilder>();
        builder.Initial(xCount, yCount, lodCount);

        if (Archive.current != null)
        {
            Archive.current.currScene = "Scene3";
            Vector3 pos;
            if (Archive.current.GetPlayerPosition(out pos))
            {
                player.position = pos + new Vector3(0, 1, 0);
            }
        }

        allTerrain = new Terrain[,]
        {
            { new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(1, MeshType.LeftBottom), new Terrain(1, MeshType.Bottom),     new Terrain(1, MeshType.Bottom),   new Terrain(1, MeshType.Bottom),      new Terrain(1, MeshType.RightBottom), new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.LeftBottom), new Terrain(0, MeshType.Bottom),   new Terrain(0, MeshType.RightBottom), new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.Left),       new Terrain(0, MeshType.Original), new Terrain(0, MeshType.Right),       new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(1, MeshType.Left),       new Terrain(0, MeshType.LeftTop),    new Terrain(0, MeshType.Top),      new Terrain(0, MeshType.RightTop),    new Terrain(1, MeshType.Right),       new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(1, MeshType.LeftTop),    new Terrain(1, MeshType.Top),        new Terrain(1, MeshType.Top),      new Terrain(1, MeshType.Top),         new Terrain(1, MeshType.RightTop),    new Terrain(2, MeshType.Original)},
            { new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original),   new Terrain(2, MeshType.Original), new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original),    new Terrain(2, MeshType.Original)},
        };

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                allTerrain[i, j].rect = new Rect(Mathf.Floor(player.position.x / xLength) * xLength - 7 / 2 * xLength + j * xLength,
                    Mathf.Floor(player.position.z / yLength) * yLength - 7 / 2 * yLength + i * yLength, xLength, yLength);
                builder.Build(allTerrain[i, j]);
            }
        }

    }

    private void Update()
    {
        var pos = player.position;
        int yTileCount = allTerrain.GetLength(0);
        int xTileCount = allTerrain.GetLength(1);
        var center = allTerrain[yTileCount / 2, xTileCount / 2].rect;

        if (pos.x < center.xMin)
        {
            for (int i = 0; i < yTileCount; i++)
            {
                builder.Delete(allTerrain[i, xTileCount - 1]);
                for (int j = xTileCount - 1; j >= 1; j--)
                {
                    MoveTerrain(allTerrain[i, j - 1], allTerrain[i, j]);
                    builder.UpdateTerrain(allTerrain[i, j]);
                }
                allTerrain[i, 0].rect.x -= xLength;
                builder.Build(allTerrain[i, 0]);
            }
        }
        else if (pos.z < center.yMin)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                builder.Delete(allTerrain[yTileCount - 1, j]);
                for (int i = yTileCount - 1; i >= 1; i--)
                {
                    MoveTerrain(allTerrain[i - 1, j], allTerrain[i, j]);
                    builder.UpdateTerrain(allTerrain[i, j]);
                }
                allTerrain[0, j].rect.y -= yLength;
                builder.Build(allTerrain[0, j]);
            }
        }
        else if (pos.x > center.xMax)
        {
            for (int i = 0; i < yTileCount; i++)
            {
                builder.Delete(allTerrain[i, 0]);
                for (int j = 0; j < xTileCount - 1; j++)
                {
                    MoveTerrain(allTerrain[i, j + 1], allTerrain[i, j]);
                    builder.UpdateTerrain(allTerrain[i, j]);
                }
                allTerrain[i, yTileCount - 1].rect.x += xLength;
                builder.Build(allTerrain[i, xTileCount - 1]);
            }
        }
        else if (pos.z > center.yMax)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                builder.Delete(allTerrain[0, j]);
                for (int i = 0; i < yTileCount - 1; i++)
                {
                    MoveTerrain(allTerrain[i + 1, j], allTerrain[i, j]);
                    builder.UpdateTerrain(allTerrain[i, j]);
                }
                allTerrain[yTileCount - 1, j].rect.y += yLength;
                builder.Build(allTerrain[yTileCount - 1, j]);
            }
        }

    }

    private void MoveTerrain(Terrain source, Terrain target)
    {
        target.rect = source.rect;
        target.mesh = source.mesh;
        target.terrainObject = source.terrainObject;
        target.plantObjects = source.plantObjects;
        target.enemyObjects = source.enemyObjects;
    }

}
