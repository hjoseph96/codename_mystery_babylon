using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class TrafficJamResolver : BaseManager<MovingCharacter>
    {
        const int MAX_TRAFFIC_JAM = 100;

        public static TrafficJamResolver instance { get; protected set; }

        List<MovingCharacter> trafficJamList = new List<MovingCharacter>(MAX_TRAFFIC_JAM);

        public override void FixedUpdateMe()
        {
            DetectTrafficJam();
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

        void DetectTrafficJam()
        {
            for (int i = 0; i < list.Count; i++)
            {
                trafficJamList.Clear();

                for (int j = 0; j < list[i].Count; j++)
                {
                    if (list[i][j].trafficJamState == MovingCharacter.TrafficJamState.TRAFFIC_JAM)
                    {
                        trafficJamList.Add(list[i][j]);
                    }
                }

                if (trafficJamList.Count > 1)
                {
                    trafficJamList.Sort(SortByDistanceToTarget);

                    for (int k = 0; k < trafficJamList.Count; k++)
                    {
                        trafficJamList[k].offsetPriority = k + 1;
                        trafficJamList[k].trafficJamState = MovingCharacter.TrafficJamState.TRAFFIC_JAM_RESOLVED;
                    }
                }
            }
        }
    } 
}