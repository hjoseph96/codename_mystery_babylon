using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupBattleForRoadToFort : MonoBehaviour, IInitializable
{
    [SerializeField] private Defend _defenseObjective;
    public void Init()
    {
        var players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        players.Add(GameObject.FindGameObjectWithTag("Main Player"));

        foreach(var player in players)
        {
            var unit = player.GetComponent<Unit>();
            _defenseObjective.AddUnitWhoCannotDie(unit);
        }
    }
}
