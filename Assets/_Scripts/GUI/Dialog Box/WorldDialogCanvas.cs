using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Unity.Interfaces;
using Articy.Codename_Mysterybabylon;




public class WorldDialogCanvas : CanvasManager
{
    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes")]
    [SerializeField] private DialogBox _leftDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private DialogBox _rightDialogBoxPrefab;

    private Canvas _canvas;

    private List<DialogBox> _shownDialogBoxes = new List<DialogBox>();
    public List<DialogBox> ShownDialogBoxes { get => _shownDialogBoxes; }

    public bool IsTyping => _shownDialogBoxes.Any((dialogBox) => dialogBox.IsTyping);


    private bool _isShowing = false;
    public bool IsShowing { get => _isShowing; }

    // Start is called before the first frame update
    void Start()
    {
        _canvas = GetComponent<Canvas>();
        DialogueManager.Instance.SetWorldCanvas(this);
    }


    public DialogBox GetDialogBox(EntityReference entityReference, EntityReference speakingToReference)
    {
        // TODO: We need to switch to checking for a GUID based on Entity.
        // IE: a cutscene where two "Garrisoned Knights" are talking to each other
        // IE: a cutscene where two "Villager" NPCs are talking to each other, but different portraits...

        // This will break in that context -- but we do not YET have those portraits nor an ArticyTemplate for NPCs...
        // Each Unit/NPC will have to get an Entity with a unique GUID -- so we can accurately tell aside from their names

        // First -- Check if there's already a DialogBox for this Speaker

        var existingSpeakerDialogBox = GetDialogBoxBySpeaker(entityReference);

        // Canvas updating is not as fast as we clear the List -- Check the list
        if (ShownDialogBoxes.Contains(existingSpeakerDialogBox))
            return existingSpeakerDialogBox;
        else
            return SpawnDialogBox(entityReference, DirectionUtility.GetHorizontalDirection(speakingToReference.transform.position, entityReference.transform.position));

        throw new System.Exception("[DialogueManager] Unable to find a Dialog Box to spawn or update...");
    }

    public DialogBox GetDialogBoxBySpeaker(EntityReference entityRef)
    {
        var matchingBoxes = ShownDialogBoxes.Where((dialogBox) => dialogBox.Speaker.GUID == entityRef.GUID);
        if (matchingBoxes.Count() > 0)
            return matchingBoxes.First();

        return null;
    }

    public DialogBox SpawnDialogBox(EntityReference entityRef, Direction directionToSpawn)
    {
        if (directionToSpawn != Direction.Left && directionToSpawn != Direction.Right)
            throw new System.Exception($"[DialogManager] Given Direction: '{directionToSpawn}' is invalid. Only Direction.Left or Direction.Left is allowed.");

        Vector3 spawnPoint = entityRef.GetSpeechBubblePos(directionToSpawn);

        var boxPrefab = _leftDialogBoxPrefab;

        if (directionToSpawn == Direction.Right)
        {
            //spawnPoint.x += 5;
            boxPrefab = _rightDialogBoxPrefab;
        }

        var newDialogBox = Instantiate(
                boxPrefab.gameObject,
                spawnPoint,
                Quaternion.identity,
                _canvas.transform
            ).GetComponent<DialogBox>();

        newDialogBox.SetSpeaker(entityRef);

        _shownDialogBoxes.Add(newDialogBox);

        return newDialogBox;
    }


    private List<System.Guid> ShownGUIDs()
    {
        var guids = new List<System.Guid>();

        foreach (var dialogBox in ShownDialogBoxes)
            guids.Add(dialogBox.Speaker.GUID);

        return guids;
    }


    public void ClearDialogBoxes()
    {
        foreach (var dialogBox in ShownDialogBoxes)
            Destroy(dialogBox.gameObject);

        _shownDialogBoxes = new List<DialogBox>();
    }
}
