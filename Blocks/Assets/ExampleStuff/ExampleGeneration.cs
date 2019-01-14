using System.Collections;
using System.Collections.Generic;
using System;
using Example_pack;
using Blocks;

public class ExampleGeneration : GenerationClass
{
    public ChunkProperty elevationProp;
    public ChunkProperty riverProp;
    public ChunkPropertyEvent riverEvent;
    public ChunkPropertyEvent treeEvent;
    public ChunkPropertyEvent caveEvent;
    public override void OnGenerationInit()
    {
        float minVal = 10.0f;
        float maxVal = 40.0f;
        //Simplex.Noise.Seed = 27;
        elevationProp = new ChunkProperty("elevation", minVal, maxVal, usesY: false);
        riverProp = new ChunkProperty("river", 0.0f, 1.0f, scale: 1.0f, usesY: true);
        world.AddChunkProperty(elevationProp);
        world.AddChunkProperty(riverProp);
        world.AddChunkPropertyEvent(new ChunkPropertyEvent(100.0f, OnTree, 1));
        world.AddChunkPropertyEvent(new ChunkPropertyEvent(300000.0f, OnRiver, 1));
        world.AddChunkPropertyEvent(new ChunkPropertyEvent(400.0f, OnCave, 2));
        world.AddChunkPropertyEvent(new ChunkPropertyEvent(2.0f, OnIronOre, 1));
    }

    public void OnIronOre(long x, long y, long z, BlockData outBlock)
    {
        //float randVal = Simplex.Noise.rand(x, y, z);

        int numThings = Simplex.Noise.randInt(5, 9, x, y, z);

        long curX = x;
        long curY = y;
        long curZ = z;

        int[] xOffsets = new int[] { 1, -1, 0, 0, 0, 0 };
        int[] yOffsets = new int[] { 1, -1, 0, 0, 1, -1 };
        int[] zOffsets = new int[] { 0, 0, 1, -1, 0, 0 };

        for (int i = 0; i < numThings; i++)
        {
            long distFromSurface;
            if (OnOrBelowSurface(curX, curY, curZ, out distFromSurface))
            {
                if (distFromSurface < 2)
                {
                    return;
                }
                SetBlock(curX, curY, curZ, Example.IronOre);
            }
            int val = Simplex.Noise.randInt(0, xOffsets.Length, curX, curY, curZ);
            curX += xOffsets[val];
            curY += yOffsets[val];
            curZ += zOffsets[val];
        }
    }


