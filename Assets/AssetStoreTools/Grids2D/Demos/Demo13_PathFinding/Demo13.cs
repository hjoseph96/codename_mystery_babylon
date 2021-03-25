using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D {
	public class Demo13 : MonoBehaviour {
	
		Grid2D grid;
		bool isSelectingStart;
		int cellStartIndex;
		GUIStyle labelStyle;

		// example enums for grouping cells
		const int CELLS_ALL = -1;
		const int CELL_DEFAULT = 1;
		const int CELL_PLAYER = 2;
		const int CELL_ENEMY = 4;
		const int CELL_OBSTACLE = 8;
		const int CELLS_ALL_NAVIGATABLE = ~(CELL_OBSTACLE | CELL_PLAYER | CELL_ENEMY);

		// Use this for initialization
		void Start () {
			grid = Grid2D.instance;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;

			isSelectingStart = true;

			Random.InitState (2);

			// Draw some blocked areas
			for (int i = 0; i < 25; i++) {

				int row = Random.Range (2, grid.rowCount - 3);
				int col = Random.Range (2, grid.columnCount - 3);

				for (int j = -2; j <= 2; j++) {
					for (int k = -2; k <= 2; k++) {
						int cellIndex = grid.CellGetIndex (row + j, col + k);
						grid.CellSetGroup (cellIndex, CELL_OBSTACLE);
						grid.CellToggle (cellIndex, true, Color.white);
					}
				}
			}


			// Example: sets crossing costs for hexagon sides and draw a line
			int barrierCost = 100;
			Color barrierColor = Color.blue;
			float barrierWidth = 5f;
			for (int k = 0; k < 10; k++) {
				int cellIndex = grid.CellGetIndex (10, k + 20);
				if (grid.cells [cellIndex].group == CELL_OBSTACLE)
					continue;
				if (grid.cells [cellIndex].column % 2 == 0) {
					// Assign a crossing cost to barrier for path-finding purposes
					grid.CellSetSideCrossCost (cellIndex, CELL_SIDE.Top, barrierCost);
					grid.CellSetSideCrossCost (cellIndex, CELL_SIDE.TopLeft, barrierCost);
					grid.CellSetSideCrossCost (cellIndex, CELL_SIDE.TopRight, barrierCost);
					// Make the barrier block LOS (Line-Of-Sight)
					grid.CellSetSideBlocksLOS (cellIndex, CELL_SIDE.Top, true);
					grid.CellSetSideBlocksLOS (cellIndex, CELL_SIDE.TopLeft, true);
					grid.CellSetSideBlocksLOS (cellIndex, CELL_SIDE.TopRight, true);
					// Draw the barrier
					grid.DrawLine (cellIndex, CELL_SIDE.Top, barrierColor, barrierWidth);
					grid.DrawLine (cellIndex, CELL_SIDE.TopLeft, barrierColor, barrierWidth);
					grid.DrawLine (cellIndex, CELL_SIDE.TopRight, barrierColor, barrierWidth);
				} else {
					// Assign a crossing cost to barrier for path-finding purposes
					grid.CellSetSideCrossCost (cellIndex, CELL_SIDE.Top, barrierCost);
					// Make the barrier block LOS (Line-Of-Sight)
					grid.CellSetSideBlocksLOS (cellIndex, CELL_SIDE.Top, true);
					// Draw the barrier
					grid.DrawLine (cellIndex, CELL_SIDE.Top, barrierColor, barrierWidth);
				}
			}


			// Hook into cell click event to toggle start selection or draw a computed path using A* path finding algorithm
			grid.OnCellClick += (cellIndex) => BuildPath (cellIndex);

			grid.OnCellEnter += (cellIndex) => ShowLineOfSight (cellIndex);


		}


		void BuildPath (int clickedCellIndex) {

			DestroyLOSMarkers ();

			if (isSelectingStart) {
				// Selects start cell
				cellStartIndex = clickedCellIndex;
				grid.CellToggle (cellStartIndex, true, Color.yellow);
			} else {
				// Clicked on the end cell, then show the path
				// First clear color of start cell
				grid.CellToggle (cellStartIndex, false, Color.white);
				// Get Path
				List<int> path = grid.FindPath (cellStartIndex, clickedCellIndex, cellGroupMask: CELLS_ALL_NAVIGATABLE);
				// Color the path
				if (path != null) {
					for (int k = 0; k < path.Count; k++) {
						grid.CellFadeOut (path [k], Color.green, 1f);
					}
				}
			}
			isSelectingStart = !isSelectingStart;
		}

		void OnGUI () {
			// Do autoresizing of GUI layer
			GUIResizer.AutoResize ();
			
			if (isSelectingStart) {
				GUI.Label (new Rect (10, 10, 160, 30), "Select the starting cell", labelStyle);
			} else {
				GUI.Label (new Rect (10, 10, 160, 30), "Select the ending cell", labelStyle);
			}

			if (automate) {
				GUI.Label (new Rect (10, 30, 160, 30), "Press A again to stop.", labelStyle);
			} else {
				GUI.Label (new Rect (10, 30, 160, 30), "Or press A to automate.", labelStyle);
				GUI.Label (new Rect (10, 50, 160, 30), "Or press R for range (6) sample.", labelStyle);
			}
		}


		bool automate;

		void Update () {
			if (Input.GetKeyDown (KeyCode.R))
				ShowRange ();
			if (Input.GetKeyDown (KeyCode.A))
				automate = !automate;

			if (Input.GetKeyDown (KeyCode.B)) {
				grid.columnCount = 199;
			}
			if (automate) {
				// Random paths
				int cellFrom = Random.Range (0, grid.numCells - 1);
				int cellTo = Random.Range (0, grid.numCells - 1);
				List<int> path = grid.FindPath (cellFrom, cellTo, 0, 0, CELLS_ALL_NAVIGATABLE);
				if (path != null) {
					grid.CellFadeOut (path, Color.green, 1f);
				}
			}
		}

		void ShowRange () {
			List<int> path = grid.CellGetNeighbours (grid.cellHighlightedIndex, 6, -1, CELLS_ALL_NAVIGATABLE);
            if (path != null) {
				for (int k = 0; k < path.Count; k++) {
					grid.CellFlash (path [k], Color.green, 1f);
				}
			}
		}

		// LOS (Line of sight) example
		List<int> cellIndices;
		List<Vector3> worldPositions;
		List<GameObject> LOSMarkers;

		void ShowLineOfSight (int targetCellIndex) {
			if (isSelectingStart || automate)
				return;
												
			// Destroys markers of a previous LOS
			DestroyLOSMarkers ();

			// Compute LOS and get list of cell indices and world positions
			bool isLOS = grid.CellGetLineOfSight (cellStartIndex, targetCellIndex, ref cellIndices, ref worldPositions, CELLS_ALL_NAVIGATABLE);

			// Add small dots (spheres) along the LOS
			worldPositions.ForEach ((Vector3 obj) => {
				GameObject sphere =	GameObject.CreatePrimitive (PrimitiveType.Sphere);
				LOSMarkers.Add (sphere);
				sphere.transform.position = obj;
				sphere.transform.localScale = Vector3.one * 5f;
				sphere.GetComponent<Renderer> ().material.color = isLOS ? Color.green : Color.red;
			});

		}

		void DestroyLOSMarkers () {
			if (LOSMarkers == null) {
				LOSMarkers = new List<GameObject> ();
			} else {
				LOSMarkers.ForEach ((GameObject obj) => DestroyImmediate (obj));
			}
		}
	
	}
}