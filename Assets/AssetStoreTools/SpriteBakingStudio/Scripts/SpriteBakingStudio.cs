using UnityEngine;
using System.Collections.Generic;
using System;

namespace SBS
{
    public delegate void ModelRotationCallback();

    [DisallowMultipleComponent]
    public class SpriteBakingStudio : MonoBehaviour
    {
        public StudioSetting setting;

        [NonSerialized]
        public bool folding = false;

        [NonSerialized]
        public string[] atlasSizes = new string[] { "128", "256", "512", "1024", "2048", "4096", "8192" };

        [NonSerialized]
        public List<SamplingData> samplings = new List<SamplingData>();
        [NonSerialized]
        public List<Frame> selectedFrames = new List<Frame>();

        [NonSerialized]
        public bool isSamplingReady = false;
        [NonSerialized]
        public bool isBakingReady = false;
    }
}
