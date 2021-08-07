using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Codename_Mysterybabylon;

using Com.LuisPedroFonseca.ProCamera2D;


public enum DialogType
{
    Map,
    Portrait
}


[RequireComponent(typeof(ArticyFlowPlayer))]
public class DialogueManager : SerializedMonoBehaviour, IInitializable, IArticyFlowPlayerCallbacks
{
    public static DialogueManager Instance;

    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes with Portrait")]
    [SerializeField] private DialogBox _leftPortraitDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private DialogBox _rightPortraitDialogBoxPrefab;

    [SerializeField] private bool _nightMode;
    public bool NightMode { get => _nightMode; }

    private WorldDialogCanvas _worldCanvas;
    [ShowInInspector] public WorldDialogCanvas WorldCanvas { get => _worldCanvas; }

    [SerializeField] private DialogueChoiceGUI _dialogueChoiceGUI;

    private ArticyFlowPlayer _flowPlayer;
  

    private DialogType _dialogType;
    public DialogType DialogType { get => _dialogType; }

    public bool IsDialogShown => _currentPortraitDialogBoxes.Count > 0 || _worldCanvas.ShownDialogBoxes.Count > 0;

    public bool IsTyping => _currentPortraitDialogBoxes.Any((dialogBox) => dialogBox.IsTyping) || _worldCanvas.IsTyping;

    private MapDialogue _currentMapDialog;
    public MapDialogue CurrentMapDialog { get => _currentMapDialog; }

    private bool _multipleChoice = false;

    // Currently Shown Portrait Boxes, max of 2 (Left and Right corners of Canvas)
    private List<DialogBox> _currentPortraitDialogBoxes = new List<DialogBox>();


    [HideInInspector] public Action OnDialogueComplete;
    public bool IsRunningAnAction { get; private set; }

    public bool IsPlaying { get; private set; }

    public void Play() => _flowPlayer.Play(-99);

