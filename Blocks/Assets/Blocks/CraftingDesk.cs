﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Example_pack;
using Blocks;

public class CraftingDesk : InventoryListener {


    Recipe addedTmpItemWithRecipe;
    Blocks.BlockOrItem prevCustomItem = null;


    bool IsEmptyBlockStack(BlockStack blockStack)
    {
        return blockStack == null || (blockStack.Block == BlockValue.Air && blockStack.count == 0);
    }

    public override void OnInventoryChange(InventoryGui inventoryGui, Inventory inventory, int numRows, int maxItems)
    {
        if (inventory.resultBlocks == null || inventory.resultBlocks.Length == 0)
        {
            Debug.LogWarning("crafting inventory has resultBlocks array = null or len = 0, bailing");
            return;
        }

        if (addedTmpItemWithRecipe != null)
        {
            // if switched to something else, clear the result slots
            if (prevCustomItem != inventoryGui.customBlockOwner)
            {
                addedTmpItemWithRecipe = null;
                inventory.resultBlocks[0] = null;
                prevCustomItem = inventoryGui.customBlockOwner;
            }
            else
            {
                if (IsEmptyBlockStack(inventory.resultBlocks[0]))
                {
                    if (addedTmpItemWithRecipe.InventoryMatchesRecipe(inventory, numRows, maxItems, useResources: true))
                    {

                    }
                    else
                    {
                        Debug.LogWarning("took result but can't get the resources for it?");
                    }
                    addedTmpItemWithRecipe = null;
                }
                else
                {
                    addedTmpItemWithRecipe = null;
                    inventory.resultBlocks[0] = new BlockStack(BlockValue.Air, 0);
                }
            }
        }
        else if (!IsEmptyBlockStack(inventory.resultBlocks[0]))
        {
            inventory.resultBlocks[0] = new BlockStack(BlockValue.Air, 0);
        }

        prevCustomItem = inventoryGui.customBlockOwner;

        Debug.Log("inventory changed, checking recipe");
        if (inventoryGui.customBlockOwner != null)
        {
            foreach (Recipe recipe in inventoryGui.customBlockOwner.recipes)
            {
                if (recipe.InventoryMatchesRecipe(inventory, numRows, maxItems, false))
                {
                    addedTmpItemWithRecipe = recipe;
                    inventory.resultBlocks[0] = recipe.result.Copy();
                    break;
                }
            }
        }
    }


    List<Recipe> recipes = new List<Recipe>();

    public void AddRecipe(Recipe recipe)
    {
        if (recipes == null)
        {
            recipes = new List<Recipe>();
        }
        recipes.Add(recipe);
    }

    // Use this for initialization
    void Start ()
    {
        // ordered recipe: requires this shape
        /*
        Recipe makingChest = new Recipe(new BlockValue[,]
        {
            { Example.Rock, Example.Rock },
            { Example.Rock, Example.Rock },
        }, new BlockStack(Example.CraftingTable, 1));
        
        recipes.Add(makingChest);

        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Shovel, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.Air, Example.LargeSharpRock,  Example.Air},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Axe, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { Example.LargeSharpRock, Example.LargeRock,  Example.LargeSharpRock},
            { Example.String, Example.Stick, Example.String },
            { Example.Air, Example.Stick, Example.Air }
        }, new BlockStack(Example.Pickaxe, 1)));
        
        // unordered recipe: any shape is ok as long as it has these ingredients somewhere in it
        //Recipe makingSharpRock = new Recipe(new BlockValue[]
        //{BlockValue.ROCK, BlockValue.ROCK, BlockValue.ROCK}, new BlockStack(BlockValue.SharpRock, 1));
        */

    }

    // Update is called once per frame
    void Update () {
		
	}
}
