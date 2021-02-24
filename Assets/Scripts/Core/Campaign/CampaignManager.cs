using System.Collections.Generic;
using Sirenix.OdinInspector;

using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CampaignManager : SerializedMonoBehaviour, IInitializable
{
    public static CampaignManager Instance;


    private int _turn;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public int Turn { get { return _turn; } }


    private TurnPhase _phase;
    [FoldoutGroup("Game State")]
    [ShowInInspector] public TurnPhase Phase { get { return _phase; } }


    [FoldoutGroup("Game State")]
    [ReadOnly] private List<Unit> _allUnits = new List<Unit>();


    [FoldoutGroup("Cameras")]
    public ProCamera2D GridCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _gridUICamera;
    private ProCamera2DTransitionsFX _gridTransitionFX;

    [FoldoutGroup("Scenes")]
    [SerializeField] private GameObject _gridScene;
    [FoldoutGroup("Scenes")]
    [SerializeField] private GameObject _battleScene;
    

    private CombatManager _combatManager;


    public void Init()
    {
        Instance = this;

        _turn = 0;
        _phase = TurnPhase.Player;
        _combatManager = GetComponent<CombatManager>();

        foreach(Unit unit in GetComponents<Unit>())
            _allUnits.Add(unit);
        
        _gridTransitionFX = GridCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();
        
        _battleScene.SetActive(false);
        
        _gridUICamera.enabled = false;
        _gridUICamera.enabled = true;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartBattle(Unit attacker, Unit defender)
    {
        _gridTransitionFX.OnTransitionExitEnded += delegate () {
            _gridScene.SetActive(false);
            _battleScene.SetActive(true);
            _combatManager.Load(attacker, defender);
        };
        _gridTransitionFX.TransitionExit();
    }

}
