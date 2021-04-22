using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;

public class CampaignManager : SerializedMonoBehaviour, IInitializable
{
    public static CampaignManager Instance;
    public static Transform AudioListenerTransform {
        get {
            if (Instance.BattleScene.activeSelf)
                return Instance.CombatManager.BattleCamera.transform;
            else
                return Instance.GridCamera.transform;
        }
    }


    [SerializeField]
    private TurnPhase _startingPhase;

    [SerializeField]
    private bool _displayPhaseOnStart = false;

    
    public Vector2Int PlayerDestination;

    private int _turn;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public int Turn { get { return _turn; } }


    private TurnPhase _phase;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public TurnPhase Phase { get { return _phase; } }


    private List<Unit> _allUnits = new List<Unit>();
    [FoldoutGroup("Game State")]
    [ShowInInspector] public List<Unit> AllUnits {  get { return _allUnits; } }

    
    [FoldoutGroup("Cameras")]
    public ProCamera2D GridCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _gridUICamera;
    private ProCamera2DTransitionsFX _gridTransitionFX;

    [FoldoutGroup("Scenes")]
    [Header("Grid")]
    public GameObject GridScene;
    [FoldoutGroup("Scenes")]
    public GameObject BattleScene;


    // Events
    [HideInInspector] public Action OnCombatReturn;


    [HideInInspector] public CombatManager CombatManager;
    private GridCursor _gridCursor;

    private TurnDisplay _turnDisplay;
    private PhaseDisplay _phaseDisplay;

    // Phase Boolean Flags
    private bool _beganPlayerPhase = false;
    private bool _beganEnemyPhase       = false;
    private bool _beganOtherEnemyPhase  = false;
    private bool _beganAllyPhase        = false;
    private bool _beganNeutralPhase     = false;
    private bool _initiatedCombat       = false;

    private Dictionary<int, List<Unit>> _unitsByTeam = new Dictionary<int, List<Unit>> {
        { Player.LocalPlayer.TeamId, new List<Unit>() },
        { Player.Enemy.TeamId,       new List<Unit>() },
        { Player.OtherEnemy.TeamId,  new List<Unit>() },
        { Player.Ally.TeamId,        new List<Unit>() },
        { Player.Neutral.TeamId,     new List<Unit>() },
    };
    [HideInInspector] public Dictionary<int, List<Unit>> UnitsByTeam { get { return _unitsByTeam; } }


    public void Init()
    {
        Instance = this;

        _turn = 0;
        _phase = TurnPhase.Player;
        _turnDisplay = UIManager.Instance.GridBattleCanvas.TurnDisplay;
        _phaseDisplay = UIManager.Instance.GridBattleCanvas.PhaseDisplay;

        UIManager.Instance.GridBattleCanvas.SetCamera(_gridUICamera);

        CombatManager = GetComponent<CombatManager>();

        GridCamera.GetComponent<EventSounds>().enabled = true;
        
        _gridTransitionFX = GridCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();

        if (!SceneLoader.Instance.DontShowCamera)
            _gridTransitionFX.TransitionEnter();
        
        BattleScene.SetActive(false);

        ToggleCamera(_gridUICamera);
    }


    void Start()
    {
        BeginBattleMap();
    }

    public void BeginBattleMap(bool displayPhase = false)
    {
        _turn = 1;
        _phase = _startingPhase;
        _gridCursor = GridCursor.Instance;

        SetInitialPositions();

        if (_displayPhaseOnStart)
            displayPhase = true;
        
        if (displayPhase)
        {
            Action displayComplete = delegate ()
            {
                _turnDisplay.Show(_turn);
                _gridCursor.SetFreeMode();
            };

            DisplayPhase(displayComplete);
        } else
        {
            _turnDisplay.Show(_turn);
            _gridCursor.SetFreeMode();
        }    
    }


    public void AddUnit(Unit unit)
    {
        _allUnits.Add(unit);
        _unitsByTeam[unit.TeamId].Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        if (!_allUnits.Contains(unit))
            throw new Exception($"There is no unit: {unit.Name} on the map that's currently tracked...");

        _allUnits.Remove(unit);
        _unitsByTeam[unit.TeamId].Remove(unit);
        Destroy(unit.gameObject);

        if (GridCamera.GetCameraTarget(unit.transform) != null)
            _gridCursor.SetAsCameraTarget();
    }

    // Transition to Battle Map
    public void StartCombat(Unit attacker, Unit defender, string attackSound)
    {
        if (!_initiatedCombat)
        {
            _initiatedCombat = true;

            _gridTransitionFX.OnTransitionExitEnded += async delegate () {
                await new WaitForSeconds(1.6f);

                GridScene.SetActive(false);
                BattleScene.SetActive(true);
                DisableGridLighting();

                CombatManager.Load(attacker, defender);
            };

            UIManager.Instance.GridBattleCanvas.Disable();

            _gridTransitionFX.TransitionExit();
            MasterAudio.PlaySound3DFollowTransform(attackSound, AudioListenerTransform);
        }
    }

