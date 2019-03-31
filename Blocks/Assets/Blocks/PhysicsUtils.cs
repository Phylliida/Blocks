using Example_pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blocks.ExtensionMethods;

namespace Blocks
{
    public struct SplitInt
    {
        public uint rawVal;
        public int numPieces;
        public int sizeOfEachPieceInBits;

        public uint this[int i]
        {
            get
            {
                int startIInclusive = i * sizeOfEachPieceInBits;
                int endIExclusive = (i + 1) * sizeOfEachPieceInBits;
                return rawVal.GetBits(startIInclusive, endIExclusive);
            }
            set
            {
                int startIInclusive = i * sizeOfEachPieceInBits;
                int endIExclusive = (i + 1) * sizeOfEachPieceInBits;
                rawVal = rawVal.SettingBits(startIInclusive, endIExclusive, value);
            }
        }

        public SplitInt(uint rawVal, int numPieces)
        {
            this.rawVal = rawVal;
            this.numPieces = numPieces;
            sizeOfEachPieceInBits = (sizeof(int) * 8) / numPieces;
            if (sizeOfEachPieceInBits == 0)
            {
                throw new System.Exception("Cannot fit more than 32 pieces in a single int (ints are only 32 bits wide)");
            }
        }
    }

    public class DoEveryMS
    {
        public long ms;
        long randVariance = 0;
        long timeWhenLastDid;
        public DoEveryMS(long ms, long randVariance = 0)
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

    public static class RotationUtils
    {

        public static int[] CONNECTIVITY_OFFSETS = new int[]
        {

            -1,0,0,
            1,0,0,
            // 0,0,0,
            0,0,-1,
            0,0,1,

            -1,-1,0,
            1,-1,0,
            0,-1,0,
            0,-1,-1,
            0,-1,1,


            -1,1,0,
            1,1,0,
            0,1,0,
            0,1,-1,
            0,1,1
        };


        public static int NUM_CONNECTIVITY_OFFSETS = CONNECTIVITY_OFFSETS.Length / 3;

        public class RotationVariant
        {
            public bool allowAppend;
            public bool allowRotate;
            public bool[] values = new bool[NUM_CONNECTIVITY_OFFSETS];

            public override int GetHashCode()
            {
                return ToMergedVal();
            }

            int mergedVal;
            bool foundMergedVal = false;

            public int ToMergedVal()
            {
                if (foundMergedVal)
                {
                    return mergedVal;
                }
                mergedVal = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i])
                    {
                        mergedVal = mergedVal | (1 << i);
                    }
                }
                foundMergedVal = true;
                return mergedVal;
            }

            BlockModelElement[] elements;



            Dictionary<int, Blocks.RenderTriangle[]>[] cachedStateAlternatives = new Dictionary<int, Blocks.RenderTriangle[]>[] {
                new Dictionary<int, Blocks.RenderTriangle[]>(),
                new Dictionary<int, Blocks.RenderTriangle[]>(),
                new Dictionary<int, Blocks.RenderTriangle[]>(),
                new Dictionary<int, Blocks.RenderTriangle[]>()
            };


            public Blocks.RenderTriangle[] ToRenderTriangles(BlockData.BlockRotation blockRotation = BlockData.BlockRotation.Degrees0, int state = 0, BlockModel parent=null)
            {
                int rotationI = PhysicsUtils.RotationToDegrees(blockRotation) / 90;
                if (cachedStateAlternatives[rotationI].ContainsKey(state))
                {
                    return cachedStateAlternatives[rotationI][state];
                }

                // Degrees to rotation does mod 360 for us so we don't need to worry about that
                BlockData.BlockRotation combinedRotation = PhysicsUtils.DegreesToRotation(PhysicsUtils.RotationToDegrees(blockRotation) + PhysicsUtils.RotationToDegrees(rot));

                List<Blocks.RenderTriangle> res = new List<RenderTriangle>();

                foreach (BlockModelElement element in elements)
                {
                    res.AddRange(element.ToRenderTriangles(combinedRotation, state));
                }

                if (mergedA != null)
                {
                    res.AddRange(mergedA.ToRenderTriangles(combinedRotation, state, parent));
                }
                if (mergedB != null)
                {
                    res.AddRange(mergedB.ToRenderTriangles(combinedRotation, state, parent));
                }

                if (parent != null)
                {
                    res.AddRange(parent.ToRenderTriangles(blockRotation, state, ToMergedVal()));
                }

                Blocks.RenderTriangle[] result = res.ToArray();
                cachedStateAlternatives[rotationI][state] = result;
                return result;
            }

