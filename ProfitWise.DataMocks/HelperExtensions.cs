using System;
using System.Collections.Generic;

namespace ProfitWise.DataMocks
{
    public static class HelperExtensions
    {
        private static readonly Random random = new Random();

        public static T GetRandomItem<T>(this IList<T> input)
        {
            int r = random.Next(input.Count);
            return input[r];
        }
    }
}
