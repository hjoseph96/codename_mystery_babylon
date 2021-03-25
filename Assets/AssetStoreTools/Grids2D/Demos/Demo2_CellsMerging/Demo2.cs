using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Grids2D {
	public class Demo2 : MonoBehaviour {

		Grid2D grid;
		GUIStyle labelStyle;
		string cellNoString= "No cell selected";
		Cell lastCell = null;
		List<Cell> neighbours = new List<Cell>();

		void Start () {
			grid = Grid2D.instance;

			// setup GUI styles
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
		}

		void OnGUI () {
			GUI.Label (new Rect (0, 5, Screen.width, 30), "Left click to select first cell. Right click to show info.", labelStyle);
			GUI.Label(new Rect(0, 40, Screen.width, 30), cellNoString, labelStyle);
		}

		void Update() {
			if (grid.cellHighlighted!=null) {
				if (Input.GetMouseButtonDown(0)) {
					MergeCell(grid.cellHighlighted);
				} else if (Input.GetMouseButtonDown(1)) {
					cellNoString = "Cell #" + grid.cellHighlightedIndex + " Territory #" + grid.cellHighlighted.territoryIndex;
				}
			}
		}

		/// <summary>
		/// Merge cell example.
		/// </summary>
		void MergeCell(Cell cell1) {

			if (lastCell != null)
			{
				neighbours = grid.CellGetNeighbours(cell1);
				if (neighbours.Contains(lastCell)) {
					grid.CellMerge(lastCell, cell1);
					cellNoString = "Cells merged!";
				} else {
					cellNoString = "Cell is not a neighbour!";
				}
				lastCell = null;
			}
			else
			{
				lastCell = cell1;
				cellNoString = "Now, click on a second cell to merge it.";
			}

			grid.Redraw();
		}

	}
}
