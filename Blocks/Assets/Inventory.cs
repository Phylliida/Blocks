using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Block
{
    public int block;

    public Block(int block)
    {
        this.block = block;
    }
}

public class BlockStack
{
    public int block;
    public int count;

    public BlockStack(int block, int count)
    {
        this.block = block;
        this.count = count;
    }

    public bool CanAddToStack(int block)
    {
        return (block == this.block && World.stackableSize.ContainsKey(block) && World.stackableSize[block] > count);
    }

    public bool TryToAddToStack(int block)
    {
        if (CanAddToStack(block))
        {
            this.count += 1;
            return true;
        }
        else
        {
            return false;
        }
    }
}

public class Inventory {

    
    public bool CanAddBlock(int block)
    {
        // look for a stack
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null && blocks[i].block == block)
            {
                if (blocks[i].CanAddToStack(block))
                {
                    return true;
                }
            }
        }

        // no stack, look for empty slot
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null)
            {
                return true;
            }
        }

        // no empty spots or stacks we can put it in, return false
        return false;
    }
    public bool TryToAddBlock(int block)
    {
        // look for a stack
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null && blocks[i].block == block)
            {
                if (blocks[i].TryToAddToStack(block))
                {
                    return true;
                }
            }
        }

        // no stack, look for empty slot
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null)
            {
                blocks[i] = new BlockStack(block, 1);
                return true;
            }
        }

        // no empty spots or stacks we can put it in, return false
        return false;
    }

    public int capacity;

    public BlockStack[] blocks;

    public Inventory(int capacity)
    {
        this.capacity = capacity;
        blocks = new BlockStack[capacity];
    }

    public override string ToString()
    {
        string resString = "";
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] == null)
            {
                resString += "[] ";
            }
            else
            {
                resString += "[" + World.BlockToString(blocks[i].block) + "(" + blocks[i].count + ")" + "] ";
            }
        }
        return resString;
    }
}
