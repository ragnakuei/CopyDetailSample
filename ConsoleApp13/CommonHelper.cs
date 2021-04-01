using System;
using System.Collections.Generic;

namespace ConsoleApp13
{
    public static class CommonHelper
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                return;
            }

            foreach (T obj in source)
            {
                action.Invoke(obj);
            }
        }

        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action<TElement, int> action)
        {
            int index = 0;

            foreach (var item in source)
            {
                action.Invoke(item, index);
                index++;
            }
        }
    }
}
