using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{

    public class BlocksPlayer : MonoBehaviour
    {
        public SmoothMouseLook mouseLook;
        public MovingEntity body;
        public Camera mainCamera;
        public Inventory inventory
        {
            get
            {
                return body.inventory;
            }
            set
            {
                body.inventory = value;
            }
        }
        public InventoryGui inventoryGui;
        public float reachRange = 6.0f;

        
        public BlockStack blocksHoldingWithMouse = null;
        public BlockEntity holdingWithMouseEntity = null;
        int blockPlacing;


        bool showingHotbarOnly = true;
        int hotbarSize = 8;
        public void Start()
        {
            body.inventorySize = 16;
            body.inventory = new Inventory(body.inventorySize);
            inventoryGui.playerUsing = this;
            inventoryGui.inventory = inventory;
            inventoryGui.displaying = true;
        }

        public bool paused = false;
        public float timeBreaking = 0.0f;

        LVector3 curBlockBreaking;
        int curSelectionBreakingWith = -1;
        LVector3 chunkPos;
        LVector3 startPathingPos;

        public int redstoneState = 8;

        public void Update()
        {
            /*
            float modAmount = World.mainWorld.chunkSize*World.mainWorld.blocksWorld.worldScale*8.0f;
            transform.position = new Vector3(
                PhysicsUtils.fmod(transform.position.x, modAmount),
                PhysicsUtils.fmod(transform.position.y, modAmount),
                PhysicsUtils.fmod(transform.position.z, modAmount));
            */


            if (Input.GetKeyDown(KeyCode.O))
            {
                startPathingPos = LVector3.FromUnityVector3(transform.position);
                Debug.Log("start pathing pos set to " + startPathingPos);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                LVector3 endPathingPos = LVector3.FromUnityVector3(transform.position);
                Debug.Log("trying to do pathing, starting from position " + startPathingPos + " and going to position " + endPathingPos);

                bool pathingSuccess;
                PathingChunk.Pathfind(World.mainWorld, startPathingPos, endPathingPos, new MobilityCriteria(1,1,2,1), out pathingSuccess, verbose: World.mainWorld.blocksWorld.verbosePathing);

            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                LVector3 chunkPos;
                World.mainWorld.GetChunkCoordinatesAtPos(LVector3.FromUnityVector3(transform.position), out chunkPos);
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        for (int z = -2; z <= 2; z++)
                        {
                            Chunk spooker = World.mainWorld.GetChunk(chunkPos.x + x, chunkPos.y + y, chunkPos.z + z);
                            if (spooker != null)
                            {
                                //spooker.GetPathingChunk(1, 1, 2, 1).ResetData();
                            }
                        }
                    }
                }

                MobilityCriteria mobilityCriteria = new MobilityCriteria(1,1,2,1);
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        for (int z = -2; z <= 2; z++)
                        {
                            Chunk spooker = World.mainWorld.GetChunk(chunkPos.x + x, chunkPos.y + y, chunkPos.z + z);
                            if (spooker != null)
                            {
                                spooker.GetPathingChunk(mobilityCriteria).Refresh();
                                spooker.GetPathingChunk(mobilityCriteria).ExpandWalls();
                            }
                        }
                    }
                }
                //PathingNode node = World.mainWorld.GetChunkAtPos(LVector3.FromUnityVector3(transform.position)).GetPathingNode(1, 1, 2, 1, verbose: true);
                //node.Refresh();
                //node.ExpandWalls();
            }

            LVector3 curChunkPos;
            World.mainWorld.GetChunkCoordinatesAtPos(LVector3.FromUnityVector3(transform.position), out curChunkPos);
            if (curChunkPos != chunkPos && Time.frameCount > 100)
            {
                int viewDist = 1;
                World.mainWorld.playerMovedChunks = true;

                for (int i = -viewDist; i <= viewDist; i++)
                {
                    for (int j = viewDist; j >= -viewDist; j--)
                    {
                        for (int k = -viewDist; k <= viewDist; k++)
                        {
                            Chunk prev = World.mainWorld.GetOrGenerateChunk(chunkPos.x + i, chunkPos.y + j, chunkPos.z + k);
                            prev.mustRenderMe = false;
                        }
                    }
                }

                chunkPos = curChunkPos;
                for (int i = -viewDist; i <= viewDist; i++)
                {
                    for (int j = viewDist; j >= -viewDist; j--)
                    {
                        for (int k = -viewDist; k <= viewDist; k++)
                        {
                            Chunk blah = World.mainWorld.GetOrGenerateChunk(chunkPos.x + i, chunkPos.y + j, chunkPos.z + k);
                            blah.mustRenderMe = true;
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = true;
            }
            if (paused)
            {
                mouseLook.allowedToCapture = false;
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    paused = false;
                    mouseLook.allowedToCapture = true;
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!showingHotbarOnly)
                {
                    World.mainWorld.blocksWorld.otherObjectInventoryGui.displaying = false;
                    World.mainWorld.blocksWorld.otherObjectInventoryGui.playerUsing = null;
                }
                showingHotbarOnly = !showingHotbarOnly;
            }


            if (showingHotbarOnly)
            {
                World.mainWorld.blocksWorld.otherObjectInventoryGui.displaying = false;
                mouseLook.allowedToCapture = true;
                mouseLook.capturing = true;
                inventoryGui.numRows = 1;
                inventoryGui.maxItems = hotbarSize;
                inventoryGui.screenOffset.y = -Screen.height / 2.0f + 100.0f;
            }
            else
            {
                World.mainWorld.blocksWorld.otherObjectInventoryGui.displaying = true;
                mouseLook.allowedToCapture = false;
                inventoryGui.maxItems = -1;
                inventoryGui.numRows = 4;
                inventoryGui.screenOffset.y = -Screen.height / 2.0f + 300.0f;
            }

            int maxSelection = Mathf.Max(inventory.capacity, inventoryGui.maxItems);
            if (Input.GetKeyDown(KeyCode.Alpha1) && 1 <= maxSelection - 1) inventoryGui.selection = 1 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) && 2 <= maxSelection - 1) inventoryGui.selection = 2 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) && 3 <= maxSelection - 1) inventoryGui.selection = 3 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha4) && 4 <= maxSelection - 1) inventoryGui.selection = 4 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha5) && 5 <= maxSelection - 1) inventoryGui.selection = 5 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha6) && 6 <= maxSelection - 1) inventoryGui.selection = 6 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha7) && 7 <= maxSelection - 1) inventoryGui.selection = 7 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha8) && 8 <= maxSelection - 1) inventoryGui.selection = 8 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha9) && 9 <= maxSelection - 1) inventoryGui.selection = 9 - 1;
            if (Input.GetKeyDown(KeyCode.Alpha0) && 10 <= maxSelection - 1) inventoryGui.selection = 10 - 1;
            float selectionF = inventoryGui.selection;
            if (Input.mouseScrollDelta.y != 0)
            {
                selectionF -= Input.mouseScrollDelta.y;
            }

            inventoryGui.selection = Mathf.RoundToInt(selectionF);


            if (Input.mouseScrollDelta.y != 0)
            {
                Debug.Log(Input.mouseScrollDelta.y + " delta");
            }


            if (Input.GetMouseButtonDown(0))
            {
                timeBreaking = 0.0f;
            }
            if (Input.GetMouseButtonUp(0))
            {
                timeBreaking = 0.0f;
            }
            if (Input.GetMouseButton(0) && showingHotbarOnly)
            {
                RaycastResults hitResults;

                if (PhysicsUtils.MouseCast(mainCamera, 0.1f, reachRange * World.mainWorld.worldScale, out hitResults))
                {
                    if (curBlockBreaking != hitResults.hitBlock)
                    {
                        timeBreaking = 0.0f;
                    }
                    if (curSelectionBreakingWith != inventoryGui.selection)
                    {
                        timeBreaking = 0.0f;
                        curSelectionBreakingWith = inventoryGui.selection;
                    }
                    curBlockBreaking = hitResults.hitBlock;
                    timeBreaking += Time.deltaTime;
                    if (World.mainWorld.blocksWorld.RenderBlockBreaking(hitResults.hitBlock.x, hitResults.hitBlock.y, hitResults.hitBlock.z, hitResults.hitBlock.BlockV, timeBreaking, inventory.blocks[inventoryGui.selection]))
                    {
                        timeBreaking = 0.0f;
                        //Debug.Log("hit at pos " + hitPos);

                        BlockStack currentItem = inventory.blocks[inventoryGui.selection];
                        if (currentItem != null && currentItem.maxDurability != 0)
                        {
                            currentItem.durability -= 1;
                            if (currentItem.durability == 0)
                            {
                                inventory.blocks[inventoryGui.selection] = null;
                            }
                        }

                        if (World.mainWorld.DropBlockOnDestroy(hitResults.hitBlock.BlockV, hitResults.hitBlock, inventory.blocks[inventoryGui.selection], hitResults.hitBlock.BlockCentertoUnityVector3(), hitResults.blockBeforeHit.BlockCentertoUnityVector3()))
                        {
                            World.mainWorld[hitResults.hitBlock] = 0;
                        }
                    }
                }
                else
                {
                    timeBreaking = 0.0f;
                    //Debug.Log("mouse cast failed " + hitPos);
                }
            }


            if (Input.GetMouseButtonDown(2) && showingHotbarOnly)
            {
                RaycastResults hitResults;

                if (PhysicsUtils.MouseCast(mainCamera, 0.1f, reachRange * World.mainWorld.worldScale, out hitResults))
                {
                    using (BlockData middleClickedOnBlock = World.mainWorld.GetBlockData(hitResults.hitBlock.x, hitResults.hitBlock.y, hitResults.hitBlock.z))
                    {
                        if (middleClickedOnBlock.block == Example.Redstone)
                        {
                            middleClickedOnBlock.connectivityFlags = redstoneState;
                        }
                        else
                        {
                            int rotAngle = 90 * (int)middleClickedOnBlock.rotation;
                            int oldAngle = rotAngle;
                            rotAngle = (rotAngle + 90) % 360;
                            middleClickedOnBlock.rotation = (BlockData.BlockRotation)(rotAngle / 90);
                            //Debug.Log("rotating spooker from old rotation of " + oldAngle + " and spooker of " + hitResults.hitBlock + " to new rotation of " + rotAngle + " which is actually " + (BlockData.BlockRotation)rotAngle);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonDown(1) && showingHotbarOnly)
            {
                //Debug.Log("made click");


                RaycastResults hitResults;

                if (PhysicsUtils.MouseCast(mainCamera, 0.1f, reachRange * World.mainWorld.worldScale, out hitResults))
                {
                    // don't let you place a block in yourself
                    LVector3 myPos = LVector3.FromUnityVector3(transform.position);
                    LVector3 myFeetPos = LVector3.FromUnityVector3(transform.position + new Vector3(0, -body.heightBelowHead + 0.02f, 0));
                    LVector3 myBodyPos = LVector3.FromUnityVector3(transform.position + new Vector3(0, -body.heightBelowHead / 2.0f, 0));
                    LVector3 myHeadPos = LVector3.FromUnityVector3(transform.position + new Vector3(0, body.heightAboveHead, 0));
                    Inventory blockInventory;
                    if (showingHotbarOnly && World.mainWorld.BlockHasInventory(hitResults.hitBlock, out blockInventory))
                    {
                        /*
                        if (World.mainWorld.blocksWorld.blockInventories.ContainsKey(hitResults.hitBlock))
                        {
                            blockInventory = World.mainWorld.blocksWorld.blockInventories[hitResults.hitBlock];
                        }
                        else
                        {
                            blockInventory = new Inventory(9);
                            blockInventory.resultBlocks = new BlockStack[1];
                            World.mainWorld.blocksWorld.blockInventories[hitResults.hitBlock] = blockInventory;
                        }
                        */
                        World.mainWorld.blocksWorld.otherObjectInventoryGui.playerUsing = this;
                        World.mainWorld.blocksWorld.otherObjectInventoryGui.displaying = true;
                        showingHotbarOnly = false;
                        mouseLook.allowedToCapture = false;
                        World.mainWorld.blocksWorld.otherObjectInventoryGui.inventory = blockInventory;
                        World.mainWorld.blocksWorld.otherObjectInventoryGui.screenOffset = new Vector2(0, 300);
                        World.mainWorld.blocksWorld.otherObjectInventoryGui.displaying = true;
                    }
                    else if (inventoryGui != null && inventory != null && inventory.blocks[inventoryGui.selection] != null && inventory.blocks[inventoryGui.selection].count > 0)
                    {
                        if (hitResults.blockBeforeHit != myPos && hitResults.blockBeforeHit != myFeetPos && hitResults.blockBeforeHit != myHeadPos && hitResults.blockBeforeHit != myBodyPos)
                        {
                            if (World.mainWorld.AllowedtoPlaceBlock(inventory.blocks[inventoryGui.selection].block, hitResults.axisHitFrom, hitResults.blockBeforeHit))
                            {
                                BlockValue blockPlacing = World.mainWorld.PrePlaceBlock(inventory.blocks[inventoryGui.selection].block, hitResults.blockBeforeHit, hitResults.axisHitFrom);
                                BlockData.BlockRotation rotation = PhysicsUtils.AxisToRotation(hitResults.axisHitFrom);
                                using (BlockData dat = World.mainWorld.GetBlockData(hitResults.blockBeforeHit.x, hitResults.blockBeforeHit.y, hitResults.blockBeforeHit.z))
                                {
                                    dat.block = blockPlacing;
                                    dat.rotation = rotation;
                                }
                                inventory.blocks[inventoryGui.selection].count -= 1;
                                if (inventory.blocks[inventoryGui.selection].count <= 0)
                                {
                                    inventory.blocks[inventoryGui.selection] = null;
                                }
                            }
                        }
                    }


                    //Debug.Log("hit at pos " + hitPos);
                }
                else
                {

                    //Debug.Log("mouse cast failed " + hitPos);
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                body.usingShift = true;
            }
            else
            {
                body.usingShift = false;
            }

            body.jumping = Input.GetKey(KeyCode.Space);


            Vector3 desiredMove = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                desiredMove += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                desiredMove -= Vector3.forward;
            }
            if (Input.GetKey(KeyCode.A))
            {
                desiredMove -= Vector3.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                desiredMove += Vector3.right;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                World.mainWorld.Save("C:/Users/yams/Desktop/yams/prog/unity/Blocks/repo/Blocks/Saves/coolAndGood");
            }


            if (Input.GetKeyDown(KeyCode.L))
            {
                World.mainWorld.Load("C:/Users/yams/Desktop/yams/prog/unity/Blocks/repo/Blocks/Saves/coolAndGood");
            }

            body.SetRelativeDesiredMove(desiredMove);
        }

        public void OnPostRender()
        {
        }
    }


    /*

    public float heightBelowHead = 1.8f;
    public float heightAboveHead = 0.2f;
    public float playerWidth = 0.5f;
    public float feetWidth = 0.1f;
    public float playerSpeed = 3.0f;
    public float gravity = 20.0f;
    public float jumpSpeed = 7.0f;
    public float reachRange = 6.0f;
    public BlocksWorld world;
    public Vector3 vel;

	// Use this for initialization
	void Start () {
        vel = Vector3.zero;

    }

    public Vector3 VectorProjection(Vector3 a, Vector3 b)
    {
        float a1 = Vector3.Dot(a, b.normalized);
        return a1 * b.normalized;
    }

    public Vector3 VectorRejection(Vector3 a, Vector3 b)
    {
        return a - VectorProjection(a, b);
    }

    bool TryToMoveBody(Vector3 offset, out Vector3 hitNormal, out Vector3 recommendedOffset)
    {
        bool hitSomething = false;
        // do 20 points around a circle (basically pretend we are a cylinder, body/head fatter than feet)
        float shortestDist = offset.magnitude;
        Vector3 resPos = transform.position + offset;
        int numSteps = 20;
        hitNormal = new Vector3(1, 1, 1).normalized;
        for (int i = 0; i < numSteps+1; i++)
        {
            float p = i / (numSteps - 1.0f);
            float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
            float zDiff = Mathf.Cos(p * 2 * Mathf.PI);
            // also do a 0-width one in case we are on something very thin (somehow?)
            if (i == numSteps)
            {
                xDiff = 0;
                zDiff = 0;
            }
            Vector3 bodyOffset = (new Vector3(xDiff, 0, zDiff)).normalized * playerWidth;
            Vector3 feetOffset = (new Vector3(xDiff, 0, zDiff)).normalized * feetWidth + (new Vector3(0, -heightBelowHead, 0));
            Vector3 headOffset = (new Vector3(xDiff, 0, zDiff)).normalized * feetWidth + (new Vector3(0, heightAboveHead, 0)); // heads are basically feet? this is probably a bad idea but I don't want more hyperparams and it should work good enough
            LVector3 hitBlock, prevBeforeHitBlock;
            Vector3 hitPos;
            Vector3 normal;
            // does our body run into anything?
            if (RayCast(transform.position + bodyOffset, offset.normalized, out hitBlock, offset.magnitude, out prevBeforeHitBlock, out hitPos, out normal))
            {
                Vector3 curResPos = hitPos - bodyOffset;
                float curDist = Vector3.Distance(curResPos, transform.position);
                if (curDist < shortestDist)
                {
                    shortestDist = curDist;
                    resPos = curResPos;
                    hitNormal = normal;
                    hitSomething = true;
                }
            }

            // does our feet run into anything?
            if (RayCast(transform.position + feetOffset, offset.normalized, out hitBlock, offset.magnitude, out prevBeforeHitBlock, out hitPos, out normal))
            {
                Vector3 curResPos = hitPos - feetOffset;
                float curDist = Vector3.Distance(curResPos, transform.position);
                if (curDist < shortestDist)
                {
                    shortestDist = curDist;
                    resPos = curResPos;
                    hitNormal = normal;
                    hitSomething = true;
                }
            }

            // does our head run into anything?
            if (RayCast(transform.position + headOffset, offset.normalized, out hitBlock, offset.magnitude, out prevBeforeHitBlock, out hitPos, out normal))
            {
                Vector3 curResPos = hitPos - headOffset;
                float curDist = Vector3.Distance(curResPos, transform.position);
                if (curDist < shortestDist)
                {
                    shortestDist = curDist;
                    resPos = curResPos;
                    hitNormal = normal;
                    hitSomething = true;
                }
            }



            / *
            if (Vector3.Dot(desiredOffset, curOffset) > 0) // if that offset is in the same direction we are going, test to see if it moves us into a block
            {
                if (IntersectingBodyExceptFeet(transform.position + curOffset))
                {
                    // vector rejection
                    goodOffset = goodOffset - Vector3.Project(goodOffset, curOffset);
                }
            }
            * /
}

        Vector3 resOffset = resPos - transform.position;
        recommendedOffset = resOffset;
        //Debug.Log("moving shortest dist " + shortestDist + " with offset " + resOffset + " actual desired dist was " + offset.magnitude + " with offset " + offset);
        return hitSomething;
    }

    bool IntersectingBody(Vector3 desiredOffset, out Vector3 goodOffset)
    {
        goodOffset = desiredOffset;
        for (int i = 0; i < 20; i++)
        {
            float p = i / (20 - 1.0f);
            float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
            float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
            Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized* playerWidth;
            if (Vector3.Dot(desiredOffset, curOffset) > 0) // if that offset is in the same direction we are going, test to see if it moves us into a block
            {
                if (IntersectingBodyExceptFeet(transform.position + curOffset))
                {
                    // vector rejection
                    goodOffset = goodOffset - Vector3.Project(goodOffset, curOffset);
                }
            }
        }
        if (Vector3.Dot(goodOffset, desiredOffset) <= 0.0001f)
        {
            return true;
        }
        return false;
    }

    bool TouchingLand()
    {
        if (TouchingLand(transform.position, true))
        {
            return true;
        }
        for (int i = 0; i < 20; i++)
        {
            float p = i / (20 - 1.0f);
            float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
            float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
            Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized* feetWidth;
            if (TouchingLand(transform.position + curOffset, false))
            {
                return true;
            }
        }
        return false;
    }


    bool IntersectingHead(Vector3 position)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.9f) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        return atHead != 0 || atEyes != 0;

    }
    bool IntersectingBodyExceptFeet(Vector3 position)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.9f) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        long bodyX = (long)Mathf.Floor(position.x / world.worldScale);
        long bodyY = (long)Mathf.Floor((position.y + heightAboveHead/2.0f) / world.worldScale);
        long bodyZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        int atBody = world.world[bodyX, bodyY, bodyZ];
        return atHead != 0 || atEyes != 0 || atFeet != 0 || atBody != 0;
    }


    bool TouchingLand(Vector3 position, bool moveFeet)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        Vector3 resPos = transform.position/world.worldScale;
        bool touching = false;
        if (atFeet != 0)
        {
            //if (moveFeet) { resPos.y = feetY + 1 + heightBelowHead / world.worldScale; }
            touching = true;

            if (moveFeet)
            {
                transform.position = resPos * world.worldScale;
            }
        }
        return touching;

    }


    // Works by hopping to nearest plane in dir, then looking at block inside midpoint between cur and next. If a block is there, we use the step before cur to determine direction (unless first step, in which case step before next should be right)
    bool RayCast(Vector3 origin, Vector3 dir, out LVector3 hitPos, float maxDist, out LVector3 posBeforeHit, out Vector3 surfaceHitPos, out Vector3 normal, int maxSteps=10)
    {

        long camX = (long)Mathf.Floor(origin.x / world.worldScale);
        long camY = (long)Mathf.Floor(origin.y / world.worldScale);
        long camZ = (long)Mathf.Floor(origin.z / world.worldScale);
        int prevMinD = 0;
        // start slightly back in case we are very very close to a block
        float[] curPosF = new float[] { origin.x, origin.y, origin.z };
        LVector3 curPosL = LVector3.FromUnityVector3(world, new Vector3(curPosF[0], curPosF[1], curPosF[2]));
        float[] rayDir = new float[] { dir.x, dir.y, dir.z };
        hitPos = new LVector3(camX, camY, camZ);
        surfaceHitPos = new Vector3(origin.x, origin.y, origin.z);
        posBeforeHit = new LVector3(camX, camY, camZ);
        normal = new Vector3(1, 1, 1).normalized;
        if (maxSteps == -1)
        {
            maxSteps = 10000;
        }
        for (int i = 0; i < maxSteps; i++)
        {
            float minT = float.MaxValue;
            int minD = -1;

            // find first plane we will hit
            for (int d = 0; d < curPosF.Length; d++)
            {
                // call velocity = rayDir[d]
                // dist = velocity*time
                // time = dist/velocity
                if (rayDir[d] != 0)
                {
                    int offsetSign;
                    float nextWall;
                    if (rayDir[d] > 0)
                    {
                        nextWall = Mathf.Ceil(curPosF[d] / world.worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d] / world.worldScale) < 0.0001f)
                        {
                            nextWall += 1.0f;
                        }
                        offsetSign = 1;
                    }
                    else
                    {
                        nextWall = Mathf.Floor(curPosF[d] / world.worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d] / world.worldScale) < 0.0001f)
                        {
                            nextWall -= 1.0f;
                        }
                        offsetSign = -1;
                    }
                    long dest;
                    long offset;
                    / *
                    // if on first step, try just going to the next face without an offset if it is ahead
                    if (Mathf.Sign(curPosL[d] - curPosF[d] / world.worldScale) == offsetSign && i == 0)
                    {
                        dest = curPosL[d];
                    }
                    * /


                    //float nearestPointFive = Mathf.Round(curPosF[d]/world.worldScale) + offsetSign * 0.5f;
                    //float dist = Mathf.Abs(curPosF[d] / world.worldScale - nearestPointFive);
                    //float dist = Mathf.Abs(curPosF[d] / world.worldScale - dest);
                    float dist = Mathf.Abs(curPosF[d] / world.worldScale - nextWall);
                    float tToPlane = dist / Mathf.Abs(rayDir[d]);
                    if (tToPlane > maxDist) // too far
                    {
                        continue;
                    }

                    //Debug.Log("testing with tToPlane = " + tToPlane + " and camPos=" + curPosF[d] + " and dest=" + nextWall + " and dist=" + dist + " and hitPos = " + hitPos + " and minT = " + minT + " and curPosL = " + curPosL  + " and rayDir[d] = " + rayDir[d]);
                    // if tToX (actual dist) is less than furthest block hit so far, this might be a good point, check if there is a block there
                    if (tToPlane < minT)
                    {
                        minT = tToPlane;
                        minD = d;
                        if (i == 0)
                        {
                            prevMinD = minD;
                        }
                    }
                }
            }
            if (minT > maxDist)
            {
                //Debug.Log("too big " + minT + " greater than maxDist " + maxDist);
                return false;
            }
            // step towards it and check if block
            Vector3 prevPosF = new Vector3(curPosF[0], curPosF[1], curPosF[2]);
            Vector3 resPosF = prevPosF + dir * minT * world.worldScale;
            Vector3 midPoint = (prevPosF + resPosF) / 2.0f;
            LVector3 newCurPosL = LVector3.FromUnityVector3(world, midPoint);
            //Debug.Log("stepped from " + curPosL + " and cur pos (" + curPosF[0] + "," + curPosF[1] + "," + curPosF[2] + ") to point " + newCurPosL + " and cur pos " + resPosF);
            curPosL = LVector3.FromUnityVector3(world, resPosF);

            if (Vector3.Distance(prevPosF, origin) > maxDist) // too far
            {
                return false;
            }

            if (world.world[newCurPosL.x, newCurPosL.y, newCurPosL.z] != 0)
            {
                hitPos = newCurPosL;
                surfaceHitPos = prevPosF;
                posBeforeHit = new LVector3(newCurPosL.x, newCurPosL.y, newCurPosL.z);
                posBeforeHit[prevMinD] -= (long)Mathf.Sign(rayDir[prevMinD]);
                float[] normalArr = new float[] { 0, 0, 0 };
                normalArr[prevMinD] = -Mathf.Sign(rayDir[prevMinD]);
                if (Mathf.Sign(rayDir[prevMinD]) == 0)
                {
                    Debug.Log("spooky why u 0");
                    normalArr[prevMinD] = 0.0f;
                }
                normal = new Vector3(normalArr[0], normalArr[1], normalArr[2]);
                return true;
            }
            prevMinD = minD;
            curPosF[0] = resPosF.x;
            curPosF[1] = resPosF.y;
            curPosF[2] = resPosF.z;
        }
        return false;
    }

    bool MouseCast(out LVector3 hitPos, float maxDist, out LVector3 posBeforeHit, out Vector3 surfaceHitPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.01f));

        Vector3 origin = ray.origin + ray.direction * -playerWidth / 2.0f;
        Vector3 normal;
        return RayCast(origin, ray.direction, out hitPos, maxDist, out posBeforeHit, out surfaceHitPos, out normal);
    }

    bool MouseCastOldBad(out LVector3 hitPos, float maxDist, out LVector3 posBeforeHit)
    {

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.01f));

        long camX = (long)Mathf.Floor(ray.origin.x / world.worldScale);
        long camY = (long)Mathf.Floor(ray.origin.y / world.worldScale);
        long camZ = (long)Mathf.Floor(ray.origin.z / world.worldScale);

        float[] camPosF = new float[] { ray.origin.x, ray.origin.y, ray.origin.z };
        float[] rayDir = new float[] { ray.direction.x, ray.direction.y, ray.direction.z };
        LVector3 camPosL = new LVector3(camX, camY, camZ);
        hitPos = new LVector3(camX, camY, camZ);
        posBeforeHit = new LVector3(camX, camY, camZ);
        float minT = float.MaxValue;

        int maxSteps = 10;
        // do raycast for each dim (plane at x+1, x+2, ..., then plane at y+1, y+2, ..., etc.) and find nearest hit that has a block
        for (int d = 0; d < camPosF.Length; d++)
        {
            // call velocity = rayDir[d]
            // dist = velocity*time
            // time = dist/velocity
            if (rayDir[d] != 0)
            {
                int offsetSign;
                if (rayDir[d] > 0)
                {
                    offsetSign = 1;
                }
                else
                {
                    offsetSign = -1;
                }
                for (int i = 0; i < maxSteps; i++)
                {
                    long dest;
                    long offset;
                    dest = camPosL[d] + i * offsetSign;
                    float dist = Mathf.Abs(camPosF[d] / world.worldScale - dest);
                    float tToPlane = dist / Mathf.Abs(rayDir[d]);
                    tToPlane += world.worldScale * 0.1f; // go a little further because of rounding issues
                    if (tToPlane > maxDist) // too far
                    {
                        break;
                    }

                    Debug.Log("testing with tToPlane = " + tToPlane + " and camPos=" + ray.origin + " and dest=" + dest +" and dist=" + dist + " and hitPos = " + hitPos + " and minT = " + minT + " and camPosL = " + camPosL + " and rayDir[d] = " + rayDir[d]);
                    // if tToX (actual dist) is less than furthest block hit so far, this might be a good point, check if there is a block there
                    if (tToPlane < minT)
                    {
                        Vector3 resPtF = (ray.direction * tToPlane + ray.origin/ world.worldScale);
                        LVector3 resPt = new LVector3((long)Mathf.Floor(resPtF.x), (long)Mathf.Floor(resPtF.y), (long)Mathf.Floor(resPtF.z));
                        Debug.Log("smaller at res pt " + resPt + " and resPtF " + resPtF);
                        if (resPt == camPosL)
                        {
                            continue;
                        }
                        if (world.world[resPt.x, resPt.y, resPt.z] != 0) // if there is a block there
                        {
                            Debug.Log("hit at res pt " + resPt + " and resPtF " + resPtF + " and dim " + d);
                            // store this as closest hit so far
                            minT = tToPlane;
                            hitPos = resPt;
                            posBeforeHit = new LVector3(resPt.x, resPt.y, resPt.z);
                            posBeforeHit[d] -= offsetSign;
                            // break and move onto next dim since in this dim we are closest
                            break;
                        }
                    }
                }
            }
        }

        bool didHit;
        if (minT != float.MaxValue)
        {
            didHit = true;
        }
        else
        {
            didHit = false;
        }
        return didHit;
    }

    int blockPlacing = World.GRASS;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && mouseLook.prevCapturing)
        {
            //Debug.Log("made click");

            LVector3 hitPos;
            LVector3 posBeforeHit;
            Vector3 surfaceHitPos;

            if (MouseCast(out hitPos, reachRange * world.worldScale, out posBeforeHit, out surfaceHitPos))
            {
                //Debug.Log("hit at pos " + hitPos);
                world.world[hitPos.x, hitPos.y, hitPos.z] = 0;
            }
            else
            {

                //Debug.Log("mouse cast failed " + hitPos);
            }
        }

        if (Input.GetMouseButtonDown(1) && mouseLook.prevCapturing)
        {
            //Debug.Log("made click");

            LVector3 hitPos;
            LVector3 posBeforeHit;
            Vector3 surfaceHitPos;

            if (MouseCast(out hitPos, reachRange * world.worldScale, out posBeforeHit, out surfaceHitPos))
            {
                // don't let you place a block in yourself
                LVector3 myPos = LVector3.FromUnityVector3(world, transform.position);
                LVector3 myFeetPos = LVector3.FromUnityVector3(world, transform.position + new Vector3(0, -heightBelowHead+0.02f, 0));
                LVector3 myBodyPos = LVector3.FromUnityVector3(world, transform.position + new Vector3(0, -heightBelowHead/2.0f, 0));
                LVector3 myHeadPos = LVector3.FromUnityVector3(world, transform.position + new Vector3(0, heightAboveHead, 0));
                if (posBeforeHit != myPos && posBeforeHit != myFeetPos && posBeforeHit != myHeadPos && posBeforeHit != myBodyPos)
                {
                    Debug.Log((myPos == posBeforeHit) + " " + (myFeetPos == posBeforeHit) + " " + (myHeadPos == posBeforeHit) + " " + (myBodyPos == posBeforeHit));
                    world.world[posBeforeHit.x, posBeforeHit.y, posBeforeHit.z] = blockPlacing;
                }
                //Debug.Log("hit at pos " + hitPos);
            }
            else
            {

                //Debug.Log("mouse cast failed " + hitPos);
            }
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            blockPlacing = World.GRASS;
            Debug.Log("placing grass");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            blockPlacing = World.DIRT;
            Debug.Log("placing dirt");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            blockPlacing = World.STONE;
            Debug.Log("placing stone");
        }


        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            blockPlacing = World.SAND;
            Debug.Log("placing sand");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            blockPlacing = World.BEDROCK;
            Debug.Log("placing bedrock");
        }


        Pose curPose = new Pose(transform.position, transform.rotation);
        curPose.rotation = Quaternion.Euler(0, curPose.rotation.eulerAngles.y, 0);
        Vector3 desiredPosition = transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            desiredPosition += curPose.forward * playerSpeed*Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            desiredPosition -= curPose.forward * playerSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            desiredPosition -= curPose.right * playerSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            desiredPosition += curPose.right * playerSpeed * Time.deltaTime;
        }

        Vector3 desiredDiff = desiredPosition - transform.position;
        Vector3 goodDiff;

        bool wasTouchingGround = TouchingLand();

        if (!IntersectingBody(desiredDiff, out goodDiff))
        {
            transform.position += goodDiff;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (wasTouchingGround && !TouchingLand())
            {
                transform.position -= goodDiff;
            }
        }



        if (!TouchingLand())
        {
            vel -= Vector3.up * gravity * Time.deltaTime;
        }
        else
        {
            if (vel.y <= 0)
            {
                vel.y = 0.0f;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                vel.y = jumpSpeed;
            }

        }

        Vector3 velDiff = vel * Time.deltaTime;
        if (vel.y > 0)
        {
            if (!IntersectingHead(transform.position + velDiff))
            {
                transform.position += velDiff;
            }
            else
            {
                vel.y = 0;
            }
        }
        else
        {
            LVector3 a, b;
            Vector3 c, d;
            float maxMag = velDiff.magnitude + heightBelowHead;
            if (!RayCast(transform.position, new Vector3(0, -1, 0), out a, maxMag, out b, out c, out d, maxSteps:-1))
            {
                transform.position += velDiff;
            }
            else
            {
                float dist = Vector3.Distance(transform.position, c);
                if (dist <= velDiff.magnitude + heightBelowHead + 0.01f)
                {
                    //Debug.Log("touching land with normal " + hitNormal + " and magnitude " + velDiff.magnitude + " and offset " + velDiff);
                    vel.y = 0;
                    transform.position = c + new Vector3(0, heightBelowHead - 0.01f, 0);
                }
                else
                {
                    transform.position += velDiff;
                }
            }
            / *
            Vector3 hitNormal;
            //if (!IntersectingBody(velDiff, out goodDiff))
            // {
            //    transform.position += goodDiff;
            // }

            Vector3 recommendedOffset;
            if (TryToMoveBody(velDiff, out hitNormal, out recommendedOffset))
            {
                if (hitNormal.y != 0)
                {
                    Debug.Log("hit with normal " + hitNormal + " and magnitude " + velDiff.magnitude + " and offset " + velDiff);
                    transform.position += recommendedOffset;
                    
                    //vel = VectorRejection(vel, hitNormal);
                }
                else
                {
                    transform.position += velDiff;
                }
            }
            else
            {
                transform.position += velDiff;
            }
            * /
            
        }
    }
}

*/

}