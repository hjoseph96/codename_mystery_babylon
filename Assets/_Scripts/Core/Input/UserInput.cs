using System;
using System.Collections.Generic;
using Tazdraperm.Utility;
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
        KeyCode.Space,
    };

    public static UserInput Instance;


    public IInputTarget InputTarget
    {
        get => _inputTargets.Count > 0 ? _inputTargets[0] : null;

        set
        {
            _inputTargets.Clear();
            if (value != null)
                _inputTargets.Add(value);
        }
    }

    public IEnumerable<IInputTarget> AllInputTargets => _inputTargets;

    private List<IInputTarget> _inputTargets = new List<IInputTarget>();
    private List<IInputTarget> _currentInputTargets = new List<IInputTarget>();
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

        _inputData.MovementVectorPressed = new Vector2Int(
            Input.GetKeyDown(KeyCode.RightArrow).ToInt() - Input.GetKeyDown(KeyCode.LeftArrow).ToInt(),
            Input.GetKeyDown(KeyCode.UpArrow).ToInt() - Input.GetKeyDown(KeyCode.DownArrow).ToInt());

        _inputData.KeyCode = KeyCode.None;
        foreach (var keyCode in AvailableInputKeys)
        {
            if (Input.GetKeyDown(keyCode))
            {
                _inputData.KeyCode = keyCode;
                _inputData.KeyState = KeyState.Down;
                break;
            }

            if (Input.GetKeyUp(keyCode))
            {
                _inputData.KeyCode = keyCode;
                _inputData.KeyState = KeyState.Up;
                break;
            }
        }

        _currentInputTargets.Clear();
        _currentInputTargets.AddRange(_inputTargets);

        foreach (var target in _currentInputTargets)
            target.ProcessInput(_inputData);
    }

    public void AddInputTarget(IInputTarget target)
    {
        _inputTargets.Add(target);
    }

    public void RemoveInputTarget(IInputTarget target)
    {
        _inputTargets.Remove(target);
    }

    public void RemoveAllInputTargets()
    {
        _inputTargets.Clear();
    }
}
