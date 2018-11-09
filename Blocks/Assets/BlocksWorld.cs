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
    bool using1;
    public int numRendereredCubes;

    public ChunkRenderer(Chunk chunk, int chunkSize)
    {
        this.chunk = chunk;
        this.chunkSize = chunkSize;

        drawData = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
    }

    public void Tick()
    {

    }
    public void Render()
    {
        if (chunk.chunkData.needToBeUpdated)
        {
            chunk.chunkData.needToBeUpdated = false;
            numRendereredCubes = chunk.world.blocksWorld.MakeChunkTris(chunk);
        }
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

public class ChunkBiomeData
{
    public float altitude;
    public long cx, cy, cz;
    public ChunkBiomeData(long cx, long cy, long cz)
    {
        Perlin a = new Perlin();
        this.cx = cx;
        this.cy = cy;
        this.cz = cz;
        // should be in -1 to 1 range now?
        altitude = (float)Simplex.Noise.Generate(cx/10.0f, 0, cz/10.0f)*30.0f+40.0f;
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
    public ChunkBiomeData chunkBiomeData;

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
        this.chunkBiomeData = new ChunkBiomeData(chunkX, chunkY, chunkZ);
        this.chunkPos = new long[] { chunkX, chunkY, chunkZ };
        posChunks = new Chunk[] { null, null, null };
        negChunks = new Chunk[] { null, null, null };
        chunkData = new ChunkData(chunkSize);

        long baseX = chunkX * chunkSize;
        long baseY = chunkY * chunkSize;
        long baseZ = chunkZ * chunkSize;

        generating = true;

    }

    public void Generate()
    {
        generating = true;
        long baseX = cx * chunkSize;
        long baseY = cy * chunkSize;
        long baseZ = cz * chunkSize;
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

                    /*
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
                    */
                    SetState(x, y, z, World.maxCapacities[this[x, y, z]], 2);
                }
            }
        }
        chunkData.blocksNeedUpdating.Clear();
        chunkData.blocksNeedUpdatingNextFrame.Clear();
        generating = false;
    }

    public void TickStart()
    {

        this.chunkData.TickStart();
    }

    public void Tick()
    {
        if (generating)
        {
            Generate();


            generating = false;
        }
        this.chunkRenderer.Tick();


        if (chunkData.blocksNeedUpdating.Count != 0)
        {
            /*
            int[] oldBlocksNeedUpdating = new int[chunkData.blocksNeedUpdating.Count];
            int i = 0;
            foreach (int ind in chunkData.blocksNeedUpdating)
            {
                oldBlocksNeedUpdating[i] = ind;
                i += 1;
            }
            chunkData.blocksNeedUpdating.Clear();
            */
            foreach (int i in chunkData.blocksNeedUpdating)
            {
                long ind = (long)(i);
                long x, y, z;
                chunkData.to3D(ind, out x, out y, out z);
                long wx = x + cx * chunkSize;
                long wy = y + cy * chunkSize;
                long wz = z + cz * chunkSize;
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
            }
        }
    }


    public bool generating = true;


    public void SetState(long x, long y, long z, int state, int stateI)
    {
        long relativeX = x - cx * chunkSize;
        long relativeY = y - cy * chunkSize;
        long relativeZ = z - cz * chunkSize;
        bool addedUpdate;
        chunkData.SetState(relativeX, relativeY, relativeZ, state, stateI, out addedUpdate);
        // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
        if (!generating && addedUpdate)
        {
            world.AddBlockUpdateToNeighbors(x, y, z);
        }
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
            bool addedUpdate;
            chunkData.SetBlock(relativeX, relativeY, relativeZ, value, out addedUpdate);
            // if we aren't generating (so we don't trickle updates infinately) and we modified the block, add a block update call to this block's neighbors
            if (!generating && addedUpdate)
            {
                world.AddBlockUpdateToNeighbors(x, y, z);
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
    public HashSet<int> blocksNeedUpdating = new HashSet<int>();
    public HashSet<int> blocksNeedUpdatingNextFrame = new HashSet<int>();
    int[] data;
    int chunkSize, chunkSize_2, chunkSize_3;
    public ChunkData(int chunkSize)
    {
        this.chunkSize = chunkSize;
        this.chunkSize_2 = chunkSize* chunkSize;
        this.chunkSize_3 = chunkSize* chunkSize* chunkSize;
        data = new int[chunkSize * chunkSize * chunkSize * 4];
    }

    public void TickStart()
    {
        HashSet<int> tmp = blocksNeedUpdating;
        blocksNeedUpdating = blocksNeedUpdatingNextFrame;
        blocksNeedUpdatingNextFrame = tmp;
        blocksNeedUpdatingNextFrame.Clear();
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
        if (stateI <= 0 || stateI >= 4)
        {
            throw new System.ArgumentOutOfRangeException("stateI can only be 1 or 2, was " + stateI + " instead");
        }
        long ind = to1D(i, j, k);
        return data[ind * 4 + stateI];
    }

    public void SetState(long i, long j, long k, int state, int stateI, out bool addedBlockUpdate, bool forceBlockUpdate = false)
    {
        addedBlockUpdate = false;
        long ind = to1D(i, j, k);
        if (stateI <= 0 || stateI >= 4)
        {
            throw new System.ArgumentOutOfRangeException("stateI can only be 1 or 2, was " + stateI + " instead");
        }
        if (data[ind * 4 + stateI] != state)
        {
            //needToBeUpdated = true;
        }
        if (forceBlockUpdate || (data[ind * 4 + stateI] != state && stateI != 3))
        {
            addedBlockUpdate = true;
            blocksNeedUpdatingNextFrame.Add((int)ind);
        }

        data[ind * 4 + stateI] = state;
    }


    public void AddBlockUpdate(long i, long j, long k)
    {
        blocksNeedUpdatingNextFrame.Add((int)to1D(i, j, k));
    }

    public int GetBlock(long i, long j, long k)
    {
        long ind = to1D(i, j, k);
        return data[ind * 4 + 1];
    }

    public void SetBlock(long i, long j, long k, int block, out bool addedBlockUpdate, bool forceBlockUpdate = false)
    {
        addedBlockUpdate = false;
        long ind = to1D(i, j, k);
        if (data[ind * 4] != block)
        {
            needToBeUpdated = true;
        }
        if (forceBlockUpdate || data[ind * 4] != block)
        {
            addedBlockUpdate = true;
            blocksNeedUpdatingNextFrame.Add((int)ind);
        }

        data[ind * 4] = block;
    }


    public int this[long i, long j, long k]
    {
        get
        {
            return data[(i + j * chunkSize + k * chunkSize_2)*4];
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

public class World
{
    public static World mainWorld;

    public static int numBlocks = 7;
    public const int WATER_NOFLOW = 7;
    public const int WATER = 6;
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

    public List<Chunk> chunkCache;
    int cacheSize = 20;

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

        chunkCache = new List<Chunk>(cacheSize);

        maxCapacities = new Dictionary<int, int>();
        maxCapacities[DIRT] = 3;
        maxCapacities[STONE] = 5;
        maxCapacities[GRASS] = 4;
        maxCapacities[SAND] = 0;
        maxCapacities[AIR] = 0;
        maxCapacities[WATER] = 0;
        maxCapacities[WATER_NOFLOW] = 0;
        maxCapacities[BEDROCK] = 6;
        numBlocks = maxCapacities.Count;

        int viewDist = 3;
        for (int i = -viewDist; i <= viewDist; i++)
        {
            for (int j = -viewDist; j <= viewDist; j++)
            {
                for (int k = -viewDist; k <= viewDist; k++)
                {
                    GenerateChunk(i,j,k);
                }
            }
        }
    }




    public delegate float ChunkValueGetter(ChunkBiomeData chunk);

    public float AverageChunkValues(long x, long y, long z, ChunkValueGetter getChunkValue)
    {
        ChunkBiomeData chunkx1z1 = new ChunkBiomeData(divWithFloor(x, chunkSize), divWithFloor(y, chunkSize), divWithFloor(z, chunkSize));
        ChunkBiomeData chunkx2z1 = new ChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithFloor(z, chunkSize));
        ChunkBiomeData chunkx1z2 = new ChunkBiomeData(divWithFloor(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));
        ChunkBiomeData chunkx2z2 = new ChunkBiomeData(divWithCeil(x, chunkSize), divWithFloor(y, chunkSize), divWithCeil(z, chunkSize));

        long x1Weight = x - chunkx1z1.cx * chunkSize;
        long x2Weight = chunkx2z1.cx * chunkSize - x;
        long z1Weight = z - chunkx1z1.cz * chunkSize;
        long z2Weight = chunkx2z1.cz * chunkSize - z;

        float px = x1Weight / (float)chunkSize;
        float pz = z1Weight / (float)chunkSize;


        float valZ1 = getChunkValue(chunkx1z1) * (1 - px) + getChunkValue(chunkx2z1) * px;
        float valZ2 = getChunkValue(chunkx1z2) * (1 - px) + getChunkValue(chunkx2z2) * px;

        return valZ1 * (1 - pz) + valZ2 * pz;
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




    public IEnumerable<LVector3> SidewaysNeighbors(bool up=false, bool down=false)
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


    public IEnumerable<LVector3> SidewaysNeighborsRelative(LVector3 pos, bool vertical=false)
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
    
    // if a unpushed water block gets an update, flood fill back and replace all water on top of water with pushed water
     

    public bool IsWater(int block)
    {
        return block == WATER || block == WATER_NOFLOW;
    }
    
    public int GetNumAirNeighbors(long wx, long wy, long wz)
    {
        return
            (this[wx + 1, wy, wz] == AIR ? 1 : 0) +
            (this[wx - 1, wy, wz] == AIR ? 1 : 0) +
            (this[wx, wy + 1, wz] == AIR ? 1 : 0) +
            (this[wx, wy - 1, wz] == AIR ? 1 : 0) +
            (this[wx, wy, wz + 1] == AIR ? 1 : 0) +
            (this[wx, wy, wz - 1] == AIR ? 1 : 0);

    }

    public int UpdateBlock(long wx, long wy, long wz, int block, int state1, int state2, int state3, out int resState1, out int resState2, out int resState3, out bool needsAnotherUpdate)
    {
        needsAnotherUpdate = false;
        resState1 = state1;
        resState2 = state2;
        resState3 = state3;
        //Debug.Log("updating block " + wx + " " + wy + " " + wz + " " + block + " " + state);
        if (block == AIR)
        {
            resState1 = 0;
            resState2 = 0;
            needsAnotherUpdate = false;
            return AIR;
        }

        if (block == SAND)
        {
            if (this[wx, wy-1, wz] == WATER || this[wx, wy-1, wz] == WATER_NOFLOW)
            {
                this[wx, wy - 1, wz] = WATER;
                needsAnotherUpdate = true;
                resState1 = 1 - state1;
                SetState(wx, wy - 1, wz, resState1, 1);
                return block;
            }
            else if(this[wx, wy-1, wz] == AIR)
            {
                this[wx, wy - 1, wz] = WATER;
                //SetState(wx, wy - 1, wz, 1, 3);
                return block;
            }
        }




        if (block == WATER || block == WATER_NOFLOW)
        {

            needsAnotherUpdate = false;


            // if we are WATER without water above and with water below, pathfind to look for open space
            if (block == WATER && IsWater(this[wx, wy - 1, wz]) && !IsWater(this[wx, wy + 1, wz]))
            {
                // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz) =>
                    {
                        return by < wy && (b == WATER || b == WATER_NOFLOW);
                    },
                   isBlockDesiredResult: (b, bx, by, bz) =>
                   {
                       if (b == AIR && by < wy)
                       {
                           this[bx, by, bz] = WATER;
                           SetState(bx, by, bz, GetNumAirNeighbors(bx, by, bz), 3);
                           return true;
                       }
                       return false;
                   }
                ))
                {
                    resState3 = 0;
                    return AIR;
                }
                else
                {
                    needsAnotherUpdate = true;
                    return WATER_NOFLOW;
                }
                /*
                HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
                Queue<LVector3> nodes = new Queue<LVector3>();
                LVector3 myPos = new LVector3(wx, wy, wz);
                nodes.Enqueue(myPos);
                nodesSoFar.Add(myPos);
                int steps = 0;
                int maxSteps = 10;
                while (nodes.Count > 0)
                {
                    LVector3 curNode = nodes.Dequeue();
                    foreach (LVector3 neighbor in AllNeighborsRelative(curNode))
                    {
                        // only consider blocks with y level lower than us
                        if (neighbor.y >= myPos.y)
                        {
                            continue;
                        }
                        int neighborBlock = neighbor.Block;
                        // if we found a valid air block, flow WATER into there and replace us with AIR
                        if (neighborBlock == AIR)
                        {
                            resState3 = 0;
                            this[neighbor.x, neighbor.y, neighbor.z] = WATER;
                            SetState(neighbor.x, neighbor.y, neighbor.z, GetNumAirNeighbors(neighbor.x, neighbor.y, neighbor.z), 3);
                            return AIR;
                        }
                        // otherwise, keep searching through other water blocks
                        else if (!nodesSoFar.Contains(neighbor) && IsWater(neighborBlock))
                        {
                            nodes.Enqueue(neighbor);
                            nodesSoFar.Add(neighbor);
                        }
                    }
                    steps += 1;
                    if (steps >= maxSteps)
                    {
                        break;
                    }
                }
                */
                needsAnotherUpdate = true;
                //resState3 = GetNumAirNeighbors(wx, wy, wz);
                // failed to find, set us to WATER_NOFLOW
                return WATER_NOFLOW;
            }
            else
            {
                int prevNumAirNeighbors = state3;
                int curNumAirNeighbors = GetNumAirNeighbors(wx, wy, wz);
                resState3 = curNumAirNeighbors;
                // if we have a more air neighbors, flood fill back and set valid blocks that have WATER_NOFLOW back to WATER so they can try again
                if (curNumAirNeighbors > prevNumAirNeighbors)
                {
                    LVector3 airNeighbor = new LVector3(wx, wy, wz);
                    foreach (LVector3 neighbor in AllNeighborsRelative(new LVector3(wx, wy, wz)))
                    {
                        if (neighbor.Block == AIR)
                        {
                            airNeighbor = neighbor;
                            break;
                        }
                    }

                    // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                    if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz) =>
                    {
                        return (b == WATER || b == WATER_NOFLOW);
                    },
                       isBlockDesiredResult: (b, bx, by, bz) =>
                       {
                           if (b == WATER_NOFLOW && IsWater(this[bx, by - 1, bz]) && airNeighbor.y < by)
                           {
                               this[bx, by, bz] = AIR;
                               return true;
                           }
                           return false;
                       }
                    ))
                    {

                        this[airNeighbor.x, airNeighbor.y, airNeighbor.z] = WATER;
                        SetState(airNeighbor.x, airNeighbor.y, airNeighbor.z, GetNumAirNeighbors(airNeighbor.x, airNeighbor.y, airNeighbor.z), 3);
                        resState3 = curNumAirNeighbors - 1; // we just replaced an air neighbor with water
                        return WATER;
                    }
                    else
                    {
                        needsAnotherUpdate = true;
                        return WATER_NOFLOW;
                    }
                }
            }
            /*

                    HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
                    Queue<LVector3> nodes = new Queue<LVector3>();
                    LVector3 myPos = new LVector3(wx, wy, wz);
                    nodes.Enqueue(myPos);
                    nodesSoFar.Add(myPos);
                    int steps  = 0;
                    int maxSteps = 10;
                    while (nodes.Count > 0)
                    {
                        LVector3 curNode = nodes.Dequeue();
                        foreach (LVector3 neighbor in AllNeighborsRelative(curNode))
                        {
                            int neighborBlock = neighbor.Block;
                            // if we found a valid air block, flow WATER into there and replace us with AIR
                            if (!nodesSoFar.Contains(neighbor) && IsWater(neighborBlock))
                            {
                                nodes.Enqueue(neighbor);
                                nodesSoFar.Add(neighbor);

                                if (this[neighbor.x, neighbor.y, neighbor.z] == WATER_NOFLOW && IsWater(this[neighbor.x, neighbor.y-1, neighbor.z])  && !IsWater(this[neighbor.x, neighbor.y+1, neighbor.z]))
                                {
                                    foreach (LVector3 myAirNeighbor in AllNeighborsRelative(myPos))
                                    {
                                        if (myAirNeighbor.Block == AIR)
                                        {
                                            this[neighbor.x, neighbor.y, neighbor.z] = AIR;
                                            this[myAirNeighbor.x, myAirNeighbor.y, myAirNeighbor.z] = WATER;
                                            SetState(myAirNeighbor.x, myAirNeighbor.y, myAirNeighbor.z, GetNumAirNeighbors(myAirNeighbor.x, myAirNeighbor.y, myAirNeighbor.z), 3); // tell it to trickle more if it fails
                                            steps = maxSteps;
                                            break;
                                        }
                                    }
                                    //this[neighbor.x, neighbor.y, neighbor.z] = WATER;
                                }
                            }
                        }
                        steps += 1;
                        if (steps >= maxSteps)
                        {
                            break;
                        }
                    }
                }
            }
            */


            // if air below, set below = water and us = air
            if (this[wx, wy - 1, wz] == AIR)
            {
                this[wx, wy - 1, wz] = WATER;
                resState3 = 0;
                SetState(wx, wy - 1, wz, GetNumAirNeighbors(wx, wy - 1, wz)+1, 3); // +1 because we are now air instead of water
                return AIR;
            }
            else
            {
                // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                foreach (LVector3 neighbor in SidewaysNeighbors())
                {
                    LVector3 pos = new LVector3(wx, wy, wz);
                    LVector3 nPos = pos + neighbor;
                    LVector3 nPos2 = pos + neighbor * 2;
                    if ((nPos.Block == AIR && (this[nPos.x, nPos.y-1, nPos.z] == AIR || (nPos2.Block == AIR && this[nPos2.x, nPos2.y-1, nPos2.z] == AIR))))
                    {
                        this[nPos.x, nPos.y, nPos.z] = WATER;
                        resState3 = 0;
                        SetState(nPos.x, nPos.y, nPos.z, GetNumAirNeighbors(nPos.x, nPos.y, nPos.z) + 1, 3); // +1 because we are now air instead of water
                        return AIR;
                    }
                }
            }
            return block;

        }
        /*
        if (block == WATER_NOFLOW)
        {
            needsAnotherUpdate = false;
            if (this[wx, wy - 1, wz] == AIR)
            {
                HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
                Queue<LVector3> nodes = new Queue<LVector3>();
                LVector3 myPos = new LVector3(wx, wy, wz);
                nodes.Enqueue(myPos);
                nodesSoFar.Add(myPos);
                int steps = 0;
                int maxSteps = 2000;
                while (nodes.Count > 0)
                {
                    LVector3 curNode = nodes.Dequeue();
                    foreach (LVector3 neighbor in AllNeighborsRelative(curNode))
                    {
                        if (!nodesSoFar.Contains(neighbor) && (neighbor.Block == WATER || neighbor.Block == WATER_NOFLOW))
                        {
                            nodes.Enqueue(neighbor);
                            nodesSoFar.Add(neighbor);
                            this[neighbor.x, neighbor.y, neighbor.z] = WATER;
                        }
                    }
                    steps += 1;
                    if (steps >= maxSteps)
                    {
                        break;
                    }
                }
                //resState3 = 0;
                this[wx, wy - 1, wz] = WATER;
                //if (state1 == 0)
                //{
                //SetState(wx, wy - 1, wz, 1, 3);
                return AIR;
            }
            return WATER_NOFLOW;

        }
        if (block == WATER)
        {
            / *
            if (state3 > 0)
            {
                needsAnotherUpdate = true;
                resState3 = state3 - 1;
            }
            else if (state3 == 0)
            {
                resState3 = state3 - 1;
                needsAnotherUpdate = false;
            }
            else
            {
                resState3 = 0;
                needsAnotherUpdate = true;
            }
            * /
            if (this[wx, wy - 1, wz] == AIR)
            {
                //resState3 = 0;
                this[wx, wy - 1, wz] = WATER;
                //SetState(wx, wy - 1, wz, 1, 3);
                return AIR;
            }
            else if (this[wx, wy - 1, wz] == WATER || this[wx, wy-1, wz] == WATER_NOFLOW)
            {
                //if (state1 == 0)
                //{
                
            }
            needsAnotherUpdate = false;
            Debug.Log("water update " + wx + " " + wy + " " + wz + " " + resState3);
            return WATER_NOFLOW;
            
            return WATER;
        }

            /*


           // }
            resState1 = 0;
            if (this[wx, wy-1, wz] == AIR)
            {
                this[wx, wy - 1, wz] = PUSHED_WATER;
                SetState(wx, wy - 1, wz, 0, 1);
                resState3 = DOWN;
                //AddBlockUpdateToNeighbors(wx, wy, wz);
                return AIR;
            }

            if(this[wx, wy-1, wz] == WATER)
            {
                this[wx, wy - 1, wz] = PUSHED_WATER;
                SetState(wx, wy - 1, wz, 0, 1);
                return WATER;
            }

            if (block == PUSHED_WATER)
            {

                LVector3 neighbor;
                
                int nNeighbors = RandomlyChooseNeighborMeetingCondition(new LVector3(wx, wy, wz), val => val.Block == AIR, out neighbor, false);
                if (nNeighbors > 0)
                {
                    this[neighbor] = WATER;
                    return AIR;
                }

                
                foreach (LVector3 waterNeighbor in SidewaysNeighborsRelative(new LVector3(wx, wy, wz)))
                {
                    if (waterNeighbor.Block == WATER)
                    {
                        this[waterNeighbor] = PUSHED_WATER;
                    }
                }
                / *
                nNeighbors = RandomlyChooseNeighborMeetingCondition(new LVector3(wx, wy, wz), val => val.Block == WATER, out neighbor, false);
                if (nNeighbors > 0)
                {
                    this[neighbor] = PUSHED_WATER;
                    return WATER;
                }
                * /
                / *
                if (!madePushed && this[wx, wy + 1, wz] == WATER)
                {
                    this[wx, wy + 1, wz] = PUSHED_WATER;
                    AddBlockUpdateToNeighbors(wx, wy + 1, wz);
                    needsAnotherUpdate = true;
                    return PUSHED_WATER;
                }
                if (this[wx, wy+1, wz] == AIR && !madePushed)
                {
                    this[wx, wy + 1, wz] = WATER;
                    SetState(wx, wy + 1, wz, 6, 1);
                    AddBlockUpdateToNeighbors(wx, wy - 1, wz);
                    AddBlockUpdateToNeighbors(wx, wy + 1, wz);
                    AddBlockUpdate(wx, wy + 1, wz);
                    needsAnotherUpdate = false;
                    return AIR;
                }
                AddBlockUpdateToNeighbors(wx, wy, wz);
                * /
                / *
                // random pr of flowing up if failed to flow sideways and pushed
                if (!madePushed && (this[wx, wy + 1, wz] == AIR || this[wx, wy + 1, wz] == WATER))
                {
                    if (Random.value < 0.1f)
                    {
                        if (this[wx, wy + 1, wz] == AIR && this[wx, wy-1, wz] == PUSHED_WATER)
                        {
                            int depth = 0;
                            while(this[wx, wy-depth, wz] == PUSHED_WATER)
                            {
                                depth += 1;
                            }
                            depth -= 1;
                            for (int i = 0; i < depth; i++)
                            {
                                this[wx, wy - i, wz] = WATER;
                            }
                            this[wx, wy + 1, wz] = WATER;
                            this[wx, wy - depth, wz] = AIR;
                            AddBlockUpdate(wx+1, wy - depth, wz);
                            AddBlockUpdate(wx-1, wy - depth, wz);
                            AddBlockUpdate(wx, wy - depth, wz+1);
                            AddBlockUpdate(wx, wy - depth, wz-1);
                            return AIR;
                        }
                        else if(this[wx, wy + 1, wz] == WATER)
                        {

                            this[wx, wy + 1, wz] = PUSHED_WATER;
                            AddBlockUpdate(wx, wy + 1, wz);
                            return PUSHED_WATER;
                        }
                    }
                    else
                    {
                        needsAnotherUpdate = true;
                    }
                }
        */

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
                    if (this[wx + 1, wy+y, wz] == DIRT && this[wx + 1, wy + y+1, wz] == AIR)
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

                    if (this[wx - 1, wy + y, wz] == DIRT && this[wx - 1, wy + y + 1, wz] == AIR)
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

                    if (this[wx, wy + y, wz + 1] == DIRT && this[wx, wy + y + 1, wz+1] == AIR)
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

                    if (this[wx, wy + y, wz - 1] == DIRT && this[wx, wy + y + 1, wz-1] == AIR)
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
        int numInCache = chunkCache.Count;
        for (int i = 0; i < numInCache; i++)
        {
            Chunk chunk = chunkCache[i];
            if (chunk.cx == pos[0] && chunk.cy == pos[1] && chunk.cz == pos[2])
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
            foreach (Chunk chunk in chunksPer[minDim][pos[minDim]])
            {
                if (chunk.cx == pos[0] && chunk.cy == pos[1] && chunk.cz == pos[2])
                {
                    while (chunkCache.Count >= cacheSize)
                    {
                        chunkCache.RemoveAt(0);
                    }
                    chunkCache.Add(chunk);
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

    public Chunk GetOrGenerateChunkAtPos(long x, long y, long z)
    {
        long chunkX = divWithFloor(x, chunkSize);
        long chunkY = divWithFloor(y, chunkSize);
        long chunkZ = divWithFloor(z, chunkSize);
        Chunk chunk = GetOrGenerateChunk(chunkX, chunkY, chunkZ);
        return chunk;
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
        // what direction is checked first: goes in a weird order after that
        globalPreference = (globalPreference + 1) % 4;
        List<Chunk> allChunksHere = new List<Chunk>(allChunks);

        foreach (Chunk chunk in allChunksHere)
        {
            chunk.TickStart();
        }

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


    public int numChunksTotal;
    // Update is called once per frame
    void Update () {

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
            world.Tick();
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



    public int MakeChunkTris(Chunk chunk)
    {
        chunk.chunkRenderer.drawData.SetCounterValue(0);
        chunkBlockData.SetData(chunk.chunkData.GetRawData());
        cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawData);
        cullBlocksShader.Dispatch(0, chunkSize/8, chunkSize / 8, chunkSize / 8);
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
