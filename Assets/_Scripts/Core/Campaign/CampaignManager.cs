using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;

#if UNITY_EDITOR
using UnityEditor;
#endif 

public class CampaignManager : SerializedMonoBehaviour, IInitializable
{
    #region Variables
    public static CampaignManager Instance;

    public static Transform AudioListenerTransform
    {
        get
        {
            var cameraTransform = ProCamera2D.Instance.transform;

            if (!cameraTransform.gameObject.activeInHierarchy && Instance != null)
            {
                if (Instance.GridScene.activeInHierarchy)
                    return Instance.GridCamera.transform;
                else if (Instance.BattleScene.activeInHierarchy)
                    return Instance.BattleSceneManager.BattleCamera.transform;
            }

            return cameraTransform;
        }
    }

    [SerializeField] private TurnPhase _startingPhase;

    [SerializeField] private bool _displayPhaseOnStart = false;

    [SerializeField] private bool _isInCombat;

    public bool IsInCombat { get { return _isInCombat; } }

    public Vector2Int PlayerDestination;

    // Win Lose Conditions
    [SerializeField] private List<MapObjective> _mapObjectives;
    public List<MapObjective> MapObjectives { get => _mapObjectives; }

    private int _turn;

    [FoldoutGroup("Game State")] [ShowInInspector] public int Turn { get { return _turn; } }

    private TurnPhase _phase;

    [FoldoutGroup("Game State")] [ShowInInspector] public TurnPhase Phase { get { return _phase; } }

    private List<Unit> _allUnits = new List<Unit>();

    [FoldoutGroup("Game State")]
    [ShowInInspector] public List<Unit> AllUnits { get { return _allUnits; } }

    [FoldoutGroup("Cameras")] public ProCamera2D GridCamera;
    [FoldoutGroup("Cameras")] [SerializeField] private Camera _gridUICamera;
    private ProCamera2DTransitionsFX _gridTransitionFX;

    [FoldoutGroup("Scenes")] [Header("Grid")] public GameObject GridScene;
    [FoldoutGroup("Scenes")] public GameObject BattleScene;

    

    // Events
    [HideInInspector] public Action OnCombatReturn;
    [HideInInspector] public Action<Unit> OnUnitKilled;
    [HideInInspector] public Action UponCampaignWon;
    [HideInInspector] public Action UponCampaignLost;


    [HideInInspector] public BattleSceneManager BattleSceneManager;
    private GridCursor _gridCursor;

    private TurnDisplay _turnDisplay;
    private PhaseDisplay _phaseDisplay;

    // Phase Boolean Flags
    private bool _beganPlayerPhase = false;
    private bool _beganEnemyPhase = false;
    private bool _beganOtherEnemyPhase = false;
    private bool _beganAllyPhase = false;
    private bool _beganNeutralPhase = false;
    private bool _initiatedCombat = false;


    private Dictionary<int, List<Unit>> _unitsByTeam = new Dictionary<int, List<Unit>> {
        { Player.LocalPlayer.TeamId, new List<Unit>() },
        { Player.Enemy.TeamId,       new List<Unit>() },
        { Player.OtherEnemy.TeamId,  new List<Unit>() },
        { Player.Ally.TeamId,        new List<Unit>() },
        { Player.Neutral.TeamId,     new List<Unit>() },
    };
    [HideInInspector] public Dictionary<int, List<Unit>> UnitsByTeam { get { return _unitsByTeam; } }

    [SerializeField] [Tooltip("Add items to this with data to create a team that will spawn with the given settings on this map if called")]
    private List<ReinforcementGroup> reinforcementGroups = new List<ReinforcementGroup>();
    #endregion

    #region Base Functions
    public void Init()
    {
        Instance = this;

        _turn = 0;
        _isInCombat = true;
        _phase = TurnPhase.Player;
        _turnDisplay = UIManager.Instance.GridBattleCanvas.TurnDisplay;
        _phaseDisplay = UIManager.Instance.GridBattleCanvas.PhaseDisplay;

        UIManager.Instance.GridBattleCanvas.SetCamera(_gridUICamera);

        BattleSceneManager = GetComponent<BattleSceneManager>();

        var sound = GridCamera.GetComponent<EventSounds>();
        if (sound != null) sound.enabled = true;

        _gridTransitionFX = GridCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();

        if (!SceneLoader.Instance.DontShowCamera)
            _gridTransitionFX.TransitionEnter();

        BattleScene.SetActive(false);

        ToggleCamera(_gridUICamera);
    }

