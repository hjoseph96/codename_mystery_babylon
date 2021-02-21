using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using UnityEngine;


[SelectionBase]
[RequireComponent(typeof(CellHighlighter))]
public class GridCursor : SerializedMonoBehaviour, IInitializable, IInputTarget
{
    [Header("Movement settings")]
    [SerializeField] private float _totalMovementTime = 0.1f;
    [SerializeField, Range(0, 1)]
    private float _normalizedDistanceThreshold = 0.95f;

    [Header("References")]
    [SerializeField] private ArrowPath _arrowPath;

    public static GridCursor Instance;

    public ActionSelectMenu actionSelectMenu;
    public Vector2Int GridPosition { get; private set; }

    private CursorMode _mode = CursorMode.Free;
    private Vector2Int _targetGridPosition;
    private HashSet<Vector2Int> _allowedPositions;
    private WorldGrid _worldGrid;
    private UserInput _userInput;

    private bool _isMoving;
    private float _movementStartTime;

    private Unit _selectedUnit;

    private Renderer[] _renderers;
    private CellHighlighter _cellHighlighter;
    private ProCamera2D _camera;

    public void Init()
    {
        Instance = this;

        GridPosition = GridUtility.SnapToGrid(this);
        _arrowPath = Instantiate(_arrowPath);

        _worldGrid = WorldGrid.Instance;
        _userInput = UserInput.Instance;
        _userInput.InputTarget = this;

        _camera = ProCamera2D.Instance;
        _renderers = GetComponentsInChildren<Renderer>();
        _cellHighlighter = GetComponent<CellHighlighter>();
        _cellHighlighter.Init();
    }

    private void Update()
    {
        // Move the cursor
        if (_isMoving)
            Move();
    }

    public void ProcessInput(InputData inputData) {
        if (_mode != CursorMode.Locked && inputData.MovementVector != Vector2Int.zero)
        {
            var newPosition = GridPosition + inputData.MovementVector;
            if (_worldGrid.PointInGrid(newPosition))
                StartMovement(newPosition);
        }
        
        switch(_mode) {
            case CursorMode.Free:
                if (inputData.KeyCode == KeyCode.Z) 
                {
                    var unit  = _worldGrid[GridPosition].Unit;
                    if (unit != null && unit.IsLocalPlayerUnit)
                        SetRestrictedMode(unit);
                }
                
                break;

            case CursorMode.Restricted:
                switch (inputData.KeyCode)
                {
                    case KeyCode.Z:
                        if (_allowedPositions.Contains(GridPosition))
                            MoveSelectedUnit();
                        break;

                    case KeyCode.X: case KeyCode.Escape:
                        _arrowPath.Clear();
                        _cellHighlighter.Clear();
                        SetFreeMode();
                        break;
                }

                break;

            case CursorMode.Locked:
                break;

            default:
                throw new Exception("Invalid CursorMode: " + _mode);
        }
    }

    public void SetFreeMode()
    {
        _mode = CursorMode.Free;
        _cellHighlighter.ResetSelectionHighlightingSprite();
    }

    public void SetRestrictedMode(Unit unit, bool attackOnly = false)
    {
        _selectedUnit = unit;
        _mode = CursorMode.Restricted;

        //var t = Time.realtimeSinceStartup;

        _allowedPositions = GridUtility.GetReachableCells(unit);

        var attackPositions = GridUtility.GetAttackableCells(unit, _allowedPositions);
        if (attackOnly) {
            _allowedPositions = new HashSet<Vector2Int>();
            attackPositions = unit.AllAttackableCells();
        }
        
        _cellHighlighter.HighLight(_allowedPositions, attackPositions, _selectedUnit);

        //Debug.Log("Highlighting: " + (Time.realtimeSinceStartup - t));

        _cellHighlighter.UpdateSelectionHighlightingSprite(GridPosition);
        _arrowPath.Init(unit);
    }

    public void SetLockedMode()
    {
        _mode = CursorMode.Locked;
    }

