using System;
using UnityEngine;

namespace SBS
{
    [Serializable]
    public class FrameProperty : PropertyBase
    {
        public Vector2 resolution = new Vector2(500.0f, 400.0f);
        public int simulatedFrame = 0;
        public int frameSize = 10;
        public double delay = 0;
    }
}
