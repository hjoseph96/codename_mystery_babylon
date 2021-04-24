using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Codename_Mysterybabylon;


[RequireComponent(typeof(ArticyFlowPlayer))]
public class DialogueManager : SerializedMonoBehaviour, IInitializable, IArticyFlowPlayerCallbacks
{   
    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes with Portrait")]
    [SerializeField] private DialogBox _leftPortraitDialogBoxPrefab;
    
    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private DialogBox _rightPortraitDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes")]
    [SerializeField] private DialogBox _leftDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private DialogBox _rightDialogBoxPrefab;

    private ArticyFlowPlayer _flowPlayer;
    private ArticyReference _articyRef;

    private ArticyChapter _chapter;
    public ArticyChapter Chapter { get => _chapter; }


    // Currently Shown Portrait Boxes, max of 2 (Left and Right corners of Canvas)
    private List<DialogBox> _currentPortraitDialogBoxes = new List<DialogBox>();

    // Currently Shown Dialog Boxes that are shown on a map basis. These might need to have their own canvas that's in world space, not camera...
    private List<DialogBox> _currentNormalDialogBoxes = new List<DialogBox>();

    // Start is called before the first frame update
    public void Init()
    {
        _flowPlayer = GetComponent<ArticyFlowPlayer>();
        _articyRef  = GetComponent<ArticyReference>();

        var chapterDialogueObj = _articyRef.GetObject<ArticyObject>();

        if (chapterDialogueObj is Dialogue == false)
            throw new System.Exception("[DialogueManager] Assigned ArticyRef is not a Dialogue...");

        var chapterDialogue = chapterDialogueObj as Dialogue;

        _chapter = new ArticyChapter(chapterDialogue);

        _flowPlayer.startOn = (ArticyRef)chapterDialogue;

        _flowPlayer.Play();
    }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        if (aObject is IDialogue)
        {
            _flowPlayer.Play();
            return;
        }

        var objWithText = aObject as IObjectWithText;
        if (objWithText != null)
        {
            ShowDialogBox(objWithText);
        }
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
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
                    dialogBox.HideNextButton();
                    _flowPlayer.Play();

                };

                dialogBox.SetText(textToDisplay);

                dialogBox.ShowNextText();
            }
        }
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


        throw new System.Exception("[DialogueManager] Unable to find a Dialog Box to spawn or update...");
    }

    private DialogBox SpawnPortraitDialogBox(AnimatedPortrait portrait, Direction directionToSpawn)
    {
        if (directionToSpawn != Direction.Left && directionToSpawn != Direction.Right)
            throw new System.Exception($"[DialogManager] Given Direction: '{directionToSpawn}' is invalid. Only Direction.Left or Direction.Left is allowed.");

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
