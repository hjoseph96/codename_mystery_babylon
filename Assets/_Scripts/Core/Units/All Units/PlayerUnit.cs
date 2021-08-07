using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Animancer;

public class PlayerUnit : Unit
{
    protected override Player Player { get; } = Player.LocalPlayer;

    protected override List<Vector2Int> ThreatDetectionRange() => GridUtility.GetReachableCells(this, -1, true).ToList();

    public override Vector2Int PreferredDestination { get => CampaignManager.Instance.PlayerDestination; }


    private Unit atkTarget;
    private AnimationEventReceiver _OnAttackLanded;

    public override void Init()
    {
        base.Init();
    }

    private void Update()
    {
        if (isDodging)
            StartCoroutine(DodgeMovement());
    }

        public List<AIUnit> Enemies()
    {
        var enemies = new List<AIUnit>();

        var campaignManager = CampaignManager.Instance;

        foreach (EnemyUnit enemy in campaignManager.EnemyUnits())
            enemies.Add(enemy);

        foreach (OtherEnemyUnit otherEnemy in campaignManager.OtherEnemyUnits())
            enemies.Add(otherEnemy);

        return enemies;
    }

    public List<AIUnit> Allies() => new List<AIUnit>(CampaignManager.Instance.AllyUnits());

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }

    public override void GainExperience(int amount = 100)
    {
        base.GainExperience(amount);
    }

    // Artur Specific
    public void DisplayAttackAnimation(Unit target)
    {
        atkTarget = target;

        var lookingDown = LookDirection.y < 0;
        if (!lookingDown)
            _renderer.sortingOrder = atkTarget.OrderInLayer + 1;

        var animations = _attackAnimations.CurrentAnimation();

        var clip = animations.GetClip(LookDirection);
        var state = _animancer.Play(clip);

        state.Events.OnEnd += delegate ()
        {
            SetIdle();
        };


        _OnAttackLanded.Set(state, HitReaction());
    }

    private void OnHitLanded(AnimationEvent animationEvent)
    {
        _OnAttackLanded.SetFunctionName("OnHitLanded");
        _OnAttackLanded.HandleEvent(animationEvent);
    }

    private Action<AnimationEvent> HitReaction()
    {
        return delegate (AnimationEvent animationEvent)
        {
            atkTarget.TakeDamage(atkTarget.CurrentHealth, false, true);
        };
    }
}