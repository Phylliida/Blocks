using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class BlockEntity : MonoBehaviour
    {
        public int blockId;
        public string blockName;
        public BlockStack blockStack;
        public BlockStack Stack
        {
            get
            {
                if (blockStack == null)
                {
                    blockStack = new BlockStack(blockId, 1);
                    return blockStack;
                }
                else
                {
                    return blockStack;
                }
            }
            set
            {
                blockStack = value;
                blockId = value.block;
            }
        }
        int displayedBlockId = -1;
        public MovingEntity playerPulling;
        public MovingEntity playerThrowing;
        public MovingEntity movingEntity;
        public float timeThrown = 0.0f;
        float timeUntilCanGrab = 0.15f;
        public float timeSinceSpawned = 0.0f;
        public bool pullable = true;
        public bool selected = false;
        public float pullingSpeed = 0.0f;
        bool initialized = false;
        // Use this for initialization
        void Start()
        {
            initialized = false;
            UpdateTexture();
            timeSinceSpawned = 0.0f;
            if (GetComponent<MovingEntity>() != null)
            {
                GetComponent<MovingEntity>().desiredMove = new Vector3(Random.Range(-0.3f, 0.3f), 0.0f, Random.Range(-0.3f, 0.3f));
                GetComponent<MovingEntity>().SetYVel(Random.Range(4.0f, 4.5f));
            }
            //Update();
        }


        DoEveryMS informStuffBelow = new DoEveryMS(10);



        public bool CanIGrabU(MovingEntity playerAsking)
        {
            if (!enabled)
            {
                return false;
            }
            if (!pullable)
            {
                return false;
            }
            if (playerAsking == playerThrowing)
            {
                return false;
            }
            if  (timeSinceSpawned < timeUntilCanGrab)
            {
                return false;
            }
            if (playerPulling != null && playerPulling != playerAsking)
            {
                return false;
            }
            return true;
        }

        // Update is called once per frame
        void Update()
        {
            if(movingEntity == null)
            {
                movingEntity = GetComponent<MovingEntity>();
            }
            if (playerPulling != null)
            {
                transform.position += movingEntity.GetVel() * Time.deltaTime+movingEntity.desiredMove*Time.deltaTime;
                //movingEntity.SetVel(movingEntity.GetVel() * 0.99f);
            }


            timeSinceSpawned += Time.deltaTime;

            if (blockName != "")
            {
                try
                {

                    blockId = BlockUtils.StringToBlockId(blockName);
                    if (blockStack != null)
                    {
                        blockStack.block = blockId;
                    }
                    blockName = "";
                }
                catch
                {

                }
            }

            if (movingEntity != null && movingEntity.IsTouchingGround())
            {
                movingEntity.desiredMove *= 0.9f;
            }

            if (playerThrowing != null)
            {
                movingEntity.speed = 5.0f;
                if (movingEntity.IsTouchingGround())
                {
                    //movingEntity.desiredMove *= 0.9f;
                }
                if (Time.time - timeThrown > 3.3f)
                {
                    playerThrowing = null;
                }
            }
            if (blockStack == null)
            {
                blockStack = new BlockStack(blockId, 1);
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
                if (movingEntity != null)
                {
                    movingEntity.enabled = false;
                }
            }
            if (playerPulling == null && pullable)
            {
                if (movingEntity != null)
                {
                    movingEntity.enabled = true;
                }
            }


            if (playerPulling == null && movingEntity != null && movingEntity.enabled)
            {
                if (movingEntity.IsTouchingGround() && informStuffBelow.Do())
                {
                    RaycastResults blockStandingOn = movingEntity.BlockStandingOn();
                    //Debug.Log("checking below entity, hit block " + BlockValue.IdToBlockName(blockStandingOn.hitBlock.Block));

                    Blocks.BlockOrItem customBlock;
                    if(World.mainWorld.customBlocks.ContainsKey(blockStandingOn.hitBlock.Block, out customBlock))
                    {
                        //Debug.Log("calling block stack above on it");
                        using (BlockData hitBlockData = World.mainWorld.GetBlockData(blockStandingOn.hitBlock))
                        {
                            BlockStack myStack = Stack;
                            BlockStack resStack;
                            customBlock.BlockStackAbove(hitBlockData, myStack, transform.position, out resStack);

                            if (resStack == null || resStack.count <= 0 || resStack.block == BlockValue.Air)
                            {
                                Destroy(transform.gameObject);
                            }
                            else
                            {
                                Stack = resStack;
                            }
                        }
                    }
                }

            }
            if (GetComponent<UnityEngine.UI.Outline>() != null)
            {
                GetComponent<UnityEngine.UI.Outline>().enabled = selected;
            }

            if (GetComponentInChildren<DurabilityBar>() != null)
            {
                DurabilityBar bar = GetComponentInChildren<DurabilityBar>();
                bar.enabled = blockStack != null && blockStack.maxDurability != 0;
                if (bar.enabled)
                {
                    bar.durability = blockStack.durability/(float)blockStack.maxDurability;
                }
            }

            UpdateTexture();

            if (pullable)
            {
                transform.rotation *= Quaternion.Euler(0, 12.0f * Time.deltaTime, 0);
            }
        }


        public void UpdateTexture()
        {

            if ((displayedBlockId != blockId && !(blockId == -1 && displayedBlockId == (int)BlockValue.Air)) || !initialized)
            {
                initialized = true;
                displayedBlockId = blockId;
                if (transform.GetComponent<MeshFilter>() != null)
                {
                    transform.GetComponent<MeshFilter>().mesh = BlocksWorld.blockMesh;
                }
                if (blockId == -1)
                {
                    displayedBlockId = (int)BlockValue.Air;
                }
                if (transform.GetComponent<UnityEngine.UI.Image>() != null)
                {
                    transform.GetComponent<UnityEngine.UI.Image>().material.mainTextureScale = new Vector2(0.5f / World.maxAnimFrames, 1.0f / (World.numBlocks * 3.0f));
                    transform.GetComponent<UnityEngine.UI.Image>().material.mainTextureOffset = new Vector2(0.0f, (System.Math.Abs(displayedBlockId) - 1.0f) * 3.0f / (World.numBlocks * 3.0f));
                }
                else
                {
                    transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f / World.maxAnimFrames, 1.0f / 3.0f);
                    transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.0f, (System.Math.Abs(displayedBlockId) - 1.0f) * 3.0f / (World.numBlocks * 3.0f));
                }
            }
        }
    }
}