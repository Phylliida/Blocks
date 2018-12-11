using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovingEntity))]
public class MobWithBehavior : MonoBehaviour {

    public class ThingDoingTarget
    {
        public enum TypeOfThingDoingTarget
        {
            Block,
            Entity
        }
        public LVector3 block;
        public MovingEntity entity;

        public ThingDoingTarget(LVector3 block)
        {
            this.block = block;
        }

        public ThingDoingTarget(MovingEntity entity)
        {
            this.entity = entity;
        }
    }

    public enum TypeOfThingDoing
    {
        Standing,
        Wandering,
        Sitting,
        Chasing,
        Searching,
        RunningAway,
        Socializing
    }

    public class ThingDoing
    {
        public ThingDoingTarget target;
        public TypeOfThingDoing typeOfThing;
        public ThingDoing(TypeOfThingDoing typeOfThing, ThingDoingTarget target=null)
        {
            this.target = target;
            this.typeOfThing = typeOfThing;
        }
    }

    public TypeOfThingDoing typeOfThingDoing;

    ThingDoing thingDoing;

    public ThingDoing action
    {
        get
        {
            return thingDoing;
        }
        set
        {
            thingDoing = value;
        }
    }

    public int blocksHeight = 2;

    World world;
    public bool isValid(long px, long py, long pz, long x, long y, long z)
    {
        bool isCurrentlyValid = true;
        for (int i = 0; i < blocksHeight; i++)
        {
            isCurrentlyValid = isCurrentlyValid && world[x, y - i, z] == (int)BlockValue.Air;
        }
        // bad:
        // X  
        //    2
        // 1
        // good:
        // 
        //     2
        // 1 
        if (y > py)
        {
            isCurrentlyValid = isCurrentlyValid && world[px, py + 1, pz] == (int)BlockValue.Air;
        }
        else if (y < py)
        {
            isCurrentlyValid = isCurrentlyValid && world[x, y + 1, z] == (int)BlockValue.Air;
        }
        if (py == y && z != pz && x != px)
        {
            isCurrentlyValid = isCurrentlyValid && (world[px, py, z] == (int)BlockValue.Air || world[x, py, pz] == (int)BlockValue.Air);
        }
        return isCurrentlyValid && world[x, y - blocksHeight, z] != (int)BlockValue.Air;
    }

    public void Start()
    {
        if (world == null)
        {
            world = World.mainWorld;
        }
        hungerMeter = baseHunger;
        stamina = baseStamina;
        wantToFindFoodThresh = hungerMeterMax / 2.0f;
    }



    PathNode curPath;

    float pathfindsPerSecond = 0.5f;

    long lastPathfind = 0;

    public LVector3 pathingTarget;

    public float baseHunger = 2.0f;
    /// <summary>
    /// Lower is more hungry, 0 is starving, hungerMeterMax is full
    /// </summary>
    public float hungerMeter;
    public float hungerMeterMax = 10.0f;

    public float wantToFindFoodThresh;
    public float veryHungryThresh = 2.0f;

    public float baseStamina = 3.0f;
    public float stamina;
    public float staminaMax;


