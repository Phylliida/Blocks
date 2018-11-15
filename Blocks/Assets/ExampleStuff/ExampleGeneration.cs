using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExampleGeneration : GenerationClass
{
    public ChunkProperty elevationProp;
    public ChunkProperty caveProp;
    public override void OnGenerationInit()
    {
        float minVal = 10.0f;
        float maxVal = 40.0f;
        elevationProp = new ChunkProperty("elevation", minVal, maxVal, usesY: false);
        caveProp = new ChunkProperty("cave", 0.0f, 1.0f, scale: 1.0f, usesY: true);
        world.AddChunkProperty(elevationProp);
        world.AddChunkProperty(caveProp);
    }

    public override void OnGenerateBlock(long x, long y, long z, BlockData outBlock)
    {
        float elevation = GetChunkProperty(x, y, z, elevationProp);
        float caveLevel = GetChunkProperty(x, y, z, caveProp);
        //if (y <= 0)
        //{
        //    outBlock.block = BlockValue.STONE;
        //}
        long elevationL = (long)Mathf.Round(elevation);
        if (y >= elevationL)
        {
            // air, but allow other things to go here instead
            outBlock.block = BlockValue.WILDCARD;
        }
        else
        {
            long distFromSurface = elevationL - y;
            if (distFromSurface == 1)
            {
                outBlock.block = BlockValue.GRASS;
                // low pr of making tree
                if (Simplex.Noise.rand(x, y, z) < 0.01f)
                {
                    int treeHeight = Mathf.RoundToInt(Simplex.Noise.rand(x, y + 2, z) * 20 + 6);
                    for (int i = 0; i < treeHeight; i++)
                    {
                        if (i == 0)
                        {
                            SetBlock(x, y + i, z, BlockValue.TRUNK);
                            continue;
                        }
                        float pAlong = (i + 1) / (float)(treeHeight);

                        float maxWidth = 3.0f;
                        float topWidth = 0.5f;
                        float bottomWidth = 0.2f;
                        float decreasePoint = 0.3f;

                        float p;
                        float minVal;
                        float maxVal;
                        if (pAlong < decreasePoint)
                        {
                            minVal = bottomWidth;
                            maxVal = maxWidth;
                            p = (decreasePoint - pAlong) / decreasePoint;
                        }
                        else
                        {
                            minVal = maxWidth;
                            maxVal = topWidth;
                            p = 1 - (pAlong - decreasePoint) / (1.0f - decreasePoint);
                        }

                        float width = minVal * p + maxVal * (1 - p);
                        int widthI = Mathf.FloorToInt(width);



                        for (int j = -widthI; j <= widthI; j++)
                        {
                            for (int k = -widthI; k <= widthI; k++)
                            {
                                if (Mathf.Sqrt(j * j + k * k) <= width)
                                {
                                    SetBlock(x + j, y + i, z + k, BlockValue.LEAF);
                                }
                            }
                        }
                    }
                }
            }
            else if (distFromSurface < 4)
            {
                outBlock.block = BlockValue.DIRT;
            }
            else
            {
                if (Simplex.Noise.rand(x, y, z) < 0.005f && y == elevationL-20 && false)
                {
                    long curX = x;
                    long curY = y;
                    long curZ = z;

                    int offsetX = 0;
                    int offsetY = 0;
                    int offsetZ = 0;
                    int[] xOffsets = new int[] { 1, -1, 0, 0, 0, 0 };
                    int[] yOffsets = new int[] { 0, 0, 1, -1, 0, 0 };
                    int[] zOffsets = new int[] { 0, 0, 0, 0, 1, -1 };

                    Debug.Log("making cave");
                    for (int i = 0; i < 100; i++)
                    {
                        int val = Random.Range(0, xOffsets.Length);
                        offsetX += xOffsets[val];
                        offsetY += yOffsets[val];
                        offsetZ += zOffsets[val];
                        if (Mathf.Abs(offsetX) > Mathf.Abs(offsetY) && Mathf.Abs(offsetX) > Mathf.Abs(offsetZ))
                        {
                            curX += System.Math.Sign(offsetX) * 3;
                        }
                        else if (Mathf.Abs(offsetY) > Mathf.Abs(offsetX) && Mathf.Abs(offsetY) > Mathf.Abs(offsetZ))
                        {
                            offsetY = 0;
                            curY += System.Math.Sign(offsetY);
                        }
                        else if (Mathf.Abs(offsetZ) > Mathf.Abs(offsetY) && Mathf.Abs(offsetZ) > Mathf.Abs(offsetX))
                        {
                            curZ += System.Math.Sign(offsetZ) * 3;
                        }
                        else
                        {
                            val = Random.Range(0, xOffsets.Length);
                            curX += xOffsets[val]* 3;
                            curY += yOffsets[val]* 3;
                            curZ += zOffsets[val]* 3;
                        }
                        float curElevation = GetChunkProperty(curX, curY, curZ, elevationProp);
                        if (curY > curElevation-10)
                        {
                            break;
                        }
                        float caveWidth = 5.0f;
                        int caveWidthI = Mathf.FloorToInt(caveWidth);

                        for (int j = -caveWidthI; j <= caveWidthI; j++)
                        {
                            for (int k = -caveWidthI; k <= caveWidthI; k++)
                            {
                                for (int l = -caveWidthI; l <= caveWidthI; l++)
                                {
                                    if (Mathf.Sqrt(j * j + k * k + l*l) <= caveWidth)
                                    {
                                        SetBlock(x + j, y + i, z + k, BlockValue.AIR);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    outBlock.block = BlockValue.STONE;
                }
            }
        }
    }
}
