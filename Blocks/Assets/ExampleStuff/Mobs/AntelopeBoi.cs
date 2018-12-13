using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Example_pack;
using Blocks;

public class AntelopeBoi : MobWithBehavior
{
    public override void UpdateBehavior(ref ThingDoing curDoing)
    {
        if (curDoing == null)
        {
            curDoing = new ThingDoing(TypeOfThingDoing.Searching, null);
        }

        if (hungerMeter < wantToFindFoodThresh && curDoing.typeOfThing != TypeOfThingDoing.Chasing && curDoing.typeOfThing != TypeOfThingDoing.Searching)
        {
            curDoing = new ThingDoing(TypeOfThingDoing.Searching);
        }

        if (hungerMeter >= wantToFindFoodThresh && curDoing.typeOfThing != TypeOfThingDoing.RunningAway)
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
            if (PhysicsUtils.millis() - lastSearchTime > millisBetweenSearch)
            {
                LVector3 foundThing;
                if (Search(out foundThing))
                {
                    // found it
                    curDoing = new ThingDoing(TypeOfThingDoing.Chasing, new ThingDoingTarget(foundThing));
                    //Debug.Log("found thing in " + steps + " steps");
                }
                else
                {
                    // did not find
                    //Debug.Log("did not find thing in " + steps + " steps");
                }
                lastSearchTime = PhysicsUtils.millis();
            }
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
}