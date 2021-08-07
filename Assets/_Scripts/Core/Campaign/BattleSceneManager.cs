using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class BattleSceneManager : MonoBehaviour
{
    private CombatPhase _phase = CombatPhase.NotInCombat;
    [ShowInInspector] public CombatPhase Phase { get => _phase; }
    
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _friendlyBattler;
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _hostileBattler;


    [FoldoutGroup("Battle Scene")]
    [Header("Battler Spawn Points")]
    [SerializeField] private Transform _enemyBattlerSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _playerBattlerSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [Header("Magic Circle Spawn Points")]
    [SerializeField] private Transform _playerMagicCircleSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _enemyMagicCircleSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [Header("Camera OnEnter Tween Targets")]
    [SerializeField] private Transform _cameraStartPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _cameraEndPoint;

    [FoldoutGroup("Cameras")]
    [SerializeField] public Camera BattleCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _battleUICamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _battlerCamera;

    [FoldoutGroup("Cameras")]
    [SerializeField] private float _transitionSpeed = 0.5f;
    private ProCamera2DTransitionsFX _battleTransitionFX;


    private BattleHUD _enemyHUD;
    private BattleHUD _playerHUD;
    private ExperienceBar _expBarUI;

    private Vector3 _platformOriginalPosition;

    private Battler _attackingBattler;
    private Battler _defendingBattler;

    private bool _cameraTweening = false;
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

    public IEnumerator Load(Unit attacker, Unit defender)
    {
        BattleCamera.transform.position = _cameraStartPoint.position;

        UIManager.Instance.BattleSceneCanvas.SetCamera(_battleUICamera);
        UIManager.Instance.BattleSceneCanvas.Enable();

        _battleUICamera.enabled = false;
        _battleUICamera.enabled = true;

        _battlerCamera.enabled = false;
        _battlerCamera.enabled = true;

        yield return SetupBattlers(attacker, defender);

        // State Boolean Flags
        _cameraTweening      = false;
        _beganAttacks       = false;
        _gainedExp          = false;
        _transitionedOut    = false;

        UIManager.Instance.BattleSceneCanvas.Show();

        _expBarUI.SetActive(false);


        _battleTransitionFX = BattleCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();
        _battleTransitionFX.OnTransitionEnterStarted += delegate () {
            _phase = CombatPhase.Transition;

            // SetToTransitionPosition(_friendlyBattler.transform);
            // SetToTransitionPosition(_hostileBattler.transform, 4f);
        };

        _battleTransitionFX.OnTransitionEnterEnded += delegate ()
        {
            _cameraTweening = true;
        };

        _battleTransitionFX.TransitionEnter();
    }


    // Update is called once per frame
    void Update()
    {
        switch (_phase)
        {
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
        // bool friendlyReached    = ReachedPosition(_friendlyBattler.transform, _playerBattlerSpawnPoint.position);
        // bool enemyReached       = ReachedPosition(_hostileBattler.transform, _enemyBattlerSpawnPoint.position);
        bool cameraReached      = ReachedPosition(BattleCamera.transform, _cameraEndPoint.position);

        bool bothFightersReady = _friendlyBattler.IsReadyToFight && _hostileBattler.IsReadyToFight;
        if (cameraReached && bothFightersReady)
        {
            _cameraTweening = false;

            _friendlyBattler.Unit.UponDeath += HandleUnitDeath;
            _hostileBattler.Unit.UponDeath += HandleUnitDeath;

            _phase = CombatPhase.Attacking;
        }
    }

    private void ProcessAttackingPhase()
    {
        BattleInCombatScene();
    }

    private void BattleInCombatScene()
    {
        if (!_beganAttacks)
        {
            _defendingBattler.OnAttackComplete += delegate () {
                _phase = CombatPhase.GainExperience;

                _attackingBattler.Unit.TookAction();
            };

            _attackingBattler.OnAttackComplete += delegate () {
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

            int expGained = BattleUtility.CalculateEXPGain(_friendlyBattler, _hostileBattler);
            int totalXp = friendlyUnit.Experience + expGained;

            _expBarUI.OnBarFilled += delegate () 
            {
                if(totalXp < 100)
                    _phase = CombatPhase.Complete;
                else if(friendlyUnit.IsAlive && totalXp >= 100)
                    LevelUpGUI.Instance.ShowLevelUpGUI(friendlyUnit);

                friendlyUnit.GainExperience(expGained);
            };

            yield return new WaitForSeconds(0.5f);
            _expBarUI.StartFilling(totalXp);
            yield return new WaitForSeconds(0.1f);

            //If total XP is enough for a level up, perform level up GUI effects if it was a player unit
            if (totalXp >= 100)
            {
                while (!LevelUpGUI.Instance.finishedDisplay)
                    yield return null;

                _phase = CombatPhase.Complete;
            }
        }
    }

    private IEnumerator BackToMap()
    {
        yield return new WaitForSeconds(0.5f);

        _battleTransitionFX.OnTransitionExitEnded += delegate () {
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

            LevelUpGUI.Instance.Hide();

            CampaignManager.Instance.SwitchToMap(attackingUnit, defendingUnit);

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
        if (deadUnit == null)
            return;

        if (deadUnit.Unkillable)
            CampaignManager.Instance.OnCombatReturn += delegate () { deadUnit.Incapacitate(); };
        else
            CampaignManager.Instance.OnCombatReturn += delegate () { deadUnit.Die(); };

    }

    private IEnumerator SetupBattlers(Unit attacker, Unit defender)
    {
        yield return AssignBattlerByTeam(attacker, defender);
        yield return AssignBattlerByTeam(defender, attacker, false);
    }


    private IEnumerator AssignBattlerByTeam(Unit unit, Unit opposingUnit, bool isAttacker = true)
    {
        Battler newBattler;

        Transform parent = _playerBattlerSpawnPoint;
        bool isFriendly = false;

        switch (unit.TeamId)
        {
            case Team.LocalPlayerTeamId:
            case Team.AllyTeamId:
            case Team.NeutralTeamId:
                isFriendly = true;
                break;
            case Team.EnemyTeamId:
            case Team.OtherEnemyTeamId:
                isFriendly = false;
                parent = _enemyBattlerSpawnPoint;
                break;
        }

        var battlerObject = Instantiate(unit.BattlerPrefab, parent.position, unit.BattlerPrefab.transform.rotation, parent);
        newBattler = battlerObject.GetComponent<Battler>();

        yield return BattleUtility.CalculateBattleResults(unit, opposingUnit);
        var battleResults = BattleUtility.BattleResults;

        if (isFriendly)
        {
            newBattler.Setup(unit, _playerHUD, battleResults);
            _friendlyBattler = newBattler;

            // TODO: Refactor
            if (newBattler is Magician)
            {
                var magician = newBattler as Magician;
                magician.SetMagicCircleSpawnPoint(_playerMagicCircleSpawnPoint);
            }
        }
        else
        {
            newBattler.Setup(unit, _enemyHUD, battleResults);
            _hostileBattler = newBattler;

            // TODO: refactor
            if (newBattler is Magician)
            {
                var magician = newBattler as Magician;
                magician.SetMagicCircleSpawnPoint(_enemyMagicCircleSpawnPoint);
            }
        }

        if (isAttacker)
            _attackingBattler = newBattler;
        else
            _defendingBattler = newBattler;
    }


    private void SetToTransitionPosition(Transform objTransform, float amount = -4f)
    {
        var transitionStartPos = new Vector3(
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
