using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColourMap, FalloffMap, PolygonMap };
     
    public DrawMode drawMode;

    public int mapSize;

    [Range(0,1)]
    public float border;
    public float scale;
    [Range(1,8)]
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    
    public bool autoUpdate;

    public bool useFalloff;
    public enum FalloffMode { SquareFalloff, CircleFalloff };
    public FalloffMode falloffMode;
    public Vector2 falloff;

    public int polygonSeed;
    [Range(0,1)]
    public float polygonSize;

    public TerrainType[] regions;
    
    public void GenerateMap()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        if (drawMode == DrawMode.NoiseMap || drawMode == DrawMode.ColourMap)
        {
            float[,] noiseMapI = Mesh.GenerateNoiseMap(mapSize, mapSize, seed, scale, octaves, persistance, lacunarity, offset);

            if (drawMode == DrawMode.ColourMap)
            {
                float[,] falloffMapI = FalloffGenerator.GenerateFalloffMap(mapSize, falloff.x, falloff.y, falloffMode.ToString());
                Color[] colourMap = new Color[mapSize * mapSize];
                for (int y = 0; y < mapSize; y++)
                {
                    for (int x = 0; x < mapSize; x++)
                    {
                        if (useFalloff)
                        {
                            noiseMapI[x, y] = Mathf.Clamp(noiseMapI[x, y] - falloffMapI[x, y], 0, border);
                        }
                        float currentHeight = noiseMapI[x, y];
                        for (int i = 0; i < regions.Length; i++)
                        {
                            if (currentHeight <= regions[i].height)
                            {
                                colourMap[y * mapSize + x] = regions[i].colour;
                                break;
                            }
                        }
                    }
                }
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapSize));
            }
            else
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMapI));
            }
            Debug.Log("Max height jump between neighbours: " + MaxNeighbourJump(noiseMapI) + "\n");
        }
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapSize, falloff.x, falloff.y, falloffMode.ToString())));
        else if (drawMode == DrawMode.PolygonMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(MeshGenerator.GeneratePolygons(mapSize, polygonSize, polygonSeed)));

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
             ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        Debug.Log("PolygonsRunTime " + elapsedTime);
    }

    private void OnValidate()
    {
        if (mapSize < 1)
            mapSize = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (scale < 0)
            scale = 0.001f;
    }

    private float MaxNeighbourJump (float[,] noiseMapI)
    {
        float maxJump = 0;
        float tmp;

        for (int y = 2; y < noiseMapI.GetLength(0) - 2; y++)
        {
            for (int x = 2; x < noiseMapI.GetLength(0) - 2; x++)
            {
                {
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x - 1, y - 1]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x - 1, y]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x - 1, y + 1]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x, y - 1]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x, y + 1]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x + 1, y - 1]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x + 1, y]);
                    if (maxJump < tmp) maxJump = tmp;
                    tmp = Mathf.Abs(noiseMapI[x, y] - noiseMapI[x + 1, y + 1]);
                    if (maxJump < tmp) maxJump = tmp;
                }
            }
        }
        return maxJump;
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}
