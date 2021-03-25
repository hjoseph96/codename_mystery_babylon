using UnityEngine;

namespace SBS
{
    public class SamplingData
    {
        public Texture2D tex;
        public float time;

        public SamplingData(Texture2D tex_, float time_)
        {
            tex = tex_;
            time = time_;
        }
    }
}
