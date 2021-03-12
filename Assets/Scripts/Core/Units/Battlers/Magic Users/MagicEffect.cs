using System;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public enum MagicEffectType {
    Projectile,
    Area
}

public class MagicEffect : MonoBehaviour
{
    public MagicEffectType EffectType;
    public float moveSpeed = 5f;
    public GameObject hitEffect;
    [SoundGroupAttribute] public string hitSound;
    [HideInInspector] public Action OnHitTarget;

    private Transform _target;
    private bool _startedMoving = false;

    
    public void StartMoving(Transform target)
    {
        _target = target;
        _startedMoving = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_startedMoving)
            TravelToTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EffectType == MagicEffectType.Projectile)
        {
            Destroy(this.gameObject);
            var shownHitEffect = Instantiate(hitEffect, this.transform.position, hitEffect.transform.rotation).GetComponent<ParticleSystem>();
            Destroy(shownHitEffect, shownHitEffect.main.startLifetime.constant + shownHitEffect.main.duration);

            OnHitTarget.Invoke();
        }       
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
                this.transform.LookAt(_target, Vector3.up);
                this.transform.position = Vector3.Slerp(
                    this.transform.position,
                    _target.position,
                    moveSpeed * Time.deltaTime
                );

                break;
        }
    }
}
