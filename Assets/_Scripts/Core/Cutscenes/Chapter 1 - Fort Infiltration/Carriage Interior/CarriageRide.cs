using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class CarriageRide : Cutscene
{
    [SerializeField] Transform _arturMoveTarget;

    // Start is called before the first frame update
    public override void Init()
    {
        UponCutsceneComplete += delegate ()
        {
            StopCameraShaking();

            MasterAudio.StopAllOfSound("riding horse carriage with people loop");
            MasterAudio.StopAllOfSound("wheels on dirt road - looping");

            if (InfiniteScrollBackground.Instance != null)
                InfiniteScrollBackground.Instance.Stop();

            SetMapDialogues();

            var arturEntity = EntityManager.Instance.GetEntityRef("Artur", EntityType.PlayableCharacter);
            var arturController = arturEntity.GetComponent<SpriteCharacterControllerExt>();

            arturController.StopSitting();
            arturController.EnableCollider();

            arturController.SetIdle();
            arturController.AllowInput();

            UponCutsceneComplete = null;
        };

        base.Init();
    }

    private void SetMapDialogues()
    {
        var jacques = EntityManager.Instance.GetEntityRef("Jacques", EntityType.PlayableCharacter);
        var jacquesDialogues = jacques.GetComponentInChildren<ArticyDataContainer>();

        jacquesDialogues.AddDialogue("Artur Speaks To Jacques In Carriage 1");
        jacquesDialogues.AddDialogue("Artur Speaks to Jacques in Carriage 2");
        jacquesDialogues.SetReferences();

        var jacquesMapDialogue = jacques.GetComponentInChildren<MapDialogue>();
        jacquesMapDialogue.Clear();
        jacquesMapDialogue.Init();

        var zenovia = EntityManager.Instance.GetEntityRef("Zenovia", EntityType.PlayableCharacter);
        var zenoviaDialogues = zenovia.GetComponentInChildren<ArticyDataContainer>();

        zenoviaDialogues.AddDialogue("Artur Speaks to Zenovia In Carriage 1");
        zenoviaDialogues.AddDialogue("Artur Speaks to Zenovia in Carriage 2");
        zenoviaDialogues.SetReferences();

        var zenoviaMapDialogue = zenovia.GetComponentInChildren<MapDialogue>();
        zenoviaMapDialogue.Clear();
        zenoviaMapDialogue.Init();

        var penelope = EntityManager.Instance.GetEntityRef("Penelope", EntityType.PlayableCharacter);
        var penelopeDialogues = penelope.GetComponentInChildren<ArticyDataContainer>();


        penelopeDialogues.AddDialogue("Artur Speaks to Penelope In Carriage 1");
        penelopeDialogues.AddDialogue("Artur Speaks to Penelope in Carriage 2");
        penelopeDialogues.SetReferences();

        var penelopeMapDialogue = penelope.GetComponentInChildren<MapDialogue>();
        penelopeMapDialogue.Clear();
        penelopeMapDialogue.Init();
    }
}
