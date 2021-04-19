using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public enum HighlightingMode
{
    Movement,
    Attack
}

public class CellHighlighter : SerializedMonoBehaviour, IInitializable
{
    private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

    private readonly Color32[,] ColorMasks =
    {
        {
            new Color32(1, 0, 0, 0),
            new Color32(2, 0, 0, 0),
            new Color32(4, 0, 0, 0),
            new Color32(8, 0, 0, 0)
        },
        {
            new Color32(1, 0, 0, 255),
            new Color32(2, 0, 0, 255),
            new Color32(4, 0, 0, 255),
            new Color32(8, 0, 0, 255)
        }
    };

    [FoldoutGroup("Main Settings")] [SerializeField]
    private Canvas _highlightingCanvas;
    [FoldoutGroup("Main Settings")] [SerializeField]
    private Material _cornerCuttingMaterial;
    [FoldoutGroup("Highlighting prefabs")] [SerializeField]
    private RawImage _moveHighlightingPrefab, _attackHighlightingPrefab;

    [FoldoutGroup("Selection highlighting")] [SerializeField]
    private SpriteRenderer _selectionHighlighting;
    [FoldoutGroup("Selection highlighting")] [SerializeField]
    private Sprite _selectionSquare, _selectionTriangle;


    private readonly List<RawImage> _highlightings = new List<RawImage>();

    private IReadOnlyCollection<Vector2Int> _positions;
    private Vector2Int _center;
    private int _range, _size, _sortingLayerId;

    private WorldGrid _worldGrid;

    private Color32[] _movementHighlightingMask;

    public void Init()
    {
        _worldGrid = WorldGrid.Instance;
    }

    /// <summary>
    /// Highlights movePositions and attackPositions centered around unit
    /// </summary>
    /// <param name="movePositions"></param>
    /// <param name="attackPositions"></param>
    /// <param name="unit"></param>
    public void HighLight(IReadOnlyCollection<Vector2Int> movePositions, IReadOnlyCollection<Vector2Int> attackPositions, Unit unit)
    {
        HighlightWithMode(movePositions, unit, HighlightingMode.Movement);
        HighlightWithMode(attackPositions, unit, HighlightingMode.Attack);
    }

    /// <summary>
    /// Highlight positions using provided HighlightingMode
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="unit"></param>
    /// <param name="mode"></param>
    public void HighlightWithMode(IReadOnlyCollection<Vector2Int> positions, Unit unit, HighlightingMode mode)
    {
        _sortingLayerId = unit.SortingLayerId;

        switch (mode)
        {
            case HighlightingMode.Movement:
                Highlight(positions, unit.GridPosition, unit.CurrentMovementPoints, mode);
                break;

            case HighlightingMode.Attack:
                Highlight(positions, unit.GridPosition, unit.CurrentMovementPoints, mode);
                break;
        }
    }

    /// <summary>
    /// Updates selection highlighting sprite. Uses triangle sprite on stairs and square sprite for usual tiles
    /// </summary>
    /// <param name="sortingLayerId"></param>
    /// <param name="position"></param>
    public void UpdateSelectionHighlightingSprite(int sortingLayerId, Vector2Int position)
    {
        if (_movementHighlightingMask == null || !_worldGrid[position].IsStairs(sortingLayerId))
        {
            ResetSelectionHighlightingSprite();
            return;
        }

        _selectionHighlighting.sprite = _selectionTriangle;
        _selectionHighlighting.flipX = false;
        _selectionHighlighting.flipY = false;

            var topLeft = _center - Vector2Int.one * (_range + 1);
        var relativePos = position - topLeft;
        var index = relativePos.x + relativePos.y * _size;
        if (index < 0 || index >= _movementHighlightingMask.Length)
        {
            ResetSelectionHighlightingSprite();
            return;
        }

        var maskValue = _movementHighlightingMask[index];

        if (maskValue.EqualsTo(Color.clear))
        {
            ResetSelectionHighlightingSprite();
        }
        else if (maskValue.EqualsTo(ColorMasks[0, 0]))
        {
            _selectionHighlighting.flipX = true;
        }
        else if (maskValue.EqualsTo(ColorMasks[0, 2]))
        {
            _selectionHighlighting.flipY = true;
        }
        else if (maskValue.EqualsTo(ColorMasks[0, 3]))
        {
            _selectionHighlighting.flipX = true;
            _selectionHighlighting.flipY = true;
        }
    }

