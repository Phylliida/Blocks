using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Example_pack;

public class CraftingDesk : InventoryListener {


    Recipe addedTmpItemWithRecipe;
    public override void OnInventoryChange(InventoryGui inventoryGui, Inventory inventory, int numRows, int maxItems)
    {
        if (inventory.resultBlocks == null)
        {
            Debug.LogWarning("crafting inventory has resultBlocks array = null, bailing");
            return;
        }
        if (addedTmpItemWithRecipe != null)
        {
            if (inventory.resultBlocks[0] == null)
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
                inventory.resultBlocks[0] = null;
            }
        }
        else if(inventory.resultBlocks[0] != null)
        {
            inventory.resultBlocks[0] = null;
        }

        Debug.Log("inventory changed, checking recipe");
        foreach (Recipe recipe in recipes)
        {
            if (recipe.InventoryMatchesRecipe(inventory, numRows, maxItems, false))
            {
                addedTmpItemWithRecipe = recipe;
                inventory.resultBlocks[0] = new BlockStack(recipe.result.block, recipe.result.count);
                break;
            }
        }
    }


    List<Recipe> recipes;

    // Use this for initialization
    void Start ()
    {
        recipes = new List<Recipe>();
        // ordered recipe: requires this shape
        Recipe makingChest = new Recipe(new BlockValue[,]
        {
            { BlockValue.Rock, BlockValue.Rock },
            { BlockValue.Rock, BlockValue.Rock },
        }, new BlockStack(BlockValue.CraftingTable, 1));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { BlockValue.Air, BlockValue.LargeRock,  BlockValue.Air},
            { BlockValue.String, BlockValue.Stick, BlockValue.String },
            { BlockValue.Air, BlockValue.Stick, BlockValue.Air }
        }, new BlockStack(BlockValue.Shovel, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { BlockValue.Air, BlockValue.LargeSharpRock,  BlockValue.Air},
            { BlockValue.String, BlockValue.Stick, BlockValue.String },
            { BlockValue.Air, BlockValue.Stick, BlockValue.Air }
        }, new BlockStack(BlockValue.Axe, 1)));


        recipes.Add(new Recipe(new BlockValue[,]
        {
            { BlockValue.LargeSharpRock, BlockValue.LargeRock,  BlockValue.LargeSharpRock},
            { BlockValue.String, BlockValue.Stick, BlockValue.String },
            { BlockValue.Air, BlockValue.Stick, BlockValue.Air }
        }, new BlockStack(BlockValue.Pickaxe, 1)));

        // unordered recipe: any shape is ok as long as it has these ingredients somewhere in it
        //Recipe makingSharpRock = new Recipe(new BlockValue[]
        //{BlockValue.ROCK, BlockValue.ROCK, BlockValue.ROCK}, new BlockStack(BlockValue.SharpRock, 1));


        recipes.Add(makingChest);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
