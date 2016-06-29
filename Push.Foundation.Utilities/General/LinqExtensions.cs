﻿using System;
using System.Collections.Generic;

namespace Push.Utilities.General
{
    public static class LinqExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            foreach (var member in input)
            {
                action(member);
            }
        }
    }
}