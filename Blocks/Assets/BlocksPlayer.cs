using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This allows us to have a very large world while not having any rounding error worse than if we were in -16.0f to 16.0f
// Unfortunately unity still requires us to output a floating point position for the camera, but internally we will work with this
// We can get around unity's limitation by moving the world around, which while that is supported by moving the rendering root, I don't like it because multiplayer/multiple entities could cause weird issues
// so we will just do this, and accept that rendering rounding error will happen for now
// we could use doubles here, but unity does everything with floats and so I'll stick with floats to avoid the annoyance of always having to remember to worry about precision issues when converting
public class PVector3
{
    public static float chunkSize = 16.0f;

    public long cx, cy, cz;
    public float lx, ly, lz;

    public PVector3(long cx, long cy, long cz, float lx, float ly, float lz)
    {
        this.cx = cx; this.cy = cy; this.cz = cz;
        this.lx = lx; this.ly = ly; this.lz = lz;
    }

    public Vector3 ToVec()
    {
        return new Vector3(chunkSize * cx + lx, chunkSize * cy + ly, chunkSize * cz + lz);
    }

    public long chunk(int d)
    {
        if (d == 0)
        {
            return cx;
        }
        else if (d == 1)
        {
            return cy;
        }
        else if (d == 2)
        {
            return cz;
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("we are only working in 3 dimensions (0,1, or 2) but d=" + d + " which is not 0,1, or 2");
        }
    }
    public float localPos(int d)
    {
        if (d == 0)
        {
            return lx;
        }
        else if (d == 1)
        {
            return ly;
        }
        else if (d == 2)
        {
            return lz;
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("we are only working in 3 dimensions (0,1, or 2) but d=" + d + " which is not 0,1, or 2");
        }
    }
}

public class LVector3
{
    public long x, y, z;
    public LVector3(long x, long y, long z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }


    public static LVector3 operator +(LVector3 a, LVector3 b)
    {
        return new LVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static LVector3 operator -(LVector3 a, LVector3 b)
    {
        return a + (-b);
    }
    public static LVector3 operator -(LVector3 a)
    {
        return new LVector3(-a.x, -a.y, -a.z);
    }

    public static LVector3 FromUnityVector3(BlocksWorld world, Vector3 vec)
    {
        return new LVector3((long)Mathf.Floor(vec.x / world.worldScale), (long)Mathf.Floor(vec.y / world.worldScale), (long)Mathf.Floor(vec.z / world.worldScale));
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() == this.GetType())
        {
            LVector3 other = (LVector3)obj;
            return other.x == x && other.y == y && other.z == z;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }
    public long this[int index]
    {
        get
        {
            if (index == 0)
            {
                return x;
            }
            else if (index == 1)
            {
                return y;
            }
            else if (index == 2)
            {
                return z;
            }
            else
            {
                throw new System.IndexOutOfRangeException("index of LVector 3 needs to be 0,1, or 2, is " + index + " instead");
            }
        }

        set
        {
            if (index == 0)
            {
                x = value;
            }
            else if (index == 1)
            {
                y = value;
            }
            else if (index == 2)
            {
                z = value;
            }
            else
            {
                throw new System.IndexOutOfRangeException("index of LVector 3 needs to be 0,1, or 2, is " + index + " instead");
            }
        }
    }
    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }
}
public class BlocksPlayer : MonoBehaviour {

    public SmoothMouseLook mouseLook;

    public float heightBelowHead = 1.8f;
    public float heightAboveHead = 0.2f;
    public float playerWidth = 0.5f;
    public float feetWidth = 0.1f;
    public float playerSpeed = 3.0f;
    public float gravity = 20.0f;
    public float jumpSpeed = 7.0f;
    public float reachRange = 6.0f;
    public BlocksWorld world;
    public Vector3 vel;
    public Camera mainCamera;

	// Use this for initialization
	void Start () {
        vel = Vector3.zero;

    }


