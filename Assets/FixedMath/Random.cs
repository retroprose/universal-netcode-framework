using System;

namespace FixedMath
{

    public class Random
    {
        // static data and functions
        private const int N = 624;
        private const int M = 397;
        private const uint UPPER_MASK = 0x80000000;
        private const uint LOWER_MASK = 0x7fffffff;
        private const uint MATRIX_A = 0x9908b0df;

        private static uint imul(uint a, uint b)
        {
            uint al = a & 0xffff;
            uint ah = a >> 16;
            uint bl = b & 0xffff;
            uint bh = b >> 16;
            uint ml = al * bl;
            uint mh = ((((ml >> 16) + al * bh) & 0xffff) + ah * bl) & 0xffff;
            return (mh << 16) | (ml & 0xffff);
        }

        // local member variables
        private int p;
        private int q;
        private int r;
        private uint[] x = new uint[N];

        public void Copy(Random other)
        {
            p = other.p;
            q = other.q;
            r = other.r;
            Array.Copy(other.x, x, x.Length);
        }

        // public interface
        public Random()
        {
            SetSeed(0);
        }

        public Random(uint s)
        {
            SetSeed(s);
        }

        public void SetSeed(uint s)
        {
            x[0] = s;
            for (uint i = 1; i < N; i++)
            {
                x[i] = imul(1812433253, x[i - 1] ^ (x[i - 1] >> 30)) + i;
                x[i] &= 0xffffffff;
            }
            p = 0;
            q = 1;
            r = M;
        }

        //public double NextDouble()
        //{
        //    return GetUint(32) / 4294967296.0f;
        //}

        public int Next(int min, int max)
        {
            return (int)(GetUint() % (max + 1 - min)) + min;
        }

        public int Next(int max)
        {
            return (int)(GetUint() % (max + 1));
        }

        public int Next()
        {
            return (int)GetUint();
        }

        public uint GetUint()
        {
            uint y = (x[p] & UPPER_MASK) | (x[q] & LOWER_MASK);
            x[p] = x[r] ^ (y >> 1) ^ ((y & 1) * MATRIX_A);
            y = x[p];

            if (++p == N) p = 0;
            if (++q == N) q = 0;
            if (++r == N) r = 0;

            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);

            return y;
        }

        public uint GetUint(uint bits)
        {
            uint y = (x[p] & UPPER_MASK) | (x[q] & LOWER_MASK);
            x[p] = x[r] ^ (y >> 1) ^ ((y & 1) * MATRIX_A);
            y = x[p];

            if (++p == N) p = 0;
            if (++q == N) q = 0;
            if (++r == N) r = 0;

            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);

            return y >> (32 - (int)bits);
        }

    }

}