            public RotationVariant[] RotatedVariants()
            {
                List<RotationVariant> variants = new List<RotationVariant>();
                if (allowRotate)
                {
                    variants.Add(new RotationVariant(this, BlockData.BlockRotation.Degrees90));
                    variants.Add(new RotationVariant(this, BlockData.BlockRotation.Degrees180));
                    variants.Add(new RotationVariant(this, BlockData.BlockRotation.Degrees270));
                }
                return variants.ToArray();
            }

            BlockData.BlockRotation rot = BlockData.BlockRotation.Degrees0;

            public RotationVariant(RotationVariant prev, BlockData.BlockRotation rot)
            {
                this.elements = prev.elements;
                this.rot = rot;
                this.allowAppend = prev.allowAppend;

                for (int i = 0; i < NUM_CONNECTIVITY_OFFSETS; i++)
                {

                    if (!prev.values[i])
                    {
                        continue;
                    }
                    int pos = i * 3;
                    long offsetX = CONNECTIVITY_OFFSETS[pos];
                    long offsetY = CONNECTIVITY_OFFSETS[pos + 1];
                    long offsetZ = CONNECTIVITY_OFFSETS[pos + 2];
                    long myOffX, myOffY, myOffZ;

                    PhysicsUtils.RotateOffsetRelativeToRotation(rot, offsetX, offsetY, offsetZ, out myOffX, out myOffY, out myOffZ);

                    int newOffX = (int)myOffX;
                    int newOffY = (int)myOffY;
                    int newOffZ = (int)myOffZ;
                    // find which index that maps to
                    bool foundIt = false;
                    //Debug.Log("rotating " + offsetX + " " + offsetY + " " + offsetZ + " to " + newOffX + " " + newOffY + " " + newOffZ);
                    for (int j = 0; j < NUM_CONNECTIVITY_OFFSETS; j++)
                    {
                        if (CONNECTIVITY_OFFSETS[j * 3] == newOffX &&
                            CONNECTIVITY_OFFSETS[j * 3 + 1] == newOffY &&
                            CONNECTIVITY_OFFSETS[j * 3 + 2] == newOffZ)
                        {
                            values[j] = true;
                            foundIt = true;
                            break;
                        }
                    }
                    if (!foundIt)
                    {
                        Debug.LogWarning("could not find rotated offsets " + newOffX + " " + newOffY + " " + newOffZ);
                    }
                }
            }


            RotationVariant mergedA, mergedB;

            public RotationVariant(RotationVariant a, RotationVariant b)
            {
                this.elements = new BlockModelElement[0];
                for (int i = 0; i < NUM_CONNECTIVITY_OFFSETS; i++)
                {
                    if (a.values[i] || b.values[i])
                    {
                        values[i] = true;
                    }
                }

                this.mergedA = a;
                this.mergedB = b;
            }

