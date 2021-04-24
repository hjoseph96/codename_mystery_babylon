using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using System;

public class AIGroup : MonoBehaviour, IComparable<AIGroup>
{
    private List<AIUnit> _groupMembers = new List<AIUnit>();
    [ShowInInspector] public List<AIUnit> Members { get => _groupMembers; }

    private Dictionary<AIUnit, Vector2Int> _unitDestinations;

    // Target Point for the whole group

    private Vector2Int _targetCell;
    public Vector2Int TargetCell { get => _targetCell; }


    //********************************************************************

    public AIGroupRole GroupRole;
    public AIGroupFormation GroupFormation;
    public AIGroupMovementMode MovementMode;
    public AIGroup CollaboratorGroup;

    public RelativePosition PreferredGroupPosition;
    private Vector2Int _lastPreferredGroupPosition;
    public AIFormation CurrentFormation;

    public int SelectedFormationIndex;
    public List<string> _formationNames = new List<string>();
    public Dictionary<AIGroup, Vector2Int> FlankGuards;

    private static Dictionary<AIGroupRole, int> GroupRolesPriority;


    //*********** Gizmos *********************************
    private Vector2Int _preferredPositionInTIghtMode;


    private void Start()
    {
        MovementMode = AIGroupMovementMode.TightFormation;
        InitGroupRolesPriority();

        var agentsInGroup = new List<AIUnit>(GetComponentsInChildren<AIUnit>());
        _groupMembers = agentsInGroup.OrderByDescending((member) => member.Priority()).ToList();
        Init();
        if (SightedEnemies().Count > 0)
        {
            var firstSightedEnemy = SightedEnemies()[0];
            _targetCell = firstSightedEnemy.GridPosition;
            //SetFormation();
        }
        
        PreferredGroupPosition = new RelativePosition(Members.Select(m => m).Where(m => m.IsLeader).First(), Members.Select(m => m).Where(m => m.IsLeader).First().Enemies()[0].GridPosition);
        CurrentFormation = FormationsDB.Instance.Get(_formationNames[SelectedFormationIndex]);

        FlankGuards = new Dictionary<AIGroup, Vector2Int>();

    }


    public List<Vector2Int> MoveRange()
    {
        List<Vector2Int> combinedMoveRange = new List<Vector2Int>();

        foreach (AIUnit member in _groupMembers)
        {
            var memberMoveRange = GridUtility.GetReachableCells(member).ToList();
            combinedMoveRange = combinedMoveRange.Union(memberMoveRange).ToList();
        }

        return combinedMoveRange;
    }

    public List<Vector2Int> ReachableCellsByAllMembers()
    {
        List<Vector2Int> combinedMoveRange = new List<Vector2Int>();

        foreach (AIUnit member in _groupMembers)
        {
            var memberMoveRange = GridUtility.GetReachableCells(member).ToList();
            combinedMoveRange.AddRange(memberMoveRange);
        }

        return combinedMoveRange.GroupBy(p => p).Where(g => g.Count() == Members.Count).Select(p => p.Key).ToList();
    }

    public List<Vector2Int> VisionRange()
    {
        List<Vector2Int> combinedVisionRange = new List<Vector2Int>();

        foreach (AIUnit member in _groupMembers)
            combinedVisionRange = combinedVisionRange.Union(member.VisionRange()).ToList();

        return combinedVisionRange;
    }

    public List<Unit> SightedEnemies()
    {
        List<Unit> sightedEnemies = new List<Unit>();

        foreach (Vector2Int gridPos in VisionRange())
        {
            var unit = WorldGrid.Instance[gridPos].Unit;

            if (unit != null && _groupMembers[0].IsEnemy(WorldGrid.Instance[gridPos].Unit))
                sightedEnemies.Add(unit);
        }

        return sightedEnemies;
    }

    public Vector2Int CenterOfGravity()
    {
        Vector2Int averagePosition = Vector2Int.zero;

        foreach (var member in _groupMembers)
            averagePosition += member.GridPosition;

        return averagePosition / _groupMembers.Count;
    }

    public void SetFormation()
    {
        if (TargetCell == null)
            throw new System.Exception($"TargetCell for {this.name}[AIGroup] ");

        for (int i = 0; i < CurrentFormation.Width; i++)
        {
            for (int j = 0; j < CurrentFormation.Height; j++)
            {
                _unitDestinations[Members[i + j]] = new Vector2Int(TargetCell.x + i, TargetCell.y + j);

            }
        }

    }

    public Vector2 GetDirection()
    {
        Vector2 normalizedDir = (PreferredGroupPosition.Target - PreferredGroupPosition.Position);
        normalizedDir.Normalize();
        return normalizedDir;
    }

    public Vector2 TravelDirection()
    {
        Vector2 normalizedDir = (PreferredGroupPosition.Position - _lastPreferredGroupPosition);
        normalizedDir.Normalize();
        return normalizedDir;
    }

