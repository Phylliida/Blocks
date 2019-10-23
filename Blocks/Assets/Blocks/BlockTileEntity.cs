using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class BlockTileEntity : MonoBehaviour
    {

        public BlockStack blockStack;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        int textureBlockId = 0;
        // Update is called once per frame
        void Update()
        {
            if (blockStack != null && blockStack.count > 0 && blockStack.block != BlockValue.Air && blockStack.block != textureBlockId)
            {
                GetComponent<Renderer>().material.mainTexture = BlockValue.allBlocksTexture;
                GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.5f / World.maxAnimFrames, 1.0f / (World.numBlocks * 3.0f));
                GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, (System.Math.Abs(blockStack.block) - 1.0f) * 3.0f / (World.numBlocks * 3.0f));
                textureBlockId = blockStack.block;
            }
        }
    }
}