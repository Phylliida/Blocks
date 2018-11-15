using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingDesk : InventoryListener {


    bool addedTmpItem = false;
    public override void OnInventoryChange(InventoryGui inventoryGui, Inventory inventory, int numRows, int maxItems)
    {
        if (inventory.resultBlocks == null)
        {
            Debug.LogWarning("crafting inventory has resultBlocks array = null, bailing");
            return;
        }
        if (addedTmpItem)
        {
            if (inventory.resultBlocks[0] == null)
            {
                if (makingChest.InventoryMatchesRecipe(inventory, numRows, maxItems, useResources:true))
                {
                }
                else
                {
                    Debug.LogWarning("took result but can't get the resources for it?");
                }
                addedTmpItem = false;
            }
            else
            {
                addedTmpItem = false;
                inventory.resultBlocks[0] = null;
            }
        }
        else if(inventory.resultBlocks[0] != null)
        {
            inventory.resultBlocks[0] = null;
        }

        Debug.Log("inventory changed, checking recipe");
        if (makingChest.InventoryMatchesRecipe(inventory, numRows, maxItems, false))
        {
            addedTmpItem = true;
            inventory.resultBlocks[0] = new BlockStack(makingChest.result.block, makingChest.result.count);
        }
    }


    Recipe makingChest;
    // Use this for initialization
    void Start () {
        makingChest = new Recipe(new BlockValue[,]
        {
            { BlockValue.EMPTY, BlockValue.STONE },
            { BlockValue.TRUNK, BlockValue.EMPTY },
        }, new BlockStack(BlockValue.CHEST, 1));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
