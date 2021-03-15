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


    private int _turn;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public int Turn { get { return _turn; } }


    private TurnPhase _phase;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public TurnPhase Phase { get { return _phase; } }


    private List<Unit> _allUnits = new List<Unit>();
    [FoldoutGroup("Game State")]
    [ShowInInspector] public List<Unit> AllUnits {  get { return _allUnits; } }

    [FoldoutGroup("Battle UI")]
    [SerializeField] private TurnDisplay _turnDisplay;
    [FoldoutGroup("Battle UI")]
    [SerializeField] private PhaseDisplay _phaseDisplay;

    [FoldoutGroup("Cameras")]
    public ProCamera2D GridCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _gridUICamera;
    private ProCamera2DTransitionsFX _gridTransitionFX;

    [FoldoutGroup("Scenes")]
    public GameObject GridScene;
    [FoldoutGroup("Scenes")]
    public GameObject BattleScene;
    

    [HideInInspector] public CombatManager CombatManager;
    private WorldGrid _worldGrid;
    private GridCursor _gridCursor;


    public void Init()
    {
        Instance = this;

        _turn = 0;
        _phase = TurnPhase.Player;
        _worldGrid = WorldGrid.Instance;
        CombatManager = GetComponent<CombatManager>();
        
        _gridTransitionFX = GridCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();
        
        BattleScene.SetActive(false);
        
        ToggleCamera(_gridUICamera);
    }


    void Start()
    {
        BeginBattleMap();
    }

    void Update()
    {
    }


    public void AddUnit(Unit unit) => _allUnits.Add(unit);

    public void RemoveUnit(Unit unit)
    {
        if (!_allUnits.Contains(unit))
            throw new System.Exception($"There is no unit: {unit.Name} on the map that's currently tracked...");

        Destroy(unit.gameObject);
        _allUnits.Remove(unit);
    }

    public void StartCombat(Unit attacker, Unit defender, string attackSound)
    {
        _gridTransitionFX.OnTransitionExitEnded += async delegate () {
            await new WaitForSeconds(1.6f);

            GridScene.SetActive(false);
            BattleScene.SetActive(true);
            CombatManager.Load(attacker, defender);
        };

        _gridTransitionFX.TransitionExit();
        MasterAudio.PlaySound3DFollowTransform(attackSound, CampaignManager.AudioListenerTransform);
    }

    public void SwitchToMap(Unit attacker, Unit defender)
    {
        _gridTransitionFX.OnTransitionExitEnded = null;

        BattleScene.SetActive(false);
        GridScene.SetActive(true);

        SetAllUnitsIdle();

        GridCursor.Instance.ClearAll();
        GridCursor.Instance.MoveInstant(attacker.GridPosition);
        GridCursor.Instance.SetFreeMode();

        ToggleCamera(_gridUICamera);

        _gridTransitionFX.TransitionEnter();
    }

    private void BeginBattleMap()
    {
        _turn = 1;
        _phase = TurnPhase.Player;
        _gridCursor = GridCursor.Instance;

        _gridCursor.SetLockedMode();
        _phaseDisplay.OnDisplayComplete += delegate ()
        {
            _turnDisplay.Show(_turn);
            _gridCursor.SetFreeMode();
        };

        _phaseDisplay.Show(_phase);
    }

    private void SetAllUnitsIdle()
    {
        foreach(Unit unit in _allUnits)
            unit.SetIdle();
    }
    
    private void ToggleCamera(Camera camera)
    {
        camera.enabled = false;
        camera.enabled = true;
    }
}
