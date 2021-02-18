using System.Collections.Generic;
using UnityEngine;

public class ArrowSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _mainRenderer, _firstCorner, _secondCorner;
    [SerializeField] private SpriteMask _mask;

    private readonly Dictionary<Direction, (Vector2Int, Vector2Int)> _cornersPositions = new Dictionary<Direction, (Vector2Int, Vector2Int)>
    {
        [Direction.LeftUp] = (Vector2Int.down, Vector2Int.right),
        [Direction.LeftDown] = (Vector2Int.right, Vector2Int.up),
        [Direction.RightUp] = (Vector2Int.left, Vector2Int.down),
        [Direction.RightDown] = (Vector2Int.up, Vector2Int.left)
    };

    public void SetSprites(Sprite mainSprite, Sprite firstCorner, Sprite secondCorner, Direction direction)
    {
        _mainRenderer.sprite = mainSprite;
        _firstCorner.sprite = firstCorner;
        _secondCorner.sprite = secondCorner;

        if (firstCorner == null || secondCorner == null)
            return;

        var (firstOffset, secondOffset) = _cornersPositions[direction];
        var pos = WorldGrid.Instance.Grid.WorldToCell(transform.position);
        _firstCorner.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld(pos + (Vector3Int) firstOffset);
        _secondCorner.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld(pos + (Vector3Int) secondOffset);
    }

    public void SetSprites(Sprite mainSprite)
    {
        _mainRenderer.sprite = mainSprite;
    }

    public void Cut(Direction direction)
    {
        var vector = direction.ToVector();
        var maskTransform = _mask.transform;
        maskTransform.localScale = Vector2.one - new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y)) * 0.5f;
        maskTransform.localPosition = (Vector2) vector * 0.25f;
    }

    public void ResetSprites()
    {
        _mainRenderer.sprite = null;
        _firstCorner.sprite = null;
        _secondCorner.sprite = null;

    }
}