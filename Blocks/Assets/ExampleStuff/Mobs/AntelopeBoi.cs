using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Example_pack;
using Blocks;

public class AntelopeBoi : MobWithBehavior
{
    public override BaseGenome GetBaseGenome()
    {
        BaseGenome res = new BaseGenome();
        // required ones
        res.AddKey("baseHunger", 2.0f, 2.0f);
        res.AddKey("baseStamina", 2.0f, 2.0f);
        res.AddKey("maxStamina", 5.0f, 10.0f);
        res.AddKey("maxFood", 5.0f, 10.0f);

        // for weighting behaviors
        res.AddKey("easilySpooked", 0.5f, 0.9f);
        res.AddKey("curiosity", 0.2f, 0.4f);
        res.AddKey("extrovertednes", 0.3f, 0.7f);
        return res;
    }

    public override float GetBaseWeight(TypeOfThingDoing thingDoing)
    {
        if (thingDoing == TypeOfThingDoing.GettingFood) return 0.0f;
        else if (thingDoing == TypeOfThingDoing.RunningAway) return 0.0f;
        else if (thingDoing == TypeOfThingDoing.Standing) return 1.0f - genome["curiosity"];
        else if (thingDoing == TypeOfThingDoing.Socializing) return genome["extrovertednes"];
        else if (thingDoing == TypeOfThingDoing.Wandering) return genome["curiosity"];
        return 0.0f;
    }

    public override float GetWeight(TypeOfThingDoing thingDoing)
    {
        if (thingDoing == TypeOfThingDoing.GettingFood) return 1.0f - food / maxFood;
        //else if (thingDoing == TypeOfThingDoing.RunningAway) return genome["easilySpooked"];
        //else if (thingDoing == TypeOfThingDoing.Standing) return 1.0f - genome["curiosity"];
        //else if (thingDoing == TypeOfThingDoing.Socializing) return genome["extrovertednes"];
        //else if (thingDoing == TypeOfThingDoing.Wandering) return genome["curiosity"];
        return 0.0f;
    }

    public override bool IsImportant(TypeOfThingDoing thingDoing, ref float maxValue)
    {
        if (thingDoing == TypeOfThingDoing.GettingFood)
        {
            maxValue = 30.0f;
            return true;
        }
        else if (thingDoing == TypeOfThingDoing.RunningAway)
        {
            maxValue = 30.0f;
            return true;
        }
        //else if (thingDoing == TypeOfThingDoing.Standing) return 1.0f - genome["curiosity"];
        //else if (thingDoing == TypeOfThingDoing.Socializing) return genome["extrovertednes"];
        //else if (thingDoing == TypeOfThingDoing.Wandering) return genome["curiosity"];
        return false;
    }

    /*
    public override void UpdateBehavior(ref ThingDoing curDoing)
    {
        
        if (curDoing == null)
        {
            curDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
        }

        if (food < wantToFindFoodThresh && curDoing.typeOfThing != TypeOfThingDoing.Chasing && curDoing.typeOfThing != TypeOfThingDoing.Searching)
        {
            curDoing = new ThingDoing(TypeOfThingDoing.Searching);
        }

        if (food >= wantToFindFoodThresh && curDoing.typeOfThing != TypeOfThingDoing.RunningAway)
        {
            curDoing = new ThingDoing(TypeOfThingDoing.Standing);
        }


        BlocksPlayer[] players = FindObjectsOfType<BlocksPlayer>();

        foreach (BlocksPlayer player in players)
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 10.0f)
            {
                curDoing = new ThingDoing(TypeOfThingDoing.RunningAway, new ThingDoingTarget(player.GetComponent<MovingEntity>()));
            }
        }

        MovingEntity self = GetComponent<MovingEntity>();
        if (curDoing.typeOfThing == TypeOfThingDoing.Chasing)
        {
            if (curDoing.target.entity == null)
            {
                pathingTarget = curDoing.target.block;
            }
            else
            {
                pathingTarget = LVector3.FromUnityVector3(curDoing.target.entity.transform.position);
            }


            if (pathingTarget.BlockV != Example.FlowerWithNectar)
            {
                curDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
            }
            else
            {
                float myDist = LVector3.CityBlockDistance(LVector3.FromUnityVector3(transform.position), pathingTarget);
                // found it, eat it
                if (myDist < 4)
                {
                    DidEatObject(pathingTarget, 1.0f);
                    world[pathingTarget] = (int)Example.Flower;
                    curDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
                }
                // still pathing to it
                else
                {
                    UpdatePathing();
                }
            }
            //pathingTarget = LVector3.FromUnityVector3(FindObjectOfType<BlocksPlayer>().transform.position);
        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.RunningAway)
        {
            if (curDoing.target.entity == null)
            {
                pathingTarget = curDoing.target.block;
            }
            else
            {
                pathingTarget = LVector3.FromUnityVector3(curDoing.target.entity.transform.position);
            }
        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.Searching)
        {
        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.Sitting)
        {

        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.Socializing)
        {

        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.Standing)
        {
            GetComponent<CrocodileDoggy>().SetSpeed(0.0f);
            self.desiredMove = Vector3.zero;
        }
        else if (curDoing.typeOfThing == TypeOfThingDoing.Wandering)
        {

        }
    }

    */

    public override ThingDoing UpdateBehavior(TypeOfThingDoing newTypeOfThingDoing)
    {
        if (newTypeOfThingDoing == TypeOfThingDoing.RunningAway)
        {
            newTypeOfThingDoing = TypeOfThingDoing.GettingFood;
        }
        return new ThingDoing(newTypeOfThingDoing, null);
    }

    public override void OnReachFoodBlock(LVector3 foodBlock)
    {
        if (foodBlock.Block == Example.FlowerWithNectar)
        {
            this.food = Mathf.Min(this.food + 1.0f, maxFood);
            foodBlock.Block = Example.Flower;
        }
    }


    public override void OnSearchForFood(out bool lookForBlock, out BlockValue lookForBlockValue)
    {
        lookForBlock = true;
        lookForBlockValue = Example.FlowerWithNectar;
    }
}