    public Vector2Int GetFlankPoint(int index, int deltaSpreadSide = 10, int deltaSpreadBack = 10)
    {
        var dir = GetDirection();
        var deltaSpread = deltaSpreadSide;
        var deltaFlankCenter = dir * deltaSpreadBack;

        var spread = deltaSpread;
        var flankCenter = (PreferredGroupPosition.Position - (dir + deltaFlankCenter * Mathf.CeilToInt(index / 2))).ToVector2Int();

        int symmetryX = 1;
        int symmetryY = 1;

        if (index % 2 != 0)
        {
            symmetryX = Mathf.Abs(dir.y) > Mathf.Abs(dir.x) ? symmetryX * -1 : symmetryX;
            symmetryY = (Mathf.Abs(dir.y) > Mathf.Abs(dir.x)) ? symmetryY : symmetryY * -1;
        }

        var flankPoint = flankCenter - (new Vector2(spread * symmetryX * Mathf.Sign(dir.x), spread * symmetryY * Mathf.Sign(dir.y))).ToVector2Int();

        return flankPoint;
    }

    public Vector2Int GetFlankPoint(AIGroup group)
    {
        if (!FlankGuards.Keys.Contains(group))
            FlankGuards.Add(group, GetFlankPoint(FlankGuards.Count));
        else
            FlankGuards[group] = GetFlankPoint(FlankGuards.Keys.ToList().FindIndex(g => g == group));

        return FlankGuards[group];
    }

    public RelativePosition UpdatePreferredGroupPosition()
    {
        _lastPreferredGroupPosition = PreferredGroupPosition.Position;
        return GroupMovement.UpdateDestination(this);
    }


    public List<Vector2Int> FormationPositions()
    {

        List<Vector2Int> gridPositions = new List<Vector2Int>();
        for (int i = 0; i < CurrentFormation.Width; i++)
        {
            for (int j = 0; j < CurrentFormation.Height; j++)
            {
                if (CurrentFormation[i, j] != -1)
                    gridPositions.Add(new Vector2Int(PreferredGroupPosition.Position.x + i - CurrentFormation.Pivot.x, PreferredGroupPosition.Position.y + j - CurrentFormation.Pivot.y));

            }
        }

        return gridPositions;
    }

    private void Init()
    {
        int position = 1;
        foreach (var member in Members)
        {
            member.group = this;
            member.positionInGroup = position;
            position++;
        }
    }

    public void DrawFormationGizmos()
    {
        if (!Application.isPlaying)
            return;

        var worldGrid = WorldGrid.Instance;

        Gizmos.color = new Color(0, 0, 1, 1);
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)PreferredGroupPosition.Position), Vector3.one);

        List<Vector2Int> groupPositions = FormationPositions();
        Gizmos.color = new Color(.5f, .5f, .5f, 1);
        foreach (var pos in groupPositions)
        {
            Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)pos), new Vector3(.5f, .5f, .5f));
        }

        Gizmos.color = new Color(0, 0, .6f, 1);
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)_preferredPositionInTIghtMode), Vector3.one / 1.5f);

        List<Vector2Int> groupPositionsTight = FormationPositions();
        Gizmos.color = new Color(.5f, .5f, .5f, 1);
        foreach (var pos in groupPositionsTight)
        {
            Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)pos), new Vector3(.5f, .5f, .5f));
        }



        //Group Pathing
        if (Application.isPlaying)
        {
            // Center of gravity
            Gizmos.color = new Color(0, 1, 0, 1);
            Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)CenterOfGravity()), new Vector3(.5f, .5f, .5f));




            //Gizmos.color = new Color(0, 1, 0, 1);
            //var gl = new GridLine(_lastPreferredGroupPosition, PreferredGroupPosition.Position);
            //for (int i = 0; i < worldGrid.Width; i++)
            //{
            //    var p = new Vector2Int(i, gl.Resolve(i));
            //    if (p.y >= 0 && p.y < worldGrid.Height)
            //        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)p), new Vector3(.5f, .5f, .5f));
            //}

            Gizmos.color = new Color(0, 1, 0, 1);
            GridPath pathToCollaborator = GridUtility.FindPath(Members[0], CenterOfGravity(), PreferredGroupPosition.Position);

            foreach (var item in pathToCollaborator.Path())
            {
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)item), new Vector3(.5f, .5f, .5f));
            }

            // Flank Wing
            Gizmos.color = new Color(24 / 255f, 196 / 255f, 216 / 255f, 1);

            foreach (var item in FlankGuards)
            {
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)item.Value), new Vector3(.5f, .5f, .5f));
            }

        }


    }

    private void OnDrawGizmos()
    {
        if (_unitDestinations != null)
        {
            foreach (Vector2Int gridPosition in _unitDestinations.Values)
                Gizmos.DrawWireSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)gridPosition), 1);
        }

        DrawFormationGizmos();
    }

    //Lower value for the more prioritized
    private void InitGroupRolesPriority()
    {
        if (GroupRolesPriority != null)
            return;

        GroupRolesPriority = new Dictionary<AIGroupRole, int>();
        GroupRolesPriority.Add(AIGroupRole.Vanguard, 0);
        GroupRolesPriority.Add(AIGroupRole.Flank, 1);
    }

    public int CompareTo(AIGroup other)
    {
        return GroupRolesPriority[GroupRole] - GroupRolesPriority[other.GroupRole];
    }
}
