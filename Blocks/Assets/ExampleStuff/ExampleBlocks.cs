using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Grass : Block
{
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
            if (neighbor.block == BlockValue.DIRT && GetBlock(neighbor.x, neighbor.y+1, neighbor.z) == BlockValue.AIR)
            {
                if (rand() < 0.01f)
                {
                    // grow
                    neighbor.block = BlockValue.GRASS;
                }
                else
                {
                    // we failed to grow but still need to, try again next tick
                    block.needsAnotherTick = true;
                }
            }
        }
    }
}



public class Clay : Block
{
    public override void OnTick(BlockData block)
    {
    }
}