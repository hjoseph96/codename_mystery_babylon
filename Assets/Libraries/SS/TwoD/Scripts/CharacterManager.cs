using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class CharacterManager : BaseManager<Character>
    {
        public static CharacterManager instance { get; protected set; }

        public override void FixedUpdateMe()
        {
            FindTarget();
        }

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            list.Clear();
            instance = null;
        }

        protected void FindTarget()
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int k = 0; k < list[i].Count; k++)
                {
                    if (list[i][k].state != Character.State.Attacking)
                    {
                        Character target = null;
                        float minDistance = float.MaxValue;

                        for (int j = 0; j < list.Count; j++)
                        {
                            if (j != i)
                            {
                                for (int l = 0; l < list[j].Count; l++)
                                {
                                    float distance = (list[i][k].transform.position - list[j][l].transform.position).sqrMagnitude;
                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        target = list[j][l];
                                    }
                                }
                            }
                        }

                        if (target != null && target.state != Character.State.Die)
                        {
                            list[i][k].target = target.transform;
                        }
                    }
                }
            }
        }
    }
}