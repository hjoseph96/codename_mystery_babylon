using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.Generic
{
    public class SmartList<T> where T : class
    {
        public static void Add(List<T> list, T item)
        {
            list.Add(item);
        }

        public static bool Contains(List<T> list, T item)
        {
            return IndexOf(list, item) > -1;
        }

        public static int IndexOf(List<T> list, T item)
        { 
            int i, count;

            count = list.Count;
            for (i = 0; i < count; i++)
                if (list[i] == item)
                    return i;
            return -1;
        }

        public static bool Remove(List<T> list, T item)
        {
            int i;

            i = IndexOf(list, item);
            if (i == -1)
                return false;
            list.RemoveAt(i);

            return true;
        }
    }

}