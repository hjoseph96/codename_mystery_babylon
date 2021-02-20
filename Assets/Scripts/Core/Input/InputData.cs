using UnityEngine;

public class InputData
{
    public Vector2Int MovementVector;
    public KeyCode KeyCode;

    public int Horizontal => MovementVector.x;
    public int Vertical => MovementVector.y;

    public InputData() { }

    public InputData(Vector2Int movementVector, KeyCode keyCode)
    {
        MovementVector = movementVector;
        KeyCode = keyCode;
    }
}