using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryGui : MonoBehaviour {



    public BlocksPlayer player;
	// Use this for initialization
	void Start () {
      

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
            blockItems.Add(GameObject.Instantiate(World.mainWorld.blocksWorld.blockRenderPrefab).GetComponent<BlockEntity>());
            blockItems[blockItems.Count - 1].GetComponent<UnityEngine.UI.Image>().material = new Material(blockItems[blockItems.Count - 1].GetComponent<UnityEngine.UI.Image>().material);
            blockItems[blockItems.Count - 1].blockId = -1;
            blockItems[blockItems.Count - 1].pullable = false;
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
        displayItem.transform.SetParent(World.mainWorld.blocksWorld.blockRenderCanvas.transform);
        if (itemStack != null && itemStack.count > 1)
        {
            displayItem.GetComponentInChildren<UnityEngine.UI.Text>().text = itemStack.count + "";
        }
        else
        {
            displayItem.GetComponentInChildren<UnityEngine.UI.Text>().text = "";
        }
        displayItem.transform.localScale = new Vector3(10, 10, 10);
        displayItem.transform.localPosition = Vector3.right * xPos + Vector3.up * yPos;
        if (index == selection)
        {
            displayItem.selected = true;
        }
        else
        {
            displayItem.selected = false;
        }
    }
	// Update is called once per frame
	void Update () {



        if (player != null && player.inventory != null)
        {
            inventoryOffset.y = -Screen.height / 2.0f+inventoryHeight*2.0f;
            int maxSelection = player.inventory.capacity;
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
            selection = Mathf.Min(player.inventory.capacity - 1, Mathf.Max(0, selection));
            if (Input.mouseScrollDelta.y != 0)
            {
                Debug.Log(Input.mouseScrollDelta.y + " delta");
            }
            ShowInventory(1);
        }
    }
}
