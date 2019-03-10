using Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public enum PathingFlags
    {
        Visited = (1 << 30),
        CanWeFitAbove = (1 << 29),
        CachedIfWeCanFitAbove = (1 << 28),
        CachedCanWeStandOn = (1 << 27),
        CanWeStandOn = (1 << 26)
    }


    public class MobilityCriteria
    {
        public int jumpHeight;
        public int neededSizeForward, neededSizeSide, neededSizeUp;

        public MobilityCriteria(int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight)
        {
            this.neededSizeForward = neededSizeForward;
            this.neededSizeSide = neededSizeSide;
            this.neededSizeUp = neededSizeUp;
            this.jumpHeight = jumpHeight;
        }

        // from https://stackoverflow.com/questions/2363143/whats-the-best-strategy-for-equals-and-gethashcode
        public override bool Equals(object obj)
        {
            var myClass = obj as MobilityCriteria;

            if (myClass != null)
            {
                // Order these by the most different first.
                // That is, whatever value is most selective, and the fewest
                // instances have the same value, put that first.
                return this.neededSizeUp == myClass.neededSizeUp
                   && this.jumpHeight == myClass.jumpHeight
                   && this.neededSizeForward == myClass.neededSizeForward
                   && this.neededSizeSide == myClass.neededSizeSide;
            }
            else
            {
                // This may not make sense unless GetHashCode refers to `base` as well!
                return base.Equals(obj);
            }
        }

        // from https://stackoverflow.com/questions/2363143/whats-the-best-strategy-for-equals-and-gethashcode
        public override int GetHashCode()
        {
            int hash = 19;
            unchecked
            { // allow "wrap around" in the int
                hash = hash * 31 + this.jumpHeight; // assuming integer
                hash = hash * 31 + this.neededSizeForward;  // assuming integer
                hash = hash * 31 + this.neededSizeSide; // assuming integer
                hash = hash * 31 + this.neededSizeUp;  // assuming integer
            }
            return hash;
        }
    }

    public class PathingResult
    {

        public PathingRegionPos result;

        public List<LVector3> positions = new List<LVector3>();
        public PathNode pathNode;

        public PathNode GetPathNode()
        {
            return pathNode;
        }

        public PathingResult(PathingRegionPos result)
        {
            this.result = result;

            int length = 0;

            PathingRegionPos tmp = result;
            while (tmp != null)
            {
                LVector3 curPos = new LVector3(tmp.wx, tmp.emptyBlockAboveY, tmp.wz);
                // don't add duplicate positions (this sometimes happens due to pathing stuffs)
                if (positions.Count > 0 && positions[positions.Count - 1] == curPos)
                {
                    //Debug.Log(curPos + " duplicate with " + positions[positions.Count - 1]);
                }
                else
                {
                    positions.Add(curPos);
                }
                tmp = tmp.prev;
            }

            // path normally goes from finish to start
            positions.Reverse();


            PathNode prev = null;
            for (int i = 0; i < positions.Count; i++)
            {
                PathNode cur = new PathNode(positions[i], prev);
                if (prev != null)
                {
                    prev.nextNode = cur;
                }
                prev = cur;
            }

            pathNode = prev;

        }



    }




    // For a chunk:
    //     for each non-air block in the chunk:
    //          if not in a region already and can be stood on/inside
    //             grow region to all blocks that can be reached from that block in a reversable way (so you can get back, i.e., no falling)
    //               ways to move:
    //                  sideways (spread immediately to blocks that also have air above them or can be stood on/inside)
    //                  up stairs (spread jump height (regular jump height is 1) below any block with air above it)
    //                  jump: TODO
    //             connect region to all blocks that can be reached from that block in an irreversable way (falling)
    //               we'll just make a "falling region": vertical column of air
    // For neighboring chunks:
    //   mark as "linked" any regions that can be reached in a reversable way
    //   mark as "linked" any regions that can be reached through the "falling regions"

    public class PathingRegionConnection
    {
        public PathingRegion from;
        public PathingRegion to;

        // used as a "visited"
        public long pathingCounter = -1;

        public PathingRegionPos posInFrom;
        public PathingRegionPos posInTo;

        public PathingRegionPos ConnectPathToConection(PathingRegionPos startPosInFrom)
        {
            PathingRegionPos pathToStartOfConnection = from.TraverseThroughRegion(startPosInFrom, posInFrom);
            PathingRegionPos pathToEndOfConnection = new PathingRegionPos(posInTo.localX, posInTo.localY, posInTo.localZ, pathToStartOfConnection, to.parentChunk);
            return pathToEndOfConnection;
        }

        public PathingRegionConnection(PathingRegion from, PathingRegion to, PathingRegionPos posInFrom, PathingRegionPos posInTo)
        {
            this.from = from;
            this.to = to;
            this.posInFrom = posInFrom;
            this.posInTo = posInTo;
        }
    }

    public class PathingRegionPos
    {
        public PathingChunk parentRegion;
        public PathingRegionPos prev;
        public int pathLen;
        public int localX, localY, localZ;
        public long wx { get { return parentRegion.locationSpec.minX + localX; } private set { } }
        public long wy { get { return parentRegion.locationSpec.minY + localY; } private set { } }
        public long wz { get { return parentRegion.locationSpec.minZ + localZ; } private set { } }



        public BlockValue Block
        {
            get
            {
                return parentRegion.locationSpec[localX, localY, localZ];
            }
            private set
            {

            }
        }

        public PathingRegionPos CopyPath()
        {
            if (prev != null)
            {
                return new PathingRegionPos(localX, localY, localZ, prev.CopyPath(), parentRegion);
            }
            else
            {
                return new PathingRegionPos(localX, localY, localZ, null, parentRegion);
            }
        }

        public void RemoveDuplicatePositionsInPath()
        {
            while (prev != null && prev.prev != null && PathingRegionPos.PositionsAreEqual(this, prev))
            {
                prev = prev.prev;
            }

            if (prev != null)
            {
                prev.RemoveDuplicatePositionsInPath();
            }
        }


        public long emptyBlockAboveY
        {
            get
            {
                for (long curY = wy; curY < wy + parentRegion.mobilityCriteria.jumpHeight + parentRegion.mobilityCriteria.neededSizeUp; curY++)
                {
                    bool canBreathe = true;

                    for (long curY2 = curY; curY2 < curY + parentRegion.mobilityCriteria.neededSizeUp; curY2++)
                    {
                        if (parentRegion.world[wx, curY2, wz] != BlockValue.Air)
                        {
                            canBreathe = false;
                        }
                    }

                    if (canBreathe)
                    {
                        return curY;
                    }
                }
                return wy;
            }
            set
            {

            }
        }

        Vector3 unityVec;
        bool gotUnityVec = false;
        public Vector3 UnityVector3()
        {
            if (!gotUnityVec)
            {
                unityVec = (new LVector3(wx, wy, wz)).BlockCentertoUnityVector3();
                gotUnityVec = true;
            }
            return unityVec;
        }

        public static long Dist(PathingRegionPos a, PathingRegionPos b)
        {
            return Math.Abs(a.wx - b.wx) + Math.Abs(a.wy - b.wy) + Math.Abs(a.wz - b.wz);
        }


        public static long Dist(PathingRegionPos a, LVector3 b)
        {
            return Math.Abs(a.wx - b.x) + Math.Abs(a.wy - b.y) + Math.Abs(a.wz - b.z);
        }

        public override string ToString()
        {
            return "[" +
                localX + "(" + wx + ")" + "," +
                localY + "(" + wy + ")" + "," +
                localZ + "(" + wz + ")" + "]";
        }


        public static long Dist(LVector3 a, PathingRegionPos b)
        {
            return Dist(b, a);
        }

        public static bool PositionsAreEqual(PathingRegionPos a, PathingRegionPos b)
        {
            return a.wx == b.wx && a.wy == b.wy && a.wz == b.wz;
        }

        public PathingRegionPos(LVector3 pos, PathingRegionPos prev, PathingChunk parentRegion)
        {
            InitFields(
                (int)(pos.x - parentRegion.locationSpec.minX),
                (int)(pos.y - parentRegion.locationSpec.minY),
                (int)(pos.z - parentRegion.locationSpec.minZ),
                prev, parentRegion);
        }
        public PathingRegionPos(int localX, int localY, int localZ, PathingRegionPos prev, PathingChunk parentRegion)
        {
            InitFields(localX, localY, localZ, prev, parentRegion);
        }

        void InitFields(int localX, int localY, int localZ, PathingRegionPos prev, PathingChunk parentRegion)
        {
            this.localX = localX;
            this.localY = localY;
            this.localZ = localZ;
            this.prev = prev;
            this.parentRegion = parentRegion;
            if (prev != null)
            {
                if (PositionsAreEqual(this, prev))
                {
                    pathLen = prev.pathLen;
                }
                else
                {
                    pathLen = prev.pathLen + 1;
                }
            }
            else
            {
                pathLen = 0;
            }
        }
    }

    public class PathingRegion
    {
        public int index;
        public long pathingVisited = -1;
        public enum PathingRegionType
        {
            Regular,
            Falling,
            FallingFromAnotherChunk,
        }

        public long cacheRunIndex = -1;
        public LVector3 cacheTmpBlock = LVector3.Invalid;
        public PathingRegionPos cacheTmpBlockVisibleFrom = null;
        public bool canSeeResourcesFromHere = false;

        public bool RegionTypeIsFalling()
        {
            return regionType == PathingRegionType.Falling || regionType == PathingRegionType.FallingFromAnotherChunk;
        }


        public Dictionary<int, int> blockCounts = new Dictionary<int, int>();

        public PathingRegionType regionType;

        public List<PathingRegionPos> positions;

        public List<PathingRegionConnection> thingsWeCanGoTo = new List<PathingRegionConnection>();

        public PathingRegionPos BottomPosition
        {
            get
            {
                return positions[positions.Count - 1];
            }
            set
            {

            }
        }

        public bool CanWeFitHere(long wx, long wy, long wz)
        {
            for (long curY = wy; curY < wy+parentChunk.mobilityCriteria.neededSizeUp; curY++)
            {
                if (PhysicsUtils.IsBlockSolid(parentChunk.world[wx, wy, wz]))
                {
                    return false;
                }
            }
            return true;
        }

        public void ExpandBottomToAccountForJump()
        {
            PathingRegionPos bottomPos = BottomPosition;
            PathingRegionPos belowBottomPos;
            PathingRegion regionBelowBottomPos = parentChunk.GetRegionNotUsingLocalCoordinates(bottomPos.wx, bottomPos.wy - 1, bottomPos.wz, out belowBottomPos);

            // we fall onto a regular region, see if we can jump up this chunk and get to other regular regions. If so, connect them
            if (regionBelowBottomPos != null && !regionBelowBottomPos.RegionTypeIsFalling())
            {
                for (long curY = bottomPos.wy; curY < bottomPos.wy+parentChunk.mobilityCriteria.jumpHeight; curY++)
                {
                    // can be here + 1 by jumping?

                    // no, we can't jump any higher then, be done
                    if (!CanWeFitHere(bottomPos.wx, curY+1, bottomPos.wz))
                    {
                        break;
                    }
                    // yes, see if any neighbor stairs go up so we can jump on them
                    else
                    {
                        parentChunk.LoopThroughNeighboringPositionsOnlyHorizontal(bottomPos.wx, curY, bottomPos.wz, (wnx, wny, wnz) =>
                        {
                            PathingRegionPos posInNeighborRegion;
                            PathingRegion neighborRegion = parentChunk.GetRegionNotUsingLocalCoordinates(wnx, wny, wnz, out posInNeighborRegion);

                            // we found a sparse stair, connect (connection from neighbor region to below is already made by going through us, we only need the below to them)
                            if (neighborRegion != null && !neighborRegion.RegionTypeIsFalling() && posInNeighborRegion.parentRegion.CanWeStandOn(posInNeighborRegion.localX, posInNeighborRegion.localY, posInNeighborRegion.localZ))
                            {
                                regionBelowBottomPos.AddRegionWeCanGoTo(new PathingRegionConnection(regionBelowBottomPos, neighborRegion, belowBottomPos, posInNeighborRegion));
                            }
                        });
                    }
                }
            }
        }


        /// <summary>
        /// Note: this will attach start pos to the path (using the prev link), so choose params accordingly
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public PathingRegionPos TraverseThroughRegion(PathingRegionPos startPos, PathingRegionPos endPos)
        {
            if (startPos.localX == endPos.localX && startPos.localY == endPos.localY && startPos.localZ == endPos.localZ)
            {
                return startPos;
            }


            // clear visited (we only need to do this for the positions in the region)
            foreach (PathingRegionPos position in this.positions)
            {
                parentChunk.data[position.localX, position.localY, position.localZ, PathingFlags.Visited] = false;
            }
            Priority_Queue.SimplePriorityQueue<PathingRegionPos, long> pqueue = new Priority_Queue.SimplePriorityQueue<PathingRegionPos, long>();
            long distToEnd = PathingRegionPos.Dist(startPos, endPos);
            pqueue.Enqueue(startPos, distToEnd);
            ushort regionNum = (ushort)(this.index + 1);
            PathingRegionPos result = null;
            while (pqueue.Count > 0 && result == null)
            {
                PathingRegionPos curPos = pqueue.Dequeue();

                parentChunk.LoopThroughNeighborsInChunk(curPos.localX, curPos.localY, curPos.localZ, (nx, ny, nz) =>
                {
                    if (result != null)
                    {
                        return;
                    }
                    if (!parentChunk.data[nx, ny, nz, PathingFlags.Visited] && parentChunk.data[nx, ny, nz] == regionNum)
                    {
                        parentChunk.data[nx, ny, nz, PathingFlags.Visited] = true;
                        PathingRegionPos neighborNode = new PathingRegionPos(nx, ny, nz, curPos, parentChunk);
                        pqueue.Enqueue(neighborNode, PathingRegionPos.Dist(neighborNode, endPos) + neighborNode.pathLen);

                        if (PathingRegionPos.PositionsAreEqual(neighborNode, endPos))
                        {
                            result = neighborNode;
                        }
                    }
                });


            }
            return result;
        }

        public void Draw()
        {
            if (RegionTypeIsFalling())
            {
                if (regionType == PathingRegionType.FallingFromAnotherChunk)
                {
                    GL.Color(Color.magenta);
                }
                else
                {
                    GL.Color(Color.red);
                }
                bool first = true;
                for (int i = 0; i < positions.Count; i++)
                {
                    GL.Vertex(positions[i].UnityVector3());
                    // lines need two points, so we get two vertices per pos unless we are the endpoints
                    if (i != 0 && i != positions.Count-1)
                    {
                        GL.Vertex(positions[i].UnityVector3());
                    }
                    if (positions.Count == 1)
                    {
                        GL.Vertex(positions[i].UnityVector3() - new Vector3(0, 0.5f, 0));
                    }
                }
            }
            else
            {
                GL.Color(Color.blue);
                bool first = true;
                for (int i = 0; i < positions.Count; i++)
                {
                    if (positions[i].prev != null)
                    {
                        GL.Vertex(positions[i].UnityVector3());
                        GL.Vertex(positions[i].prev.UnityVector3());
                    }
                    else
                    {
                        GL.Vertex(positions[i].UnityVector3() - new Vector3(0, 0.5f, 0));
                        GL.Vertex(positions[i].UnityVector3() + new Vector3(0, 0.5f, 0));
                    }
                }
            }

            foreach (PathingRegionConnection connection in thingsWeCanGoTo)
            {
                GL.Color(Color.cyan);
                GL.Vertex(connection.posInFrom.UnityVector3() + UnityEngine.Random.onUnitSphere * 0.01f);
                GL.Color(Color.green);
                GL.Vertex(connection.posInTo.UnityVector3() + UnityEngine.Random.onUnitSphere * 0.01f);

            }
        }

        /// <summary>
        /// Will also add connection in the other way if connection is regular -> falling and a jump can go the other way
        /// </summary>
        /// <param name="connection"></param>
        public void AddRegionWeCanGoTo(PathingRegionConnection connection)
        {
            thingsWeCanGoTo.Add(connection);
        }

        public PathingChunk parentChunk;

        public PathingRegion(int index, List<PathingRegionPos> positions, PathingRegionType regionType, PathingChunk parentChunk)
        {
            this.index = index;
            this.positions = positions;
            this.regionType = regionType;
            this.parentChunk = parentChunk;
        }

        public void Finish()
        {
            foreach (PathingRegionPos pos in positions)
            {
                BlockValue value = parentChunk.locationSpec[pos.localX, pos.localY, pos.localZ];
                if (!blockCounts.ContainsKey(value))
                {
                    blockCounts[value] = 0;
                }
                blockCounts[value] += 1;
            }
        }
    }

    public class PathingChunk
    {
        long editId;
        public PathingNodeBlockChunk locationSpec;
        public PatchingNodeBlockChunkData data;
        Chunk chunk;
        public World world;
        public MobilityCriteria mobilityCriteria;
        public List<Tuple<long, PathingChunk>> chunksDependingOn = new List<Tuple<long, PathingChunk>>();
        public HashSet<PathingChunk> chunksDependingOnQuickDupCheck = new HashSet<PathingChunk>();
        List<PathingRegion> regions = new List<PathingRegion>();

        public PathingChunk(Chunk chunk, MobilityCriteria mobilityCriteria)
        {
            this.locationSpec = new PathingNodeBlockChunk(chunk.world,
                chunk.cx * chunk.world.chunkSize,
                chunk.cy * chunk.world.chunkSize,
                chunk.cz * chunk.world.chunkSize,
                chunk.cx * chunk.world.chunkSize + chunk.world.chunkSize - 1,
                chunk.cy * chunk.world.chunkSize + chunk.world.chunkSize - 1,
                chunk.cz * chunk.world.chunkSize + chunk.world.chunkSize - 1);
            this.chunk = chunk;
            this.world = chunk.world;
            this.data = new PatchingNodeBlockChunkData(locationSpec);
            this.mobilityCriteria = mobilityCriteria;

            this.Refresh();
        }

        public void UpdateIfNeeded()
        {
            bool needToRefresh = false;
            if (chunk.chunkData.editNum != editId)
            {
                needToRefresh = true;
            }

            // check chunks we are depending on (usually the chunk above us, see if it has been 
            // modified cause that might modify some of our region data based on if air blocks were
            // removed above us so blocks that were regions may no longer be or vice versa)
            foreach (Tuple<long, PathingChunk> chunkDependingOn in chunksDependingOn)
            {
                if (chunkDependingOn.b.chunk.chunkData.editNum != chunkDependingOn.a)
                {
                    needToRefresh = true;
                }
            }
            if (needToRefresh)
            {
                Refresh();
            }
        }

        public void Draw()
        {
            world.blocksWorld.debugLineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            foreach (PathingRegion region in regions)
            {
                region.Draw();
            }
            GL.End();
        }

        public delegate void PositionCallback(int localX, int localY, int localZ);


        public void ResetData()
        {
            // disconnect us from our neighbors
            foreach (PathingChunk neighborConnectedTo in neighborsConnectedTo)
            {
                foreach (PathingRegion region in neighborConnectedTo.regions)
                {
                    List<PathingRegionConnection> connectionsWithoutMe = new List<PathingRegionConnection>();
                    foreach (PathingRegionConnection connection in region.thingsWeCanGoTo)
                    {
                        if (connection.to.parentChunk != this)
                        {
                            connectionsWithoutMe.Add(connection);
                        }
                    }
                    region.thingsWeCanGoTo = connectionsWithoutMe;
                }

                if (neighborConnectedTo.neighborsConnectedTo.Contains(this))
                {
                    neighborConnectedTo.neighborsConnectedTo.Remove(this);
                }
            }


            LoopThroughAllPositions((x, y, z) =>
            {
                data[x, y, z] = 0;
                data[x, y, z, PathingFlags.Visited] = false;
                data[x, y, z, PathingFlags.CachedIfWeCanFitAbove] = false;
                data[x, y, z, PathingFlags.CanWeFitAbove] = false;
                data[x, y, z, PathingFlags.CachedCanWeStandOn] = false;
                data[x, y, z, PathingFlags.CanWeStandOn] = false;
            });

            chunksDependingOn.Clear();
            chunksDependingOnQuickDupCheck.Clear();
            regions.Clear();
            neighborsConnectedTo.Clear();
            wallsExpanded = false;
        }

        // starts at top y position and goes down
        void LoopThroughAllPositions(PositionCallback callback)
        {
            for (int y = (int)(locationSpec.yWidth - 1); y >= 0; y--)
            {
                for (int x = 0; x < locationSpec.xWidth; x++)
                {
                    for (int z = 0; z < locationSpec.zWidth; z++)
                    {
                        callback(x, y, z);
                    }
                }
            }
        }

        public void LoopThroughNeighborsInChunk(int x, int y, int z, PositionCallback callback)
        {
            if (x > 0) callback(x - 1, y, z);
            if (y > 0) callback(x, y - 1, z);
            if (z > 0) callback(x, y, z - 1);

            if (x < locationSpec.xWidth - 1) callback(x + 1, y, z);
            if (y < locationSpec.yWidth - 1) callback(x, y + 1, z);
            if (z < locationSpec.zWidth - 1) callback(x, y, z + 1);
        }


        // x and z neighbors only
        void LoopThroughNonVerticalNeighborsInChunk(int x, int y, int z, PositionCallback callback)
        {
            if (x > 0) callback(x - 1, y, z);
            if (z > 0) callback(x, y, z - 1);

            if (x < locationSpec.xWidth - 1) callback(x + 1, y, z);
            if (z < locationSpec.zWidth - 1) callback(x, y, z + 1);
        }

        public bool CanWeStandOn(int x, int y, int z, int additionalSpaceNeeded=0)
        {
            // only cache if we don't need additional space
            if (additionalSpaceNeeded == 0)
            {
                if (data[x, y, z, PathingFlags.CachedCanWeStandOn])
                {
                    return data[x, y, z, PathingFlags.CanWeStandOn];
                }
                data[x, y, z, PathingFlags.CachedCanWeStandOn] = true;
            }
            BlockValue block = locationSpec[x, y, z];
            bool res = (PhysicsUtils.IsBlockSolid(block) || PhysicsUtils.IsBlockLiquid(block)) && CanWeFitAbove(x, y, z, additionalSpaceNeeded: additionalSpaceNeeded);

            //Debug.Log("got can we stand on " + res + " for block " + x + " " + y + " " + z + " with can we fit above " + CanWeFitAbove(x, y, z));
            if (additionalSpaceNeeded == 0)
            {
                data[x, y, z, PathingFlags.CanWeStandOn] = res;
            }
            return res;
        }

        public bool CanWeFitAbove(int x, int y, int z, int additionalSpaceNeeded=0)
        {
            if (additionalSpaceNeeded == 0)
            {
                if (data[x, y, z, PathingFlags.CachedIfWeCanFitAbove])
                {
                    return data[x, y, z, PathingFlags.CanWeFitAbove];
                }
                data[x, y, z, PathingFlags.CachedIfWeCanFitAbove] = true;
            }
            for (int curY = y+1; curY < y+1+mobilityCriteria.neededSizeUp+ additionalSpaceNeeded; curY++)
            {
                BlockValue blockAtPos;
                if (curY >= locationSpec.yWidth)
                {
                    blockAtPos = GetBlockOutsideRange(x, curY, z);
                }
                else
                {
                    blockAtPos = locationSpec[x, curY, z];
                }
                if (PhysicsUtils.IsBlockSolid(blockAtPos))
                {
                    if (additionalSpaceNeeded == 0)
                    {
                        data[x, y, z, PathingFlags.CanWeFitAbove] = false;
                    }
                    return false;
                }
            }
            if (additionalSpaceNeeded == 0)
            {
                data[x, y, z, PathingFlags.CanWeFitAbove] = true;
            }
            return true;
        }

        BlockValue GetBlockOutsideRange(int localX, int localY, int localZ)
        {
            long wx = localX + locationSpec.minX;
            long wy = localY + locationSpec.minY;
            long wz = localZ + locationSpec.minZ;
            Chunk thatChunk = world.GetChunkAtPos(wx, wy, wz);
            if (thatChunk == null)
            {
                return BlockValue.Wildcard;
            }
            else
            {
                PathingChunk newDep = thatChunk.GetPathingChunk(mobilityCriteria);
                if (!chunksDependingOnQuickDupCheck.Contains(newDep))
                {
                    chunksDependingOnQuickDupCheck.Add(newDep);
                    chunksDependingOn.Add(new Tuple<long, PathingChunk>(newDep.editId, newDep));
                }
                return thatChunk[wx, wy, wz];
            }
        }
        
        delegate bool ShouldSpreadCallback(PathingRegionPos pos);
        delegate bool ShouldBeDoneCallback(PathingRegionPos pos);

        /// <summary>
        /// Spreads out from start pos
        /// Note that shouldSpread and shouldBeDone will be called on start pos first
        /// shouldSpread and shouldBeDone will always either both be called on a point, or neither be called on a point.
        /// shouldSpread will always be called on a point before shouldBeDone.
        /// </summary>
        /// <param name="startX">local x position to start</param>
        /// <param name="startY">local y position to start</param>
        /// <param name="startZ">local z position to start</param>
        /// <param name="shouldSpread">should return true if we should spread out from that node</param>
        /// <param name="shouldBeDone">should return true if we found what we are looking for (if relevant), once this returns true, no more spreading will occur</param>
        void Spread(int startX, int startY, int startZ, ShouldSpreadCallback shouldSpread, ShouldBeDoneCallback shouldBeDone)
        {
            PathingRegionPos curPos = new PathingRegionPos(startX, startY, startZ, null, this);

            Queue<PathingRegionPos> stuffToExpand = new Queue<PathingRegionPos>();
            // first call stuff on the start pos
            bool startShouldSpread = shouldSpread(curPos);
            bool startShouldBeDone = shouldBeDone(curPos);
            if (startShouldSpread && !startShouldBeDone)
            {
                stuffToExpand.Enqueue(curPos);
            }

            bool weAreDone = false;
            while (stuffToExpand.Count > 0 && !weAreDone)
            {
                PathingRegionPos curToExpand = stuffToExpand.Dequeue();

                LoopThroughNeighborsInChunk(curToExpand.localX, curToExpand.localY, curToExpand.localZ, (nx, ny, nz) => {
                    if (!weAreDone)
                    {
                        PathingRegionPos next = new PathingRegionPos(nx, ny, nz, curToExpand, this);
                        bool spread = shouldSpread(next);
                        bool done = shouldBeDone(next);
                        if (done)
                        {
                            weAreDone = true;
                        }
                        if (spread)
                        {
                            if (!weAreDone)
                            {
                                stuffToExpand.Enqueue(next);
                            }
                        }
                    }
                });
            }
        }

        delegate void SpreadReversableTraversableCallback(PathingRegionPos pos);


        void ClearVisited()
        {
            LoopThroughAllPositions((x, y, z) =>
            {
                data[x, y, z, PathingFlags.Visited] = false;
            });
        }

        /// <summary>
        /// The first thing called back is the start pos (if we can stand on it)
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="startZ"></param>
        /// <param name="callback"></param>
        /// <param name="clearVisited"></param>
        void SpreadReversableTraversable(int startX, int startY, int startZ, SpreadReversableTraversableCallback callback, bool clearVisited=true)
        {
            if (clearVisited)
            {
                ClearVisited();
            }

            if (data[startX,startY,startZ,PathingFlags.Visited])
            {
                return;
            }

            if (CanWeStandOn(startX, startY, startZ))
            {
                PathingRegionPos curPos = new PathingRegionPos(startX, startY, startZ, null, this);

                Queue<PathingRegionPos> nextStuff = new Queue<PathingRegionPos>();

                nextStuff.Enqueue(curPos);

                while (nextStuff.Count > 0)
                {
                    curPos = nextStuff.Dequeue();

                    callback(curPos);

                    LoopThroughNeighborsInChunk(curPos.localX, curPos.localY, curPos.localZ, (nx, ny, nz) =>
                    {
                        // if we have not visited it in our search yet
                        if (!data[nx, ny, nz, PathingFlags.Visited])
                        {
                            bool reversableTraversable = false;
                            // if neighbor can be stood on, that is good we can spread
                            if (CanWeStandOn(nx, ny, nz))
                            {
                                reversableTraversable = true;
                            }
                            // otherwise, if neighbor has a block above it jumpHeight number of blocks (usually 1) that can be stood on, we can still spread
                            else if(PhysicsUtils.IsBlockSolid(locationSpec[nx, ny, nz]) || PhysicsUtils.IsBlockLiquid(locationSpec[nx, ny, nz]))
                            {
                                for (int curY = ny + 1; curY < ny + 1 + mobilityCriteria.jumpHeight && curY < locationSpec.yWidth; curY++)
                                {
                                    if (CanWeStandOn(nx, curY, nz))
                                    {
                                        // check if we can get from them to us
                                        // they are below us, make sure they have that many extra spaces above them
                                        // this is to prevent situations like this:
                                        // g x
                                        // x o
                                        // x x
                                        // o is trying to get to g, x is in the way above (x is a solid block, o and g are air)
                                        if (curY < curPos.localY)
                                        {
                                            if (CanWeFitAbove(nx, curY, nz, additionalSpaceNeeded: (curPos.localY - curY)))
                                            {
                                                reversableTraversable = true;
                                            }
                                        }
                                        // we are below them, make sure we have that many extra spaces above us to get to them
                                        else
                                        {
                                            if (CanWeFitAbove(curPos.localX, curPos.localY, curPos.localZ, additionalSpaceNeeded: (curY - curPos.localY)))
                                            {
                                                reversableTraversable = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // if we can get to it reversably, mark it as visited
                            if (reversableTraversable)
                            {
                                data[nx, ny, nz, PathingFlags.Visited] = true;
                                PathingRegionPos nextPos = new PathingRegionPos(nx, ny, nz, curPos, this);
                                nextStuff.Enqueue(nextPos);
                            }
                        }
                    });
                }
            }
        }

        public PathingRegion GetRegionStandingOn(LVector3 worldPos, out PathingRegionPos posInRegion, bool branchToSides=true)
        {
            posInRegion = null;

            if (!locationSpec.ContainsPosition(worldPos.x, worldPos.y, worldPos.z))
            {
                Chunk worldChunk = world.GetChunkAtPos(worldPos);
                if (worldChunk != null)
                {
                    return worldChunk.GetPathingChunk(mobilityCriteria).GetRegionStandingOn(worldPos, out posInRegion, branchToSides: branchToSides);
                }
                else
                {
                    return null;
                }
            }

            int localX = (int)(worldPos.x - locationSpec.minX);
            int localY = (int)(worldPos.y - locationSpec.minY);
            int localZ = (int)(worldPos.z - locationSpec.minZ);

            ushort regionNum = data[localX, localY, localZ];

            if (branchToSides && !PhysicsUtils.IsBlockSolid(chunk.world[worldPos.x, worldPos.y, worldPos.z]) && !PhysicsUtils.IsBlockSolid(chunk.world[worldPos.x, worldPos.y - 1, worldPos.z]) && !PhysicsUtils.IsBlockSolid(chunk.world[worldPos.x, worldPos.y - 2, worldPos.z]))
            {
                PathingRegionPos actuallyResPos = null;
                PathingRegion actuallyResRegion = null;
                LoopThroughNeighboringPositionsOnlyHorizontal(worldPos.x, worldPos.y, worldPos.z, (nwx, nwy, nwz) =>
                {
                    if (!PhysicsUtils.IsBlockSolid(chunk.world[nwx, nwy, nwz]))
                    {
                        PathingRegionPos tmpActuallyResPos = null;
                        PathingRegion tmpActuallyResRegion = GetRegionStandingOn(new LVector3(nwx, nwy, nwz), out tmpActuallyResPos, branchToSides: false);
                        if (tmpActuallyResRegion != null)
                        {
                            if (actuallyResRegion == null || actuallyResPos.wy < tmpActuallyResPos.wy)
                            {
                                actuallyResRegion = tmpActuallyResRegion;
                                actuallyResPos = tmpActuallyResPos;
                            }
                        }
                    }
                });

                if (actuallyResRegion != null && actuallyResPos.wy >= worldPos.y-3)
                {
                    posInRegion = actuallyResPos;
                    return actuallyResRegion;
                }
                // we are not shifting/standing on a ledge, look below
            }




            Chunk chunkBelowUs = null;
            for (int curY = localY; curY >= 0; curY--)
            {
                regionNum = data[localX, curY, localZ];
                if (PhysicsUtils.IsBlockSolid(locationSpec[localX, curY, localZ]) && regionNum != 0)
                {
                    int regionIndex = regionNum - 1;
                    posInRegion = new PathingRegionPos(localX, curY, localZ, null, this);
                    PathingRegion regionRes = regions[regionIndex];
                    if (regionRes.RegionTypeIsFalling())
                    {
                        Debug.LogWarning("somehow hit falling region as region we are in? pathfinding to this may be difficult");
                    }
                    return regionRes;
                }

                if (PhysicsUtils.IsBlockSolid(locationSpec[localX, curY, localZ]))
                {
                    Debug.LogWarning("went down and hit solid block and we still couldn't find region, maybe the player is suffocating?");
                    return null;
                }
            }
            // look a chunk below, maybe we are falling or our head is just poking into this chunk at the bottom?
            chunkBelowUs = world.GetChunk(chunk.cx, chunk.cy - 1, chunk.cz);
            if (chunkBelowUs != null)
            {
                PathingChunk pathingChunkBelowUs = chunkBelowUs.GetPathingChunk(mobilityCriteria);
                return pathingChunkBelowUs.GetRegionStandingOn(new LVector3(worldPos.x, pathingChunkBelowUs.locationSpec.maxY, worldPos.z), out posInRegion);
            }
            // couldn't find any, sorry
            return null;
        }


        public static long pathingCounter = 0;

        public delegate bool IsGoalPathingRegion(PathingRegion region);
        public delegate long EstimateCost(PathingRegionPos pos);


        static bool ArrContains(BlockValue[] arr, BlockValue val)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == val)
                {
                    return true;
                }
            }
            return false;
        }

        public static PathingRegionPos PathindToResource(World world, LVector3 startPos, float reach, BlockValue[] resourcesLookingFor, MobilityCriteria mobilityCriteria, out bool success, out LVector3 blockSeenAtEnd, bool verbose = false)
        {
            blockSeenAtEnd = LVector3.Invalid;
            PathingRegionPos res = PathfindGeneric(world, startPos, (pathingRegion) =>
            {
                if (pathingRegion.cacheRunIndex != pathingCounter)
                {
                    pathingRegion.cacheRunIndex = pathingCounter;
                    if (pathingRegion.cacheTmpBlock != LVector3.Invalid)
                    {
                        // we have already found it in the past, there is one, we are good
                        if (ArrContains(resourcesLookingFor, pathingRegion.cacheTmpBlock.Block))
                        {
                            if (pathingRegion.cacheTmpBlockVisibleFrom == null || pathingRegion.cacheTmpBlock == LVector3.Invalid)
                            {
                                Debug.LogWarning("cache tmp block visble from " + pathingRegion.cacheTmpBlockVisibleFrom + " or cache tmp block " + pathingRegion.cacheTmpBlock + "  is null/invalid even though you have cached result?");
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }


                    // look through blocks in chunk first
                    for (int i = 0; i < resourcesLookingFor.Length; i++)
                    {
                        // if we know we have a block of that kind, go find it
                        if (pathingRegion.blockCounts.ContainsKey(resourcesLookingFor[i]) && pathingRegion.blockCounts[resourcesLookingFor[i]] > 0)
                        {
                            // go through each position and see if it is that block
                            for (int j = 0; j < pathingRegion.positions.Count; j++)
                            {
                                // it is that block, yay, we are good
                                if (pathingRegion.positions[j].Block == resourcesLookingFor[i])
                                {
                                    pathingRegion.cacheTmpBlock = new LVector3(pathingRegion.positions[j].wx, pathingRegion.positions[j].wy, pathingRegion.positions[j].wz);
                                    pathingRegion.cacheTmpBlockVisibleFrom = pathingRegion.positions[i];
                                    pathingRegion.canSeeResourcesFromHere = true;
                                    return true;
                                }
                            }
                        }
                    }

                    // in each position in chunk, cast 20 random rays of length equal to our reach to see if we run into the block we want
                    for (int i = 0; i < pathingRegion.positions.Count; i++)
                    {
                        LVector3 curPos = new LVector3(pathingRegion.positions[i].wx, pathingRegion.positions[i].wy, pathingRegion.positions[i].wz);

                        LVector3 found = LVector3.Invalid;
                        RaycastResults rayRes;
                        Vector3 curPosVec3 = curPos.BlockCentertoUnityVector3();
                        for (int t = 0; t < 20; t++)
                        {
                            if (PhysicsUtils.CustomRaycast(curPosVec3, UnityEngine.Random.onUnitSphere, reach, (b, bx, by, bz, pbx, pby, pbz) => { return true; }, (b, bx, by, bz, pbx, pby, pbz) =>

                            {
                                return ArrContains(resourcesLookingFor, b);
                            }, out rayRes))
                            {
                                pathingRegion.cacheTmpBlock = rayRes.hitBlock;
                                pathingRegion.cacheTmpBlockVisibleFrom = pathingRegion.positions[i];
                                pathingRegion.canSeeResourcesFromHere = true;
                                return true;
                            }
                        }
                    }
                    pathingRegion.cacheTmpBlock = LVector3.Invalid;
                    pathingRegion.cacheTmpBlockVisibleFrom = null;
                    pathingRegion.canSeeResourcesFromHere = false;
                    return false;
                }
                else
                {
                    return pathingRegion.canSeeResourcesFromHere;
                }
                return false;
            },
            // there isn't really an easy way to determine "how close we are" to a resource, so just return 0
            (pos) =>
            {
                return 0;
            }, mobilityCriteria, out success, verbose: false);

            if (res == null)
            {
                success = false;
                return null;
            }
            else
            {
                PathingRegion resRegion = res.parentRegion.GetRegion(res.localX, res.localY, res.localZ);


                PathingRegionPos resPos = resRegion.cacheTmpBlockVisibleFrom;
                blockSeenAtEnd = resRegion.cacheTmpBlock;

                PathingRegionPos fullRes = resRegion.TraverseThroughRegion(res, resPos);
                fullRes.RemoveDuplicatePositionsInPath();

                if (verbose)
                {
                    PathingRegionPos tmp = fullRes;
                    PathingRegionPos prev = null;
                    int curPosI = 0;
                    while (tmp != null)
                    {
                        if (prev != null && !PathingRegionPos.PositionsAreEqual(tmp, prev) && verbose)
                        {
                            world.MakeLoggingNode("resPath", "path " + curPosI, Color.green, tmp.wx, tmp.emptyBlockAboveY, tmp.wz);
                        }
                        prev = tmp;
                        tmp = tmp.prev;
                        curPosI += 1;
                    }

                }
                return fullRes;
            }

        }

        public static PathingRegionPos PathfindGeneric(World world, LVector3 startPos, IsGoalPathingRegion isGoal, EstimateCost costEstimator, MobilityCriteria mobilityCriteria, out bool success, bool verbose = false)
        {
            // TODO: sparse staircase, fix chunk connectings in sparse staircase too

            foreach (LoggingNode loggingNode in GameObject.FindObjectsOfType<LoggingNode>())
            {
                if (loggingNode.logTag == "resPath")
                {
                    GameObject.Destroy(loggingNode.gameObject);
                }
            }

            pathingCounter += 1;
            success = false;

            Chunk startChunk = world.GetChunkAtPos(startPos);
            if (startChunk == null)
            {
                Debug.LogWarning("start chunk " + startChunk + " for startPos " + startPos + " was null (not generated yet), cannot pathfind");
                return null;
            }


            PathingChunk startPathingChunk = startChunk.GetPathingChunk(mobilityCriteria);

            PathingRegionPos startPosInRegion;
            PathingRegion startRegion = startPathingChunk.GetRegionStandingOn(startPos, out startPosInRegion);
            if (startRegion == null)
            {
                Debug.LogWarning("start region " + startRegion + " for startPos " + startPos + " was null (not generated yet), cannot pathfind");
                return null;
            }

            if (verbose)
            {
                world.MakeLoggingNode("resPath", "start air", Color.gray, startPosInRegion.wx, startPosInRegion.emptyBlockAboveY, startPosInRegion.wz);
                world.MakeLoggingNode("resPath", "start", Color.gray, startPosInRegion.wx, startPosInRegion.wy, startPosInRegion.wz);
            }

            startRegion.parentChunk.ExpandWalls();
            PathingRegionPos res = null;

            if (isGoal(startRegion))
            {
                //Debug.Log("started in goal, ez");
                //res = startRegion.TraverseThroughRegion(startPosInRegion, endPosInRegion);
                res = startPosInRegion;
            }


            Priority_Queue.SimplePriorityQueue<Tuple<PathingRegionPos, PathingRegionConnection>, long> regionConnectionsToConsider = new Priority_Queue.SimplePriorityQueue<Tuple<PathingRegionPos, PathingRegionConnection>, long>();

            //Debug.Log("start region has " + startRegion.thingsWeCanGoTo.Count + " connections");

            foreach (PathingRegionConnection connection in startRegion.thingsWeCanGoTo)
            {
                if (res != null)
                {
                    break;
                }
                PathingRegionPos pathThroughConnection = connection.ConnectPathToConection(startPosInRegion);
                long cost = costEstimator(pathThroughConnection) + pathThroughConnection.pathLen;
                regionConnectionsToConsider.Enqueue(new Tuple<PathingRegionPos, PathingRegionConnection>(pathThroughConnection, connection), cost);


                if (isGoal(connection.to))
                {
                    if (verbose) Debug.Log("found exit region in initial set!");
                    res = pathThroughConnection;
                    //res = endRegion.TraverseThroughRegion(pathThroughConnection, endPosInRegion);
                    break;
                }
            }


            long startTime = PhysicsUtils.millis();
            int jj = 0;
            while (regionConnectionsToConsider.Count > 0 && res == null)
            {
                if ((PhysicsUtils.millis() - startTime) > 200)
                {
                    Debug.Log("bailing, took too long");
                    break;
                }
                jj++;
                Tuple<PathingRegionPos, PathingRegionConnection> curConnection = regionConnectionsToConsider.Dequeue();
                PathingRegionPos curPosInRegion = curConnection.a;
                curConnection.b.to.parentChunk.ExpandWalls();
                if (verbose) world.MakeLoggingNode("resPath", "considered " + jj + " " + curConnection.b.to.thingsWeCanGoTo.Count, Color.blue, curPosInRegion.wx, curPosInRegion.emptyBlockAboveY, curPosInRegion.wz);
                foreach (PathingRegionConnection connection in curConnection.b.to.thingsWeCanGoTo)
                {
                    // this is our "visited" marker, doing this lets us not have to clear it each time
                    if (connection.to.pathingVisited != pathingCounter)
                    {
                        connection.to.pathingVisited = pathingCounter;
                        PathingRegionPos pathThroughConnection = connection.ConnectPathToConection(curPosInRegion);
                        long cost = costEstimator(pathThroughConnection) + pathThroughConnection.pathLen;
                        regionConnectionsToConsider.Enqueue(new Tuple<PathingRegionPos, PathingRegionConnection>(pathThroughConnection, connection), cost);

                        if (isGoal(connection.to))
                        {
                            //res = endRegion.TraverseThroughRegion(pathThroughConnection, endPosInRegion);
                            res = pathThroughConnection;
                            if (verbose) Debug.Log("found exit region!");
                            break;
                        }
                    }
                }
            }


            if (res != null)
            {
                success = true;

                res.RemoveDuplicatePositionsInPath();


                //Debug.Log("successfully pathfound with len = " + res.pathLen);
                /*
                int i = 0;
                PathingRegionPos tmp = res;
                PathingRegionPos prev = null;
                while (tmp != null)
                {
                    i += 1;
                    if (prev != null && !PathingRegionPos.PositionsAreEqual(tmp, prev) && verbose)
                    {
                        world.MakeLoggingNode("resPath", "path " + i, Color.green, tmp.wx, tmp.emptyBlockAboveY, tmp.wz);
                    }
                    prev = tmp;
                    tmp = tmp.prev;
                }
                */
            }
            else
            {
                Debug.Log("failed to pathfind");
            }

            return res;
        }
        public static PathingRegionPos Pathfind(World world, LVector3 startPos, LVector3 endPos, MobilityCriteria mobilityCriteria, out bool success, bool verbose = false)
        {
            Chunk endChunk = world.GetChunkAtPos(endPos);
            success = false;
            if (endChunk == null)
            {
                Debug.LogWarning("either end chunk " + endChunk + " for endPos " + endPos + " was null (not generated yet), cannot pathfind");
                return null;
            }


            PathingChunk endPathingChunk = endChunk.GetPathingChunk(mobilityCriteria);

            PathingRegionPos endPosInRegion;
            PathingRegion endRegion = endPathingChunk.GetRegionStandingOn(endPos, out endPosInRegion);
            if (endRegion == null)
            {
                Debug.LogWarning("end region " + endRegion + " for endPos " + endPos + " was null (not generated yet, or not a region at all and instead inside blocks), cannot pathfind");
                return null;
            }

            endRegion.parentChunk.ExpandWalls();

            if (verbose)
            {
                world.MakeLoggingNode("resPath", "end air", Color.gray, endPosInRegion.wx, endPosInRegion.emptyBlockAboveY, endPosInRegion.wz);
                world.MakeLoggingNode("resPath", "end", Color.gray, endPosInRegion.wx, endPosInRegion.wy, endPosInRegion.wz);
                Debug.Log("end region has " + endRegion.thingsWeCanGoTo.Count + " connections");
            }



            PathingRegionPos foundPosInEndRegion = PathfindGeneric(world, startPos, (region) =>
            {
                return region == endRegion;
            }, (pos) =>
            {
                return PathingRegionPos.Dist(pos, endPos);
            }, mobilityCriteria, out success, verbose: verbose);


            // connect up to final position
            if (foundPosInEndRegion != null)
            {
                success = true;
                return endRegion.TraverseThroughRegion(foundPosInEndRegion, endPosInRegion);
            }
            else
            {
                success = false;
                return null;
            }
        }
        


        public void Refresh()
        {
            // clears visited, regions, region numbers, and CanWeStandOn and CanWeFitAbove caches
            ResetData();

            this.editId = chunk.chunkData.editNum;
            ushort regionI = 1;
            regions = new List<PathingRegion>();

            // get regular regions
            LoopThroughAllPositions((x, y, z) =>
            {
                if (data[x,y,z] == 0 && CanWeStandOn(x, y, z))
                {
                    List<PathingRegionPos> positions = new List<PathingRegionPos>();
                    SpreadReversableTraversable(x, y, z, (n) =>
                    {
                        data[n.localX, n.localY, n.localZ] = regionI;
                        positions.Add(n);
                    },
                    clearVisited: false);
                    PathingRegion region = new PathingRegion(regionI - 1, positions, PathingRegion.PathingRegionType.Regular, this);
                    regionI += 1;
                    regions.Add(region);
                    region.Finish();
                    //Debug.Log("added region at pos " + x + " " + y + " " + z);
                }
            });

            // get drop regions
            LoopThroughAllPositions((x, y, z) =>
            {
                ushort regionNum = data[x, y, z];
                int regionIndex = regionNum - 1;
                // if we are a region and we have a neighbor that is not, test to see if it is a drop possability
                if (regionNum != 0 && CanWeStandOn(x, y, z))
                {
                    LoopThroughNonVerticalNeighborsInChunk(x, y, z, (nx, ny, nz) =>
                    {
                        PathingRegion fromRegion = regions[regionIndex];
                        ushort neighborRegionNum = data[nx, ny, nz];
                        // if it is empty, not a region, and we can fit above it, we can drop into it, and below it is not the same region we came from (this also includes being not a region), mark it and the stuff below as a drop region
                        if (neighborRegionNum == 0 && CanWeFitAbove(nx, ny, nz) && !PhysicsUtils.IsBlockSolid(locationSpec[nx, ny, nz]) && 
                            ((ny > 0 && data[nx, ny-1, nz] != regionNum) || (ny == 0)))
                        {
                            MakeFallingRegionStartingAtPos(nx, ny, nz, new PathingRegionPos(x, y, z, null, this), fromRegion);
                        }
                        else if(neighborRegionNum != 0 && CanWeFitAbove(nx, ny, nz) && !PhysicsUtils.IsBlockSolid(locationSpec[nx, ny, nz]))
                        {
                            PathingRegion neighborRegion = GetRegion(nx, ny, nz);
                            if (neighborRegion.RegionTypeIsFalling())
                            {
                                fromRegion.AddRegionWeCanGoTo(new PathingRegionConnection(fromRegion, neighborRegion, new PathingRegionPos(x, y, z, null, this), new PathingRegionPos(nx, ny, nz, null, this)));
                            }
                        }
                    });
                }
            });

        }


        bool ValidPositionForAFallingRegion(int startX, int startY, int startZ, bool allowExistingRegion=false)
        {
            ushort fallingRegionNum = data[startX, startY, startZ];
            return ((allowExistingRegion || fallingRegionNum == 0) && CanWeFitAbove(startX, startY, startZ) && !PhysicsUtils.IsBlockSolid(locationSpec[startX, startY, startZ])); // && startY > 0 && data[nx, ny - 1, nz] != regionNum)
        }

        public delegate void LoopThroughNeighboringPositionsCallback(long wx, long wy, long wz);

        public void LoopThroughNeighboringPositionsOnlyHorizontal(long wx, long wy, long wz, LoopThroughNeighboringPositionsCallback callback)
        {
            callback(wx - 1, wy, wz);
            callback(wx + 1, wy, wz);
            callback(wx, wy, wz - 1);
            callback(wx, wy, wz + 1);
        }


        /// <summary>
        /// Also adds connections
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="startZ"></param>
        /// <param name="fromPos"></param>
        /// <param name="fromRegion"></param>
        void MakeFallingRegionStartingAtPos(int startX, int startY, int startZ, PathingRegionPos fromPos, PathingRegion fromRegion, bool lookOutsideChunk=false)
        {
            bool intraChunk = this != fromRegion.parentChunk;
            //Debug.Log("adding drop region at " + x + " " + y + " " + z + " with drop " + nx + " " + ny + " " + nz);
            List<PathingRegionPos> positions = new List<PathingRegionPos>();
            ushort regionNum = (ushort)(regions.Count+1);
            PathingRegion region;
            if (!intraChunk)
            {
                region = new PathingRegion(regionNum - 1, positions, PathingRegion.PathingRegionType.Falling, this);
            }
            else
            {
                region = new PathingRegion(regionNum - 1, positions, PathingRegion.PathingRegionType.FallingFromAnotherChunk, this);
            }
            
            regions.Add(region);
            PathingRegionPos curPos = null;
            // this goes through nx, ny, nz first so we don't have to duplicate code
            for (int curY = startY; curY >= 0; curY--)
            {
                if (!PhysicsUtils.IsBlockSolid(locationSpec[startX, curY, startZ]) && data[startX, curY, startZ] == 0)
                {
                    data[startX, curY, startZ, PathingFlags.Visited] = true;
                    data[startX, curY, startZ] = regionNum;
                    curPos = new PathingRegionPos(startX, curY, startZ, curPos, this);
                    positions.Add(curPos);
                }
                else
                {
                    // we passed another region, connect us
                    if (data[startX, curY, startZ] != 0)
                    {
                        int toRegionIndex = data[startX, curY, startZ] - 1;
                        PathingRegion toRegion = regions[toRegionIndex];
                        region.AddRegionWeCanGoTo(new PathingRegionConnection(region, toRegion, curPos, new PathingRegionPos(startX, curY, startZ, null, this)));
                        
                        // if we landed on a non falling region, see if to our side is anything that we can connect to (disconnected stairs)
                        if (curPos != null && CanWeFitAbove(startX, curY, startZ, additionalSpaceNeeded: mobilityCriteria.jumpHeight))
                        {
                            PathingRegionPos prevPos = curPos;
                            if (!toRegion.RegionTypeIsFalling())
                            {
                                List<Tuple<PathingRegionPos, PathingRegion>> connectedRegionsMaybe = new List<Tuple<PathingRegionPos, PathingRegion>>();

                            }
                        }
                        
                    }
                    break;
                }
            }

            fromRegion.AddRegionWeCanGoTo(new PathingRegionConnection(fromRegion, region, fromPos, new PathingRegionPos(startX, startY, startZ, null, this)));


            region.Finish();
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
            ///// IT IS IMPORTANT WE DO THESE TWO IN THIS ORDER SO WE TRICKLE FALLING REGIONS FROM THE CHUNK ABOVE US TO THE ONE BELOW /////
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy + 1, myChunk.cz));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy - 1, myChunk.cz));
            ////// 
           
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy, myChunk.cz - 1));
            neighboringChunks.Add(world.GetChunk(myChunk.cx, myChunk.cy, myChunk.cz + 1));

            foreach (Chunk neighborChunk in neighboringChunks)
            {
                if (neighborChunk != null)
                {
                    PathingChunk neighborNode = neighborChunk.GetPathingChunk(mobilityCriteria);
                    ConnectExitsToNeighbor(neighborNode);
                    neighborNode.ConnectExitsToNeighbor(this);
                }
            }

            // expand to account for sparse stairs
            foreach (PathingRegion region in regions)
            {
                if (region.RegionTypeIsFalling())
                {
                    region.ExpandBottomToAccountForJump();
                }
            }

        }

        public delegate void ProcessNeighboringCells(int myX, int myY, int myZ, int theirX, int theirY, int theirZ);
        public void LoopThroughNeighboringCells(PathingChunk neighbor, ProcessNeighboringCells callback)
        {
            if (neighbor.locationSpec.minX - 1 == locationSpec.maxX)
            {
                int myX = (int)(locationSpec.xWidth - 1);
                //int theirX = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int theirX = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (neighbor.locationSpec.minY - 1 == locationSpec.maxY)
            {
                int myY = (int)(locationSpec.yWidth - 1);
                int theirY = 0;
                for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (neighbor.locationSpec.minZ - 1 == locationSpec.maxZ)
            {
                int myZ = (int)(locationSpec.zWidth - 1);
                //int theirZ = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int theirZ = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minX - 1 == neighbor.locationSpec.maxX)
            {
                int theirX = (int)(neighbor.locationSpec.xWidth - 1);
                //int myX = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int myX = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minY - 1 == neighbor.locationSpec.maxY)
            {

                int theirY = (int)(neighbor.locationSpec.yWidth - 1);
                int myY = 0; // todo: double check this
                for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minZ - 1 == neighbor.locationSpec.maxZ)
            {
                int theirZ = (int)(neighbor.locationSpec.zWidth - 1);
                //int myZ = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int myZ = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
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

        public static PathingRegion GetRegion(World world, MobilityCriteria mobilityCriteria, long wx, long wy, long wz, out PathingRegionPos posInRegion)
        {
            posInRegion = null;
            Chunk worldChunk = World.mainWorld.GetChunkAtPos(wx, wy, wz);
            if (worldChunk != null)
            {
                PathingChunk worldPathingChunk = worldChunk.GetPathingChunk(mobilityCriteria);
                return worldPathingChunk.GetRegionNotUsingLocalCoordinates(wx, wy, wz, out posInRegion);
            }
            else
            {
                return null;
            }
        }

        // if outside bounds, this will look around at other chunks to get the region
        public PathingRegion GetRegionNotUsingLocalCoordinates(long wx, long wy, long wz, out PathingRegionPos posInRegion)
        {
            posInRegion = null;
            if (!locationSpec.ContainsPosition(wx, wy, wz))
            {
                return PathingChunk.GetRegion(world, mobilityCriteria, wx, wy, wz, out posInRegion);
            }
            int localX = (int)(wx - locationSpec.minX);
            int localY = (int)(wy - locationSpec.minY);
            int localZ = (int)(wz - locationSpec.minZ);

            PathingRegion res = GetRegion(localX, localY, localZ);
            posInRegion = new PathingRegionPos(localX, localY, localZ, null, this);
            return res;
        }

        PathingRegion GetRegion(int localX, int localY, int localZ)
        {
            ushort regionVal = data[localX, localY, localZ];
            int regionIndex = regionVal - 1;
            if (regionVal == 0)
            {
                return null;
            }
            else
            {
                return regions[regionIndex];
            }
        }

        HashSet<PathingChunk> neighborsConnectedTo = new HashSet<PathingChunk>();

        public void ConnectExitsToNeighbor(PathingChunk neighbor)
        {
            if (neighborsConnectedTo.Contains(neighbor))
            {
                return;
            }

            neighborsConnectedTo.Add(neighbor);

            LoopThroughNeighboringCells(neighbor, (myX, myY, myZ, theirX, theirY, theirZ) =>
            {
                PathingRegion myRegion = GetRegion(myX, myY, myZ);
                PathingRegion theirRegion = neighbor.GetRegion(theirX, theirY, theirZ);

                PathingRegionPos myPos = new PathingRegionPos(myX, myY, myZ, null, this);
                PathingRegionPos theirPos = new PathingRegionPos(theirX, theirY, theirZ, null, neighbor);
                // if they are directly above us, consider fall regions falling into a spot where we don't currently have a region
                if (neighbor.chunk.cy == chunk.cy+1)
                {
                    // they are falling into us, we need to make a new region
                    if (theirRegion != null && myRegion == null && theirRegion.RegionTypeIsFalling())
                    {
                        MakeFallingRegionStartingAtPos(myX, myY, myZ, theirPos, theirRegion);
                    }
                    // they are falling into us but we already have a region, connect us to them
                    else if (theirRegion != null && myRegion != null && theirRegion.RegionTypeIsFalling())
                    {
                        theirRegion.AddRegionWeCanGoTo(new PathingRegionConnection(theirRegion, myRegion, theirPos, myPos));
                    }
                    // they are just a block up, this is a stairs thing probably
                    else if(theirRegion != null && myRegion == null && theirRegion.regionType == PathingRegion.PathingRegionType.Regular)
                    {
                        LoopThroughNonVerticalNeighborsInChunk(myX, myY, myZ, (nx, ny, nz) =>
                        {
                            PathingRegion neighborRegion = GetRegion(nx, ny, nz);
                            if (neighborRegion != null && neighborRegion.regionType == PathingRegion.PathingRegionType.Regular)
                            {
                                myRegion = neighborRegion;
                            }
                        });

                        if (myRegion != null)
                        {
                            data[myX, myY, myZ] = (ushort)(myRegion.index + 1);
                            myRegion.positions.Add(new PathingRegionPos(myX, myY, myZ, null, myRegion.parentChunk));
                            theirRegion.AddRegionWeCanGoTo(new PathingRegionConnection(theirRegion, myRegion, theirPos, myPos));
                            myRegion.AddRegionWeCanGoTo(new PathingRegionConnection(myRegion, theirRegion, myPos, theirPos));
                        }
                    }
                }
                // if they are to the side of us, they might make a falling region into us
                else if(neighbor.chunk.cy == chunk.cy)
                {
                    if (theirRegion != null && myRegion == null && theirRegion.regionType == PathingRegion.PathingRegionType.Regular && ValidPositionForAFallingRegion(myX, myY, myZ))
                    {
                        MakeFallingRegionStartingAtPos(myX, myY, myZ, theirPos, theirRegion, lookOutsideChunk: true);
                    }
                    else if(theirRegion != null && myRegion != null && theirRegion.regionType == PathingRegion.PathingRegionType.Regular && myRegion.regionType == PathingRegion.PathingRegionType.Regular)
                    {
                        theirRegion.AddRegionWeCanGoTo(new PathingRegionConnection(theirRegion, myRegion, theirPos, myPos));
                        myRegion.AddRegionWeCanGoTo(new PathingRegionConnection(myRegion, theirRegion, myPos, theirPos));
                    }
                    // sparse staircases maybe, look down one to see if we can traverse
                    // alternatively, they want to make a falling region but there is already one here, in that case, just connect them to it
                    else if(theirRegion != null && myRegion != null && theirRegion.regionType == PathingRegion.PathingRegionType.Regular && myRegion.RegionTypeIsFalling() && ValidPositionForAFallingRegion(myX, myY, myZ, allowExistingRegion: true))
                    {
                        theirRegion.AddRegionWeCanGoTo(new PathingRegionConnection(theirRegion, myRegion, theirPos, myPos));
                    }
                    // will be covered by them in the above else if
                    else if (theirRegion != null && myRegion != null && myRegion.regionType == PathingRegion.PathingRegionType.Regular && theirRegion.RegionTypeIsFalling())
                    {

                    }
                }
                // they are below us, that logic will be handled by them (see the  if (neighbor.chunk.cy == chunk.cy+1)  piece of logic above)
                

            });
        }
    }


    public class BlocksPathing
    {
        public static PathingRegionPos Pathfind(World world, LVector3 startPos, LVector3 endPos, int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight, out bool success, bool verbose=false)
        {

            MobilityCriteria mobilityCriteria = new MobilityCriteria(neededSizeForward, neededSizeSide, neededSizeUp, jumpHeight);
            PathingRegionPos res = PathingChunk.Pathfind(world, startPos, endPos, mobilityCriteria, out success, verbose: verbose);




            if (res == null)
            {
                if (verbose)
                {
                    Debug.LogWarning("pathing failed");
                }
            }
            else
            {
                if (verbose)
                {
                    Debug.Log("got pathing result with path of length: " + res.pathLen + " and success=" + success);
                }


                if (verbose)
                {
                    /*


                    int pos = 0;
                    res.LoopThroughPositions((wx, wy, wz, segment, pathingNode) =>
                    {
                            world.MakeLoggingNode("resPos", "Path pos " + pos, Color.green, wx, wy, wz).transform.position -= new Vector3(0, 0.5f, 0);
                        pos += 1;
                    });
                    */
                }
            }



            return res;
        }
    }

    public class PathingSpreadNode
    {
        public PathingNodeExit curExit;
        public PathingSpreadNode prevNode;
        public SpreadNode pathToPrevNode;


        public enum PathingSpreadNodeType
        {
            ConnectingExitsInSameChunk,
            ConnectingExitsInTwoDifferentChunks,
            ConnectingStartToExit,
            ConnectingEndToExit,
            ConnectingStartToEndDirectlyInSameChunk,
            Unknown
        }


        public PathingSpreadNodeType pathingNodeSpreadType = PathingSpreadNodeType.Unknown;

        public int distFromStart;


        public delegate void LoopThroughPositionsCallback(long wx, long wy, long wz, SpreadNode segment, PathingSpreadNode pathingNode);

        public void LoopThroughPositions(LoopThroughPositionsCallback callback)
        {
            PathingSpreadNode curNode = this;

            int numTimes = 0;
            do
            {
                SpreadNode curPos = curNode.pathToPrevNode;
                do
                {
                    callback(curPos.wx, curPos.wy, curPos.wz, curPos, curNode);
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


        /*
        bool cachedValuesOnLand { get { return GetCacheBit(0); } set { SetCacheBit(0, value); } }
        bool onLandJumping { get { return GetCacheBit(1); } set { SetCacheBit(1, value); } }

        byte onLandHeightJumped { get { return GetCacheValue(8+8); } set { SetCacheValue(8 + 8, value); } }
        bool onLandFalling { get { return GetCacheBit(2); } set { SetCacheBit(2, value); } }
        byte onLandHeightFallen { get { return GetCacheValue(8 + 16); } set { SetCacheValue(8 + 16, value); } }
        bool onLandIsValid { get { return GetCacheBit(3); } set { SetCacheBit(3, value); } }

        bool cachedValuesOffLand { get { return GetCacheBit(4); } set { SetCacheBit(4, value); } }
        bool offLandJumping { get { return GetCacheBit(5); } set { SetCacheBit(5, value); } }
        byte offLandHeightJumped { get { return GetCacheValue(8 + 24); } set { SetCacheValue(8 + 24, value); } }
        bool offLandFalling { get { return GetCacheBit(6); } set { SetCacheBit(6, value); } }
        byte offLandHeightFallen { get { return GetCacheValue(8 + 32); } set { SetCacheValue(8 + 32, value); } }
        bool offLandIsValid { get { return GetCacheBit(7); } set { SetCacheBit(7, value); } }

        bool onLand { get { return GetCacheBit(8); } set { SetCacheBit(8, value); } }
        */

        bool cachedValuesOnLand = false;
        bool onLandJumping;

        byte onLandHeightJumped;
        bool onLandFalling;
        byte onLandHeightFallen;
        bool onLandIsValid;

        bool cachedValuesOffLand = false;
        bool offLandJumping;
        byte offLandHeightJumped;
        bool offLandFalling;
        byte offLandHeightFallen;
        bool offLandIsValid;

        public bool cachedOnLand = false;
        public bool OnLand
        {
            get
            {
                if (cachedOnLand)
                {
                    return onLand_;
                }
                else
                {
                    parentNode.MeetsFitCriteria(this, prev, allowFalling: false);
                    return onLand_;
                }
            }
            set
            {
                onLand_ = value;
                cachedOnLand = true;
            }
        }

        bool onLand_;

        ulong cachedValues;


        public void SetCacheValue(int pos, byte value)
        {
            ulong clearedField = cachedValues & (~((ulong)0xFF << pos));
            cachedValues = clearedField | ((ulong)(((ulong)value) << pos));
        }

        public byte GetCacheValue(int pos)
        {
            return (byte)((cachedValues >> pos) & 0xFF);
        }

        public void SetCacheBit(int pos, bool value)
        {
            if (value)
            {
                cachedValues = cachedValues | ((ulong)(1) << pos);
            }
            else
            {
                cachedValues = cachedValues & (~(((ulong)(1) << pos)));
            }
        }

        public bool GetCacheBit(int pos)
        {
            return (((ulong)(1) << pos) & cachedValues) != 0;
        }


        LoggingNode loggingNodea;
        LoggingNode loggingNodeb;

        /// <summary>
        /// Gets some info about the movement. Returns true if the movement is valid, false if not
        /// Falling is always allowed, allowFalling is only used for recursive calls if prev hasn't had MeetsFitCriteria called on it yet (to determine if it is touching land)
        /// </summary>
        /// <param name="x">local x position in pathing chunk</param>
        /// <param name="y">local y position in pathing chunk</param>
        /// <param name="z">local z position in pathing chunk</param>
        /// <param name="prev">position we were previously in, null is okay to pass in here if unknown.
        /// If null is passed in and we are not on land, jumping and falling are false and all of the heights are 0.
        /// Otherwise, jumping and falling will be set to true and all of the heights will be set to 1.</param>
        /// <param name="jumping">Whether or not we are jumping</param>
        /// <param name="jumpHeight">How high we are jumping (if jumping = true), 0 otherwise</param>
        /// <param name="falling">Whether or not we are falling</param>
        /// <param name="fallHeight">How far we have fallen (if falling = true, or falling = false and we have just landed), 0 otherwise</param>
        /// <returns>true if the movement can be done, false if not</returns>
        public bool GetInfoAboutJumpingOrFalling(bool onLand, SpreadNode prev, out bool jumping, out int heightJumped, out bool falling, out int heightFallen, bool allowFalling)
        {
            SpreadNode node = this;
            //this.onLand = node.parentNode.OnLand(node.localX, node.localY, node.localZ);
            //onLand = this.onLand;
            this.OnLand = onLand;
            if (onLand)
            {

                if (!cachedValuesOnLand)
                {
                    bool onLandJumpingTmp, onLandFallingTmp;
                    int onLandHeightFallenTmp, onLandHeightJumpedTmp;
                    onLandIsValid = GetInfoAboutJumpingOrFallingHelper(onLand, prev, out onLandJumpingTmp, out onLandHeightJumpedTmp, out onLandFallingTmp, out onLandHeightFallenTmp, allowFalling);
                    cachedValuesOnLand = true;
                    onLandJumping = onLandJumpingTmp;
                    onLandFalling = onLandFallingTmp;
                    if (onLandHeightFallenTmp > 255) { onLandHeightFallenTmp = 255; }
                    onLandHeightFallen = (byte)onLandHeightFallenTmp;
                    if (onLandHeightJumpedTmp > 255) { onLandHeightJumpedTmp = 255; }
                    onLandHeightJumped = (byte)onLandHeightJumpedTmp;
                }
                jumping = onLandJumping;
                heightJumped = onLandHeightJumped;
                falling = onLandFalling;
                heightFallen = onLandHeightFallen;
                if (onLandIsValid && loggingNodea == null)
                {
                    //loggingNodea = World.mainWorld.MakeLoggingNode(node + "", "prev " + prev + " onLand " + onLand + " jumping " + jumping + " heightJumped " + heightJumped + " falling " + falling + " heightFallen " + heightFallen, Color.cyan, node.wx, node.wy, node.wz).GetComponent<LoggingNode>();
                    //loggingNodea.transform.localScale *= 0.5f;
                    //loggingNodea.transform.position += new Vector3(0.5f, 0.5f, 0.5f);
                }
                return onLandIsValid;
            }
            else
            {
                if (!cachedValuesOffLand)
                {
                    bool offLandJumpingTmp, offLandFallingTmp;
                    int offLandHeightFallenTmp, offLandHeightJumpedTmp;
                    offLandIsValid = GetInfoAboutJumpingOrFallingHelper(onLand, prev, out offLandJumpingTmp, out offLandHeightJumpedTmp, out offLandFallingTmp, out offLandHeightFallenTmp, allowFalling);
                    cachedValuesOffLand = true;
                    offLandJumping = offLandJumpingTmp;
                    offLandFalling = offLandFallingTmp;
                    // cap at 255 because if you fall more than that then whatever that is sprucy enough
                    if (offLandHeightFallenTmp > 255) { offLandHeightFallenTmp = 255; }
                    offLandHeightFallen = (byte)offLandHeightFallenTmp;
                    if (offLandHeightJumpedTmp > 255) { offLandHeightJumpedTmp = 255; }
                    offLandHeightJumped = (byte)offLandHeightJumpedTmp;
                }
                jumping = offLandJumping;
                heightJumped = offLandHeightJumped;
                falling = offLandFalling;
                heightFallen = offLandHeightFallen;

                if (offLandIsValid && loggingNodeb == null)
                {
                    //loggingNodeb = World.mainWorld.MakeLoggingNode(node + "", "prev " + prev + "onLand " + onLand + " jumping " + jumping + " heightJumped " + heightJumped + " falling " + falling + " heightFallen " + heightFallen, Color.cyan, node.wx, node.wy, node.wz).GetComponent<LoggingNode>();
                    //loggingNodeb.transform.localScale *= 0.5f;
                    //loggingNodeb.transform.position -= new Vector3(0.5f, 0.5f, 0.5f);
                }

                return offLandIsValid;
            }
        }


        public static bool VERBOSE_PATHING
        {
            get
            {
                return World.mainWorld.blocksWorld.verbosePathing;
            }
            set
            {

            }
        }

        // The max depth thing is just to prevent this from recursing always as deep as possible (since that isn't actually needed)
        bool GetInfoAboutJumpingOrFallingHelper(bool onLand,  SpreadNode prev, out bool jumping, out int heightJumped, out bool falling, out int heightFallen, bool allowFalling)
        {
            SpreadNode node = this;
            heightJumped = 0;
            heightFallen = 0;
            jumping = false;
            falling = false;
            long wx = node.wx;
            long wy = node.wy;
            long wz = node.wz;
            if (prev == null)
            {
                // not much info to go off of, assume jumping and falling if not on land, otherwise assume neither
                jumping = !onLand;
                falling = !onLand;
                if (jumping)
                {
                    heightJumped = 1;
                }
                if (falling)
                {
                    heightFallen = 1;
                }

                return true;
            }
            else
            {
                bool prevOnLand = prev.OnLand;

                // we were falling or jumping
                if (!prevOnLand)
                {
                    bool prevJumping, prevFalling;
                    int prevHeightJumped, prevHeightFallen;

                    // if prev is not valid, we aren't valid, this can happen when we are doing reverse
                    if (!prev.GetInfoAboutJumpingOrFalling(prevOnLand, prev.prev, out prevJumping, out prevHeightJumped, out prevFalling, out prevHeightFallen, allowFalling))
                    {
                        //Debug.LogWarning("Warning: In OnLandOrJumpingOrFalling got invalid prev of " + prev + ", we should not have branched from it why are we here (local here position of " + x + " " + y + " " + z + ")");
                        return false;
                    }

                    // on same y column
                    if (wx == prev.wx && wz == prev.wz)
                    {
                        // going up
                        if (wy > prev.wy)
                        {
                            // we are jumping, this is ok
                            if (prevJumping)
                            {
                                jumping = true;
                                heightJumped = prevHeightJumped + (int)(wy - prev.wy);
                                return true;
                            }
                            // we aren't jumping, we can't go up, this is invalid
                            else
                            {
                                return false;
                            }
                        }
                        // same position, what are you doing?
                        else if (wy == prev.wy)
                        {
                            Debug.LogWarning("Warning: Same position as prev, why? prev and us is " + prev);
                            return false;
                        }
                        // going down
                        else
                        {
                            // we are falling, that is fine
                            if (prevFalling)
                            {
                                // we hit ground, we aren't falling anymore, still we should record the fall height if needed elsewhere
                                if (onLand)
                                {
                                    falling = false;
                                    heightFallen = prevHeightFallen + (int)(prev.wy - wy);
                                }
                                // we are still falling
                                else
                                {
                                    falling = true;
                                    heightFallen = prevHeightFallen + (int)(prev.wy - wy);
                                }
                                return true;
                            }
                            // we aren't falling so we shouldn't be going down, this is invalid
                            else
                            {
                                return false;
                            }
                        }
                    }
                    // on different y column
                    else
                    {
                        // we just jumped up onto a block (up a staircase type thing)
                        if (onLand && prevJumping)
                        {
                            // that is too high, this is invalid
                            if (prevHeightJumped > this.parentNode.jumpHeight)
                            {
                                return false;
                            }
                            // we are on land now so this is valid
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                        /*
                        // jumping over a hole
                        else if(prevJumping && prev.prev != null && prev.prev.OnLand)
                        {
                            jumping = true;
                            heightJumped = prevHeightJumped;
                            return true;
                        }
                        // done jumping, now we are falling since we made our jump
                        else if(prevJumping && prev.prev != null && !prev.prev.OnLand && prev.prev.prev != null && prev.prev.prev.OnLand && 
                            ((prev.wx == wx && prev.prev.wx == wx) ||
                            (prev.wz == wz && prev.prev.wz == wz)))
                        {
                            falling = true;
                            heightFallen = 0;
                            return true;
                        }
                        // don't allow moving around in the air for pathing, except up and down and over one if we are going up a staircase type thing and jumping over a hole
                        else
                        {
                            return false;
                        }
                        */
                    }
                }
                // we came from being on land
                else
                {
                    // we are still on land, this is good
                    if (onLand)
                    {
                        return true;
                    }
                    // we are no longer on land, we are either jumping or falling
                    else
                    {
                        // same y column, we must be jumping
                        if (prev.wx == wx && prev.wz == wz)
                        {
                            // we are jumping
                            if (prev.wy < wy)
                            {
                                jumping = true;
                                heightJumped = (int)(wy - prev.wy);
                                return true;
                            }
                            // same block? why u do this
                            else if (prev.wy == wy)
                            {
                                Debug.LogWarning("Warning: Same position as prev, why? prev and us is " + prev);
                                return false;
                            }
                            // we have just fallen through a block somehow? This is invalid
                            else
                            {
                                return false;
                            }
                        }
                        // we have moved horizontally, we must be falling
                        // TODO: allow jumping over gaps
                        else
                        {
                            // we have walked over a hole, begin falling
                            if (prev.wy == wy)
                            {
                                falling = true;
                                heightFallen = 0;
                                return true;
                            }
                            // we are not allowed to do jump over gaps yet
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
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

        public SpreadNode GetInvertedPath()
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
            return Math.Abs(worldX - wx) + Math.Abs(worldY - wy) + Math.Abs(worldZ - wz);
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
        public int index;
        public long visited = -1;
        public long connectedToExit = -1;
        public int distToExitIfConected = 0;

        public PathingNodeExit(int index, PathingNode parentNode, Tuple<int, int, int>[] localPositions)
        {
            this.index = index;
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


                long curDist = Math.Abs(x - actualX) + Math.Abs(y - actualY) + Math.Abs(z - actualZ);
                minDist = Math.Min(curDist, minDist);
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



        public bool MeetsFitCriteria(SpreadNode node, SpreadNode prev = null, bool allowFalling = true)
        {
            bool onLand;
            return MeetsFitCriteria(node, out onLand, prev, allowFalling);
        }

        /*
        public bool OnLand(int x, int y, int z)
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
        */


        bool TestJumpingOrFallingConditions(bool onLand, SpreadNode node, SpreadNode prev, bool allowFalling)
        {
            bool jumping, falling;
            int heightJumped, heightFallen;

            if (node.GetInfoAboutJumpingOrFalling(onLand, prev, out jumping, out heightJumped, out falling, out heightFallen, allowFalling))
            {
                if (heightJumped > this.jumpHeight)
                {
                    return false;
                }
                else
                {
                    if (falling)
                    {
                        if (allowFalling)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

                /*


                    // we are jumping, we can go up jumpHeight blocks than over 1
                    if (y > prev.localY)
                    {

                    }
                    // we are falling
                    else
                    {

                    }
                    if (x == prev.localX && z == prev.localZ)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                // we just came from something on land
                else
                {
                    // we are still on land, we are fine
                    if (onLand)
                    {
                        return true;
                    }
                    // we are no longer on land
                    else
                    {
                        
                    }
                }
            }


            if (!onLand && prev != null)
            {
                // check if we were falling so we can't float around
                if (!prevOnLand)
                {
                    if (allowFalling && x == prev.localX && z == prev.localZ && y == prev.localY-1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // we are just falling or about to fall, that is fine
                if (prev.localY == y + 1 || (prev.localY == y && OnLand(prev.localX, prev.localY, prev.localZ)))
                {
                    if (allowFalling)
                    {
                        return true;
                    }
                }
                // we aren't falling, are we jumping?
                else if (prev.localX == x && prev.localZ == z)
                {
                    int effectiveJumpDist = jumpHeight;
                    SpreadNode effectivePrev = prev;
                    // we are allowed to travel jumpHeight steps in the air without touching ground
                    while (effectiveJumpDist > 0 && effectivePrev != null)
                    {
                        bool eprevOnLand = OnLand(effectivePrev.localX, effectivePrev.localY, effectivePrev.localZ);
                        if (eprevOnLand)
                        {
                            return true;
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
        }

            */


        bool MeetsFitCriteria(SpreadNode node, out bool onLand, SpreadNode prev = null, bool allowFalling=true)
        {
            int x = node.localX;
            int y = node.localY;
            int z = node.localZ;

            // try 2 rotations of us
            bool failed = false;
            onLand = false;


            if (neededSizeSide == 1 && neededSizeForward == 1)
            {
                for (int curY = y;  curY < y + neededSizeUp; curY++)
                {
                    if (curY >= locationSpec.yWidth)
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(x, curY, z)))
                        {
                            failed = true;
                            break;
                        }
                    }
                    else
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec[x, curY, z]))
                        {
                            failed = true;
                            break;
                        }
                    }
                }

                if (y > 0)
                {
                    if (PhysicsUtils.IsBlockSolid(this.locationSpec[x, y - 1, z]))
                    {
                        onLand = true;
                    }
                }
                else
                {
                    if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(x, y - 1, z)))
                    {
                        onLand = true;
                    }
                }

                node.OnLand = onLand;

                if (failed)
                {
                    return false;
                }
                else
                {
                    return TestJumpingOrFallingConditions(onLand, node, prev, allowFalling);
                }

                
            }




            for (int curX = x; curX >= 0 && curX > x - neededSizeSide; curX--)
            {
                for (int curZ = z; curZ >= 0 && curZ > z - neededSizeForward; curZ--)
                {
                    bool failedFirst = false;
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failedFirst = true;
                            break;
                        }
                    }
                    bool failedSecond = false;
                    for (int curY = y; curY >= 0 && curY > y - neededSizeUp; curY--)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failedSecond = true;
                            break;
                        }
                    }

                    if (failedFirst && failedSecond)
                    {
                        failed = true;
                        break;
                    }

                    if (y > 0)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, y-1, curZ]))
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(curX, y-1, curZ)))
                        {
                            onLand = true;
                        }
                    }
                    if (failed)  break;
                }
                if (failed) break;
            }

            bool onLandOrJumpingOrFalling = TestJumpingOrFallingConditions(onLand, node, prev, allowFalling);

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
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failed = true;
                            break;
                        }
                    }

                    if (y > 0)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, y - 1, curZ]))
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ)))
                        {
                            onLand = true;
                        }
                    }
                    if (failed) break;
                }
                if (failed) break;
            }
            
            onLandOrJumpingOrFalling = TestJumpingOrFallingConditions(onLand, node, prev, allowFalling);

            // good
            if (!failed && onLandOrJumpingOrFalling)
            {
                return true;
            }
























            // try 2 rotations of us
            failed = false;
            onLand = false;
            for (int curX = x; curX < locationSpec.xWidth && curX < x + neededSizeSide; curX++)
            {
                for (int curZ = z; curZ < locationSpec.zWidth && curZ < z + neededSizeForward; curZ++)
                {
                    bool failedFirst = false;
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failedFirst = true;
                            break;
                        }
                    }
                    bool failedSecond = false;
                    for (int curY = y; curY >= 0 && curY > y - neededSizeUp; curY--)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failedSecond = true;
                            break;
                        }
                    }

                    if (failedFirst && failedSecond)
                    {
                        failed = true;
                        break;
                    }

                    if (y > 0)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, y - 1, curZ]))
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ)))
                        {
                            onLand = true;
                        }
                    }
                    if (failed) break;
                }
                if (failed) break;
            }

            onLandOrJumpingOrFalling = TestJumpingOrFallingConditions(onLand, node, prev, allowFalling);

            // good
            if (!failed && onLandOrJumpingOrFalling)
            {
                return true;
            }
            // not good, try rotated 90 degrees

            failed = false;
            onLand = false;
            for (int curX = x; curX < locationSpec.xWidth && curX < x + neededSizeForward; curX++)
            {
                for (int curZ = z; curZ < locationSpec.zWidth && curZ < z + neededSizeSide; curZ++)
                {
                    for (int curY = y; curY < locationSpec.yWidth && curY < y + neededSizeUp; curY++)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, curY, curZ]))
                        {
                            failed = true;
                            break;
                        }
                    }

                    if (y > 0)
                    {
                        if (PhysicsUtils.IsBlockSolid(this[curX, y - 1, curZ]))
                        {
                            onLand = true;
                        }
                    }
                    else
                    {
                        if (PhysicsUtils.IsBlockSolid(this.locationSpec.GetBlockOutsideRange(curX, y - 1, curZ)))
                        {
                            onLand = true;
                        }
                    }
                    if (failed) break;
                }
                if (failed) break;
            }

            onLandOrJumpingOrFalling = TestJumpingOrFallingConditions(onLand, node, prev, allowFalling);

            // good
            if (!failed && onLandOrJumpingOrFalling)
            {
                return true;
            }


            return !failed && onLandOrJumpingOrFalling;
        }

        public void Refresh(bool verbose=false)
        {
            editNum = locationSpec.chunk.chunkData.editNum;
            cachedConnectedExits = null;
            cachedExits = null;
            cachedPathsBetween = null;

            bool wallsWereExpanded = wallsExpanded;
            wallsExpanded = false;

            // reset exit values to zero
            for (int x = 0; x < locationSpec.xWidth; x++)
            {
                for (int y = 0; y < locationSpec.yWidth; y++)
                {
                    for (int z = 0; z < locationSpec.zWidth; z++)
                    {
                        data[x, y, z] = 0;
                    }
                }
            }

            GetExits(verbose);
            ConnectExits();


            if (neighborsConnectedTo.Count > 0)
            {
                foreach (PathingNode node in neighborsConnectedTo)
                {
                    node.DisconnectExitsFromNeighbor(this);
                }
                wallsExpanded = false;
                neighborsConnectedTo.Clear();

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
                    if (fithsShiftedColumn)
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
                for (int y = (int)(locationSpec.yWidth-1); y >= 0; y--)
                {
                    callback(x, y, 0);
                    /*
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
                    */
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
                        //callback(x, neededSizeUp - 1, z);
                        callback(x, 0, z);
                    }
                }
            }


            for (int y = (int)(locationSpec.yWidth - 1); y >= 0; y--)
            {
                for (int z = 0; z < locationSpec.zWidth; z++)
                {
                    callback(0, y, z);
                    /*
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
                    */
                }
            }


            if (locationSpec.zWidth > 1)
            {
                int maxZ = (int)(locationSpec.zWidth)-1;
                for (int x = 0; x < locationSpec.xWidth; x++)
                {
                    for (int y = (int)(locationSpec.yWidth - 1); y >= 0; y--)
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
                    for (int y = (int)(locationSpec.yWidth - 1); y >= 0; y--)
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
            Queue<SpreadNode> needToBeProcessed = new Queue<SpreadNode>();
            foreach (Tuple<int, int, int> startPoint in startPoints)
            {
                SpreadNode startNode = new SpreadNode(startPoint.a, startPoint.b, startPoint.c, null, this);
                // run function on starting positions, but don't actually care about output
                meetsSpreadCriteria(startNode);
                needToBeProcessed.Enqueue(startNode);
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



        List<Tuple<PathingNodeExit, SpreadNode, int>> FindConnectedExits(long startX, long startY, long startZ, bool allowFalling, bool reversed=false, bool verbose=false, bool allowAlmostThere=false)
        {
            // reversed, we need to go from the exits and look for the start pos
            if (reversed)
            {
                int localStartX = (int)(startX - locationSpec.minX);
                int localStartY = (int)(startY - locationSpec.minY);
                int localStartZ = (int)(startZ - locationSpec.minZ);
                List<Tuple<PathingNodeExit, SpreadNode, int>> res = new List<Tuple<PathingNodeExit, SpreadNode, int>>();
                List<PathingNodeExit> exits = GetExits();


                for (int i = 0; i < exits.Count; i++)
                {
                    PathingNodeExit curExit = exits[i];
                    SpreadNode closest = null;
                    int closestDist = int.MaxValue;
                    ClearVisited();

                    Priority_Queue.SimplePriorityQueue<SpreadNode, int> pQueue = new Priority_Queue.SimplePriorityQueue<SpreadNode, int>();

                    bool foundConnectionForThisExit = false;
                    for (int j = 0; j < curExit.localPositions.Length; j++)
                    {
                        int curLocalX = curExit.localPositions[j].a;
                        int curLocalY = curExit.localPositions[j].b;
                        int curLocalZ = curExit.localPositions[j].c;
                        int estimatedDistFromExit =
                            System.Math.Abs(localStartX - curLocalX) +
                            System.Math.Abs(localStartY - curLocalY) +
                            System.Math.Abs(localStartZ - curLocalZ);


                        // mark as visited (so we don't spread there again) and add to pqueue
                        data[curLocalX, curLocalY, curLocalZ, PathingFlags.Visited] = true;
                        // use A*, so cost = dist from start + estimated dist from end
                        // in this case, dist from start = 0 since our path just started
                        SpreadNode n = new SpreadNode(curLocalX, curLocalY, curLocalZ, null, this);

                        if (estimatedDistFromExit < closestDist)
                        {
                            closestDist = estimatedDistFromExit;
                            closest = n;
                        }

                        pQueue.Enqueue(n, estimatedDistFromExit);
                        if(verbose) Debug.Log("adding new spooker from initial set " + n + " with  estimatedDistFromExit " + estimatedDistFromExit + " and goal pos " + startX + " " + startY + " " + startZ);

                        if (verbose) world.MakeLoggingNode("resPos", "(0)+" +  estimatedDistFromExit + "=" + estimatedDistFromExit, Color.blue, n.wx, n.wy, n.wz);
                        // see if it is the pos we are looking for
                        if (n.localX == localStartX && n.localY == localStartY && n.localZ == localStartZ)
                        {
                            if (verbose) Debug.Log("found connection to pos for exit " + i + " from initial set of positions");
                            res.Add(new Tuple<PathingNodeExit, SpreadNode, int>(curExit, n, 0));
                            foundConnectionForThisExit = true;
                            break;
                        }


                    }
                    if (verbose) Debug.Log("pqueue initially is of size " + pQueue.Count);

                    int iff = 0;
                    while (pQueue.Count > 0 && !foundConnectionForThisExit)
                    {
                        SpreadNode n = pQueue.Dequeue();

                        if (verbose) Debug.Log("got node in pqueue " + n);
                        LoopThroughNeighbors(n.localX, n.localY, n.localZ, (nx, ny, nz) =>
                        {
                            // only spread if we haven't been there yet
                            if(!data[nx, ny, nz, PathingFlags.Visited])
                            {
                                SpreadNode neighborNode = new SpreadNode(nx, ny, nz, n, this);
                                //Debug.Log("trying neighbor in pqueue " + neighborNode);
                                bool meetsCriteria = MeetsFitCriteria(neighborNode, neighborNode.prev, allowFalling: allowFalling);

                                if (meetsCriteria)
                                {
                                    int estimatedDistFromExit =
                                        System.Math.Abs(localStartX - nx) +
                                        System.Math.Abs(localStartY - ny) +
                                        System.Math.Abs(localStartZ - nz);
                                    // use A*, so cost = dist from start + estimated dist from end
                                    int distFromStart = neighborNode.pathLen;
                                    int totalCost = estimatedDistFromExit + distFromStart;


                                    // mark as visited (so we don't spread there agian) and add to pqueue
                                    data[nx, ny, nz, PathingFlags.Visited] = true;
                                    pQueue.Enqueue(neighborNode, totalCost);

                                    if (estimatedDistFromExit < closestDist)
                                    {
                                        closestDist = estimatedDistFromExit;
                                        closest = n;
                                    }
                                    if (verbose) world.MakeLoggingNode("resPos", distFromStart + "+" + estimatedDistFromExit + "=" + totalCost + " :: " + iff + " " + neighborNode + " " + neighborNode.OnLand + " : " + neighborNode.prev + " " + n.OnLand, Color.blue, neighborNode.wx, neighborNode.wy, neighborNode.wz);
                                    if (verbose) iff++;
                                    //if (verbose) world.MakeLoggingNode("resPos", , Color.blue, neighborNode.wx, neighborNode.wy, neighborNode.wz);
                                    if (verbose) Debug.Log("adding new spooker " + neighborNode + " with prev " + n + " and dist from start " + distFromStart + " and totalCost " + totalCost + " and goal pos " + startX + " " + startY + " " + startZ);
                                    // see if it is the pos we are looking for
                                    if (neighborNode.localX == localStartX && neighborNode.localY == localStartY && neighborNode.localZ == localStartZ)
                                    {
                                        res.Add(new Tuple<PathingNodeExit, SpreadNode, int>(curExit, neighborNode, 0));
                                        foundConnectionForThisExit = true;
                                        if (verbose) Debug.Log("found connection to pos for exit " + i + " from spread to neighbors");
                                    }
                                }
                                else
                                {
                                    if (neighborNode.localX == localStartX && neighborNode.localY == localStartY && neighborNode.localZ == localStartZ)
                                    {
                                        if (verbose) Debug.Log(" also, " + i + " rip " + iff + " " + neighborNode + " " + neighborNode.OnLand + " : " + n + " " + n.OnLand + " "  + n.cachedOnLand);
                                        if (verbose) Debug.Log("n meets fit criteria: " + MeetsFitCriteria(n, n.prev, allowFalling) + " also i meet fit criteria " + MeetsFitCriteria(neighborNode, neighborNode.prev, allowFalling) + " " + i + " rip " + iff + " " + neighborNode + " " + neighborNode.OnLand + " : " + n + " " + n.OnLand);
                                        if (verbose) world.MakeLoggingNode("resPos", i + " rip " + iff + " " + neighborNode + " " + neighborNode.OnLand + " : " + n + " " + n.OnLand, Color.red, neighborNode.wx, neighborNode.wy, neighborNode.wz);
                                        if (verbose) iff++;
                                        if (verbose) Debug.Log("neighbor of " + n + " failed in pqueue " + neighborNode);
                                    }
                                    else
                                    {
                                        if (verbose) world.MakeLoggingNode("resPos", i + " rip " + iff + " " + neighborNode + " " + neighborNode.OnLand + " : " + n + " " + n.OnLand, Color.red, neighborNode.wx, neighborNode.wy, neighborNode.wz);
                                        if (verbose) iff++;
                                        if (verbose) Debug.Log("neighbor of " + n + " failed in pqueue " + neighborNode);
                                    }
                                }
                            }
                        });
                    }

                    if (!foundConnectionForThisExit && allowAlmostThere)
                    {
                        //Debug.Log("failed to find connection to pos for exit " + i + " returning closest thing found");
                        if (closest != null)
                        {
                            res.Add(new Tuple<PathingNodeExit, SpreadNode, int>(curExit, closest, closestDist));
                        }
                    }
                }

                return res;
            }
            // not reversed, we need to go from the start pos and look for the exits
            else
            {
                ClearVisited();

                int localStartX = (int)(startX - locationSpec.minX);
                int localStartY = (int)(startY - locationSpec.minY);
                int localStartZ = (int)(startZ - locationSpec.minZ);



                List<PathingNodeExit> exits = GetExits();
                List<Tuple<PathingNodeExit, SpreadNode, int>> res = new List<Tuple<PathingNodeExit, SpreadNode, int>>();
                HashSet<int> foundExits = new HashSet<int>();



                DoSpread(localStartX, localStartY, localStartZ, (n) =>
                {
                    if (data[n.localX, n.localY, n.localZ, PathingFlags.Visited])
                    {
                        return false;
                    }


                    ushort exitNum = data[n.localX, n.localY, n.localZ];

                    bool canWeFit;
                    //if (reversed && n.prev != null)
                    //{
                    //    canWeFit = MeetsFitCriteria(n.prev, n, allowFalling: allowFalling);
                    //}
                    //else
                    //{
                    canWeFit = MeetsFitCriteria(n, n.prev, allowFalling: allowFalling);
                    //}
                    // if we found an exit node (non-zero means exit node) and we haven't seen it yet, record the path
                    if (exitNum != 0 && !foundExits.Contains(exitNum) && canWeFit)
                    {
                        foundExits.Add(exitNum);
                        int exitNumI = exitNum - 1;
                        res.Add(new Tuple<PathingNodeExit, SpreadNode, int>(exits[exitNumI], n, 0));

                    }
                    //GameObject spook = world.MakeLoggingNode("resPos", "connected to pos " + startX + " " + startY + " " + startZ, Color.blue, n.wx, n.wy, n.wz);
                    //spook.transform.localScale *= 0.5f;
                    //spook.transform.position += new Vector3(0.5f, 0, 0.5f);

                    if (canWeFit)
                    {
                        data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;
                    }

                    // only trickle if we can actually move through here
                    return canWeFit;
                });

                return res;
            }
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

        public SpreadNode FindPathThroughSingleNode(int localStartX, int localStartY, int localStartZ, int localEndX, int localEndY, int localEndZ, bool allowFalling=false)
        {
            ClearVisited();

            SpreadNode pathToEnd = null;

            data[localStartX, localStartY, localStartZ, PathingFlags.Visited] = true;
            // connect current position in exit to place where we are leaving exit
            DoSpread(localStartX, localStartY, localStartZ, (n) =>
            {
                // if we found the location, we are good, we don't need to spread anymore
                if (pathToEnd != null)
                {
                    return false;
                }
                // only spread through non visited

                if (data[n.localX, n.localY, n.localZ, PathingFlags.Visited])
                {
                    return false;
                }
                if (n.localX == localEndX && n.localY == localEndY && n.localZ == localEndZ)
                {
                    pathToEnd = n;
                    return false;
                }
                else
                {
                    bool canWalkThrough = MeetsFitCriteria(n, n.prev, allowFalling);

                    if (canWalkThrough)
                    {
                        data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;
                    }

                    return canWalkThrough;
                }
            });

            return pathToEnd;
        }


        public SpreadNode FindPathThroughExit(int localStartX, int localStartY, int localStartZ, int localEndX, int localEndY, int localEndZ, ushort exitNum, bool verbose=false)
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

            data[localStartX, localStartY, localStartZ, PathingFlags.Visited] = true;
            // connect current position in exit to place where we are leaving exit
            DoSpread(localStartX, localStartY, localStartZ, (n) =>
            {
                // if we found the location, we are good, we don't need to spread anymore
                if (pathThroughExit != null)
                {
                    return false;
                }
                if (verbose)
                {
                    Debug.Log("spread to node " + n + " with exitVal " + data[n.localX, n.localY, n.localZ] + " (desired exit val is " + exitNum + " )");
                }
                // only spread through exit

                if (data[n.localX, n.localY, n.localZ, PathingFlags.Visited])
                {
                    return false;
                }


                // we still need to test criteria because if exit spreads from the ground on the edge of a chunk it'll fill all the spots above it as well as exit nodes since you can jump to them,
                // but if you only went along those nodes then you would be floating so that's invalid
                // TODO: I'm not sure if this is worth the efficiency loss of having to call MeetsFitCriteria? an alternate option is in that floating case, just go on the position below them instead. But idk
                bool meetsCriteria = MeetsFitCriteria(n, n.prev, allowFalling: true);

                if (meetsCriteria && data[n.localX, n.localY, n.localZ] == exitNum)
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
                Debug.LogWarning("unable to find path through exit, perhaps exit is segmented by a falling section? also, the ids were " + data[localStartX, localStartY, localStartZ] + " " + data[localEndX, localEndY, localEndZ] + " and the positions were " + localStartX+ " " + localStartY + " " + localStartZ + " and " + localEndX + " " + localEndY + " " + localEndZ);
            }
            return pathThroughExit;
        }

        List<GameObject> myLoggingNodes = new List<GameObject>();



        // TODO: fastest path through an exit might not actually be going through that exit (with a U shape, you want to cross across the middle)
        public static long curRunId = 0;
        public static PathingSpreadNode Pathfind(World world, LVector3 startPos, LVector3 endPos, int neededSizeForward, int neededSizeSide, int neededSizeUp, int jumpHeight, out bool success, bool verbose=false)
        {
            int goDownAmount = neededSizeUp-1;
            // go down until we hit the ground
            while (!PhysicsUtils.IsBlockSolid(world[startPos-new LVector3(0,1,0)]) && goDownAmount > 0)
            {
                goDownAmount -= 1;
                startPos = startPos - new LVector3(0, 1, 0);
            }


            goDownAmount = neededSizeUp - 1;
            // go down until we hit the ground
            while (!PhysicsUtils.IsBlockSolid(world[endPos - new LVector3(0, 1, 0)]) && goDownAmount > 0)
            {
                goDownAmount -= 1;
                endPos = endPos - new LVector3(0, 1, 0);
            }



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



            Priority_Queue.SimplePriorityQueue<PathingSpreadNode, long> pQueue = new Priority_Queue.SimplePriorityQueue<PathingSpreadNode, long>();



            long midTime = PhysicsUtils.millis();
            List<Tuple<PathingNodeExit, SpreadNode, int>> connectedExitsToStart = startNode.FindConnectedExits(startPos.x, startPos.y, startPos.z, true);
            // the issue right now is this traverses from the end node out to the exits, but we need to traverse from the exits to the end node to have the code that checks "can you walk there" do it properly
            List<Tuple<PathingNodeExit, SpreadNode, int>> connectedExitsToEnd = endNode.FindConnectedExits(endPos.x, endPos.y, endPos.z, allowFalling:true, reversed:true, verbose: SpreadNode.VERBOSE_PATHING);

            if (connectedExitsToEnd.Count == 0)
            {
                Debug.LogWarning("could not find any way to get to player from the exits of the players chunk, using closest we can find");
                connectedExitsToEnd = endNode.FindConnectedExits(endPos.x, endPos.y, endPos.z, allowFalling: true, reversed: true, allowAlmostThere: true);
            }


            if (verbose)
            {
                world.MakeLoggingNode("resPos", "start", Color.gray, startPos.x, startPos.y, startPos.z);
                world.MakeLoggingNode("resPos", "end", Color.gray, endPos.x, endPos.y, endPos.z);
            }

            foreach (Tuple<PathingNodeExit, SpreadNode, int> connectedExit in connectedExitsToEnd)
            {
                // set the values of the exits connected to the end pos to be equal to curRunId, this lets us easily test if we are connected to end in constant time in the logic below
                connectedExit.a.connectedToExit = curRunId;
                connectedExit.a.distToExitIfConected = connectedExit.c;
            }

            PathingSpreadNode closest = null;
            long closestEstimatedDistToEnd = long.MaxValue;

            foreach (Tuple<PathingNodeExit, SpreadNode, int> connectedExit in connectedExitsToStart)
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
            int numSteps = 0;
            bool foundCompletePath = false;

            bool foundCloseExit = false;

            while (pQueue.Count > 0)
            {
                if (numSteps > 20 && PhysicsUtils.millis() - loopTime > 1000)
                {
                    Debug.Log("bailing, failed, with steps " + numSteps + " and millis " + (PhysicsUtils.millis() - loopTime));
                    break;
                }
                numSteps += 1;
                PathingSpreadNode curNode = pQueue.Dequeue();
                Debug.Log("has cur node " + curNode + " with dist from start " + curNode.distFromStart + " and estimated dist from end = " + curNode.curExit.Dist(endPos.x, endPos.y, endPos.z) + " for total of " + (curNode.curExit.Dist(endPos.x, endPos.y, endPos.z) + curNode.distFromStart));

                //world.MakeLoggingNode("resPos", "considered step " + numSteps, UnityEngine.Random.ColorHSV(), curNode.wx, curNode.wy, curNode.wz);


                // if we set connectedToExit to curRunId above, that means we found a node that is connected to the exit! We are done
                if (curNode.curExit.connectedToExit == curRunId)
                {
                    // actually connected, we are actually done
                    if (curNode.curExit.distToExitIfConected == 0)
                    {
                        closest = curNode;
                        foundCompletePath = true;
                        break;
                    }
                    // not actually connected, just close. This means there are no exits that actually reach the player so we will just get as close as possible, but that we technically aren't done yet cause there might be a closer one
                    else
                    {
                        foundCloseExit = true;

                        int distToEnd = curNode.curExit.distToExitIfConected;
                        if (distToEnd < closestEstimatedDistToEnd)
                        {
                            closestEstimatedDistToEnd = distToEnd;
                            closest = curNode;
                        }
                    }
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
                                    if (verbose)
                                    {
                                        Debug.Log("failed to connect exit pieces, pos leaving exit " + posLeavingExit + " pos of cur node in exit " + posOfCurNodeInExit + " with exit value " + exitValue + " and leaving exit value " + otherExitValue);
                                    }
                                    curNodeParent.FindPathThroughExit(posOfCurNodeInExit.localX, posOfCurNodeInExit.localY, posOfCurNodeInExit.localZ, posLeavingExit.localX, posLeavingExit.localY, posLeavingExit.localZ, exitValue, verbose: false);

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


            //Debug.Log(startTime + " " + (midTime-startTime) + " " + (loopTime-midTime) + " " + (endTime-loopTime));

            curRunId += 1;

            // we found one! Now lookup the final path and then we are done
            if (foundCompletePath || foundCloseExit)
            {
                PathingSpreadNode actualRes = null;
                foreach (Tuple<PathingNodeExit, SpreadNode, int> connectedExit in connectedExitsToEnd)
                {
                    if (connectedExit.a == closest.curExit)
                    {
                        SpreadNode pathToGoal = connectedExit.b;

                        SpreadNode currentPos = closest.pathToPrevNode;
                        SpreadNode startOfPathToGoal = connectedExit.b.Root;

                        ushort exitNumCurPos = startOfPathToGoal.parentNode.data[currentPos.localX, currentPos.localY, currentPos.localZ];
                        ushort exitNumGoal = startOfPathToGoal.parentNode.data[startOfPathToGoal.localX, startOfPathToGoal.localY, startOfPathToGoal.localZ];

                        if (startOfPathToGoal.wx != currentPos.wx || startOfPathToGoal.wy != currentPos.wy || startOfPathToGoal.wz != currentPos.wz)
                        {
                            if (verbose)
                            {
                                Debug.Log("current pos " + currentPos + " start of path to exit " + startOfPathToGoal + " trying to find path between with exit nums " + exitNumGoal + " " + exitNumCurPos);
                            }
                            SpreadNode pathThroughExit = startOfPathToGoal.parentNode.FindPathThroughExit(currentPos.localX, currentPos.localY, currentPos.localZ, startOfPathToGoal.localX, startOfPathToGoal.localY, startOfPathToGoal.localZ, exitNumCurPos);
                            if (pathThroughExit != null)
                            {
                                closest = new PathingSpreadNode(connectedExit.a, closest, pathThroughExit);
                            }
                            else
                            {
                                startOfPathToGoal.parentNode.FindPathThroughExit(currentPos.localX, currentPos.localY, currentPos.localZ, startOfPathToGoal.localX, startOfPathToGoal.localY, startOfPathToGoal.localZ, exitNumCurPos, verbose: true);

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



            if (startNode == endNode)
            {

                SpreadNode pathThroughNode = startNode.FindPathThroughSingleNode(
                    (int)(startPos.x - startNode.locationSpec.minX),
                    (int)(startPos.y - startNode.locationSpec.minY),
                    (int)(startPos.z - startNode.locationSpec.minZ),
                    (int)(endPos.x - startNode.locationSpec.minX),
                    (int)(endPos.y - endNode.locationSpec.minY),
                    (int)(endPos.z - endNode.locationSpec.minZ),
                    allowFalling: true);


                if (pathThroughNode == null || closest == null)
                {
                    return closest;
                }
                else
                {
                    int totalLenComplexPathing = closest.distFromStart;
                    int totalLenSimplePathing = pathThroughNode.pathLen;

                    if (totalLenComplexPathing < totalLenSimplePathing)
                    {
                        return closest;
                    }
                    else
                    {
                        return new PathingSpreadNode(null, null, pathThroughNode);
                    }
                }

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
                //int theirX = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int theirX = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (neighbor.locationSpec.minY - 1 == locationSpec.maxY)
            {
                int myY = (int)(locationSpec.yWidth - 1);
                int theirY = 0;
                for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (neighbor.locationSpec.minZ - 1 == locationSpec.maxZ)
            {
                int myZ = (int)(locationSpec.zWidth - 1);
                //int theirZ = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int theirZ = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minX - 1 == neighbor.locationSpec.maxX)
            {
                int theirX = (int)(neighbor.locationSpec.xWidth - 1);
                //int myX = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int myX = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myY = (int)(y - locationSpec.minY);
                        int theirY = (int)(y - neighbor.locationSpec.minY);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minY - 1 == neighbor.locationSpec.maxY)
            {

                int theirY = (int)(neighbor.locationSpec.yWidth - 1);
                int myY = 0; // todo: double check this
                for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
                {
                    for (long z = Math.Max(locationSpec.minZ, neighbor.locationSpec.minZ); z <= locationSpec.maxZ && z <= neighbor.locationSpec.maxZ; z++)
                    {
                        int myX = (int)(x - locationSpec.minX);
                        int theirX = (int)(x - neighbor.locationSpec.minX);
                        int myZ = (int)(z - locationSpec.minZ);
                        int theirZ = (int)(z - neighbor.locationSpec.minZ);
                        callback(myX, myY, myZ, theirX, theirY, theirZ);

                    }
                }
            }
            if (locationSpec.minZ - 1 == neighbor.locationSpec.maxZ)
            {
                int theirZ = (int)(neighbor.locationSpec.zWidth - 1);
                //int myZ = Math.Min(neededSizeForward, neededSizeSide) - 1; // todo: double check this
                int myZ = 0;
                for (long y = Math.Max(locationSpec.minY, neighbor.locationSpec.minY); y <= locationSpec.maxY && y <= neighbor.locationSpec.maxY; y++)
                {
                    for (long x = Math.Max(locationSpec.minX, neighbor.locationSpec.minX); x <= locationSpec.maxX && x <= neighbor.locationSpec.maxX; x++)
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
                Debug.Log("removing neighbor");
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

                    bool canTraverse = MeetsFitCriteria(startPathToThem, endPathToThem, allowFalling: true);

                    if (canTraverse || true)
                    {
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

                }
                else if(myExitVal == 0 && theirExitVal != 0)
                {
                    List<Tuple<PathingNodeExit, SpreadNode, int>> connectedExits = FindConnectedExits(myX, myY, myZ, allowFalling: true, reversed: true);
                    if (SpreadNode.VERBOSE_PATHING) world.MakeLoggingNode("spook", "check " + connectedExits.Count, Color.magenta, myX - locationSpec.minX, myY - locationSpec.minY, myZ - locationSpec.minZ);
                    foreach (Tuple<PathingNodeExit, SpreadNode, int> connectedExit in connectedExits)
                    {
                        int myExitI = connectedExit.a.index;
                        int theirExitI = theirExitVal - 1;
                        PathingNodeExit myExit = connectedExit.a;
                        PathingNodeExit theirExit = neighborExits[theirExitI];
                        SpreadNode startPathToThem = connectedExit.b;
                        SpreadNode endPathToThem = new SpreadNode(theirX, theirY, theirZ, startPathToThem, neighbor);

                        bool canTraverse = MeetsFitCriteria(startPathToThem, endPathToThem, allowFalling: true);

                        if (canTraverse || true)
                        {
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
                    }
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
                    ushort spreadExitVal = data[n.localX, n.localY, n.localZ];
                    int spreadExitI = spreadExitVal - 1;
                    // we reached an exit that is distinct from ours that we haven't reached before, mark that path
                    // since we are doing breadth first search, the first result will be optimal (or tied for optimal)
                    bool weCanFit = MeetsFitCriteria(n, n.prev);
                    if (spreadExitVal != 0 && spreadExitVal != curExitVal && pathsBetween[curExitI, spreadExitI] == null && weCanFit)
                    {

                        PathingNodeExitConnection exitConnection = new PathingNodeExitConnection(exits[curExitI], exits[spreadExitI]);
                        exitConnection.AddPath(n);
                        exits[curExitI].AddExitConnection(exitConnection);
                        pathsBetween[curExitI, spreadExitI] = n;
                    }

                    if (weCanFit)
                    {
                        data[n.localX, n.localY, n.localZ, PathingFlags.Visited] = true;
                    }

                    return weCanFit;
                });
            }

            cachedPathsBetween = pathsBetween;

            return cachedPathsBetween;
        }


        List<PathingNodeExit> cachedExits;

        public const int MAX_EXIT_SIZE = 100000;

        public List<PathingNodeExit> GetExits(bool verbose=false)
        {
            //verbose = true;
            if (cachedExits != null)
            {
                return cachedExits;
            }
            // reset visited to false
            LoopThroughBorder((x, y, z) =>
            {
                data[x, y, z, PathingFlags.Visited] = false;
            });


            foreach (GameObject node in myLoggingNodes)
            {
                GameObject.Destroy(node);
            }
            myLoggingNodes.Clear();


            List<PathingNodeExit> exits = new List<PathingNodeExit>();
            ushort exitI = 1;
            // go through all places on the border
            LoopThroughBorder((x, y, z) =>
            {
                SpreadNode firstNode = new SpreadNode(x, y, z, null, this);
                // if we haven't been visited yet (no one else has spread into us) and can fit there, we are part of a new exit segment
                if (!data[x,y,z, PathingFlags.Visited] && MeetsFitCriteria(firstNode, allowFalling: false))
                {
                    List<Tuple<int, int, int>> curExit = new List<Tuple<int, int, int>>();

                    // mark us as visited, now we can create a new exit and mark us as in it
                    data[x, y, z, PathingFlags.Visited] = true;
                    data[x, y, z] = exitI;
                    curExit.Add(new Tuple<int, int, int>(x, y, z));
                    
                    DoSpread(x, y, z, (n) =>
                    {

                        // don't spread if we have reached the max exit size
                        // if MAX_EXIT_SIZE is around the size of all wall, this prevents U shapes forming where all the walls of a pathing chunk are a single exit
                        // while that is valid, that has the downside of the fastest path through an exit is no longer staying in that exit. If the exits stay relatively small that isn't as big of an issue
                        // this comes at the cost of performance (more nodes in the meta graph), but we'll have to see how much of a difference that makes cause idk
                        if (curExit.Count > MAX_EXIT_SIZE)
                        {
                            return false;
                        }
                        int sx = n.localX;
                        int sy = n.localY;
                        int sz = n.localZ;
                        //if (verbose) Debug.Log("get exits checking neighbor " + sx + " " + sy + " " + sz + " of node " + x + " " + y + " " + z);
                        // spread out through all positions on the border that we can fit in
                        if (!data[sx,sy,sz,PathingFlags.Visited] && IsBorder(sx,sy,sz) && MeetsFitCriteria(n, n.prev, allowFalling: false))
                        {
                            if (verbose) Debug.Log("get exits spreading at " + sx + " " + sy + " " + sz);
                            if (verbose) myLoggingNodes.Add(world.MakeLoggingNode("resPos", "spreading " + n + " " + n.prev, Color.cyan, n.wx, n.wy, n.wz));
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


                    int exitInd = (int)(exitI) - 1;
                    PathingNodeExit resCurExit = new PathingNodeExit(exitInd, this, curExit.ToArray());

                    exits.Add(resCurExit);
                    // increment exit number since we just finished finding this one
                    exitI += 1;
                }

            });

            cachedExits = exits;
            verbose = SpreadNode.VERBOSE_PATHING;
            if (verbose)
            {
                for (int i = 0; i < exits.Count; i++)
                {
                    Color exitColor = UnityEngine.Random.ColorHSV();
                    string locationString = this.locationSpec.ToString();

                    foreach (Tuple<int, int, int> pos in exits[i].localPositions)
                    {
                        GameObject exitNode = world.MakeLoggingNode(locationString, "exit " + i + " for " + locationSpec, exitColor, pos.a + locationSpec.minX, pos.b + locationSpec.minY, pos.c + locationSpec.minZ);
                        exitNode.transform.localScale *= 0.5f;
                        exitNode.transform.localPosition += new Vector3(0.5f, 0.5f, 0.5f);
                        myLoggingNodes.Add(exitNode);
                    }
                }
            }

            return exits;
        }




        public bool IsBorder(int x, int y, int z)
        {
            /*
            return x == neededSizeForward-1 || x == neededSizeSide - 1 || x == (locationSpec.xWidth - 1) ||
                y == 0 || y == (locationSpec.yWidth - 1) ||
                z == neededSizeForward-1 || z == neededSizeSide - 1 || z == (locationSpec.zWidth - 1);
             */

            return x == 0 || x == (int)(locationSpec.xWidth - 1) ||
                y == 0 || y == (int)(locationSpec.yWidth - 1) ||
                z == 0 || z == (int)(locationSpec.zWidth - 1);
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

        public bool ContainsPosition(long wx, long wy, long wz)
        {
            return (wx >= minX && wx <= maxX && wy >= minY && wy <= maxY && wz >= minZ && wz <= maxZ);
        }

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
