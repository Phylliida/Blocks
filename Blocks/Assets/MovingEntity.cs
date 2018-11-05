using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEntity : MonoBehaviour {


    float bodyWidth = 0.5f;
    public float heightBelowHead = 2.5f;
    public float heightAboveHead = 0.2f;
    float feetWidth = 0.2f;
    public float speed = 5.0f;
    float gravity = 37.0f;
    float jumpSpeed = 14.0f;

    // Use this for initialization
    void Start () {
        desiredMove = Vector3.zero;
        usingShift = false;
	}

    Vector3 vel;


    public Vector3 desiredMove;
    public bool usingShift;
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


	// Update is called once per frame
	void Update () {
        
        Vector3 goodDiff;
        Vector3 desiredDiff = desiredMove * Time.deltaTime * speed;
        bool wasTouchingGround = TouchingLand();

        if (!IntersectingBody(desiredDiff, out goodDiff))
        {
            transform.position += goodDiff;
        }
        if (usingShift)
        {
            if (wasTouchingGround && !TouchingLand())
            {
                transform.position -= goodDiff;
            }
        }



        if (!TouchingLand())
        {
            vel -= Vector3.up * gravity * Time.deltaTime;
        }
        else
        {
            if (vel.y <= 0)
            {
                vel.y = 0.0f;
            }
            if (jumping)
            {
                vel.y = jumpSpeed;
            }

        }

        Vector3 velDiff = vel * Time.deltaTime;
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
                    transform.position = hitResults.hitPos + new Vector3(0, heightBelowHead - 0.01f, 0);
                }
                else
                {
                    transform.position += velDiff;
                }
            }
        }
    }


    bool IntersectingBody(Vector3 position)
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
        long bodyX = (long)Mathf.Floor(position.x / worldScale);
        long bodyY = (long)Mathf.Floor((position.y + heightAboveHead / 2.0f) / worldScale);
        long bodyZ = (long)Mathf.Floor(position.z / worldScale);
        int atHead = World.mainWorld[headX, headY, headZ];
        int atEyes = World.mainWorld[eyesX, eyesY, eyesZ];
        int atFeet = World.mainWorld[feetX, feetY, feetZ];
        int atBody = World.mainWorld[bodyX, bodyY, bodyZ];
        return atHead != 0 || atEyes != 0 || atFeet != 0 || atBody != 0;
        /*
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
        if (Vector3.Dot(goodOffset.normalized, desiredOffset.normalized) <= 0.0001f)
        {
            return true;
        }
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
