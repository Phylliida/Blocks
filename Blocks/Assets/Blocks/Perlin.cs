﻿// perlin noise below from http://flafla2.github.io/2014/08/09/perlinnoise.html

// simplex noise from https://raw.githubusercontent.com/WardBenjamin/SimplexNoise/master/SimplexNoise/Noise.cs
// SimplexNoise for C#
// Author: Benjamin Ward
// Originally authored by Heikki Törmälä

using System;


namespace Blocks
{


    // from http://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf
    public class SimplexNoise
    { // Simplex noise in 2D, 3D and 4D
        private static int[,] grad3 = new int[,] {{1,1,0},{-1,1,0},{1,-1,0},{-1,-1,0},
             {1,0,1},{-1,0,1},{1,0,-1},{-1,0,-1},
             {0,1,1},{0,-1,1},{0,1,-1},{0,-1,-1}};
         private static int[,] grad4 = new int[,] {{0,1,1,1}, {0,1,1,-1}, {0,1,-1,1}, {0,1,-1,-1},
             {0,-1,1,1}, {0,-1,1,-1}, {0,-1,-1,1}, {0,-1,-1,-1},
             {1,0,1,1}, {1,0,1,-1}, {1,0,-1,1}, {1,0,-1,-1},
             {-1,0,1,1}, {-1,0,1,-1}, {-1,0,-1,1}, {-1,0,-1,-1},
             {1,1,0,1}, {1,1,0,-1}, {1,-1,0,1}, {1,-1,0,-1},
             {-1,1,0,1}, {-1,1,0,-1}, {-1,-1,0,1}, {-1,-1,0,-1},
             {1,1,1,0}, {1,1,-1,0}, {1,-1,1,0}, {1,-1,-1,0},
             {-1,1,1,0}, {-1,1,-1,0}, {-1,-1,1,0}, {-1,-1,-1,0}};

        private static int[] perm = new int[] {151,160,137,91,90,15,
         131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
         190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
         88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
         77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
         102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
         135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
         5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
         223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
         129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
         251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
         49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
         138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,151,160,137,91,90,15,
         131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
         190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
         88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
         77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
         102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
         135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
         5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
         223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
         129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
         251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
         49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
         138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};

