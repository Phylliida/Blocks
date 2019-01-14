using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    [System.Serializable]
    public class BlockStack
    {
        public int block;
        public int count;
        public int durability = 0;
        public int maxDurability = 0;

        public BlockStack Copy()
        {
            return new BlockStack(block, count, durability, maxDurability);
        }
        public BlockValue Block
        {
            get
            {
                return (BlockValue)block;
            }
            set
            {
                block = (int)value;
            }
        }

        public BlockStack(int block, int count, int durability=0, int maxDurability=0)
        {
            this.block = block;
            this.count = count;
            this.durability = durability;
            this.maxDurability = maxDurability;
        }

        public BlockStack(BlockValue block, int count, int durability = 0, int maxDurability =0)
        {
            this.block = (int)block;
            this.count = count;
            this.durability = durability;
            this.maxDurability = maxDurability;
        }

        public bool CanAddToStack(BlockStack block)
        {
            if (maxDurability != 0 || block.maxDurability != 0)
            {
                return false;
            }
            return (block.block == this.block && World.stackableSize.ContainsKey(this.block) && World.stackableSize[this.block] > count);
        }

        public bool TryToAddToStack(BlockStack block)
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



    public class Inventory
    {


        public void ThrowAllBlocks(Vector3 position)
        {
            if (resultBlocks != null)
            {
                for (int i = 0; i < resultBlocks.Length; i++)
                {
                    resultBlocks[i] = null;
                }
            }
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null)
                {
                    for (int j = 0; j < blocks[i].count; j++)
                    {
                        World.mainWorld.CreateBlockEntity(blocks[i], position + Random.insideUnitSphere * 0.5f);
                    }
                    blocks[i] = null;
                }
            }
        }

        public bool CanAddBlock(BlockStack block)
        {
            // look for a stack
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null && blocks[i].block == block.block)
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
        public bool TryToAddBlock(BlockStack block)
        {
            // look for a stack
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != null && blocks[i].block == block.block)
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
                    blocks[i] = block.Copy();
                    return true;
                }
            }

            // no empty spots or stacks we can put it in, return false
            return false;
        }

        public BlockStack[] resultBlocks;

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
}