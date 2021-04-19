using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MainCanvas : MonoBehaviour
{
    public static MainCanvas Instance;

    private RectTransform _canvasRectTransform;

    private void Awake()
    {
        Instance = this;
        _canvasRectTransform = GetComponent<RectTransform>();
    }

    public Vector2 WorldToCanvas(Vector3 worldPosition)
    {
        return ((Vector2) Camera.main.WorldToViewportPoint(worldPosition) - _canvasRectTransform.pivot) * _canvasRectTransform.sizeDelta;
    }
}
