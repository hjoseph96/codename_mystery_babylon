using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Grids2D.Geom;
using Grids2D.PathFinding;

namespace Grids2D {

    public enum CanCrossCheckType {
        Default = 0,
        IgnoreCanCrossCheckOnAllCells = 1,
        ignoreCanCrossCheckOnStartAndEndCells = 2
    }


	/* Event definitions */
	public delegate float PathFindingEvent (int cellIndex);

	public partial class Grid2D : MonoBehaviour {

		/// <summary>
		/// Fired when path finding algorithmn evaluates a cell. Return the increased cost for cell.
		/// </summary>
		public event PathFindingEvent OnPathFindingCrossCell;

		[SerializeField]
		HeuristicFormula
			_pathFindingHeuristicFormula = HeuristicFormula.EuclideanNoSQR;

		/// <summary>
		/// The path finding heuristic formula to estimate distance from current position to destination
		/// </summary>
		public PathFinding.HeuristicFormula pathFindingHeuristicFormula {
			get { return _pathFindingHeuristicFormula; }
			set {
				if (value != _pathFindingHeuristicFormula) {
					_pathFindingHeuristicFormula = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		int
			_pathFindingMaxSteps = 2000;

		/// <summary>
		/// The maximum number of steps that a path can return.
		/// </summary>
		public int pathFindingMaxSteps {
			get { return _pathFindingMaxSteps; }
			set {
				if (value != _pathFindingMaxSteps) {
					_pathFindingMaxSteps = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		float
			_pathFindingMaxCost = 200000;

		/// <summary>
		/// The maximum search cost of the path finding execution.
		/// </summary>
		public float pathFindingMaxCost {
			get { return _pathFindingMaxCost; }
			set {
				if (value != _pathFindingMaxCost) {
					_pathFindingMaxCost = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_pathFindingUseDiagonals = true;

		/// <summary>
		/// If path can include diagonals between cells
		/// </summary>
		public bool pathFindingUseDiagonals {
			get { return _pathFindingUseDiagonals; }
			set {
				if (value != _pathFindingUseDiagonals) {
					_pathFindingUseDiagonals = value;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		float
			_pathFindingHeavyDiagonalsCost = 1.4f;

		/// <summary>
		/// The cost for crossing diagonals.
		/// </summary>
		public float pathFindingHeavyDiagonalsCost {
			get { return _pathFindingHeavyDiagonalsCost; }
			set {
				if (value != _pathFindingHeavyDiagonalsCost) {
					_pathFindingHeavyDiagonalsCost = value;
					isDirty = true;
				}
			}
		}


		#region Public Path Finding functions

		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options.
		/// </summary>
		/// <returns>The route consisting of a list of cell indexes.</returns>
		/// <param name="startPosition">Start position in map coordinates (-0.5...0.5)</param>
		/// <param name="endPosition">End position in map coordinates (-0.5...0.5)</param>
		/// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
		public List<int> FindPath (int cellIndexStart, int cellIndexEnd, float maxSearchCost = 0, int maxSteps = 0, int cellGroupMask = -1, CanCrossCheckType canCrossCheck = CanCrossCheckType.Default) {
			float dummy;
			return FindPath (cellIndexStart, cellIndexEnd, maxSearchCost, out dummy, maxSteps, cellGroupMask, canCrossCheck);
		}

		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options.
		/// </summary>
		/// <returns>The route consisting of a list of cell indexes.</returns>
		/// <param name="cellIndexStart">Index of first cell</param>
		/// <param name="cellIndexEnd">Index of last cell</param>
		/// <param name="maxSearchCost">Maximum search cost for the path finding algorithm. A value of 0 will use the global default defined by pathFindingMaxCost</param>
		/// <param name="cost">The cost for the entire returned path</param>
		/// <param name="ignoreStartEndCellCanCrossCheck">Pass true to ignore verification if starting/end cell are marked as blocked or not.</param>
		public List<int> FindPath (int cellIndexStart, int cellIndexEnd, float maxSearchCost, out float cost, int maxSteps = 0, int cellGroupMask = -1, CanCrossCheckType canCrossCheck = CanCrossCheckType.Default) {
			cost = 0;
			if (cellIndexStart < 0 || cellIndexStart >= cells.Count || cellIndexEnd < 0 || cellIndexEnd >= cells.Count)
				return null;

			Cell startCell = cells [cellIndexStart];
			Cell endCell = cells [cellIndexEnd];

			List<int> routePoints = null;
			
			// Minimum distance for routing?
			if (cellIndexStart != cellIndexEnd) {
				bool startCellCanCross = startCell.canCross;
				bool endCellCanCross = endCell.canCross;
				if (canCrossCheck == CanCrossCheckType.ignoreCanCrossCheckOnStartAndEndCells) {
					startCell.canCross = endCell.canCross = true;
				} else if (!startCell.canCross || !endCell.canCross)
					return null;
				PathFindingPoint startingPoint = new PathFindingPoint (startCell.column, startCell.row);
				PathFindingPoint endingPoint = new PathFindingPoint (endCell.column, endCell.row);
				ComputeRouteMatrix ();
				finder.Formula = _pathFindingHeuristicFormula;
				finder.MaxSteps = maxSteps > 0 ? maxSteps : _pathFindingMaxSteps;
				finder.Diagonals = _pathFindingUseDiagonals;
				finder.HeavyDiagonalsCost = _pathFindingHeavyDiagonalsCost;
				finder.HexagonalGrid = _gridTopology == GRID_TOPOLOGY.Hexagonal;
				finder.MaxSearchCost = maxSearchCost > 0 ? maxSearchCost : _pathFindingMaxCost;
				finder.CellGroupMask = cellGroupMask;
                finder.CheckCellCanCross = canCrossCheck != CanCrossCheckType.IgnoreCanCrossCheckOnAllCells;
				if (OnPathFindingCrossCell != null) {
					finder.OnCellCross = FindRoutePositionValidator;
				} else {
					finder.OnCellCross = null;
				}
				List<PathFinderNode> route = finder.FindPath (startingPoint, endingPoint, out cost, _evenLayout);
				startCell.canCross = startCellCanCross;
				endCell.canCross = endCellCanCross;
				if (route != null) {
					int routeCount = route.Count;
					routePoints = new List<int> (routeCount);
					for (int r = routeCount - 2; r >= 0; r--) {
						int cellIndex = route [r].PY * _cellColumnCount + route [r].PX;
						routePoints.Add (cellIndex);
					}
					routePoints.Add (cellIndexEnd);
				} else {
					return null;	// no route available
				}
			}
			return routePoints;
		}

		/// <summary>
		/// Traces a line between two positions and check if there's no cell blocking the line
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="cellGroupMask">Optional cell layer mask</param>
		/// <param name="lineResolution">Resolution of the line. Increase to improve line accuracy.</param>
		/// <param name="exhaustiveCheck">If set to true, all vertices of destination cell will be considered instead of its center</param>
		public bool CellGetLineOfSight (Vector3 startPosition, Vector3 endPosition, ref List<int> cellIndices, ref List<Vector3> worldPositions, int cellGroupMask = -1, int lineResolution = 2, bool exhaustiveCheck = false) {

			cellIndices = null;

			Cell startCell = CellGetAtPosition (startPosition, true);
			Cell endCell = CellGetAtPosition (endPosition, true);
			if (startCell == null || endCell == null) {
				return false;
			}

			int cell1 = CellGetIndex (startCell);
			int cell2 = CellGetIndex (endCell);
			if (cell1 < 0 || cell2 < 0)
				return false;

			return CellGetLineOfSight (cell1, cell2, ref cellIndices, ref worldPositions, cellGroupMask, lineResolution, exhaustiveCheck);
		}

		/// <summary>
		/// Traces a line between two positions and check if there's no cell blocking the line
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="cellGroupMask">Optional cell layer mask</param>
		/// <param name="lineResolution">Resolution of the line. Increase to improve line accuracy.</param>
		/// <param name="exhaustiveCheck">If set to true, all vertices of destination cell will be considered instead of its center</param>
		public bool CellGetLineOfSight (int startCellIndex, int endCellIndex, ref List<int> cellIndices, ref List<Vector3> worldPositions, int cellGroupMask = -1, int lineResolution = 2, bool exhaustiveCheck = false) {

			if (cellIndices == null)
				cellIndices = new List<int> ();
			else
				cellIndices.Clear ();
			if (worldPositions == null)
				worldPositions = new List<Vector3> ();
			else
				worldPositions.Clear ();
			if (startCellIndex < 0 || startCellIndex >= cells.Count || endCellIndex < 0 || endCellIndex >= cells.Count)
				return false;

			Vector3 startPosition = CellGetPosition (startCellIndex);
			Vector3 endPosition;
			int vertexCount = exhaustiveCheck ? cells [endCellIndex].region.points.Count : 0;
			bool success = true;

			lineResolution = Mathf.Max (2, lineResolution);

			for (int p = 0; p <= vertexCount; p++) {
				if (p == 0) {
					endPosition = CellGetPosition (endCellIndex);
				} else {
					cellIndices.Clear ();
					worldPositions.Clear ();
					endPosition = CellGetVertexPosition (endCellIndex, p - 1);
				}

				int numSteps;
				switch (_gridTopology) {
				case GRID_TOPOLOGY.Hexagonal: 
                    // Hexagon distance
					numSteps = CellGetHexagonDistance (startCellIndex, endCellIndex);
					numSteps *= lineResolution;
					break;
				case GRID_TOPOLOGY.Box:
					numSteps = CellGetBoxDistance (startCellIndex, endCellIndex);
					numSteps *= lineResolution;
					if (numSteps % 2 == 0) numSteps++;
					break;
				default:
					float dist = Vector3.Distance (startPosition, endPosition);
					numSteps = (int)(dist * lineResolution);
					break;
				}

				Cell lastCell = cells [startCellIndex];
				success = true;
				for (int k = 1; k <= numSteps; k++) {
					Vector3 position = Vector3.Lerp (startPosition, endPosition, (float)k / numSteps);
					Cell cell = k == numSteps ? cells [endCellIndex] : CellGetAtPosition (position, true);
					if (cell != null && cell != lastCell) {
						if (cell != cells [endCellIndex]) {
							if (!cell.canCross || (cell.group & cellGroupMask) == 0) {
								success = false;
								break;
							}
						}
						// Check LOD blocks
						if (LOSIsBlocked(lastCell, cell) || LOSIsBlocked(cell, lastCell)) {
							success = false;
							break;
						}
						cellIndices.Add (CellGetIndex (cell));
						lastCell = cell;
					}
					worldPositions.Add (position);
				}
				if (success) {
					if (p == 0) {
						return true;
					}
					break;
				}
			}
			if (success) {
				CellGetLine (startCellIndex, endCellIndex, ref cellIndices, ref worldPositions, lineResolution);
			}
			return success;
		}

		bool LOSIsBlocked (Cell cell1, Cell cell2) {
			switch (_gridTopology) {
			case GRID_TOPOLOGY.Box:
				int row1 = cell1.row;
				int column1 = cell1.column;
				int row2 = cell2.row;
				int column2 = cell2.column;
				if (column1 == column2 && row1 == row2)
					return false;

				bool blocksVertically = row1 < row2 ? cell2.GetSideBlocksLOS (CELL_SIDE.Bottom) : cell2.GetSideBlocksLOS (CELL_SIDE.Top);
				bool blocksHorizontally = column1 < column2 ? cell2.GetSideBlocksLOS (CELL_SIDE.Left) : cell2.GetSideBlocksLOS (CELL_SIDE.Right);

				if (row1 == row2) {
					return blocksHorizontally;
				} else if (column1 == column2) {
					return blocksVertically;
				} else {
					return blocksHorizontally || blocksVertically;
				}
			default:
				Vector2 dir = cell2.center - cell1.center;
				CELL_SIDE side = GetSideByVector (dir);
				return cell2.GetSideBlocksLOS (side);
			}
		}


		/// <summary>
		/// Returns a line composed of cells and world positions from starting cell to ending cell
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="lineResolution">Resolution of the line. Increase to improve line accuracy.</param>
		public void CellGetLine (int startCellIndex, int endCellIndex, ref List<int> cellIndices, ref List<Vector3> worldPositions, int lineResolution = 2) {

			if (cellIndices == null)
				cellIndices = new List<int> ();
			else
				cellIndices.Clear ();
			if (worldPositions == null)
				worldPositions = new List<Vector3> ();
			else
				worldPositions.Clear ();
			if (startCellIndex < 0 || startCellIndex >= cells.Count || endCellIndex < 0 || endCellIndex >= cells.Count)
				return;

			lineResolution = Mathf.Max (2, lineResolution);

			Vector3 startPosition = CellGetPosition (startCellIndex);
			Vector3 endPosition = CellGetPosition (endCellIndex);

			int numSteps;
			switch (_gridTopology) {
			case GRID_TOPOLOGY.Hexagonal: 
				// Hexagon distance
				numSteps = CellGetHexagonDistance (startCellIndex, endCellIndex);
				numSteps *= lineResolution;
				break;
			case GRID_TOPOLOGY.Box:
				numSteps = CellGetBoxDistance (startCellIndex, endCellIndex);
				numSteps *= lineResolution;
				if (numSteps % 2 == 0) numSteps++;
				break;
			default:
				float dist = Vector3.Distance (startPosition, endPosition);
				numSteps = (int)(dist * lineResolution);
				break;
			}

			Cell lastCell = cells[startCellIndex];
			for (int k = 1; k <= numSteps; k++) {
				Vector3 position = Vector3.Lerp (startPosition, endPosition, (float)k / numSteps);
				Cell cell = CellGetAtPosition (position, true);
				if (cell != null && cell != lastCell) {
					cellIndices.Add (CellGetIndex (cell));
				}
				worldPositions.Add (position);
				lastCell = cell;
			}
		}

		/// <summary>
		/// Returns the hexagon distance between two cells (number of steps to reach end cell from start cell).
		/// This method does not take into account cell masks or blocking cells. It just returns the distance.
		/// </summary>
		/// <returns>The get hexagon distance.</returns>
		/// <param name="startCellIndex">Start cell index.</param>
		/// <param name="endCellIndex">End cell index.</param>
		public int CellGetHexagonDistance (int startCellIndex, int endCellIndex) {
			if (cells == null)
				return -1;
			int cellCount = cells.Count;
			if (startCellIndex < 0 || startCellIndex >= cellCount || endCellIndex < 0 || endCellIndex >= cellCount)
				return -1;
			int r0 = cells [startCellIndex].row;
			int c0 = cells [startCellIndex].column;
			int r1 = cells [endCellIndex].row;
			int c1 = cells [endCellIndex].column;
			int offset = _evenLayout ? 0 : 1;
			int y0 = r0 - Mathf.FloorToInt ((c0 + offset) / 2);
			int x0 = c0;
			int y1 = r1 - Mathf.FloorToInt ((c1 + offset) / 2);
			int x1 = c1;
			int dx = x1 - x0;
			int dy = y1 - y0;
			int numSteps = Mathf.Max (Mathf.Abs (dx), Mathf.Abs (dy));
			numSteps = Mathf.Max (numSteps, Mathf.Abs (dx + dy));
			return numSteps;
		}


		/// <summary>
		/// Returns the number of steps between two cells in box topology.
		/// This method does not take into account cell masks or blocking cells. It just returns the distance.
		/// </summary>
		/// <param name="startCellIndex">Start cell index.</param>
		/// <param name="endCellIndex">End cell index.</param>
		public int CellGetBoxDistance (int startCellIndex, int endCellIndex) {
			if (cells == null)
				return -1;
			int cellCount = cells.Count;
			if (startCellIndex < 0 || startCellIndex >= cellCount || endCellIndex < 0 || endCellIndex >= cellCount)
				return -1;
			int r0 = cells [startCellIndex].row;
			int c0 = cells [startCellIndex].column;
			int r1 = cells [endCellIndex].row;
			int c1 = cells [endCellIndex].column;
			int dx = c1 - c0;
			int dy = r1 - r0;
			return Mathf.Max (Mathf.Abs (dx), Mathf.Abs (dy));
		}

		#endregion


	
	}
}