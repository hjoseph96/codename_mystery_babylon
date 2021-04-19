using System;
using System.Linq;
using System.Text.RegularExpressions;
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

    private PostEffectMaskRenderer _pixelateShaderRenderer;
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
    
    [FoldoutGroup("Animation Overrides")]
    [SerializeField] private Dictionary<string, float> animationRotations = new Dictionary<string, float>();
    private float _defaultRotation;


    [FoldoutGroup("Audio")]
    [SoundGroupAttribute] public string hitImpactSound; 


    private bool _readyToFight;
    private BattlerState _state;
    [FoldoutGroup("Battler State")]
    [ShowInInspector] public BattlerState State { get { return _state; } }
    [FoldoutGroup("Battler State")]
    [ShowInInspector] public bool IsReadyToFight { get { return _readyToFight; } }
    [FoldoutGroup("Battler State")]
    [ReadOnly] protected Battler targetBattler;
    [FoldoutGroup("Battler State")]
    [ReadOnly] private int _timesHit = 0;
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
    private string _previousAnim;

    private List<Renderer> _renderers;
    private bool _isDead = false;
    private bool _startedDissolving = false;
    private float _dissolveStartTime;
    [SerializeField] private float _dissolveSpeed = 0.68f;


    private void Awake() =>_renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());

    public virtual void Setup(Unit unit, BattleHUD hud, Dictionary<string, bool> battleResults, PostEffectMask pixelShaderMask)
    {
        Unit = unit;
        HUD = hud;
        BattleResults = battleResults;

        _pixelateShaderRenderer = GetComponent<PostEffectMaskRenderer>();
        _pixelateShaderRenderer.mask = pixelShaderMask;
        _defaultRotation = transform.eulerAngles.y;
        Animator = GetComponent<Animator>();
        currentAttackIndex = 0;

        _state = BattlerState.Idle;

        SetupAttacks();

        HUD.Populate(Unit);

        Unit.UponDeath  += delegate(Unit unit) {
            _state = BattlerState.Dead;
        };
    }

    public bool Attack(Battler target)
    {
        targetBattler = target;
        
        if (Unit.CanAttack(target.Unit))
        {
            if (_state == BattlerState.Dead)
                return false;
            
            _state = BattlerState.Attacking;

            return true;
        } 
        else {
            if (State != BattlerState.Dead)
                FinishFighting();
            
            return false;
        }
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
                if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !IsFinished)
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
                throw new System.Exception("Unsupported BattlerState: " + _state);
        }
    }

    protected virtual void ProcessAttackingState()
    {
        if (!currentlyAttacking)
        {
            var attackType = GetAttackType();
            string chosenAnimation;

            switch(attackType)
            {
                case AttackType.Normal:
                    chosenAnimation = GetAnimVariation(NormalAttackAnims());
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Critical:
                    chosenAnimation = GetAnimVariation(CriticalAttackAnims());
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Multiple:
                    if (!IsMultiAttacking())
                    {
                        chosenAnimation = GetAnimVariation(DoubleAttackAnims());
                        PlayAnimation(chosenAnimation);
                        currentlyAttacking = true;
                    }

                    break;
            }
        }
    }

    protected virtual void ProcessDeadState()
    {
        if (!_isDead)
        {
            PlayAnimation("Death");
            _isDead = true;
            IsFinished = false;
        }

        if (_startedDissolving)
        {
            float dissolveEnd = 0.57f;
            foreach(Renderer renderer in _renderers)
            {
                float dissolveAmount = AnimationCurve.Linear(_dissolveStartTime, 0f, _dissolveStartTime + _dissolveSpeed, dissolveEnd + 0.01f).Evaluate(Time.time);

                renderer.material.SetFloat("_DissolveCutoff", dissolveAmount);

                if (dissolveAmount >= dissolveEnd)
                    renderer.enabled = false;
            }
        }

        if (_renderers.All(renderer => !renderer.enabled ))
        {
            _startedDissolving = false;
            IsFinished = true;
        }
    }

    protected string GetAnimVariation(List<string> options)
    {
        var animVariations = new List<String>(options);
        if (options.Contains(_previousAnim))    
            animVariations.Remove(_previousAnim);
        
        int choice = UnityEngine.Random.Range(0, animVariations.Count -1);
        
        return animVariations[choice];
    }

    protected void PlayAnimation(string stateName)
    {
        try
        {
            Animator.Play(AnimationsAsHashes[stateName]);
            _previousAnim = stateName;

            // Override Rotation for certain anims
            if (animationRotations.Keys.Contains(stateName))
                SetRotation(animationRotations[stateName]);
            else if (transform.rotation.y != _defaultRotation)
                SetRotation(_defaultRotation);
        }
        catch (Exception e)
        {

            Debug.LogError("Exception In Battler.cs > PlayAnimation with StateName " + stateName + " : " + e.Message);
        }
       
    }

    private void SetRotation(float yRotation)
    {
        Vector3 newRotation = new Vector3(
            transform.rotation.x, yRotation, transform.rotation.z
        );
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotation), 25f * Time.smoothDeltaTime);
    }

    protected bool IsMultiAttacking()
    {
        var multiAttacks = DoubleAttackAnims();

        return multiAttacks.Any( (anim) => Animator.GetCurrentAnimatorStateInfo(0).IsName(anim) );
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
        if (_attacks.Count >= 2)
            return AttackType.Multiple;
        
        if (CurrentAttack.IsCritical)
            return AttackType.Critical;
        
        return AttackType.Normal;
    }
    
    public void HitReaction()
    {
        var chosenAnimation = GetAnimVariation(HitReactionAnims());
        PlayAnimation(chosenAnimation);
        
        MasterAudio.PlaySound3DAtTransform(hitImpactSound, CampaignManager.AudioListenerTransform);

        _state = BattlerState.HitReaction;
        _timesHit += 1;
    }

    private void BackToIdle() => _state = BattlerState.Idle;

    public void Dodge() 
    {
        PlayAnimation("Dodge");
        _state = BattlerState.Dodging;
    }

    private void StartBlocking()
    {
        PlayAnimation("Start Blocking");
        _state = BattlerState.Blocking;
    }

    private void BlockImpact() => PlayAnimation("Block Impact");

    private void WeaponMeleeSound()
    {
        MasterAudio.PlaySound3DFollowTransform(Unit.EquippedWeapon.MeleeSound, CampaignManager.AudioListenerTransform);
    }

    private void StartDissolving()
    {
        _startedDissolving = true;
        _dissolveStartTime = Time.time;
    }

    private void ReadyToFight() => _readyToFight = true;

    private void ReceiveDamage(int damageAmount)
    {
        _damageReceived += damageAmount;
        HUD.DecreaseHealth(damageAmount);

        HitReaction();
    }


    protected virtual void NextAttack()
    {
        bool targetDead = CurrentAttack.Landed && CurrentAttack.Damage(targetBattler.Unit) > targetBattler.Unit.CurrentHealth;
        if (targetDead || currentAttackIndex == _attacks.Count - 1)
            FinishFighting();
        else
            currentAttackIndex += 1;
        
        currentlyAttacking = false;
    }

    protected void FinishFighting()
    {
        _state = BattlerState.Idle;
            
        if (targetBattler.State == BattlerState.Blocking)
            targetBattler.BackToIdle();

        IsFinished = true;
        OnAttackComplete.Invoke();
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
                    DamageDealt += targetBattler.Unit.CurrentHealth;
                    
                targetBattler.ReceiveDamage(attackDamage);
            }   
            else
                targetBattler.BlockImpact();
        }
        else
            targetBattler.Dodge();
    }

    private void BeginAttack()
    {
        currentlyAttacking = true;

        var attackDamage = CurrentAttack.Damage(targetBattler.Unit);

        if (attackDamage == 0)
            targetBattler.StartBlocking();
    }

    protected List<string> NormalAttackAnims()
    {
        var regexp = new Regex(@"^Attack \d\d");
        var normalAttackAnimNames = new List<string>();

        foreach(string animName in AnimationsAsHashes.Keys)
            if (regexp.Match(animName).Success)
                normalAttackAnimNames.Add(animName);

        return normalAttackAnimNames;
    }

    protected List<string> CriticalAttackAnims()
    {
        var regexp = new Regex(@"^Critical Attack \d\d");
        var critAttackAnimNames = new List<string>();

        foreach(string animName in AnimationsAsHashes.Keys)
            if (regexp.Match(animName).Success)
                critAttackAnimNames.Add(animName);

        return critAttackAnimNames;
    }

    
    protected List<string> DoubleAttackAnims()
    {
        var regexp = new Regex(@"^Double Attack \d\d");
        var doubleAttackAnimNames = new List<string>();

        foreach(string animName in AnimationsAsHashes.Keys)
            if (regexp.Match(animName).Success)
                doubleAttackAnimNames.Add(animName);

        return doubleAttackAnimNames;
    }

    
    protected List<string> HitReactionAnims()
    {
        var regexp = new Regex(@"^Damage \d\d");
        var hitReactionAnimNames = new List<string>();

        foreach(string animName in AnimationsAsHashes.Keys)
            if (regexp.Match(animName).Success)
                hitReactionAnimNames.Add(animName);

        return hitReactionAnimNames;
    }
}
