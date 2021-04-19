using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

[RequireComponent(typeof(ProCamera2DCameraWindow))]
public class DynamicCameraWindow : BasePC2D
{
    public static string ExtensionName = "Dynamic Camera Window";

    [SerializeField] private Vector2 _gridCursorWindowSize;
    [SerializeField] private Vector2 _unitWindowSize;

    private ProCamera2DCameraWindow _cameraWindow;

    private void Start()
    {
        _cameraWindow = GetComponent<ProCamera2DCameraWindow>();
        _cameraWindow.CameraWindowRect = new Rect(Vector2.zero, _gridCursorWindowSize);
    }

    public void SetMode(CameraWindowMode mode)
    {
        var size = mode == CameraWindowMode.Unit ? _unitWindowSize : _gridCursorWindowSize;
        _cameraWindow.CameraWindowRect = new Rect(Vector2.zero, size);
    }
}
