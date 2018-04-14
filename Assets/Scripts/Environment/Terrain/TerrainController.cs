using System.Collections;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [Header("一定要等于 2^n + 1")]
    public int xCount = 33;
    public int yCount = 33;
    public float xTick = 1.0f;
    public float yTick = 1.0f;
    public int lodCount = 6;

    private TerrainBuilder builder;
    private ChangeCharacter changeCharacter;
    private Terrain[,] allTerrain;
    private float xLength, yLength;
    private float updateGap = 10;
    private WaitForSeconds[] waits;
    private int xTileCount;
    private int yTileCount;


    private IEnumerator Start()
    {
        xLength = (xCount - 1) * xTick;
        yLength = (yCount - 1) * yTick;
        xTileCount = yTileCount = lodCount * 2 + 1;

        changeCharacter = FindObjectOfType<ChangeCharacter>();

        //保证从场景3开始可以调试
        if (changeCharacter==null)
        {
            changeCharacter = gameObject.AddComponent<ChangeCharacter>();
            changeCharacter.currCharacter = FindObjectOfType<PlayerMove>().transform;
        }

        builder = GetComponent<TerrainBuilder>();

        //保证下面的异步builder初始化完成之前的update不会出错
        allTerrain = new Terrain[yTileCount, xTileCount];             //行数是y方向
        allTerrain[yTileCount / 2, xTileCount / 2] = new Terrain(0, MeshType.Original)
        {
            rect = new Rect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity)
        };

        yield return StartCoroutine(builder.InitialAsync(xCount, yCount, lodCount));

        //这里在几帧之后才会执行到
        InitialTerrain();

        waits = new WaitForSeconds[xTileCount]; //xcount等于ycount,用哪个都一样。
        for (int i = 0; i < xTileCount; i++)
        {
            waits[i] = new WaitForSeconds(0.1f * i);
        }
    }

    private void InitialTerrain()
    {
        allTerrain = new Terrain[yTileCount, xTileCount];
        allTerrain[yTileCount / 2, xTileCount / 2] = new Terrain(0, MeshType.Original);
        for (int i = 0; i < lodCount; i++)
        {
            ConfigTerrain(i);
        }

        BuildTerrain();
    }

    /// <summary>
    /// 配置一圈的地形参数
    /// </summary>
    /// <param name="lod"></param>
    private void ConfigTerrain(int lod)
    {
        int xCenter = xTileCount / 2;
        int yCenter = yTileCount / 2;
        int offset = lod + 1;

        //四个边
        for (int i = -lod; i <= lod; i++)
        {
            allTerrain[yCenter - offset, xCenter + i] = new Terrain(lod, MeshType.Bottom);
            allTerrain[yCenter + offset, xCenter + i] = new Terrain(lod, MeshType.Top);
            allTerrain[yCenter + i, xCenter - offset] = new Terrain(lod, MeshType.Left);
            allTerrain[yCenter + i, xCenter + offset] = new Terrain(lod, MeshType.Right);
        }
        
        //四个角的
        allTerrain[yCenter - offset, xCenter - offset] = new Terrain(lod, MeshType.LeftBottom);
        allTerrain[yCenter + offset, xCenter - offset] = new Terrain(lod, MeshType.LeftTop);
        allTerrain[yCenter - offset, xCenter + offset] = new Terrain(lod, MeshType.RightBottom);
        allTerrain[yCenter + offset, xCenter + offset] = new Terrain(lod, MeshType.RightTop);
    }

    private void BuildTerrain()
    {
        for (int i = 0; i < yTileCount; i++)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                allTerrain[i, j].rect = new Rect(Mathf.Floor(changeCharacter.currCharacter.position.x / xLength) * xLength - xTileCount / 2 * xLength + j * xLength,
                    Mathf.Floor(changeCharacter.currCharacter.position.z / yLength) * yLength - yTileCount / 2 * yLength + i * yLength, xLength, yLength);
                builder.Build(allTerrain[i, j]);
            }
        }
    }

    public void ReInitialTerrain(int seed)
    {
        builder.ReInitial(seed);
        for (int i = 0; i < yTileCount; i++)
        {
            for (int j = 0; j < xTileCount; j++)
            {
                builder.Delete(allTerrain[i, j]);
            }
        }
        BuildTerrain();
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