    #region Monobehaviour
    void Start()
    {
        if (IsInCombat)
            BeginBattleMap();
    }

    private void Update()
    {
        if (IsInCombat)
            CombatLoop();
    }
    #endregion

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
        }
        else
        {
            _turnDisplay.Show(_turn);
            _gridCursor.SetFreeMode();
        }
    }

    private void SetAllUnitsIdle()
    {
        foreach (Unit unit in _allUnits)
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
    #endregion

    #region Win/Lose Conditions

    public List<MapObjective> LossConditions()
    {
        return MapObjectives.Where((objective) => objective.ObjectiveType == ObjectiveType.Loss).ToList();
    }

    public List<MapObjective> WinConditions()
    {
        return MapObjectives.Where((objective) => objective.ObjectiveType == ObjectiveType.Win).ToList();
    }

    /// <summary>
    /// This should be called after events like units dying, end of turns or anything else that might be added in the future
    /// </summary>
    public void CheckGameConditions()
    {
        var playerHasLost = LossConditions().Any((loseCondition) => loseCondition.CheckConditions());
        if (playerHasLost)
        {
            _turnDisplay.Hide();
            // Game over, etc.
            _isInCombat = false;
            UponCampaignLost?.Invoke();
            StartCoroutine(SceneLoader.Instance.LoadScene("GameOverScreen"));
        }

        var playerHasWon = WinConditions().All((winCondition) => winCondition.CheckConditions());
        if (playerHasWon)
        {
            _turnDisplay.Hide();


            // Save Stats, EXP, etc. for PLayableCharacter Entities
            foreach(var player in PlayerUnits())
            {
                var entity = player.GetComponent<EntityReference>().AssignedEntity as PlayableCharacter;
                entity.UpdateUnitData(player);
            }

            // Map Completion Logic goes here!
            _isInCombat = false;
            UponCampaignWon?.Invoke();
        }
    }

    #endregion

    #region Reinforcements
    /// <summary>
    /// Calls in a group of reinforcements, if no id is supplied, will spawn the first group in the list by default
    /// </summary>
    /// <param name="id"></param>
    public void SpawnReinforcementGroup(int id = 0)
    {
        var reinforcements = reinforcementGroups.Find(group => group.id == id);

        if(reinforcements == null)
        {
            Debug.LogWarning("Reinforcement group to spawn was not found, verify ID is correct, or that at least one group is set");
            return;
        }

        // Handle instantiation of unit 
        List<Unit> unitsAdded = new List<Unit>();
        for(int i = 0; i < reinforcements.unitsToSpawn.Count; i++)
        {
            
            // Position to spawn the unit at
            // unitsAdded.Add(unitSpawned);
            //unitsAdded[i].GridPosition = reinforcements.gridPositionsToSpawn[i];
            // If Ai group, assign group behaviour (or in the prefab itself?)
        }
    }
    #endregion

    #region Unit Related Functions
    public void SpawnReinforcements()
    {
        // Determine type of reinforcements to spawn and where to place them

    }


    public void AddUnit(Unit unit)
    {
        _allUnits.Add(unit);
        _unitsByTeam[unit.TeamId].Add(unit);

        // if Defend condition is in the map, add the important unit into the conditions important units
        if (unit.ImportantUnit)
        {
            var defend = LossConditions().Find(objective => objective is Defend);
            if (defend is Defend d)
            {
                d.AddUnitWhoCannotDie(unit);
            }
        }
    }

    /// <summary>
    /// Removes all references of unit, and removes unit from active objects, adds unit to units current cell's dead body pile list
    /// <br>Note*** if unit should not be inside dead pile list, or used to remove unit from means other than death, add in further checks</br>
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveUnit(Unit unit)
    {
        Debug.Log("Remove unit is called");
        if (!_allUnits.Contains(unit))
            throw new Exception($"There is no unit: {unit.Name} on the map that's currently tracked...");

        _allUnits.Remove(unit);
        _unitsByTeam[unit.TeamId].Remove(unit);

        var aiUnit = unit as AIUnit;
        if (aiUnit != null && aiUnit.group != null)
        {
            aiUnit.group.RemoveMember(aiUnit);

            if (aiUnit.group.Members.Count == 0)
                aiUnit.group.TryAssignCollaboratorToMyRole();
        }
        else if (aiUnit.IsLeader && aiUnit.group != null)
            aiUnit.group.AssignNewLeaderBeside(aiUnit);

        if (!unit.Unkillable)
        {
            //TODO: ensure this is only getting called when unit is dying, if unit is leaving map via other means, we don't want to call add to loot pile
            WorldGrid.Instance[unit.GridPosition].AddUnitToLootPile(unit);
            WorldGrid.Instance[unit.GridPosition].Unit = null;
        }

        if (GridCamera.GetCameraTarget(unit.transform) != null)
            _gridCursor.SetAsCameraTarget();

        OnUnitKilled?.Invoke(unit);
        CheckGameConditions();
    }
    #endregion

    #region Battling Functions
    // Transition to Battle Map
    public void StartCombat(Unit attacker, Unit defender, string attackSound)
    {
        if (!_initiatedCombat)
        {
            _initiatedCombat = true;

            _gridTransitionFX.OnTransitionExitEnded += delegate ()
            {
                GridScene.SetActive(false);
                BattleScene.SetActive(true);
                DisableGridLighting();

                StartCoroutine(BattleSceneManager.Load(attacker, defender));
            };

            UIManager.Instance.GridBattleCanvas.Disable();

            _gridTransitionFX.TransitionExit();
            MasterAudio.PlaySound3DFollowTransform(attackSound, AudioListenerTransform);
        }
    }

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

        _gridTransitionFX.OnTransitionEnterEnded += delegate ()
        {
            OnCombatReturn?.Invoke();
            OnCombatReturn = null;

            _initiatedCombat = false;
        };
        _gridTransitionFX.TransitionEnter();

        attacker.UpdateHealthBar();
        defender.UpdateHealthBar();
    }

    // Heal Unit on Grid Map
    public void HealUnit(Unit unit, int amount)
    {
        // Todo: implement diffferent types of heal effects
        ApplyNormalHeal.HealUnit(unit, amount);
    }
    #endregion

    #region Lighting 
    private void DisableGridLighting()
    {
        /*        var lightmaps = LightingManager2D.Get().cameraSettings[0].Lightmaps;

                for (var i = 0; i < lightmaps.Length; i++)
                    lightmaps[i].renderMode = CameraLightmap.RenderMode.Disabled;

                lightmaps = LightingManager2D.Get().cameraSettings[1].Lightmaps;
                for (var i = 0; i < lightmaps.Length; i++)
                    lightmaps[i].renderMode = CameraLightmap.RenderMode.Disabled;*/
    }

    private void EnableGridLighting()
    {
        /*        var lightmaps = LightingManager2D.Get().cameraSettings[0].Lightmaps;

                for (var i = 0; i < lightmaps.Length; i++)
                    lightmaps[i].renderMode = CameraLightmap.RenderMode.Draw;

                lightmaps = LightingManager2D.Get().cameraSettings[1].Lightmaps;
                for (var i = 0; i < lightmaps.Length; i++)
                    lightmaps[i].renderMode = CameraLightmap.RenderMode.Draw;*/
    }
    #endregion

    #region Turns
    private void CombatLoop()
    {
        switch (_phase)
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
            callback.Invoke();
            /*_phaseDisplay.OnDisplayComplete = null;
            _phaseDisplay.OnDisplayComplete += callback;*/
        }

        if (_phase != TurnPhase.Player)
        {
            _gridCursor.SetLockedMode();
            _gridCursor.SetActive(false);
        }

        callback();
        
        // _phaseDisplay.Show(_phase);
    }

    private void NextTurn()
    {
        _turn += 1;

        _beganPlayerPhase = false;
        _beganEnemyPhase = false;
        _beganOtherEnemyPhase = false;
        _beganAllyPhase = false;
        _beganNeutralPhase = false;

        _phase = TurnPhase.Player;
        CheckGameConditions();
    }
    #endregion

    #region Turn - Player
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

            UpdateGroupsPreferredPositions(players[0].Enemies());


            // Set position for player units to rewind to.
            foreach (PlayerUnit player in players)
                player.SetInitialPosition();

            _beganPlayerPhase = true;
        };

        if (Turn > 1 && !_phaseDisplay.IsDisplaying && !_beganPlayerPhase)
        {
            // _phaseDisplay.SetActive(true);
            DisplayPhase(phaseDisplayComplete);
        }

        // Wait until every PlayerUnit has taken action.
        bool finishedMoving = players.All(player => player.HasTakenAction);


        if (finishedMoving)
        {
            _gridCursor.ClearAll();
            _gridCursor.SetActive(false);
            _phase = TurnPhase.Enemy;

            if (players[0].Enemies().Count > 0)
                UpdateGroupsPreferredPositions(players[0].Enemies());

            foreach (PlayerUnit player in players)
                player.AllowAction();

            if (Turn > 1)
                _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;
        }
    }

    public List<PlayerUnit> PlayerUnits()
    {
        var playerUnits = new List<PlayerUnit>();

        foreach (Unit unit in UnitsByTeam[Player.LocalPlayer.TeamId])
        {
            if (unit is PlayerUnit)
                playerUnits.Add(unit as PlayerUnit);
            else
                throw new Exception($"{unit.Name} is NOT a PlayerUnit!");
        }

        return playerUnits;
    }
    #endregion

    #region Turn - Enemy | OtherEnemy
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
                enemies.Sort((enemyOne, enemyTwo) => enemyOne.Priority().CompareTo(enemyTwo.Priority()));

                List<AIUnit> aiAgents = new List<AIUnit>(enemies);
                StartCoroutine(InitiateAction(aiAgents));

                bool finishedMoving = enemies.All(enemy => enemy.HasTakenAction);

                if (finishedMoving)
                {
                    _phase = TurnPhase.OtherEnemy;
                    _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;


                    foreach (EnemyUnit enemy in enemies)
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

    public List<EnemyUnit> EnemyUnits()
    {
        var enemyUnits = new List<EnemyUnit>();

        foreach (Unit unit in UnitsByTeam[Player.Enemy.TeamId])
        {
            if (unit is EnemyUnit)
                enemyUnits.Add(unit as EnemyUnit);
            else
                throw new Exception($"{unit.Name} is NOT an EnemyUnit!");
        }

        return enemyUnits;
    }

    private void ProcessOtherEnemyPhase()
    {
        // Wait until phase display is done to begin AI behavior
        Action phaseDisplayComplete = delegate ()
        {
            _beganOtherEnemyPhase = true;
        };

        // If not already displayed and we havent started executing AI behavior
        if (!_phaseDisplay.IsDisplaying && !_beganOtherEnemyPhase)
            DisplayPhase(phaseDisplayComplete);

        if (_beganOtherEnemyPhase)
        {
            var otherEnemies = OtherEnemyUnits();
            if (otherEnemies.Count > 0)
            {
                otherEnemies.Sort((enemyOne, enemyTwo) => enemyOne.Priority().CompareTo(enemyTwo.Priority()));

                List<AIUnit> aiAgents = new List<AIUnit>(otherEnemies);
                StartCoroutine(InitiateAction(aiAgents));

                bool finishedMoving = otherEnemies.All(enemy => enemy.HasTakenAction);

                if (finishedMoving)
                {
                    _phase = TurnPhase.Ally;
                    _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;


                    foreach (OtherEnemyUnit enemy in otherEnemies)
                        enemy.AllowAction();
                }

            }
            else if (GridScene.activeSelf)
            {
                _phase = TurnPhase.Ally;
                _phaseDisplay.OnDisplayComplete -= phaseDisplayComplete;
            }

        }
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
    #endregion

    #region Turn - Ally | Neutral
    private void ProcessAllyPhase()
    {
        if (AllyUnits().Count > 0)
        {
        }
        else
            _phase = TurnPhase.Neutral;
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

    private void ProcessNeutralPhase()
    {
        if (NeutralUnits().Count > 0)
        {
        }
        else
            NextTurn();
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
    #endregion

    #region AI related
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

            // Wait until Unit Dies or HasTakenAction 
            yield return new WaitUntil(() => movingAgent.HasTakenAction);
        }
    }

    private void UpdateGroupsPreferredPositions(List<AIUnit> agents)
    {
        var groups = AgentsAsGroups(agents);

        foreach (var group in groups)
            group.UpdatePreferredGroupPosition();
    }

    private List<AIGroup> AgentsAsGroups(List<AIUnit> enemies)
    {
        var groups = enemies.Select(enemy => enemy.group).Distinct().ToList();
        groups.Sort();
        //Debug
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i] == null)
                Debug.LogError("AI group is null ");
        }

        return groups;
    }
    #endregion
}