    // A lookup table to traverse the simplex around a given point in 4D.
    // Details can be found where this table is used, in the 4D noise method.
    private static int[,] simplex = {
         {0,1,2,3},{0,1,3,2},{0,0,0,0},{0,2,3,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,2,3,0},
         {0,2,1,3},{0,0,0,0},{0,3,1,2},{0,3,2,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,3,2,0},
         {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
         {1,2,0,3},{0,0,0,0},{1,3,0,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,3,0,1},{2,3,1,0},
         {1,0,2,3},{1,0,3,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,0,3,1},{0,0,0,0},{2,1,3,0},
         {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
         {2,0,1,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,0,1,2},{3,0,2,1},{0,0,0,0},{3,1,2,0},
         {2,1,0,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,1,0,2},{0,0,0,0},{3,2,0,1},{3,2,1,0}};
         // This method is a *lot* faster than using (int)Math.floor(x)
         private static int fastfloor(double x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }
        private static double dot(int[] g, double x, double y)
        {
            return g[0] * x + g[1] * y;
        }
        private static double dot(int[] g, double x, double y, double z)
        {
            return g[0] * x + g[1] * y + g[2] * z;
        }
        private static double dot(int[] g, double x, double y, double z, double w)
        {
            return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
        }

        double seedX, seedY, seedZ;
        public SimplexNoise(double seed)
        {

            System.Random blah = new Random((int)(seed * 1000));

            seedX = blah.NextDouble() * 1024.0f;
            seedY = blah.NextDouble() * 1024.0f;
            seedZ = blah.NextDouble() * 1024.0f;


        }

        public float this[double x]
        {
            get
            {
                return (float)noise(x + seedX, 0.0);
            }
            set
            {

            }
        }


        public float this[double x, double y]
        {
            get
            {
                return (float)noise(x + seedX, y+seedY);
            }
            set
            {

            }
        }


        public float this[double x, double y, double z]
        {
            get
            {
                return (float)noise(x + seedX, y + seedY, z+seedZ);
            }
            set
            {

            }
        }

        // 2D simplex noise
        public static double noise(double xin, double yin)
        {
            double n0, n1, n2; // Noise contributions from the three corners
                               // Skew the input space to determine which simplex cell we're in
            double F2 = 0.5 * (Math.Sqrt(3.0) - 1.0);
            double s = (xin + yin) * F2; // Hairy factor for 2D
            int i = fastfloor(xin + s);
            int j = fastfloor(yin + s);
            double G2 = (3.0 - Math.Sqrt(3.0)) / 6.0;
            double t = (i + j) * G2;
            double X0 = i - t; // Unskew the cell origin back to (x,y) space
            double Y0 = j - t;
            double x0 = xin - X0; // The x,y distances from the cell origin
            double y0 = yin - Y0;
            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; } // upper triangle, YX order: (0,0)->(0,1)->(1,1)
                                     // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
                                     // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
                                     // c = (3-sqrt(3))/6
            double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
            double y2 = y0 - 1.0 + 2.0 * G2;
            // Work out the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int gi0 = perm[ii + perm[jj]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
            int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
            // Calculate the contribution from the three corners
            double t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * (grad3[gi0,0]* x0 + grad3[gi0,1]* y0); // (x,y) of grad3 used for 2D gradient
            }
            double t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * (grad3[gi1, 0] * x1 + grad3[gi1, 1] * y1);
            }
            double t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * (grad3[gi2, 0] * x2 + grad3[gi2, 1] * y2);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            return (70.0 * (n0 + n1 + n2) + 1.0f)/ 2.0f; // actually tweaked to be from 0 to 1;
        }
        // 3D simplex noise
        public static double noise(double xin, double yin, double zin)
        {
            double n0, n1, n2, n3; // Noise contributions from the four corners
                                   // Skew the input space to determine which simplex cell we're in
            double F3 = 1.0 / 3.0;
            double s = (xin + yin + zin) * F3; // Very nice and simple skew factor for 3D
            int i = fastfloor(xin + s);
            int j = fastfloor(yin + s);
            int k = fastfloor(zin + s);
            double G3 = 1.0 / 6.0; // Very nice and simple unskew factor, too
            double t = (i + j + k) * G3;
            double X0 = i - t; // Unskew the cell origin back to (x,y,z) space
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = xin - X0; // The x,y,z distances from the cell origin
            double y0 = yin - Y0;
            double z0 = zin - Z0;
            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
            }
            else
            { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
            }
            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.
            double x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            double x2 = x0 - i2 + 2.0 * G3; // Offsets for third corner in (x,y,z) coords
            double y2 = y0 - j2 + 2.0 * G3;
            double z2 = z0 - k2 + 2.0 * G3;
            double x3 = x0 - 1.0 + 3.0 * G3; // Offsets for last corner in (x,y,z) coords
            double y3 = y0 - 1.0 + 3.0 * G3;
            double z3 = z0 - 1.0 + 3.0 * G3;
            // Work out the hashed gradient indices of the four simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int gi0 = perm[ii + perm[jj + perm[kk]]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]] % 12;
            int gi2 = perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]] % 12;
            int gi3 = perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]] % 12;
            // Calculate the contribution from the four corners
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * (grad3[gi0, 0] * x0 + grad3[gi0, 1] * y0 + grad3[gi0, 2] * z0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * (grad3[gi1, 0] * x1 + grad3[gi1, 1] * y1 + grad3[gi1, 2] * z1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * (grad3[gi2, 0] * x2 + grad3[gi2, 1] * y2 + grad3[gi2, 2] * z2);
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * (grad3[gi3, 0] * x3 + grad3[gi3, 1] * y3 + grad3[gi3, 2] * z3);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1]
            return (32.0 * (n0 + n1 + n2 + n3)+1.0f)/2.0f; // actually tweaked to be from 0 to 1
        }
    }


    public class Noise
    {

        // A Javascript implementaion of Richard Brent's Xorgens xor4096 algorithm.
        //
        // This fast non-cryptographic random number generator is designed for
        // use in Monte-Carlo algorithms. It combines a long-period xorshift
        // generator with a Weyl generator, and it passes all common batteries
        // of stasticial tests for randomness while consuming only a few nanoseconds
        // for each prng generated.  For background on the generator, see Brent's
        // paper: "Some long-period random number generators using shifts and xors."
        // http://arxiv.org/pdf/1104.3115.pdf
        //
        // Usage:
        //
        // var xor4096 = require('xor4096');
        // random = xor4096(1);                        // Seed with int32 or string.
        // assert.equal(random(), 0.1520436450538547); // (0, 1) range, 53 bits.
        // assert.equal(random.int32(), 1806534897);   // signed int32, 32 bits.
        //
        // For nonzero numeric keys, this impelementation provides a sequence
        // identical to that by Brent's xorgens 3 implementaion in C.  This
        // implementation also provides for initalizing the generator with
        // string seeds, or for saving and restoring the state of the generator.
        //
        // On Chrome, this prng benchmarks about 4.5 times slower than
        // Javascript's built-in Math.random().



        public class XorGen
        {

            public Int32 next
            {
                get
                {
                    //int w = this.w,
                    //    int[] X = this.X, i = this.i, t, v;
                    int t, v;

                    // Update Weyl generator.
                    w = (w + 0x61c88647) | 0;
                    // Update xor generator.
                    v = X[(i + 34) & 127];
                    i = ((i + 1) & 127);
                    t = X[i];
                    v ^= v << 13;
                    t ^= t << 17;
                    v ^= unsignedRightShift(v, 15);
                    t ^= unsignedRightShift(t, 12);
                    // Update Xor generator array state.
                    v = X[i] = v ^ t;
                    // Result is the combination.
                    return (v + (w ^ unsignedRightShift(w,16))) | 0;
                }

                private set
                {

                }
            }

            int w, i;
            int[] X;

            static int unsignedRightShift(int a, int b)
            {
                return (int)(((uint)a) >> b);
            }

            static int unsignedLeftShift(int a, int b)
            {
                return (int)(((uint)a) << b);
            }
            public XorGen(int iSeed)
            {
                bool stringSeed = false;
                string sSeed = "";
                //var t, v, i, j, w, X = [], limit = 128;
                /*
                if (seed === (seed | 0))
                {
                    // Numeric seeds initialize v, which is used to generates X.
                    v = seed;
                    seed = null;
                }
                else
                {
                    // String seeds are mixed into v and X one character at a time.
                    seed = seed + '\0';
                    v = 0;
                    limit = Math.max(limit, seed.length);
                }
                */

                int v = iSeed;
                int limit = 128;
                int[] X = new int[limit];
                if (stringSeed)
                {
                    sSeed += '\0';
                    v = 0;
                    limit = Math.Max(limit, sSeed.Length);
                }
                int i, j;
                int t = 0, w = 0;
                // Initialize circular array and weyl value.
                for (i = 0, j = -32; j < limit; ++j)
                {
                    ////// Put the unicode characters into the array, and shuffle them.
                    if (stringSeed) v ^= sSeed[((j + 32) % sSeed.Length)];

                    // After 32 shuffles, take v as the starting w value.
                    if (j == 0) w = v;
                    v ^= v << 10;
                    v ^= unsignedRightShift(v, 15); // v >>> 15
                    v ^= v << 4;
                    v ^= unsignedRightShift(v, 13); // v >>> 13
                    if (j >= 0)
                    {
                        w = (w + 0x61c88647) | 0;     // Weyl.
                        t = (X[j & 127] ^= (v + w));  // Combine xor and weyl to init array.
                        i = (0 == t) ? i + 1 : 0;     // Count zeroes.
                    }
                }
                // We have detected all zeroes; make the key nonzero.
                if (i >= 128)
                {
                    if (!stringSeed || sSeed.Length == 0)
                    {
                        X[0] = -1;
                    }
                    else
                    {
                        X[1] = -1;
                    }
                    //X[((seed && seed.length) || 0) & 127] = -1;
                }
                // Run the generator 512 times to further mix the state before using it.
                // Factoring this as a function slows the main generator, so it is just
                // unrolled here.  The weyl generator is not advanced while warming up.
                i = 127;
                for (j = 4 * 128; j > 0; --j)
                {
                    v = X[(i + 34) & 127];
                    t = X[i = ((i + 1) & 127)];
                    v ^= v << 13;
                    t ^= t << 17;
                    v ^= unsignedRightShift(v, 15);
                    t ^= unsignedRightShift(t, 12);
                    X[i] = v ^ t;
                }
                // Storing state as object members is faster than using closure variables.
                this.w = w;
                this.X = X;
                this.i = i;
            }
        }
    }
}
namespace Simplex
{
    /// <summary>
    /// Implementation of the Perlin simplex noise, an improved Perlin noise algorithm.
    /// Based loosely on SimplexNoise1234 by Stefan Gustavson <http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/>
    /// </summary>
    public class Noise
    {
        public static float[] Calc1D(int width, float scale)
        {
            float[] values = new float[width];
            for (int i = 0; i < width; i++)
                values[i] = Generate(i * scale) * 128 + 128;
            return values;
        }

