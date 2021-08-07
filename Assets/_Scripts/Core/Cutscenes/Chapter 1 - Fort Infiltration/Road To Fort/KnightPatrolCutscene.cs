using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Com.LuisPedroFonseca.ProCamera2D;

public class KnightPatrolCutscene : Cutscene
{
    public static KnightPatrolCutscene Instance;

    [Header("Knights")]
    [SerializeField] private List<SpriteCharacterControllerExt> _knightsByID;
    [SerializeField] private List<GridPathComponent> _gridPathsByID;

    [Header("Players")]
    public SpriteCharacterControllerExt _artur;
    public SpriteCharacterControllerExt _jacques;
    public SpriteCharacterControllerExt _zenovia;
    public SpriteCharacterControllerExt _penelope;

    [Header("Battle Setup")]
    [SerializeField] private CampaignManager _campaignManager;
    [SerializeField] private GridCursor _gridCursor;

    private void Awake()
    {
        Instance = this;

        UponCutsceneComplete += delegate() { StartCoroutine(GetIntoFormation()); } ;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsTriggered)
        {
            StartCoroutine(MovePlayersInPosition());

            IsTriggered = false;
        }
    }

    public IEnumerator ThreeKnightsEmerge()
    {
        var worldGrid = WorldGrid.Instance;

        var knightThree = _knightsByID[2];
        var knightThreeStart = _gridPathsByID[2].Path[0];
        
        var startPositionThree = worldGrid.Grid.CellToWorld((Vector3Int)knightThreeStart);
        knightThree.transform.position = startPositionThree;

        yield return knightThree.WalkToCoroutine(_gridPathsByID[2].GridPath, true);

        var knightFour = _knightsByID[3];
        var knightFourStart = _gridPathsByID[3].Path[0];

        var startPositionFour = worldGrid.Grid.CellToWorld((Vector3Int)knightFourStart);
        knightFour.transform.position = startPositionFour;

        yield return knightFour.WalkToCoroutine(_gridPathsByID[3].GridPath, true);


        var knightFive = _knightsByID[4];
        var knightFiveStart = _gridPathsByID[4].Path[0];

        var startPositionFive = worldGrid.Grid.CellToWorld((Vector3Int)knightFiveStart);
        knightFive.transform.position = startPositionFour;

        var knightFiveGoal = worldGrid.Grid.GetCellCenterWorld((Vector3Int)_gridPathsByID[4].Path.Last());

        yield return knightFive.WalkToCoroutine(_gridPathsByID[4].GridPath, true);



        yield return new WaitUntil(() => knightFive.transform.position == knightFiveGoal);
    }

    private IEnumerator MovePlayersInPosition()
    {
        var arturGridPosition = new Vector2Int(13, 44);
        yield return _artur.WalkToCoroutine(arturGridPosition);
        _artur.Rotate(Direction.Up);
        _artur.SetIdle();

        var jacquesGridPosition = new Vector2Int(15, 44);
        StartCoroutine(_jacques.WalkToCoroutine(jacquesGridPosition));

        yield return new WaitForSeconds(0.8f);

        var penelopeGridPosition = new Vector2Int(13, 42);
        _penelope.OnAutoMoveComplete += delegate ()
        {
            _penelope.Rotate(Direction.Up);
            _penelope.SetIdle();
        };
        StartCoroutine(_penelope.WalkToCoroutine(penelopeGridPosition));

        yield return new WaitForSeconds(1f);

        var zenoviaGridPosition = new Vector2Int(15, 42);
        yield return _zenovia.WalkToCoroutine(zenoviaGridPosition);

        yield return new WaitForSeconds(1.6f);

        var knightOne = _knightsByID[0];
        var knightOneStart = _gridPathsByID[0].Path[0];

        var startPositionOne = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)knightOneStart);
        
        knightOne.transform.position = startPositionOne;
        StartCoroutine(knightOne.WalkToCoroutine(_gridPathsByID[0].GridPath));

        var camera2D = ProCamera2D.Instance;
        camera2D.RemoveAllCameraTargets();
        camera2D.SetSingleTarget(_knightsByID[0].transform);

        var knightTwo = _knightsByID[1];
        var startPositionTwo = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)_gridPathsByID[1].Path[0]);
        _knightsByID[1].transform.position = startPositionTwo;

        yield return new WaitForSeconds(0.2f);

        _knightsByID[1].OnAutoMoveComplete += delegate () { Play(); };
        StartCoroutine(_knightsByID[1].WalkToCoroutine(_gridPathsByID[1].GridPath));

    }

    private IEnumerator GetIntoFormation()
    {
        var camera2D = ProCamera2D.Instance;
        camera2D.RemoveAllCameraTargets();
        camera2D.SetSingleTarget(_artur.transform);

        var arturBattlePosition = new Vector2Int(13, 37);
        StartCoroutine(_artur.WalkToCoroutine(arturBattlePosition, true));

        var jacquesBattlePosition = new Vector2Int(15, 37);
        StartCoroutine(_jacques.WalkToCoroutine(jacquesBattlePosition, true));

        var penelopeBattlePosition = new Vector2Int(13, 32);
        _penelope.OnAutoMoveComplete += delegate ()
        {
            _penelope.Rotate(Direction.Up);
            _penelope.SetIdle();
        };
        StartCoroutine(_penelope.WalkToCoroutine(penelopeBattlePosition));

        var zenoviaBattlePosition = new Vector2Int(15, 32);
        StartCoroutine(_zenovia.WalkToCoroutine(zenoviaBattlePosition, true));

        yield return new WaitForSeconds(3f);

        BeginTutorialBattle();
    }

    private void BeginTutorialBattle()
    {
        _artur.Rotate(Direction.Up);
        _artur.SetIdle();

        _jacques.Rotate(Direction.Up);
        _jacques.SetIdle();

        _penelope.Rotate(Direction.Up);
        _penelope.SetIdle();

        _zenovia.Rotate(Direction.Up);
        _zenovia.SetIdle();

        _campaignManager.Init();

        var arturUnit = _artur.GetComponent<PlayerUnit>();
        var jacquesUnit = _jacques.GetComponent<PlayerUnit>();
        var penelopeUnit = _penelope.GetComponent<PlayerUnit>();
        var zenoviaUnit = _zenovia.GetComponent<PlayerUnit>();

        arturUnit.Init();
        _campaignManager.AddUnit(arturUnit);

        jacquesUnit.Init();
        _campaignManager.AddUnit(jacquesUnit);

        penelopeUnit.Init();
        _campaignManager.AddUnit(penelopeUnit);

        zenoviaUnit.Init();
        _campaignManager.AddUnit(zenoviaUnit);

        foreach (var knight in _knightsByID)
        {
            knight.DisableCollider();
            knight.SwitchToBattleMode();

            var knightUnit = knight.GetComponent<EnemyUnit>();
            knightUnit.Init();
            
            _campaignManager.AddUnit(knightUnit);
        }

        var group = _knightsByID[0].GetComponentInParent<AIGroup>();
        if (group != null)
            group.Init();

        // These are a bit buggy.
        /*if (_campaignManager != null)
            foreach (var unit in _campaignManager.AllUnits)
                unit.ApplyAuras(unit.GridPosition);*/

        _gridCursor.SetActive(true);
        _gridCursor.MoveInstant(new Vector2Int(13, 37));
        _gridCursor.transform.position = _artur.transform.position;
        _gridCursor.Init();



        var camera2D = ProCamera2D.Instance;
        camera2D.RemoveAllCameraTargets();
        camera2D.SetSingleTarget(_gridCursor.transform);

        _campaignManager.BeginBattleMap();

        _artur.SwitchToBattleMode();
        _zenovia.SwitchToBattleMode();
        _jacques.SwitchToBattleMode();
        _penelope.SwitchToBattleMode();

        TutorialMenu.Instance.Show();
    }
}
