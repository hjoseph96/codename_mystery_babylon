using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{
	public class Demo16 : MonoBehaviour
	{
	
		public GameObject token;
		Grid2D grid;
		GUIStyle labelStyle;
		int movePoints;
		int tokenRow, tokenColumn;

		// Use this for initialization
		void Start ()
		{
			grid = Grid2D.instance;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500); 
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;

			// Configure the grid crossing cost depending on the textures
			for (int k=0;k<grid.cells.Count;k++) {
				int textureIndex = grid.CellGetTextureIndex(k);
				if (textureIndex==2) {	
					grid.CellSetCrossCost(k, 10);
				} else {
					grid.CellSetCrossCost(k, 1);
				}
			}

			// Hook into cell click event to toggle start selection or draw a computed path using A* path finding algorithm
			grid.OnCellClick += BuildPath;

			// Position the token 
			tokenRow = grid.rowCount-1;
			tokenColumn = 5;
			Vector3 position = grid.CellGetPosition(tokenRow, tokenColumn);
			token.transform.position = position;

			// Prepare move points and show available positions
			movePoints = 10;
		}

	
		void OnGUI ()
		{
			// Do autoresizing of GUI layer
			GUIResizer.AutoResize ();
			
			GUI.Label (new Rect (10, 10, 250, 30), "Sea movement cost: 1 point");
			GUI.Label (new Rect (10, 30, 250, 30), "Land movement cost: 10 points");
			GUI.Label (new Rect (10, 50, 250, 30), "Press R to show movement range.");

			if (movePoints > 5 ||  ((int)Time.time) % 2 != 0) {
				GUI.Label (new Rect (10, 80, 250, 30), "Ship move points: " + movePoints);
			}
			if (movePoints<5) {
				GUI.Label (new Rect (10, 100, 250, 30), "Press M to add more move points.");
			}
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.M)) {
				movePoints += 10;
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				ShowMoveRange();
			}
		}


		void BuildPath (int clickedCellIndex)
		{
			// If clicked cell can't be crossed, return
			if (!grid.cells[clickedCellIndex].canCross) return;
			
			// Get Path
			int cellIndex = grid.CellGetIndex(tokenRow, tokenColumn);
			List<int> path = grid.FindPath (cellIndex, clickedCellIndex, movePoints);
			if (path!=null) {
				movePoints -= path.Count;
				// Color the path
				for (int k=0; k<path.Count; k++) {
					grid.CellFlash (path [k], Color.green, 1f);
				}
				// Start animating/moving the ship
				StartCoroutine(MoveShipAlongPath(path));
				// Annotate new ship row and column
				tokenRow = grid.CellGetRow(clickedCellIndex);
				tokenColumn = grid.CellGetColumn(clickedCellIndex);
			} else {
				// Indicate that cell is not reachable
				grid.CellFlash (clickedCellIndex, Color.red, 1f);
			}
		}

		IEnumerator MoveShipAlongPath(List<int>path) {
			for (int k=0;k<path.Count;k++) {
				Vector3 position = grid.CellGetPosition(path[k]);
				token.transform.position = position;
				yield return new WaitForSeconds(0.1f);
			}
		}


		void ShowMoveRange() {
			int cellIndex = grid.CellGetIndex(tokenRow, tokenColumn);
			List<int>cells = grid.CellGetNeighbours(cellIndex, 100, movePoints);
			grid.CellBlink (cells, Color.blue, 1f);
		}
	
	}
}