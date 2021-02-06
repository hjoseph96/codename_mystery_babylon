using UnityEngine;

namespace Tazdraperm.Utility
{
    public static class GridUtility
    {
        public static Vector3 CellToWorldInterpolated(this Grid grid, Vector3 cellPosition)
        {
            var localPosition = grid.CellToLocalInterpolated(cellPosition);
            return grid.LocalToWorld(localPosition);
        }
    }
}