        public static float[,] Calc2D(int width, int height, float scale)
        {
            float[,] values = new float[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    values[i, j] = Generate(i * scale, j * scale) * 128 + 128;
            return values;
        }

        public static float[,,] Calc3D(int width, int height, int length, float scale)
        {
            float[,,] values = new float[width, height, length];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    for (int k = 0; k < length; k++)
                        values[i, j, k] = Generate(i * scale, j * scale, k * scale) * 128 + 128;
            return values;
        }

        public static float CalcPixel1D(int x, float scale)
        {
            return Generate(x * scale) * 128 + 128;
        }

        public static float CalcPixel2D(int x, int y, float scale)
        {
            return Generate(x * scale, y * scale) * 128 + 128;
        }

        public static float CalcPixel3D(int x, int y, int z, float scale)
        {
            return Generate(x * scale, y * scale, z * scale) * 128 + 128;
        }

        static Noise()
        {
            perm = new byte[permOriginal.Length];
            Simplex.Noise.permOriginal.CopyTo(perm, 0);
        }

        public static int Seed
        {
            get { return seed; }
            set
            {
                if (value == 0)
                {
                    perm = new byte[permOriginal.Length];
                    Simplex.Noise.permOriginal.CopyTo(perm, 0);
                }
                else
                {
                    perm = new byte[512];
                    Random random = new Random(value);
                    random.NextBytes(perm);
                }
            }
        }
        private static int seed = 0;

        /// <summary>
        /// 1D simplex noise
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float Generate(float x)
        {
            int i0 = FastFloor(x);
            int i1 = i0 + 1;
            float x0 = x - i0;
            float x1 = x0 - 1.0f;

            float n0, n1;

            float t0 = 1.0f - x0 * x0;
            t0 *= t0;
            n0 = t0 * t0 * grad(perm[i0 & 0xff], x0);

            float t1 = 1.0f - x1 * x1;
            t1 *= t1;
            n1 = t1 * t1 * grad(perm[i1 & 0xff], x1);
            // The maximum value of this noise is 8*(3/4)^4 = 2.53125
            // A factor of 0.395 scales to fit exactly within [-1,1]
            return 0.395f * (n0 + n1);
        }

        /// <summary>
        /// 2D simplex noise
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float Generate(float x, float y)
        {
            const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
            const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

            float n0, n1, n2; // Noise contributions from the three corners

            // Skew the input space to determine which simplex cell we're in
            float s = (x + y) * F2; // Hairy factor for 2D
            float xs = x + s;
            float ys = y + s;
            int i = FastFloor(xs);
            int j = FastFloor(ys);

            float t = (float)(i + j) * G2;
            float X0 = i - t; // Unskew the cell origin back to (x,y) space
            float Y0 = j - t;
            float x0 = x - X0; // The x,y distances from the cell origin
            float y0 = y - Y0;

            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6

            float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
            float y2 = y0 - 1.0f + 2.0f * G2;

            // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
            int ii = Mod(i, 256);
            int jj = Mod(j, 256);

            // Calculate the contribution from the three corners
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0.0f) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * grad(perm[ii + perm[jj]], x0, y0);
            }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0.0f) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * grad(perm[ii + i1 + perm[jj + j1]], x1, y1);
            }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0.0f) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * grad(perm[ii + 1 + perm[jj + 1]], x2, y2);
            }

            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            return 40.0f * (n0 + n1 + n2); // TODO: The scale factor is preliminary!
        }


        static uint hash(uint x)
        {

            x += (x << 10);
            x ^= (x >> 6);
            x += (x << 3);
            x ^= (x >> 11);
            x += (x << 15);
            return x;
        }
        static uint hash(uint x, uint y)
        {
            return hash(x ^ hash(y));
        }

        static uint hash(uint x, uint y, uint z)
        {
            return hash(x ^ hash(y) ^ hash(z));
        }

        static uint hash(uint x, uint y, uint z, uint w)
        {
            return hash(x ^ hash(y) ^ hash(z) ^ hash(w));
        }


        public static float rand(float f)
        {
            const uint mantissaMask = 0x007FFFFFu;
            const uint one = 0x3F800000u;

            uint h = hash(asuint(f));
            h &= mantissaMask;
            h |= one;

            float r2 = asfloat(h);
            return r2 - 1.0f;
        }

        /// <summary>
        /// Returns a random value from 0.0 to 1.0 (inclusive)
        /// from some glsl code somewhere i don't remember exactly where
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float rand(float x, float y)
        {
            
            const uint mantissaMask = 0x007FFFFFu;
            const uint one = 0x3F800000u;

            uint h = hash(asuint(x), asuint(y));
            h &= mantissaMask;
            h |= one;

            float r2 = asfloat(h);
            return r2 - 1.0f;
        }



        public static float randOffsetX, randOffsetY, randOffsetZ;

        public static int randInt(int minValInclusive, int maxValExclusive, float x, float y, float z)
        {
            float val = rand(x, y, z);
            if (val >= 1.0f)
            {
                val = 0.9999f;
            }
            return (int)((maxValExclusive - minValInclusive) * val + minValInclusive);
        }

        public static float rand(float x, float y, float z)
        {
            x += randOffsetX;
            y += randOffsetY;
            z += randOffsetZ;
            const uint mantissaMask = 0x007FFFFFu;
            const uint one = 0x3F800000u;

            uint h = hash(asuint(x), asuint(y), asuint(z));
            h &= mantissaMask;
            h |= one;

            float r2 = asfloat(h);
            return r2 - 1.0f;
        }

        public static unsafe uint asuint(float value)
        {
            return *(uint*)(&value);
        }
        public static unsafe float asfloat(uint value)
        {
            return *(float*)(&value);
        }


        public static float randOld(float x, float y, float z)
        {
            return (Generate(x, y, z) + 1.0f) / 2.0f;
        }
        public static float randOld(long x, long y, long z)
        {
            return (Generate((float)x, (float)y, (float)z) + 1.0f) / 2.0f;
        }

        public static float Generate(float x, float y, float z)
        {
            // Simple skewing factors for the 3D case
            const float F3 = 0.333333333f;
            const float G3 = 0.166666667f;

            float n0, n1, n2, n3; // Noise contributions from the four corners

            // Skew the input space to determine which simplex cell we're in
            float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
            float xs = x + s;
            float ys = y + s;
            float zs = z + s;
            int i = FastFloor(xs);
            int j = FastFloor(ys);
            int k = FastFloor(zs);

            float t = (float)(i + j + k) * G3;
            float X0 = i - t; // Unskew the cell origin back to (x,y,z) space
            float Y0 = j - t;
            float Z0 = k - t;
            float x0 = x - X0; // The x,y,z distances from the cell origin
            float y0 = y - Y0;
            float z0 = z - Z0;

            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

            /* This code would benefit from a backport from the GLSL version! */
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
            }
            else
            { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
            }

            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.

            float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            float y1 = y0 - j1 + G3;
            float z1 = z0 - k1 + G3;
            float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
            float y2 = y0 - j2 + 2.0f * G3;
            float z2 = z0 - k2 + 2.0f * G3;
            float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
            float y3 = y0 - 1.0f + 3.0f * G3;
            float z3 = z0 - 1.0f + 3.0f * G3;

            // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
            int ii = Mod(i, 256);
            int jj = Mod(j, 256);
            int kk = Mod(k, 256);

            // Calculate the contribution from the four corners
            float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0.0f) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * grad(perm[ii + perm[jj + perm[kk]]], x0, y0, z0);
            }

            float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0.0f) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * grad(perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]], x1, y1, z1);
            }

            float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0.0f) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * grad(perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]], x2, y2, z2);
            }

            float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0.0f) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * grad(perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]], x3, y3, z3);
            }

            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1]
            float res = 32.0f * (n0 + n1 + n2 + n3); // TODO: The scale factor is preliminary!
            res = (res + 1.0f) / 2.0f; // fix so it is from 0 to 1 instead
            return res;
        }

        private static byte[] perm;

        private static readonly byte[] permOriginal = new byte[]
        {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        private static int FastFloor(float x)
        {
            return (x > 0) ? ((int)x) : (((int)x) - 1);
        }

        private static int Mod(int x, int m)
        {
            int a = x % m;
            return a < 0 ? a + m : a;
        }

        private static float grad(int hash, float x)
        {
            int h = hash & 15;
            float grad = 1.0f + (h & 7);   // Gradient value 1.0, 2.0, ..., 8.0
            if ((h & 8) != 0) grad = -grad;         // Set a random sign for the gradient
            return (grad * x);           // Multiply the gradient with the distance
        }

        private static float grad(int hash, float x, float y)
        {
            int h = hash & 7;      // Convert low 3 bits of hash code
            float u = h < 4 ? x : y;  // into 8 simple gradient directions,
            float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }

        private static float grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
            float u = h < 8 ? x : y; // gradient directions, and compute dot product.
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
        }

        private static float grad(int hash, float x, float y, float z, float t)
        {
            int h = hash & 31;      // Convert low 5 bits of hash code into 32 simple
            float u = h < 24 ? x : y; // gradient directions, and compute dot product.
            float v = h < 16 ? y : z;
            float w = h < 8 ? z : t;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
        }
    }
}
public class Perlin
{

