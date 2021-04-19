using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _friendlyBattler;
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _hostileBattler;
    
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private GameObject _playerForeground;
    
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _enemyBattlerSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _playerBattlerSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _playerMagicCircleSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _enemyMagicCircleSpawnPoint;


    [FoldoutGroup("Cameras")]
    [SerializeField] public Camera BattleCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _battleUICamera;
     [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _pixelationCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private float _transitionSpeed = 0.5f;
    private ProCamera2DTransitionsFX _battleTransitionFX;

    [FoldoutGroup("Battle Status")]
    private CombatPhase _phase = CombatPhase.NotInCombat;
    [ShowInInspector] public CombatPhase Phase { get =>_phase; }


    private BattleHUD _enemyHUD;
    private BattleHUD _playerHUD;
    private ExperienceBar _expBarUI;
    private PostEffectMask _pixelateEffectMask;

    private Vector3 _platformOriginalPosition;

    private Battler _attackingBattler;
    private Battler _defendingBattler;

    private bool _beganAttacks = false;
    private bool _gainedExp = false;
    private bool _transitionedOut = false;


    private void Start()
    {
        var uiManager = UIManager.Instance;
        _enemyHUD   = uiManager.BattleSceneCanvas.EnemyHUD;
        _playerHUD  = uiManager.BattleSceneCanvas.PlayerHUD;
        _expBarUI   = uiManager.BattleSceneCanvas.ExpBar;
    }


    public async void Load(Unit attacker, Unit defender)
    {
        UIManager.Instance.BattleSceneCanvas.SetCamera(_battleUICamera);
        UIManager.Instance.BattleSceneCanvas.Enable();
        
        _battleUICamera.enabled = false;
        _battleUICamera.enabled = true;

        _pixelationCamera.enabled = false;
        _pixelationCamera.enabled = true;
        
        // TODO: Remove once pixel battle animations are completed
        _pixelateEffectMask = _pixelationCamera.GetComponent<PostEffectMask>();

        await SetupBattlers(attacker, defender);

        // State Boolean Flags
        _beganAttacks   = false;
        _gainedExp      = false;
        _transitionedOut = false;

        _platformOriginalPosition = _playerForeground.transform.position;
        _battleTransitionFX = BattleCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();
        _battleTransitionFX.OnTransitionEnterStarted += delegate () {
            _phase = CombatPhase.Transition;
            
            SetToTransitionPosition(_friendlyBattler.transform);
            SetToTransitionPosition(_playerForeground.transform);
            SetToTransitionPosition(_hostileBattler.transform, 4f);
        };
        
        _battleTransitionFX.TransitionEnter();
    }

    public async Task<Dictionary<string, bool>> BattleResults(Unit attacker, Unit defender)
    {
        Dictionary<string, bool> battleResults = new Dictionary<string, bool>();
        
        // If the attacking cannot defend himself, return empty
        if (!attacker.CanDefend())
            return battleResults;

        var hitResults = await TrueRandomUtility.HitResults(attacker, defender);
        var critResults = await TrueRandomUtility.CriticalHitResults(attacker, defender);

        // Merge the dictionaries
        return hitResults.Concat(critResults)
            .ToLookup(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, g => g.First());        
    }

    // Update is called once per frame
    void Update()
    {
        switch (_phase) {
            case CombatPhase.NotInCombat:
                break;
            case CombatPhase.Transition:
                ProcessTransitionPhase();
                break;
            case CombatPhase.Attacking:
                ProcessAttackingPhase();
                break;
            case CombatPhase.GainExperience:
                StartCoroutine(ProcessGainExperiencePhase());
                break;
            case CombatPhase.Complete:
                StartCoroutine(BackToMap());
                break;
        }

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
            StartCoroutine(BackToMap());
        #endif
    }

    private void ProcessTransitionPhase()
    {
        bool friendlyReached = ReachedPosition(_friendlyBattler.transform, _playerBattlerSpawnPoint.position);
        bool enemyReached = ReachedPosition(_hostileBattler.transform, _enemyBattlerSpawnPoint.position);
        bool platformReached = ReachedPosition(_playerForeground.transform, _platformOriginalPosition);  
        
        bool bothFightersReady = _friendlyBattler.IsReadyToFight && _hostileBattler.IsReadyToFight;
        if (friendlyReached && enemyReached && platformReached && bothFightersReady)
        {
            _friendlyBattler.Unit.UponDeath += HandleUnitDeath;
            _hostileBattler.Unit.UponDeath += HandleUnitDeath;

            _phase = CombatPhase.Attacking;
        }
    }

    private void ProcessAttackingPhase()
    {   
        if (!_beganAttacks)
        {
            _attackingBattler.OnAttackComplete += delegate() {
                _defendingBattler.OnAttackComplete += delegate() {
                    _phase = CombatPhase.GainExperience;
                };
                
                _defendingBattler.Attack(_attackingBattler);
            };

            _attackingBattler.Attack(_defendingBattler);
            _beganAttacks = true;
        }

        if (_attackingBattler.IsFinished && _defendingBattler.IsFinished)
            _phase = CombatPhase.GainExperience;
    }

    private IEnumerator ProcessGainExperiencePhase()
    {
        var friendlyUnit = _friendlyBattler.Unit;
        var hostileUnit = _hostileBattler.Unit;



        if (friendlyUnit.TeamId == Player.LocalPlayer.TeamId && !_gainedExp)
        {
            _gainedExp = true;

            _expBarUI.Show(friendlyUnit.Experience);

            var damageDealt = _friendlyBattler.DamageDealt;

            int expGained = ((31 + hostileUnit.Level + hostileUnit.Class.PromotedBonus) - (friendlyUnit.Level - friendlyUnit.Class.PromotedBonus)) / friendlyUnit.Class.RelativePower;
            if (damageDealt <= 0)
                expGained = 1;


            // [EXP from doing damage] + [Silencer factor] × [Enemy's level × Enemy Class relative power + Enemy class bonus - 
            // ([Player's level × Player Class relative power + Player class bonus] / Mode divisor) + 20 + Thief bonus + Boss bonus + Entombed bonus
            if (_hostileBattler.Unit.CurrentHealth == 0)
            {
                var enemyExpCalc = (hostileUnit.Level * hostileUnit.Class.RelativePower + hostileUnit.Class.PromotedBonus);
                var playerExpCalc = (friendlyUnit.Level * friendlyUnit.Class.RelativePower + friendlyUnit.Class.PromotedBonus);

                // TODO: replace 2 with mode divisor, add boss units
                expGained += ((enemyExpCalc - playerExpCalc) / 2) + 20;
            }
            
            _expBarUI.OnBarFilled  += delegate() {
                friendlyUnit.GainExperience(expGained);

                
                // TODO: Logic for level up display
                _phase = CombatPhase.Complete;
            };

            yield return new WaitForSeconds(2f);

            _expBarUI.StartFilling(friendlyUnit.Experience + expGained);
        }
    }

    private IEnumerator BackToMap()
    {
        yield return new WaitForSeconds(1f);

        _battleTransitionFX.OnTransitionExitEnded += delegate() {
            var attackingUnit = _attackingBattler.Unit;
            var defendingUnit = _defendingBattler.Unit;
            attackingUnit.UponDeath = null;
            defendingUnit.UponDeath = null;

            _attackingBattler.OnAttackComplete = null;
            _defendingBattler.OnAttackComplete = null;
            _battleTransitionFX.OnTransitionEnterStarted = null;
            
            if (_friendlyBattler)
                Destroy(_friendlyBattler.gameObject);
            if (_hostileBattler)        
                Destroy(_hostileBattler.gameObject);

            _friendlyBattler = null;
            _hostileBattler = null;

            CampaignManager.Instance.SwitchToMap(attackingUnit, defendingUnit);
            _playerForeground.transform.position = _platformOriginalPosition;

            _phase = CombatPhase.NotInCombat;            
        };

        if (!_transitionedOut)
        {
            _transitionedOut = true;

            UIManager.Instance.BattleSceneCanvas.Disable();

            _battleTransitionFX.TransitionExit();
        }
    }

    private void HandleUnitDeath(Unit deadUnit)
    {
        var unitClass = deadUnit.GetType();
        if (unitClass.IsSubclassOf(typeof(AIUnit)))
        {
            CampaignManager.Instance.OnCombatReturn += delegate ()
            {
                CampaignManager.Instance.RemoveUnit(deadUnit);
            };

            // TODO: Check for any special dialogue or events to happen upon specific unit's death
        }
    }
    
    async Task<bool> SetupBattlers(Unit attacker, Unit defender)
    {
        _attackingBattler = await AssignBattlerByTeam(attacker, defender);
        _defendingBattler = await AssignBattlerByTeam(defender, attacker);

        return true;
    }


    async Task<Battler> AssignBattlerByTeam(Unit unit, Unit opposingUnit)
    {
        Battler newBattler;

        Transform parent = _playerBattlerSpawnPoint;
        bool isFriendly = false;

        switch(unit.TeamId) {
            case Team.LocalPlayerTeamId: case Team.AllyTeamId: case Team.NeutralTeamId:
                isFriendly = true;
                break;
            case Team.EnemyTeamId: case Team.OtherEnemyTeamId:
                isFriendly = false;
                parent = _enemyBattlerSpawnPoint;
                break;
        }

        var battlerObject = Instantiate( unit.BattlerPrefab, parent.position, unit.BattlerPrefab.transform.rotation, parent );
        newBattler = battlerObject.GetComponent<Battler>();

        var battleResults = await BattleResults(unit, opposingUnit);
        if (isFriendly)
        {
            newBattler.Setup(unit, _playerHUD, battleResults, _pixelateEffectMask);
            _friendlyBattler = newBattler;

            // TODO: Refactor
            if (newBattler is Magician)
            {
                var magician = newBattler as Magician;
                magician.SetMagicCircleSpawnPoint(_playerMagicCircleSpawnPoint);
            }
        }
        else {
            newBattler.Setup(unit, _enemyHUD, battleResults, _pixelateEffectMask);
            _hostileBattler = newBattler;
            
            // TODO: refactor
            if (newBattler is Magician)
            {
                var magician = newBattler as Magician;
                magician.SetMagicCircleSpawnPoint(_enemyMagicCircleSpawnPoint);
            }
        }


        return newBattler;
    }


    private void SetToTransitionPosition(Transform objTransform, float amount = -4f)
    {
        var transitionStartPos =  new Vector3(
            objTransform.position.x + amount, objTransform.position.y, objTransform.transform.position.z
        );
        
        objTransform.position = transitionStartPos;
    }

    private bool ReachedPosition(Transform objTransform, Vector3 originalPosition)
    {
        objTransform.position = Vector3.Slerp(objTransform.transform.position, originalPosition, (_transitionSpeed * Time.deltaTime));

        if (Vector3.Distance(objTransform.position, originalPosition) < .03f)
        {
            objTransform.position = originalPosition;
            return true;
        }

        return false;
    }
}
