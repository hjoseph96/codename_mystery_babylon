using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;


[SelectionBase]
[RequireComponent(typeof(CellHighlighter))]
public class GridCursor : SerializedMonoBehaviour, IInitializable, IInputTarget
{
    public static GridCursor Instance;
    
    [Header("Cursor State")]
    [SerializeField] private CursorMode _mode = CursorMode.Free;

    [Header("Audio")]
    [SoundGroup] public string selectedUnitSound;
    [SoundGroup] public string deselectedUnitSound;
    [SoundGroup] public string cursorMoveSound;
    [SoundGroup] public string notAllowedSound;


    [Header("Movement settings")]
    [SerializeField] private float _totalMovementTime = 0.1f;
    [SerializeField, Range(0, 1)]
    private float _normalizedDistanceThreshold = 0.95f;
    public List<ProCamera2DTriggerBoundaries> CameraBoundaries;

    [Header("References")]
    [SerializeField] private ArrowPath _arrowPath;


    public Vector2Int GridPosition { get; private set; }
    public bool IsMoving { get; private set; }

    // Those fields only used in CursorMode.RestrictedToList
    private Action<Vector2Int> _onPositionChanged, _onCellSelected;
    private Action _onExit;

    private Vector2Int _targetGridPosition;
    private HashSet<Vector2Int> _allowedPositions;
    private List<Vector2Int> _restrictedPositionsList;
    private WorldGrid _worldGrid;
    private UserInput _userInput;

    private float _movementStartTime;

    private Unit _selectedUnit;
    private ActionSelectMenu _actionSelectMenu;

    private Renderer[] _renderers;
    private CellHighlighter _cellHighlighter;
    private ProCamera2D _camera;

    private Unit _hoveringUnit;


    public void Init()
    {
        Instance = this;

        GridPosition = GridUtility.SnapToGrid(this);
        _arrowPath = Instantiate(_arrowPath);

        _worldGrid = WorldGrid.Instance;
        _userInput = UserInput.Instance;
        _camera    = CampaignManager.Instance.GridCamera;
        _actionSelectMenu = UIManager.Instance.GridBattleCanvas.ActionSelectMenu;
        _userInput.InputTarget = this;

        _renderers = GetComponentsInChildren<Renderer>();
        _cellHighlighter = GetComponent<CellHighlighter>();
        _cellHighlighter.Init();
    }

    private void Update()
    {
        // Move the cursor
        if (IsMoving)
            Move();

        // Show & Hide Hovering Unit's health
        var hoveringUnit = _worldGrid[GridPosition].Unit;
        if (hoveringUnit != null)
        {
            _hoveringUnit = hoveringUnit;
            _hoveringUnit.ShowHealthBar();
        }
        else if (_hoveringUnit != null)
        {
            _hoveringUnit.HideHealthBar();
            _hoveringUnit = null;
        }

    }

