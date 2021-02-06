using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    None,
    Left,
    Down,
    Right,
    Up
}

public class ArrowPath : MonoBehaviour
{
    [Header("Arrow body sprites")]
    [SerializeField] private Sprite _arrowHorizontal;
    [SerializeField] private Sprite _arrowVertical;
    [SerializeField] private Sprite _arrowLeftUp;
    [SerializeField] private Sprite _arrowRightUp;
    [SerializeField] private Sprite _arrowLeftDown;
    [SerializeField] private Sprite _arrowRightDown;

    [Header("Arrow head sprites")]
    [SerializeField] private Sprite _arrowHeadLeft;
    [SerializeField] private Sprite _arrowHeadRight;
    [SerializeField] private Sprite _arrowHeadUp;
    [SerializeField] private Sprite _arrowHeadDown;

    #region Private fields and properties

    private Unit _unit;

    private readonly List<SpriteRenderer> _instantiatedObjects = new List<SpriteRenderer>();
    private readonly List<Vector2Int> _positions = new List<Vector2Int>();
    private readonly List<int> _travelCosts = new List<int>();

    private SpriteRenderer Head => _instantiatedObjects.Count > 0 ? _instantiatedObjects[_instantiatedObjects.Count - 1] : null;
    private SpriteRenderer Tail => _instantiatedObjects[0];
    private int Length => _instantiatedObjects.Count;
    private Vector2Int LastPosition => _positions[_positions.Count - 1];
    private int LastTravelCost => _travelCosts[_travelCosts.Count - 1];

    #endregion

    public void Init(Unit unit)
    {
        _positions.Clear();
        _unit = unit;

        _positions.Add(_unit.GridPosition);
        _travelCosts.Add(0);

        SpawnObject(_unit.GridPosition);
        Tail.drawMode = SpriteDrawMode.Sliced;
    }

    public void Move(Vector2Int position)
    {
        // Self intersection
        var index = _positions.IndexOf(position);

        if (index == 0)
        {
            ResetPath();
            return;
        }
        
        if (index > 0)
        {
            if (index == 1)
            {
                UpdateTailSprite();
            }

            Truncate(index + 1);
            if (Head != null)
            {
                var direction = GridUtility.GetDirection(_positions[_positions.Count - 2], LastPosition);
                Head.sprite = GetHeadSprite(direction);
            }

            return;
        }

        // Calculate new cost as last path point cost + new cell cost
        var newCost = LastTravelCost + WorldGrid.Instance[position].TravelCost(UnitType.None);

        // Calculate direction
        var directionOut = GridUtility.GetDirection(LastPosition, position);

        // If we can't move to the desired position (exit/entrance is blocked) or direction is None
        if (!WorldGrid.Instance[LastPosition].CanMove(position, UnitType.None) || directionOut == Direction.None)
        {
            // Calculate new path and extend old path with it
            var path = GridUtility.FindPath(_unit, LastPosition, position);
            newCost = LastTravelCost + (int) path.TravelCost;
            if (newCost <= _unit.MovePoints)
            {
                for (var i = 0; i < path.Length; i++)
                {
                    Move(path[i]);
                }

                return;
            }
        }

        // If we ran out of cost, calculate best possible path with A* and use it instead
        if (newCost > _unit.MovePoints)
        {
            ResetPath();
            var path = GridUtility.FindPath(_unit, position);
            for (var i = 0; i < path.Length; i++)
            {
                Move(path[i]);
            }

            return;
        }

        SpawnObject(position);
        Head.sprite = GetHeadSprite(directionOut);

        if (Length > 2)
        {
            var directionIn = GridUtility.GetDirection(_positions[_positions.Count - 2], _positions[_positions.Count - 1]);
            _instantiatedObjects[_instantiatedObjects.Count - 2].sprite = GetBodySprite(directionIn, directionOut);
        }

        _positions.Add(position);
        _travelCosts.Add(newCost);

        if (Length == 2)
        {
            UpdateTailSprite();
        }
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

    private void ResetPath()
    {
        Clear();
        Init(_unit);
    }

    private void SpawnObject(Vector2Int position)
    {
        var go = new GameObject("Arrow segment");
        go.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)position);
        go.transform.parent = transform;
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        
        // TODO: Correct sorting order/sorting layer
        spriteRenderer.sortingOrder = 10;
        _instantiatedObjects.Add(spriteRenderer);
    }

    private void UpdateTailSprite()
    {
        if (Length == 1)
        {
            Tail.sprite = null;
        }
        else
        {
            var direction = GridUtility.GetDirection(_positions[0], _positions[1]);
            var sprite = GetBodySprite(direction);
            Tail.sprite = sprite;
            Tail.transform.position = 
                WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int) _unit.GridPosition);

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
            }
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
        if (direction == Direction.Left)
            return _arrowHeadLeft;
        
        if (direction == Direction.Right)
            return _arrowHeadRight;

        if (direction == Direction.Up)
            return _arrowHeadUp;

        if (direction == Direction.Down)
            return _arrowHeadDown;

        return null;
    }

    private Sprite GetBodySprite(Direction direction)
    {
        if (direction == Direction.Left || direction == Direction.Right)
            return _arrowHorizontal;

        if (direction == Direction.Up || direction == Direction.Down)
            return _arrowVertical;

        return null;
    }

    private Sprite GetBodySprite(Direction directionIn, Direction directionOut)
    {
        if (directionIn == directionOut)
            return GetBodySprite(directionIn);

        if (directionIn == Direction.Left)
        {
            if (directionOut == Direction.Up)
            {
                return _arrowLeftUp;
            }
            else if (directionOut == Direction.Down)
            {
                return _arrowLeftDown;
            }
        }
        else if (directionIn == Direction.Right)
        {
            if (directionOut == Direction.Up)
            {
                return _arrowRightUp;
            }
            else if (directionOut == Direction.Down)
            {
                return _arrowRightDown;
            }
        }
        else if (directionIn == Direction.Up)
        {
            if (directionOut == Direction.Left)
            {
                return _arrowRightDown;
            }
            else if (directionOut == Direction.Right)
            {
                return _arrowLeftDown;
            }
        }
        else if (directionIn == Direction.Down)
        {
            if (directionOut == Direction.Left)
            {
                return _arrowRightUp;
            }
            else if (directionOut == Direction.Right)
            {
                return _arrowLeftUp;
            }
        }

        return null;
    }
}