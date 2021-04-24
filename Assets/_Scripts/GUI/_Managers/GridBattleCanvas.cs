using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class GridBattleCanvas : CanvasManager
{
    [SerializeField] private ActionSelectMenu _actionSelectMenu;
    public ActionSelectMenu ActionSelectMenu { get => _actionSelectMenu; }

    
    [FoldoutGroup("UI Elements")]
    [SerializeField] private TurnDisplay _turnDisplay;
    public TurnDisplay TurnDisplay      { get => _turnDisplay; }

    [FoldoutGroup("UI Elements")]
    [SerializeField] private PhaseDisplay _phaseDisplay;
    public PhaseDisplay PhaseDisplay    { get => _phaseDisplay; }
    
    [FoldoutGroup("UI Elements")]
    [SerializeField] private ActionNoticeManager _actionNoticeManager;

    [FoldoutGroup("UI Elements")]
    [SerializeField] private Transform _leftPortraitDialogSpawnPoint;
    public Transform LeftPortraitDialogSpawnPoint   { get => _leftPortraitDialogSpawnPoint; }

    [FoldoutGroup("UI Elements")]
    [SerializeField] private Transform _rightPortraitDialogSpawnPoint;
    public Transform RightPortraitDialogSpawnPoint  { get => _rightPortraitDialogSpawnPoint; }


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        _actionNoticeManager.Init();
    }

    public Dictionary<Direction, DialogBox> GetPortraitDialogBoxes()
    {
        var portraitDialogBoxes = new Dictionary<Direction, DialogBox>();

        var leftDialogBox   = _leftPortraitDialogSpawnPoint.GetComponentInChildren<DialogBox>();
        if (leftDialogBox != null)
            portraitDialogBoxes[Direction.Left]     = leftDialogBox;

        var rightDialogBox  = _rightPortraitDialogSpawnPoint.GetComponentInChildren<DialogBox>();
        if (rightDialogBox)
            portraitDialogBoxes[Direction.Right]    = rightDialogBox;

        return portraitDialogBoxes;
    }
}
