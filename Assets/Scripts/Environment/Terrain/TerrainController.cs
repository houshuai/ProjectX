using System.Collections;
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
    private ChangeCharacter changeCharacter;
    private Terrain[,] allTerrain;
    private float xLength, yLength;
    private float updateGap = 10;
    private WaitForSeconds[] waits;
    private int xTileCount = 7;
    private int yTileCount = 7;


    private IEnumerator Start()
    {
        xLength = (xCount - 1) * xTick;
        yLength = (yCount - 1) * yTick;
        changeCharacter = FindObjectOfType<ChangeCharacter>();

        builder = GetComponent<TerrainBuilder>();

        //保证下面的异步builder初始化完成之前的update不会出错
        allTerrain = new Terrain[7, 7];
        allTerrain[3, 3] = new Terrain(0, MeshType.Original)
        {
            rect = new Rect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity)
        };

        yield return StartCoroutine(builder.InitialAsync(xCount, yCount, lodCount));

        //这里在几帧之后才会执行到
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
                allTerrain[i, j].rect = new Rect(Mathf.Floor(changeCharacter.currCharacter.position.x / xLength) * xLength - 7 / 2 * xLength + j * xLength,
                    Mathf.Floor(changeCharacter.currCharacter.position.z / yLength) * yLength - 7 / 2 * yLength + i * yLength, xLength, yLength);
                builder.Build(allTerrain[i, j]);
            }
        }

        waits = new WaitForSeconds[7];
        for (int i = 0; i < 7; i++)
        {
            waits[i] = new WaitForSeconds(0.3f * i);
        }
    }

    /// <summary>
    /// 一次更新一列，y方向排列的一列
    /// </summary>
    /// <param name="index">更新的的列</param>
    /// <param name="isIncreasing">是从第一列往最后一列更新吗</param>
    /// <returns></returns>
    private IEnumerator UpdateY(int index, bool isIncreasing)
    {
        int offset = 1;
        int waitIndex = index;
        if (!isIncreasing)
        {
            offset = -1;
            waitIndex = xTileCount - 1 - index;
        }

        //先将terrain转移
        for (int i = 0; i < yTileCount; i++)
        {
            MoveTerrain(allTerrain[i, index + offset], allTerrain[i, index]);
        }
        yield return waits[waitIndex];
        for (int i = 0; i < yTileCount; i++)
        {
            builder.UpdateTerrain(allTerrain[i, index]);
        }
    }

    private IEnumerator UpdateX(int index, bool isIncreasing)
    {
        int offset = 1;
        int waitIndex = index;
        if (!isIncreasing)
        {
            offset = -1;
            waitIndex = yTileCount - 1 - index;
        }

        for (int j = 0; j < xTileCount; j++)
        {
            MoveTerrain(allTerrain[index + offset, j], allTerrain[index, j]);
        }
        yield return waits[waitIndex];
        for (int j = 0; j < xTileCount; j++)
        {
            builder.UpdateTerrain(allTerrain[index, j]);
        }
    }

    /// <summary>
    /// 新建一列
    /// </summary>
    /// <param name="index">第几列</param>
    /// <returns></returns>
    private IEnumerator BuildY(int index, bool isIncreasing)
    {
        float offset = xLength;
        if (!isIncreasing)
        {
            offset = -xLength;
        }

        for (int i = 0; i < yTileCount; i++)
        {
            allTerrain[i, index].rect.x += offset;
        }
        yield return waits[6];
        for (int i = 0; i < yTileCount; i++)
        {
            builder.Build(allTerrain[i, index]);
        }
    }

    private IEnumerator BuildX(int index, bool isIncreasing)
    {
        float offset = xLength;
        if (!isIncreasing)
        {
            offset = -xLength;
        }

        for (int j = 0; j < xTileCount; j++)
        {
            allTerrain[index, j].rect.y += offset;
        }
        yield return waits[6];
        for (int j = 0; j < xTileCount; j++)
        {
            builder.Build(allTerrain[index, j]);
        }

    }

    private void Update()
    {
        var pos = changeCharacter.currCharacter.position;
        var center = allTerrain[yTileCount / 2, xTileCount / 2].rect;

        if (pos.x < center.xMin - updateGap)
        {
            //删除最后一列
            for (int i = 0; i < yTileCount; i++)
            {
                builder.Delete(allTerrain[i, xTileCount - 1]);
            }
            //从最后一列往前更新
            for (int j = xTileCount - 1; j >= 1; j--)
            {
                StartCoroutine(UpdateY(j, false));
            }
            //新建第一列
            StartCoroutine(BuildY(0, false));
        }
        else if (pos.z < center.yMin - updateGap)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                builder.Delete(allTerrain[yTileCount - 1, j]);
            }

            for (int i = yTileCount - 1; i >= 1; i--)
            {
                StartCoroutine(UpdateX(i, false));
            }
            StartCoroutine(BuildX(0, false));

        }
        else if (pos.x > center.xMax + updateGap)
        {
            for (int i = 0; i < yTileCount; i++)
            {
                builder.Delete(allTerrain[i, 0]);
            }
            for (int j = 0; j < xTileCount - 1; j++)
            {
                StartCoroutine(UpdateY(j, true));
            }
            StartCoroutine(BuildY(xTileCount - 1, true));
        }
        else if (pos.z > center.yMax + updateGap)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                builder.Delete(allTerrain[0, j]);
            }
            for (int i = 0; i < yTileCount - 1; i++)
            {
                StartCoroutine(UpdateX(i, true));
            }
            StartCoroutine(BuildX(yTileCount - 1, true));
        }
    }

    private void MoveTerrain(Terrain source, Terrain target)
    {
        target.rect = source.rect;
        target.mesh = source.mesh;
        target.terrainObject = source.terrainObject;
        target.plantObjects = source.plantObjects;
        target.enemyObjects = source.enemyObjects;
        target.vertices = source.vertices;
    }

}
