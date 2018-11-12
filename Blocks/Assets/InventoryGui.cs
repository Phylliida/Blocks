using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryGui : MonoBehaviour {



    public BlocksPlayer player;
	// Use this for initialization
	void Start () {
      

    }

    BlockEntity MakeNewBlockEntity()
    {
        BlockEntity res = GameObject.Instantiate(World.mainWorld.blocksWorld.blockRenderPrefab).GetComponent<BlockEntity>();
        res.GetComponent<UnityEngine.UI.Image>().material = new Material(res.GetComponent<UnityEngine.UI.Image>().material);
        res.blockId = -1;
        res.pullable = false;
        res.transform.SetParent(World.mainWorld.blocksWorld.blockRenderCanvas.transform);
        res.transform.localScale = new Vector3(10, 10, 10);
        res.transform.localRotation = Quaternion.identity;
        return res;
    }


    void ShowInventory(int nRows=4, int maxItems=-1)
    {
        int numItems = player.inventory.capacity;
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
                ShowInventoryItem(pos, i/(float)Mathf.Max(1, (nRows-1)), k/(float)Mathf.Max(1, (rowLen-1)), player.inventory.blocks[pos]);
                pos += 1;
            }
        }
    }

    public List<BlockEntity> blockItems = new List<BlockEntity>();
    public float inventoryWidth = 2.0f;
    public float inventoryHeight = 2.0f;
    public float inventoryForward = 0.5f;

    float actualInventoryWidth;
    float actualInventoryHeight;

    public int selection = 0;

    public Vector2 inventoryOffset = new Vector2(0, -100.0f);

    public void ShowInventoryItem(int index, float rowP, float columnP, BlockStack itemStack)
    {
        // 0 to 1 -> -0.5 to 0.5
        rowP -= 0.5f;
        columnP -= 0.5f;
        BlockEntity displayItem = blockItems[index];
        float yPos = actualInventoryHeight * rowP + inventoryOffset.y;
        float xPos = actualInventoryWidth * columnP + inventoryOffset.x;
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

    public BlockStack blocksHoldingWithMouse = null;
    public BlockEntity holdingWithMouseEntity = null;

    bool MouseIntersectsBlockEntity(BlockEntity blockEntity)
    {
        if (blockEntity.GetComponent<UnityEngine.UI.Image>() != null)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(blockEntity.GetComponent<UnityEngine.UI.Image>().rectTransform, Input.mousePosition, player.mainCamera);
        }
        else
        {
            return false;
        }
    }

    public bool displaying = false;

    void ThrowStuff()
    {
        for (int i = 0; i < blocksHoldingWithMouse.count; i++)
        {
            BlockEntity worldEntity = World.mainWorld.CreateBlockEntity(blocksHoldingWithMouse.block, transform.position + transform.forward * 1.0f);
            worldEntity.timeThrown = Time.time;
            worldEntity.GetComponent<MovingEntity>().SetAbsoluteDesiredMove(transform.forward);
            worldEntity.playerThrowing = player.GetComponent<MovingEntity>();
            Debug.Log(transform.forward);
        }

        blocksHoldingWithMouse = null;
        GameObject.Destroy(holdingWithMouseEntity.gameObject);
        holdingWithMouseEntity = null;
        player.mouseLook.allowedToCapture = true;
    }
    
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
            */
        }

        for (int i = 0; i < blockItems.Count; i++)
        {
            blockItems[i].GetComponent<UnityEngine.UI.Image>().enabled = true;
            blockItems[i].GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        }

        if (player != null && player.inventory != null)
        {
            inventoryOffset.y = -Screen.height / 2.0f+inventoryHeight*2.0f;
            int maxSelection = Mathf.Max(player.inventory.capacity, maxItems);
            if (Input.GetKeyDown(KeyCode.Alpha1) && 1 <= maxSelection - 1) selection = 1 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) && 2 <= maxSelection - 1) selection = 2 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) && 3 <= maxSelection - 1) selection = 3 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha4) && 4 <= maxSelection - 1) selection = 4 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha5) && 5 <= maxSelection - 1) selection = 5 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha6) && 6 <= maxSelection - 1) selection = 6 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha7) && 7 <= maxSelection - 1) selection = 7 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha8) && 8 <= maxSelection - 1) selection = 8 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha9) && 9 <= maxSelection - 1) selection = 9 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha0) && 10 <= maxSelection - 1) selection = 10 - 1;
            float selectionF = selection;
            if (Input.mouseScrollDelta.y != 0)
            {
                selectionF -= Input.mouseScrollDelta.y;
            }
            selection = Mathf.RoundToInt(selectionF);
            selection = Mathf.Min(maxItems - 1, Mathf.Max(0, selection));
            if (Input.mouseScrollDelta.y != 0)
            {
                Debug.Log(Input.mouseScrollDelta.y + " delta");
            }
            if (displaying)
            {
                ShowInventory(3, maxItems: maxItems);
            }
            else
            {
                ShowInventory(1, maxItems: maxItems);
            }
        }


        // click when not capturing
        if (blocksHoldingWithMouse == null)
        {
            if (Input.GetMouseButtonDown(0) && displaying)
            {
                for (int i = 0; i < blockItems.Count; i++)
                {
                    if (MouseIntersectsBlockEntity(blockItems[i]) && player.inventory.blocks[i] != null)
                    {
                        blocksHoldingWithMouse = player.inventory.blocks[i];
                        holdingWithMouseEntity = MakeNewBlockEntity();
                        holdingWithMouseEntity.blockStack = player.inventory.blocks[i];
                        player.inventory.blocks[i] = null;
                        player.mouseLook.allowedToCapture = false;
                        break;
                    }
                }
            }
        }

        else if (blocksHoldingWithMouse != null && displaying)
        {
            player.mouseLook.allowedToCapture = false;
            Vector3 offset;
            Ray ray = player.mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f));
            holdingWithMouseEntity.transform.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2.0f, Input.mousePosition.y - Screen.height / 2.0f, 0.01f);
            if (Input.GetMouseButtonDown(0))
            {
                bool foundCollision = false;
                for (int i = 0; i < blockItems.Count; i++)
                {
                    if (MouseIntersectsBlockEntity(blockItems[i]))
                    {
                        foundCollision = true;
                        bool letGo = false;
                        if (player.inventory.blocks[i] == null)
                        {
                            player.inventory.blocks[i] = blocksHoldingWithMouse;
                            letGo = true;
                        }
                        else
                        {
                            if (player.inventory.blocks[i].block == blocksHoldingWithMouse.block)
                            {
                                int numMaxStack = 1;
                                if (World.stackableSize.ContainsKey(blocksHoldingWithMouse.block))
                                {
                                    numMaxStack = World.stackableSize[blocksHoldingWithMouse.block];
                                }
                                // can fit some in
                                if (player.inventory.blocks[i].count < numMaxStack)
                                {
                                    // can fit all in
                                    if (player.inventory.blocks[i].count + blocksHoldingWithMouse.count <= numMaxStack)
                                    {
                                        player.inventory.blocks[i].count += blocksHoldingWithMouse.count;
                                        letGo = true;
                                        blocksHoldingWithMouse = null;
                                    }
                                    // can only fit some in
                                    else
                                    {
                                        int numCanFitIn = numMaxStack - player.inventory.blocks[i].count;
                                        player.inventory.blocks[i].count += numCanFitIn;
                                        blocksHoldingWithMouse.count -= numCanFitIn;
                                        if (blocksHoldingWithMouse.count <= 0)
                                        {
                                            Debug.LogError("we should have count > 0 when not putting all into block, got count " + blocksHoldingWithMouse.count + " instead");
                                        }
                                    }
                                }
                                // can't fit any more in, swap
                                else
                                {
                                    player.inventory.blocks[i].count = blocksHoldingWithMouse.count;
                                    blocksHoldingWithMouse.count = numMaxStack;
                                }
                            }
                            // different things, swap
                            else
                            {
                                BlockStack tmp = player.inventory.blocks[i];
                                player.inventory.blocks[i] = blocksHoldingWithMouse;
                                blocksHoldingWithMouse = tmp;
                                holdingWithMouseEntity.blockStack = tmp;
                            }
                        }
                        if (letGo)
                        {
                            blocksHoldingWithMouse = null;
                            GameObject.Destroy(holdingWithMouseEntity.gameObject);
                            holdingWithMouseEntity = null;
                            player.mouseLook.allowedToCapture = true;
                        }
                    }
                }
                // didn't click on anything, threw on ground instead
                if (!foundCollision)
                {
                    ThrowStuff();
                }
            }
        }
    }
}
