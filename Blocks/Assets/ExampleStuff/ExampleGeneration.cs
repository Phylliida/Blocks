using System.Collections;
using System.Collections.Generic;
using System;

public class ExampleGeneration : GenerationClass
{
    public ChunkProperty elevationProp;
    public ChunkProperty riverProp;
    public override void OnGenerationInit()
    {
        float minVal = 10.0f;
        float maxVal = 40.0f;
        elevationProp = new ChunkProperty("elevation", minVal, maxVal, usesY: false);
        riverProp = new ChunkProperty("river", 0.0f, 1.0f, scale: 1.0f, usesY: true);
        world.AddChunkProperty(elevationProp);
        world.AddChunkProperty(riverProp);
    }

    public override void OnGenerateBlock(long x, long y, long z, BlockData outBlock)
    {
        float elevation = outBlock.GetChunkProperty(elevationProp);
        //if (y <= 0)
        //{
        //    outBlock.block = BlockValue.STONE;
        //}
        long elevationL = (long)Math.Round(elevation);
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
                    int treeHeight = (int)Math.Round(Simplex.Noise.rand(x, y + 2, z) * 20 + 6);
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
                        int widthI = (int)Math.Floor(width);



                        for (int j = -widthI; j <= widthI; j++)
                        {
                            for (int k = -widthI; k <= widthI; k++)
                            {
                                if (Math.Sqrt(j * j + k * k) <= width)
                                {
                                    SetBlock(x + j, y + i, z + k, BlockValue.LEAF);
                                }
                            }
                        }
                    }
                }
                else
                {
                    float caveLevelX = outBlock.GetChunkProperty(riverProp);
                    if (Simplex.Noise.rand(x, y, z) > 0.9999f)
                    {
                        long curX = x;
                        long curY = y + 20;
                        long curZ = z;

                        int offsetX = 0;
                        int offsetY = 0;
                        int offsetZ = 0;
                        int[] xOffsets = new int[] { 1, -1, 0, 0 };
                        int[] zOffsets = new int[] { 0, 0, 1, -1 };

                        UnityEngine.Debug.Log("making river");
                        for (int i = 0; i < 100; i++)
                        {
                            int val = UnityEngine.Random.Range(0, xOffsets.Length);
                            offsetX += xOffsets[val];
                            offsetZ += zOffsets[val];
                            if (Math.Abs(offsetX) > Math.Abs(offsetZ))
                            {
                                curX += Math.Sign(offsetX) * 3;
                            }
                            else
                            {
                                curZ += Math.Sign(offsetX) * 3;
                            }

                            curY = (long)Math.Round(GetChunkProperty(curX, 0, curZ, elevationProp));
                            float caveWidth = 3.0f;
                            int caveWidthI = (int)Math.Floor(caveWidth);

                            for (int j = -caveWidthI; j <= caveWidthI; j++)
                            {
                                for (int l = -caveWidthI; l <= caveWidthI; l++)
                                {
                                    curY = (int)Math.Round(GetChunkProperty(curX+j, 0, curZ+l, elevationProp));
                                    for (int k = -caveWidthI; k <= -1; k++)
                                    {
                                        if (Math.Sqrt(j * j + k * k + l * l) <= caveWidth)
                                        {
                                            SetBlock(curX + j, curY + k, curZ + l, BlockValue.WATER);
                                        }
                                        else if (Math.Sqrt(j * j + k * k + l * l) <= 5.0f)
                                        {
                                            if (GetBlock(curX + j, curY + k, curZ + l) != BlockValue.WATER)
                                            {
                                                SetBlock(curX + j, curY + k, curZ + l, BlockValue.CLAY);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (distFromSurface < 3)
            {
                outBlock.block = BlockValue.DIRT;
            }
            else if(distFromSurface < 5)
            {
                outBlock.block = BlockValue.CLAY;
            }
            else
            {
                if (Simplex.Noise.rand(x,y,z) < 0.8 && distFromSurface == 6)
                {
                    outBlock.block = BlockValue.LOOSE_ROCKS;
                }
                else if (Simplex.Noise.rand(x, y, z) < 0.5 && distFromSurface == 7)
                {
                    outBlock.block = BlockValue.LOOSE_ROCKS;
                }
                else if (Simplex.Noise.rand(x, y, z) < 0.2 && distFromSurface == 8)
                {
                    outBlock.block = BlockValue.LOOSE_ROCKS;
                }
                else if (Simplex.Noise.rand(x, y, z) < 0.04)
                {
                    outBlock.block = BlockValue.LOOSE_ROCKS;
                }
                else
                {
                    outBlock.block = BlockValue.STONE;
                }
            }
        }
    }
}
