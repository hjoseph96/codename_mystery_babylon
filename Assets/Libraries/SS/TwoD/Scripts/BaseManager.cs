using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class BaseManager<T> : UpdateRegister where T : Character
    {
        const int MAX_TEAM = 10;
        const int MAX_CHARACTER = 200;

        public List<List<T>> list = new List<List<T>>(MAX_TEAM);

        public void Add(T t)
        {
            if (list.Count < t.teamIndex + 1)
            {
                int length = t.teamIndex + 1 - list.Count;
                for (int i = 0; i < length; i++)
                {
                    List<T> l = new List<T>(MAX_CHARACTER);
                    list.Add(l);
                }
            }

            if (list[t.teamIndex].Count < list[t.teamIndex].Capacity)
            {
                SS.Generic.SmartList<T>.Add(list[t.teamIndex], t);
            }
            else
            {
                Destroy(t.gameObject);
            }
        }

        public void Remove(T t)
        {
            SS.Generic.SmartList<T>.Remove(list[t.teamIndex], t);
        }

        protected int SortByDistanceToTarget(T a, T b)
        {
            if (a.sqrDistanceToTarget < b.sqrDistanceToTarget)
            {
                return -1;
            }
            else if (a.sqrDistanceToTarget > b.sqrDistanceToTarget)
            {
                return 1;
            }
            return 0;
        }
    }
}