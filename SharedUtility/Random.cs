using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SUS.Shared.Utility
{
    public static class RandomImpl
    {
        private static readonly CSPRandom _Random;

        static RandomImpl()
        {
            _Random = new CSPRandom();
        }

        public static int Next()
        {
            return _Random.Next();
        }

        public static int Next(int values)
        {
            return _Random.Next(values);
        }

        public static int Next(int min, int max)
        {
            return _Random.Next(min, max+1);
        }

        public static double NextDouble()
        {
            return _Random.NextDouble();
        }
    }

    public sealed class CSPRandom
    {
        private readonly RNGCryptoServiceProvider cspRng = new RNGCryptoServiceProvider();
        private const int BUFFER_SIZE = sizeof(double);
        private byte[] buffer = new byte[BUFFER_SIZE];

        public int Next()
        {
            this.cspRng.GetBytes(buffer);
            return BitConverter.ToInt32(this.buffer, 0) & 0x7fffffff;
        }

        public int Next(int values)
        {
            if (values <= 0)
                return 0;

            return Next() % values; // Return the remainder (range based.)
        }

        public int Next(int min, int max)
        {
            if (min <= 0 || max <= 0)
                return 0;
            if (min >= max)
                return max;

            return min + Next(max - min);   // Since it can proc '0', we add the min to it.
        }

        public double NextDouble()
        {
            this.cspRng.GetBytes(buffer);
            var ul = BitConverter.ToUInt64(this.buffer, 0) / (1 << 11);
            return ul / (Double)(1UL << 53);
        }
    }
}
