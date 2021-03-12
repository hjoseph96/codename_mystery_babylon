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
    [SoundGroupAttribute] public string movingSound;
    [SoundGroupAttribute] public string hitSound;
    [HideInInspector] public Action OnHitTarget;

    private Vector3 _target;
    private bool _startedMoving = false;

    
    public void StartMoving(Vector3 target)
    {
        _target = target;
        _startedMoving = true;
        MasterAudio.PlaySound3DFollowTransform(movingSound, CampaignManager.AudioListenerTransform);
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
            var shownHitEffect = Instantiate(hitEffect, this.transform.position, hitEffect.transform.rotation).GetComponent<ParticleLifetimeEvents>();
            
            shownHitEffect.ParticleDied += delegate() {
                Destroy(shownHitEffect);
            };
            MasterAudio.PlaySound3DFollowTransform(hitSound, CampaignManager.AudioListenerTransform);

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
                    _target,
                    moveSpeed * Time.deltaTime
                );

                break;
        }
    }
}
