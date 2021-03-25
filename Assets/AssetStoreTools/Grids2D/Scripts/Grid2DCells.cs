using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Grids2D.Geom;
using System.Globalization;

namespace Grids2D {

	/* Event definitions */
	public delegate void OnCellEvent (int cellIndex);
	public delegate void OnCellHighlight (int cellIndex, ref bool cancelHighlight);


	public partial class Grid2D : MonoBehaviour {

		public event OnCellEvent OnCellEnter;
		public event OnCellEvent OnCellExit;
		public event OnCellEvent OnCellClick;
		public event OnCellEvent OnCellMouseUp;
		public event OnCellHighlight OnCellHighlight;

		/// <summary>
		/// Complete array of states and cells and the territory name they belong to.
		/// </summary>
		[NonSerialized]
		public List<Cell> cells;

		[SerializeField]
		int _numCells = 3;

		/// <summary>
		/// Gets or sets the desired number of cells in irregular topology.
		/// </summary>
		public int numCells { 
			get {
				if (_gridTopology == GRID_TOPOLOGY.Irregular) {
					return _numCells; 
				} else {
					return _cellRowCount * _cellColumnCount;
				}
			}
			set {
				if (_numCells != value) {
					_numCells = Mathf.Clamp (value, 1, MAX_CELLS);
					GenerateMap ();
					isDirty = true;
				}
			}
		}


		[NonSerialized]
		List<Vector2> _voronoiSites;

		public List<Vector2> voronoiSites {
			get { return _voronoiSites; }
			set {
				if (_voronoiSites != value) {
					_voronoiSites = value;
					if (_voronoiSites != null) {
						_numCells = _voronoiSites.Count;
					}
					GenerateMap ();
				}
			}
		}


		[SerializeField]
		bool _showCells = true;

		/// <summary>
		/// Toggle cells frontiers visibility.
		/// </summary>
		public bool showCells { 
			get {
				return _showCells; 
			}
			set {
				if (value != _showCells) {
					_showCells = value;
					isDirty = true;
					if (cellLayer != null) {
						cellLayer.SetActive (_showCells);
					} else if (_showCells) {
						Redraw ();
					}
				}
			}
		}

		[SerializeField]
		Color
			_cellBorderColor = new Color (0, 1, 0, 1.0f);

		/// <summary>
		/// Cells border color
		/// </summary>
		public Color cellBorderColor {
			get {
				if (cellsMat != null) {
					return cellsMat.color;
				} else {
					return _cellBorderColor;
				}
			}
			set {
				if (value != _cellBorderColor) {
					_cellBorderColor = value;
					isDirty = true;
					if (cellsMat != null && _cellBorderColor != cellsMat.color) {
						cellsMat.color = _cellBorderColor;
					}
				}
			}
		}

		public float cellBorderAlpha {
			get {
				return _cellBorderColor.a;
			}
			set {
				if (_cellBorderColor.a != value) {
					cellBorderColor = new Color (_cellBorderColor.r, _cellBorderColor.g, _cellBorderColor.b, Mathf.Clamp01 (value));
				}
			}
		}


		[SerializeField]
		Color
			_cellHighlightColor = new Color (1, 0, 0, 0.7f);

		/// <summary>
		/// Fill color to use when the mouse hovers a cell's region.
		/// </summary>
		public Color cellHighlightColor {
			get {
				return _cellHighlightColor;
			}
			set {
				if (value != _cellHighlightColor) {
					_cellHighlightColor = value;
					isDirty = true;
					if (hudMatCellOverlay != null && _cellHighlightColor != hudMatCellOverlay.color) {
						hudMatCellOverlay.color = _cellHighlightColor;
					}
					if (hudMatCellGround != null && _cellHighlightColor != hudMatCellGround.color) {
						hudMatCellGround.color = _cellHighlightColor;
					}
				}
			}
		}

		[SerializeField]
		bool _cellHighlightNonVisible = true;

