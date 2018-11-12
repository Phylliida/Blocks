using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEntity : MonoBehaviour {



    public int blockId;
    public BlockStack blockStack;
    int displayedBlockId = -1;
    public MovingEntity playerPulling;
    public MovingEntity playerThrowing;
    public float timeThrown = 0.0f;
    public bool pullable = true;
    public bool selected = false;
    bool initialized = false;
	// Use this for initialization
	void Start () {
        initialized = false;
    }
	
	// Update is called once per frame
	void Update () {

        if (playerThrowing != null)
        {
            GetComponent<MovingEntity>().speed = 5.0f;
            if (transform.GetComponent<MovingEntity>().IsTouchingGround())
            {
                GetComponent<MovingEntity>().desiredMove *= 0.9f;
            }
            if (Time.time - timeThrown > 3.3f)
            {
                GetComponent<MovingEntity>().desiredMove = Vector3.zero;
                playerThrowing = null;
            }
        }
        if (blockStack != null)
        {
            if (blockStack.count <= 0)
            {
                blockStack = null;
            }
            else
            {
                blockId = blockStack.block;
                if (transform.GetComponentInChildren<UnityEngine.UI.Text>() != null)
                {
                    if (blockStack.count > 1)
                    {
                        transform.GetComponentInChildren<UnityEngine.UI.Text>().text = blockStack.count + "";
                    }
                    else
                    {
                        transform.GetComponentInChildren<UnityEngine.UI.Text>().text = "";
                    }
                }
            }
        }

        if (playerPulling != null || !pullable)
        {
            if (transform.GetComponent<MovingEntity>() != null)
            {
                transform.GetComponent<MovingEntity>().enabled = false;
            }
        }
        if (playerPulling == null && pullable)
        {

            if (transform.GetComponent<MovingEntity>() != null)
            {
                transform.GetComponent<MovingEntity>().enabled = true;
            }
        }
        if (GetComponent<UnityEngine.UI.Outline>() != null)
        {
            GetComponent<UnityEngine.UI.Outline>().enabled = selected;
        }
		if ((displayedBlockId != blockId && !(blockId == -1 && displayedBlockId == World.EMPTY)) || !initialized)
        {
            initialized = true;
            displayedBlockId = blockId;
            if (transform.GetComponent<MeshFilter>() != null)
            {
                transform.GetComponent<MeshFilter>().mesh = BlocksWorld.blockMesh;
            }
            if (blockId == -1)
            {
                displayedBlockId = World.EMPTY;
            }
            if (transform.GetComponent<UnityEngine.UI.Image>() != null)
            {
                transform.GetComponent<UnityEngine.UI.Image>().material.mainTextureScale = new Vector2(0.5f, 1.0f / (World.numBlocks * 3.0f));
                transform.GetComponent<UnityEngine.UI.Image>().material.mainTextureOffset = new Vector2(0.0f, (displayedBlockId - 1.0f) * 3.0f / (World.numBlocks * 3.0f));
            }
            else
            {
                transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f, 1.0f / 3.0f);
                transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, (displayedBlockId - 1.0f) * 3.0f / (World.numBlocks * 3.0f));
            }
        }

        if (pullable)
        {
            transform.rotation *= Quaternion.Euler(0, 12.0f * Time.deltaTime, 0);
        }
    }
}
