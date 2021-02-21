using Sirenix.OdinInspector;
using UnityEngine;

public class AIUnit : Unit
{
    [FoldoutGroup("AI properties")]
    [SerializeField] 
    private int _visionRange = 10;

    [FoldoutGroup("AI properties")] 
    [SerializeField] 
    private bool _showVisionRange;

    private int CurrentVisionRange => _visionRange +
                                      Mathf.RoundToInt(WorldGrid.Instance[GridPosition].Height * 
                                                       WorldGrid.Instance.ExtraVisionRangePerHeightUnit);

    public bool HasVision(Vector2Int position)
    {
        return (GridPosition - position).sqrMagnitude <= CurrentVisionRange * CurrentVisionRange &&
               GridUtility.HasLineOfSight(GridPosition, position);
    }

    private void OnDrawGizmos()
    {
        if (!_showVisionRange || !Application.isPlaying)
            return;

        var worldGrid = WorldGrid.Instance;
        var range = CurrentVisionRange;

        var minX = Mathf.Max(0, GridPosition.x - range);
        var maxX = Mathf.Min(worldGrid.Width - 1, GridPosition.x + range);
        var minY = Mathf.Max(0, GridPosition.y - range);
        var maxY = Mathf.Min(worldGrid.Height - 1, GridPosition.y + range);

        var color = Color.yellow;
        color.a = 0.3f;
        Gizmos.color = color;

        for (var i = minX; i <= maxX; i++)
        {
            for (var j = minY; j <= maxY; j++)
            {
                var currentPosition = new Vector2Int(i, j);
                if (HasVision(currentPosition))
                    Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) currentPosition), Vector3.one);
            }
        }
    }
}