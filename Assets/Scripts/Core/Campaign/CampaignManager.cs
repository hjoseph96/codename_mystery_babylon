using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CampaignManager : SerializedMonoBehaviour, IInitializable
{
    public static CampaignManager Instance;

    [ReadOnly]
    private int _turn;
    public int Turn { get { return _turn; } }


    [ReadOnly]
    private TurnPhase _phase;
    public TurnPhase Phase { get { return _phase; } }


    [ReadOnly]
    private List<Unit> _allUnits = new List<Unit>();

    public void Init()
    {
        Instance = this;

        _turn = 0;
        _phase = TurnPhase.Player;

        foreach(Unit unit in GetComponents<Unit>())
            _allUnits.Add(unit);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
