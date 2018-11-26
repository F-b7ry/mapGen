using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator{

    public static float[,] GenerateFalloffMap(int size, float a, float b, string mode)
    {
        float value = 0;

        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {   
                if (mode == "SquareFalloff")
                {
                    float x = i / (float)size * 2 - 1;
                    float y = j / (float)size * 2 - 1;
                    value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                }else if (mode == "CircleFalloff")
                {
                    float x = Vector2.Distance(new Vector2(0, 0), new Vector2(size / 2, size / 2));
                    value = Vector2.Distance(new Vector2(size / 2, size / 2), new Vector2(i, j)) / x;
                }

                map[i, j] = Evaluate(value, a, b, mode); 
            }
        }
        return map;
    }

    static float Evaluate(float value, float a, float b, string mode)
    {
        float xa;
        float xb;
        if (mode == "SquareFalloff")
        {
            xa = a / 2;
            xb = b * 3.333f;
        }else
        {
            xa = a;
            xb = b;
        }
        return Mathf.Pow(value, xa) / (Mathf.Pow(value, xa) + Mathf.Pow(xb - xb * value, xa));
    }
}
