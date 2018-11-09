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

public struct LVector3
{
    public long x, y, z;
    public LVector3(long x, long y, long z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int Block
    {
        get
        {
            return World.mainWorld[this];
        }
        set
        {
            World.mainWorld[this] = value;
        }
    }

    public int State1
    {
        get
        {
            return World.mainWorld.GetState(x, y, z, 1);
        }
        set
        {
            World.mainWorld.SetState(x, y, z, value, 1);
        }
    }



    public int State2
    {
        get
        {
            return World.mainWorld.GetState(x, y, z, 2);
        }
        set
        {
            World.mainWorld.SetState(x, y, z, value, 2);
        }
    }

    public static LVector3 operator *(long a, LVector3 b)
    {
        return new LVector3(b.x * a, b.y * a, b.z * a);
    }
    public static LVector3 operator *(LVector3 a, long b)
    {
        return b * a;
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
    public static LVector3 FromUnityVector3(Vector3 vec)
    {
        return new LVector3((long)Mathf.Floor(vec.x / World.mainWorld.worldScale), (long)Mathf.Floor(vec.y / World.mainWorld.worldScale), (long)Mathf.Floor(vec.z / World.mainWorld.worldScale));
    }

    public Vector3 BlockCentertoUnityVector3()
    {
        return new Vector3(x * World.mainWorld.worldScale, y* World.mainWorld.worldScale, z * World.mainWorld.worldScale) + Vector3.one * 0.5f * World.mainWorld.worldScale;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj is LVector3)
        {
            LVector3 item = (LVector3)obj;
            return (this.x == item.x) && (this.y == item.y) && (this.z == item.z);
        }
        else
        {
            return false;
        }
    }

    public bool Equals(LVector3 other)
    {
        return (other.x == x) && (other.y == y) && (other.z == z);
    }

    // see https://msdn.microsoft.com/en-us/library/ms173147.aspx
    public static bool operator ==(LVector3 a, LVector3 b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        // Return true if the fields match:
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(LVector3 a, LVector3 b)
    {
        return !(a == b);
    }

    public static long CityBlockDistance(LVector3 a, LVector3 b)
    {
        return (long)(Vector3.Distance(a.BlockCentertoUnityVector3(), b.BlockCentertoUnityVector3())*10.0f);
        return System.Math.Abs(a.x - b.x) + System.Math.Abs(a.y - b.y) + System.Math.Abs(a.z - b.z);
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


public class RaycastResults
{
    public Vector3 hitPos;
    public LVector3 hitBlock;
    public LVector3 blockBeforeHit;
    public float dist;
    public Vector3 normal;

    public RaycastResults(Vector3 hitPos, LVector3 hitBlock, LVector3 blockBeforeHit, float dist, Vector3 normal)
    {
        this.hitPos = hitPos;
        this.hitBlock = hitBlock;
        this.blockBeforeHit = blockBeforeHit;
        this.dist = dist;
        this.normal = normal;
    }
}


public class PhysicsUtils  {



    // from https://stackoverflow.com/a/4016511/2924421
    public static long millis()
    {
        return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }
    public static bool MouseCast(Camera camera, float playerWidth, float maxDist, out RaycastResults hitResults, int maxSteps=-1)
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.01f));

        Vector3 origin = ray.origin + ray.direction * -playerWidth / 2.0f;
        return RayCast(origin, ray.direction, maxDist, out hitResults, maxSteps);
    }

    public delegate bool IsBlockValidForSearch(int block, long x, long y, long z);


    static int searchCounter = int.MaxValue;

    static int[] visited;
    static int curMaxSteps;

    static Queue<int> xOffsets;
    static Queue<int> yOffsets;
    static Queue<int> zOffsets;
    public static int xStep, yStep, zStep;
    public static void InitializeGlobalSearchVars(int maxSteps)
    {
        searchCounter -= 1;
        if (curMaxSteps < maxSteps || visited == null || searchCounter == 0)
        {
            curMaxSteps = maxSteps;
            visited = new int[(maxSteps*2+1) * (maxSteps * 2 + 1) * (maxSteps * 2 + 1)];
            if (searchCounter <= 0)
            {
                searchCounter = int.MaxValue;
            }
            xStep = 1;
            yStep = (maxSteps * 2 + 1);
            zStep = (maxSteps * 2 + 1) * (maxSteps * 2 + 1);
        }
        if (xOffsets == null)
        {
            xOffsets = new Queue<int>(maxSteps);
        }
        if (yOffsets == null)
        {
            yOffsets = new Queue<int>(maxSteps);
        }
        if (zOffsets == null)
        {
            zOffsets = new Queue<int>(maxSteps);
        }
        xOffsets.Clear();
        yOffsets.Clear();
        zOffsets.Clear();
    }

    // from https://stackoverflow.com/a/34363187/2924421
    public void to3D(int ind, int width, out int x, out int y, out int z)
    {
        z = ind / (width);
        ind -= (z * width);
        y = ind / width;
        x = ind % width;
    }

    public static int to1D(int x, int y, int z, int width)
    {
        return x + y * width + z * width*width;
    }

    public static bool RunOnNeighbor(long curX, long curY, long curZ, int ind, int xOffset, int yOffset, int zOffset, int nx, int ny, int nz, IsBlockValidForSearch isBlockValid, IsBlockValidForSearch isBlockDesiredResult)
    {
        // if is not visited
        if (visited[ind + nx * xStep + ny * yStep + nz * zStep] != searchCounter)
        {
            // mark as visited
            visited[ind + nx * xStep + ny * yStep + nz * zStep] = searchCounter;
            int neighborBlock = World.mainWorld[curX + nx, curY + ny, curZ + nz];

            // if is desired result, done
            if (isBlockDesiredResult(neighborBlock, curX + nx, curY + ny, curZ + nz))
            {
                return true;
            }

            // if is valid add to quue
            if (isBlockValid(neighborBlock, curX + nx, curY + ny, curZ + nz))
            {
                xOffsets.Enqueue(xOffset + nx);
                yOffsets.Enqueue(yOffset + ny);
                zOffsets.Enqueue(zOffset + nz);
            }
        }
        return false;
    }
    public static int blockPreference = 0;
    public static bool SearchOutwards(LVector3 start, int maxSteps, bool searchUp, bool searchDown, IsBlockValidForSearch isBlockValid, IsBlockValidForSearch isBlockDesiredResult)
    {
        World world = World.mainWorld;
        InitializeGlobalSearchVars(maxSteps);
        xOffsets.Enqueue(0);
        yOffsets.Enqueue(0);
        zOffsets.Enqueue(0);
        int nSteps = 0;
        blockPreference = (blockPreference + 1) % 4;
        while (xOffsets.Count != 0)
        {
            int xOffset = xOffsets.Dequeue();
            int yOffset = yOffsets.Dequeue();
            int zOffset = zOffsets.Dequeue();

            int xOffsetShifted = xOffset + maxSteps+1;
            int yOffsetShifted = yOffset + maxSteps+1;
            int zOffsetShifted = zOffset + maxSteps+1;
            int ind = to1D(xOffsetShifted, yOffsetShifted, zOffsetShifted, yStep);
            
            long curX = start.x + xOffset;
            long curY = start.y + yOffset;
            long curZ = start.z + zOffset;

            int nx, ny, nz;
            
            if (blockPreference == 0)
            {
                nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }
            else if(blockPreference == 1)
            {
                nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }
            else if(blockPreference == 2)
            {
                nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }
            else if(blockPreference == 3)
            {
                nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
                nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }

            if (searchDown)
            {
                nx = 0; ny = -1; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }

            if (searchUp)
            {
                nx = 0; ny = 1; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult)) return true;
            }

            // if not visited yet

            nSteps += 1;
            if (nSteps >= maxSteps)
            {
                break;
            }
        }

