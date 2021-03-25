using System;
using UnityEngine;

namespace SBS
{
    [Serializable]
    public class LightProperty : PropertyBase
    {
        public Light obj = null;
        public bool followCamera = true;
        public Vector3 pos = Vector3.zero;
    }
}
