using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grids2D;
using Sirenix.OdinInspector;

public class GridSelect : SerializedMonoBehaviour
{
    public float moveSpeed = 8f;
    [SerializeField] private ArrowPath _arrowPathPrefab;

    private List<int> _selectedMovePath = new List<int>();
    public List<int> SelectedMovePath {
        get { return _selectedMovePath; }
        set {
            _selectedMovePath = value;
        }
    }
    GridBattleManager _gridInfo;
    Dictionary<int, Bounds> _cellPositions = new Dictionary<int, Bounds>();
    List<int> _limitedToCells = new List<int>();
    int _movingFromCellIndex;
    int _destinationCellIndex;
    Dictionary<Vector3, GameObject> _arrowSprites = new Dictionary<Vector3, GameObject>();
    Dictionary<string, UnityEngine.Object> _arrowHeadSprites = new Dictionary<string, Object>();
    Dictionary<string, UnityEngine.Object> _arrowPathSprites = new Dictionary<string, Object>();

    private ArrowPath _arrowPath;

    public void SetCursor(GridBattleManager grid, Dictionary<int, Bounds> cellPositions) {
        _gridInfo = grid;
        _cellPositions = cellPositions;
    }

    public void SetMovingMode(List<int> moveableCells, int startingCellIndex, string initialLookingDirection) {
        _limitedToCells = moveableCells;
        _movingFromCellIndex = startingCellIndex;
    }

    public void SetFreeMode() {
        _limitedToCells = new List<int>();
        _movingFromCellIndex = default;
        _arrowPath.Clear();
    }

    public void StartPath(int startingCellIndex)
    {
        //_arrowPath.Init(_cellPositions[startingCellIndex].center);
    }

    private void Awake()
    {
        _arrowPath = Instantiate(_arrowPathPrefab);
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if (input.x != 0 || input.y!= 0) Move(input);
    }


    void Move(Vector2 inputVector) {
        float horizontalInfluence = Mathf.Abs(inputVector.x);
        float verticalInfluence = Mathf.Abs(inputVector.y);

        string direction;
        Vector3 destination = transform.position;

        if (horizontalInfluence > verticalInfluence) {
            direction = inputVector.x < 0 ? "LEFT" : "RIGHT";
            destination = NextDestination("Horizontal", direction);
        }

        if (verticalInfluence > horizontalInfluence) {
            direction = inputVector.y < 0 ? "DOWN" : "UP";
            destination = NextDestination("Vertical", direction);
        }

        if (destination != transform.position) {
            transform.position = Vector3.Slerp(transform.position, destination, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) < .895) {
                transform.position = destination;
                _gridInfo.SelectedCell = GridInterface.CellAtPosition(destination);

                if (_limitedToCells.Count > 0) {
                    
                    //_arrowPath.Move(transform.position);
                }
            }
        }
    }

  
    Vector3 NextDestination(string axis, string lookDirection) {
        Vector3 destination;

        int rowCount = _gridInfo.RowCount;
        int currentCellIndex = _gridInfo.SelectedCell.index;

        // EDIT for all the way right, left, up and down.
        switch(lookDirection) {
            case "LEFT":
                _destinationCellIndex = currentCellIndex - 1;
                destination = _cellPositions[_destinationCellIndex].center;
                break;
            case "RIGHT":
                _destinationCellIndex = currentCellIndex + 1;
                destination = _cellPositions[_destinationCellIndex].center;
                break;
            case "UP":
                _destinationCellIndex = currentCellIndex + rowCount;
                destination = _cellPositions[_destinationCellIndex].center;
                break;
            case "DOWN":
                _destinationCellIndex = currentCellIndex - rowCount;
                destination = _cellPositions[_destinationCellIndex].center;
                break;
            default:
                throw new System.Exception($"Look Direction: {lookDirection} is invalid.");
        }

        // Disallow movement outside cells
        if (_limitedToCells.Count > 0)
            if (!_limitedToCells.Contains(_destinationCellIndex)) 
                return transform.position;

        return destination;
    }
}
