using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grids2D;

public class GridSelect : MonoBehaviour
{
    public float moveSpeed = 8f;

    GridBattleManager _gridInfo;
    Dictionary<int, Bounds> _cellPositions = new Dictionary<int, Bounds>();

    // Start is called before the first frame update
    public void SetCursor(GridBattleManager grid, Dictionary<int, Bounds> cellPositions) {
        _gridInfo = grid;
        _cellPositions = cellPositions;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

            if (Vector3.Distance(transform.position, destination) < .295) {
                _gridInfo.SelectedCell = GridInterface.CellAtPosition(destination);
                transform.position = destination;
            }
        }
    }

    Vector3 NextDestination(string axis, string lookDirection) {
        Vector3 destination;

        int columnCount = _gridInfo.ColumnCount;
        int currentCellIndex = _gridInfo.SelectedCell.index;

        // EDIT for all the way right, left, up and down.
        switch(lookDirection) {
            case "LEFT":
                destination = _cellPositions[currentCellIndex - 1].center;
                break;
            case "RIGHT":
                destination = _cellPositions[currentCellIndex + 1].center;
                break;
            case "UP":
                destination = _cellPositions[currentCellIndex + columnCount].center;
                break;
            case "DOWN":
                destination = _cellPositions[currentCellIndex - columnCount].center;
                break;
            default:
                throw new System.Exception($"Look Direction: {lookDirection} is invalid.");
        }

        return destination;
    }

}
