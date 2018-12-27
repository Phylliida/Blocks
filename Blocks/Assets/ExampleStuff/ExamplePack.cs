using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

public class Sand : Block
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Sand, positionOfBlock);
        destroyBlock = true;
    }

    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        using (BlockData below = GetBlockData(block.x, block.y - 1, block.z))
        {
            if (below.block == Example.Water || below.block == Example.WaterNoFlow)
            {
                below.block = Example.Water;
                block.state1 = 1 - block.state1;
                below.state1 = block.state1;
            }
            else if (below.block == Example.Air)
            {
                below.block = Example.Water;
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 0.1f;
    }
}


public class SimpleWater : Block2
{
    static int maxUpdates = 100;
    static int numUpdatesThisTick = 0;
    public override void OnTickStart()
    {
        numUpdatesThisTick = 0;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }

    bool IsWater(long x, long y, long z)
    {
        BlockValue block = GetBlock(x, y, z);
        return block == Example.Water || block == Example.WaterNoFlow;
    }


    public override void OnTick(BlockData block)
    {
        numUpdatesThisTick += 1;
        if (maxUpdates < numUpdatesThisTick)
        {
            block.needsAnotherTick = true;
            return;
        }

        // water: state 2 = time I got here


            // if air below, set below = water and us = air
        if (GetBlock(block.x, block.y - 1, block.z) == Example.Air)
        {
            SetBlock(block.x, block.y - 1, block.z, Example.Water);
            block.block = Example.Air;
            return;
        }
        else
        {
            // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
            foreach (BlockData neighbor in GetNeighbors(block, includingUp: false, includingDown: false))
            {
                if (neighbor.block == Example.Air && GetBlock(neighbor.x, neighbor.y - 1, neighbor.z) == Example.Air)
                {
                    SetBlock(neighbor.x, neighbor.y - 1, neighbor.z, Example.Water);
                    block.block = Example.Air;
                    return;
                }
            }
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        return 10000.0f;
    }
}


public class Water : Block2
{
    static int maxUpdates = 100;
    static int numUpdatesThisTick = 0;
    public override void OnTickStart()
    {
        numUpdatesThisTick = 0;
    }
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        destroyBlock = false;
    }

    bool IsWater(long x, long y, long z)
    {
        BlockValue block = GetBlock(x, y, z);
        return block == Example.Water || block == Example.WaterNoFlow;
    }

