using UnityEngine;

namespace SBS
{
    public abstract class StudioAnimatedModel : StudioModel
    {
        public AnimationClip animClip;

        public override float GetTimeForRatio(float ratio)
        {
            if (animClip == null)
                return 0f;
            return animClip.length * ratio;
        }
    }
}
