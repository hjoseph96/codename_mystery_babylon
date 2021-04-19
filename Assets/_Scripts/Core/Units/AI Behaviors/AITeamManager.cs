using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class AITeamManager : MonoBehaviour
{
    [TeamId]
    public int AssignedTeam;

    private List<AIGroup> _aiGroups = new List<AIGroup>();

    private AIGroup _vanguard;

    // Start is called before the first frame update
    void Start()
    {
        foreach (AIGroup group in GetComponentsInChildren<AIGroup>())
            _aiGroups.Add(group);
    }


}