    /// <summary>
    /// Resets selection highlighting sprite back to normal (square)
    /// </summary>
    public void ResetSelectionHighlightingSprite()
    {
        _selectionHighlighting.sprite = _selectionSquare;
    }

    private void Highlight(IReadOnlyCollection<Vector2Int> positions, Vector2Int center, int range, HighlightingMode mode)
    {
        if (positions.Count == 0)
            return;

        _positions = positions;
        _center = center;
        _range = range;
        _size = 2 * range + 3;

        var mainTexture = GetMainTexture();
        var maskTexture = GetMaskTexture(mode);
        var finalTexture = GetFinalTexture(mainTexture, maskTexture);

        Destroy(maskTexture);
        Destroy(maskTexture);

        SpawnHighlightingObject(finalTexture, mode == HighlightingMode.Movement ? _moveHighlightingPrefab : _attackHighlightingPrefab);
    }

    /// <summary>
    /// Clears all highlighting
    /// </summary>
    public void Clear()
    {
        foreach (var rawImage in _highlightings)
        {
            Destroy(rawImage.texture);
            Destroy(rawImage.gameObject);
        }

        _highlightings.Clear();
        _movementHighlightingMask = null;
    }

    private Texture GetMainTexture()
    {
        // We create a texture2D that is a bit larger than highlighting range (1px border)
        var tex = new Texture2D(_size, _size, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };
        var pixels = new Color32[_size * _size];

        var topLeft = _center - Vector2Int.one * (_range + 1);
        // For each highlighted cell we draw white pixel onto texture2D
        foreach (var pos in _positions)
        {
            var relativePos = pos - topLeft;
            var index = relativePos.x + relativePos.y * _size;
            pixels[index] = Color.white;
        }

        tex.SetPixels32(pixels);
        tex.Apply(false);
        return tex;
    }