    public int repeat;

    public Perlin(int repeat = -1)
    {
        this.repeat = repeat;
    }

    public double OctavePerlin(double x, double y, double z, int octaves, double persistence)
    {
        double total = 0;
        double frequency = 1;
        double amplitude = 1;
        double maxValue = 0;            // Used for normalizing result to 0.0 - 1.0
        for (int i = 0; i < octaves; i++)
        {
            total += perlin(x * frequency, y * frequency, z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    private static readonly int[] permutation = { 151,160,137,91,90,15,					// Hash lookup table as defined by Ken Perlin.  This is a randomly
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,	// arranged array of all numbers from 0-255 inclusive.
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    private static readonly int[] p;                                                    // Doubled permutation to avoid overflow

    static Perlin()
    {
        p = new int[512];
        for (int x = 0; x < 512; x++)
        {
            p[x] = permutation[x % 256];
        }
    }

    public double perlin(double x, double y, double z)
    {
        if (repeat > 0)
        {                                   // If we have any repeat on, change the coordinates to their "local" repetitions
            x = x % repeat;
            y = y % repeat;
            z = z % repeat;
        }

        int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
        int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
        int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
        double xf = x - (int)x;                             // We also fade the location to smooth the result.
        double yf = y - (int)y;

        double zf = z - (int)z;
        double u = fade(xf);
        double v = fade(yf);
        double w = fade(zf);

        int aaa, aba, aab, abb, baa, bba, bab, bbb;
        aaa = p[p[p[xi] + yi] + zi];
        aba = p[p[p[xi] + inc(yi)] + zi];
        aab = p[p[p[xi] + yi] + inc(zi)];
        abb = p[p[p[xi] + inc(yi)] + inc(zi)];
        baa = p[p[p[inc(xi)] + yi] + zi];
        bba = p[p[p[inc(xi)] + inc(yi)] + zi];
        bab = p[p[p[inc(xi)] + yi] + inc(zi)];
        bbb = p[p[p[inc(xi)] + inc(yi)] + inc(zi)];

        double x1, x2, y1, y2;
        x1 = lerp(grad(aaa, xf, yf, zf),                // The gradient function calculates the dot product between a pseudorandom
                    grad(baa, xf - 1, yf, zf),              // gradient vector and the vector from the input coordinate to the 8
                    u);                                     // surrounding points in its unit cube.
        x2 = lerp(grad(aba, xf, yf - 1, zf),                // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                    grad(bba, xf - 1, yf - 1, zf),              // values we made earlier.
                      u);
        y1 = lerp(x1, x2, v);

        x1 = lerp(grad(aab, xf, yf, zf - 1),
                    grad(bab, xf - 1, yf, zf - 1),
                    u);
        x2 = lerp(grad(abb, xf, yf - 1, zf - 1),
                      grad(bbb, xf - 1, yf - 1, zf - 1),
                      u);
        y2 = lerp(x1, x2, v);

        return ((lerp(y1, y2, w) + 1) / 2)/255.0f;                       // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
    }

    public int inc(int num)
    {
        num++;
        if (repeat > 0) num %= repeat;

        return num;
    }

    public static double grad(int hash, double x, double y, double z)
    {
        int h = hash & 15;                                  // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
        double u = h < 8 /* 0b1000 */ ? x : y;              // If the most significant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

        double v;                                           // In Ken Perlin's original implementation this was another conditional operator (?:).  I
                                                            // expanded it for readability.

        if (h < 4 /* 0b0100 */)                             // If the first and second significant bits are 0 set v = y
            v = y;
        else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/)// If the first and second significant bits are 1 set v = x
            v = x;
        else                                                // If the first and second significant bits are not equal (0/1, 1/0) set v = z
            v = z;

        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
    }

    public static double fade(double t)
    {
        // Fade function as defined by Ken Perlin.  This eases coordinate values
        // so that they will "ease" towards integral values.  This ends up smoothing
        // the final output.
        return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
    }

    public static double lerp(double a, double b, double x)
    {
        return a + x * (b - a);
    }
}