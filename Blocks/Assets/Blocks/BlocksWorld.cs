﻿using Example_pack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;
using ExtensionMethods;
using Blocks.ExtensionMethods;
using System.IO.Compression;

namespace Blocks
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Float4
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }


    public class Util
    {

        public static Float4 MakeFloat4(float x, float y, float z, float w)
        {
            Float4 res = new Float4();
            res.x = x;
            res.y = y;
            res.z = z;
            res.w = w;
            return res;
        }



        public static Float4 MakeFloat4(Vector3 pos)
        {
            Float4 res = new Float4();
            res.x = pos.x;
            res.y = pos.y;
            res.z = pos.z;
            res.w = 1.0f;
            return res;
        }


        public static Float2 MakeFloat2(float x, float y)
        {
            Float2 res = new Float2();
            res.x = x;
            res.y = y;
            return res;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Float3
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Float2
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RenderTriangle
    {
        public int state1;
        public int state2;
        public int state3;
        public int state4;
        public Float4 vertex1;
        public Float4 vertex2;
        public Float4 vertex3;
        public Float2 uv1;
        public Float2 uv2;
        public Float2 uv3;
    }

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

        public enum RenderStatus
        {
            None,
            HasTriangles,
            StoredToComputeBuffer
        }


        public object renderingLock = new object();
        public RenderStatus renderStatus = RenderStatus.None;
        public List<RenderTriangle> triangles = null;
        

        Chunk chunk;
        int chunkSizeX, chunkSizeY, chunkSizeZ;
        public bool hasCustomBlocks = false;
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

        public ChunkRenderer(Chunk chunk)
        {
            this.chunk = chunk;
            InitStuff();
        }

        bool didInit = false;
        void InitStuff()
        {
            // we can only init compute buffers on the main thread
            if (Thread.CurrentThread != World.mainWorld.helperThread)
            {
                //drawDataTransparent = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ * 12, sizeof(int) * 22, ComputeBufferType.Append);
                if (World.DO_CPU_RENDER)
                {
                    // we don't need this anymore since we use meshes
                    //drawDataNotTransparent = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ * 12, sizeof(int) * 22, ComputeBufferType.Default); // modified to be GPUMemory
                }
                else
                {
                    Debug.LogWarning("making compute buffer");
                    drawDataNotTransparent = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ * 12, sizeof(int) * 22, ComputeBufferType.Append); // modified to be GPUMemory
                }
            }

            if (chunk.cx % 2 == 0 && chunk.cy % 2 == 0 && chunk.cz % 2 == 0)
            {
                isMetaNode = true;
                if (Thread.CurrentThread != World.mainWorld.helperThread)
                {
                    //combinedDrawDataNotTransparent = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ * 12 * 8, sizeof(int) * 22, ComputeBufferType.Append);
                }

                if (otherChunks == null)
                {
                    otherChunks = new ChunkRenderer[8];
                }

                otherChunks[0] = this;
                parentChunkRenderer = this;
            }
            else
            {
                isMetaNode = false;
            }
            didInit = true;
        }

        public void Tick()
        {

        }


        public void FinishRenderSync(Chunk chunk)
        {
            if (needToFinishSync)
            {
                int numAllowedToRender = 1;
                chunk.world.blocksWorld.FinishChunkTrisSync(chunk, out numRenderedCubesCombinedNotTransparent, out numRendereredCubesTransparent, ref numAllowedToRender);
                needToFinishSync = false;
                chunk.world.blocksWorld.RenderChunk(chunk, false);
            }
        }

        public bool needToFinishSync = false;
        bool needToFinishSyncAsParent = false;

        public void RenderLogging()
        {
            for(int i = 0; i < chunk.pathingChunks.Count; i++)
            {
                chunk.pathingChunks[i].Draw();
            }
        }

        public bool RenderAsync(bool onlyTransparent, Chunk chunk, ref int numAllowedToDoFullRender)
        {

            // this object was made on a seperate thread, init stuff
            if (!didInit)
            {
                InitStuff();
            }
            if (chunk.chunkData.needToBeUpdated && (numAllowedToDoFullRender > 0 && (chunk.chunkRenderer.triangles != null && renderStatus == RenderStatus.HasTriangles)) && chunk.threadRenderingMe == -1)
            {
                // if we have done all the lighting updates we need, we can be done updating this chunk
                lock(chunk.modifyLock)
                {
                    if (!chunk.needToUpdateLighting)
                    {
                        chunk.chunkData.needToBeUpdated = false;
                    }
                }
                // new stuff




                if (chunk.chunkRenderer.triangles.Count > 0)
                {
                    if (myMesh != null)
                    {
                        //myMesh.Dispose();
                        //myMesh = null;
                    }
                    //myMesh = TrianglesToMesh(chunk.chunkRenderer.triangles);
                }
                numRendereredCubesTransparent = 0;
                numRendereredCubesNotTransparent = chunk.chunkRenderer.triangles.Count;
                chunk.chunkRenderer.renderStatus = ChunkRenderer.RenderStatus.StoredToComputeBuffer; // set it to up to date now
                // done new stuff

                //chunk.world.blocksWorld.MakeChunkTrisAsync(chunk);
                //chunk.world.blocksWorld.FinishChunkTrisSync(chunk, out numRendereredCubesNotTransparent, out numRendereredCubesTransparent, ref numAllowedToDoFullRender);


                numTimesRenderedAfterFinishedTriangles = 0;

                //numRendereredCubesNotTransparent = 0;
                //numRendereredCubesTransparent = 0;
                //needToFinishSync = true;
                // if we don't have a parent, try to find it
                /*
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
                */
            }

            /*
            bool combineSpookers = false;

            // if we are a parent and aren't combining but something has updated, check to see if we can now
            if (isMetaNode && !combiningDrawData && maybeCombineDrawData && numAllowedToDoFullRender > 8 && combineSpookers)
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




            bool justRenderUs = false;

            if (!isMetaNode)
            {
                // parent is rendering for us, we don't need to render
                if (parentChunkRenderer != null && parentChunkRenderer.combiningDrawData)
                {
                    Debug.Log("parent is combining??");
                }
                // parent isn't rendering for us, we are renderering right now because all of our siblings aren't finished yet
                else
                {
                    if (!needToFinishSync)
                    {
                        justRenderUs = true;
                    }
                }
            }
            // we are a parent
            else
            {
                // we are drawing our combined data
                if (combiningDrawData)
                {
                    Debug.Log("combining??");
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
                    if (!needToFinishSync)
                    {
                        justRenderUs = true;
                    }
                }
            }
            */

            BlocksMesh curMyMesh = myMesh; // in case of threading stuffs

            if (curMyMesh != null && curMyMesh.meshVertices.Length > 0)
            {
                // new simpler stuff
                chunk.world.blocksWorld.RenderChunkBlocksMesh(chunk, curMyMesh);

                /*
                int numTimesToConsiderStatic = 20;
                if (numTimesRenderedAfterFinishedTriangles == numTimesToConsiderStatic) // random so we don't do them all on the same frame
                {
                    List<RenderTriangle> myTriangles = triangles;
                    if (myTriangles != null && myTriangles.Count > 0)
                    {
                        numTimesRenderedAfterFinishedTriangles += 1;
                        //Debug.Log("making mesh for chunk " + chunk.cx + " " + chunk.cy + " " + chunk.cz + " with " + myTriangles.Count + " triangles");
                        myMesh = TrianglesToMesh(myTriangles);
                    }
                    else
                    {
                        numTimesRenderedAfterFinishedTriangles = numTimesToConsiderStatic;
                        myMesh = null;
                    }
                }
                else if (numTimesRenderedAfterFinishedTriangles < numTimesToConsiderStatic)
                {
                    myMesh = null;
                }
                if (myMesh != null)
                {
                    chunk.world.blocksWorld.RenderChunkMesh(chunk, myMesh);
                }
                else
                {
                    chunk.world.blocksWorld.RenderChunk(chunk, onlyTransparent);
                    if (numTimesRenderedAfterFinishedTriangles < numTimesToConsiderStatic)
                    {
                        numTimesRenderedAfterFinishedTriangles += 1;
                    }
                }
                */
            }


            return needToFinishSync;
        }


        public BlocksMesh TrianglesToMesh(List<RenderTriangle> triangles)
        {
            MeshVertex[] meshVertices = new MeshVertex[triangles.Count*3];
            int ind = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                int j = i * 3;
                RenderTriangle curTri = triangles[i];
                float blockLight = (curTri.state3 & 0xF) / 15.0f;
                float worldLight = ((curTri.state3 & 0xF0) >> 4) / 15.0f;
                meshVertices[j] = new MeshVertex()
                {
                    pos = new Float4()
                    {
                        x = curTri.vertex1.x,
                        y = curTri.vertex1.y,
                        z = curTri.vertex1.z,
                        w = 1.0f
                    },
                    uv = new Float2()
                    {
                        x = curTri.uv1.x,
                        y = curTri.uv1.y
                    },
                    color = new Float2()
                    {
                        x = blockLight,
                        y = worldLight
                    }
                };
                meshVertices[j + 1] = new MeshVertex()
                {
                    pos = new Float4()
                    {
                        x = curTri.vertex2.x,
                        y = curTri.vertex2.y,
                        z = curTri.vertex2.z,
                        w = 1.0f
                    },
                    uv = new Float2()
                    {
                        x = curTri.uv2.x,
                        y = curTri.uv2.y
                    },
                    color = new Float2()
                    {
                        x = blockLight,
                        y = worldLight
                    }
                };

                meshVertices[j + 2] = new MeshVertex()
                {
                    pos = new Float4()
                    {
                        x = curTri.vertex3.x,
                        y = curTri.vertex3.y,
                        z = curTri.vertex3.z,
                        w = 1.0f
                    },
                    uv = new Float2()
                    {
                        x = curTri.uv3.x,
                        y = curTri.uv3.y
                    },
                    color = new Float2()
                    {
                        x = blockLight,
                        y = worldLight
                    }
                };

            }
            BlocksMesh result = new BlocksMesh(meshVertices);
            //Mesh result = new Mesh();
            //result.SetVertices(vertices);
            //result.SetUVs(0, uvs);
            //result.SetTriangles(indices, 0, false);
            //result.SetColors(colors);
            return result;
        }

        public BlocksMesh prevMesh;
        public BlocksMesh myMesh;
        int numTimesRenderedAfterFinishedTriangles = 0;

        public void CombineAndRenderChildrenDrawData()
        {
            if (World.DO_CPU_RENDER)
            {

            }
            else
            {
                combinedDrawDataNotTransparent.SetCounterValue(0);
            }
            int curOffset = 0;
            for (int i = 0; i < otherChunks.Length; i++)
            {
                curOffset += chunk.world.blocksWorld.MakeChunkTrisForCombined(otherChunks[i].chunk, combinedDrawDataNotTransparent, curOffset);
            }

            if (World.DO_CPU_RENDER)
            {
                numRenderedCubesCombinedNotTransparent = curOffset;
            }
            else
            {
                int[] args = new int[] { 0 };
                ComputeBuffer.CopyCount(combinedDrawDataNotTransparent, chunk.world.argBuffer, 0);
                chunk.world.argBuffer.GetData(args);
                numRenderedCubesCombinedNotTransparent = args[0];
            }

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

                if (myMesh != null)
                {
                    myMesh.Dispose();
                    myMesh = null;
                }
            }
        }
    }



    public class BlocksTouchingSky
    {
        public class BlockTouchingSkyChunk
        {
            public long cx, cz;

            public const int chunkSizeX = World.chunkSizeX;
            public const int chunkSizeY = World.chunkSizeY;
            public const int chunkSizeZ = World.chunkSizeZ;

            public long[,] highestBlocks;
            public BlockTouchingSkyChunk(long cx, long cz)
            {
                this.cx = cx;
                this.cz = cz;
                highestBlocks = new long[chunkSizeX, chunkSizeZ];
                for (int i = 0; i < chunkSizeX; i++)
                {
                    for (int j = 0; j < chunkSizeZ; j++)
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
            long cx = world.divWithFloorForChunkSizeX(x);
            long cz = world.divWithFloorForChunkSizeZ(z);
            return GetOrCreateSkyChunk(cx, cz);
        }


        public BlockTouchingSkyChunk GetOrCreateSkyChunk(long cx, long cz)
        {
            BlockTouchingSkyChunk skyChunk = GetSkyChunk(cx, cz);
            if (skyChunk == null)
            {
                skyChunk = new BlockTouchingSkyChunk(cx, cz);
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
            long cx = world.divWithFloorForChunkSizeX(x);
            long cz = world.divWithFloorForChunkSizeZ(z);
            long relativeX = x - cx * World.chunkSizeX;
            long relativeZ = z - cz * World.chunkSizeZ;
            BlockTouchingSkyChunk skyChunk = GetOrCreateSkyChunk(cx, cz);
            long curY = y;
            long cy = world.divWithFloorForChunkSizeY(y);
            while(true)
            {
                curY -= 1;
                long curCy = world.divWithFloorForChunkSizeY(curY);
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
                        curY = curCy * World.chunkSizeY + World.chunkSizeY - 1;
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
            long cx = world.divWithFloorForChunkSizeX(x);
            long cz = world.divWithFloorForChunkSizeZ(z);
            long relativeX = x - cx * World.chunkSizeX;
            long relativeZ = z - cz * World.chunkSizeZ;
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
            long chunkX = chunk.cx * World.chunkSizeX;
            long chunkY = chunk.cy * World.chunkSizeY;
            long chunkZ = chunk.cz * World.chunkSizeZ;
            for (long x = 0; x < World.chunkSizeX; x++)
            {
                for (long z = 0; z < World.chunkSizeZ; z++)
                {
                    long curHighestY = skyChunk.highestBlocks[x, z];
                    long highestYInNewChunk = chunk.cy * World.chunkSizeY + World.chunkSizeY - 1;
                    // we already have something higher than the top of this chunk, ignore this xz position
                    if (highestYInNewChunk < curHighestY)
                    {
                        continue;
                    }
                    else
                    {
                        // there might be something higher, check the column of blocks starting at the roof of the chunk
                        for (long y = World.chunkSizeY-1; y >= 0; y--)
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
        public long bx, by, bz;
        public int biomeDataSizeX, biomeDataSizeY, biomeDataSizeZ;

        public float[] chunkProperties;
        public ChunkProperties chunkPropertiesObj;


        public ChunkBiomeData(ChunkProperties chunkProperties, int biomeDataSizeX, int biomeDataSizeY, int biomeDataSizeZ, long bx, long by, long bz)
        {
            this.bx = bx;
            this.by = by;
            this.bz = bz;
            this.biomeDataSizeX = biomeDataSizeX;
            this.biomeDataSizeY = biomeDataSizeY;
            this.biomeDataSizeZ = biomeDataSizeZ;
            this.chunkPropertiesObj = chunkProperties;
            this.chunkProperties = chunkProperties.GenerateChunkPropertiesArr(bx, by, bz);
        }

        public void RunChunkPropertyEventsOnGeneration()
        {
            chunkPropertiesObj.RunEvents(bx, by, bz);
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
            x1y0z0 = World.mainWorld.GetChunkBiomeData(bx + 1, by, bz);
            x0y1z0 = World.mainWorld.GetChunkBiomeData(bx, by + 1, bz);
            x1y1z0 = World.mainWorld.GetChunkBiomeData(bx + 1, by + 1, bz);
            x0y0z1 = World.mainWorld.GetChunkBiomeData(bx, by, bz + 1);
            x1y0z1 = World.mainWorld.GetChunkBiomeData(bx + 1, by, bz + 1);
            x0y1z1 = World.mainWorld.GetChunkBiomeData(bx, by + 1, bz + 1);
            x1y1z1 = World.mainWorld.GetChunkBiomeData(bx + 1, by + 1, bz + 1);
        }

        public float AverageBiomeData(long wx, long wy, long wz, ChunkProperty chunkProperty)
        {
            int key = chunkProperty.index;
            if (x0y0z0 == null)
            {
                FetchNeighbors();
            }

            // relative to us at 0
            long relx = wx - bx * biomeDataSizeX;
            long rely = wy - by * biomeDataSizeY;
            long relz = wz - bz * biomeDataSizeZ;

            float x0Weight = 1.0f - relx / (float)biomeDataSizeX;
            float y0Weight = 1.0f - rely / (float)biomeDataSizeY;
            float z0Weight = 1.0f - relz / (float)biomeDataSizeZ;

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


        public int blockLighting
        {
            get
            {
                int skyLighting, blockLighting,touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);
                return blockLighting;
            }
            private set
            {

            }
        }

        public int skyLighting
        {
            get
            {

                int skyLighting, blockLighting, touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);
                return skyLighting;
            }
            private set
            {

            }
        }

        public int touchingSkyFlags
        {
            get
            {

                int skyLighting, blockLighting, touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);
                return touchingSkyFlags;
            }
            private set
            {

            }
        }

        public bool touchingSky
        {
            get
            {

                int skyLighting, blockLighting, touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);
                return touchingSky;
            }
            private set
            {

            }
        }

        public int lightProduced
        {
            get
            {

                int skyLighting, blockLighting, touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);

                if (makingBlockLight)
                {
                    return blockLighting;
                }
                else
                {
                    return 0;
                }
            }
            set
            {

                int skyLighting, blockLighting, touchingSkyFlags, producedLighting;
                bool touchingSky, touchingTransparentOrAir, makingBlockLight;
                Chunk.GetLightingValues(lightingState, out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedLighting);

                // clamp from 0 to 15 (otherwise the flags will overflow and it'll overwrite other irrelevant lighting data since only 4 bits are alloted to this field)
                int clampedValue = System.Math.Min(15, System.Math.Max(0, value));
                if (value > 0)
                {
                    lightingState = Chunk.PackLightingValues(skyLighting, System.Math.Max(blockLighting, clampedValue), touchingSky, touchingTransparentOrAir, touchingSkyFlags, makingBlockLight: true, producedLight: clampedValue);
                }
                else
                {
                   lightingState = Chunk.PackLightingValues(skyLighting, blockLighting, touchingSky, touchingTransparentOrAir, touchingSkyFlags, makingBlockLight: false, producedLight: 0);
                }
            }
        }

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
        public long bx, by, bz;

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
                world.blockModifyState += 1;
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
                world.blockModifyState += 1;
            }
            else
            {
                myChunk[wx, wy, wz] = value;
                world.blockModifyState += 1;
            }
        }

        public int state { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState1) { cachedState1 = GetState(wx, wy, wz, cx, cy, cz, BlockState.State); curBlockModifyState1 = world.blockModifyState; } return cachedState1; } set { if (value != state) { wasModified = true; cachedState1 = value; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.State); curBlockModifyState1 = world.blockModifyState; CheckLocalStates(); } } }
        public int lightingState { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState2) { cachedState2 = GetState(wx, wy, wz, cx, cy, cz, BlockState.Lighting); curBlockModifyState2 = world.blockModifyState; } return cachedState2; } set { if (value != lightingState) { wasModified = true; cachedState2 = value; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.Lighting); curBlockModifyState2 = world.blockModifyState; CheckLocalStates(); } } }
        int animationState_ { get { if (!isGenerated) { return 0; } if (world.blockModifyState != curBlockModifyState3) { cachedState3 = GetState(wx, wy, wz, cx, cy, cz, BlockState.Animation); curBlockModifyState3 = world.blockModifyState; } return cachedState3; } set { if (value != animationState_) { wasModified = true; cachedState3 = value; cachedNumConnected = false; SetState(wx, wy, wz, cx, cy, cz, value, BlockState.Animation); curBlockModifyState3 = world.blockModifyState; CheckLocalStates(); } } }



        public short animationState {
            get
            {
                short animState, rotState;
                PhysicsUtils.UnpackValuesFromInt(animationState_, out animState, out rotState);
                return animState;
            }
            set {

                short animState, rotState;
                PhysicsUtils.UnpackValuesFromInt(animationState_, out animState, out rotState);
                animationState_ = PhysicsUtils.PackTwoValuesIntoInt(value, rotState);
            }
        }
        short rotationState_
        {
            get
            {
                short animState, rotState;
                PhysicsUtils.UnpackValuesFromInt(animationState_, out animState, out rotState);

                ushort rotRes = (ushort)rotState;

               
                return (short)rotRes.GetBits(minRotBitInclusive, maxRotBitExclusive);
            }
            set
            {

                ushort curConnectivityVal = (ushort)connectivityFlags;
                ushort actualVal = 0;
                actualVal = actualVal.SettingBits(minRotBitInclusive, maxRotBitExclusive, (ushort)value);
                actualVal = actualVal.SettingBits(minConnectedBitInclusive, maxConnectedBitExclusive, curConnectivityVal);

                animationState_ = PhysicsUtils.PackTwoValuesIntoInt(animationState, (short)actualVal);
            }
        }


        public int numConnected
        {
            get
            {
                if (!cachedNumConnected)
                {
                    int cflags = connectivityFlags;
                    int res = 0;
                    for (int i = 0; i < 24; i++)
                    {
                        if (((1 << i) & cflags) != 0)
                        {
                            res += 1;
                        }
                    }
                    numConnected_ = res;
                    cachedNumConnected = true;
                    return res;
                }
                else
                {
                    return numConnected_;
                }
            }
            private set
            {

            }
        }


        int numConnected_;
        bool cachedNumConnected = false;

        public int connectivityFlags
        {
            get
            {
                short animState, rotState;
                PhysicsUtils.UnpackValuesFromInt(animationState_, out animState, out rotState);

                ushort rotRes = (ushort)rotState;


                return (int)rotRes.GetBits(minConnectedBitInclusive, maxConnectedBitExclusive);
            }
            set
            {

                ushort curRotVal = (ushort)rotationState_;
                ushort actualVal = 0;
                actualVal = actualVal.SettingBits(minRotBitInclusive, maxRotBitExclusive, curRotVal);
                actualVal = actualVal.SettingBits(minConnectedBitInclusive, maxConnectedBitExclusive, (ushort)value);
                cachedNumConnected = false;

                animationState_ = PhysicsUtils.PackTwoValuesIntoInt(animationState, (short)actualVal);
            }
        }





        public BlockData.BlockRotation GetRelativeRotationOf(BlockData block)
        {
            int baseRotation = (int)PhysicsUtils.RotationToDegrees(rotation);
            int otherRotation = (int)PhysicsUtils.RotationToDegrees(block.rotation);
            // Good mod always returns a positive value
            int res = PhysicsUtils.GoodMod(baseRotation - otherRotation, 360);
            return (BlockRotation)PhysicsUtils.DegreesToRotation(res);
        }


        public void RotateOffsetRelativeToMe(long offX, long offY, long offZ, out long relativeOffX, out long relativeOffY, out long relativeOffZ)
        {
            PhysicsUtils.RotateOffsetRelativeToRotation(rotation, offX, offY, offZ, out relativeOffX, out relativeOffY, out relativeOffZ);
        }


        public void LocalOffsetToWorldOffset(long relativeOffX, long relativeOffY, long relativeOffZ, out long offsetX, out long offsetY, out long offsetZ)
        {
            BlockRotation invertedRotation = PhysicsUtils.DegreesToRotation(360 - PhysicsUtils.RotationToDegrees(rotation));
            PhysicsUtils.RotateOffsetRelativeToRotation(invertedRotation, relativeOffX, relativeOffY, relativeOffZ, out offsetX, out offsetY, out offsetZ);
        }


        public void GetRelativePosOf(BlockData block, out long relativeX, out long relativeY, out long relativeZ)
        {
            RotateOffsetRelativeToMe(block.x - x, block.y - y, block.z - z, out relativeX, out relativeY, out relativeZ);
        }



        /// <summary>
        /// Degrees counterclockwise (with respect to looking down) around the y axis 
        /// </summary>
        public enum BlockRotation
        {
            /// <summary>
            /// Applies no rotation
            /// </summary>
            Degrees0=0,
            /// <summary>
            /// Causes newX = oldZ
            /// newZ = -oldX
            /// </summary>
            Degrees90=1,
            /// <summary>
            /// Causes newX = -oldX
            /// newZ = -oldZ
            /// </summary>
            Degrees180=2,
            /// <summary>
            /// Causes newX = -oldZ
            /// newZ = oldX
            /// </summary>
            Degrees270=3
        }

        /*
        public static int[] offsets = new int[]
        {
            -1,-1,-1,
            0,-1,-1,
            1,-1,-1,

            -1,0,-1,
            0,0,-1,
            1,0,-1,

            -1,1,-1,
            0,1,-1,
            1,1,-1,




            -1,-1,0,
            0,-1,0,
            1,-1,0,

            -1,0,0,
            //0,0,0, we don't need a flag specifying connectivity to ourselves
            1,0,0,

            -1,1,0,
            0,1,0,
            1,1,0,




            -1,-1,1,
            0,-1,1,
            1,-1,1,

            -1,0,1,
            0,0,1,
            1,0,1,

            -1,1,1,
            0,1,1,
            1,1,1
        };

        */

        



        const int minRotBitInclusive = 0;
        const int maxRotBitExclusive = 2; // need 2 bits for 0,1,2,3 (4 possible rotation values)
        const int minConnectedBitInclusive = maxRotBitExclusive;
        const int maxConnectedBitExclusive = maxRotBitExclusive + 14; // need 14 bits for all possible connectivity flags (sideways, up, down, sideways up, sideways down)


        // because first 16 bits are anim state, second 16 bits are 
        public static BlockRotation RotationFromRawAnimInt(int animInt)
        {
            short a, b;
            PhysicsUtils.UnpackValuesFromInt(animInt, out a, out b);
            ushort val = (ushort)b;
            // See PhysicsUtils extension methods, this requires
            // using Blocks.ExtensionMethods
            ushort rotVal = val.GetBits(minRotBitInclusive, maxRotBitExclusive);
            return (BlockRotation)((int)rotVal);
        }

        public static int GetConnectivityFromRawAnimInt(int animInt)
        {
            short a, b;
            PhysicsUtils.UnpackValuesFromInt(animInt, out a, out b);
            ushort val = (ushort)b;
            // See PhysicsUtils extension methods, this requires
            // using Blocks.ExtensionMethods
            ushort connectionVal = val.GetBits(minConnectedBitInclusive, maxConnectedBitExclusive);
            return (int)connectionVal;
        }

        public static short AnimStateFromRawAnimInt(int animInt)
        {
            short a, b;
            PhysicsUtils.UnpackValuesFromInt(animInt, out a, out b);
            return a;
        }


        // degrees clockwise facing up around the y axis.
        public BlockRotation rotation
        {
            get
            {
                return RotationFromRawAnimInt(animationState_);
            }
            set
            {
                rotationState_ = (short)((int)value);
            }
        }

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
            ReassignValues(x, y, z);

            if (World.mainWorld.GetChunk(cx, cy, cz) == null)
            {
                isGenerated = false;
            }
            else
            {
                isGenerated = true;
            }
        }

        public bool isGenerated = false;

        public float GetChunkProperty(ChunkProperty chunkProperty)
        {
            if (chunkBiomeData == null)
            {
                chunkBiomeData = World.mainWorld.GetChunkBiomeData(bx, by, bz);
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
            this.cachedNumConnected = false;
            this.wx = x;
            this.wy = y;
            this.wz = z;
            myChunk = null;
            World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out this.cx, out this.cy, out this.cz);

            if (!isGenerated)
            {
                if (World.mainWorld.GetChunk(cx, cy, cz) == null)
                {
                    isGenerated = false;
                }
                else
                {
                    isGenerated = true;
                }
            }

            World.mainWorld.GetBiomeCoordinatesAtPos(x, y, z, out this.bx, out this.by, out this.bz);

            //cachedBlock = world[wx, wy, wz, cx, cy, cz];
            //this.curBlockModifyStateBlock = world.blockModifyState;
        }
    }


    public enum AxisDir
    {
        YPlus,
        YMinus,
        XPlus,
        XMinus,
        ZPlus,
        ZMinus,
        None
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


    public class RelativeBlockDataGetter
    {

        public World world;
        public BlockGetter blockGetter;
        public RelativeBlockDataGetter(World world, BlockGetter blockGetter)
        {
            this.world = world;
            this.blockGetter = blockGetter;
        }

        public RelativeBlockDataGetter()
        {
            this.world = World.mainWorld;
            this.blockGetter = World.mainWorld;
        }

        void GetWorldCoordinatesFromRelativeCoordinates(BlockData block, long relativeX, long relativeY, long relativeZ, out long x, out long y, out long z)
        {
            if (block.rotation == BlockData.BlockRotation.Degrees0)
            {
                x = block.x + relativeX;
                y = block.y + relativeY;
                z = block.z + relativeZ;
            }
            else if(block.rotation == BlockData.BlockRotation.Degrees90)
            {
                x = block.x - relativeZ;
                y = block.y + relativeY;
                z = block.z + relativeX;
            }
            else if(block.rotation == BlockData.BlockRotation.Degrees180)
            {
                x = block.x - relativeX;
                y = block.y + relativeY;
                z = block.z - relativeZ;
            }

            else if (block.rotation == BlockData.BlockRotation.Degrees270)
            {
                x = block.x + relativeZ;
                y = block.y + relativeY;
                z = block.z - relativeX;
            }
            else
            {
                x = block.x + relativeX;
                y = block.y + relativeY;
                z = block.z + relativeZ;
            }
        }

        public BlockValue GetBlockRelative(BlockData relativeTo, long relativeX, long relativeY, long relativeZ)
        {
            long x, y, z;
            GetWorldCoordinatesFromRelativeCoordinates(relativeTo, relativeX, relativeY, relativeZ, out x, out y, out z);
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return GetBlockNotRelative(x, y, z);
        }

        public BlockValue GetBlockNotRelative(long x, long y, long z)
        {
            return blockGetter[x, y, z];
        }

        public void SetBlockRelative(BlockData relativeTo, long relativeX, long relativeY, long relativeZ, BlockValue value)
        {
            long x, y, z;
            GetWorldCoordinatesFromRelativeCoordinates(relativeTo, relativeX, relativeY, relativeZ, out x, out y, out z);
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            SetBlockNotRelative(x, y, z, value);
        }

        public void SetBlockNotRelative(long x, long y, long z, BlockValue value)
        {
            blockGetter[x, y, z] = (int)value;
        }

        public int GetStateRelative(BlockData relativeTo, long relativeX, long relativeY, long relativeZ, BlockState stateType)
        {
            long x, y, z;
            GetWorldCoordinatesFromRelativeCoordinates(relativeTo, relativeX, relativeY, relativeZ, out x, out y, out z);
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return GetStateNotRelative(x, y, z, stateType);
        }

        public int GetStateNotRelative(long x, long y, long z, BlockState stateType)
        {
            return blockGetter.GetState(x, y, z, stateType);
        }

        public void SetStateRelative(BlockData relativeTo, long relativeX, long relativeY, long relativeZ, int state, BlockState stateType)
        {
            long x, y, z;
            GetWorldCoordinatesFromRelativeCoordinates(relativeTo, relativeX, relativeY, relativeZ, out x, out y, out z);
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            SetStateNotRelative(x, y, z, state, stateType);
        }

        public void SetStateNotRelative(long x, long y, long z, int state, BlockState stateType)
        {
            blockGetter.SetState(x, y, z, state, stateType);
        }


        public BlockData GetBlockDataRelative(BlockData relativeTo, long relativeX, long relativeY, long relativeZ)
        {
            long x, y, z;
            GetWorldCoordinatesFromRelativeCoordinates(relativeTo, relativeX, relativeY, relativeZ, out x, out y, out z);
            //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
            return GetBlockDataNotRelative(x, y, z);
        }

        public BlockData GetBlockDataNotRelative(long x, long y, long z)
        {
            return blockGetter.GetBlockData(x, y, z);
        }
    }


    public class SimpleBlock : Block
    {
        float baseBreakTime;
        Tuple<BlockValue, float>[] breakTimes;
        BlockValue block;
        public SimpleBlock(BlockValue block, float baseBreakTime, params Tuple<BlockValue, float>[] breakTimes)
        {
            this.block = block;
            this.baseBreakTime = baseBreakTime;
            this.breakTimes = breakTimes;
        }

        public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
        {
            return block;
        }


        public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
        {
            destroyBlock = true;
            CreateBlockEntity(block.block, positionOfBlock);
        }

        public override void OnTick(BlockData block)
        {

        }

        public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
        {
            for (int i = 0; i < breakTimes.Length; i++)
            {
                if (breakTimes[i].a == thingBreakingWith.Block)
                {
                    return breakTimes[i].b;
                }
            }
            return baseBreakTime;
        }
    }



    public abstract class Block : BaseBlockOrItem
    {

    }
    

    public abstract class Item : BaseBlockOrItem
    {
        public override bool CanBePlaced()
        {
            return false;
        }

        public override BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos)
        {
            return BlockValue.Air;
        }

        public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
        {
            destroyBlock = true;
        }
    }


    public abstract class BaseBlockOrItem : BlockOrItem
    {
        public override bool CanBePlaced()
        {
            return true;
        }

        public override void BlockStackAbove(BlockData block, BlockStack blockStack, Vector3 blockStackPos, out BlockStack consumedBlockStack)
        {
            consumedBlockStack = blockStack;
        }

        public override void MovingEntityAbove(BlockData block, MovingEntity movingEntity)
        {
            
        }

        public override bool CanConnect()
        {
            return false;
        }

        public override bool CanBePlaced(AxisDir facePlacedOn, LVector3 pos)
        {
            return true;
        }

        public override bool CanConnect(BlockData block, BlockData other, bool onSameYPlane, int numConnectedSoFar)
        {
            return false;
        }

        public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
        {
            CreateBlockEntity(block.block, positionOfBlock);
            destroyBlock = true;
        }

        public override Recipe[] GetRecipes()
        {
            return null;
        }

        public override Vector2 InventorySlotOffset(int slotNum)
        {
            return new Vector2(0, 0);
        }

        public override int InventorySpace()
        {
            return 0;
        }

        public override bool ReturnsItemsWhenDeselected()
        {
            return false;
        }

        public override Vector2 OutputSlotOffset(int outputNum)
        {
            return new Vector2(0, 0);
        }

        public override int NumCraftingOutputs()
        {
            return 0;
        }

        public override int NumInventoryRows()
        {
            return 1;
        }

        public override void OnRandomTick(BlockData block)
        {
            
        }

        public override void OnTick(BlockData block)
        {
            
        }

        public override int ConstantLightEmitted()
        {
            return 0;
        }

        public override void OnTickStart()
        {

        }

        public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
        {
            return 0.1f;
        }
    }

    public abstract class BlockOrItem : RelativeBlockDataGetter
    {
        public static System.Random randomGen = new System.Random();
        public float rand()
        {
            return (float)randomGen.NextDouble();
        }
        static int globalPreference = 0;

        public int stackSize = 1;

        Recipe[] recipes_ = null;

        public Recipe[] recipes
        {
            get
            {
                if (recipes_ == null)
                {
                    recipes_ = GetRecipes();
                }
                return recipes_;
            }
            private set
            {

            }
        }


        public abstract void MovingEntityAbove(BlockData block, MovingEntity movingEntity);

        public abstract void BlockStackAbove(BlockData block, BlockStack blockStack, Vector3 blockStackPos, out BlockStack consumedBlockStack);

        public IEnumerable<BlockData> GetNeighbors(BlockData block, bool includingUp = true, bool includingDown = true)
        {
            foreach (BlockData n in GetHorizontalNeighbors(block))
            {
                yield return n;
            }
            if (includingUp)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z)) yield return n;
            }
            if (includingDown)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z)) yield return n;
            }
        }

        public int GetConnectivityFlag(long offsetX, long offsetY, long offsetZ)
        {
            for (int i = 0; i < RotationUtils.NUM_CONNECTIVITY_OFFSETS; i++)
            {

                long curOffsetX = RotationUtils.CONNECTIVITY_OFFSETS[i * 3];
                long curOffsetY = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 1];
                long curOffsetZ = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 2];

                if (offsetX == curOffsetX && offsetY == curOffsetY && offsetZ == curOffsetZ)
                {
                    return (1 << i);
                }
            }
            return 0;
        }

        public void ForceAddConnectedTo(BlockData block, long wx, long wy, long wz)
        {
            long offsetX = wx - block.x;
            long offsetY = wy - block.y;
            long offsetZ = wz - block.z;

            block.connectivityFlags = block.connectivityFlags | GetConnectivityFlag(offsetX, offsetY, offsetZ);
        }

        public void UpdateConnections(BlockData block)
        {
            int connectedFlags = 0;
            int numConnected = 0;
            if (CanConnect())
            {
                for (int h = 0; h < RotationUtils.NUM_CONNECTIVITY_OFFSETS; h++)
                {
                    int offsetX = RotationUtils.CONNECTIVITY_OFFSETS[h * 3];
                    int offsetY = RotationUtils.CONNECTIVITY_OFFSETS[h * 3 + 1];
                    int offsetZ = RotationUtils.CONNECTIVITY_OFFSETS[h * 3 + 2];

                    using (BlockData neighborData = world.GetBlockData(block.x + offsetX, block.y + offsetY, block.z + offsetZ))
                    {
                        if (CanConnect(block, neighborData, offsetY == 0, numConnected))
                        {
                            connectedFlags = connectedFlags | (1 << h);
                            numConnected += 1;

                            if (offsetY != 0)
                            {
                                for (int w = 0; w < RotationUtils.NUM_CONNECTIVITY_OFFSETS; w++)
                                {
                                    int offsetX2 = RotationUtils.CONNECTIVITY_OFFSETS[w * 3];
                                    int offsetY2 = RotationUtils.CONNECTIVITY_OFFSETS[w * 3 + 1];
                                    int offsetZ2 = RotationUtils.CONNECTIVITY_OFFSETS[w * 3 + 2];

                                    if (offsetX2 == -offsetX && offsetY2 == -offsetY && offsetZ2 == -offsetZ)
                                    {
                                        if ((neighborData.connectivityFlags & (1 << w)) == 0)
                                        {
                                            world.AddBlockUpdate(block.x + offsetX, block.y + offsetY, block.z + offsetZ, alsoToNeighbors: true);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            block.connectivityFlags = connectedFlags;
        }


        public IEnumerable<BlockData> GetConnectedTo(BlockData block)
        {
            int connectivityFlags = block.connectivityFlags;
            for (int i = 0; i < RotationUtils.NUM_CONNECTIVITY_OFFSETS; i++)
            {
                if ((connectivityFlags & (1 << i)) != 0)
                {
                    int offsetX = RotationUtils.CONNECTIVITY_OFFSETS[i * 3];
                    int offsetY = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 1];
                    int offsetZ = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 2];

                    using (BlockData n = GetBlockDataNotRelative(block.x+offsetX, block.y +offsetY, block.z+offsetZ)) yield return n;
                }
            }
        }


        public IEnumerable<BlockData> GetConnectedToMe(BlockData block)
        {
            int connectivityFlags = block.connectivityFlags;
            for (int i = 0; i < RotationUtils.NUM_CONNECTIVITY_OFFSETS; i++)
            {
                int offsetX = RotationUtils.CONNECTIVITY_OFFSETS[i * 3];
                int offsetY = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 1];
                int offsetZ = RotationUtils.CONNECTIVITY_OFFSETS[i * 3 + 2];

                using (BlockData n = GetBlockDataNotRelative(block.x - offsetX, block.y - offsetY, block.z - offsetZ))
                {
                    if ((n.connectivityFlags & (1 << i)) != 0)
                    {
                        yield return n;
                    }
                }
            }
        }

        public IEnumerable<BlockData> GetHorizontalNeighbors(BlockData block)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
            }
            else if (globalPreference == 1)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
            }
            else if (globalPreference == 2)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
            }
            else if (globalPreference == 3)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
            }
        }

        public IEnumerable<BlockData> Get26Neighbors(BlockData block)
        {
            globalPreference = (globalPreference + 1) % 4;
            if (globalPreference == 0)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z - 1)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z - 1)) yield return n;
            }
            else if (globalPreference == 1)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z + 1)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z + 1)) yield return n;
            }
            else if (globalPreference == 2)
            {

                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z + 1)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z + 1)) yield return n;
            }
            else if (globalPreference == 3)
            {
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y, block.z + 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y, block.z - 1)) yield return n;

                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x, block.y + 1, block.z - 1)) yield return n;


                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y - 1, block.z + 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x + 1, block.y + 1, block.z - 1)) yield return n;
                using (BlockData n = GetBlockDataNotRelative(block.x - 1, block.y + 1, block.z - 1)) yield return n;
            }
        }


        public abstract int NumCraftingOutputs();
        public abstract int NumInventoryRows();
        public abstract bool ReturnsItemsWhenDeselected();
        public abstract int InventorySpace();
        public abstract Vector2 OutputSlotOffset(int outputNum);
        public abstract Vector2 InventorySlotOffset(int slotNum);
        public abstract Recipe[] GetRecipes();
        public abstract void OnTick(BlockData block);
        public abstract float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith);
        public abstract void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock);
        public abstract bool CanBePlaced();
        public virtual bool CanBePlaced(AxisDir facePlacedOn, LVector3 pos)
        {
            return CanBePlaced();
        }

        public abstract void OnRandomTick(BlockData block);

        public abstract void OnTickStart();
        public abstract BlockValue PlaceMe(AxisDir facePlacedOn, LVector3 pos);
        public abstract int ConstantLightEmitted();
        public abstract bool CanConnect(BlockData block, BlockData other, bool onSameYPlane, int numConnectedSoFar);
        public abstract bool CanConnect();

        public BlockEntity CreateBlockEntity(BlockValue block, Vector3 position)
        {
            if (block != BlockValue.Air)
            {
                GameObject blockEntity = GameObject.Instantiate(World.mainWorld.blocksWorld.blockEntityPrefab);
                blockEntity.transform.position = position;
                blockEntity.GetComponent<BlockEntity>().blockId = (int)block;
                return blockEntity.GetComponent<BlockEntity>();
            }
            else
            {
                return null;
            }
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
            lambda = System.Math.Max(World.biomeDataSizeX, World.biomeDataSizeZ) * World.biomeDataSizeY / avgNumBlocksBetween;
            this.eventCallback = eventCallback;
            this.priority = priority;
        }

        public void Run(long bx, long by, long bz)
        {
            LVector3[] points = GeneratePoints(bx, by, bz);
            foreach (LVector3 point in points)
            {
                using (BlockData blockData = World.mainWorld.GetBlockData(point.x, point.y, point.z))
                {
                    eventCallback(point.x, point.y, point.z, blockData);
                }
            }
        }

        // see https://en.wikipedia.org/wiki/Poisson_distribution#Probability_of_events_for_a_Poisson_distribution
        public LVector3[] GeneratePoints(long bx, long by, long bz)
        {
            float val = Simplex.Noise.Generate(bx, by, bz);
            int biomeDataSizeX = World.biomeDataSizeX;
            int biomeDataSizeY = World.biomeDataSizeY;
            int biomeDataSizeZ = World.biomeDataSizeZ;
            float totalPr = 0.0f;
            int factorial = 1;
            int numPoints = System.Math.Max(biomeDataSizeX, biomeDataSizeZ) * biomeDataSizeY;
            for (int i = 0; i < numPoints; i++)
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

            System.Random gen = new System.Random((int)(Simplex.Noise.Generate(bx, by, bz) * 1000.0f));
            //Debug.Log(cx + " " + cy + " " + cz + " got numPoints = " + numPoints + " with lambda = " + lambda + " with chunkSizeX= " + chunkSizeX + " chunkSizeY=" + chunkSizeY + " chunkSizeZ=" + chunkSizeZ);

            LVector3[] resPoints = new LVector3[numPoints];
            for (int i = 0; i < resPoints.Length; i++)
            {
                resPoints[i] = new LVector3(gen.Next(0, biomeDataSizeX) + bx * biomeDataSizeX, gen.Next(0, biomeDataSizeY) + by * biomeDataSizeY, gen.Next(0, biomeDataSizeZ) + bz * biomeDataSizeZ);
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
            int chunkSizeX = World.biomeDataSizeX;
            int chunkSizeY = World.biomeDataSizeY;
            int chunkSizeZ = World.biomeDataSizeZ;
            int sideLength = numChunksWide * System.Math.Max(System.Math.Max(chunkSizeX, chunkSizeY), chunkSizeZ);
            float randSeed = Simplex.Noise.rand(baseCX, baseCY, baseCZ);
            System.Random gen = new System.Random((int)(randSeed * 10000.0f));

            int numPointsToSample = HowManyPointsToSample(gen.NextDouble(), sideLength);
            Debug.Log("running world generation event with base chunk " + baseCX + " " + baseCY + " " + baseCZ + " and chunksWide=" + numChunksWide + " and numPointsToSample = " + numPointsToSample + " and avgNumBlocksBetween " + avgNumBlocksBetween);
            //Debug.Log(cx + " " + cy + " " + cz + " got numPoints = " + numPoints + " with lambda = " + lambda + " with chunkSizeX= " + chunkSizeX + " chunkSizeY=" + chunkSizeY + " chunkSizeZ=" + chunkSizeZ);
            long baseX = baseCX * chunkSizeX;
            long baseY = baseCY * chunkSizeY;
            long baseZ = baseCZ * chunkSizeZ;
            LVector3[] resPoints = new LVector3[numPointsToSample];
            for (int i = 0; i < resPoints.Length; i++)
            {
                long x = baseX + gen.Next(0, numChunksWide*chunkSizeX);
                long y = baseY + gen.Next(0, numChunksWide*chunkSizeY);
                long z = baseZ + gen.Next(0, numChunksWide*chunkSizeZ);
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
            int chunkSizeX = World.biomeDataSizeX;
            int chunkSizeY = World.biomeDataSizeY;
            int chunkSizeZ = World.biomeDataSizeZ;
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

        public float GenerateValue(long bx, long by, long bz)
        {
            if (usesY)
            {
                return Simplex.Noise.Generate(bx / scale, by / scale, bz / scale) * (maxVal - minVal) + minVal;
            }
            else
            {
                return Simplex.Noise.Generate(bx / scale, 0, bz / scale) * (maxVal - minVal) + minVal;
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

        public void RunEvents(long bx, long by, long bz)
        {
            foreach (ChunkPropertyEvent chunkEvent in chunkPropertyEvents)
            {
                chunkEvent.Run(bx, by,bz);
            }
        }

        public float[] GenerateChunkPropertiesArr(long bx, long by, long bz)
        {
            float[] res = new float[chunkProperties.Count];
            for (int i = 0; i < chunkProperties.Count; i++)
            {
                res[i] = chunkProperties[i].GenerateValue(bx, by, bz);
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

        public IntegerSet blocksNeedUpdatingWithLight;
        public IntegerSet blocksNeedUpdatingWithLightNextFrame;
        public volatile int threadRenderingMe = -1;
        public DoEveryMS needToDoAnotherTick = new DoEveryMS(10);
        public DoEveryMS needToDoRandomTick = new DoEveryMS(10);
        public List<PathingNode> pathingNodes = new List<PathingNode>();
        public List<PathingChunk> pathingChunks = new List<PathingChunk>();
        public bool mustRenderMe = false;
        public bool valid = true;
        public long cx, cy, cz;

        public long numTimesRendered = 0;

        public int indexUsingInBatch;


        public List<Tuple<long, long, long, int>> distancesOfPlacesToExpand;
        long lastWorldNumChunks;


        public PathingChunk GetPathingChunk(MobilityCriteria mobilityCritera, bool verbose = false)
        {
            PathingChunk res = null;
            foreach (PathingChunk pathingChunk in pathingChunks)
            {
                if (pathingChunk.mobilityCriteria.Equals(mobilityCritera))
                {
                    res = pathingChunk;
                    break;
                }
            }
            if (res == null)
            {
                res = new PathingChunk(this, mobilityCritera);
                pathingChunks.Add(res);
            }
            res.UpdateIfNeeded();
            return res;
        }

        public bool needToUpdateLighting = false;

        Chunk x0Chunk;
        Chunk y0Chunk;
        Chunk z0Chunk;
        Chunk x1Chunk;
        Chunk y1Chunk;
        Chunk z1Chunk;


        public void FetchNeighboringChunks()
        {
            if (x0Chunk == null) x0Chunk = world.GetChunk(cx - 1, cy, cz);
            if (y0Chunk == null) y0Chunk = world.GetChunk(cx, cy - 1, cz);
            if (z0Chunk == null) z0Chunk = world.GetChunk(cx, cy, cz - 1);
            if (x1Chunk == null) x1Chunk = world.GetChunk(cx + 1, cy, cz);
            if (y1Chunk == null) y1Chunk = world.GetChunk(cx, cy + 1, cz);
            if (z1Chunk == null) z1Chunk = world.GetChunk(cx, cy, cz + 1);
        }

        public object modifyLock = new object();
        public object modifyLock2 = new object();

        public int UpdateLightingForAllBlocks()
        {
            FetchNeighboringChunks();
            int numLightings = 0;
            lock (modifyLock2)
            {
                lock(modifyLock)
                {
                    if (!needToUpdateLighting)
                    {
                        return numLightings;
                    }
                    else
                    {
                        needToUpdateLighting = false;
                    }
                    IntegerSet tmp = blocksNeedUpdatingWithLight;
                    blocksNeedUpdatingWithLight = blocksNeedUpdatingWithLightNextFrame;
                    blocksNeedUpdatingWithLightNextFrame = tmp;
                }

                if (blocksNeedUpdatingWithLightNextFrame.Count > 0)
                {
                    for (int i = 0; i < blocksNeedUpdatingWithLightNextFrame.Count; i++)
                    {
                        World.mainWorld.blocksWorld.lightingTicksThisFrame += 1;
                        numLightings += 1;
                        int ind = blocksNeedUpdatingWithLightNextFrame[i];
                        int x, y, z;
                        chunkData.to3D(ind, out x, out y, out z);
                        try
                        {
                            long wx = x + chunkSizeX * cx;
                            long wy = y + chunkSizeY * cy;
                            long wz = z + chunkSizeZ * cz;
                            int skyLighting;
                            int blockLighting;
                            int touchingSkyFlags;
                            bool touchingSky;
                            bool oldTouchingTransparentOrAir;
                            bool makingBlockLight;
                            int producedBlockLight;
                            int oldTouchingSkyFlags;
                            int lightingStateRawValue = chunkData.GetState(x, y, z, BlockState.Lighting);
                            GetLightingValues(lightingStateRawValue, out skyLighting, out blockLighting, out touchingSky, out oldTouchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedBlockLight);
                            int highestSkyLighting = 0;
                            int highestBlockLighting = 0;
                            oldTouchingSkyFlags = touchingSkyFlags;
                            // check neighbors to see if we need to trickle their light values
                            bool touchingTransparentOrAir = false;
                            bool iAmAir = chunkData.GetBlock(x, y, z) == BlockValue.Air;
                            UpdateTouchingNeighbors(wx, wy, wz, x, y, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);

                            bool lightModified = false;

                            if (touchingSkyFlags != oldTouchingSkyFlags)
                            {
                                lightModified = true;
                            }

                            // neighbors light has changed so we need to trickle their values
                            if (skyLighting < highestSkyLighting - 1 || blockLighting < highestBlockLighting - 1 || (touchingTransparentOrAir != oldTouchingTransparentOrAir) || (touchingSkyFlags != oldTouchingSkyFlags))
                            {
                                skyLighting = System.Math.Max(skyLighting, highestSkyLighting - 1);
                                blockLighting = System.Math.Max(blockLighting, highestBlockLighting - 1);
                                lightModified = true;
                            }

                            if (!touchingSky)
                            {
                                if (highestSkyLighting <= skyLighting && skyLighting > 0)
                                {
                                    skyLighting = System.Math.Max(0, highestSkyLighting - 1);
                                    lightModified = true;
                                }
                            }


                            // neighbor's light has changed and we don't produce light, we need to trickle
                            if (!makingBlockLight && blockLighting > System.Math.Max(0, highestBlockLighting - 1))
                            {
                                blockLighting = System.Math.Max(0, highestBlockLighting - 1);
                                lightModified = true;
                            }

                            if (lightModified)
                            {
                                int resLightingState = PackLightingValues(skyLighting, blockLighting, touchingSky, touchingTransparentOrAir, touchingSkyFlags, makingBlockLight: makingBlockLight, producedLight: producedBlockLight);
                                //numLightUpdated += 1;
                                AddLightingUpdatesToBlock(x, y, z);
                                AddLightingUpdatesToNeighbors(x, y, z);
                                needToUpdateLighting = true;
                                bool tmpBlah;
                                chunkData.SetState(x, y, z, resLightingState, BlockState.Lighting, out tmpBlah);
                                chunkData.needToBeUpdated = true;
                                //world.AddBlockUpdateToNeighbors(wx, wy, wz);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log("has ind " + ind + " and local pos " + x + " " + y + " " + z + " with chunk data size " + chunkData.GetRawData().Length + " and error " + e);
                            throw e;
                        }
                    }

                    blocksNeedUpdatingWithLightNextFrame.Clear();
                }

            }

            return numLightings;
        }



        public void AddLightingUpdatesToBlock(LVector3 pos)
        {
            int localX = (int)(pos.x - cx * chunkSizeX);
            int localY = (int)(pos.y - cy * chunkSizeY);
            int localZ = (int)(pos.z - cz * chunkSizeZ);
            AddLightingUpdates(chunkData.to1D(localX, localY, localZ));
        }

        public void AddLightingUpdatesToBlock(int localX, int localY, int localZ)
        {
            AddLightingUpdates(chunkData.to1D(localX, localY, localZ));
        }

        public void AddLightingUpdatesToNeighbors(LVector3 pos)
        {
            FetchNeighboringChunks();
            int localX = (int)(pos.x - cx * chunkSizeX);
            int localY = (int)(pos.y - cy * chunkSizeY);
            int localZ = (int)(pos.z - cz * chunkSizeZ);
            AddLightingUpdatesToNeighbors(localX, localY, localZ);
        }

        public void AddLightingUpdates(int i)
        {
            lock(modifyLock)
            {
                blocksNeedUpdatingWithLight.Add(i);
                //blocksNeedUpdatingWithLightNextFrame.Add(i);
                needToUpdateLighting = true;
                chunkData.needToBeUpdated = true;
            }
        }

        public void AddLightingUpdatesToNeighbors(int localX, int localY, int localZ)
        {
            FetchNeighboringChunks();
            int x = localX;
            int y = localY;
            int z = localZ;
            if (x == 0) { if (x0Chunk != null) { x0Chunk.AddLightingUpdates(chunkData.to1D(chunkSizeX - 1, y, z)); } }
            else { AddLightingUpdates(chunkData.to1D(x - 1, y, z)); }
            if (y == 0) { if (y0Chunk != null) { y0Chunk.AddLightingUpdates(chunkData.to1D(x, chunkSizeY - 1, z)); } }
            else { AddLightingUpdates(chunkData.to1D(x, y - 1, z)); }
            if (z == 0) { if (z0Chunk != null) { z0Chunk.AddLightingUpdates(chunkData.to1D(x, y, chunkSizeZ - 1));  } }
            else { AddLightingUpdates(chunkData.to1D(x, y, z - 1)); }
            if (x == chunkSizeX - 1) { if (x1Chunk != null) { x1Chunk.AddLightingUpdates(chunkData.to1D(0, y, z));  } }
            else { AddLightingUpdates(chunkData.to1D(x + 1, y, z)); }
            if (y == chunkSizeY - 1) { if (y1Chunk != null) { y1Chunk.AddLightingUpdates(chunkData.to1D(x, 0, z)); } }
            else { AddLightingUpdates(chunkData.to1D(x, y + 1, z)); }
            if (z == chunkSizeZ - 1) { if (z1Chunk != null) { z1Chunk.AddLightingUpdates(chunkData.to1D(x, y, 0)); } }
            else { AddLightingUpdates(chunkData.to1D(x, y, z + 1)); }
        }

        public PathingNode GetPathingNode(int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight, bool verbose=false)
        {
            // see if we have already made one for the right body specs
            foreach (PathingNode pathingNode in pathingNodes)
            {
                if (pathingNode.neededSizeSide ==  neededSizeSide && pathingNode.neededSizeForward == neededSizeForward && pathingNode.neededSizeUp == neededSizeUp && pathingNode.jumpHeight == jumpHeight)
                {
                    // refresh it if we have been modified
                    if (pathingNode.editNum != editNum)
                    {
                        pathingNode.Refresh();
                        pathingNode.editNum = editNum;
                    }

                    return pathingNode;
                }
            }

            // we have not, we need to make one
            PathingNode res = new PathingNode(world, new PathingNodeBlockChunk(world,
                cx * World.chunkSizeX, cy * World.chunkSizeY, cz * World.chunkSizeZ,
                cx * World.chunkSizeX + World.chunkSizeX - 1, cy * World.chunkSizeY + World.chunkSizeY - 1, cz * World.chunkSizeZ + World.chunkSizeZ - 1), neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight);

            pathingNodes.Add(res);

            res.Refresh(verbose);

            res.editNum = editNum;

            return res;
        }

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



        public const int chunkSizeX = World.chunkSizeX;
        public const int chunkSizeY = World.chunkSizeY;
        public const int chunkSizeZ = World.chunkSizeZ;

        public ChunkData chunkData;
        public ChunkRenderer chunkRenderer;
        public ChunkBiomeData[,,] chunkBiomeDatas;
        public bool createdBiomeData = false;
        public object biomeDataLock = new object();


        public void InitBiomeData(ChunkProperties chunkProperties)
        {
            lock(biomeDataLock)
            {
                int biomeDatasPerX = chunkSizeX / World.biomeDataSizeX;
                int biomeDatasPerY = chunkSizeY / World.biomeDataSizeY;
                int biomeDatasPerZ = chunkSizeZ / World.biomeDataSizeZ;


                chunkBiomeDatas = new ChunkBiomeData[biomeDatasPerX,biomeDatasPerY,biomeDatasPerZ];


                long wx = cx * chunkSizeX;
                long wy = cy * chunkSizeY;
                long wz = cz * chunkSizeZ;

                long baseBx = world.divWithFloor(wx, World.biomeDataSizeX);
                long baseBy = world.divWithFloor(wy, World.biomeDataSizeY);
                long baseBz = world.divWithFloor(wz, World.biomeDataSizeZ);

                for (int bx = 0; bx < biomeDatasPerX; bx++)
                {
                    for (int by = 0; by < biomeDatasPerY; by++)
                    {
                        for (int bz = 0; bz < biomeDatasPerZ; bz++)
                        {
                            LVector3 biomePos = new LVector3(baseBx + bx, baseBy + by, baseBz + bz);
                            if (world.ungeneratedBiomeDatas.ContainsKey(biomePos))
                            {
                                chunkBiomeDatas[bx, by, bz] = world.ungeneratedBiomeDatas[biomePos];
                                world.ungeneratedBiomeDatas.Remove(biomePos);
                            }
                            else
                            {
                                chunkBiomeDatas[bx, by, bz] = new ChunkBiomeData(chunkProperties, World.biomeDataSizeX, World.biomeDataSizeY, World.biomeDataSizeZ, baseBx + bx, baseBy + by, baseBz + bz);
                            }
                        }
                    }
                }


            }
        }


        public ChunkBiomeData GetBiomeData(long x, long y, long z)
        {

            long baseBx = world.divWithFloor(cx * chunkSizeX, World.biomeDataSizeX);
            long baseBy = world.divWithFloor(cy * chunkSizeY, World.biomeDataSizeY);
            long baseBz = world.divWithFloor(cz * chunkSizeZ, World.biomeDataSizeZ);

            long bx = world.divWithFloor(x, World.biomeDataSizeX);
            long by = world.divWithFloor(y, World.biomeDataSizeY);
            long bz = world.divWithFloor(z, World.biomeDataSizeZ);

            long relativeBx = bx - baseBx;
            long relativeBy = by - baseBy;
            long relativeBz = bz - baseBz;

            if (cx == 0 && cy == 0 && (cz == 0 || cz == -1))
            {
                //Debug.Log("got spookers " + cx + " " + cy + " " + cz + " " + relativeBx + " " + relativeBy + " " + relativeBz + " " + baseBx + " " + baseBy + " " + baseBz + " " + x + " " + y + " " + z + " " + bx + " " + by + " " + bz);
            }


            if (chunkBiomeDatas == null)
            {
                Debug.LogWarning("chunk biome datas for chunk is null " + chunkBiomeDatas);
                return null;
            }
            else if (chunkBiomeDatas[relativeBx, relativeBy, relativeBz] == null)
            {
                Debug.LogWarning("chunk biome datas specific value is null " + chunkBiomeDatas[relativeBx, relativeBy, relativeBz]);
                return null;
            }
            return chunkBiomeDatas[relativeBx, relativeBy, relativeBz];
        }

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



        long editNum
        {
            get
            {
                return chunkData.editNum;
            }
            set
            {
                chunkData.editNum = value;
            }
        }

        public bool TryGetHighestSolidBlockY(long x, long z, out long highestBlockY)
        {
            highestBlockY = long.MinValue;
            long relativeX = x - cx * chunkSizeX;
            long relativeZ = z - cz * chunkSizeZ;
            for (int y = chunkSizeY-1; y >= 0; y--)
            {
                if (chunkData[relativeX, y, relativeZ] != BlockValue.Air)
                {
                    highestBlockY = y+cy*chunkSizeY;
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
            long relativeX = i - cx * chunkSizeX;
            long relativeY = j - cy * chunkSizeY;
            long relativeZ = k - cz * chunkSizeZ;
            chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
            //blocksNeedUpdatingWithLight.Add(chunkData.to1D((int)relativeX, (int)relativeY, (int)relativeZ));
            //blocksNeedUpdatingWithLightNextFrame.Add(chunkData.to1D((int)relativeX, (int)relativeY, (int)relativeZ));
            //needToUpdateLighting = true;
            chunkData.needToBeUpdated = true;
        }


        public void CreateStuff()
        {
            this.chunkRenderer = new ChunkRenderer(this);
        }


        public Chunk(World world, ChunkProperties chunkProperties, long chunkX, long chunkY, long chunkZ, bool createStuff = true)
        {
            this.blocksNeedUpdatingWithLight = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
            this.blocksNeedUpdatingWithLightNextFrame = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
            this.world = world;
            this.chunkData = new ChunkData(fillWithWildcard: false);
            this.chunkData.attachedChunks.Add(this);
            this.cx = chunkX;
            this.cy = chunkY;
            this.cz = chunkZ;
            posChunks = new Chunk[] { null, null, null };
            negChunks = new Chunk[] { null, null, null };


            generating = true;

            if (createStuff)
            {
                CreateStuff();
            }

        }


        public const int TOUCHING_SKY_MASK = 255;
        public void Generate()
        {
            generating = true;
            world.blocksWorld.generationsThisFrame += 1;
            long baseX = cx * chunkSizeX;
            long baseY = cy * chunkSizeY;
            long baseZ = cz * chunkSizeX;
            Structure myStructure = new Structure(cx + " " + cy + " " + cz, true, this, priority:0);
            //long start = PhysicsUtils.millis();
            //Debug.Log("generating chunk " + cx + " " + cy + " " + cz + " ");
            try
            {
                //lock (modifyLock)
                {
                    world.worldGeneration.blockGetter = myStructure;
                    for (long x = baseX; x < baseX + World.chunkSizeX; x++)
                    {
                        for (long z = baseZ; z < baseZ + World.chunkSizeZ; z++)
                        {
                            //long curHighestBlockY = long.MinValue;
                            //world.worldGeneration.blockGetter = world;
                            //bool wasAPreviousHighest = world.TryGetHighestSolidBlockY(x, z, out curHighestBlockY);
                            //world.worldGeneration.blockGetter = myStructure;
                            //float elevation = world.AverageChunkValues(x, 0, z, c => c.chunkProperties["elevation"]);
                            // going from top to bottom lets us update the "highest block touching the sky" easily
                            for (long y = baseY + World.chunkSizeY - 1; y >= baseY; y--)
                            {
                                //long elevation = (long)Mathf.Round(world.AverageChunkValues(x, 0, z, "altitude"));
                                using (BlockData block = myStructure.GetBlockData(x, y, z))
                                {
                                    world.worldGeneration.OnGenerateBlock(x, y, z, block);
                                    block.animationState = 0;
                                    block.lightingState = 0;
                                    if (block.block != BlockValue.Wildcard && block.block != BlockValue.Air)
                                    {
                                        BlockOrItem customBlock;
                                        if (world.customBlocks.ContainsKey(block.block, out customBlock) && customBlock.ConstantLightEmitted() > 0)
                                        {
                                            block.lightProduced = customBlock.ConstantLightEmitted();
                                            AddLightingUpdatesToBlock((int)(x - baseX), (int)(y - baseY), (int)(z - baseZ));
                                            AddLightingUpdatesToNeighbors((int)(x - baseX), (int)(y - baseY), (int)(z - baseZ));
                                        }
                                        //chunkData.blocksNeedUpdating.Add((int)chunkData.to1D(x - cx * chunkSizeX, y - cy * chunkSizeY, z - cz * chunkSizeZ));
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

                    long baseXPos = cx * chunkSizeX;
                    long baseYPos = cy * chunkSizeY;
                    long baseZPos = cz * chunkSizeZ;

                    long baseBx = world.divWithFloor(baseXPos, World.biomeDataSizeX);
                    long baseBy = world.divWithFloor(baseYPos, World.biomeDataSizeY);
                    long baseBz = world.divWithFloor(baseZPos, World.biomeDataSizeZ);

                    int biomeDatasPerX = chunkSizeX / World.biomeDataSizeX;
                    int biomeDatasPerY = chunkSizeY / World.biomeDataSizeY;
                    int biomeDatasPerZ = chunkSizeZ / World.biomeDataSizeZ;
                    for (int bx = 0; bx < biomeDatasPerX; bx++)
                    {
                        for (int by = 0; by < biomeDatasPerY; by++)
                        {
                            for (int bz = 0; bz < biomeDatasPerZ; bz++)
                            {
                                ChunkBiomeData chunkBiomeData = chunkBiomeDatas[bx, by, bz];
                                chunkBiomeData.chunkPropertiesObj.chunkPropertyEvents.Sort((x, y) =>
                                {
                                    return x.priority.CompareTo(y.priority);
                                });

                                // give each event a seperate structure with the correct priority so the filling in priority algorithms elsewhere will overwrite properly
                                foreach (ChunkPropertyEvent chunkPropertyEvent in chunkBiomeData.chunkPropertiesObj.chunkPropertyEvents)
                                {
                                    Structure myStructure2 = new Structure(cx + " " + cy + " " + cz + " " + bx + " " + by + " " + bz, true, this, priority: chunkPropertyEvent.priority);
                                    world.worldGeneration.blockGetter = myStructure2;
                                    chunkPropertyEvent.Run(bx + baseBx, by + baseBy, bz + baseBz);
                                    if (!myStructure2.HasAllChunksGenerated())
                                    {
                                        world.AddUnfinishedStructure(myStructure2);
                                    }

                                }
                            }
                        }
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
            long baseX = cx * chunkSizeX;
            long baseY = cy * chunkSizeY;
            long baseZ = cz * chunkSizeZ;
            Structure myStructure = new Structure(cx + " " + cy + " " + cz, true);
            world.worldGeneration.blockGetter = myStructure;
            for (long x = baseX; x < baseX + this.chunkSizeX; x++)
            {
                for (long z = baseZ; z < baseZ + this.chunkSizeZ; z++)
                {
                    long elevation = (long)Mathf.Round(world.AverageChunkValues(x, 0, z, c => c.altitude));
                    for (long y = baseY; y < baseY + this.chunkSizeY; y++)
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


            lock (modifyLock)
            {
                for (int i = 0; i < chunkSizeX * chunkSizeY * chunkSizeZ; i++)
                {
                    //blocksNeedUpdatingWithLight.Add(i);
                }
                needToUpdateLighting = true;
            }
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
                //this.valid = false;
                //newValidOne.valid = true;

            }
            valid = true;
        }



        public const int MAKING_BLOCK_LIGHT_BIT = 1 << 10;
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT = 1 << 9;

        public const int OFFSET_FOR_TOUCHING_TRANSPARENT_OR_AIR_BIT_SIDES = 16;
        public const int OFFSET_FOR_PRODUCED_LIGHT = 12;
        // bottom, top
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_NY = 1 << 16;
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_PY = 1 << 17;
        // front, back
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_NZ = 1 << 18;
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_PZ = 1 << 19;
        // left, right
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_NX = 1 << 20;
        public const int TOUCHING_TRANPARENT_OR_AIR_BIT_PX = 1 << 21;

        public const int TOUCHING_SKY_BIT = 1 << 8;
        public const int SKY_LIGHTING_MASK = 0xF0;
        public const int BLOCK_LIGHTING_MASK = 0xF;
        public const int GENERATED_LIGHTING_MASK = 0xF000; // 0b1111000000000000

        /// <summary>
        /// Automatically sets blockLighting to producedLight if makingBlockLight is true and producedLight is greater than blockLight
        /// </summary>
        /// <param name="skyLighting"></param>
        /// <param name="blockLighting"></param>
        /// <param name="touchingSky"></param>
        /// <param name="touchingTransparentOrAir"></param>
        /// <param name="makingBlockLight"></param>
        /// <param name="producedLight"></param>
        /// <returns></returns>
        public static int PackLightingValues(int skyLighting, int blockLighting, bool touchingSky, bool touchingTransparentOrAir, int touchingTransparentOrAirFlags, bool makingBlockLight=false, int producedLight=0)
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

            if (makingBlockLight)
            {
                res = res | MAKING_BLOCK_LIGHT_BIT;
                // produced lighting is bits 12-15
                producedLight = System.Math.Min(System.Math.Max(producedLight, 0), 15);
                res = res | (producedLight << 12);

                if (blockLighting < producedLight)
                {
                    blockLighting = producedLight;
                }
            }

            skyLighting = System.Math.Min(System.Math.Max(skyLighting, 0), 15);

            res = res | (skyLighting << 4) | blockLighting  | (touchingTransparentOrAirFlags);

            

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetLightingValues(int lightingState, out int skyLighting, out int blockLighting, out bool touchingSky, out bool touchingTransparentOrAir, out bool makingBlockLight, out int touchingSkyFlags, out int producedLighting)
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

            makingBlockLight = (MAKING_BLOCK_LIGHT_BIT & lightingState) != 0;

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

            // 0x3F is 0b111111 (six ones)
            touchingSkyFlags = (lightingState & (0x3F << OFFSET_FOR_TOUCHING_TRANSPARENT_OR_AIR_BIT_SIDES));

            // produced lighting is bits 12-15
            if (makingBlockLight)
            {
                producedLighting = (lightingState & GENERATED_LIGHTING_MASK) >> 12;
                producedLighting = System.Math.Min(15, System.Math.Max(producedLighting, 0));
                if (producedLighting > blockLighting)
                {
                    blockLighting = producedLighting;
                }
            }
            else
            {
                producedLighting = 0;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetHighestLightings(long x, long y, long z, int offsetFlag, ref int curHighestSkyLight, ref int curHighestBlockLight, bool aboveNeighbor, ref bool touchingTransparentOrAir, bool iAmAir, ref int touchingSkyFlags)
        {
            int lightingState = chunkData.GetState(x, y, z, BlockState.Lighting);
            bool isSolid = chunkData.GetBlock(x, y, z) > 0;
            if (!isSolid)
            {
                touchingTransparentOrAir = true;
                // set the flag to one
                touchingSkyFlags |= offsetFlag;
            }
            else
            {
                // set that flag to zero by anding with bitwise not of it
                touchingSkyFlags = ((~offsetFlag) & touchingSkyFlags);
            }

            int neighborSkyLighting = (lightingState & SKY_LIGHTING_MASK) >> 4;
            int neighborBlockLighting = (lightingState & BLOCK_LIGHTING_MASK);
            bool neighborTouchingSky = (lightingState & TOUCHING_SKY_BIT) != 0;
            bool neighborMakingBlockLight = (MAKING_BLOCK_LIGHT_BIT & lightingState) != 0;

            if (!isSolid || (iAmAir && (neighborMakingBlockLight || (neighborTouchingSky && aboveNeighbor))))
            {
                if (neighborSkyLighting > curHighestSkyLight)
                {
                    curHighestSkyLight = neighborSkyLighting;
                }
                if (neighborBlockLighting > curHighestBlockLight)
                {
                    curHighestBlockLight = neighborBlockLighting;
                }
            }
        }


        Chunk negX, posX, negY, posY, negZ, posZ;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolid(int block)
        {
            return block > 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetHighestLightingsOutsideChunk(long wx, long wy, long wz, int offsetFlag, ref int curHighestSkyLight, ref int curHighestBlockLight, bool aboveNeighbor, Chunk chunk, ref bool touchingTransparentOrAir, bool iAmAir, ref int touchingSkyFlags)
        {
            if (chunk != null && !chunk.generating)
            {

                int lightingState = chunk.GetState(wx, wy, wz, BlockState.Lighting);
                bool isSolid = chunk[wx, wy, wz] > 0;
                if (!isSolid)
                {
                    touchingTransparentOrAir = true;
                    // set the flag to one
                    touchingSkyFlags |= offsetFlag;
                }
                else
                {
                    // set that flag to zero by anding with bitwise not of it
                    touchingSkyFlags = ((~offsetFlag) & touchingSkyFlags);
                }

                int neighborSkyLighting = (lightingState & SKY_LIGHTING_MASK) >> 4;
                int neighborBlockLighting = (lightingState & BLOCK_LIGHTING_MASK);
                bool neighborTouchingSky = (lightingState & TOUCHING_SKY_BIT) != 0;
                bool neighborMakingBlockLight = (MAKING_BLOCK_LIGHT_BIT & lightingState) != 0;

                if (!isSolid || (iAmAir && (neighborMakingBlockLight || (neighborTouchingSky && aboveNeighbor))))
                {
                    if (neighborSkyLighting > curHighestSkyLight)
                    {
                        curHighestSkyLight = neighborSkyLighting;
                    }
                    if (neighborBlockLighting > curHighestBlockLight)
                    {
                        curHighestBlockLight = neighborBlockLighting;
                    }
                }
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

        public void UpdateLighting(long wx, long wy, long wz)
        {
            using (BlockData block = world.GetBlockData(wx, wy, wz))
            {
                UpdateLighting(block);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateTouchingNeighbors(long wx, long wy, long wz, long x, long y, long z, ref int highestSkyLighting, ref int highestBlockLighting, ref bool touchingTransparentOrAir, bool iAmAir, ref int touchingSkyFlags)
        {

            // this code needed to be a little gross because it needs to be very fast so ideally we want to not use the world lookup unless we have to since usually we'll be inside this chunk
            if (x == 0) GetHighestLightingsOutsideChunk(wx - 1, wy, wz, TOUCHING_TRANPARENT_OR_AIR_BIT_NX, ref highestSkyLighting, ref highestBlockLighting, false, x0Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x - 1, y, z, TOUCHING_TRANPARENT_OR_AIR_BIT_NX, ref highestSkyLighting, ref highestBlockLighting, false, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            if (y == 0) GetHighestLightingsOutsideChunk(wx, wy - 1, wz, TOUCHING_TRANPARENT_OR_AIR_BIT_NY, ref highestSkyLighting, ref highestBlockLighting, true, y0Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x, y - 1, z, TOUCHING_TRANPARENT_OR_AIR_BIT_NY, ref highestSkyLighting, ref highestBlockLighting, true, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            if (z == 0) GetHighestLightingsOutsideChunk(wx, wy, wz - 1, TOUCHING_TRANPARENT_OR_AIR_BIT_NZ, ref highestSkyLighting, ref highestBlockLighting, false, z0Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x, y, z - 1, TOUCHING_TRANPARENT_OR_AIR_BIT_NZ, ref highestSkyLighting, ref highestBlockLighting, false, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);

            if (x == chunkSizeX - 1) GetHighestLightingsOutsideChunk(wx + 1, wy, wz, TOUCHING_TRANPARENT_OR_AIR_BIT_PX, ref highestSkyLighting, ref highestBlockLighting, false, x1Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x + 1, y, z, TOUCHING_TRANPARENT_OR_AIR_BIT_PX, ref highestSkyLighting, ref highestBlockLighting, false, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            if (y == chunkSizeY - 1) GetHighestLightingsOutsideChunk(wx, wy + 1, wz, TOUCHING_TRANPARENT_OR_AIR_BIT_PY, ref highestSkyLighting, ref highestBlockLighting, false, y1Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x, y + 1, z, TOUCHING_TRANPARENT_OR_AIR_BIT_PY, ref highestSkyLighting, ref highestBlockLighting, false, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            if (z == chunkSizeZ - 1) GetHighestLightingsOutsideChunk(wx, wy, wz + 1, TOUCHING_TRANPARENT_OR_AIR_BIT_PZ, ref highestSkyLighting, ref highestBlockLighting, false, z1Chunk, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            else GetHighestLightings(x, y, z + 1, TOUCHING_TRANPARENT_OR_AIR_BIT_PZ, ref highestSkyLighting, ref highestBlockLighting, false, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);

        }

        public void UpdateLighting(BlockData block)
        {
            long wx = block.x;
            long wy = block.y;
            long wz = block.z;
            long x = block.x - chunkSizeX * cx;
            long y = block.y - chunkSizeY * cy;
            long z = block.z - chunkSizeZ * cz;
            block.myChunk = this;
            int skyLighting;
            int blockLighting;
            int touchingSkyFlags;
            bool touchingSky;
            bool oldTouchingTransparentOrAir;
            bool makingBlockLight;
            int producedBlockLight;
            int oldTouchingSkyFlags;
            GetLightingValues(block.lightingState, out skyLighting, out blockLighting, out touchingSky, out oldTouchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags, out producedBlockLight);
            int highestSkyLighting = 0;
            int highestBlockLighting = 0;
            oldTouchingSkyFlags = touchingSkyFlags;
            // check neighbors to see if we need to trickle their light values
            bool touchingTransparentOrAir = false;
            bool iAmAir = block.block == BlockValue.Air;

            UpdateTouchingNeighbors(wx, wy, wz, x, y, z, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);
            bool lightModified = false;

            if (touchingSkyFlags != oldTouchingSkyFlags)
            {
                lightModified = true;
            }

            // neighbors light has changed so we need to trickle their values
            if (skyLighting < highestSkyLighting - 1 || blockLighting < highestBlockLighting - 1 || (touchingTransparentOrAir != oldTouchingTransparentOrAir) || (touchingSkyFlags != oldTouchingSkyFlags))
            {
                skyLighting = System.Math.Max(skyLighting, highestSkyLighting - 1);
                blockLighting = System.Math.Max(blockLighting, highestBlockLighting - 1);
                lightModified = true;
            }

            if (!touchingSky)
            {
                if (highestSkyLighting <= skyLighting && skyLighting > 0)
                {
                    skyLighting = System.Math.Max(0, highestSkyLighting - 1);
                    lightModified = true;
                }
            }

            
            /*
            // we produce light, but neighbor is brighter
            if (makingBlockLight && highestBlockLighting-1 >= producedBlockLight)
            {
                blockLighting = System.Math.Max(highestBlockLighting - 1, 0);
                lightModified = true;
            }
            */


            // neighbor's light has changed and we don't produce light, we need to trickle
            if (!makingBlockLight && blockLighting > System.Math.Max(0, highestBlockLighting-1))
            {
                blockLighting = System.Math.Max(0, highestBlockLighting - 1);
                lightModified = true;
            }


            if (chunkData.blocksNeedUpdating.Count == 7)
            {
                // Debug.Log("block update " + cx + " " + cy + " " + cz + "      " + wx + " " + wy + " " + wz + "       " + x + " " + y + " " + z + " " + touchingTransparentOrAir + " " + oldTouchingTransparentOrAir + " " + World.BlockToString(blockValue));
            }
            if (lightModified)
            {
                block.lightingState = PackLightingValues(skyLighting, blockLighting, touchingSky, touchingTransparentOrAir, touchingSkyFlags, makingBlockLight: makingBlockLight, producedLight: producedBlockLight);
                //numLightUpdated += 1;
                chunkData.needToBeUpdated = true;
                AddLightingUpdatesToBlock((int)x, (int)y, (int)z);
                AddLightingUpdatesToNeighbors((int)x, (int)y, (int)z);
            }
        }

        public bool Tick(long frameId, bool allowGenerate, bool allowTick, ref int maxMillisInFrame)
        {

            needToDoAnotherTick.ms = (int)(1000.0 / world.blocksWorld.ticksPerSecond);
            needToDoRandomTick.ms = (int)(1000.0 / world.blocksWorld.randomTicksPerSecond);
            if (cleanedUp)
            {
                Debug.LogWarning("Chunk " + cx + " " + cy + " " + cz + " is already cleaned up, and yet is having Tick() ran on it, did you forget to remove the reference somewhere?");
                return false;
            }
            bool didGenerate = false;
            if (generating)
            {
                if (allowGenerate)
                {
                    this.TickStart(frameId);
                    long curTime = PhysicsUtils.millis();
                    Generate();
                    long elapsedTime = PhysicsUtils.millis() - curTime;
                    //Debug.Log("took " + elapsedTime + " time to generate chunk " + cx + " " + cy + " " + cz);
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

            if (!valid)
            {
                //return false;
            }

            this.chunkRenderer.Tick();

            if (!allowTick) // || !needToDoAnotherTick.Do())
            {
                return didGenerate;
            }


            bool doRandomTick = false;
            if (needToDoRandomTick.Do())
            {
                doRandomTick = true;
            }

            this.TickStart(frameId);

            int numLightUpdated = 0;
            if (chunkData.blocksNeedUpdating.Count != 0)
            {
                //lock (modifyLock)
                {
                    //Debug.Log("chunk " + cx + " " + cy + " " + cz + " has " + chunkData.blocksNeedUpdating.Count + " block updates");

                    bool modifiedSomething = false;
                    //bool relevantChunk = true;
                    //int k = 0;
                    for (int j = 0; j < chunkData.blocksNeedUpdating.Count; j++)
                    {
                        int i = chunkData.blocksNeedUpdating[j];

                        world.blocksWorld.ticksThisFrame += 1;
                        //if (PhysicsUtils.millis() - world.frameTimeStart > maxMillisInFrame)
                        // {
                        //     chunkData.blocksNeedUpdatingNextFrame.Add(i);
                        //     continue;
                        //}
                        long ind = (long)(i);
                        long x, y, z;
                        chunkData.to3D(ind, out x, out y, out z);
                        long wx = x + cx * chunkSizeX;
                        long wy = y + cy * chunkSizeY;
                        long wz = z + cz * chunkSizeZ;
                        //long mwx, mwy, mwz;
                        //PhysicsUtils.ModPos(wx, wy, wz, out mwx, out mwy, out mwz);
                        using (BlockData block = world.GetBlockData(wx, wy, wz))
                        {
                            block.myChunk = this;
                            BlockValue oldBlockValue = block.block;
                            int oldLightingState = block.lightingState;
                            int oldAnimationState = block.animationState;
                            int oldState = block.state;
                            //world.blocksWorld.lightingTicksThisFrame += 1;
                            //UpdateLighting(block);
                            BlockOrItem customBlock;
                            BlockValue blockValue = block.block;
                            if (world.customBlocks.ContainsKey(blockValue, out customBlock))
                            {
                                customBlock.OnTick(block);

                                if (doRandomTick)
                                {
                                    customBlock.OnRandomTick(block);
                                }
                            }

                            if (block.needsAnotherTick)
                            {
                                chunkData.blocksNeedUpdatingNextFrame.Add((int)ind);
                            }


                            if (oldBlockValue != block.block)
                            {
                                chunkData.blocksNeedUpdatingNextFrame.Add((int)ind);
                            }
                            if (oldLightingState != block.lightingState)
                            {
                                AddLightingUpdates(i);
                                //blocksNeedUpdatingWithLight.Add(i);
                                //blocksNeedUpdatingWithLightNextFrame.Add(i);
                                AddLightingUpdatesToNeighbors((int)x, (int)y, (int)z);
                                //needToUpdateLighting = true;
                                //chunkData.needToBeUpdated = true;
                            }

                            if (block.WasModified || oldAnimationState != block.animationState || oldLightingState != block.lightingState || oldState != block.state || oldBlockValue != block.block)
                            {
                                modifiedSomething = true;
                                // don't call lots of chunk lookups if we don't need to
                                if (x == 0) AddBlockUpdateOutsideChunk(wx - 1, wy, wz, ref negX);
                                else chunkData.AddBlockUpdate(x - 1, y, z);
                                if (y == 0) AddBlockUpdateOutsideChunk(wx, wy - 1, wz, ref negY);
                                else chunkData.AddBlockUpdate(x, y - 1, z);
                                if (z == 0) AddBlockUpdateOutsideChunk(wx, wy, wz - 1, ref negZ);
                                else chunkData.AddBlockUpdate(x, y, z - 1);

                                if (x == chunkSizeX - 1) AddBlockUpdateOutsideChunk(wx + 1, wy, wz, ref posX);
                                else chunkData.AddBlockUpdate(x + 1, y, z);
                                if (y == chunkSizeY - 1) AddBlockUpdateOutsideChunk(wx, wy + 1, wz, ref posY);
                                else chunkData.AddBlockUpdate(x, y + 1, z);
                                if (z == chunkSizeZ - 1) AddBlockUpdateOutsideChunk(wx, wy, wz + 1, ref posZ);
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
                                (x != 0 && x != chunkSizeX - 1) &&
                                (y != 0 && y != chunkSizeY - 1) &&
                                (z != 0 && z != chunkSizeZ - 1);

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

                    if(modifiedSomething)
                    {
                        chunkData.needToBeUpdated = true;
                    }

                    //Debug.Log("updated " + chunkData.blocksNeedUpdating.Count + " blocks with " + numLightUpdated + " lighting updates " + cx + " " + cy + " " + cz);

                }
            }


            //chunkData.blocksNeedUpdating.Clear();

            return didGenerate;
        }


        public bool generating = true;


        public void SetState(long x, long y, long z, int state, BlockState stateType)
        {
            long relativeX = x - cx * chunkSizeX;
            long relativeY = y - cy * chunkSizeY;
            long relativeZ = z - cz * chunkSizeZ;
            bool addedUpdate;
            long curEditNum = chunkData.editNum;
            chunkData.SetState(relativeX, relativeY, relativeZ, state, stateType, out addedUpdate);
            if (stateType == BlockState.Lighting && chunkData.editNum != curEditNum)
            {
                AddLightingUpdates((int)chunkData.to1D(relativeX, relativeY, relativeZ));
            }
            // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
            if (!generating && addedUpdate)
            {
                world.AddBlockUpdateToNeighbors(x, y, z);
                chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
            }
        }


        public void SetNeighborsAsTouchingAir(long x, long y, long z)
        {
            long relativeX = x - cx * chunkSizeX;
            long relativeY = y - cy * chunkSizeY;
            long relativeZ = z - cz * chunkSizeZ;
            if (relativeX == 0) world.SetTouchingAir(x - 1, y, z, true);
            else SetTouchingAir(x - 1, y, z, true);

            if (relativeY == 0) world.SetTouchingAir(x, y - 1, z, true);
            else SetTouchingAir(x, y - 1, z, true);

            if (relativeZ == 0) world.SetTouchingAir(x, y, z - 1, true);
            else SetTouchingAir(x, y, z - 1, true);


            if (relativeX == chunkSizeX - 1) world.SetTouchingAir(x + 1, y, z, true);
            else SetTouchingAir(x + 1, y, z, true);

            if (relativeY == chunkSizeY - 1) world.SetTouchingAir(x, y + 1, z, true);
            else SetTouchingAir(x, y + 1, z, true);

            if (relativeZ == chunkSizeZ - 1) world.SetTouchingAir(x, y, z + 1, true);
            else SetTouchingAir(x, y, z + 1, true);

        }




        public void SetTouchingAir(long x, long y, long z, bool value)
        {

            UpdateLighting(x, y, z);
            return;
            /*
            long relativeX = x - cx * chunkSizeX;
            long relativeY = y - cy * chunkSizeY;
            long relativeZ = z - cz * chunkSizeZ;
            bool modified;

            int skyLighting, blockLighting, touchingSkyFlags;
            bool touchingSky, touchingTransparentOrAir, makingBlockLight;
            GetLightingValues(chunkData.GetState(relativeX, relativeY, relativeZ, BlockState.Lighting), out skyLighting, out blockLighting, out touchingSky, out touchingTransparentOrAir, out makingBlockLight, out touchingSkyFlags);
            int highestSkyLighting = 0;
            int highestBlockLighting = 0;
            int oldTouchingSkyFlags = touchingSkyFlags;
            bool oldTouchingTransparentOrAir = touchingTransparentOrAir;
            // check neighbors to see if we need to trickle their light values
            bool iAmAir = chunkData.GetBlock(relativeX, relativeY, relativeZ) == BlockValue.Air;

            chunkData.SetTouchingAir(relativeX, relativeY, relativeZ, value, out modified);
            touchingTransparentOrAir = false;
            UpdateTouchingNeighbors(x, y, z, relativeX, relativeY, relativeZ, ref highestSkyLighting, ref highestBlockLighting, ref touchingTransparentOrAir, iAmAir, ref touchingSkyFlags);

            if (makingBlockLight)
            {
                highestBlockLighting = 15;
            }

            if (touchingSkyFlags != oldTouchingSkyFlags || oldTouchingTransparentOrAir != touchingTransparentOrAir)
            {
                touchingTransparentOrAir = value;
                bool addedUpdate;
                chunkData.SetState(relativeX, relativeY, relativeZ, PackLightingValues(highestSkyLighting, highestBlockLighting, touchingSky, touchingTransparentOrAir, touchingSkyFlags), BlockState.Lighting, out addedUpdate);
                chunkData.needToBeUpdated = true;
            }

            // if was not visible and is now, we need to get the right lighting value
            if (modified && value)
            {
                //UpdateLighting(x, y, z);
                //chunkData.needToBeUpdated = true;
            }
            */

        }

        public int GetState(long x, long y, long z, BlockState stateType)
        {
            long relativeX = x - cx * chunkSizeX;
            long relativeY = y - cy * chunkSizeY;
            long relativeZ = z - cz * chunkSizeZ;
            return chunkData.GetState(relativeX, relativeY, relativeZ, stateType);
        }

        public int this[long x, long y, long z]
        {
            get
            {
                long relativeX = x - cx * chunkSizeX;
                long relativeY = y - cy * chunkSizeY;
                long relativeZ = z - cz * chunkSizeZ;
                return chunkData[relativeX, relativeY, relativeZ];
            }
            set
            {
                //this.threadRenderingMe = 100; // so we don't get renders until we are done 
                long relativeX = x - cx * chunkSizeX;
                long relativeY = y - cy * chunkSizeY;
                long relativeZ = z - cz * chunkSizeZ;
                bool addedUpdate;

                int prev = 0;

                // if we are already generated, do lighting updates on modification
                if (!generating)
                {
                    prev = chunkData[relativeX, relativeY, relativeZ];
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

                // after we assigned the value, do some more updates:
                if (!generating)
                {
                    // clear flags that specify that it makes lighting
                    int prevLighting = chunkData.GetState(relativeX, relativeY, relativeZ, BlockState.Lighting);
                    if ((prevLighting & Chunk.MAKING_BLOCK_LIGHT_BIT) != 0)
                    {
                        prevLighting = prevLighting & (~Chunk.MAKING_BLOCK_LIGHT_BIT);
                        prevLighting = prevLighting & (~Chunk.GENERATED_LIGHTING_MASK);
                        bool addedUpdatef;
                        chunkData.SetState(relativeX, relativeY, relativeZ, prevLighting, BlockState.Lighting, out addedUpdatef);
                        UpdateLighting(x, y, z);
                    }

                    // turning solid into non-solid (less than 0 is transparent, greater than 0 is solid, 0 is empty)
                    if (prev > 0 && value <= 0)
                    {
                        // we need to update "am i touching air" flag immediately (we can't wait for the block update) since it affects rendering
                        SetNeighborsAsTouchingAir(x, y, z);
                    }
                    // turning non-solid into solid
                    if (prev <= 0 && value > 0)
                    {
                    }
                    // turned transparent into air, set lighting to 0 and update
                    if (prev < 0 && value == 0)
                    {
                        //SetNeighborsAsTouchingAir(x, y, z);
                    }
                }
                // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
                if (!generating && addedUpdate)
                {
                    world.AddBlockUpdateToNeighbors(x, y, z);
                    chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
                    //Debug.Log("added update");
                }

                // we are all done with fiddling, allow other render threads to update us now
                //this.threadRenderingMe = -1;
            }
        }

        object chunkSaveLock = new object();
        long editNumWrittenToDisk = -5;
        public bool WriteToDisk()
        {
            if (editNumWrittenToDisk != chunkData.editNum && !generating)
            {
                lock(chunkSaveLock)
                {
                    if (editNumWrittenToDisk != chunkData.editNum && !generating)
                    {
                        string rootDir = World.currentWorldDir;
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
                        string chunkName = cx + "." + cy + "." + cz + ".dat";
                        chunkData.WriteToRLEFile(chunksDir + "/" + chunkName);
                        Debug.Log("Writing chunk " + cx + " " + cy + " " + cz + " to " + chunksDir + "/" + chunkName);
                        editNumWrittenToDisk = chunkData.editNum;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
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
        public const int chunkSizeX = World.chunkSizeX;
        public const int chunkSizeY = World.chunkSizeY;
        public const int chunkSizeZ = World.chunkSizeZ;


        public List<Chunk> attachedChunks = new List<Chunk>();



        public static int[] ToRunLengthEncodedData(int[] originalData)
        {
            int[] result = new int[originalData.Length*2]; // worst case it is twice as long (if every value is distinct)

            // first two values are size, then a 0
            result[0] = originalData.Length;
            result[1] = 0;
            int resultI = 2;
            int curValue = 0;
            int curCount = 0;
            for (int i = 0; i < originalData.Length; i++)
            {
                if (originalData[i] == curValue)
                {
                    curCount += 1;
                }
                else if(curCount > 0)
                {
                    result[resultI] = curValue;
                    result[resultI+1] = curCount;
                    curValue = originalData[i];
                    curCount = 1;
                    resultI = resultI + 2;
                }
            }

            int[] actualResult = new int[resultI];
            for (int i = 0; i < resultI; i++)
            {
                actualResult[i] = result[i];
            }
            return actualResult;
        }

        public static int[] FromRunLengthEncodedData(int[] rleData)
        {
            // first two values are size, then a zero
            int originalSize = rleData[0];
            int[] result = new int[originalSize];
            int iInResult = 0;
            for (int i = 2; i < rleData.Length; i+=2)
            {
                int val = rleData[i];
                int count = rleData[i + 1];
                for (int j = 0; j < count; j++)
                {
                    result[iInResult] = val;
                    iInResult += 1;
                }
            }
            return result;
        }

        public ChunkData(bool fillWithWildcard = false)
        {
            data = new int[chunkSizeX * chunkSizeY * chunkSizeZ * 4];
            this.blocksNeedUpdating = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
            this.blocksNeedUpdatingNextFrame = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
            if (fillWithWildcard)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (int)BlockValue.Wildcard;
                }
            }
        }

        public ChunkData(int[] data)
        {
            this.data = data;
            this.blocksNeedUpdating = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
            this.blocksNeedUpdatingNextFrame = new IntegerSet(chunkSizeX * chunkSizeY * chunkSizeZ);
        }


        public void WriteToRLEFile(string path)
        {
            int[] rleData = ToRunLengthEncodedData(data);
            // convert ints to bytes
            byte[] byteData = new byte[sizeof(int) * rleData.Length];
            System.Buffer.BlockCopy(rleData, 0, byteData, 0, byteData.Length); // size is in number of bytes
            // write to file
            File.WriteAllBytes(path, byteData);
        }

        public void ReadFromRLEFile(string path)
        {
            // read from file
            byte[] byteData = File.ReadAllBytes(path);
            int[] rleData = new int[byteData.Length / sizeof(int)];
            System.Buffer.BlockCopy(byteData, 0, rleData, 0, rleData.Length * sizeof(int)); // size is in number of bytes
            this.data = FromRunLengthEncodedData(rleData);
            this.needToBeUpdated = true;
        }

        public void CopyIntoChunk(Chunk chunk, int priority=0)
        {
            editNum += 1;
            int[] chunkData = chunk.chunkData.data;
            int totalLen = System.Math.Min(data.Length, chunkData.Length);
            for (int i = 0; i < totalLen; i++)
            {
                // if we are on a block, check if wildcard
                if (i % 4 == 0)
                {
                    bool skipAhead = false;
                    // if base generation and something else has already filled this in, skip ahead
                    if (priority == 0 && (chunkData[i] != BlockValue.Wildcard && chunkData[i] != BlockValue.Air))
                    {
                        skipAhead = true;
                    }
                    // if we are not a whildcard, assign us and also assign the chunk internal states 
                    else if (data[i] != (int)BlockValue.Wildcard && data[i] != chunkData[i])
                    {
                        chunkData[i] = data[i];
                        blocksNeedUpdatingNextFrame.Add(i / 4);
                        int localX, localY, localZ;
                        to3D(i / 4, out localX, out localY, out localZ);
                        long wx = chunk.cx * chunkSizeX + localX;
                        long wy = chunk.cy * chunkSizeY + localY;
                        long wz = chunk.cz * chunkSizeZ + localZ;
                        if (chunkData[i] > 0 && data[i] <= 0 && (localX == 0 || localX == chunkSizeX-1 || localY == 0 || localY == chunkSizeY-1 || localZ == 0 || localZ == chunkSizeZ-1))
                        {
                            if (localX == 0) chunk.world.CheckIfTouchingAir(wx - 1, wy, wz);
                            if (localY == 0) chunk.world.CheckIfTouchingAir(wx, wy - 1, wz);
                            if (localZ == 0) chunk.world.CheckIfTouchingAir(wx, wy, wz - 1);
                            if (localX == chunkSizeX - 1) chunk.world.CheckIfTouchingAir(wx + 1, wy, wz);
                            if (localY == chunkSizeY - 1) chunk.world.CheckIfTouchingAir(wx, wy + 1, wz);
                            if (localZ == chunkSizeZ - 1) chunk.world.CheckIfTouchingAir(wx, wy, wz + 1);

                            //chunk.world.AddBlockUpdateToNeighbors(chunk.cx * chunkSizeX + localX, chunk.cy * chunkSizeY + localY, chunk.cz * chunkSizeZ + localZ);
                        }
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
                // State
                else if (i % 4 == 1)
                {
                    chunkData[i] = data[i];
                }
                // Lighting
                else if(i % 4 == 2)
                {
                    if (data[i] != chunkData[i])
                    {
                        chunk.AddLightingUpdates(i / 4);
                    }
                }
                // Animation State
                else if(i % 4 == 3)
                {
                    chunkData[i] = data[i];
                }
            }
            needToBeUpdated = true;
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
            z = ind / (chunkSizeX * chunkSizeY);
            ind -= (z * chunkSizeX * chunkSizeY);
            y = ind / chunkSizeX;
            x = ind % chunkSizeX;

            /*
            z = ind / (chunkSize_2);
            ind -= (z * chunkSize_2);
            y = ind / chunkSize;
            x = ind % chunkSize;
            */
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int to1D(int x, int y, int z)
        {
            return x + y * chunkSizeX + z * chunkSizeX * chunkSizeY;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void to3D(long ind, out long x, out long y, out long z)
        {
            z = ind / (chunkSizeX * chunkSizeY);
            ind -= (z * chunkSizeX * chunkSizeY);
            y = ind / chunkSizeX;
            x = ind % chunkSizeX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long to1D(long x, long y, long z)
        {
            return x + y * chunkSizeX + z * chunkSizeX * chunkSizeY;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetState(long i, long j, long k, BlockState stateType)
        {
            //try
            //{
                long ind = to1D(i, j, k);
                return data[ind * 4 + (int)stateType];
            //}
            //catch (System.Exception e)
            //{
            //    long ind = to1D(i, j, k);
            //    Debug.Log("failed with data len " + data.Length + " and ind " + ind +" and local " + i + " " + j + " " + k);
            //    throw e;
            //}
        }

        public void SetState(long i, long j, long k, int state, BlockState stateType, out bool addedBlockUpdate, bool forceBlockUpdate = false)
        {
            addedBlockUpdate = false;
            long ind = to1D(i, j, k);
            if (data[ind * 4 + (int)stateType] != state)
            {
                editNum += 1;
                needToBeUpdated = true;
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
            int val = (int)to1D(i, j, k);
            if (val < data.Length && val >= 0)
            {
                blocksNeedUpdatingNextFrame.Add(val);
            }
            else
            {
                throw new System.Exception("error: tried to add update with invalid relative pos " + i + " " + j + " " + k);

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlock(long i, long j, long k)
        {
            long ind = to1D(i, j, k);
            return data[ind * 4];
        }

        public void SetTouchingAir(long i, long j, long k, bool value, out bool modified)
        {
            int oldValue = GetState(i, j, k, BlockState.Lighting);
            bool wasTouchingAir = (oldValue & Chunk.TOUCHING_TRANPARENT_OR_AIR_BIT) != 0;
            modified = wasTouchingAir != value;
            // clear the bit
            int newValue = oldValue & (~Chunk.TOUCHING_TRANPARENT_OR_AIR_BIT);
            if (value)
            {
                // set it to 1 if the value is true
                newValue = newValue | Chunk.TOUCHING_TRANPARENT_OR_AIR_BIT;
            }
            bool a;
            // update the lighting state if we modified it
            if (modified)
            {
                long ind = to1D(i, j, k);
                this.data[ind * 4 + (int)BlockState.Lighting] = newValue;
                //SetState(i, j, k, newValue, BlockState.Lighting, out a);
            }
        }

        public void SetBlock(long i, long j, long k, int block, out bool addedBlockUpdate, bool forceBlockUpdate = false)
        {
            addedBlockUpdate = false;
            long ind = to1D(i, j, k);
            if (data[ind * 4] != block)
            {
                editNum += 1;
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

        public long editNum = -1;


        public int this[long i, long j, long k]
        {
            get
            {
                long ind = (i + j * chunkSizeX + k * chunkSizeX*chunkSizeY)*4;
                if (ind < 0 || ind >= data.Length)
                {
                    Debug.LogWarning("warning: out of range, with local " + i + " " + j + " " + k + " and data " + data.Length);
                    return 0;
                }
                return data[(i + j * chunkSizeX + k * chunkSizeX * chunkSizeY) * 4];
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

        public BlockData GetBlockData(LVector3 pos)
        {
            return blockDataCache.GetNewBlockData(pos.x, pos.y, pos.z);
        }

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
        public string uid;
        public SavedChunk[] savedChunks;

        public SavedStructure()
        {

        }

        public SavedStructure(string name, string uid, bool madeInGeneration, SavedChunk[] savedChunks, int priority)
        {
            this.name = name;
            this.uid = uid;
            this.madeInGeneration = madeInGeneration;
            this.savedChunks = savedChunks;
            this.priority = priority;
        }
    }
    [System.Serializable]
    public class SavedChunk
    {
        public int[] chunkDataRLE;
        public long cx, cy, cz;

        public SavedChunk()
        {

        }

        public SavedChunk(int[] chunkData, long cx, long cy, long cz)
        {
            this.chunkDataRLE = ChunkData.ToRunLengthEncodedData(chunkData);
            this.cx = cx;
            this.cy = cy;
            this.cz = cz;
        }
    }

    public class Structure : BlockGetter
    {
        public string name;
        public bool madeInGeneration;
        public string uid;
        Dictionary<LVector3, ChunkData> ungeneratedChunkPositions;

        public static HashSet<string> allUids;

        public static string getNewUID()
        {
            if (allUids == null)
            {
                allUids = new HashSet<string>();
            }
            string curUID = System.Guid.NewGuid().ToString();
            while (allUids.Contains(curUID))
            {
                curUID = System.Guid.NewGuid().ToString();
            }
            allUids.Add(curUID);
            return curUID;

        }

        public int priority;
        public Chunk baseChunk;

        public Structure(string name, bool madeInGeneration, Chunk baseChunk, int priority=0)
        {
            this.priority = priority;
            this.name = name;
            this.uid = getNewUID();
            this.baseChunk = baseChunk;
            this.blockDataCache = new BlockDataCache(this);
            ungeneratedChunkPositions = new Dictionary<LVector3, ChunkData>();
            this.madeInGeneration = madeInGeneration;
        }


        public Structure(SavedStructure savedStructure)
        {
            if (allUids == null)
            {
                allUids = new HashSet<string>();
            }
            name = savedStructure.name;
            priority = savedStructure.priority;
            uid = savedStructure.uid;
            allUids.Add(uid);
            this.baseChunk = null;
            this.blockDataCache = new BlockDataCache(this);
            ungeneratedChunkPositions = new Dictionary<LVector3, ChunkData>();
            this.madeInGeneration = savedStructure.madeInGeneration;
            for (int i = 0; i < savedStructure.savedChunks.Length; i++)
            {
                SavedChunk savedChunk = savedStructure.savedChunks[i];
                LVector3 savedChunkPos = new LVector3(savedChunk.cx, savedChunk.cy, savedChunk.cz);
                int[] savedChunkData = ChunkData.FromRunLengthEncodedData(savedChunk.chunkDataRLE);

                ChunkData resSavedChunkData = new ChunkData(savedChunkData);
                ungeneratedChunkPositions[savedChunkPos] = resSavedChunkData;

            }
        }


        public bool HasAllChunksGenerated()
        {
            return ungeneratedChunkPositions.Count == 0;
        }

        bool savedToFile = false;

        object saveLock = new object();


        public void RemoveFromDisk()
        {
            string rootDir = World.currentWorldDir;
            DirectoryInfo rootInfo = new DirectoryInfo(rootDir);
            if (!rootInfo.Exists)
            {
                Directory.CreateDirectory(rootInfo.FullName);
            }
            string cleanedRootDir = rootInfo.FullName.Replace("\\", "/");
            string savedStructuresDir = cleanedRootDir + "/savedStructures";
            DirectoryInfo savedStructuresDirInfo = new DirectoryInfo(savedStructuresDir);
            if (!savedStructuresDirInfo.Exists)
            {
                Directory.CreateDirectory(savedStructuresDirInfo.FullName);
            }
            string filePath = savedStructuresDir + "/" + uid + ".dat";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool WriteToDisk()
        {
            if (!savedToFile)
            {
                lock (saveLock)
                {
                    if (!savedToFile && ungeneratedChunkPositions.Count > 0)
                    {
                        Debug.Log("savivng structure " + uid);
                        string rootDir = World.currentWorldDir;
                        DirectoryInfo rootInfo = new DirectoryInfo(rootDir);
                        if (!rootInfo.Exists)
                        {
                            Directory.CreateDirectory(rootInfo.FullName);
                        }
                        string cleanedRootDir = rootInfo.FullName.Replace("\\", "/");
                        string savedStructuresDir = cleanedRootDir + "/savedStructures";
                        DirectoryInfo savedStructuresDirInfo = new DirectoryInfo(savedStructuresDir);
                        if (!savedStructuresDirInfo.Exists)
                        {
                            Directory.CreateDirectory(savedStructuresDirInfo.FullName);
                        }
                        WriteToFile(savedStructuresDir + "/" + uid + ".dat");
                        savedToFile = true;
                        return true;
                    }
                    else if(ungeneratedChunkPositions.Count == 0)
                    {
                        RemoveFromDisk();
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public void WriteToFile(string filePath)
        {
            File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(ToSavedStructure()));
        }

        public SavedStructure ToSavedStructure()
        {
            List<SavedChunk> savedChunks = new List<SavedChunk>();
            // for multithreaded accesss
            List<KeyValuePair<LVector3, ChunkData>> ungeneratedChunkPositionsCopy = new List<KeyValuePair<LVector3, ChunkData>>();
            lock (ungeneratedChunkPositions)
            {
                foreach (KeyValuePair<LVector3, ChunkData> savedChunk in ungeneratedChunkPositions)
                {
                    ungeneratedChunkPositionsCopy.Add(savedChunk);
                }
            }

            foreach (KeyValuePair<LVector3, ChunkData> savedChunk in ungeneratedChunkPositionsCopy)
            {
                savedChunks.Add(new SavedChunk(savedChunk.Value.GetRawData(), savedChunk.Key.x, savedChunk.Key.y, savedChunk.Key.z));
            }


            return new SavedStructure(name, uid, madeInGeneration, savedChunks.ToArray(), priority);
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
                lock(ungeneratedChunkPositions)
                {
                    ungeneratedChunkPositions.Remove(chunkPos);
                }
            }


            if (ungeneratedChunkPositions.Count == 0)
            {
                RemoveFromDisk();
            }
            else
            {
                savedToFile = false;
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
                    int chunkSizeX = World.chunkSizeX;
                    int chunkSizeY = World.chunkSizeY;
                    int chunkSizeZ = World.chunkSizeZ;
                    if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                    {
                        ungeneratedChunkPositions[chunkPos] = new ChunkData(fillWithWildcard: true);
                    }
                    long localPosX = x - chunkPos.x * chunkSizeX;
                    long localPosY = y - chunkPos.y * chunkSizeY;
                    long localPosZ = z - chunkPos.z * chunkSizeZ;
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

                    int chunkSizeX = World.chunkSizeX;
                    int chunkSizeY = World.chunkSizeY;
                    int chunkSizeZ = World.chunkSizeZ;
                    if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                    {
                        ungeneratedChunkPositions[chunkPos] = new ChunkData(fillWithWildcard: true);
                    }
                    long localPosX = x - chunkPos.x * chunkSizeX;
                    long localPosY = y - chunkPos.y * chunkSizeY;
                    long localPosZ = z - chunkPos.z * chunkSizeZ;
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

                        int chunkSizeX = World.chunkSizeX;
                        int chunkSizeY = World.chunkSizeY;
                        int chunkSizeZ = World.chunkSizeZ;
                        if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                        {
                            ungeneratedChunkPositions[chunkPos] = new ChunkData(fillWithWildcard: true);
                        }
                        long localPosX = x - chunkPos.x * chunkSizeX;
                        long localPosY = y - chunkPos.y * chunkSizeY;
                        long localPosZ = z - chunkPos.z * chunkSizeZ;
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
                        int chunkSizeX = World.chunkSizeX;
                        int chunkSizeY = World.chunkSizeY;
                        int chunkSizeZ = World.chunkSizeZ;
                        if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                        {
                            ungeneratedChunkPositions[chunkPos] = new ChunkData(fillWithWildcard: true);
                        }
                        long localPosX = x - chunkPos.x * chunkSizeX;
                        long localPosY = y - chunkPos.y * chunkSizeY;
                        long localPosZ = z - chunkPos.z * chunkSizeZ;
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
            count = 0;
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
                for (int i = 0; i < count; i++)
                {
                    int j = (i + frontPos) % list.Length;
                    newList[i] = list[j];
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
                T res = list[(frontPos + count - 1) % list.Length];
                count -= 1;
                return res;
            }
        }
    }

    public class BlockDataCache
    {
        FastStackQueue<BlockData> blockDatasNotInUseBackground;
        FastStackQueue<BlockData> blockDatasNotInUseMainThread;

        BlockGetter world;

        public BlockDataCache(BlockGetter world)
        {
            this.world = world;
            blockDatasNotInUseBackground = new FastStackQueue<BlockData>(100);
            blockDatasNotInUseMainThread = new FastStackQueue<BlockData>(100);
        }

        public BlockData GetNewBlockData(long x, long y, long z)
        {
            if (Thread.CurrentThread == World.mainWorld.helperThread)
            {
                if (blockDatasNotInUseBackground.Count == 0)
                {
                    blockDatasNotInUseBackground.Enqueue(new BlockData(world, x, y, z));
                    blockDatasNotInUseBackground.Enqueue(new BlockData(world, x, y, z));
                    blockDatasNotInUseBackground.Enqueue(new BlockData(world, x, y, z));
                    return new BlockData(world, x, y, z);
                }
                else
                {
                    BlockData res = blockDatasNotInUseBackground.Dequeue();
                    res.ReassignValues(x, y, z);
                    return res;
                }
            }
            else
            {
                if (blockDatasNotInUseMainThread.Count == 0)
                {
                    blockDatasNotInUseMainThread.Enqueue(new BlockData(world, x, y, z));
                    blockDatasNotInUseMainThread.Enqueue(new BlockData(world, x, y, z));
                    blockDatasNotInUseMainThread.Enqueue(new BlockData(world, x, y, z));
                    return new BlockData(world, x, y, z);
                }
                else
                {
                    BlockData res = blockDatasNotInUseMainThread.Dequeue();
                    res.ReassignValues(x, y, z);
                    return res;
                }
            }
        }

        public void DoneWithBlockData(BlockData blockData)
        {
            if (Thread.CurrentThread == World.mainWorld.helperThread)
            {
                blockDatasNotInUseBackground.Enqueue(blockData);
            }
            else
            {
                blockDatasNotInUseMainThread.Enqueue(blockData);
            }
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


    [System.Serializable]
    public class WorldGenOptions
    {
        public int seed;
        public string versionString;
        public WorldGenOptions(int seed, string versionString)
        {
            this.seed = seed;
            this.versionString = versionString;
        }
        public WorldGenOptions()
        {

        }
    }

    public class World : BlockGetter
    {

        public float loadingStatus
        {
            get
            {
                return System.Math.Min(1.0f, chunksLoaded / (float)numChunksNeededToLoad);
            }
            private set
            {

            }
        }

        public bool fullyLoaded
        {
            get
            {
                return loadingStatus == 1.0f;
            }
            private set
            {

            }
        }
        public int chunksLoaded = 0;
        int numChunksNeededToLoad = 9;
        public System.Diagnostics.Stopwatch startLoadStopwatch;
        public static bool DO_CPU_RENDER = true;

        static bool creativeMode_;
        public static bool creativeMode {
            get
            {
                if (mainWorld != null && mainWorld.blocksWorld != null)
                {
                    return mainWorld.blocksWorld.creativeMode;
                }
                else
                {
                    return creativeMode_;
                }
            }
            set
            {
                if (mainWorld != null && mainWorld.blocksWorld != null)
                {
                    mainWorld.blocksWorld.creativeMode = value;
                }
                creativeMode_ = value;
            }
        }
        public static World mainWorld;
        public static WorldGenOptions worldGenOptions;
        public const int maxAnimFrames = 64;
        public const int numBlocks = 64;
        public const int numBreakingFrames = 10;

        public static bool needToGenerate = true;
        public static string currentWorldDir = "";
        public static string VERSION_STRING = "0.1";

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


        [System.Serializable]
        public class EntityData
        {
            public float posX, posY, posZ;
            public int[] blocks;
            public int[] counts;
            public int[] durabilities;
            public int[] maxDurabilities;

            public EntityData()
            {

            }

            public EntityData(MovingEntity movingEntity, Inventory inventory)
            {
                this.posX = movingEntity.transform.position.x;
                this.posY = movingEntity.transform.position.y;
                this.posZ = movingEntity.transform.position.z;

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

        public GameObject MakeLoggingNode(string tag, string text, Color color, long wx, long wy, long wz)
        {
            return blocksWorld.MakeLoggingNode(tag, text, color, wx, wy, wz);
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

        [System.Serializable]
        public class SavedEntityInfo
        {
            public string playerName;
            public string entityUid;
            public EntityData playerData;
            public bool isPlayer;

            public SavedEntityInfo(BlocksPlayer player)
            {
                playerData = new EntityData(player.GetComponent<MovingEntity>(), player.GetComponent<MovingEntity>().inventory);
                playerName = player.name;
                entityUid = player.GetComponent<MovingEntity>().uid;
                isPlayer = true;
            }


            public SavedEntityInfo(MovingEntity entity)
            {
                playerData = new EntityData(entity, entity.inventory);
                entityUid = entity.uid;
                isPlayer = false;
            }

            public SavedEntityInfo()
            {

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

        public static string ROOT_SAVE_DIR = "C:/Users/yams/Desktop/yams/prog/unity/Blocks/repo/Blocks/Saves";


        [System.Serializable]
        public class SavedEntityInfoCollection
        {
            public SavedEntityInfo[] infos;

            public SavedEntityInfoCollection()
            {

            }

            public SavedEntityInfoCollection(SavedEntityInfo[] infos)
            {
                this.infos = infos;
            }
        }
        public SavedEntityInfoCollection GetEntityInfos()
        {
            List<SavedEntityInfo> savedInfos = new List<SavedEntityInfo>();

            foreach (MovingEntity movingEntity in GameObject.FindObjectsOfType<MovingEntity>())
            {
                if (movingEntity.GetComponent<BlocksPlayer>() != null)
                {
                    savedInfos.Add(new SavedEntityInfo(movingEntity.GetComponent<BlocksPlayer>()));
                }
                else
                {
                    savedInfos.Add(new SavedEntityInfo(movingEntity));
                }
            }

            return new SavedEntityInfoCollection(savedInfos.ToArray());
        }

        bool allowedToGenerate = true;
        public void Save(string rootDir)
        {
            long curTime = PhysicsUtils.millis();
            Debug.Log("starting save ");
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
            allowedToGenerate = false;
            List<Chunk> curAllChunks = new List<Chunk>(allChunks);
            foreach (Chunk chunk in curAllChunks)
            {
                if(chunk.WriteToDisk())
                {
                }
            }
            Debug.Log("done with saving chunks in " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            string configPath = cleanedRootDir + "/blocksConfig.json";
            File.WriteAllText(configPath, BlockValue.SaveIdConfigToJsonString());

            Debug.Log("done with saving id configs " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            string inventoriesPath = cleanedRootDir + "/blockInventories.json";
            File.WriteAllText(inventoriesPath, Newtonsoft.Json.JsonConvert.SerializeObject(GetBlockInventories()));

            Debug.Log("done with saving block inventories in " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            string entityInfosPath = cleanedRootDir + "/entityInfos.json";
            File.WriteAllText(entityInfosPath, Newtonsoft.Json.JsonConvert.SerializeObject(GetEntityInfos()));

            Debug.Log("done with saving entity infos " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            string structurePath = cleanedRootDir + "/generatingStructures.json";

            Debug.Log("done with saving generating structures " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            List<SavedStructure> savedStructures = new List<SavedStructure>();
            int numSaved = 0;

            List<Structure> copiedStructures = new List<Structure>();
            lock (unfinishedStructures)
            {
                foreach (Structure structure in unfinishedStructures)
                {
                    copiedStructures.Add(structure);
                }
            }

            foreach (Structure structure in copiedStructures)
            {
                if(structure.WriteToDisk())
                {
                    numSaved += 1;
                }
                //savedStructures.Add(structure.ToSavedStructure());
            }



            //SavedStructureCollection savedStructuresCollection = new SavedStructureCollection(savedStructures.ToArray());

            //File.WriteAllText(structurePath, Newtonsoft.Json.JsonConvert.SerializeObject(savedStructuresCollection));

            Debug.Log("done with saving " + numSaved + " unsaved structures " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();

            if (worldGenOptions == null)
            {
                Debug.LogWarning("Warning in saving: no world gen options object exists, using default");
                worldGenOptions = new WorldGenOptions();
            }

            string worldGenOptionsPath = cleanedRootDir + "/worldGenOptions.json";
            File.WriteAllText(worldGenOptionsPath, Newtonsoft.Json.JsonConvert.SerializeObject(worldGenOptions));

            Debug.Log("done with saving world gen options " + (PhysicsUtils.millis() - curTime));
            curTime = PhysicsUtils.millis();
            allowedToGenerate = true;
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

            BlockValue.FinishAddNewBlocks();
            Debug.Log("done loading config json from file " + configJson);





            blocksWorld.triMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.triMaterialWithTransparency.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.meshTriMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.simpleMeshMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.blockEntityMaterial.mainTexture = BlockValue.allBlocksTexture;
            blocksWorld.blockIconMaterial.mainTexture = BlockValue.allBlocksTexture;



            Debug.Log("loading chunks");
            string chunksDir = cleanedRootDir + "/chunks";
            DirectoryInfo chunksDirInfo = new DirectoryInfo(chunksDir);

            // clear structures
            lock(unfinishedStructures)
            {

                unfinishedStructures.Clear();
            }

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
            ungeneratedBiomeDatas.Clear();
            lastChunk = null;


            string savedStructuresDir = cleanedRootDir + "/savedStructures";
            DirectoryInfo savedStructuresDirInfo = new DirectoryInfo(savedStructuresDir);
            /*
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
            */

            // load saved structures
            if (savedStructuresDirInfo.Exists)
            {
                string[] savedStructuresFiles = Directory.GetFiles(savedStructuresDirInfo.FullName, "*.dat", SearchOption.TopDirectoryOnly);
                foreach (string savedStructuresFile in savedStructuresFiles)
                {
                    FileInfo fInfo = new FileInfo(savedStructuresFile);
                    if (fInfo.Exists)
                    {
                        SavedStructure savedStructure = Newtonsoft.Json.JsonConvert.DeserializeObject<SavedStructure>(File.ReadAllText(fInfo.FullName));
                        Structure structure = new Structure(savedStructure);
                        lock(unfinishedStructures)
                        {
                            unfinishedStructures.Add(structure);
                        }
                    }
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
                                spruce.chunkData.ReadFromRLEFile(fInfo.FullName);
                                World.mainWorld.blocksTouchingSky.GeneratedChunk(spruce);
                                spruce.touchingSkyChunk = blocksTouchingSky.GetOrCreateSkyChunk(cx, cz);
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
                    BlockOrItem customBlockThing;
                    if (customBlocks.ContainsKey(this[blockPos], out customBlockThing))
                    {
                        blockInventory.resultBlocks = new BlockStack[customBlockThing.NumCraftingOutputs()];
                    }
                }
            }


            string entityInfosPath = cleanedRootDir + "/entityInfos.json";
            if (!File.Exists(entityInfosPath))
            {
                Debug.LogWarning("in loading world, entity infos json " + entityInfosPath + " does not exist, placing all entities at default position with empty inventories");
            }
            else
            {
                SavedEntityInfoCollection savedEntityInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<SavedEntityInfoCollection>(File.ReadAllText(entityInfosPath));
                foreach (SavedEntityInfo savedEntityInfo in savedEntityInfos.infos)
                {
                    if (savedEntityInfo.isPlayer)
                    {
                        foreach (BlocksPlayer player in GameObject.FindObjectsOfType<BlocksPlayer>())
                        {
                            if (player.name == savedEntityInfo.playerName)
                            {
                                player.transform.position = new Vector3(savedEntityInfo.playerData.posX, savedEntityInfo.playerData.posY, savedEntityInfo.playerData.posZ);
                                Inventory newResInventory = new Inventory(savedEntityInfo.playerData.blocks.Length);

                                for (int i = 0; i < savedEntityInfo.playerData.blocks.Length; i++)
                                {
                                    newResInventory.blocks[i] = new BlockStack(savedEntityInfo.playerData.blocks[i], savedEntityInfo.playerData.counts[i], savedEntityInfo.playerData.durabilities[i], savedEntityInfo.playerData.maxDurabilities[i]);
                                }
                                player.inventory = newResInventory;
                                player.GetComponent<MovingEntity>().uid = savedEntityInfo.entityUid;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("haven't set up reloading non player entities yet, they are just deleted for now");
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

        Dictionary<BlockValue, RenderTriangle[]>[] blockCustomTriangles = new Dictionary<BlockValue, RenderTriangle[]>[] {
            new Dictionary<BlockValue, RenderTriangle[]>(),
            new Dictionary<BlockValue, RenderTriangle[]>(),
            new Dictionary<BlockValue, RenderTriangle[]>(),
            new Dictionary<BlockValue, RenderTriangle[]>()
        };
        Dictionary<BlockValue, BlockModel>[] blockCustomTrianglesDependsOnState = new Dictionary<BlockValue, BlockModel>[]
        {
            new Dictionary<BlockValue, BlockModel>(),
            new Dictionary<BlockValue, BlockModel>(),
            new Dictionary<BlockValue, BlockModel>(),
            new Dictionary<BlockValue, BlockModel>()
        };


        public static RenderTriangle[] defaultCubeTris
        {
            get
            {
                if (World.mainWorld != null && World.mainWorld.blocksWorld.cubeTris != null)
                {
                    return World.mainWorld.blocksWorld.cubeTris;
                }
                else
                {
                    return BlocksWorld.GetDefaultCubeTris();
                }
            }
            private set
            {
                
            }
        }

        public bool HasNonBlockModel(BlockValue blockId)
        {
            return blockCustomTriangles[0].ContainsKey(blockId) || blockCustomTrianglesDependsOnState[0].ContainsKey(blockId);
        }


        RenderTriangle dummyTemplate = new RenderTriangle();
        public RenderTriangle[] GetTrianglesForBlock(LVector3 pos)
        {
            LVector3 chunkPos;
            GetChunkCoordinatesAtPos(pos, out chunkPos);
            Vector3 offset = (new Vector3(chunkPos.x*chunkSizeX, chunkPos.y*chunkSizeY, chunkPos.z*chunkSizeZ)) * worldScale;
            Matrix4x4 helperMat = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            return GetTrianglesForBlock(pos.BlockV, GetState(pos.x, pos.y, pos.z, BlockState.Animation), dummyTemplate, new Vector3(pos.x, pos.y, pos.z), helperMat);
        }


        public RenderTriangle[] GetTrianglesForBlock(BlockValue blockId, int animInt, RenderTriangle template, Vector3 blockPos, Matrix4x4 localToWorldMat)
        {

            BlockData.BlockRotation rotation = BlockData.RotationFromRawAnimInt(animInt);
            int connectedFlags = BlockData.GetConnectivityFromRawAnimInt(animInt);
            int rotationI = PhysicsUtils.RotationToDegrees(rotation) / 90;
            int state = BlockData.AnimStateFromRawAnimInt(animInt);

            if ((int)rotation != 0)
            {
                //Debug.Log("got other rotation of " + (int)rotation + " with rotation i " + rotationI);
            }

            if (connectedFlags != 0)
            {
                //Debug.Log("got other connectivity of " + connectedFlags);
            }

            RenderTriangle[] blockTris;
            if (blockCustomTriangles[rotationI].ContainsKey(blockId))
            {
                blockTris = blockCustomTriangles[rotationI][blockId];
            }
            else if (blockCustomTrianglesDependsOnState[rotationI].ContainsKey(blockId))
            {
                blockTris = blockCustomTrianglesDependsOnState[rotationI][blockId].ToRenderTriangles(rotation, state, connectedFlags);
            }
            else
            {
                blockTris = defaultCubeTris;
            }
            RenderTriangle[] res = new RenderTriangle[blockTris.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new RenderTriangle();
                res[i].state1 = template.state1;
                res[i].state2 = template.state2;
                res[i].state3 = template.state3;
                res[i].state4 = template.state4;
                res[i].vertex1 = Util.MakeFloat4(localToWorldMat.MultiplyPoint(new Vector3(blockTris[i].vertex1.x + blockPos.x, blockTris[i].vertex1.y + blockPos.y, blockTris[i].vertex1.z + blockPos.z) * worldScale));
                res[i].vertex2 = Util.MakeFloat4(localToWorldMat.MultiplyPoint(new Vector3(blockTris[i].vertex2.x + blockPos.x, blockTris[i].vertex2.y + blockPos.y, blockTris[i].vertex2.z + blockPos.z) * worldScale));
                res[i].vertex3 = Util.MakeFloat4(localToWorldMat.MultiplyPoint(new Vector3(blockTris[i].vertex3.x + blockPos.x, blockTris[i].vertex3.y + blockPos.y, blockTris[i].vertex3.z + blockPos.z) * worldScale));
         
                float yOffset = (Mathf.Abs(template.state1) - 1) / 64.0f;
                //yOffset = 18.0f/64.0f;
                
                res[i].uv1 = Util.MakeFloat2(blockTris[i].uv1.x, blockTris[i].uv1.y + yOffset);
                res[i].uv2 = Util.MakeFloat2(blockTris[i].uv2.x, blockTris[i].uv2.y + yOffset);
                res[i].uv3 = Util.MakeFloat2(blockTris[i].uv3.x, blockTris[i].uv3.y + yOffset);
            }
            return res;
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

        public const int chunkSizeX = BlocksWorld.chunkSizeX;
        public const int chunkSizeY = BlocksWorld.chunkSizeY;
        public const int chunkSizeZ = BlocksWorld.chunkSizeZ;

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


        /// <summary>
        /// Setup stuff like inventory at block position
        /// Also, get what block we should actually be placing, given that we placed it from the given axis
        /// </summary>
        /// <param name="block"></param>
        /// <param name="pos"></param>
        /// <param name="axisPlacedFrom"></param>
        /// <returns></returns>
        public BlockValue PrePlaceBlock(BlockValue block, LVector3 pos, AxisDir axisPlacedFrom, out int lightProduced)
        {
            BlockOrItem customBlock;
            lightProduced = 0;
            if (customBlocks.ContainsKey(block, out customBlock))
            {
                if (customBlock.InventorySpace() > 0)
                {
                    Inventory blockInventory = new Inventory(customBlock.InventorySpace());
                    if (customBlock.NumCraftingOutputs() > 0)
                    {
                        blockInventory.resultBlocks = new BlockStack[customBlock.NumCraftingOutputs()];
                    }
                    blocksWorld.blockInventories[pos] = blockInventory;
                }

                lightProduced = Mathf.Clamp(customBlock.ConstantLightEmitted(), 0, 15);
                return customBlock.PlaceMe(axisPlacedFrom, pos);
            }

            return block;
        }

        public bool BlockHasInventory(LVector3 pos, out Inventory inventory)
        {
            if(blocksWorld.blockInventories.ContainsKey(pos))
            {
                inventory = blocksWorld.blockInventories[pos];
                return true;
            }
            else
            {
                inventory = null;
                return false;
            }
        }

        public bool DestroyBlockByBreaking(LVector3 pos, BlockStack thingHolding)
        {
            if (DropBlockOnDestroy(pos.BlockV, pos, thingHolding, pos.BlockCentertoUnityVector3(), pos.BlockCentertoUnityVector3()))
            {
                this[pos] = BlockValue.Air;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DropBlockOnDestroy(BlockValue block, LVector3 pos, BlockStack thingHolding, Vector3 positionOfBlock, Vector3 posOfOpening)
        {
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

                    if (destroyBlock)
                    {
                        int inventorySize = customBlock.InventorySpace();
                        if (inventorySize > 0 && blocksWorld.blockInventories.ContainsKey(pos))
                        {
                            Inventory blockInventory = World.mainWorld.blocksWorld.blockInventories[pos];
                            blockInventory.ThrowAllBlocks(positionOfBlock);
                            World.mainWorld.blocksWorld.blockInventories.Remove(pos);
                        }
                    }
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


        public bool AllowedtoPlaceBlock(BlockValue block, AxisDir axisPlacedOn, LVector3 pos)
        {
            BlockOrItem customBlock;
            if (customBlocks.ContainsKey(block, out customBlock))
            {
                return customBlock.CanBePlaced() && customBlock.CanBePlaced(axisPlacedOn, pos);
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


        public BlockTileEntity CreateTileEntity(BlockStack blockStack, Vector3 position)
        {
            GameObject blockEntity = GameObject.Instantiate(blocksWorld.blockTileEntityPrefab);
            blockEntity.transform.position = position;
            blockEntity.GetComponent<BlockTileEntity>().blockStack = blockStack;
            return blockEntity.GetComponent<BlockTileEntity>();
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

        public const int biomeDataSizeX = BlocksWorld.biomeDataSizeX;
        public const int biomeDataSizeY = BlocksWorld.biomeDataSizeY;
        public const int biomeDataSizeZ = BlocksWorld.biomeDataSizeZ;

        public World(BlocksWorld blocksWorld, BlocksPack blocksPack)
        {
            startLoadStopwatch = new System.Diagnostics.Stopwatch();
            startLoadStopwatch.Start();

            if (chunkSizeX % biomeDataSizeX != 0)
            {
                Debug.LogError("Error: biomeDataSizeX of " + biomeDataSizeX + " needs to divide chunkSizeX of " + chunkSizeX);
            }
            if (chunkSizeY % biomeDataSizeY != 0)
            {
                Debug.LogError("Error: biomeDataSizeY of " + biomeDataSizeY + " needs to divide chunkSizeY of " + chunkSizeY);
            }
            if (chunkSizeZ % biomeDataSizeZ != 0)
            {
                Debug.LogError("Error: biomeDataSizeZ of " + biomeDataSizeZ + " needs to divide chunkSizeZ of " + chunkSizeZ);
            }

            blocksWorld.creativeMode = creativeMode_;
            this.worldGeneration = blocksPack.customGeneration;
            this.customBlocks = blocksPack.customBlocks;
            this.blocksTouchingSky = new BlocksTouchingSky(this);
            stackableSize = new Dictionary<int, int>();

            if (World.worldGenOptions == null)
            {
                Debug.LogWarning("world gen options are null, using default");
                World.worldGenOptions = new WorldGenOptions();
            }
            Simplex.Noise.Seed = (int)World.worldGenOptions.seed;


            foreach (KeyValuePair<BlockValue, BlockOrItem> customBlock in customBlocks)
            {
                customBlock.Value.blockGetter = this;
                customBlock.Value.world = this;
                stackableSize[customBlock.Key] = customBlock.Value.stackSize;
            }





            blockDataCache = new BlockDataCache(this);
            chunkProperties = new ChunkProperties();
            World.mainWorld = this;
            this.blocksWorld = blocksWorld;

            BlockData.BlockRotation[] rotations = new BlockData.BlockRotation[]
            {
                BlockData.BlockRotation.Degrees0,
                BlockData.BlockRotation.Degrees90,
                BlockData.BlockRotation.Degrees180,
                BlockData.BlockRotation.Degrees270
            };


            foreach (KeyValuePair<BlockValue, BlockModel> blockModel in BlockValue.customModels)
            {
                bool dependsOnState = false;
                for (int i = 0; i < rotations.Length; i++)
                {
                    RenderTriangle[] blockTriangles = blockModel.Value.ToRenderTriangles(out dependsOnState, rotations[i]);
                    if (dependsOnState)
                    {
                        this.blockCustomTrianglesDependsOnState[i][blockModel.Key] = blockModel.Value;
                    }
                    else
                    {
                        this.blockCustomTriangles[i][blockModel.Key] = blockTriangles;
                    }
                }
            }


            //this.blockCustomTriangles[Example.Flower] = BlockModel.FromJSONFilePath(@"C:\Users\yams\Desktop\yams\prog\unity\Blocks\repo\Blocks\BlockSpecs\Example\blocks\Flower\model.json").ToRenderTriangles();

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

            if (World.currentWorldDir == "" || World.currentWorldDir == null)
            {
                World.currentWorldDir = Path.Combine(World.ROOT_SAVE_DIR, "Untitled World");
            }
            if (needToGenerate)
            {
                string worldName = new DirectoryInfo(World.currentWorldDir).Name;
                int num = 1;
                while ((new DirectoryInfo(World.currentWorldDir)).Exists)
                {
                    num += 1;
                    World.currentWorldDir = Path.Combine(World.ROOT_SAVE_DIR, worldName + num);
                }

                this.worldGeneration.OnGenerationInit();
                RunWorldGenerationEvents(-10, -10, -10, 20);
                GenerateChunk(0, 0, 0);
                //return;
                int viewDistX = 0;
                int viewDistY = 0;
                int viewDistZ = 0;
                for (int i = -viewDistX; i <= viewDistX; i++)
                {
                    for (int j = viewDistY; j >= -viewDistY; j--)
                    {
                        for (int k = -viewDistZ; k <= viewDistZ; k++)
                        {
                            GenerateChunk(i, j, k);
                        }
                    }
                }
            }
            else
            {
                this.worldGeneration.OnGenerationInit();
                Load(World.currentWorldDir);
            }

        }

        public void AddUnfinishedStructure(Structure structure)
        {
            lock(unfinishedStructures)
            {
                unfinishedStructures.Add(structure);
            }
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


        public Dictionary<LVector3, ChunkBiomeData> ungeneratedBiomeDatas = new Dictionary<LVector3, ChunkBiomeData>();


        ChunkBiomeData lastRequest;

        public ChunkBiomeData GetChunkBiomeData(long bx, long by, long bz)
        {
            long worldX = bx * biomeDataSizeX;
            long worldY = by * biomeDataSizeY;
            long worldZ = bz * biomeDataSizeZ;

            Chunk chunk = GetChunkAtPos(worldX, worldY, worldZ);
            if (chunk != null)
            {
                while (true)
                {
                    lock (chunk.biomeDataLock)
                    {
                        if (chunk.chunkBiomeDatas != null)
                        {
                            return chunk.GetBiomeData(worldX, worldY, worldZ);
                        }
                    }
                    // we are in a seperate thread, wait for it to finish
                    Debug.LogWarning("waiting for chunk " + chunk.cx + " " + chunk.cy + " " + chunk.cz + " to make biome data");
                    Thread.Sleep(10);
                }
            }
            else
            {
                LVector3 biomePos = new LVector3(bx, by, bz);
                if (ungeneratedBiomeDatas.ContainsKey(biomePos))
                {
                    return ungeneratedBiomeDatas[biomePos];
                }
                else
                {
                    ChunkBiomeData res = new ChunkBiomeData(chunkProperties, biomeDataSizeX, biomeDataSizeY, biomeDataSizeZ, bx, by, bz);
                    ungeneratedBiomeDatas[biomePos] = res;
                    return res;
                }
            }
        }


        public float AverageChunkValues(long x, long y, long z, ChunkProperty chunkProperty)
        {
            long bx, by, bz;
            GetBiomeCoordinatesAtPos(x, y, z, out bx, out by, out bz);
            return AverageChunkValues(x, y, z, bx, by, bz, chunkProperty);
        }

        public float AverageChunkValues(long x, long y, long z, long bx, long by, long bz, ChunkProperty chunkProperty)
        {
            ChunkBiomeData chunkBiomeData = GetChunkBiomeData(bx, by, bz);
            return chunkBiomeData.AverageBiomeData(x, y, z, chunkProperty);
            /*
            ChunkBiomeData chunkx2z1 = GetChunkBiomeData(divWithCeil(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithFloor(z, chunkSizeZ));
            ChunkBiomeData chunkx1z2 = GetChunkBiomeData(divWithFloor(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithCeil(z, chunkSizeZ));
            ChunkBiomeData chunkx2z2 = GetChunkBiomeData(divWithCeil(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithCeil(z, chunkSizeZ));

            long x1Weight = x - chunkx1z1.cx * chunkSizeX;
            long x2Weight = chunkx2z1.cx * chunkSizeX - x;
            long z1Weight = z - chunkx1z1.cz * chunkSizeZ;
            long z2Weight = chunkx2z1.cz * chunkSizeZ - z;

            float px = x1Weight / (float)chunkSizeX;
            float pz = z1Weight / (float)chunkSizeZ;


            float valZ1 = getChunkValue(chunkx1z1) * (1 - px) + getChunkValue(chunkx2z1) * px;
            float valZ2 = getChunkValue(chunkx1z2) * (1 - px) + getChunkValue(chunkx2z2) * px;

            return valZ1 * (1 - pz) + valZ2 * pz;
            */
        }
        /*

        public float AverageChunkValues(long x, long y, long z, string valueKey)
        {
            ChunkBiomeData chunkx1z1 = GetChunkBiomeData(divWithFloor(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithFloor(z, chunkSizeZ));
            ChunkBiomeData chunkx2z1 = GetChunkBiomeData(divWithCeil(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithFloor(z, chunkSizeZ));
            ChunkBiomeData chunkx1z2 = GetChunkBiomeData(divWithFloor(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithCeil(z, chunkSizeZ));
            ChunkBiomeData chunkx2z2 = GetChunkBiomeData(divWithCeil(x, chunkSizeX), divWithFloor(y, chunkSizeY), divWithCeil(z, chunkSizeZ));

            long x1Weight = x - chunkx1z1.cx * chunkSizeX;
            long x2Weight = chunkx2z1.cx * chunkSizeX - x;
            long z1Weight = z - chunkx1z1.cz * chunkSizeZ;
            long z2Weight = chunkx2z1.cz * chunkSizeZ - z;

            float px = x1Weight / (float)chunkSizeX;
            float pz = z1Weight / (float)chunkSizeZ;


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
            long cx = divWithFloorForChunkSizeX(x);
            long cz = divWithFloorForChunkSizeZ(z);
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
        public long divWithFloorForChunkSizeX(long a)
        {
            return a / chunkSizeX - ((a < 0 && a % chunkSizeX != 0) ? 1 : 0);
        }


        public long divWithFloorForChunkSizeY(long a)
        {
            return a / chunkSizeY - ((a < 0 && a % chunkSizeY != 0) ? 1 : 0);
        }


        public long divWithFloorForChunkSizeZ(long a)
        {
            return a / chunkSizeZ - ((a < 0 && a % chunkSizeZ != 0) ? 1 : 0);
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
                Chunk chunk = GetChunk(cx, cy, cz);
                if (chunk == null)
                {
                    return 0;
                }
                return chunk[x, y, z];
            }

            set
            {
                blockModifyState += 1;
                Chunk chunk = GetChunk(cx, cy, cz);
                if (chunk != null)
                {
                    chunk[x, y, z] = value;
                }
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
        public List<Thread> helperThread2s;

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

                    if (chunk == null)
                    {
                        Debug.LogWarning("warning: null chunk");
                        continue;
                    }
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
                Chunk res = new Chunk(this, chunkProperties, chunkX, chunkY, chunkZ, createStuff: true);
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


            chunk.InitBiomeData(chunkProperties);

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
            long chunkX = divWithFloorForChunkSizeX(x);
            long chunkY = divWithFloorForChunkSizeY(y);
            long chunkZ = divWithFloorForChunkSizeZ(z);
            Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }

        public void GetChunkCoordinatesAtPos(LVector3 worldPos, out LVector3 chunkPos)
        {
            long chunkX = divWithFloorForChunkSizeX(worldPos.x);
            long chunkY = divWithFloorForChunkSizeY(worldPos.y);
            long chunkZ = divWithFloorForChunkSizeZ(worldPos.z);
            chunkPos = new LVector3(chunkX, chunkY, chunkZ);
        }


        public void GetChunkCoordinatesAtPos(long x, long y, long z, out long cx, out long cy, out long cz)
        {
            cx = divWithFloorForChunkSizeX(x);
            cy = divWithFloorForChunkSizeY(y);
            cz = divWithFloorForChunkSizeZ(z);
        }


        public void GetBiomeCoordinatesAtPos(LVector3 worldPos, out LVector3 biomePos)
        {
            long bx = divWithFloor(worldPos.x, biomeDataSizeX);
            long by = divWithFloor(worldPos.y, biomeDataSizeY);
            long bz = divWithFloor(worldPos.z, biomeDataSizeZ);
            biomePos = new LVector3(bx, by, bz);
        }


        public void GetBiomeCoordinatesAtPos(long x, long y, long z, out long bx, out long by, out long bz)
        {
            bx = divWithFloor(x, biomeDataSizeX);
            by = divWithFloor(y, biomeDataSizeY);
            bz = divWithFloor(z, biomeDataSizeZ);
        }


        public void GetChunkCoordinatesAtPos(long x, long y, long z, out LVector3 chunkPos)
        {
            long chunkX = divWithFloorForChunkSizeX(x);
            long chunkY = divWithFloorForChunkSizeY(y);
            long chunkZ = divWithFloorForChunkSizeZ(z);
            chunkPos = new LVector3(chunkX, chunkY, chunkZ);
        }

        public Chunk GetChunkAtPos(LVector3 pos)
        {
            return GetChunkAtPos(pos.x, pos.y, pos.z);
        }

        public Chunk GetChunkAtPos(long x, long y, long z)
        {
            long chunkX = divWithFloorForChunkSizeX(x);
            long chunkY = divWithFloorForChunkSizeY(y);
            long chunkZ = divWithFloorForChunkSizeZ(z);
            Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
            return chunk;
        }

        public void AddBlockUpdate(LVector3 pos)
        {
            AddBlockUpdate(pos.x, pos.y, pos.z);
        }
        public void AddBlockUpdate(long i, long j, long k, bool alsoToNeighbors = false)
        {
            long chunkX = divWithFloorForChunkSizeX(i);
            long chunkY = divWithFloorForChunkSizeY(j);
            long chunkZ = divWithFloorForChunkSizeZ(k);
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

        public void CheckIfTouchingAir(long i, long j, long k)
        {
            if (this[i - 1, j, k] < 0 || this[i, j - 1, k] < 0 || this[i, j, k - 1] < 0 || this[i + 1, j, k] < 0 || this[i, j + 1, k] < 0 || this[i, j, k - 1] < 0)
            {
                SetTouchingAir(i, j, k, true);
            }
            else
            {
                SetTouchingAir(i, j, k, false);
            }
        }

        public void SetTouchingAir(long i, long j, long k, bool value)
        {
            Chunk chunk = GetChunkAtPos(i, j, k);
            if (chunk != null && !chunk.generating)
            {
                chunk.SetTouchingAir(i, j, k, value);
            }
        }
        public void AddBlockUpdateToNeighbors(LVector3 pos)
        {
            AddBlockUpdateToNeighbors(pos.x, pos.y, pos.z);
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

        public List<Tuple<Chunk, long>> GetChunksCloserThanRenderDist(bool onMainThread=true, int maxDist=-1)
        {
            List<Chunk> allChunksHere = new List<Chunk>();
            Vector3[] blocksPlayerPositions = blocksWorld.playerPositions;
            LVector3[] blocksPlayerLPositionsDivChunkSize = new LVector3[blocksPlayerPositions.Length];
            for (int i = 0; i < blocksPlayerPositions.Length; i++)
            {
                LVector3 tmp = LVector3.FromUnityVector3(blocksPlayerPositions[i]);
                blocksPlayerLPositionsDivChunkSize[i] = new LVector3(divWithFloorForChunkSizeX(tmp.x), divWithFloorForChunkSizeY(tmp.y), divWithFloorForChunkSizeZ(tmp.z));
            }


            List<Tuple<Chunk, long>> allChunksWithDists = new List<Tuple<Chunk, long>>();

            if (maxDist == -1)
            {
                maxDist = blocksWorld.chunkRenderDist;
            }
            for (int i = 0; i < allChunks.Count; i++)
            {
                Chunk curChunk = allChunks[i];
                long minPlayerDist = long.MaxValue;
                foreach (LVector3 playerPosDivChunkSize in blocksPlayerLPositionsDivChunkSize)
                {
                    long distInChunks =
                        System.Math.Abs(curChunk.cx - playerPosDivChunkSize.x) +
                        System.Math.Abs(curChunk.cy - playerPosDivChunkSize.y) +
                        System.Math.Abs(curChunk.cz - playerPosDivChunkSize.z);

                    minPlayerDist = System.Math.Min(distInChunks, minPlayerDist);

                }

                if (onMainThread)
                {
                    if (curChunk.chunkRenderer.prevMesh != null)
                    {
                        curChunk.chunkRenderer.prevMesh.Dispose();
                        curChunk.chunkRenderer.prevMesh = null;
                    }
                }

                if (curChunk.mustRenderMe)
                {
                    allChunksWithDists.Add(new Tuple<Chunk, long>(curChunk, minPlayerDist));
                }
                else
                {
                    // only render if closer than or equal to render dist
                    if (minPlayerDist <= maxDist)
                    {
                        allChunksWithDists.Add(new Tuple<Chunk, long>(curChunk, minPlayerDist));
                    }
                    // otherwise, do some cleanup
                    else
                    {
                        if (onMainThread)
                        {
                            if (curChunk.chunkRenderer.myMesh != null && curChunk.chunkRenderer.myMesh.drawData != null)
                            {
                                // dispose of draw data on the gpu if we aren't currently using it
                                curChunk.chunkRenderer.myMesh.DisposeOfGPUData();
                            }
                        }
                    }
                }
                
            }

            // Sort by closest to player(s), so those are rendered first
            allChunksWithDists.Sort((x, y) => { return x.b.CompareTo(y.b); });

            return allChunksWithDists;
        }

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public void Render()
        {
            long maxMillis = 1000 / 60;
            long pleaseMillis = 1000 / 60;
            long elapsedTime = 0;
            int numAllowedToDoFullRender = 100000;
            if (frameId < 100)
            {
                numAllowedToDoFullRender = 10000;
            }
            // render non-transparent
            stopwatch.Reset();
            stopwatch.Start();

            List<Chunk> chunksNotFinished = new List<Chunk>();

            List<Tuple<Chunk, long>> chunksToRender = GetChunksCloserThanRenderDist(true);

            int tmp = 10000;
            int iff = 0;
            foreach (Tuple<Chunk, long> chunkAndDist in chunksToRender)
            {
                iff += 1;
                numAllowedToDoFullRender = 1;
                Chunk chunk = chunkAndDist.a;
                int prev = numAllowedToDoFullRender;
                int prevTemp = tmp;
                if (chunkAndDist.b < 1)
                {
                    if(chunk.chunkRenderer.RenderAsync(false, chunk, ref tmp))
                    {
                        //chunk.chunkRenderer.FinishRenderSync(chunk);
                        //chunksNotFinished.Add(chunk);
                    }

                }
                else
                {
                    if (chunk.chunkRenderer.RenderAsync(false, chunk, ref numAllowedToDoFullRender))
                    {
                        //chunk.chunkRenderer.FinishRenderSync(chunk);
                        //chunksNotFinished.Add(chunk);
                    }
                }
                
                if (!World.DO_CPU_RENDER)
                {
                    if (chunksNotFinished.Count >= blocksWorld.chunkBlockDatas.Length)
                    {
                        for (int i = 0; i < chunksNotFinished.Count; i++)
                        {
                            //chunksNotFinished[i].chunkRenderer.FinishRenderSync(chunksNotFinished[i]);
                        }
                        chunksNotFinished.Clear();
                    }
                }
                
                if (stopwatch.ElapsedMilliseconds > 1000/60.0f)
                {
                    numAllowedToDoFullRender = 0;
                    tmp = 0;
                }

                if (numAllowedToDoFullRender != prev || prevTemp != tmp)
                {
                    if (stopwatch.ElapsedMilliseconds > maxMillis)
                    {
                        numAllowedToDoFullRender = 0;
                    }
                }
            }

            for (int i = 0; i < chunksNotFinished.Count; i++)
            {
                chunksNotFinished[i].chunkRenderer.FinishRenderSync(chunksNotFinished[i]);
            }
            chunksNotFinished.Clear();

            if (blocksWorld.verbosePathing)
            {
                for (int i = 0; i < chunksToRender.Count; i++)
                {
                    chunksToRender[i].a.chunkRenderer.RenderLogging();
                }
            }
            return;
            /*
            // render transparent
            foreach (Chunk chunk in allChunks)
            {
                //chunk.chunkRenderer.Render(true, chunk, ref numAllowedToDoFullRender);
                if (frameId > 100 && PhysicsUtils.millis() - curMillis > maxMillis)
                {
                    numAllowedToDoFullRender = 0;
                }
            }
            Debug.Log("has total of " + elapsedTime + " elapsed time");
            */
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



        public bool playerMovedChunks = false;
        long frameId = 0;
        public long frameTimeStart = 0;
        public void Tick(bool onlyGenerate)
        {
            if (!onlyGenerate)
            {
                foreach (KeyValuePair<BlockValue, BlockOrItem> block in customBlocks)
                {
                    block.Value.OnTickStart();
                }

                List<Tuple<Chunk, long>> allChunksHere2 = GetChunksCloserThanRenderDist(true, maxDist: 100000);

                foreach (Tuple<Chunk, long> chunkAndDist in allChunksHere2)
                {
                    if (!chunkAndDist.a.generating)
                    {
                        int maxTickStepsf = 10000000;
                        chunkAndDist.a.Tick(frameId, false, true, ref maxTickStepsf);
                    }
                }

                return;
            }
            


            bool allowTick = !onlyGenerate;
            frameId += 1;
            // what direction is checked first: goes in a weird order after that
            globalPreference = (globalPreference + 1) % 4;
            List<Tuple<Chunk, long>> allChunksHere = GetChunksCloserThanRenderDist(false);

            // Randomly shuffle the order we update the chunks in, for the memes when they are tiled procedurally
            //Shuffle(allChunksHere);


            frameTimeStart = PhysicsUtils.millis();
            numBlockUpdatesThisTick = 0;
            numWaterUpdatesThisTick = 0;




            int numGenerated = 0;
            int maxGenerating = 1000;
            int maxTickSteps = 10000000;

            bool allowGenerate = true;

            Vector3[] playerPositions = blocksWorld.playerPositions;
            foreach (Vector3 playerPosition in playerPositions)
            {
                LVector3 playerPos = LVector3.FromUnityVector3(playerPosition);

                Chunk playerChunk = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z);
                RunTick(playerChunk, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkBelow = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y-chunkSizeY, playerPos.z);
                RunTick(playerChunkBelow, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);


                Chunk playerChunkAbove = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y + chunkSizeY, playerPos.z);
                RunTick(playerChunkAbove, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkX = GetOrGenerateChunkAtPos(playerPos.x + chunkSizeX, playerPos.y, playerPos.z);
                RunTick(playerChunkX, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkX2 = GetOrGenerateChunkAtPos(playerPos.x - chunkSizeX, playerPos.y, playerPos.z);
                RunTick(playerChunkX2, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkZ = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z+chunkSizeZ);
                RunTick(playerChunkZ, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);

                Chunk playerChunkZ2 = GetOrGenerateChunkAtPos(playerPos.x, playerPos.y, playerPos.z- chunkSizeZ);
                RunTick(playerChunkZ2, true, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);



                if ((playerMovedChunks || playerChunk.distancesOfPlacesToExpand == null || playerChunk.distancesOfPlacesToExpand.Count > 0) && frameId % 10 == 0)
                {
                    int viewDistX = World.mainWorld.blocksWorld.viewDistX;
                    int viewDistY = World.mainWorld.blocksWorld.viewDistY;
                    int viewDistZ = World.mainWorld.blocksWorld.viewDistZ;

                    long pcx = divWithFloorForChunkSizeX(playerPos.x);
                    long pcy = divWithFloorForChunkSizeY(playerPos.y);
                    long pcz = divWithFloorForChunkSizeZ(playerPos.z);
                    int numDone = 0;
                    int maxGenThisStep = 10;

                    if (playerMovedChunks || playerChunk.distancesOfPlacesToExpand == null)
                    {
                        playerMovedChunks = false;
                        List<Tuple<long, long, long, int>> positionsToGen = new List<Tuple<long, long, long, int>>();

                        // sort potential options by closeness
                        for (int i = -viewDistX; i <= viewDistX; i++)
                        {
                            for (int j = viewDistY; j >= -viewDistY; j--)
                            {
                                for (int k = -viewDistZ; k <= viewDistZ; k++)
                                {
                                    int dist = System.Math.Abs(i) + System.Math.Abs(j) + System.Math.Abs(k);

                                    if (dist >= 1 && dist < blocksWorld.chunkRenderDist)
                                    {
                                        Chunk blah = GetChunk(pcx + i, pcy + j, pcz + k);
                                        if (blah == null)
                                        {
                                            positionsToGen.Add(new Tuple<long, long, long, int>(pcx + i, pcy + j, pcz + k, dist));
                                        }
                                    }
                                }
                            }
                        }
                        positionsToGen.Sort((x, y) => { return x.d.CompareTo(y.d); });

                        playerChunk.distancesOfPlacesToExpand = positionsToGen;
                    }


                    int numToGen = System.Math.Min(playerChunk.distancesOfPlacesToExpand.Count, maxGenThisStep);
                    for (int i = 0; i < numToGen; i++)
                    {
                        GetOrGenerateChunk(playerChunk.distancesOfPlacesToExpand[i].a, playerChunk.distancesOfPlacesToExpand[i].b, playerChunk.distancesOfPlacesToExpand[i].c);
                    }
                    playerChunk.distancesOfPlacesToExpand.RemoveRange(0, numToGen);
                }
            }

            long frameTimeStart2 = PhysicsUtils.millis();
            if (blocksWorld.optimize)
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
            maxGenerating = 1;
            for (int i = 0; i < allChunksHere.Count; i++)
            {
                Chunk chunk = allChunksHere[i].a;
                if (!chunk.generating && chunk.chunkData.blocksNeedUpdatingNextFrame.Count == 0)
                {
                    continue;
                }
                if (maxGenerating <= numGenerated)
                {
                    allowGenerate = false;
                }
                if (PhysicsUtils.millis() - frameTimeStart > maxTickSteps && !chunk.mustRenderMe)
                {
                    //Debug.Log("bailed on chunk " + (chunkI + 1) + "/" + allChunksHere.Count);
                    bailed = true;
                    break;
                }
                chunkI += 1;
                RunTick(chunk, allowGenerate, allowTick, ref maxTickSteps, ref numGenerated, maxGenerating);
            }
            if (!bailed)
            {
                //Debug.Log("did not bail " + frameId);
            }
        }


        public void RunTick(Chunk chunk, bool allowGenerate, bool allowTick, ref int maxTickSteps, ref int numGenerated, int maxGenerating)
        {
            if (!allowedToGenerate)
            {
                allowGenerate = false;
            }
            if (chunk.Tick(frameId, allowGenerate, allowTick, ref maxTickSteps))
            {
                numGenerated += 1;
                if (numGenerated > maxGenerating)
                {
                    allowGenerate = false;
                }
                List<Structure> leftoverStructures = new List<Structure>();
                Priority_Queue.SimplePriorityQueue<Structure, int> chunkStructures = new Priority_Queue.SimplePriorityQueue<Structure, int>();

                List<Structure> copiedStructures;
                lock(unfinishedStructures)
                {
                    copiedStructures = new List<Structure>(unfinishedStructures);
                }

                foreach (Structure structure in copiedStructures)
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

                lock(unfinishedStructures)
                {

                    unfinishedStructures = leftoverStructures;
                }

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
                else if(block.hasCustomModel)
                {
                    // append directory for this block
                    string blockModelPath = assetsPath + "/" + packName + "/" + block.blockName;

                    DirectoryInfo blockModelPathInfo = new DirectoryInfo(blockModelPath);
                    if (!blockModelPathInfo.Exists)
                    {
                        Directory.CreateDirectory(blockModelPathInfo.FullName);
                    }

                    foreach (string texturePath in block.blockCustomModelTexturePaths)
                    {
                        FileInfo texturePathInfo = new FileInfo(texturePath);
                        File.WriteAllBytes(blockModelPath + "/" + texturePathInfo.Name, File.ReadAllBytes(texturePath));
                    }
                    File.WriteAllBytes(blockModelPath + "/model.json", File.ReadAllBytes(block.blockModelPath));
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

            string exampleInitFunc = "";
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
                    exampleThings += "        public static BlockValue " + packBlocks[b].blockName + ";";
                    exampleInitFunc += "            " + packBlocks[b].blockName + " = new BlockValue(true, '" + packBlocks[b].blockName + "', '" + packName + "');";
                }
                else
                {
                    exampleThings += "        public static BlockValue " + packBlocks[b].blockName + ";";
                    exampleInitFunc += "            " + packBlocks[b].blockName + " = new BlockValue(false, '" + packBlocks[b].blockName + "', '" + packName + "');";
                }
                if (b != packBlocks.Count - 1)
                {
                    exampleThings += "\r\n";
                    exampleInitFunc += "\r\n";
                }
            }

            exampleThings = exampleThings.Replace("'", "\"");


            string beforeInitFunc = @"
        // This needed to be added to sync with Unity, since for some reason unity initializes
        // static fields on a seperate thread so sometimes we didn't finish initializing these
        // before Update was already being called for some classes
        public static void InitBlocks()
        {
";
            string afterInitFunc = @"
        }
";

            exampleThings += beforeInitFunc + exampleInitFunc.Replace("'", "\"") + afterInitFunc;

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
        public string blockModelPath;
        public BlockModel blockModel;
        public bool isValid;
        public bool isTransparent;
        public bool hasCustomModel = false;
        public string[] blockAnimationImagePaths;
        public string[] blockCustomModelTexturePaths;
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
                            //Debug.Log("got image for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
                            if (fInfo.Name == blockName + fInfo.Extension)
                            {
                                Debug.Log("image matches block name, using it for block art");
                                blockImagePath = fInfo.FullName;
                                isValid = true;
                            }
                        }
                        else if (fInfo.Extension.ToLower() == ".json")
                        {
                            Debug.Log("got model for block " + blockName + " with model path " + fInfo.FullName + " and model name " + fInfo.Name + " and extension " + fInfo.Extension);
                            if (fInfo.Name == blockName + fInfo.Extension || fInfo.Name == "model.json")
                            {
                                Debug.Log("model name matches block name or is model.json, using it for block model");
                                blockModelPath = fInfo.FullName;
                                hasCustomModel = true;

                            }
                        }
                        else {
                            Debug.Log("got non-image or model for block " + blockName + " with path " + fInfo.FullName + " and name " + fInfo.Name + " and extension " + fInfo.Extension);
                        }
                    }
                }
                // if this block has a custom model, use it
                if (hasCustomModel)
                {
                    hasCustomModel = false;
                    try
                    {
                        blockModel = BlockModel.FromJSONFilePath(blockModelPath);
                        blockCustomModelTexturePaths = blockModel.GetTexturePaths();
                        bool dependsOnState;
                        blockModel.ToRenderTriangles(out dependsOnState, BlockData.BlockRotation.Degrees0); // just for testing

                        if (dependsOnState)
                        {
                            blockModel.ToRenderTriangles(BlockData.BlockRotation.Degrees0, 0); // just for testing
                        }

                        // successfully parsed
                        Debug.Log("successfully processed model for block " + blockName + " with model path " + blockModelPath + " and " + blockCustomModelTexturePaths.Length + " textures with depends on state " + dependsOnState);
                        isValid = true;
                        hasCustomModel = true;
                        isTransparent = true;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("got exception when processing model file of " + blockModelPath + " for block " + blockName + " with block root dir " + blockRootDir + ". Exception was: " + e);
                    }

                }
                // otherwise, if we haven't found a block art or model file yet it might be animated
                else if (!isValid)
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
                                //Debug.Log("got image for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
                                if (fInfo.Name.Substring(0, blockName.Length) == blockName) // does the first piece match the block name?
                                {
                                    string leftoverPieces = fInfo.Name.Substring(blockName.Length); // get stuff after blockName
                                    leftoverPieces = leftoverPieces.Substring(0, leftoverPieces.Length - fInfo.Extension.Length); // remove extension
                                   // Debug.Log("is potential frame with key " + leftoverPieces + " for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);

                                    int frameNum;
                                    if (int.TryParse(leftoverPieces, out frameNum))
                                    {
                                        //Debug.Log("is actual frame with index " + frameNum + " for block " + blockName + " with image path " + fInfo.FullName + " and image name " + fInfo.Name + " and extension " + fInfo.Extension);
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

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshVertex
    {
        public Float4 pos;
        public Float2 uv;
        public Float2 color;
    }
    
    public class BlocksMesh : System.IDisposable
    {

        public static List<BlocksMesh> allBuffersMade = new List<BlocksMesh>();
        public static List<BlocksMesh> buffersNotRenderedThisFrame = new List<BlocksMesh>();

        public MeshVertex[] meshVertices;

        public ComputeBuffer drawData;

        public BlocksMesh(MeshVertex[] meshVertices)
        {
            this.meshVertices = meshVertices;
        }

        public BlocksMesh(Vector3[] vertices, Vector2[] uvs, Vector2[] colors)
        {
            meshVertices = new MeshVertex[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                meshVertices[i] = new MeshVertex()
                {
                    pos = new Float4()
                    {
                        x = vertices[i].x,
                        y = vertices[i].y,
                        z = vertices[i].z,
                        w = 1.0f
                    },
                    uv = new Float2()
                    {
                        x = uvs[i].x,
                        y = uvs[i].y
                    },
                    color = new Float2()
                    {
                        x = colors[i].x,
                        y = colors[i].y
                    }
                };
            }
        }


        public void MoveDataToGPU()
        {
            World.mainWorld.blocksWorld.drawDataMovedToGPUMemoryThisFrame += 1;

            // cleanup old data if modified
            DisposeOfGPUData();

            int sizeInBytesOfMeshVertex = Marshal.SizeOf(typeof(MeshVertex));
            drawData = new ComputeBuffer(meshVertices.Length, sizeInBytesOfMeshVertex);
            drawData.SetData(meshVertices, 0, 0, meshVertices.Length);
            allBuffersMade.Add(this);
        }


        public void Render(Matrix4x4 transformMat, Material mat)
        {
            if (drawData == null)
            {
                World.mainWorld.chunksLoaded += 1;
                MoveDataToGPU();
            }
            if (Camera.current == World.mainWorld.blocksWorld.mainCameraPlayer)
            {

                World.mainWorld.blocksWorld.drawCallsThisFrame += 1;
                if (buffersNotRenderedThisFrame.Contains(this))
                {
                    buffersNotRenderedThisFrame.Remove(this);
                }
            }
            mat.SetBuffer("meshData", drawData);
            mat.SetPass(0);
            World.mainWorld.blocksWorld.trianglesDrawnThisFrame += (meshVertices.Length / 3);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, meshVertices.Length);
        }

        public void DisposeOfGPUData()
        {
            if (drawData != null)
            {
                allBuffersMade.Remove(this);
                if (!drawData.IsValid())
                {
                    Debug.Log("going to clean up but already cleaned up, rip me");
                }
                else
                {
                    drawData.Dispose();
                }
                drawData = null;
            }
        }


        bool disposed = false;
        public void Dispose()
        {
            disposed = true;
            DisposeOfGPUData();

        }

        ~BlocksMesh()
        {
            if (!disposed)
            {
                Debug.Log("destructor called but dispose was never called");
            }
            //Dispose();
        }
    }

    public class BlocksWorld : MonoBehaviour
    {

        public int viewDistX = 4;
        public int viewDistY = 2;
        public int viewDistZ = 4;
        public TimeSeries renderFpsTimeSeries;
        public TimeSeries fpsTimeSeries;
        public TimeSeries ticksPerFrameTimeSeries;
        public TimeSeries lightingTicksPerFrameTimeSeries;
        public TimeSeries generationsPerFrameTimeSeries;
        public TimeSeries rendersPerFrameTimeSeries;
        public TimeSeries drawCallsTimeSeries;
        public TimeSeries drawDataMovedToMemoryTimeSeries;
        public TimeSeries trianglesDrawnPerFrameTimeSeries;

        public Camera mainCameraPlayer;
        public int lightingTicksThisFrame = 0;
        public int ticksThisFrame = 0;
        public int generationsThisFrame = 0;
        public int rendersThisFrame = 0;
        public int drawCallsThisFrame = 0;
        public int drawDataMovedToGPUMemoryThisFrame = 0;
        public long trianglesDrawnThisFrame = 0;
        public static Material currentPassMat;

        public enum ProfileType
        {
            Generation,
            Tick
        }


        public ProfileType profileType;
        public bool doProfile = false;

        public int chunkRenderDist = 3;
        [HideInInspector]
        public Material debugLineMaterial;
        public float skyLightLevel = 1.0f;
        public ComputeShader cullBlocksShader;
        public bool creativeMode = false;
        public bool verbosePathing = false;
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
        public Material meshTriMaterial;
        public Material triMaterialWithTransparency;
        public Material breakingMaterial;
        public Material simpleMeshMaterial;
        public Transform renderTransform;
        public GameObject loggingNodePrefab;

        public GameObject blockEntityPrefab;
        public GameObject blockTileEntityPrefab;
        public GameObject blockRenderPrefab;
        public GameObject blockRenderCanvas;
        public GameObject blockRenderFrontCanvas;
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

        public ComputeBuffer[] chunkBlockDatas;
        public ComputeBuffer[] chunkBlockDatasForCombined;
        public ComputeBuffer[] nonCubePositions;

        int[] worldData;

        public const int chunkSizeX = 32;
        public const int chunkSizeY = 128;
        public const int chunkSizeZ = 32;

        public const int biomeDataSizeX = 16;
        public const int biomeDataSizeY = 16;
        public const int biomeDataSizeZ = 16;

        public World world;


        // from https://stackoverflow.com/a/4016511/2924421
        public long millis()
        {
            return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        }

        long lastTick = 0;
        public float ticksPerSecond = 20.0f;
        public float randomTicksPerSecond = 5.0f;



        public GameObject MakeLoggingNode(string tag, string text, Color color, long wx, long wy, long wz)
        {
            LVector3 pos = new LVector3(wx, wy, wz);
            Vector3 unityPos = pos.BlockCentertoUnityVector3();

            GameObject loggingNode = GameObject.Instantiate(loggingNodePrefab.gameObject);

            loggingNode.transform.GetComponent<Renderer>().material.color = color;
            loggingNode.GetComponent<LoggingNode>().logTag = tag;
            loggingNode.GetComponent<LoggingNode>().text.text = text;
            loggingNode.transform.position = unityPos;
            loggingNode.transform.name = text;

            return loggingNode;
        }

        public ComputeBuffer blockBreakingBuffer;
        public static Mesh blockMesh;

        public static float ApplyEpsPerturb(float val, float eps)
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
        public static Vector3 ApplyEpsPerturb(Vector3 offset, float eps)
        {
            return new Vector3(ApplyEpsPerturb(offset.x, eps), ApplyEpsPerturb(offset.y, eps), ApplyEpsPerturb(offset.z, eps));
        }

        public RenderTriangle[] cubeTris;
        
        
        static Float4 MakeFloat4(float x, float y, float z, float w)
        {
            Float4 res = new Float4
            {
                x = x,
                y = y,
                z = z,
                w = w
            };
            return res;
        }


        static Float2 MakeFloat2(float x, float y)
        {
            Float2 res = new Float2
            {
                x = x,
                y = y
            };
            return res;
        }


        ComputeBuffer helperForCustomBlocks;


        public static RenderTriangle[] GetDefaultCubeTris()
        {
            float epsPerturb = 1.0f / 64.0f;
            float[] texOffsets = new float[36 * 4];
            float[] vertNormals = new float[36 * 4];
            List<Vector2> texOffsetsGood = new List<Vector2>();
            List<Vector3> vertOffsetsGood = new List<Vector3>();
            List<RenderTriangle> defaultCubeTris = new List<RenderTriangle>();

            RenderTriangle curCubeTri = new RenderTriangle();
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

                Vector2 resTexOffset = new Vector2(texOffset.x / 2.0f, texOffset.y / 3.0f);
                Vector3 resOffset = offset - new Vector3(0.5f, 0.5f, 0.5f);
                texOffsetsGood.Add(resTexOffset);
                vertOffsetsGood.Add(resOffset);

                resOffset = offset;
                resTexOffset = new Vector2(texOffset.x / (2.0f * 64.0f), resTexOffset.y / (float)World.numBlocks);
                if (i % 3 == 0)
                {
                    curCubeTri.vertex1 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv1 = MakeFloat2(resTexOffset.x, resTexOffset.y);
                }
                else if (i % 3 == 1)
                {
                    curCubeTri.vertex2 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv2 = MakeFloat2(resTexOffset.x, resTexOffset.y);
                }
                else if (i % 3 == 2)
                {
                    curCubeTri.vertex3 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv3 = MakeFloat2(resTexOffset.x, resTexOffset.y);

                    // we are finished with current tri, store it and start a new one

                    defaultCubeTris.Add(curCubeTri);

                    curCubeTri = new RenderTriangle();
                }
            }

            return defaultCubeTris.ToArray();
        }


        void SetupRendering()
        {
            LoadMaterials();
            float epsPerturb = 1.0f/64.0f;

            float[] texOffsets = new float[36 * 4];
            float[] vertNormals = new float[36 * 4];
            List<Vector2> texOffsetsGood = new List<Vector2>();
            List<Vector3> vertOffsetsGood = new List<Vector3>();
            List<RenderTriangle> defaultCubeTris = new List<RenderTriangle>();

            RenderTriangle curCubeTri = new RenderTriangle();
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

                Vector2 resTexOffset = new Vector2(texOffset.x / 2.0f, texOffset.y / 3.0f);
                Vector3 resOffset = offset - new Vector3(0.5f, 0.5f, 0.5f);
                texOffsetsGood.Add(resTexOffset);
                vertOffsetsGood.Add(resOffset);

                resOffset = offset;
                resTexOffset = new Vector2(texOffset.x/(2.0f*64.0f), resTexOffset.y / (float)World.numBlocks);
                if (i % 3 == 0)
                {
                    curCubeTri.vertex1 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv1 = MakeFloat2(resTexOffset.x, resTexOffset.y);
                }
                else if (i % 3 == 1)
                {
                    curCubeTri.vertex2 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv2 = MakeFloat2(resTexOffset.x, resTexOffset.y);
                }
                else if (i % 3 == 2)
                {
                    curCubeTri.vertex3 = MakeFloat4(resOffset.x, resOffset.y, resOffset.z, 0);
                    curCubeTri.uv3 = MakeFloat2(resTexOffset.x, resTexOffset.y);

                    // we are finished with current tri, store it and start a new one

                    defaultCubeTris.Add(curCubeTri);

                    curCubeTri = new RenderTriangle();
                }
            }

            cubeTris = defaultCubeTris.ToArray();

            for (int i = 0; i < cubeTris.Length; i++)
            {
                //Debug.Log(i + " " + cubeTris[i].vertex1.x + " " + cubeTris[i].vertex1.y + " " + cubeTris[i].vertex1.z + " " +
                //    cubeTris[i].vertex2.x + " " + cubeTris[i].vertex2.y + " " + cubeTris[i].vertex2.z + " " +
                //    cubeTris[i].vertex3.x + " " + cubeTris[i].vertex3.y + " " + cubeTris[i].vertex3.z + " ");
            }

            blockBreakingBuffer = new ComputeBuffer(1, sizeof(int) * 4);
            cubeOffsets = new ComputeBuffer(36, sizeof(float) * 4);
            cubeOffsets.SetData(triOffsets);
            cubeOffsetsDefault = new List<Vector3>();
            cubeUVOffsetsDefault = new List<Vector2>();

            for (int i = 0;i < triOffsets.Length/4; i++)
            {
                Vector3 cur = new Vector3(triOffsets[i * 4], triOffsets[i * 4 + 1], triOffsets[i * 4 + 2]);
                cubeOffsetsDefault.Add(cur);
            }

            for (int i = 0; i < texOffsets.Length / 4; i++)
            {
                Vector2 cur = new Vector2(triOffsets[i * 4], triOffsets[i * 4 + 1]);
                cubeUVOffsetsDefault.Add(cur);
            }
            cubeNormals = new ComputeBuffer(36, sizeof(float) * 4);
            cubeNormals.SetData(vertNormals);
            uvOffsets = new ComputeBuffer(36, sizeof(float) * 4);
            uvOffsets.SetData(texOffsets);
            if (!World.DO_CPU_RENDER)
            {
                helperForCustomBlocks = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ * 12, sizeof(int) * 22, ComputeBufferType.GPUMemory);
            }

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
            Debug.LogWarning("Setting values to the breaking thing");
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

            if (!World.DO_CPU_RENDER)
            {
                drawPositions1 = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ, sizeof(int) * 4, ComputeBufferType.Append);
                drawPositions2 = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ, sizeof(int) * 4, ComputeBufferType.Append);
            }

            // do multiple so we can run the cull blocks in parallel and don't have to wait for each one to finish
            int numInBatch = 1;
            if (!World.DO_CPU_RENDER)
            {
                chunkBlockDatas = new ComputeBuffer[numInBatch];
                for (int i = 0; i < chunkBlockDatas.Length; i++)
                {
                    ComputeBuffer chunkBlockDataI = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ, sizeof(int) * 4, ComputeBufferType.GPUMemory);
                    chunkBlockDatas[i] = chunkBlockDataI;
                }

                nonCubePositions = new ComputeBuffer[numInBatch];
                for (int i = 0; i < nonCubePositions.Length; i++)
                {
                    ComputeBuffer nonCubePositionDataI = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ, sizeof(int), ComputeBufferType.Append);
                    nonCubePositions[i] = nonCubePositionDataI;
                }

                chunkBlockDatasForCombined = new ComputeBuffer[1];
                for (int i = 0; i < chunkBlockDatasForCombined.Length; i++)
                {
                    ComputeBuffer chunkBlockDataI = new ComputeBuffer(chunkSizeX * chunkSizeY * chunkSizeZ, sizeof(int) * 4, ComputeBufferType.GPUMemory);
                    chunkBlockDatasForCombined[i] = chunkBlockDataI;
                }
            }
             


            //cullBlocksShader.SetBuffer(0, "DataIn", chunkBlockData);
            //cullBlocksShader.SetBuffer(1, "DataIn", chunkBlockData);

            cullBlocksShader.SetBuffer(0, "cubeOffsets", cubeOffsets);
            cullBlocksShader.SetBuffer(0, "uvOffsets", uvOffsets);
            cullBlocksShader.SetBuffer(1, "cubeOffsets", cubeOffsets);
            cullBlocksShader.SetBuffer(1, "uvOffsets", uvOffsets);



            triMaterial.mainTexture = BlockValue.allBlocksTexture;
            triMaterialWithTransparency.mainTexture = BlockValue.allBlocksTexture;
            meshTriMaterial.mainTexture = BlockValue.allBlocksTexture;
            simpleMeshMaterial.mainTexture = BlockValue.allBlocksTexture;
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

        public List<Vector3> cubeOffsetsDefault;
        public List<Vector2> cubeUVOffsetsDefault;

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

            if (!debugLineMaterial)
            {
                Shader shader = Shader.Find("Lines/Colored Blended");
                debugLineMaterial = new Material(shader);
                debugLineMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public BlocksPack blocksPack;


        public BlocksPlayer[] players;
        public Vector3[] playerPositions;


        private void Awake()
        {
            BlockValue.SetupAirAndWildcard();
            Example.InitBlocks();
            BlockValue.FinishAddNewBlocks();
        }
        // Use this for initialization
        void Start()
        {


            creativeMode = World.creativeMode;

            SetupRendering();

            //Dictionary<BlockValue, Block> customBlocks = new Dictionary<BlockValue, Block>();
            //customBlocks[BlockValue.GRASS] = new Grass();
            //GenerationClass customGeneration = new ExampleGeneration();
            world = new World(this, blocksPack);

            lastTick = 0;

           
           world.helperThread = new Thread(() =>
           {
               while (threadKeepGoing)
               {
                   try
                   {

                       if (okToRunTicks)
                       {
                           if (doProfile && profileType == ProfileType.Tick)
                           {
                               world.Tick(true);
                           }
                           else
                           {
                               world.Tick(true);
                           }
                       }
                   }
                   catch (System.Exception e)
                   {
                       Debug.LogError(e);
                   }
                   Thread.Sleep(1);
             
               }
           });



            Thread savingThread = new Thread(() =>
            {
                while (threadKeepGoing)
                {
                    int numWroteChunks = 0;
                    List<Tuple<Chunk, long>> allChunksHere = world.GetChunksCloserThanRenderDist(false, int.MaxValue);
                    foreach (Tuple<Chunk, long> chunk in allChunksHere)
                    {
                        if(chunk.a.WriteToDisk())
                        {
                            numWroteChunks += 1;
                        }
                    }
                    int numWroteStructures = 0;
                    List<Structure> unfinishedStructuresCopy;
                    List<Structure> unfinishedStructuresTmp = world.unfinishedStructures;
                    if (unfinishedStructuresTmp == null) // this ensures we check to see if it is null first, relevant because threading
                    {
                        break;
                    }
                    unfinishedStructuresCopy = new List<Structure>(unfinishedStructuresTmp);
                    foreach (Structure structure in unfinishedStructuresCopy)
                    {
                        if (structure.WriteToDisk())
                        {
                            numWroteStructures += 1;
                        }
                    }
                    Debug.Log("Got through all, wrote " + numWroteStructures + " structures and " + numWroteChunks + " chunks");
                    if (numWroteStructures == 0 && numWroteChunks == 0)
                    {
                        Thread.Sleep(1000);
                    }
                }
            });

            savingThread.Start();

            if (World.DO_CPU_RENDER)
            {
                world.helperThread2s = new List<Thread>();
                int NUM_RENDER_THREADS = 8;
                for (int i = 0;i < NUM_RENDER_THREADS; i++)
                {
                    int myThreadI = i;
                    Thread curThread = new Thread(() =>
                    {
                        while (threadKeepGoing)
                        {
                            try
                            {

                                if (okToRunTicks)
                                {
                                    int maxDist = -1;
                                    if (myThreadI <= 1) // dedicate first thread to only rendering chunks nearby players
                                    {
                                        //maxDist = 1;
                                    }
                                    if (myThreadI <= 0)
                                    {
                                        //maxDist = 0;
                                    }
                                    List<Tuple<Chunk, long>> allChunksHere = world.GetChunksCloserThanRenderDist(false, maxDist: maxDist);

                                    if (myThreadI > NUM_RENDER_THREADS/2)
                                    {
                                        //allChunksHere.Shuffle();
                                    }
                                    foreach (Tuple<Chunk, long> chunk in allChunksHere)
                                    {
                                        if (!okToRunTicks || !threadKeepGoing)
                                        {
                                            break;
                                        }
                                        if (okToRunTicks && (chunk.a.chunkData.needToBeUpdated || chunk.a.needToUpdateLighting) && chunk.a.threadRenderingMe == -1 && chunk.a.chunkRenderer.renderStatus != ChunkRenderer.RenderStatus.HasTriangles && chunk.a.chunkRenderer.prevMesh == null)
                                        {
                                            bool foundSomething = false;
                                            // it is probably good to render, double check that someone hasn't changed that (by getting the lock before us)
                                            lock (chunk.a.chunkRenderer.renderingLock)
                                            {
                                                if (okToRunTicks && (chunk.a.chunkData.needToBeUpdated || chunk.a.needToUpdateLighting) && chunk.a.threadRenderingMe == -1 && chunk.a.chunkRenderer.renderStatus != ChunkRenderer.RenderStatus.HasTriangles && chunk.a.chunkRenderer.prevMesh == null)
                                                {
                                                    chunk.a.threadRenderingMe = myThreadI;
                                                    chunk.a.chunkData.needToBeUpdated = true;
                                                    foundSomething = true;
                                                }
                                                // someone else got the lock before us, don't render, skip to the next chunk
                                                else
                                                {
                                                    continue;
                                                }
                                            }

                                            if (!foundSomething)
                                            {
                                                Debug.LogWarning("continue didn't work, pls halp");
                                            }

                                            string stringVal = "thread " + myThreadI + " with " + chunk.a.threadRenderingMe + " is start rendering chunk " + chunk.a.cx + " " + chunk.a.cy + " " + chunk.a.cz + " with render status " + chunk.a.chunkRenderer.renderStatus + " and num times rendered " + chunk.a.numTimesRendered;

                                            if (chunk.a.threadRenderingMe != myThreadI)
                                            {
                                                Debug.LogWarning("hmm issue " + chunk.a.threadRenderingMe + " ??");
                                                continue;
                                            }

                                            int numUpdates = 0;
                                            while (chunk.a.needToUpdateLighting && okToRunTicks && threadKeepGoing)
                                            {
                                                long curTime = PhysicsUtils.millis();
                                                int numLightings = chunk.a.UpdateLightingForAllBlocks();
                                                if (PhysicsUtils.millis() - curTime > 200)
                                                {
                                                    Debug.Log("thread " + myThreadI + " doing lighting update " + numUpdates + " for chunk  " + chunk.a.cx + " " + chunk.a.cy + " " + chunk.a.cz + " with " + numLightings + " blocks updated lighting took " + (PhysicsUtils.millis() - curTime));
                                                }
                                                numUpdates += 1;
                                            }

                                            if (!threadKeepGoing || !okToRunTicks)
                                            {
                                                break;
                                            }

                                            //Debug.Log(stringVal);
                                            int numTris;
                                            long curMillis = PhysicsUtils.millis();
                                            List<RenderTriangle> renderTriangles = world.blocksWorld.MakeChunkTrisInCPU(chunk.a, null, out numTris);
                                            chunk.a.chunkRenderer.triangles = renderTriangles;
                                            chunk.a.chunkRenderer.prevMesh = chunk.a.chunkRenderer.myMesh; // store prev mesh object so it can be disposed (if needed), this must be done on the render thread because of disposing of compute buffers
                                            if (renderTriangles.Count > 0) // only make a mesh if it is not empty
                                            {
                                                chunk.a.chunkRenderer.myMesh = chunk.a.chunkRenderer.TrianglesToMesh(chunk.a.chunkRenderer.triangles);
                                            }
                                            rendersThisFrame += 1;
                                            if (renderTriangles.Count > 0)
                                            {
                                                //Debug.Log("thread " + myThreadI + " with " + chunk.a.threadRenderingMe + " is finished rendering  chunk " + chunk.a.cx + " " + chunk.a.cy + " " + chunk.a.cz + " it took " + (PhysicsUtils.millis() - curMillis) + " millis" + " and started at millis " + curMillis + " and ended at millis " + PhysicsUtils.millis() + " and has " + renderTriangles.Count + " triangles " + " with render status " + chunk.a.chunkRenderer.renderStatus + " and has been rendered " + chunk.a.numTimesRendered + " times");
                                            }
                                            // we need to acquire this lock because otherwise another thread reading halfway through our writing could cause it to read the wrong value
                                            lock (chunk.a.chunkRenderer.renderingLock)
                                            {
                                                chunk.a.chunkRenderer.renderStatus = ChunkRenderer.RenderStatus.HasTriangles;
                                                if (chunk.a.threadRenderingMe == myThreadI)
                                                {
                                                    chunk.a.threadRenderingMe = -1;
                                                }
                                                chunk.a.numTimesRendered += 1;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError(e);
                            }
                            Thread.Sleep(1);
                        }
                    });

                    if (i == 0)
                    {
                        curThread.Priority = System.Threading.ThreadPriority.Normal;
                    }
                    else
                    {
                        curThread.Priority = System.Threading.ThreadPriority.Lowest;
                    }
                    curThread.Start();
                    world.helperThread2s.Add(curThread);
                }
            }

            world.helperThread.Priority = System.Threading.ThreadPriority.Normal;
            world.helperThread.Start();

        }


        bool threadKeepGoing = true;

        int ax = 0;
        int ay = 0;
        int az = 0;


        public int numChunksTotal;
        public int drawCallsCount;
        public int allBuffersMadeCount;
        bool okToRunTicks = true;


        DoEveryMS doTick = new DoEveryMS(100);
        long millisAtStartOfLastUpdateFrame = 0;
        // Update is called once per frame
        void Update()
        {
            float millisPerTick = 1000.0f / ticksPerSecond;
            doTick.ms = (int)millisPerTick;
            if (doTick.Do())
            {
                world.Tick(false);
            }

            long curMillis = PhysicsUtils.millis();
            long passedMillis = curMillis - millisAtStartOfLastUpdateFrame;
            float fps = 1000.0f / (float)passedMillis;
            float millisPassed = Time.deltaTime * 1000.0f;
            if (millisPassed > 50)
            {
                millisPassed = 50;
            }
            fpsTimeSeries.Push(millisPassed);
            millisAtStartOfLastUpdateFrame = curMillis;

            players = FindObjectsOfType<BlocksPlayer>();
            List<BlocksPlayer> activePlayers = new List<BlocksPlayer>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].enabled)
                {
                    activePlayers.Add(players[i]);
                }
            }

            players = activePlayers.ToArray();

            playerPositions = new Vector3[players.Length];
            for(int i = 0; i < players.Length; i++)
            {
                playerPositions[i] = players[i].transform.position;
            }

            World.creativeMode = creativeMode;
            triMaterial.SetFloat("globalLightLevel", skyLightLevel);
            triMaterialWithTransparency.SetFloat("globalLightLevel", skyLightLevel);
            meshTriMaterial.SetFloat("globalLightLevel", skyLightLevel);
            simpleMeshMaterial.SetFloat("globalLightLevel", skyLightLevel);
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


        ComputeBuffer tmpNonCubePositions;


        int curChunkBlockDataI = 0;
        int curChunkBlockDataCombinedI = 0;


        public List<RenderTriangle> MakeChunkTrisInCPU(Chunk chunk, ComputeBuffer drawData, out int numTris, int offsetInDrawData = 0)
        {
            int numAllowedToDoFullRender = 1;
            return MakeChunkTrisInCPU(chunk, drawData, out numTris, ref numAllowedToDoFullRender, offsetInDrawData: offsetInDrawData);
        }


        public List<RenderTriangle> MakeChunkTrisInCPU(Chunk chunk, ComputeBuffer drawData, out int numTris, ref int numAllowedToDoFullRender, int offsetInDrawData = 0)
        {
            /*
            if (drawData != null && chunk.chunkRenderer.triangles != null) // if we aren't making this on a seperate thread (drawData will be null in that case) and a seperate thread has already made our triangles for us, we don't need to!
            {
                List<RenderTriangle> cachedTriangles = chunk.chunkRenderer.triangles;
                if (cachedTriangles != null) // check needed due to multithreading
                {
                    numTris = cachedTriangles.Count;
                    drawData.SetData<RenderTriangle>(cachedTriangles);
                    //chunk.chunkRenderer.triangles = null; // set to null since we done now
                    chunk.chunkRenderer.renderStatus = ChunkRenderer.RenderStatus.StoredToComputeBuffer; // set it to up to date now
                    return cachedTriangles;
                }
            }
            */

            if (drawData != null)
            {
                Debug.Log("doing full render in render thread");
            }

            numAllowedToDoFullRender -= 1;


            RenderTriangle[] defaultCubeTris = World.defaultCubeTris;
            Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;

            Matrix4x4 offsetMatrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);

            int[] chunkData = chunk.chunkData.GetRawData();
            int i = 0;
            int blockValue, blockState, blockLightingState, blockAnimState,lightingTouchingFlags;
            List<RenderTriangle> triangles = new List<RenderTriangle>();
            for (int z = 0; z < chunkSizeZ; z++)
            {
                for (int y = 0; y < chunkSizeY; y++)
                {
                    for (int x = 0; x < chunkSizeX; x++)
                    {
                        blockValue = chunkData[i++];
                        blockState = chunkData[i++];
                        blockLightingState = chunkData[i++];
                        blockAnimState = chunkData[i++];
                        lightingTouchingFlags = blockLightingState >> Chunk.OFFSET_FOR_TOUCHING_TRANSPARENT_OR_AIR_BIT_SIDES;

                        if (blockValue > 0 && ((blockLightingState & Chunk.TOUCHING_TRANPARENT_OR_AIR_BIT) != 0))
                        {
                            for (int k = 0; k < defaultCubeTris.Length; k++)
                            {
                                int blah = k/2;
                                int spook = 1 << blah;
                                int touching = lightingTouchingFlags & spook;
                                // cull hidden faces
                                if (touching != 0)
                                {
                                    Vector3 blockPos = new Vector3(x, y, z);
                                    RenderTriangle cur = new RenderTriangle();
                                    cur.state1 = blockValue;
                                    cur.state2 = blockState;
                                    cur.state3 = blockLightingState;
                                    cur.state4 = blockAnimState;
                                    cur.vertex1 = Util.MakeFloat4(offsetMatrix.MultiplyPoint(new Vector3(defaultCubeTris[k].vertex1.x + blockPos.x, defaultCubeTris[k].vertex1.y + blockPos.y, defaultCubeTris[k].vertex1.z + blockPos.z) * worldScale));
                                    cur.vertex2 = Util.MakeFloat4(offsetMatrix.MultiplyPoint(new Vector3(defaultCubeTris[k].vertex2.x + blockPos.x, defaultCubeTris[k].vertex2.y + blockPos.y, defaultCubeTris[k].vertex2.z + blockPos.z) * worldScale));
                                    cur.vertex3 = Util.MakeFloat4(offsetMatrix.MultiplyPoint(new Vector3(defaultCubeTris[k].vertex3.x + blockPos.x, defaultCubeTris[k].vertex3.y + blockPos.y, defaultCubeTris[k].vertex3.z + blockPos.z) * worldScale));

                                    float yOffset = (Mathf.Abs(blockValue) - 1) / 64.0f;
                                    //yOffset = 18.0f/64.0f;

                                    cur.uv1 = Util.MakeFloat2(defaultCubeTris[k].uv1.x, defaultCubeTris[k].uv1.y + yOffset);
                                    cur.uv2 = Util.MakeFloat2(defaultCubeTris[k].uv2.x, defaultCubeTris[k].uv2.y + yOffset);
                                    cur.uv3 = Util.MakeFloat2(defaultCubeTris[k].uv3.x, defaultCubeTris[k].uv3.y + yOffset);

                                    triangles.Add(cur);
                                }
                                    
                            }
                        }
                        else if(blockValue < 0 && ((blockLightingState & Chunk.TOUCHING_TRANPARENT_OR_AIR_BIT) != 0))
                        {
                            Vector3 blockPos = new Vector3(x, y, z);
                            RenderTriangle template = new RenderTriangle();
                            template.state1 = blockValue;
                            template.state2 = blockState;
                            template.state3 = blockLightingState;
                            template.state4 = blockAnimState;
                            triangles.AddRange(world.GetTrianglesForBlock(blockValue, blockAnimState, template, blockPos, offsetMatrix));
                        }
                    }
                }
            }

            if (drawData != null)
            {
                drawData.SetData<RenderTriangle>(triangles, 0, offsetInDrawData, triangles.Count);
            }



            numTris = triangles.Count;

            return triangles;
        }

        public int MakeChunkTrisForCombined(Chunk chunk,  ComputeBuffer drawData, int offsetInDrawData)
        {
            if (World.DO_CPU_RENDER)
            {
                int numTris;
                MakeChunkTrisInCPU(chunk, drawData, out numTris, offsetInDrawData:offsetInDrawData);
                return numTris;
            }
            else
            {
                Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz * chunkSizeZ)) * worldScale;
                renderTransform.transform.position += offset;

                ComputeBuffer curChunkBlockData = chunkBlockDatasForCombined[curChunkBlockDataCombinedI];
                curChunkBlockDataCombinedI = (curChunkBlockDataCombinedI + 1) % chunkBlockDatasForCombined.Length;
                curChunkBlockData.SetData(chunk.chunkData.GetRawData());

                for (int i = 0; i < 2; i++)
                {
                    cullBlocksShader.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    //cullBlocksShader.SetInt("ptCloudWidth", chunkSize);
                    cullBlocksShader.SetFloat("ptCloudScale", worldScale);
                    cullBlocksShader.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    cullBlocksShader.SetBuffer(0, "DataIn", curChunkBlockData);
                    cullBlocksShader.SetBuffer(1, "DataIn", curChunkBlockData);
                }
                // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
                cullBlocksShader.SetBuffer(0, "DrawingThings", drawData);
                cullBlocksShader.Dispatch(0, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);

                renderTransform.transform.position -= offset;

                //cullBlocksShader.SetBuffer(2, "NonCubePositions", )

                return 0;
            }

        }


        public void MakeChunkTrisAsync(Chunk chunk)
        {
            //int[] args = new int[] { 0 };

            if (World.DO_CPU_RENDER)
            {

            }
            else
            {

                Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;
                renderTransform.transform.position += offset;

                chunk.chunkRenderer.drawDataNotTransparent.SetCounterValue(0);

                curChunkBlockDataI = 0;
                ComputeBuffer curChunkBlockData = chunkBlockDatas[curChunkBlockDataI];
                ComputeBuffer curNotCubePositions = nonCubePositions[curChunkBlockDataI];
                chunk.indexUsingInBatch = curChunkBlockDataI;
                curChunkBlockDataI = (curChunkBlockDataI + 1) % chunkBlockDatas.Length;
                curChunkBlockData.SetData(chunk.chunkData.GetRawData());

                for (int i = 0; i < 2; i++)
                {
                    cullBlocksShader.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    //cullBlocksShader.SetInt("ptCloudWidth", chunkSize);
                    cullBlocksShader.SetFloat("ptCloudScale", worldScale);
                    cullBlocksShader.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    cullBlocksShader.SetBuffer(0, "DataIn", curChunkBlockData);
                    cullBlocksShader.SetBuffer(1, "DataIn", curChunkBlockData);
                    cullBlocksShader.SetBuffer(2, "DataIn", curChunkBlockData);
                }
                // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
                cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
                cullBlocksShader.Dispatch(0, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);

                curNotCubePositions.SetCounterValue(0);
                cullBlocksShader.SetBuffer(2, "NonCubePositions", curNotCubePositions);
                cullBlocksShader.Dispatch(2, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);


                /*
                ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataNotTransparent, world.argBuffer, 0);
                world.argBuffer.GetData(args);
                numNotTransparent = args[0];
                float[] resVals = new float[numNotTransparent*(4+4+4+2)];
                //Debug.Log("got num not transparent = " + numNotTransparent + " (" + (numNotTransparent / 36) + ")");

                chunk.chunkRenderer.drawDataTransparent.SetCounterValue(0);
                //chunkBlockData.SetData(chunk.chunkData.GetRawData());
                // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
                cullBlocksShader.SetBuffer(1, "DrawingThings", chunk.chunkRenderer.drawDataTransparent);
                cullBlocksShader.Dispatch(1, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);
                ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataTransparent, world.argBuffer, 0);
                world.argBuffer.GetData(args);
                numTransparent = args[0];
                */

                renderTransform.transform.position -= offset;

            }





        }



        public void FinishChunkTrisSync(Chunk chunk, out int numNotTransparent, out int numTransparent, ref int numAllowedToDoFullRender)
        {

            if (World.DO_CPU_RENDER)
            {
                MakeChunkTrisInCPU(chunk, chunk.chunkRenderer.drawDataNotTransparent, out numNotTransparent, numAllowedToDoFullRender: ref numAllowedToDoFullRender);
                numTransparent = 0;
            }
            else
            {

                int[] args = new int[] { 0 };
                Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;
                renderTransform.transform.position += offset;

                /*

                chunk.chunkRenderer.drawDataNotTransparent.SetCounterValue(0);

                ComputeBuffer curChunkBlockData = chunkBlockDatas[curChunkBlockDataI];
                curChunkBlockDataI = (curChunkBlockDataI + 1) % chunkBlockDatas.Length;
                curChunkBlockData.SetData(chunk.chunkData.GetRawData());

                for (int i = 0; i < 2; i++)
                {
                    cullBlocksShader.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    cullBlocksShader.SetInt("ptCloudWidth", chunkSize);
                    cullBlocksShader.SetFloat("ptCloudScale", worldScale);
                    cullBlocksShader.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    cullBlocksShader.SetBuffer(0, "DataIn", curChunkBlockData);
                    cullBlocksShader.SetBuffer(1, "DataIn", curChunkBlockData);
                }
                // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
                cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
                cullBlocksShader.Dispatch(0, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);
                */
                ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataNotTransparent, world.argBuffer, 0);
                world.argBuffer.GetData(args);
                numNotTransparent = args[0];

                ComputeBuffer curNonCubePositions = nonCubePositions[chunk.indexUsingInBatch];

                ComputeBuffer.CopyCount(curNonCubePositions, world.argBuffer, 0);
                world.argBuffer.GetData(args);
                int numNonCubes = args[0];



                if (numNonCubes > 0)
                {
                    //Debug.Log("drawing custom model " + chunk.cx + " " + chunk.cy + " " + chunk.cz);
                    int[] nonCubePositions = new int[numNonCubes];
                    chunk.chunkRenderer.hasCustomBlocks = true;
                    curNonCubePositions.GetData(nonCubePositions);

                    List<RenderTriangle> bonusTriangles = new List<RenderTriangle>();

                    //Debug.Log("got " + numNonCubes + " non cubes for a total of " + bonusTriangles.Count + " bonus triangles");
                    /*
                    RenderTriangle[] prevTriangles = new RenderTriangle[numNotTransparent];
                    for (int i = 0; i < prevTriangles.Length; i++)
                    {
                        prevTriangles[i] = new RenderTriangle();
                        prevTriangles[i].vertex1 = new Float4();
                        prevTriangles[i].vertex2 = new Float4();
                        prevTriangles[i].vertex3 = new Float4();
                        prevTriangles[i].uv1 = new Float2();
                        prevTriangles[i].uv2 = new Float2();
                        prevTriangles[i].uv3 = new Float2();
                    }
                    */


                    //chunk.chunkRenderer.drawDataNotTransparent.GetStructData(prevTriangles);
                    //bonusTriangles.AddRange(prevTriangles);



                    int[] rawData = chunk.chunkData.GetRawData();
                    RenderTriangle template = new RenderTriangle();
                    for (int i = 0; i < nonCubePositions.Length; i++)
                    {
                        int pos = nonCubePositions[i] * 4;
                        int blockId = rawData[pos];
                        template.state1 = rawData[pos];
                        template.state2 = rawData[pos + 1];
                        template.state3 = rawData[pos + 2];
                        template.state4 = rawData[pos + 3];

                        int lx, ly, lz;
                        chunk.chunkData.to3D(nonCubePositions[i], out lx, out ly, out lz);
                        Vector3 blockPos = new Vector3(lx, ly, lz);
                        int stateForBlockAnim = rawData[pos + 3];


                        bonusTriangles.AddRange(world.GetTrianglesForBlock(blockId, stateForBlockAnim, template, blockPos, renderTransform.localToWorldMatrix));
                    }



                    helperForCustomBlocks.SetStructData(bonusTriangles.ToArray());

                    cullBlocksShader.SetBuffer(3, "DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
                    cullBlocksShader.SetBuffer(3, "ThingsToAddToDrawingThings", helperForCustomBlocks);

                    cullBlocksShader.Dispatch(3, bonusTriangles.Count, 1, 1);

                    RenderTriangle[] resTriangles = bonusTriangles.ToArray();
                    //chunk.chunkRenderer.drawDataNotTransparent.SetCounterValue((uint)resTriangles.Length);
                    //chunk.chunkRenderer.drawDataNotTransparent.SetStructData(resTriangles);
                    numNotTransparent += bonusTriangles.Count;

                }
                else
                {

                    chunk.chunkRenderer.hasCustomBlocks = false;
                }


                //float[] resVals = new float[numNotTransparent * (4 + 4 + 4 + 2)];
                //Debug.Log("got num not transparent = " + numNotTransparent + " (" + (numNotTransparent / 36) + ")");

                /*
                chunk.chunkRenderer.drawDataTransparent.SetCounterValue(0);
                //chunkBlockData.SetData(chunk.chunkData.GetRawData());
                // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
                cullBlocksShader.SetBuffer(1, "DrawingThings", chunk.chunkRenderer.drawDataTransparent);
                cullBlocksShader.Dispatch(1, chunkSizeX / 8, chunkSizeY / 8, chunkSizeZ / 8);
                ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataTransparent, world.argBuffer, 0);
                world.argBuffer.GetData(args);
                numTransparent = args[0];
                */

                //// new: I added this tmp and commented out the above
                numTransparent = 0;

                renderTransform.transform.position -= offset;
            }
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
            Vector3 offset = (new Vector3(cx*chunkSizeX, cy*chunkSizeY, cz*chunkSizeZ)) * worldScale;
            renderTransform.transform.position += offset;
            Chunk chunkIn = world.GetChunk(cx, cy, cz);
            if (chunkIn != null)
            {
                //chunkIn.SetNeighborsAsTouchingAir(x, y, z);
                Debug.Log("Not in chunk");
            }

            LVector3 pos = new LVector3(x, y, z);
            int brokenState = (int)Mathf.Clamp(Mathf.Round(howBroken * 8), 0.0f, 8.0f);
            blockBreakingBuffer.SetData(new int[] { (int)(x - cx * chunkSizeX), (int)(y - cy * chunkSizeY), (int)(z - cz * chunkSizeZ), brokenState });
            if (Camera.current != uiCamera)
            {
                Debug.Log("drawing thing at " + x + " " + y + " " + z + " with howBroken=" + howBroken);
                breakingMaterial.SetBuffer("DrawingThings", blockBreakingBuffer);
                breakingMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                //breakingMaterial.SetInt("ptCloudWidth", chunkSize);
                breakingMaterial.SetFloat("ptCloudScale", worldScale);
                breakingMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                breakingMaterial.SetPass(0);
                //Graphics.DrawProceduralNow(breakingMaterial, new Bounds(pos.BlockCentertoUnityVector3(), 10000.0f * new Vector3(worldScale, worldScale, worldScale)), MeshTopology.Triangles, 1*36, 1*36, Camera.current);
                Graphics.DrawProceduralNow(MeshTopology.Triangles, 1 * (36), 1);
            }
            renderTransform.transform.position -= offset;
        }

        MaterialPropertyBlock properties = null;

        public void RenderChunkMesh(Chunk chunk, Mesh mesh)
        {
            if (properties == null)
            {
                properties = new MaterialPropertyBlock();
            }
            if (Camera.current != uiCamera)
            {
                //Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;
                //renderTransform.transform.position += offset;

                meshTriMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, renderTransform.transform.localToWorldMatrix);

                //renderTransform.transform.position -= offset;
            }
        }


        public void RenderChunkBlocksMesh(Chunk chunk, BlocksMesh mesh)
        {
            if (Camera.current != uiCamera)
            {
                //Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;
                //renderTransform.transform.position += offset;

                mesh.Render(renderTransform.transform.localToWorldMatrix, simpleMeshMaterial);

                //renderTransform.transform.position -= offset;
            }
        }

        public void RenderChunk(Chunk chunk, bool onlyTransparent)
        {
            //if (Camera.current != transform.GetComponent<Camera>() || settings.isOn)
            //{
            //    return;
            //}
            //Vector3 offset = metaSize * (renderTransform.transform.forward * sandScale * sandDim / 2.0f + renderTransform.transform.right * sandScale * sandDim / 2.0f + renderTransform.transform.up * sandScale * sandDim / 2.0f);
            //Vector3 offset = (new Vector3(chunk.cx*chunkSizeX, chunk.cy*chunkSizeY, chunk.cz*chunkSizeZ)) * worldScale;
            //renderTransform.transform.position += offset;
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
                    //triMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                    //triMaterial.SetInt("ptCloudWidth", chunkSize);
                    //triMaterial.SetBuffer("PixelData", sandPixelData);
                    //triMaterial.SetFloat("ptCloudScale", worldScale);
                    //triMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                    triMaterial.SetPass(0);
                    Graphics.DrawProceduralNow(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesNotTransparent*3);


                    if (chunk.chunkRenderer.hasCustomBlocks)
                    {
                        //Debug.Log("Drawing with cutom blcoks " + chunk.cx + " " + chunk.cy + " " + chunk.cz + " with " + chunk.chunkRenderer.numRendereredCubesNotTransparent + " triangles at position " + renderTransform.transform.position);
                    }

                    // Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesNotTransparent * (36));
                }
            }
            //renderTransform.transform.position -= offset;
        }

        LVector3 blockBreaking;
        long millisAtStartOfLastRenderFrame;

        List<Tuple<LVector3, float>> blocksBreakingThisFrame = new List<Tuple<LVector3, float>>();

        public void OnRenderObject()
        {
            world.Render();

            if (Camera.current == World.mainWorld.blocksWorld.mainCameraPlayer)
            {

                long curMillis = PhysicsUtils.millis();
                long passedMillis = curMillis - millisAtStartOfLastRenderFrame;
                float fps = 1000.0f / (float)passedMillis;
                renderFpsTimeSeries.Push(fps);
                millisAtStartOfLastRenderFrame = curMillis;

                generationsPerFrameTimeSeries.Push(generationsThisFrame);
                ticksPerFrameTimeSeries.Push(ticksThisFrame);
                lightingTicksPerFrameTimeSeries.Push(lightingTicksThisFrame);
                rendersPerFrameTimeSeries.Push(rendersThisFrame);
                drawCallsTimeSeries.Push(drawCallsThisFrame);
                drawDataMovedToMemoryTimeSeries.Push(drawDataMovedToGPUMemoryThisFrame);
                drawCallsThisFrame = System.Math.Max(drawCallsThisFrame, 1); // make sure not 0 for averaging
                trianglesDrawnPerFrameTimeSeries.Push((float)trianglesDrawnThisFrame / (float)drawCallsThisFrame);

                drawCallsCount = drawCallsThisFrame;
                allBuffersMadeCount = BlocksMesh.allBuffersMade.Count;
                if (drawCallsThisFrame != BlocksMesh.allBuffersMade.Count)
                {
                    //Debug.Log("draw calls this frame = " + drawCallsThisFrame + " and allBuffersMade count = " + BlocksMesh.allBuffersMade.Count);
                }

                lightingTicksThisFrame = 0;
                ticksThisFrame = 0;
                generationsThisFrame = 0;
                rendersThisFrame = 0;
                drawCallsThisFrame = 0;
                drawDataMovedToGPUMemoryThisFrame = 0;
                trianglesDrawnThisFrame = 0;

                if (BlocksMesh.buffersNotRenderedThisFrame.Count > 0)
                {
                    for (int i =0; i < BlocksMesh.buffersNotRenderedThisFrame.Count; i++)
                    {
                        if(BlocksMesh.buffersNotRenderedThisFrame[i].drawData != null)
                        {
                            BlocksMesh.buffersNotRenderedThisFrame[i].DisposeOfGPUData();
                            //Debug.Log("didn't draw buffer");
                        }
                    }
                }

                BlocksMesh.buffersNotRenderedThisFrame.Clear();
                BlocksMesh.buffersNotRenderedThisFrame.AddRange(BlocksMesh.allBuffersMade);
            }
            
            if (Camera.current == uiCamera)
            {
                blocksBreakingThisFrame.Clear();

                // Move them from the queue into the list (we need to do this since this code is executed multiple times per frame, once for each camera)
                while (blocksBreaking.Count > 0)
                {
                    Tuple<LVector3, float> blockBreaking = blocksBreaking.Dequeue();
                    blocksBreakingThisFrame.Add(blockBreaking);
                }
            }


            // Actually render the blocks breaking this frame
            for (int i = 0; i < blocksBreakingThisFrame.Count; i++)
            {
                Tuple<LVector3, float> blockBreaking = blocksBreakingThisFrame[i];
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
            if (helperForCustomBlocks != null)
            {
                helperForCustomBlocks.Dispose();
                helperForCustomBlocks = null;
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


            if (chunkBlockDatas != null)
            {
                for (int i = 0; i < chunkBlockDatas.Length; i++)
                {

                    chunkBlockDatas[i].Dispose();
                    chunkBlockDatas[i] = null;
                }
                chunkBlockDatas = null;
            }


            if (chunkBlockDatasForCombined != null)
            {
                for (int i = 0; i < chunkBlockDatasForCombined.Length; i++)
                {

                    chunkBlockDatasForCombined[i].Dispose();
                    chunkBlockDatasForCombined[i] = null;
                }
                chunkBlockDatasForCombined = null;
            }



            if (nonCubePositions != null)
            {
                for (int i = 0; i < nonCubePositions.Length; i++)
                {
                    if (nonCubePositions[i] != null)
                    {
                        nonCubePositions[i].Dispose();
                        nonCubePositions[i] = null;
                    }
                }
                nonCubePositions = null;
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
                okToRunTicks = false;
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


namespace ExtensionMethods
{
    public static class BlocksComputeBufferExtensions
    {

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static unsafe extern void CopyMemory(void* dest, void* src, int count);

        private static unsafe void BlockCopy(byte[] src, int srcOffset, Blocks.RenderTriangle[] dst, int dstOffset, int count) 
        {
            fixed (void* s = &src[0])
            {
                fixed (void* d = &dst[0])
                {
                    CopyMemory(d, s, src.Length);
                }
            }
        }

        // from https://stackoverflow.com/a/25311889/2924421
        private static byte[] ToByteArray<T>(T[] source) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            try
            {
                System.IntPtr pointer = handle.AddrOfPinnedObject();
                byte[] destination = new byte[source.Length * Marshal.SizeOf(typeof(T))];
                Marshal.Copy(pointer, destination, 0, destination.Length);
                return destination;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        private static void CopyFromByteArray<T>(byte[] source, T[] destination) 
        {
            GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            try
            {
                System.IntPtr pointer = handle.AddrOfPinnedObject();
                Marshal.Copy(source, 0, pointer, source.Length);
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        private static unsafe void BlockCopy(Blocks.RenderTriangle[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            fixed (void* s = &src[0])
            {
                fixed (void* d = &dst[0])
                {
                    CopyMemory(d, s, dst.Length);
                }
            }
        }

        public static void GetStructData(this ComputeBuffer buffer, Blocks.RenderTriangle[] outData)
        {
            if (outData.Length == 0)
            {
                return;
            }

            byte[] rawData = new byte[System.Runtime.InteropServices.Marshal.SizeOf(typeof(Blocks.RenderTriangle)) * outData.Length];
            buffer.GetData(outData);


            //CopyFromByteArray(rawData, outData);

            //BlockCopy(rawData, 0, outData, 0, rawData.Length);
        }

        public static void SetStructData(this ComputeBuffer buffer, Blocks.RenderTriangle[] inData)
        {
            if (inData.Length == 0)
            {
                return;
            }

            //byte[] rawData = ToByteArray(inData);
            buffer.SetData(inData);

            //byte[] rawData = new byte[System.Runtime.InteropServices.Marshal.SizeOf(typeof(Blocks.RenderTriangle)) * inData.Length];
            //BlockCopy(inData, 0, rawData, 0, rawData.Length);
            //buffer.SetData(rawData);
        }
    }
}