    private Texture GetMaskTexture(HighlightingMode mode)
    {
        // We create a texture2D that is a bit larger than highlighting range (1px border)
        var mask = new Texture2D(_size, _size, TextureFormat.ARGB32, false) { filterMode = FilterMode.Point };
        var maskPixels = new Color32[_size * _size];

        /*
         Mask texture is used to remove/add some triangles from square highlighting to match stairs shape. It is passed to the shader
         First index in ColorMasks array is used to determine, whether mask should REMOVE (0) or ADD (1) triangle to the highlighting
         Second index determines position of the right angle of the triangle that we a going to remove/add
            [0] = TOP-RIGHT
            [1] = TOP-LEFT
            [2] = BOTTOM-LEFT
            [3] = BOTTOM-RIGHT

        We usually REMOVE triangles when this method is called with mode set to HighlightingMode.Movement
        We usually ADD triangles when this method is called with mode set to HighlightingMode.Attack

        We use some complex conditions to determine whether triangle should be removed (added) or not
        For example, if tile IS NOT stairs tile and the tile on top of us IS STAIRS tile with LeftToRight orientation then we should cut bottom-right triangle
        */

        var topLeft = _center - Vector2Int.one * (_range + 1);
        foreach (var pos in _positions)
        {
            var relativePos = pos - topLeft;
            var index = relativePos.x + relativePos.y * _size;

            var cut = 0;
            var downPosition = pos + Vector2Int.down; 
            var upPosition = pos + Vector2Int.up; 
            switch (_worldGrid[pos].GetStairsOrientation(_sortingLayerId))
            {
                case StairsOrientation.None:
                    if (_worldGrid[pos].IsStairs(_sortingLayerId))
                        break;

                    if (_worldGrid.PointInGrid(downPosition))
                    {
                        if (_worldGrid[downPosition].GetStairsOrientation(_sortingLayerId) == StairsOrientation.LeftToRight)
                            maskPixels[index] = ColorMasks[0, 3];
                        else if (_worldGrid[downPosition].GetStairsOrientation(_sortingLayerId) == StairsOrientation.RightToLeft)
                            maskPixels[index] = ColorMasks[0, 2];
                    }
                    break;

                case StairsOrientation.RightToLeft:
                    if (_worldGrid.PointInGrid(upPosition) && !_worldGrid[upPosition].IsStairs(_sortingLayerId))
                    {
                        maskPixels[index] = ColorMasks[0, 0];
                        if (mode == HighlightingMode.Movement)
                            break;
                    }

                    if (_worldGrid.PointInGrid(downPosition) && !_worldGrid[downPosition].IsStairs(_sortingLayerId))
                    {
                        maskPixels[index] = ColorMasks[0, 2];
                        if (mode == HighlightingMode.Movement)
                            break;
                    }

                    if (mode == HighlightingMode.Attack)
                    {
                        if (_worldGrid.PointInGrid(upPosition) && _worldGrid[upPosition].IsStairs(_sortingLayerId) && 
                            (_movementHighlightingMask[index + _size].EqualsTo(ColorMasks[0, 2]) ||
                             !_worldGrid[pos + 2 * Vector2Int.up].IsStairs(_sortingLayerId) &&
                             _movementHighlightingMask[index + _size].EqualsTo(Color.clear)))
                        {
                            maskPixels[index + _size] = ColorMasks[1, 2];
                        }

                        if (_worldGrid.PointInGrid(downPosition) && _worldGrid[downPosition].IsStairs(_sortingLayerId) && 
                            (_movementHighlightingMask[index - _size].EqualsTo(ColorMasks[0, 0]) ||
                             !_worldGrid[pos + 2 * Vector2Int.down].IsStairs(_sortingLayerId) &&
                             _movementHighlightingMask[index - _size].EqualsTo(Color.clear)))
                        {
                            maskPixels[index - _size] = ColorMasks[1, 0];
                        }

                        break;
                    }

                    if (IsNotHighlightedStairsTile(pos + Vector2Int.right) &&
                        IsNotHighlightedStairsTile(pos + Vector2Int.up)
                        ||
                        IsNotHighlightedStairsTile(pos + Vector2Int.up) &&
                        !IsNotHighlightedStairsTile(pos + new Vector2Int(-1, 1)) &&
                        !IsNotHighlightedStairsTile(pos - new Vector2Int(-1, 1)))
                    {
                        maskPixels[index] = ColorMasks[0, 0];
                    }

                    if (IsNotHighlightedStairsTile(pos + Vector2Int.left) &&
                        IsNotHighlightedStairsTile(pos + Vector2Int.down)
                        ||
                        IsNotHighlightedStairsTile(pos + Vector2Int.down) &&
                        !IsNotHighlightedStairsTile(pos + new Vector2Int(-1, 1)) &&
                        !IsNotHighlightedStairsTile(pos - new Vector2Int(-1, 1)))
                    {
                        cut++;
                        maskPixels[index] = ColorMasks[0, 2];
                    }

                    if (cut == 2)
                        maskPixels[index] = Color.clear;
                    break;

                case StairsOrientation.LeftToRight:
                    if (_worldGrid.PointInGrid(upPosition) && !_worldGrid[upPosition].IsStairs(_sortingLayerId))
                    {
                        maskPixels[index] = ColorMasks[0, 1];
                        if (mode == HighlightingMode.Movement)
                            break;
                    }

                    if (_worldGrid.PointInGrid(downPosition) && !_worldGrid[downPosition].IsStairs(_sortingLayerId))
                    {
                        maskPixels[index] = ColorMasks[0, 3];
                        if (mode == HighlightingMode.Movement)
                            break;
                    }

                    if (mode == HighlightingMode.Attack)
                    {
                        if (_worldGrid.PointInGrid(upPosition) && _worldGrid[upPosition].IsStairs(_sortingLayerId) &&
                            (_movementHighlightingMask[index + _size].EqualsTo(ColorMasks[0, 3]) ||
                            !_worldGrid[pos + 2 * Vector2Int.up].IsStairs(_sortingLayerId) &&
                            _movementHighlightingMask[index + _size].EqualsTo(Color.clear)))
                        {
                            maskPixels[index + _size] = ColorMasks[1, 3];
                        }

                        if (_worldGrid.PointInGrid(downPosition) && _worldGrid[pos + Vector2Int.down].IsStairs(_sortingLayerId) && 
                            (_movementHighlightingMask[index - _size].EqualsTo(ColorMasks[0, 1]) ||
                            !_worldGrid[pos + 2 * Vector2Int.down].IsStairs(_sortingLayerId) &&
                            _movementHighlightingMask[index - _size].EqualsTo(Color.clear)))
                        {
                            maskPixels[index - _size] = ColorMasks[1, 1];
                        }

                        break;
                    }

                    if (IsNotHighlightedStairsTile(pos + Vector2Int.left) &&
                        IsNotHighlightedStairsTile(pos + Vector2Int.up)
                        ||
                        IsNotHighlightedStairsTile(pos + Vector2Int.up) &&
                        !IsNotHighlightedStairsTile(pos + Vector2Int.one) &&
                        !IsNotHighlightedStairsTile(pos + Vector2Int.one))

                    {
                        cut++;
                        maskPixels[index] = ColorMasks[0, 1];
                    }

                    if (IsNotHighlightedStairsTile(pos + Vector2Int.right) &&
                        IsNotHighlightedStairsTile(pos + Vector2Int.down)
                        ||
                        IsNotHighlightedStairsTile(pos + Vector2Int.down) &&
                        !IsNotHighlightedStairsTile(pos + Vector2Int.one) &&
                        !IsNotHighlightedStairsTile(pos - Vector2Int.one))
                    {
                        cut++;
                        maskPixels[index] = ColorMasks[0, 3];
                    }

                    if (cut == 2)
                        maskPixels[index] = Color.clear;
                    break;
            }
        }

        if (mode == HighlightingMode.Movement)
            _movementHighlightingMask = maskPixels;

        mask.SetPixels32(maskPixels);
        mask.Apply(false);
        return mask;
    }

