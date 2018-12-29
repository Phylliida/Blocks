using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks
{
    public class PathingNode
    {
        PathingNodeBlockChunk locationSpec;
        public PathingNode(PathingNodeBlockChunk locationSpec)
        {
            this.locationSpec = locationSpec;
        }
    }

    public class PathingNodeLocationSpec
    {

    }

    public class PathingNodeBlockChunk : PathingNodeLocationSpec
    {
        World world;
        long minX, minY, minZ;
        long maxX, maxY, maxZ;
        public PathingNodeBlockChunk(World world, long minX, long minY, long minZ, long maxX, long maxY, long maxZ)
        {
            this.world = world;
            this.minX = minX; this.maxX = maxX;
            this.minY = minY; this.maxY = maxY;
            this.minZ = minZ; this.maxZ = maxZ;
        }
    }
}
