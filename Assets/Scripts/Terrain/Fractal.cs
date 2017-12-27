using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    /// <summary>
    /// get a (2^n+1) x (2^n)+1 random map 
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static float[,] RandomMap(int n, float smooth)
    {
        int count = (1 << n) + 1;
        var result = new float[count, count];
        float h = 2;

        result[0, 0] = Random.value;
        result[count - 1, 0] = Random.value;
        result[0, count - 1] = Random.value;
        result[count - 1, count - 1] = Random.value;

        int step = (count - 1) / 2; //每个循环中“正方形”阶段的正方形边长的网格步数
        while (step > 0)
        {
            int stepX2 = step * 2;

            //Diamond stage
            for (int i = 0; i < count - 1; i += stepX2)
            {
                for (int j = 0; j < count - 1; j += stepX2)
                {
                    result[i + step, j + step] =
                        (result[i, j] + result[i + stepX2, j] + result[i, j + stepX2] + result[i + stepX2, j + stepX2]) / 4 +
                        (Random.value - 0.5f) * h;
                }
            }

            //Square stage
            for (int i = 0; i < count - 1; i += stepX2)
            {
                for (int j = 0; j < count - 1; j += stepX2)
                {
                    float lb = result[i, j];
                    float rb = result[i + stepX2, j];
                    float lt = result[i, j + stepX2];
                    float rt = result[i + stepX2, j + stepX2];
                    float cnt = result[i + step, j + step];
                    int index = j - step;
                    if (index < 0)
                    {
                        index += count - 1;
                    }
                    float b = result[i + step, index];
                    index = j + stepX2 + step;
                    if (index > count - 1)
                    {
                        index -= count - 1;
                    }
                    float t = result[i + step, index];
                    index = i - step;
                    if (index < 0)
                    {
                        index += count - 1;
                    }
                    float l = result[index, j + step];
                    index = i + stepX2 + step;
                    if (index > count - 1)
                    {
                        index -= count - 1;
                    }
                    float r = result[index, j + step];

                    result[i + step, j] = (lb + rb + b + cnt) / 4 + (Random.value - 0.5f) * h;
                    result[i, j + step] = (lb + lt + l + cnt) / 4 + (Random.value - 0.5f) * h;
                    result[i + step, j + stepX2] = (lt + rt + t + cnt) / 4 + (Random.value - 0.5f) * h;
                    result[i + stepX2, j + step] = (rb + rt + r + cnt) / 4 + (Random.value - 0.5f) * h;

                }
            }

            step /= 2;
            h *= Mathf.Pow(2, -smooth);
        }

        return result;
    }
}
