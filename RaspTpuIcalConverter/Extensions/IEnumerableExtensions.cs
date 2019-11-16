using System;
using System.Collections.Generic;

namespace RaspTpuIcalConverter.Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> SplitBy<T>(this List<T> source, Func<T, bool> predicate)
        {
            var items = new List<List<T>>();

            if (source.Count == 0)
            {
                return items;
            }

            items.Add(new List<T>());
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    items.Add(new List<T>());
                }
                else
                {
                    items[items.Count - 1].Add(item);
                }
            }

            return items;
        }
    }
}