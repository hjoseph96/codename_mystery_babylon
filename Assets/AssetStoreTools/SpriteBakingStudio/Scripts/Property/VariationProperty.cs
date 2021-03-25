using System;
using UnityEngine;

namespace SBS
{
    [Serializable]
    public class VariationProperty : PropertyBase
    {
        public bool on = false;
        public Color tintColor = new Color(1f, 0f, 0f, .5f);
        public BlendFactor tintBlendFactor = BlendFactor.SrcAlpha;
        public BlendFactor imageBlendFactor = BlendFactor.OneMinusSrcAlpha;
        public bool excludeShadow = true;
        public BlendFactor bodyBlendFactor = BlendFactor.One;
        public BlendFactor shadowBlendFactor = BlendFactor.OneMinusSrcAlpha;
    }
}
