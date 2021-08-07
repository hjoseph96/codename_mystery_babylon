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

    public void Init()
    {
        Instance = this;
    }

    public void StartCutscene(Cutscene cutscene)
    {
        var dialogues = ArticyDatabase.GetAllOfType<Dialogue>();

        var matchingDialogue = dialogues.Where((d) => d.DisplayName == cutscene.CurrentDialogue).First();

        var dialogueManager = DialogueManager.Instance;

        var uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        uiCamera.enabled = false;
        uiCamera.enabled = true;

        dialogueManager.OnDialogueComplete += delegate ()
        {
            cutscene.UponCutsceneComplete?.Invoke();
            cutscene.UponCutsceneComplete = null;
        };

        if (cutscene.DialogType == DialogType.Portrait)
            dialogueManager.SetDialogueToPlay(matchingDialogue, DialogType.Portrait);
        else if (cutscene.DialogType == DialogType.Map)
            dialogueManager.SetDialogueToPlay(matchingDialogue, DialogType.Map, cutscene.MapDialogue);

        dialogueManager.Play();
    }

}
