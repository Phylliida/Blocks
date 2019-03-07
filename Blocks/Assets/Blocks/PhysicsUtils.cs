using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Blocks
{
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


    public class PathNode
    {
        public LVector3 pos;
        public PathNode prevNode;
        public PathNode nextNode;


        public PathNode GetCurNode(Vector3 myPos, out bool shouldJump, out Vector3 directionToMove, out bool pushedOffPath)
        {
            LVector3 myLPos = LVector3.FromUnityVector3(myPos);
            PathNode curNode = this;
            PathNode posMovingTo;
            pushedOffPath = false;
            // at current position
            if (pos == myLPos)
            {
                posMovingTo = nextNode; 
            }
            else
            {
                // try moving back, find closest
                PathNode tmp = this;
                PathNode closest = tmp;
                float closestDist = Vector3.Distance(tmp.pos.BlockCentertoUnityVector3(),myPos);
                while (tmp.pos != myLPos && tmp.prevNode != null)
                {
                    tmp = tmp.prevNode;
                    float curDist = Vector3.Distance(tmp.pos.BlockCentertoUnityVector3(),myPos);
                    if (curDist < closestDist)
                    {
                        closestDist = curDist;
                        closest = tmp;
                    }
                }
                while (tmp.pos != myLPos && tmp.nextNode != null)
                {
                    tmp = tmp.nextNode;
                    float curDist = Vector3.Distance(tmp.pos.BlockCentertoUnityVector3(), myPos);
                    if (curDist < closestDist)
                    {
                        closestDist = curDist;
                        closest = tmp;
                    }
                }

                if (closest != myLPos)
                {
                    pushedOffPath = true;
                }
                else
                {
                    pushedOffPath = false;
                }
                curNode = closest;
                posMovingTo = closest.nextNode;
            }

            shouldJump = false;

            if (posMovingTo == null)
            {
                directionToMove = Vector3.zero;
            }
            else
            {
                if (posMovingTo.pos.y > curNode.pos.y)
                {
                    shouldJump = true;
                }
                else
                {
                    shouldJump = false;
                }
                Vector3 dirToMove = posMovingTo.pos.BlockCentertoUnityVector3() - myPos;
                directionToMove = new Vector3(dirToMove.x, 0, dirToMove.z).normalized;
            }



            if (pushedOffPath && curNode.pos.y > myLPos.y)
            {
                shouldJump = true;
            }

            return curNode;
        }

        public PathNode(LVector3 pos, PathNode prevNode)
        {
            this.pos = pos;
            this.prevNode = prevNode;
        }
    }


    public struct LVector3
    {


        public static LVector3 Invalid = new LVector3(-10000000000L, -10000000000L, -10000000000L);

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


        public BlockValue BlockV
        {
            get
            {
                return (BlockValue)World.mainWorld[this];
            }
            set
            {
                World.mainWorld[this] = (int)value;
            }
        }

        public int State1
        {
            get
            {
                return World.mainWorld.GetState(x, y, z, BlockState.State);
            }
            set
            {
                World.mainWorld.SetState(x, y, z, value, BlockState.State);
            }
        }



        public int LightingState
        {
            get
            {
                return World.mainWorld.GetState(x, y, z, BlockState.Lighting);
            }
            set
            {
                World.mainWorld.SetState(x, y, z, value, BlockState.Lighting);
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
            return new Vector3(x * World.mainWorld.worldScale, y * World.mainWorld.worldScale, z * World.mainWorld.worldScale) + Vector3.one * 0.5f * World.mainWorld.worldScale;
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

        public static bool operator ==(object a, LVector3 b)
        {
            return b == a;
        }

        public static bool operator !=(object a, LVector3 b)
        {
            return !(a == b);
        }

        public static bool operator ==(LVector3 a, object b)
        {
            if (((object)a) == null && b == null)
            {
                return true;
            }

            if (b == null)
            {
                return false;
            }

            if (b is LVector3)
            {
                LVector3 item = (LVector3)b;
                return (a.x == item.x) && (a.y == item.y) && (a.z == item.z);
            }
            else
            {
                return false;
            }
        }



        public static bool operator !=(LVector3 a, object b)
        {
            return !(a == b);
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

        public static float EuclideanDistance(LVector3 a, LVector3 b)
        {
            LVector3 diff = a - b;
            return diff.BlockCentertoUnityVector3().magnitude;
        }
        public static long CityBlockDistance(LVector3 a, LVector3 b)
        {
            //return (long)(Vector3.Distance(a.BlockCentertoUnityVector3(), b.BlockCentertoUnityVector3()));
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


    public class PhysicsUtils
    {



        // from https://stackoverflow.com/a/4016511/2924421
        public static long millis()
        {
            return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        }
        public static bool MouseCast(Camera camera, float playerWidth, float maxDist, out RaycastResults hitResults, int maxSteps = -1)
        {
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0.01f));

            Vector3 origin = ray.origin + ray.direction * -playerWidth / 2.0f;
            return RayCast(origin, ray.direction, maxDist, out hitResults, maxSteps);
        }

        public delegate bool IsBlockValidForSearch(int block, long x, long y, long z, long prevX, long prevY, long prevZ);


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
                visited = new int[(maxSteps * 2 + 1) * (maxSteps * 2 + 1) * (maxSteps * 2 + 1)];
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
            return x + y * width + z * width * width;
        }

        public static bool RunOnNeighbor(long curX, long curY, long curZ, int ind, int xOffset, int yOffset, int zOffset, int nx, int ny, int nz, IsBlockValidForSearch isBlockValid, IsBlockValidForSearch isBlockDesiredResult, GetBlock getBlock)
        {
            // if is not visited
            if (visited[ind + nx * xStep + ny * yStep + nz * zStep] != searchCounter)
            {
                // mark as visited
                visited[ind + nx * xStep + ny * yStep + nz * zStep] = searchCounter;
                int neighborBlock;
                if (getBlock == null)
                {
                    neighborBlock = World.mainWorld[curX + nx, curY + ny, curZ + nz];
                }
                else
                {
                    neighborBlock = getBlock(curX + nx, curY + ny, curZ + nz);
                }


                // if is desired result, done
                if (isBlockDesiredResult(neighborBlock, curX + nx, curY + ny, curZ + nz, curX, curY, curZ))
                {
                    return true;
                }

                // if is valid add to quue
                if (isBlockValid(neighborBlock, curX + nx, curY + ny, curZ + nz, curX, curY, curZ))
                {
                    xOffsets.Enqueue(xOffset + nx);
                    yOffsets.Enqueue(yOffset + ny);
                    zOffsets.Enqueue(zOffset + nz);
                }
            }
            return false;
        }
        public static int blockPreference = 0;

        public delegate int GetBlock(long x, long y, long z);

        public static bool SearchOutwards(LVector3 start, int maxSteps, bool searchUp, bool searchDown, IsBlockValidForSearch isBlockValid, IsBlockValidForSearch isBlockDesiredResult, GetBlock getBlock = null)
        {
            //World world = World.mainWorld;
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

                int xOffsetShifted = xOffset + maxSteps + 1;
                int yOffsetShifted = yOffset + maxSteps + 1;
                int zOffsetShifted = zOffset + maxSteps + 1;
                int ind = to1D(xOffsetShifted, yOffsetShifted, zOffsetShifted, yStep);

                long curX = start.x + xOffset;
                long curY = start.y + yOffset;
                long curZ = start.z + zOffset;

                int nx, ny, nz;

                if (blockPreference == 0)
                {
                    nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                }
                else if (blockPreference == 1)
                {
                    nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                }
                else if (blockPreference == 2)
                {
                    nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                }
                else if (blockPreference == 3)
                {
                    nx = 0; ny = 0; nz = -1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = -1; ny = 0; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                    nx = 0; ny = 0; nz = 1; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                }

                if (searchDown)
                {
                    nx = 0; ny = -1; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
                }

                if (searchUp)
                {
                    nx = 0; ny = 1; nz = 0; if (RunOnNeighbor(curX, curY, curZ, ind, xOffset, yOffset, zOffset, nx, ny, nz, isBlockValid, isBlockDesiredResult, getBlock)) return true;
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


        public static float fmod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        public static bool IsBlockLiquid(int block)
        {
            return block == Example.Water || block == Example.WaterNoFlow;
        }
        public static bool IsBlockSolid(int block)
        {
            return block != (int)BlockValue.Air && block != (int)Example.Water && block != (int)Example.WaterNoFlow && block != BlockValue.Wildcard;
        }

        public static bool RayCast(Vector3 origin, Vector3 dir, float maxDist, int maxSteps = -1)
        {
            RaycastResults results;
            return RayCast(origin, dir, maxDist, out results, maxSteps);
        }

        public static void ModChunkPos(long cx, long cy, long cz, out long mcx, out long mcy, out long mcz)
        {
            mcx = cx;
            mcy = cy;
            mcz = cz;
            return;
            //mcx = GoodMod(cx, 1);
            //mcy = GoodMod(cy, 1);
            //mcz = GoodMod(cz, 1);
        }

        public static void ModPos(long x, long y, long z, out long mx, out long my, out long mz)
        {
            mx = x;
            my = y;
            mz = z;
            return;
            //mx = GoodMod(x, BlocksWorld.chunkSize * 1);
            //my = GoodMod(y, BlocksWorld.chunkSize * 1);
            //mz = GoodMod(z, BlocksWorld.chunkSize * 1);
        }

        // fixes issues with a being negative giving negative answers, this is the mathematical mod I prefer
        public static int GoodMod(int a, int b)
        {
            int res = a % b;
            if (res < 0)
            {
                res = b + res;
            }
            return res;
        }


        // fixes issues with a being negative giving negative answers, this is the mathematical mod I prefer
        public static long GoodMod(long a, long b)
        {
            long res = a % b;
            if (res < 0)
            {
                res = b + res;
            }
            return res;
        }



        // Works by hopping to nearest plane in dir, then looking at block inside midpoint between cur and next. If a block is there, we use the step before cur to determine direction (unless first step, in which case step before next should be right)
        public static bool RayCastAlsoHitWater(Vector3 origin, Vector3 dir, float maxDist, out RaycastResults hitResults, int maxSteps = -1)
        {
            return CustomRaycast(origin, dir, maxDist, (b, bx, by, bz, pbx, pby, pbz) => { return b == BlockValue.Air; }, (b, bx, by, bz, pbx, pby, pbz) => { return b != BlockValue.Air; }, out hitResults, maxSteps: maxSteps);
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
            //LVector3 curPosL = LVector3.FromUnityVector3(new Vector3(curPosF[0], curPosF[1], curPosF[2]));
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
                        //int offsetSign;
                        float nextWall;
                        if (rayDir[d] > 0)
                        {
                            nextWall = Mathf.Ceil(curPosF[d] / worldScale);
                            if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
                            {
                                nextWall += 1.0f;
                            }
                            //offsetSign = 1;
                        }
                        else
                        {
                            nextWall = Mathf.Floor(curPosF[d] / worldScale);
                            if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
                            {
                                nextWall -= 1.0f;
                            }
                            //offsetSign = -1;
                        }
                        //long dest;
                        //long offset;
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
                if (minT > maxDist * World.mainWorld.worldScale)
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
                //curPosL = LVector3.FromUnityVector3(resPosF);

                if (Vector3.Distance(prevPosF, origin) > maxDist) // too far
                {
                    return false;
                }
                //Debug.Log("stepped from " + curPosL + " and cur pos (" + curPosF[0] + "," + curPosF[1] + "," + curPosF[2] + ") to point " + newCurPosL + " and cur pos " + resPosF + " with distance " + Vector3.Distance(prevPosF, origin) + " and max distance " + maxDist);


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



        // Works by hopping to nearest plane in dir, then looking at block inside midpoint between cur and next. If a block is there, we use the step before cur to determine direction (unless first step, in which case step before next should be right)
        public static bool CustomRaycast(Vector3 origin, Vector3 dir, float maxDist, IsBlockValidForSearch isBlockValid, IsBlockValidForSearch isBlockDesiredResult, out RaycastResults hitResults, int maxSteps = -1)
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
            LVector3 prevPosLTested = curPosL;
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
                        //int offsetSign;
                        float nextWall;
                        if (rayDir[d] > 0)
                        {
                            nextWall = Mathf.Ceil(curPosF[d] / worldScale);
                            if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
                            {
                                nextWall += 1.0f;
                            }
                            //offsetSign = 1;
                        }
                        else
                        {
                            nextWall = Mathf.Floor(curPosF[d] / worldScale);
                            if (Mathf.Abs(nextWall - curPosF[d] / worldScale) < 0.0001f)
                            {
                                nextWall -= 1.0f;
                            }
                            //offsetSign = -1;
                        }
                        //long dest;
                        //long offset;
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
                if (minT > maxDist * World.mainWorld.worldScale)
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
                //Debug.Log("stepped from " + curPosL + " and cur pos (" + curPosF[0] + "," + curPosF[1] + "," + curPosF[2] + ") to point " + newCurPosL + " and cur pos " + resPosF + " with distance " + Vector3.Distance(prevPosF, origin) + " and max distance " + maxDist);



                if (isBlockDesiredResult(newCurPosL.Block, newCurPosL.x, newCurPosL.y, newCurPosL.z, prevPosLTested.x, prevPosLTested.y, prevPosLTested.z))
                //if (IsBlockSolid(World.mainWorld[newCurPosL.x, newCurPosL.y, newCurPosL.z]))
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


                if (!isBlockValid(newCurPosL.Block, newCurPosL.x, newCurPosL.y, newCurPosL.z, prevPosLTested.x, prevPosLTested.y, prevPosLTested.z))
                {
                    return false;
                }

                prevPosLTested = newCurPosL;

                prevMinD = minD;
                curPosF[0] = resPosF.x;
                curPosF[1] = resPosF.y;
                curPosF[2] = resPosF.z;
            }
            return false;
        }





        static World world { get { return World.mainWorld; } }

        public static bool isValidForPathfind(int blocksHeight, LVector3 prevPos, LVector3 pos)
        {
            bool isCurrentlyValid = true;
            for (int i = 0; i < blocksHeight; i++)
            {
                isCurrentlyValid = isCurrentlyValid && world[pos.x, pos.y - i, pos.z] == (int)BlockValue.Air;
            }
            // bad:
            // X  
            //    2
            // 1
            // good:
            // 
            //     2
            // 1 
            if (pos.y > prevPos.y)
            {
                isCurrentlyValid = isCurrentlyValid && world[prevPos.x, prevPos.y + 1, prevPos.z] == (int)BlockValue.Air;
            }
            else if (pos.y < prevPos.y)
            {
                isCurrentlyValid = isCurrentlyValid && world[pos.x, pos.y + 1, pos.z] == (int)BlockValue.Air;
            }
            if (prevPos.y == pos.y && pos.z != prevPos.z && pos.x != prevPos.x)
            {
                isCurrentlyValid = isCurrentlyValid && (world[prevPos.x, prevPos.y, pos.z] == (int)BlockValue.Air || world[pos.x, prevPos.y, prevPos.z] == (int)BlockValue.Air);
            }
            return isCurrentlyValid && world[pos.x, pos.y - blocksHeight, pos.z] != (int)BlockValue.Air;
        }



        public static bool isValidForPathfind(int blocksHeight, LVector3 pos)
        {
            bool isCurrentlyValid = true;
            for (int i = 0; i < blocksHeight; i++)
            {
                isCurrentlyValid = isCurrentlyValid && world[pos.x, pos.y - i, pos.z] == (int)BlockValue.Air;
            }
            return isCurrentlyValid && world[pos.x, pos.y - blocksHeight, pos.z] != (int)BlockValue.Air;
        }



        public static IEnumerable<LVector3> AllNeighbors(LVector3 pos)
        {
            yield return new LVector3(pos.x + 1, pos.y, pos.z);
            yield return new LVector3(pos.x - 1, pos.y, pos.z);
            yield return new LVector3(pos.x + 1, pos.y, pos.z + 1);
            yield return new LVector3(pos.x - 1, pos.y, pos.z + 1);
            yield return new LVector3(pos.x + 1, pos.y, pos.z - 1);
            yield return new LVector3(pos.x - 1, pos.y, pos.z - 1);
            yield return new LVector3(pos.x + 1, pos.y + 1, pos.z);
            yield return new LVector3(pos.x - 1, pos.y + 1, pos.z);
            yield return new LVector3(pos.x + 1, pos.y - 1, pos.z);
            yield return new LVector3(pos.x - 1, pos.y - 1, pos.z);
            yield return new LVector3(pos.x, pos.y + 1, pos.z + 1);
            yield return new LVector3(pos.x, pos.y + 1, pos.z - 1);
            yield return new LVector3(pos.x, pos.y - 1, pos.z + 1);
            yield return new LVector3(pos.x, pos.y - 1, pos.z - 1);
            yield return new LVector3(pos.x, pos.y, pos.z + 1);
            yield return new LVector3(pos.x, pos.y, pos.z - 1);
        }


        static bool TryLinePathfind(int blocksHeight, ref PathNode curPath, out bool youShouldJump, LVector3 startPos, LVector3 goalPos)
        {
            youShouldJump = false;
            Vector3 startPosVec3 = startPos.BlockCentertoUnityVector3();
            Vector3 goalPosVec3 = goalPos.BlockCentertoUnityVector3();
            //long maxSteps = LVector3.CityBlockDistance(startPos, goalPos) + 5;
            RaycastResults results;
            List<PathNode> positions = new List<PathNode>();
            positions.Add(new PathNode(startPos, null));
            if (CustomRaycast(startPosVec3, (goalPosVec3 - startPosVec3).normalized, 100000.0f, (b, bx, by, bz, pbx, pby, pbz) =>
            {
                if (isValidForPathfind(blocksHeight, pbx, pby, pbz, bx, by, bz))
                {
                    Debug.Log("called in raycast " + bx + " " + by + " " + bz + " " + positions.Count);
                    PathNode nextNode = new PathNode(new LVector3(bx, by, bz), positions[positions.Count - 1]);
                //PathNode curNode = positions[positions.Count - 1];
                //nextNode.prevNode = curNode;
                //curNode.nextNode = nextNode;
                positions.Add(nextNode);
                    return true;
                }
                else
                {
                    return false;
                }
            }, (b, bx, by, bz, pbx, pby, pbz) =>
            {
                if (isValidForPathfind(blocksHeight, pbx, pby, pbz, bx, by, bz) && LVector3.CityBlockDistance(new LVector3(bx, by, bz), goalPos) <= 1)
                {
                    Debug.Log("called in raycast 2 " + bx + " " + by + " " + bz + " " + positions.Count);
                    PathNode nextNode = new PathNode(new LVector3(bx, by, bz), positions[positions.Count - 1]);
                //PathNode curNode = positions[positions.Count - 1];
                //nextNode.prevNode = curNode;
                //curNode.nextNode = nextNode;
                positions.Add(nextNode);
                    return true;
                }
                else
                {
                    return false;
                }
            }, out results))
            {
                int pathLen = 0;
                curPath = positions[positions.Count - 1];
                if (curPath != null && curPath.prevNode != null)
                {
                    PathNode curBlock = curPath.prevNode;
                    PathNode nextBlock = curPath;
                    while (curBlock.prevNode != null)
                    {
                        pathLen += 1;
                        curBlock.nextNode = nextBlock;
                        nextBlock = curBlock;
                        curBlock = curBlock.prevNode;
                    }
                    curBlock.nextNode = nextBlock;
                    curPath = curBlock;
                }
                Debug.Log("line pathfind with path len = " + pathLen);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isValidForPathfind(int blocksHeight, long px, long py, long pz, long x, long y, long z)
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

        public static void Pathfind(int blocksHeight, ref PathNode curPath, out bool youShouldJump, LVector3 startPos, LVector3 goalPos, int maxSteps = 100)
        {

            // doesn't quite work yet
            //if (TryLinePathfind(blocksHeight, ref curPath, out youShouldJump, startPos, goalPos))
            //{
            //    Debug.Log("got line pathfind");
            //    return;
            //}
            youShouldJump = false;
            HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
            Queue<PathNode> nodes = new Queue<PathNode>();
            LVector3 myPos = startPos;
            PathNode closest = new PathNode(myPos, null);
            float closestDist = float.MaxValue;
            for (int i = 0; i < 2; i++)
            {
                LVector3 tmpPos = myPos - new LVector3(0, i, 0);
                if (!nodesSoFar.Contains(tmpPos) && isValidForPathfind(blocksHeight, tmpPos))
                {
                    nodes.Enqueue(new PathNode(tmpPos, null));
                    nodesSoFar.Add(tmpPos);
                }
            }

            int steps = 0;
            if (nodes.Count == 0)
            {
                //body.jumping = true;
                youShouldJump = true;
            }
            while (nodes.Count > 0)
            {
                PathNode curNode = nodes.Dequeue();
                float dist = LVector3.EuclideanDistance(curNode.pos, goalPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = curNode;
                }
                foreach (LVector3 neighbor in AllNeighbors(curNode.pos))
                {
                    if (!nodesSoFar.Contains(neighbor) && isValidForPathfind(blocksHeight, curNode.pos, neighbor))
                    {
                        nodes.Enqueue(new PathNode(neighbor, curNode));
                        nodesSoFar.Add(neighbor);
                    }
                }
                steps += 1;
                if (steps >= maxSteps || dist <= 1)
                {
                    //Debug.Log("found path with dist = " + dist + " in " + steps + " steps");
                    break;
                }
            }

            curPath = closest;
            if (curPath != null && curPath.prevNode != null)
            {
                PathNode curBlock = curPath.prevNode;
                PathNode nextBlock = curPath;
                while (curBlock.prevNode != null)
                {
                    curBlock.nextNode = nextBlock;
                    nextBlock = curBlock;
                    curBlock = curBlock.prevNode;
                }
                curBlock.nextNode = nextBlock;
                curPath = curBlock;
            }

        }


        public static void PathfindAway(int blocksHeight, ref PathNode curPath, out bool youShouldJump, LVector3 startPos, LVector3 goalPos, int maxSteps = 100)
        {

            // doesn't quite work yet
            //if (TryLinePathfind(blocksHeight, ref curPath, out youShouldJump, startPos, goalPos))
            //{
            //    Debug.Log("got line pathfind");
            //    return;
            //}
            youShouldJump = false;
            HashSet<LVector3> nodesSoFar = new HashSet<LVector3>();
            Queue<PathNode> nodes = new Queue<PathNode>();
            LVector3 myPos = startPos;
            PathNode furthest = new PathNode(myPos, null);
            float furthestDist = float.MinValue;
            for (int i = 0; i < 2; i++)
            {
                LVector3 tmpPos = myPos - new LVector3(0, i, 0);
                if (!nodesSoFar.Contains(tmpPos) && isValidForPathfind(blocksHeight, tmpPos))
                {
                    nodes.Enqueue(new PathNode(tmpPos, null));
                    nodesSoFar.Add(tmpPos);
                }
            }

            int steps = 0;
            if (nodes.Count == 0)
            {
                //body.jumping = true;
                youShouldJump = true;
            }
            while (nodes.Count > 0)
            {
                PathNode curNode = nodes.Dequeue();
                float dist = LVector3.EuclideanDistance(curNode.pos, goalPos);
                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    furthest = curNode;
                }
                foreach (LVector3 neighbor in AllNeighbors(curNode.pos))
                {
                    if (!nodesSoFar.Contains(neighbor) && isValidForPathfind(blocksHeight, curNode.pos, neighbor))
                    {
                        nodes.Enqueue(new PathNode(neighbor, curNode));
                        nodesSoFar.Add(neighbor);
                    }
                }
                steps += 1;
                if (steps >= maxSteps || dist <= 1)
                {
                    //Debug.Log("found path with dist = " + dist + " in " + steps + " steps");
                    break;
                }
            }

            curPath = furthest;
            if (curPath != null && curPath.prevNode != null)
            {
                PathNode curBlock = curPath.prevNode;
                PathNode nextBlock = curPath;
                while (curBlock.prevNode != null)
                {
                    curBlock.nextNode = nextBlock;
                    nextBlock = curBlock;
                    curBlock = curBlock.prevNode;
                }
                curBlock.nextNode = nextBlock;
                curPath = curBlock;
            }

        }



    }
}