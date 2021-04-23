using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class AnimatedPortraitForEntity : MonoBehaviour
{
    [ReadOnly]
    public AnimatedPortrait AnimatedPortrait;

    [ReadOnly]
    public EntityType EntityType;

    [OdinSerialize, ReadOnly]
    public string Name;

    public static AnimatedPortraitForEntity Populate(EntityType entityType, string name, AnimatedPortrait animatedPortrait)
    {
        var animatedPortraitForEntity = new AnimatedPortraitForEntity();

        animatedPortraitForEntity.name              = name;
        animatedPortraitForEntity.EntityType        = entityType;
        animatedPortraitForEntity.AnimatedPortrait  = animatedPortrait;

        return animatedPortraitForEntity;
    }

    public void Setup(EntityType entityType, string name, AnimatedPortrait animatedPortrait)
    {
        Name                = name;
        EntityType          = entityType;
        AnimatedPortrait    = animatedPortrait;
    }
}