    // Start is called before the first frame update
    public void Init()
    {
        Instance = this;

        _flowPlayer = GetComponent<ArticyFlowPlayer>();
    }


    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
       StartCoroutine(OnFlowPlayerPausedCoroutine(aObject));
    }

    public EntityReference FetchEntityById(int entity_id)
    {
        if (_currentMapDialog == null)
            throw new Exception($"[DialogueManager] You attempted to get an EntityReference by ID, but there's no current MapDialogue attached!");

        return _currentMapDialog.GetEntityByID(entity_id);
    }

    private void AssignParticipants(Dialogue dialogue)
    {
        foreach (var fragment in dialogue.Children)
        {
            if (fragment is DialogueFragment)
            {
                var dialogueFragment = fragment as DialogueFragment;

                if (dialogueFragment.Speaker != null)
                {
                    var participant = EntityManager.Instance.GetEntityRef(dialogueFragment.Speaker.TechnicalName);
                    CurrentMapDialog.AddParticipant(participant);
                }
            }
        }
    }

    private IEnumerator OnFlowPlayerPausedCoroutine(IFlowObject aObject)
    {
        if (aObject is IDialogue || aObject is IInstruction)
        {
            // Get Participants from articy data
            var dialogue = aObject as Dialogue;
            if (CurrentMapDialog != null && dialogue != null)
                AssignParticipants(dialogue);

            _flowPlayer.Play();
            yield break;
        }

        if (aObject is IOutputPin)
            EndDialogSequence();

        var objWithText = aObject as IObjectWithText;
        if (objWithText != null)
        {
            if (objWithText.Text[0] == '$')
            {
                IsRunningAnAction = true;
                yield return DialogueActionParser.TryRunAction(objWithText.Text, delegate()
                {
                    IsRunningAnAction = false;
                    
                    _flowPlayer.Play();
                });
                yield break;
            }
            
            if (DialogType == DialogType.Portrait)
                ShowPortraitDialogBox(objWithText);
            else if (DialogType == DialogType.Map && objWithText is DialogueFragment)
                ShowMapDialogBoxes(objWithText);
        }
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        if (aBranches.Count > 1)
            StartCoroutine(ShowDialogChoices(aBranches));
    }


    private void EndDialogSequence()
    {
        ClearPortraitDialogBoxes();

        if (_worldCanvas != null)
            _worldCanvas.ClearDialogBoxes();

        if (_currentMapDialog != null)
            _currentMapDialog.Reset();

        IsPlaying = false;

        OnDialogueComplete?.Invoke();
        OnDialogueComplete = null;
    }


    public void SetDialogueToPlay(ArticyObject dialogueRef, DialogType dialogType, MapDialogue mapDialogue = null)
    {
        _dialogType = dialogType;


        if (mapDialogue != null)
            _currentMapDialog = mapDialogue;

        IsPlaying = true;

        _flowPlayer.StartOn = dialogueRef;
    }


    public void SetWorldCanvas(WorldDialogCanvas worldCanvas) => _worldCanvas = worldCanvas;


    // TODO: Fix the wait time issue when pressing z skipping this hold
    public IEnumerator ShowDialogChoices(IList<Branch> aBranches)
    {
        yield return new WaitUntil(() => IsDialogShown);
        yield return new WaitUntil(() => !IsTyping);

        _multipleChoice = true;

        _dialogueChoiceGUI.OnChoiceSelected += delegate (Branch chosenBranch)
        {
            _flowPlayer.Play(chosenBranch);
            _multipleChoice = false;
        };
        _dialogueChoiceGUI.Show(aBranches);
    }

    private void ShowPortraitDialogBox(IObjectWithText textObj)
    {
        var entityManager = EntityManager.Instance;

        var textToDisplay = textObj.Text;

        if (textObj is DialogueFragment)
        {
            var dialogueFragment = textObj as DialogueFragment;

            var stageDirections = ParseStageDirections(dialogueFragment.StageDirections);
            if (stageDirections.Contains("Clear"))
                ClearPortraitDialogBoxes();

            if (dialogueFragment.Speaker is DefaultMainCharacterTemplate == true)
            {
                var articyCharacterTemplate = dialogueFragment.Speaker as DefaultMainCharacterTemplate;
                var playableCharacter = entityManager.GetPlayableCharacterByName(articyCharacterTemplate.DisplayName);

                var dialogBox = GetPortraitDialogBox(playableCharacter.Name, playableCharacter);
                dialogBox.OnDialogueDisplayComplete = null;
                dialogBox.OnDialogueDisplayComplete += delegate ()
                {
                    if (!_multipleChoice)
                    {
                        dialogBox.HideNextButton();
                        _flowPlayer.Play();
                    }
                };

                dialogBox.SetText(textToDisplay);
                dialogBox.Activate();
                dialogBox.ShowNextText();
            }
        }
    }

    private void ToggleWorldUICamera()
    {
        var childCameras = ProCamera2D.Instance.GetComponentsInChildren<Camera>();
        foreach(var camera in childCameras)
        {
            if (camera.gameObject.tag == "World Space UI Camera")
            {
                camera.enabled = false;
                camera.enabled = true;
            }
        }
    }

    public IEnumerator ClearMapDialogBoxes()
    {
        _worldCanvas.ClearDialogBoxes();

        yield return new WaitUntil(() => _worldCanvas.ShownDialogBoxes.Count == 0);
    }

    private void ShowMapDialogBoxes(IObjectWithText objWithText)
    {
        ToggleWorldUICamera();

        var dialogueFragment = objWithText as DialogueFragment;

        var stageDirections = ParseStageDirections(dialogueFragment.StageDirections);

        _worldCanvas.ClearDialogBoxes();

        var articyEntity = dialogueFragment.Speaker as Articy.Codename_Mysterybabylon.Entity;

        var matchingParticipants = CurrentMapDialog.Participants.Where((entityReference) => entityReference.EntityName == articyEntity.DisplayName);
        var secondRefParticipant = CurrentMapDialog.Participants.Where((entityReference) => entityReference.EntityName != articyEntity.DisplayName);

        if (matchingParticipants.Count() == 0)
            throw new Exception($"[DialogManager] There's no participant matching the DisplayName: {articyEntity.DisplayName}...");

        string finalText = dialogueFragment.Text;
        Dictionary<string, object> results = DialogueAttributesParser.GetEntityInstanceSpeaker(dialogueFragment.Text, out finalText);
        var entityRef = results.ContainsKey("entity_id") ? results["entity_id"] as EntityReference : matchingParticipants.First();



        EntityReference secondRef;
        if (secondRefParticipant.Count() > 0)
            secondRef = secondRefParticipant.First();
        else // Catch and use the primary entity in case of no other participants
            secondRef = entityRef;

        // TODO: Figure out how to have many instances of the Same Articy Entity speak to each other (when they have the same name)
        DialogBox dialogBox = _worldCanvas.GetDialogBox(entityRef, secondRef);

        DialogueStep(dialogBox);

        dialogBox.SetText(finalText);

        dialogBox.ShowNextText();
    }

    /// <summary>
    /// Used to call the initialization of a dialogue box for subscribing and setting night mode
    /// </summary>
    /// <param name="dialogBox"></param>
    private void DialogueStep(DialogBox dialogBox)
    {
        if (_nightMode)
            dialogBox.SetNightModeColor();

        dialogBox.OnDialogueDisplayComplete = null;
        dialogBox.OnDialogueDisplayComplete += delegate ()
        {
            if (!_multipleChoice)
            {
                dialogBox.HideNextButton();
                _flowPlayer.Play();
            }
        };
    }

    private DialogBox GetPortraitDialogBox(string speakerName, Entity entity)
    {
        var dialogCanvas = UIManager.Instance.GridBattleCanvas;
        var portraitBoxes = dialogCanvas.GetPortraitDialogBoxes();


        // TODO: We need to switch to checking for a GUID based on Entity.
        // IE: a cutscene where two "Garrisoned Knights" are talking to each other
        // IE: a cutscene where two "Villager" NPCs are talking to each other, but different portraits...

        // This will break in that context -- but we do not YET have those portraits nor an ArticyTemplate for NPCs...
        // Each Unit/NPC will have to get an Entity with a unique GUID -- so we can accurately tell aside from their names


        // First -- Check if there's already a DialogBox for this Speaker
        var existingSpeakerDialogBox = GetDialogBoxBySpeakerName(speakerName);

        // Canvas updating is not as fast as we clear the List -- Check the list
        if (_currentPortraitDialogBoxes.Contains(existingSpeakerDialogBox))
        {
            if (_currentPortraitDialogBoxes.Count == 2)
                _currentPortraitDialogBoxes.Where((box) => box != existingSpeakerDialogBox).First().Deactivate();

            return existingSpeakerDialogBox;
        }

        if (_currentPortraitDialogBoxes.Count == 2)
        {
            var dialogBox = portraitBoxes[Direction.Left];

            _currentPortraitDialogBoxes.Where((box) => box != dialogBox).First().Deactivate();

            dialogBox.SetSpeakerGender(entity.Gender);
            dialogBox.SetActivePortrait(entity.Portrait);

            return dialogBox;
        }

        if (_currentPortraitDialogBoxes.Count == 0)
            return SpawnPortraitDialogBox(entity, Direction.Left);
        else if (_currentPortraitDialogBoxes.Count == 1)
        {
            _currentPortraitDialogBoxes[0].Deactivate();
            return SpawnPortraitDialogBox(entity, Direction.Right);
        }


        throw new Exception("[DialogueManager] Unable to find a Dialog Box to spawn or update...");
    }

    private DialogBox SpawnPortraitDialogBox(Entity entity, Direction directionToSpawn)
    {
        if (directionToSpawn != Direction.Left && directionToSpawn != Direction.Right)
            throw new Exception($"[DialogManager] Given Direction: '{directionToSpawn}' is invalid. Only Direction.Left or Direction.Left is allowed.");

        var dialogCanvas = UIManager.Instance.GridBattleCanvas;

        // Default to Spawn in the Left Corner or Canvas
        Transform spawnPoint = dialogCanvas.LeftPortraitDialogSpawnPoint;
        DialogBox dialogBoxPrefab = _leftPortraitDialogBoxPrefab;

        // Default to Spawn in the Right Corner, if told to
        if (directionToSpawn == Direction.Right)
        {
            spawnPoint = dialogCanvas.RightPortraitDialogSpawnPoint;
            dialogBoxPrefab = _rightPortraitDialogBoxPrefab;
        }

        var newPortraitDialogBox = Instantiate(
                dialogBoxPrefab.gameObject,
                spawnPoint.position,
                Quaternion.identity,
                spawnPoint
            ).GetComponent<DialogBox>();

        newPortraitDialogBox.SetActive(false);

        if (_nightMode)
        {
            entity.Portrait.SetNightModeColor();
            newPortraitDialogBox.SetNightModeColor();
        }

        newPortraitDialogBox.SetSpeakerGender(entity.Gender);

        newPortraitDialogBox.SetActivePortrait(entity.Portrait);

        _currentPortraitDialogBoxes.Add(newPortraitDialogBox);

        return newPortraitDialogBox;
    }

    private DialogBox GetDialogBoxBySpeakerName(string speakerName)
    {
        var dialogCanvas = UIManager.Instance.GridBattleCanvas;

        var currentlyShownDialogBoxes = dialogCanvas.GetPortraitDialogBoxes();

        if (currentlyShownDialogBoxes.Count == 0)
            return null;

        var matchingDialogBox = currentlyShownDialogBoxes.Values.Where((dialogBox) => dialogBox.CurrentPortrait.Name == speakerName);

        if (matchingDialogBox.Count() > 0)
            return matchingDialogBox.First();

        return null;
    }

    private void ClearPortraitDialogBoxes()
    {
        foreach (var dialogBox in _currentPortraitDialogBoxes)
            Destroy(dialogBox.gameObject);

        _currentPortraitDialogBoxes = new List<DialogBox>();
    }

    private List<string> ParseStageDirections(string stageDirections)
    {
        var listOfDirections = new List<string>();

        foreach (var stageDirection in stageDirections.Split(','))
            listOfDirections.Add(stageDirection.Trim(' '));

        return listOfDirections;
    }
}
