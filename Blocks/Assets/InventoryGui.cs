using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryGui : MonoBehaviour {

    public BlocksPlayer playerUsing;

	// Use this for initialization
	void Start () {
      

    }

    public int selection = 0;

    public Inventory inventory;
    public bool displaying = false;

    float inventoryWidth = 100.0f;
    float inventoryHeight = 100.0f;

    public int numRows;
    public int maxItems = -1;


    public Vector2 screenOffset = new Vector2(0, -100.0f);

    public List<BlockEntity> blockItems = new List<BlockEntity>();


    private void Update()
    {

        if (!displaying || playerUsing == null)
        {
            for (int i = 0; i < blockItems.Count; i++)
            {
                GameObject.Destroy(blockItems[i].gameObject);
            }
            blockItems.Clear();
            return;
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
            selection = maxItems;
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

        // click when not capturing
        if (playerUsing.blocksHoldingWithMouse == null)
        {
            if (Input.GetMouseButtonDown(0) && displaying)
            {
                for (int i = 0; i < blockItems.Count; i++)
                {
                    Debug.Log("trying "  + i + " " + inventory.blocks[i] + " " + name);
                    if (MouseIntersectsBlockEntity(blockItems[i]) && inventory.blocks[i] != null)
                    {
                        Debug.Log("got dat boi");
                        playerUsing.blocksHoldingWithMouse = inventory.blocks[i];
                        playerUsing.holdingWithMouseEntity = MakeNewBlockEntity();
                        playerUsing.holdingWithMouseEntity.blockStack = inventory.blocks[i];
                        inventory.blocks[i] = null;
                        //playerUsing.mouseLook.allowedToCapture = false;
                        break;
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
                for (int i = 0; i < blockItems.Count; i++)
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
                                            letGo = true;
                                            playerUsing.blocksHoldingWithMouse = null;
                                        }
                                        // put 1 in and leave rest of stack in hand
                                        else
                                        {
                                            inventory.blocks[i].count += 1;
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
                                            letGo = true;
                                            playerUsing.blocksHoldingWithMouse = null;
                                        }
                                        // can only fit some in
                                        else
                                        {
                                            int numCanFitIn = numMaxStack - inventory.blocks[i].count;
                                            inventory.blocks[i].count += numCanFitIn;
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
                                        playerUsing.blocksHoldingWithMouse.count = numMaxStack;
                                    }

                                }
                            }
                            // different things, swap
                            else if (Input.GetMouseButtonDown(0))
                            {
                                BlockStack tmp = inventory.blocks[i];
                                inventory.blocks[i] = playerUsing.blocksHoldingWithMouse;
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
        int numItems = inventory.capacity;
        if (maxItems != -1)
        {
            numItems = System.Math.Min(numItems, maxItems);
        }
        while (blockItems.Count < numItems)
        {
            blockItems.Add(MakeNewBlockEntity());
        }
        while (blockItems.Count > numItems)
        {
            BlockEntity ripU = blockItems[blockItems.Count - 1];
            blockItems.RemoveAt(blockItems.Count - 1);
            GameObject.Destroy(ripU.gameObject);
        }

        int pos = 0;
        for (int i = 0; i < nRows; i++)
        {
            int rowLen = numItems / nRows;
            if (i == 0 && numItems % nRows != 0)
            {
                rowLen += numItems % nRows;
            }

            for (int k = 0; k < rowLen; k++)
            {
                actualInventoryWidth = inventoryWidth * rowLen;
                actualInventoryHeight = inventoryHeight * nRows;
                ShowInventoryItem(pos, i/(float)Mathf.Max(1, (nRows-1)), k/(float)Mathf.Max(1, (rowLen-1)), inventory.blocks[pos]);
                pos += 1;
            }
        }
    }


    float actualInventoryWidth;
    float actualInventoryHeight;



    public void ShowInventoryItem(int index, float rowP, float columnP, BlockStack itemStack)
    {
        // 0 to 1 -> -0.5 to 0.5
        rowP -= 0.5f;
        columnP -= 0.5f;
        BlockEntity displayItem = blockItems[index];
        float xPos = actualInventoryWidth * columnP + screenOffset.x;
        float yPos = actualInventoryHeight * rowP + screenOffset.y;
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
        Debug.Log("moved to " + displayItem.transform.localPosition + " " + name);
    }


    bool MouseIntersectsBlockEntity(BlockEntity blockEntity)
    {
        if (blockEntity.GetComponent<UnityEngine.UI.Image>() != null)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(blockEntity.GetComponent<UnityEngine.UI.Image>().rectTransform, Input.mousePosition, playerUsing.mainCamera))
            {
                Debug.Log("hit " + blockEntity + " " + Input.mousePosition);
                return true;
            }
            else
            {
                Debug.Log("not hit " + blockEntity + " " + Input.mousePosition);
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
            BlockEntity worldEntity = World.mainWorld.CreateBlockEntity(playerUsing.blocksHoldingWithMouse.block, transform.position + transform.forward * 1.0f);
            worldEntity.timeThrown = Time.time;
            worldEntity.GetComponent<MovingEntity>().SetAbsoluteDesiredMove(playerUsing.transform.forward);
            worldEntity.playerThrowing = playerUsing.GetComponent<MovingEntity>();
            Debug.Log(playerUsing.transform.forward);
        }

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
