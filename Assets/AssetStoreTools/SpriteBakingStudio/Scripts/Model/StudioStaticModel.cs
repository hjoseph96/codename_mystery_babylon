using UnityEngine;

namespace SBS
{
    public class StudioStaticModel : StudioModel
    {
        public MeshRenderer meshRndr;

        public override Vector3 GetSize()
        {
            return meshRndr != null ? meshRndr.bounds.size : Vector3.one;
        }

        public override Vector3 GetMinPos()
        {
            return meshRndr != null ? meshRndr.bounds.min : Vector3.zero;
        }

        public override Vector3 GetMaxPos()
        {
            return meshRndr != null ? meshRndr.bounds.max : Vector3.zero;
        }

        public override float GetTimeForRatio(float ratio) { return 0f; }
        public override void UpdateModel(Frame frame) { }

        public override bool IsReady()
        {
            return (meshRndr != null);
        }

        public void AutoFindMeshRenderer()
        {
            if (meshRndr == null)
                meshRndr = GetComponentInChildren<MeshRenderer>();
        }
    }
}
