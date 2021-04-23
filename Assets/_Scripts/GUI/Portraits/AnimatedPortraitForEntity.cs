using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedPortraitForEntity : MonoBehaviour
{
    public PortraitTarget PortraitTarget;
    public AnimatedPortrait AnimatedPortrait;

    public void Setup(PortraitTarget portraitTarget, AnimatedPortrait animatedPortrait)
    {
        PortraitTarget = portraitTarget;
        AnimatedPortrait = animatedPortrait;
    }
}