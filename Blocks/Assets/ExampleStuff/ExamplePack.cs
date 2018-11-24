using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
        SetCustomGeneration(new ExampleGeneration());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
