using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMob : MonoBehaviour {
    public enum MobType
    {
        Pet,
        Sprucer
    }


    public MobType mobType;


    Vector3 offset;

    public int blocksHeight = 2;



    PathNode curPath;

    public float pathfindsPerSecond = 4.0f;

    public long lastPathfind = 0;

    public MovingEntity pathingTarget;

    public void UpdatePathing()
    {


        MovingEntity body = GetComponent<MovingEntity>();
        if (pathingTarget == null)
        {
            body.desiredMove = Vector3.zero;
        }
        if (PhysicsUtils.millis() - lastPathfind > 1000.0 / pathfindsPerSecond)
        {
            LVector3 myPos = LVector3.FromUnityVector3(transform.position);
            RaycastResults blockStandingOn = body.BlockStandingOn();
            // if we are using shift and standing over an empty block, but our feet are on a neighboring block, use that neighboring block for pathfinding instead
            if (blockStandingOn != null)
            {
                myPos = blockStandingOn.hitBlock + new LVector3(0, blocksHeight, 0);
            }
            LVector3 playerPos = LVector3.FromUnityVector3(pathingTarget.transform.position);
            bool iShouldJump;
            PhysicsUtils.Pathfind(blocksHeight, ref curPath, out iShouldJump, myPos, playerPos, 200);

            if (iShouldJump)
            {
                body.jumping = true;
            }
            lastPathfind = PhysicsUtils.millis();
        }

        body.desiredMove = Vector3.zero;
        Vector3 targetPos = transform.position;
        if (curPath != null)
        {
            LVector3 myPos = LVector3.FromUnityVector3(transform.position);
            LVector3 myPosBeforeJump = myPos - new LVector3(0, 1, 0);
            if (curPath.prevNode != null && (myPos == curPath.prevNode.pos || myPosBeforeJump == curPath.prevNode.pos))
            {

            }
            else if (curPath.nextNode != null && (myPos == curPath.nextNode.pos || myPosBeforeJump == curPath.nextNode.pos))
            {
                curPath = curPath.nextNode;
            }
            else if (myPos == curPath.pos || myPosBeforeJump == curPath.pos)
            {
                if (curPath.nextNode != null)
                {
                    curPath = curPath.nextNode;
                }
            }

            LVector3 targetBlock = curPath.pos;
            if (targetBlock.y == LVector3.FromUnityVector3(transform.position).y)
            {
                body.usingShift = true;
            }
            else
            {
                body.usingShift = false;
            }
            if (targetBlock.y > myPos.y)
            {
                body.jumping = true;
            }
            else
            {
                body.jumping = false;
            }
            targetPos = targetBlock.BlockCentertoUnityVector3();
            body.SetAbsoluteDesiredMove((targetPos - transform.position).normalized);
        }
    }

    public void UpdateOld()
    {

        MovingEntity body = GetComponent<MovingEntity>();
        if (PhysicsUtils.millis() - lastPathfind > 1000.0/pathfindsPerSecond)
        {
            LVector3 myPos = LVector3.FromUnityVector3(transform.position);
            RaycastResults blockStandingOn = body.BlockStandingOn();
            // if we are using shift and standing over an empty block, but our feet are on a neighboring block, use that neighboring block for pathfinding instead
            if (blockStandingOn != null)
            {
                myPos = blockStandingOn.hitBlock + new LVector3(0, blocksHeight, 0);
            }
            LVector3 playerPos = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);
            bool iShouldJump;
            PhysicsUtils.Pathfind(blocksHeight, ref curPath, out iShouldJump, myPos, playerPos, 200);

            if (iShouldJump)
            {
                body.jumping = true;
            }
            lastPathfind = PhysicsUtils.millis();
        }

        body.desiredMove = Vector3.zero;
        Vector3 targetPos = transform.position;
        if (curPath != null)
        {
            LVector3 myPos = LVector3.FromUnityVector3(transform.position);
            LVector3 myPosBeforeJump = myPos - new LVector3(0, 1, 0);
            if (curPath.prevNode != null && (myPos == curPath.prevNode.pos || myPosBeforeJump == curPath.prevNode.pos))
            {

            }
            else if (curPath.nextNode != null && (myPos == curPath.nextNode.pos || myPosBeforeJump == curPath.nextNode.pos))
            {
                curPath = curPath.nextNode;
            }
            else if (myPos == curPath.pos || myPosBeforeJump == curPath.pos)
            {
                if (curPath.nextNode != null)
                {
                    curPath = curPath.nextNode;
                }
            }
            else if(myPos != curPath.pos && myPosBeforeJump != curPath.pos && curPath.prevNode != null)
            {
                //curPath = curPath.prevNode;
            }

            /*
            while (myPos != curPath.pos && myPosBeforeJump != curPath.pos)
            {
                if (curPath.nextNode == null)
                {
                    break;
                }
                else
                {
                    curPath = curPath.nextNode;
                }
            }
            */

            //if (myPos == curPath.pos || myPosBeforeJump == curPath.pos)
           // {
            LVector3 targetBlock = curPath.pos;
            if (targetBlock.y == LVector3.FromUnityVector3(transform.position).y)
            {
                body.usingShift = true;
            }
            else
            {
                body.usingShift = false;
            }
            if (curPath.nextNode != null)
            {
                //targetBlock = curPath.nextNode.pos;
            }
            if (targetBlock.y > myPos.y)
            {
                body.jumping = true;
            }
            else
            {
                body.jumping = false;
            }
            targetPos = targetBlock.BlockCentertoUnityVector3();
            body.SetAbsoluteDesiredMove((targetPos - transform.position).normalized);
            //}
            //else
            //{
           // }
        }
        /*
        if (curPath != null && curPath.nextNode != null)
        {
            //PathNode next = curPath;
            //while (Vector3.Distance(next.pos.BlockCentertoUnityVector3(), transform.position) < 1.0f && next.nextNode != null)
            //{
            //    next = next.nextNode;
            //}
            targetPos = curPath.nextNode.pos.BlockCentertoUnityVector3();
            //LVector3 playerPos = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);
            //Debug.Log("going to " + curPath.nextNode.pos.BlockCentertoUnityVector3() + " player position is " + FindObjectOfType<BlocksPlayer>().transform.position + " player block pos is " + playerPos);
            //body.SetAbsoluteDesiredMove((curPath.nextNode.pos.BlockCentertoUnityVector3() - transform.position));
            //body.jumping = true;
        }
        else if (curPath != null)
        {
            //Debug.Log("going to " + curPath.nextNode.pos.BlockCentertoUnityVector3() + " player position is " + FindObjectOfType<BlocksPlayer>().transform.position + " player block pos is " + playerPos);
            // body.SetAbsoluteDesiredMove((curPath.pos.BlockCentertoUnityVector3() - transform.position));
            targetPos = curPath.pos.BlockCentertoUnityVector3();
            //body.jumping = true;
        }
        LVector3 targetBlock = LVector3.FromUnityVector3(targetPos);
        LVector3 myBlock = LVector3.FromUnityVector3(transform.position - new Vector3(0, body.heightBelowHead / 2.0f, 0));
        if (targetBlock.y > myBlock.y)
        {
            body.jumping = true;
        }
        else
        {
            body.jumping = false;
        }
        body.SetAbsoluteDesiredMove((targetPos - transform.position)+Random.insideUnitSphere*0.1f);
        */
        /*
        Vector3 curOff = Random.insideUnitCircle;
        offset += curOff * 0.1f;
        if (offset.magnitude > 1.0f)
        {
            offset = offset.normalized * 1.0f;
        }
        if (Random.value < 0.01f)
        {
            body.jumping = true;
        }
        else
        {
            body.jumping = false;
        }
        body.desiredMove = offset;
        */
    }

}