    public void Show()
    {
        foreach (var currentRenderer in _renderers)
            currentRenderer.enabled = true;
    }

    public void Hide()
    {
        foreach (var currentRenderer in _renderers)
            currentRenderer.enabled = false;
    }

    public void MoveInstant(Vector2Int destination)
    {
        GridPosition = destination;
        transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int)GridPosition);
    }

    public void MoveSelectedUnit()
    {
        var path = _arrowPath.GetPath();
        if (path.Length > 0)
            StartCoroutine(MoveSelectedUnitCoroutine(path));
    }

    public void ClearAll()
    {
        _arrowPath.Clear();
        _cellHighlighter.Clear();
    }

    private IEnumerator MoveSelectedUnitCoroutine(GridPath path)
    {
        // Lock and hide cursor
        Hide();
        SetLockedMode();
        // Set camera target to selected unit
        _camera.SetSingleTarget(_selectedUnit.transform);
        _camera.SetCameraWindowMode(CameraWindowMode.Unit);
        // Clear arrow path and all highlightings
        ClearAll();

        // Wait while camera moves to the new target
        while (!_camera.TargetInCameraWindow())
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.05f);

        // Wait for unit to move
        yield return _selectedUnit.MovementCoroutine(path);

        // Unlock and show cursor
        Show();

        // Move to selected unit's position and set camera target to self
        MoveInstant(_selectedUnit.GridPosition);
        _camera.SetSingleTarget(transform);
        _camera.SetCameraWindowMode(CameraWindowMode.Cursor);

        // Temporary workaround, feel free to delete
        /*actionSelectMenu.OnMenuClose = () =>
        {
            // Clear arrow path and all highlightings
            //_arrowPath.Clear();
            //_cellHighlighter.Clear();
            SetRestrictedMode(_selectedUnit);
            // Accept input
            UserInput.Instance.InputTarget = this;
        };*/

        actionSelectMenu.Show(_selectedUnit);
    }

    private void Move()
    {
        // Our movement vector might appear to be diagonal, since we allow diagonal input
        // But since we can not move cursor diagonally, we calculate actual target position
        // E.g. if we move from (1,1) to (2,2), we will use destination position of (2,1) first
        var actualTargetGridPosition = _targetGridPosition;

        // If we have to move horizontally, ignore vertical movement to prevent diagonal movement
        // Continuing example: we replace (2,2) destination with (2,1) here
        if (_targetGridPosition.x != GridPosition.x)
            actualTargetGridPosition.y = GridPosition.y;

        // Calculate world position
        var destination = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) actualTargetGridPosition);

        // Calculate t value for Slerp
        var t = (Time.time - _movementStartTime) / _totalMovementTime;

        // If t is greater than threshold value
        if (t >= _normalizedDistanceThreshold)
        {
            // Move to the destination position
            GridPosition = actualTargetGridPosition;
            transform.position = destination;

            // If we can lengthen the arrow path, do it
            if (_mode == CursorMode.Restricted && _allowedPositions.Contains(GridPosition))
            {
                _arrowPath.Move(GridPosition);
            }

            // Check if we can end movement
            // Continuing example: we just moved from (1,1) to (2,1), but the real destination is (2,2), so we restart movement
            // Next time we will move from (2,1) to (2,2) and end the movement
            EndMovement();

            if (GridPosition != _targetGridPosition)
            {
                StartMovement(_targetGridPosition);
            }
        }
        // If we didn't reach threshold value yet, simply call Slerp
        else
        {
            transform.position = Vector3.Slerp(transform.position, destination, t);
        }
    }

    private void StartMovement(Vector2Int destination)
    {
        _isMoving = true;
        _targetGridPosition = destination;
        _movementStartTime = Time.time;

        _userInput.InputTarget = null;
    }

    private void EndMovement()
    {
        if (_mode == CursorMode.Restricted)
        {
           _cellHighlighter.UpdateSelectionHighlightingSprite(GridPosition);
        }

        _isMoving = false;
        _userInput.InputTarget = this;
    }
}