    public int GetNumAirNeighbors(long wx, long wy, long wz)
    {
        return
            (GetBlock(wx + 1, wy, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlock(wx - 1, wy, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlock(wx, wy + 1, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlock(wx, wy - 1, wz) == (int)Example.Air ? 1 : 0) +
            (GetBlock(wx, wy, wz + 1) == (int)Example.Air ? 1 : 0) +
            (GetBlock(wx, wy, wz - 1) == (int)Example.Air ? 1 : 0);

    }

    public override void OnTick(BlockData block)
    {
        numUpdatesThisTick += 1;
        if (maxUpdates < numUpdatesThisTick)
        {
            block.needsAnotherTick = true;
            return;
        }

        // water: state 2 = time I got here


        block.needsAnotherTick = false;


        // if we are WATER without water above and with water below, pathfind to look for open space
        //if (block == (int)Example.Water && IsWater(this[wx, wy - 1, wz]) && !IsWater(this[wx, wy + 1, wz]))
        if (block.block == Example.Water && IsWater(block.x, block.y-1, block.z) && !IsWater(block.x, block.y+1, block.z))
        {
            // returns true if search found something in maxSteps or less. Search "finds something" if isBlockDesiredResult was ever called and returned true
            //if (PhysicsUtils.SearchOutwards(new LVector3(wx, wy, wz), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            //numWaterUpdatesThisTick += 1;
            if (PhysicsUtils.SearchOutwards(new LVector3(block.x, block.y, block.z), maxSteps: 30, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
            {
                return by < block.y && (b == (int)Example.Water || b == (int)Example.WaterNoFlow);
            },
                isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                {
                    if (b == (int)Example.Air && by < block.y)
                    {
                        SetBlock(bx, by, bz, Example.Water);
                        SetState(bx, by, bz, GetNumAirNeighbors(bx, by, bz), 3);
                        return true;
                    }
                    return false;
                }
            ))
            {
                block.state3 = 0;
                block.block = Example.Air;
                return;
            }
            else
            {
                block.needsAnotherTick = true;
                block.block = Example.WaterNoFlow;
                return;
            }
        }
        else
        {

            // if air below, set below = water and us = air
            if (GetBlock(block.x, block.y-1, block.z) == Example.Air)
            {
                SetBlock(block.x, block.y-1, block.z, Example.Water);
                block.state3 = 0;
                SetState(block.x, block.y - 1, block.z, GetNumAirNeighbors(block.x, block.y - 1, block.z), 3); // +1 because we are now air instead of water
                block.block = Example.Air;
                return;
            }
            else
            {
                // otherwise, look if air neighbors (or air neighbors of air neighbors one block out in a line) have air below them, if so flow into them
                foreach (BlockData neighbor in GetNeighbors(block, includingUp: false, includingDown: false))
                {
                    LVector3 pos2 = 2 * (neighbor.pos - block.pos) + block.pos;
                    if (neighbor.block == Example.Air)
                    {
                        if (GetBlock(neighbor.x, neighbor.y-1, neighbor.z) == Example.Air)
                        {
                            SetBlock(neighbor.x, neighbor.y - 1, neighbor.z, Example.Water);
                            SetState(neighbor.x, neighbor.y - 1, neighbor.z, GetNumAirNeighbors(neighbor.x, neighbor.y-1, neighbor.z), 3);
                            block.state3 = 0;
                            block.block = Example.Air;
                            return;
                        }
                        else if (pos2.BlockV == Example.Air && GetBlock(pos2.x, pos2.y - 1, pos2.z) == Example.Air)
                        {
                            SetBlock(pos2.x, pos2.y - 1, pos2.z, Example.Water);
                            SetState(pos2.x, pos2.y - 1, pos2.z, GetNumAirNeighbors(pos2.x, pos2.y - 1, pos2.z), 3);
                            block.state3 = 0;
                            block.block = Example.Air;
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
                        if (neighbor.block == Example.Air)
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
                        return (b == (int)Example.Water || b == (int)Example.WaterNoFlow);
                    },
                        isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                        {
                            if (b == (int)Example.WaterNoFlow && IsWater(bx, by - 1, bz) && airNeighbor.y < by)
                            {
                                SetBlock(bx, by, bz, Example.Air);
                                SetState(bx, by, bz, 0, 3);
                                return true;
                            }
                            return false;
                        }
                    ))
                    {

                        SetBlock(airNeighbor.x, airNeighbor.y, airNeighbor.z, Example.Water);
                        SetState(airNeighbor.x, airNeighbor.y, airNeighbor.z, GetNumAirNeighbors(airNeighbor.x, airNeighbor.y, airNeighbor.z), 3);
                        block.state3 = curNumAirNeighbors - 1; // we just replaced an air neighbor with water
                        block.needsAnotherTick = true;
                        return;
                    }
                    else
                    {
                        block.needsAnotherTick = true;
                        block.block = Example.WaterNoFlow;
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

public class Grass : Block2
{
    public override void DropBlockOnDestroy(BlockData block, BlockStack thingBreakingWith, Vector3 positionOfBlock, Vector3 posOfOpening, out bool destroyBlock)
    {
        CreateBlockEntity(Example.Dirt, positionOfBlock);
        destroyBlock = true;
    }

    int numGrassTicks;
    public override void OnTickStart()
    {
        if (numGrassTicks > 0)
        {

            Debug.Log("num grass ticks = " + numGrassTicks);
            numGrassTicks = 0;
        }
    }

    public override void OnTick(BlockData block)
    {
        long x = block.x;
        long y = block.y;
        long z = block.z;
        long state1 = block.state1;
        long state2 = block.state2;
        long state3 = block.state3;
        block.needsAnotherTick = false;
        if (GetBlock(block.x, block.y+1, block.z) != Example.Air)
        {
            block.block = Example.Dirt;
            return;
        }
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            // if neighbor is dirt and it has air above it, try growing into it
            if (neighbor.block == Example.Dirt && GetBlock(neighbor.x, neighbor.y + 1, neighbor.z) == Example.Air)
            {
                if (rand() < 0.5f)
                {
                    // grow
                    neighbor.block = Example.Grass;
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
        if (thingBreakingWith.Block == Example.Shovel)
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
        CreateBlockEntity(Example.Stick, positionOfBlock);
        if (Random.value < 0.2f)
        {
            CreateBlockEntity(Example.Stick, positionOfBlock);
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
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock ||
            thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock ||
            thingBreakingWith.Block == Example.Pickaxe)
        {
            CreateBlockEntity(Example.SharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(Example.Rock, positionOfBlock);
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock)
        {
            return 3.0f;
        }
        else if(thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock)
        {
            return 2.0f;
        }
        else if(thingBreakingWith.Block == Example.Pickaxe)
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
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock ||
            thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock ||
            thingBreakingWith.Block == Example.Pickaxe)
        {
            CreateBlockEntity(Example.LargeSharpRock, positionOfBlock);
        }
        else
        {
            CreateBlockEntity(Example.LargeRock, positionOfBlock);
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Rock || thingBreakingWith.Block == Example.SharpRock)
        {
            return 5.0f;
        }
        else if (thingBreakingWith.Block == Example.LargeRock || thingBreakingWith.Block == Example.LargeSharpRock)
        {
            return 4.0f;
        }
        else if (thingBreakingWith.Block == Example.Pickaxe)
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
            CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere*0.1f);
        }
        CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
        CreateBlockEntity(Example.String, positionOfBlock + Random.insideUnitSphere * 0.1f);
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
            CreateBlockEntity(Example.Rock, posOfOpening);
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(Example.LargeRock, positionOfBlock);
            destroyBlock = true;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        if (thingBreakingWith.Block == Example.Pickaxe)
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
            CreateBlockEntity(Example.Bark, posOfOpening);
            block.state1 -= 1;
            destroyBlock = false;
        }
        else
        {
            CreateBlockEntity(Example.Trunk, positionOfBlock);
            destroyBlock = true;
        }
    }

    public override float TimeNeededToBreak(BlockData block, BlockStack thingBreakingWith)
    {
        // breaking bark off off tree
        if (block.state1 > 0)
        {
            if (thingBreakingWith.Block == Example.Axe)
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
            if (thingBreakingWith.Block == Example.Axe)
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
        CreateBlockEntity(Example.Bark, positionOfBlock);
    }

    public override void OnTick(BlockData block)
    {
        block.needsAnotherTick = true;
        foreach (BlockData neighbor in Get26Neighbors(block))
        {
            if (neighbor.block == Example.Water || neighbor.block == Example.WaterNoFlow)
            {
                if (Random.value < 0.05f)
                {
                    block.block = Example.WetBark;
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
        AddCustomBlock(Example.Grass, new Grass(), 64);
        AddCustomBlock(Example.Bark, new Bark(), 64);
        AddCustomBlock(Example.Trunk, new Trunk(), 64);
        AddCustomBlock(Example.Dirt, new SimpleBlock(2.0f, new Tuple<BlockValue, float>(Example.Shovel, 1.0f)), 64);
        AddCustomBlock(Example.Clay, new SimpleBlock(1.0f, new Tuple<BlockValue, float>(Example.Shovel, 0.6f)), 64);
        AddCustomBlock(Example.Stone, new SimpleBlock(10.0f, new Tuple<BlockValue, float>(Example.Pickaxe, 3.0f)), 64);
        AddCustomBlock(Example.Leaf, new Leaf(), 64);
        AddCustomBlock(Example.LooseRocks, new LooseRocks(), 64);
        AddCustomBlock(Example.Rock, new Rock(), 64);
        AddCustomBlock(Example.LargeRock, new LargeRock(), 64);
        AddCustomBlock(Example.LooseRocks, new LooseRocks(), 64);
        AddCustomBlock(Example.Stick, new SimpleItem(), 64);
        AddCustomBlock(Example.Pickaxe, new SimpleItem(), 64);
        AddCustomBlock(Example.SharpRock, new SimpleItem(), 64);
        AddCustomBlock(Example.LargeSharpRock, new SimpleItem(), 64);
        AddCustomBlock(Example.Shovel, new SimpleItem(), 1);
        AddCustomBlock(Example.Axe, new SimpleItem(), 1);
        AddCustomBlock(Example.WetBark, new WetBark(), 64);
        AddCustomBlock(Example.Sand, new Sand(), 64);
        AddCustomBlock(Example.String, new SimpleItem(), 64);
        //AddCustomBlock(Example.Water, new Water(), 64);
        //AddCustomBlock(Example.WaterNoFlow, new Water(), 64);
        AddCustomBlock(Example.Water, new SimpleWater(), 64);
        AddCustomBlock(Example.WaterNoFlow, new SimpleWater(), 64);


        AddCustomRecipe(new Recipe(new BlockValue[,]
        {
            { Example.Rock, Example.Rock },
            { Example.Rock, Example.Rock },
        }, new BlockStack(Example.CraftingTable, 1)));

        AddCustomRecipe(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Shovel, 1)));

        AddCustomRecipe(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeSharpRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Axe, 1)));

        AddCustomRecipe(new Recipe(new BlockValue[,]
        {
            { Example.LargeSharpRock, Example.LargeRock,  Example.LargeSharpRock},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Pickaxe, 1)));

        AddCustomRecipe(new Recipe(new BlockValue[,]
        {
            { Example.LargeSharpRock, Example.LargeSharpRock,  Example.LargeSharpRock},
            { Example.Stick, Example.Stick, Example.Stick },
            { Example.Dirt, Example.Dirt, Example.Dirt }
        }, new BlockStack(Example.Sand, 1)));



        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
