using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class Recipe
    {
        BlockValue[,] recipe;
        BlockValue[] unorderedRecipe;
        public BlockStack result;
        public Recipe(BlockValue[,] recipe, BlockStack result)
        {
            this.recipe = recipe;
            this.unordered = false;
            this.result = result;
        }


        public Recipe(BlockValue[] recipe, BlockStack result)
        {
            this.unorderedRecipe = recipe;
            this.unordered = true;
            this.result = result;
        }

        bool unordered = false;

        public bool InventoryMatchesRecipe(Inventory inventory, int nRows, int maxBlocks, bool useResources)
        {
            if (maxBlocks == -1)
            {
                maxBlocks = inventory.capacity;
            }
            if (maxBlocks > inventory.capacity)
            {
                maxBlocks = inventory.capacity;
            }
            if (maxBlocks % nRows != 0)
            {
                Debug.LogError("maxBlocks size of " + maxBlocks + " is not a multiple of given nRows=" + nRows);
                return false;
            }


            if (unordered)
            {
                int numMatchesNeeded = unorderedRecipe.Length;
                int numMatches = 0;
                int[] matches = new int[unorderedRecipe.Length];
                for (int i = 0; i < matches.Length; i++)
                {
                    matches[i] = -1;
                }
                for (int i = 0; i < maxBlocks; i++)
                {
                    if (inventory.blocks[i] != null)
                    {
                        bool foundMatch = false;
                        for (int j = 0; j < unorderedRecipe.Length; j++)
                        {
                            if (matches[j] == -1 && unorderedRecipe[j] == inventory.blocks[i].Block)
                            {
                                matches[j] = i;
                                foundMatch = true;
                                numMatches += 1;
                                break;
                            }
                        }
                        if (!foundMatch)
                        {
                            return false;
                        }
                    }
                }

                if (numMatches == numMatchesNeeded)
                {
                    if (useResources)
                    {
                        for (int i = 0; i < matches.Length; i++)
                        {
                            inventory.blocks[matches[i]].count -= 1;
                            if (inventory.blocks[matches[i]].count <= 0)
                            {
                                inventory.blocks[matches[i]] = null;
                            }
                        }
                    }
                    return true;
                }



                return false;
            }
            else
            {
                int nColumns = maxBlocks / nRows;
                int numMatchesNeeded = recipe.GetLength(0) * recipe.GetLength(1);
                for (int topLeftX = 0; topLeftX < nColumns; topLeftX++)
                {
                    for (int topLeftY = 0; topLeftY < nRows; topLeftY++)
                    {
                        int numMatches = 0;
                        for (int i = 0; i < recipe.GetLength(1); i++)
                        {
                            for (int j = 0; j < recipe.GetLength(0); j++)
                            {
                                int x = topLeftX + i;
                                int y = topLeftY + j;
                                if (x < nColumns && y < nRows)
                                {
                                    int index = x + y * nColumns;
                                    BlockStack cur = inventory.blocks[index];
                                    BlockValue recipeCur = recipe[recipe.GetLength(0) - j - 1, i];
                                    if (cur == null)
                                    {
                                        if (recipeCur == BlockValue.Air)
                                        {
                                            Debug.Log(x + " " + y + "inventory empty here and recipe is too, recipeCur=" + World.BlockToString((int)recipeCur));
                                            numMatches += 1;
                                        }
                                        else
                                        {
                                            Debug.Log(x + " " + y + "inventory empty here and recipe is not, recipeCur=" + World.BlockToString((int)recipeCur));
                                        }
                                    }
                                    else if (cur != null)
                                    {
                                        if (cur.block == recipeCur)
                                        {
                                            Debug.Log(x + " " + y + "same: inventory is " + World.BlockToString(cur.block) + " and recipe is " + World.BlockToString((int)recipeCur));
                                            numMatches += 1;
                                        }
                                        else
                                        {
                                            Debug.Log(x + " " + y + "different: inventory is " + World.BlockToString(cur.block) + " and recipe is " + World.BlockToString((int)recipeCur));
                                        }
                                    }
                                }
                            }
                        }
                        Debug.Log("got num matches " + numMatches + " with top left x=" + topLeftX + " and topLeftY=" + topLeftY);
                        if (numMatches == numMatchesNeeded)
                        {
                            bool hasExtraStuff = false;
                            for (int x = 0; x < topLeftX; x++)
                            {
                                for (int y = 0; y < nRows; y++)
                                {
                                    int index = x + y * nColumns;
                                    if (inventory.blocks[index] != null)
                                    {
                                        hasExtraStuff = true;
                                        break;
                                    }
                                }
                                if (hasExtraStuff)
                                {
                                    break;
                                }
                            }

                            for (int x = topLeftX; x < nColumns; x++)
                            {
                                for (int y = 0; y < topLeftY; y++)
                                {
                                    int index = x + y * nColumns;
                                    if (inventory.blocks[index] != null)
                                    {
                                        hasExtraStuff = true;
                                        break;
                                    }
                                }
                                if (hasExtraStuff)
                                {
                                    break;
                                }
                            }
                            if (hasExtraStuff)
                            {
                                Debug.Log("matches recipe but has extra stuff");
                                return false;
                            }
                            else
                            {
                                Debug.Log("matches recipe completely");


                                if (useResources)
                                {
                                    for (int i = 0; i < recipe.GetLength(1); i++)
                                    {
                                        for (int j = 0; j < recipe.GetLength(0); j++)
                                        {
                                            int x = topLeftX + i;
                                            int y = topLeftY + j;
                                            int index = x + y * nColumns;
                                            if (inventory.blocks[index] != null)
                                            {
                                                inventory.blocks[index].count -= 1;
                                                if (inventory.blocks[index].count <= 0)
                                                {
                                                    inventory.blocks[index] = null;
                                                }
                                            }
                                        }
                                    }
                                }
                                return true;
                            }
                        }
                        else if (numMatches > numMatchesNeeded)
                        {
                            Debug.LogWarning("numMatches = " + numMatches + " but maximum num matches is " + numMatchesNeeded + " this is invalid how u do this");
                        }

                    }
                }
                return false;

            }

        }
    }


    public class BlockInventoryGui : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}