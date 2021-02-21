using System;
using UnityEngine;

public class UserInput : MonoBehaviour, IInitializable
{
    private readonly KeyCode[] AvailableInputKeys =
    {
        KeyCode.X,
        KeyCode.Z,
        KeyCode.Q,
        KeyCode.E,
        KeyCode.Escape,
        KeyCode.Space
    };

    public static UserInput Instance;
    public IInputTarget InputTarget;

    private readonly InputData _inputData = new InputData();

    public void Init()
    {
        Instance = this;
    }

    private void Update()
    {
        if (InputTarget == null)
        {
            return;
        }

        _inputData.MovementVector = new Vector2Int(Math.Sign(Input.GetAxisRaw("Horizontal")),
                                                   Math.Sign(Input.GetAxisRaw("Vertical")));

        _inputData.KeyCode = KeyCode.None;
        foreach (var keyCode in AvailableInputKeys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                _inputData.KeyCode = keyCode;
                break;
            }
        }

        InputTarget.ProcessInput(_inputData);
    }
}
