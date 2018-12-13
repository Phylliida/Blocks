using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blocks;

public class CrocodileDoggy : MonoBehaviour {

    public Transform leftFrontLeg;
    public Transform rightFrontLeg;
    public Transform leftBackLeg;
    public Transform rightBackLeg;

    public float legSpeed = 0.1f;
    float offsetAmount = 10.0f;
    float actualLegSpeed = 0.1f;
    public enum AnimationStateOfMe
    {
        Walking,
        Standing,
        Jumping
    }

    public AnimationStateOfMe animationState;
    // Use this for initialization
    void Start () {
		
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="speed">From 0 to 1, 0 is stopped, 1 is as fast as possible</param>
    public void SetSpeed(float speed)
    {
        actualLegSpeed = speed * 15.0f;
        if (speed == 0.0f)
        {
            animationState = AnimationStateOfMe.Standing;
        }
        else
        {
            animationState = AnimationStateOfMe.Walking;
        }
    }
	
	// Update is called once per frame
	void Update () {

        MovingEntity moving = GetComponent<MovingEntity>();
        if (moving != null)
        {
            if (moving.desiredMove.magnitude > 0)
            {
                transform.forward = moving.desiredMove*0.1f + transform.forward*0.9f;
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                SetSpeed(moving.desiredMove.magnitude / moving.speed);
                SetSpeed(1.0f);
            }
            else
            {
                SetSpeed(0.0f);
            }
        }

        if (animationState == AnimationStateOfMe.Walking)
        {
            float minLegRot = 0;
            float maxLegRot = 20;
            Vector3 resEulerAngles = new Vector3(0, 0, 0);
            leftFrontLeg.transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.time * actualLegSpeed + 0 * offsetAmount / 4.0f) * (maxLegRot - minLegRot) + minLegRot);
            rightFrontLeg.transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.time * actualLegSpeed + 1 * offsetAmount / 4.0f) * (maxLegRot - minLegRot) + minLegRot);
            leftBackLeg.transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.time * actualLegSpeed + 2 * offsetAmount / 4.0f) * (maxLegRot - minLegRot) + minLegRot);
            rightBackLeg.transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.time * actualLegSpeed + 3 * offsetAmount / 4.0f) * (maxLegRot - minLegRot) + minLegRot);
        }
        else
        {
            leftFrontLeg.transform.localEulerAngles = new Vector3(0, 0, 0);
            rightFrontLeg.transform.localEulerAngles = new Vector3(0, 0, 0);
            leftBackLeg.transform.localEulerAngles = new Vector3(0, 0, 0);
            rightBackLeg.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}
