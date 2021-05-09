using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class JumpOption : ActionMenuOption
{
    public override void Execute()
    {
        var unit = Menu.SelectedUnit;
        var gridCursor = GridCursor.Instance;

        unit.UponJumpLanding += delegate ()
        {
            var worldGrid = WorldGrid.Instance;
            var newCellPosition = worldGrid.Grid.WorldToCell(unit.transform.position);

            worldGrid[newCellPosition.x, newCellPosition.y].Unit = unit;
            unit.SetGridPosition((Vector2Int)newCellPosition);

            gridCursor.SetAsCameraTarget();
            gridCursor.MoveInstant(unit.GridPosition);
            gridCursor.ClearAll();
            gridCursor.SetRestrictedMode(unit);

            unit.DisableJumping();
        };
        Menu.SelectedUnit.Jump();

        var camera = ProCamera2D.Instance;
        camera.RemoveAllCameraTargets();
        camera.AddCameraTarget(unit.transform);

        gridCursor.ClearAll();
        Menu.ResetAndHide();
    }
}