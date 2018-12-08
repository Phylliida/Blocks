using Example_pack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;




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
    bool using1;
    public int numRendereredCubesTransparent;
    public int numRendereredCubesNotTransparent;

    public ChunkRenderer(Chunk chunk, int chunkSize)
    {
        this.chunk = chunk;
        this.chunkSize = chunkSize;

        drawDataTransparent = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
        drawDataNotTransparent = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
    }

    public void Tick()
    {

    }
    public void Render(bool onlyTransparent, Chunk chunk)
    {
        if (chunk.chunkData.needToBeUpdated)
        {
            chunk.chunkData.needToBeUpdated = false;
            chunk.world.blocksWorld.MakeChunkTris(chunk, out numRendereredCubesNotTransparent, out numRendereredCubesTransparent);
        }
        chunk.world.blocksWorld.RenderChunk(chunk, onlyTransparent);
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
        }
    }
}

public class ChunkBiomeData
{
    public float altitude;
    public long cx, cy, cz;

    public float[] chunkProperties;


    public ChunkBiomeData(ChunkProperties chunkProperties, long cx, long cy, long cz)
    {
        this.chunkProperties = chunkProperties.GenerateChunkPropertiesArr(cx, cy, cz);
        this.cx = cx;
        this.cy = cy;
        this.cz = cz;
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

    public int state1 { get { if (world.blockModifyState != curBlockModifyState1) { cachedState1 = world.GetState(wx, wy, wz, cx, cy, cz, 1); curBlockModifyState1 = world.blockModifyState; } return cachedState1; } set { if (value != state1) { wasModified = true; cachedState1 = value; world.SetState(wx, wy, wz, cx, cy, cz, value, 1); curBlockModifyState1 = world.blockModifyState; CheckLocalStates(); } } }
    public int state2 { get { if (world.blockModifyState != curBlockModifyState2) { cachedState2 = world.GetState(wx, wy, wz, cx, cy, cz, 2); curBlockModifyState2 = world.blockModifyState; } return cachedState2; } set { if (value != state2) { wasModified = true; cachedState2 = value; world.SetState(wx, wy, wz, cx, cy, cz, value, 2); curBlockModifyState2 = world.blockModifyState; CheckLocalStates(); } } }
    public int state3 { get { if (world.blockModifyState != curBlockModifyState3) { cachedState3 = world.GetState(wx, wy, wz, cx, cy, cz, 3); curBlockModifyState3 = world.blockModifyState; } return cachedState3; } set { if (value != state3) { wasModified = true; cachedState3 = value; world.SetState(wx, wy, wz, cx, cy, cz, value, 3); curBlockModifyState3 = world.blockModifyState; CheckLocalStates(); } } }
    public BlockValue block { get { if (world.blockModifyState != curBlockModifyStateBlock) { cachedBlock = (BlockValue)world[wx, wy, wz, cx, cy, cz]; curBlockModifyStateBlock = world.blockModifyState; } return cachedBlock; } set { if (value != block) { wasModified = true; cachedBlock = value; world[wx, wy, wz, cx, cy, cz] = (int)value; curBlockModifyStateBlock = world.blockModifyState; CheckLocalStates(); } } }
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
        needsAnotherTick = false;
    }

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
        World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);

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
        return (BlockValue)blockGetter[x, y, z];
    }

    public void SetBlock(long x, long y, long z, BlockValue value)
    {
        //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
        blockGetter[x, y, z] = (int)value;
    }

    public int GetState(long x, long y, long z, int stateI)
    {
        //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
        return blockGetter.GetState(x, y, z, stateI);
    }

    public void SetState(long x, long y, long z, int state, int stateI)
    {
        //PhysicsUtils.ModPos(x, y, z, out x, out y, out z);
        blockGetter.SetState(x, y, z, state, stateI);
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


    public IEnumerable<BlockData> GetNeighbors(BlockData block, bool includingUp=true, bool includingDown=true)
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
        else if(globalPreference == 1)
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


public class ChunkProperty
{
    public int index;
    public string name;
    public float minVal;
    public float maxVal;
    public bool usesY;
    public float scale;
    public ChunkProperty(string name, float minVal, float maxVal, float scale=10.0f, bool usesY=true)
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
            return Simplex.Noise.Generate(cx / scale, cy / scale, cz / scale) *(maxVal-minVal) + minVal;
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
    public int AddChunkProperty(ChunkProperty chunkProperty)
    {
        chunkProperties.Add(chunkProperty);
        chunkProperty.index = chunkProperties.Count - 1;
        return chunkProperties.Count - 1;
    }

    public float[] GenerateChunkPropertiesArr(long cx, long cy, long cz)
    {
        // if we have more than 1000 properties for each chunk this needs to be increased but that is probably excessive so we should probably be ok? BUT HERE IS SOMEWHERE TO CHECK IF A BUG OCCURS, FYI
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
    
    public long GetPos(int d)
    {
        if (d == 0)
        {
            return cx;
        }
        else if(d == 1)
        {
            return cy;
        }
        else if(d == 2)
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
    
    public void CreateStuff()
    {
        this.chunkRenderer = new ChunkRenderer(this, chunkSize);
        chunkData = new ChunkData(chunkSize);
        chunkData.attachedChunks.Add(this);
    }


    public Chunk(World world, ChunkProperties chunkProperties, long chunkX, long chunkY, long chunkZ, int chunkSize, bool createStuff=true)
    {
        this.world = world;
        this.chunkSize = chunkSize;
        this.cx = chunkX;
        this.cy = chunkY;
        this.cz = chunkZ;
        this.chunkBiomeData = new ChunkBiomeData(chunkProperties, cx, cy, cz);
        posChunks = new Chunk[] { null, null, null };
        negChunks = new Chunk[] { null, null, null };

        long baseX = chunkX * chunkSize;
        long baseY = chunkY * chunkSize;
        long baseZ = chunkZ * chunkSize;

        generating = true;

        if (createStuff)
        {
            CreateStuff();
        }

    }

    public void Generate()
    {
        generating = true;
        long baseX = cx * chunkSize;
        long baseY = cy * chunkSize;
        long baseZ = cz * chunkSize;
        Structure myStructure = new Structure(cx + " " + cy + " " + cz, true, this);
        long start = PhysicsUtils.millis();
        //Debug.Log("generating chunk " + cx + " " + cy + " " + cz + " ");
        try
        {
            world.worldGeneration.blockGetter = myStructure;
            for (long x = baseX; x < baseX + this.chunkSize; x++)
            {
                for (long z = baseZ; z < baseZ + this.chunkSize; z++)
                {
                    //float elevation = world.AverageChunkValues(x, 0, z, c => c.chunkProperties["elevation"]);
                    for (long y = baseY; y < baseY + this.chunkSize; y++)
                    {
                        //long elevation = (long)Mathf.Round(world.AverageChunkValues(x, 0, z, "altitude"));
                        using (BlockData block = myStructure.GetBlockData(x, y, z))
                        {
                            world.worldGeneration.OnGenerateBlock(x, y, z, block);
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
        long end = PhysicsUtils.millis();
        float secondsTaken = (end - start) / 1000.0f;
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

    public void TickStart(long chunkId)
    {
        if (valid && this.chunkData.TickStart(chunkId)) // this.chunkData.TickStart(chunkId) only returns true if it is the first TickStart called this frame. That ensures that we don't swap around valid multiple times during a single tick start
        {
            int maxNews = 10;
            Chunk newValidOne = chunkData.attachedChunks[Random.Range(0, Mathf.Min(maxNews, chunkData.attachedChunks.Count))];

            // this ordering of these two statements is important because newValidOne could be us
            this.valid = false;
            newValidOne.valid = true; 

        }
    }


    public bool Tick()
    {
        bool didGenerate = false;
        if (generating)
        {
            Generate();
            didGenerate = true;

            generating = false;
        }

        this.chunkRenderer.Tick();


        if (!valid)
        {
            return false;
        }

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
            bool relevantChunk = true;
            int k = 0;
            foreach (int i in chunkData.blocksNeedUpdating)
            {
                long ind = (long)(i);
                long x, y, z;
                chunkData.to3D(ind, out x, out y, out z);
                long wx = x + cx * chunkSize;
                long wy = y + cy * chunkSize;
                long wz = z + cz * chunkSize;
                long mwx, mwy, mwz;
                //PhysicsUtils.ModPos(wx, wy, wz, out mwx, out mwy, out mwz);
                using (BlockData block = world.GetBlockData(wx, wy, wz))
                {
                    BlockValue blockValue = block.block;
                    if (world.customBlocks.ContainsKey(blockValue))
                    {
                        BlockOrItem customBlock = world.customBlocks[blockValue];
                        customBlock.OnTick(block);
                        if (block.needsAnotherTick)
                        {
                            chunkData.blocksNeedUpdatingNextFrame.Add((int)ind);
                        }
                        if (block.WasModified)
                        {
                            bool neighborsInsideThisChunk =
                                (x != 0 && x != chunkSize - 1) &&
                                (y != 0 && y != chunkSize - 1) &&
                                (z != 0 && z != chunkSize - 1);

                            // don't call lots of chunk lookups if we don't need to
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
                                chunkData.AddBlockUpdate(x, y, z - 1);
                                chunkData.AddBlockUpdate(x, y, z + 1);
                            }
                            if (block.block == BlockValue.Air)
                            {
                                block.state1 = 0;
                                block.state2 = 0;
                                block.state3 = 0;
                            }
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
        }

        chunkData.blocksNeedUpdating.Clear();

        return didGenerate;
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
            chunkData.AddBlockUpdate(relativeX, relativeY, relativeZ);
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
    public HashSet<int> blocksNeedUpdating = new HashSet<int>();
    public HashSet<int> blocksNeedUpdatingNextFrame = new HashSet<int>();
    int[] data;
    int chunkSize, chunkSize_2, chunkSize_3;

    public List<Chunk> attachedChunks = new List<Chunk>();

    public ChunkData(int chunkSize, bool fillWithWildcard = false)
    {
        this.chunkSize = chunkSize;
        this.chunkSize_2 = chunkSize* chunkSize;
        this.chunkSize_3 = chunkSize* chunkSize* chunkSize;
        data = new int[chunkSize * chunkSize * chunkSize * 4];
        if (fillWithWildcard)
        {
            for (int i= 0; i < data.Length; i++)
            {
                data[i] = (int)BlockValue.Wildcard;
            }
        }
    }

    public void CopyIntoChunk(Chunk chunk)
    {
        int[] chunkData = chunk.chunkData.data;
        int totalLen = System.Math.Min(data.Length, chunkData.Length);
        for (int i = 0; i < totalLen; i++)
        {
            if (i % 4 == 0)
            {
                if (data[i] != (int)BlockValue.Wildcard)
                {
                    chunkData[i] = data[i];
                }
                else
                {
                    i += 4;
                }
            }
            else
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
            HashSet<int> tmp = blocksNeedUpdating;
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
            //blocksNeedUpdatingNextFrame.Add((int)ind);
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
            //blocksNeedUpdatingNextFrame.Add((int)ind);
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

    public void SetState(long x, long y, long z, int state, int stateI)
    {
        long cx, cy, cz;
        World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
        SetState(x, y, z, cx, cy, cz, state, stateI);
    }
    public int GetState(long x, long y, long z, int stateI)
    {
        long cx, cy, cz;
        World.mainWorld.GetChunkCoordinatesAtPos(x, y, z, out cx, out cy, out cz);
        return GetState(x, y, z, cx, cy, cz, stateI);
    }

    public abstract void SetState(long x, long y, long z, long cx, long cy, long cz, int state, int stateI);
    public abstract int GetState(long x, long y, long z, long cx, long cy, long cz, int stateI);
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

public class Structure : BlockGetter
{
    public string name;
    public bool madeInGeneration;
    Dictionary<LVector3,ChunkData> ungeneratedChunkPositions;

    public Dictionary<LVector3, int> block;
    public Dictionary<LVector3, int> state1;
    public Dictionary<LVector3, int> state2;
    public Dictionary<LVector3, int> state3;

    public Chunk baseChunk;

    public Structure(string name, bool madeInGeneration, Chunk baseChunk)
    {
        this.name = name;
        this.baseChunk = baseChunk;
        this.blockDataCache = new BlockDataCache(this);
        block = new Dictionary<LVector3, int>();
        state1 = new Dictionary<LVector3, int>();
        state2 = new Dictionary<LVector3, int>();
        state3 = new Dictionary<LVector3, int>();
        ungeneratedChunkPositions = new Dictionary<LVector3, ChunkData>();
        this.madeInGeneration = madeInGeneration;
    }


    public bool HasAllChunksGenerated()
    {
        return ungeneratedChunkPositions.Count == 0;
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
            chunkData.CopyIntoChunk(chunk);
            ungeneratedChunkPositions.Remove(chunkPos);
        }
        return ungeneratedChunkPositions.Count == 0;
    }

    public override void SetState(long x, long y, long z, long cx, long cy, long cz, int state, int stateI)
    {
        blockModifyState += 1;
        if (cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
        {
            baseChunk.SetState(x, y, z, state, stateI);
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
                ungeneratedChunkPositions[chunkPos].SetState(localPosX, localPosY, localPosZ, state, stateI, out addedBlockUpdate);
            }
            else
            {
                bool wasGenerating = chunk.generating;
                chunk.generating = true;
                chunk.SetState(x, y, z, state, stateI);
                chunk.generating = wasGenerating;
            }
        }
        else
        {
            World.mainWorld.SetState(x, y, z, state, stateI);
        }
    }
    public override int GetState(long x, long y, long z, long cx, long cy, long cz, int stateI)
    {
        if (cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
        {
            return baseChunk.GetState(x, y, z, stateI);
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
                return ungeneratedChunkPositions[chunkPos].GetState(localPosX, localPosY, localPosZ, stateI);
            }
            else
            {
                return chunk.GetState(x, y, z, stateI);
            }
        }
        else
        {
            return World.mainWorld.GetState(x, y, z, stateI);
        }
    }

    public override int this[long x, long y, long z, long cx, long cy, long cz]
    {
        get
        {
            if (cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
            {
                return baseChunk[x,y,z];
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
                return World.mainWorld[x,y,z];
            }
        }
        set
        {
            blockModifyState += 1;
            if (cx == baseChunk.cx && cy == baseChunk.cy && cz == baseChunk.cz)
            {
                if ((BlockValue)value != BlockValue.Wildcard)
                {
                    baseChunk[x, y, z] = value;
                }
                return;
            }
            if (madeInGeneration)
            {
                Chunk chunk = World.mainWorld.GetChunkAtPos(x, y, z);
                if (chunk == null)
                {
                    LVector3 chunkPos;
                    World.mainWorld.GetChunkCoordinatesAtPos(x,y,z, out chunkPos);
                    int chunkSize = World.mainWorld.chunkSize;
                    if (!ungeneratedChunkPositions.ContainsKey(chunkPos))
                    {
                        ungeneratedChunkPositions[chunkPos] = new ChunkData(chunkSize, fillWithWildcard:true);
                    }
                    long localPosX = x - chunkPos.x * chunkSize;
                    long localPosY = y - chunkPos.y * chunkSize;
                    long localPosZ = z - chunkPos.z * chunkSize;
                    ungeneratedChunkPositions[chunkPos][localPosX, localPosY, localPosZ] = value;
                }
                else
                {
                    if ((BlockValue)value != BlockValue.Wildcard)
                    {
                        bool wasGenerating = chunk.generating;
                        chunk.generating = true;
                        chunk[x, y, z] = value;
                        chunk.generating = wasGenerating;
                    }
                }
            }
            else
            {
                World.mainWorld[x,y,z] = value;
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
                int j = (i +frontPos) % list.Length;
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
            T res = list[frontPos+count-1];
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
    public Dictionary<BlockValue, BlockOrItem> customBlocks = new Dictionary<BlockValue, BlockOrItem>();
    public GenerationClass customGeneration;

    public void AddCustomBlock(BlockValue block, BlockOrItem customBlock)
    {
        customBlocks[block] = customBlock;
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

public class World : BlockGetter
{
    public static World mainWorld;

    public const int numBlocks = 64;
    public const int numBreakingFrames = 10;

    
    public BlocksWorld blocksWorld;

    ChunkProperties chunkProperties;
    


    public int AddChunkProperty(ChunkProperty chunkProperty)
    {
        return chunkProperties.AddChunkProperty(chunkProperty);
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

    public List<Structure> unfinishedStructures;


    public bool DropBlockOnDestroy(BlockValue block, LVector3 pos, BlockStack thingHolding, Vector3 positionOfBlock, Vector3 posOfOpening)
    {
        //CreateBlockEntity(block, positionOfBlock);
        //return true;
        if (thingHolding == null)
        {
            thingHolding = new BlockStack(BlockValue.Air, 1);
        }
        if (customBlocks.ContainsKey(block))
        {
            BlockOrItem customBlock = customBlocks[block];
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
        BlockValue.Stick,
        BlockValue.SharpRock,
        BlockValue.LargeSharpRock
    };

    public bool AllowedtoPlaceBlock(BlockValue block)
    {
        foreach (BlockValue item in items)
        {
            if (item == block)
            {
                return false;
            }
        }
        return true;
    }

    public BlockEntity CreateBlockEntity(BlockValue block, Vector3 position)
    {
        GameObject blockEntity = GameObject.Instantiate(blocksWorld.blockEntityPrefab);
        blockEntity.transform.position = position;
        blockEntity.GetComponent<BlockEntity>().blockId = (int)block;
        return blockEntity.GetComponent<BlockEntity>();
    }

    public Dictionary<BlockValue, BlockOrItem> customBlocks;
    public GenerationClass worldGeneration;

    public World(BlocksWorld blocksWorld, int chunkSize, BlocksPack blocksPack)
    {
        this.worldGeneration = blocksPack.customGeneration;
        this.customBlocks = blocksPack.customBlocks;
        foreach (KeyValuePair<BlockValue, BlockOrItem> customBlock in customBlocks)
        {
            customBlock.Value.blockGetter = this;
            customBlock.Value.world = this;
        }
        blockDataCache = new BlockDataCache(this);
        chunkProperties = new ChunkProperties();
        World.mainWorld = this;
        this.blocksWorld = blocksWorld;
        this.chunkSize = chunkSize;


        stackableSize = new Dictionary<int, int>();
        stackableSize[(int)BlockValue.Dirt] = 16;
        stackableSize[(int)BlockValue.Stone] = 45;
        stackableSize[(int)BlockValue.Grass] = 64;
        stackableSize[(int)BlockValue.Sand] = 32;
        stackableSize[(int)BlockValue.Bedrock] = 16;
        stackableSize[(int)BlockValue.Clay] = 64;
        stackableSize[(int)BlockValue.Leaf] = 64;
        stackableSize[(int)BlockValue.Stick] = 64;
        stackableSize[(int)BlockValue.Rock] = 64;
        stackableSize[(int)BlockValue.LargeRock] = 64;
        stackableSize[(int)BlockValue.SharpRock] = 64;
        stackableSize[(int)BlockValue.LargeSharpRock] = 64;
        stackableSize[(int)BlockValue.Bark] = 64;
        stackableSize[(int)BlockValue.String] = 64;


        chunksPerX = new Dictionary<long, List<Chunk>>();
        chunksPerY = new Dictionary<long, List<Chunk>>();
        chunksPerZ = new Dictionary<long, List<Chunk>>();
        chunksPer = new Dictionary<long, List<Chunk>>[] { chunksPerX, chunksPerY, chunksPerZ };

        argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        allChunks = new List<Chunk>();

        chunkCache = new List<Chunk>(cacheSize);

        unfinishedStructures = new List<Structure>();

        maxCapacities = new Dictionary<int, int>();
        maxCapacities[(int)BlockValue.Dirt] = 3;
        maxCapacities[(int)BlockValue.Stone] = 5;
        maxCapacities[(int)BlockValue.Grass] = 4;
        maxCapacities[(int)BlockValue.Sand] = 0;
        maxCapacities[(int)BlockValue.Air] = 0;
        maxCapacities[(int)BlockValue.Bedrock] = 6;
        this.worldGeneration.world = this;
        this.worldGeneration.blockGetter = this;
        this.worldGeneration.OnGenerationInit();
        GenerateChunk(0, 0, 0);
        //return;
        int viewDist = 1;
        for (int i = -viewDist; i <= viewDist; i++)
        {
            for (int j = -viewDist; j <= viewDist; j++)
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


    public bool NeedsInitialUpdate(int block)
    {
        if (block == (int)BlockValue.Grass)
        {
            return true;
        }
        return false;
    }


    public override void SetState(long i, long j, long k, long cx, long cy, long cz, int state, int stateI)
    {
        blockModifyState += 1;
        Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
        chunk.SetState(i, j, k, state, stateI);
    }
    public override int GetState(long i, long j, long k, long cx, long cy, long cz, int stateI)
    {
        Chunk chunk = GetOrGenerateChunk(cx, cy, cz);
        return chunk.GetState(i, j, k, stateI);
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
        return powerFrom; // or we just carry max power for up?
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
    
    // if a unpushed water block gets an update, flood fill back and replace all wate on top of water with pushed water
     

    public bool IsWater(int block)
    {
        return block == (int)BlockValue.Water || block == (int)BlockValue.WaterNoFlow;
    }
    
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
        if (this[wx, wy+1, wz] == (int)BlockValue.Air &&
            this[wx+1, wy, wz] != (int)BlockValue.Air &&
            this[wx-1, wy, wz] != (int)BlockValue.Air &&
            this[wx, wy, wz+1] != (int)BlockValue.Air &&
            this[wx, wy, wz-1] != (int)BlockValue.Air)
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }



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
    public int UpdateBlock(long wx, long wy, long wz, int block, int state1, int state2, int state3, out int resState1, out int resState2, out int resState3, out bool needsAnotherUpdate)
    {
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
    // here we assume b is never less than 0

    long divWithFloorForChunkSize(long a)
    {
        return a / chunkSize - ((a < 0 && a % chunkSize != 0) ? 1 : 0);
    }

    long divWithFloor(long a, long b)
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

    public Chunk GetChunk(long chunkX, long chunkY, long chunkZ)
    {
        if (lastChunk != null && lastChunk.cx == chunkX && lastChunk.cy == chunkY && lastChunk.cz == chunkZ)
        {
            return lastChunk;
        }
        Chunk res = GetChunk(new long[] { chunkX, chunkY, chunkZ });
        if (res != null)
        {
            lastChunk = res;
        }
        return res;
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
        Chunk res = new Chunk(this, chunkProperties, chunkX, chunkY, chunkZ, chunkSize, createStuff:false);
        AddChunkToDataStructures(res);
        return res;
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

    int off = 0;
    void AddChunkToDataStructures(Chunk chunk)
    {
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


        chunk.CreateStuff();
        return;

        int loopMax = 2;

        Chunk otherLoop = chunk;

        long otherLoopCx, otherLoopCy, otherLoopCz;
        PhysicsUtils.ModChunkPos(chunk.cx, chunk.cy, chunk.cz, out otherLoopCx, out otherLoopCy, out otherLoopCz);

        if (otherLoopCx != chunk.cx || otherLoopCy != chunk.cy ||  otherLoopCz != chunk.cz)
        {
            otherLoop = GetOrGenerateChunk(otherLoopCx, otherLoopCy, otherLoopCz);
        }
        
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


    public void AddBlockUpdate(long i, long j, long k, bool alsoToNeighbors=true)
    {
        GetOrGenerateChunk(divWithFloorForChunkSize(i), divWithFloorForChunkSize(j), divWithFloorForChunkSize(k)).AddBlockUpdate(i, j, k);
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
        // render non-transparent
        foreach (Chunk chunk in allChunks)
        {
            chunk.chunkRenderer.Render(false, chunk);
        }
        // render transparent
        foreach (Chunk chunk in allChunks)
        {
            chunk.chunkRenderer.Render(true, chunk);
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
    public void Tick()
    {
        frameId += 1;
        foreach (KeyValuePair<BlockValue, BlockOrItem> block in customBlocks)
        {
            block.Value.OnTickStart();
        }
        // what direction is checked first: goes in a weird order after that
        globalPreference = (globalPreference + 1) % 4;
        List<Chunk> allChunksHere = new List<Chunk>(allChunks);

        // Randomly shuffle the order we update the chunks in, for the memes when they are tiled procedurally
        //Shuffle(allChunksHere);


        numBlockUpdatesThisTick = 0;
        numWaterUpdatesThisTick = 0;


        foreach (Chunk chunk in allChunksHere)
        {
            chunk.TickStart(frameId);
        }

        foreach (Chunk chunk in allChunksHere)
        {
            if(chunk.Tick())
            {
                List<Structure> leftoverStructures = new List<Structure>();
                foreach (Structure structure in unfinishedStructures)
                {
                    if (!structure.AddNewChunk(chunk))
                    {
                        leftoverStructures.Add(structure);
                    }
                }

                unfinishedStructures = leftoverStructures;
                break;
            }
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
            byte[] imageData = File.ReadAllBytes(block.blockImagePath);
            Texture2D blockTexture = new Texture2D(2, 2);
            blockTexture.LoadImage(imageData); // (will automatically resize as needed)
            blockTexture.Apply();
            Texture2D argbTexture = new Texture2D(blockTexture.width, blockTexture.height, TextureFormat.ARGB32, false, true);
            argbTexture.SetPixels(blockTexture.GetPixels());
            argbTexture.Apply();
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
            i += 1;
        }
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

        packTexture.alphaIsTransparency = true;
        string assetsPathDir = assetsPathInfo.FullName;

        File.WriteAllBytes(assetsPathDir + "/" + packName + ".png", packTexture.EncodeToPNG());

        // delete meta file so unity will reload it
        if (File.Exists(assetsPathDir + "/" + packName + ".png.meta"))
        {
            File.Delete(assetsPathDir + "/" + packName + ".png.meta");
        }


        string exampleThings =
"namespace " + packName + @"_pack {
    public enum BlockValue
    {
        Air = 0,
        Wildcard = -1,
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


            exampleThings += "        " + packBlocks[b].blockName + "=" + index;
            if (b != packBlocks.Count - 1)
            {
                exampleThings += ",\n";
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

        exampleThings += afterExampleThings + testFunc + testFunc2  + "\r\n}";

        File.WriteAllText(assetsPathDir + "/" + packName + "Pack" + " .cs", exampleThings);

        // delete meta file so unity will reload it
        if (File.Exists(assetsPathDir + "/" + packName + "Pack" + " .cs.meta"))
        {
            File.Delete(assetsPathDir + "/" + packName + "Pack" + " .cs.meta");
        }
    }
}


public class PackBlock
{
    public string blockName;
    public string blockRootDir;
    public string blockImagePath;
    public bool isValid;
    public bool isTransparent;
    public PackBlock(string blockDirectory)
    {
        isValid = false;
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
                Debug.LogError("could not find any images for block " + blockName + " at path " + blockRootDir + " so we are ignoring this block");
            }
        }
        else
        {
            Debug.LogError("pack block directory " + blockDirectory + " does not exist");
        }
    }
}

public class BlocksWorld : MonoBehaviour {

    public ComputeShader cullBlocksShader;
    ComputeBuffer cubeNormals;
    ComputeBuffer cubeOffsets;
    ComputeBuffer uvOffsets;
    ComputeBuffer breakingUVOffsets;
    ComputeBuffer linesOffsets;
    Material outlineMaterial;
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

    public const int chunkSize = 32;

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
    void SetupRendering()
    {
        LoadMaterials();

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
            if (i >= 0 && i <= 5)
            {
                texX = offset.x + 1.0f;
                texY = offset.z + 2.0f;
                normal = new Vector3(0, -1, 0);
            }
            // top
            else if (i >= 6 && i <= 11)
            {
                texX = offset.x;
                texY = offset.z + 2.0f;
                normal = new Vector3(0, 1, 0);
            }
            // left
            else if (i >= 12 && i <= 17)
            {
                texX = offset.x;
                texY = offset.y + 1.0f;
                normal = new Vector3(0, 0, -1);
            }
            // right
            else if (i >= 18 && i <= 23)
            {
                texX = offset.x + 1.0f;
                texY = offset.y + 1.0f;
                normal = new Vector3(0, 0, 1);
            }
            // forward
            else if (i >= 24 && i <= 29)
            {
                texX = offset.z;
                texY = offset.y;
                normal = new Vector3(-1, 0, 0);
            }
            // back
            else if (i >= 30 && i <= 35)
            {
                texX = offset.z + 1.0f;
                texY = offset.y;
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
            vertNormals[i * 4+1] = normal.y;
            vertNormals[i * 4+2] = normal.z;
            vertNormals[i * 4+3] = 0.0f;

            texOffsetsGood.Add(new Vector2(texOffset.x / 2.0f, texOffset.y / 3.0f));
            vertOffsetsGood.Add(offset - new Vector3(0.5f, 0.5f, 0.5f));
        }

        blockBreakingBuffer = new ComputeBuffer(1, sizeof(int) * 4, ComputeBufferType.GPUMemory);
        cubeOffsets = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        cubeOffsets.SetData(triOffsets);
        cubeNormals = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        cubeNormals.SetData(vertNormals);
        uvOffsets = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
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
            if (i >= 0 && i <= 5)
            {
                texX = offset.x;
                texY = offset.z;
            }
            // top
            else if (i >= 6 && i <= 11)
            {
                texX = offset.x;
                texY = offset.z;
            }
            // left
            else if (i >= 12 && i <= 17)
            {
                texX = offset.x;
                texY = offset.y;
            }
            // right
            else if (i >= 18 && i <= 23)
            {
                texX = offset.x;
                texY = offset.y;
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
                texX = offset.z;
                texY = offset.y;
            }
            texOffset = new Vector2(texX, texY);

            texOffsetsSingleBlock[i * 4] = texOffset.x;
            texOffsetsSingleBlock[i * 4 + 1] = texOffset.y / (float)World.numBreakingFrames;
            texOffsetsSingleBlock[i * 4 + 2] = 0;
            texOffsetsSingleBlock[i * 4 + 3] = 0;
        }

        breakingUVOffsets = new ComputeBuffer(36, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        breakingUVOffsets.SetData(texOffsetsSingleBlock);

        blockMesh = new Mesh();
        List<Vector3> blockVertices = new List<Vector3>();
        List<Vector2> blockUvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < triOffsets.Length/4; i++)
        {
            Vector3 vertex = new Vector3(triOffsets[i * 4], triOffsets[i * 4 + 1], triOffsets[i * 4 + 2]) - new Vector3(0.5f, 0.5f, 0.5f);
            Vector2 uv = new Vector2(texOffsets[i * 4], texOffsets[i * 4 + 1]);
            blockVertices.Add(vertex);
            blockUvs.Add(uv);
            triangles.Add(i);
        }
        int[] actualTriangles = new int[triangles.Count];
        for (int i = 0; i < triangles.Count/3; i++)
        {
            actualTriangles[i * 3] = triangles[i * 3 + 2];
            actualTriangles[i * 3+1] = triangles[i * 3 + 1];
            actualTriangles[i * 3+2] = triangles[i * 3];
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



        linesOffsets = new ComputeBuffer(24, sizeof(float) * 4, ComputeBufferType.GPUMemory);
        linesOffsets.SetData(lineOffsets);

        outlineMaterial.SetBuffer("lineOffsets", linesOffsets);


        drawPositions1 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);
        drawPositions2 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.Append);

        chunkBlockData = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(int) * 4, ComputeBufferType.GPUMemory);
        cullBlocksShader.SetBuffer(0, "DataIn", chunkBlockData);
        cullBlocksShader.SetBuffer(1, "DataIn", chunkBlockData);
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
    void Start () {
        
        SetupRendering();

        //Dictionary<BlockValue, Block> customBlocks = new Dictionary<BlockValue, Block>();
        //customBlocks[BlockValue.GRASS] = new Grass();
        //GenerationClass customGeneration = new ExampleGeneration();
        world = new World(this, chunkSize, blocksPack);
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



    public void MakeChunkTris(Chunk chunk, out int numNotTransparent, out int numTransparent)
    {
        int[] args = new int[] { 0 };


        chunk.chunkRenderer.drawDataNotTransparent.SetCounterValue(0);
        chunkBlockData.SetData(chunk.chunkData.GetRawData());
        // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
        cullBlocksShader.SetBuffer(0, "DrawingThings", chunk.chunkRenderer.drawDataNotTransparent);
        cullBlocksShader.Dispatch(0, chunkSize/8, chunkSize / 8, chunkSize / 8);

        ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataNotTransparent, world.argBuffer, 0);
        world.argBuffer.GetData(args);
        numNotTransparent = args[0];


        chunk.chunkRenderer.drawDataTransparent.SetCounterValue(0);
        chunkBlockData.SetData(chunk.chunkData.GetRawData());
        // 0 keeps non-transparent blocks, 1 keeps only transparent blocks
        cullBlocksShader.SetBuffer(1, "DrawingThings", chunk.chunkRenderer.drawDataTransparent);
        cullBlocksShader.Dispatch(1, chunkSize / 8, chunkSize / 8, chunkSize / 8);
        ComputeBuffer.CopyCount(chunk.chunkRenderer.drawDataTransparent, world.argBuffer, 0);
        world.argBuffer.GetData(args);
        numTransparent = args[0];
    }

    public Queue<Tuple<LVector3, float>> blocksBreaking = new Queue<Tuple<LVector3, float>>();

    public float TimeNeededToBreak(LVector3 blockPos, BlockValue block, BlockStack itemHittingWith)
    {
        if (itemHittingWith == null)
        {
            itemHittingWith = new BlockStack(BlockValue.Air, 1);
        }
        if (world.customBlocks.ContainsKey(block))
        {
            BlockOrItem customBlock = world.customBlocks[block];
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
        float timeNeeded = TimeNeededToBreak(new LVector3(x,y,z), blockHitting, itemHittingWith);
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
        blockBreakingBuffer.SetData(new int[] { (int)(x - cx*chunkSize), (int)(y - cy * chunkSize), (int)(z - cz * chunkSize), brokenState });
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
                triMaterialWithTransparency.SetBuffer("DrawingThings", chunk.chunkRenderer.drawDataTransparent);
                triMaterialWithTransparency.SetMatrix("localToWorld", renderTransform.localToWorldMatrix);
                triMaterialWithTransparency.SetInt("ptCloudWidth", chunkSize);
                //triMaterial.SetBuffer("PixelData", sandPixelData);
                triMaterialWithTransparency.SetFloat("ptCloudScale", worldScale);
                triMaterialWithTransparency.SetVector("ptCloudOffset", new Vector4(0, 0, 0, 0));
                triMaterialWithTransparency.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesTransparent * (36));
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
                Graphics.DrawProcedural(MeshTopology.Triangles, chunk.chunkRenderer.numRendereredCubesNotTransparent * (36));
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
            ActuallyRenderBlockBreaking(blockBreaking.a.x, blockBreaking.a.y, blockBreaking.a.z, 1.0f-blockBreaking.b);
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
