using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBattleCanvas : CanvasManager
{
    [SerializeField] private ActionSelectMenu _actionSelectMenu;
    public ActionSelectMenu ActionSelectMenu { get => _actionSelectMenu; }

    [SerializeField] private TurnDisplay _turnDisplay;
    public TurnDisplay TurnDisplay { get => _turnDisplay; }

    [SerializeField] private PhaseDisplay _phaseDisplay;
    public PhaseDisplay PhaseDisplay { get => _phaseDisplay; }

    [SerializeField] private ActionNoticeManager _actionNoticeManager;

    [SerializeField] private Transform _leftPortraitDialogSpawnPoint;

    [SerializeField] private Transform _rightPortraitDialogSpawnPoint;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _actionNoticeManager.Init();
    }

    public Dictionary<Direction, DialogBox> GetPortraitDialogBoxes()
    {
        var portraitDialogBoxes = new Dictionary<Direction, DialogBox>();

        var leftDialogBox = _leftPortraitDialogSpawnPoint.GetComponentInChildren<DialogBox>();
        if (leftDialogBox != null)
            portraitDialogBoxes[Direction.Left] = leftDialogBox;

        var rightDialogBox = _rightPortraitDialogSpawnPoint.GetComponentInChildren<DialogBox>();
        if (rightDialogBox)
            portraitDialogBoxes[Direction.Right] = rightDialogBox;

        return portraitDialogBoxes;
    }
}
