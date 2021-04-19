using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class RenderingOrderManager : UpdateRegister
    {
        const int MAX = 200;

        public static RenderingOrderManager instance { get; protected set; }

        List<RenderingOrderRegister> list = new List<RenderingOrderRegister>(MAX);

        public void Add(RenderingOrderRegister t)
        {
            SS.Generic.SmartList<RenderingOrderRegister>.Add(list, t);
        }

        public void Remove(RenderingOrderRegister t)
        {
            SS.Generic.SmartList<RenderingOrderRegister>.Remove(list, t);
        }

        void Awake()
        {
            instance = this;
        }

        public override void LateUpdateMe()
        {
            InsertionSort<RenderingOrderRegister>.Sort(list, SortBy.Z);

            int count = 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].spriteRenderers != null && list[i].spriteRenderers.Length > 0)
                {
                    for (int j = 0; j < list[i].spriteRenderers.Length; j++)
                    {
                        list[i].spriteRenderers[j].sortingOrder = count;
                        count++;
                    }
                }
                else
                {
                    list[i].spriteRenderer.sortingOrder = count;
                    count++;
                }
            }
        }

        void OnDestroy()
        {
            list.Clear();
            instance = null;
        }
    }
}