    public static int frameUpdatedLast = 0;
    public void UpdatePathing()
    {
        MovingEntity body = GetComponent<MovingEntity>();
        if (pathingTarget == null)
        {
            body.desiredMove = Vector3.zero;
            return;
        }
        if (PhysicsUtils.millis() - lastPathfind > 1000.0 / pathfindsPerSecond && frameUpdatedLast != Time.frameCount) // offset so everyone isn't aligned on the same frame
        {
            LVector3 myPos = LVector3.FromUnityVector3(transform.position);
            if (hungerMeter <= veryHungryThresh)
            {
                LVector3 foundThing;
                if (Search(out foundThing))
                {
                    // found it, is it closer?
                    if(LVector3.CityBlockDistance(foundThing, myPos) <LVector3.CityBlockDistance(pathingTarget, myPos))
                    {
                        // if so, go to it instead
                        thingDoing = new ThingDoing(TypeOfThingDoing.Chasing, new ThingDoingTarget(foundThing));
                    }
                    //Debug.Log("found thing in " + steps + " steps");
                }
                else
                {
                    // did not find
                    //Debug.Log("did not find thing in " + steps + " steps");
                }
            }
            frameUpdatedLast = Time.frameCount;
            //Debug.Log("updating pathing");
            RaycastResults blockStandingOn;
            if (PhysicsUtils.RayCastAlsoHitWater(body.transform.position, -Vector3.up, 20.0f, out blockStandingOn))
            {
                // if we are using shift and standing over an empty block, but our feet are on a neighboring block, use that neighboring block for pathfinding instead
                if (blockStandingOn != null)
                {
                    myPos = blockStandingOn.hitBlock + new LVector3(0, blocksHeight, 0);
                }
                //LVector3 playerPos = LVector3.FromUnityVector3(pathingTarget.transform.position);
                bool iShouldJump;
                if (thingDoing.typeOfThing == TypeOfThingDoing.RunningAway)
                {
                    PhysicsUtils.PathfindAway(blocksHeight, ref curPath, out iShouldJump, myPos, pathingTarget, 100);
                }
                else
                {
                    PhysicsUtils.Pathfind(blocksHeight, ref curPath, out iShouldJump, myPos, pathingTarget, 100);
                }
                if (curPath != null)
                {
                    //Debug.Log("curPath = " + curPath.pos + " next " + curPath.nextNode);
                }
                if (iShouldJump)
                {
                    body.jumping = true;
                }
            }
            else
            {
                //Debug.Log("falling far, cannot pathfind");
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
                //body.usingShift = true;
                body.usingShift = false;
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

    long lastSearchTime = 0;
    long millisBetweenSearch = 50;


    public bool Search(out LVector3 found)
    {
        found = new LVector3();
        RaycastResults res;
        for (int i = 0; i < 10; i++)
        {
            if (PhysicsUtils.CustomRaycast(transform.position, Random.onUnitSphere, 100.0f, (b, bx, by, bz, pbx, pby, pbz) => { return true; }, (b, bx, by, bz, pbx, pby, pbz) => { return b == Example.FlowerWithNectar; }, out res))
            {
                found = res.hitBlock;
                return true;
            }
        }
        return false;
    }

    void EatObject(LVector3 block)
    {
        hungerMeter = Mathf.Min(hungerMeterMax, hungerMeter + 1.0f);
    }

    public void Update()
    {
        if (world == null)
        {
            world = World.mainWorld;
        }
        if (thingDoing == null)
        {
            thingDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
        }


        if (hungerMeter < wantToFindFoodThresh && thingDoing.typeOfThing != TypeOfThingDoing.Chasing && thingDoing.typeOfThing != TypeOfThingDoing.Searching)
        {
            thingDoing = new ThingDoing(TypeOfThingDoing.Searching);
        }

        if (hungerMeter >= wantToFindFoodThresh && thingDoing.typeOfThing != TypeOfThingDoing.RunningAway)
        {
            thingDoing = new ThingDoing(TypeOfThingDoing.Standing);
        }


        BlocksPlayer[] players = FindObjectsOfType<BlocksPlayer>();

        foreach (BlocksPlayer player in players)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 10.0f)
            {
                thingDoing = new ThingDoing(TypeOfThingDoing.RunningAway, new ThingDoingTarget(player.GetComponent<MovingEntity>()));
            }
        }

        typeOfThingDoing = thingDoing.typeOfThing;
        MovingEntity self = GetComponent<MovingEntity>();
        if (thingDoing.typeOfThing == TypeOfThingDoing.Chasing)
        {
            if (thingDoing.target.entity == null)
            {
                pathingTarget = thingDoing.target.block;
            }
            else
            {
                pathingTarget = LVector3.FromUnityVector3(thingDoing.target.entity.transform.position);
            }


            if (pathingTarget.BlockV != Example.FlowerWithNectar)
            {
                thingDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
            }
            else
            {
                float myDist = LVector3.CityBlockDistance(LVector3.FromUnityVector3(transform.position), pathingTarget);
                // found it, eat it
                if (myDist < 4)
                {
                    EatObject(pathingTarget);
                    world[pathingTarget] = (int)Example.Flower;
                    thingDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
                }
                // still pathing to it
                else
                {
                    UpdatePathing();
                }
            }
            //pathingTarget = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);
        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.RunningAway)
        {
            if (thingDoing.target.entity == null)
            {
                pathingTarget = thingDoing.target.block;
            }
            else
            {
                pathingTarget = LVector3.FromUnityVector3(thingDoing.target.entity.transform.position);
            }
        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.Searching)
        {
            if (PhysicsUtils.millis() - lastSearchTime > millisBetweenSearch)
            {
                LVector3 foundThing;
                if(Search(out foundThing))
                {
                    // found it
                    thingDoing = new ThingDoing(TypeOfThingDoing.Chasing, new ThingDoingTarget(foundThing));
                    //Debug.Log("found thing in " + steps + " steps");
                }
                else
                {
                    // did not find
                    //Debug.Log("did not find thing in " + steps + " steps");
                }
                /*
                Debug.Log("running search");
                LVector3 pos = LVector3.FromUnityVector3(self.transform.position);
                RaycastResults hitGround;
                if (PhysicsUtils.RayCast(self.transform.position, -Vector3.up, 10.0f, out hitGround))
                {
                    LVector3 foundThing = pos;
                    int steps = 0;
                    if (PhysicsUtils.SearchOutwards(hitGround.hitBlock, maxSteps: 200, searchUp: true, searchDown: true, isBlockValid: (b, bx, by, bz, pbx, pby, pbz) =>
                    {
                        steps += 1;
                        return (BlockValue)b != BlockValue.Air && (world.GetNumAirNeighbors(bx, by, bz) > 0 || world.GetNumAirNeighbors(bx, by+1, bz) > 0 || world.GetNumAirNeighbors(bx, by + 2, bz) > 0);
                    },
                        isBlockDesiredResult: (b, bx, by, bz, pbx, pby, pbz) =>
                        {
                            if ((BlockValue)b == BlockValue.Flower)
                            {
                                foundThing = new LVector3(bx, by, bz);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }))
                    {
                        // found it
                        thingDoing = new ThingDoing(TypeOfThingDoing.Chasing, new ThingDoingTarget(foundThing));
                        //Debug.Log("found thing in " + steps + " steps");
                    }
                    else
                    {
                        // did not find
                        //Debug.Log("did not find thing in " + steps + " steps");
                    }
                }
               */
                lastSearchTime = PhysicsUtils.millis();
            }
        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.Sitting)
        {

        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.Socializing)
        {

        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.Standing)
        {
            GetComponent<CrocodileDoggy>().SetSpeed(0.0f);
            self.desiredMove = Vector3.zero;
        }
        else if(thingDoing.typeOfThing == TypeOfThingDoing.Wandering)
        { 

        }
    }
}
