using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(BlockValue.Sand, positionOfBlock);
        destroyBlock = true;
    }

    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        using (BlockData below = GetBlockData(block.x, block.y - 1, block.z))
        {
            if (below.block == BlockValue.Water || below.block == BlockValue.WaterNoFlow)
            {
                below.block = BlockValue.Water;
                block.state1 = 1 - block.state1;
                below.state1 = block.state1;
            }
            else if (below.block == BlockValue.Air)
            {
                below.block = BlockValue.Water;
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}



public class Water : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }

    bool IsWater(long x, long y, long z)
    {
        BlockValue block = GetBlock(x, y, z);
        return block == BlockValue.Water || block == BlockValue.WaterNoFlow;
    }

    public int GetNumAirNeighbors(long wx, long wy, long wz)
    {
        return
            (GetBlock(wx + 1, wy, wz) == (int)BlockValue.Air ? 1 : 0) +
            (GetBlock(wx - 1, wy, wz) == (int)BlockValue.Air ? 1 : 0) +
            (GetBlock(wx, wy + 1, wz) == (int)BlockValue.Air ? 1 : 0) +
            (GetBlock(wx, wy - 1, wz) == (int)BlockValue.Air ? 1 : 0) +
            (GetBlock(wx, wy, wz + 1) == (int)BlockValue.Air ? 1 : 0) +
            (GetBlock(wx, wy, wz - 1) == (int)BlockValue.Air ? 1 : 0);

    }

    public override void OnTick(BlockData block)
    {


        // water: state 2 = time I got here


        block.needsAnotherTick = false;



        // if we are WATER without water above and with water below, pathfind to look for open space
        //if (block == (int)BlockValue.Water && IsWater(this[wx, wy - 1, wz]) && !IsWater(this[wx, wy + 1, wz]))
        if (block.block == BlockValue.Water && IsWater(block.x, block.y-1, block.z) && !IsWater(block.x, block.y+1, block.z))
        {
            // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
            //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            //numWaterUpdatesThisTick += 1;
            if (PhysicsUtils.SearchOutwards(new LVector3(block.x, block.y, block.z), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            {
                return by < block.y && (b == (int)BlockValue.Water || b == (int)BlockValue.WaterNoFlow);
            },
                isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                {
                    if (b == (int)BlockValue.Air && by < block.y)
                    {
                        SetBlock(bx, by, bz, BlockValue.Water);
                        SetState(bx, by, bz, GetNumAirNeighbors(bx, by, bz), 3);
                        return true;
                    }
                    return false;
                }
            ))
            {
                block.state3 = 0;
                block.block = BlockValue.Air;
                return;
            }
            else
            {
                block.needsAnotherTick = true;
                block.block = BlockValue.WaterNoFlow;
                return;
            }
        }
        else
        {

            // if air below, set below = water and us = air
            if (GetBlock(block.x, block.y-1, block.z) == BlockValue.Air)
            {
                SetBlock(block.x, block.y-1, block.z, BlockValue.Water);
                block.state3 = 0;
                SetState(block.x, block.y - 1, block.z, GetNumAirNeighbors(block.x, block.y - 1, block.z), 3); // +1 because we are now air instead of water
                block.block = BlockValue.Air;
                return;
            }
            else
            {
                // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                foreach (BlockData neighbor in GetNeighbors(block, includingUp: false, includingDown: false))
                {
                    LVector3 pos2 = 2 * (neighbor.pos - block.pos) + block.pos;
                    if (neighbor.block == BlockValue.Air)
                    {
                        if (GetBlock(neighbor.x, neighbor.y-1, neighbor.z) == BlockValue.Air)
                        {
                            SetBlock(neighbor.x, neighbor.y - 1, neighbor.z, BlockValue.Water);
                            SetState(neighbor.x, neighbor.y - 1, neighbor.z, GetNumAirNeighbors(neighbor.x, neighbor.y-1, neighbor.z), 3);
                            block.state3 = 0;
                            block.block = BlockValue.Air;
                            return;
                        }
                        else if (pos2.BlockV == BlockValue.Air && GetBlock(pos2.x, pos2.y - 1, pos2.z) == BlockValue.Air)
                        {
                            SetBlock(pos2.x, pos2.y - 1, pos2.z, BlockValue.Water);
                            SetState(pos2.x, pos2.y - 1, pos2.z, GetNumAirNeighbors(pos2.x, pos2.y - 1, pos2.z), 3);
                            block.state3 = 0;
                            block.block = BlockValue.Air;
                            return;
                        }
                    }
                }
            }
            int prevNumAirNeighbors = block.state3;
            int curNumAirNeighbors = GetNumAirNeighbors(block.x, block.y, block.z);
            // if we have a more air neighbors, flood fill back and set valid blocks that have WATER_NOFLOW back to WATER so they can try again
            if (curNumAirNeighbors != prevNumAirNeighbors)
            {
                block.state3 = curNumAirNeighbors;
                if (curNumAirNeighbors > 0)
                {
                    LVector3 airNeighbor = new LVector3(block.x, block.y, block.z);
                    foreach (BlockData neighbor in GetNeighbors(block))
                    {
                        if (neighbor.block == BlockValue.Air)
                        {
                            airNeighbor = neighbor.pos;
                            break;
                        }
                    }
                    //numBlockUpdatesThisTick += 1;

                    // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
                    //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    if (PhysicsUtils.SearchOutwards(block.pos, maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    {
                        return (b == (int)BlockValue.Water || b == (int)BlockValue.WaterNoFlow);
                    },
                        isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                        {
                            if (b == (int)BlockValue.WaterNoFlow && IsWater(bx, by - 1, bz) && airNeighbor.y < by)
                            {
                                SetBlock(bx, by, bz, (int)BlockValue.Air);
                                SetState(bx, by, bz, 0, 3);
                                return true;
                            }
                            return false;
                        }
                    ))
                    {

                        SetBlock(airNeighbor.x, airNeighbor.y, airNeighbor.z, BlockValue.Water);
                        SetState(airNeighbor.x, airNeighbor.y, airNeighbor.z, GetNumAirNeighbors(airNeighbor.x, airNeighbor.y, airNeighbor.z), 3);
                        block.state3 = curNumAirNeighbors - 1; // we just replaced an air neighbor with water
                        block.needsAnotherTick = true;
                        return;
                    }
                    else
                    {
                        block.needsAnotherTick = true;
                        block.block = BlockValue.WaterNoFlow;
                        return;
                    }
                }
            }
        }

    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 10000.0f;
    }
}

