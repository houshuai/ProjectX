﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplexNoise
{
    private SimplexNoiseOctave[] octaves;
    private SimplexNoiseOctave octave;
    private float[] frequencys;
    private float[] amplitudes;
    private float scale;
    private float pow;

    public SimplexNoise(float scale, float persistence, int octaveCount, float pow, int seed)
    {
        octaves = new SimplexNoiseOctave[octaveCount];
        frequencys = new float[octaveCount];
        amplitudes = new float[octaveCount];

        var random = new System.Random(seed);

        for (int i = 0; i < octaveCount; i++)
        {
            octaves[i] = new SimplexNoiseOctave(random.Next());
            frequencys[i] = Mathf.Pow(2, i) / scale;
            amplitudes[i] = Mathf.Pow(persistence, i);
        }

        octave = new SimplexNoiseOctave(random.Next());
        this.scale = scale;
        this.pow = pow;
    }

    public float Get2DPow(float x, float y)
    {
        float result = 0;

        for (int i = 0; i < octaves.Length; i++)
        {
            result += octaves[i].Noise(x * frequencys[i], y * frequencys[i]) * amplitudes[i];
        }
        //result += octaves[2].Noise(x / frequencys[2], y / frequencys[2]) * amplitudes[2];
        return Mathf.Pow(result, pow);
    }

    public float Get2D(float x,float y)
    {
        //float result = 0;

        //for (int i = 0; i < octaves.Length; i++)
        //{
        //    result += octaves[i].Noise(x * frequencys[i], y * frequencys[i]) * amplitudes[i];
        //}
        return octave.Noise(x / scale, y / scale);
        //return result;
    }
}

public class SimplexNoiseOctave
{
    // Skewing and unskewing factors
    private static readonly float F2 = 0.5f * (Mathf.Sqrt(3.0f) - 1.0f);
    private static readonly float G2 = (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;

    private static int[][] grad3 =
    {
        new int[]{1,1,0}, new int[]{-1,1,0}, new int[]{1,-1,0}, new int[]{-1,-1,0},
        new int[]{1,0,1}, new int[]{-1,0,1}, new int[]{1,0,-1}, new int[]{-1,0,-1},
        new int[]{0,1,1}, new int[]{0,-1,1}, new int[]{0,1,-1}, new int[]{0,-1,-1}
    };

    private static short[] pSource =
    {   151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    private static short[] perm = new short[512];
    private static short[] permMod12 = new short[512];

    private short[] p;

    public SimplexNoiseOctave(int seed)
    {
        if (seed == 0)
        {
            var rand = new System.Random();
            seed = rand.Next();
        }

        var random = new System.Random(seed);
        p = (short[])pSource.Clone();
        for (int i = 0; i < 400; i++)
        {
            int swapFrom = random.Next(255);
            int swapTo = random.Next(255);
            short temp = p[swapFrom];
            p[swapFrom] = p[swapTo];
            p[swapTo] = temp;
        }

        for (int i = 0; i < 512; i++)
        {
            perm[i] = p[i & 255];
            permMod12[i] = (short)(perm[i] % 12);
        }
    }


    public float Noise(float xin, float yin)
    {
        float n0, n1, n2; // Noise contributions from the three corners
                          // Skew the input space to determine which simplex cell we're in
        float s = (xin + yin) * F2; // Hairy factor for 2D
        int i = FastFloor(xin + s);
        int j = FastFloor(yin + s);
        float t = (i + j) * G2;
        float X0 = i - t; // Unskew the cell origin back to (x,y) space
        float Y0 = j - t;
        float x0 = xin - X0; // The x,y distances from the cell origin
        float y0 = yin - Y0;
        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
        int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
        if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
        else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)
                                      // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
                                      // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
                                      // c = (3-sqrt(3))/6
        float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
        float y2 = y0 - 1.0f + 2.0f * G2;
        // Work out the hashed gradient indices of the three simplex corners
        int ii = i & 255;
        int jj = j & 255;
        int gi0 = permMod12[ii + perm[jj]];
        int gi1 = permMod12[ii + i1 + perm[jj + j1]];
        int gi2 = permMod12[ii + 1 + perm[jj + 1]];
        // Calculate the contribution from the three corners
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0) n0 = 0.0f;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Dot(grad3[gi0], x0, y0);  // (x,y) of grad3 used for 2D gradient
        }
        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0) n1 = 0.0f;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Dot(grad3[gi1], x1, y1);
        }
        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0) n2 = 0.0f;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Dot(grad3[gi2], x2, y2);
        }
        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [0,1].
        return 35.0f * (n0 + n1 + n2) + 0.5f;
    }

    // This method is a *lot* faster than using (int)Math.floor(x)
    private static int FastFloor(float x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    private static float Dot(int[] g, float x, float y)
    {
        return g[0] * x + g[1] * y;
    }
}