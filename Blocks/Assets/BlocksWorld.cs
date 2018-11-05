using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

public class ChunkRenderer
{
    Chunk chunk;
    int chunkSize;
    public ComputeBuffer drawData;
    public int numRendereredCubes;

    public ChunkRenderer(Chunk chunk, int chunkSize)
    {
        this.chunk = chunk;
        this.chunkSize = chunkSize;

        drawData = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);

    }

    public void Tick()
    {
        if (chunk.chunkData.needToBeUpdated)
        {
            chunk.chunkData.needToBeUpdated = false;
            numRendereredCubes = chunk.world.blocksWorld.MakeChunkTris(chunk);
        }
    }
    public void Render()
    {
        chunk.world.blocksWorld.RenderChunk(chunk);
    }


    bool cleanedUp = false;

    public void Dispose()
    {
        if (!cleanedUp)
        {
            cleanedUp = true;
            if (drawData != null)
            {
                drawData.Dispose();
                drawData = null;
            }
        }
    }
}


public class Chunk
{
    public long[] chunkPos;
    public long cx { get { return chunkPos[0]; } set { chunkPos[0] = value; } }
    public long cy { get { return chunkPos[1]; } set { chunkPos[1] = value; } }
    public long cz { get { return chunkPos[2]; } set { chunkPos[2] = value; } }

    int chunkSize;
    public ChunkData chunkData;
    public ChunkRenderer chunkRenderer;

    public Chunk[] posChunks;
    public Chunk xPosChunk { get { return posChunks[0]; } set { posChunks[0] = value; } }
    public Chunk yPosChunk { get { return posChunks[1]; } set { posChunks[1] = value; } }
    public Chunk zPosChunk { get { return posChunks[2]; } set { posChunks[2] = value; } }

    public Chunk[] negChunks;
    public Chunk xNegChunk { get { return negChunks[0]; } set { negChunks[0] = value; } }
    public Chunk yNegChunk { get { return negChunks[1]; } set { negChunks[1] = value; } }
    public Chunk zNegChunk { get { return negChunks[2]; } set { negChunks[2] = value; } }

    public World world;



    public void AddBlockUpdate(long i, long j, long k)
    {
        long relativeX = i - cx * chunkSize;
        long relativeY = j - cy * chunkSize;
        long relativeZ = k - cz * chunkSize;
        chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
    }

    public Chunk(World world, long chunkX, long chunkY, long chunkZ, int chunkSize)
    {
        this.world = world;
        this.chunkSize = chunkSize;
        this.chunkRenderer = new ChunkRenderer(this, chunkSize);
        this.chunkPos = new long[] { chunkX, chunkY, chunkZ };
        posChunks = new Chunk[] { null, null, null };
        negChunks = new Chunk[] { null, null, null };
        chunkData = new ChunkData(chunkSize);

        long baseX = chunkX * chunkSize;
        long baseY = chunkY * chunkSize;
        long baseZ = chunkZ * chunkSize;

        generating = true;
        for (long x = baseX; x < baseX + this.chunkSize; x++)
        {
            for (long y = baseY; y < baseY + this.chunkSize; y++)
            {
                for (long z = baseZ; z < baseZ + this.chunkSize; z++)
                {
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
                    SetState(x, y, z, World.maxCapacities[this[x, y, z]], 2);
                }
            }
        }



        generating = false;
    }

    public void Tick()
    {
        this.chunkRenderer.Tick();

        if (chunkData.blocksNeedUpdating.Count != 0)
        {
            int[] oldBlocksNeedUpdating = new int[chunkData.blocksNeedUpdating.Count];
            int i = 0;
            foreach (int ind in chunkData.blocksNeedUpdating)
            {
                oldBlocksNeedUpdating[i] = ind;
                i += 1;
            }
            chunkData.blocksNeedUpdating.Clear();
            for (i = 0; i < oldBlocksNeedUpdating.Length; i++)
            {
                long ind = (long)oldBlocksNeedUpdating[i];
                long x, y, z;
                chunkData.to3D(ind, out x, out y, out z);
                long wx = x + cx * chunkSize;
                long wy = y + cy * chunkSize;
                long wz = z + cz * chunkSize;
                int resState1;
                int resState2;
                bool needsAnotherUpdate;
                int resBlock = world.UpdateBlock(wx, wy, wz, chunkData[x,y,z], chunkData.GetState(x,y,z, 1), chunkData.GetState(x, y, z, 2), out resState1, out resState2, out needsAnotherUpdate);
                chunkData.SetState(x, y, z, resState1, 1, needBlockUpdate: needsAnotherUpdate, forceBlockUpdate: needsAnotherUpdate);
                chunkData.SetState(x, y, z, resState2, 2, needBlockUpdate: needsAnotherUpdate, forceBlockUpdate: needsAnotherUpdate);
                chunkData.SetBlock(x, y, z, resBlock, needBlockUpdate: needsAnotherUpdate, forceBlockUpdate: needsAnotherUpdate);
            }
        }
    }


