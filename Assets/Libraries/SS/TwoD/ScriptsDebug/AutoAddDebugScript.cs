using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class AutoAddDebugScript : MonoBehaviour
    {
        void Start()
        {
            SpriteAnimator sa = FindObjectOfType<SpriteAnimator>();
            if (sa != null)
            {
                sa.gameObject.AddComponent<DebugSpriteAnimatorController>();
            }
        }
    }  
}