    private void DisableGridLighting() => LightingManager2D.Get().cameraSettings[0].renderMode = CameraSettings.RenderMode.Disabled;
    private void EnableGridLighting() => LightingManager2D.Get().cameraSettings[0].renderMode = CameraSettings.RenderMode.Draw;

    // Come back to Grid Map Scene
    public void SwitchToMap(Unit attacker, Unit defender)
    {
        _gridTransitionFX.OnTransitionExitEnded = null;

        EnableGridLighting();

        BattleScene.SetActive(false);
        GridScene.SetActive(true);

        SetAllUnitsIdle();

        _gridCursor.ClearAll();
        _gridCursor.MoveInstant(attacker.GridPosition);
        _gridCursor.SetFreeMode();


        UIManager.Instance.GridBattleCanvas.Enable();

        GridCamera.SetActive(true);
        ToggleCamera(_gridUICamera);

        _gridTransitionFX.OnTransitionEnterEnded += delegate() {
            OnCombatReturn.Invoke();
            _initiatedCombat = false;
        };
        _gridTransitionFX.TransitionEnter();
    }

    // Heal Unit on Grid Map
    public void HealUnit(Unit unit, int amount)
    {
        // Todo: implement diffferent types of heal effects

        ApplyNormalHeal.HealUnit(unit, amount);
    }

    private void Update() => CombatLoop();

    private void CombatLoop()
    {
        switch(_phase)
        {
            case TurnPhase.Player:
                ProcessPlayerPhase();
                break;
            case TurnPhase.Enemy:
                ProcessEnemyPhase();
                break;
            case TurnPhase.OtherEnemy:
                ProcessOtherEnemyPhase();
                break;
            case TurnPhase.Ally:
                ProcessAllyPhase();
                break;
            case TurnPhase.Neutral:
                ProcessNeutralPhase();
                break;
        }
    }

    private void DisplayPhase(Action callback = null)
    {
        if (_phase == TurnPhase.Player)
            _gridCursor.SetLockedMode();
    

        if (callback != null)
        {
            _phaseDisplay.OnDisplayComplete = null;

            _phaseDisplay.OnDisplayComplete += callback;
            
        }

        if (_phase != TurnPhase.Player)
        {
            _gridCursor.SetLockedMode();
            _gridCursor.SetActive(false);
        }

        _phaseDisplay.Show(_phase);
    }

    private void NextTurn()
    {
        _turn += 1;
        
        _beganPlayerPhase   = false;
        _beganEnemyPhase    = false;
        _beganOtherEnemyPhase   = false;
        _beganAllyPhase         = false;
        _beganNeutralPhase      = false;
        
        _phase = TurnPhase.Player;
    }

    private void ProcessPlayerPhase()
    {
        var players = PlayerUnits();

        // We use unique display logic on the first turn. From then on, we'll display like this.
        Action phaseDisplayComplete = delegate ()
        {
            _turnDisplay.ChangeNumber(_turn);

            _gridCursor.SetActive(true);
            _gridCursor.SetAsCameraTarget();
            _gridCursor.SetFreeMode();

            // Set position for player units to rewind to.
            foreach(PlayerUnit player in players)
                player.SetInitialPosition();

            _beganPlayerPhase = true;
        };

        if (Turn > 1 && !_phaseDisplay.IsDisplaying && !_beganPlayerPhase) {
            _phaseDisplay.SetActive(true);
            DisplayPhase(phaseDisplayComplete);
        }


        // Wait until every PlayerUnit has taken action.
        bool finishedMoving = players.All(player => player.HasTakenAction);
        if (finishedMoving)
        {
            _gridCursor.SetActive(false);
            _phase = TurnPhase.Enemy;
            
            UpdateGroupsPreferredPositions(players[0].Enemies());

            foreach (PlayerUnit player in players)
                player.AllowAction();

            if (Turn > 1)
                _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;
        }
    }

    private IEnumerator InitiateAction(List<AIUnit> agents)
    {
        for (int i = 0; i < agents.Count; i++)
        {
            var movingAgent = agents[i];

            if (movingAgent.HasTakenAction) continue;

            // Follow AI Agent while it takes action
            GridCamera.SetSingleTarget(movingAgent.transform);
            GridCamera.SetCameraWindowMode(CameraWindowMode.Unit);

            if (!movingAgent.IsTakingAction)
                movingAgent.PerformAction();

            yield return new WaitUntil(() => movingAgent.HasTakenAction);

            Debug.Log("Done");
        }
    }

