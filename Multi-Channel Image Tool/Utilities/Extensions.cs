
using System;
using System.Collections.Generic;
using System.Linq;

namespace Multi_Channel_Image_Tool
{
    public static class Extensions
    {
        public static T Pop<T>(this HashSet<T> hashSet)
        {
            T first = hashSet.First();
            hashSet.Remove(first);
            return first;
        }

        public static List<T2> ConvertAll<T, T2>(this List<T> list, Func<T, T2> Converter)
        {
            List<T2> result = new List<T2>();
            int listSize = list.Count;

            for (int i = 0; i < listSize; i++)
            {
                result.Add(Converter(list[i]));
            }

            return result;
        }

        public static void Concat<T>(this List<T> list, IEnumerable<T> toAdd)
        {
            foreach (T item in toAdd)
            {
                list.Add(item);
            }
        }

        public static bool Contains<T>(this T[] array, T toFind)
        {
            foreach (T item in array)
            {
                if (item.Equals(toFind)) { return true; }
            }
            return false;
        }
    }
}