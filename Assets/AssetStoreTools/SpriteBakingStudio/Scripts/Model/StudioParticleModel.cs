using UnityEngine;

namespace SBS
{
    public class StudioParticleModel : StudioModel
    {
        public ParticleSystem mainParticleSystem;

        public bool targetChecked = false;
        public bool targetChecking = false;
        public Vector3 maxSize;
        public Vector3 minPos;
        public Vector3 maxPos;

        public override Vector3 GetSize()
        {
            return maxSize;
        }

        public override Vector3 GetMinPos()
        {
            return minPos;
        }

        public override Vector3 GetMaxPos()
        {
            return maxPos;
        }

        public override float GetTimeForRatio(float ratio)
        {
            if (mainParticleSystem == null)
                return 0f;

            return mainParticleSystem.main.duration * ratio;
        }

        public override void UpdateModel(Frame frame)
        {
            if (mainParticleSystem == null)
                return;

            if (frame.number == 0)
            {
                mainParticleSystem.Simulate(0.0f);
            }
            else
            {
                float period = frame.time - mainParticleSystem.time;
                mainParticleSystem.Simulate(period, true, false);
            }

            if (customizer != null)
                customizer.UpdateFrame(frame.number, frame.time);
        }

        public override bool IsReady()
        {
            return (mainParticleSystem != null && targetChecked);
        }
    }
}
