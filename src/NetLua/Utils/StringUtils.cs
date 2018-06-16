using System;
using System.Linq;

namespace NetLua.Utils
{
    internal class StringUtils
    {
        private static readonly Random Random = new Random();

        public static string GetRandomHexNumber(int digits)
        {
            var buffer = new byte[digits / 2];
            Random.NextBytes(buffer);

            var result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            return digits % 2 == 0
                ? result
                : result + Random.Next(16).ToString("X");
        }
    }
}
