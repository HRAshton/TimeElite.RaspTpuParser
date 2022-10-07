using System;
using System.Collections.Generic;

namespace HRAshton.TimeElite.RaspTpuParser.Extensions
{
    /// <summary>
    /// Расширение <see cref="List{T}" />.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Разбивает упорядоченный список на подсписки по условию.
        /// Разбивка происходит, когда <see cref="Predicate{T}"/> равен true.
        /// Сам элемент при этом не попадает в подсписки.
        /// </summary>
        /// <typeparam name="T">Тип списка.</typeparam>
        /// <param name="source">Разбиваемый список.</param>
        /// <param name="predicate">Условие разбиения.</param>
        /// <returns>Подсписки входного списка.</returns>
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