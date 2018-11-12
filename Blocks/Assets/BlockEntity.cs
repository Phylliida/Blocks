using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEntity : MonoBehaviour {



    public int blockId;
    int displayedBlockId = -1;
    public MovingEntity playerPulling;
    public bool pullable = true;
    public bool selected = false;
    bool initialized = false;
	// Use this for initialization
	void Start () {
        initialized = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (playerPulling != null || !pullable)
        {
            if (transform.GetComponent<MovingEntity>() != null)
            {
                transform.GetComponent<MovingEntity>().enabled = false;
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
