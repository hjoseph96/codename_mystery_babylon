using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupCutscenesForRoadToFort : MonoBehaviour, IInitializable
{
    private Dictionary<string, SpriteCharacterControllerExt> playerControllers = new Dictionary<string, SpriteCharacterControllerExt>();

    public void Init()
    {
        var players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        players.Add(GameObject.FindGameObjectWithTag("Main Player"));

        foreach(var player in players)
        {
            var controller = player.GetComponent<SpriteCharacterControllerExt>();
            playerControllers.Add(player.name, controller);
        }

        var exitCarriageSequence = FindObjectOfType<ExitCarriageCutscene>();
        if (exitCarriageSequence != null)
        {
            exitCarriageSequence._artur     = playerControllers["Artur"];
            exitCarriageSequence._jacques   = playerControllers["Jacques"];
            exitCarriageSequence._penelope  = playerControllers["Penelope"];
            exitCarriageSequence._zenovia   = playerControllers["Zenovia"];
            exitCarriageSequence.Init();
        }

        var knightPatrolSequence = FindObjectOfType<KnightPatrolCutscene>();
        if (knightPatrolSequence != null)
        {
            knightPatrolSequence._artur     = playerControllers["Artur"];
            knightPatrolSequence._jacques   = playerControllers["Jacques"];
            knightPatrolSequence._penelope  = playerControllers["Penelope"];
            knightPatrolSequence._zenovia   = playerControllers["Zenovia"];
            knightPatrolSequence.Init();
        }

        var postTutorialSequence = FindObjectOfType<PostTutorialCutscene>();
        if (postTutorialSequence != null)
        {
            postTutorialSequence._Artur     = playerControllers["Artur"];
            postTutorialSequence._Jacques   = playerControllers["Jacques"];
            postTutorialSequence._Penelope  = playerControllers["Penelope"];
            postTutorialSequence._Zenovia   = playerControllers["Zenovia"];

            postTutorialSequence.AddArticyReference(playerControllers["Jacques"], "Jacques - Change your mind?");
            postTutorialSequence.AddArticyReference(playerControllers["Penelope"], "Penelope - Sparing Christian");
            postTutorialSequence.AddArticyReference(playerControllers["Zenovia"], "Zenovia - Sparing Christian");
            postTutorialSequence.AddArticyReference(playerControllers["Zenovia"], "Zenovia - Sparing Christian 2");

            postTutorialSequence.Init();
        }

        
        var arturOutfitChangeSequence = FindObjectOfType<ArturChangeIntoKnightCutscene>();
        if (arturOutfitChangeSequence != null)
        {
            arturOutfitChangeSequence._Artur = playerControllers["Artur"];
            arturOutfitChangeSequence.Init();
        }
    }


}