		/// <summary>
		/// Gets or sets whether invisible cells should also be highlighted when pointer is over them
		/// </summary>
		public bool cellHighlightNonVisible {
			get { return _cellHighlightNonVisible; }
			set {
				if (_cellHighlightNonVisible != value) {
					_cellHighlightNonVisible = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		int _cellRowCount = 8;

		/// <summary>
		/// Returns the number of rows for box and hexagonal grid topologies
		/// </summary>
		public int rowCount { 
			get {
				return _cellRowCount;
			}
			set {
				if (value != _cellRowCount) {
					_cellRowCount = value;
					isDirty = true;
					GenerateMap ();
				}
			}

		}

		[Obsolete ("Use rowCount instead.")]
		public int cellRowCount {
			get { return rowCount; }
			set { rowCount = value; }
		}

		[SerializeField]
		int _cellColumnCount = 8;

		/// <summary>
		/// Returns the number of columns for box and hexagonal grid topologies
		/// </summary>
		public int columnCount { 
			get {
				return _cellColumnCount;
			}
			set {
				if (value != _cellColumnCount) {
					_cellColumnCount = value;
					isDirty = true;
					GenerateMap ();
				}
			}
		}

		[Obsolete ("Use columnCount instead.")]
		public int cellColumnCount {
			get { return columnCount; }
			set { columnCount = value; }
		}


		#region State variables

		/// <summary>
		/// Returns Cell under mouse position or null if none.
		/// </summary>
		public Cell cellHighlighted { get { return _cellHighlighted; } }

		/// <summary>
		/// Returns current highlighted cell index.
		/// </summary>
		public int cellHighlightedIndex { get { return _cellHighlightedIndex; } }

		/// <summary>
		/// Returns Cell index which has been clicked
		/// </summary>
		public int cellLastClickedIndex { get { return _cellLastClickedIndex; } }

		/// <summary>
		/// Returns last clicked Cell object.
		/// </summary>
		public Cell cellLastClicked {
			get {
				if (_cellLastClickedIndex >= 0 && _cellLastClickedIndex < cells.Count)
					return cells [_cellLastClickedIndex];
				else
					return null;
			}
		}


		#endregion

		#region Public Cells API

		
		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		public int CellGetIndex (Cell cell) {
			if (cell == null)
				return -1;
			if (cellLookup.ContainsKey (cell))
				return _cellLookup [cell];
			else
				return -1;
		}

		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		/// <returns>The get index.</returns>
		/// <param name="row">Row.</param>
		/// <param name="column">Column.</param>
		/// <param name="clampToBorders">If set to <c>true</c> row and column values will be clamped inside current grid size (in case their values exceed the number of rows or columns). If set to false, it will wrap around edges.</param>
		public int CellGetIndex (int row, int column, bool clampToBorders = true) {
			if (_gridTopology != GRID_TOPOLOGY.Box && _gridTopology != GRID_TOPOLOGY.Hexagonal) {
				Debug.LogWarning ("Grid topology does not support row/column indexing.");
				return -1;
			}
			
			if (clampToBorders) {
				row = Mathf.Clamp (row, 0, _cellRowCount - 1);
				column = Mathf.Clamp (column, 0, _cellColumnCount - 1);
			} else {
				row = (row + _cellRowCount) % _cellRowCount;
				column = (column + _cellColumnCount) % _cellColumnCount;
			}
			
			return row * _cellColumnCount + column;
		}

		
		/// <summary>
		/// Specifies if a given cell can be crossed by using the pathfinding engine.
		/// </summary>
		public void CellSetCanCross (int cellIndex, bool canCross) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].canCross = canCross;
			needRefreshRouteMatrix = true;
		}

		/// <summary>
		/// Sets the additional cost of crossing an hexagon side.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the hexagon.</param>
		/// <param name="additionalCost">Additional cost.</param>
		/// <param name="symmetrical">Applies crossing cost to both directions of the given side.</param>
		public void CellSetSideCrossCost (int cellIndex, CELL_SIDE side, float cost, CELL_DIRECTION direction = CELL_DIRECTION.Both) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			Cell cell = cells [cellIndex];
			if (direction != CELL_DIRECTION.Entering) {
				cell.SetSideCrossCost (side, cost);
			}
			if (direction != CELL_DIRECTION.Exiting) {
				int r = cell.row;
				int c = cell.column;
				int or = r, oc = c;
				CELL_SIDE os = side;
				if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
					switch (side) {
					case CELL_SIDE.Bottom:
						or--;
						os = CELL_SIDE.Top;
						break;
					case CELL_SIDE.Top:
						or++;
						os = CELL_SIDE.Bottom;
						break;
					case CELL_SIDE.BottomRight: 
						if (oc % 2 != 0) {
							or--;
						}
						oc++;
						os = CELL_SIDE.TopLeft;
						break;
					case CELL_SIDE.TopRight: 
						if (oc % 2 == 0) {
							or++;
						}
						oc++;
						os = CELL_SIDE.BottomLeft;
						break;
					case CELL_SIDE.TopLeft: 
						if (oc % 2 == 0) {
							or++;
						}
						oc--;
						os = CELL_SIDE.BottomRight;
						break;
					case CELL_SIDE.BottomLeft: 
						if (oc % 2 != 0) {
							or--;
						}
						oc--;
						os = CELL_SIDE.TopRight;
						break;
					}
				} else {
					switch (side) {
					case CELL_SIDE.Bottom:
						or--;
						os = CELL_SIDE.Top;
						break;
					case CELL_SIDE.Top:
						or++;
						os = CELL_SIDE.Bottom;
						break;
					case CELL_SIDE.BottomRight: 
						or--;
						oc++;
						os = CELL_SIDE.TopLeft;
						break;
					case CELL_SIDE.TopRight: 
						or++;
						oc++;
						os = CELL_SIDE.BottomLeft;
						break;
					case CELL_SIDE.TopLeft: 
						or++;
						oc--;
						os = CELL_SIDE.BottomRight;
						break;
					case CELL_SIDE.BottomLeft: 
						or--;
						oc--;
						os = CELL_SIDE.TopRight;
						break;
					case CELL_SIDE.Left:
						oc--;
						os = CELL_SIDE.Right;
						break;
					case CELL_SIDE.Right:
						oc++;
						os = CELL_SIDE.Left;
						break;
					}
				}
				if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
					int oindex = CellGetIndex (or, oc);
					if (oindex >= 0) {
						cells [oindex].SetSideCrossCost (os, cost);
					}
				}
			}
		}

	
		/// <summary>
		/// Sets cost of entering a given hexagonal cell.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		[Obsolete ("Use CellSetCrossCost")]
		public void CellSetAllSidesCrossCost (int cellIndex, float cost) {
			CellSetCrossCost (cellIndex, cost);
		}


		/// <summary>
		/// Sets cost of entering or exiting a given hexagonal cell across any edge.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		public void CellSetCrossCost (int cellIndex, float cost, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			for (int side = 0; side < 8; side++) {
				CellSetSideCrossCost (cellIndex, (CELL_SIDE)side, cost, direction);
			}
		}


		/// <summary>
		/// Returns the cost of entering or exiting a given hexagonal cell without specifying a specific edge. This method is used along CellSetCrossCost which doesn't take into account per-edge costs.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		public float CellGetCrossCost (int cellIndex, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			return CellGetSideCrossCost (cellIndex, CELL_SIDE.Top, direction);
		}

		CELL_SIDE GetSideByVector (Vector2 dir) {
			switch (_gridTopology) {
			case GRID_TOPOLOGY.Box:
				if (Mathf.Abs (dir.x) > Mathf.Abs (dir.y)) {
					return dir.x < 0 ? CELL_SIDE.Right : CELL_SIDE.Left;
				} else {
					return dir.y < 0 ? CELL_SIDE.Top : CELL_SIDE.Bottom;
				}
			default:
				// hexagons
				if (dir.x == 0) {
					return dir.y < 0 ? CELL_SIDE.Top : CELL_SIDE.Bottom;
				} else if (dir.x < 0) {
					return dir.y < 0 ? CELL_SIDE.TopRight : CELL_SIDE.BottomRight;
				} else {
					return dir.y < 0 ? CELL_SIDE.TopLeft : CELL_SIDE.BottomLeft;
				}
			}
		}
					
		/// <summary>
		/// Sets the cost for going from one cell to another (both cells must be adjacent).
		/// </summary>
		/// <param name="cellStartIndex">Cell start index.</param>
		/// <param name="cellEndIndex">Cell end index.</param>
		public void CellSetCrossCost (int cellStartIndex, int cellEndIndex, float cost) {
			if (cellStartIndex < 0 || cellStartIndex >= cells.Count || cellEndIndex < 0 || cellEndIndex >= cells.Count)
				return;
			CELL_SIDE side = GetSideByVector (cells [cellStartIndex].center - cells [cellEndIndex].center);
			CellSetSideCrossCost (cellEndIndex, side, cost, CELL_DIRECTION.Entering);
		}

		/// <summary>
		/// Returns the cost of going from one cell to another (both cells must be adjacent)
		/// </summary>
		/// <returns>The get cross cost.</returns>
		/// <param name="cellStartIndex">Cell start index.</param>
		/// <param name="cellEndIndex">Cell end index.</param>
		public float CellGetCrossCost (int cellStartIndex, int cellEndIndex) {
			if (cellStartIndex < 0 || cellStartIndex >= cells.Count || cellEndIndex < 0 || cellEndIndex >= cells.Count)
				return 0;
			CELL_SIDE side = GetSideByVector (cells [cellStartIndex].center - cells [cellEndIndex].center);
			return CellGetSideCrossCost (cellEndIndex, side, CELL_DIRECTION.Entering);
		}

		/// Gets the cost of crossing any hexagon side.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the cell.</param>/// 
		/// <param name="direction">The direction for getting the cost. Entering or exiting values are acceptable. Both will return the entering cost.</param>
		public float CellGetSideCrossCost (int cellIndex, CELL_SIDE side, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			Cell cell = cells [cellIndex];
			if (direction == CELL_DIRECTION.Exiting) {
				return cell.GetSideCrossCost (side);
			}
			int r = cell.row;
			int c = cell.column;
			int or = r, oc = c;
			CELL_SIDE os = side;
			if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					if (oc % 2 != 0) {
						or--;
					}
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					if (oc % 2 == 0) {
						or++;
					}
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					if (oc % 2 == 0) {
						or++;
					}
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					if (oc % 2 != 0) {
						or--;
					}
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				}
			} else {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					or--;
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					or++;
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					or++;
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					or--;
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				case CELL_SIDE.Right:
					oc++;
					os = CELL_SIDE.Left;
					break;
				case CELL_SIDE.Left:
					oc--;
					os = CELL_SIDE.Right;
					break;
				}
			}
			if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
				int oindex = CellGetIndex (or, oc);
				return cells [oindex].GetSideCrossCost (os);
			} else {
				return 0;
			}
		}

		/// <summary>
		/// Makes a side of a cell block the LOS.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the cell.</param>
		/// <param name="blocks">Status of the block.</param>
		public void CellSetSideBlocksLOS (int cellIndex, CELL_SIDE side, bool blocks) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			Cell cell = cells [cellIndex];
			cell.SetSideBlocksLOS (side, blocks);

			int r = cell.row;
			int c = cell.column;
			int or = r, oc = c;
			CELL_SIDE os = side;
			if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					if (oc % 2 != 0) {
						or--;
					}
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					if (oc % 2 == 0) {
						or++;
					}
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					if (oc % 2 == 0) {
						or++;
					}
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					if (oc % 2 != 0) {
						or--;
					}
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				}
			} else {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					or--;
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					or++;
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					or++;
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					or--;
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				case CELL_SIDE.Left:
					oc--;
					os = CELL_SIDE.Right;
					break;
				case CELL_SIDE.Right:
					oc++;
					os = CELL_SIDE.Left;
					break;
				}
			}
			if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
				int oindex = CellGetIndex (or, oc);
				if (oindex >= 0) {
					cells [oindex].SetSideBlocksLOS (os, blocks);
				}
			}
		}

		/// </summary>
		/// Returns true if the side of a cell blocks LOS.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the cell.</param>/// 
		public bool CellGetSideBlocksLOS (int cellIndex, CELL_SIDE side) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			Cell cell = cells [cellIndex];
			if (cell.GetSideBlocksLOS (side))
				return true;
			int r = cell.row;
			int c = cell.column;
			int or = r, oc = c;
			CELL_SIDE os = side;
			if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					if (oc % 2 != 0) {
						or--;
					}
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					if (oc % 2 == 0) {
						or++;
					}
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					if (oc % 2 == 0) {
						or++;
					}
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					if (oc % 2 != 0) {
						or--;
					}
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				}
			} else {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					or--;
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					or++;
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					or++;
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					or--;
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				case CELL_SIDE.Right:
					oc++;
					os = CELL_SIDE.Left;
					break;
				case CELL_SIDE.Left:
					oc--;
					os = CELL_SIDE.Right;
					break;
				}
			}
			if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
				int oindex = CellGetIndex (or, oc);
				return cells [oindex].GetSideBlocksLOS (os);
			} else {
				return false;
			}
		}


		/// <summary>
		/// Texture cell by index.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Texture2D texture, bool useCanvasRect = false) {
			return CellToggle (cellIndex, visible, Color.white, false, texture, Misc.Vector2one, Misc.Vector2zero, 0, false, useCanvasRect);
		}

		/// <summary>
		/// Colorize cell by index.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Color color, bool refreshGeometry = false) {
			return CellToggle (cellIndex, visible, color, refreshGeometry, null, Misc.Vector2one, Misc.Vector2zero, 0);
		}

		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Color color, bool refreshGeometry, int textureIndex, bool useCanvasRect = false) {
			Texture2D texture = null;
			if (textureIndex >= 0 && textureIndex < textures.Length)
				texture = textures [textureIndex];
			return CellToggle (cellIndex, visible, color, refreshGeometry, texture, Misc.Vector2one, Misc.Vector2zero, 0, false, useCanvasRect);
		}

		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, bool useCanvasRect = false) {
			return CellToggle (cellIndex, visible, color, refreshGeometry, texture, Misc.Vector2one, Misc.Vector2zero, 0, false, useCanvasRect);
		}

		/// <summary>
		/// Colorize specified region of a territory by indexes.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
			return CellToggle (cellIndex, visible, color, refreshGeometry, texture, textureScale, textureOffset, textureRotation, false, false);
		}

		/// <summary>
		/// Colorize specified region of a territory by indexes.
		/// </summary>
		public GameObject CellToggle (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, bool useCanvasRect = false) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;

			if (!visible) {
				CellHide (cellIndex);
				return null;
			}
			int cacheIndex = GetCacheIndexForCellRegion (cellIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (existsInCache && surfaces [cacheIndex] == null) {
				surfaces.Remove (cacheIndex);
				existsInCache = false;
			}
			if (refreshGeometry && existsInCache) {
				GameObject obj = surfaces [cacheIndex];
				surfaces.Remove (cacheIndex);
				DestroyImmediate (obj);
				existsInCache = false;
			}
			GameObject surf = null;
			Region region = cells [cellIndex].region;
			if (existsInCache)
				surf = surfaces [cacheIndex];
			
			// Should the surface be recreated?
			Material surfMaterial; 
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (textureScale != region.customTextureScale || textureOffset != region.customTextureOffset || textureRotation != region.customTextureRotation)) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = cellHighlightedIndex == cellIndex;
			if (surf != null) {
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if ((texture == null && !surfMaterial.name.Equals (coloredMat.name)) || (texture != null && !surfMaterial.name.Equals (texturizedMat.name))
				    || (surfMaterial.color != color && !isHighlighted) || (texture != null && (region.customMaterial == null || region.customMaterial.mainTexture != texture))) {
					Material goodMaterial = GetColoredTexturedMaterial (color, texture);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (surf, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial (color, texture);
				surf = GenerateCellRegionSurface (cellIndex, surfMaterial, textureScale, textureOffset, textureRotation, rotateInLocalSpace, useCanvasRect);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureScale = textureScale;
				region.customTextureRotation = textureRotation;
				region.customTextureRotationInLocalSpace = rotateInLocalSpace;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted) {
				if (region.customMaterial != null) {
					hudMatCell.mainTexture = region.customMaterial.mainTexture;
				} else {
					hudMatCell.mainTexture = null;
				}
				surf.GetComponent<Renderer> ().sharedMaterial = hudMatCell;
				_highlightedObj = surf;
			}

			if (!cells [cellIndex].visible)
				surf.SetActive (false);
			return surf;
		}

		/// <summary>
		/// Uncolorize/hide specified cell by index in the cells collection.
		/// </summary>
		public void CellHide (int cellIndex) {
			if (_cellHighlightedIndex != cellIndex) {
				int cacheIndex = GetCacheIndexForCellRegion (cellIndex);
				if (surfaces.ContainsKey (cacheIndex)) {
					if (surfaces [cacheIndex] == null) {
						surfaces.Remove (cacheIndex);
					} else {
						surfaces [cacheIndex].SetActive (false);
					}
				}
			}
			cells [cellIndex].region.customMaterial = null;
		}

		/// <summary>
		/// Uncolorize/hide specified all cells.
		/// </summary>
		public void CellHideAll () {
			for (int k = 0; k < cells.Count; k++) {
				CellHide (k);
			}
		}

		/// <summary>
		/// Get a list of cells which are nearer than a given distance in cell count
		/// </summary>
		public List<int> CellGetNeighbours (int cellIndex, int maxDistance, float maxCost = -1, int cellGroupMask = -1, CanCrossCheckType canCrossCheck = CanCrossCheckType.Default, bool sortByDistance = false) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			Cell cell = cells [cellIndex];
			List<int> cc = new List<int> ();
            float dummyCost;
			for (int x = cell.column - maxDistance; x <= cell.column + maxDistance; x++) {
				if (x < 0 || x >= _cellColumnCount)
					continue;
				for (int y = cell.row - maxDistance; y <= cell.row + maxDistance; y++) {
					if (y < 0 || y >= _cellRowCount)
						continue;
					if (x == cell.column && y == cell.row)
						continue;
					int ci = CellGetIndex (y, x);
					List<int> steps = FindPath (cellIndex, ci, maxCost, out dummyCost, 0, cellGroupMask, canCrossCheck);
					if (steps != null && steps.Count <= maxDistance) {
						cc.Add (ci);
					}
				}
			}
			if (sortByDistance) {
				Vector2 origin = cells[cellIndex].center;
				cc.Sort((x, y) => {
					Cell cellX = cells[x];
					float dx = cellX.center.x - origin.x;
					float dy = cellX.center.y - origin.y;
					float dist1 = dx * dx + dy * dy;
					Cell cellY = cells[y];
					dx = cellY.center.x - origin.x;
					dy = cellY.center.y - origin.x;
					float dist2 = dx * dx + dy * dy;
					if (dist1 < dist2) {
						return -1;
					} else if (dist1 > dist2) {
						return 1;
					} else {
						return 0;
					}
				});
			}
			return cc;
		}

		/// <summary>
		/// Specifies the cell group (by default 1) used by FindPath cellGroupMask optional argument
		/// </summary>
		public void CellSetGroup (int cellIndex, int group) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].group = group;
			needRefreshRouteMatrix = true;
		}

		/// <summary>
		/// Returns cell group (default 1)
		/// </summary>
		public int CellGetGroup (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].group;
		}

		/// <summary>
		/// Returns the indices of all cells belonging to a group in the indices array which must be fully allocated when passed. Also the length of this array determines the maximum number of indices returned.
		/// This method returns the actual number of indices returned, regardless of the length the array. This design helps reduce heap allocations.
		/// </summary>
		public int CellGetFromGroup (int group, int[] indices) {
			if (indices == null || cells == null)
				return 0;
			int cellCount = cells.Count;
			int count = 0;
			for (int k = 0; k < cellCount && k < indices.Length; k++) {
				if (cells [k].group == group) {
					indices [count++] = k;
				}
			}
			return count;
		}

		
		/// <summary>
		/// Colors a cell and fades it out for "duration" in seconds.
		/// </summary>
		public void CellFadeOut (Cell cell, Color color, float duration) {
			int cellIndex = CellGetIndex (cell);
			CellFadeOut (cellIndex, color, duration);
		}

		/// <summary>
		/// Colors a cell and fades it out during "duration" in seconds.
		/// </summary>
		public void CellFadeOut (int cellIndex, Color color, float duration) {
			CellAnimate (FADER_STYLE.FadeOut, cellIndex, color, duration);
		}

		/// <summary>
		/// Fades out a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellFadeOut (List<int>cellIndices, Color color, float duration) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.FadeOut, cellIndices [k], color, duration);
			}
		}

		/// <summary>
		/// Flashes a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (Cell cell, Color color, float duration) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Flash, cellIndex, color, duration);
		}

		
		/// <summary>
		/// Flashes a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (int cellIndex, Color color, float duration) {
			CellAnimate (FADER_STYLE.Flash, cellIndex, color, duration);
		}

		/// <summary>
		/// Flashes a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (List<int>cellIndices, Color color, float duration) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Flash, cellIndices [k], color, duration);
			}
		}

		/// <summary>
		/// Blinks a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (Cell cell, Color color, float duration) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Blink, cellIndex, color, duration);
		}


		/// <summary>
		/// Blinks a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (int cellIndex, Color color, float duration) {
			CellAnimate (FADER_STYLE.Blink, cellIndex, color, duration);
		}

		/// <summary>
		/// Blinks a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (List<int>cellIndices, Color color, float duration) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Blink, cellIndices [k], color, duration);
			}
		}

		/// <summary>
		/// Gets the cell's center position in world space.
		/// </summary>
		public Vector3 CellGetPosition (int cellIndex) {
			return CellGetPosition (cellIndex, 0f);
		}


		/// <summary>
		/// Gets the cell's center position in world space including a separation from the grid given in "elevation" argument
		/// </summary>
		public Vector3 CellGetPosition (int cellIndex, float elevation) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return Vector3.zero;
			Vector2 cellGridCenter = cells [cellIndex].center;
			return GetWorldSpacePosition (cellGridCenter, elevation);
		}

		/// <summary>
		/// Gets the cell's center position in world space.
		/// </summary>
		public Vector3 CellGetPosition (int row, int column) {
			return CellGetPosition (row, column, 0f);
		}

		
		/// <summary>
		/// Gets the cell's center position in world space including a separation from the grid given in "elevation" argument
		/// </summary>
		public Vector3 CellGetPosition (int row, int column, float elevation) {
			int cellIndex = CellGetIndex (row, column);
			if (cellIndex < 0)
				return Vector3.zero;
			return CellGetPosition (cellIndex, elevation);
		}

		/// <summary>
		/// Returns the rect enclosing the cell in world space
		/// </summary>
		public Bounds CellGetRectWorldSpace(int cellIndex) {
			if (cells == null || cellIndex < 0 || cellIndex >= cells.Count)
				return new Bounds(Misc.Vector3zero, Misc.Vector3zero);
			Rect rect = cells[cellIndex].region.rect2D;
			Vector3 min = GetWorldSpacePosition(rect.min);
			Vector3 max = GetWorldSpacePosition(rect.max);
			Bounds bounds = new Bounds((min + max) * 0.5f, max - min);
			return bounds;
		}

		/// <summary>
		/// Returns cell's row or -1 if cellIndex is not valid.
		/// </summary>
		public int CellGetRow (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].row;
		}

		/// <summary>
		/// Returns cell's column or -1 if cellIndex is not valid.
		/// </summary>
		public int CellGetColumn (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].column;
		}

		/// <summary>
		/// Returns the number of vertices of the cell
		/// </summary>
		public int CellGetVertexCount (int cellIndex) {
			return cells [cellIndex].region.points.Count;
		}

		/// <summary>
		/// Returns the world space position of the vertex
		/// </summary>
		public Vector3 CellGetVertexPosition (int cellIndex, int vertexIndex, float elevation = 0) {
			
			if (cells == null || cellIndex < 0 || cellIndex >= cells.Count || vertexIndex < 0)
				return Misc.Vector3zero;
			Cell cell = cells [cellIndex];
			if (vertexIndex >= cell.region.points.Count)
				return Misc.Vector3zero;
			Vector2 localPosition = cells [cellIndex].region.points [vertexIndex];
			return GetWorldSpacePosition (localPosition, elevation);
		}


		/// <summary>
		/// Returns a list of neighbour cells for specificed cell.
		/// </summary>
		public List<Cell> CellGetNeighbours (Cell cell) {
			int cellIndex = CellGetIndex (cell);
			return CellGetNeighbours (cellIndex);
		}

		/// <summary>
		/// Returns a list of neighbour cells for specificed cell index.
		/// </summary>
		public List<Cell> CellGetNeighbours (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			List<Cell> neighbours = new List<Cell> ();
			Region region = cells [cellIndex].region;
			for (int k = 0; k < region.neighbours.Count; k++) {
				neighbours.Add ((Cell)region.neighbours [k].entity);
			}
			return neighbours;
		}

		
		/// <summary>
		/// Returns cell's territory index to which it belongs to.
		/// </summary>
		public int CellGetTerritoryIndex (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].territoryIndex;
		}

		/// <summary>
		/// Returns current cell's fill color
		/// </summary>
		public Color CellGetColor (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return new Color (0, 0, 0, 0);
			return cells [cellIndex].region.customMaterial.color;
		}

		/// <summary>
		/// Returns current cell's fill texture
		/// </summary>
		public Texture2D CellGetTexture (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return null;
			return (Texture2D)cells [cellIndex].region.customMaterial.mainTexture;
		}

		/// <summary>
		/// Returns current cell's fill texture index (if texture exists in textures list).
		/// Texture index is from 1..32. It will return 0 if texture does not exist or it does not match any texture in the list of textures.
		/// </summary>
		public int CellGetTextureIndex (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return 0;
			Texture2D tex = (Texture2D)cells [cellIndex].region.customMaterial.mainTexture;
			for (int k = 1; k < textures.Length; k++) {
				if (tex == textures [k])
					return k;
			}
			return 0;
		}

		/// <summary>
		/// Set current cell's fill texture.
		/// </summary>
		public void CellSetTexture (int cellIndex, Texture2D texture) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			Cell cell = cells [cellIndex];
			GameObject cellSurface = null;
			int cacheIndex = GetCacheIndexForCellRegion (cellIndex);
			if (surfaces.ContainsKey (cacheIndex)) {
				if (surfaces [cacheIndex] != null) {
					cellSurface = surfaces [cacheIndex];
				}
			}
			if (texture != null) {
				if (cell.region.customMaterial == null) {
					cell.region.customMaterial = GetColoredTexturedMaterial (Color.white, texture);
				}
				if (cellSurface != null) {
					surfaces [cacheIndex].SetActive (true);
				}
			}
			if (cell.region.customMaterial != null) {
				cell.region.customMaterial.mainTexture = texture;
				if (texture != null && cellSurface == null) {
					CellToggle (cellIndex, true, texture);
				} else {
					Renderer renderer = cellSurface.GetComponent<Renderer> ();
					if (renderer != null)
						renderer.sharedMaterial = cell.region.customMaterial;
				}
			}
			if (_highlightedObj == cellSurface) {
				hudMatCell.mainTexture = texture;
			}
		}

		/// <summary>
		/// Set current cell's fill texture with a sprite.
		/// </summary>
		public GameObject CellSetSprite (int cellIndex, Color tintColor, Sprite sprite) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			int cacheIndex = GetCacheIndexForCellRegion (cellIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (existsInCache && surfaces [cacheIndex] == null) {
				surfaces.Remove (cacheIndex);
				existsInCache = false;
			}
			GameObject surf = null;
			Region region = cells [cellIndex].region;
			if (existsInCache)
				surf = surfaces [cacheIndex];
			
			// Should the surface be recreated?
			Material surfMaterial; 
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = cellHighlightedIndex == cellIndex;
			if (surf != null) {
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (!surfMaterial.name.Equals (texturizedMat.name) || (surfMaterial.color != tintColor && !isHighlighted) || ((region.customMaterial == null || region.customTextureOffset != sprite.textureRectOffset || region.customMaterial.mainTexture != sprite.texture))) {
					Material goodMaterial = GetColoredTexturedMaterial (tintColor, sprite.texture);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (surf, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial (tintColor, sprite.texture);
				surf = GenerateRegionSurfaceHexSprite (cellIndex, surfMaterial, sprite.textureRect);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = sprite.textureRectOffset;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted) {
				if (region.customMaterial != null) {
					hudMatCell.mainTexture = region.customMaterial.mainTexture;
				} else {
					hudMatCell.mainTexture = null;
				}
				surf.GetComponent<Renderer> ().sharedMaterial = hudMatCell;
				_highlightedObj = surf;
			}
			
			if (!cells [cellIndex].visible)
				surf.SetActive (false);
			return surf;
		}

		/// <summary>
		/// Colors a cell
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="color">Color.</param>
		public void CellSetColor(int cellIndex, Color color) {
			CellToggle(cellIndex, true, color);
		}

		/// <summary>
		/// Return true if cell is at border
		/// </summary>
		/// <returns><c>true</c>, if is border was celled, <c>false</c> otherwise.</returns>
		/// <param name="cellIndex">Cell index.</param>
		public bool CellIsBorder (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			Cell cell = cells [cellIndex];
			return (cell.column == 0 || cell.column == _cellColumnCount - 1 || cell.row == 0 || cell.row == _cellRowCount - 1);
		}


		/// <summary>
		/// Returns true if cell is visible
		/// </summary>
		public bool CellIsVisible (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			return cells [cellIndex].visible;
		}


		/// <summary>
		/// Specifies if a given cell is visible.
		/// </summary>
		public void CellSetVisible (int cellIndex, bool visible) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].visible = visible;
			needRefreshRouteMatrix = true;
		}



		/// <summary>
		/// Specifies if a given cell's border is visible.
		/// </summary>
		public void CellSetBorderVisible (int cellIndex, bool visible) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].borderVisible = visible;
		}


		/// <summary>
		/// Merges cell2 into cell1. Cell2 is removed.
		/// Only cells which are neighbours can be merged.
		/// </summary>
		public bool CellMerge (Cell cell1, Cell cell2) {
			if (cell1 == null || cell2 == null)
				return false;
			if (!cell1.region.neighbours.Contains (cell2.region))
				return false;
			cell1.center = (cell2.center + cell1.center) / 2.0f;
			// Polygon UNION operation between both regions
			PolygonClipper pc = new PolygonClipper (cell1.region.polygon, cell2.region.polygon);
			pc.Compute (PolygonOp.UNION);
			// Remove cell2 from lists
			CellRemove (cell2);
			// Updates geometry data on cell1
			UpdateCellGeometry (cell1, pc.subject);
			return true;
		}

		/// <summary>
		/// Removes a cell from the cells and territories lists. Note that this operation only removes cell structure but does not affect polygons - mostly internally used
		/// </summary>
		public void CellRemove (Cell cell) {
			int territoryIndex = cell.territoryIndex;
			if (territoryIndex >= 0) {
				if (territories [territoryIndex].cells.Contains (cell)) {
					territories [territoryIndex].cells.Remove (cell);
				}
			}
			// remove cell from global list
			if (cells.Contains (cell))
				cells.Remove (cell);

			needRefreshRouteMatrix = true;
			needUpdateTerritories = true;
		}

		/// <summary>
		/// Tags a cell with a user-defined integer tag. Cell can be later retrieved very quickly using CellGetWithTag.
		/// </summary>
		public void CellSetTag (Cell cell, int tag) {
			// remove previous tag register
			if (cellTagged.ContainsKey (cell.tag)) {
				cellTagged.Remove (cell.tag);
			}
			// override existing tag
			if (cellTagged.ContainsKey (tag)) {
				cellTagged.Remove (tag);
			}
			cellTagged.Add (tag, cell);
			cell.tag = tag;
		}

		/// <summary>
		/// Tags a cell with a user-defined integer tag. Cell can be later retrieved very quickly using CellGetWithTag.
		/// </summary>
		public void CellSetTag (int cellIndex, int tag) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			CellSetTag (cells [cellIndex], tag);
		}

		/// <summary>
		/// Returns the tag value of a given cell.
		/// </summary>
		public int CellGetTag (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			return cells [cellIndex].tag;
		}

		/// <summary>
		/// Retrieves Cell object with associated tag.
		/// </summary>
		public Cell CellGetWithTag (int tag) {
			if (cellTagged.ContainsKey (tag))
				return cellTagged [tag];
			return null;
		}


		/// <summary>
		/// Returns the cell object under position in local coordinates
		/// </summary>
		public Cell CellGetAtPosition (Vector3 localPosition) {
			return GetCellAtPoint (localPosition, false);
		}

		/// <summary>
		/// Returns the cell object under position in local or worldSpace coordinates
		/// </summary>
		public Cell CellGetAtPosition (Vector3 position, bool worldSpace) {
			return GetCellAtPoint (position, worldSpace);
		}

		/// <summary>
		/// Returns the cell located at given row and column
		/// </summary>
		public Cell CellGetAtPosition (int column, int row) {
			int cellsCount = cells.Count;
			if (cellsCount != _cellColumnCount * _cellRowCount) {
				// cells may be merged so use traditional method
				float x = (column + 0.5f) / _cellColumnCount - 0.5f;
				float y = (row + 0.5f) / _cellRowCount - 0.5f;
				return CellGetAtPosition (new Vector3 (x, y, 0));
			}
			int index = row * _cellColumnCount + column;
			if (index >= 0 && index < cells.Count)
				return cells [index];
			return null;
		}


		/// <summary>
		/// Sets the territory of a cell triggering territory boundary recalculation
		/// </summary>
		/// <returns><c>true</c>, if cell was transferred., <c>false</c> otherwise.</returns>
		public bool CellSetTerritory (int cellIndex, int territoryIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			Cell cell = cells [cellIndex];
			if (cell.territoryIndex >= 0 && cell.territoryIndex < territories.Count && territories [cell.territoryIndex].cells.Contains (cell)) {
				territories [cell.territoryIndex].cells.Remove (cell);
			}
			cells [cellIndex].territoryIndex = territoryIndex;
			needUpdateTerritories = true;
			return true;
		}

		/// <summary>
		/// Returns a string-packed representation of current cells settings.
		/// Each cell separated by ;
		/// Individual settings mean:
		/// Position	Meaning
		/// 0			Visibility (0 = invisible, 1 = visible)
		/// 1			Territory Index
		/// 2			Color R (0..1)
		/// 3			Color G (0..1)
		/// 4			Color B (0..1)
		/// 5			Color A (0..1)
		/// 6			Texture Index
		/// </summary>
		/// <returns>The get configuration data.</returns>
		public string CellGetConfigurationData () {
			StringBuilder sb = new StringBuilder ();
			for (int k = 0; k < cells.Count; k++) {
				if (k > 0)
					sb.Append (";");
				// 0
				Cell cell = cells [k];
				if (cell.visible) {
					sb.Append ("1");
				} else {
					sb.Append ("0");
				}
				// 1
				sb.Append (",");
				sb.Append (cell.territoryIndex);
				// 2
				sb.Append (",");
				Color color = CellGetColor (k);
				sb.Append (color.a.ToString ("F3", CultureInfo.InvariantCulture));
				// 3
				sb.Append (",");
				sb.Append (color.r.ToString ("F3", CultureInfo.InvariantCulture));
				// 4
				sb.Append (",");
				sb.Append (color.g.ToString ("F3", CultureInfo.InvariantCulture));
				// 5
				sb.Append (",");
				sb.Append (color.b.ToString ("F3", CultureInfo.InvariantCulture));
				// 6
				sb.Append (",");
				sb.Append (CellGetTextureIndex (k));
				// 7 tag
				sb.Append (",");
				sb.Append (cell.tag);
			}
			return sb.ToString ();
		}


		public void CellSetConfigurationData (string cellData) {
			if (cells == null)
				return;
			string[] cellsInfo = cellData.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			char[] separators = new char[] { ',' };
			int cellsCount = cells.Count;
			for (int k = 0; k < cellsInfo.Length && k < cellsCount; k++) {
				string[] cellInfo = cellsInfo [k].Split (separators, StringSplitOptions.RemoveEmptyEntries);
				int length = cellInfo.Length;
				if (length > 0) {
					if (cellInfo [0].Length > 0) {
						cells [k].visible = cellInfo [0] [0] != '0'; // cellInfo[0].Equals("0");
					}
				}
				if (length > 1) {
					cells [k].territoryIndex = FastConvertToInt (cellInfo [1]);
				}
				Color color = new Color (0, 0, 0, 0);
				if (length > 5) {
					Single.TryParse (cellInfo [2], out color.a);
					if (color.a > 0) {
						Single.TryParse (cellInfo [3], NumberStyles.Any, CultureInfo.InvariantCulture, out color.r);
						Single.TryParse (cellInfo [4], NumberStyles.Any, CultureInfo.InvariantCulture, out color.g);
						Single.TryParse (cellInfo [5], NumberStyles.Any, CultureInfo.InvariantCulture, out color.b);
					}
				} 
				int textureIndex = -1;
				if (length > 6) {
					textureIndex = FastConvertToInt (cellInfo [6]);
				}
				if (color.a > 0 || textureIndex >= 1) {
					CellToggle (k, true, color, false, textureIndex);
				}
				if (length > 7) {
					CellSetTag (k, FastConvertToInt (cellInfo [7]));
				}
			}
			needUpdateTerritories = true;
			needRefreshRouteMatrix = true;
			Redraw ();
			isDirty = true;
		}

		#endregion


	
	}
}

