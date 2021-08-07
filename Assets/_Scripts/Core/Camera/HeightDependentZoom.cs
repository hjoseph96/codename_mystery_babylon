using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class HeightDependentZoom : BasePC2D
{
    public static string ExtensionName = "Height-dependent Zoom";

    [SerializeField] private float _cameraExtraSizePerHeightUnit = 0.1f;
    [SerializeField] private float _cameraTotalZoomTime = 0.35f;
    [SerializeField] private bool _useTargetHeight = false;

    private int _currentHeight;
    private float _cameraDefaultSize;

    private void Start()
    {
        _cameraDefaultSize = ProCamera2D.ScreenSizeInWorldCoordinates.y * 0.5f;
        Zoom(GetHeight(), true);
    }

    private void Update()
    {
        var height = GetHeight();

        if (height != _currentHeight)
        {
            Zoom(height);
            _currentHeight = height;
        }
    }

    private int GetHeight()
    {
        if (ProCamera2D.CameraTargets.Count == 0 || WorldGrid.Instance == null || WorldGrid.Instance.Grid == null)
            return 0;

        var pos = (Vector2Int) WorldGrid.Instance.Grid.WorldToCell(_useTargetHeight ? ProCamera2D.CameraTargets[0].TargetPosition : transform.position);
        return WorldGrid.Instance.PointInGrid(pos) ? WorldGrid.Instance[pos].Height : 0;
    }

    private void Zoom(int height, bool instant = false)
    {
        ProCamera2D.UpdateScreenSize(_cameraDefaultSize + _cameraExtraSizePerHeightUnit * height, instant ? 0 : _cameraTotalZoomTime);
    }
}