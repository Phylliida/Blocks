using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{


    public class BaseGenome
    {
        public Dictionary<string, Tuple<float, float>> keys = new Dictionary<string, Tuple<float, float>>();

        public BaseGenome()
        {

        }

        public void AddKey(string key, float minVal, float maxVal)
        {
            keys[key] = new Tuple<float, float>(minVal, maxVal);
        }
    }

    public class Genome
    {
        public FastSmallDictionary<string, float> keys;
        BaseGenome baseGenome;
        public Genome(BaseGenome baseGenome)
        {
            this.baseGenome = baseGenome;
            keys = new FastSmallDictionary<string, float>(baseGenome.keys.Count);
            foreach (KeyValuePair<string, Tuple<float, float>> key in baseGenome.keys)
            {
                keys[key.Key] = Random.Range(key.Value.a, key.Value.b);
            }
        }

        public Genome Clone()
        {
            Genome res = new Genome(baseGenome);
            foreach (KeyValuePair<string, Tuple<float, float>> key in baseGenome.keys)
            {
                res[key.Key] = this[key.Key];
            }
            return res;
        }

        /// <summary>
        /// Makes a new genome will all values offset by a random value in [-rate*valueRange,rate*valueRange] clamped to min and max valid values
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public Genome Mutate(float rate)
        {
            Genome res = new Genome(baseGenome);
            foreach (KeyValuePair<string, Tuple<float, float>> key in baseGenome.keys)
            {
                float minVal = key.Value.a;
                float maxVal = key.Value.b;

                float myVal01 = (this[key.Key] - minVal) / (maxVal - minVal);
                if (maxVal == minVal)
                {
                    myVal01 = 1.0f;
                }
                float randNeg11 = Random.value * 2 - 1; // in [-1,1]
                myVal01 = Mathf.Clamp01(myVal01 + randNeg11 * rate);
                float resVal = myVal01 * (maxVal - minVal) + minVal;
                res.keys[key.Key] = resVal;
            }
            return res;
        }

        public Genome Breed(Genome other, float mutationRate)
        {
            Genome res = new Genome(baseGenome);
            foreach (KeyValuePair<string, Tuple<float, float>> key in baseGenome.keys)
            {
                float rVal = Random.value;
                // randomly weighted average from their value and ours
                res[key.Key] = this[key.Key] * rVal + other[key.Key] * (1 - rVal);
            }
            return res.Mutate(mutationRate);
        }

        public float this[string key]
        {
            get
            {
                return keys[key];
            }
            private set
            {

            }
        }
    }


    [RequireComponent(typeof(MovingEntity))]
    public abstract class MobWithBehavior : MonoBehaviour
    {


        float[] actionWeights = new float[(int)TypeOfThingDoing.MaxValue];
        float[] actionBaseWeights = new float[(int)TypeOfThingDoing.MaxValue];
        bool[] isImportant = new bool[(int)TypeOfThingDoing.MaxValue];
        float[] importantMaxValues = new float[(int)TypeOfThingDoing.MaxValue];


        public class ThingDoingTarget
        {
            public enum TypeOfThingDoingTarget
            {
                Block,
                Entity
            }
            public LVector3 block;
            public MovingEntity entity;
            public BlockValue[] validBlocks;

            public ThingDoingTarget(LVector3 block, BlockValue[] validBlocks=null)
            {
                this.block = block;
                this.validBlocks = validBlocks;
            }

            public ThingDoingTarget(BlockValue[] validBlocks)
            {
                this.validBlocks = validBlocks;
                this.block = LVector3.Invalid;
            }

            public ThingDoingTarget(MovingEntity entity)
            {
                this.entity = entity;
                this.block = LVector3.Invalid;
            }
        }

        public enum TypeOfThingDoing
        {
            Standing=0,
            Wandering=1,
            GettingFood=2,
            RunningAway=3,
            Socializing=4,
            GoingTo=5,
            Gathering= 6,
            MaxValue = 7
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


        public ThingDoing thingDoing;

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

        /// <summary>
        /// Required values:
        /// baseFood
        /// maxFood
        /// baseStamina
        /// maxStamina
        /// </summary>
        /// <returns></returns>
        public abstract BaseGenome GetBaseGenome();

        public Genome genome;

        public void Start()
        {
            if (world == null)
            {
                world = World.mainWorld;
            }

            thingDoing = new ThingDoing(TypeOfThingDoing.Standing, null);
            genome = new Genome(GetBaseGenome());

            food = genome["baseHunger"];
            stamina = genome["baseStamina"];
            maxStamina = genome["maxStamina"];
            maxFood = genome["maxFood"];


            for (int i = 0; i < actionWeights.Length; i++)
            {
                TypeOfThingDoing typeOfThing = (TypeOfThingDoing)i;
                float maxValue = 10.0f;
                if (IsImportant(typeOfThing, ref maxValue))
                {
                    isImportant[i] = true;
                    importantMaxValues[i] = maxValue;
                }
                else
                {
                    isImportant[i] = false;
                }
                actionBaseWeights[i] = GetBaseWeight(typeOfThing);
            }
        }



        PathNode curPath;

        float pathfindsPerSecond = 1f;

        long lastPathfind = 0;

        public LVector3 pathingTarget;

        public float baseHunger = 2.0f;
        /// <summary>
        /// Lower is more hungry, 0 is starving, hungerMeterMax is full
        /// </summary>
        public float food;
        public float maxFood = 10.0f;

        public float wantToFindFoodThresh;
        public float veryHungryThresh = 2.0f;

        public float baseStamina = 3.0f;
        public float stamina;
        public float maxStamina;

        public bool iNeedToPathfindAgain = true;

        public static int frameUpdatedLast = 0;
        public void UpdatePathing()
        {

            ////// new temp stuff
            MovingEntity body = GetComponent<MovingEntity>();
            //pathingTarget = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);

            /*
            // find position on ground below player and path to that instead
            int goDownAmount = 10; // don't get into infinite loop, give up eventually
            while (pathingTarget.BlockV == BlockValue.Air && goDownAmount > 0)
            {
                pathingTarget = pathingTarget - new LVector3(0, 1, 0);
                goDownAmount--;
            }
            pathingTarget += new LVector3(0, 2, 0);
            */
            //////

            pathingTarget = LVector3.Invalid;

            if (thingDoing != null)
            {
                if (thingDoing.target != null)
                {
                    if (thingDoing.target.entity != null)
                    {
                        pathingTarget = LVector3.FromUnityVector3(thingDoing.target.entity.transform.position);
                        iNeedToPathfindAgain = true;
                    }
                    else if(thingDoing.target.block != LVector3.Invalid)
                    {
                        pathingTarget = thingDoing.target.block;
                        //Debug.Log("going to pathing target with block in thing doing of " + pathingTarget);
                    }
                }
            }

            if (curPath == null)
            {
                iNeedToPathfindAgain = true;
            }

            if (pathingTarget == LVector3.Invalid)
            {
                body.desiredMove = Vector3.zero;
            }
            if (PhysicsUtils.millis() - lastPathfind > 1000.0 / pathfindsPerSecond && frameUpdatedLast != Time.frameCount && iNeedToPathfindAgain) // offset so everyone isn't aligned on the same frame
            {
                LVector3 myPos = LVector3.FromUnityVector3(transform.position);
                LVector3 foundThing;
                /*
                if (Search(out foundThing))
                {
                    // found it, is it closer?
                    if (pathingTarget == LVector3.Invalid || LVector3.CityBlockDistance(foundThing, myPos) < LVector3.CityBlockDistance(pathingTarget, myPos))
                    {
                        // if so, go to it instead
                        ThingDoing newThingDoing = new ThingDoing(thingDoing.typeOfThing, new ThingDoingTarget(foundThing));
                        // copy over valid blocks for future reference
                        if (thingDoing != null && thingDoing.target != null)
                        {
                            newThingDoing.target.validBlocks = thingDoing.target.validBlocks;
                        }
                        thingDoing = newThingDoing;
                        pathingTarget = foundThing;
                    }
                    //Debug.Log("found thing when looking");
                }
                else
                {
                    // did not find
                    //Debug.Log("did not find thing when looking");
                    //Debug.Log("did not find thing in " + steps + " steps");
                }
                */
                frameUpdatedLast = Time.frameCount;
                //Debug.Log("updating pathing");
                RaycastResults blockStandingOn;

                bool lookingForBlocks = false;
                if (thingDoing != null && thingDoing.target != null && thingDoing.target.entity == null && thingDoing.target.block == LVector3.Invalid && thingDoing.target.validBlocks != null)
                {
                    lookingForBlocks = true;
                }

                if (pathingTarget != LVector3.Invalid || lookingForBlocks)
                {
                    if (PhysicsUtils.RayCastAlsoHitWater(body.transform.position, -Vector3.up, 20.0f, out blockStandingOn))
                    {
                        // if we are using shift and standing over an empty block, but our feet are on a neighboring block, use that neighboring block for pathfinding instead
                        if (blockStandingOn != LVector3.Invalid)
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
                            //// new stuff
                            bool pathingSuccess = false;
                            PathingRegionPos resPath = null;
                            if (lookingForBlocks)
                            {
                                resPath = PathingChunk.PathindToResource(World.mainWorld,  myPos, GetComponent<MovingEntity>().reachRange, thingDoing.target.validBlocks, new MobilityCriteria(1, 1, blocksHeight, 1), out pathingSuccess, out blockWeCanSeeOnceWeGetThere, verbose: world.blocksWorld.verbosePathing);
                                if (resPath != null)
                                {
                                    pathingTarget = new LVector3(resPath.wx, resPath.wy, resPath.wz);
                                    thingDoing = new ThingDoing(thingDoing.typeOfThing, new ThingDoingTarget(pathingTarget, thingDoing.target.validBlocks));
                                }
                            }
                            else
                            {
                                resPath = BlocksPathing.Pathfind(World.mainWorld, myPos, pathingTarget, 1, 1, blocksHeight, 1, out pathingSuccess, verbose: false);
                            }
                            iShouldJump = false;
                            if (resPath != null)
                            {
                                curPath = (new PathingResult(resPath)).GetPathNode();

                                iNeedToPathfindAgain = false;

                                //Debug.Log("got path with next pos " + curPath.pos + " also, my pos is " + myPos + " and pathing target is " + pathingTarget);
                                if (curPath.nextNode != null && curPath.nextNode.pos.y > curPath.pos.y)
                                {
                                    iShouldJump = true;
                                }

                                if (curPath.nextNode != null)
                                {
                                    //curPath = curPath.nextNode;
                                }
                            }
                            else
                            {
                                curPath = null;
                                iNeedToPathfindAgain = true;
                            }

                            
                            /////
                            ///

                            // old thing:
                            //PhysicsUtils.Pathfind(blocksHeight, ref curPath, out iShouldJump, myPos, pathingTarget+new LVector3(0,1,0), 100);
                        }
                        if (curPath != null)
                        {
                            if (curPath.nextNode == null)
                            {
                                //Debug.Log("curPath = " + curPath.pos + " myPos = " + myPos + " next null");
                            }
                            else
                            {
                                //Debug.Log("curPath = " + curPath.pos + " myPos = " + myPos + " nextPath = " + curPath.nextNode.pos);
                            }
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
                }
                lastPathfind = PhysicsUtils.millis();
            }


            if (pathingTarget == LVector3.Invalid)
            {
                //Debug.Log("has invalid pathing target?");
                iNeedToPathfindAgain = true;
            }
            body.desiredMove = Vector3.zero;
            Vector3 targetPos = transform.position;
            if (curPath != null && pathingTarget != LVector3.Invalid)
            {
                /* // dani commented this out recently
                LVector3 myPos = LVector3.FromUnityVector3(transform.position);
                RaycastResults blockStandingOn;
                if (PhysicsUtils.RayCastAlsoHitWater(body.transform.position, -Vector3.up, 20.0f, out blockStandingOn))
                {
                    // if we are using shift and standing over an empty block, but our feet are on a neighboring block, use that neighboring block for pathfinding instead
                   myPos = blockStandingOn.hitBlock + new LVector3(0, blocksHeight, 0);
                }
                LVector3 myPosBeforeJump = myPos - new LVector3(0, 1, 0);
                LVector3 myPosBeforeFall = myPos + new LVector3(0, 1, 0);
                */








                //if (curPath.prevNode != null && (myPos == curPath.prevNode.pos || myPosBeforeJump == curPath.prevNode.pos))
                //{

                //}
                //else 

                /*
                PathNode closest = curPath;
                float closestDist = LVector3.EuclideanDistance(closest.pos, myPos);
                PathNode curTmp = closest;
                while(curTmp.prevNode != null)
                {
                    curTmp = curTmp.prevNode;
                    float tmpDist = LVector3.EuclideanDistance(curTmp.pos, myPos);
                    if (tmpDist < closestDist)
                    {
                        closest = curTmp;
                        closestDist = tmpDist;
                    }
                }
                curTmp = curPath;
                while (curTmp.nextNode != null)
                {
                    curTmp = curTmp.nextNode;
                    float tmpDist = LVector3.EuclideanDistance(curTmp.pos, myPos);
                    if (tmpDist < closestDist)
                    {
                        closest = curTmp;
                        closestDist = tmpDist;
                    }
                }

                if (closest.pos == myPos)
                {
                    if (closest.nextNode == null)
                    {
                        Debug.Log("reached end of path");
                    }
                    else
                    {
                        curPath = closest.nextNode;
                    }
                }
                else
                {
                    curPath = closest;
                }*/


                /* // dani commented this out recently
                if (curPath.nextNode != null && (myPos == curPath.nextNode.pos || myPosBeforeJump == curPath.nextNode.pos || myPosBeforeFall == curPath.nextNode.pos))
                {
                    curPath = curPath.nextNode;
                }
                else if (myPos == curPath.pos || myPosBeforeJump == curPath.pos || myPosBeforeFall == curPath.pos)
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
                */


                Vector3 dirToMove;
                bool pushedOffPath;
                curPath = curPath.GetCurNode(transform.position, out body.jumping, out dirToMove, out pushedOffPath);
                body.SetAbsoluteDesiredMove(dirToMove);

                if (pushedOffPath)
                {
                    //Debug.Log("pushed off path");
                    //iNeedToPathfindAgain = true;
                }

                if (curPath.nextNode != null)
                {
                    //Debug.Log("got cur path " + curPath.pos + " with jumping " + body.jumping + " and dir to move " + dirToMove + " and next " + curPath.nextNode.pos + " and pushed off path " + pushedOffPath);
                }
                else
                {
                    //Debug.Log("got cur path " + curPath.pos + " with jumping " + body.jumping + " and dir to move " + dirToMove + " and no next and pushed off path " + pushedOffPath);
                }


                BlockValue blockWeAreGoingTo = blockWeCanSeeOnceWeGetThere.BlockV;

                if (!DesiredBlock(blockWeAreGoingTo) && thingDoing != null && thingDoing.target != null && thingDoing.target.entity == null)
                {
                    Debug.Log("rip someone took my block , my block is now " + World.BlockToString(blockWeAreGoingTo));
                    curPath = null;
                    pathingTarget = LVector3.Invalid;
                }
                else
                {
                    float myDist = LVector3.CityBlockDistance(LVector3.FromUnityVector3(transform.position), pathingTarget);
                    // found it, eat it
                    if (myDist <= 2)
                    {
                        Debug.Log("got to desired block");
                        if (thingDoing != null && thingDoing.typeOfThing == TypeOfThingDoing.GettingFood)
                        {
                            OnReachFoodBlock(blockWeCanSeeOnceWeGetThere);
                        }
                        else if(thingDoing != null && thingDoing.typeOfThing == TypeOfThingDoing.Gathering)
                        {
                            OnReachBlockToGather(blockWeCanSeeOnceWeGetThere);
                        }
                        /*
                        DidEatObject(pathingTarget, 1.0f);
                        world[pathingTarget] = (int)Example.Flower;
                        */
                        //this.thingDoing = new ThingDoing(TypeOfThingDoing.GettingFood, null);
                        if (thingDoing != null && thingDoing.target != null)
                        {
                            this.thingDoing = new ThingDoing(thingDoing.typeOfThing, new ThingDoingTarget(thingDoing.target.validBlocks));
                        }
                        else if (thingDoing != null)
                        {
                            this.thingDoing = new ThingDoing(thingDoing.typeOfThing, null);
                        }
                        curPath = null;
                        pathingTarget = LVector3.Invalid;
                        body.jumping = false;
                        iNeedToPathfindAgain = true;
                    }
                    // still pathing to it
                    else
                    {
                    }
                }
            }
        }

        protected long lastSearchTime = 0;
        protected long millisBetweenSearch = 50;

        public void SetPathTarget(LVector3 pathingTarget)
        {
            this.pathingTarget = pathingTarget;
        }

        public abstract ThingDoing UpdateBehavior(TypeOfThingDoing newTypeOfThingDoing);


        bool DesiredBlock(BlockValue block)
        {
            if (thingDoing != null && thingDoing.target != null && thingDoing.target.validBlocks != null)
            {
                for (int j = 0; j < thingDoing.target.validBlocks.Length; j++)
                {
                    if (thingDoing.target.validBlocks[j] == block)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return block == blockLookingFor;
            }
        }

        public bool Search(out LVector3 found)
        {
            found = LVector3.Invalid;
            if (lookingForBlock || true)
            {
                RaycastResults res;
                for (int i = 0; i < 50; i++)
                {
                    if (PhysicsUtils.CustomRaycast(transform.position, Random.onUnitSphere, 20.0f, (b, bx, by, bz, pbx, pby, pbz) => { return true; }, (b, bx, by, bz, pbx, pby, pbz) => 
                    
                        {
                            return DesiredBlock(b);
                        }, out res))
                    {
                        found = res.hitBlock;
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log("not looking for block but called search?");
            }
            return false;
        }

        public void DidEatObject(LVector3 block, float hungerGain)
        {
            food = Mathf.Min(maxFood, food + hungerGain);
        }

        public abstract float GetBaseWeight(TypeOfThingDoing thingDoing);
        public abstract float GetWeight(TypeOfThingDoing thingDoing);
        public abstract bool IsImportant(TypeOfThingDoing thingDoing, ref float maxValue);
        public abstract void OnSearchForFood(out bool lookForBlock, out BlockValue lookForBlockValue);
        public abstract void OnReachFoodBlock(LVector3 foodBlock);
        public abstract void OnReachBlockToGather(LVector3 gatherBlock);

        public IEnumerable<TypeOfThingDoing> TypesOfThingsToDo()
        {
            for (int i = 0; i < (int)TypeOfThingDoing.MaxValue; i++)
            {
                yield return (TypeOfThingDoing)i;
            }
        }


        public TypeOfThingDoing typeOfThingDoing;
        bool lookingForBlock = false;
        BlockValue blockLookingFor;
        LVector3 blockWeCanSeeOnceWeGetThere;

        public float zeroOneThing(float x, float a)
        {
            // handling bad input cases
            if (a <= 0)
            {
                return a;
            }

            // regular cases:

            // we want something that has
            // f(0) = 0
            // f(1) = maxVal
            // smoothly between, but not linear
            // concave up
            // I like
            // f(x) = 1/(-x+c) + b
            // so if you solve c and b s.t. f(0) = 0 and f(1) = maxVal
            // you get 
            // f(0) = 1/c + b
            // f(0) = 1/

            double c = (System.Math.Sqrt(a + 4) + System.Math.Sqrt(a)) / (2 * System.Math.Sqrt(a));
            double b = -1.0 / c;

            return (float)(1.0 / (-x + c) + b);
        }



        public class DoEveryMS
        {
            long ms;
            long randVariance = 0;
            long timeWhenLastDid;
            public DoEveryMS(long ms, long randVariance=0)
            {
                this.ms = ms;
                this.timeWhenLastDid = 0;
            }

            public bool Do()
            {
                long curTime = PhysicsUtils.millis();
                if (curTime - timeWhenLastDid > ms)
                {
                    timeWhenLastDid = curTime;

                    // randomly wait +- randVariance*[random value in 0-1] until the next
                    if (randVariance != 0)
                    {
                        long randOff = (long)((Random.value * 2.0f - 1.0f) * randVariance);
                        timeWhenLastDid += randOff;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        DoEveryMS updateWander = new DoEveryMS(100, 100);
        DoEveryMS updateAction = new DoEveryMS(2000, 100);

        public void Update()
        {
            this.typeOfThingDoing = this.thingDoing.typeOfThing;
            if (world == null)
            {
                world = World.mainWorld;

                if (world == null) // not initialized yet, wait
                {
                    return;
                }
            }

            if (updateAction.Do())
            {
                //Debug.Log("updating action");
                float totalWeight = 0.0f;
                for (int i = 0; i < actionWeights.Length; i++)
                {
                    actionWeights[i] = actionBaseWeights[i] + GetWeight((TypeOfThingDoing)i);
                    if (isImportant[i])
                    {
                        actionWeights[i] = zeroOneThing(actionWeights[i], importantMaxValues[i]);
                    }
                    totalWeight += actionWeights[i];
                }
                if (totalWeight <= 0.0f)
                {
                    totalWeight = 1.0f;
                }

                // randomly sample proportional to weights
                float randVal = Random.value;
                float cumulSum = 0.0f;
                int chosenThing = 0;
                for (int i = 0; i < (int)TypeOfThingDoing.MaxValue; i++)
                {
                    cumulSum += actionWeights[i] / totalWeight;
                    if (randVal <= cumulSum)
                    {
                        chosenThing = i;
                        break;
                    }
                }

                int curAct = (int)thingDoing.typeOfThing;
                float curWeight = actionWeights[curAct];
                float chosenWeight = actionWeights[chosenThing];

                float diff = curWeight - chosenWeight;
                // if current is much more likely, don't change
                if (diff > 1.0f)
                {

                }
                // otherwise, change with diff pr (if diff < 0 this means always change)
                else/* if (diff < 0)
                {
                    TypeOfThingDoing chosenThingDoing = (TypeOfThingDoing)chosenThing;
                    thingDoing = UpdateBehavior(chosenThingDoing);
                    if (thingDoing.typeOfThing == TypeOfThingDoing.GettingFood)
                    {
                        OnSearchForFood(out lookingForBlock, out blockLookingFor);
                    }
                }
                else if (Random.value < diff)*/
                {
                    TypeOfThingDoing chosenThingDoing = (TypeOfThingDoing)chosenThing;
                    ThingDoing prevThingDoing = thingDoing;
                    thingDoing = UpdateBehavior(chosenThingDoing);
                    if (thingDoing != prevThingDoing)
                    {
                        iNeedToPathfindAgain = true;
                    }
                    if (thingDoing.typeOfThing == TypeOfThingDoing.GettingFood)
                    {
                        OnSearchForFood(out lookingForBlock, out blockLookingFor);
                    }
                }
            }


            if (thingDoing.typeOfThing == TypeOfThingDoing.GettingFood || thingDoing.typeOfThing == TypeOfThingDoing.RunningAway || thingDoing.typeOfThing == TypeOfThingDoing.Gathering || thingDoing.typeOfThing == TypeOfThingDoing.GoingTo)
            {
                UpdatePathing();
            }
            else if(thingDoing.typeOfThing == TypeOfThingDoing.Socializing)
            {
                thingDoing.typeOfThing = TypeOfThingDoing.Wandering;
            }
            else if(thingDoing.typeOfThing == TypeOfThingDoing.Standing)
            {
                GetComponent<MovingEntity>().desiredMove = Vector3.zero;
            }
            else if (thingDoing.typeOfThing == TypeOfThingDoing.Wandering)
            {
                if (updateWander.Do())
                {
                    //int randVal = Random.Range(0, 5);
                    Vector3[] options = new Vector3[] { Vector3.zero, Vector3.forward, Vector3.right, Vector3.left, Vector3.back };
                    Vector3 desiredMove = options[Random.Range(0, options.Length)];
                    MovingEntity me = GetComponent<MovingEntity>();
                    me.desiredMove = Vector3.zero;
                    me.jumping = false;
                    RaycastResults blockStandingOn = me.BlockStandingOn();
                    if (blockStandingOn != null && desiredMove != Vector3.zero)
                    {
                        LVector3 myPos = blockStandingOn.hitBlock + new LVector3(0, 1, 0);
                        LVector3 nextPos = myPos + new LVector3((long)desiredMove.x, (long)desiredMove.y, (long)desiredMove.z);
                        LVector3 belowNextPos = nextPos + new LVector3(0, -1, 0);
                        LVector3 aboveNextPos = nextPos + new LVector3(0, 1, 0);

                        bool needsToJump = false;
                        bool okayToMove = false;
                        bool needsToShift = false;
                        // we need to hop up
                        if (nextPos.Block != Example.Air)
                        {
                            // we can hop up
                            if (aboveNextPos.Block == Example.Air)
                            {
                                okayToMove = true;
                                needsToJump = true;
                            }
                            // we can't hop up
                            else
                            {
                            }
                        }
                        // we won't run into anything and can walk there
                        else
                        {
                            // going down
                            if (belowNextPos.Block == Example.Air)
                            {
                                LVector3 belowBelowNextPos = belowNextPos + new LVector3(0, -1, 0);
                                // too far to fall, don't do it
                                if (belowBelowNextPos.Block == Example.Air)
                                {

                                }
                                // down stairs, that's fine
                                else
                                {
                                    okayToMove = true;
                                }
                            }
                            // just horizontal, that's fine
                            else
                            {
                                okayToMove = true;
                            }
                        }


                        me.jumping = needsToJump;
                        me.usingShift = needsToShift;
                        if (okayToMove)
                        {
                            me.desiredMove = desiredMove;
                        }
                    }

                }
            }

        }
    }
}