    public void OnRiver(long x, long y, long z, BlockData outBlock)
    {
        //float caveLevelX = outBlock.GetChunkProperty(riverProp);
        long curX = x;
        long curY = y + 20;
        long curZ = z;

        int offsetX = 0;
        //int offsetY = 0;
        int offsetZ = 0;
        int[] xOffsets = new int[] { 1, -1, 0, 0 };
        int[] zOffsets = new int[] { 0, 0, 1, -1 };

        for (int i = 0; i < 100; i++)
        {
            int val = Simplex.Noise.randInt(0, xOffsets.Length, curX, curY, curZ);
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
                    curY = (int)Math.Round(GetChunkProperty(curX + j, 0, curZ + l, elevationProp));
                    for (int k = -caveWidthI; k <= -1; k++)
                    {
                        if (Math.Sqrt(j * j + k * k + l * l) <= caveWidth)
                        {
                            SetBlock(curX + j, curY + k, curZ + l, Example.Water);
                        }
                        else if (Math.Sqrt(j * j + k * k + l * l) <= 5.0f)
                        {
                            if (GetBlock(curX + j, curY + k, curZ + l) != Example.Water)
                            {
                                SetBlock(curX + j, curY + k, curZ + l, Example.Clay);
                            }
                        }
                    }
                }
            }
        }
    }


    public void OnCave(long x, long y, long z, BlockData outBlock)
    {
        long elevation = (long)GetChunkProperty(x, y, z, elevationProp);
        if (elevation < y)
        {
            return;
        }
        long curX = x;
        long curY = y;
        long curZ = z;

        int offsetX = 0;
        int offsetY = 0;
        int offsetZ = 0;
        int[] xOffsets = new int[] { 1, -1, 0, 0, 0,0 };
        int[] yOffsets = new int[] { 1, -1, 0, 0, 1, -1 };
        int[] zOffsets = new int[] { 0, 0, 1, -1, 0, 0 };
        UnityEngine.Vector3 velocity = new UnityEngine.Vector3(0, 0, 0);

        for (int i = 0; i < 200; i++)
        {
            // from negative one to one instead of 0 to 1
            float valX = Simplex.Noise.rand(curX, curY, curZ)*2-1;
            float valY = Simplex.Noise.rand(curX+3, curY, curZ)*2-1;
            float valZ = Simplex.Noise.rand(curX, curY+3, curZ+2)*2-1;

            float scaleChange = 0.4f;
            velocity += new UnityEngine.Vector3(valX* scaleChange, valY* scaleChange, valZ* scaleChange);
            velocity = velocity.normalized;
            curX = (long)(curX + velocity.x * 4.0f);
            curY = (long)(curY + velocity.y * 4.0f);
            curZ = (long)(curZ + velocity.z * 4.0f);

            //curY = (long)Math.Round(GetChunkProperty(curX, 0, curZ, elevationProp));
            float caveWidth = 4.0f;
            int caveWidthI = (int)Math.Floor(caveWidth);

            for (int j = -caveWidthI; j <= caveWidthI; j++)
            {
                for (int l = -caveWidthI; l <= caveWidthI; l++)
                {
                    for (int k = -caveWidthI; k <= caveWidthI; k++)
                    {
                        if (Math.Sqrt(j * j + k * k + l * l) <= caveWidth)
                        {
                            SetBlock(curX + j, curY + k, curZ + l, Example.Air);
                        }
                    }
                }
            }
        }
    }

    public void OnTree(long x, long y, long z, BlockData outBlock)
    {
        float elevation = outBlock.GetChunkProperty(elevationProp);
        long elevationL = (long)Math.Round(elevation);

        if (Math.Abs(y-elevationL) < 10)
        {
            y = elevationL;
            int treeHeight = (int)Math.Round(Simplex.Noise.rand(x, y + 2, z) * 20 + 6);
            for (int i = 0; i < treeHeight; i++)
            {
                if (i == 0)
                {
                    SetBlock(x, y + i, z, Example.Trunk);
                    using (BlockData trunkData = GetBlockData(x, y + i, z))
                    {
                        trunkData.state = 4;
                        trunkData.lightingState = 0;
                        trunkData.animationState = 0;
                    }
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


                bool addedLeaf = false;
                for (int j = -widthI; j <= widthI; j++)
                {
                    for (int k = -widthI; k <= widthI; k++)
                    {
                        if (Math.Sqrt(j * j + k * k) <= width)
                        {
                            if (j != 0 || k != 0)
                            {
                                addedLeaf = true;
                            }
                            SetBlock(x + j, y + i, z + k, Example.Leaf);
                        }
                    }
                }
                if (addedLeaf)
                {

                    SetBlock(x, y + i, z, Example.Trunk);
                    using (BlockData trunkData = GetBlockData(x, y + i, z))
                    {
                        trunkData.state = 4;
                        trunkData.lightingState = 0;
                        trunkData.animationState = 0;
                    }
                }

            }
        }
    }

    /// <summary>
    /// Returns true if the given block pos is on or below the surface, otherwise returns false
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="distFromSurface">if this equals 1, we are touching the sky</param>
    /// <returns></returns>
    public bool OnOrBelowSurface(long x, long y, long z, out long distFromSurface)
    {
        long elevationL = (long)Math.Round(GetChunkProperty(x, y, z, elevationProp));
        long elevationAbove = (long)Math.Round(GetChunkProperty(x, y + 1, z, elevationProp));

        distFromSurface = elevationL - y;
        long aboveDistFromSurface = elevationAbove - (y + 1);
        bool topInAir = aboveDistFromSurface <= 0;
        bool curInAir = distFromSurface <= 0;
        if (curInAir)
        {
            return false;
        }
        else
        {
            // force dist to be 1 if the one above us is air (due to rounding we might go from 0 to 2 in elevation so this allows us to ensure everything knows if it is exactly on the surface)
            if (topInAir)
            {
                distFromSurface = 1;
            }
            return true;
        }
    }

    public override void OnGenerateBlock(long x, long y, long z, BlockData outBlock)
    {
        
        float elevation = outBlock.GetChunkProperty(elevationProp);
        //if (y <= 0)
        //{
        //    outBlock.block = BlockValue.STONE;
        //}
        long elevationL = (long)Math.Round(elevation);
        long elevationAbove = (long)Math.Round(GetChunkProperty(x, y + 1, z, elevationProp));
        if (y >= elevationL)
        {
            // air, but allow other things to go here instead
            outBlock.block = Example.Wildcard;
        }
        else
        {
            long distFromSurface = elevationL - y;
            long aboveDistFromSurface = elevationAbove - (y+1);
            bool topInAir = aboveDistFromSurface <= 0;

            if (topInAir)
            {
                outBlock.block = Example.Grass;
                // low pr of making tree
                if (Simplex.Noise.rand(x, y, z) < 0.01f)
                {
                    if (Simplex.Noise.rand(x*2+1, y, z) < 0.9f)
                    {
                        SetBlock(x, y + 1, z, Example.FlowerWithNectar);
                    }
                    else
                    {

                    }
                }
                else if(false)
                {
                   
                }
            }
            else if (distFromSurface < 3)
            {
                outBlock.block = Example.Dirt;
            }
            else if(distFromSurface < 5)
            {
                outBlock.block = Example.Clay;
            }
            else
            {
                float rVal = Simplex.Noise.rand(x, y, z);
                if (rVal < 0.9 && distFromSurface == 5)
                {
                    outBlock.block = Example.LooseRocks;
                }
                else if (rVal < 0.5 && distFromSurface == 6)
                {
                    outBlock.block = Example.LooseRocks;
                }
                else if (rVal < 0.2 && distFromSurface == 7)
                {
                    outBlock.block = Example.LooseRocks;
                }
                else if (rVal < 0.04)
                {
                    outBlock.block = Example.LooseRocks;
                }
                else
                {
                    outBlock.block = Example.Stone;
                }
                if (outBlock.block == Example.LooseRocks)
                {
                    outBlock.state = (int)System.Math.Round(Simplex.Noise.rand(x * 3, y * 5, z * 6) * 2.0f + 1.0f);
                }
                else if(outBlock.block == Example.Stone)
                {
                    int numRocks = (int)System.Math.Round(Simplex.Noise.rand(x * 3, y * 5, z * 6) * 2.0f + 1.0f);
                    int numLargeRocks = (int)System.Math.Round(Simplex.Noise.rand(x * 3, y * 2, z * 3) * 2.0f + 1.0f);
                    outBlock.state = numRocks + numLargeRocks * 6;
                }
            }
        }
    }
}
