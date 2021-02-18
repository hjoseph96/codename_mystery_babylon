using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererFlash : ImageFlash
{
    private SpriteRenderer _renderer;

    protected override void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        base.Awake();
    }
    
    protected override Color GetColor() => _renderer.color;
    protected override void SetColor(Color color) => _renderer.color = color;
}