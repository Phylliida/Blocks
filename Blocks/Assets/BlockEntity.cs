using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEntity : MonoBehaviour {



    public int blockId;
    int displayedBlockId = -1;
    public MovingEntity playerPulling;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (playerPulling != null)
        {
            transform.GetComponent<MovingEntity>().enabled = false;
        }
		if (displayedBlockId != blockId)
        {
            transform.GetComponent<MeshFilter>().mesh = BlocksWorld.blockMesh;
            transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f, 1.0f/3.0f);
            transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, (blockId-1.0f)*3.0f / (World.numBlocks * 3.0f));
            displayedBlockId = blockId;
        }
        transform.rotation *= Quaternion.Euler(0, 12.0f * Time.deltaTime, 0);
    }
}
