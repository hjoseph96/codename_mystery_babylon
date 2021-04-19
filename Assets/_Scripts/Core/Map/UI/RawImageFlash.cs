using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RawImageFlash : ImageFlash
{
    private Material _material;

    protected override void Awake()
    {
        var rawImage = GetComponent<RawImage>();
        _material = new Material(rawImage.material);
        rawImage.material = _material;

        base.Awake();
    }

    protected override Color GetColor() => _material.color;
    protected override void SetColor(Color color) => _material.color = color;
}