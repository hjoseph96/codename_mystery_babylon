using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;
using Com.LuisPedroFonseca.ProCamera2D;

public class Cutscene : SerializedMonoBehaviour, IInitializable
{
    [SerializeField] private bool _playOnStart;
    [SerializeField] private bool _repeatableTrigger;

    [ValueDropdown("ArticyObjectNames"), SerializeField]
    private string _currentDialogue;
    public string CurrentDialogue { get => _currentDialogue; }
    private List<string> ArticyObjectNames()
    {
        var objNames = new List<string>();
        var objects = ArticyDatabase.GetAllOfType<Dialogue>();

        foreach (var obj in objects)
            objNames.Add(obj.DisplayName);

        return objNames;
    }

    [SerializeField] private DialogType _dialogType;
    public DialogType DialogType { get => _dialogType; }

    private bool IsMapDialogue => _dialogType == DialogType.Map;

    [Header("Map Dialogue"), ShowIf("IsMapDialogue")]
    [SerializeField] private MapDialogue _mapDialogue;
    public MapDialogue MapDialogue { get => _mapDialogue;  }


    [HideInInspector] public Action UponCutsceneComplete;

    private List<Collider2D> _triggers = new List<Collider2D>();
    protected bool IsTriggered = false;

    public virtual void Init()
    {
        if (_playOnStart) Play();

        foreach (var collider in GetComponents<Collider2D>())
            if (collider.isTrigger)
                _triggers.Add(collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Main Player")
        {
            DeactivateTriggers();

            var controller = other.GetComponent<SpriteCharacterControllerExt>();
            controller.FreezeInput();

            IsTriggered = true;

            UponCutsceneComplete += delegate () 
            { 
                IsTriggered = false;

                if (_repeatableTrigger && !DialogueManager.Instance.IsRunningAnAction)
                    StartCoroutine(WaitAndActivateTriggers(controller));
            };
        }
    }

    private IEnumerator WaitAndActivateTriggers(SpriteCharacterControllerExt controller)
    {
        controller.AllowInput();

        yield return new WaitForSeconds(1f);

        ActivateTriggers();
    }

    public void Play()
    {
        CutsceneManager.Instance.StartCutscene(this);
    }

    public void DeactivateTriggers()
    {
        foreach (var trigger in _triggers)
            trigger.enabled = false;
    }

    public void ActivateTriggers()
    {
        foreach (var trigger in _triggers)
            trigger.enabled = true;
    }

    public static IEnumerator WalkAway(EntityReference player)
    {
        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(player.transform.position);
        var controller = player.GetComponent<SpriteCharacterControllerExt>();

        var targetGridPosition = currentGridPosition;

        if (controller.Facing == Direction.Left.ToVector())
            targetGridPosition.x += 2;

        if (controller.Facing == Direction.Right.ToVector())
            targetGridPosition.x -= 2;

        if (controller.Facing == Direction.Up.ToVector())
            targetGridPosition.y -= 2;

        if (controller.Facing == Direction.Down.ToVector())
            targetGridPosition.y += 2;

        if (!WorldGrid.Instance.PointInGrid(targetGridPosition))
            throw new Exception($"[Cutscene] ");

        yield return controller.WalkToCoroutine(targetGridPosition);

        controller.AllowInput();
    }

    protected void StopCameraShaking()
    {
        var camera = ProCamera2D.Instance;

        var shaker = camera.GetComponent<ProCamera2DShake>();
        if (shaker != null)
        {
            shaker.StopShaking();
            shaker.StopConstantShaking();
        }
    }

}
