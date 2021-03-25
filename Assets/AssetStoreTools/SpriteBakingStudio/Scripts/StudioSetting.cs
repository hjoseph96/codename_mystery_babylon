using System;
using UnityEngine;

namespace SBS
{
    [Serializable]
    public class StudioSetting
    {
        public ModelProperty model = new ModelProperty();
        public CameraProperty camera = new CameraProperty();
        public LightProperty light = new LightProperty();
        public ViewProperty view = new ViewProperty();
        public ShadowProperty shadow = new ShadowProperty();
        public ExtractProperty extract = new ExtractProperty();
        public VariationProperty variation = new VariationProperty();
        public PreviewProperty preview = new PreviewProperty();
        public FrameProperty frame = new FrameProperty();
        public TrimProperty trim = new TrimProperty();
        public OutputProperty output = new OutputProperty();
        public PathProperty path = new PathProperty();

        public int frameSamples = 20; // for animation clip
        public int spriteInterval = 1; // for animation clip

        public bool IsSkinnedModel()
        {
            return model.obj is StudioSkinnedModel;
        }

        public bool IsAnimatedModel()
        {
            return model.obj is StudioAnimatedModel;
        }

        public StudioAnimatedModel GetAnimatedModel()
        {
            return model.obj as StudioAnimatedModel;
        }

        public bool IsStaticModel()
        {
            return model.obj is StudioStaticModel || model.obj is StaticModelGroup;
        }

        public bool IsSingleStaticModel()
        {
            return model.obj is StudioStaticModel;
        }

        public bool IsStaticModelGroup()
        {
            return model.obj is StaticModelGroup;
        }

        public StaticModelGroup GetStaticModelGroup()
        {
            return model.obj as StaticModelGroup;
        }

        public bool IsParticleModel()
        {
            return model.obj is StudioParticleModel;
        }

        public StudioParticleModel GetParticleModel()
        {
            return model.obj as StudioParticleModel;
        }

        public bool IsTopView()
        {
            return view.slopeAngle == 90f;
        }

        public bool IsSideView()
        {
            return view.slopeAngle == 0f;
        }

        public bool IsDynamicRealShadow()
        {
            return shadow.type == ShadowType.Real && shadow.method == RealShadowMethod.Dynamic;
        }

        public bool IsStaticRealShadow()
        {
            return shadow.type == ShadowType.Real && shadow.method == RealShadowMethod.Static;
        }
    }
}
