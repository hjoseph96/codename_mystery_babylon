using UnityEngine;
using UnityEditor;

namespace SS
{
    [InitializeOnLoad]
    public class Autorun
    {
        #if UNITY_EDITOR
        static Autorun()
        {
        }
        #endif
    }
}