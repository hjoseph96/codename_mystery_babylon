using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Sirenix.OdinInspector;

// TODO: This should be refactored into a platform specific setup
// IE: PC buttons and Gamepad/Console [Switch, Xbox, PS5] Buttons

public class ActionButton : MonoBehaviour
{
    [SerializeField] private KeyCode _buttonType;
    public KeyCode ButtonType { get => _buttonType; }

    [SerializeField] private bool _isInputReactive;

    private Animator _animator;
    private Image _image;

    private void Update()
    {
        
    }

}
