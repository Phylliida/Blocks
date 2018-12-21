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


        public float[] actionWeights = new float[(int)TypeOfThingDoing.MaxValue];
        public float[] actionBaseWeights = new float[(int)TypeOfThingDoing.MaxValue];
        public bool[] isImportant = new bool[(int)TypeOfThingDoing.MaxValue];
        public float[] importantMaxValues = new float[(int)TypeOfThingDoing.MaxValue];


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
            Standing=0,
            Wandering=1,
            GettingFood=2,
            RunningAway=3,
            Socializing=4,
            MaxValue=5
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

        float pathfindsPerSecond = 0.5f;

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


        public static int frameUpdatedLast = 0;
        public void UpdatePathing()
        {
            MovingEntity body = GetComponent<MovingEntity>();
            if (pathingTarget == null)
            {
                body.desiredMove = Vector3.zero;
            }
            if (PhysicsUtils.millis() - lastPathfind > 1000.0 / pathfindsPerSecond && frameUpdatedLast != Time.frameCount) // offset so everyone isn't aligned on the same frame
            {
                LVector3 myPos = LVector3.FromUnityVector3(transform.position);
                LVector3 foundThing;
                if (Search(out foundThing))
                {
                    // found it, is it closer?
                    if (pathingTarget == null || LVector3.CityBlockDistance(foundThing, myPos) < LVector3.CityBlockDistance(pathingTarget, myPos))
                    {
                        // if so, go to it instead
                        thingDoing = new ThingDoing(TypeOfThingDoing.GettingFood, new ThingDoingTarget(foundThing));
                        pathingTarget = foundThing;
                    }
                    //Debug.Log("found thing in " + steps + " steps");
                }
                else
                {
                    // did not find
                    //Debug.Log("did not find thing in " + steps + " steps");
                }
                frameUpdatedLast = Time.frameCount;
                //Debug.Log("updating pathing");
                RaycastResults blockStandingOn;
                if (pathingTarget != null && PhysicsUtils.RayCastAlsoHitWater(body.transform.position, -Vector3.up, 20.0f, out blockStandingOn))
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


                if (pathingTarget.BlockV != Example.FlowerWithNectar)
                {
                    this.thingDoing = new ThingDoing(TypeOfThingDoing.GettingFood, null);
                }
                else
                {
                    float myDist = LVector3.CityBlockDistance(LVector3.FromUnityVector3(transform.position), pathingTarget);
                    // found it, eat it
                    if (myDist < 2)
                    {
                        OnReachFoodBlock(pathingTarget);
                        /*
                        DidEatObject(pathingTarget, 1.0f);
                        world[pathingTarget] = (int)Example.Flower;
                        */
                        this.thingDoing = new ThingDoing(TypeOfThingDoing.GettingFood, null);
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

        public bool Search(out LVector3 found)
        {
            found = new LVector3();
            if (lookingForBlock)
            {
                RaycastResults res;
                for (int i = 0; i < 10; i++)
                {
                    if (PhysicsUtils.CustomRaycast(transform.position, Random.onUnitSphere, 100.0f, (b, bx, by, bz, pbx, pby, pbz) => { return true; }, (b, bx, by, bz, pbx, pby, pbz) => { return b == blockLookingFor; }, out res))
                    {
                        found = res.hitBlock;
                        return true;
                    }
                }
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

            float c = (Mathf.Sqrt(a + 4) + Mathf.Sqrt(a)) / (2 * Mathf.Sqrt(a));
            float b = -1.0f / c;

            return 1.0f / (-x + c) + b;
        }

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

            float totalWeight = 0.0f;
            for (int i = 0; i < (int)TypeOfThingDoing.MaxValue; i++)
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
                thingDoing = UpdateBehavior(chosenThingDoing);
                if (thingDoing.typeOfThing == TypeOfThingDoing.GettingFood)
                {
                    OnSearchForFood(out lookingForBlock, out blockLookingFor);
                }
            }



            if (thingDoing.typeOfThing == TypeOfThingDoing.GettingFood || thingDoing.typeOfThing == TypeOfThingDoing.RunningAway)
            {
                UpdatePathing();
            }

        }
    }
}