        return false;

    }

    public static bool IsBlockSolid(int block)
    {
        return block != World.AIR && block != World.WATER && block != World.WATER_NOFLOW;
    }

    public static bool RayCast(Vector3 origin, Vector3 dir, float maxDist, int maxSteps = -1)
    {
        RaycastResults results;
        return RayCast(origin, dir, maxDist, out results, maxSteps);
    }

    // Works by hopping to nearest plane in dir, then looking at block inside midpoint between cur and next. If a block is there, we use the step before cur to determine direction (unless first step, in which case step before next should be right)
    public static bool RayCast(Vector3 origin, Vector3 dir, float maxDist, out RaycastResults hitResults, int maxSteps = -1)
    {
        dir = dir.normalized;
        hitResults = null;
        float worldScale = World.mainWorld.worldScale;
        LVector3 hitPos;
        LVector3 posBeforeHit;
        Vector3 surfaceHitPos;
        Vector3 normal;
        long camX = (long)Mathf.Floor(origin.x / worldScale);
        long camY = (long)Mathf.Floor(origin.y / worldScale);
        long camZ = (long)Mathf.Floor(origin.z / worldScale);
        int prevMinD = 0;
        // start slightly back in case we are very very close to a block
        float[] curPosF = new float[] { origin.x, origin.y, origin.z };
        LVector3 curPosL = LVector3.FromUnityVector3(new Vector3(curPosF[0], curPosF[1], curPosF[2]));
        float[] rayDir = new float[] { dir.x, dir.y, dir.z };
        hitPos = new LVector3(camX, camY, camZ);
        surfaceHitPos = new Vector3(origin.x, origin.y, origin.z);
        posBeforeHit = new LVector3(camX, camY, camZ);
        normal = new Vector3(1, 1, 1).normalized;
        if (maxSteps == -1)
        {
            maxSteps = 1000;
        }
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
                        nextWall = Mathf.Ceil(curPosF[d] / worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
                        {
                            nextWall += 1.0f;
                        }
                        offsetSign = 1;
                    }
                    else
                    {
                        nextWall = Mathf.Floor(curPosF[d] / worldScale);
                        if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
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
                    float dist = Mathf.Abs(curPosF[d] / worldScale - nextWall);
                    float tToPlane = dist / Mathf.Abs(rayDir[d]);
                    if (tToPlane > maxDist * World.mainWorld.worldScale) // too far
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
            if (minT > maxDist*World.mainWorld.worldScale)
            {
                //Debug.Log("too big " + minT + " greater than maxDist " + maxDist);
                return false;
            }
            // step towards it and check if block
            Vector3 prevPosF = new Vector3(curPosF[0], curPosF[1], curPosF[2]);
            Vector3 resPosF = prevPosF + dir * minT * worldScale;
            Vector3 midPoint = (prevPosF + resPosF) / 2.0f;
            LVector3 newCurPosL = LVector3.FromUnityVector3(midPoint);
            //Debug.Log("stepped from " + curPosL + " and cur pos (" + curPosF[0] + "," + curPosF[1] + "," + curPosF[2] + ") to point " + newCurPosL + " and cur pos " + resPosF);
            curPosL = LVector3.FromUnityVector3(resPosF);

            if (Vector3.Distance(prevPosF, origin) > maxDist) // too far
            {
                return false;
            }

            if (IsBlockSolid(World.mainWorld[newCurPosL.x, newCurPosL.y, newCurPosL.z]))
            {
                hitPos = newCurPosL;
                surfaceHitPos = prevPosF;
                posBeforeHit = new LVector3(newCurPosL.x, newCurPosL.y, newCurPosL.z);
                posBeforeHit[prevMinD] -= (long)Mathf.Sign(rayDir[prevMinD]);
                float[] normalArr = new float[] { 0, 0, 0 };
                normalArr[prevMinD] = -Mathf.Sign(rayDir[prevMinD]);
                if (Mathf.Sign(rayDir[prevMinD]) == 0)
                {
                    Debug.Log("spooky why u 0");
                    normalArr[prevMinD] = 0.0f;
                }
                normal = new Vector3(normalArr[0], normalArr[1], normalArr[2]);


                hitResults = new RaycastResults(surfaceHitPos, hitPos, posBeforeHit, Vector3.Distance(prevPosF, origin), normal);
                return true;
            }
            prevMinD = minD;
            curPosF[0] = resPosF.x;
            curPosF[1] = resPosF.y;
            curPosF[2] = resPosF.z;
        }
        return false;
    }
}
