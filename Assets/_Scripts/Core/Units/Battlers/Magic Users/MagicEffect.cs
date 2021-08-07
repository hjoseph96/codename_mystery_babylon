using System;
using System.Collections;
using UnityEngine;

using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;

public enum MagicEffectType {
    Projectile,
    Area
}

public class MagicEffect : MonoBehaviour
{
    [ShowInInspector] public MagicEffectType EffectType;
    [ShowInInspector] public bool ApplyPostProcessing;
    [ShowIf("IsProjectile")] public float moveSpeed = 5f;
    [ShowIf("IsProjectile")] public GameObject hitEffect;


    private bool IsProjectile => EffectType == MagicEffectType.Projectile;
    private bool IsArea => EffectType == MagicEffectType.Area;

    [SoundGroup] public string movingSound;
    [SoundGroup, ShowIf("IsProjectile")] public string hitSound;
    [HideInInspector] public Action OnHitTarget;
    [HideInInspector] public Action<Battler> UponEffectActive;

    private bool _isActive = false;
    public bool IsActive => _isActive;

    private Vector3 _target;
    private Collider _collider;
    private bool _startedMoving = false;

    protected virtual void Start() 
    {
        _collider = GetComponent<Collider>();
    
        // play spell sfx on spawn for area spells.
        if (EffectType == MagicEffectType.Area)
            MasterAudio.PlaySound3DFollowTransform(movingSound, CampaignManager.AudioListenerTransform);

        _isActive = true;
    }

    protected virtual void Update()
    {
        if (_startedMoving)
            TravelToTarget();
    }

    public virtual IEnumerator PostProcessWhileActive(Battler target)
    {
        throw new Exception("[MagicEffect] You've enabled ApplyPostProcessing but did not override this method!");
        /*while (IsActive)
        {
            // do something
        }*/
    }

    public void StartMoving(Vector3 target)
    {
        _target = target;
        _startedMoving = true;
        MasterAudio.PlaySound3DFollowTransform(movingSound, CampaignManager.AudioListenerTransform);
    }
    public void TravelToTarget()
    {
        this.transform.LookAt(_target, Vector3.up);
        this.transform.position = Vector3.Slerp(
            this.transform.position,
            _target,
            moveSpeed * Time.deltaTime
        );
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        
        _collider.enabled = false;
        Destroy(this.gameObject);
        _isActive = false;
        
        if (EffectType == MagicEffectType.Projectile)
        {
            Instantiate(hitEffect, this.transform.position, hitEffect.transform.rotation);
            MasterAudio.PlaySound3DFollowTransform(hitSound, CampaignManager.AudioListenerTransform);
        }

        OnHitTarget?.Invoke();
    }

    private void HitTarget()
    {
        Destroy(this.gameObject);
        _isActive = false;

        OnHitTarget?.Invoke();
    }

}
