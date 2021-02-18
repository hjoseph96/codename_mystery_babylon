using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArrowPath : SerializedMonoBehaviour
{
    [Header("Arrow sprites")]
    [OdinSerialize] private readonly Dictionary<DirectionPair, Sprite> _arrowBodySprites = new Dictionary<DirectionPair, Sprite>();
    [OdinSerialize] private readonly Dictionary<Direction, Sprite> _arrowHeadSprites = new Dictionary<Direction, Sprite>();
    [OdinSerialize] private readonly Dictionary<Direction, Sprite> _cornersSprites = new Dictionary<Direction, Sprite>();

    [SerializeField] private ArrowSprite _arrowSpritePrefab;

    #region Private fields and properties

    private Unit _unit;
    private UnitType UnitType => _unit.UnitType;

    private readonly List<ArrowSprite> _instantiatedObjects = new List<ArrowSprite>();
    private readonly List<Vector2Int> _positions = new List<Vector2Int>();
    private readonly List<int> _travelCosts = new List<int>();

    private ArrowSprite Head => _instantiatedObjects.Count > 0 ? _instantiatedObjects[_instantiatedObjects.Count - 1] : null;
    private ArrowSprite Tail => _instantiatedObjects[0];
    private int Length => _instantiatedObjects.Count;
    private Vector2Int LastPosition => _positions[_positions.Count - 1];
    private int LastTravelCost => _travelCosts[_travelCosts.Count - 1];

    private WorldGrid _worldGrid;

    #endregion

    public void CheckLOSTest(Vector2Int position, int range)
    {
        var minX = Mathf.Max(0, position.x - range);
        var maxX = Mathf.Min(_worldGrid.Width - 1, position.x + range);
        var minY = Mathf.Max(0, position.y - range);
        var maxY = Mathf.Min(_worldGrid.Height - 1, position.y + range);
        var rangeSq = range * range;

        for (var i = minX; i <= maxX; i++)
        {
            for (var j = minY; j <= maxY; j++)
            {
                var currentPosition = new Vector2Int(i, j);
                if ((currentPosition - position).sqrMagnitude <= rangeSq)
                {
                    // Do whatever we want
                    if (GridUtility.HasLineOfSight(position, currentPosition))
                    {
                        Debug.DrawLine(_worldGrid.Grid.GetCellCenterWorld((Vector3Int) position),
                                         _worldGrid.Grid.GetCellCenterWorld((Vector3Int) currentPosition), Color.yellow, 5f);
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
        }

        if (_positions.Count == 0)
            return;

        var prev = _positions[0];
        for (var i = 1; i < _positions.Count; i++)
        {
            var curr = _positions[i];
            Debug.DrawLine(_worldGrid.Grid.GetCellCenterWorld((Vector3Int) prev),
                _worldGrid.Grid.GetCellCenterWorld((Vector3Int) curr), Color.yellow);
            prev = curr;
        }
    }

    public void Init(Unit unit)
    {
        _worldGrid = WorldGrid.Instance;

        _positions.Clear();
        _unit = unit;

        _positions.Add(_unit.GridPosition);
        _travelCosts.Add(0);

        SpawnObject(_unit.GridPosition);
        //Tail.drawMode = SpriteDrawMode.Sliced;
    }

    public void Move(Vector2Int position, bool performChecks = true)
    {
        // Self intersection
        var index = _positions.IndexOf(position);

        if (index == 0)
        {
            ResetPath();
            return;
        }

        Sprite first, second;
        
        if (index > 0)
        {
            if (index == 1)
            {
                UpdateTailSprite();
            }

            Truncate(index + 1);
            if (Head != null)
            {
                var direction = GridUtility.GetDirection(_positions[_positions.Count - 2], LastPosition, false, true);
                GetCornersSprites(direction, out first, out second);
                Head.SetSprites(GetHeadSprite(direction), first, second, direction);
            }

            return;
        }

        // Calculate new cost as last path point cost + new cell cost
        var newCost = LastTravelCost + _worldGrid[LastPosition].GetTravelCost(position, UnitType);

        // If we can replace two points with one diagonal
        if (Length > 1 && (position - _positions[Length - 2]).sqrMagnitude == 2)
        {
            var diagonalDirection = GridUtility.GetDirection(_positions[Length - 2], position, false, true);
            if (
                (_worldGrid[position].StairsOrientation == StairsOrientation.RightToLeft || 
                  _worldGrid[_positions[Length - 2]].StairsOrientation == StairsOrientation.RightToLeft) 
                 &&
                (diagonalDirection == Direction.LeftUp || diagonalDirection == Direction.RightDown)
                ||
                (_worldGrid[position].StairsOrientation == StairsOrientation.LeftToRight || 
                  _worldGrid[_positions[Length - 2]].StairsOrientation == StairsOrientation.LeftToRight) 
                 &&
                 (diagonalDirection == Direction.LeftDown || diagonalDirection == Direction.RightUp)
                )
            {
                Truncate(Length - 1);
                performChecks = false;
            }
        }

        // Calculate direction
        var directionNext = GridUtility.GetDirection(LastPosition, position, false, true);

        // If we can't move to the desired position (exit/entrance is blocked) or direction is None
        if (performChecks && 
            (!_worldGrid[LastPosition].CanMove(position, UnitType) || 
             directionNext == Direction.None ||
             !GridUtility.GetNeighboursOffsets(LastPosition).Contains(directionNext.ToVector())))
        {
            // Calculate new path and extend old path with it
            var movementPointsRemainder = _unit.CurrentMovementPoints - LastTravelCost;
            var path =
                GridUtility.FindPath(_unit, LastPosition, position, movementPointsRemainder); 

            if (path == null)
            {
                newCost = int.MaxValue;
            }
            else
            {
                newCost = LastTravelCost + (int) path.TravelCost;
                if (newCost <= _unit.CurrentMovementPoints)
                {
                    for (var i = 0; i < path.Length; i++)
                    {
                        Move(path[i], false);
                    }

                    PostProcessPath();
                    return;
                }
            }
        }

        // If we ran out of cost, calculate best possible path with A* and use it instead
        if (performChecks && newCost > _unit.CurrentMovementPoints)
        {
            ResetPath();
            var path = GridUtility.FindPath(_unit, position, _unit.Stats[UnitStat.Movement].ValueInt);
            for (var i = 0; i < path.Length; i++)
            {
                Move(path[i], false);
            }

            PostProcessPath();
            return;
        }

        SpawnObject(position);

        GetCornersSprites(directionNext, out first, out second);
        Head.SetSprites(GetHeadSprite(directionNext), first, second, directionNext);

        if (Length > 2)
        {
            var directionPrev = GridUtility.GetDirection(LastPosition, _positions[_positions.Count - 2], false, true);
            //GetCornersSprites(directionNext, out first, out second);

            _instantiatedObjects[_instantiatedObjects.Count - 2].SetSprites(GetBodySprite(directionPrev, directionNext));
        }

        _positions.Add(position);
        _travelCosts.Add(newCost);
        //Debug.Log(newCost);

        if (Length == 2)
        {
            UpdateTailSprite();
        }
    }

    private void PostProcessPath(int start = 0)
    {
        //if (!PostProcess)
        //  return;

        bool MatchesPattern(int i, Direction first, Direction second, Vector2Int offset)
        {
            if (GridUtility.GetDirection(_positions[i], _positions[i + 1], false, true) == first &&
                GridUtility.GetDirection(_positions[i + 1], _positions[i + 2], false, true) == second &&
                (i > 0 && _positions[i - 1].y == _positions[i + 1].y ||
                 i < Length - 3 && _positions[i + 1].y == _positions[i + 3].y))
            {
                var cellNew = _worldGrid[_positions[i + 1] + offset];
                if (cellNew.Unit != null)
                    return false;

                var cell0 = _worldGrid[_positions[i]];
                var cell1 = _worldGrid[_positions[i + 1]];
                var cell2 = _worldGrid[_positions[i + 2]];

                return cell0.CanMove(cellNew, UnitType) &&
                       cell0.GetTravelCost(cellNew, UnitType) <= cell1.GetTravelCost(cell2, UnitType) &&
                       cellNew.CanMove(cell2, UnitType) &&
                       cellNew.GetTravelCost(cell2, UnitType) <= cell0.GetTravelCost(cell1, UnitType);
            }

            return false;
        }

        void CheckAll(Direction first, Direction second, Vector2Int offset)
        {
            var changes = new Dictionary<int, int>();
            for (var i = start; i < Length - 2; i++)
            {
                if (!MatchesPattern(i, first, second, offset))
                    continue;

                var newPosition = _positions[i + 1] + offset;
                if (_positions.Contains(newPosition))
                {
                    foreach (var key in changes.Keys)
                    {
                        _positions[key] -= new Vector2Int(changes[key], -2);
                        _instantiatedObjects[key].transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) _positions[key]);
                    }
                    
                    //Debug.LogError("REVERT");
                    return;
                }

                _positions[i + 1] = newPosition;
                _instantiatedObjects[i + 1].transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) newPosition);
                
                changes.Add(i + 1, offset.x);
                i = Mathf.Max(0, i - 2);
            }

            // Update sprites
            foreach (var index in changes.Keys)
            {
                for (var i = index - 1; i <= index + 1; i++)
                {
                    if (i == 0)
                        UpdateTailSprite();
                    else if (i == Length - 1)
                    {
                        var direction = GridUtility.GetDirection(_positions[i - 1], _positions[i], false, true);
                        GetCornersSprites(direction, out var sprite1, out var sprite2);
                        _instantiatedObjects[i].SetSprites(GetHeadSprite(direction), sprite1, sprite2, direction);
                    }
                    else
                    {
                        var directionPrev = GridUtility.GetDirection(_positions[i], _positions[i - 1], false, true);
                        var directionNext = GridUtility.GetDirection(_positions[i], _positions[i + 1], false, true);
                        GetCornersSprites(directionPrev.Inverse(), out var sprite1, out var sprite2);
                        _instantiatedObjects[i].SetSprites(GetBodySprite(directionNext, directionPrev), sprite1, sprite2, directionPrev.Inverse());
                    }
                }
            }
        }

        //var t = Time.realtimeSinceStartup;
        if (Length - start > 2)
        {
            CheckAll(Direction.Up, Direction.RightDown, new Vector2Int(1, -2));
            CheckAll(Direction.LeftUp, Direction.Down, new Vector2Int(1, -2));
            CheckAll(Direction.RightUp, Direction.Down, new Vector2Int(-1, -2));
            CheckAll(Direction.Up, Direction.LeftDown, new Vector2Int(-1, -2));
        }

        //Debug.Log(Time.realtimeSinceStartup - t);
    }

    public void Clear()
    {
        foreach (var obj in _instantiatedObjects)
        {
            Destroy(obj.gameObject);
        }

        _instantiatedObjects.Clear();
        _positions.Clear();
        _travelCosts.Clear();
    }

    public GridPath GetPath()
    {
        var positions = new List<Vector2Int>(_positions);
        positions.RemoveAt(0);
        var travelCost = _positions.Sum(pos => _worldGrid[pos].GetTravelCost(UnitType)); 
                        // _worldGrid[positions[0]].GetTravelCost(_unitType);

        // Post-processing for stairs cases
        /*for (var i = 0; i < positions.Count - 2; i++)
        {
            if ((_worldGrid[positions[i]].IsStairs ||
                _worldGrid[positions[i + 2]].IsStairs) &&
                (positions[i] - positions[i + 2]).sqrMagnitude == 2)
            {
                Debug.LogError("This should not happen!");
                positions.RemoveAt(i + 1);
            }
        }*/

        return new GridPath(positions, travelCost);
    }

    private void ResetPath()
    {
        Clear();
        Init(_unit);
    }

    private void SpawnObject(Vector2Int position)
    {
        var obj = Instantiate(_arrowSpritePrefab, _worldGrid.Grid.GetCellCenterWorld((Vector3Int)position), Quaternion.identity, transform);
        _instantiatedObjects.Add(obj);
    }

    private void UpdateTailSprite()
    {
        Tail.SetSprites(null);

        if (Length == 1)
        {
            Tail.ResetSprites();
        }
        else
        {
            var direction = GridUtility.GetDirection(_positions[0], _positions[1], false, true);
            var sprite = GetBodySprite(direction);
            Tail.SetSprites(sprite);
            Tail.Cut(direction);

            
            /*Tail.sprite = sprite;
            Tail.transform.position = 
                _worldGrid.Grid.GetCellCenterWorld((Vector3Int) _unit.GridPosition);

            switch (direction)
            {
                case Direction.Left:
                    Tail.size = new Vector2(0.5f, 1f);
                    Tail.transform.Translate(new Vector2(-0.25f, 0f));
                    break;

                case Direction.Right:
                    Tail.size = new Vector2(0.5f, 1f);
                    Tail.transform.Translate(new Vector2(0.25f, 0f));
                    break;

                case Direction.Up:
                    Tail.size = new Vector2(1f, 0.5f);
                    Tail.transform.Translate(new Vector2(0f, 0.25f));
                    break;

                case Direction.Down:
                    Tail.size = new Vector2(1f, 0.5f);
                    Tail.transform.Translate(new Vector2(0f, -0.25f));
                    break;
            }*/
        }
    }

    private void Truncate(int newSize)
    {
        while (_instantiatedObjects.Count != newSize)
        {
            Destroy(_instantiatedObjects[newSize].gameObject);
            _instantiatedObjects.RemoveAt(newSize);
            _positions.RemoveAt(newSize);
            _travelCosts.RemoveAt(newSize);
        }
    }

    private Sprite GetHeadSprite(Direction direction)
    {
        //Debug.LogError(direction);
        return _arrowHeadSprites[direction];
    }

    private Sprite GetBodySprite(Direction direction)
    {
        if (direction.IsHorizontal())
            return _arrowBodySprites[new DirectionPair(Direction.Left, Direction.Right)];

        if (direction.IsVertical())
            return _arrowBodySprites[new DirectionPair(Direction.Down, Direction.Up)];

        if (direction == Direction.LeftUp || direction == Direction.RightDown)
            return _arrowBodySprites[new DirectionPair(Direction.LeftUp, Direction.RightDown)];

        if (direction == Direction.RightUp || direction == Direction.LeftDown)
            return _arrowBodySprites[new DirectionPair(Direction.LeftDown, Direction.RightUp)];

        return null;
    }

    private Sprite GetBodySprite(Direction directionNext, Direction directionPrev)
    {
        var first = (Direction) Mathf.Min((int) directionNext, (int) directionPrev);
        var second = (Direction) Mathf.Max((int) directionNext, (int) directionPrev);
        //Debug.Log(directionNext + "____" + directionPrev);

        return _arrowBodySprites[new DirectionPair(first, second)];
    }

    private void GetCornersSprites(Direction direction, out Sprite first, out Sprite second)
    {
        if (direction.IsCardinal())
        {
            first = null;
            second = null;
            return;
        }

        var vector = direction.ToVector();
        var firstDirection = DirectionExtensions.FromVector(new Vector2Int(vector.x, -vector.y), true);
        var secondsDirection = DirectionExtensions.FromVector(new Vector2Int(-vector.x, vector.y), true);

        if (vector.x == vector.y)
        {
            first = _cornersSprites[firstDirection];
            second = _cornersSprites[secondsDirection];
        }
        else
        {
            first = _cornersSprites[secondsDirection];
            second = _cornersSprites[firstDirection];
        }
    }
}