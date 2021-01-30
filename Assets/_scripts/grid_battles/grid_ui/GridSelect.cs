using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grids2D;
using Sirenix.OdinInspector;

public class GridSelect : SerializedMonoBehaviour
{
    public float moveSpeed = 8f;
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

    public void SetCursor(GridBattleManager grid, Dictionary<int, Bounds> cellPositions) {
        _gridInfo = grid;
        _cellPositions = cellPositions;
    }

    public void SetMovingMode(List<int> moveableCells, int startingCellIndex, string initialLookingDirection) {
        _limitedToCells = moveableCells;
        _movingFromCellIndex = startingCellIndex;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if (input.x != 0 || input.y!= 0) Move(input);

        if (_limitedToCells.Count > 0) {
            _arrowHeadSprites = _gridInfo.ArrowHeadSprites;
            _arrowPathSprites = _gridInfo.ArrowPathSprites;

            if (input.x == 0 && input.y == 0) _destinationCellIndex = -1;


            SetSelectedPath();
            if (_movingFromCellIndex != _gridInfo.SelectedCell.index) {
                DrawArrowPath();
                
                string movePathDebug = "Selected Move Path: ";
                foreach(int cellIndex in _selectedMovePath) movePathDebug += $" {cellIndex}"; 
                Debug.Log(movePathDebug);


                Debug.Log($"Selected Cell Index: {_gridInfo.SelectedCell.index}");
                Debug.Log($"Destination Cell Index: {_destinationCellIndex}");
            } else {
                _selectedMovePath = new List<int>();
                ClearArrows();
            }
        }
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

            if (Vector3.Distance(transform.position, destination) < .295) {
                transform.position = destination;

                if (_limitedToCells.Count > 0) {
                    if (_selectedMovePath.Contains(_destinationCellIndex)) {
                        int firstIndex = _selectedMovePath.IndexOf(_destinationCellIndex);
                        int lastindex = _selectedMovePath.Count - 1;
                        int destroyRange = lastindex - firstIndex;
                        // Logic for removing duplicate cells from user drawn path
                        _selectedMovePath.RemoveRange(firstIndex, destroyRange);
                        

                        ClearArrows();
                    }
                }

                _gridInfo.SelectedCell = GridInterface.CellAtPosition(destination);
            }
        }
    }

    string DirectionFromCell(int lastCellIndex, int currentCellIndex) {
        Vector3 lastCellPosition = _cellPositions[lastCellIndex].center;
        Vector3 nextCellPosition = _cellPositions[currentCellIndex].center;

        if (lastCellPosition.y > nextCellPosition.y) return "DOWN";
        if (lastCellPosition.y < nextCellPosition.y) return "UP";
        if (lastCellPosition.x < nextCellPosition.x) return "RIGHT";
        if (lastCellPosition.x > nextCellPosition.x) return "LEFT";

        throw new System.Exception("Unable to determine directions...");
    }
    string DirectionToCell(int currentCellIndex, int nextCellIndex) {
        Vector3 currentCellPosition = _cellPositions[currentCellIndex].center;
        Vector3 nextCellPosition = _cellPositions[nextCellIndex].center;

        if (currentCellPosition.y > nextCellPosition.y) return "DOWN";
        if (currentCellPosition.y < nextCellPosition.y) return "UP";
        if (currentCellPosition.x > nextCellPosition.x) return "LEFT";
        if (currentCellPosition.x < nextCellPosition.x) return "RIGHT";

        throw new System.Exception("Unable to determine directions...");

    }

    void ClearArrows() {
        List<Vector3> toBeDestroyed = new List<Vector3>();
        foreach(KeyValuePair<Vector3, GameObject> entry in _arrowSprites)
            toBeDestroyed.Add(entry.Key);
        
        foreach(Vector3 position in toBeDestroyed) {
            GameObject arrowSprite = _arrowSprites[position];

            Destroy(arrowSprite);

            _arrowSprites.Remove(position);
        }
    }


    void DrawArrowPath() {
        List<int> resetRangeCellIndexes = _gridInfo.Grid.CellGetNeighbours(_movingFromCellIndex, 2);

        if (_selectedMovePath.Count == 1) {
            int singleCellIndex = _selectedMovePath[0];
            Vector3 singleCellPosition = _cellPositions[singleCellIndex].center;
            string direction = DirectionFromCell(_movingFromCellIndex, singleCellIndex);

            AddArrowHead(direction, singleCellPosition);
            return;
        }

        for(int i = 0; i < _selectedMovePath.Count; i++) {
            int cellIndex = _selectedMovePath[i];
            Vector3 cellCenter = _cellPositions[cellIndex].center;

            int lastCellIndex = _movingFromCellIndex;
            if (_selectedMovePath.IndexOf(cellIndex) > 0) lastCellIndex = _selectedMovePath[i - 1];

            string fromDirection = DirectionFromCell(lastCellIndex, cellIndex);
            
            bool isLastCell = _selectedMovePath.IndexOf(cellIndex) == _selectedMovePath.Count - 1;
            if (isLastCell == false) {
                int nextCellIndex = _selectedMovePath[i + 1];
                string toDirection = DirectionToCell(cellIndex,nextCellIndex);
                
                AddArrowPath(fromDirection, toDirection, cellCenter);
            } else {
                AddArrowHead(fromDirection, cellCenter, lastCellIndex);
            }
        }

    }

    void AddArrowHead(string direction, Vector3 position, int lastCellIndex = -1) {
        int headIndex = _selectedMovePath[_selectedMovePath.Count - 1];
        int rowCount = _gridInfo.Grid.rowCount;

        bool goingBackwards;
        switch (direction) {
                case "UP":
                    goingBackwards = lastCellIndex != -1 && lastCellIndex < headIndex && _selectedMovePath.Contains(headIndex + rowCount);
                    if (goingBackwards) {
                        _selectedMovePath.RemoveAt(_selectedMovePath.IndexOf(lastCellIndex));
                        ClearArrows();

                        foreach(int horizontalNeighborIndex in _gridInfo.GetHorizontalNeighbors(headIndex)) {
                            if (_selectedMovePath.Contains(horizontalNeighborIndex)) {
                                string diretion = DirectionFromCell(horizontalNeighborIndex, headIndex);
                                InstantiateArrowHead($"HORIZONTAL - {direction}", position);
                                return;
                            }

                        }

                        InstantiateArrowHead("VERTICAL - DOWN", position);

                        break;
                    }

                    InstantiateArrowHead("VERTICAL - UP", position);
                    break;
                case "DOWN":
                    goingBackwards = lastCellIndex != -1 && lastCellIndex == headIndex + 100 && _selectedMovePath.Contains(headIndex - rowCount);
                    if (goingBackwards) {
                        _selectedMovePath.RemoveAt(_selectedMovePath.IndexOf(lastCellIndex));
                        ClearArrows();
                        
                        foreach(int horizontalNeighborIndex in _gridInfo.GetHorizontalNeighbors(headIndex)) {
                            if (_selectedMovePath.Contains(horizontalNeighborIndex)) {
                                string diretion = DirectionFromCell(horizontalNeighborIndex, headIndex);
                                InstantiateArrowHead($"HORIZONTAL - {direction}", position);
                                return;
                            }

                        }
                        
                        InstantiateArrowHead("VERTICAL - UP", position);

                        break;
                    }

                    InstantiateArrowHead("VERTICAL - DOWN", position);
                    break;
                case "LEFT":
                    goingBackwards = lastCellIndex != -1 && lastCellIndex > headIndex && _selectedMovePath.Contains(headIndex-1);
                    if (goingBackwards) {
                        _selectedMovePath.RemoveAt(_selectedMovePath.IndexOf(lastCellIndex));
                        ClearArrows();

                        foreach(int verticalNeighborIndex in _gridInfo.GetVerticalNeighbors(headIndex)) {
                            if (_selectedMovePath.Contains(verticalNeighborIndex)) {
                                string diretion = DirectionFromCell(verticalNeighborIndex, headIndex);
                                InstantiateArrowHead($"VERTICAL - {direction}", position);
                                return;
                            }

                        }

                        InstantiateArrowHead("HORIZONTAL - RIGHT", position);
                        break;
                    }

                    InstantiateArrowHead("HORIZONTAL - LEFT", position);
                    break;
                case "RIGHT":
                    goingBackwards = lastCellIndex != -1 && lastCellIndex < headIndex && _selectedMovePath.Contains(headIndex+1);
                    if (goingBackwards) {
                        _selectedMovePath.RemoveAt(_selectedMovePath.IndexOf(lastCellIndex));
                        ClearArrows();

                        foreach(int verticalNeighborIndex in _gridInfo.GetVerticalNeighbors(headIndex)) {
                            if (_selectedMovePath.Contains(verticalNeighborIndex)) {
                                string diretion = DirectionFromCell(verticalNeighborIndex, headIndex);
                                InstantiateArrowHead($"VERTICAL - {direction}", position);
                                return;
                            }

                        }

                        InstantiateArrowHead("HORIZONTAL - LEFT", position);

                        break;
                    }
                    
                    InstantiateArrowHead("HORIZONTAL - RIGHT", position);
                    break;
            }
    }

    void InstantiateArrowHead(string keyName, Vector3 position) {
        if (!_arrowSprites.ContainsKey(position))
             _arrowSprites.Add(
                position,
                Instantiate(
                    _arrowHeadSprites[keyName], 
                    position, 
                    Quaternion.identity
                ) as GameObject
            );
    }


    void AddArrowPath(string fromDirection, string toDirection, Vector3 position) {
        if (fromDirection == "RIGHT" && toDirection == "UP") {
            InstantiateArrowPath("VERTICAL - LEFT - FROM TOP", position);

            return;
        }

        if (fromDirection == "LEFT" && toDirection == "UP") {
            InstantiateArrowPath("VERTICAL - RIGHT - FROM TOP", position);

            return;
        }

        if (toDirection == "DOWN") {
            if (fromDirection == "DOWN") {
                AddStraightVerticalArrow(position);
            } else {
                AddHorizontalDownArrows(fromDirection, position);
            }
        }

        if (toDirection == "UP") {
            if (fromDirection == "UP") {
                AddStraightVerticalArrow(position);
            } else {
                AddHorizontalUpArrows(fromDirection, position);
            }
        }

        if (toDirection == "LEFT") {
            if (fromDirection =="LEFT") {
                AddStraightHorizontalArrow(position);
            } else {
                AddVerticalLeftArrows(fromDirection, position);
            }
        }
                
        if (toDirection == "RIGHT") {
            if (fromDirection == "RIGHT") {
                AddStraightHorizontalArrow(position);
            } else {
                AddVerticalRightArrows(fromDirection, position);
            }
        }
    }

    void InstantiateArrowPath(string keyName, Vector3 position, bool force = false) {
        if (!_arrowSprites.ContainsKey(position))
             _arrowSprites.Add(
                position,
                Instantiate(
                    _arrowPathSprites[keyName], 
                    position, 
                    Quaternion.identity
                ) as GameObject
            );
        
        if (force){
            if (_arrowSprites.ContainsKey(position)) {
                Destroy(_arrowSprites[position]);

                _arrowSprites.Remove(position);
            }
        
             _arrowSprites.Add(
                position,
                Instantiate(
                    _arrowPathSprites[keyName], 
                    position, 
                    Quaternion.identity
                ) as GameObject
            );
        
        }
    }
    void AddStraightVerticalArrow(Vector3 position) {
        InstantiateArrowPath("VERTICAL", position, true);
    }

    
    void AddStraightHorizontalArrow(Vector3 position) {
        InstantiateArrowPath("HORIZONTAL", position, true);
    }

    void AddHorizontalDownArrows(string fromDirection, Vector3 position) {
        switch (fromDirection) {
            case "RIGHT":
                InstantiateArrowPath("HORIZONTAL - DOWN - FROM LEFT", position);

                break;
            case "LEFT":
                InstantiateArrowPath("HORIZONTAL - DOWN - FROM RIGHT", position);

                break;
        }
    }

    
    void AddHorizontalUpArrows(string fromDirection, Vector3 position) {
        switch (fromDirection) {
            case "RIGHT":
                InstantiateArrowPath("HORIZONTAL - UP - FROM RIGHT", position);

                break;
            case "LEFT":
                InstantiateArrowPath("HORIZONTAL - UP - FROM LEFT", position);

                break;
        }
    }

    void AddVerticalLeftArrows(string fromDirection, Vector3 position) {
        switch (fromDirection) {
            case "DOWN":
                InstantiateArrowPath("VERTICAL - LEFT - FROM TOP", position);

                break;
            case "UP":
                InstantiateArrowPath("VERTICAL - LEFT - FROM BOTTOM", position);

                break;
        }
    }

    void AddVerticalRightArrows(string fromDirection, Vector3 position) {
        switch (fromDirection) {
            case "DOWN":
                InstantiateArrowPath("VERTICAL - RIGHT - FROM TOP", position);

                break;
            case "UP":
                InstantiateArrowPath("VERTICAL - RIGHT - FROM BOTTOM", position);

                break;
        }
    }
    void SetSelectedPath() {
        if (_destinationCellIndex == _movingFromCellIndex) {
            SelectedMovePath = new List<int>();
            return;
        }

        if (_destinationCellIndex != -1) {

            List<Cell> immediateNeighbors = _gridInfo.Grid.CellGetNeighbours(_movingFromCellIndex);
            Cell destinationCell = _gridInfo.Grid.cells[_destinationCellIndex];
            
            // Check if destination cell is next door, if so, empty the path 
            if (immediateNeighbors.Contains(destinationCell)) {
                SelectedMovePath = new List<int>();
            }

            if (!_selectedMovePath.Contains(_destinationCellIndex)) {
                // Only Add to move path after arriving to cell
                if (!_limitedToCells.Contains(_destinationCellIndex))
                    return;
                
                Vector3 destination = _cellPositions[_destinationCellIndex].center;
                if (Vector3.Distance(transform.position, destination) < 0.04)
                    AddToSelectedMovePath(_destinationCellIndex);
            }
        } else {
            int selectedCellIndex = _gridInfo.SelectedCell.index;

            if (!_selectedMovePath.Contains(selectedCellIndex))
                _selectedMovePath.Add(selectedCellIndex);
        }

    }

    void RemoveFromSelectedMovePath(int index) {
        if (Vector3.Distance(transform.position, _cellPositions[index].center) < 0.295)
            _selectedMovePath.RemoveAt(index);
        ClearArrows();
    }

    void AddToSelectedMovePath(int cellIndex) {
        SelectedMovePath.Add(cellIndex);
        ClearArrows();
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
