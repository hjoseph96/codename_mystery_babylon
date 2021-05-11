using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Codename_Mysterybabylon;



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


    private WorldDialogCanvas _worldCanvas;
    [ShowInInspector] public WorldDialogCanvas WorldCanvas { get => _worldCanvas; }

    [SerializeField] private DialogueChoiceGUI _dialogueChoiceGUI;

    private ArticyFlowPlayer _flowPlayer;
    private ArticyReference _articyRef;

    private ArticyChapter _chapter;
    public ArticyChapter Chapter { get => _chapter; }

    private DialogType _dialogType;
    public DialogType DialogType { get => _dialogType; }

    public bool IsDialogShown =>  _currentPortraitDialogBoxes.Count > 0 || _worldCanvas.ShownDialogBoxes.Count > 0;

    public bool IsTyping => _currentPortraitDialogBoxes.Any((dialogBox) => dialogBox.IsTyping) || _worldCanvas.IsTyping;

    private MapDialogue _currentMapDialog;

    private bool _multipleChoice = false;
    private bool _dialogIsFinished = true;
    public bool DialogIsfinished { get => _dialogIsFinished; }
    

    // Currently Shown Portrait Boxes, max of 2 (Left and Right corners of Canvas)
    private List<DialogBox> _currentPortraitDialogBoxes = new List<DialogBox>();

    private bool _isLastFragment = false;

    [HideInInspector] public Action OnDialogueComplete;

    public void Play() => _flowPlayer.Play(-99);

    // Start is called before the first frame update
    public void Init()
    {
        Instance = this;

        _flowPlayer = GetComponent<ArticyFlowPlayer>();
        _articyRef  = GetComponent<ArticyReference>();

        var chapterDialogueObj = _articyRef.GetObject<ArticyObject>();

        if (chapterDialogueObj is Dialogue == false)
            throw new Exception("[DialogueManager] Assigned ArticyRef is not a Dialogue...");

        var chapterDialogue = chapterDialogueObj as Dialogue;

        _chapter = new ArticyChapter(chapterDialogue);
    }


    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        if (aObject is IDialogue || aObject is IInstruction)
        {
            _flowPlayer.Play();
            return;
        }

        if (aObject is IOutputPin)
            EndDialogSequence();

        var objWithText = aObject as IObjectWithText;
        if (objWithText != null)
        {
            _dialogIsFinished = false;

            if (DialogType == DialogType.Portrait)
                ShowDialogBox(objWithText);
            else if (DialogType == DialogType.Map && objWithText is DialogueFragment)
                ShowMapDialogBoxes(objWithText);
        }
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        _dialogIsFinished = true;

        foreach (var branch in aBranches)
        {
            if (branch.Target is IDialogueFragment)
                _dialogIsFinished = false;
            else if (branch.Target is IOutputPin)
            {
                _dialogIsFinished = false;
                _isLastFragment = true;
            }

        }

        if (aBranches.Count > 1)
            StartCoroutine(ShowDialogChoices(aBranches));
    }


    private void EndDialogSequence()
    {
        _isLastFragment = false;

        ClearPortraitDialogBoxes();
        _worldCanvas.ClearDialogBoxes();

        if (_currentMapDialog != null)
            _currentMapDialog.Reset();

        if (OnDialogueComplete != null)
            OnDialogueComplete.Invoke();
    }


    public void SetDialogueToPlay(ArticyObject dialogueRef, DialogType dialogType, MapDialogue mapDialogue)
    {
        _dialogType         = dialogType;
        _currentMapDialog   = mapDialogue;

        _flowPlayer.StartOn = dialogueRef;
    }

    
    public void SetWorldCanvas(WorldDialogCanvas worldCanvas) => _worldCanvas = worldCanvas;

    
    public IEnumerator ShowDialogChoices(IList<Branch> aBranches)
    {
        yield return new WaitForSecondsRealtime(2f);

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

    private void ShowDialogBox(IObjectWithText textObj)
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

                var dialogBox = GetPortraitDialogBox(playableCharacter.Name, playableCharacter.Portrait);
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

                dialogBox.ShowNextText();
            }
        }
    }

    private void ShowMapDialogBoxes(IObjectWithText objWithText)
    {
        var dialogueFragment = objWithText as DialogueFragment;

        var stageDirections = ParseStageDirections(dialogueFragment.StageDirections);

        _worldCanvas.ClearDialogBoxes();

        var articyEntity = dialogueFragment.Speaker as Articy.Codename_Mysterybabylon.Entity;

        var matchingParticipants = _currentMapDialog.Participants.Where((entityReference) => entityReference.EntityName == articyEntity.DisplayName);

        if (matchingParticipants.Count() == 0)
            throw new Exception($"[DialogManager] There's no participant matching the DisplayName: {articyEntity.DisplayName}...");

        var entityRef = matchingParticipants.First();

        // TODO: Figure out how to have many instances of the Same Articy Entity speak to each other (when they have the same name)


        var dialogBox = _worldCanvas.GetDialogBox(entityRef);
        dialogBox.OnDialogueDisplayComplete = null;
        dialogBox.OnDialogueDisplayComplete += delegate ()
        {
            if (!_multipleChoice)
            {
                dialogBox.HideNextButton();
                _flowPlayer.Play();
            }
        };

        dialogBox.SetText(dialogueFragment.Text);

        dialogBox.ShowNextText();
    }


    private DialogBox GetPortraitDialogBox(string speakerName, AnimatedPortrait portrait)
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
            return existingSpeakerDialogBox;

        if (_currentPortraitDialogBoxes.Count == 2)
        {
            var dialogBox = portraitBoxes[Direction.Left];
            dialogBox.SetActivePortrait(portrait);
            return dialogBox;
        }

        if (_currentPortraitDialogBoxes.Count == 0)
            return SpawnPortraitDialogBox(portrait, Direction.Left);
        else
            if (_currentPortraitDialogBoxes.Count == 1)
                return SpawnPortraitDialogBox(portrait, Direction.Right);


        throw new Exception("[DialogueManager] Unable to find a Dialog Box to spawn or update...");
    }

    private DialogBox SpawnPortraitDialogBox(AnimatedPortrait portrait, Direction directionToSpawn)
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

        newPortraitDialogBox.SetActivePortrait(portrait);

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
        foreach(var dialogBox in _currentPortraitDialogBoxes)
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
