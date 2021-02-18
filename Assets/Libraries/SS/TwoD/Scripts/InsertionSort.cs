using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public enum SortBy
    {
        Y,
        Z
    }

    public class InsertionSort<T> where T : MonoBehaviour
    {
        public static void Sort(List<T> input, SortBy sortBy)
        {
            for (int i = 0; i < input.Count - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (NeedSwap(input[j - 1].transform.position, input[j].transform.position, sortBy))
                    {
                        T temp = input[j - 1];
                        input[j - 1] = input[j];
                        input[j] = temp;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        static bool NeedSwap(Vector3 a, Vector3 b, SortBy sortBy)
        {
            switch (sortBy)
            {
                case SortBy.Y:
                    return ((a.y < b.y) || (a.y == b.y && a.x < b.x));
                case SortBy.Z:
                    return ((a.z < b.z) || (a.z == b.z && a.x < b.x));
            }

            return false;
        }
    }
}