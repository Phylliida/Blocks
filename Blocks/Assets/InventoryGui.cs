using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class InventoryListener : MonoBehaviour
{
    public abstract void OnInventoryChange(InventoryGui inventoryGui, Inventory inventory, int numRows, int maxItems);
}

public class InventoryGui : MonoBehaviour {

    public BlocksPlayer playerUsing;

	// Use this for initialization
	void Start () {

    }

    public int selection = 0;

    public Inventory inventory;
    public bool displaying = false;

    float inventoryWidth = 110.0f;
    float inventoryHeight = 110.0f;

    public int numRows;
    public int maxItems = -1;


    public Vector2 screenOffset = new Vector2(0, -100.0f);

    public List<BlockEntity> blockItems = new List<BlockEntity>();


    bool hasInventory = false;

    private void Update()
    {

        if (inventory == null || !displaying || playerUsing == null)
        {
            hasInventory = false;
        }
        if (!displaying || playerUsing == null)
        {
            for (int i = 0; i < blockItems.Count; i++)
            {
                GameObject.Destroy(blockItems[i].gameObject);
            }
            blockItems.Clear();
            return;
        }

        if (!hasInventory && inventory != null && displaying && playerUsing != null)
        {
            hasInventory = true;
            CallInventoryModifiedCallbacks();
        }



        if (selection < 0)
        {
            selection = 0;
        }
        int actualMaxItems = inventory.capacity;
        if (maxItems != -1)
        {
            actualMaxItems = maxItems;
        }
        if (selection > actualMaxItems)
        {
            selection = actualMaxItems;
        }

        ShowInventory(numRows, maxItems);
        if (playerUsing.mouseLook.allowedToCapture)
        {
            if (playerUsing.blocksHoldingWithMouse != null)
            {
                ThrowStuff();

            }
            return;
        }

        bool inventoryModified = false;
        // click when not capturing
        if (playerUsing.blocksHoldingWithMouse == null)
        {
            if (Input.GetMouseButtonDown(0) && displaying)
            {
                for (int i = 0; i < blockItems.Count; i++)
                {
                    if (i < inventory.blocks.Length)
                    {
                        //Debug.Log("trying "  + i + " " + inventory.blocks[i] + " " + name);
                        if (MouseIntersectsBlockEntity(blockItems[i]) && inventory.blocks[i] != null)
                        {
                            //Debug.Log("got dat boi");
                            playerUsing.blocksHoldingWithMouse = inventory.blocks[i];
                            playerUsing.holdingWithMouseEntity = MakeNewBlockEntity();
                            playerUsing.holdingWithMouseEntity.blockStack = inventory.blocks[i];
                            inventory.blocks[i] = null;
                            inventoryModified = true;
                            //playerUsing.mouseLook.allowedToCapture = false;
                            break;
                        }
                    }
                    else
                    {
                        int ind = i - inventory.blocks.Length;
                        //Debug.Log("trying "  + i + " " + inventory.blocks[i] + " " + name);
                        if (MouseIntersectsBlockEntity(blockItems[i]) && inventory.resultBlocks[ind] != null)
                        {
                            //Debug.Log("got dat boi");
                            playerUsing.blocksHoldingWithMouse = inventory.resultBlocks[ind];
                            playerUsing.holdingWithMouseEntity = MakeNewBlockEntity();
                            playerUsing.holdingWithMouseEntity.blockStack = inventory.resultBlocks[ind];
                            inventory.resultBlocks[ind] = null;
                            inventoryModified = true;
                            //playerUsing.mouseLook.allowedToCapture = false;
                            break;
                        }
                    }
                }
            }
        }

        else if (playerUsing.blocksHoldingWithMouse != null && displaying)
        {
            //player.mouseLook.allowedToCapture = false;
            Vector3 offset;
            Ray ray = playerUsing.mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f));
            playerUsing.holdingWithMouseEntity.transform.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2.0f, Input.mousePosition.y - Screen.height / 2.0f, 0.01f);
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                bool foundCollision = false;
                for (int i = 0; i < blockItems.Count && i < inventory.blocks.Length; i++)
                {
                    if (MouseIntersectsBlockEntity(blockItems[i]))
                    {
                        foundCollision = true;
                        bool letGo = false;
                        if (inventory.blocks[i] == null)
                        {
                            // right click, drop one
                            if (Input.GetMouseButtonDown(1))
                            {

                                inventory.blocks[i] = new BlockStack(playerUsing.blocksHoldingWithMouse.block, 1);
                                inventoryModified = true;
                                if (playerUsing.blocksHoldingWithMouse.count == 1)
                                {
                                    letGo = true;
                                }
                                else
                                {
                                    playerUsing.blocksHoldingWithMouse.count -= 1;
                                }
                            }
                            // left click, put all
                            else if (Input.GetMouseButtonDown(0))
                            {
                                inventory.blocks[i] = playerUsing.blocksHoldingWithMouse;
                                inventoryModified = true;
                                letGo = true;
                            }
                        }
                        else
                        {
                            if (inventory.blocks[i].block == playerUsing.blocksHoldingWithMouse.block)
                            {
                                int numMaxStack = 1;
                                if (World.stackableSize.ContainsKey(playerUsing.blocksHoldingWithMouse.block))
                                {
                                    numMaxStack = World.stackableSize[playerUsing.blocksHoldingWithMouse.block];
                                }
                                // right click, drop one
                                if (Input.GetMouseButtonDown(1))
                                {
                                    // can fit some in, put 1 in
                                    if (inventory.blocks[i].count < numMaxStack)
                                    {
                                        // we are only holding one, put it all in
                                        if (playerUsing.blocksHoldingWithMouse.count == 1)
                                        {
                                            inventory.blocks[i].count += playerUsing.blocksHoldingWithMouse.count;
                                            inventoryModified = true;
                                            letGo = true;
                                            playerUsing.blocksHoldingWithMouse = null;
                                        }
                                        // put 1 in and leave rest of stack in hand
                                        else
                                        {
                                            inventory.blocks[i].count += 1;
                                            inventoryModified = true;
                                            playerUsing.blocksHoldingWithMouse.count -= 1;
                                            if (playerUsing.blocksHoldingWithMouse.count <= 0)
                                            {
                                                Debug.LogError("we should have count > 0 when not putting all (right click and we have more than 1 put only put 1 in) into block, got count " + playerUsing.blocksHoldingWithMouse.count + " instead");
                                            }
                                        }
                                    }
                                }
                                // left click, place all sorta
                                else if (Input.GetMouseButtonDown(0))
                                {
                                    // can fit some in
                                    if (inventory.blocks[i].count < numMaxStack)
                                    {
                                        // can fit all in
                                        if (inventory.blocks[i].count + playerUsing.blocksHoldingWithMouse.count <= numMaxStack)
                                        {
                                            inventory.blocks[i].count += playerUsing.blocksHoldingWithMouse.count;
                                            inventoryModified = true;
                                            letGo = true;
                                            playerUsing.blocksHoldingWithMouse = null;
                                        }
                                        // can only fit some in
                                        else
                                        {
                                            int numCanFitIn = numMaxStack - inventory.blocks[i].count;
                                            inventory.blocks[i].count += numCanFitIn;
                                            inventoryModified = true;
                                            playerUsing.blocksHoldingWithMouse.count -= numCanFitIn;
                                            if (playerUsing.blocksHoldingWithMouse.count <= 0)
                                            {
                                                Debug.LogError("we should have count > 0 when not putting all into block, got count " + playerUsing.blocksHoldingWithMouse.count + " instead");
                                            }
                                        }
                                    }
                                    // can't fit any more in, swap
                                    else
                                    {
                                        inventory.blocks[i].count = playerUsing.blocksHoldingWithMouse.count;
                                        inventoryModified = true;
                                        playerUsing.blocksHoldingWithMouse.count = numMaxStack;
                                    }

                                }
                            }
                            // different things, swap
                            else if (Input.GetMouseButtonDown(0))
                            {
                                BlockStack tmp = inventory.blocks[i];
                                inventory.blocks[i] = playerUsing.blocksHoldingWithMouse;
                                inventoryModified = true;
                                playerUsing.blocksHoldingWithMouse = tmp;
                                playerUsing.holdingWithMouseEntity.blockStack = tmp;
                            }
                        }
                        if (letGo)
                        {
                            playerUsing.blocksHoldingWithMouse = null;
                            GameObject.Destroy(playerUsing.holdingWithMouseEntity.gameObject);
                            playerUsing.holdingWithMouseEntity = null;
                            //mouseLook.allowedToCapture = true;
                        }
                    }
                }
                // didn't click on anything, threw on ground instead
                //if (!foundCollision)
                //{
                //    ThrowStuff();
                //}
            }
        }

        if (playerUsing.blocksHoldingWithMouse != null && displaying)
        {
            //player.mouseLook.allowedToCapture = false;
            Vector3 offset;
            Ray ray = playerUsing.mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f));
            playerUsing.holdingWithMouseEntity.transform.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2.0f, Input.mousePosition.y - Screen.height / 2.0f, 0.01f);
        }


        if (inventoryModified)
        {
            CallInventoryModifiedCallbacks();
        }
    }

    void CallInventoryModifiedCallbacks()
    {
        int curMaxItems = maxItems;
        if (maxItems == -1)
        {
            curMaxItems = inventory.capacity;
        }
        foreach (InventoryListener inventoryListener in GetComponents<InventoryListener>())
        {
            inventoryListener.OnInventoryChange(this, inventory, numRows, curMaxItems);
        }
    }





    BlockEntity MakeNewBlockEntity()
    {
        BlockEntity res = GameObject.Instantiate(World.mainWorld.blocksWorld.blockRenderPrefab).GetComponent<BlockEntity>();
        res.GetComponent<UnityEngine.UI.Image>().material = new Material(res.GetComponent<UnityEngine.UI.Image>().material);
        res.blockId = -1;
        res.pullable = false;
        res.transform.SetParent(World.mainWorld.blocksWorld.blockRenderCanvas.transform);
        res.transform.localPosition = new Vector3(0, 0, 0);
        res.transform.localScale = new Vector3(10, 10, 10);
        res.transform.localRotation = Quaternion.identity;
        return res;
    }

    void ShowInventory(int nRows=4, int maxItems=-1)
    {
        ShowInventoryArray(inventory.blocks, screenOffset, nRows, maxItems);
        if (inventory.resultBlocks != null)
        {
            ShowInventoryArray(inventory.resultBlocks, resultOffset, nRows:1, maxItems:inventory.resultBlocks.Length, numPrev:blockItems.Count);
        }
    }

    void ShowInventoryArray(BlockStack[] blocks, Vector2 offset, int nRows=4, int maxItems=-1, int numPrev=0)
    {
        int numItems = blocks.Length;
        if (maxItems != -1)
        {
            numItems = System.Math.Min(numItems, maxItems);
        }
        while (blockItems.Count- numPrev < numItems)
        {
            blockItems.Add(MakeNewBlockEntity());
        }
        while (blockItems.Count - numPrev > numItems)
        {
            BlockEntity ripU = blockItems[blockItems.Count - 1];
            blockItems.RemoveAt(blockItems.Count - 1);
            GameObject.Destroy(ripU.gameObject);
        }
        int pos = numPrev;
        for (int i = 0; i < nRows; i++)
        {
            int rowLen = numItems / nRows;
            if (i == 0 && numItems % nRows != 0)
            {
                rowLen += numItems % nRows;
            }
            if (nRows == 4 && maxItems == 10)
            {
                if (i < 3)
                {
                    rowLen = 3;
                }
                else
                {
                    rowLen = 1;
                }
            }

            for (int k = 0; k < rowLen; k++)
            {
                actualInventoryWidth = inventoryWidth* rowLen;
                actualInventoryHeight = inventoryHeight * nRows;
                ShowInventoryItem(pos, offset, i, k, blocks[pos-numPrev]);
                pos += 1;
            }
        }
    }

    public Vector2 resultOffset;


    float actualInventoryWidth;
    float actualInventoryHeight;



    public void ShowInventoryItem(int index, Vector2 offset, float rowP, float columnP, BlockStack itemStack)
    {
        // 0 to 1 -> -0.5 to 0.5
        BlockEntity displayItem = blockItems[index];
        float xPos = inventoryWidth * columnP + offset.x;
        float yPos = inventoryHeight * rowP + offset.y;
        xPos -= actualInventoryWidth/2.0f;
        yPos -= actualInventoryHeight/2.0f;
        if (itemStack == null)
        {
            displayItem.blockId = -1;
        }
        else
        {
            displayItem.blockId = itemStack.block;
        }

        if (itemStack != null && itemStack.count > 1)
        {
            displayItem.GetComponentInChildren<UnityEngine.UI.Text>().text = itemStack.count + "";
        }
        else
        {
            displayItem.GetComponentInChildren<UnityEngine.UI.Text>().text = "";
        }
        if (index == selection)
        {
            displayItem.selected = true;
        }
        else
        {
            displayItem.selected = false;
        }
        displayItem.transform.localPosition = Vector3.right * xPos + Vector3.up * yPos;
    }


    bool MouseIntersectsBlockEntity(BlockEntity blockEntity)
    {
        if (blockEntity.GetComponent<UnityEngine.UI.Image>() != null)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(blockEntity.GetComponent<UnityEngine.UI.Image>().rectTransform, Input.mousePosition, playerUsing.mainCamera))
            {
                //Debug.Log("hit " + blockEntity + " " + Input.mousePosition);
                return true;
            }
            else
            {
                //Debug.Log("not hit " + blockEntity + " " + Input.mousePosition);
                return false;
            }
        }
        else
        {
            Debug.Log("rip me");
            return false;
        }
    }


    void ThrowStuff()
    {
        for (int i = 0; i < playerUsing.blocksHoldingWithMouse.count; i++)
        {
            BlockEntity worldEntity = World.mainWorld.CreateBlockEntity(playerUsing.blocksHoldingWithMouse.Block, transform.position + transform.forward * 1.0f);
            worldEntity.timeThrown = Time.time;
            worldEntity.GetComponent<MovingEntity>().SetAbsoluteDesiredMove(playerUsing.transform.forward);
            worldEntity.playerThrowing = playerUsing.GetComponent<MovingEntity>();
            //Debug.Log(playerUsing.transform.forward);
        }
        CallInventoryModifiedCallbacks();

        playerUsing.blocksHoldingWithMouse = null;
        GameObject.Destroy(playerUsing.holdingWithMouseEntity.gameObject);
        playerUsing.holdingWithMouseEntity = null;
    }

    
    /*
    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (displaying)
            {
                player.mouseLook.allowedToCapture = true;
                player.mouseLook.capturing = true;
                if (blocksHoldingWithMouse != null)
                {
                    ThrowStuff();
                }
            }
            else
            {
                player.mouseLook.allowedToCapture = false;
            }
            displaying = !displaying;
        }
        int maxItems = player.inventory.capacity;
        if (!displaying)
        {
            maxItems = 8;
            /*
            for (int i = 0; i < blockItems.Count; i++)
            {
                blockItems[i].GetComponent<UnityEngine.UI.Image>().enabled = false;
                blockItems[i].GetComponentInChildren<UnityEngine.UI.Text>().enabled = false;
            }
            return;
            * /
        }

        for (int i = 0; i < blockItems.Count; i++)
        {
            blockItems[i].GetComponent<UnityEngine.UI.Image>().enabled = true;
            blockItems[i].GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        }

        if (player != null && player.inventory != null)
        {
            if (displaying)
            {
                ShowInventory(3, maxItems: maxItems);
            }
            else
            {
                ShowInventory(1, maxItems: maxItems);
            }
        }


    }
    */
}
