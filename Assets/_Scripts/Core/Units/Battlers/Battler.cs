using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
 #if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;

public class Battler : SerializedMonoBehaviour
{
    [ReadOnly] protected string Name;
    [FoldoutGroup("Battle HUD")]
    [SerializeField] protected BattleHUD HUD;

    public Unit Unit { get; private set; }
    [HideInInspector] public Action OnAttackComplete;

    protected Dictionary<string, bool> BattleResults = new Dictionary<string, bool>();

    protected Animator Animator;

    [FoldoutGroup("Animation Overrides")]
    public Dictionary<string, int> AnimationsAsHashes = new Dictionary<string, int>();
    
    #if UNITY_EDITOR
    [FoldoutGroup("Animation Overrides"), PropertyOrder(0)]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void SetAnimationStateNames()   // Get State Names from Animator in Editor, turn to hashes
    {
        AnimationsAsHashes = new Dictionary<string, int>();

        AnimatorController ac = GetComponent<Animator>().runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine sm = ac.layers[0].stateMachine;
        ChildAnimatorState[] states = sm.states;
        foreach (ChildAnimatorState s in states)
            AnimationsAsHashes.Add(s.state.name, Animator.StringToHash(s.state.name));
    }
    #endif

    [FoldoutGroup("Audio")]
    [SoundGroup] public string hitImpactSound; 


    private bool _readyToFight;
    private BattlerState _state;
    [FoldoutGroup("Battler State")]
    [ShowInInspector] public BattlerState State { get { return _state; } }
    [FoldoutGroup("Battler State")]
    [ShowInInspector] public bool IsReadyToFight { get { return _readyToFight; } }
    [FoldoutGroup("Battler State")]
    [ReadOnly] protected Battler targetBattler;
    [FoldoutGroup("Battler State")]
    [ReadOnly] public int DamageDealt;
    [FoldoutGroup("Battler State")]
    [ReadOnly] private int _damageReceived;
    [FoldoutGroup("Battler State")]
    [ReadOnly] protected int currentAttackIndex;
    [FoldoutGroup("Battler State")]
    [ReadOnly] private List<Attack> _attacks = new List<Attack>();
    public List<Attack> Attacks { get => _attacks; }

    protected Attack CurrentAttack => _attacks[currentAttackIndex];

    protected bool currentlyAttacking = false;
    public bool IsFinished { get; private set; }

    protected bool isAnimating;
    public bool IsAnimating { get => isAnimating; }
    

    protected string previousAnim;
    protected Vector2 startingPoint;

    private SpriteRenderer _renderer;
    private Material _allInOneMat;

    protected bool _isDead = false;
    public bool IsDead { get => _isDead; }

    public bool _hasDied = false;

    private bool _startedDissolving = false;
    private float _dissolveStartTime;
    [SerializeField] private float _dissolveSpeed = 0.68f;



    public virtual void Setup(Unit unit, BattleHUD hud, Dictionary<string, bool> battleResults)
    {
        Unit            = unit;
        HUD             = hud;
        BattleResults   = battleResults;

        Animator        = GetComponent<Animator>();
        Animator.speed = GlobalVariables.Instance.gameSpeed;
        _renderer       = GetComponent<SpriteRenderer>();
        _allInOneMat    = _renderer.material;

        _state = BattlerState.Idle;

        currentAttackIndex = 0;
        SetupAttacks();

        HUD.Populate(Unit);

        startingPoint = transform.position;

        Unit.UponDeath  += delegate(Unit unit) {
            _state = BattlerState.Dead;
            IsFinished = true;
            OnAttackComplete?.Invoke();
        };
    }

    public bool Attack(Battler target)
    {
        targetBattler = target;

        if (isAnimating)
        {
            StartCoroutine(WaitUntilDoneAnimating(target));
            return false;
        }

        if (Unit.CanAttack(target.Unit))
        {
            if (_state == BattlerState.Dead || target.IsDead)
                return false;
            
            _state = BattlerState.Attacking;

            return true;
        } 
        else {
            if (State != BattlerState.Dead)
                StartCoroutine(WaitForReaction(FinishFighting));
            
            return false;
        }
    }

    private IEnumerator WaitUntilDoneAnimating(Battler target)
    {
        yield return new WaitUntil(() => !isAnimating);

        if (!target.IsDead)
            Attack(target);
        else
            FinishFighting();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Animator == null)
            return;
        

        switch(_state)
        {
            case BattlerState.Idle:
                PlayAnimation("Idle");
                
                break;
            case BattlerState.Attacking:
                // Only attack from Idle Animation
                if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !IsFinished && !HUD.IsCalculating)
                    ProcessAttackingState();
                