    private Texture GetFinalTexture(Texture mainTexture, Texture maskTexture)
    {
        // To get final texture, we upscale main texture to match pixel-per-cell value
        // Then we apply shader to the upscaled texture to get the final texture
        var cellSize = WorldGrid.CellSize;
        var rt = new RenderTexture(_size * cellSize, _size * cellSize, 32, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
        var temp = RenderTexture.GetTemporary(_size * cellSize, _size * cellSize, 32, RenderTextureFormat.ARGB32);

        rt.Create();
        temp.filterMode = FilterMode.Point;

        Graphics.Blit(mainTexture, temp);
        _cornerCuttingMaterial.SetTexture(MaskTex, maskTexture);
        Graphics.Blit(temp, rt, _cornerCuttingMaterial);
        RenderTexture.ReleaseTemporary(temp);

        return rt;
    }

    private void SpawnHighlightingObject(Texture finalTexture, RawImage prefab)
    {
        var rawImage = Instantiate(prefab, _highlightingCanvas.transform, true);
        rawImage.transform.SetSiblingIndex(0);
        rawImage.rectTransform.sizeDelta = new Vector2(_size, _size);
        rawImage.texture = finalTexture;
        rawImage.transform.localPosition = _worldGrid.Grid.GetCellCenterWorld((Vector3Int)_center);

        _highlightings.Add(rawImage);
    }

    private bool IsNotHighlightedStairsTile(Vector2Int position)
    {
        return _worldGrid.PointInGrid(position) && (!_worldGrid[position].IsStairs(_sortingLayerId) || !_positions.Contains(position));
    }
}