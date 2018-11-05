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
    public MovingEntity body;


    Vector3 offset;



    PathNode curPath;

    public class PathNode
    {
        public LVector3 pos;
        public PathNode prevNode;
        public PathNode nextNode;

        public PathNode(LVector3 pos, PathNode prevNode)
        {
            this.pos = pos;
            this.prevNode = prevNode;
        }
    }


    World world { get  { return World.mainWorld; }   }

    public bool isValid(LVector3 pos)
    {
        return world[pos] == World.AIR && world[pos.x, pos.y + 1, pos.z] == World.AIR && world[pos.x, pos.y - 1, pos.z] != World.AIR;
    }


    public IEnumerable<LVector3> AllNeighbors(LVector3 pos)
    {
        yield return new LVector3(pos.x + 1, pos.y, pos.z);
        yield return new LVector3(pos.x - 1, pos.y, pos.z);
        yield return new LVector3(pos.x + 1, pos.y + 1, pos.z);
        yield return new LVector3(pos.x - 1, pos.y + 1, pos.z);
        yield return new LVector3(pos.x + 1, pos.y - 1, pos.z);
        yield return new LVector3(pos.x - 1, pos.y - 1, pos.z);
        yield return new LVector3(pos.x, pos.y + 1, pos.z + 1);
        yield return new LVector3(pos.x, pos.y + 1, pos.z - 1);
        yield return new LVector3(pos.x, pos.y - 1, pos.z + 1);
        yield return new LVector3(pos.x, pos.y - 1, pos.z - 1);
        yield return new LVector3(pos.x, pos.y, pos.z + 1);
        yield return new LVector3(pos.x, pos.y, pos.z - 1);
    }

    public void Pathfind(LVector3 goalPos, int maxSteps = 100)
    {
        Debug.Log((new LVector3(1, 2, 3)).BlockCentertoUnityVector3() + " is center of 1,2,3");
        HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
        Queue<PathNode> nodes = new Queue<PathNode>();
        LVector3 myPos = LVector3.FromUnityVector3(transform.position);
        PathNode closest = new PathNode(myPos, null);
        long closestDist = long.MaxValue;
        for (int i = 0; i < 6; i++)
        {
            LVector3 tmpPos = myPos - new LVector3(0, i, 0);
            if (!nodesSoFar.Contains(tmpPos) && isValid(tmpPos))
            {
                nodes.Enqueue(new PathNode(tmpPos, null));
                nodesSoFar.Add(tmpPos);
            }
        }

        int steps = 0;
        if (nodes.Count == 0)
        {
            body.jumping = true;
        }
        while (nodes.Count > 0)
        {
            PathNode curNode = nodes.Dequeue();
            long dist = LVector3.CityBlockDistance(curNode.pos, goalPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = curNode;
            }
            foreach (LVector3 neighbor in AllNeighbors(curNode.pos))
            {
                if (!nodesSoFar.Contains(neighbor) && isValid(neighbor))
                {
                    nodes.Enqueue(new PathNode(neighbor, curNode));
                    nodesSoFar.Add(neighbor);
                }
            }
            steps += 1;
            if (steps == maxSteps || dist <= 1)
            {
                Debug.Log("found path with dist = " + dist + " in " + steps + " steps");
                break;
            }
        }

        curPath = closest;
        if (curPath != null && curPath.prevNode != null)
        {
            PathNode curBlock = curPath.prevNode;
            PathNode nextBlock = curPath;
            while (curBlock.prevNode != null)
            {
                curBlock.nextNode = nextBlock;
                nextBlock = curBlock;
                curBlock = curBlock.prevNode;
            }
            curBlock.nextNode = nextBlock;
            curPath = curBlock;
        }

    }

    public float pathfindsPerSecond = 4.0f;

    public long lastPathfind = 0;

    public void Update()
    {

        if (PhysicsUtils.millis() - lastPathfind > 1000.0/pathfindsPerSecond && body.TouchingLand())
        {
            LVector3 playerPos = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);
            Pathfind(playerPos, 200);
            lastPathfind = PhysicsUtils.millis();
        }


        Vector3 targetPos = transform.position;
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
