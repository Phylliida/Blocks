using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    [RequireComponent(typeof(MovingEntity))]
    public abstract class MobWithBehavior : MonoBehaviour
    {

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
            public ThingDoing(TypeOfThingDoing typeOfThing, ThingDoingTarget target = null)
            {
                this.target = target;
                this.typeOfThing = typeOfThing;
            }
        }


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

        public World world;
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
                        if (LVector3.CityBlockDistance(foundThing, myPos) < LVector3.CityBlockDistance(pathingTarget, myPos))
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

        protected long lastSearchTime = 0;
        protected long millisBetweenSearch = 50;

        public void SetPathTarget(LVector3 pathingTarget)
        {
            this.pathingTarget = pathingTarget;
        }

        public abstract void UpdateBehavior(ref ThingDoing curDoing);

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

        public void DidEatObject(LVector3 block, float hungerGain)
        {
            hungerMeter = Mathf.Min(hungerMeterMax, hungerMeter + hungerGain);
        }

        public void Update()
        {
            if (world == null)
            {
                world = World.mainWorld;
            }
            if (world != null)
            {
                UpdateBehavior(ref this.thingDoing);
            }

        }
    }
}