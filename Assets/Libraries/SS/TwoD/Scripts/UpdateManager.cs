using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class UpdateManager : MonoBehaviour
    {
        const int MAX = 1000;

        public static UpdateManager instance { get; protected set; }

        List<UpdateRegister> list = new List<UpdateRegister>(MAX);

        public void Add(UpdateRegister t)
        {
            SS.Generic.SmartList<UpdateRegister>.Add(list, t);
        }

        public void Remove(UpdateRegister t)
        {
            SS.Generic.SmartList<UpdateRegister>.Remove(list, t);
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].UpdateMe();
            }
        }

        void LateUpdate()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].LateUpdateMe();
            }
        }

        void FixedUpdate()
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].FixedUpdateMe();
            }
        }

        void OnDestroy()
        {
            list.Clear();
            instance = null;
        }
    }
}