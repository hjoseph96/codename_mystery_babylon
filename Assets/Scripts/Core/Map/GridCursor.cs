using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CursorMode
{
    Free,
    Restricted,
    Locked
}

public class GridCursor : SerializedMonoBehaviour, IInitializable
{
    [Header("Movement settings")]
    [SerializeField] private float _totalMovementTime = 0.1f;
    [SerializeField, Range(0, 1)]
    private float _normalizedDistanceThreshold = 0.95f;

    [Header("Prefab links")]
    [SerializeField] private ArrowPath _arrowPath;
    public GameObject CellMoveHighlightPrefab;
    public GameObject CellAttackHighlightPrefab;

    private CursorMode _mode = CursorMode.Free;
    private Vector2Int _gridPosition, _targetGridPosition;
    private List<Vector2Int> _allowedPositions;
    private WorldGrid _worldGrid;

    private bool _isMoving;
    private float _movementStartTime;

    private readonly List<GameObject> _highlightedCells = new List<GameObject>();

    public void Init()
    {
        _gridPosition = GridUtility.SnapToGrid(this);
        _arrowPath = Instantiate(_arrowPath);
    }

    private void  Start() {
        _worldGrid = WorldGrid.Instance;
    }

    private void Update()
    {
        // Make these into functions explaining what they do...
        if (_isMoving)
        {
            var actualTargetGridPosition = _targetGridPosition;
            var movementTime = _totalMovementTime;

            // If we have to move horizontally, ignore vertical movement to prevent diagonal movement
            if (_targetGridPosition.x != _gridPosition.x)
                actualTargetGridPosition.y = _gridPosition.y;

            var destination = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) actualTargetGridPosition);
            var t = (Time.time - _movementStartTime) / movementTime;
            if (t >= _normalizedDistanceThreshold)
            {
                _gridPosition = actualTargetGridPosition;
                if (_mode == CursorMode.Restricted && _allowedPositions.Contains(_gridPosition))
                    _arrowPath.Move(_gridPosition);


                transform.position = destination;

                if (_gridPosition == _targetGridPosition)
                {
                    _isMoving = false;
                }
                else
                {
                    _movementStartTime = Time.time;
                }
            }
            else
            {
                transform.position = Vector3.Slerp(transform.position, destination, t);
            }

            return;
        }

        if (_mode == CursorMode.Locked)
            return;

        var input = new Vector2Int(Math.Sign(Input.GetAxisRaw("Horizontal")),
                                   Math.Sign(Input.GetAxisRaw("Vertical")));
        
        if (input != Vector2Int.zero)
        {
            var newPosition = _gridPosition + input;
            if (_worldGrid.PointInGrid(newPosition))
            {
                _targetGridPosition = newPosition;
                _isMoving = true;
                _movementStartTime = Time.time;
            }
        }

        
        switch(_mode) {
            case CursorMode.Free:
                if (Input.GetKeyDown(KeyCode.Z)) 
                {
                    Unit unit  = _worldGrid[_gridPosition].Unit;
                    if (unit != null)
                    {
                        var positions = GridUtility.GetReachableCells(unit, out var attackPositions, unit.MovePoints, unit.MinAttackRange, unit.MaxAttackRange);
                        GenerateMoveHighlights(positions);

                        _arrowPath.Init(unit);
                        SetRestrictedMode(positions);

                        GenerateAttackHighlights(attackPositions);
                    }
                }
                
                break;
            case CursorMode.Restricted:
                if (Input.GetKeyDown(KeyCode.X))
                {
                    ClearAllHighlights();
                    SetFreeMode();
                }
                break;
            default:
                throw new Exception("Invalid CursorMode: " + _mode);
        }
    }

    public void SetFreeMode()
    {
        _mode = CursorMode.Free;
    }

    public void SetRestrictedMode(List<Vector2Int> allowedPositions)
    {
        _mode = CursorMode.Restricted;
        _allowedPositions = allowedPositions;
    }

    public void Lock()
    {
        _mode = CursorMode.Locked;
    }

    private void GenerateMoveHighlights(List<Vector2Int> positions) {
        foreach (var pos in positions)
        {
            var obj = Instantiate(CellMoveHighlightPrefab, _worldGrid.Grid.GetCellCenterWorld((Vector3Int) pos), Quaternion.identity);
            _highlightedCells.Add(obj);
        }
    }

    private void GenerateAttackHighlights(List<Vector2Int> attackablePositions) {
        foreach (var pos in attackablePositions)
        {
            var obj = Instantiate(CellAttackHighlightPrefab, _worldGrid.Grid.GetCellCenterWorld((Vector3Int)pos), Quaternion.identity);
            _highlightedCells.Add(obj);
        }
    }

    private void ClearAllHighlights() {
        foreach (var obj in _highlightedCells)
            Destroy(obj);

        _highlightedCells.Clear();
        _arrowPath.Clear();
    }
}
