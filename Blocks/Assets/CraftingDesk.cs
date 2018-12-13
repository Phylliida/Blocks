using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Example_pack;
using Blocks;

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
