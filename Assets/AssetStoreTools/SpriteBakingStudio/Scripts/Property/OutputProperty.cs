using System;

namespace SBS
{
    public enum OutputType
    {
        Separately,
        SpriteSheet
    }

    public enum PackingAlgorithm
    {
        Optimized,
        InOrder
    }

    [Serializable]
    public class OutputProperty : PropertyBase
    {
        public OutputType type = OutputType.SpriteSheet;
        public PackingAlgorithm algorithm = PackingAlgorithm.Optimized;
        public int atlasSizeIndex = 4;
        public int spritePadding = 0;
        public bool allInOneAtlas = false;
        public bool loopAnimationClip = true;

        public OutputProperty CloneForBaking(bool isStaticModel)
        {
            OutputProperty clone = new OutputProperty();
            clone.type = type;
            clone.algorithm = algorithm;
            clone.atlasSizeIndex = atlasSizeIndex;
            clone.spritePadding = spritePadding;
            clone.allInOneAtlas = (type == OutputType.SpriteSheet && isStaticModel) ? allInOneAtlas : false;
            clone.loopAnimationClip = loopAnimationClip;
            return clone;
        }
    }
}
