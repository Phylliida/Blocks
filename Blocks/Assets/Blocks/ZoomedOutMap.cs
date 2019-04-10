using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace Blocks
{
    public abstract class Abstract2DWorld<T> where T : class
    {


        QuickLongDict<Dictionary<long, T>> lookupX = new QuickLongDict<Dictionary<long, T>>(100);
        QuickLongDict<Dictionary<long, T>> lookupZ = new QuickLongDict<Dictionary<long, T>>(100);

        public T GetOrGenerateChunk(long x, long z)
        {
            T res = GetChunk(x, z);
            if (res == null)
            {
                return GenerateChunk(x, z);
            }
            else
            {
                return res;
            }
        }

        public T GetChunk(long x, long z)
        {
            int numX = 0;
            if (lookupX.ContainsKey(x))
            {
                numX = lookupX[x].Count;
            }
            int numZ = 0;
            if (lookupZ.ContainsKey(z))
            {
                numZ = lookupZ[z].Count;
            }

            if (numX == 0 && numZ == 0)
            {
                return null;
            }
            Dictionary<long, T> thingsToLookThrough;
            long keyToLookWith;

            if (numX > numZ)
            {
                thingsToLookThrough = lookupZ[z];
                keyToLookWith = x;
            }
            else
            {
                thingsToLookThrough = lookupX[x];
                keyToLookWith = z;
            }

            if (thingsToLookThrough.ContainsKey(keyToLookWith))
            {
                return thingsToLookThrough[keyToLookWith];
            }
            else
            {
                return null;
            }
        }

        public abstract T GenerateChunk(long x, long z);
    }










    class MapCellData
    {
        public float water;

        public MapCellData(float water)
        {
            this.water = water;
        }
    }

    class ZoomedOutMap : Abstract2DWorld<MapCellData>
    {
        SimplexNoise noise;
        public ZoomedOutMap(int seed)
        {
            noise = new SimplexNoise(seed);
        }

        public override MapCellData GenerateChunk(long x, long z)
        {
            return new MapCellData(noise[x, z]);
        }
    }
}