            public RotationVariant(string name, BlockModelElement[] elements)
            {
                this.rot = BlockData.BlockRotation.Degrees0;
                this.elements = elements;
                name = name.ToLower();
                if (name.Substring(0, "connectedto".Length) == "connectedto")
                {
                    string[] pieces = name.Substring("connectedto".Length).Split(new char[] { '_' });

                    foreach (string piece in pieces)
                    {
                        int offsetX = 0;
                        int offsetY = 0;
                        int offsetZ = 0;
                        if (piece == "px") offsetX = 1;
                        else if (piece == "py") offsetY = 1;
                        else if (piece == "pz") offsetZ = 1;
                        else if (piece == "nx") offsetX = -1;
                        else if (piece == "ny") offsetY = -1;
                        else if (piece == "nz") offsetZ = -1;
                        else if (piece == "nux") { offsetX = -1; offsetY = 1; }
                        else if (piece == "nuz") { offsetZ = -1; offsetY = 1; }
                        else if (piece == "pux") { offsetX = 1; offsetY = 1; }
                        else if (piece == "puz") { offsetZ = 1; offsetY = 1; }
                        else if (piece == "ndx") { offsetX = -1; offsetY = -1; }
                        else if (piece == "ndz") { offsetZ = -1; offsetY = -1; }
                        else if (piece == "pdx") { offsetX = 1; offsetY = -1; }
                        else if (piece == "pdz") { offsetZ = 1; offsetY = -1; }
                        else if (piece == "append") allowAppend = true;
                        else if (piece == "rotate") allowRotate = true;

                        for (int i = 0; i < NUM_CONNECTIVITY_OFFSETS; i++)
                        {
                            if (CONNECTIVITY_OFFSETS[i*3] == offsetX &&
                                CONNECTIVITY_OFFSETS[i*3+1] == offsetY && 
                                CONNECTIVITY_OFFSETS[i*3+2] == offsetZ)
                            {
                                values[i] = true;
                            }
                        }

                    }
                }
            }
        }


        public class RotationVariantCollection
        {
            Dictionary<int, RotationVariant> collection = new Dictionary<int, RotationVariant>();
            List<RotationVariant> baseVariants = new List<RotationVariant>();
            BlockModel rootModel;
            public RotationVariantCollection(Dictionary<string, BlockModelElement[]> variants, BlockModel rootModel)
            {
                this.rootModel = rootModel;
                foreach (KeyValuePair<string, BlockModelElement[]> variant in variants)
                {
                    RotationVariant cur = new RotationVariant(variant.Key, variant.Value);
                    foreach (BlockModelElement element in variant.Value)
                    {
                        element.rootModel = rootModel;
                    }
                    Debug.Log("added base value of " + cur.ToMergedVal());
                    baseVariants.Add(cur);
                    collection[cur.ToMergedVal()] = cur;
                }

                List<RotationVariant> curBaseVariants = new List<RotationVariant>(baseVariants);

                foreach (RotationVariant baseVariant in curBaseVariants)
                {
                    if (baseVariant.allowRotate)
                    {
                        foreach (RotationVariant rotatedVariant in baseVariant.RotatedVariants())
                        {
                            //Debug.Log("have rotated variant " + rotatedVariant.ToMergedVal());
                            // add if we don't have that represented yet
                            if(!collection.ContainsKey(rotatedVariant.ToMergedVal()))
                            {
                                //Debug.Log("added rotated value of " + rotatedVariant.ToMergedVal());
                                baseVariants.Add(rotatedVariant);
                                collection[rotatedVariant.ToMergedVal()] = rotatedVariant;
                            }
                        }
                    }
                }
            }

            public bool IsSubsetOrEqualToFlags(int maybeSubsetOrEqualTo, int flags)
            {
                return (maybeSubsetOrEqualTo & flags) == maybeSubsetOrEqualTo;
            }


            public RotationVariant GetRotationVariant(int rotationFlags)
            {
                //Debug.Log("getting rotation variant of " + rotationFlags);
                if (collection.ContainsKey(rotationFlags))
                {
                    //Debug.Log("has cached " + rotationFlags);
                    return collection[rotationFlags];
                }
                else
                {
                    //Debug.Log("does not have cached " + rotationFlags);
                    int mergedFlags = 0;
                    RotationVariant merged = null;
                    if (collection.ContainsKey(0) && collection[0].allowAppend)
                    {
                        merged = collection[0];
                    }
                    // we could not find one, try merging
                    foreach (RotationVariant variant in baseVariants)
                    {
                        // see if it is a subset of the needed flags
                        if (IsSubsetOrEqualToFlags(variant.ToMergedVal(), rotationFlags))
                        {
                            mergedFlags = mergedFlags | variant.ToMergedVal();
                            if (merged == null)
                            {
                                merged = variant;
                            }
                            else
                            {
                                // make sure it actually contributes something
                                if (IsSubsetOrEqualToFlags(variant.ToMergedVal(), merged.ToMergedVal()))
                                {

                                }
                                else
                                {
                                    // merge it with the current merged one
                                    merged = new RotationVariant(merged, variant);

                                    // see if we have all the needed flags filled now
                                    if (merged.ToMergedVal() == rotationFlags)
                                    {
                                        // we have them all, we are good
                                        collection[merged.ToMergedVal()] = merged;
                                        return merged;
                                    }
                                }
                            }
                            //Debug.Log("merging with " + variant.ToMergedVal() + " to give us " + merged.ToMergedVal());

                        }
                    }

                    //Debug.Log("got result of " + merged);
                    // could not find any and could not manage to merge things or rotate things to get something that works
                    return merged;
                }
            }
        }
    }


    public class RaycastResults
    {
        public Vector3 hitPos;
        public LVector3 hitBlock;
        public LVector3 blockBeforeHit;
        public float dist;
        public Vector3 normal;
        public AxisDir axisHitFrom;


        public RaycastResults(Vector3 hitPos, LVector3 hitBlock, LVector3 blockBeforeHit, float dist, Vector3 normal)
        {
            this.hitPos = hitPos;
            this.hitBlock = hitBlock;
            this.blockBeforeHit = blockBeforeHit;
            this.dist = dist;
            this.normal = normal;

            if (blockBeforeHit == hitBlock)
            {
                axisHitFrom = AxisDir.None;
            }
            else
            {
                if (blockBeforeHit.x < hitBlock.x) axisHitFrom = AxisDir.XMinus;
                else if (blockBeforeHit.x > hitBlock.x) axisHitFrom = AxisDir.XPlus;
                else if (blockBeforeHit.y < hitBlock.y) axisHitFrom = AxisDir.YMinus;
                else if (blockBeforeHit.y > hitBlock.y) axisHitFrom = AxisDir.YPlus;
                else if (blockBeforeHit.z < hitBlock.z) axisHitFrom = AxisDir.ZMinus;
                else if (blockBeforeHit.z > hitBlock.z) axisHitFrom = AxisDir.ZPlus;
                else axisHitFrom = AxisDir.None;
            }

        }
    }


    namespace ExtensionMethods
    {

        public static class ListExtensionMethods
        {
            public static bool Contains<T>(this T[] arr, T item)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Equals(item))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static class IntBitFiddleExtensions
        {
            /// <summary>
            /// Gets the uint represented by the given bits
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <returns></returns>
            public static uint GetBits(this uint a, int lowestBitInclusive, int highestBitExclusive = 32)
            {
                return PhysicsUtils.GetBits(a, lowestBitInclusive, highestBitExclusive);
            }


            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static uint SettingBits(this uint a, int lowestBitInclusive, int highestBitExclusive, uint value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, highestBitExclusive, value);
            }


            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static uint SettingBits(this uint a, int lowestBitInclusive, uint value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, value);
            }
        }



        public static class ShortBitFiddleExtensions
        {
            /// <summary>
            /// Gets the ushort represented by the given bits
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <returns></returns>
            public static ushort GetBits(this ushort a, int lowestBitInclusive, int highestBitExclusive = 16)
            {
                return PhysicsUtils.GetBits(a, lowestBitInclusive, highestBitExclusive);
            }


            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static ushort SettingBits(this ushort a, int lowestBitInclusive, int highestBitExclusive, ushort value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, highestBitExclusive, value);
            }


            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static ushort SettingBits(this ushort a, int lowestBitInclusive, ushort value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, value);
            }
        }




        public static class ByteBitFiddleExtensions
        {
            /// <summary>
            /// Gets the byte represented by the given bits
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <returns></returns>
            public static byte GetBits(this byte a, int lowestBitInclusive, int highestBitExclusive = 16)
            {
                return PhysicsUtils.GetBits(a, lowestBitInclusive, highestBitExclusive);
            }

            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static byte SettingBits(this byte a, int lowestBitInclusive, int highestBitExclusive, byte value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, highestBitExclusive, value);
            }


            /// <summary>
            /// Overwrites the specified bits with the new value
            /// </summary>
            /// <param name="a"></param>
            /// <param name="lowestBitInclusive"></param>
            /// <param name="highestBitExclusive"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static byte SettingBits(this byte a, int lowestBitInclusive, byte value)
            {
                return PhysicsUtils.SettingBits(a, lowestBitInclusive, value);
            }
        }
    }


    public class PhysicsUtils
    {


        public static int RotationToDegrees(BlockData.BlockRotation rotation)
        {
            if (rotation == BlockData.BlockRotation.Degrees0)
            {
                return 0;
            }
            else if(rotation == BlockData.BlockRotation.Degrees90)
            {
                return 90;
            }
            else if(rotation == BlockData.BlockRotation.Degrees180)
            {
                return 180;
            }
            else if(rotation == BlockData.BlockRotation.Degrees270)
            {
                return 270;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// This will do mod and rounding for you, so negative values and values greater than 360 will be put back in the correct range and rounded to nearest multiple of 90
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static BlockData.BlockRotation DegreesToRotation(int degrees)
        {
            degrees = GoodMod(degrees, 360);
            // floor to nearest 90
            degrees = degrees / 90;
            degrees *= 90;
            if (degrees == 0)
            {
                return BlockData.BlockRotation.Degrees0;
            }
            else if(degrees == 90)
            {
                return BlockData.BlockRotation.Degrees90;
            }
            else if(degrees == 180)
            {
                return BlockData.BlockRotation.Degrees180;
            }
            else if(degrees == 270)
            {
                return BlockData.BlockRotation.Degrees270;
            }
            else
            {
                return BlockData.BlockRotation.Degrees0;
            }
        }

        public static void RotateOffsetRelativeToRotation(BlockData.BlockRotation rotation, long offX, long offY, long offZ, out long relativeOffX, out long relativeOffY, out long relativeOffZ)
        {
            if (rotation == BlockData.BlockRotation.Degrees0)
            {
                relativeOffX = offX;
                relativeOffY = offY;
                relativeOffZ = offZ;
            }
            else if (rotation == BlockData.BlockRotation.Degrees90)
            {
                relativeOffX = offZ;
                relativeOffY = offY;
                relativeOffZ = -offX;
            }
            else if (rotation == BlockData.BlockRotation.Degrees180)
            {
                relativeOffX = -offX;
                relativeOffY = offY;
                relativeOffZ = -offZ;
            }
            else if (rotation == BlockData.BlockRotation.Degrees270)
            {
                relativeOffX = -offZ;
                relativeOffY = offY;
                relativeOffZ = offX;
            }
            else
            {
                relativeOffX = offX;
                relativeOffY = offY;
                relativeOffZ = offZ;
            }
        }

        /// <summary>
        /// Gets the uint that is repesented by the bits from lowestBitInclusive to all the bits before highestBitExclusive
        /// This is like accessing a python array, also negative values are allowed and they will behave in the same way as a python array
        /// This means that a negative value is treated as 32 + that negative value (since there are 32 bits in a c# uint)
        /// For example:
        /// GetBits(10, 0, -2)
        /// Gets everything except the top two highest order bits
        /// GetBits(10,2,0)
        /// Will return 0 since highestBitExclusive is lower than lowestBitInclusive (no bits are retreived, default value is 0)
        /// GetBits(10,0,32)
        /// Will just return num (gets all the bits)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="lowestBit"></param>
        /// <param name="highestBit"></param>
        /// <returns></returns>
        public static uint GetBits(uint num, int lowestBitInclusive, int highestBitExclusive=32)
        {
            int maxSize = sizeof(uint) * 8; // sizeof gives size in bytes, I need size in bits
            int highestBit = highestBitExclusive - 1;
            // lets you use -1 for highest bit
            if (highestBitExclusive < 0)
            {
                highestBitExclusive += maxSize;
            }
            if (lowestBitInclusive < 0)
            {
                lowestBitInclusive += maxSize;
            }

            if (highestBitExclusive > maxSize)
            {
                highestBitExclusive = maxSize;
            }

            if (lowestBitInclusive > maxSize)
            {
                return 0;
            }

            if (lowestBitInclusive >= highestBitExclusive)
            {
                return 0;
            }

            uint res = num >> lowestBitInclusive;

            // we aren't using the highest bit, get everything
            if (highestBitExclusive == maxSize)
            {
                return res;
            }
            // otherwise, mask off the bits that are too high
            else
            {

                int numBits = highestBitExclusive - lowestBitInclusive;
                // etc.
                uint maskForThatManyBits = (uint)((1 << numBits) - 1);

                return res & maskForThatManyBits;
            }
        }

        /// <summary>
        /// Returns 0b111...111 with nBits ones
        /// </summary>
        /// <param name="nBits"></param>
        /// <returns></returns>
        public static uint MaskForNBits(int nBits)
        {
            int maxSize = sizeof(uint) * 8; // sizeof gives size in bytes, I need size in bits
            if (nBits >= maxSize)
            {
                uint res = 0;
                return ~res; // invert all the bits, the trick below doesn't work if we are using all the bits
            }
            // if numBits = 1, 1 << 1 = 2 = 10 in binary. Minus 1 = 1
            // if numBits = 2, 1 << 2 = 4 = 100 in binary. Minus 1 = 11
            // if numBits = 3, 1 << 3 = 8 = 1000 in binary. Minus 1 = 111
            return (uint)((1 << nBits) - 1);
        }

        public static uint ClearBits(uint originalVal, int lowestBitInclusive, int highestBitExclusive)
        {
            int maxSize = sizeof(uint) * 8; // sizeof gives size in bytes, I need size in bits
            if (lowestBitInclusive >= maxSize)
            {
                return originalVal;
            }
            else
            {
                // not using highest bit exclusive
                if (highestBitExclusive >= maxSize)
                {
                    highestBitExclusive = maxSize;
                    // invert the bits

                }
                if (lowestBitInclusive >= highestBitExclusive)
                {
                    return originalVal;
                }
                else
                {
                    int nBits = highestBitExclusive - lowestBitInclusive;
                    // bitwise invert mask that only gives you those bits, then bitwise and with original value to clear those bits
                    uint bitMask = ~(MaskForNBits(nBits) << lowestBitInclusive);
                    return originalVal & bitMask;
                }
            }
        }

        public static uint SettingBits(uint originalVal, int lowestBitInclusive, int highestBitExclusive, uint value)
        {
            int maxSize = sizeof(uint) * 8; // sizeof gives size in bytes, I need size in bits
            uint clearedVal = ClearBits(originalVal, lowestBitInclusive, highestBitExclusive);
            uint fixedValue = ClearBits(value << lowestBitInclusive, highestBitExclusive, maxSize); // clear everything past the number of bits
            // bitwise or the two things together to get the results
            return clearedVal | fixedValue;
        }

        public static uint SettingBits(uint originalVal, int lowestBitInclusive, uint value)
        {
            int maxSize = sizeof(uint) * 8; // sizeof gives size in bytes, I need size in bits
            return SettingBits(originalVal, lowestBitInclusive, maxSize, value);
        }



        /// <summary>
        /// Gets the uint that is repesented by the bits from lowestBitInclusive to all the bits before highestBitExclusive
        /// This is like accessing a python array, also negative values are allowed and they will behave in the same way as a python array
        /// This means that a negative value is treated as 32 + that negative value (since there are 32 bits in a c# uint)
        /// For example:
        /// GetBits(10, 0, -2)
        /// Gets everything except the top two highest order bits
        /// GetBits(10,2,0)
        /// Will return 0 since highestBitExclusive is lower than lowestBitInclusive (no bits are retreived, default value is 0)
        /// GetBits(10,0,32)
        /// Will just return num (gets all the bits)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="lowestBit"></param>
        /// <param name="highestBit"></param>
        /// <returns></returns>
        public static ushort GetBits(ushort num, int lowestBitInclusive, int highestBitExclusive = 16)
        {
            // I could the same code above for shorts, but since all computation is done in ints anyway (as far as I know?) I might as well just do this
            // the cast will get rid of the bits we can't use
            return (ushort)GetBits((uint)num, lowestBitInclusive, highestBitExclusive);
        }

        public static ushort ClearBits(ushort originalVal, int lowestBitInclusive, int highestBitExclusive)
        {
            return (ushort)ClearBits((uint)originalVal, lowestBitInclusive, highestBitExclusive);
        }

        public static ushort SettingBits(ushort originalVal, int lowestBitInclusive, int highestBitExclusive, ushort value)
        {
            return (ushort)SettingBits((uint)originalVal, lowestBitInclusive, highestBitExclusive, (uint)value);
        }

        public static ushort SettingBits(ushort originalVal, int lowestBitInclusive, ushort value)
        {
            int maxSize = sizeof(ushort) * 8; // sizeof gives size in bytes, I need size in bits
            return SettingBits(originalVal, lowestBitInclusive, maxSize, value);
        }


        /// <summary>
        /// Gets the uint that is repesented by the bits from lowestBitInclusive to all the bits before highestBitExclusive
        /// This is like accessing a python array, also negative values are allowed and they will behave in the same way as a python array
        /// This means that a negative value is treated as 32 + that negative value (since there are 32 bits in a c# uint)
        /// For example:
        /// GetBits(10, 0, -2)
        /// Gets everything except the top two highest order bits
        /// GetBits(10,2,0)
        /// Will return 0 since highestBitExclusive is lower than lowestBitInclusive (no bits are retreived, default value is 0)
        /// GetBits(10,0,32)
        /// Will just return num (gets all the bits)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="lowestBit"></param>
        /// <param name="highestBit"></param>
        /// <returns></returns>
        public static byte GetBits(byte num, int lowestBitInclusive, int highestBitExclusive = 16)
        {
            // I could the same code above for shorts, but since all computation is done in ints anyway (as far as I know?) I might as well just do this.
            // the cast will get rid of the bits we can't use
            return (byte)GetBits((uint)num, lowestBitInclusive, highestBitExclusive);
        }

        public static byte ClearBits(byte originalVal, int lowestBitInclusive, int highestBitExclusive)
        {
            return (byte)ClearBits((byte)originalVal, lowestBitInclusive, highestBitExclusive);
        }

        public static byte SettingBits(byte originalVal, int lowestBitInclusive, int highestBitExclusive, byte value)
        {
            return (byte)SettingBits((uint)originalVal, lowestBitInclusive, highestBitExclusive, (uint)value);
        }

        public static byte SettingBits(byte originalVal, int lowestBitInclusive, byte value)
        {
            int maxSize = sizeof(byte) * 8; // sizeof gives size in bytes, I need size in bits
            return SettingBits(originalVal, lowestBitInclusive, maxSize, value);
        }

        public static BlockData.BlockRotation AxisToRotation(AxisDir dir)
        {
            if (dir == AxisDir.XMinus)
            {
                return BlockData.BlockRotation.Degrees0;
            }
            else if(dir == AxisDir.XPlus)
            {
                return BlockData.BlockRotation.Degrees180;
            }
            else if(dir == AxisDir.ZMinus)
            {
                return BlockData.BlockRotation.Degrees90;
            }
            else if(dir == AxisDir.ZPlus)
            {
                return BlockData.BlockRotation.Degrees270;
            }
            else
            {
                return BlockData.BlockRotation.Degrees0;
            }
        }

        public static int PackTwoValuesIntoInt(short a, short b)
        {
            ushort sa = (ushort)a;
            ushort sb = (ushort)b;
            uint ua = sa;
            uint ub = sb;
            uint res = (ua << 16) | ub;

            return (int)res;
        }

        public static void UnpackValuesFromInt(int x, out short a, out short b)
        {
            uint ux = (uint)x;

            ushort ua = (ushort)(ux >> 16);
            ushort ub = (ushort)(ux & 0xFFFFF);
            short sa = (short)ua;
            short sb = (short)ub;

            a = (short)sa;
            b = (short)sb;

        }


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


                bool hitBlock;


                Vector3 tmpHitPos;
                if (World.mainWorld.HasNonBlockModel(newCurPosL.Block))
                {
                    if (IntersectWithBlockModel(newCurPosL, newCurPosL.Block, prevPosF, dir, World.mainWorld.worldScale * 100, out tmpHitPos))
                    {
                        hitBlock = true;
                    }
                    else
                    {
                        hitBlock = false;
                    }
                }
                else
                {
                    tmpHitPos = prevPosF;
                    hitBlock = true;
                }


                if (hitBlock && IsBlockSolid(World.mainWorld[newCurPosL.x, newCurPosL.y, newCurPosL.z]))
                {
                    hitPos = newCurPosL;
                    surfaceHitPos = tmpHitPos;
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


        public static bool IntersectWithBlockModel(LVector3 pos, BlockValue block, Vector3 origin, Vector3 dir, float maxDist, out Vector3 hitPos)
        {
            hitPos = Vector3.zero;
            if (!World.mainWorld.HasNonBlockModel(block))
            {
                return true;
            }
            else
            {
                Vector3 actualOrigin = origin * World.mainWorld.worldScale;
                dir = dir.normalized;
                //Debug.Log("trying to intersect custom model with block pos " + pos + " and block " + block);
                RenderTriangle[] triangles = World.mainWorld.GetTrianglesForBlock(pos);
                for(int i = 0; i < triangles.Length; i++)
                {
                    Vector3 v1 = new Vector3(triangles[i].vertex1.x, triangles[i].vertex1.y, triangles[i].vertex1.z);
                    Vector3 v2 = new Vector3(triangles[i].vertex2.x, triangles[i].vertex2.y, triangles[i].vertex2.z);
                    Vector3 v3 = new Vector3(triangles[i].vertex3.x, triangles[i].vertex3.y, triangles[i].vertex3.z);

                    //Debug.Log("triangle has pos " + v1 + " " + v2 + " " + v3 + " and we have origin " + origin + " with dir " + dir);
                    if (RayTriangleIntersect(origin, dir, v1, v2, v3, maxDist, out hitPos))
                    {
                        //Debug.Log("hit triangle!");
                        return true;
                    }
                }
                //Debug.Log("failed to intersect custom model");
                return false;
            }
        }


        static float kEpsilon = 0.00000001f;
        // modified from https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        static bool RayTriangleIntersect(Vector3 orig, Vector3 dir,
            Vector3 v0, Vector3 v1, Vector3 v2,
            float t, out Vector3 hitPos)
        {
            hitPos = Vector3.zero;
            // compute plane's normal
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;
            // no need to normalize
            Vector3 N = Vector3.Cross(v0v1, v0v2); // N 
            float area2 = N.magnitude;

            // Step 1: finding P

            // check if ray and plane are parallel ?
            float NdotRayDirection = Vector3.Dot(N, dir);
            if (System.Math.Abs(NdotRayDirection) < kEpsilon) // almost 0 
            {
                return false; // they are parallel so they don't intersect ! 
            }

            // compute d parameter using equation 2
            float d = Vector3.Dot(N, v0);


            t = Vector3.Dot((v0 - orig), N.normalized) / Vector3.Dot(dir.normalized, N.normalized);



            // compute t (equation 3)
            // t = (Vector3.Dot(N, orig) + d) / NdotRayDirection;
            // check if the triangle is in behind the ray
            if (t < 0)
            {
                //Debug.Log("the triangle is behind? with t = " + t);
                return false;
            }
            else
            {
                //Debug.Log("the triangle is not behind, with t = " + t);
            }

            // compute the intersection point using equation 1
            Vector3 P = orig + t * dir;

            //Debug.Log("got intersection point " + P + " with origin " + orig + " and dir " + dir + " and triangle points " + v0 + " " + v1 + " " + v2);

            // Step 2: inside-outside test
            Vector3 C; // vector perpendicular to triangle's plane 

            // edge 0
            Vector3 edge0 = v1 - v0;
            Vector3 vp0 = P - v0;
            C = Vector3.Cross(edge0, vp0);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side 

            // edge 1
            Vector3 edge1 = v2 - v1;
            Vector3 vp1 = P - v1;
            C = Vector3.Cross(edge1, vp1);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side 

            // edge 2
            Vector3 edge2 = v0 - v2;
            Vector3 vp2 = P - v2;
            C = Vector3.Cross(edge2, vp2);
            if (Vector3.Dot(N, C) < 0) return false; // P is on the right side; 

            hitPos = orig + dir * t;
            return true; // this ray hits the triangle 
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


                bool hitBlock;


                Vector3 tmpHitPos;
                if (World.mainWorld.HasNonBlockModel(newCurPosL.Block))
                {
                    if (IntersectWithBlockModel(newCurPosL, newCurPosL.Block, origin, dir, maxDist, out tmpHitPos))
                    {
                        hitBlock = true;
                    }
                    else
                    {
                        hitBlock = false;
                    }
                }
                else
                {
                    tmpHitPos = prevPosF;
                    hitBlock = true;
                }



                if (hitBlock && isBlockDesiredResult(newCurPosL.Block, newCurPosL.x, newCurPosL.y, newCurPosL.z, prevPosLTested.x, prevPosLTested.y, prevPosLTested.z))
                //if (IsBlockSolid(World.mainWorld[newCurPosL.x, newCurPosL.y, newCurPosL.z]))
                {
                    hitPos = newCurPosL;
                    surfaceHitPos = tmpHitPos;
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


                if (hitBlock && !isBlockValid(newCurPosL.Block, newCurPosL.x, newCurPosL.y, newCurPosL.z, prevPosLTested.x, prevPosLTested.y, prevPosLTested.z))
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