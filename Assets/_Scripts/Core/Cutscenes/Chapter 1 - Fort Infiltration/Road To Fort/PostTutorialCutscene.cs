using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Com.LuisPedroFonseca.ProCamera2D;
using Articy.Unity;
using Articy.Codename_Mysterybabylon;
using Articy.Codename_Mysterybabylon.GlobalVariables;

public class PostTutorialCutscene : Cutscene
{
    public static PostTutorialCutscene Instance;

    [Header("Campaign Manager")]
    [SerializeField] private CampaignManager _campaignManager;


    [Header("Actors")]
    public SpriteCharacterControllerExt _Artur;
    public SpriteCharacterControllerExt _Jacques;
    public SpriteCharacterControllerExt _Zenovia;
    public SpriteCharacterControllerExt _Penelope;
    [SerializeField] private SpriteCharacterControllerExt _Christian;
    [SerializeField] private SpriteCharacterControllerExt _CarriageDriver;

    [Header("Knight Corpses")]
    [SerializeField] private List<SpriteCharacterControllerExt> _KnightCorpses;

    [Header("Cached Positions")]
    [SerializeField] private Transform _cameraStart;
    [SerializeField] private Transform _ChristianRestrainedPosition;

    [Header("Map Dialogues")]
    [ValueDropdown("SelectableEntities"), SerializeField, PropertyOrder(100)] private SpriteCharacterControllerExt _mapDialogueEntity;
    [ValueDropdown("ArticyObjectNames"), SerializeField, PropertyOrder(100)] private string _dialogueToAssign;


    [Button("Add Articy Reference"), PropertyOrder(100)]
    private void AddArticyReference()
    {
        if (_SparingChristianDialogues.ContainsKey(_mapDialogueEntity))
        {
            _SparingChristianDialogues[_mapDialogueEntity].Add(_dialogueToAssign);
        } else
        {
            _SparingChristianDialogues.Add(_mapDialogueEntity, new List<string> { _dialogueToAssign } );
        }
    }

    private List<SpriteCharacterControllerExt> SelectableEntities()
    {
        return new List<SpriteCharacterControllerExt>
        {
            _Jacques, _Penelope, _Zenovia, _Christian, _CarriageDriver
        };
    }

    private List<string> ArticyObjectNames()
    {
        var objNames = new List<string>();
        var objects = ArticyDatabase.GetAllOfType<Dialogue>();

        foreach (var obj in objects)
            objNames.Add(obj.DisplayName);

        return objNames;
    }
    [OdinSerialize, PropertyOrder(100)] private Dictionary<SpriteCharacterControllerExt, List<string>> _SparingChristianDialogues;

    [SerializeField, PropertyOrder(101)] private Transform _CarriageDriverWhistlePoint;
    [SerializeField] private GridPathComponent _carriageDriverReturnPath;

    private WorldGrid _worldGrid;
    private ProCamera2D _camera;
    private ProCamera2DTransitionsFX _cameraTransitions;

    private Vector2 _cameraTargetPos;

    private bool _isCameraTweening = false;

    public override void Init()
    {
        base.Init();

        Instance = this;

        _worldGrid = WorldGrid.Instance;
        _camera = ProCamera2D.Instance;
        _cameraTransitions = _camera.GetComponent<ProCamera2DTransitionsFX>();

        _cameraTargetPos = _cameraStart.position;
        _cameraTargetPos.y -= 6f;

        _campaignManager.UponCampaignWon += delegate ()
        {
            StartCoroutine(SetScene());
        };
        _cameraTargetPos = _camera.transform.position;
        _cameraTargetPos.y -= 6f;

        foreach(var corpse in _KnightCorpses)
        {
            var entityRef = corpse.GetComponent<EntityReference>();

            EntityManager.Instance.RemoveEntityReference(entityRef);
        }

        //StartCoroutine(SetScene());
    }



