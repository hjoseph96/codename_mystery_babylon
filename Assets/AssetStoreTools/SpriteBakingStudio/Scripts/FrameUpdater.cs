using UnityEngine;

namespace SBS
{
    public abstract class FrameUpdater : MonoBehaviour
    {
        public abstract void UpdateFrame(int frame, float time = 0.0f);
    }
}
