using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class MovingEntity : MonoBehaviour
    {

        float actualBodyWidth = 0.2f;
        float bodyWidth = 0.5f;
        public float heightBelowHead = 2.5f;
        public float heightAboveHead = 0.2f;
        public float feetWidth = 0.2f;
        public float speed = 5.0f;
        float gravity = 37.0f;
        float jumpSpeed = 14.0f;



        string id_ = "";
        public string uid
        {
            get
            {
                if (id_ == "")
                {
                    id_ = System.Guid.NewGuid().ToString();
                }
                return id_;
            }
            set
            {
                id_ = value;
            }
        }

        public float reachRange = 6.0f;
        public int inventorySize = 0;
        public Inventory inventory;

        // Use this for initialization
        void Start()
        {
        }

        Vector3 vel;


        public Vector3 desiredMove;
        public bool usingShift = false;
        public bool jumping;


        public void SetRelativeDesiredMove(Vector3 relativeDesiredMove)
        {
            Pose curPose = new Pose(transform.position, transform.rotation);
            curPose.rotation = Quaternion.Euler(0, curPose.rotation.eulerAngles.y, 0);
            desiredMove = new Vector3(relativeDesiredMove.x, 0, relativeDesiredMove.z).normalized;
            desiredMove = curPose.rotation * desiredMove;
            desiredMove = desiredMove.normalized;
            if (desiredMove.magnitude < 1.0f)
            {
                desiredMove *= desiredMove.magnitude;
            }
        }

        public void SetAbsoluteDesiredMove(Vector3 absoluteDesiredMove)
        {
            desiredMove = absoluteDesiredMove.normalized;
            if (desiredMove.magnitude < 1.0f)
            {
                desiredMove *= desiredMove.magnitude;
            }
        }


        float footDepth = 0.1f;

        // how do we solve the issue of "close enough to be touching, but not too close that we are seen as colliding?"
        // one way: have feet be "deep". So collision is at head - heightBelowHead, but "hit ground" is at head - heightBelowHead - foodDepth



        public bool IsTouchingGround()
        {
            RaycastResults hitResults;
            return IsTouchingGround(transform.position, out hitResults);
        }

        public bool IsTouchingGround(Vector3 offset)
        {
            RaycastResults hitResults;
            return IsTouchingGround(transform.position + offset, out hitResults);
        }

        public RaycastResults BlockStandingOn()
        {
            RaycastResults hitResults;
            if (IsTouchingGround(transform.position, out hitResults))
            {
                return hitResults;
            }
            else
            {
                return null;
            }
        }


        public bool IsTouchingGround(Vector3 pos, out RaycastResults res)
        {
            int iters = 20;
            res = null;
            for (int i = -1; i < iters; i++)
            {
                float p = i / (iters - 1.0f);
                float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
                float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
                Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized * feetWidth;
                if (i == -1) // start with no offset (-1 is just a dumb hack to do this)
                {
                    curOffset = new Vector3(0, 0, 0);
                }

                if (PhysicsUtils.RayCast(pos + curOffset, new Vector3(0, -1, 0), heightBelowHead + footDepth, out res))
                {
                    return true;
                }
            }
            return false;
        }

        public bool RayCastWithWidth(Vector3 origin, Vector3 direction, float width, float maxMag, out RaycastResults hitResults)
        {
            int iters = 20;
            hitResults = null;
            for (int i = -1; i < iters; i++)
            {
                float p = i / (iters - 1.0f);
                float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
                float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
                Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized * width;
                if (i == -1) // start with no offset (-1 is just a dumb hack to do this)
                {
                    curOffset = new Vector3(0, 0, 0);
                }

                if (PhysicsUtils.RayCast(origin + curOffset, direction, maxMag, out hitResults))
                {
                    hitResults.hitPos -= curOffset;
                    return true;
                }
            }
            return false;
        }

        public bool OKToMove(Vector3 offset)
        {
            if (IntersectingBody(transform.position + offset))
            {
                return false;
            }
            else
            {
                if (usingShift)
                {
                    if (IsTouchingGround())
                    {
                        if (!IsTouchingGround(offset))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        bool IsInWater(Vector3 pos)
        {
            LVector3 bPos = LVector3.FromUnityVector3(pos);
            return bPos.BlockV == Example_pack.Example.Water || bPos.BlockV == Example_pack.Example.WaterNoFlow;
        }
        bool TouchingWater()
        {
            return IsInWater(transform.position) ||
                IsInWater(transform.position - new Vector3(0, heightBelowHead, 0)) ||
                IsInWater(transform.position - new Vector3(0, heightBelowHead / 2.0f, 0)) ||
                IsInWater(transform.position - new Vector3(0, 2.0f * heightBelowHead / 3.0f, 0)) ||
                IsInWater(transform.position - new Vector3(0, heightBelowHead / 3.0f, 0)) ||

                IsInWater(transform.position + new Vector3(0, heightAboveHead, 0)) ||
                IsInWater(transform.position + new Vector3(0, heightAboveHead / 2.0f, 0)) ||
                IsInWater(transform.position + new Vector3(0, 2.0f * heightAboveHead / 3.0f, 0)) ||
                IsInWater(transform.position + new Vector3(0, heightAboveHead / 3.0f, 0));

        }



        public void GrabBlockEntity(BlockEntity blockEntity)
        {
            if (inventory.TryToAddBlock(blockEntity.Stack))
            {
                GameObject.Destroy(blockEntity.gameObject);
            }
        }

        void PullBlockEntitiesNearby(float reachRange)
        {
            BlockEntity[] entities = FindObjectsOfType<BlockEntity>();
            foreach (BlockEntity blockEntity in entities)
            {
                if (!blockEntity.enabled)
                {
                    continue;
                }
                if (!blockEntity.pullable || (blockEntity.playerThrowing != null && blockEntity.playerThrowing == this))
                {
                    continue;
                }
                Vector3 grabFromPos = transform.position - Vector3.up * this.heightBelowHead / 3.0f;
                if (inventory.CanAddBlock(blockEntity.Stack))
                {
                    if ((Vector3.Distance(blockEntity.transform.position, grabFromPos) < reachRange && blockEntity.playerPulling == null) || blockEntity.playerPulling == this)
                    {
                        blockEntity.playerPulling = this;
                        float moveDist = 10.0f * Time.deltaTime;
                        if (Vector3.Distance(blockEntity.transform.position, grabFromPos) < Mathf.Max(0.2f, moveDist))
                        {
                            GrabBlockEntity(blockEntity);
                        }
                        else
                        {
                            blockEntity.transform.position += (grabFromPos - blockEntity.transform.position).normalized * moveDist;
                        }
                    }
                }
                else if (blockEntity.playerPulling == this)
                {
                    blockEntity.playerPulling = null;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

            // don't do stuff until world is loaded
            if (World.mainWorld == null || !World.mainWorld.fullyLoaded)
            {
                if (World.mainWorld != null)
                {
                    Debug.Log("loading status: " + World.mainWorld.loadingStatus);
                }
                return;
            }


            if (inventorySize > 0)
            {
                if (inventory == null)
                {
                    inventory = new Inventory(inventorySize);
                }
                else if(inventory.capacity != inventorySize)
                {
                    inventory.ChangeSize(inventorySize, transform.position);
                }
            }
            else
            {
                if (inventory != null && inventory.blocks != null && inventory.capacity > 0)
                {
                    inventory.ThrowAllBlocks(transform.position);
                    inventory = null;
                }
            }


            if (inventory != null && inventory.capacity > 0)
            {
                PullBlockEntitiesNearby(reachRange);
            }

            //Vector3 goodDiff;
            Vector3 desiredDiff = desiredMove * Time.deltaTime * speed;
            desiredDiff = new Vector3(desiredDiff.x, 0, desiredDiff.z);
            if (OKToMove(desiredDiff))
            {
                transform.position += desiredDiff;
            }
            else if (OKToMove(new Vector3(desiredDiff.x, 0, 0)))
            {
                transform.position += new Vector3(desiredDiff.x, 0, 0);
            }
            else if (OKToMove(new Vector3(0, 0, desiredDiff.z)))
            {
                transform.position += new Vector3(0, 0, desiredDiff.z);
            }


            /*
            bool wasTouchingGround = IsTouchingGround();
            if (!IntersectingBody(transform.position+desiredDiff))
            {

            }
            else
            {
                Vector3 tmpDesiredDiffX = new Vector3(desiredDiff.x, 0, 0);
                Vector3 tmpDesiredDiffZ = new Vector3(0, 0, desiredDiff.z);
                if (tmpDesiredDiffX.x != 0 && !IntersectingBody(transform.position + tmpDesiredDiffX))
                {
                    desiredDiff = tmpDesiredDiffX;
                }
                else if(tmpDesiredDiffZ.z != 0 && !IntersectingBody(transform.position + tmpDesiredDiffZ))
                {
                    desiredDiff = tmpDesiredDiffZ;
                }
                else
                {
                    desiredDiff = Vector3.zero;
                }
            }

            transform.position += desiredDiff;
            if (usingShift)
            {
                if (wasTouchingGround && !IsTouchingGround())
                {
                    transform.position -= desiredDiff;
                }
            }
            */

            //while (LVector3.FromUnityVector3(transform.position - new Vector3(0,heightBelowHead, 0)).Block != World.AIR || LVector3.FromUnityVector3(transform.position).Block != World.AIR)
            //{
            //    transform.position += new Vector3(0, 0.1f,0);
            //}
            while (IntersectingBody(transform.position))
            {
                transform.position += new Vector3(0, 0.1f, 0);
            }

            bool touchingWater = TouchingWater();
            if (!IsTouchingGround())
            {
                if (touchingWater)
                {
                    if (jumping && vel.y < jumpSpeed)
                    {
                        vel.y += jumpSpeed * Time.deltaTime * 2.0f;
                        if (vel.y > jumpSpeed / 2.0f)
                        {
                            vel.y = jumpSpeed / 2.0f;
                        }
                    }
                    else if (!jumping)
                    {
                        vel -= Vector3.up * gravity * Time.deltaTime * 0.1f;
                    }
                }
                else
                {
                    vel -= Vector3.up * gravity * Time.deltaTime;
                }
            }
            else
            {
                if (vel.y <= 0)
                {
                    vel.y = 0.0f;
                }
                if (jumping)
                {
                    if (touchingWater)
                    {
                        vel.y += jumpSpeed * Time.deltaTime * 2.0f;
                        if (vel.y > jumpSpeed / 2.0f)
                        {
                            vel.y = jumpSpeed / 2.0f;
                        }
                    }
                    else
                    {
                        vel.y = jumpSpeed;
                    }
                }

            }


            Vector3 velDiff = vel * Time.deltaTime;
            if (vel.y != 0)
            {
                if (vel.y > 0)
                {
                    RaycastResults hitResults;
                    if (RayCastWithWidth(transform.position - new Vector3(0, heightBelowHead, 0), (new Vector3(0, 1, 0)).normalized, feetWidth, velDiff.magnitude + heightAboveHead + heightBelowHead, out hitResults))
                    {
                        transform.position = hitResults.hitPos - new Vector3(0, heightAboveHead + 0.1f, 0);
                        vel.y = 0;
                    }
                    else
                    {
                        transform.position += velDiff;
                    }
                }
                else
                {
                    RaycastResults hitResults;
                    if (RayCastWithWidth(transform.position, (new Vector3(0, -1, 0)).normalized, feetWidth, velDiff.magnitude + heightBelowHead, out hitResults))
                    {
                        transform.position = hitResults.hitPos + new Vector3(0, heightBelowHead + footDepth * 0.9f, 0);
                        vel.y = 0;
                    }
                    else
                    {
                        transform.position += velDiff;
                    }
                }
            }
            /*
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
                float maxMag = velDiff.magnitude + heightBelowHead;
                RaycastResults hitResults;
                if (!PhysicsUtils.RayCast(transform.position, new Vector3(0, -1, 0), maxMag, out hitResults))
                {
                    transform.position += velDiff;
                }
                else
                {
                    if (hitResults.dist <= velDiff.magnitude + heightBelowHead + 0.01f)
                    {
                        //Debug.Log("touching land with normal " + hitNormal + " and magnitude " + velDiff.magnitude + " and offset " + velDiff);
                        vel.y = 0;
                        ////transform.position = hitResults.hitPos + new Vector3(0, heightBelowHead - 0.01f, 0);
                        transform.position = hitResults.hitPos + new Vector3(0, heightBelowHead, 0);
                    }
                    else
                    {
                        transform.position += velDiff;
                    }
                }
            }
            */
        }


        bool IntersectingBody(Vector3 position)
        {

            int iters = 20;

            Vector3 topOfHead = position + new Vector3(0, heightAboveHead, 0);
            Vector3 bottomOfHead = position - new Vector3(0, heightBelowHead, 0);
            float height = Vector3.Distance(topOfHead, bottomOfHead);

            for (int i = -1; i < iters; i++)
            {
                float p = i / (iters - 1.0f);
                float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
                float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
                Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized * feetWidth;
                if (i == -1) // start with no offset (-1 is just a dumb hack to do this)
                {
                    curOffset = new Vector3(0, 0, 0);
                }

                if (PhysicsUtils.RayCast(topOfHead + curOffset, new Vector3(0, -1, 0), height) || PhysicsUtils.RayCast(bottomOfHead + curOffset, new Vector3(0, 1, 0), height))
                {
                    return true;
                }
            }
            return false;
            /*
            float worldScale = World.mainWorld.worldScale;



            long eyesX = (long)Mathf.Floor(position.x / worldScale);
            long eyesY = (long)Mathf.Floor(position.y / worldScale);
            long eyesZ = (long)Mathf.Floor(position.z / worldScale);
            long feetX = (long)Mathf.Floor(position.x / worldScale);
            long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.9f) / worldScale);
            long feetZ = (long)Mathf.Floor(position.z / worldScale);
            long headX = (long)Mathf.Floor(position.x / worldScale);
            long headY = (long)Mathf.Floor((position.y + heightAboveHead) / worldScale);
            long headZ = (long)Mathf.Floor(position.z / worldScale);
            long bodyX = (long)Mathf.Floor(position.x / worldScale);
            long bodyY = (long)Mathf.Floor((position.y + heightAboveHead / 2.0f) / worldScale);
            long bodyZ = (long)Mathf.Floor(position.z / worldScale);
            int atHead = World.mainWorld[headX, headY, headZ];
            int atEyes = World.mainWorld[eyesX, eyesY, eyesZ];
            int atFeet = World.mainWorld[feetX, feetY, feetZ];
            int atBody = World.mainWorld[bodyX, bodyY, bodyZ];
            return atHead != 0 || atEyes != 0 || atFeet != 0 || atBody != 0;
            / *
            LVector3 eyesPos = LVector3.FromUnityVector3(position);
            LVector3 feetPos = LVector3.FromUnityVector3(position + new Vector3(0, -heightBelowHead+0.02f, 0));
            LVector3 topOfHeadPos = LVector3.FromUnityVector3(position + new Vector3(0, heightAboveHead, 0));
            LVector3 bodyPos = LVector3.FromUnityVector3(position + new Vector3(0, -heightBelowHead / 2.0f, 0));
            LVector3 middleOfHeadPos = LVector3.FromUnityVector3(position + new Vector3(0, heightAboveHead / 2.0f, 0));
            int atEyes = World.mainWorld[eyesPos];
            int atFeet = World.mainWorld[feetPos];
            int atTopOfHead = World.mainWorld[topOfHeadPos];
            int atBody = World.mainWorld[bodyPos];
            int atMiddleOfHead = World.mainWorld[middleOfHeadPos];
            return (atEyes != 0 || atTopOfHead != 0 || atBody != 0 || atMiddleOfHead != 0);
            */
        }

        bool IntersectingBody(Vector3 desiredOffset, out Vector3 goodOffset)
        {
            goodOffset = desiredOffset;
            for (int i = 0; i < 20; i++)
            {
                float p = i / (20 - 1.0f);
                float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
                float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
                Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized * 0.5f;
                if (Vector3.Dot(desiredOffset, curOffset) > 0) // if that offset is in the same direction we are going, test to see if it moves us into a block
                {
                    if (IntersectingBody(transform.position + curOffset))
                    {
                        // vector rejection
                        goodOffset = goodOffset - Vector3.Project(goodOffset, curOffset);
                    }
                }
            }
            Debug.Log(desiredOffset + " " + goodOffset + " " + name);
            if (Vector3.Dot(goodOffset.normalized, desiredOffset.normalized) <= 0.0001f)
            {
                return true;
            }
            Debug.Log(desiredOffset + " " + goodOffset + " " + name);
            return false;
        }


        bool IntersectingHead(Vector3 position)
        {
            float worldScale = World.mainWorld.worldScale;
            long eyesX = (long)Mathf.Floor(position.x / worldScale);
            long eyesY = (long)Mathf.Floor(position.y / worldScale);
            long eyesZ = (long)Mathf.Floor(position.z / worldScale);
            long feetX = (long)Mathf.Floor(position.x / worldScale);
            long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.9f) / worldScale);
            long feetZ = (long)Mathf.Floor(position.z / worldScale);
            long headX = (long)Mathf.Floor(position.x / worldScale);
            long headY = (long)Mathf.Floor((position.y + heightAboveHead) / worldScale);
            long headZ = (long)Mathf.Floor(position.z / worldScale);
            int atHead = World.mainWorld[headX, headY, headZ];
            int atEyes = World.mainWorld[eyesX, eyesY, eyesZ];
            int atFeet = World.mainWorld[feetX, feetY, feetZ];
            return atHead != 0 || atEyes != 0;
        }

        bool TouchingLand(Vector3 position, bool moveFeet)
        {
            float worldScale = World.mainWorld.worldScale;
            long eyesX = (long)Mathf.Floor(position.x / worldScale);
            long eyesY = (long)Mathf.Floor(position.y / worldScale);
            long eyesZ = (long)Mathf.Floor(position.z / worldScale);
            long feetX = (long)Mathf.Floor(position.x / worldScale);
            long feetY = (long)Mathf.Floor((position.y - heightBelowHead) / worldScale);
            long feetZ = (long)Mathf.Floor(position.z / worldScale);
            long headX = (long)Mathf.Floor(position.x / worldScale);
            long headY = (long)Mathf.Floor((position.y + heightAboveHead) / worldScale);
            long headZ = (long)Mathf.Floor(position.z / worldScale);
            int atHead = World.mainWorld[headX, headY, headZ];
            int atEyes = World.mainWorld[eyesX, eyesY, eyesZ];
            int atFeet = World.mainWorld[feetX, feetY, feetZ];
            Vector3 resPos = transform.position / worldScale;
            bool touching = false;
            if (atFeet != 0)
            {
                //if (moveFeet) { resPos.y = feetY + 1 + heightBelowHead / world.worldScale; }
                touching = true;

                if (moveFeet)
                {
                    transform.position = resPos * worldScale;
                }
            }
            return touching;

        }


        public bool TouchingLand()
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
                Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized * feetWidth;
                if (TouchingLand(transform.position + curOffset, false))
                {
                    return true;
                }
            }
            return false;
        }

    }
}