    public void ProcessInput(InputData inputData) 
    {
        if (inputData.MovementVector != Vector2Int.zero) 
        {
            if (_mode != CursorMode.Locked && _mode != CursorMode.RestrictedToList)
            {
                var newPosition = GridPosition + inputData.MovementVector;
                var worldPosition = _worldGrid.Grid.GetCellCenterWorld((Vector3Int)newPosition);
                if (_worldGrid.PointInGrid(newPosition) && IsWithinCameraBounds(worldPosition))
                    StartMovement(newPosition);
            }
        }

        
        
        switch(_mode) {
            case CursorMode.Free:
                if (inputData.KeyCode == KeyCode.Z && inputData.KeyState == KeyState.Up) 
                {
                    var unit  = _worldGrid[GridPosition].Unit;

                    if (unit != null)
                    {
                        switch(unit.TeamId)
                        {
                            case Team.LocalPlayerTeamId:
                                if (!unit.HasTakenAction)
                                {
                                    MasterAudio.PlaySound3DFollowTransform(selectedUnitSound, CampaignManager.AudioListenerTransform);
                                    SetRestrictedMode(unit);
                                }
                                else
                                    MasterAudio.PlaySound3DAtTransform(notAllowedSound, CampaignManager.AudioListenerTransform);

                                break;
                        }
                    }
                    
                }
                
                break;

            case CursorMode.Restricted:
                if (inputData.KeyState != KeyState.Up)
                    break;

                switch (inputData.KeyCode)
                {
                    case KeyCode.Z:
                        if (_allowedPositions.Contains(GridPosition))
                            MoveSelectedUnit();
                        break;

                    case KeyCode.X: 
                    case KeyCode.Escape:
                        MasterAudio.PlaySound3DFollowTransform(deselectedUnitSound, CampaignManager.AudioListenerTransform);
                        ClearAll();
                        SetFreeMode();
                        break;
                }

                break;

            case CursorMode.RestrictedToList:
                if (inputData.MovementVectorPressed != Vector2.zero)
                {
                    var input = Mathf.RoundToInt(Mathf.Clamp(inputData.MovementVectorPressed.x + inputData.MovementVectorPressed.y, -1, 1));

                    var currentCellIndex = _restrictedPositionsList.IndexOf(GridPosition);
                    var nextAttackTargetIndex = currentCellIndex + input;

                    if (nextAttackTargetIndex < 0) nextAttackTargetIndex = _restrictedPositionsList.Count - 1;
                    if (nextAttackTargetIndex > _restrictedPositionsList.Count - 1) nextAttackTargetIndex = 0;

                    var newPosition = _restrictedPositionsList[nextAttackTargetIndex];

                    if (_worldGrid.PointInGrid(newPosition))
                        StartMovement(newPosition);
                }
                else if (inputData.KeyState == KeyState.Up)
                {
                    switch (inputData.KeyCode)
                    {
                        case KeyCode.Z:
                            _onCellSelected?.Invoke(GridPosition); 
                            break;

                        case KeyCode.X:
                            _onExit?.Invoke();
                            break;
                    }
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
        _userInput.InputTarget = this;
    }

    public void SetRestrictedMode(Unit unit)
    {
        _userInput.InputTarget = this;

        _selectedUnit = unit;
        _mode = CursorMode.Restricted;

        _allowedPositions = GridUtility.GetReachableCells(unit);

        var attackPositions = GridUtility.GetAttackableCells(unit, _allowedPositions);
        
        _cellHighlighter.HighLight(_allowedPositions, attackPositions, _selectedUnit);
        _cellHighlighter.UpdateSelectionHighlightingSprite(unit.SortingLayerId, GridPosition);
        _arrowPath.Init(unit);
    }

    public void SetRestrictedToListMode(List<Vector2Int> cells, Action<Vector2Int> onMove, Action<Vector2Int> onSelect, Action onExit)
    {
        _cellHighlighter.HighlightWithMode(cells, _worldGrid[GridPosition].Unit, HighlightingMode.Attack);

        _restrictedPositionsList = cells;
        _onPositionChanged = onMove;
        _onCellSelected = onSelect;
        _onExit = onExit;

        _mode = CursorMode.RestrictedToList;
    }

    public void ExitRestrictedMode()
    {
        ClearAll();

        _restrictedPositionsList = null;
        _onCellSelected = null;
        _onPositionChanged = null;
        _onExit = null;

        SetLockedMode();
    }

    public void SetLockedMode() => _mode = CursorMode.Locked;

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

    public void ResetUnit(Unit unit)
    {
        unit.ResetToInitialPosition();
        MoveInstant(unit.GridPosition);

        MasterAudio.PlaySound3DFollowTransform(notAllowedSound, CampaignManager.AudioListenerTransform);

        SetFreeMode();
    }

    public void MoveInstant(Vector2Int destination)
    {
        GridPosition = destination;
        transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int)GridPosition);
    }

    public void MoveSelectedUnit()
    {
        var path = _arrowPath.GetPath();
        StartCoroutine(MoveSelectedUnitCoroutine(path));
    }

    public void ClearAll()
    {
        _arrowPath.Clear();
        _cellHighlighter.Clear();
    }

    public void SetAsCameraTarget() 
    {
        _camera.SetSingleTarget(transform);
        _camera.SetCameraWindowMode(CameraWindowMode.Cursor);
    }

    private IEnumerator MoveSelectedUnitCoroutine(GridPath path)
    {
        if (path.Length == 0)
        {
            ClearAll();
            ShowActionSelectMenu();
            yield break;
        }

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

        ShowActionSelectMenu();
    }

    private void ShowActionSelectMenu()
    {
        var gridBattleCanvas = UIManager.Instance.GridBattleCanvas;
        (_actionSelectMenu.transform as RectTransform).anchoredPosition =
            gridBattleCanvas.WorldToCanvas(_selectedUnit.transform.position) + new Vector2(195, 45);
        _actionSelectMenu.Show(_selectedUnit);
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
            MasterAudio.PlaySound3DFollowTransform(cursorMoveSound, CampaignManager.AudioListenerTransform);


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
            else if (_mode == CursorMode.RestrictedToList)
            {
                _onPositionChanged?.Invoke(GridPosition);
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
        IsMoving = true;
        _targetGridPosition = destination;
        _movementStartTime = Time.time;

        if (_userInput.InputTarget == this as IInputTarget)
            _userInput.InputTarget = null;
    }

    private void EndMovement()
    {
        if (_mode == CursorMode.Restricted)
           _cellHighlighter.UpdateSelectionHighlightingSprite(_selectedUnit.SortingLayerId, GridPosition);

        IsMoving = false;

        if (_userInput.InputTarget == null)
            _userInput.InputTarget = this;
    }

    private bool IsWithinCameraBounds(Vector3 worldPosition)
    {
        if (CameraBoundaries.Count == 0)
            return true;

        var isWithinBoundary = false;
        foreach (var boundary in CameraBoundaries)
            if (boundary.IsWithinTrigger(worldPosition))
                isWithinBoundary = true;

        return isWithinBoundary;
    }
}