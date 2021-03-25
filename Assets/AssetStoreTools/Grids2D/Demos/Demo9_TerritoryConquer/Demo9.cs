using UnityEngine;
using System.Collections;

namespace Grids2D {
	public class Demo9 : MonoBehaviour {

		Grid2D grid;
		GUIStyle labelStyle;

		void Start () {
			// setup GUI styles
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.black;

			// Get a reference to Grids 2D System's API
			grid = Grid2D.instance;

			// listen to events
			grid.OnCellClick += (int cellIndex) => toggleCell(cellIndex);

		}

		void OnGUI () {
			GUI.Label (new Rect (0, 5, Screen.width, 30), "Click on any frontier cell to transfer it to the opposite territory.", labelStyle);
			GUI.Label (new Rect (0, 25, Screen.width, 30), "Note that territories can't be split between two or more areas.", labelStyle);
			GUI.Label (new Rect (0, 45, Screen.width, 30), "If you need separate areas, just color cells with same 'territory color' and don't use territories.", labelStyle);
		}

		void toggleCell(int cellIndex) {
			int currentTerritory = grid.cells[cellIndex].territoryIndex;
			if (currentTerritory == 0) {
				currentTerritory = 1;
			} else {
				currentTerritory = 0;
			}
			grid.CellSetTerritory(cellIndex, currentTerritory);
			grid.Redraw();
		}


	}
}