    private void ProcessEnemyPhase()
    {
        // Wait until phase display is done to begin AI behavior
        Action phaseDisplayComplete = delegate ()
        {
            _beganEnemyPhase = true;
        };

        // If not already displayed and we havent started executing AI behavior
        if (!_phaseDisplay.IsDisplaying && !_beganEnemyPhase)
            DisplayPhase(phaseDisplayComplete);

        if (_beganEnemyPhase)
        {
            var enemies = EnemyUnits();

            if (enemies.Count > 0)
            {
                
                enemies.Sort((enemyOne, enemyTwo) => enemyOne.Speed.CompareTo(enemyTwo.Speed));

                List<AIUnit> aiAgents = new List<AIUnit>(enemies);
                StartCoroutine(InitiateAction(aiAgents));
                
                bool finishedMoving = enemies.All(enemy => enemy.HasTakenAction);

                if (finishedMoving)
                {
                    _phase = TurnPhase.OtherEnemy;
                    _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;
                    

                    foreach(EnemyUnit enemy in enemies)
                        enemy.AllowAction();
                }
            }
            else if (GridScene.activeSelf)
            {
                _phase = TurnPhase.OtherEnemy;
                _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;
            }
        }
    }


    private void UpdateGroupsPreferredPositions(List<AIUnit> agents)
    {
        var groups = AgentsAsGroups(agents);

        foreach (var group in groups)
            group.UpdatePreferredGroupPosition();
    }

    private List<AIGroup> AgentsAsGroups(List<AIUnit> agents) => agents.Select(enemy => enemy.group).Distinct().ToList();

    private void ProcessOtherEnemyPhase()
    {
        if (OtherEnemyUnits().Count > 0)
        {
        }
        else
            _phase = TurnPhase.Ally;

    }
    private void ProcessAllyPhase()
    {
        if (AllyUnits().Count > 0)
        {
        }
        else
            _phase = TurnPhase.Neutral;
    }
    private void ProcessNeutralPhase()
    {
        if (NeutralUnits().Count > 0)
        {
        }
        else
            NextTurn();
    }

    public List<PlayerUnit> PlayerUnits()
    {
        var playerUnits = new List<PlayerUnit>();

        foreach(Unit unit in UnitsByTeam[Player.LocalPlayer.TeamId])
        {
            if (unit is PlayerUnit)
                playerUnits.Add(unit as PlayerUnit);
            else
                throw new Exception($"{unit.Name} is NOT a PlayerUnit!");
        }

        return playerUnits;
    }

    public List<EnemyUnit> EnemyUnits()
    {
        var enemyUnits = new List<EnemyUnit>();

        foreach(Unit unit in UnitsByTeam[Player.Enemy.TeamId])
        {
            if (unit is EnemyUnit)
                enemyUnits.Add(unit as EnemyUnit);
            else
                throw new Exception($"{unit.Name} is NOT an EnemyUnit!");
        }

        return enemyUnits;
    }

    public List<OtherEnemyUnit> OtherEnemyUnits()
    {
        var otherEnemyUnits = new List<OtherEnemyUnit>();

        foreach (Unit unit in UnitsByTeam[Player.OtherEnemy.TeamId])
        {
            if (unit is OtherEnemyUnit)
                otherEnemyUnits.Add(unit as OtherEnemyUnit);
            else
                throw new Exception($"{unit.Name} is NOT an OtherEnemyUnit!");
        }

        return otherEnemyUnits;
    }

    public List<AllyUnit> AllyUnits()
    {
        var allyUnits = new List<AllyUnit>();

        foreach (Unit unit in UnitsByTeam[Player.Ally.TeamId])
        {
            if (unit is AllyUnit)
                allyUnits.Add(unit as AllyUnit);
            else
                throw new Exception($"{unit.Name} is NOT an AllyUnit!");
        }

        return allyUnits;
    }

    public List<NeutralUnit> NeutralUnits()
    {
        var neutralUnits = new List<NeutralUnit>();

        foreach (Unit unit in UnitsByTeam[Player.Neutral.TeamId])
        {
            if (unit is NeutralUnit)
                neutralUnits.Add(unit as NeutralUnit);
            else
                throw new Exception($"{unit.Name} is NOT an NeutralUnit!");
        }

        return neutralUnits;
    }

    private void SetAllUnitsIdle()
    {
        foreach(Unit unit in _allUnits)
            unit.SetIdle();
    }

    // Mark where all units were when turn began
    // This is used to rewind PlayerUnits back to where they started
    // When you go back from ActionSelectMenu.
    private void SetInitialPositions()
    {
        foreach (Unit unit in AllUnits)
            unit.SetInitialPosition();
    }
    
    private void ToggleCamera(Camera camera)
    {
        camera.gameObject.SetActive(false);
        camera.gameObject.SetActive(true);
    }
}