    private void Update()
    {
        if (_isCameraTweening)
        {
            var cameraIsInPlace = Vector2.Distance(_cameraTargetPos, _cameraStart.position) < 0.1f;
            if (cameraIsInPlace)
                _camera.transform.position = Vector2.Lerp(_cameraStart.position, _cameraTargetPos, Time.deltaTime * .01f);
            else
            {
                _camera.transform.position = _cameraTargetPos;
                _camera.SetSingleTarget(_Christian.transform);
                _camera.UpdateType = UpdateType.LateUpdate;

                _isCameraTweening = false;
            }
        }
    }

    public void AddArticyReference(SpriteCharacterControllerExt entity, string dialogueName)
    {
        if (_SparingChristianDialogues.ContainsKey(entity))
        {
            _SparingChristianDialogues[entity].Add(dialogueName);
        }
        else
        {
            _SparingChristianDialogues.Add(entity, new List<string> { dialogueName });
        }
    }

    public IEnumerator KillChristian()
    {
        var inFrontOfChristian = new Vector2Int(20, 36);
        yield return _Artur.WalkToCoroutine(inFrontOfChristian);

        var arturPlayerUnit = _Artur.GetComponent<PlayerUnit>();
        var christianUnit = _Christian.GetComponent<Unit>();

        arturPlayerUnit.Init();
        christianUnit.Init();

        arturPlayerUnit.Rotate(Direction.Left);
        arturPlayerUnit.SetIdle();

        var controller = arturPlayerUnit.GetComponent<SpriteCharacterControllerExt>();
        controller.enabled = false;

        arturPlayerUnit.DisplayAttackAnimation(christianUnit);

        yield return new WaitUntil(() => _Christian.IsDead);

        controller.enabled = true;

        yield return _Artur.WalkToCoroutine(new Vector2Int(19, 36));
    }

    public IEnumerator ArturEquipsKnightArmor()
    {
        var fadedOut = false;

        _cameraTransitions.OnTransitionExitEnded = null;
        _cameraTransitions.OnTransitionExitEnded += delegate () { fadedOut = true; };
        _cameraTransitions.TransitionExit();

        yield return new WaitUntil(() => fadedOut);

        var newCorpsePositions = new List<Vector2Int> {
            new Vector2Int(102, 85),
            new Vector2Int(102, 84),
            new Vector2Int(101, 83),
            new Vector2Int(104, 83),
        };

        for(var i = 0; i < _KnightCorpses.Count; i++)
        {
            var corpseLocation = newCorpsePositions[i];
            var corpse = _KnightCorpses[i];

            _worldGrid.PlaceGameObject(corpse.gameObject, corpseLocation);
        }

        _worldGrid.PlaceGameObject(_Christian.gameObject, new Vector2Int(102, 81));

        PlaceAndRotate(_Jacques, Direction.Down, new Vector2Int(110, 83));
        PlaceAndRotate(_Zenovia, Direction.Left, new Vector2Int(112, 82));
        PlaceAndRotate(_Penelope, Direction.Left, new Vector2Int(111, 81));
        PlaceAndRotate(_Artur, Direction.Right, new Vector2Int(109, 81));

        var knightAnimset = _Artur.GetComponent<ArturKnightAnimationSet>();
        knightAnimset.OverrideToKnightAnimations();
        _Artur.SetIdle();

        _camera.UpdateType = UpdateType.ManualUpdate;
        _camera.SetSingleTarget(_Artur.transform);
        _camera.Move(99999999f);

        yield return new WaitForSeconds(2f);

        DialogueManager.Instance.OnDialogueComplete += delegate ()
        {
            _Artur.AllowInput();
        };

        var fadedIn = false;
        
        _cameraTransitions.OnTransitionEnterEnded = null;
        _cameraTransitions.OnTransitionEnterEnded += delegate () { fadedIn = true; };
        _cameraTransitions.TransitionEnter();

        yield return new WaitUntil(() => fadedIn);

        _camera.UpdateType = UpdateType.LateUpdate;
    }

