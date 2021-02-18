using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InfluenceZone : MonoBehaviour
{
    private SpriteRenderer _renderer;

    private void OnValidate()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public Rect GetWorldRect()
    {
        var bounds = _renderer.bounds;
        return new Rect(bounds.min, bounds.size);
    }

    public RectInt GetWorldRectInt()
    {
        var bounds = _renderer.bounds;
        var minPos = new Vector2Int(Mathf.RoundToInt(bounds.min.x), Mathf.RoundToInt(bounds.min.y));
        var maxPos = new Vector2Int(Mathf.RoundToInt(bounds.max.x), Mathf.RoundToInt(bounds.max.y));

        return new RectInt(minPos, maxPos - minPos);
    }
}