    bool IntersectingBody(Vector3 desiredOffset, out Vector3 goodOffset)
    {
        goodOffset = desiredOffset;
        for (int i = 0; i < 20; i++)
        {
            float p = i / (20 - 1.0f);
            float xDiff = Mathf.Sin(p * 2 * Mathf.PI);
            float yDiff = Mathf.Cos(p * 2 * Mathf.PI);
            Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized* playerWidth;
            if (Vector3.Dot(desiredOffset, curOffset) > 0) // if that offset is in the same direction we are going, test to see if it moves us into a block
            {
                if (IntersectingBodyExceptFeet(transform.position + curOffset))
                {
                    // vector rejection
                    goodOffset = goodOffset - Vector3.Project(goodOffset, curOffset);
                }
            }
        }
        if (Vector3.Dot(goodOffset, desiredOffset) <= 0.0001f)
        {
            return true;
        }
        return false;
    }

    bool TouchingLand()
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
            Vector3 curOffset = (new Vector3(xDiff, 0, yDiff)).normalized* feetWidth;
            if (TouchingLand(transform.position + curOffset, false))
            {
                return true;
            }
        }
        return false;
    }


    bool IntersectingHead(Vector3 position)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.9f) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        return atHead != 0 || atEyes != 0;

    }
    bool IntersectingBodyExceptFeet(Vector3 position)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead * 0.8f) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        return atHead != 0 || atEyes != 0 || atFeet != 0;
    }


    bool TouchingLand(Vector3 position, bool moveFeet)
    {
        long eyesX = (long)Mathf.Floor(position.x / world.worldScale);
        long eyesY = (long)Mathf.Floor(position.y / world.worldScale);
        long eyesZ = (long)Mathf.Floor(position.z / world.worldScale);
        long feetX = (long)Mathf.Floor(position.x / world.worldScale);
        long feetY = (long)Mathf.Floor((position.y - heightBelowHead) / world.worldScale);
        long feetZ = (long)Mathf.Floor(position.z / world.worldScale);
        long headX = (long)Mathf.Floor(position.x / world.worldScale);
        long headY = (long)Mathf.Floor((position.y + heightAboveHead) / world.worldScale);
        long headZ = (long)Mathf.Floor(position.z / world.worldScale);
        int atHead = world.world[headX, headY, headZ];
        int atEyes = world.world[eyesX, eyesY, eyesZ];
        int atFeet = world.world[feetX, feetY, feetZ];
        Vector3 resPos = transform.position/world.worldScale;
        bool touching = false;
        if (atFeet != 0)
        {
            //if (moveFeet) { resPos.y = feetY + 1 + heightBelowHead / world.worldScale; }
            touching = true;

            if (moveFeet)
            {
                transform.position = resPos * world.worldScale;
            }
        }
        return touching;

    }

    // Works by hopping to nearest plane in dir, then looking at block inside midpoint between cur and next. If a block is there, we use the step before cur to determine direction (unless first step, in which case step before next should be right)
    bool MouseCast(out LVector3 hitPos, float maxDist, out LVector3 posBeforeHit)
    {

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.01f));

        Vector3 origin = ray.origin + ray.direction * -playerWidth / 2.0f;
        long camX = (long)Mathf.Floor(origin.x / world.worldScale);
        long camY = (long)Mathf.Floor(origin.y / world.worldScale);
        long camZ = (long)Mathf.Floor(origin.z / world.worldScale);
        int prevMinD = 0;
        // start slightly back in case we are very very close to a block
        float[] curPosF = new float[] { origin.x, origin.y, origin.z };
        LVector3 curPosL = LVector3.FromUnityVector3(world, new Vector3(curPosF[0], curPosF[1], curPosF[2]));
        float[] rayDir = new float[] { ray.direction.x, ray.direction.y, ray.direction.z };
        hitPos = new LVector3(camX, camY, camZ);
        posBeforeHit = new LVector3(camX, camY, camZ);
        int maxSteps = 10;
        for (int i = 0; i < maxSteps; i++)
        {
            float minT = float.MaxValue;
            int minD = -1;

            // find first plane we will hit
            for (int d = 0; d < curPosF.Length; d++)
            {
                // call velocity = rayDir[d]
                // dist = velocity*time
                // time = dist/velocity
                if (rayDir[d] != 0)
                {
                    int offsetSign;
                    float nextWall;
                    if (rayDir[d] > 0)
                    {
                        nextWall = Mathf.Ceil(curPosF[d]/world.worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d]/world.worldScale) < 0.0001f)
                        {
                            nextWall += 1.0f;
                        }
                        offsetSign = 1;
                    }
                    else
                    {
                        nextWall = Mathf.Floor(curPosF[d] / world.worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d] / world.worldScale) < 0.0001f)
                        {
                            nextWall -= 1.0f;
                        }
                        offsetSign = -1;
                    }
                    long dest;
                    long offset;
                    /*
                    // if on first step, try just going to the next face without an offset if it is ahead
                    if (Mathf.Sign(curPosL[d] - curPosF[d] / world.worldScale) == offsetSign && i == 0)
                    {
                        dest = curPosL[d];
                    }
                    */


                    //float nearestPointFive = Mathf.Round(curPosF[d]/world.worldScale) + offsetSign * 0.5f;
                    //float dist = Mathf.Abs(curPosF[d] / world.worldScale - nearestPointFive);
                    //float dist = Mathf.Abs(curPosF[d] / world.worldScale - dest);
                    float dist = Mathf.Abs(curPosF[d]/world.worldScale - nextWall);
                    float tToPlane = dist / Mathf.Abs(rayDir[d]);
                    if (tToPlane > maxDist) // too far
                    {
                        continue;
                    }

                    //Debug.Log("testing with tToPlane = " + tToPlane + " and camPos=" + curPosF[d] + " and dest=" + nextWall + " and dist=" + dist + " and hitPos = " + hitPos + " and minT = " + minT + " and curPosL = " + curPosL  + " and rayDir[d] = " + rayDir[d]);
                    // if tToX (actual dist) is less than furthest block hit so far, this might be a good point, check if there is a block there
                    if (tToPlane < minT)
                    {
                        minT = tToPlane;
                        minD = d;
                        if (i == 0)
                        {
                            prevMinD = minD;
                        }
                    }
                }
            }
            if (minT > maxDist)
            {
                //Debug.Log("too big " + minT + " greater than maxDist " + maxDist);
                return false;
            }
            // step towards it and check if block
            Vector3 prevPosF = new Vector3(curPosF[0], curPosF[1], curPosF[2]);
            Vector3 resPosF = prevPosF + ray.direction * minT*world.worldScale;
            Vector3 midPoint = (prevPosF + resPosF) / 2.0f;
            LVector3 newCurPosL = LVector3.FromUnityVector3(world, midPoint);
            //Debug.Log("stepped from " + curPosL + " and cur pos (" + curPosF[0] + "," + curPosF[1] + "," + curPosF[2] + ") to point " + newCurPosL + " and cur pos " + resPosF);
            curPosL = LVector3.FromUnityVector3(world, resPosF);
            if (world.world[newCurPosL.x, newCurPosL.y, newCurPosL.z] != 0)
            {
                hitPos = newCurPosL;
                posBeforeHit = new LVector3(newCurPosL.x, newCurPosL.y, newCurPosL.z);
                posBeforeHit[prevMinD] -= (long)Mathf.Sign(rayDir[prevMinD]);
                return true;
            }
            prevMinD = minD;
            curPosF[0] = resPosF.x;
            curPosF[1] = resPosF.y;
            curPosF[2] = resPosF.z;
        }
        return false;
    }

    bool MouseCastOldBad(out LVector3 hitPos, float maxDist, out LVector3 posBeforeHit)
    {

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.01f));

        long camX = (long)Mathf.Floor(ray.origin.x / world.worldScale);
        long camY = (long)Mathf.Floor(ray.origin.y / world.worldScale);
        long camZ = (long)Mathf.Floor(ray.origin.z / world.worldScale);

        float[] camPosF = new float[] { ray.origin.x, ray.origin.y, ray.origin.z };
        float[] rayDir = new float[] { ray.direction.x, ray.direction.y, ray.direction.z };
        LVector3 camPosL = new LVector3(camX, camY, camZ);
        hitPos = new LVector3(camX, camY, camZ);
        posBeforeHit = new LVector3(camX, camY, camZ);
        float minT = float.MaxValue;

        int maxSteps = 10;
        // do raycast for each dim (plane at x+1, x+2, ..., then plane at y+1, y+2, ..., etc.) and find nearest hit that has a block
        for (int d = 0; d < camPosF.Length; d++)
        {
            // call velocity = rayDir[d]
            // dist = velocity*time
            // time = dist/velocity
            if (rayDir[d] != 0)
            {
                int offsetSign;
                if (rayDir[d] > 0)
                {
                    offsetSign = 1;
                }
                else
                {
                    offsetSign = -1;
                }
                for (int i = 0; i < maxSteps; i++)
                {
                    long dest;
                    long offset;
                    dest = camPosL[d] + i * offsetSign;
                    float dist = Mathf.Abs(camPosF[d] / world.worldScale - dest);
                    float tToPlane = dist / Mathf.Abs(rayDir[d]);
                    tToPlane += world.worldScale * 0.1f; // go a little further because of rounding issues
                    if (tToPlane > maxDist) // too far
                    {
                        break;
                    }

                    Debug.Log("testing with tToPlane = " + tToPlane + " and camPos=" + ray.origin + " and dest=" + dest +" and dist=" + dist + " and hitPos = " + hitPos + " and minT = " + minT + " and camPosL = " + camPosL + " and rayDir[d] = " + rayDir[d]);
                    // if tToX (actual dist) is less than furthest block hit so far, this might be a good point, check if there is a block there
                    if (tToPlane < minT)
                    {
                        Vector3 resPtF = (ray.direction * tToPlane + ray.origin/ world.worldScale);
                        LVector3 resPt = new LVector3((long)Mathf.Floor(resPtF.x), (long)Mathf.Floor(resPtF.y), (long)Mathf.Floor(resPtF.z));
                        Debug.Log("smaller at res pt " + resPt + " and resPtF " + resPtF);
                        if (resPt == camPosL)
                        {
                            continue;
                        }
                        if (world.world[resPt.x, resPt.y, resPt.z] != 0) // if there is a block there
                        {
                            Debug.Log("hit at res pt " + resPt + " and resPtF " + resPtF + " and dim " + d);
                            // store this as closest hit so far
                            minT = tToPlane;
                            hitPos = resPt;
                            posBeforeHit = new LVector3(resPt.x, resPt.y, resPt.z);
                            posBeforeHit[d] -= offsetSign;
                            // break and move onto next dim since in this dim we are closest
                            break;
                        }
                    }
                }
            }
        }

        bool didHit;
        if (minT != float.MaxValue)
        {
            didHit = true;
        }
        else
        {
            didHit = false;
        }
        return didHit;
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && mouseLook.prevCapturing)
        {
            //Debug.Log("made click");

            LVector3 hitPos;
            LVector3 posBeforeHit;

            if (MouseCast(out hitPos, reachRange/world.worldScale, out posBeforeHit))
            {
                //Debug.Log("hit at pos " + hitPos);
                world.world[hitPos.x, hitPos.y, hitPos.z] = 0;
            }
            else
            {

                //Debug.Log("mouse cast failed " + hitPos);
            }
        }

        if (Input.GetMouseButtonDown(1) && mouseLook.prevCapturing)
        {
            //Debug.Log("made click");

            LVector3 hitPos;
            LVector3 posBeforeHit;

            if (MouseCast(out hitPos, reachRange / world.worldScale, out posBeforeHit))
            {
                //Debug.Log("hit at pos " + hitPos);
                world.world[posBeforeHit.x, posBeforeHit.y, posBeforeHit.z] = World.GRASS;
            }
            else
            {

                //Debug.Log("mouse cast failed " + hitPos);
            }
        }
        Pose curPose = new Pose(transform.position, transform.rotation);
        curPose.rotation = Quaternion.Euler(0, curPose.rotation.eulerAngles.y, 0);
        Vector3 desiredPosition = transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            desiredPosition += curPose.forward * playerSpeed*Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            desiredPosition -= curPose.forward * playerSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            desiredPosition -= curPose.right * playerSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            desiredPosition += curPose.right * playerSpeed * Time.deltaTime;
        }

        Vector3 desiredDiff = desiredPosition - transform.position;
        Vector3 goodDiff;
        if (!IntersectingBody(desiredDiff, out goodDiff))
        {
            transform.position += goodDiff;
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
            if (Input.GetKey(KeyCode.Space))
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
            transform.position += velDiff;
        }
    }
}
