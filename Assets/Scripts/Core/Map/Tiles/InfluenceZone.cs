using UnityEngine;

using Sirenix.OdinInspector;

[RequireComponent(typeof(SpriteRenderer))]
public class InfluenceZone : SerializedMonoBehaviour
{
    private SpriteRenderer _renderer;

    public void SetInvisible()
    {
        _renderer = GetComponent<SpriteRenderer>();
        var color = _renderer.color;
        color.a = 0;
        _renderer.color = color;
    }


    public Rect GetWorldRect()
    {
        _renderer = GetComponent<SpriteRenderer>();
        var bounds = _renderer.bounds;
        return new Rect(bounds.min, bounds.size);
    }

    public RectInt GetWorldRectInt()
    {
        _renderer = GetComponent<SpriteRenderer>();

        var bounds = _renderer.bounds;
        var minPos = new Vector2Int(Mathf.RoundToInt(bounds.min.x), Mathf.RoundToInt(bounds.min.y));
        var maxPos = new Vector2Int(Mathf.RoundToInt(bounds.max.x), Mathf.RoundToInt(bounds.max.y));

        return new RectInt(minPos, maxPos - minPos);
    }
}