using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpOption : ActionMenuOption
{
    public override void Execute()
    {
        var unit = Menu.SelectedUnit;
        
        unit.UponJumpLanding += delegate ()
        {
            var worldGrid = WorldGrid.Instance;
            var newCellPosition = worldGrid.Grid.WorldToCell(unit.transform.position);

            worldGrid[newCellPosition.x, newCellPosition.y].Unit = unit;
            unit.SetGridPosition((Vector2Int)newCellPosition);

            GridCursor.Instance.MoveInstant(unit.GridPosition);
            GridCursor.Instance.SetRestrictedMode(unit);

            unit.DisableJumping();
        };
        Menu.SelectedUnit.Jump();

        GridCursor.Instance.ClearAll();
        Menu.ResetAndHide();
    }
}