public class Grass : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(BlockValue.Dirt, positionOfBlock);
        destroyBlock = true;
    }

    public override void OnTick(BlockData block)
    {
        long x = block.x;
        long y = block.y;
        long z = block.z;
        long state1 = block.state1;
        long state2 = block.state2;
        long state3 = block.state3;

        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            // if neighbor is dirt and it has air above it, try growing into it
            if (neighbor.block == BlockValue.Dirt && GetBlock(neighbor.x, neighbor.y + 1, neighbor.z) == BlockValue.Air)
            {
                if (rand() < 0.01f)
                {
                    // grow
                    neighbor.block = BlockValue.Grass;
                }
                else
                {
                    // we failed to grow but still need to, try again next tick
                    block.needsAnotherTick = true;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == BlockValue.Shovel)
        {
            return 1.0f;
        }
        else
        {
            return 2.0f;
        }
    }
}


public class Leaf : StaticBlock
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(BlockValue.Stick, positionOfBlock);
        if (Random.value < 0.2f)
        {
            CreateBlockEntity(BlockValue.Stick, positionOfBlock);
        }
        destroyBlock = true;
    }
    

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.7f;
    }
}


public class Rock : StaticBlock
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (thingBreakingWith.Block == BlockValue.Rock || thingBreakingWith.Block == BlockValue.SharpRock ||
            thingBreakingWith.Block == BlockValue.LargeRock || thingBreakingWith.Block == BlockValue.LargeSharpRock ||
            thingBreakingWith.Block == BlockValue.Pickaxe)
        {
            CreateBlockEntity(BlockValue.SharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(BlockValue.Rock, positionOfBlock);
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == BlockValue.Rock || thingBreakingWith.Block == BlockValue.SharpRock)
        {
            return 3.0f;
        }
        else if(thingBreakingWith.Block == BlockValue.LargeRock || thingBreakingWith.Block == BlockValue.LargeSharpRock)
        {
            return 2.0f;
        }
        else if(thingBreakingWith.Block == BlockValue.Pickaxe)
        {
            return 1.5f;
        }
        else
        {
            return 0.1f;
        }
    }
}

