using System;

namespace SBS
{
    [Serializable]
    public class TrimProperty : PropertyBase
    {
        public bool on = true;
        public int spriteMargin = 2;
        public bool useUnifiedSize = false;
        public bool allUnified = false;
        public bool pivotSymmetrically = false;

        public TrimProperty CloneForBaking(bool isSingleStaticModel)
        {
            TrimProperty clone = new TrimProperty();
            clone.on = false;
            clone.spriteMargin = spriteMargin;
            clone.useUnifiedSize = false;
            clone.allUnified = false;
            clone.pivotSymmetrically = false;
            
            if (on)
            {
                clone.on = true;

                if (useUnifiedSize)
                {
                    clone.useUnifiedSize = true;
                    if (!isSingleStaticModel)
                        clone.allUnified = allUnified;
                    clone.pivotSymmetrically = pivotSymmetrically;
                }
            }

            return clone;
        }
    }
}
