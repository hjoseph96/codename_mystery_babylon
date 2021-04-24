using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasManager : MonoBehaviour
{
    public Canvas Canvas;
    protected RectTransform CanvasRectTransform;

    protected virtual void Awake()
    {
        Canvas = GetComponent<Canvas>();
        CanvasRectTransform = GetComponent<RectTransform>();
    }

    public Vector2 WorldToCanvas(Vector3 worldPosition)
    {
        return ((Vector2)Camera.main.WorldToViewportPoint(worldPosition) - CanvasRectTransform.pivot) * CanvasRectTransform.sizeDelta;
    }


    public void Disable()   => gameObject.SetActive(false);
    public void Enable()    => gameObject.SetActive(true);

    public void SetCamera(Camera camera) => Canvas.worldCamera = camera;
}
