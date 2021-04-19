using System;

namespace Tazdraperm_Utility
{
    static class MathUtility
    {
        public static int ClampInt(int val, int min, int max)
        {
            return Math.Min(Math.Max(val, min), max);
        }
    }
}
