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
    public AIGroupTraits GroupTrait;
    public AIGroupIntention GroupIntention;
    public AIGroupFormation GroupFormation;
    public AIGroupMovementMode MovementMode;
    public AIGroup CollaboratorGroup;

    public RelativePosition PreferredGroupPosition;
    private Vector2Int _lastPreferredGroupPosition;
    public AIFormation CurrentFormation;

    public int SelectedFormationIndex;
    public List<string> _formationNames = new List<string>();
    public Dictionary<AIGroup, Vector2Int> FlankGuards;
    private IAIIntentResolver _intentResolver;

    private static Dictionary<AIGroupRole, int> GroupRolesPriority;


    //*********** Gizmos *********************************
    private Vector2Int _preferredPositionInTIghtMode;


    private void Start()
    {
        Init();
    }
    
    public void Init()
    {
        MovementMode = AIGroupMovementMode.TightFormation;
        InitGroupRolesPriority();
        InitIntentResolver();


        var agentsInGroup = new List<AIUnit>(GetComponentsInChildren<AIUnit>());

        foreach (var unit in agentsInGroup)
            unit.Init();

        _groupMembers = agentsInGroup.OrderByDescending((member) => member.Priority()).ToList();

        int position = 1;
        foreach (var member in Members)
        {
            member.group = this;
            member.positionInGroup = position;
            position++;
        }

        if (SightedEnemies().Count > 0)
        {
            var firstSightedEnemy = SightedEnemies()[0];
            _targetCell = firstSightedEnemy.GridPosition;
            //SetFormation();
        }

        var leader = Members.Select(m => m).Where(m => m.IsLeader).First();
        PreferredGroupPosition = new RelativePosition(leader, leader.Enemies()[0].GridPosition);
        CurrentFormation = FormationsDB.Instance.Get(_formationNames[SelectedFormationIndex]);

        FlankGuards = new Dictionary<AIGroup, Vector2Int>();
    }

    public void RemoveMember(AIUnit groupMember)
    {
        if (_groupMembers.Contains(groupMember))
            _groupMembers.Remove(groupMember);

        if (_groupMembers.Count == 0)
            Destroy(this);
    }

    public Unit GetLeader()
    {
        var leaders = Members.Select(m => m).Where(m => m.IsLeader);
        
        if (leaders.Count() == 0)
            return Members[0];

        return leaders.First();
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
            throw new Exception($"TargetCell for {this.name}[AIGroup] ");

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
        var deltaFlankCenter = dir * deltaSpreadBack * (1 - GetLeader().ThreatLevel());

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
        return _intentResolver.Resolve(this);
    }


    public List<Vector2Int> FormationPositions()
    {

        List<Vector2Int> gridPositions = new List<Vector2Int>();
        for (int i = 0; i < CurrentFormation.Width; i++)
        {
            for (int j = 0; j < CurrentFormation.Height; j++)
            {
                if (CurrentFormation[i, j] != -1)
                {
                    var rotatedPoints = RotateFormationToTarget(new Vector2Int(i, j), CurrentFormation.Pivot);
                    var formationPos = new Vector2Int(PreferredGroupPosition.Position.x + rotatedPoints.x - CurrentFormation.Pivot.x
                        , PreferredGroupPosition.Position.y + rotatedPoints.y - CurrentFormation.Pivot.y);

                    gridPositions.Add(formationPos);
                }


            }
        }

        return gridPositions;
    }

    public Vector2Int RotateFormationToTarget(Vector2Int point, Vector2Int pivot)
    {
        var dir = GetDirection();
        Vector2Int newOffset = point - pivot;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (Mathf.Sign(dir.x) > 0)
                if (newOffset.x > 0)
                    newOffset.x *= -1;
                else
                    newOffset.y *= -1;
        }
        else
        {
            newOffset.y *= -(int)Mathf.Sign(dir.y);
        }

        return pivot + newOffset;
    }

    public void TryAssignCollaboratorToMyRole()
    {
        switch (GroupRole)
        {
            case AIGroupRole.Vanguard:
                var nearestFlank = FlankGuards.Keys.Select(fg => fg)
            .OrderBy(fg => GridUtility.GetBoxDistance(fg.CenterOfGravity(), PreferredGroupPosition.Position)).FirstOrDefault();

                if (nearestFlank == null)
                    return;

                FlankGuards.Remove(nearestFlank);

                nearestFlank.CollaboratorGroup = null;
                nearestFlank.ChangeRole(AIGroupRole.Vanguard);
                

                foreach (var item in FlankGuards)
                    item.Key.CollaboratorGroup = nearestFlank;
                return;
            case AIGroupRole.Flank:
                CollaboratorGroup.FlankGuards.Remove(this);
                return;
            default:
                return;
        }
        
    }

    public void ChangeRole(AIGroupRole role)
    {
        GroupRole = role;
        InitIntentResolver();
    }

    public void AssignNewLeaderBeside(Unit member)
    {
        member.IsLeader = false;
        Members.Select(m => m).Where(m => m != member).ToList()[UnityEngine.Random.Range(0, Members.Count - 1)].IsLeader = true;
    }


    public void DrawFormationGizmos()
    {
        if (!Application.isPlaying || CampaignManager.Instance == null || PreferredGroupPosition == null)
            return;

        var worldGrid = WorldGrid.Instance;

        Gizmos.color = new Color(0, 0, 1, 1);
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)PreferredGroupPosition.Position), Vector3.one);

        Gizmos.color = new Color(.5f, .5f, .5f, 1);
        foreach (var item in Members)
        {
            Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)(PreferredGroupPosition.Position + item.MyCellInFormation())), new Vector3(.5f, .5f, .5f));
        }

        //List<Vector2Int> groupPositions = FormationPositions();
        //Gizmos.color = new Color(.5f, .5f, .5f, 1);
        //foreach (var pos in groupPositions)
        //{
        //    Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)pos), new Vector3(.5f, .5f, .5f));
        //}

        //Gizmos.color = new Color(0, 0, .6f, 1);
        //Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)_preferredPositionInTIghtMode), Vector3.one / 1.5f);

        //List<Vector2Int> groupPositionsTight = FormationPositions();
        //Gizmos.color = new Color(.5f, .5f, .5f, 1);
        //foreach (var pos in groupPositionsTight)
        //{
        //    Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)pos), new Vector3(.5f, .5f, .5f));
        //}



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

            //Gizmos.color = new Color(0, 1, 0, 1);
            //GridPath pathToCollaborator = GridUtility.FindPath(Members[0], CenterOfGravity(), PreferredGroupPosition.Position);

            //foreach (var item in pathToCollaborator.Path())
            //{
            //    Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)item), new Vector3(.5f, .5f, .5f));
            //}

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

    private void InitIntentResolver()
    {
        switch (GroupRole)
        {
            case AIGroupRole.Vanguard:
                _intentResolver = new VanguardAIResolver();
                break;
            case AIGroupRole.Flank:
                _intentResolver = new FlankAIResolver();
                break;
            default:
                break;
        }
    }

    public int CompareTo(AIGroup other)
    {
        return GroupRolesPriority[GroupRole] - GroupRolesPriority[other.GroupRole];
    }
}
