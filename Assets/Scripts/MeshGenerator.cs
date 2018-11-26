using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Debug = UnityEngine.Debug;

public static class MeshGenerator {

    // Voronoi diagram, Fortune's algorithm
    public static float[,] GeneratePolygons(int mapSize, float polygonSize, int seed) // , int relaxation);
    {
        float[,] noiseMap = new float[mapSize, mapSize];
        System.Random prng = new System.Random(seed);
        List<Vector2> polygonCenters = new List<Vector2>();
        int numberOfPolyCenters = 0;

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                if (prng.Next(100001) >= (100000 - (1 - polygonSize) * 50))
                {
                    numberOfPolyCenters++;
                    noiseMap[x, z] = 1;
                    polygonCenters.Add(new Vector2(x, z));
                }
                else
                {
                    noiseMap[x, z] = 0;
                }
            }
        }
        Debug.Log(String.Format("{0}", numberOfPolyCenters) + " pixels");

        if (polygonCenters.Count < 2) //if there is only one pixel or less - no need to make polygons
            return noiseMap;

        float[] f = new float[numberOfPolyCenters]; //distance from vertex to directrix or latus rectum / focus
        float[] parabola = new float[numberOfPolyCenters]; //solved parabola value from the center
        List<int> activePolyCenters;
        //int[] centersOrder = new int[3];
        List<int> delPolyCenter = new List<int>(); // add if parabola for that center was not in any allowed area within (directrix - Jump) iterations
        //float searchDistance = 5f; // search for voronoi points in such area

        /*
        float[] Xvx = new float[numberOfPolyCenters]; // x of vertex
        int[] XvxPixel = new int[numberOfPolyCenters];
        CheckArms checkArms;
        bool directrixIteration = true;
        float XIteration;
        float XIterationJump = -1f;// get more dense points on z-axis iterations */

        float directrix = polygonCenters[1].x; //start from posistion of second polygonCenter [1]
        float directrixJump = 4f;// consider .001f for more dense parabolas = get narrow one
        float ZIteration;
        float ZIterationJump = .1f;
        
        activePolyCenters = new List<int> { 0 }; // 0 for not breaking loop at first check
        while (directrix <= 1000)
        {
            if (!activePolyCenters.Except(delPolyCenter).Any())
            {
                Debug.Log("No more active polygon centers.");
                break;
            }
            if (directrix < mapSize)
            {
                activePolyCenters.Clear(); // = new List<int>(); 
                for (int k = 0; k < numberOfPolyCenters; k++)
                {
                    if (directrix < polygonCenters[k].x) //only solve for pixel before directrix
                        break;
                    else if (!delPolyCenter.Contains(k))
                    {
                        activePolyCenters.Add(k);
                    }
                }
            }
            foreach (int i in activePolyCenters)
                delPolyCenter.Add(i);

            foreach (var k in activePolyCenters)
            {
                f[k] = (directrix - polygonCenters[k].x) / 2f;
            }

            ZIteration = 0;
            while (ZIteration < mapSize)
            {
                int[] centersOrder = { -1, -1, -1};
                int firstCenter = activePolyCenters[0];
                foreach (int activePolyCenter in activePolyCenters)
                {
                    parabola[activePolyCenter] = directrix - f[activePolyCenter] - (ZIteration - polygonCenters[activePolyCenter].y) * (ZIteration - polygonCenters[activePolyCenter].y) / 4 / f[activePolyCenter];
                    
                    if (centersOrder[2] == -1 || parabola[centersOrder[2]] > parabola[activePolyCenter])
                    {
                        centersOrder[0] = centersOrder[1];
                        centersOrder[1] = centersOrder[2];
                        centersOrder[2] = activePolyCenter;
                    }

                    if (parabola[activePolyCenter] > parabola[firstCenter])
                    {
                        firstCenter = activePolyCenter;
                    }
                    if (f[activePolyCenter] == 0)
                        delPolyCenter.Remove(activePolyCenter);
                }
                if (parabola[firstCenter] >= 0 && parabola[firstCenter] < mapSize) //&& parabola[firstCenter] < directrix)
                {
                    //Debug.Log(String.Format("parabola value {0}", parabola[firstCenter]));
                    delPolyCenter.Remove(firstCenter);
                    noiseMap[(int)parabola[firstCenter], (int)ZIteration] = 0.7f;
                }

                ZIteration += ZIterationJump;
            }
            directrix += directrixJump;
        }

        Debug.Log(String.Format("directrix {0}", directrix));
        return noiseMap;
    }
}


        /*  SQRT used! terrible performance!
        activePolyCenters = new List<int> {0}; // 0 for not breaking loop at first check
        while (directrix <= 900) //moving dirextrix along x-axis
        {
            Arms[] arms = new Arms[numberOfPolyCenters]; //matrix of arms points of each Center
                                                         //zArea meaning = there can be only one point in all next x iterations at z-Axis

            if (activePolyCenters.All(delPolyCenter.Contains))
                break;
            if (directrix < mapSize)
            {
                activePolyCenters.Clear(); // = new List<int>(); 
                for (int k = 0; k < numberOfPolyCenters; k++) //solving from point close to directrix
                {
                    if (directrix < polygonCenters[k].x) //only solve for pixel before directrix
                        break;
                    else if (!delPolyCenter.Contains(k))
                    {
                        activePolyCenters.Add(k);
                    }
                }
            }                        
            //if (directrix >= 490)
            //    Debug.Log(String.Format("Kick list length {0} and active pixels {1} for directrix: {2}", delPolyCenter.Count, activePolyCenters.Count, directrix));

            if (directrix > 1000)
                break;
            foreach (int i in activePolyCenters)
                delPolyCenter.Add(i);
            //Debug.Log(String.Format("ActivePixels: {0} for directrix: {1}", activePolyCenters.Count, directrix));

            foreach (var k in activePolyCenters) //solving from point close to directrix, for checking restricted areas problem
            {
                f[k] = (directrix - polygonCenters[k].x) / 2f;
                Xvx[k] = polygonCenters[k].x + f[k];
                XvxPixel[k] = (int)Xvx[k];
                //Debug.Log(String.Format("Active pixel: {0}, f: {1}, vertex X: {2}", k, f[k], Xvx[k]));
            }

            XIteration = polygonCenters[activePolyCenters[activePolyCenters.Count - 1]].x + f[activePolyCenters[activePolyCenters.Count - 1]]; //or directrix - f[activePixels - 1]; its the same, just start from vertex of last parabola
            //Debug.Log(String.Format("Iteration start: {0}", XIteration));
           
            while (XIteration >= 0)
            {
                for (int activePolyCenter = activePolyCenters[activePolyCenters.Count - 1]; activePolyCenter >= 0; activePolyCenter--) //solve from last parabola for restricted area problem
                {
                    if (directrix != polygonCenters[activePolyCenter].x && XIteration <= Xvx[activePolyCenter])
                    {
                        parabola[activePolyCenter] = Mathf.Sqrt(4 * f[activePolyCenter] * (Xvx[activePolyCenter] - XIteration));
                        if (parabola[activePolyCenter] < 0)
                            break;
                        //Debug.Log(String.Format("parabola: {0}", parabola[activePixel]));

                        arms[activePolyCenter].upper = polygonCenters[activePolyCenter].y - parabola[activePolyCenter]; //upper point for restricted area
                        arms[activePolyCenter].lower = polygonCenters[activePolyCenter].y + parabola[activePolyCenter]; //lower point for restricted area
                        checkArms = checkIfInArea(parabola[activePolyCenter], polygonCenters[activePolyCenter].y, arms);
                        //if (checkArms.upperCheck == true || checkArms.lowerCheck == true)
                            //kickList.Remove(activePixel);

                        if (polygonCenters[activePolyCenter].y - parabola[activePolyCenter] >= 0 && checkArms.upperCheck && XIteration < mapSize) //if on map and out of restricted area
                        {
                            //if (activePolyCenter == 9)
                            { 
                                //if (XIteration == mapSize - 1 && directrix > 490)
                                    //Debug.Log(String.Format("NEW point for directrix: {0} U: {1}", directrix, (int)(polygonCenters[activePixel].y - parabola[activePixel])));
                                noiseMap[(int)XIteration, (int)(polygonCenters[activePolyCenter].y - parabola[activePolyCenter])] = 0.5f; //parabola upper arm [0]
                            }
                            delPolyCenter.Remove(activePolyCenter);
                        }
                        
                        if (polygonCenters[activePolyCenter].y + parabola[activePolyCenter] < mapSize && checkArms.lowerCheck && XIteration < mapSize)   //if on map and out of restricted area
                        {
                            //if (activePolyCenter == 9)
                            {
                                //if (XIteration == mapSize - 1 && directrix > 490)
                                    //Debug.Log(String.Format("NEW point for directrix: {0} L: {1}", directrix, (int)(polygonCenters[activePixel].y + parabola[activePixel])));
                                noiseMap[(int)XIteration, (int)(polygonCenters[activePolyCenter].y + parabola[activePolyCenter])] = 0.5f; //parabola lower arm
                            }
                            delPolyCenter.Remove(activePolyCenter);
                        }
                    }
                    else if (XIteration < polygonCenters[activePolyCenter].x && 
                             checkIfInArea(parabola[activePolyCenter], polygonCenters[activePolyCenter].y, arms).upperCheck)  //if before point and out of restricted area
                    {
                        noiseMap[(int)XIteration, (int)polygonCenters[activePolyCenter].y] = 0.5f;
                        delPolyCenter.Remove(activePolyCenter);
                    }
                }
                if (XIteration > mapSize - 2 && delPolyCenter.Count == activePolyCenters.Count && !delPolyCenter.Except(activePolyCenters).Any())
                {
                    //directrixIteration = false;
                    break;
                }
                XIteration += XIterationJump;
            }
            directrix += directrixJump;
        }
        polygonCenters.Clear();
        
        return noiseMap;
    }

    private static CheckArms checkIfInArea (float parabola, float vertex, Arms[] area)
    {
        CheckArms arms = new CheckArms();
        arms.upperCheck = true;
        arms.lowerCheck = true; //true for upper arm; true for lower arm
        float offset = 0f; //adjust if algorithm cannot find voronoi points

        for (int i = 0; i < area.Length; i++)
        {
            if (area[i].upper == 0 && area[i].lower == 0)
                continue;
            if (vertex - parabola > area[i].upper + offset && vertex - parabola < area[i].lower - offset)
            {
                arms.upperCheck = false;
            }
            if (vertex + parabola > area[i].upper + offset && vertex + parabola < area[i].lower - offset)
            {
                arms.lowerCheck = false;
            }
        }
        return arms; 
    }
}

    [System.Serializable]
public struct Arms
{
    public float upper;
    public float lower;
}

public struct CheckArms
{
    public bool upperCheck;
    public bool lowerCheck;
}*/