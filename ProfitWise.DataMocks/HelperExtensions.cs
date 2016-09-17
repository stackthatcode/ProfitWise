using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.DataMocks
{
    public static class HelperExtensions
    {
        private static readonly Random random = new Random();

        public static T GetRandomItem<T>(this IList<T> input)
        {
            int r = random.Next(input.Count);
            // Console.WriteLine(input.Count + " " + r);
            return input[r];
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int GenerateRandomInteger(int max)
        {
            return random.Next(max);
        }
    }
}
