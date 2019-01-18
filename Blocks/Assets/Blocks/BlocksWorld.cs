using Example_pack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Blocks
{
    public class QuickGrowingArray<T> where T : class
    {
        T[] arr;
        long count;
        public long Count
        {
            get
            {
                return count;
            }
            private set
            {

            }
        }
        public QuickGrowingArray(int initialCapacity)
        {
            arr = new T[initialCapacity];
            count = 0;
        }

        public T this[long i]
        {
            get
            {
                if (i >= arr.LongLength)
                {
                    return null;
                }
                if (count < i+1)
                {
                    count = i + 1;
                }
                return arr[i];
            }
            set
            {
                while (i >= arr.LongLength)
                {
                    ExpandArr();
                }
                if (count < i + 1)
                {
                    count = i + 1;
                }
                arr[i] = value;
            }
        }

        void ExpandArr()
        {
            T[] newArr = new T[arr.Length * 2];
            for (int i = 0; i < count; i++)
            {
                newArr[i] = arr[i];
            }
            arr = newArr;
        }

        public void Clear()
        {
            if (count > 0)
            {
                System.Array.Clear(arr, 0, arr.Length);
            }
        }
    }

    public class QuickLongDict<T> : IEnumerable<KeyValuePair<long, T>> where T : class
    {
        QuickGrowingArray<T> negativeValues;
        QuickGrowingArray<T> positiveValues;
        public QuickLongDict(int initialCapacity)
        {
            negativeValues = new QuickGrowingArray<T>(initialCapacity);
            positiveValues = new QuickGrowingArray<T>(initialCapacity);
        }

        public void Clear()
        {
            negativeValues.Clear();
            positiveValues.Clear();
        }

        public bool ContainsKey(long i)
        {
            return this[i] != null;
        }

        public bool ContainsKey(long i, out T val)
        {
            val = this[i];
            return val != null;
        }

        public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
        {
            for (long i = 1; i < negativeValues.Count; i++)
            {
                if (negativeValues[i] != null)
                {
                    yield return new KeyValuePair<long, T>(-i, negativeValues[i]);
                }
            }

            for (long i = 0; i < positiveValues.Count; i++)
            {
                if (positiveValues[i] != null)
                {
                    yield return new KeyValuePair<long, T>(i, positiveValues[i]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (long i = 1; i < negativeValues.Count; i++)
            {
                if (negativeValues[i] != null)
                {
                    yield return new KeyValuePair<long, T>(-i, negativeValues[i]);
                }
            }

            for (long i = 0; i < positiveValues.Count; i++)
            {
                if (positiveValues[i] != null)
                {
                    yield return new KeyValuePair<long, T>(i, positiveValues[i]);
                }
            }
        }

        public T this[long i]
        {
           get
            {
                if (i < 0)
                {
                    return negativeValues[-i];
                }
                else
                {
                    return positiveValues[i];
                }
            }
            set
            {
                if (i < 0)
                {
                    negativeValues[-i] = value;
                }
                else
                {
                    positiveValues[i] = value;
                }
            }
        }
    }




    public class FastSimpleBlockLookup : IEnumerable<KeyValuePair<BlockValue, BlockOrItem>>
    {
        public int[] ids;
        public BlockValue[] values;
        public BlockOrItem[] customItems;
        int maxBlocks;

        public FastSimpleBlockLookup(int maxBlocks)
        {
            ids = new int[maxBlocks];
            values = new BlockValue[maxBlocks];
            customItems = new BlockOrItem[maxBlocks];
            this.maxBlocks = maxBlocks;
        }

        public BlockOrItem this[BlockValue key]
        {
            get
            {
                int uid = System.Math.Abs(key.id);
                return customItems[uid];
            }
            set
            {
                int uid = System.Math.Abs(key.id);
                ids[uid] = key.id;
                values[uid] = key;
                customItems[uid] = value;
            }
        }

        public bool ContainsKey(BlockValue value, out BlockOrItem customItem)
        {
            int uid = System.Math.Abs(value.id);
            customItem = customItems[uid];
            return customItem != null;
        }

        public void Clear()
        {
            ids = new int[maxBlocks];
            values = new BlockValue[maxBlocks];
            customItems = new BlockOrItem[maxBlocks];
        }

        
        public IEnumerator<KeyValuePair<BlockValue, BlockOrItem>> GetEnumerator()
        {
            for (int i = 0; i < maxBlocks; i++)
            {
                if (customItems[i] != null)
                {
                    yield return new KeyValuePair<BlockValue, BlockOrItem>(values[i], customItems[i]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < maxBlocks; i++)
            {
                if (customItems[i] != null)
                {
                    yield return new KeyValuePair<BlockValue, BlockOrItem>(values[i], customItems[i]);
                }
            }
        }
    }



    // fast for just a few elements (10-20)
    public class FastSmallDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        T1[] keys;
        T2[] values;

        int firstEmptyPos;
        public FastSmallDictionary(int maxCapacity)
        {
            keys = new T1[maxCapacity];
            values = new T2[maxCapacity];
            firstEmptyPos = 0;
        }


        public bool ContainsKey(T1 key, out T2 value)
        {
            value = default(T2);
            for (int i = 0; i < firstEmptyPos; i++)
            {
                if (keys[i].Equals(key))
                {
                    value = values[i];
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            firstEmptyPos = 0;
        }

        public T2 this[T1 key]
        {
            get
            {
                for (int i = 0; i < firstEmptyPos; i++)
                {
                    if (keys[i].Equals(key))
                    {
                        return values[i];
                    }
                }
                return default(T2); // this usually means null
            }
            set
            {
                keys[firstEmptyPos] = key;
                values[firstEmptyPos] = value;
                firstEmptyPos += 1;
            }
        }

        public T2 this[int key]
        {
            get
            {
                return values[key];
            }
            set
            {
                values[key] = value;
            }
        }

        public int KeyToInt(T1 key)
        {
            for (int i = 0; i < firstEmptyPos; i++)
            {
                if (keys[i].Equals(key))
                {
                    return i;
                }
            }
            throw new System.ArgumentException("key " + key + " is not in the dictionary");
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            for (int i = 0; i < firstEmptyPos; i++)
            {
                yield return new KeyValuePair<T1, T2>(keys[i], values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < firstEmptyPos; i++)
            {
                yield return new KeyValuePair<T1, T2>(keys[i], values[i]);
            }
        }
    }

    public class WorldPosition
    {

        long chunkX, chunkY, chunkZ;
        float xInChunk, yInChunk, zInChunk;

        public WorldPosition(long chunkX, long chunkY, long chunkZ, float xInChunk, float yInChunk, float zInChunk)
        {
            this.chunkX = chunkX;
            this.chunkY = chunkY;
            this.chunkZ = chunkZ;
            this.xInChunk = xInChunk;
            this.yInChunk = yInChunk;
            this.zInChunk = zInChunk;
        }
    }


    public class Tuple<T1, T2, T3, T4>
    {
        public T1 a;
        public T2 b;
        public T3 c;
        public T4 d;
        public Tuple(T1 a, T2 b, T3 c, T4 d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }
    public class Tuple<T1, T2, T3>
    {
        public T1 a;
        public T2 b;
        public T3 c;
        public Tuple(T1 a, T2 b, T3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public class Tuple<T1, T2>
    {
        public T1 a;
        public T2 b;
        public Tuple(T1 a, T2 b)
        {
            this.a = a;
            this.b = b;
        }
    }


    public class ChunkRenderer
    {
        Chunk chunk;
        int chunkSize;
        public ComputeBuffer drawDataTransparent;
        public ComputeBuffer drawDataNotTransparent;
        public int numRendereredCubesTransparent;
        public int numRendereredCubesNotTransparent;

        public ComputeBuffer combinedDrawDataNotTransparent;
        public int numRenderedCubesCombinedNotTransparent;

        // True if meta node is drawing for us because it combined draw data of children nodes
        public ChunkRenderer parentChunkRenderer = null;

        // True if (cx, cy, cz) % 2 == (0,0,0)
        public bool isMetaNode = false;
        public bool combiningDrawData = false;
        public bool maybeCombineDrawData = true;
        ChunkRenderer[] otherChunks;

        public ChunkRenderer(Chunk chunk, int chunkSize)
        {
            this.chunk = chunk;
            this.chunkSize = chunkSize;

            drawDataTransparent = new ComputeBuffer(chunkSize * chunkSize * chunkSize*12, sizeof(int) * 22, ComputeBufferType.Append);
            drawDataNotTransparent = new ComputeBuffer(chunkSize * chunkSize * chunkSize*12, sizeof(int) * 22, ComputeBufferType.Append);


            if (chunk.cx % 2 == 0 && chunk.cy % 2 == 0 && chunk.cz % 2 == 0)
            {
                isMetaNode = true;
                combinedDrawDataNotTransparent = new ComputeBuffer(chunkSize * chunkSize * chunkSize * 12*8, sizeof(int) * 22, ComputeBufferType.Append);
                otherChunks = new ChunkRenderer[8];
                otherChunks[0] = this;
                parentChunkRenderer = this;
            }
            else
            {
                isMetaNode = false;
            }
        }

        public void Tick()
        {

        }

        public void Render(bool onlyTransparent, Chunk chunk, ref int numAllowedToDoFullRender)
        {
            if (chunk.chunkData.needToBeUpdated && numAllowedToDoFullRender > 0)
            {
                numAllowedToDoFullRender -= 1;
                chunk.chunkData.needToBeUpdated = false;
                chunk.world.blocksWorld.MakeChunkTris(chunk, out numRendereredCubesNotTransparent, out numRendereredCubesTransparent);
                // if we don't have a parent, try to find it
                if (parentChunkRenderer == null)
                {
                    long parentCX = chunk.world.divWithFloor(chunk.cx, 2) * 2;
                    long parentCY = chunk.world.divWithFloor(chunk.cy, 2) * 2;
                    long parentCZ = chunk.world.divWithFloor(chunk.cz, 2) * 2;
                    Chunk parentChunk = chunk.world.GetChunk(parentCX, parentCY, parentCZ);
                    if (parentChunk != null)
                    {
                        parentChunkRenderer = parentChunk.chunkRenderer;
                    }
                }
                // this should not be an else since the above test might modify parentChunkRender to not null
                if (parentChunkRenderer != null)
                {
                    ////  (this is okay to do even if we are our own parent since we will go check in a moment):
                    // it should not be since we just modified ourself
                    parentChunkRenderer.combiningDrawData = false;
                    // tell it to check again
                    parentChunkRenderer.maybeCombineDrawData = true;
                }
            }


            // if we are a parent and aren't combining but something has updated, check to see if we can now
            if (isMetaNode && !combiningDrawData && maybeCombineDrawData && numAllowedToDoFullRender > 8)
            {
                int ind = 0;
                bool allRendered = true;
                int numGood = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            if (otherChunks[ind] == null)
                            {
                                Chunk other = chunk.world.GetChunk(chunk.cx + i, chunk.cy + j, chunk.cz + k);
                                if (other != null)
                                {
                                    otherChunks[ind] = other.chunkRenderer;
                                    other.chunkRenderer.parentChunkRenderer = this;

                                    // a child is not finished, we can't combine them
                                    if (other.generating || other.chunkData.needToBeUpdated)
                                    {
                                        allRendered = false;
                                    }
                                    else
                                    {
                                        numGood += 1;
                                    }
                                }
                                else
                                {
                                    allRendered = false;
                                }
                            }
                            else
                            {
                                numGood += 1;
                            }
                            ind += 1;
                        }
                    }
                }
                // everyone is done, we can combine them into one!
                if (allRendered)
                {
                    numAllowedToDoFullRender -= 8;
                    CombineAndRenderChildrenDrawData();
                }
                else
                {
                }
            }




            if (!isMetaNode)
            {
                // parent is rendering for us, we don't need to render
                if (parentChunkRenderer != null && parentChunkRenderer.combiningDrawData)
                {

                }
                // parent isn't rendering for us, we are renderering right now because all of our siblings aren't finished yet
                else
                {
                    chunk.world.blocksWorld.RenderChunk(chunk, onlyTransparent);
                }
            }
            // we are a parent
            else
            {
                // we are drawing our combined data
                if (combiningDrawData)
                {
                    // temporairly set our draw data to be the combined stuff instead
                    ComputeBuffer tmp = drawDataNotTransparent;
                    int tmp2 = numRendereredCubesNotTransparent;
                    numRendereredCubesNotTransparent = numRenderedCubesCombinedNotTransparent;
                    drawDataNotTransparent = combinedDrawDataNotTransparent;
                    // draw us with the combined data
                    chunk.world.blocksWorld.RenderChunk(chunk, onlyTransparent);
                    // set the data back to what it was before
                    drawDataNotTransparent = tmp;
                    numRendereredCubesNotTransparent = tmp2;
                }
                // just render us
                else
                {
                    chunk.world.blocksWorld.RenderChunk(chunk, onlyTransparent);
                }
            }
        }

        public void CombineAndRenderChildrenDrawData()
        {
            combinedDrawDataNotTransparent.SetCounterValue(0);
            for (int i = 0; i < otherChunks.Length; i++)
            {
                chunk.world.blocksWorld.MakeChunkTrisForCombined(otherChunks[i].chunk, combinedDrawDataNotTransparent);
            }

            int[] args = new int[] { 0 };
            ComputeBuffer.CopyCount(combinedDrawDataNotTransparent, chunk.world.argBuffer, 0);
            chunk.world.argBuffer.GetData(args);
            numRenderedCubesCombinedNotTransparent = args[0];
            combiningDrawData = true;
        }

        bool cleanedUp = false;

        public void Dispose()
        {
            if (!cleanedUp)
            {
                cleanedUp = true;
                if (drawDataTransparent != null)
                {
                    drawDataTransparent.Dispose();
                    drawDataTransparent = null;
                }

                if (drawDataNotTransparent != null)
                {
                    drawDataNotTransparent.Dispose();
                    drawDataNotTransparent = null;
                }

                if (combinedDrawDataNotTransparent != null)
                {
                    combinedDrawDataNotTransparent.Dispose();
                    combinedDrawDataNotTransparent = null;
                }
            }
        }
    }



    public class BlocksTouchingSky
    {
        public class BlockTouchingSkyChunk
        {
            public long cx, cz;
            public int chunkSize;
            public long[,] highestBlocks;
            public BlockTouchingSkyChunk(long cx, long cz, int chunkSize)
            {
                this.chunkSize = chunkSize;
                this.cx = cx;
                this.cz = cz;
                highestBlocks = new long[chunkSize, chunkSize];
                for (int i = 0; i < chunkSize; i++)
                {
                    for (int j = 0; j < chunkSize; j++)
                    {
                        highestBlocks[i, j] = long.MinValue;
                    }
                }
            }
        }

        Dictionary<long, List<BlockTouchingSkyChunk>> xLookup;
        Dictionary<long, List<BlockTouchingSkyChunk>> zLookup;

        World world;
        public BlocksTouchingSky(World world)
        {
            this.world = world;
            xLookup = new Dictionary<long, List<BlockTouchingSkyChunk>>();
            zLookup = new Dictionary<long, List<BlockTouchingSkyChunk>>();
        }


        BlockTouchingSkyChunk GetOrCreateSkyChunkAtPosition(long x, long z)
        {
            long cx = world.divWithFloorForChunkSize(x);
            long cz = world.divWithFloorForChunkSize(z);
            return GetOrCreateSkyChunk(cx, cz);
        }


        public BlockTouchingSkyChunk GetOrCreateSkyChunk(long cx, long cz)
        {
            BlockTouchingSkyChunk skyChunk = GetSkyChunk(cx, cz);
            if (skyChunk == null)
            {
                skyChunk = new BlockTouchingSkyChunk(cx, cz, world.chunkSize);
                if (!xLookup.ContainsKey(cx))
                {
                    xLookup[cx] = new List<BlockTouchingSkyChunk>();
                }
                if (!zLookup.ContainsKey(cz))
                {
                    zLookup[cz] = new List<BlockTouchingSkyChunk>();
                }
                xLookup[cx].Add(skyChunk);
                zLookup[cz].Add(skyChunk);
            }
            return skyChunk;
        }

        BlockTouchingSkyChunk GetSkyChunk(long cx, long cz)
        {
            List<BlockTouchingSkyChunk> xChunks = null;
            if (xLookup.ContainsKey(cx))
            {
                xChunks = xLookup[cx];
            }
            List<BlockTouchingSkyChunk> zChunks = null;
            if (zLookup.ContainsKey(cz))
            {
                zChunks = zLookup[cz];
            }

            if (xChunks == null && zChunks == null)
            {
                return null;
            }

            List<BlockTouchingSkyChunk> chunksLookingThrough = null;

            if (xChunks != null && zChunks != null)
            {
                if (xChunks.Count < zChunks.Count)
                {
                    chunksLookingThrough = xChunks;
                }
                else
                {
                    chunksLookingThrough = zChunks;
                }
            }
            else if(xChunks != null)
            {
                chunksLookingThrough = xChunks;
            }
            else
            {
                chunksLookingThrough = zChunks;
            }


            foreach (BlockTouchingSkyChunk chunk in chunksLookingThrough)
            {
                if (chunk.cx == cx && chunk.cz == cz)
                {
                    return chunk;
                }
            }
            return null;
        }


        // 0 is air, negative is transparent, positive is solid
        bool IsSolid(int blockId)
        {
            return blockId > 0;
        }

        public void SetNotTouchingSky(long x, long y, long z)
        {
            int prevLightingState = world.GetState(x, y, z, BlockState.Lighting);
            world.SetState(x, y, z, prevLightingState & (~Chunk.TOUCHING_SKY_BIT), BlockState.Lighting);
            world.AddBlockUpdate(x, y, z, true);
        }

        public void SetTouchingSky(long x, long y, long z)
        {
            int prevLightingState = world.GetState(x, y, z, BlockState.Lighting);
            world.SetState(x, y, z, prevLightingState | Chunk.TOUCHING_SKY_BIT | (15 << 4), BlockState.Lighting);
            world.AddBlockUpdate(x, y, z, true);
        }

        /// <summary>
        /// Assumes you update the BlockTouchingSkyChunk data elsewhere (before or after calling this method is fine), this just updates the block lighting states
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="prevY"></param>
        public void AddedHigherBlockTouchingSky(long x, long z, long prevHighestY, long newHighestY)
        {
            // we didn't have one before, don't update the previous one
            if (prevHighestY == long.MinValue)
            {

            }
            // we did have one before, tell it it isn't touching sky anymore
            else
            {
                SetNotTouchingSky(x, prevHighestY, z);
                world.AddBlockUpdateToNeighbors(x, prevHighestY, z);
            }
            // tell the chunk that that block is touching sky
            SetTouchingSky(x, newHighestY, z);
            world.AddBlockUpdateToNeighbors(x, newHighestY, z);
        }

        public void RemovedSolidBlockTouchingSky(long x, long y, long z)
        {
            SetNotTouchingSky(x, y, z);
            long cx = world.divWithFloorForChunkSize(x);
            long cz = world.divWithFloorForChunkSize(z);
            long relativeX = x - cx * world.chunkSize;
            long relativeZ = z - cz * world.chunkSize;
            BlockTouchingSkyChunk skyChunk = GetOrCreateSkyChunk(cx, cz);
            long curY = y;
            long cy = world.divWithFloorForChunkSize(y);
            while(true)
            {
                curY -= 1;
                long curCy = world.divWithFloorForChunkSize(curY);
                // went to next chunk down
                if (curCy != cy)
                {
                    bool hoppedDown = false;
                    // chunk isn't generated yet, skip to next chunk below until we find a generated one
                    while (world.GetChunkAtPos(x,curY,z) == null)
                    {
                        curCy -= 1;
                        hoppedDown = true;
                        // lower than any chunk so there aren't any, bail
                        if (curCy < world.GetLowestCy())
                        {
                            // don't have any, set back to default "unknown" value
                            skyChunk.highestBlocks[relativeX, relativeZ] = long.MinValue;
                            return;
                        }
                    }
                    // if we hopped down a chunk or more (due to the chunk inbetween not being generated), start searching at the ceiling of that chunk
                    if (hoppedDown)
                    {
                        curY = curCy * world.chunkSize + world.chunkSize - 1;
                    }
                    cy = curCy;
                }

                // found something solid that the sky is now touching
                if (IsSolid(world[x, curY, z]))
                {
                    skyChunk.highestBlocks[relativeX, relativeZ] = curY;
                    SetTouchingSky(x, curY, z);
                    return;
                }
            }

            // look for lower things that will now be touching the sky
            
        }

        public void AddedSolidBlock(long x, long y, long z)
        {
            long cx = world.divWithFloorForChunkSize(x);
            long cz = world.divWithFloorForChunkSize(z);
            long relativeX = x - cx * world.chunkSize;
            long relativeZ = z - cz * world.chunkSize;
            BlockTouchingSkyChunk skyChunk = GetOrCreateSkyChunk(cx, cz);
            long curHighestY = skyChunk.highestBlocks[relativeX, relativeZ];
            if (y > curHighestY)
            {
                AddedHigherBlockTouchingSky(x, z, curHighestY, y);
                skyChunk.highestBlocks[relativeX, relativeZ] = y;
            }
        }




        public void GeneratedChunk(Chunk chunk)
        {
            BlockTouchingSkyChunk skyChunk = GetOrCreateSkyChunk(chunk.cx, chunk.cz);
            long chunkX = chunk.cx * world.chunkSize;
            long chunkY = chunk.cy * world.chunkSize;
            long chunkZ = chunk.cz * world.chunkSize;
            for (long x = 0; x < world.chunkSize; x++)
            {
                for (long z = 0; z < world.chunkSize; z++)
                {
                    long curHighestY = skyChunk.highestBlocks[x, z];
                    long highestYInNewChunk = chunk.cy * world.chunkSize + world.chunkSize - 1;
                    // we already have something higher than the top of this chunk, ignore this xz position
                    if (highestYInNewChunk < curHighestY)
                    {
                        continue;
                    }
                    else
                    {
                        // there might be something higher, check the column of blocks starting at the roof of the chunk
                        for (long y = world.chunkSize-1; y >= 0; y--)
                        {
                            long curY = chunkY + y;
                            // we are now below the known highest, we are done
                            if (curY < curHighestY)
                            {
                                break;
                            }

                            // found one that is higher, update
                            if (IsSolid(chunk.chunkData[x,y,z]))
                            {
                                AddedHigherBlockTouchingSky(x+ chunkX, z+chunkZ, curHighestY, chunkY+y);
                                curHighestY = chunkY + y;
                                skyChunk.highestBlocks[x, z] = curHighestY;
                                break;
                            }
                            // previously highest one has been removed
                            else if(curY == curHighestY)
                            {
                                RemovedSolidBlockTouchingSky(chunkX + x, chunkY + y, chunkZ + z);
                                // update cur highest y to the new height of the tallest one that RemovedSolidBlockTouchingSky found
                                curHighestY = skyChunk.highestBlocks[x, z];
                            }
                        }
                    }
                }
            }
        }
    }

    public class ChunkBiomeData
    {
        public float altitude;
        public long cx, cy, cz;

        public float[] chunkProperties;
        public ChunkProperties chunkPropertiesObj;


        public ChunkBiomeData(ChunkProperties chunkProperties, long cx, long cy, long cz)
        {
            this.chunkPropertiesObj = chunkProperties;
            this.chunkProperties = chunkProperties.GenerateChunkPropertiesArr(cx, cy, cz);
            this.cx = cx;
            this.cy = cy;
            this.cz = cz;
        }

        public void RunChunkPropertyEventsOnGeneration()
        {
            chunkPropertiesObj.RunEvents(cx, cy, cz);
        }

        ChunkBiomeData x0y0z0,
                       x1y0z0,
                       x0y1z0,
                       x1y1z0,
                       x0y0z1,
                       x1y0z1,
                       x0y1z1,
                       x1y1z1;

        public void FetchNeighbors()
        {
            x0y0z0 = this;
            x1y0z0 = World.mainWorld.GetChunkBiomeData(cx + 1, cy, cz);
            x0y1z0 = World.mainWorld.GetChunkBiomeData(cx, cy + 1, cz);
            x1y1z0 = World.mainWorld.GetChunkBiomeData(cx + 1, cy + 1, cz);
            x0y0z1 = World.mainWorld.GetChunkBiomeData(cx, cy, cz + 1);
            x1y0z1 = World.mainWorld.GetChunkBiomeData(cx + 1, cy, cz + 1);
            x0y1z1 = World.mainWorld.GetChunkBiomeData(cx, cy + 1, cz + 1);
            x1y1z1 = World.mainWorld.GetChunkBiomeData(cx + 1, cy + 1, cz + 1);
        }

        public float AverageBiomeData(long wx, long wy, long wz, ChunkProperty chunkProperty)
        {
            int key = chunkProperty.index;
            if (x0y0z0 == null)
            {
                FetchNeighbors();
            }
            int chunkSize = World.mainWorld.chunkSize;

            // relative to us at 0
            long relx = wx - cx * chunkSize;
            long rely = wy - cy * chunkSize;
            long relz = wz - cz * chunkSize;

            float x0Weight = 1.0f - relx / (float)chunkSize;
            float y0Weight = 1.0f - rely / (float)chunkSize;
            float z0Weight = 1.0f - relz / (float)chunkSize;

            // average over x first

            float y0z0 = x0y0z0.chunkProperties[key] * x0Weight + x1y0z0.chunkProperties[key] * (1 - x0Weight);
            float y1z0 = x0y1z0.chunkProperties[key] * x0Weight + x1y1z0.chunkProperties[key] * (1 - x0Weight);
            float y0z1 = x0y0z1.chunkProperties[key] * x0Weight + x1y0z1.chunkProperties[key] * (1 - x0Weight);
            float y1z1 = x0y1z1.chunkProperties[key] * x0Weight + x1y1z1.chunkProperties[key] * (1 - x0Weight);

            // then average over y

            float z0 = y0z0 * y0Weight + y1z0 * (1 - y0Weight);
            float z1 = y0z1 * y0Weight + y1z1 * (1 - y0Weight);

            // then average over z

            return z0 * z0Weight + z1 * (1 - z0Weight);
        }


    }

    /*
    public enum BlockValue
    {
        AXE = -24,
        PICKAXE = -23,
        SHOVEL = -22,
        STRING = -21,
        WET_BARK = 20,
        BARK = 19,
        LARGE_SHARP_ROCK = -18,
        SHARP_ROCK = -17,
        LARGE_ROCK = -16,
        ROCK = -15,
        STICK = -14,
        LOOSE_ROCKS = 13,
        CLAY = 12,
        CHEST = 11,
        EMPTY = 10,
        LEAF = 9,
        TRUNK = 8,
        WATER_NOFLOW = 7,
        WATER = -6,
        BEDROCK = 5,
        DIRT = 4,
        GRASS = 3,
        STONE = 2,
        SAND = 1,
        Air = 0,
        Wildcard = -1
    }
    */


    public class BlockData : System.IDisposable
    {
        bool wasModified;
        // world.blockModifyState is incremented whenever a change occurs
        // this lets us not have to check for changes that may have occured unless the world's value of this is different than ours
        // once we make a change, we can set ours to the world's value since it will be incremented (see the getters of state1-3 and block below)
        long curBlockModifyState1 = 0;
        long curBlockModifyState2 = 0;
        long curBlockModifyState3 = 0;
        long curBlockModifyStateBlock = 0;
        public Chunk myChunk;
        public bool WasModified
        {
            get
            {
                return wasModified;
            }
            private set
            {

            }
        }

        public float rand()
        {
            return Random.value;
        }

        private long wx, wy, wz;
        public long cx, cy, cz;

        public long x { get { return wx; } private set { } }
        public long y { get { return wy; } private set { } }
        public long z { get { return wz; } private set { } }
        public LVector3 pos
        {
            get
            {
                return new LVector3(wx, wy, wz);
            }
            private set
            {

            }
        }
        BlockGetter world;

        int cachedState1 = 0;
        int cachedState2 = 0;
        int cachedState3 = 0;
        BlockValue cachedBlock = BlockValue.Air;

        int GetState(long wx, long wy, long wz, long cx, long cy, long cz, BlockState stateType)
        {
            if (myChunk == null)
            {
                return world.GetState(wx, wy, wz, cx, cy, cz, stateType);
            }
            else
            {
                return myChunk.GetState(wx, wy, wz, stateType);
            }
        }

        void SetState(long wx, long wy, long wz, long cx, long cy, long cz, int value, BlockState stateType)
        {
            if (myChunk == null)
            {
                world.SetState(wx, wy, wz, cx, cy, cz, value, stateType);
            }
            else
            {
                myChunk.SetState(wx, wy, wz, value, stateType);
                world.blockModifyState += 1;
            }
        }

        BlockValue GetBlock(long wx, long wy, long wz, long cx, long cy, long cz)
        {
            if (myChunk == null)
            {
                return world[wx, wy, wz, cx, cy, cz];
            }
            else
            {
                return myChunk[wx, wy, wz];
            }
        }

        void SetBlock(long wx, long wy, long wz, long cx, long cy, long cz, BlockValue value)
        {
            if (myChunk == null)
            {
                world[wx, wy, wz, cx, cy, cz] = value;
            }
            else
            {
                myChunk[wx, wy, wz] = value;
                world.blockModifyState += 1;
            }
        }

        public int state { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState1) { cachedState1 = GetState(wx, wy, wz, cx, cy, cz, BlockState.State); curBlockModifyState1 = world.blockModifyState; } return cachedState1; } set { if (value != state) { wasModified = true; cachedState1 = value; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.State); curBlockModifyState1 = world.blockModifyState; CheckLocalStates(); } } }
        public int lightingState { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState2) { cachedState2 = GetState(wx, wy, wz, cx, cy, cz, BlockState.Lighting); curBlockModifyState2 = world.blockModifyState; } return cachedState2; } set { if (value != lightingState) { wasModified = true; cachedState2 = value; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.Lighting); curBlockModifyState2 = world.blockModifyState; CheckLocalStates(); } } }
        public int animationState { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState3) { cachedState3 = GetState(wx, wy, wz, cx, cy, cz, BlockState.Animation); curBlockModifyState3 = world.blockModifyState; } return cachedState3; } set { if (value != animationState) { wasModified = true; cachedState3 = value; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.Animation); curBlockModifyState3 = world.blockModifyState; CheckLocalStates(); } } }
        public BlockValue block { get { if (!isGenerated) { return BlockValue.Wildcard; } if (world.blockModifyState != curBlockModifyStateBlock) { cachedBlock = GetBlock(wx, wy, wz, cx, cy, cz); curBlockModifyStateBlock = world.blockModifyState; } return cachedBlock; } set { if (value != block) { wasModified = true; cachedBlock = value; SetBlock(wx, wy, wz, cx, cy, cz,value); curBlockModifyStateBlock = world.blockModifyState; CheckLocalStates(); } } }
        //public int state2 { get { return world.GetState(wx, wy, wz, cx, cy, cz, 2); } set { if (curBlockModifyState != world.blockModifyState && world.GetState(wx, wy, wz, cx, cy, cz, 2) != value) { wasModified = true; world.SetState(wx, wy, wz, cx, cy, cz, value, 2); curBlockModifyState = world.blockModifyState; } } }
        //public int state3 { get { return world.GetState(wx, wy, wz, cx, cy, cz, 3); } set { if (curBlockModifyState != world.blockModifyState && world.GetState(wx, wy, wz, cx, cy, cz, 3) != value) { wasModified = true; world.SetState(wx, wy, wz, cx, cy, cz, value, 3); curBlockModifyState = world.blockModifyState; } } }
        //public BlockValue block { get { return (BlockValue)world[wx, wy, wz, cx, cy, cz]; } set { if (curBlockModifyState != world.blockModifyState && (BlockValue)world[wx, wy, wz, cx, cy, cz] != value) { wasModified = true; world[wx, wy, wz, cx, cy, cz] = (int)value; curBlockModifyState = world.blockModifyState; } } }
        ChunkBiomeData chunkBiomeData;
        public bool needsAnotherTick;

        // we just modified the world, if one of our other states are only 1 behind, we are the cause of the change, therefore they are still correct, update their states so they don't need to recheck to see that they are still right
        public void CheckLocalStates()
        {
            if (curBlockModifyState1 == world.blockModifyState - 1) curBlockModifyState1 = world.blockModifyState;
            if (curBlockModifyState2 == world.blockModifyState - 1) curBlockModifyState2 = world.blockModifyState;
            if (curBlockModifyState3 == world.blockModifyState - 1) curBlockModifyState3 = world.blockModifyState;
            if (curBlockModifyStateBlock == world.blockModifyState - 1) curBlockModifyStateBlock = world.blockModifyState;
        }

        public BlockData(BlockGetter world, long x, long y, long z)
        {
            this.wasModified = false;
            this.world = world;
            this.wx = x;
            this.wy = y;
            this.wz = z;
            World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            if (World.mainWorld.GetChunk(cx, cy, cz) == null)
            {
                isGenerated = false;
            }
            else
            {
                isGenerated = true;
            }
            needsAnotherTick = false;
            myChunk = null;
        }

        public bool isGenerated = false;

        public float GetChunkProperty(ChunkProperty chunkProperty)
        {
            if (chunkBiomeData == null)
            {
                chunkBiomeData = World.mainWorld.GetChunkBiomeData(cx, cy, cz);
            }
            return chunkBiomeData.AverageBiomeData(this.wx, this.wy, this.wz, chunkProperty);
        }

        public void Dispose()
        {
            world.DoneWithBlockData(this);
        }

        public void ReassignValues(long x, long y, long z)
        {
            this.curBlockModifyState1 = -1;
            this.curBlockModifyState2 = -1;
            this.curBlockModifyState3 = -1;
            this.needsAnotherTick = false;
            this.curBlockModifyStateBlock = -1;
            this.wasModified = false;
            this.chunkBiomeData = null;
            this.wx = x;
            this.wy = y;
            this.wz = z;
            myChunk = null;
            World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            //cachedBlock = world[wx, wy, wz, cx, cy, cz];
            //this.curBlockModifyStateBlock = world.blockModifyState;
        }
    }
    public class BlockDataGetter
    {

        public World world;
        public BlockGetter blockGetter;
        public BlockDataGetter(World world, BlockGetter blockGetter)
        {
            this.world = world;
            this.blockGetter = blockGetter;
        }

        public BlockDataGetter()
        {
            this.world = World.mainWorld;
            this.blockGetter = World.mainWorld;
        }


        public BlockValue GetBlock(long x, long y, long z)
        {
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return blockGetter[x, y, z];
        }

        public void SetBlock(long x, long y, long z, BlockValue value)
        {
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            blockGetter[x, y, z] = (int)value;
        }

        public int GetState(long x, long y, long z, BlockState stateType)
        {
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return blockGetter.GetState(x, y, z, stateType);
        }

        public void SetState(long x, long y, long z, int state, BlockState stateType)
        {
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            blockGetter.SetState(x, y, z, state, stateType);
        }

        public BlockData GetBlockData(long x, long y, long z)
        {
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return blockGetter.GetBlockData(x, y, z);
        }
    }

    public abstract class StaticBlock : Block
    {
        public override void OnTick(BlockData block)
        {

        }
    }


    public abstract class Block : BlockOrItem
    {
        public override void OnTickStart()
        {

        }
        public override bool CanBePlaced()
        {
            return true;
        }
    }


    public abstract class Block2 : BlockOrItem
    {
        public override bool CanBePlaced()
        {
            return true;
        }
    }

    public abstract class Item : BlockOrItem
    {
        public override void OnTickStart()
        {

        }
        public override bool CanBePlaced()
        {
            return false;
        }

        public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
        {
            destroyBlock = true;
        }

        public override void OnTick(BlockData block)
        {

        }

        public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
        {
            return 0.0f;
        }
    }

    public abstract class BlockOrItem : BlockDataGetter
    {
        public float rand()
        {
            return Random.value;
        }
        static int globalPreference = 0;

        public int stackSize = 1;

        public IEnumerable<BlockData> GetNeighbors(BlockData block, bool includingUp = true, bool includingDown = true)
        {
            foreach (BlockData n in GetHorizontalNeighbors(block))
            {
                yield return n;
            }
            if (includingUp)
            {
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z)) yield return n;
            }
            if (includingDown)
            {
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z)) yield return n;
            }
        }


        public IEnumerable<BlockData> GetHorizontalNeighbors(BlockData block)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
            }
            else if (globalPreference == 1)
            {
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
            }
            else if (globalPreference == 2)
            {
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
            }
            else if (globalPreference == 3)
            {
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
            }
        }

        public IEnumerable<BlockData> Get26Neighbors(BlockData block)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z)) yield return n;


                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z - 1)) yield return n;


                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z - 1)) yield return n;
            }
            else if (globalPreference == 1)
            {
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z)) yield return n;


                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z + 1)) yield return n;


                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z + 1)) yield return n;
            }
            else if (globalPreference == 2)
            {

                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z)) yield return n;


                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z + 1)) yield return n;


                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z + 1)) yield return n;
            }
            else if (globalPreference == 3)
            {
                using (BlockData n = GetBlockData(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z)) yield return n;

                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z)) yield return n;


                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x, block.y + 1, block.z - 1)) yield return n;


                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockData(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockData(block.x - 1, block.y + 1, block.z - 1)) yield return n;
            }
        }

        public abstract void OnTick(BlockData block);
        public abstract float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith);
        public abstract void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock);
        public abstract bool CanBePlaced();
        public abstract void OnTickStart();


        public BlockEntity CreateBlockEntity(BlockValue block, Vector3 position)
        {
            GameObject blockEntity = GameObject.Instantiate(World.mainWorld.blocksWorld.blockEntityPrefab);
            blockEntity.transform.position = position;
            blockEntity.GetComponent<BlockEntity>().blockId = (int)block;
            return blockEntity.GetComponent<BlockEntity>();
        }
    }

    public class ChunkPropertyEvent
    {
        float lambda;

        public delegate void ChunkPropertyEventCallback(long x, long y, long z, BlockData blockData);

        public int priority;

        ChunkPropertyEventCallback eventCallback;

        public ChunkPropertyEvent(float avgNumBlocksBetween, ChunkPropertyEventCallback eventCallback, int priority)
        {
            // pretend on 1d line cause that should be good enough
            lambda = World.mainWorld.chunkSize* World.mainWorld.chunkSize / avgNumBlocksBetween;
            this.eventCallback = eventCallback;
            this.priority = priority;
        }

        public void Run(long cx, long cy, long cz)
        {
            LVector3[] points = GeneratePoints(cx, cy, cz);
            foreach (LVector3 point in points)
            {
                using (BlockData blockData = World.mainWorld.GetBlockData(point.x, point.y, point.z))
                {
                    eventCallback(point.x, point.y, point.z, blockData);
                }
            }
        }

        // see https://en.wikipedia.org/wiki/Poisson_distribution#Probability_of_events_for_a_Poisson_distribution
        public LVector3[] GeneratePoints(long cx, long cy, long cz)
        {
            float val = Simplex.Noise.Generate(cx, cy, cz);
            int chunkSize = World.mainWorld.chunkSize;
            float totalPr = 0.0f;
            int factorial = 1;
            int numPoints = chunkSize*chunkSize;
            for (int i = 0; i < chunkSize*chunkSize; i++)
            {
                float prOfThat = (float)(System.Math.Pow(lambda, i) * System.Math.Exp(-lambda) / factorial);
                if (i > 1)
                {
                    factorial *= i;
                }
                totalPr += prOfThat;
                if (val < totalPr)
                {
                    numPoints = i;
                    break;
                }
            }

            System.Random gen = new System.Random((int)(Simplex.Noise.Generate(cx, cy, cz) * 1000.0f));
            //Debug.Log(cx + " " + cy + " " + cz + " got numPoints = " + numPoints + " with lambda = " + lambda + " with chunkSize= " + chunkSize);

            LVector3[] resPoints = new LVector3[numPoints];
            for (int i = 0; i < resPoints.Length; i++)
            {
                resPoints[i] = new LVector3(gen.Next(0, chunkSize) + cx*chunkSize, gen.Next(0, chunkSize) + cy * chunkSize, gen.Next(0, chunkSize) + cz * chunkSize);
            }
            return resPoints;
        }
    }


    /// <summary>
    /// Intended for things like caves and large structures that span many chunks
    /// Will be generated in large "batches"
    /// </summary>
    public class WorldGenerationEvent
    {
        float avgNumBlocksBetween;

        public delegate void WorldPropertyEventCallback(long x, long y, long z);

        public int priority;

        WorldPropertyEventCallback eventCallback;

        public WorldGenerationEvent(float avgNumBlocksBetween, WorldPropertyEventCallback eventCallback, int priority)
        {
            this.avgNumBlocksBetween = avgNumBlocksBetween;
            this.eventCallback = eventCallback;
            this.priority = priority;
        }

        public void Run(long baseCX, long baseCY, long baseCZ, int numChunksWide)
        {
            long chunkSize = World.mainWorld.chunkSize;
            int sideLength = numChunksWide * World.mainWorld.chunkSize;
            float randSeed = Simplex.Noise.rand(baseCX, baseCY, baseCZ);
            System.Random gen = new System.Random((int)(randSeed * 10000.0f));

            int numPointsToSample = HowManyPointsToSample(gen.NextDouble(), sideLength);
            Debug.Log("running world generation event with base chunk " + baseCX + " " + baseCY + " " + baseCZ + " and chunksWide=" + numChunksWide + " and numPointsToSample = " + numPointsToSample + " and avgNumBlocksBetween " + avgNumBlocksBetween);
            //Debug.Log(cx + " " + cy + " " + cz + " got numPoints = " + numPoints + " with lambda = " + lambda + " with chunkSize= " + chunkSize);
            long baseX = baseCX * chunkSize;
            long baseY = baseCY * chunkSize;
            long baseZ = baseCZ * chunkSize;
            LVector3[] resPoints = new LVector3[numPointsToSample];
            for (int i = 0; i < resPoints.Length; i++)
            {
                long x = baseX + gen.Next(0, sideLength);
                long y = baseY + gen.Next(0, sideLength);
                long z = baseZ + gen.Next(0, sideLength);
                eventCallback(x, y, z);
            }
        }



        // it seems that the typical distance to the closest point on a 1x1x1 cube an open question so I just approximated the curve after doing numeric samples of the average minimum distnace
        // y = 1.286265*x**-0.4427251 
        // inverting that with wolfram gives us
        // x = 1.76583/(y**(2.258737984360950))
        // thus if we want the typical minimum distance between the nearest point in a 1x1x1 cube to be d, we need to uniformly sample roughly 1.76583/(d**(2.258737984360950)) points
        // the volume of this is 1. If you scale the sides to be of size wxwxw, distances scale by w as well, so we can just divide the distance by w before passing into this and that'll give us how many we need for a wxwxw cube instead
        // for rectangular shapes, don't use rectangular shapes. Instead, partition into cube pieces and do that instead

        public double PointsNeededForAverageDistance(double averageDistance, double cubeSideLength)
        {
            return 1.76583 / (System.Math.Pow(averageDistance/ cubeSideLength, 2.258737984360950));
        }

        // see https://en.wikipedia.org/wiki/Poisson_distribution#Probability_of_events_for_a_Poisson_distribution
        // I'm doing a poisson distribution that will have expected number of points = the number of points that result in typical distance to closest other structure = desired val
        public int HowManyPointsToSample(double noiseVal, long cubeSideLength)
        {
            int chunkSize = World.mainWorld.chunkSize;
            double totalPr = 0.0f;
            int factorial = 1;
            double lambda = PointsNeededForAverageDistance(avgNumBlocksBetween, cubeSideLength);
            return Mathf.RoundToInt((float)lambda);
            long maxNumPoints = cubeSideLength * cubeSideLength;
            for (int i = 0; i < maxNumPoints; i++)
            {
                double prOfThat = (System.Math.Pow(lambda, i) * System.Math.Exp(-lambda) / factorial);
                if (i > 1)
                {
                    factorial *= i;
                }
                totalPr += prOfThat;
                if (noiseVal < totalPr)
                {
                    return i;
                }
            }
            return (int)maxNumPoints;
        }
    }

    public class ChunkProperty
    {
        public int index;
        public string name;
        public float minVal;
        public float maxVal;
        public bool usesY;
        public float scale;
        public bool makeThatManyPoints;

        public ChunkProperty(string name, float minVal, float maxVal, float scale = 10.0f, bool usesY = true)
        {
            this.name = name;
            this.minVal = minVal;
            this.maxVal = maxVal;
            this.usesY = usesY;
            this.scale = scale;
            this.index = 0;
        }

        public float GenerateValue(long cx, long cy, long cz)
        {
            if (usesY)
            {
                return Simplex.Noise.Generate(cx / scale, cy / scale, cz / scale) * (maxVal - minVal) + minVal;
            }
            else
            {
                return Simplex.Noise.Generate(cx / scale, 0, cz / scale) * (maxVal - minVal) + minVal;
            }
        }
    }
    public class ChunkProperties
    {
        public List<ChunkProperty> chunkProperties = new List<ChunkProperty>();
        public List<ChunkPropertyEvent> chunkPropertyEvents = new List<ChunkPropertyEvent>();
        public int AddChunkProperty(ChunkProperty chunkProperty)
        {
            chunkProperties.Add(chunkProperty);
            chunkProperty.index = chunkProperties.Count - 1;
            return chunkProperties.Count - 1;
        }

        public void AddChunkPropertyEvent(ChunkPropertyEvent chunkPropertyEvent)
        {
            chunkPropertyEvents.Add(chunkPropertyEvent);
        }

        public void RunEvents(long cx, long cy, long cz)
        {
            foreach (ChunkPropertyEvent chunkEvent in chunkPropertyEvents)
            {
                chunkEvent.Run(cx, cy,cz);
            }
        }

        public float[] GenerateChunkPropertiesArr(long cx, long cy, long cz)
        {
            float[] res = new float[chunkProperties.Count];
            for (int i = 0; i < chunkProperties.Count; i++)
            {
                res[i] = chunkProperties[i].GenerateValue(cx, cy, cz);
            }
            return res;
        }
    }


    public abstract class GenerationClass : BlockDataGetter
    {

        public float GetChunkProperty(long x, long y, long z, ChunkProperty chunkProperty)
        {
            return world.AverageChunkValues(x, y, z, chunkProperty);
        }

        public abstract void OnGenerationInit();
        public abstract void OnGenerateBlock(long x, long y, long z, BlockData outBlock);

    }

    public class Chunk
    {
        public bool valid = true;
        public long cx, cy, cz;

        long lastWorldNumChunks;

        public long GetPos(int d)
        {
            if (d == 0)
            {
                return cx;
            }
            else if (d == 1)
            {
                return cy;
            }
            else if (d == 2)
            {
                return cz;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException("only has 3 dimensions (0-2) right now, you passed in d=" + d);
            }
        }



        int chunkSize;
        public ChunkData chunkData;
        public ChunkRenderer chunkRenderer;
        public ChunkBiomeData chunkBiomeData;
        public BlocksTouchingSky.BlockTouchingSkyChunk touchingSkyChunk;

        public Chunk[] posChunks;
        public Chunk xPosChunk { get { return posChunks[0]; } set { posChunks[0] = value; } }
        public Chunk yPosChunk { get { return posChunks[1]; } set { posChunks[1] = value; } }
        public Chunk zPosChunk { get { return posChunks[2]; } set { posChunks[2] = value; } }

        public Chunk[] negChunks;
        public Chunk xNegChunk { get { return negChunks[0]; } set { negChunks[0] = value; } }
        public Chunk yNegChunk { get { return negChunks[1]; } set { negChunks[1] = value; } }
        public Chunk zNegChunk { get { return negChunks[2]; } set { negChunks[2] = value; } }

        public World world;


        public bool TryGetHighestSolidBlockY(long x, long z, out long highestBlockY)
        {
            highestBlockY = long.MinValue;
            long relativeX = x - cx * chunkSize;
            long relativeZ = z - cz * chunkSize;
            for (int y = chunkSize-1; y >= 0; y--)
            {
                if (chunkData[relativeX, y, relativeZ] != BlockValue.Air)
                {
                    highestBlockY = y+cy*chunkSize;
                    return true;
                }
            }
            return false;
        }


        public void UpdateLighting()
        {
        }

        public void AddBlockUpdate(long i, long j, long k)
        {
            long relativeX = i - cx * chunkSize;
            long relativeY = j - cy * chunkSize;
            long relativeZ = k - cz * chunkSize;
            chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
        }


        public void CreateStuff()
        {
            this.chunkRenderer = new ChunkRenderer(this, chunkSize);
            chunkData = new ChunkData(chunkSize, fillWithWildcard: false);
            chunkData.attachedChunks.Add(this);
        }


        public Chunk(World world, ChunkProperties chunkProperties, long chunkX, long chunkY, long chunkZ, int chunkSize, bool createStuff = true)
        {
            this.world = world;
            this.chunkSize = chunkSize;
            this.cx = chunkX;
            this.cy = chunkY;
            this.cz = chunkZ;
            this.chunkBiomeData = new ChunkBiomeData(chunkProperties, cx, cy, cz);
            posChunks = new Chunk[] { null, null, null };
            negChunks = new Chunk[] { null, null, null };


            generating = true;

            if (createStuff || true)
            {
                CreateStuff();
            }

        }


        public const int TOUCHING_SKY_MASK = 255;
        public void Generate()
        {
            generating = true;
            long baseX = cx * chunkSize;
            long baseY = cy * chunkSize;
            long baseZ = cz * chunkSize;
            Structure myStructure = new Structure(cx + " " + cy + " " + cz, true, this, priority:0);
            //long start = PhysicsUtils.millis();
            //Debug.Log("generating chunk " + cx + " " + cy + " " + cz + " ");
            try
            {
                world.worldGeneration.blockGetter = myStructure;
                for (long x = baseX; x < baseX + this.chunkSize; x++)
                {
                    for (long z = baseZ; z < baseZ + this.chunkSize; z++)
                    {
                        //long curHighestBlockY = long.MinValue;
                        //world.worldGeneration.blockGetter = world;
                        //bool wasAPreviousHighest = world.TryGetHighestSolidBlockY(x, z, out curHighestBlockY);
                        //world.worldGeneration.blockGetter = myStructure;
                        //float elevation = world.AverageChunkValues(x, 0, z, c => c.chunkProperties["elevation"]);
                        // going from top to bottom lets us update the "highest block touching the sky" easily
                        for (long y = baseY + this.chunkSize-1; y >= baseY; y--)
                        {
                            //long elevation = (long)Mathf.Round(world.AverageChunkValues(x, 0, z, "altitude"));
                            using (BlockData block = myStructure.GetBlockData(x, y, z))
                            {
                                world.worldGeneration.OnGenerateBlock(x, y, z, block);
                                block.animationState = 0;
                                block.lightingState = 0;
                                if (block.block != BlockValue.Wildcard && block.block != BlockValue.Air)
                                {
                                    //chunkData.blocksNeedUpdating.Add((int)chunkData.to1D(x - cx * chunkSize, y - cy * chunkSize, z - cz * chunkSize));
                                }
                                /*
                                // if we became a solid block and we are higher than the previous highest block y, set us to the highest one
                                if (block.block != BlockValue.Air && block.block != BlockValue.Wildcard && y > curHighestBlockY)
                                {
                                    // tell the previously highest one that it is no longer the highest one
                                    if (wasAPreviousHighest)
                                    {
                                        world.worldGeneration.blockGetter = world;
                                        world.SetState(x, curHighestBlockY, z, world.GetState(x, curHighestBlockY, z, 2) & (~TOUCHING_SKY_MASK), 2);
                                        world.worldGeneration.blockGetter = myStructure;
                                        curHighestBlockY = y;
                                    }

                                    // set us to be touching the sky now
                                    block.state2 = (block.state2 | TOUCHING_SKY_MASK);
                                }
                                */
                            }
                        }
                    }
                }

                // sort events by priority

                chunkBiomeData.chunkPropertiesObj.chunkPropertyEvents.Sort((x, y) =>
                {
                    return x.priority.CompareTo(y.priority);
                });

                // give each event a seperate structure with the correct priority so the filling in priority algorithms elsewhere will overwrite properly
                foreach (ChunkPropertyEvent chunkPropertyEvent in chunkBiomeData.chunkPropertiesObj.chunkPropertyEvents)
                {
                    Structure myStructure2 = new Structure(cx + " " + cy + " " + cz, true, this, priority: chunkPropertyEvent.priority);
                    world.worldGeneration.blockGetter = myStructure2;
                    chunkPropertyEvent.Run(cx, cy, cz);
                    if (!myStructure2.HasAllChunksGenerated())
                    {
                        world.AddUnfinishedStructure(myStructure2);
                    }

                }
            }
            catch (System.Exception e)
            {
                world.worldGeneration.blockGetter = world;
                chunkData.blocksNeedUpdating.Clear();
                chunkData.blocksNeedUpdatingNextFrame.Clear();
                generating = false;
                Debug.LogError("error in generating chunk " + cx + " " + cy + " " + cz + " " + e);
            }
            //long end = PhysicsUtils.millis();
            //float secondsTaken = (end - start) / 1000.0f;
            //Debug.Log("done generating chunk " + cx + " " + cy + " " + cz + " in " + secondsTaken + " seconds");
            if (!myStructure.HasAllChunksGenerated())
            {
                Debug.Log("adding unfinished " + cx + " " + cy + " " + cz);
                world.AddUnfinishedStructure(myStructure);
            }
            else
            {
                //Debug.Log("done with thing " + cx + " " + cy + " " + cz);
            }
            world.worldGeneration.blockGetter = world;
            chunkData.blocksNeedUpdating.Clear();
            chunkData.blocksNeedUpdatingNextFrame.Clear();
            generating = false;

            this.touchingSkyChunk = world.blocksTouchingSky.GetOrCreateSkyChunk(cx, cz);
            //world.blocksTouchingSky.GeneratedChunk(this);

            /*

            generating = true;
            long baseX = cx * chunkSize;
            long baseY = cy * chunkSize;
            long baseZ = cz * chunkSize;
            Structure myStructure = new Structure(cx + " " + cy + " " + cz, true);
            world.worldGeneration.blockGetter = myStructure;
            for (long x = baseX; x < baseX + this.chunkSize; x++)
            {
                for (long z = baseZ; z < baseZ + this.chunkSize; z++)
                {
                    long elevation = (long)Mathf.Round(world.AverageChunkValues(x, 0, z, c => c.altitude));
                    for (long y = baseY; y < baseY + this.chunkSize; y++)
                    {
                        if (y <= 0)
                        {
                            this[x, y, z] = World.BEDROCK;
                        }
                        else if(y >= elevation)
                        {
                            // this[x, y, z] = World.AIR;
                        }
                        else
                        {

                            long distFromSurface = elevation - y;
                            if (distFromSurface == 1)
                            {
                                this[x, y, z] = World.GRASS;
                                // low pr of making tree
                                if (Simplex.Noise.rand(x,y,z) < 0.01f)
                                {
                                    Structure tree = new Structure("tree", true);
                                    int treeHeight = Mathf.RoundToInt(Simplex.Noise.rand(x, y + 2, z) * 20 + 4);
                                    for (int i = 0; i < treeHeight; i++)
                                    {
                                        if (i == 0)
                                        {
                                            tree[x, y + i, z] = World.TRUNK;
                                            continue;
                                        }
                                        float pAlong = (i+1) / (float)(treeHeight);

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
                                            p = 1-(pAlong - decreasePoint) / (1.0f - decreasePoint);
                                        }

                                        float width = minVal * p + maxVal * (1 - p);
                                        int widthI = Mathf.FloorToInt(width);



                                        for (int j = -widthI; j <= widthI; j++)
                                        {
                                            for (int k = -widthI; k <= widthI; k++)
                                            {
                                                if (Mathf.Sqrt(j*j + k*k) <= width)
                                                {
                                                    tree[x + j, y + i, z + k] = World.LEAF;
                                                }
                                            }
                                        }
                                    }
                                    / *
                                    PhysicsUtils.SearchOutwards(new LVector3(x, y + treeHeight, z), 20+ treeHeight, true, true, (b, bx, by, bz) => b == World.AIR || b == World.WILDCARD, (b, bx, by, bz) =>
                                    {
                                        tree[bx, by, bz] = World.LEAF;
                                        return false;
                                    }, getBlock: (bx, by, bz) => tree[bx, by, bz]);
                                    * /


                                    if (!tree.HasAllChunksGenerated())
                                    {
                                        World.mainWorld.AddUnfinishedStructure(tree);
                                    }
                                }

                            }
                            else if (distFromSurface <= 4)
                            {
                                this[x, y, z] = World.DIRT;
                            }
                            else
                            {
                               this[x, y, z] = World.STONE;
                            }
                        }

                        / *
                        if (y > 3 && y < 5)
                        {
                            this[x, y, z] = World.BEDROCK;
                        }
                        else if (y <= 3 && y >= 10)
                        {
                            this[x, y, z] = World.STONE;
                        }
                        else if (y < 10)
                        {
                            this[x, y, z] = World.BEDROCK;
                        }
                        * /
                        if (World.maxCapacities.ContainsKey(this[x,y,z]))
                        {
                            SetState(x, y, z, World.maxCapacities[this[x, y, z]], 2);
                        }
                        else
                        {
                            SetState(x, y, z, 8, 2);
                        }
                    }
                }
            }
            world.worldGeneration.blockGetter = world;
            chunkData.blocksNeedUpdating.Clear();
            chunkData.blocksNeedUpdatingNextFrame.Clear();
            generating = false;
            */
        }

        System.Random randomGen = new System.Random();

        public void TickStart(long chunkId)
        {
            valid = true;
            if (valid && this.chunkData.TickStart(chunkId)) // this.chunkData.TickStart(chunkId) only returns true if it is the first TickStart called this frame. That ensures that we don't swap around valid multiple times during a single tick start
            {
                int maxNews = 10;
                Chunk newValidOne = chunkData.attachedChunks[randomGen.Next(0, Mathf.Min(maxNews, chunkData.attachedChunks.Count))];

                // this ordering of these two statements is important because newValidOne could be us
                this.valid = false;
                newValidOne.valid = true;

            }
        }



        public const int TOUCHING_TRANPARENT_OR_AIR_BIT = 1 << 9;
        public const int TOUCHING_SKY_BIT = 1 << 8;
        public const int SKY_LIGHTING_MASK = 0xF0;
        public const int BLOCK_LIGHTING_MASK = 0xF;


        public int PackLightingValues(int skyLighting, int blockLighting, bool touchingSky, bool touchingTransparentOrAir)
        {
            int res = 0;
            if (touchingSky)
            {
                res = res | TOUCHING_SKY_BIT;
                skyLighting = 15;
            }
            if (touchingTransparentOrAir)
            {
                res = res | TOUCHING_TRANPARENT_OR_AIR_BIT;
            }
            res = res | (skyLighting << 4) | blockLighting;
            return res;
        }

        public void GetLightingValues(int lightingState, out int skyLighting, out int blockLighting, out bool touchingSky, out bool touchingTransparentOrAir)
        {
            if ((lightingState & TOUCHING_SKY_BIT) != 0)
            {
                touchingSky = true;
            }
            else
            {
                touchingSky = false;
            }

            touchingTransparentOrAir = (TOUCHING_TRANPARENT_OR_AIR_BIT & lightingState) != 0;

            if (touchingSky)
            {
                skyLighting = 15;
            }
            else
            {
                // sky lighting is bits 4-7
                skyLighting = (lightingState & SKY_LIGHTING_MASK) >> 4;
            }

            // block lighting is bits 0-3
            blockLighting = (lightingState & BLOCK_LIGHTING_MASK);
        }


        public void GetHighestLightings(long x, long y, long z, ref int curHighestSkyLight, ref int curHighestBlockLight, ref bool touchingTransparentOrAir)
        {
            int lightingState = chunkData.GetState(x, y, z, BlockState.Lighting);
            if (!IsSolid(chunkData.GetBlock(x,y,z)))
            {
                touchingTransparentOrAir = true;
            }
            int neighborSkyLighting;
            int neighborBlockLighting;
            bool neighborTouchingSky;
            bool neighborTouchingTransparentOrAir;
            GetLightingValues(lightingState, out neighborSkyLighting, out neighborBlockLighting, out neighborTouchingSky, out neighborTouchingTransparentOrAir);
            curHighestSkyLight = System.Math.Max(neighborSkyLighting, curHighestSkyLight);
            curHighestBlockLight = System.Math.Max(neighborBlockLighting, curHighestBlockLight);
        }


        Chunk negX, posX, negY, posY, negZ, posZ;

        public bool IsSolid(int block)
        {
            return block > 0;
        }

        public void GetHighestLightingsOutsideChunk(long wx, long wy, long wz, ref int curHighestSkyLight, ref int curHighestBlockLight,ref Chunk chunk, ref bool touchingTransparentOrAir)
        {
            if (chunk == null)
            {
                chunk = world.GetChunkAtPos(wx, wy, wz);
            }
            if (chunk != null && !chunk.generating)
            {
                int lightingState = chunk.GetState(wx, wy, wz, BlockState.Lighting);
                if(!IsSolid(chunk[wx, wy, wz]))
                {
                    touchingTransparentOrAir = true;
                }
                int neighborSkyLighting;
                int neighborBlockLighting;
                bool neighborTouchingSky;
                bool neighborTouchingTransparentOrAir;
                GetLightingValues(lightingState, out neighborSkyLighting, out neighborBlockLighting, out neighborTouchingSky, out neighborTouchingTransparentOrAir);
                curHighestSkyLight = System.Math.Max(neighborSkyLighting, curHighestSkyLight);
                curHighestBlockLight = System.Math.Max(neighborBlockLighting, curHighestBlockLight);
            }
        }

        public void AddBlockUpdateOutsideChunk(long wx, long wy, long wz, ref Chunk chunk)
        {
            if (chunk == null)
            {
                chunk = world.GetChunkAtPos(wx, wy, wz);
            }
            if (chunk != null && !chunk.generating)
            {
                chunk.AddBlockUpdate(wx, wy, wz);
            }
        }

        public bool Tick(bool allowGenerate, ref int maxMillisInFrame)
        {
            if (cleanedUp)
            {
                Debug.LogWarning("Chunk " + cx + " " + cy + " " + cz + " is already cleaned up, and yet is having Tick() ran on it, did you forget to remove the reference somewhere?");
            }
            bool didGenerate = false;
            if (generating)
            {
                if (allowGenerate)
                {

                    Generate();
                    this.chunkRenderer.Tick();
                    return true;
                }
                else
                {
                    return false;
                }
                didGenerate = true;

                generating = false;
            }

            this.chunkRenderer.Tick();


            if (!valid)
            {
                return false;
            }

            int numLightUpdated = 0;
            if (chunkData.blocksNeedUpdating.Count != 0)
            {
                //Debug.Log("chunk " + cx + " " + cy + " " + cz + " has " + chunkData.blocksNeedUpdating.Count + " block updates");
                
                //bool relevantChunk = true;
                //int k = 0;
                for (int j = 0; j < chunkData.blocksNeedUpdating.Count; j++)
                {
                    int i = chunkData.blocksNeedUpdating[j];

                    //if (PhysicsUtils.millis() - world.frameTimeStart > maxMillisInFrame)
                   // {
                   //     chunkData.blocksNeedUpdatingNextFrame.Add(i);
                   //     continue;
                    //}
                    long ind = (long)(i);
                    long x, y, z;
                    chunkData.to3D(ind, out x, out y, out z);
                    long wx = x + cx * chunkSize;
                    long wy = y + cy * chunkSize;
                    long wz = z + cz * chunkSize;
                    //long mwx, mwy, mwz;
                    //PhysicsUtils.ModPos(wx, wy, wz, out mwx, out mwy, out mwz);
                    using (BlockData block = world.GetBlockData(wx, wy, wz))
                    {
                        block.myChunk = this;
                        BlockValue blockValue = block.block;
                        int skyLighting;
                        int blockLighting;
                        bool touchingSky;
                        bool oldTouchingTransparentOrAir;
                        GetLightingValues(block.lightingState, out skyLighting, out blockLighting, out touchingSky, out oldTouchingTransparentOrAir);
                        int highestSkyLighting = 0;
                        int highestBlockLighting = 0;
                        // check neighbors to see if we need to trickle their light values
                        bool touchingTransparentOrAir = false;

                        // this code needed to be a little gross because it needs to be very fast so ideally we want to not use the world lookup unless we have to since usually we'll be inside this chunk
                        if (x == 0) GetHighestLightingsOutsideChunk(wx - 1, wy, wz, ref highestSkyLighting, ref highestBlockLighting, ref negX, ref touchingTransparentOrAir);
                        else                    GetHighestLightings(x - 1, y, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);
                        if (y == 0) GetHighestLightingsOutsideChunk(wx, wy - 1, wz, ref highestSkyLighting, ref highestBlockLighting, ref negY, ref touchingTransparentOrAir);
                        else                    GetHighestLightings(x, y - 1, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);
                        if (z == 0) GetHighestLightingsOutsideChunk(wx, wy, wz - 1, ref highestSkyLighting, ref highestBlockLighting, ref negZ, ref touchingTransparentOrAir);
                        else                    GetHighestLightings(x, y, z - 1, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);

                        if (x == chunkSize-1) GetHighestLightingsOutsideChunk(wx + 1, wy, wz, ref highestSkyLighting, ref highestBlockLighting, ref posX, ref touchingTransparentOrAir);
                        else                              GetHighestLightings(x + 1, y, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);
                        if (y == chunkSize-1) GetHighestLightingsOutsideChunk(wx, wy + 1, wz, ref highestSkyLighting, ref highestBlockLighting, ref posY, ref touchingTransparentOrAir);
                        else                              GetHighestLightings(x, y + 1, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);
                        if (z == chunkSize-1) GetHighestLightingsOutsideChunk(wx, wy, wz + 1, ref highestSkyLighting, ref highestBlockLighting, ref posZ, ref touchingTransparentOrAir);
                        else                              GetHighestLightings(x, y, z + 1, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir);

                        bool lightModified = false;

                        // neighbors light has changed so we need to trickle their values
                        if (skyLighting < highestSkyLighting - 1 || blockLighting < highestBlockLighting - 1 || (touchingTransparentOrAir != oldTouchingTransparentOrAir))
                        {
                            skyLighting = System.Math.Max(skyLighting, highestSkyLighting - 1);
                            blockLighting = System.Math.Max(blockLighting, highestBlockLighting - 1);
                            lightModified = true;
                        }

                        if (!touchingSky)
                        {
                            // we produce the light
                            if (highestSkyLighting <= skyLighting && skyLighting > 0)
                            {
                                skyLighting = System.Math.Max(0, highestSkyLighting - 1);
                                lightModified = true;
                            }
                        }
                        
                        // we produce the light
                        if (highestBlockLighting <= blockLighting && blockLighting > 0)
                        {
                            if (block.block != Example.Sand)
                            {
                                blockLighting = System.Math.Max(highestBlockLighting - 1, 0);
                                lightModified = true;
                            }
                        }


                        if (chunkData.blocksNeedUpdating.Count == 7)
                        {
                            // Debug.Log("block update " + cx + " " + cy + " " + cz + "      " + wx + " " + wy + " " + wz + "       " + x + " " + y + " " + z + " " + touchingTransparentOrAir + " " + oldTouchingTransparentOrAir + " " + World.BlockToString(blockValue));
                        }
                        if (lightModified)
                        {
                            block.lightingState = PackLightingValues(skyLighting, blockLighting, touchingSky, touchingTransparentOrAir);
                            numLightUpdated += 1;
                            chunkData.needToBeUpdated = true;
                        }

                        BlockOrItem customBlock;
                        if (world.customBlocks.ContainsKey(blockValue, out customBlock))
                        {
                            customBlock.OnTick(block);
                        }

                        if (block.needsAnotherTick)
                        {
                            chunkData.blocksNeedUpdatingNextFrame.Add((int)ind);
                        }

                        if (block.WasModified)
                        {
                            chunkData.needToBeUpdated = true;
                            // don't call lots of chunk lookups if we don't need to
                            if (x == 0) AddBlockUpdateOutsideChunk(wx - 1, wy, wz, ref negX);
                            else chunkData.AddBlockUpdate(x - 1, y, z);
                            if (y == 0) AddBlockUpdateOutsideChunk(wx, wy - 1, wz, ref negY);
                            else chunkData.AddBlockUpdate(x, y - 1, z);
                            if (z == 0) AddBlockUpdateOutsideChunk(wx, wy, wz - 1, ref negZ);
                            else chunkData.AddBlockUpdate(x, y, z - 1);

                            if (x == chunkSize - 1) AddBlockUpdateOutsideChunk(wx + 1, wy, wz, ref posX);
                            else chunkData.AddBlockUpdate(x + 1, y, z);
                            if (y == chunkSize - 1) AddBlockUpdateOutsideChunk(wx, wy + 1, wz, ref posY);
                            else chunkData.AddBlockUpdate(x, y + 1, z);
                            if (z == chunkSize - 1) AddBlockUpdateOutsideChunk(wx, wy, wz + 1, ref posZ);
                            else chunkData.AddBlockUpdate(x, y, z + 1);

                            if (block.block == BlockValue.Air)
                            {
                                block.state = 0;
                                block.animationState = 0;
                            }
                        }
                    }
                    /*


                    int resState1;
                    int resState2;
                    int resState3;
                    bool needsAnotherUpdate;
                    int resBlock = world.UpdateBlock(wx, wy, wz, chunkData[x,y,z], chunkData.GetState(x,y,z, 1), chunkData.GetState(x, y, z, 2), chunkData.GetState(x, y, z, 3), out resState1, out resState2, out resState3, out needsAnotherUpdate);
                    bool chunkDataAddedBlockUpdate;
                    bool dontCareActually;
                    chunkData.SetState(x, y, z, resState1, 1, out dontCareActually, forceBlockUpdate: needsAnotherUpdate);
                    chunkData.SetState(x, y, z, resState2, 2, out dontCareActually, forceBlockUpdate: needsAnotherUpdate);
                    chunkData.SetState(x, y, z, resState3, 3, out dontCareActually, forceBlockUpdate: needsAnotherUpdate);
                    chunkData.SetBlock(x, y, z, resBlock, out chunkDataAddedBlockUpdate, forceBlockUpdate: needsAnotherUpdate);


                    if (chunkDataAddedBlockUpdate)
                    {
                        bool neighborsInsideThisChunk =
                            (x != 0 && x != chunkSize - 1) &&
                            (y != 0 && y != chunkSize - 1) &&
                            (z != 0 && z != chunkSize - 1);

                        if (!neighborsInsideThisChunk)
                        {
                            world.AddBlockUpdateToNeighbors(wx, wy, wz);
                        }
                        else
                        {
                            chunkData.AddBlockUpdate(x - 1, y, z);
                            chunkData.AddBlockUpdate(x + 1, y, z);
                            chunkData.AddBlockUpdate(x, y - 1, z);
                            chunkData.AddBlockUpdate(x, y + 1, z);
                            chunkData.AddBlockUpdate(x, y, z-1);
                            chunkData.AddBlockUpdate(x, y, z+1);
                        }
                    }
                    */
                }


                //Debug.Log("updated " + chunkData.blocksNeedUpdating.Count + " blocks with " + numLightUpdated + " lighting updates " + cx + " " + cy + " " + cz);
            }


            //chunkData.blocksNeedUpdating.Clear();

            return didGenerate;
        }


        public bool generating = true;


        public void SetState(long x, long y, long z, int state, BlockState stateType)
        {
            long relativeX = x - cx * chunkSize;
            long relativeY = y - cy * chunkSize;
            long relativeZ = z - cz * chunkSize;
            bool addedUpdate;
            chunkData.SetState(relativeX, relativeY, relativeZ, state, stateType, out addedUpdate);
            // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
            if (!generating && addedUpdate)
            {
                world.AddBlockUpdateToNeighbors(x, y, z);
                chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
            }
        }
        public int GetState(long x, long y, long z, BlockState stateType)
        {
            long relativeX = x - cx * chunkSize;
            long relativeY = y - cy * chunkSize;
            long relativeZ = z - cz * chunkSize;
            return chunkData.GetState(relativeX, relativeY, relativeZ, stateType);
        }

        public int this[long x, long y, long z]
        {
            get
            {
                long relativeX = x - cx * chunkSize;
                long relativeY = y - cy * chunkSize;
                long relativeZ = z - cz * chunkSize;
                return chunkData[relativeX, relativeY, relativeZ];
            }
            set
            {
                long relativeX = x - cx * chunkSize;
                long relativeY = y - cy * chunkSize;
                long relativeZ = z - cz * chunkSize;
                bool addedUpdate;


                // if we are already generated, do lighting updates on modification
                if (!generating)
                {
                    int prev = chunkData[relativeX, relativeY, relativeZ];
                    // turning solid into non-solid (less than 0 is transparent, greater than 0 is solid, 0 is empty)
                    if (prev > 0 && value <= 0)
                    {
                        int prevLighting = chunkData.GetState(relativeX, relativeY, relativeZ, BlockState.Lighting);
                        // is touching sky?
                        if ((prevLighting & Chunk.TOUCHING_SKY_BIT) != 0)
                        {
                            // notify lighting so it can find a new block below us that is now touching sky 
                            world.blocksTouchingSky.RemovedSolidBlockTouchingSky(x, y, z);
                        }
                    }
                    // turning non-solid into solid, check lighting
                    if (prev <= 0 && value > 0)
                    {
                        long highestY = touchingSkyChunk.highestBlocks[relativeX, relativeZ];
                        if (y > highestY)
                        {
                            world.blocksTouchingSky.AddedHigherBlockTouchingSky(x, z, highestY, y);
                            touchingSkyChunk.highestBlocks[relativeX, relativeZ] = y;
                        }
                    }
                }
                chunkData.SetBlock(relativeX, relativeY, relativeZ, value, out addedUpdate);
                // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
                if (!generating && addedUpdate)
                {
                    world.AddBlockUpdateToNeighbors(x, y, z);
                    chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
                }
            }
        }


        bool cleanedUp = false;
        public void Dispose()
        {
            if (!cleanedUp)
            {
                cleanedUp = true;
                chunkRenderer.Dispose();
                chunkData.Dispose();
            }
        }
    }

    public class ChunkData
    {
        public bool needToBeUpdated = false;
        public IntegerSet blocksNeedUpdating;
        public IntegerSet blocksNeedUpdatingNextFrame;
        int[] data;
        public int chunkSize;

        int chunkSize_2, chunkSize_3;

        public List<Chunk> attachedChunks = new List<Chunk>();

        public ChunkData(int chunkSize, bool fillWithWildcard = false)
        {
            this.chunkSize = chunkSize;
            this.chunkSize_2 = chunkSize * chunkSize;
            this.chunkSize_3 = chunkSize * chunkSize * chunkSize;
            data = new int[chunkSize * chunkSize * chunkSize * 4];
            this.blocksNeedUpdating = new IntegerSet(chunkSize * chunkSize * chunkSize);
            this.blocksNeedUpdatingNextFrame = new IntegerSet(chunkSize * chunkSize * chunkSize);
            if (fillWithWildcard)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (int)BlockValue.Wildcard;
                }
            }
        }

        public ChunkData(int chunkSize, int[] data)
        {
            this.chunkSize = chunkSize;
            this.chunkSize_2 = chunkSize * chunkSize;
            this.chunkSize_3 = chunkSize * chunkSize * chunkSize;
            this.data = data;
        }

        public void WriteToFile(string path)
        {
            // convert ints to bytes
            byte[] byteData = new byte[sizeof(int) * data.Length];
            System.Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length); // size is in number of bytes

            // write to file
            File.WriteAllBytes(path, byteData);

        }

        public void ReadFromFile(string path)
        {
            // read from file
            byte[] byteData = File.ReadAllBytes(path);
            data = new int[byteData.Length / sizeof(int)];
            System.Buffer.BlockCopy(byteData, 0, data, 0, data.Length * sizeof(int)); // size is in number of bytes
            this.needToBeUpdated = true;
        }

        public void CopyIntoChunk(Chunk chunk, int priority=0)
        {
            int[] chunkData = chunk.chunkData.data;
            int totalLen = System.Math.Min(data.Length, chunkData.Length);
            for (int i = 0; i < totalLen; i++)
            {
                // if we are on a block, check if wildcard
                if (i % 4 == 0)
                {
                    bool skipAhead = false;
                    // if base generation and something else has already filled this in, skip ahead
                    if (priority == 0 && chunkData[i] != BlockValue.Wildcard)
                    {
                        skipAhead = true;
                    }
                    // if we are not a whildcard, assign us and also assign the chunk internal states 
                    else if (data[i] != (int)BlockValue.Wildcard)
                    {
                        chunkData[i] = data[i];
                    }
                    // otherwise this is wildcard, skip to next block (only 3 instead of 4 because i++ is default in loop)
                    else
                    {
                        skipAhead = true;
                    }
                    if (skipAhead)
                    {
                        i += 3;
                    }
                }
                else if (i % 4 == 1)
                {
                    chunkData[i] = data[i];
                }
            }
        }

        long curFrame = -1;

        public bool TickStart(long frameId)
        {
            if (curFrame != frameId)
            {
                curFrame = frameId;
                IntegerSet tmp = blocksNeedUpdating;
                blocksNeedUpdating = blocksNeedUpdatingNextFrame;
                blocksNeedUpdatingNextFrame = tmp;
                blocksNeedUpdatingNextFrame.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        public int[] GetRawData()
        {
            return data;
        }

        // from https://stackoverflow.com/a/34363187/2924421
        public void to3D(int ind, out int x, out int y, out int z)
        {
            z = ind / (chunkSize_2);
            ind -= (z * chunkSize_2);
            y = ind / chunkSize;
            x = ind % chunkSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int to1D(int x, int y, int z)
        {
            return x + y * BlocksWorld.chunkSize + z * BlocksWorld.chunkSize * BlocksWorld.chunkSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void to3D(long ind, out long x, out long y, out long z)
        {
            z = ind / (chunkSize_2);
            ind -= (z * chunkSize_2);
            y = ind / chunkSize;
            x = ind % chunkSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long to1D(long x, long y, long z)
        {
            return x + y * BlocksWorld.chunkSize + z * BlocksWorld.chunkSize * BlocksWorld.chunkSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetState(long i, long j, long k, BlockState stateType)
        {
            long ind = to1D(i, j, k);
            return data[ind * 4 + (int)stateType];
        }

        public void SetState(long i, long j, long k, int state, BlockState stateType, out bool addedBlockUpdate, bool forceBlockUpdate = false)
        {
            addedBlockUpdate = false;
            long ind = to1D(i, j, k);
            if (data[ind * 4 + (int)stateType] != state)
            {
                //needToBeUpdated = true;
            }
            //if (forceBlockUpdate || data[ind*4+(int)stateType] != state)
            //{
            //    addedBlockUpdate = true;
            //}
            //if (forceBlockUpdate || (data[ind * 4 + (int)stateType] != state && stateI != 2))
            //{
            //     addedBlockUpdate = true;
            //blocksNeedUpdatingNextFrame.Add((int)ind);
            //}
            data[ind * 4 + (int)stateType] = state;
        }


        public void AddBlockUpdate(long i, long j, long k)
        {
            blocksNeedUpdatingNextFrame.Add((int)to1D(i, j, k));
        }

        public int GetBlock(long i, long j, long k)
        {
            long ind = to1D(i, j, k);
            return data[ind * 4];
        }

        public void SetBlock(long i, long j, long k, int block, out bool addedBlockUpdate, bool forceBlockUpdate = false)
        {
            addedBlockUpdate = false;
            long ind = to1D(i, j, k);
            if (data[ind * 4] != block)
            {
                addedBlockUpdate = true;
                needToBeUpdated = true;
            }
            //if (forceBlockUpdate || data[ind * 4] != block)
            //{
            //    addedBlockUpdate = true;
            //    //blocksNeedUpdatingNextFrame.Add((int)ind);
            //}

            data[ind * 4] = block;
        }


        public int this[long i, long j, long k]
        {
            get
            {
                return data[(i + j * chunkSize + k * chunkSize_2) * 4];
            }
            set
            {
                bool addedBlockUpdate;
                SetBlock(i, j, k, value, out addedBlockUpdate);
            }
        }


        public void Dispose()
        {

        }
    }


    public abstract class BlockGetter
    {


        public long blockModifyState = 1;
        protected BlockDataCache blockDataCache;
        public BlockData GetBlockData(long x, long y, long z)
        {
            return blockDataCache.GetNewBlockData(x, y, z);
        }

        public void DoneWithBlockData(BlockData blockData)
        {
            blockDataCache.DoneWithBlockData(blockData);
        }


        public void SetState(long x, long y, long z, int state, BlockState stateType)
        {
            long cx, cy, cz;
            World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            SetState(x, y, z, cx, cy, cz, state, stateType);
        }
        public int GetState(long x, long y, long z, BlockState stateType)
        {
            long cx, cy, cz;
            World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            return GetState(x, y, z, cx, cy, cz, stateType);
        }

        public abstract void SetState(long x, long y, long z, long cx, long cy, long cz, int state, BlockState stateType);
        public abstract int GetState(long x, long y, long z, long cx, long cy, long cz, BlockState stateType);
        public int this[long x, long y, long z]
        {
            get
            {
                long cx, cy, cz;
                World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
                return this[x, y, z, cx, cy, cz];
            }
            set
            {
                long cx, cy, cz;
                World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
                this[x, y, z, cx, cy, cz] = value;
            }
        }
        public abstract int this[long x, long y, long z, long cx, long cy, long cz]
        {
            get; set;
        }
    }

    [System.Serializable]
    public class SavedStructure
    {
        public string name;
        public int priority=0;
        public bool madeInGeneration;
        public SavedChunk[] savedChunks;

        public SavedStructure()
        {

        }

        public SavedStructure(string name, bool madeInGeneration, SavedChunk[] savedChunks, int priority)
        {
            this.name = name;
            this.madeInGeneration = madeInGeneration;
            this.savedChunks = savedChunks;
            this.priority = priority;
        }
    }
    [System.Serializable]
    public class SavedChunk
    {
        public int chunkSize;
        public int[] chunkData;
        public long cx, cy, cz;

        public SavedChunk()
        {

        }

        public SavedChunk(int chunkSize, int[] chunkData, long cx, long cy, long cz)
        {
            this.chunkSize = chunkSize;
            this.chunkData = chunkData;
            this.cx = cx;
            this.cy = cy;
            this.cz = cz;
        }
    }

    public class Structure : BlockGetter
    {
        public string name;
        public bool madeInGeneration;
        Dictionary<LVector3, ChunkData> ungeneratedChunkPositions;


        public int priority;
        public Chunk baseChunk;

        public Structure(string name, bool madeInGeneration, Chunk baseChunk, int priority=0)
        {
            this.priority = priority;
            this.name = name;
            this.baseChunk = baseChunk;
            this.blockDataCache = new BlockDataCache(this);
            ungeneratedChunkPositions = new Dictionary<LVector3, ChunkData>();
            this.madeInGeneration = madeInGeneration;
        }


        public Structure(SavedStructure savedStructure)
        {
            name = savedStructure.name;
            priority = savedStructure.priority;
            this.baseChunk = null;
            this.blockDataCache = new BlockDataCache(this);
            ungeneratedChunkPositions = new Dictionary<LVector3, ChunkData>();
            this.madeInGeneration = savedStructure.madeInGeneration;
            for (int i = 0; i < savedStructure.savedChunks.Length; i++)
            {
                SavedChunk savedChunk = savedStructure.savedChunks[i];
                LVector3 savedChunkPos = new LVector3(savedChunk.cx, savedChunk.cy, savedChunk.cz);
                int[] savedChunkData = savedChunk.chunkData;

                ChunkData resSavedChunkData = new ChunkData(savedChunk.chunkSize, savedChunkData);
                ungeneratedChunkPositions[savedChunkPos] = resSavedChunkData;
            }
        }


        public bool HasAllChunksGenerated()
        {
            return ungeneratedChunkPositions.Count == 0;
        }



        public SavedStructure ToSavedStructure()
        {
            List<SavedChunk> savedChunks = new List<SavedChunk>();
            foreach (KeyValuePair<LVector3, ChunkData> savedChunk in ungeneratedChunkPositions)
            {
                savedChunks.Add(new SavedChunk(savedChunk.Value.chunkSize, savedChunk.Value.GetRawData(), savedChunk.Key.x, savedChunk.Key.y, savedChunk.Key.z));
            }

            return new SavedStructure(name, madeInGeneration, savedChunks.ToArray(), priority);
        }


        public bool CanAddToChunk(Chunk chunk)
        {
            LVector3 chunkPos = new LVector3(chunk.cx, chunk.cy, chunk.cz);
            return ungeneratedChunkPositions.ContainsKey(chunkPos);
        }

        /// <summary>
        /// Returns true if filled in all of its chunks, otherwise false
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public bool AddNewChunk(Chunk chunk)
        {
            LVector3 chunkPos = new LVector3(chunk.cx, chunk.cy, chunk.cz);
            if (ungeneratedChunkPositions.ContainsKey(chunkPos))
            {
                ChunkData chunkData = ungeneratedChunkPositions[chunkPos];
                chunkData.CopyIntoChunk(chunk, priority);
                ungeneratedChunkPositions.Remove(chunkPos);
            }
            return ungeneratedChunkPositions.Count == 0;
        }

        public override void SetState(long x, long y, long z, long cx, long cy, long cz, int state, BlockState stateType)
        {
            blockModifyState += 1;
            if (baseChunk != null && cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
            {
                baseChunk.SetState(x, y, z, state, stateType);
                return;
            }

            if (madeInGeneration)
            {
                Chunk chunk = World.mainWorld.GetChunkAtPos(x, y, z);
                if (chunk == null)
                {
                    LVector3 chunkPos;
                    World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out chunkPos);
                    int chunkSize = World.mainWorld.chunkSize;
                    if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                    {
                        ungeneratedChunkPositions[chunkPos] = new ChunkData(chunkSize, fillWithWildcard: true);
                    }
                    long localPosX = x - chunkPos.x * chunkSize;
                    long localPosY = y - chunkPos.y * chunkSize;
                    long localPosZ = z - chunkPos.z * chunkSize;
                    bool addedBlockUpdate;
                    ungeneratedChunkPositions[chunkPos].SetState(localPosX, localPosY, localPosZ, state, stateType, out addedBlockUpdate);
                }
                else
                {
                    bool wasGenerating = chunk.generating;
                    chunk.generating = true;
                    chunk.SetState(x, y, z, state, stateType);
                    chunk.generating = wasGenerating;
                }
            }
            else
            {
                World.mainWorld.SetState(x, y, z, state, stateType);
            }
        }
        public override int GetState(long x, long y, long z, long cx, long cy, long cz, BlockState stateType)
        {
            if (baseChunk != null && cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
            {
                return baseChunk.GetState(x, y, z, stateType);
            }
            if (madeInGeneration)
            {
                Chunk chunk = World.mainWorld.GetChunkAtPos(x, y, z);
                if (chunk == null)
                {
                    LVector3 chunkPos;
                    World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out chunkPos);

                    int chunkSize = World.mainWorld.chunkSize;
                    if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                    {
                        ungeneratedChunkPositions[chunkPos] = new ChunkData(chunkSize, fillWithWildcard: true);
                    }
                    long localPosX = x - chunkPos.x * chunkSize;
                    long localPosY = y - chunkPos.y * chunkSize;
                    long localPosZ = z - chunkPos.z * chunkSize;
                    return ungeneratedChunkPositions[chunkPos].GetState(localPosX, localPosY, localPosZ, stateType);
                }
                else
                {
                    return chunk.GetState(x, y, z, stateType);
                }
            }
            else
            {
                return World.mainWorld.GetState(x, y, z, stateType);
            }
        }

        public override int this[long x, long y, long z, long cx, long cy, long cz]
        {
            get
            {
                if (baseChunk != null && cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
                {
                    return baseChunk[x, y, z];
                }
                if (madeInGeneration)
                {
                    Chunk chunk = World.mainWorld.GetChunkAtPos(x, y, z);
                    if (chunk == null)
                    {
                        LVector3 chunkPos;
                        World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out chunkPos);

                        int chunkSize = World.mainWorld.chunkSize;
                        if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                        {
                            ungeneratedChunkPositions[chunkPos] = new ChunkData(chunkSize, fillWithWildcard: true);
                        }
                        long localPosX = x - chunkPos.x * chunkSize;
                        long localPosY = y - chunkPos.y * chunkSize;
                        long localPosZ = z - chunkPos.z * chunkSize;
                        return ungeneratedChunkPositions[chunkPos][localPosX, localPosY, localPosZ];
                    }
                    else
                    {
                        return chunk[x, y, z];
                    }
                }
                else
                {
                    return World.mainWorld[x, y, z];
                }
            }
            set
            {
                blockModifyState += 1;
                if (baseChunk != null && cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
                {
                    if (value != BlockValue.Wildcard)
                    {
                        baseChunk[x, y, z] = value;
                    }
                    return;
                }
                if (madeInGeneration)
                {
                    Chunk chunk = World.mainWorld.GetChunkAtPos(x, y, z);
                    if (chunk == null || chunk.generating)
                    {
                        LVector3 chunkPos;
                        World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out chunkPos);
                        int chunkSize = World.mainWorld.chunkSize;
                        if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                        {
                            ungeneratedChunkPositions[chunkPos] = new ChunkData(chunkSize, fillWithWildcard: true);
                        }
                        long localPosX = x - chunkPos.x * chunkSize;
                        long localPosY = y - chunkPos.y * chunkSize;
                        long localPosZ = z - chunkPos.z * chunkSize;
                        ungeneratedChunkPositions[chunkPos][localPosX, localPosY, localPosZ] = value;
                    }
                    else
                    {
                        if (value != BlockValue.Wildcard)
                        {
                            //bool wasGenerating = chunk.generating;
                            //chunk.generating = true;
                            chunk[x, y, z] = value;
                            //chunk.generating = wasGenerating;
                        }
                    }
                }
                else
                {
                    World.mainWorld[x, y, z] = value;
                }

            }
        }
    }

    // Has O(1) access for enqueue, dequeue, push, pop, and individual element access.
    // Does not allow removing any element, only last or first
    // The only time it can be slow is when it has to grow the internal list, it doubles the size each time so this shouldn't happen very often
    public class FastStackQueue<T>
    {
        int count;
        int frontPos;

        T[] list;

        public int Count
        {
            get
            {
                return count;
            }
            private set
            {

            }
        }

        public FastStackQueue(int initialCount)
        {
            frontPos = 0;
            list = new T[initialCount];
        }

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= count)
                {
                    throw new System.ArgumentOutOfRangeException("index = " + i + " but list is of size " + count);
                }
                else
                {
                    return list[(i + frontPos) % count];
                }
            }
            set
            {
                if (i < 0 || i >= count)
                {
                    throw new System.ArgumentOutOfRangeException("index = " + i + " but list is of size " + count);
                }
                else
                {
                    list[(i + frontPos) % count] = value;
                }
            }
        }

        public void Enqueue(T value)
        {
            if (count + 1 > list.Length)
            {
                T[] newList = new T[list.Length * 2];
                int k = 0;
                for (int i = 0; i < count; i++)
                {
                    int j = (i + frontPos) % list.Length;
                    newList[k] = list[j];
                }
                list = newList;
                frontPos = 0;
            }
            int ind = (frontPos + count) % list.Length;
            list[ind] = value;
            count += 1;
        }

        public T Dequeue()
        {
            if (count == 0)
            {
                throw new System.ArgumentOutOfRangeException("dequeing an empty FastStackQueue, this is invalid");
            }
            else
            {
                T res = list[frontPos];
                frontPos = (frontPos + 1) % list.Length;
                count -= 1;
                return res;
            }
        }

        public void Push(T value)
        {
            Enqueue(value);
        }

        public T Pop()
        {
            if (count == 0)
            {
                throw new System.ArgumentOutOfRangeException("dequeing an empty FastStackQueue, this is invalid");
            }
            else
            {
                T res = list[frontPos + count - 1];
                count -= 1;
                return res;
            }
        }
    }

    public class BlockDataCache
    {
        FastStackQueue<BlockData> blockDatasNotInUse;

        BlockGetter world;

        public BlockDataCache(BlockGetter world)
        {
            this.world = world;
            blockDatasNotInUse = new FastStackQueue<BlockData>(100);
        }

        public BlockData GetNewBlockData(long x, long y, long z)
        {
            if (blockDatasNotInUse.Count == 0)
            {
                blockDatasNotInUse.Enqueue(new BlockData(world, x, y, z));
                blockDatasNotInUse.Enqueue(new BlockData(world, x, y, z));
                blockDatasNotInUse.Enqueue(new BlockData(world, x, y, z));
                return new BlockData(world, x, y, z);
            }
            else
            {
                BlockData res = blockDatasNotInUse.Dequeue();
                res.ReassignValues(x, y, z);
                return res;
            }
        }

        public void DoneWithBlockData(BlockData blockData)
        {
            blockDatasNotInUse.Enqueue(blockData);
        }
    }

    public abstract class BlocksPack : MonoBehaviour
    {
        /// <summary>
        ///  todo: increase size if we have more than 10000 custom blocks
        /// </summary>
        public FastSimpleBlockLookup customBlocks = new FastSimpleBlockLookup(2000);
        public GenerationClass customGeneration;
        public List<Recipe> customRecipes = new List<Recipe>();

        public void AddCustomRecipe(Recipe recipe)
        {
            customRecipes.Add(recipe);
        }

        public void AddCustomBlock(BlockValue block, BlockOrItem customBlock, int stackSize = 1)
        {
            customBlocks[block] = customBlock;
            customBlock.stackSize = stackSize;
        }

        public void SetCustomGeneration(GenerationClass customGeneration)
        {
            this.customGeneration = customGeneration;
        }


        public BlockEntity CreateBlockEntity(BlockValue block, Vector3 position)
        {
            GameObject blockEntity = GameObject.Instantiate(World.mainWorld.blocksWorld.blockEntityPrefab);
            blockEntity.transform.position = position;
            blockEntity.GetComponent<BlockEntity>().blockId = (int)block;
            return blockEntity.GetComponent<BlockEntity>();
        }
    }

    public enum BlockState
    {
        State = 1,
        Lighting = 2,
        Animation = 3
    }

    public class World : BlockGetter
    {
        public static bool creativeMode = false;
        public static World mainWorld;
        public const int maxAnimFrames = 64;
        public const int numBlocks = 64;
        public const int numBreakingFrames = 10;




        public BlocksWorld blocksWorld;

        ChunkProperties chunkProperties;

        [System.Serializable]
        public class BlockInventory
        {
            public long x, y, z;
            public int[] blocks;
            public int[] counts;
            public int[] durabilities;
            public int[] maxDurabilities;

            public BlockInventory()
            {

            }

            public BlockInventory(LVector3 block, Inventory inventory)
            {
                x = block.x;
                y = block.y;
                z = block.z;

                blocks = new int[inventory.blocks.Length];
                counts = new int[inventory.blocks.Length];
                durabilities = new int[inventory.blocks.Length];
                maxDurabilities = new int[inventory.blocks.Length];
                for (int i = 0; i < blocks.Length; i++)
                {
                    if (inventory.blocks[i] != null)
                    {
                        blocks[i] = inventory.blocks[i].block;
                        counts[i] = inventory.blocks[i].count;
                        durabilities[i] = inventory.blocks[i].durability;
                        maxDurabilities[i] = inventory.blocks[i].maxDurability;
                    }
                }
            }
        }

        BlockInventoryCollection GetBlockInventories()
        {
            BlockInventory[] res = new BlockInventory[blocksWorld.blockInventories.Count];
            int i = 0;
            foreach (KeyValuePair<LVector3, Inventory> blockInventory in blocksWorld.blockInventories)
            {
                res[i] = new BlockInventory(blockInventory.Key, blockInventory.Value);
                i += 1;
            }
            return new BlockInventoryCollection(res);
        }

        [System.Serializable]
        public class BlockInventoryCollection
        {
            public BlockInventory[] inventories;

            public BlockInventoryCollection()
            {

            }

            public BlockInventoryCollection(BlockInventory[] inventories)
            {
                this.inventories = inventories;
            }
        }

        public class SavedStructureCollection
        {
            public SavedStructure[] savedStructures;

            public SavedStructureCollection()
            {

            }

            public SavedStructureCollection(SavedStructure[] savedStructures)
            {
                this.savedStructures = savedStructures;
            }

        }
        public void Save(string rootDir)
        {
            DirectoryInfo rootInfo = new DirectoryInfo(rootDir);
            if (!rootInfo.Exists)
            {
                Directory.CreateDirectory(rootInfo.FullName);
            }
            string cleanedRootDir = rootInfo.FullName.Replace("\\", "/");
            string chunksDir = cleanedRootDir + "/chunks";
            DirectoryInfo chunksDirInfo = new DirectoryInfo(chunksDir);
            if (!chunksDirInfo.Exists)
            {
                Directory.CreateDirectory(chunksDirInfo.FullName);
            }
            foreach (Chunk chunk in allChunks)
            {
                string chunkName = chunk.cx + "." + chunk.cy + "." + chunk.cz + ".dat";
                chunk.chunkData.WriteToFile(chunksDir + "/" + chunkName);
            }
            string configPath = cleanedRootDir + "/blocksConfig.json";
            File.WriteAllText(configPath, BlockValue.SaveIdConfigToJsonString());

            string inventoriesPath = cleanedRootDir + "/blockInventories.json";
            File.WriteAllText(inventoriesPath, Newtonsoft.Json.JsonConvert.SerializeObject(GetBlockInventories()));


            string structurePath = cleanedRootDir + "/generatingStructures.json";

            List<SavedStructure> savedStructures = new List<SavedStructure>();
            foreach (Structure structure in unfinishedStructures)
            {
                savedStructures.Add(structure.ToSavedStructure());
            }

            SavedStructureCollection savedStructuresCollection = new SavedStructureCollection(savedStructures.ToArray());

            File.WriteAllText(structurePath, Newtonsoft.Json.JsonConvert.SerializeObject(savedStructuresCollection));


        }

        public void Load(string rootDir)
        {
            DirectoryInfo rootInfo = new DirectoryInfo(rootDir);
            if (!rootInfo.Exists)
            {
                throw new System.ArgumentException("in loading world, root dir " + rootDir + " does not exist");
            }
            string cleanedRootDir = rootInfo.FullName.Replace("\\", "/");


            // it is important we do this before reloading inventories 
            string configPath = cleanedRootDir + "/blocksConfig.json";
            if (!File.Exists(configPath))
            {
                throw new System.ArgumentException("in loading world, config json " + configPath + " does not exist");
            }
            string configJson = File.ReadAllText(configPath);
            Debug.Log("loading config json from file " + configJson);
            BlockValue.LoadIdConfigFromJsonString(configJson);
            Debug.Log("done loading config json from file " + configJson);







            blocksWorld.triMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.triMaterialWithTransparency.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.blockEntityMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.blockIconMaterial.mainTexture = BlockValue.allBlocksTexture;
            Debug.Log("loading chunks");
            string chunksDir = cleanedRootDir + "/chunks";
            DirectoryInfo chunksDirInfo = new DirectoryInfo(chunksDir);

            // clear structures
            unfinishedStructures.Clear();

            FastSimpleBlockLookup newCustomBlocks = new FastSimpleBlockLookup(100000);
            foreach (KeyValuePair<BlockValue, BlockOrItem> block in customBlocks)
            {
                newCustomBlocks[block.Key.id] = block.Value;
                Debug.Log("old custom block " + block.Key.id + " is id for block " + block.Key.name);
            }
            customBlocks.Clear();
            customBlocks = newCustomBlocks;
            stackableSize.Clear();
            foreach (KeyValuePair<BlockValue, BlockOrItem> block in customBlocks)
            {
                Debug.Log("new custom block " + block.Key.id + " is id for block " + block.Key.name);
                stackableSize[block.Key] = block.Value.stackSize;
            }

            Debug.Log(customBlocks[Example.Water] + " is the thing for water");
            Debug.Log(customBlocks[Example.WaterNoFlow] + " is the thing for water no flow");

            // delete current chunks
            foreach (KeyValuePair<long, List<Chunk>> chunkList in chunksPer[0])
            {
                foreach (Chunk chunk in chunkList.Value)
                {
                    chunk.Dispose();
                }
            }
            for (int i = 0; i < chunksPer.Length; i++)
            {
                chunksPer[i].Clear();
            }

            allChunks.Clear();
            chunkCache.Clear();
            ungeneratedChunkBiomeDatas.Clear();
            lastChunk = null;
            string generatingStructuresPath = cleanedRootDir + "/generatingStructures.json";
            if (!File.Exists(generatingStructuresPath))
            {
                Debug.LogWarning("in loading world, generating structures json " + generatingStructuresPath + " does not exist, assuming there are no structures that have not finished generating yet");
            }
            else
            {
                SavedStructureCollection savedStructures = Newtonsoft.Json.JsonConvert.DeserializeObject<SavedStructureCollection>(File.ReadAllText(generatingStructuresPath));
                foreach (SavedStructure structure in savedStructures.savedStructures)
                {
                    Structure savedStructure = new Structure(structure);
                    unfinishedStructures.Add(savedStructure);
                }
            }

            // load new chunks
            if (chunksDirInfo.Exists)
            {
                string[] chunkFiles = Directory.GetFiles(chunksDirInfo.FullName, "*.dat", SearchOption.TopDirectoryOnly);
                foreach (string chunkFile in chunkFiles)
                {
                    FileInfo fInfo = new FileInfo(chunkFile);
                    if (fInfo.Exists)
                    {
                        string nameWithoutDotDat = fInfo.Name.Substring(0, fInfo.Name.Length - ".dat".Length);
                        string[] coordinates = nameWithoutDotDat.Split('.');
                        if (coordinates.Length != 3)
                        {
                            Debug.LogWarning("invalid chunk file name " + fInfo.Name + " ignoring that file");
                        }
                        else
                        {
                            int cx, cy, cz;
                            if (int.TryParse(coordinates[0], out cx) && int.TryParse(coordinates[1], out cy) && int.TryParse(coordinates[2], out cz))
                            {
                                Chunk spruce = GetOrGenerateChunk(cx, cy, cz);
                                spruce.generating = false;
                                spruce.chunkData.ReadFromFile(fInfo.FullName);
                            }
                        }
                    }
                }
            }


            // it is important we load config  before loading inventories since inventories are stored by block id and block id mapping to blocks can vary so the config sets up the proper mapping for block ids to blocks
            // loading the chunks is also probably important so the inventories are assigned to the right kinds of blocks
            string inventoriesPath = cleanedRootDir + "/blockInventories.json";
            if (!File.Exists(inventoriesPath))
            {
                Debug.LogWarning("in loading world, inventory json " + inventoriesPath + " does not exist, assuming there are no block inventories/chests");
            }
            else
            {
                blocksWorld.blockInventories.Clear();
                BlockInventoryCollection blockInventories = Newtonsoft.Json.JsonConvert.DeserializeObject<BlockInventoryCollection>(File.ReadAllText(inventoriesPath));
                for (int i = 0; i < blockInventories.inventories.Length; i++)
                {
                    BlockInventory inventory = blockInventories.inventories[i];
                    LVector3 blockPos = new LVector3(inventory.x, inventory.y, inventory.z);
                    Inventory blockInventory = new Inventory(inventory.blocks.Length);
                    for (int j = 0; j < inventory.blocks.Length; j++)
                    {
                        if (inventory.counts[j] != 0 && inventory.blocks[j] != 0)
                        {
                            blockInventory.blocks[j] = new BlockStack(inventory.blocks[j], inventory.counts[j], inventory.durabilities[j], inventory.maxDurabilities[j]);
                        }
                    }
                    blocksWorld.blockInventories[blockPos] = blockInventory;
                    if (this[blockPos] == Example.CraftingTable)
                    {
                        blockInventory.resultBlocks = new BlockStack[1];
                    }
                }
            }



            Debug.Log("done loading chunks");
        }

        public int AddChunkProperty(ChunkProperty chunkProperty)
        {
            return chunkProperties.AddChunkProperty(chunkProperty);
        }

        List<WorldGenerationEvent> worldGenerationEvents = new List<WorldGenerationEvent>();

        public void AddWorldGenerationEvent(WorldGenerationEvent worldGenerationEvent)
        {
            worldGenerationEvents.Add(worldGenerationEvent);
        }


        public void AddChunkPropertyEvent(ChunkPropertyEvent chunkPropertyEvent)
        {
            chunkProperties.AddChunkPropertyEvent(chunkPropertyEvent);
        }

        public static string BlockToString(int block)
        {
            return BlockToString((BlockValue)block);
        }


        public static string BlockToString(BlockValue block)
        {
            try
            {

                return BlockUtils.BlockIdToString((int)block);
            }
            catch
            {
                return "Unknown block id " + (int)block;
            }
            /*
            switch (block)
            {
                case BlockValue.Rock: return "Rock";
                case BlockValue.SharpRock: return "Sharp Rock";
                case BlockValue.LargeRock: return "Large Rock";
                case BlockValue.LargeSharpRock: return "Large Sharp Rock";
                case BlockValue.Clay: return "Clay"; ;
                //case BlockValue.LEAF: return "Leaf"; ;
               // case BlockValue.TRUNK: return "Log";
                case BlockValue.Bark: return "Bark";
                case BlockValue.LooseRocks: return "Loose Rocks";
                case BlockValue.WaterNoFlow: return "Water (no flow)";
                case BlockValue.Water: return "Water";
                case BlockValue.Bedrock: return "Bedrock";
                case BlockValue.Dirt: return "Dirt";
                case BlockValue.Grass: return "Grass";
                case BlockValue.Stone: return "Stone";
                case BlockValue.Air: return "Air";
                case BlockValue.Wildcard: return "Wildcard";
                case BlockValue.CraftingTable: return "Chest";
                default: return "unknown??";
            }
            */
        }

        public float worldScale
        {
            get
            {
                return blocksWorld.worldScale;
            }
            set
            {
                blocksWorld.worldScale = value;
            }
        }

        public static Dictionary<int, int> stackableSize;

        public int chunkSize;
        QuickLongDict<List<Chunk>> chunksPerX;
        QuickLongDict<List<Chunk>> chunksPerY;
        QuickLongDict<List<Chunk>> chunksPerZ;
        QuickLongDict<List<Chunk>>[] chunksPer;
        public const int DIM = 3;
        long lowestCy = long.MaxValue;

        public long GetLowestCy()
        {
            return lowestCy;
        }

        public ComputeBuffer argBuffer;

        public List<Chunk> allChunks;
        public static Dictionary<int, int> maxCapacities;

        public List<Chunk> chunkCache;
        int cacheSize = 20;

        public List<Structure> unfinishedStructures;


        public bool DropBlockOnDestroy(BlockValue block, LVector3 pos, BlockStack thingHolding, Vector3 positionOfBlock, Vector3 posOfOpening)
        {
            if (World.creativeMode)
            {
                return true;
            }
            //CreateBlockEntity(block, positionOfBlock);
            //return true;
            if (thingHolding == null)
            {
                thingHolding = new BlockStack(BlockValue.Air, 1);
            }
            BlockOrItem customBlock;
            if (customBlocks.ContainsKey(block, out customBlock))
            {
                bool destroyBlock;
                using (BlockData blockData = GetBlockData(pos.x, pos.y, pos.z))
                {
                    customBlock.DropBlockOnDestroy(blockData, thingHolding, positionOfBlock, posOfOpening, out destroyBlock);
                }
                return destroyBlock;
            }
            Debug.Log("warning, drop block on destroy fell through, block = " + World.BlockToString(block) + " and block hitting with = " + World.BlockToString(thingHolding.Block));
            CreateBlockEntity(block, positionOfBlock);
            return true;
        }


        public BlockValue[] items = new BlockValue[] {
        ////BlockValue.Stick,
        ////BlockValue.SharpRock,
        ////BlockValue.LargeSharpRock
    };

        public bool AllowedtoPlaceBlock(BlockValue block)
        {
            BlockOrItem customBlock;
            if (customBlocks.ContainsKey(block, out customBlock))
            {
                return customBlock.CanBePlaced();
            }
            foreach (BlockValue item in items)
            {
                if (item == block)
                {
                    return false;
                }
            }
            return true;
        }

        BlockEntity CreateBlockEntity(BlockValue block, Vector3 position)
        {
            GameObject blockEntity = GameObject.Instantiate(blocksWorld.blockEntityPrefab);
            blockEntity.transform.position = position;
            blockEntity.GetComponent<BlockEntity>().blockId = (int)block;
            return blockEntity.GetComponent<BlockEntity>();
        }

        public BlockEntity CreateBlockEntity(BlockStack block, Vector3 position)
        {
            GameObject blockEntity = GameObject.Instantiate(blocksWorld.blockEntityPrefab);
            blockEntity.transform.position = position;
            blockEntity.GetComponent<BlockEntity>().blockId = block.block;
            blockEntity.GetComponent<BlockEntity>().blockStack = block.Copy();
            return blockEntity.GetComponent<BlockEntity>();
        }

        public FastSimpleBlockLookup customBlocks;
        public GenerationClass worldGeneration;
        public BlocksTouchingSky blocksTouchingSky;

        public World(BlocksWorld blocksWorld, int chunkSize, BlocksPack blocksPack)
        {
            this.chunkSize = chunkSize;
            this.worldGeneration = blocksPack.customGeneration;
            this.customBlocks = blocksPack.customBlocks;
            this.blocksTouchingSky = new BlocksTouchingSky(this);
            stackableSize = new Dictionary<int, int>();
            foreach (KeyValuePair<BlockValue, BlockOrItem> customBlock in customBlocks)
            {
                customBlock.Value.blockGetter = this;
                customBlock.Value.world = this;
                stackableSize[customBlock.Key] = customBlock.Value.stackSize;
            }



            foreach (Recipe recipe in blocksPack.customRecipes)
            {
                blocksWorld.otherObjectInventoryGui.GetComponent<CraftingDesk>().AddRecipe(recipe);
            }


            blockDataCache = new BlockDataCache(this);
            chunkProperties = new ChunkProperties();
            World.mainWorld = this;
            this.blocksWorld = blocksWorld;

            /*

            stackableSize[(int)Example.Dirt] = 16;
            stackableSize[(int)Example.Stone] = 45;
            stackableSize[(int)Example.Grass] = 64;
            stackableSize[(int)Example.Sand] = 32;
            stackableSize[(int)Example.Bedrock] = 16;
            stackableSize[(int)Example.Clay] = 64;
            stackableSize[(int)Example.Leaf] = 64;
            stackableSize[(int)Example.Stick] = 64;
            stackableSize[(int)Example.Rock] = 64;
            stackableSize[(int)Example.LargeRock] = 64;
            stackableSize[(int)Example.SharpRock] = 64;
            stackableSize[(int)Example.LargeSharpRock] = 64;
            stackableSize[(int)Example.Bark] = 64;
            stackableSize[(int)Example.String] = 64;
            */

            chunksPerX = new QuickLongDict<List<Chunk>>(8);
            chunksPerY = new QuickLongDict<List<Chunk>>(8);
            chunksPerZ = new QuickLongDict< List<Chunk>>(8);
            chunksPer = new QuickLongDict<List<Chunk>>[] { chunksPerX, chunksPerY, chunksPerZ };

            argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            allChunks = new List<Chunk>();

            chunkCache = new List<Chunk>(cacheSize);

            unfinishedStructures = new List<Structure>();

            maxCapacities = new Dictionary<int, int>();
            maxCapacities[(int)Example.Dirt] = 3;
            maxCapacities[(int)Example.Stone] = 5;
            maxCapacities[(int)Example.Grass] = 4;
            maxCapacities[(int)Example.Sand] = 0;
            maxCapacities[(int)Example.Air] = 0;
            maxCapacities[(int)Example.Bedrock] = 6;
            this.worldGeneration.world = this;
            this.worldGeneration.blockGetter = this;
            this.worldGeneration.OnGenerationInit();
            RunWorldGenerationEvents(-10, -10, -10, 20);
            GenerateChunk(0, 0, 0);
            //return;
            int viewDist = 0;
            for (int i = -viewDist; i <= viewDist; i++)
            {
                for (int j = viewDist; j >= -viewDist; j--)
                {
                    for (int k = -viewDist; k <= viewDist; k++)
                    {
                        GenerateChunk(i, j, k);
                    }
                }
            }

        }

        public void AddUnfinishedStructure(Structure structure)
        {
            unfinishedStructures.Add(structure);
        }


        void RunWorldGenerationEvents(long baseChunkCX, long baseChunkCY, long baseChunkCZ, int numChunksWide)
        {
            foreach (WorldGenerationEvent generationEvent in worldGenerationEvents)
            {
                Structure generationEventStructure = new Structure("bah", true, GetOrGenerateChunk(baseChunkCX, baseChunkCY, baseChunkCZ), generationEvent.priority);
                BlockGetter prevGetter = worldGeneration.blockGetter;
                worldGeneration.blockGetter = generationEventStructure;
                generationEvent.Run(baseChunkCX, baseChunkCY, baseChunkCZ, numChunksWide);
                if (!generationEventStructure.HasAllChunksGenerated())
                {
                    AddUnfinishedStructure(generationEventStructure);
                }
                worldGeneration.blockGetter = prevGetter;
            }
        }

        public delegate float ChunkValueGetter(ChunkBiomeData chunk);


        Dictionary<LVector3, ChunkBiomeData> ungeneratedChunkBiomeDatas = new Dictionary<LVector3, ChunkBiomeData>();


        ChunkBiomeData lastRequest;

        public ChunkBiomeData GetChunkBiomeData(long cx, long cy, long cz)
        {
            Chunk chunk = GetChunk(cx, cy, cz);
            if (chunk != null)
            {
                return chunk.chunkBiomeData;
            }
            else
            {
                LVector3 chunkPos = new LVector3(cx, cy, cz);
                if (ungeneratedChunkBiomeDatas.ContainsKey(chunkPos))
                {
                    return ungeneratedChunkBiomeDatas[chunkPos];
                }
                else
                {
                    ChunkBiomeData res = new ChunkBiomeData(chunkProperties, chunkPos.x, chunkPos.y, chunkPos.z);
                    ungeneratedChunkBiomeDatas[chunkPos] = res;
                    return res;
                }
            }
        }


        public float AverageChunkValues(long x, long y, long z, ChunkProperty chunkProperty)
        {
            long cx, cy, cz;
            GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            return AverageChunkValues(x, y, z, cx, cy, cz, chunkProperty);
        }

        public float AverageChunkValues(long x, long y, long z, long cx, long cy, long cz, ChunkProperty chunkProperty)
        {
            ChunkBiomeData chunkBiomeData = GetChunkBiomeData(cx, cy, cz);
            return chunkBiomeData.AverageBiomeData(x, y, z, chunkProperty);
            /*
            ChunkBiomeData chunkx2z1 = GetChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithFloor(z, chunkSize));
            ChunkBiomeData chunkx1z2 = GetChunkBiomeData(divWithFloor(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));
            ChunkBiomeData chunkx2z2 = GetChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));

            long x1Weight = x - chunkx1z1.cx * chunkSize;
            long x2Weight = chunkx2z1.cx * chunkSize - x;
            long z1Weight = z - chunkx1z1.cz * chunkSize;
            long z2Weight = chunkx2z1.cz * chunkSize - z;

            float px = x1Weight / (float)chunkSize;
            float pz = z1Weight / (float)chunkSize;


            float valZ1 = getChunkValue(chunkx1z1) * (1 - px) + getChunkValue(chunkx2z1) * px;
            float valZ2 = getChunkValue(chunkx1z2) * (1 - px) + getChunkValue(chunkx2z2) * px;

            return valZ1 * (1 - pz) + valZ2 * pz;
            */
        }
        /*

        public float AverageChunkValues(long x, long y, long z, string valueKey)
        {
            ChunkBiomeData chunkx1z1 = GetChunkBiomeData(divWithFloor(x, chunkSize), divWithFloor(y, chunkSize), divWithFloor(z, chunkSize));
            ChunkBiomeData chunkx2z1 = GetChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithFloor(z, chunkSize));
            ChunkBiomeData chunkx1z2 = GetChunkBiomeData(divWithFloor(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));
            ChunkBiomeData chunkx2z2 = GetChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));

            long x1Weight = x - chunkx1z1.cx * chunkSize;
            long x2Weight = chunkx2z1.cx * chunkSize - x;
            long z1Weight = z - chunkx1z1.cz * chunkSize;
            long z2Weight = chunkx2z1.cz * chunkSize - z;

            float px = x1Weight / (float)chunkSize;
            float pz = z1Weight / (float)chunkSize;


            float valZ1 = chunkx1z1[valueKey] * (1 - px) + chunkx2z1[valueKey] * px;
            float valZ2 = chunkx1z2[valueKey] * (1 - px) + chunkx2z2[valueKey] * px;

            return valZ1 * (1 - pz) + valZ2 * pz;
        }
        */

        /*
        public bool NeedsInitialUpdate(int block)
        {
            if (block == (int)BlockValue.Grass)
            {
                return true;
            }
            return false;
        }
        */


        public override void SetState(long i, long j, long k, long cx, long cy, long cz, int state, BlockState stateType)
        {
            blockModifyState += 1;
            Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
            chunk.SetState(i, j, k, state, stateType);
        }
        public override int GetState(long i, long j, long k, long cx, long cy, long cz, BlockState stateType)
        {
            Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
            return chunk.GetState(i, j, k, stateType);
        }


        public bool TryGetHighestSolidBlockY(long x,long z, out long highestBlockY)
        {
            long cx = divWithFloorForChunkSize(x);
            long cz = divWithFloorForChunkSize(z);
            int numAtX = 0;
            if (chunksPerX.ContainsKey(cx))
            {
                numAtX = chunksPerX[cx].Count;
            }

            int numAtZ = 0;
            if (chunksPerZ.ContainsKey(cz))
            {
                numAtZ = chunksPerZ[cz].Count;
            }
            highestBlockY = long.MinValue;

            if (numAtX == 0 && numAtZ == 0)
            {
                return false;
            }
            List<Chunk> chunksToLookThrough;
            if (numAtX > numAtZ)
            {
                chunksToLookThrough = chunksPerZ[cz];
            }
            else
            {
                chunksToLookThrough = chunksPerX[cx];
            }
            bool found = false;
            for (int i = 0; i < chunksToLookThrough.Count; i++)
            {
                Chunk cur = chunksToLookThrough[i];
                if (cur.cx == cx && cur.cz == cz)
                {
                    long curHighestY;
                    if(cur.TryGetHighestSolidBlockY(x, z, out curHighestY))
                    {
                        highestBlockY = System.Math.Max(highestBlockY, curHighestY);
                        found = true;
                    }
                }
            }
            return found;
        }



        public int TrickleSupportPowerUp(int blockFrom, int powerFrom, int blockTo)
        {
            if (blockTo == (int)BlockValue.Air)
            {
                return 0;
            }
            if (blockFrom == (int)BlockValue.Air)
            {
                return 0;
            }
            int maxCapacityTo = int.MaxValue;
            if (maxCapacities.ContainsKey(blockTo))
            {
                maxCapacityTo = maxCapacities[blockTo];
            }
            return System.Math.Min(powerFrom, maxCapacityTo); // doesn't lose support power if stacked on top of each other, but certain types of blocks can only hold so much support power
            //return powerFrom; // or we just carry max power for up?
        }
        public int TrickleSupportPowerSidewaysOrDown(int blockFrom, int powerFrom, int blockTo)
        {
            if (blockTo == (int)BlockValue.Air)
            {
                return 0;
            }
            if (blockFrom == (int)BlockValue.Air)
            {
                return 0;
            }

            int maxCapacityTo = int.MaxValue;
            if (maxCapacities.ContainsKey(blockTo))
            {
                maxCapacityTo = maxCapacities[blockTo];
            }
            return System.Math.Max(0, System.Math.Min(powerFrom - 1, maxCapacityTo)); // loses 1 support power if not up, also some blocks are more "sturdy" than others
        }

        public static int[] sidewaysNeighborsX = new int[] { -1, 1, 0, 0 };
        public static int[] sidewaysNeighborsZ = new int[] { 0, 0, -1, 1 };




        public IEnumerable<LVector3> SidewaysNeighbors(bool up = false, bool down = false)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                yield return new LVector3(-1, 0, 0);
                yield return new LVector3(1, 0, 0);
                yield return new LVector3(0, 0, -1);
                yield return new LVector3(0, 0, 1);
            }
            else if (globalPreference == 1)
            {
                yield return new LVector3(1, 0, 0);
                yield return new LVector3(0, 0, -1);
                yield return new LVector3(0, 0, 1);
                yield return new LVector3(-1, 0, 0);
            }
            else if (globalPreference == 2)
            {
                yield return new LVector3(0, 0, -1);
                yield return new LVector3(0, 0, 1);
                yield return new LVector3(-1, 0, 0);
                yield return new LVector3(1, 0, 0);
            }
            else if (globalPreference == 3)
            {
                yield return new LVector3(0, 0, 1);
                yield return new LVector3(-1, 0, 0);
                yield return new LVector3(1, 0, 0);
                yield return new LVector3(0, 0, -1);
            }

            if (down)
            {
                yield return new LVector3(0, -1, 0);
            }
            if (up)
            {
                yield return new LVector3(0, 1, 0);
            }
        }


        public IEnumerable<LVector3> SidewaysNeighborsRelative(LVector3 pos, bool vertical = false)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                yield return new LVector3(pos.x - 1, pos.y, pos.z);
                yield return new LVector3(pos.x + 1, pos.y, pos.z);
                yield return new LVector3(pos.x, pos.y, pos.z + 1);
                yield return new LVector3(pos.x, pos.y, pos.z - 1);
            }
            else if (globalPreference == 1)
            {
                yield return new LVector3(pos.x + 1, pos.y, pos.z);
                yield return new LVector3(pos.x, pos.y, pos.z + 1);
                yield return new LVector3(pos.x, pos.y, pos.z - 1);
                yield return new LVector3(pos.x - 1, pos.y, pos.z);
            }
            else if (globalPreference == 2)
            {
                yield return new LVector3(pos.x, pos.y, pos.z + 1);
                yield return new LVector3(pos.x, pos.y, pos.z - 1);
                yield return new LVector3(pos.x - 1, pos.y, pos.z);
                yield return new LVector3(pos.x + 1, pos.y, pos.z);
            }
            else if (globalPreference == 3)
            {
                yield return new LVector3(pos.x, pos.y, pos.z - 1);
                yield return new LVector3(pos.x - 1, pos.y, pos.z);
                yield return new LVector3(pos.x + 1, pos.y, pos.z);
                yield return new LVector3(pos.x, pos.y, pos.z + 1);
            }

            if (vertical)
            {
                yield return new LVector3(pos.x, pos.y - 1, pos.z);
                yield return new LVector3(pos.x, pos.y + 1, pos.z);
            }
        }

        static int globalPreference = 4;

        public IEnumerable<LVector3> AllNeighborsRelative(LVector3 pos)
        {
            return SidewaysNeighborsRelative(pos, vertical: true);
        }
        public IEnumerable<LVector3> AllNeighborsExceptDown()
        {
            yield return new LVector3(-1, 0, 0);
            yield return new LVector3(1, 0, 0);
            yield return new LVector3(0, 0, -1);
            yield return new LVector3(0, 0, 1);
            yield return new LVector3(0, 1, 0);
        }
        public IEnumerable<LVector3> AllNeighbors()
        {
            yield return new LVector3(-1, 0, 0);
            yield return new LVector3(1, 0, 0);
            yield return new LVector3(0, 0, -1);
            yield return new LVector3(0, 0, 1);
            yield return new LVector3(0, -1, 0);
            yield return new LVector3(0, 1, 0);
        }


        delegate bool MeetsCondition(LVector3 block);

        int RandomlyChooseNeighborMeetingCondition(LVector3 center, MeetsCondition conditionFunc, out LVector3 neighborRes, bool allowUpDown)
        {
            List<LVector3> goods = new List<LVector3>();
            foreach (LVector3 neighbor in SidewaysNeighborsRelative(center))
            {
                if (conditionFunc(neighbor))
                {
                    goods.Add(neighbor);
                }
            }
            if (goods.Count == 0)
            {
                neighborRes = new LVector3(center.x, center.y, center.z);
                return goods.Count;
            }
            else
            {
                neighborRes = goods[Random.Range(0, goods.Count)];
                return goods.Count;
            }
            /*

            bool forwardOpen = conditionFunc(new LVector3(center.x, center.y, center.z+1));
            bool backOpen = conditionFunc(new LVector3(center.x, center.y, center.z-1));
            bool upOpen = false;
            bool downOpen = false;
            if (allowUpDown)
            {
                upOpen = conditionFunc(new LVector3(center.x, center.y, center.z));
                downOpen = conditionFunc(new LVector3(center.x, center.y, center.z));
            }
            bool leftOpen = conditionFunc(new LVector3(center.x, center.y, center.z));
            bool rightOpen = conditionFunc(new LVector3(center.x, center.y, center.z));
            bool leftOpen = this[wx, wy, wz + 1] == AIR;
            bool forwadOpen = this[wx + 1, wy, wz] == AIR;
            bool backOpen = this[wx - 1, wy, wz] == AIR;
            int numBlocks = 0;
            if (leftOpen) numBlocks += 1;
            if (rightOpen) numBlocks += 1;
            if (forwadOpen) numBlocks += 1;
            if (backOpen) numBlocks += 1;

            if (numBlocks > 0)
            {
                float[] weights = new float[] { leftOpen ? 1.0f / numBlocks : 0, rightOpen ? 1.0f / numBlocks : 0, forwadOpen ? 1.0f / numBlocks : 0, backOpen ? 1.0f / numBlocks : 0 };
                float[] weights1 = new float[] { leftOpen ? 1.0f / numBlocks : 0, rightOpen ? 1.0f / numBlocks : 0, forwadOpen ? 1.0f / numBlocks : 0, backOpen ? 1.0f / numBlocks : 0 };
                long[] xOffsets = new long[] { 0, 0, 1, -1 };
                long[] zOffsets = new long[] { -1, 1, 0, 0 };
                float val = Random.value;
                float sum = 0.0f;
                for (int i = 0; i < weights.Length; i++)
                {
                    sum += weights[i];
                    weights1[i] = sum;

                }
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights1[i] >= val && weights[i] != 0.0f)
                    {
                        this[wx + xOffsets[i], wy, wz + zOffsets[i]] = WATER;
                        return AIR;
                    }
                }
            }
            */
        }



        // for water
        // if a block is "pushed", pathfind until find an open block with y level < current pos
        //     if found, tp there, replace self with air
        //        if that position has water below it, tp there as pushed
        //        else tp there as unpushed
        //     if not found, replace self with "not pushed"

        // if a unpushed water block gets an update, flood fill back and replace all wate on top of water with pushed water

        /*
        public bool IsWater(int block)
        {
            return block == (int)BlockValue.Water || block == (int)BlockValue.WaterNoFlow;
        }
        */

        public int GetNumAirNeighbors(long wx, long wy, long wz)
        {
            return
                (this[wx + 1, wy, wz] == (int)BlockValue.Air ? 1 : 0) +
                (this[wx - 1, wy, wz] == (int)BlockValue.Air ? 1 : 0) +
                (this[wx, wy + 1, wz] == (int)BlockValue.Air ? 1 : 0) +
                (this[wx, wy - 1, wz] == (int)BlockValue.Air ? 1 : 0) +
                (this[wx, wy, wz + 1] == (int)BlockValue.Air ? 1 : 0) +
                (this[wx, wy, wz - 1] == (int)BlockValue.Air ? 1 : 0);

        }

        public int GetWaterAirOnlyAbove(long wx, long wy, long wz)
        {
            if (this[wx, wy + 1, wz] == (int)BlockValue.Air &&
                this[wx + 1, wy, wz] != (int)BlockValue.Air &&
                this[wx - 1, wy, wz] != (int)BlockValue.Air &&
                this[wx, wy, wz + 1] != (int)BlockValue.Air &&
                this[wx, wy, wz - 1] != (int)BlockValue.Air)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        /*

        // water state 2 = air accessable by me + air accessable by newers (sum of state 2 of newers)
        public int GetNewerWaterNeighborValues(long wx, long wy, long wz, int state1)
        {
            return
                ((this[wx + 1, wy, wz] == (int)BlockValue.Water && GetState(wx + 1, wy, wz, 1) < state1) ? GetState(wx + 1, wy, wz, 2) : 0) +
                ((this[wx - 1, wy, wz] == (int)BlockValue.Water && GetState(wx - 1, wy, wz, 1) < state1) ? GetState(wx - 1, wy, wz, 2) : 0) +
                ((this[wx, wy + 1, wz] == (int)BlockValue.Water && GetState(wx, wy + 1, wz, 1) < state1) ? GetState(wx, wy + 1, wz, 2) : 0) +
                ((this[wx, wy - 1, wz] == (int)BlockValue.Water && GetState(wx, wy - 1, wz, 1) < state1) ? GetState(wx, wy - 1, wz, 2) : 0) +
                ((this[wx, wy, wz + 1] == (int)BlockValue.Water && GetState(wx, wy, wz + 1, 1) < state1) ? GetState(wx, wy, wz + 1, 2) : 0) +
                ((this[wx, wy, wz - 1] == (int)BlockValue.Water && GetState(wx, wy, wz - 1, 1) < state1) ? GetState(wx, wy, wz - 1, 2) : 0);
        }

        // water state 3 = air accessable by me + air accessable by olders (sum of state 3 of olders)
        public int GetOlderWaterNeighborValues(long wx, long wy, long wz, int state1)
        {
            return
                ((this[wx + 1, wy, wz] == (int)BlockValue.Water && GetState(wx + 1, wy, wz, 1) > state1) ? GetState(wx + 1, wy, wz, 3) : 0) +
                ((this[wx - 1, wy, wz] == (int)BlockValue.Water && GetState(wx - 1, wy, wz, 1) > state1) ? GetState(wx - 1, wy, wz, 3) : 0) +
                ((this[wx, wy + 1, wz] == (int)BlockValue.Water && GetState(wx, wy + 1, wz, 1) > state1) ? GetState(wx, wy + 1, wz, 3) : 0) +
                ((this[wx, wy - 1, wz] == (int)BlockValue.Water && GetState(wx, wy - 1, wz, 1) > state1) ? GetState(wx, wy - 1, wz, 3) : 0) +
                ((this[wx, wy, wz + 1] == (int)BlockValue.Water && GetState(wx, wy, wz + 1, 1) > state1) ? GetState(wx, wy, wz + 1, 3) : 0) +
                ((this[wx, wy, wz - 1] == (int)BlockValue.Water && GetState(wx, wy, wz - 1, 1) > state1) ? GetState(wx, wy, wz - 1, 3) : 0);
        }
        */



        int waterFrameT = 0;

        public int GetWaterFrameT()
        {
            int res = waterFrameT;
            waterFrameT = (waterFrameT + 1) % (int.MaxValue - 1); // mod prevents overflow weirdness
            return res;
        }

        public int numBlockUpdatesThisTick = 0;
        public int numWaterUpdatesThisTick = 0;


        // water idea: determine where flow ahead of time, then trickle water through those "chosen directions" - allows for one quick "dam breaking" computation and then the water can flow down precomputing stuff
        //public int UpdateBlock(long wx, long wy, long wz, int block, int state1, int state2, int state3, out int resState1, out int resState2, out int resState3, out bool needsAnotherUpdate)
        // {
        /*
        needsAnotherUpdate = false;
        resState1 = state1;
        resState2 = state2;
        resState3 = state3;
        //Debug.Log("updating block " + wx + " " + wy + " " + wz + " " + block + " " + state);
        if (block == (int)BlockValue.Air)
        {
            resState1 = 0;
            resState2 = 0;
            needsAnotherUpdate = false;
            return (int)BlockValue.Air;
        }



        // water state 1 = time when put there
        // water state 2 = air accessable by me + air accessable by newers (sum of state 2 of newers)
        // water state 3 = air accessable by me + air accessable by olders (sum of state 3 of olders)

        // should ensure no cycles unless we get overflows and manage to loop back to the same number again, but that should rarely happen? idk something to consider


        if (block == (int)BlockValue.Sand)
        {
            if (this[wx, wy-1, wz] == (int)BlockValue.Air)
            {
                this[wx, wy - 1, wz] = (int)BlockValue.Water;
                SetState(wx, wy - 1, wz, GetWaterFrameT(), 1);
                // reset initial air neighbors because it'll have to recompute that anyway
                SetState(wx, wy - 1, wz, 0, 2);
                SetState(wx, wy - 1, wz, 0, 3);
            }
        }
        */
        /*
        if (block == WATER)
        {
            numWaterUpdatesThisTick += 1;
            if (this[wx, wy-1, wz] == AIR)
            {
                this[wx, wy - 1, wz] = WATER;
                SetState(wx, wy - 1, wz, GetWaterFrameT(), 1);
                // reset initial air neighbors because it'll have to recompute that anyway
                SetState(wx, wy - 1, wz, 0, 2);
                SetState(wx, wy - 1, wz, 0, 3);
                return AIR;
            }
            else
            {
                // look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                foreach (LVector3 neighbor in SidewaysNeighbors())
                {
                    LVector3 pos = new LVector3(wx, wy, wz);
                    LVector3 nPos = pos + neighbor;
                    LVector3 nPos2 = pos + neighbor * 2;
                    if ((nPos.Block == AIR && (this[nPos.x, nPos.y - 1, nPos.z] == AIR || (nPos2.Block == AIR && this[nPos2.x, nPos2.y - 1, nPos2.z] == AIR))))
                    {
                        this[nPos.x, nPos.y, nPos.z] = WATER;
                        SetState(nPos.x, nPos.y, nPos.z, GetWaterFrameT(), 1);
                        SetState(nPos.x, nPos.y, nPos.z, 0, 2);
                        SetState(nPos.x, nPos.y, nPos.z, 0, 3);
                        AddBlockUpdateToNeighbors(wx, wy, wz);
                        AddBlockUpdateToNeighbors(nPos.x, nPos.y, nPos.z);
                        return AIR;
                    }
                }
                // water state 1 = time when put there
                // water state 2 = air accessable by me + air accessable by newers (sum of state 2 of newers)
                // water state 3 = air accessable by me + air accessable by olders (sum of state 3 of olders)
                int numAirNeighbors = GetWaterAirOnlyAbove(wx, wy, wz);
                int numNewerAirs = GetNewerWaterNeighborValues(wx, wy, wz, state1);
                int numOlderAirs = GetOlderWaterNeighborValues(wx, wy, wz, state1);
                // water below, try to flow through it (but only if we just got new info)
                if (this[wx, wy - 1, wz] == WATER && (numNewerAirs + numAirNeighbors != state2 || numOlderAirs + numAirNeighbors != state3))
                {
                    int maxSteps = 30;
                    // below is older
                    if (GetState(wx, wy-1, wz, 1) < state1)
                    {
                        // below has air through olders, we can try to trickle through it
                        if (GetState(wx, wy - 1, wz, 3) > 0)
                        {
                            Debug.Log(wx + " " + wy + " " + wz);
                            numBlockUpdatesThisTick += 1;
                            if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy - 1, wz), maxSteps, true, true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                            {
                                // make sure we only go through olders
                                //int prevState1 = GetState(pbx, pby, pbz, 1);
                                //int curState1 = GetState(bx, by, bz, 1);
                                //return curState1 < prevState1 && by < wy && GetState(bx, by, bz, 2) > 0;
                                return by < wy && GetState(bx, by, bz, 2) > 0;
                            }, isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                            {
                                if (b == AIR && by < wy)
                                {
                                    this[bx, by, bz] = WATER;
                                    SetState(bx, by, bz, GetWaterFrameT(), 1);
                                    SetState(bx, by, bz, 0, 2);
                                    SetState(bx, by, bz, 0, 3);
                                    return true;
                                }
                                return false;
                            }))
                            {
                                resState1 = 0;
                                resState2 = 0;
                                resState3 = 0;
                                return AIR;
                            }
                        }
                    }
                    // below is newer
                    else if (GetState(wx, wy - 1, wz, 1) > state1)
                    {
                        // below has air through newers, we can try to trickle through it
                        if (GetState(wx, wy - 1, wz, 2) > 0)
                        {
                            Debug.Log(wx + " " + wy + " " + wz + " h");

                            numBlockUpdatesThisTick += 1;
                            if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy - 1, wz), maxSteps, true, true, isBlockValid:(b, bx, by, bz, pbx, pby, pbz) =>
                              {
                                  // make sure we only go through newers
                                  //int prevState1 = GetState(pbx, pby, pbz, 1);
                                  //int curState1 = GetState(bx, by, bz, 1);
                                  //return curState1 > prevState1 && by < wy && GetState(bx, by, bz, 2) > 0;
                                  return by < wy && GetState(bx, by, bz, 2) > 0;
                              }, isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                              {
                                  if (b == AIR && by < wy)
                                  {
                                      this[bx, by, bz] = WATER;
                                      SetState(bx, by, bz, GetWaterFrameT(), 1);
                                      SetState(bx, by, bz, 0, 2);
                                      SetState(bx, by, bz, 0, 3);
                                      return true;
                                  }
                                  return false;
                              }))
                            {
                                resState1 = 0;
                                resState2 = 0;
                                resState3 = 0;
                                return AIR;
                            }
                        }
                    }
                }



                resState2 = numNewerAirs + numAirNeighbors;
                resState3 = numOlderAirs + numAirNeighbors;
                if (state2 != resState2 || state3 != resState3)
                {
                    AddBlockUpdateToNeighbors(wx, wy, wz);
                    
                }
                return WATER;
            }
        }
        else
        {
        }
        */


        /*


        // good water, slightly inefficient
        

        if (block == (int)BlockValue.Sand)
        {
            if (this[wx, wy-1, wz] == (int)BlockValue.Water || this[wx, wy-1, wz] == (int)BlockValue.WaterNoFlow)
            {
                this[wx, wy - 1, wz] = (int)BlockValue.Water;
                needsAnotherUpdate = true;
                resState1 = 1 - state1;
                SetState(wx, wy - 1, wz, resState1, 1);
                return block;
            }
            else if(this[wx, wy-1, wz] == (int)BlockValue.Air)
            {
                this[wx, wy - 1, wz] = (int)BlockValue.Water;
                //SetState(wx, wy - 1, wz, 1, 3);
                return block;
            }
        }


        // water: state 2 = time I got here


        if (block == (int)BlockValue.Water || block == (int)BlockValue.WaterNoFlow)
        {

            needsAnotherUpdate = false;



            // if we are WATER without water above and with water below, pathfind to look for open space
            if (block == (int)BlockValue.Water && IsWater(this[wx, wy - 1, wz]) && !IsWater(this[wx, wy + 1, wz]))
            {
                // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                numWaterUpdatesThisTick += 1;
                if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                {
                    return by < wy && (b == (int)BlockValue.Water || b == (int)BlockValue.WaterNoFlow);
                    },
                   isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                   {
                       if (b == (int)BlockValue.Air && by < wy)
                       {
                           this[bx, by, bz] = (int)BlockValue.Water;
                           SetState(bx, by, bz, GetNumAirNeighbors(bx, by, bz), 3);
                           return true;
                       }
                       return false;
                   }
                ))
                {
                    resState3 = 0;
                    return (int)BlockValue.Air;
                }
                else
                {
                    needsAnotherUpdate = true;
                    return (int)BlockValue.WaterNoFlow;
                }
            }
            else
            {

                // if air below, set below = water and us = air
                if (this[wx, wy - 1, wz] == (int)BlockValue.Air)
                {
                    this[wx, wy - 1, wz] = (int)BlockValue.Water;
                    resState3 = 0;
                    SetState(wx, wy - 1, wz, GetNumAirNeighbors(wx, wy - 1, wz) + 1, 3); // +1 because we are now air instead of water
                    return (int)BlockValue.Air;
                }
                else
                {
                    // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                    foreach (LVector3 neighbor in SidewaysNeighbors())
                    {
                        LVector3 pos = new LVector3(wx, wy, wz);
                        LVector3 nPos = pos + neighbor;
                        LVector3 nPos2 = pos + neighbor * 2;
                        if (nPos.Block == (int)BlockValue.Air)
                        {
                            if (this[nPos.x, nPos.y - 1, nPos.z] == (int)BlockValue.Air)
                            {
                                this[nPos.x, nPos.y - 1, nPos.z] = (int)BlockValue.Water;
                                resState3 = 0;
                                SetState(nPos.x, nPos.y, nPos.z, GetNumAirNeighbors(nPos.x, nPos.y, nPos.z) + 1, 3); // +1 because we are now air instead of water
                                return (int)BlockValue.Air;
                            }
                            else if (this[nPos2.x, nPos2.y - 1, nPos2.z] == (int)BlockValue.Air)
                            {
                                this[nPos2.x, nPos2.y - 1, nPos2.z] = (int)BlockValue.Water;
                                resState3 = 0;
                                SetState(nPos2.x, nPos2.y - 1, nPos2.z, GetNumAirNeighbors(nPos2.x, nPos2.y - 1, nPos2.z) + 1, 3); // +1 because we are now air instead of water
                                return (int)BlockValue.Air;
                            }
                        }
                    }
                }
                int prevNumAirNeighbors = state3;
                int curNumAirNeighbors = GetNumAirNeighbors(wx, wy, wz);
                resState3 = curNumAirNeighbors;
                // if we have a more air neighbors, flood fill back and set valid blocks that have WATER_NOFLOW back to WATER so they can try again
                if (curNumAirNeighbors != prevNumAirNeighbors)
                {
                    LVector3 airNeighbor = new LVector3(wx, wy, wz);
                    foreach (LVector3 neighbor in AllNeighborsRelative(new LVector3(wx, wy, wz)))
                    {
                        if (neighbor.Block == (int)BlockValue.Air)
                        {
                            airNeighbor = neighbor;
                            break;
                        }
                    }
                    numBlockUpdatesThisTick += 1;

                    // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                    //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    {
                        return (b == (int)BlockValue.Water || b == (int)BlockValue.WaterNoFlow);
                    },
                       isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                       {
                           if (b == (int)BlockValue.WaterNoFlow && IsWater(this[bx, by - 1, bz]) && airNeighbor.y < by)
                           {
                               this[bx, by, bz] = (int)BlockValue.Air;
                               return true;
                           }
                           return false;
                       }
                    ))
                    {

                        this[airNeighbor.x, airNeighbor.y, airNeighbor.z] = (int)BlockValue.Water;
                        SetState(airNeighbor.x, airNeighbor.y, airNeighbor.z, GetNumAirNeighbors(airNeighbor.x, airNeighbor.y, airNeighbor.z), 3);
                        resState3 = curNumAirNeighbors - 1; // we just replaced an air neighbor with water
                        needsAnotherUpdate = true;
                        return (int)BlockValue.Water;
                    }
                    else
                    {
                        needsAnotherUpdate = true;
                        return (int)BlockValue.WaterNoFlow;
                    }
                }

                return block;
            }

        }


        if (!World.maxCapacities.ContainsKey(block))
        {
            return block;
        }
        if (state1 > 1)
        {
            if (block == (int)BlockValue.Air)
            {
                //Debug.Log("bad " + block + " " + state1);
                resState1 = 0;
                return block;
            }
            else
            {
                needsAnotherUpdate = true;
                resState1 = state1 - 1;
                return block;
            }
        }

        int supportPower = state2;
        if (block == (int)BlockValue.Bedrock)
        {
            supportPower = maxCapacities[(int)BlockValue.Bedrock];
        }
        else if (block != (int)BlockValue.Air)
        {
            int greatestNeighborSupportPower = 0;
            if (this[wx, wy - 1, wz] != (int)BlockValue.Air)
            {
                int belowSupportPower = GetState(wx, wy - 1, wz, 2);
                greatestNeighborSupportPower = TrickleSupportPowerUp(this[wx, wy - 1, wz], belowSupportPower, block);
            }
            foreach (LVector3 neighbor in AllNeighborsExceptDown())
            {
                int neighborBlock = this[wx + neighbor.x, wy + neighbor.y, wz + neighbor.z];
                int neighborSupportPower = GetState(wx + neighbor.x, wy + neighbor.y, wz + neighbor.z, 2);
                int trickleSupportPower = TrickleSupportPowerSidewaysOrDown(neighborBlock, neighborSupportPower, block);
                //Debug.Log("neighbor " + neighbor + " has support power " + neighborSupportPower + " and is block " + neighborBlock + " and trickled " + trickleSupportPower + " support power to me");
                if (trickleSupportPower > greatestNeighborSupportPower)
                {
                    greatestNeighborSupportPower = trickleSupportPower;
                }
            }
            //Debug.Log("updating power from neighbors " + block + " with support power " + supportPower + " and greatest neighbor power " + greatestNeighborSupportPower);
            if (supportPower != greatestNeighborSupportPower)
            {
                needsAnotherUpdate = true;
                AddBlockUpdateToNeighbors(wx, wy, wz);
                supportPower = greatestNeighborSupportPower;
                //Debug.Log("is new value");
            }
            else
            {
                needsAnotherUpdate = false;
                //Debug.Log("is not new value");
            }
        }

        if (state1 == 1)
        {
            supportPower = 0;
        }

        resState2 = supportPower;

        if (supportPower <= 0 && this[wx, wy-1, wz] == (int)BlockValue.Air)
        {
            Debug.Log("rip me support power is not good enough and I have air below");
            this[wx, wy - 1, wz] = block;
            SetState(wx, wy - 1, wz, 2, 1); // don't update again until next tick
            resState1 = 0;
            resState2 = 0;
            needsAnotherUpdate = true;
            AddBlockUpdateToNeighbors(wx, wy, wz);
            return (int)BlockValue.Air;
        }
        else
        {
            resState1 = 0;
        }

        if (block == (int)BlockValue.Grass)
        {
            float prGrass = 0.005f;
            //Debug.Log("updating grass block " + wx + " " + wy + " " + wz + " " + block + " " + state);
            if (this[wx, wy + 1, wz] == (int)BlockValue.Air)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (this[wx + 1, wy+y, wz] == (int)BlockValue.Dirt && this[wx + 1, wy + y+1, wz] == (int)BlockValue.Air)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx + 1, wy + y, wz] = (int)BlockValue.Grass;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx - 1, wy + y, wz] == (int)BlockValue.Dirt && this[wx - 1, wy + y + 1, wz] == (int)BlockValue.Air)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx - 1, wy + y, wz] = (int)BlockValue.Grass;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx, wy + y, wz + 1] == (int)BlockValue.Dirt && this[wx, wy + y + 1, wz+1] == (int)BlockValue.Air)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx, wy + y, wz + 1] = (int)BlockValue.Grass;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx, wy + y, wz - 1] == (int)BlockValue.Dirt && this[wx, wy + y + 1, wz-1] == (int)BlockValue.Air)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx, wy + y, wz - 1] = (int)BlockValue.Grass;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }
                }
                //Debug.Log("updating grass block " + needsAnotherUpdate + " <- needs update? with air above " + wx + " " + wy + " " + wz + " " + block + " " + state);
                return (int)BlockValue.Grass;
            }
            else
            {
                return (int)BlockValue.Dirt;
            }
        }
        else if (block == (int)BlockValue.Sand)
        {
            if (state1 <= 0)
            {
                // if air below, fall
                if (this[wx, wy - 1, wz] == (int)BlockValue.Air)
                {
                    this[wx, wy - 1, wz] = (int)BlockValue.Sand;
                    SetState(wx, wy - 1, wz, 1, 1); // don't update again until next tick
                    resState1 = 0; 
                    needsAnotherUpdate = true;
                    return (int)BlockValue.Air;
                }
                // block below, don't fall
                else
                {
                    resState1 = 0;
                    needsAnotherUpdate = false;
                    return (int)BlockValue.Sand;
                }
            }
            // we already moved this tick, set our state to zero so we can try moving again next tick
            else
            {
                needsAnotherUpdate = true;
                resState1 = state1 - 1;
                return (int)BlockValue.Sand;
            }
        }
        else
        {
            return block;
        }
        */
        //}
        // fixes issues with not doing floor.
        // for example,
        // 5/2 = 2.5, but is truncated towards zero to 2
        // -5/2 = -2.5, but is truncated towards zero to -2
        // what I actually want is a "floor" behavior, so
        // 5/2 = 2
        // -5/2 = -3
        // this can be achieved by simply adding -1 whenever a % b != 0, and one is negative and the other isn't
        // this is used for breaking the world into cells. Consider cells of size 2 with
        // -6,-5,-4,-3,-2,-1, 0, 1, 2, 3, 4, 5,
        // -3,-3,-2,-2,-1,-1, 0, 0, 1, 1, 2, 2,
        // which is correct because we used this function, if instead we just used default int rounding, we would get
        // -6,-5,-4,-3,-2,-1, 0, 1, 2, 3, 4, 5,
        // -3,-2,-2,-1,-1, 0, 0, 0, 1, 1, 2, 2,
        // which puts -1,0, and 1 into the same group, yet the groups are supposed to only be of size 2.
        // I didn't want to convert to a float or double and floor because that can lead to precision issues
        // here we assume b is never less than 0

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long divWithFloorForChunkSize(long a)
        {
            return a / chunkSize - ((a < 0 && a % chunkSize != 0) ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long divWithFloor(long a, long b)
        {
            return a / b - (((a < 0 == b < 0) || a % b == 0) ? 0 : 1);
            /*
            if (a % b == 0 || sa == sb)
            {
                return a / b;
            }
            else
            {
                 return a / b - 1; // if a and b differ by a sign, this rounds up, round down instead (in other words, always floor)
            }
            */
        }

        long divWithCeil(long a, long b)
        {
            if (a % b == 0)
            {
                return a / b;
            }
            else
            {
                if ((a < 0 && b < 0) || (a > 0 && b > 0))
                {
                    return a / b + 1; // if a and b have the same, this rounds down, round up instead (in other words, always ceil)
                }
                else
                {
                    return a / b;
                }
            }
        }


        public int this[LVector3 pos]
        {
            get
            {
                return this[pos.x, pos.y, pos.z];
            }
            set
            {
                this[pos.x, pos.y, pos.z] = value;
            }
        }
        public override int this[long x, long y, long z, long cx, long cy, long cz]
        {
            get
            {
                Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
                return chunk[x, y, z];
            }

            set
            {
                blockModifyState += 1;
                Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
                chunk[x, y, z] = value;
            }
        }



        public Chunk GetOrGenerateChunk(long chunkX, long chunkY, long chunkZ)
        {
            Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
            if (chunk == null)
            {
                chunk = GenerateChunk(chunkX, chunkY, chunkZ, checkIfExists: false); // TODO: best test here is to call GetChunk(...) after generating it to see if it returns it properly
            }
            return chunk;
        }

        Chunk lastChunk;
        Chunk lastChunkMainThread;


        public Chunk GetChunk(long chunkX, long chunkY, long chunkZ)
        {
            Chunk lastChunkCache = lastChunk;
            if (Thread.CurrentThread != helperThread)
            {
                lastChunkCache = lastChunkMainThread;
            }
            if (lastChunkCache != null && lastChunkCache.cx == chunkX && lastChunkCache.cy == chunkY && lastChunkCache.cz == chunkZ)
            {
                return lastChunkCache;
            }
            Chunk res = GetChunk(new long[] { chunkX, chunkY, chunkZ });
            if (res != null)
            {
                if (Thread.CurrentThread != helperThread)
                {
                    lastChunkMainThread = res;
                }
                else
                {
                    lastChunk = res;
                }
            }
            return res;
        }
        static int numWaiting = 0;

        public Thread helperThread;

        Chunk GetChunkf(long[] pos)
        {
            bool isMainBoi = Thread.CurrentThread != helperThread;
            try
            {
                if (isMainBoi)
                {
                    Interlocked.Increment(ref numWaiting);
                }

                for (; ; )
                {
                    lock (chunkAddLock)
                    {
                        if (isMainBoi || numWaiting == 0)
                        {
                            return GetChunk(pos);
                        }
                    }
                    // Sleep gives other threads a chance to enter the lock
                    Thread.Sleep(0);
                }
            }
            finally
            {
                if (isMainBoi)
                {
                    Interlocked.Decrement(ref numWaiting);
                }
            }
        }

        Chunk GetChunk(long[] pos)
        {
            for (int i = 0; i < chunkCache.Count-1; i++)
            {
                Chunk chunk = chunkCache[i];
                if (chunk != null && chunk.cx == pos[0] && chunk.cy == pos[1] && chunk.cz == pos[2])
                {
                    return chunk;
                }
            }

            long minDim = -1;
            long minNumInDict = long.MaxValue;
            for (int d = 0; d < DIM; d++)
            {
                if (chunksPer[d].ContainsKey(pos[d]))
                {
                    long numInDict = chunksPer[d][pos[d]].Count;
                    if (numInDict < minNumInDict)
                    {
                        minNumInDict = numInDict;
                        minDim = d;
                    }
                }
            }
            if (minDim == -1)
            {
                return null;
            }
            else
            {
                List<Chunk> chunksHere = chunksPer[minDim][pos[minDim]];
                for(int i = 0; i < chunksHere.Count; i++)
                {
                    Chunk chunk = chunksHere[i];
                    if (chunk.cx == pos[0] && chunk.cy == pos[1] && chunk.cz == pos[2])
                    {
                        if (Thread.CurrentThread == helperThread)
                        {
                            while (chunkCache.Count >= cacheSize)
                            {
                                chunkCache.RemoveAt(0);
                            }
                            chunkCache.Add(chunk);
                        }
                        return chunk;
                    }
                    /*
                    bool failed = false;
                    for (int d = 0; d < DIM; d++)
                    {
                        if (chunk.chunkPos[d] != pos[d])
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (!failed)
                    {
                        return chunk;
                    }
                    */
                }
                return null;
            }
        }

        public Chunk GenerateChunk(long chunkX, long chunkY, long chunkZ, bool checkIfExists = true)
        {
            if (checkIfExists)
            {
                Chunk existingChunk = GetChunk(chunkX, chunkY, chunkZ);
                if (existingChunk != null)
                {
                    return existingChunk;
                }
            }
            //lock(chunkAddLock)
            //{
                Chunk res = new Chunk(this, chunkProperties, chunkX, chunkY, chunkZ, chunkSize, createStuff: true);
                AddChunkToDataStructures(res);
                return res;
            //}
        }

        // fixes issues with a being negative giving negative answers, this is the mathematical mod I prefer
        public static int GoodMod(int a, int b)
        {
            int res = a % b;
            if (res < 0)
            {
                res = b + res;
            }
            return res;
        }


        // fixes issues with a being negative giving negative answers, this is the mathematical mod I prefer
        public static long GoodMod(long a, long b)
        {
            long res = a % b;
            if (res < 0)
            {
                res = b + res;
            }
            return res;
        }

        public object chunkAddLock = new object();
        int off = 0;
        void AddChunkToDataStructures(Chunk chunk)
        {
            lowestCy = System.Math.Min(chunk.cy, lowestCy);

            //Debug.Log("adding chunk " + chunk.cx + " " + chunk.cy + " " + chunk.cz + " " + Time.frameCount);
            long[] curPos = new long[] { chunk.cx, chunk.cy, chunk.cz };


            for (int d = 0; d < DIM; d++)
            {
                if (!chunksPer[d].ContainsKey(chunk.GetPos(d)))
                {
                    chunksPer[d][chunk.GetPos(d)] = new List<Chunk>();
                }

                chunksPer[d][chunk.GetPos(d)].Add(chunk);


                // link to node before
                curPos[d] = chunk.GetPos(d) - 1;
                Chunk beforeChunk = GetChunk(curPos);
                if (beforeChunk != null)
                {
                    beforeChunk.posChunks[d] = chunk;
                    chunk.negChunks[d] = beforeChunk;
                }


                // link to node after
                curPos[d] = chunk.GetPos(d) + 1;
                Chunk afterChunk = GetChunk(curPos);
                if (afterChunk != null)
                {
                    afterChunk.negChunks[d] = chunk;
                    chunk.posChunks[d] = afterChunk;
                }

                // reset back to chunk pos so next dim can use this array as well
                curPos[d] = chunk.GetPos(d);
            }
            allChunks.Add(chunk);

            LVector3 chunkPosVec = new LVector3(chunk.cx, chunk.cy, chunk.cz);
            if (ungeneratedChunkBiomeDatas.ContainsKey(chunkPosVec))
            {
                chunk.chunkBiomeData = ungeneratedChunkBiomeDatas[chunkPosVec];
                ungeneratedChunkBiomeDatas.Remove(chunkPosVec);
            }
            /*
            List<Structure> leftoverStructures = new List<Structure>();
            foreach (Structure structure in unfinishedStructures)
            {
                if (!structure.AddNewChunk(chunk))
                {
                    leftoverStructures.Add(structure);
                }
            }

            unfinishedStructures = leftoverStructures;


            */
            return;
            /*
            int loopMax = 2;

            Chunk otherLoop = chunk;

            long otherLoopCx, otherLoopCy, otherLoopCz;
            PhysicsUtils.ModChunkPos(chunk.cx, chunk.cy, chunk.cz, out otherLoopCx, out otherLoopCy, out otherLoopCz);

            if (otherLoopCx != chunk.cx || otherLoopCy != chunk.cy || otherLoopCz != chunk.cz)
            {
                otherLoop = GetOrGenerateChunk(otherLoopCx, otherLoopCy, otherLoopCz);
            }
            */

            /*

            if (otherLoop.cx > loopMax)
            {
                otherLoop = GetOrGenerateChunk(-loopMax, otherLoop.cy, otherLoop.cz);
            }

            if (otherLoop.cx < -loopMax)
            {
                otherLoop = GetOrGenerateChunk(loopMax, otherLoop.cy, otherLoop.cz);
            }

            if (otherLoop.cz > loopMax)
            {
                otherLoop = GetOrGenerateChunk(otherLoop.cx, otherLoop.cy, -loopMax);
            }

            if (otherLoop.cz < -loopMax)
            {
                otherLoop = GetOrGenerateChunk(otherLoop.cx, otherLoop.cy, loopMax);
            }
            */
            /*
            if (otherLoop.cx != chunk.cx || otherLoop.cy != chunk.cy || otherLoop.cz != chunk.cz)
            {
                chunk.chunkData = otherLoop.chunkData;
                chunk.chunkRenderer = otherLoop.chunkRenderer;
                otherLoop.chunkData.attachedChunks.Add(chunk);
                chunk.generating = false;
                chunk.valid = false;
            }
            else
            {
                chunk.CreateStuff();
            }
            */
            /*
            int initialCount = 0;
            // fun code that makes chunks repeat around in a very non-euclidean way
            if (allChunks.Count > initialCount+8)
            {
                if (chunk.cy > -3)
                {
                    chunk.chunkData.Dispose();
                    chunk.chunkRenderer.Dispose();
                    chunk.chunkData = allChunks[off + initialCount].chunkData;
                    chunk.chunkRenderer = allChunks[off + initialCount].chunkRenderer;
                    chunk.generating = false;
                    off = (off + 1) % 8;
                    Debug.Log("changing, off = " + (off + initialCount));
                    return;
                }
            }
            else
            {
                Debug.Log("not changing size=" + allChunks.Count);
            }
            */


        }

        public Chunk GetOrGenerateChunkAtPos(long x, long y, long z)
        {
            long chunkX = divWithFloorForChunkSize(x);
            long chunkY = divWithFloorForChunkSize(y);
            long chunkZ = divWithFloorForChunkSize(z);
            Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }

        public void GetChunkCoordinatesAtPos(LVector3 worldPos, out LVector3 chunkPos)
        {
            long chunkX = divWithFloorForChunkSize(worldPos.x);
            long chunkY = divWithFloorForChunkSize(worldPos.y);
            long chunkZ = divWithFloorForChunkSize(worldPos.z);
            chunkPos = new LVector3(chunkX, chunkY, chunkZ);
        }


        public void GetChunkCoordinatesAtPos(long x, long y, long z, out long cx, out long cy, out long cz)
        {
            cx = divWithFloorForChunkSize(x);
            cy = divWithFloorForChunkSize(y);
            cz = divWithFloorForChunkSize(z);
        }


        public void GetChunkCoordinatesAtPos(long x, long y, long z, out LVector3 chunkPos)
        {
            long chunkX = divWithFloorForChunkSize(x);
            long chunkY = divWithFloorForChunkSize(y);
            long chunkZ = divWithFloorForChunkSize(z);
            chunkPos = new LVector3(chunkX, chunkY, chunkZ);
        }

        public Chunk GetChunkAtPos(long x, long y, long z)
        {
            long chunkX = divWithFloorForChunkSize(x);
            long chunkY = divWithFloorForChunkSize(y);
            long chunkZ = divWithFloorForChunkSize(z);
            Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }


        public void AddBlockUpdate(long i, long j, long k, bool alsoToNeighbors = false)
        {
            long chunkX = divWithFloorForChunkSize(i);
            long chunkY = divWithFloorForChunkSize(j);
            long chunkZ = divWithFloorForChunkSize(k);
            Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
            if (chunk != null && !chunk.generating)
            {
                chunk.AddBlockUpdate(i, j, k);
            }
            //GetOrGenerateChunk(divWithFloorForChunkSize(i), divWithFloorForChunkSize(j), divWithFloorForChunkSize(k)).AddBlockUpdate(i, j, k);
            if (alsoToNeighbors)
            {
                AddBlockUpdateToNeighbors(i, j, k);
            }
        }


        public void AddBlockUpdateToNeighbors(long i, long j, long k)
        {
            AddBlockUpdate(i - 1, j, k, alsoToNeighbors: false);
            AddBlockUpdate(i + 1, j, k, alsoToNeighbors: false);
            AddBlockUpdate(i, j - 1, k, alsoToNeighbors: false);
            AddBlockUpdate(i, j + 1, k, alsoToNeighbors: false);
            AddBlockUpdate(i, j, k - 1, alsoToNeighbors: false);
            AddBlockUpdate(i, j, k + 1, alsoToNeighbors: false);
        }


        bool cleanedUp = false;
        public void Dispose()
        {
            if (!cleanedUp)
            {
                cleanedUp = true;
                if (argBuffer != null)
                {
                    argBuffer.Dispose();
                    argBuffer = null;
                }

                foreach (KeyValuePair<long, List<Chunk>> chunkList in chunksPer[0])
                {
                    foreach (Chunk chunk in chunkList.Value)
                    {
                        chunk.Dispose();
                    }
                }
                for (int i = 0; i < chunksPer.Length; i++)
                {
                    chunksPer[i].Clear();
                }
            }
        }

        public void Render()
        {
            int numAllowedToDoFullRender = 8;
            // render non-transparent
            foreach (Chunk chunk in allChunks)
            {
                chunk.chunkRenderer.Render(false, chunk, ref numAllowedToDoFullRender);
            }
            // render transparent
            foreach (Chunk chunk in allChunks)
            {
                chunk.chunkRenderer.Render(true, chunk, ref numAllowedToDoFullRender);
            }
        }

        public static void Shuffle<T>(List<T> arr)
        {
            for (int i = 1; i < arr.Count; i++)
            {
                int toPos = Random.Range(0, i + 1);
                if (toPos != i)
                {
                    T tmp = arr[i];
                    arr[i] = arr[toPos];
                    arr[toPos] = tmp;
                }
            }
        }

        long frameId = 0;
        public long frameTimeStart = 0;
        public void Tick()
        {
            frameId += 1;
            foreach (KeyValuePair<BlockValue, BlockOrItem> block in customBlocks)
            {
                block.Value.OnTickStart();
            }
            // what direction is checked first: goes in a weird order after that
            globalPreference = (globalPreference + 1) % 4;
            List<Chunk> allChunksHere = new List<Chunk>();
            for (int i = 0; i < allChunks.Count; i++)
            {
                allChunksHere.Add(allChunks[i]);
            }

            // Randomly shuffle the order we update the chunks in, for the memes when they are tiled procedurally
            //Shuffle(allChunksHere);

            frameTimeStart = PhysicsUtils.millis();
            numBlockUpdatesThisTick = 0;
            numWaterUpdatesThisTick = 0;




            int numGenerated = 0;
            int maxGenerating = 1000;
            int maxTickSteps = 10000000;

            bool allowGenerate = true;

            /*
            foreach (BlocksPlayer player in GameObject.FindObjectsOfType<BlocksPlayer>())
            {
                LVector3 playerPos = LVector3.FromUnityVector3(player.transform.position);

                Chunk playerChunk = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z);
                RunTick(playerChunk, true, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkBelow = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y-chunkSize, playerPos.z);
                RunTick(playerChunkBelow, true, ref maxTickSteps, ref numGenerated, maxGenerating);


                Chunk playerChunkAbove = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y + chunkSize, playerPos.z);
                RunTick(playerChunkAbove, true, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkX = GetOrGenerateChunkAtPos(playerPos.x + chunkSize, playerPos.y, playerPos.z);
                RunTick(playerChunkX, true, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkX2 = GetOrGenerateChunkAtPos(playerPos.x - chunkSize, playerPos.y, playerPos.z);
                RunTick(playerChunkX2, true, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkZ = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z+chunkSize);
                RunTick(playerChunkZ, true, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkZ2 = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z- chunkSize);
                RunTick(playerChunkZ2, true, ref maxTickSteps, ref numGenerated, maxGenerating);
            }
            */


            long frameTimeStart2 = PhysicsUtils.millis();
            if (frameId > 100 || blocksWorld.optimize)
            {
                maxTickSteps = 10000000;
                maxGenerating = 10000000;
                if (frameId % 2 != 0)
                {
                    maxGenerating = 0;
                }
            }
            int chunkI = 0;
            bool bailed = false;
            for (int i = 0; i < allChunksHere.Count; i++)
            {
                Chunk chunk = allChunksHere[i];
                if (!chunk.generating && chunk.chunkData.blocksNeedUpdatingNextFrame.Count == 0)
                {
                    continue;
                }
                if (maxGenerating == 0)
                {
                    allowGenerate = false;
                }
                if (PhysicsUtils.millis() - frameTimeStart > maxTickSteps)
                {
                    //Debug.Log("bailed on chunk " + (chunkI + 1) + "/" + allChunksHere.Count);
                    bailed = true;
                    break;
                }
                chunkI += 1;
                RunTick(chunk, allowGenerate, ref maxTickSteps, ref numGenerated, maxGenerating);
            }
            if (!bailed)
            {
                //Debug.Log("did not bail " + frameId);
            }
        }


        public void RunTick(Chunk chunk, bool allowGenerate, ref int maxTickSteps, ref int numGenerated, int maxGenerating)
        {
            chunk.TickStart(frameId);
            if (chunk.Tick(allowGenerate, ref maxTickSteps))
            {
                numGenerated += 1;
                if (numGenerated > maxGenerating)
                {
                    allowGenerate = false;
                }
                List<Structure> leftoverStructures = new List<Structure>();
                Priority_Queue.SimplePriorityQueue<Structure, int> chunkStructures = new Priority_Queue.SimplePriorityQueue<Structure, int>();
                foreach (Structure structure in unfinishedStructures)
                {
                    if (structure.CanAddToChunk(chunk))
                    {
                        chunkStructures.Enqueue(structure, structure.priority);
                    }
                    else
                    {
                        leftoverStructures.Add(structure);
                    }
                }

                foreach (Structure structure in chunkStructures)
                {
                    if (!structure.AddNewChunk(chunk))
                    {
                        leftoverStructures.Add(structure);
                    }
                }

                World.mainWorld.blocksTouchingSky.GeneratedChunk(chunk);


                unfinishedStructures = leftoverStructures;

            }
            if (chunk.chunkData.needToBeUpdated)
            {
                //chunk.chunkRenderer.Render(false, chunk, false);
            }
        }
    }

    public class IntegerSet
    {
        int[] items;
        bool[] contained;
        int count = 0;
        public int Count
        {
            get
            {
                return count;
            }
            private set
            {

            }
        }
        public IntegerSet(int maxVal)
        {
            items = new int[maxVal];
            contained = new bool[maxVal];
        }

        public void Add(int val)
        {
            if (!contained[val])
            {
                contained[val] = true;
                items[count] = val;
                count++;
            }
        }

        public int this[int i]
        {
            get
            {
                return items[i];
            }
            private set
            {

            }
        }

        public void Clear()
        {
            if (count > 0)
            {

                count = 0;
                System.Array.Clear(contained, 0, contained.Length);
            }
        }
    }


    [System.Serializable]
    public class Pack
    {
        public string packName;
        public string packRootDir;
        public List<PackBlock> packBlocks;
        public Texture2D packTexture;

        public Pack(string directory)
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            packBlocks = new List<PackBlock>();
            if (info.Exists)
            {
                packRootDir = info.FullName;
                packName = info.Name;
                Debug.Log("---processing pack " + packName + " with root directory " + packRootDir);
                string[] pDirectories = Directory.GetDirectories(packRootDir, "*", SearchOption.TopDirectoryOnly);
                foreach (string pDirectory in pDirectories)
                {
                    DirectoryInfo pInfo = new DirectoryInfo(pDirectory);
                    if (pInfo.Exists)
                    {
                        if (pInfo.Name == "blocks")
                        {
                            Debug.Log("got blocks directory " + pInfo.FullName + " with name " + pInfo.Name);
                            string[] blockDirectories = Directory.GetDirectories(pInfo.FullName, "*", SearchOption.TopDirectoryOnly);
                            foreach (string blockDirectory in blockDirectories)
                            {
                                DirectoryInfo binfo = new DirectoryInfo(blockDirectory);
                                if (binfo.Exists)
                                {
                                    Debug.Log("got block directory " + binfo.FullName + " with name " + binfo.Name);
                                    PackBlock packBlock = new PackBlock(binfo.FullName);
                                    if (packBlock.isValid)
                                    {
                                        packBlocks.Add(packBlock);
                                    }
                                    else
                                    {
                                        Debug.LogError("pack " + packName + " at path " + packRootDir + " could not make block at path " + binfo.FullName + " with name " + binfo.Name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("got other directory " + directory + " in pack with name " + info.Name);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("error: pack directory " + directory + " does not exist");
            }

            if (packBlocks.Count > 0)
            {
                Debug.Log("got more than 1 block, making pack texture");
                MakePackTexture();
            }
        }

        void MakePackTexture()
        {

            string assetsPath = Application.dataPath;
            // strip any trailing path seperators
            while (assetsPath[assetsPath.Length - 1] == '/' || assetsPath[assetsPath.Length - 1] == '\\')
            {
                assetsPath = assetsPath.Substring(0, assetsPath.Length - 1);
            }
            // append pack textures directory
            assetsPath = assetsPath.Replace("\\", "/") + "/PackTextures";

            DirectoryInfo assetsPathInfo = new DirectoryInfo(assetsPath);
            if (!assetsPathInfo.Exists)
            {
                Directory.CreateDirectory(assetsPathInfo.FullName);
            }
            // append directory for this pack
            string packAssetsPath = assetsPath + "/" + packName;

            DirectoryInfo packAssetsPathInfo = new DirectoryInfo(packAssetsPath);
            if (!packAssetsPathInfo.Exists)
            {
                Directory.CreateDirectory(packAssetsPathInfo.FullName);
            }



            packTexture = new Texture2D(16 * 2, 16 * 3 * 64, TextureFormat.ARGB32, false, true);
            Color32[] colors = new Color32[packTexture.width * packTexture.height];
            for (int j = 0; j < colors.Length; j++)
            {
                colors[j] = new Color32(255, 255, 255, 255);
            }
            packTexture.SetPixels32(colors);
            packTexture.Apply();
            packTexture.wrapMode = TextureWrapMode.Clamp;
            packTexture.filterMode = FilterMode.Point;
            int startOffset = 2;
            int i = startOffset; // start offset since 0 is empty and -1 is wildcard
            foreach (PackBlock block in packBlocks)
            {
                if (block.isAnimated)
                {
                    for (int k = 0; k < block.blockAnimationImagePaths.Length; k++)
                    {
                        string curAnimationImagePath = block.blockAnimationImagePaths[k];
                        if (curAnimationImagePath != "")
                        {
                            byte[] imageData = File.ReadAllBytes(curAnimationImagePath);
                            Texture2D blockTexture = new Texture2D(2, 2);
                            blockTexture.LoadImage(imageData); // (will automatically resize as needed)
                            blockTexture.Apply();
                            Texture2D argbTexture = new Texture2D(blockTexture.width, blockTexture.height, TextureFormat.ARGB32, false, true);
                            argbTexture.SetPixels(blockTexture.GetPixels());
                            argbTexture.Apply();

                            // write texture
                            string blockPath = packAssetsPath + "/" + block.blockName + "" + k + ".png";
                            File.WriteAllBytes(blockPath, argbTexture.EncodeToPNG());
                        }
                    }
                }
                else
                {

                    byte[] imageData = File.ReadAllBytes(block.blockImagePath);
                    Texture2D blockTexture = new Texture2D(2, 2);
                    blockTexture.LoadImage(imageData); // (will automatically resize as needed)
                    blockTexture.Apply();
                    Texture2D argbTexture = new Texture2D(blockTexture.width, blockTexture.height, TextureFormat.ARGB32, false, true);
                    argbTexture.SetPixels(blockTexture.GetPixels());
                    argbTexture.Apply();

                    // write texture
                    string blockPath = packAssetsPath + "/" + block.blockName + ".png";
                    File.WriteAllBytes(blockPath, argbTexture.EncodeToPNG());
                    // delete meta file so unity will reload it
                    //if (File.Exists(blockPath + ".meta"))
                    //{
                    //    File.Delete(blockPath + ".meta");
                    //}


                    Color32[] argbColors = argbTexture.GetPixels32();
                    // check for transparency
                    block.isTransparent = false;
                    for (int j = 0; j < argbColors.Length; j++)
                    {
                        if (argbColors[j].a < 240) // if it is only 240/255 or higher it isn't noticable enough to actually use the transparency
                        {
                            block.isTransparent = true;
                            break;
                        }
                    }
                    if (argbTexture.width != 16 * 2 || argbTexture.height != 16 * 3)
                    {
                        Debug.Log("rescaling texture of block " + block.blockName + " at path " + block.blockRootDir + " with block image path " + block.blockImagePath);
                        TextureScale.Bilinear(argbTexture, 16 * 2, 16 * 3);
                    }

                    argbTexture.Apply();
                    Color[] pixels = argbTexture.GetPixels();
                    packTexture.SetPixels(0, i * 16 * 3, 16 * 2, 16 * 3, pixels);
                    packTexture.Apply();
                }


                i += 1;
            }

            packTexture.alphaIsTransparency = true;
            string assetsPathDir = assetsPathInfo.FullName;

            /*
            File.WriteAllBytes(assetsPathDir + "/" + packName + ".png", packTexture.EncodeToPNG());

            // delete meta file so unity will reload it
            if (File.Exists(assetsPathDir + "/" + packName + ".png.meta"))
            {
                File.Delete(assetsPathDir + "/" + packName + ".png.meta");
            }
            */

            string exampleThings =
    "using Blocks;\r\n\r\nnamespace " + packName + @"_pack {
    public class " + packName + @" : BlockCollection
    {
";
            string afterExampleThings = @"
    }
";
            for (int b = 0; b < packBlocks.Count; b++)
            {
                //int index = b + startOffset + 1;


                if (packBlocks[b].isTransparent)
                {
                    exampleThings += "        public static BlockValue " + packBlocks[b].blockName + " = new BlockValue(true, '" + packBlocks[b].blockName + "', '" + packName + "');";
                }
                else
                {
                    exampleThings += "        public static BlockValue " + packBlocks[b].blockName + " = new BlockValue(false, '" + packBlocks[b].blockName + "', '" + packName + "');";
                }
                if (b != packBlocks.Count - 1)
                {
                    exampleThings += "\n";
                }
            }

            exampleThings = exampleThings.Replace("'", "\"");

            exampleThings += afterExampleThings + "\r\n}";

            File.WriteAllText(assetsPathDir + "/" + packName + "Pack" + " .cs", exampleThings);

            // delete meta file so unity will reload it
            if (File.Exists(assetsPathDir + "/" + packName + "Pack" + " .cs.meta"))
            {
                File.Delete(assetsPathDir + "/" + packName + "Pack" + " .cs.meta");
            }


            /*
            string exampleThings =
    "namespace " + packName + @"_pack {
        public class BlockValue
        {

            public static implicit operator BlockValue(int x)
            {
                return GetBlockValue(x);
            }

            public static implicit operator int(BlockValue x)
            {
                return x.id;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj is BlockValue)
                {
                    BlockValue item = (BlockValue)obj;
                    return Equals(item);
                }
                else
                {
                    return id.Equals(obj);
                }
            }

            public bool Equals(BlockValue other)
            {
                return this.id == other.id;
            }

            // see https://msdn.microsoft.com/en-us/library/ms173147.aspx
            public static bool operator ==(BlockValue a, BlockValue b)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(a, b))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return a.Equals(b);
            }


            public static bool operator !=(BlockValue a, BlockValue b)
            {
                return !(a == b);
            }

            public static bool operator ==(BlockValue a, int b)
            {
                if ((object)a == null)
                {
                    return false;
                }
                return a.id == b;
            }


            public static bool operator !=(BlockValue a, int b)
            {
                return !(a == b);
            }



            public static bool operator ==(int a, BlockValue b)
            {
                if ((object)b == null)
                {
                    return false;
                }
                return a == b.id;
            }


            public static bool operator !=(int a, BlockValue b)
            {
                return !(a == b);
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }

            int _id;
            public int id { get { return _id; } private set { } }
            BlockValue(int id)
            {
                if (allBlocks == null)
                {
                    allBlocks = new BlockValue[255];
                }
                int uid = System.Math.Abs(id);
                // increase number of blocks until we have enough
                while (allBlocks.Length <= uid)
                {
                    BlockValue[] newAllBlocks = new BlockValue[allBlocks.Length * 2];
                    for (int i = 0; i < allBlocks.Length; i++)
                    {
                        newAllBlocks[i] = allBlocks[i];
                    }
                    allBlocks = newAllBlocks;
                }
                if (allBlocks[uid] == null)
                {
                    UnityEngine.Debug.LogWarning('warning: multiple blocks have id' + uid + '(technically ' + id + ' with transparency flag)');
                }
                allBlocks[uid] = this;

                 _id = id;
            }

            public static BlockValue GetBlockValue(int id)
            {
                int uid = System.Math.Abs(id);
                if (allBlocks[uid] == null)
                {
                    throw new System.ArgumentOutOfRangeException('block value with id ' + uid + ' (technically ' + id + ' with transparency flag) ' + ' does not exist');
                }
                return allBlocks[uid];
            }

            public static BlockValue[] allBlocks;

            public static BlockValue Air = new BlockValue(0);
            public static BlockValue Wildcard = new BlockValue(-1);
    ";
            string afterExampleThings = @"
        }
    ";
            string testFunc = @"
        public class BlockUtils {
            public static string BlockIdToString(int blockId){
                if (blockId < 0){ blockId = -blockId; }
                if (blockId == 0){ return 'Air'; }
                if (blockId == 1){ return 'Wildcard'; }
                if (blockId == 2){ return '?? 2 is invalid'; }
    ";
            string testFunc2 = @"
            public static int StringToBlockId(string blockName){
                if (blockName == 'Air'){ return 0; }
                if (blockName == 'Wildcard'){ return -1; }
    ";
            for (int b = 0; b < packBlocks.Count; b++)
            {
                int index = b + startOffset+1;


                testFunc += "            if(blockId == " + index + "){ return '" + packBlocks[b].blockName + "';}\r\n";


                // we flag a block as transparent by making it's index negative. These negative signs are igored in the shaders, and are just used for choosing a rendering order (transparent blocks aren't rendered until all non-transparent ones are)
                if (packBlocks[b].isTransparent)
                {
                    index = -index;
                }

                testFunc2 += "            if(blockName == '" + packBlocks[b].blockName + "'){ return " + index + ";}\r\n";


                exampleThings += "        public static BlockValue " + packBlocks[b].blockName + " = new BlockValue(" + index + ");";
                if (b != packBlocks.Count - 1)
                {
                    exampleThings += "\n";
                }
            }
            testFunc += @"
                return 'Unknown block id ' + blockId;
            }
            ";

            testFunc2 += @"
                UnityEngine.Debug.LogError('Unknown block name ' + blockName);
                throw new System.ArgumentOutOfRangeException('Unknown block name ' + blockName);
            }
        }
            ";

            // I used single quotes above because it was cleaner in code but double quotes are required for strings in c# so we will replace them here accordingly
            testFunc = testFunc.Replace("'", "\"");
            testFunc2 = testFunc2.Replace("'", "\"");
            exampleThings = exampleThings.Replace("'", "\"");

            exampleThings += afterExampleThings + testFunc + testFunc2  + "\r\n}";

            File.WriteAllText(assetsPathDir + "/" + packName + "Pack" + " .cs", exampleThings);

            // delete meta file so unity will reload it
            if (File.Exists(assetsPathDir + "/" + packName + "Pack" + " .cs.meta"))
            {
                File.Delete(assetsPathDir + "/" + packName + "Pack" + " .cs.meta");
            }
            */
        }
    }


    public class PackBlock
    {
        public string blockName;
        public string blockRootDir;
        public string blockImagePath;
        public bool isValid;
        public bool isTransparent;
        public string[] blockAnimationImagePaths;
        public bool isAnimated;
        public PackBlock(string blockDirectory)
        {
            isValid = false;
            isAnimated = false;
            blockAnimationImagePaths = null;
            DirectoryInfo info = new DirectoryInfo(blockDirectory);
            if (info.Exists)
            {
                blockName = info.Name;
                blockRootDir = info.FullName;
                string[] filesInBlock = Directory.GetFiles(info.FullName, "*", SearchOption.TopDirectoryOnly);
                Debug.Log("parsing block " + blockName + " with root path " + blockRootDir);
                foreach (string blockFile in filesInBlock)
                {
                    FileInfo fInfo = new FileInfo(blockFile);
                    if (fInfo.Exists)
                    {
                        if (fInfo.Extension.ToLower() == ".png" || fInfo.Extension.ToLower() == ".jpg")
                        {
                            Debug.Log("got image for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
                            if (fInfo.Name == blockName + fInfo.Extension)
                            {
                                Debug.Log("image matches block name, using it for block art");
                                blockImagePath = fInfo.FullName;
                                isValid = true;
                                break;
                            }
                        }
                        else
                        {
                            Debug.Log("got non-image for block " + blockName + " with path " + fInfo.FullName + " and name " + fInfo.Name + " and extension " + fInfo.Extension);
                        }
                    }
                }
                if (!isValid)
                {
                    // see if an animated block
                    Dictionary<int, string> frames = new Dictionary<int, string>();
                    int maxFrame = int.MinValue;
                    int minFrame = int.MaxValue;
                    foreach (string blockFile in filesInBlock)
                    {
                        FileInfo fInfo = new FileInfo(blockFile);
                        if (fInfo.Exists)
                        {
                            if (fInfo.Extension.ToLower() == ".png" || fInfo.Extension.ToLower() == ".jpg")
                            {
                                Debug.Log("got image for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
                                if (fInfo.Name.Substring(0, blockName.Length) == blockName) // does the first piece match the block name?
                                {
                                    string leftoverPieces = fInfo.Name.Substring(blockName.Length); // get stuff after blockName
                                    leftoverPieces = leftoverPieces.Substring(0, leftoverPieces.Length - fInfo.Extension.Length); // remove extension
                                    Debug.Log("is potential frame with key " + leftoverPieces + " for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);

                                    int frameNum;
                                    if (int.TryParse(leftoverPieces, out frameNum))
                                    {
                                        Debug.Log("is actual frame with index " + frameNum + " for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
                                        frames[frameNum] = fInfo.FullName;
                                        maxFrame = System.Math.Max(maxFrame, frameNum);
                                        minFrame = System.Math.Min(minFrame, frameNum);
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("got non-image for block " + blockName + " with path " + fInfo.FullName + " and name " + fInfo.Name + " and extension " + fInfo.Extension);
                            }
                        }
                    }
                    int maxFrames = 32;

                    if (minFrame < 0)
                    {
                        Debug.LogError("got animation, but a frame is less than zero (" + minFrame + ") this is invalid");
                    }
                    else if(maxFrame > maxFrames)
                    {
                        Debug.LogError("got animation but a frame has index " + maxFrame + " which is greater than " + (maxFrames-1) + ", only " + maxFrames + " frames total are allowed, first animation frame should be indexed by 0, then 1, 2, ..., " + (maxFrames-1) + "");
                    }
                    else if (frames.Count == 0)
                    {
                        Debug.LogError("could not find any images for block " + blockName + " at path " + blockRootDir + " so we are ignoring this block");
                    }
                    else
                    {
                        Debug.Log("got animated block " + blockName + " with " + (maxFrame+1) + " frames");
                        // frames not filled in will have the default texture assigned and here will just have a empty string for their path by default
                        blockAnimationImagePaths = new string[maxFrame+1];
                        foreach (KeyValuePair<int, string> animFrame in frames)
                        {
                            int index = animFrame.Key;
                            string imagePath = animFrame.Value;
                            blockAnimationImagePaths[index] = imagePath;
                        }
                        isAnimated = true;
                        isValid = true;
                    }
                }
            }
            else
            {
                Debug.LogError("pack block directory " + blockDirectory + " does not exist");
            }
        }
    }

    public class BlocksWorld : MonoBehaviour
    {
        public float skyLightLevel = 1.0f;
        public ComputeShader cullBlocksShader;
        public bool creativeMode = false;
        public bool optimize = false;
        ComputeBuffer cubeNormals;
        ComputeBuffer cubeOffsets;
        ComputeBuffer uvOffsets;
        ComputeBuffer breakingUVOffsets;
        ComputeBuffer linesOffsets;
        Material outlineMaterial;
        public Material blockEntityMaterial;
        public Material blockIconMaterial;
        public Material triMaterial;
        public Material triMaterialWithTransparency;
        public Material breakingMaterial;
        public Transform renderTransform;

        public GameObject blockEntityPrefab;
        public GameObject blockRenderPrefab;
        public GameObject blockRenderCanvas;
        ComputeBuffer drawPositions1;
        ComputeBuffer drawPositions2;
        bool isUsing1 = true;
        ComputeBuffer bufferProcessingWith;
        ComputeBuffer bufferDrawingWith;
        public float worldScale = 0.1f;
        public Camera uiCamera;

        public Pack[] packs;

        public InventoryGui otherObjectInventoryGui;

        public Dictionary<LVector3, Inventory> blockInventories = new Dictionary<LVector3, Inventory>();

        public ComputeBuffer chunkBlockData;

        int[] worldData;

        public const int chunkSize = 8;

        public World world;


        // from https://stackoverflow.com/a/4016511/2924421
        public long millis()
        {
            return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        }

        long lastTick = 0;
        public float ticksPerSecond = 20.0f;


        public ComputeBuffer blockBreakingBuffer;
        public static Mesh blockMesh;

        public float ApplyEpsPerturb(float val, float eps)
        {
            if (val == 0)
            {
                return eps;
            }
            else if (val == 1.0f)
            {
                return 1.0f - eps;
            }
            else
            {
                return val;
            }
        }

        // shifts [0,1] to [eps, 1-eps] so rendering won't show the neighboring textures due to rounding errors
        public Vector3 ApplyEpsPerturb(Vector3 offset, float eps)
        {
            return new Vector3(ApplyEpsPerturb(offset.x, eps), ApplyEpsPerturb(offset.y, eps), ApplyEpsPerturb(offset.z, eps));
        }

        void SetupRendering()
        {
            LoadMaterials();
            float epsPerturb = 1.0f/64.0f;

            float[] texOffsets = new float[36 * 4];
            float[] vertNormals = new float[36 * 4];
            List<Vector2> texOffsetsGood = new List<Vector2>();
            List<Vector3> vertOffsetsGood = new List<Vector3>();
            float[] triOffsets = new float[36 * 4];
            for (int i = 0; i < 36; i++)
            {
                Vector3 offset = Vector3.zero;
                Vector3 texOffset = Vector2.zero;
                Vector3 normal = Vector3.one.normalized;
                switch (i)
                {
                    // bottom, top
                    // left, right
                    // front, back

                    // bottom
                    case 0: offset = new Vector3(1, 0, 1); break;
                    case 1: offset = new Vector3(1, 0, 0); break;
                    case 2: offset = new Vector3(0, 0, 0); break;
                    case 3: offset = new Vector3(0, 0, 0); break;
                    case 4: offset = new Vector3(0, 0, 1); break;
                    case 5: offset = new Vector3(1, 0, 1); break;

                    // top
                    case 6: offset = new Vector3(0, 1, 0); break;
                    case 7: offset = new Vector3(1, 1, 0); break;
                    case 8: offset = new Vector3(1, 1, 1); break;
                    case 9: offset = new Vector3(1, 1, 1); break;
                    case 10: offset = new Vector3(0, 1, 1); break;
                    case 11: offset = new Vector3(0, 1, 0); break;

                    // left
                    case 12: offset = new Vector3(1, 1, 0); break;
                    case 13: offset = new Vector3(0, 1, 0); break;
                    case 14: offset = new Vector3(0, 0, 0); break;
                    case 15: offset = new Vector3(1, 0, 0); break;
                    case 16: offset = new Vector3(1, 1, 0); break;
                    case 17: offset = new Vector3(0, 0, 0); break;

                    // right
                    case 18: offset = new Vector3(0, 0, 1); break;
                    case 19: offset = new Vector3(0, 1, 1); break;
                    case 20: offset = new Vector3(1, 1, 1); break;
                    case 21: offset = new Vector3(0, 0, 1); break;
                    case 22: offset = new Vector3(1, 1, 1); break;
                    case 23: offset = new Vector3(1, 0, 1); break;

                    // forward
                    case 24: offset = new Vector3(0, 0, 0); break;
                    case 25: offset = new Vector3(0, 1, 0); break;
                    case 26: offset = new Vector3(0, 1, 1); break;
                    case 27: offset = new Vector3(0, 0, 0); break;
                    case 28: offset = new Vector3(0, 1, 1); break;
                    case 29: offset = new Vector3(0, 0, 1); break;
                    // back
                    case 30: offset = new Vector3(1, 1, 1); break;
                    case 31: offset = new Vector3(1, 1, 0); break;
                    case 32: offset = new Vector3(1, 0, 0); break;
                    case 33: offset = new Vector3(1, 0, 1); break;
                    case 34: offset = new Vector3(1, 1, 1); break;
                    case 35: offset = new Vector3(1, 0, 0); break;
                }




                // bottom
                float texX = 0.0f;
                float texY = 0.0f;

                Vector3 offsetForTex = ApplyEpsPerturb(offset, epsPerturb);



                if (i >= 0 && i <= 5)
                {
                    texX = offsetForTex.x + 1.0f;
                    texY = offsetForTex.z + 2.0f;
                    normal = new Vector3(0, -1, 0);
                }
                // top
                else if (i >= 6 && i <= 11)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.z + 2.0f;
                    normal = new Vector3(0, 1, 0);
                }
                // left
                else if (i >= 12 && i <= 17)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.y + 1.0f;
                    normal = new Vector3(0, 0, -1);
                }
                // right
                else if (i >= 18 && i <= 23)
                {
                    texX = offsetForTex.x + 1.0f;
                    texY = offsetForTex.y + 1.0f;
                    normal = new Vector3(0, 0, 1);
                }
                // forward
                else if (i >= 24 && i <= 29)
                {
                    texX = offsetForTex.z;
                    texY = offsetForTex.y;
                    normal = new Vector3(-1, 0, 0);
                }
                // back
                else if (i >= 30 && i <= 35)
                {
                    texX = offsetForTex.z + 1.0f;
                    texY = offsetForTex.y;
                    normal = new Vector3(1, 0, 0);
                }
                texOffset = new Vector2(texX, texY);
                triOffsets[i * 4] = offset.x;
                triOffsets[i * 4 + 1] = offset.y;
                triOffsets[i * 4 + 2] = offset.z;
                triOffsets[i * 4 + 3] = 0;

                texOffsets[i * 4] = texOffset.x / 2.0f;
                texOffsets[i * 4 + 1] = texOffset.y / (float)World.numBlocks;
                texOffsets[i * 4 + 2] = 0;
                texOffsets[i * 4 + 3] = 0;


                vertNormals[i * 4] = normal.x;
                vertNormals[i * 4 + 1] = normal.y;
                vertNormals[i * 4 + 2] = normal.z;
                vertNormals[i * 4 + 3] = 0.0f;

                texOffsetsGood.Add(new Vector2(texOffset.x / 2.0f, texOffset.y / 3.0f));
                vertOffsetsGood.Add(offset - new Vector3(0.5f, 0.5f, 0.5f));
            }

            blockBreakingBuffer = new ComputeBuffer(1, sizeof(int) * 4);
            cubeOffsets = new ComputeBuffer(36, sizeof(float) * 4);
            cubeOffsets.SetData(triOffsets);
            cubeNormals = new ComputeBuffer(36, sizeof(float) * 4);
            cubeNormals.SetData(vertNormals);
            uvOffsets = new ComputeBuffer(36, sizeof(float) * 4);
            uvOffsets.SetData(texOffsets);

            float[] texOffsetsSingleBlock = new float[36 * 4];
            for (int i = 0; i < 36; i++)
            {
                Vector3 offset = Vector3.zero;
                Vector3 texOffset = Vector2.zero;
                switch (i)
                {
                    // bottom, top
                    // left, right
                    // front, back
                    // bottom
                    case 0: offset = new Vector3(1, 0, 1); break;
                    case 1: offset = new Vector3(1, 0, 0); break;
                    case 2: offset = new Vector3(0, 0, 0); break;
                    case 3: offset = new Vector3(0, 0, 0); break;
                    case 4: offset = new Vector3(0, 0, 1); break;
                    case 5: offset = new Vector3(1, 0, 1); break;

                    // top
                    case 6: offset = new Vector3(0, 1, 0); break;
                    case 7: offset = new Vector3(1, 1, 0); break;
                    case 8: offset = new Vector3(1, 1, 1); break;
                    case 9: offset = new Vector3(1, 1, 1); break;
                    case 10: offset = new Vector3(0, 1, 1); break;
                    case 11: offset = new Vector3(0, 1, 0); break;

                    // left
                    case 12: offset = new Vector3(1, 1, 0); break;
                    case 13: offset = new Vector3(0, 1, 0); break;
                    case 14: offset = new Vector3(0, 0, 0); break;
                    case 15: offset = new Vector3(1, 0, 0); break;
                    case 16: offset = new Vector3(1, 1, 0); break;
                    case 17: offset = new Vector3(0, 0, 0); break;

                    // right
                    case 18: offset = new Vector3(0, 0, 1); break;
                    case 19: offset = new Vector3(0, 1, 1); break;
                    case 20: offset = new Vector3(1, 1, 1); break;
                    case 21: offset = new Vector3(0, 0, 1); break;
                    case 22: offset = new Vector3(1, 1, 1); break;
                    case 23: offset = new Vector3(1, 0, 1); break;

                    // forward
                    case 24: offset = new Vector3(0, 0, 0); break;
                    case 25: offset = new Vector3(0, 1, 0); break;
                    case 26: offset = new Vector3(0, 1, 1); break;
                    case 27: offset = new Vector3(0, 0, 0); break;
                    case 28: offset = new Vector3(0, 1, 1); break;
                    case 29: offset = new Vector3(0, 0, 1); break;
                    // back
                    case 30: offset = new Vector3(1, 1, 1); break;
                    case 31: offset = new Vector3(1, 1, 0); break;
                    case 32: offset = new Vector3(1, 0, 0); break;
                    case 33: offset = new Vector3(1, 0, 1); break;
                    case 34: offset = new Vector3(1, 1, 1); break;
                    case 35: offset = new Vector3(1, 0, 0); break;
                }
                // bottom
                float texX = 0.0f;
                float texY = 0.0f;
                Vector3 offsetForTex = ApplyEpsPerturb(offset, epsPerturb);
                if (i >= 0 && i <= 5)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.z;
                }
                // top
                else if (i >= 6 && i <= 11)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.z;
                }
                // left
                else if (i >= 12 && i <= 17)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.y;
                }
                // right
                else if (i >= 18 && i <= 23)
                {
                    texX = offsetForTex.x;
                    texY = offsetForTex.y;
                }
                // forward
                else if (i >= 24 && i <= 29)
                {
                    texX = offsetForTex.z;
                    texY = offsetForTex.y;
                }
                // back
                else if (i >= 30 && i <= 35)
                {
                    texX = offsetForTex.z;
                    texY = offsetForTex.y;
                }
                texOffset = new Vector2(texX, texY);

                texOffsetsSingleBlock[i * 4] = texOffset.x;
                texOffsetsSingleBlock[i * 4 + 1] = texOffset.y / (float)World.numBreakingFrames;
                texOffsetsSingleBlock[i * 4 + 2] = 0;
                texOffsetsSingleBlock[i * 4 + 3] = 0;
            }

            breakingUVOffsets = new ComputeBuffer(36, sizeof(float) * 4);
            breakingUVOffsets.SetData(texOffsetsSingleBlock);

            blockMesh = new Mesh();
            List<Vector3> blockVertices = new List<Vector3>();
            List<Vector2> blockUvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            for (int i = 0; i < triOffsets.Length / 4; i++)
            {
                Vector3 vertex = new Vector3(triOffsets[i * 4], triOffsets[i * 4 + 1], triOffsets[i * 4 + 2]) - new Vector3(0.5f, 0.5f, 0.5f);
                Vector2 uv = new Vector2(texOffsets[i * 4], texOffsets[i * 4 + 1]);
                blockVertices.Add(vertex);
                blockUvs.Add(uv);
                triangles.Add(i);
            }
            int[] actualTriangles = new int[triangles.Count];
            for (int i = 0; i < triangles.Count / 3; i++)
            {
                actualTriangles[i * 3] = triangles[i * 3 + 2];
                actualTriangles[i * 3 + 1] = triangles[i * 3 + 1];
                actualTriangles[i * 3 + 2] = triangles[i * 3];
            }

            Mesh nice = new Mesh();
            nice.SetVertices(vertOffsetsGood);
            nice.SetUVs(0, texOffsetsGood);
            nice.SetTriangles(actualTriangles, 0);
            GameObject spruce = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spruce.name = "dat boi";
            spruce.GetComponent<MeshFilter>().mesh = nice;


            blockMesh.SetVertices(blockVertices);
            blockMesh.SetUVs(0, blockUvs);
            blockMesh.SetTriangles(actualTriangles, 0);

            triMaterial.SetBuffer("cubeOffsets", cubeOffsets);
            triMaterial.SetBuffer("cubeNormals", cubeNormals);
            triMaterial.SetBuffer("uvOffsets", uvOffsets);
            triMaterialWithTransparency.SetBuffer("cubeOffsets", cubeOffsets);
            triMaterialWithTransparency.SetBuffer("cubeNormals", cubeNormals);
            triMaterialWithTransparency.SetBuffer("uvOffsets", uvOffsets);
            breakingMaterial.SetBuffer("cubeOffsets", cubeOffsets);
            breakingMaterial.SetBuffer("cubeNormals", cubeNormals);
            breakingMaterial.SetBuffer("uvOffsets", breakingUVOffsets);

            float[] lineOffsets = new float[24 * 4];
            for (int i = 0; i < 24; i++)
            {
                Vector3 offset = Vector3.zero;

                switch (i)
                {
                    // bottom
                    case 0: offset = new Vector3(0, 0, 0); break;
                    case 1: offset = new Vector3(1, 0, 0); break;
                    case 2: offset = new Vector3(1, 0, 0); break;
                    case 3: offset = new Vector3(1, 0, 1); break;
                    case 4: offset = new Vector3(1, 0, 1); break;
                    case 5: offset = new Vector3(0, 0, 1); break;
                    case 6: offset = new Vector3(0, 0, 1); break;
                    case 7: offset = new Vector3(0, 0, 0); break;


                    case 8: offset = new Vector3(0, 1, 0); break;
                    case 9: offset = new Vector3(1, 1, 0); break;
                    case 10: offset = new Vector3(1, 1, 0); break;
                    case 11: offset = new Vector3(1, 1, 1); break;
                    case 12: offset = new Vector3(1, 1, 1); break;
                    case 13: offset = new Vector3(0, 1, 1); break;
                    case 14: offset = new Vector3(0, 1, 1); break;
                    case 15: offset = new Vector3(0, 1, 0); break;

                    case 16: offset = new Vector3(0, 0, 0); break;
                    case 17: offset = new Vector3(0, 1, 0); break;
                    case 18: offset = new Vector3(0, 0, 1); break;
                    case 19: offset = new Vector3(0, 1, 1); break;
                    case 20: offset = new Vector3(1, 0, 0); break;
                    case 21: offset = new Vector3(1, 1, 0); break;
                    case 22: offset = new Vector3(1, 0, 1); break;
                    case 23: offset = new Vector3(1, 1, 1); break;
                }
                lineOffsets[i * 4] = offset.x;
                lineOffsets[i * 4 + 1] = offset.y;
                lineOffsets[i * 4 + 2] = offset.z;
                lineOffsets[i * 4 + 3] = 0;
            }



            linesOffsets = new ComputeBuffer(24, sizeof(float) * 4);
            linesOffsets.SetData(lineOffsets);

            outlineMaterial.SetBuffer("lineOffsets", linesOffsets);


            drawPositions1 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
            drawPositions2 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);

            chunkBlockData = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4);
            cullBlocksShader.SetBuffer(0, "DataIn", chunkBlockData);
            cullBlocksShader.SetBuffer(1, "DataIn", chunkBlockData);
            cullBlocksShader.SetBuffer(0, "cubeOffsets", cubeOffsets);
            cullBlocksShader.SetBuffer(0, "uvOffsets", uvOffsets);
            cullBlocksShader.SetBuffer(1, "cubeOffsets", cubeOffsets);
            cullBlocksShader.SetBuffer(1, "uvOffsets", uvOffsets);



            triMaterial.mainTexture = BlockValue.allBlocksTexture;
            triMaterialWithTransparency.mainTexture = BlockValue.allBlocksTexture;
            blockEntityMaterial.mainTexture = BlockValue.allBlocksTexture;
            blockIconMaterial.mainTexture = BlockValue.allBlocksTexture;


            string assetsPath = Application.dataPath;
            // strip any trailing path seperators
            while (assetsPath[assetsPath.Length - 1] == '/' || assetsPath[assetsPath.Length - 1] == '\\')
            {
                assetsPath = assetsPath.Substring(0, assetsPath.Length - 1);
            }
            // append pack textures directory
            assetsPath = assetsPath.Replace("\\", "/") + "/PackTextures";

            DirectoryInfo assetsPathInfo = new DirectoryInfo(assetsPath);
            if (!assetsPathInfo.Exists)
            {
                Directory.CreateDirectory(assetsPath);
            }

            BlockValue.allBlocksTexture.alphaIsTransparency = true;
            string assetsPathDir = assetsPathInfo.FullName;

            //File.WriteAllBytes(assetsPathDir + "/CurrentTexture.png", BlockValue.allBlocksTexture.EncodeToPNG());

            // delete meta file so unity will reload it
            //if (File.Exists(assetsPathDir + "/CurrentTexture.png.meta"))
            //{
            //    File.Delete(assetsPathDir + "/CurrentTexture.png.meta");
            //}
        }

        void LoadMaterials()
        {
            if (!triMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Unlit/SandDrawer");
                triMaterial = new Material(shader);
                triMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                // Turn backface culling off
                //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                /*
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
                */

            }

            if (!triMaterialWithTransparency)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Unlit/SandDrawerWithTransparency");
                triMaterialWithTransparency = new Material(shader);
                triMaterialWithTransparency.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                //lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                // Turn backface culling off
                //lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                /*
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
                */

            }

            if (!outlineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Unlit/SandLineDrawer");
                outlineMaterial = new Material(shader);
                outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public BlocksPack blocksPack;

        // Use this for initialization
        void Start()
        {

            SetupRendering();

            //Dictionary<BlockValue, Block> customBlocks = new Dictionary<BlockValue, Block>();
            //customBlocks[BlockValue.GRASS] = new Grass();
            //GenerationClass customGeneration = new ExampleGeneration();
            world = new World(this, chunkSize, blocksPack);
            lastTick = 0;


           world.helperThread = new Thread(() =>
           {
               while (threadKeepGoing)
               {
                   try
                   {

                       world.Tick();
                   }
                   catch (System.Exception e)
                   {
                       Debug.LogError(e);
                   }
                   Thread.Sleep(1);
             
               }
           });

            world.helperThread.Priority = System.Threading.ThreadPriority.Lowest;
            world.helperThread.Start();
        }


        bool threadKeepGoing = true;

        int ax = 0;
        int ay = 0;
        int az = 0;


        public int numChunksTotal;
        // Update is called once per frame
        void Update()
        {
            World.creativeMode = creativeMode;
            triMaterial.SetFloat("globalLightLevel", skyLightLevel);
            triMaterialWithTransparency.SetFloat("globalLightLevel", skyLightLevel);
            numChunksTotal = world.allChunks.Count;
            for (int i = 0; i < 20; i++)
            {
                //world[ax, ay, az] = World.DIRT;
                ax += Random.Range(-1, 2);
                ay += Random.Range(-1, 2);
                az += Random.Range(-1, 2);
                if (ay < 5)
                {
                    ay = 5;
                }
            }
            float millisPerTick = 1000.0f / ticksPerSecond;
            if (millis() - lastTick > millisPerTick)
            {
                lastTick = millis();
                //world.Tick();
            }



            BlocksPlayer player = FindObjectOfType<BlocksPlayer>();
            if (player != null)
            {
                //Chunk playerChunk = world.GetOrGenerateChunkAtPos()
            }
        }

        public int BlockAtUnityPoint(Vector3 pos)
        {
            long pointX = (long)Mathf.Floor(pos.x / worldScale);
            long pointY = (long)Mathf.Floor(pos.x / worldScale);
            long pointZ = (long)Mathf.Floor(pos.x / worldScale);
            return world[pointX, pointY, pointZ];
        }


        public Vector3 UnityPointToBlockWorldPoint(Vector3 pos)
        {
            return new Vector3(Mathf.Floor(pos.x / worldScale), Mathf.Floor(pos.y / worldScale), Mathf.Floor(pos.z / worldScale));
        }


        public void MakeChunkTrisForCombined(Chunk chunk,  ComputeBuffer drawData)
        {
            Vector3 offset = (new Vector3(chunk.cx, chunk.cy, chunk.cz)) * chunkSize * worldScale;
            renderTransform.transform.position += offset;

            chunkBlockData.SetData(chunk.chunkData.GetRawData());

            for (int i = 0; i < 2; i++)
            {
                cullBlocksShader.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                cullBlocksShader.SetInt("ptCloudWidth", chunkSize);
                cullBlocksShader.SetFloat("ptCloudScale", worldScale);
                cullBlocksShader.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
            }
            // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
            cullBlocksShader.SetBuffer(0, "DrawingThings", drawData);
            cullBlocksShader.Dispatch(0, chunkSize / 8, chunkSize / 8, chunkSize / 8);

            renderTransform.transform.position -= offset;
        }

        public void MakeChunkTris(Chunk chunk, out int numNotTransparent, out int numTransparent)
        {
            int[] args = new int[] { 0 };

            Vector3 offset = (new Vector3(chunk.cx, chunk.cy, chunk.cz)) * chunkSize * worldScale;
            renderTransform.transform.position += offset;

            chunk.chunkRenderer.drawDataNotTransparent.SetCounterValue(0);
            chunkBlockData.SetData(chunk.chunkData.GetRawData());

            for (int i = 0; i  < 2; i++)
            {
                cullBlocksShader.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                cullBlocksShader.SetInt("ptCloudWidth", chunkSize);
                cullBlocksShader.SetFloat("ptCloudScale", worldScale);
                cullBlocksShader.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
            }
            // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
            cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
            cullBlocksShader.Dispatch(0, chunkSize / 8, chunkSize / 8, chunkSize / 8);

            ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataNotTransparent, world.argBuffer, 0);
            world.argBuffer.GetData(args);
            numNotTransparent = args[0];
            float[] resVals = new float[numNotTransparent*(4+4+4+2)];
            //Debug.Log("got num not transparent = " + numNotTransparent + " (" + (numNotTransparent / 36) + ")");

            chunk.chunkRenderer.drawDataTransparent.SetCounterValue(0);
            //chunkBlockData.SetData(chunk.chunkData.GetRawData());
            // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
            cullBlocksShader.SetBuffer(1, "DrawingThings", chunk.chunkRenderer.drawDataTransparent);
            cullBlocksShader.Dispatch(1, chunkSize / 8, chunkSize / 8, chunkSize / 8);
            ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataTransparent, world.argBuffer, 0);
            world.argBuffer.GetData(args);
            numTransparent = args[0];

            renderTransform.transform.position -= offset;
        }

        public Queue<Tuple<LVector3, float>> blocksBreaking = new Queue<Tuple<LVector3, float>>();

        public float TimeNeededToBreak(LVector3 blockPos, BlockValue block, BlockStack itemHittingWith)
        {
            if (World.creativeMode)
            {
                return 0.1f;
            }
            if (itemHittingWith == null)
            {
                itemHittingWith = new BlockStack(BlockValue.Air, 1);
            }
            BlockOrItem customBlock;
            if (world.customBlocks.ContainsKey(block, out customBlock))
            {
                using (BlockData blockData = world.GetBlockData(blockPos.x, blockPos.y, blockPos.z))
                {
                    return customBlock.TimeNeededToBreak(blockData, itemHittingWith);
                }
            }
            Debug.Log("warning, fell through, block = " + World.BlockToString(block) + " and block hitting with = " + World.BlockToString(itemHittingWith.Block));
            return 1.0f;
            /*

            Debug.Log("calling time needed to break with block = " + World.BlockToString(block) + " and item hitting with = " + itemHittingWith);
            if (block == BlockValue.LooseRocks)
            {
                if (itemHittingWith == null)
                {
                    return 1.0f;
                }
                else if (itemHittingWith.Block == BlockValue.Pickaxe)
                {
                    return 0.3f;
                }
                else if(itemHittingWith.Block == BlockValue.Shovel || itemHittingWith.Block == BlockValue.Axe)
                {
                    return 0.5f;
                }
                else
                {
                    return 1.0f;
                }
            }
            else if(block == BlockValue.Rock || block == BlockValue.LargeRock)
            {
                Debug.Log("hitting rock or large rock");
                if (itemHittingWith == null)
                {
                    Debug.Log("hitting rock or large rock with nothing");
                    return 0.0f;
                }
                else
                {
                    Debug.Log("hitting rock or large rock with " + World.BlockToString(itemHittingWith.Block));
                    if (itemHittingWith.Block == BlockValue.Rock || itemHittingWith.Block == BlockValue.SharpRock)
                    {
                        Debug.Log("hitting rock or large rock with ROCK or SHARP ROCK");
                        if (block == BlockValue.Rock)
                        {
                            return 4.0f;
                        }
                        else if (block == BlockValue.LargeRock)
                        {
                            return 7.0f;
                        }
                    }
                    else if (itemHittingWith.Block == BlockValue.LargeRock || itemHittingWith.Block == BlockValue.LargeSharpRock)
                    {
                        Debug.Log("hitting rock or large rock with LARGE ROCK or LARGE SHARP ROCK");
                        if (block == BlockValue.Rock)
                        {
                            return 2.0f;
                        }
                        else if (block == BlockValue.LargeRock)
                        {
                            return 4.0f;
                        }
                    }
                    else if (itemHittingWith.Block == BlockValue.Pickaxe)
                    {
                        if (block == BlockValue.Rock)
                        {
                            return 0.7f;
                        }
                        else if (block == BlockValue.LargeRock)
                        {
                            return 1.5f;
                        }
                    }
                    else if (itemHittingWith.Block == BlockValue.Shovel || itemHittingWith.Block == BlockValue.Axe)
                    {
                        if (block == BlockValue.Rock)
                        {
                            return 1.5f;
                        }
                        else if (block == BlockValue.LargeRock)
                        {
                            return 3.0f;
                        }
                    }
                    else
                    {
                        return 0.0f;
                    }
                }
            }
            else if(block == BlockValue.Bark)
            {
                return 0.0f;
            }
            else if(block == BlockValue.WetBark)
            {
                return 0.0f;
            }
            else if (block == BlockValue.Stone)
            {
                if (itemHittingWith != null && itemHittingWith.Block == BlockValue.Pickaxe)
                {
                    return 3.0f;
                }
                return 20.0f;
            }
            else if (block == BlockValue.Sand)
            {
                return 1.0f;
            }
            else if (block == BlockValue.Clay)
            {
                return 1.0f;
            }
            else if (block == BlockValue.Grass)
            {
                return 2.0f;
            }
            else if(block == BlockValue.Dirt)
            {
                return 2.0f;
            }
            else if(block == BlockValue.Leaf)
            {
                return 0.5f;
            }
            else if(block == BlockValue.Trunk)
            {
                using (BlockData blockData = world.GetBlockData(blockPos.x, blockPos.y, blockPos.z))
                {
                    if (blockData.state1 > 0)
                    {
                        if (itemHittingWith == null)
                        {
                            return 1.0f;
                        }
                        else if (itemHittingWith.Block == BlockValue.Axe)
                        {
                            return 0.5f;
                        }
                        else
                        {
                            return 1.0f;
                        }
                    }
                    else
                    {
                        if (itemHittingWith == null)
                        {
                            return 10.0f;
                        }
                        else if (itemHittingWith.Block == BlockValue.Axe)
                        {
                            return 2.0f;
                        }
                        else
                        {
                            return 10.0f;
                        }
                    }
                }
            }
            if (itemHittingWith == null)
            {
                Debug.Log("warning, fell through, block = " + World.BlockToString(block) + " and block hitting with = " + itemHittingWith);
            }
            else
            {
                Debug.Log("warning, fell through, block = " + World.BlockToString(block) + " and block hitting with = " + World.BlockToString(itemHittingWith.Block));
            }
            return 100.0f;
            */
        }

        public bool RenderBlockBreaking(long x, long y, long z, BlockValue blockHitting, float timeHitting, BlockStack itemHittingWith)
        {
            float timeNeeded = TimeNeededToBreak(new LVector3(x, y, z), blockHitting, itemHittingWith);
            if (timeHitting > timeNeeded)
            {
                return true;
            }
            else
            {
                blocksBreaking.Enqueue(new Tuple<LVector3, float>(new LVector3(x, y, z), timeHitting / timeNeeded));
                return false;
            }
        }
        void ActuallyRenderBlockBreaking(long x, long y, long z, float howBroken)
        {
            long cx, cy, cz;
            world.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
            Vector3 offset = (new Vector3(cx, cy, cz)) * chunkSize * worldScale;
            renderTransform.transform.position += offset;

            int brokenState = (int)Mathf.Clamp(Mathf.Round(howBroken * 8), 0.0f, 8.0f);
            blockBreakingBuffer.SetData(new int[] { (int)(x - cx * chunkSize), (int)(y - cy * chunkSize), (int)(z - cz * chunkSize), brokenState });
            if (Camera.current != uiCamera)
            {
                breakingMaterial.SetBuffer("DrawingThings", blockBreakingBuffer);
                breakingMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                breakingMaterial.SetInt("ptCloudWidth", chunkSize);
                breakingMaterial.SetFloat("ptCloudScale", worldScale);
                breakingMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                breakingMaterial.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Triangles, 1 * (36));
            }
            renderTransform.transform.position -= offset;
        }

        public void RenderChunk(Chunk chunk, bool onlyTransparent)
        {
            //if (Camera.current != transform.GetComponent<Camera>() || settings.isOn)
            //{
            //    return;
            //}
            //Vector3 offset = metaSize * (renderTransform.transform.forward * sandScale * sandDim / 2.0f + renderTransform.transform.right * sandScale * sandDim / 2.0f + renderTransform.transform.up * sandScale * sandDim / 2.0f);
            Vector3 offset = (new Vector3(chunk.cx, chunk.cy, chunk.cz)) * chunkSize * worldScale;
            renderTransform.transform.position += offset;
            /*
            if ((Camera.current.cullingMask & (1 << 5)) == 0)
            {
                outlineMaterial.SetBuffer("DrawingThings", chunk.chunkRenderer.drawData);
                outlineMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                //outlineMaterial.SetBuffer("PixelData", sandPixelData);
                outlineMaterial.SetInt("ptCloudWidth", chunkSize);
                outlineMaterial.SetFloat("ptCloudScale", worldScale);
                outlineMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                outlineMaterial.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Lines, chunk.chunkRenderer.numRendereredCubes * 24);
            }*/

            if (Camera.current != uiCamera)
            {
                if (onlyTransparent)
                {
                    /*
                    triMaterialWithTransparency.SetBuffer("DrawingThings", chunk.chunkRenderer.drawDataTransparent);
                    triMaterialWithTransparency.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    triMaterialWithTransparency.SetInt("ptCloudWidth", chunkSize);
                    //triMaterial.SetBuffer("PixelData", sandPixelData);
                    triMaterialWithTransparency.SetFloat("ptCloudScale", worldScale);
                    triMaterialWithTransparency.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    triMaterialWithTransparency.SetPass(0);
                    Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesTransparent * (36));
                    */
                }
                else
                {
                    triMaterial.SetBuffer("DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
                    triMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    triMaterial.SetInt("ptCloudWidth", chunkSize);
                    //triMaterial.SetBuffer("PixelData", sandPixelData);
                    triMaterial.SetFloat("ptCloudScale", worldScale);
                    triMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    triMaterial.SetPass(0);
                    Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesNotTransparent*3);
                    // Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesNotTransparent * (36));
                }
            }
            renderTransform.transform.position -= offset;
        }

        LVector3 blockBreaking;
        public void OnRenderObject()
        {
            world.Render();
            while (blocksBreaking.Count > 0)
            {
                Tuple<LVector3, float> blockBreaking = blocksBreaking.Dequeue();
                ActuallyRenderBlockBreaking(blockBreaking.a.x, blockBreaking.a.y, blockBreaking.a.z, 1.0f - blockBreaking.b);
            }
        }

        bool cleanedUp = false;
        void CleanupRendering()
        {
            if (linesOffsets != null)
            {
                linesOffsets.Dispose();
                linesOffsets = null;
            }
            if (cubeOffsets != null)
            {
                cubeOffsets.Dispose();
                cubeOffsets = null;
            }


            if (cubeNormals != null)
            {
                cubeNormals.Dispose();
                cubeNormals = null;
            }

            if (blockBreakingBuffer != null)
            {
                blockBreakingBuffer.Dispose();
                blockBreakingBuffer = null;
            }

            if (drawPositions1 != null)
            {
                drawPositions1.Dispose();
                drawPositions1 = null;
            }

            if (drawPositions2 != null)
            {
                drawPositions2.Dispose();
                drawPositions2 = null;
            }

            if (chunkBlockData != null)
            {
                chunkBlockData.Dispose();
                chunkBlockData = null;
            }

            if (world != null)
            {
                world.Dispose();
                world = null;
            }

            if (uvOffsets != null)
            {
                uvOffsets.Dispose();
                uvOffsets = null;
            }

            if (breakingUVOffsets != null)
            {
                breakingUVOffsets.Dispose();
                breakingUVOffsets = null;
            }
        }
        void Cleanup()
        {
            if (!cleanedUp)
            {
                threadKeepGoing = false;
                cleanedUp = true;
                CleanupRendering();
            }
        }

        void OnApplicationQuit()
        {
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }
    }
}