public class LargeRock : StaticBlock
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (thingBreakingWith.Block == BlockValue.Rock || thingBreakingWith.Block == BlockValue.SharpRock ||
            thingBreakingWith.Block == BlockValue.LargeRock || thingBreakingWith.Block == BlockValue.LargeSharpRock ||
            thingBreakingWith.Block == BlockValue.Pickaxe)
        {
            CreateBlockEntity(BlockValue.LargeSharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(BlockValue.LargeRock, positionOfBlock);
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == BlockValue.Rock || thingBreakingWith.Block == BlockValue.SharpRock)
        {
            return 5.0f;
        }
        else if (thingBreakingWith.Block == BlockValue.LargeRock || thingBreakingWith.Block == BlockValue.LargeSharpRock)
        {
            return 4.0f;
        }
        else if (thingBreakingWith.Block == BlockValue.Pickaxe)
        {
            return 2.0f;
        }
        else
        {
            return 0.1f;
        }
    }
}


public class WetBark : StaticBlock
{


    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        if (Random.value < 0.8)
        {
            CreateBlockEntity(BlockValue.String, positionOfBlock + Random.insideUnitSphere*0.1f);
        }
        CreateBlockEntity(BlockValue.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
        CreateBlockEntity(BlockValue.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
    }
    
    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.2f;
    }
}

public class LooseRocks : StaticBlock
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        if (block.state1 > 0)
        {
            block.state1 -= 1;
            CreateBlockEntity(BlockValue.Rock, posOfOpening);
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(BlockValue.LargeRock, positionOfBlock);
            destroyBlock = true;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == BlockValue.Pickaxe)
        {
            return 0.8f;
        }
        else
        {
            return 1.3f;
        }
    }
}


public class Trunk : StaticBlock
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        if (block.state1 > 0)
        {
            CreateBlockEntity(BlockValue.Bark, posOfOpening);
            block.state1 -= 1;
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(BlockValue.Trunk, positionOfBlock);
            destroyBlock = true;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        // breaking bark off off tree
        if (block.state1 > 0)
        {
            if (thingBreakingWith.Block == BlockValue.Axe)
            {
                return 0.5f;
            }
            else
            {
                return 1.0f;
            }
        }
        // breaking the log itself, not bark
        else
        {
            if (thingBreakingWith.Block == BlockValue.Axe)
            {
                return 3.0f;
            }
            else
            {
                return 10.0f;
            }
        }
    }
}


public class SimpleBlock : Block
{

    float baseBreakTime;
    Tuple<BlockValue, float>[] breakTimes;
    public SimpleBlock(float baseBreakTime, params Tuple<BlockValue, float>[] breakTimes)
    {
        this.baseBreakTime = baseBreakTime;
        this.breakTimes = breakTimes;
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


public class Bark : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = true;
        CreateBlockEntity(BlockValue.Bark, positionOfBlock);
    }

    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            if (neighbor.block == BlockValue.Water || neighbor.block == BlockValue.WaterNoFlow)
            {
                if (Random.value < 0.05f)
                {
                    block.block = BlockValue.WetBark;
                    block.needsAnotherTick = false;
                    break;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.0f;
    }
}

public class SimpleItem : Item
{

}

public class ExamplePack : BlocksPack {

    // Use this for initialization
    // Awake because we want this to happen before World calls its Start()
    void Awake () {
        AddCustomBlock(BlockValue.Grass, new Grass());
        AddCustomBlock(BlockValue.Bark, new Bark());
        AddCustomBlock(BlockValue.Trunk, new Trunk());
        AddCustomBlock(BlockValue.Dirt, new SimpleBlock(2.0f, new Tuple<BlockValue, float>(BlockValue.Shovel, 1.0f)));
        AddCustomBlock(BlockValue.Stone, new SimpleBlock(10.0f, new Tuple<BlockValue, float>(BlockValue.Pickaxe, 3.0f)));
        AddCustomBlock(BlockValue.Leaf, new Leaf());
        AddCustomBlock(BlockValue.LooseRocks, new LooseRocks());
        AddCustomBlock(BlockValue.Rock, new Rock());
        AddCustomBlock(BlockValue.LargeRock, new LargeRock());
        AddCustomBlock(BlockValue.LooseRocks, new LooseRocks());
        AddCustomBlock(BlockValue.Stick, new SimpleItem());
        AddCustomBlock(BlockValue.Pickaxe, new SimpleItem());
        AddCustomBlock(BlockValue.SharpRock, new SimpleItem());
        AddCustomBlock(BlockValue.LargeSharpRock, new SimpleItem());
        AddCustomBlock(BlockValue.Shovel, new SimpleItem());
        AddCustomBlock(BlockValue.Axe, new SimpleItem());
        AddCustomBlock(BlockValue.WetBark, new WetBark());
        AddCustomBlock(BlockValue.Sand, new Sand());
        AddCustomBlock(BlockValue.Water, new Water());
        AddCustomBlock(BlockValue.WaterNoFlow, new Water());
        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