                break;
            case BattlerState.Casting:
                break;
            case BattlerState.HitReaction:
                break;
            case BattlerState.Blocking:
                break;
            case BattlerState.Dodging:
                break;
            case BattlerState.Dead:
                ProcessDeadState();
                break;
            default:
                throw new Exception("Unsupported BattlerState: " + _state);
        }

        var animInfo = Animator.GetCurrentAnimatorStateInfo(0);
        if (animInfo.IsName("Idle") || animInfo.IsName("Dead"))
            isAnimating = false;
        else
            isAnimating = true;

    }

    protected virtual void ProcessAttackingState()
    {
        if (!currentlyAttacking)
        {
            var attackType = GetAttackType();

            switch(attackType)
            {
                case AttackType.Normal:
                    PlayAnimation("Attack");
                    currentlyAttacking = true;

                    break;
                case AttackType.Critical:
                    PlayAnimation("Critical Attack");
                    currentlyAttacking = true;

                    break;
            }
        }
    }

    protected virtual void ProcessDeadState()
    {
        if (IsDead && !_hasDied)
        {
            PlayAnimation("Death");
            _hasDied = true;
            IsFinished = false;
        }

        if (_startedDissolving)
        {
            float t = (Time.time - _dissolveStartTime) / _dissolveSpeed;
            var fadeAmount = Mathf.SmoothStep(0, 1, t);

            _allInOneMat.SetFloat("_FadeAmount", fadeAmount);

            if (fadeAmount == 1)
            {
                _startedDissolving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void PlayAnimation(string stateName, bool replay = false)
    {
        try
        {
            Animator.Play(AnimationsAsHashes[stateName]);

            previousAnim = stateName;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Battler#{this.gameObject.name}]  PlayAnimation with StateName {stateName}: {e.Message}");
        }
       
    }

    // TODO, support multiple attacks. right now, only hardcoded supports 2, 
    // want a max of 4-6, though 2 attacks will be the norm
    private void SetupAttacks()
    {
        if (BattleResults.Count == 0)   // Cannot Attack.
            return;
        
        var firstAttack = new Attack(Unit, BattleResults["HIT"], BattleResults["CRITICAL"]);
        _attacks.Add(firstAttack);
        
        if (BattleResults["DOUBLE_ATTACK"])
        {
            var secondAttack = new Attack(Unit, BattleResults["SECOND_HIT"], BattleResults["CRIT_SECOND_HIT"]);
            _attacks.Add(secondAttack);
        }
    }

    protected AttackType GetAttackType()
    {
        if (CurrentAttack.IsCritical)
            return AttackType.Critical;
        
        return AttackType.Normal;
    }
    
    public void HitReaction()
    {
        PlayAnimation("Hit Reaction", true);
        
        MasterAudio.PlaySound3DAtTransform(hitImpactSound, CampaignManager.AudioListenerTransform);

        _state = BattlerState.HitReaction;
    }


    public void Dodge() 
    {
        PlayAnimation("Dodge");
        _state = BattlerState.Dodging;
    }

    public void StartBlocking()
    {
        PlayAnimation("Block");
        _state = BattlerState.Blocking;
    }


    private void ReceiveDamage(int damageAmount)
    {
        _damageReceived += damageAmount;
        HUD.DecreaseHealth(damageAmount);

        HitReaction();
    }


    protected virtual void NextAttack()
    {
        bool targetDead = CurrentAttack.Landed && CurrentAttack.Damage(targetBattler.Unit) >= targetBattler.Unit.CurrentHealth;
        if (targetDead || currentAttackIndex == _attacks.Count - 1)
        {
            BackToIdle();
            StartCoroutine(WaitForReaction(FinishFighting));
        }
        else
            currentAttackIndex += 1;
        
        currentlyAttacking = false;
    }

    protected IEnumerator WaitForReaction(Action onComplete)
    {
        yield return new WaitUntil(() => !targetBattler.IsAnimating);

        onComplete?.Invoke();
    }

    protected void FinishFighting()
    {
        _state = BattlerState.Idle;

        IsFinished = true;
        OnAttackComplete?.Invoke();
    }

    protected void ProcessAttack()
    {
        var attackDamage = CurrentAttack.Damage(targetBattler.Unit);

        if (CurrentAttack.Landed)
        {
            if (attackDamage > 0)
            {
                if (attackDamage <= targetBattler.Unit.CurrentHealth)
                    DamageDealt += attackDamage;
                else
                {
                    targetBattler._isDead = true;
                    DamageDealt += targetBattler.Unit.CurrentHealth;
                }
                    
                targetBattler.ReceiveDamage(attackDamage);
            }   
            else
                targetBattler.StartBlocking();
        }
        else
            targetBattler.Dodge();
    }

    #region Animation Events
    private void BackToIdle() => _state = BattlerState.Idle;

    private void WeaponMeleeSound()
    {
        MasterAudio.PlaySound3DFollowTransform(Unit.EquippedWeapon.MeleeSound, CampaignManager.AudioListenerTransform);
    }

    private void TriggerTargetHitReaction()
    {
        if (CurrentAttack.Landed)
            targetBattler.HitReaction();
        else
            targetBattler.Dodge();
    }

    private void StartDissolving()
    {
        _startedDissolving = true;
        _dissolveStartTime = Time.time;
    }

    private void ReadyToFight() => _readyToFight = true;

    private void BeginAttack()
    {
        currentlyAttacking = true;
        
    }
    #endregion

}
