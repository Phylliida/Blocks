using Blocks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public enum PathingFlags
    {
        Visited = (1 << 30)

    }

    public class BlocksPathing
    {
        public static PathingSpreadNode Pathfind(World world, LVector3 startPos, LVector3 endPos, int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight, out bool success)
        {
            PathingSpreadNode res = PathingNode.Pathfind(world, startPos, endPos, neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight, out success);

            if (res == null)
            {
                Debug.LogWarning("pathing failed");
            }
            else
            {
                Debug.Log("got pathing result with path of length: " + res.distFromStart + " and success=" + success);

                foreach (LoggingNode node in GameObject.FindObjectsOfType<LoggingNode>())
                {
                    if (node.logTag == "resPos")
                    {
                        GameObject.Destroy(node.gameObject);
                    }
                }

                int pos = 0;
                res.LoopThroughPositions((wx, wy, wz) =>
                {
                    world.MakeLoggingNode("resPos", "Path pos " + pos, Color.green, wx, wy, wz);
                    pos += 1;
                });

            }



            return res;
        }
    }

    public class PathingSpreadNode
    {
        public PathingNodeExit curExit;
        public PathingSpreadNode prevNode;
        public SpreadNode pathToPrevNode;
        public int distFromStart;


        public delegate void LoopThroughPositionsCallback(long wx, long wy, long wz);

        public void LoopThroughPositions(LoopThroughPositionsCallback callback)
        {
            PathingSpreadNode curNode = this;

            int numTimes = 0;
            do
            {
                SpreadNode curPos = curNode.pathToPrevNode;
                do
                {
                    callback(curPos.wx, curPos.wy, curPos.wz);
                    curPos = curPos.prev;
                }
                while (curPos != null);

                curNode = curNode.prevNode;
            } while (curNode != null);

        }

        public PathingSpreadNode(PathingNodeExit curExit, PathingSpreadNode prevNode, SpreadNode pathToPrevNode)
        {
            this.curExit = curExit;
            this.prevNode = prevNode;
            this.pathToPrevNode = pathToPrevNode;

            if (prevNode == null)
            {
                distFromStart = pathToPrevNode.pathLen;
            }
            else
            {
                distFromStart = prevNode.distFromStart + pathToPrevNode.pathLen;
            }

        }

        public long wx
        {
            get
            {
                return pathToPrevNode.wx;
            }
            private set
            {

            }
        }



        public long wy
        {
            get
            {
                return pathToPrevNode.wy;
            }
            private set
            {

            }
        }


        public long wz
        {
            get
            {
                return pathToPrevNode.wz;
            }
            private set
            {

            }
        }

    }

    public class SpreadNode
    {
        public int localX;
        public int localY;
        public int localZ;
        public PathingNode parentNode;
        public SpreadNode prev;
        public int pathLen;

        public override string ToString()
        {
            return "[" +
                localX + "(" + wx + ")" + "," +
                localY + "(" + wy + ")" + "," +
                localZ + "(" + wz + ")" + "]";
        }

        public SpreadNode(int x, int y, int z, SpreadNode prev, PathingNode parentNode)
        {
            this.localX = x;
            this.localY = y;
            this.localZ = z;
            this.prev = prev;
            this.parentNode = parentNode;
            if (prev == null)
            {
                pathLen = 0;
            }
            else
            {
                pathLen = prev.pathLen + 1;
            }
        }

        public SpreadNode Root
        {
            get
            {

                SpreadNode root = this;
                while (root.prev != null)
                {
                    root = root.prev;
                }
                return root;
            }
            set
            {

            }
         }

        public SpreadNode InvertPath()
        {

            SpreadNode cur = this;
            SpreadNode curInverted = new SpreadNode(localX, localY, localZ, null, parentNode);
            while (cur.prev != null)
            {
                cur = cur.prev;
                curInverted = new SpreadNode(cur.localX, cur.localY, cur.localZ, curInverted, cur.parentNode);
            }

            return curInverted;
        }

        public long wx
        {
            get
            {
                return parentNode.locationSpec.minX + localX;
            }
            private set
            {

            }
        }

        public long wy
        {
            get
            {
                return parentNode.locationSpec.minY + localY;
            }
            private set
            {

            }
        }

        public long wz
        {
            get
            {
                return parentNode.locationSpec.minZ + localZ;
            }
            private set
            {

            }
        }

        public long Dist(long worldX, long worldY, long worldZ)
        {
            return System.Math.Abs(worldX - wx) + System.Math.Abs(worldY - wy) + System.Math.Abs(worldZ - wz);
        }
    }

    public class PathingNodeExitConnection
    {
        public PathingNodeExit inExit;
        public PathingNodeExit outExit;
        public List<SpreadNode> paths = new List<SpreadNode>();


        public PathingNodeExitConnection(PathingNodeExit inExit, PathingNodeExit outExit)
        {
            this.inExit = inExit;
            this.outExit = outExit;
        }

        public void AddPath(SpreadNode path)
        {
            paths.Add(path);
        }

        public int Cost()
        {
            return paths[0].pathLen;
        }
    }

    public class PathingNodeExit
    {
        public List<PathingNodeExitConnection> connectedExits = new List<PathingNodeExitConnection>();
        public PathingNode parentNode;
        public Tuple<int, int, int>[] localPositions;

        public long visited = -1;
        public long connectedToExit = -1;

        public PathingNodeExit(PathingNode parentNode, Tuple<int, int, int>[] localPositions)
        {
            this.parentNode = parentNode;
            this.localPositions = localPositions;
        }

        public void AddExitConnection(PathingNodeExitConnection exitConnection)
        {
            connectedExits.Add(exitConnection);
        }

        public void RemoveExitConnectionsFromPathingNode(PathingNode parent)
        {
            List<PathingNodeExitConnection> newConnectedExits = new List<PathingNodeExitConnection>();
            foreach (PathingNodeExitConnection connection in connectedExits)
            {
                // make sure it doesn't have that parent
                if (connection.outExit.parentNode != parent && connection.inExit.parentNode != parent)
                {
                    newConnectedExits.Add(connection);
                }
            }
            connectedExits = newConnectedExits;
        }


        public long Dist(long x, long y, long z)
        {
            long minDist = long.MaxValue;
            for (int i = 0; i < localPositions.Length; i++)
            {
                long actualX = localPositions[i].a + parentNode.locationSpec.minX;
                long actualY = localPositions[i].a + parentNode.locationSpec.minY;
                long actualZ = localPositions[i].a + parentNode.locationSpec.minZ;


                long curDist = System.Math.Abs(x - actualX) + System.Math.Abs(y - actualY) + System.Math.Abs(z - actualZ);
                minDist = System.Math.Min(curDist, minDist);
            }
            return minDist;
        }
    }

    public class PathingNode
    {

        public long editNum = -2;

        public PathingNodeBlockChunk locationSpec;
        PatchingNodeBlockChunkData data;
        public int neededSizeForward, neededSizeUp, neededSizeSide;
        public int jumpHeight;
        public World world;
        public PathingNode(World world, PathingNodeBlockChunk locationSpec, int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight)
        {
            this.world = world;
            this.locationSpec = locationSpec;
            this.neededSizeForward = neededSizeForward;
            this.neededSizeUp = neededSizeUp;
            this.neededSizeSide = neededSizeSide;
            this.jumpHeight = jumpHeight;
            this.data = new PatchingNodeBlockChunkData(locationSpec);
        }



        bool MeetsFitCriteria(int x, int y, int z, SpreadNode prev=null, bool allowFalling=true, bool reversed=false)
        {
            bool onLand;
            return MeetsFitCriteria(x, y, z, out onLand, prev, allowFalling, reversed);
        }


        bool OnLand(int x, int y, int z)
        {
            for (int curX = x; curX >= 0 && curX > x - neededSizeSide; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeForward; curZ--)
                {
                    if (y > 0)
                    {
                        if (this[curX, y - 1, curZ] != BlockValue.Air)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ) != BlockValue.Air)
                        {
                            return true;
                        }
                    }
                }
            }


            for (int curX = x; curX >= 0 && curX > x - neededSizeForward; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeSide; curZ--)
                {
                    if (y > 0)
                    {
                        if (this[curX, y - 1, curZ] != BlockValue.Air)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ) != BlockValue.Air)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        bool MeetsFitCriteria(int x, int y, int z, out bool onLand, SpreadNode prev = null, bool allowFalling=true, bool reversed=false)
        {
            // try 2 rotations of us
            bool failed = false;
            onLand = false;
            for (int curX = x; curX >= 0 && curX > x - neededSizeSide; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeForward; curZ--)
                {
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (this[curX, curY, curZ] != BlockValue.Air)
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (y > 0)
                    {
                        if (this[curX, y-1, curZ] != BlockValue.Air)
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (this.locationSpec.GetBlockOutsideRange(curX, y-1, curZ) != BlockValue.Air)
                        {
                            onLand = true;
                        }
                    }
                    if (failed)  break;
                }
                if (failed) break;
            }


            bool onLandOrJumpingOrFalling = onLand;
            if (!onLand && prev != null)
            {
                // we are just falling, that is fine
                if ((prev.localY == y+1 && !reversed) || (prev.localY + 1 == y && reversed))
                {
                    if (allowFalling)
                    {
                        onLandOrJumpingOrFalling = true;
                    }
                }
                // we aren't falling, are we jumping?
                else
                {
                    int effectiveJumpDist = jumpHeight;
                    SpreadNode effectivePrev = prev;
                    // we are allowed to travel jumpHeight steps in the air without touching ground
                    while (effectiveJumpDist > 0 && effectivePrev != null)
                    {
                        bool prevOnLand = OnLand(effectivePrev.localX, effectivePrev.localY, effectivePrev.localZ);
                        if (prevOnLand)
                        {
                            onLandOrJumpingOrFalling = true;
                            break;
                        }
                        // step back one more
                        else
                        {
                            effectiveJumpDist -= 1;
                            effectivePrev = effectivePrev.prev;
                        }
                    }
                }
            }


            // good
            if (!failed && onLandOrJumpingOrFalling)
            {
                return true;
            }

            // not good, try rotated 90 degrees

            failed = false;
            onLand = false;
            for (int curX = x; curX >= 0 && curX > x - neededSizeForward; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeSide; curZ--)
                {
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (this[curX, curY, curZ] != BlockValue.Air)
                        {
                            failed = true;
                            break;
                        }
                    }

                    if (y > 0)
                    {
                        if (this[curX, y - 1, curZ] != BlockValue.Air)
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ) != BlockValue.Air)
                        {
                            onLand = true;
                        }
                    }
                    if (failed) break;
                }
                if (failed) break;
            }

            onLandOrJumpingOrFalling = onLand;
            if (!onLand && prev != null)
            {
                // we are just falling, that is fine
                if (prev.localY == y + 1)
                {
                    if (allowFalling)
                    {
                        onLandOrJumpingOrFalling = true;
                    }
                }
                // we aren't falling, are we jumping?
                else
                {
                    int effectiveJumpDist = jumpHeight;
                    SpreadNode effectivePrev = prev;
                    // we are allowed to travel jumpHeight steps in the air without touching ground
                    while (effectiveJumpDist > 0 && effectivePrev != null)
                    {
                        bool prevOnLand = OnLand(effectivePrev.localX, effectivePrev.localY, effectivePrev.localZ);
                        if (prevOnLand)
                        {
                            onLandOrJumpingOrFalling = true;
                            break;
                        }
                        // step back one more
                        else
                        {
                            effectiveJumpDist -= 1;
                            effectivePrev = effectivePrev.prev;
                        }
                    }
                }
            }
            return !failed && onLandOrJumpingOrFalling;
        }

        public void Refresh()
        {
            editNum = locationSpec.chunk.chunkData.editNum;
            cachedConnectedExits = null;
            cachedExits = null;
            cachedPathsBetween = null;

            bool wallsWereExpanded = wallsExpanded;
            wallsExpanded = false;

            GetExits();
            ConnectExits();


            if (neighborsConnectedTo.Count > 0)
            {
                List<PathingNode> needToExpand = new List<PathingNode>();
                foreach (PathingNode node in neighborsConnectedTo)
                {
                    node.DisconnectExitsFromNeighbor(this);
                    if (node.wallsExpanded)
                    {
                        needToExpand.Add(node);
                    }
                }
                wallsExpanded = false;
                neighborsConnectedTo.Clear();

                foreach (PathingNode node in needToExpand)
                {
                    node.wallsExpanded = false;
                    node.ExpandWalls();
                }
                

                if (wallsWereExpanded)
                {
                    ExpandWalls();
                }
            }
        }


        /*
         * 

        public int NumAirAbove(int x, int y, int z, int numNeeded)
        {
            int numAbove = 0;
            for (int curY = y+1; y < locationSpec.yWidth; y++)
            {
                if (this[x, curY, z] == BlockValue.Air)
                {
                    numAbove += 1;
                    if (numAbove >= numNeeded)
                    {
                        return numAbove;
                    }
                }
                else
                {
                    break;
                }
            }
            return numAbove;
        }

        public int NumAirBelow(int x, int y, int z, int numNeeded)
        {
            int numBelow = 0;
            for (int curY = y-1; y >= 0; y--)
            {
                if (this[x, curY, z] == BlockValue.Air)
                {
                    numBelow += 1;
                    if (numBelow >= numNeeded)
                    {
                        return numBelow;
                    }
                }
                else
                {
                    break;
                }
            }
            return numBelow;
        }

        public int ColumnHeight(int x, int y, int z, int numNeeded)
        {
            if (this[x,y,z] != BlockValue.Air)
            {
                return 0;
            }
            else
            {
                numNeeded -= 1;
                int numAbove = NumAirAbove(x, y, z, numNeeded);
                numNeeded -= numAbove;
                int numBelow = NumAirBelow(x, y, z, numNeeded);
                return numAbove + numBelow + 1;
            }
        }
        public bool MeetsFitCriteriaOnBorder(int x, int y, int z)
        {
            for (int curX = x; curX >= 0 && curX > x - neededSizeSide; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeForward; curZ--)
                {
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (this[curX, curY, curZ] != BlockValue.Air)
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed) break;
                }
                if (failed) break;
            }
        }

        public bool MeetsFitCriteriaSlice(int x, int y, int z, bool widthAlongXAxis)
        {
            if (this[x, y, z] != BlockValue.Air)
            {
                return false;
            }

            bool fitsCenter = ColumnHeight(x, y, z, neededSizeUp) >= neededSizeUp;

            if (!fitsCenter)
            {
                return false;
            }

            int curNeededWidth = neededSizeSide;
            curNeededWidth -= 1;

            if (curNeededWidth <= 0)
            {
                return true;
            }

            if (widthAlongXAxis)
            {
                for (int curX = x - 1; curX >= 0; curX--)
                {
                    bool fitsShiftedColumn = ColumnHeight(curX, y, z, neededSizeUp) >= neededSizeUp;
                    if (fitsShiftedColumn)
                    {
                        curNeededWidth -= 1;
                        if (curNeededWidth <= 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                for (int curX = x + 1; curX < locationSpec.xWidth; curX++)
                {
                    bool fitsShiftedColumn = ColumnHeight(curX, y, z, neededSizeUp) >= neededSizeUp;
                    if (fitsShiftedColumn)
                    {
                        curNeededWidth -= 1;
                        if (curNeededWidth <= 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return false;
            }
            else
            {

                for (int curZ = z - 1; curZ >= 0; curZ--)
                {
                    bool fitsShiftedColumn = ColumnHeight(x, y, curZ, neededSizeUp) >= neededSizeUp;
                    if (fitsShiftedColumn)
                    {
                        curNeededWidth -= 1;
                        if (curNeededWidth <= 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                for (int curZ = z + 1; curZ < locationSpec.zWidth; curZ++)
                {
                    bool fitsShiftedColumn = ColumnHeight(x, y, curZ, neededSizeUp) >= neededSizeUp;
                    if (fitsShiftedColumn)
                    {
                        curNeededWidth -= 1;
                        if (curNeededWidth <= 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                return false;
            }
        }
        
        */

        public BlockValue this[int localX, int localY, int localZ]
        {
            get
            {
                return locationSpec[localX, localY, localZ];
            }
            set
            {

            }
        }


        public delegate void PositionCallback(int x, int y, int z);

        public void LoopThroughBorder(PositionCallback callback)
        {
            int minX = 0;
            
            for (int x = 0; x < locationSpec.xWidth; x++)
            {
                for (int y = 0; y < locationSpec.yWidth; y++)
                {
                    // because pathing works by always choosing a fixed point on you (if you are more than one block wide), MeetsFitCriteria will never return true for some borders unless we actually look at slightly inside the borders instead
                    if (neededSizeForward == 1)
                    {
                        callback(x, y, 0);
                    }
                    else if (neededSizeForward - 1 < locationSpec.zWidth)
                    {
                        callback(x, y, neededSizeForward - 1);
                    }
                    if (neededSizeSide == 1)
                    {
                        callback(x, y, 0);
                    }
                    else if (neededSizeSide - 1 < locationSpec.zWidth)
                    {
                        callback(x, y, neededSizeSide - 1);
                    }
                }
            }

            for (int x = 0; x < locationSpec.xWidth; x++)
            {
                for (int z = 0; z < locationSpec.zWidth; z++)
                {
                    if (neededSizeUp == 1)
                    {
                        callback(x, 0, z);
                    }
                    else if(neededSizeUp - 1 < locationSpec.yWidth)
                    {
                        callback(x, neededSizeUp - 1, z);
                    }
                }
            }


            for (int y = 0; y < locationSpec.yWidth; y++)
            {
                for (int z = 0; z < locationSpec.zWidth; z++)
                {
                    if (neededSizeForward == 1)
                    {
                        callback(0, y, z);
                    }
                    else if (neededSizeForward - 1 < locationSpec.xWidth)
                    {
                        callback(neededSizeForward - 1, y, z);
                    }
                    if (neededSizeSide == 1)
                    {
                        callback(0, y, z);
                    }
                    else if (neededSizeSide-1 < locationSpec.xWidth)
                    {
                        callback(neededSizeSide - 1, y, z);
                    }
                }
            }


            if (locationSpec.zWidth > 1)
            {
                int maxZ = (int)(locationSpec.zWidth)-1;
                for (int x = 0; x < locationSpec.xWidth; x++)
                {
                    for (int y = 0; y < locationSpec.yWidth; y++)
                    {
                        callback(x, y, maxZ);
                    }
                }
            }


            if (locationSpec.yWidth > 1)
            {
                int maxY = (int)(locationSpec.yWidth) - 1;
                for (int x = 0; x < locationSpec.xWidth; x++)
                {
                    for (int z = 0; z < locationSpec.zWidth; z++)
                    {
                        callback(x, maxY, z);
                    }
                }
            }


            if (locationSpec.xWidth > 1)
            {
                int maxX = (int)(locationSpec.xWidth) - 1;
                for (int z = 0; z < locationSpec.zWidth; z++)
                {
                    for (int y = 0; y < locationSpec.yWidth; y++)
                    {
                        callback(maxX, y, z);
                    }
                }
            }
        }

        public void LoopThroughNeighbors(int x, int y, int z, PositionCallback callback)
        {
            if (x > 0) callback(x - 1, y, z);
            if (y > 0) callback(x, y - 1, z);
            if (z > 0) callback(x, y, z - 1);

            if (x < locationSpec.xWidth - 1) callback(x + 1, y, z);
            if (y < locationSpec.yWidth - 1) callback(x, y + 1, z);
            if (z < locationSpec.zWidth - 1) callback(x, y, z + 1);
        }


        delegate bool MeetsCriteria(SpreadNode n);

        void DoSpread(int startX, int startY, int startZ, MeetsCriteria meetsSpreadCriteria)
        {
            List<Tuple<int, int, int>> startPoints = new List<Tuple<int, int, int>>(1);
            startPoints.Add(new Tuple<int, int, int>(startX, startY, startZ));
            DoSpread(startPoints.ToArray(), meetsSpreadCriteria);
        }


        void DoSpread(Tuple<int, int, int>[] startPoints, MeetsCriteria meetsSpreadCriteria)
        {
            Queue<SpreadNode> needToBeProcessed = new Queue<SpreadNode>(System.Math.Max(10, startPoints.Length*2));
            foreach (Tuple<int, int, int> startPoint in startPoints)
            {
                needToBeProcessed.Enqueue(new SpreadNode(startPoint.a, startPoint.b, startPoint.c, null, this));
            }

            while (needToBeProcessed.Count > 0)
            {
                SpreadNode cur = needToBeProcessed.Dequeue();
                // for each neighbor of us, see if they are also valid exit segments. If so, add them as well
                LoopThroughNeighbors(cur.localX, cur.localY, cur.localZ, (nx, ny, nz) =>
                {
                    SpreadNode neighborNode = new SpreadNode(nx, ny, nz, cur, this);
                    if (meetsSpreadCriteria(neighborNode))
                    {
                        needToBeProcessed.Enqueue(neighborNode);
                    }
                });
            }
        }

        // pathing works like this
        // Pathing(startPos, goalPos)
        //     1. Generate (or update) pathing node that contains startPos and goalPos
        //     2. Find connected exits to startPos and goalPos
        //     3. Do A* search out from those exits



        List<Tuple<PathingNodeExit, SpreadNode>> FindConnectedExits(long startX, long startY, long startZ, bool allowFalling, bool reversed=false)
        {
            ClearVisited();

            int localStartX = (int)(startX - locationSpec.minX);
            int localStartY = (int)(startY - locationSpec.minY);
            int localStartZ = (int)(startZ - locationSpec.minZ);


            List<PathingNodeExit> exits = GetExits();
            List<Tuple<PathingNodeExit, SpreadNode>> res = new List<Tuple<PathingNodeExit, SpreadNode>>();
            HashSet<int> foundExits = new HashSet<int>();
            DoSpread(localStartX, localStartY, localStartZ, (n) =>
            {
                if (data[n.localX,n.localY,n.localZ,PathingFlags.Visited])
                {
                    return false;
                }

                data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;

                ushort exitNum = data[n.localX, n.localY, n.localZ];

                bool canWeFit = MeetsFitCriteria(n.localX, n.localY, n.localZ, n.prev, allowFalling: allowFalling, reversed: reversed);
                // if we found an exit node (non-zero means exit node) and we haven't seen it yet, record the path
                if (exitNum != 0 && !foundExits.Contains(exitNum) && canWeFit)
                {
                    foundExits.Add(exitNum);
                    int exitNumI = exitNum - 1;
                    res.Add(new Tuple<PathingNodeExit, SpreadNode>(exits[exitNumI], n));
                }

                // only trickle if we can actually move through here
                return canWeFit;
            });

            return res;
        }


        void ClearVisited()
        {
            for (int x = 0; x < locationSpec.xWidth; x++)
            {
                for (int y = 0; y < locationSpec.yWidth; y++)
                {
                    for (int z = 0; z < locationSpec.zWidth; z++)
                    {
                        data[x, y, z, PathingFlags.Visited] = false;
                    }
                }
            }
        }

        List<Tuple<PathingNode, List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>>[,]>> cachedConnectedExits = new List<Tuple<PathingNode, List<Tuple<Tuple<int, int, int>, Tuple<int, int, int>>>[,]>>();




        public SpreadNode FindPathThroughExit(int localStartX, int localStartY, int localStartZ, int localEndX, int localEndY, int localEndZ, ushort exitNum)
        {
            List<PathingNodeExit> exits = GetExits();

            int exitNumI = exitNum - 1;
            PathingNodeExit exit = exits[exitNumI];

            // clear visited
            for (int j = 0; j < exit.localPositions.Length; j++)
            {
                Tuple<int, int, int> localPosInExit = exit.localPositions[j];
                data[localPosInExit.a, localPosInExit.b, localPosInExit.c, PathingFlags.Visited] = false;
            }

            SpreadNode pathThroughExit = null;
            // connect current position in exit to place where we are leaving exit
            DoSpread(localStartX, localStartY, localStartZ, (n) =>
            {
                // if we found the location, we are good, we don't need to spread anymore
                if (pathThroughExit != null)
                {
                    return false;
                }
                //Debug.Log("spread to node " + n);
                // only spread through exit

                if (data[n.localX, n.localY, n.localZ, PathingFlags.Visited])
                {
                    return false;
                }

                if (data[n.localX, n.localY, n.localZ] == exitNum)
                {
                    data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;
                    // if we found the location, we are good, we don't need to spread anymore
                    if (n.localX == localEndX && n.localY == localEndY && n.localZ == localEndZ)
                    {
                        pathThroughExit = n;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            });


            if (pathThroughExit == null)
            {
                Debug.LogWarning("unable to find path through exit, perhaps exit is segmented by a falling section? also, the ids were " + data[localStartX, localStartY, localStartZ] + " " + data[localEndX, localEndY, localEndZ]);
            }
            return pathThroughExit;
        }



        public static long curRunId = 0;
        public static PathingSpreadNode Pathfind(World world, LVector3 startPos, LVector3 endPos, int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight, out bool success)
        {
            long startTime = PhysicsUtils.millis();
            success = false;
            Chunk startChunk = world.GetChunkAtPos(startPos.x, startPos.y, startPos.z);
            Chunk endChunk = world.GetChunkAtPos(endPos.x, endPos.y, endPos.z);

            if (startChunk == null || endChunk == null)
            {
                Debug.LogWarning("pathfinding failed because either start or end chunk is not generated yet");
                return null;
            }



            PathingNode startNode = startChunk.GetPathingNode(neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight);
            PathingNode endNode = endChunk.GetPathingNode(neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight);


            if (startNode == endNode)
            {
                Debug.LogWarning("need to pathfind in same pathing node, use something else?");
                return null;
            }

            Priority_Queue.SimplePriorityQueue<PathingSpreadNode, long> pQueue = new Priority_Queue.SimplePriorityQueue<PathingSpreadNode, long>();



            long midTime = PhysicsUtils.millis();
            List<Tuple<PathingNodeExit, SpreadNode>> connectedExitsToStart = startNode.FindConnectedExits(startPos.x, startPos.y, startPos.z, true);
            // the issue right now is this traverses from the end node out to the exits, but we need to traverse from the exits to the end node to have the code that checks "can you walk there" do it properly
            List<Tuple<PathingNodeExit, SpreadNode>> connectedExitsToEnd = endNode.FindConnectedExits(endPos.x, endPos.y, endPos.z, true, reversed:true);

            world.MakeLoggingNode("resPos", "start", Color.gray, startPos.x, startPos.y, startPos.z);
            world.MakeLoggingNode("resPos", "end", Color.gray, endPos.x, endPos.y, endPos.z);

            foreach (Tuple<PathingNodeExit, SpreadNode> connectedExit in connectedExitsToEnd)
            {
                // set the values of the exits connected to the end pos to be equal to curRunId, this lets us easily test if we are connected to end in constant time in the logic below
                connectedExit.a.connectedToExit = curRunId;
            }

            PathingSpreadNode closest = null;
            long closestEstimatedDistToEnd = long.MaxValue;

            foreach (Tuple<PathingNodeExit, SpreadNode> connectedExit in connectedExitsToStart)
            {
                PathingNodeExit exit = connectedExit.a;
                SpreadNode pathFromStartToExit = connectedExit.b;
                PathingSpreadNode curPathingNode = new PathingSpreadNode(exit, null, pathFromStartToExit);
                long estimatedDistToEnd = connectedExit.b.Dist(endPos.x, endPos.y, endPos.z);
                long totalCost = curPathingNode.distFromStart + estimatedDistToEnd;

                if (estimatedDistToEnd < closestEstimatedDistToEnd)
                {
                    closestEstimatedDistToEnd = estimatedDistToEnd;
                    closest = curPathingNode;
                }

                pQueue.Enqueue(curPathingNode, totalCost);
            }


            long loopTime = PhysicsUtils.millis();
            int maxSteps = 1000;
            int numSteps = 0;
            bool foundCompletePath = false;
            while (pQueue.Count > 0 && (numSteps < maxSteps || PhysicsUtils.millis() - loopTime < 1000))
            {
                numSteps += 1;
                PathingSpreadNode curNode = pQueue.Dequeue();

                //world.MakeLoggingNode("resPos", "considered step " + numSteps, Random.ColorHSV(), curNode.wx, curNode.wy, curNode.wz);


                // if we set connectedToExit to curRunId above, that means we found a node that is connected to the exit! We are done
                if (curNode.curExit.connectedToExit == curRunId)
                {
                    closest = curNode;
                    foundCompletePath = true;
                    break;
                }


                // TODO: this can potentially be sped up by only expanding walls if they touch the exit, we just need to finish ExpandWallsByExit
                // curNode.curExit.parentNode.ExpandWallsByExit(curNode.curExit);
                curNode.curExit.parentNode.ExpandWalls();

                foreach(PathingNodeExitConnection connectedExit in curNode.curExit.connectedExits)
                {
                    // visited will equal curRunId if we have visited there, otherwise it is less.
                    // this allows us to not have to bother and clear them each run, instead, curRunId is just incremented each run
                    // it is a long so we should be fine, running that many pathfinding iterations will take millions of years I think?
                    if (connectedExit.outExit.visited < curRunId)
                    {
                        connectedExit.outExit.visited = curRunId;


                        SpreadNode posOfCurNodeInExit = curNode.pathToPrevNode;


                        foreach (SpreadNode path in connectedExit.paths)
                        {
                            SpreadNode posLeavingExit = path.Root;

                            PathingNode curNodeParent = connectedExit.inExit.parentNode;

                            ushort exitValue = curNodeParent.data[posOfCurNodeInExit.localX, posOfCurNodeInExit.localY, posOfCurNodeInExit.localZ];

                            ushort otherExitValue = curNodeParent.data[posLeavingExit.localX, posLeavingExit.localY, posLeavingExit.localZ];



                            PathingSpreadNode spreadNodeThroughExit = curNode;


                            // if we don't leave from the exit in the same place we came from, we need to connect those two points
                            if (posOfCurNodeInExit.localX != posLeavingExit.localX ||
                                posOfCurNodeInExit.localY != posLeavingExit.localY ||
                                posOfCurNodeInExit.localZ != posLeavingExit.localZ)
                            {
                                SpreadNode pathThroughExit = curNodeParent.FindPathThroughExit(posOfCurNodeInExit.localX, posOfCurNodeInExit.localY, posOfCurNodeInExit.localZ, posLeavingExit.localX, posLeavingExit.localY, posLeavingExit.localZ, exitValue);
                                
                                if (pathThroughExit == null)
                                {
                                    continue;
                                }
                                spreadNodeThroughExit = new PathingSpreadNode(connectedExit.outExit, curNode, pathThroughExit);
                            }
                            else
                            {
                                //Debug.Log("direct connection, pos leaving exit " + posLeavingExit + " pos of cur node in exit " + posOfCurNodeInExit + " with exit value " + exitValue + " and leaving exit value " +  otherExitValue);
                         
                            }





                            PathingSpreadNode newNode = new PathingSpreadNode(connectedExit.outExit, spreadNodeThroughExit, path);
                            long estimatedDistToEnd = connectedExit.outExit.Dist(endPos.x, endPos.y, endPos.z);
                            long totalCost = newNode.distFromStart + estimatedDistToEnd;

                            if (estimatedDistToEnd < closestEstimatedDistToEnd)
                            {
                                closestEstimatedDistToEnd = estimatedDistToEnd;
                                closest = newNode;
                            }

                            pQueue.Enqueue(newNode, totalCost);
                        }
                    }
                }
            }

            long endTime = PhysicsUtils.millis();


            Debug.Log(startTime + " " + (midTime-startTime) + " " + (loopTime-midTime) + " " + (endTime-loopTime));

            curRunId += 1;

            // we found one! Now lookup the final path and then we are done
            if (foundCompletePath)
            {
                PathingSpreadNode actualRes = null;
                foreach (Tuple<PathingNodeExit, SpreadNode> connectedExit in connectedExitsToEnd)
                {
                    if (connectedExit.a == closest.curExit)
                    {
                        SpreadNode pathToGoal = connectedExit.b.InvertPath();

                        SpreadNode startOfPathToExit = connectedExit.b;
                        SpreadNode currentPos = closest.pathToPrevNode;

                        ushort exitNum1 = startOfPathToExit.parentNode.data[startOfPathToExit.localX, startOfPathToExit.localY, startOfPathToExit.localZ];
                        ushort exitNum2 = startOfPathToExit.parentNode.data[currentPos.localX, currentPos.localY, currentPos.localZ];

                        if (startOfPathToExit.wx != currentPos.wx || startOfPathToExit.wy != currentPos.wy || startOfPathToExit.wz != currentPos.wz)
                        {
                            Debug.Log("current pos " + currentPos + " start of path to exit " + startOfPathToExit + " trying to find path between with exit nums " + exitNum1 + " " + exitNum2);
                            SpreadNode pathThroughExit = startOfPathToExit.parentNode.FindPathThroughExit(startOfPathToExit.localX, startOfPathToExit.localY, startOfPathToExit.localZ, currentPos.localX, currentPos.localY, currentPos.localZ, exitNum1);
                            if (pathThroughExit != null)
                            {
                                closest = new PathingSpreadNode(connectedExit.a, closest, pathThroughExit);
                            }
                        }


                        // This is a path from endPos to that exit, but we need a path from that exit to end pos so we will just invert it
                        actualRes = new PathingSpreadNode(null, closest, pathToGoal);
                        closest = actualRes;
                        success = true;
                    }
                }

            }
            // search took too long, use the closest path
            else
            {

            }

            return closest;
        }

        public void ExpandWallsByExit(PathingNodeExit exit)
        {
            bool expandXUp = false;
            bool expandXDown = false;
            bool expandYUp = false;
            bool expandYDown = false;
            bool expandZUp = false;
            bool expandZDown = false;
            for(int i = 0; i < exit.localPositions.Length; i++)
            {
                int localX = exit.localPositions[i].a;
                int localY = exit.localPositions[i].b;
                int localZ = exit.localPositions[i].c;
            }
            // TODO
        }

        bool wallsExpanded = false;

        public void ExpandWalls()
        {
            if (wallsExpanded)
            {
                return;
            }
            wallsExpanded = true;
            Chunk myChunk = locationSpec.chunk;
            List<Chunk> neighboringChunks = new List<Chunk>();
            neighboringChunks.Add(world.GetChunk(myChunk.cx - 1, myChunk.cy, myChunk.cz));
            neighboringChunks.Add(world.GetChunk(myChunk.cx + 1, myChunk.cy, myChunk.cz));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy-1, myChunk.cz));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy+1, myChunk.cz));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy, myChunk.cz-1));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy, myChunk.cz+1));

            foreach (Chunk neighborChunk in neighboringChunks)
            {
                if (neighborChunk != null)
                {
                    PathingNode neighborNode = neighborChunk.GetPathingNode(neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight);
                    ConnectExitsToNeighbor(neighborNode);
                    neighborNode.ConnectExitsToNeighbor(this);
                }
            }
        }



        public delegate void ProcessNeighboringCells(int myX, int myY, int myZ, int theirX, int theirY, int theirZ);
        public void LoopThroughNeighboringCells(PathingNode neighbor, ProcessNeighboringCells callback)
        {
            if (neighbor.locationSpec.minX - 1 == locationSpec.maxX)
            {
                int myX = (int)(locationSpec.xWidth - 1);
                int theirX = System.Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                for (long y = System.Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = System.Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            else if (neighbor.locationSpec.minY - 1 == locationSpec.maxY)
            {
                int myY = (int)(locationSpec.yWidth - 1);
                int theirY = 0;
                for (long x = System.Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = System.Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            else if (neighbor.locationSpec.minZ - 1 == locationSpec.maxZ)
            {
                int myZ = (int)(locationSpec.zWidth - 1);
                int theirZ = System.Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                for (long y = System.Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = System.Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            else if (locationSpec.minX - 1 == neighbor.locationSpec.maxX)
            {
                int theirX = (int)(neighbor.locationSpec.xWidth - 1);
                int myX = System.Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                for (long y = System.Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = System.Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            else if (locationSpec.minY - 1 == neighbor.locationSpec.maxY)
            {

                int theirY = (int)(neighbor.locationSpec.yWidth - 1);
                int myY = 0; // todo: double check this
                for (long x = System.Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = System.Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            else if (locationSpec.minZ - 1 == neighbor.locationSpec.maxZ)
            {
                int theirZ = (int)(neighbor.locationSpec.zWidth - 1);
                int myZ = System.Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                for (long y = System.Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = System.Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);

                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
        }

        List<PathingNode> neighborsConnectedTo = new List<PathingNode>();


        public void DisconnectExitsFromNeighbor(PathingNode neighbor)
        {
            if (neighborsConnectedTo.Contains(neighbor))
            {
                foreach (PathingNodeExit exit in GetExits())
                {
                    exit.RemoveExitConnectionsFromPathingNode(neighbor);
                }
                neighborsConnectedTo.Remove(neighbor);
            }
            
        }

        // todo: double check relative coordinates aren't off when accessing actual world when checking if touch ground
        public void ConnectExitsToNeighbor(PathingNode neighbor)
        {
            foreach (PathingNode node in neighborsConnectedTo)
            {
                if (node == neighbor)
                {
                    return;
                }
            }

            neighborsConnectedTo.Add(neighbor);

            List<PathingNodeExit> myExits = GetExits();
            List<PathingNodeExit> neighborExits = neighbor.GetExits();

            PathingNodeExitConnection[,] myConnectionsToThem = new PathingNodeExitConnection[myExits.Count, neighborExits.Count];
            //PathingNodeExitConnection[,] theirConnectionsToUs = new PathingNodeExitConnection[neighborExits.Count, myExits.Count];

            LoopThroughNeighboringCells(neighbor, (myX, myY, myZ, theirX, theirY, theirZ) =>
            {
                ushort myExitVal = data[myX, myY, myZ];
                ushort theirExitVal = neighbor.data[theirX, theirY, theirZ];

                if (myExitVal != 0 && theirExitVal != 0)
                {
                    int myExitI = myExitVal - 1;
                    int theirExitI = theirExitVal - 1;
                    PathingNodeExit myExit = myExits[myExitI];
                    PathingNodeExit theirExit = neighborExits[theirExitI];
                    SpreadNode startPathToThem = new SpreadNode(myX, myY, myZ, null, this);
                    SpreadNode endPathToThem = new SpreadNode(theirX, theirY, theirZ, startPathToThem, neighbor);
                    //SpreadNode startPathToUs = new SpreadNode(theirX, theirY, theirZ, null, neighbor);
                    //SpreadNode endPathToUs = new SpreadNode(myX, myY, myZ, startPathToUs, this);
                    if (myConnectionsToThem[myExitI, theirExitI] == null)
                    {
                        myConnectionsToThem[myExitI, theirExitI] = new PathingNodeExitConnection(myExit, theirExit);
                        //theirConnectionsToUs[theirExitI, myExitI] = new PathingNodeExitConnection(theirExit, myExit);

                        myExit.AddExitConnection(myConnectionsToThem[myExitI, theirExitI]);
                        //theirExit.AddExitConnection(theirConnectionsToUs[theirExitI, myExitI]);
                    }

                    PathingNodeExitConnection myConnectionToThem = myConnectionsToThem[myExitI, theirExitI];
                    //PathingNodeExitConnection theirConnectionToUs = theirConnectionsToUs[theirExitI, myExitI];

                    myConnectionToThem.AddPath(endPathToThem);
                    //theirConnectionToUs.AddPath(endPathToUs);
                }
            });
        }


        SpreadNode[,] cachedPathsBetween;

        public SpreadNode[,] ConnectExits()
        {
            if (cachedPathsBetween != null)
            {
                return cachedPathsBetween;
            }

            List<PathingNodeExit> exits = GetExits();
            SpreadNode[,] pathsBetween = new SpreadNode[exits.Count, exits.Count];

            for (int i = 0; i < exits.Count; i++)
            {
                // they start with 1 since default value is 0
                int curExitVal = i + 1;
                int curExitI = i;

                ClearVisited();

                DoSpread(exits[i].localPositions, (n) =>
                {
                    if (data[n.localX, n.localY, n.localZ, PathingFlags.Visited])
                    {
                        return false;
                    }
                    data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;
                    ushort spreadExitVal = data[n.localX, n.localY, n.localZ];
                    int spreadExitI = spreadExitVal - 1;
                    // we reached an exit that is distinct from ours that we haven't reached before, mark that path
                    // since we are doing breadth first search, the first result will be optimal (or tied for optimal)
                    bool weCanFit = MeetsFitCriteria(n.localX, n.localY, n.localZ, n.prev);
                    if (spreadExitVal != 0 && spreadExitVal != curExitVal && pathsBetween[curExitI, spreadExitI] == null && weCanFit)
                    {

                        PathingNodeExitConnection exitConnection = new PathingNodeExitConnection(exits[curExitI], exits[spreadExitI]);
                        exitConnection.AddPath(n);
                        exits[curExitI].AddExitConnection(exitConnection);
                        pathsBetween[curExitI, spreadExitI] = n;
                    }

                    return weCanFit;
                });
            }

            cachedPathsBetween = pathsBetween;

            return cachedPathsBetween;
        }


        List<PathingNodeExit> cachedExits;

        public List<PathingNodeExit> GetExits()
        {
            if (cachedExits != null)
            {
                return cachedExits;
            }
            // reset visited to false
            LoopThroughBorder((x, y, z) =>
            {
                data[x, y, z, PathingFlags.Visited] = false;
            });


            List<PathingNodeExit> exits = new List<PathingNodeExit>();
            ushort exitI = 1;
            // go through all places on the border
            LoopThroughBorder((x, y, z) =>
            {
                // if we haven't been visited yet (no one else has spread into us) and can fit there, we are part of a new exit segment
                if (!data[x,y,z, PathingFlags.Visited] && MeetsFitCriteria(x,y,z, allowFalling: false))
                {
                    List<Tuple<int, int, int>> curExit = new List<Tuple<int, int, int>>();

                    // mark us as visited, now we can create a new exit and mark us as in it
                    data[x, y, z, PathingFlags.Visited] = true;
                    data[x, y, z] = exitI;
                    curExit.Add(new Tuple<int, int, int>(x, y, z));
                    
                    DoSpread(x, y, z, (n) =>
                    {
                        int sx = n.localX;
                        int sy = n.localY;
                        int sz = n.localZ;
                        //Debug.Log("checking neighbor " + sx + " " + sy + " " + sz + " of node " + x + " " + y + " " + z);
                        // spread out through all positions on the border that we can fit in
                        if (!data[sx,sy,sz,PathingFlags.Visited] && IsBorder(sx,sy,sz) && MeetsFitCriteria(sx, sy, sz, n.prev, allowFalling: false))
                        {
                            //Debug.Log("spreading at " + sx + " " + sy + " " + sz);
                            // mark as visited. We do this before we check if we can fit there so we only need to check if we can fit there once
                            data[sx, sy, sz, PathingFlags.Visited] = true;
                            // add to current exit group, this is good
                            curExit.Add(new Tuple<int, int, int>(sx, sy, sz));
                            data[sx, sy, sz] = exitI;
                            //Debug.Log("found exit at position " + sx + " " + sy + "" + sz);
                            return true;
                        }
                        // does not meet criteria, don't spread
                        else
                        {
                            return false;
                        }
                    });
                    

                    PathingNodeExit resCurExit = new PathingNodeExit(this, curExit.ToArray());

                    exits.Add(resCurExit);
                    // increment exit number since we just finished finding this one
                    exitI += 1;
                }

            });

            cachedExits = exits;

            //Debug.Log("got " + exits.Count + " exits with location spec " + locationSpec);
            for (int i = 0; i < exits.Count; i++)
            {
                Color exitColor = Random.ColorHSV();
                foreach (Tuple<int, int, int> pos in exits[i].localPositions)
                {
                    //world.MakeLoggingNode("exit tag", "exit " + i + " for " + locationSpec, exitColor, pos.a + locationSpec.minX, pos.b + locationSpec.minY, pos.c + locationSpec.minZ);
                }
            }

            return exits;
        }




        public bool IsBorder(int x, int y, int z)
        {
            return x == 0 || x == (locationSpec.xWidth - 1) ||
                y == 0 || y == (locationSpec.xWidth - 1) ||
                z == 0 || z == (locationSpec.xWidth - 1);
        }


    }

    public class PathingNodeLocationSpec
    {

    }


    public class PatchingNodeBlockChunkData
    {
        PathingNodeBlockChunk locationSpec;
        uint[,,] rawData;
        public PatchingNodeBlockChunkData(PathingNodeBlockChunk locationSpec)
        {
            this.locationSpec = locationSpec;
            rawData = new uint[locationSpec.xWidth,locationSpec.yWidth,locationSpec.zWidth];
        }

        public ushort this[int x, int y, int z]
        {
            get
            {
                return (ushort)(rawData[x, y, z] & 0xFFFF);
            }
            set
            {
                uint bitFlags = (rawData[x, y, z] & 0xFFFF0000);
                uint res = (bitFlags | value);
                rawData[x, y, z] = res;
            }
        }

        public bool this[int x, int y, int z, PathingFlags flag]
        {
            get
            {
                uint chosenBit = (uint)flag;
                return (rawData[x, y, z] & chosenBit) != 0;
            }
            set
            {
                uint chosenBit = (uint)flag;
                if (value)
                {
                    rawData[x, y, z] |= chosenBit;
                }
                else
                {
                    rawData[x, y, z] = (rawData[x, y, z] & (~chosenBit));
                }
            }
        }
    }


    public class PathingNodeBlockChunk : PathingNodeLocationSpec
    {
        World world;
        public long minX, minY, minZ;
        public long maxX, maxY, maxZ;

        public long xWidth, yWidth, zWidth;


        public override string ToString()
        {
            return "[" +
                minX + "," + maxX + "(" + xWidth + ")    " +
                minY + "," + maxY + "(" + yWidth + ")    " +
                minZ + "," + maxZ + "(" + zWidth + ")]";

        }

        public Chunk chunk;
        bool allOneChunk = false;
        
        public PathingNodeBlockChunk(World world, long minX, long minY, long minZ, long maxX, long maxY, long maxZ)
        {
            this.world = world;
            this.minX = minX; this.maxX = maxX;
            this.minY = minY; this.maxY = maxY;
            this.minZ = minZ; this.maxZ = maxZ;

            this.xWidth = maxX - minX+1;
            this.yWidth = maxY - minY+1;
            this.zWidth = maxZ - minZ+1;

            Chunk minChunk = world.GetChunkAtPos(minX, minY, minZ);
            Chunk maxChunk = world.GetChunkAtPos(maxX, maxY, maxZ);
            if (minChunk == maxChunk)
            {
                allOneChunk = true;
                chunk = minChunk;
            }
        }


        public BlockValue GetBlockOutsideRange(int localX, int localY, int localZ)
        {
            return world[localX + minX, localY + minY, localZ + minZ];
        }

        public BlockValue this[int localX, int localY, int localZ]
        {
            get
            {

                if (allOneChunk)
                {
                    return chunk[localX + minX, localY + minY, localZ + minZ];
                }
                else
                {
                    return world[localX + minX, localY + minY, localZ + minZ];
                }
            }
            set
            {

            }
        }



    }
}
