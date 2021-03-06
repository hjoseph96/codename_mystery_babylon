using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.Utilities;
using UnityEngine;

public static class ProCamera2DExtensions
{
    private static DynamicCameraWindow DynamicCameraWindow;
    private static ProCamera2DCameraWindow CameraWindow;

    public static void SetSingleTarget(this ProCamera2D camera, Transform target)
    {
        camera.RemoveAllCameraTargets();
        camera.AddCameraTarget(target);
    }

    public static void SetCameraWindowMode(this ProCamera2D camera, CameraWindowMode mode)
    {
        if (DynamicCameraWindow == null)
        {
            DynamicCameraWindow = camera.GetComponent<DynamicCameraWindow>();
        }

        DynamicCameraWindow.SetMode(mode);
    }

    public static bool TargetInCameraWindow(this ProCamera2D camera, float extraSize = 5f)
    {
        if (CameraWindow == null)
        {
            CameraWindow = camera.GetComponent<ProCamera2DCameraWindow>();
        }

        var isWithinCamera = CameraWindow.CameraWindowRectInWorldCoords.Expand(extraSize)
            .Contains(camera.CameraTargets[0].TargetPosition);

        // CameraWindow.CameraWindowRectInWorldCoords <-- This is modified source code!! Move this to an override class for ProCamera2DCameraWindow
        // OR ELSE IT WILL BREAK WITH EVERY PACKAGE UPDATE
        return isWithinCamera;
    }
}
