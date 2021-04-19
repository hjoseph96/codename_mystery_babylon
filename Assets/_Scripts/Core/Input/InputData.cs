using UnityEngine;

public enum KeyState
{
    Up,
    Down
}

public class InputData
{
    public Vector2Int MovementVector; // This is a movement vector2 produced by arrow keys pressed OR held in the current frame
    public Vector2Int MovementVectorPressed; // This is a movement vector produced by arrow keys pressed in the current frame

    public KeyCode KeyCode;
    public KeyState KeyState;

    public int Horizontal => MovementVector.x;
    public int Vertical => MovementVector.y;
}