    public bool generating = true;


    public void SetState(long x, long y, long z, int state, int stateI)
    {
        long relativeX = x - cx * chunkSize;
        long relativeY = y - cy * chunkSize;
        long relativeZ = z - cz * chunkSize;
        chunkData.SetState(relativeX, relativeY, relativeZ, state, stateI, true, true);
    }
    public int GetState(long x, long y, long z, int stateI)
    {
        long relativeX = x - cx * chunkSize;
        long relativeY = y - cy * chunkSize;
        long relativeZ = z - cz * chunkSize;
        return chunkData.GetState(relativeX, relativeY, relativeZ, stateI);
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
            if (!generating)
            {
                world.AddBlockUpdateToNeighbors(x, y, z);
            }
            chunkData[relativeX, relativeY, relativeZ] = value;
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
    public HashSet<int> blocksNeedUpdating = new HashSet<int>();
    int[] data;
    int chunkSize, chunkSize_2, chunkSize_3;
    public ChunkData(int chunkSize)
    {
        this.chunkSize = chunkSize;
        this.chunkSize_2 = chunkSize* chunkSize;
        this.chunkSize_3 = chunkSize* chunkSize* chunkSize;
        data = new int[chunkSize * chunkSize * chunkSize * 4];
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

    public int to1D(int x, int y, int z)
    {
        return x + y * chunkSize + z * chunkSize_2;
    }

    public void to3D(long ind, out long x, out long y, out long z)
    {
        z = ind / (chunkSize_2);
        ind -= (z * chunkSize_2);
        y = ind / chunkSize;
        x = ind % chunkSize;
    }

    public long to1D(long x, long y, long z)
    {
        return x + y * chunkSize + z * chunkSize_2;
    }

    public int GetState(long i, long j, long k, int stateI)
    {
        if (stateI <= 0 || stateI >= 3)
        {
            throw new System.ArgumentOutOfRangeException("stateI can only be 1 or 2, was " + stateI + " instead");
        }
        long ind = to1D(i, j, k);
        return data[ind * 4 + stateI];
    }

    public void SetState(long i, long j, long k, int state, int stateI, bool needBlockUpdate = true, bool forceBlockUpdate = false)
    {
        long ind = to1D(i, j, k);
        if (stateI <= 0 || stateI >= 3)
        {
            throw new System.ArgumentOutOfRangeException("stateI can only be 1 or 2, was " + stateI + " instead");
        }
        if (data[ind * 4 + stateI] == state && !forceBlockUpdate)
        {
            return;
        }

        needToBeUpdated = true;
        data[ind * 4 + stateI] = state;
        if (needBlockUpdate)
        {
            blocksNeedUpdating.Add((int)ind);
        }
    }


    public void AddBlockUpdate(long i, long j, long k)
    {
        blocksNeedUpdating.Add((int)to1D(i, j, k));
    }

    public int GetBlock(long i, long j, long k)
    {
        long ind = to1D(i, j, k);
        return data[ind * 4 + 1];
    }

    public void SetBlock(long i, long j, long k, int block, bool needBlockUpdate=true, bool forceBlockUpdate = false)
    {
        long ind = to1D(i, j, k);
        if (data[ind * 4] == block && !forceBlockUpdate)
        {
            return;
        }

        needToBeUpdated = true;
        data[ind * 4] = block;
        if (needBlockUpdate)
        {
            blocksNeedUpdating.Add((int)ind);
        }
    }



    public int this[int i, int j, int k]
    {
        get
        {
            return data[(i + j * chunkSize + k * chunkSize_2)*4];
        }
        set
        {
            long ind = to1D(i, j, k);
            if (data[ind * 4] == value)
            {
                return;
            }

            needToBeUpdated = true;
            data[ind * 4] = value;
            blocksNeedUpdating.Add((int)ind);
        }
    }

    public int this[long i, long j, long k]
    {
        get
        {
            return data[(i + j * chunkSize + k * chunkSize_2)*4];
        }
        set
        {
            long ind = to1D(i, j, k);
            if (data[ind * 4] == value)
            {
                return;
            }

            needToBeUpdated = true;
            data[ind * 4] = value;
            blocksNeedUpdating.Add((int)ind);
        }
    }


    public void Dispose()
    {

    }
}

public class World
{
    public static World mainWorld;

    public static int numBlocks = 5;
    public const int BEDROCK = 5;
    public const int DIRT = 4;
    public const int GRASS = 3;
    public const int STONE = 2;
    public const int SAND = 1;
    public const int AIR = 0;
    public BlocksWorld blocksWorld;
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

    int chunkSize;
    Dictionary<long, List<Chunk>> chunksPerX;
    Dictionary<long, List<Chunk>> chunksPerY;
    Dictionary<long, List<Chunk>> chunksPerZ;
    Dictionary<long, List<Chunk>>[] chunksPer;
    public const int DIM = 3;

    public ComputeBuffer argBuffer;

    public List<Chunk> allChunks;
    public static Dictionary<int, int> maxCapacities;

    public World(BlocksWorld blocksWorld, int chunkSize)
    {
        World.mainWorld = this;
        this.blocksWorld = blocksWorld;
        this.chunkSize = chunkSize;
        chunksPerX = new Dictionary<long, List<Chunk>>();
        chunksPerY = new Dictionary<long, List<Chunk>>();
        chunksPerZ = new Dictionary<long, List<Chunk>>();
        chunksPer = new Dictionary<long, List<Chunk>>[] { chunksPerX, chunksPerY, chunksPerZ };

        argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        allChunks = new List<Chunk>();

        maxCapacities = new Dictionary<int, int>();
        maxCapacities[DIRT] = 3;
        maxCapacities[STONE] = 5;
        maxCapacities[GRASS] = 4;
        maxCapacities[SAND] = 0;
        maxCapacities[AIR] = 0;
        maxCapacities[BEDROCK] = 6;
        numBlocks = maxCapacities.Count;

        GenerateChunk(0, 0, 0);
    }

    public bool NeedsInitialUpdate(int block)
    {
        if (block == World.GRASS)
        {
            return true;
        }
        return false;
    }


    public void SetState(long i, long j, long k, int state, int stateI)
    {
        long chunkX = divWithFloor(i, chunkSize);
        long chunkY = divWithFloor(j, chunkSize);
        long chunkZ = divWithFloor(k, chunkSize);
        Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
        chunk.SetState(i, j, k, state, stateI);
    }
    public int GetState(long i, long j, long k, int stateI)
    {
        long chunkX = divWithFloor(i, chunkSize);
        long chunkY = divWithFloor(j, chunkSize);
        long chunkZ = divWithFloor(k, chunkSize);
        Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
        return chunk.GetState(i, j, k, stateI);
    }


    public int TrickleSupportPowerUp(int blockFrom, int powerFrom, int blockTo)
    {
        int maxCapacityTo = maxCapacities[blockTo];
        if (blockTo == AIR)
        {
            return 0;
        }
        if (blockFrom == AIR)
        {
            return 0;
        }
        return System.Math.Min(powerFrom, maxCapacityTo); // doesn't lose support power if stacked on top of each other, but certain types of blocks can only hold so much support power
        return powerFrom; // or we just carry max power for up?
    }
    public int TrickleSupportPowerSidewaysOrDown(int blockFrom, int powerFrom, int blockTo)
    {
        if (blockTo == AIR)
        {
            return 0;
        }
        if (blockFrom == AIR)
        {
            return 0;
        }
        int maxCapacityTo = maxCapacities[blockTo];
        return System.Math.Max(0, System.Math.Min(powerFrom-1, maxCapacityTo)); // loses 1 support power if not up, also some blocks are more "sturdy" than others
    }

    public static int[] sidewaysNeighborsX = new int[] { -1, 1, 0, 0 };
    public static int[] sidewaysNeighborsZ = new int[] { 0, 0, -1, 1 };




    public IEnumerable<LVector3> SidewaysNeighbors()
    {
        yield return new LVector3(-1, 0, 0);
        yield return new LVector3(1, 0, 0);
        yield return new LVector3(0, 0, -1);
        yield return new LVector3(0, 0, 1);
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

    public int UpdateBlock(long wx, long wy, long wz, int block, int state1, int state2, out int resState1, out int resState2, out bool needsAnotherUpdate)
    {
        needsAnotherUpdate = false;
        resState1 = state1;
        resState2 = state2;
        //Debug.Log("updating block " + wx + " " + wy + " " + wz + " " + block + " " + state);
        if (block == AIR)
        {
            resState1 = 0;
            resState2 = 0;
            needsAnotherUpdate = false;
            return AIR;
        }

        if (state1 > 1)
        {
            if (block == AIR)
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
        if (block == BEDROCK)
        {
            supportPower = maxCapacities[BEDROCK];
        }
        else if (block != AIR)
        {
            int greatestNeighborSupportPower = 0;
            if (this[wx, wy - 1, wz] != AIR)
            {
                int belowSupportPower = GetState(wx, wy - 1, wz, 2);
                greatestNeighborSupportPower = TrickleSupportPowerUp(this[wx, wy - 1, wz], belowSupportPower, block);
                /*
                if (resSupportPower != supportPower)
                {
                    needsAnotherUpdate = true;
                    supportPower = resSupportPower;
                    AddBlockUpdateToNeighbors(wx, wy, wz);
                    //Debug.Log("got support power of " + resSupportPower + " from below");
                }
                */
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

        if (supportPower <= 0 && this[wx, wy-1, wz] == AIR)
        {
            Debug.Log("rip me support power is not good enough and I have air below");
            this[wx, wy - 1, wz] = block;
            SetState(wx, wy - 1, wz, 2, 1); // don't update again until next tick
            resState1 = 0;
            resState2 = 0;
            needsAnotherUpdate = true;
            AddBlockUpdateToNeighbors(wx, wy, wz);
            return AIR;
        }
        else
        {
            resState1 = 0;
        }

        if (block == World.GRASS)
        {
            float prGrass = 0.005f;
            //Debug.Log("updating grass block " + wx + " " + wy + " " + wz + " " + block + " " + state);
            if (this[wx, wy + 1, wz] == AIR)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (this[wx + 1, wy+y, wz] == DIRT)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx + 1, wy + y, wz] = GRASS;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx - 1, wy + y, wz] == DIRT)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx - 1, wy + y, wz] = GRASS;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx, wy + y, wz + 1] == DIRT)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx, wy + y, wz + 1] = GRASS;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                    if (this[wx, wy + y, wz - 1] == DIRT)
                    {
                        if (Random.value < prGrass)
                        {
                            this[wx, wy + y, wz - 1] = GRASS;
                        }
                        else
                        {
                            needsAnotherUpdate = true;
                        }
                    }

                }
                //Debug.Log("updating grass block " + needsAnotherUpdate + " <- needs update? with air above " + wx + " " + wy + " " + wz + " " + block + " " + state);
                return GRASS;
            }
            else
            {
                return DIRT;
            }
        }
        else if (block == SAND)
        {
            if (state1 <= 0)
            {
                // if air below, fall
                if (this[wx, wy - 1, wz] == AIR)
                {
                    this[wx, wy - 1, wz] = SAND;
                    SetState(wx, wy - 1, wz, 1, 1); // don't update again until next tick
                    resState1 = 0; 
                    needsAnotherUpdate = true;
                    return AIR;
                }
                // block below, don't fall
                else
                {
                    resState1 = 0;
                    needsAnotherUpdate = false;
                    return SAND;
                }
            }
            // we already moved this tick, set our state to zero so we can try moving again next tick
            else
            {
                needsAnotherUpdate = true;
                resState1 = state1 - 1;
                return SAND;
            }
        }
        else
        {
            return block;
        }
    }
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
    long divWithFloor(long a, long b)
    {
        if (a % b == 0)
        {
            return a / b;
        }
        else
        {
            if ((a < 0 && b > 0) || (a > 0 && b < 0))
            {
                return a / b - 1; // if a and b differ by a sign, this rounds up, round down instead (in other words, always floor)
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
    public int this[long x, long y, long z]
    {
        get
        {
            long chunkX = divWithFloor(x, chunkSize);
            long chunkY = divWithFloor(y, chunkSize);
            long chunkZ = divWithFloor(z, chunkSize);
            Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
            return chunk[x, y, z];
        }

        set
        {
            long chunkX = divWithFloor(x, chunkSize);
            long chunkY = divWithFloor(y, chunkSize);
            long chunkZ = divWithFloor(z, chunkSize);
            Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
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

    public Chunk GetChunk(long chunkX, long chunkY, long chunkZ)
    {
        return GetChunk(new long[] { chunkX, chunkY, chunkZ });
    }

    Chunk GetChunk(long[] pos)
    {
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
            foreach (Chunk chunk in chunksPer[minDim][pos[minDim]])
            {
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
            }
            return null;
        }
    }

    public Chunk GenerateChunk(long chunkX, long chunkY, long chunkZ, bool checkIfExists=true)
    {
        if (checkIfExists)
        {
            Chunk existingChunk = GetChunk(chunkX, chunkY, chunkZ);
            if (existingChunk != null)
            {
                return existingChunk;
            }
        }
        Chunk res = new Chunk(this, chunkX, chunkY, chunkZ, chunkSize);
        AddChunkToDataStructures(res);
        return res;
    }

    int off = 0;
    void AddChunkToDataStructures(Chunk chunk)
    {
        long[] curPos = new long[] { chunk.cx, chunk.cy, chunk.cz };
        for (int d = 0; d < DIM; d++)
        {
            if (!chunksPer[d].ContainsKey(chunk.chunkPos[d]))
            {
                chunksPer[d][chunk.chunkPos[d]] = new List<Chunk>();
            }

            chunksPer[d][chunk.chunkPos[d]].Add(chunk);


            // link to node before
            curPos[d] = chunk.chunkPos[d] - 1;
            Chunk beforeChunk = GetChunk(curPos);
            if (beforeChunk != null)
            {
                beforeChunk.posChunks[d] = chunk;
                chunk.negChunks[d] = beforeChunk;
            }


            // link to node after
            curPos[d] = chunk.chunkPos[d] + 1;
            Chunk afterChunk = GetChunk(curPos);
            if (afterChunk != null)
            {
                afterChunk.negChunks[d] = chunk;
                chunk.posChunks[d] = afterChunk;
            }

            // reset back to chunk pos so next dim can use this array as well
            curPos[d] = chunk.chunkPos[d];
        }
        allChunks.Add(chunk);

        /*
        // fun code that makes chunks repeat around in a very non-euclidean way
        if (allChunks.Count > 32)
        {
            chunk.chunkData = allChunks[off].chunkData;
            off = (off + 1) % 32;
            return;
        }

        */
    }


    public void AddBlockUpdate(long i, long j, long k, bool alsoToNeighbors=true)
    {
        GetOrGenerateChunk(divWithFloor(i, chunkSize), divWithFloor(j, chunkSize), divWithFloor(k, chunkSize)).AddBlockUpdate(i, j, k);
        if (alsoToNeighbors)
        {
            AddBlockUpdateToNeighbors(i, j, k);
        }
    }

    public void AddBlockUpdateToNeighbors(long i, long j, long k)
    {
        AddBlockUpdate(i-1, j, k, alsoToNeighbors: false);
        AddBlockUpdate(i+1, j, k, alsoToNeighbors: false);
        AddBlockUpdate(i, j-1, k, alsoToNeighbors: false);
        AddBlockUpdate(i, j+1, k, alsoToNeighbors: false);
        AddBlockUpdate(i, j, k-1, alsoToNeighbors: false);
        AddBlockUpdate(i, j, k+1, alsoToNeighbors: false);
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
        foreach (Chunk chunk in allChunks)
        {
            chunk.chunkRenderer.Render();
        }
    }

    public void Tick()
    {
        List<Chunk> allChunksHere = new List<Chunk>(allChunks);

        foreach (Chunk chunk in allChunksHere)
        {
            chunk.Tick();
        }
    }
}

public class BlocksWorld : MonoBehaviour {

    public ComputeShader cullBlocksShader;
    ComputeBuffer cubeOffsets;
    ComputeBuffer uvOffsets;
    ComputeBuffer linesOffsets;
    Material outlineMaterial;
    public Material triMaterial;
    public Transform renderTransform;

    ComputeBuffer drawPositions1;
    ComputeBuffer drawPositions2;
    bool isUsing1 = true;
    ComputeBuffer bufferProcessingWith;
    ComputeBuffer bufferDrawingWith;
    public float worldScale = 0.1f;
    public Camera uiCamera;


    public ComputeBuffer chunkBlockData;

    int[] worldData;

    int chunkSize = 16;

    public World world;


    // from https://stackoverflow.com/a/4016511/2924421
    public long millis()
    {
        return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }

    long lastTick = 0;
    public float ticksPerSecond = 20.0f;
    void SetupRendering()
    {
        LoadMaterials();

        float[] texOffsets = new float[36 * 4];
        float[] triOffsets = new float[36 * 4];
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
            if (i >= 0 && i <= 5)
            {
                texX = offset.x + 1.0f;
                texY = offset.z + 2.0f;
            }
            // top
            else if (i >= 6 && i <= 11)
            {
                texX = offset.x;
                texY = offset.z + 2.0f;
            }
            // left
            else if (i >= 12 && i <= 17)
            {
                texX = offset.x;
                texY = offset.y + 1.0f;
            }
            // right
            else if (i >= 18 && i <= 23)
            {
                texX = offset.x + 1.0f;
                texY = offset.y + 1.0f;
            }
            // forward
            else if (i >= 24 && i <= 29)
            {
                texX = offset.z;
                texY = offset.y;
            }
            // back
            else if (i >= 30 && i <= 35)
            {
                texX = offset.z + 1.0f;
                texY = offset.y;
            }
            texOffset = new Vector2(texX, texY);
            triOffsets[i * 4] = offset.x;
            triOffsets[i * 4 + 1] = offset.y;
            triOffsets[i * 4 + 2] = offset.z;
            triOffsets[i * 4 + 3] = 0;


            texOffsets[i * 4] = texOffset.x/2.0f;
            texOffsets[i * 4 + 1] = texOffset.y/(float)World.numBlocks;
            texOffsets[i * 4 + 2] = 0;
            texOffsets[i * 4 + 3] = 0;
        }


        cubeOffsets = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        cubeOffsets.SetData(triOffsets);
        uvOffsets = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        uvOffsets.SetData(texOffsets);

        triMaterial.SetBuffer("cubeOffsets", cubeOffsets);
        triMaterial.SetBuffer("uvOffsets", uvOffsets);

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



        linesOffsets = new ComputeBuffer(24, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        linesOffsets.SetData(lineOffsets);

        outlineMaterial.SetBuffer("lineOffsets", linesOffsets);


        drawPositions1 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
        drawPositions2 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);

        chunkBlockData = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.GPUMemory);
        cullBlocksShader.SetBuffer(0, "DataIn", chunkBlockData);
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

        if (!outlineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Unlit/SandLineDrawer");
            outlineMaterial = new Material(shader);
            outlineMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    // Use this for initialization
    void Start () {
        SetupRendering();
        world = new World(this, chunkSize);
        lastTick = 0;
    }

    int ax = 0;
    int ay = 0;
    int az = 0;

    // Update is called once per frame
    void Update () {
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
            world.Tick();
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



    public int MakeChunkTris(Chunk chunk)
    {
        chunk.chunkRenderer.drawData.SetCounterValue(0);
        chunkBlockData.SetData(chunk.chunkData.GetRawData());
        cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawData);
        cullBlocksShader.Dispatch(0, 2, 2, 2);
        int[] args = new int[] { 0 };
        ComputeBuffer.CopyCount(chunk.chunkRenderer.drawData, world.argBuffer, 0);
        world.argBuffer.GetData(args);
        return args[0];
    }

    public void RenderChunk(Chunk chunk)
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
            triMaterial.SetBuffer("DrawingThings", chunk.chunkRenderer.drawData);
            triMaterial.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
            triMaterial.SetInt("ptCloudWidth", chunkSize);
            //triMaterial.SetBuffer("PixelData", sandPixelData);
            triMaterial.SetFloat("ptCloudScale", worldScale);
            triMaterial.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
            triMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubes * (36));
        }
        renderTransform.transform.position -= offset;
    }

    void OnRenderObject()
    {
        world.Render();
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
    }
    void Cleanup()
    {
        if (!cleanedUp)
        {
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