    public IEnumerator ArturLeavesForTheFort()
    {
        var arturExitPoint = new Vector2Int(126, 80);
        var arturWalking = StartCoroutine(_Artur.WalkToCoroutine(arturExitPoint));
        
        DialogueManager.Instance.OnDialogueComplete += delegate ()
        {
            if (!ArticyGlobalVariables.Default.Ch1FortInfiltration.KilledChristian)
                ArturChangeIntoKnightCutscene.Instance.ActivateTriggers();
        };

        _cameraTransitions.OnTransitionExitEnded += delegate ()
        {
            _Artur.StopCoroutine(arturWalking);
            var arturNewMapPosition = new Vector2Int(169, 76);
            PlaceAndRotate(_Artur, Direction.Right, arturNewMapPosition);
            
            _camera.UpdateType = UpdateType.ManualUpdate;
            _camera.SetSingleTarget(_Artur.transform);
            _camera.Move(99999999f);
        };


        yield return new WaitForSeconds(1.3f);
        _cameraTransitions.TransitionExit();

        _cameraTransitions.OnTransitionEnterEnded += delegate ()
        {
            _camera.UpdateType = UpdateType.LateUpdate;
            _Artur.AllowInput();
        };

        yield return new WaitForSeconds(1.3f);

        _cameraTransitions.TransitionEnter();
    }

    public IEnumerator GoRestrainChristian()
    {
        var fadedOut = false;
        _cameraTransitions.OnTransitionExitEnded += delegate () { fadedOut = true; };
        _cameraTransitions.TransitionExit();

        yield return new WaitUntil(() => fadedOut);

        _Christian.transform.position = _ChristianRestrainedPosition.position;
        _Christian.PlayRestrainedAnim();

        var arturPosition = new Vector2Int(107, 81);
        _Artur.FreezeInput();
        PlaceAndRotate(_Artur, Direction.Up, arturPosition);

        var jacquesPosition = new Vector2Int(109, 84);
        PlaceAndRotate(_Jacques, Direction.Down, jacquesPosition);

        var zenoviaPosition = new Vector2Int(111, 81);
        PlaceAndRotate(_Zenovia, Direction.Left, zenoviaPosition);

        var penelopePosition = new Vector2Int(104, 83);
        PlaceAndRotate(_Penelope, Direction.Right, penelopePosition);

        _camera.SetSingleTarget(_Christian.transform);
        _camera.UpdateType = UpdateType.ManualUpdate;
        _camera.Move(999999f);

        var fadedIn = false;
        _cameraTransitions.OnTransitionEnterEnded += delegate () { fadedIn = true; };

        yield return new WaitForSeconds(1.3f);

        _cameraTransitions.TransitionEnter();

        yield return new WaitUntil(() => fadedIn);

        _camera.UpdateType = UpdateType.LateUpdate;
    }

    public IEnumerator GoFetchRope()
    {
        var insideCarriageDoor = new Vector2Int(56, 21);
        StartCoroutine(_CarriageDriver.WalkToCoroutine(insideCarriageDoor));

        var camera = ProCamera2D.Instance;

        var fadedOut = false;
        _cameraTransitions.OnTransitionExitEnded += delegate ()
        {
            fadedOut = true;
        };

        _cameraTransitions.TransitionExit();

        _CarriageDriver.FreezeInput();

        yield return new WaitUntil(() => fadedOut);

        yield return new WaitForSeconds(1f);

        _cameraTransitions.TransitionEnter();

        yield return _CarriageDriver.WalkToCoroutine(_carriageDriverReturnPath.GridPath);

        _CarriageDriver.Rotate(Direction.Up);
        _CarriageDriver.SetIdle();

        yield return new WaitUntil(() => _CarriageDriver.Facing == Direction.Up.ToVector());
    }

    public IEnumerator GoBackToWhistling()
    {
        _CarriageDriver.OnAutoMoveComplete += delegate ()
        {
            _CarriageDriver.transform.position = _CarriageDriverWhistlePoint.position;
            var carriageDriverController = _CarriageDriver as CarriageDriverController;
            carriageDriverController.StartWhistling();

            _Artur.AllowInput();
        };
        yield return _CarriageDriver.WalkToCoroutine(new Vector2Int(53, 19));
    }

