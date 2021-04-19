using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: This should be refactored into a platform specific setup
// IE: PC buttons and Gamepad/Console [Switch, Xbox, PS5] Buttons
public enum ActionButtonType
{
    Z,
    X,
    Q,
    W,
    Space
}

public class ActionButton : MonoBehaviour
{
    public ActionButtonType ButtonType;
}
