using System.Collections;
using UnityEngine;

public class TextureGenerator {

    public static Texture2D TextureFromColourMap (Color[] colourMap, int size)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point; //brak filtrowania przejść między kolorami
        texture.wrapMode = TextureWrapMode.Clamp; //brak poświaty powtarzającej się textury
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap (float[,] heightMap)
    {
        int size = heightMap.GetLength(0);

        Color[] colourMap = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                colourMap[y * size + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return TextureFromColourMap(colourMap, size);
    }	
}