    public IEnumerator GoFindRope()
    {
        foreach(var entry in _SparingChristianDialogues)
        {
            var controller = entry.Key;

            var mapDialogue = controller.GetComponentInChildren<MapDialogue>();
            var articyData = controller.GetComponentInChildren<ArticyDataContainer>();

            foreach (var dialogue in entry.Value)
                articyData.AddDialogue(dialogue);

            articyData.SetReferences();
            mapDialogue.Init();
        }

        _CarriageDriver.transform.position = _CarriageDriverWhistlePoint.position;

        var carriageDriverController = _CarriageDriver as CarriageDriverController;
        carriageDriverController.StopSitting();
        carriageDriverController.StartWhistling();

        _camera.SetSingleTarget(_Artur.transform);

        _Artur.AllowInput();

        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator ChristianChangeOutfit()
    {
        _Christian.Rotate(Direction.Left);
        _Christian.SetIdle();

        yield return new WaitForSeconds(1f);

        var christianTreePosition = new Vector2Int(33, 36);
        StartCoroutine(_Christian.WalkToCoroutine(christianTreePosition));

        yield return new WaitForSeconds(2f);

        var fadedOut = false;

        _cameraTransitions.OnTransitionExitEnded += delegate ()
        {
            fadedOut = true;
        };
        _cameraTransitions.TransitionExit();

        yield return new WaitUntil(() => fadedOut);
        var christianAnimation = _Christian.GetComponent<ChristianAnimationSet>();
        christianAnimation.OverrideDisrobedAnimations();

        var christianReturnPos = new Vector2Int(21, 36);
        _cameraTransitions.TransitionEnter();
        
        yield return _Christian.WalkToCoroutine(christianReturnPos);
    }

    private IEnumerator SetScene()
    {
        PlaceCorpses();

        _cameraTransitions.TransitionExit();

        GridCursor.Instance.ClearAll();
        GridCursor.Instance.SetLockedMode();
        GridCursor.Instance.Hide();

        _camera.UpdateType = UpdateType.ManualUpdate;
        _camera.RemoveAllCameraTargets();
        _camera.transform.position = _cameraStart.position;


        var arturPos = new Vector2Int(18, 33);
        PlaceAndRotate(_Artur, Direction.Up, arturPos);

        var christianPos = new Vector2Int(15, 36);
        PlaceAndRotate(_Christian, Direction.Down, christianPos);


        var jacquesPos = new Vector2Int(12, 38);
        PlaceAndRotate(_Jacques, Direction.Down, jacquesPos);

        var penelopePos = new Vector2Int(12, 33);
        PlaceAndRotate(_Penelope, Direction.Up, penelopePos);

        var zenoviaPos = new Vector2Int(18, 38);
        PlaceAndRotate(_Zenovia, Direction.Down, zenoviaPos);

        yield return new WaitForSeconds(0.6f);

        _cameraTransitions.TransitionEnter();

        _Christian.SetIncapacitated();

        _isCameraTweening = true;


        Play();
    }

    private void PlaceAndRotate(SpriteCharacterControllerExt controller, Direction directionToLook, Vector2Int targetCell)
    {

        controller.enabled = true;

        _worldGrid.PlaceGameObject(controller.gameObject, targetCell);

        var healthBar = controller.GetComponentInChildren<MiniHealthBar>();
        if (healthBar != null)
            healthBar.Hide();

        controller.Rotate(directionToLook);
        controller.SetIdle();
    }

    private void PlaceCorpses()
    {
        var corpsePositions = new List<Vector2Int> {
            new Vector2Int(13, 41), 
            new Vector2Int(14, 43),
            new Vector2Int(14, 42),
            new Vector2Int(16, 41),
        };

        var directions = new List<Direction>
        {
            Direction.Left, Direction.Down, Direction.Right, Direction.Up
        };
        for (var i = 0; i < _KnightCorpses.Count; i++)
        {
            _worldGrid.PlaceGameObject(_KnightCorpses[i].gameObject, corpsePositions[i]);

            _KnightCorpses[i].Rotate(directions[i]);
            
            _KnightCorpses[i].BecomeCorpse();

            var healthBar = _KnightCorpses[i].GetComponentInChildren<MiniHealthBar>();
            if (healthBar != null)
                healthBar.Hide();
        }
    }
}
