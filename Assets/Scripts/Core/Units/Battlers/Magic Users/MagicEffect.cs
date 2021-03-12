using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public enum MagicEffectType {
    Projectile,
    Area
}

public class MagicEffect : MonoBehaviour
{
    public MagicEffectType EffectType;
    public float moveSpeed = 5f;

    private Transform _target;
    private bool _startedMoving = false;

    
    public void StartMoving(Transform target)
    {
        _target = target;
        _startedMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_startedMoving)
            TravelToTarget();
    }

    public void TravelToTarget()
    {
        switch(EffectType)
        {
            case MagicEffectType.Area:
                // Spawn Directly in target's area

                break;
            case MagicEffectType.Projectile:
                // Slerp to target's Transform and Look at target's transform;
                
                break;
        }
    }
}
