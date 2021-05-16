using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;
using Com.LuisPedroFonseca.ProCamera2D;

public class CutsceneManager : SerializedMonoBehaviour, IInitializable
{
    public static CutsceneManager Instance;

    [SerializeField] private bool _playOnStart;

    [ValueDropdown("ArticyObjectNames"), SerializeField]
    private string _currentDialogue;
    public string CurrentDialogue { get => _currentDialogue; }
    
    private Transform _moveTarget;

    private List<string> ArticyObjectNames()
    {
        var objNames = new List<string>();
        var objects = ArticyDatabase.GetAllOfType<Dialogue>();

        foreach (var obj in objects)
            objNames.Add(obj.DisplayName);

        return objNames;
    }

    public void Init()
    {
        Instance = this;

        if (_playOnStart)
            StartCutscene();
    }

    public void StartCutscene()
    {
        var dialogues = ArticyDatabase.GetAllOfType<Dialogue>();

        var matchingDialogue = dialogues.Where((d) => d.DisplayName == _currentDialogue).First();

        var dialogueManager = DialogueManager.Instance;

        dialogueManager.OnDialogueComplete += delegate ()
        {
            StopCameraShaking();

            var arturEntity = EntityManager.Instance.GetEntityRef("Artur", EntityType.PlayableCharacter);
            var arturController = arturEntity.GetComponent<SpriteCharacterControllerExt>();

            arturController.StopSitting();
            arturController.WalkTo(_moveTarget.position);
            arturController.OnAutoMoveComplete += delegate ()
            {
                arturController.Rotate(Direction.Down);
                arturController.SetIdle();
                arturController.AllowInput();
            };

            if (InfiniteScrollBackground.Instance != null)
                InfiniteScrollBackground.Instance.Stop();
        };

        dialogueManager.SetDialogueToPlay(matchingDialogue, DialogType.Portrait);
        dialogueManager.Play();
    }

    public void SetMoveTarget(Transform moveTarget) => _moveTarget = moveTarget;

    private void StopCameraShaking()
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
