using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTeamController : MonoBehaviour, IInitializable
{
    public static PlayerTeamController Instance;
    public List<BuddyController> TeamMates;
    public SpriteCharacterControllerExt Leader;

    private List<Vector2Int?> teamMatesDestinations;

    public Vector2Int? GetFollowingDestinationByIndex(int index, Vector2Int? _default = null) => index < teamMatesDestinations.Count ? teamMatesDestinations.ToList()[index] : _default;
    public Vector2Int GetFollowingDestinationByIndexOrDefault(int index, Vector2Int _default) => index < teamMatesDestinations.Count ? teamMatesDestinations[index].Value : _default;

    // Start is called before the first frame update
    void Start()
    {
        /*Init();
        TeamStartFollowing();
        CollapseTeam();*/
    }

    public void Init()
    {
        Instance = Instance == null ? this : Instance;

        TeamMates = new List<BuddyController>();


        var members = GetComponentsInChildren<SpriteCharacterControllerExt>();
        foreach (var member in members.ToList())
        {
            var buddy = member.GetComponent<BuddyController>();
            if (buddy != null)
                TeamMates.Add(buddy);
            else
            {
                if (Leader == null)
                    Leader = member;
                else
                    Debug.LogError("You have more than one leader, please leave only one character without a BuddyController component.");
            }
        }

        teamMatesDestinations = new List<Vector2Int?>(TeamMates.Count + 1);
        for (int i = 0; i < teamMatesDestinations.Capacity; i++)
            teamMatesDestinations.Add(null);

        if (Leader != null)
        {
            Leader.OnGridPositionChanged += DetermineMatesPositions;
            Leader.OnGridPositionChanged += ExpandTeamOnMovement;
        }
    }

    public void TeamStartFollowing()
    {
        int followerIndex = 0;
        foreach (var member in TeamMates)
        {
            member.StartFollowing(Leader.gameObject, followerIndex);
            followerIndex++;
        }
    }

    public void TeamSopFollowing()
    {
        foreach (var member in TeamMates)
        {
            member.StopFollowing();
        }
    }

    [Button("Collapse Team")]
    public void CollapseTeam()
    {
        foreach (var member in TeamMates)
        {
            member.Collapse(OnCollapsedTeamMate);
        }
    }

    public void CollapseTeamMember(int followerIndex)
    {
        TeamMates[followerIndex].Collapse(OnCollapsedTeamMate);
    }

    public void ExpandTeamMember(int followerIndex)
    {
        TeamMates[followerIndex].Expand(OnExpandedTeamMate);
    }

    private void UpdateTeamMatesDestination(Vector2Int oldLeaderPosition, Vector2Int newLeaderPosition)
    {
        var indexOfOverlappingPos = teamMatesDestinations.IndexOf(newLeaderPosition);
        var indexOfEmpty = teamMatesDestinations.IndexOf(null);

        if (indexOfOverlappingPos >= 0)
            teamMatesDestinations[indexOfOverlappingPos] = oldLeaderPosition;
        else if (indexOfEmpty >= 0)
            teamMatesDestinations[indexOfEmpty] = oldLeaderPosition;
        else
        {
            var alt = new List<Vector2Int?>(teamMatesDestinations.Capacity);
            alt.Add(oldLeaderPosition);
            for (int i = 0; i < teamMatesDestinations.Capacity - 1; i++)
            {
                alt.Add(teamMatesDestinations[i].Value);
            }

            teamMatesDestinations = alt;
        }
    }

    private void ExpandTeamOnMovement(Vector2Int oldLeaderPosition, Vector2Int newLeaderPosition)
    {
        var expandingPosition = Leader.transform.position;
        foreach (var member in TeamMates)
        {
            if (member.IsCollapsed && FollowerHasDestination(member.followerIndex))
            {
                member.Expand(expandingPosition, OnExpandedTeamMate);
                return;
            }
            expandingPosition = member.transform.position;
        }
    }
    private int collapsedMatesCount;

    private void OnExpandedTeamMate()
    {
        collapsedMatesCount--;
        if (collapsedMatesCount == 0)
        {
            Debug.Log("everyone has expanded");
        }
    }

    private void OnCollapsedTeamMate()
    {
        collapsedMatesCount++;
        if (collapsedMatesCount == TeamMates.Count)
        {
            Debug.Log("everyone has collapsed");
        }
    }

    private void DetermineMatesPositions(Vector2Int oldLeaderPosition, Vector2Int newLeaderPosition)
    {
        UpdateTeamMatesDestination(oldLeaderPosition, newLeaderPosition);

        foreach (var item in TeamMates)
        {
            item.DeterminePosition();
        }
    }

    private bool FollowerHasDestination(int followerIndex)
    {
        var indexOfNull = teamMatesDestinations.IndexOf(null);
        return indexOfNull == -1 || followerIndex < indexOfNull;
    }



    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = new Color(0, 1, 0, .1f);
        foreach (var item in teamMatesDestinations)
        {
            if (item != null)
                Gizmos.DrawCube(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)item.Value), Vector3.one);
        }

    }


}
