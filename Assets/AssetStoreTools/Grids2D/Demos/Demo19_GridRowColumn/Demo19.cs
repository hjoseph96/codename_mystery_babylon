using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D {
	public class Demo19 : MonoBehaviour {

		// These fields are references to the cell text gameobject and the canvas input field
		public GameObject cellNumber;
		public InputField inputField;
		public Text shotCountText;
		public int maxShotCount = 20;

		// Private fields
		Grid2D grid;
		int shotCount;

		void Start () {
			// Get a reference to Grids 2D System's API
			grid = Grid2D.instance;

			// Create the cell titles
			CreateCellTitles ();
		}


		void CreateCellTitles () {

			// Row titles
			for (int k = 0; k < grid.rowCount; k++) {
				GameObject rowNumber = Instantiate<GameObject> (cellNumber);
				TextMesh tm = rowNumber.GetComponent<TextMesh> ();
				tm.text = (grid.rowCount - k).ToString ();
				// Position the text
				Vector3 textPos = grid.CellGetPosition (k, 0);
				// Near to the left edge of the grid
				textPos.x = grid.transform.TransformPoint (new Vector3 (-0.5f, 0, 0)).x - 0.25f;
				rowNumber.transform.position = textPos;
				rowNumber.SetActive (true);
			}

			// Column titles
			for (int k = 0; k < grid.columnCount; k++) {
				GameObject columNumber = Instantiate<GameObject> (cellNumber);
				TextMesh tm = columNumber.GetComponent<TextMesh> ();
				tm.text = ((char)('A' + k)).ToString ();
				// Position the text
				Vector3 textPos = grid.CellGetPosition (grid.rowCount - 1, k);
				// Near to the top edge of the grid
				textPos.y = grid.transform.TransformPoint (new Vector3 (0, 0.5f, 0)).y + 0.25f;
				columNumber.transform.position = textPos;
				columNumber.SetActive (true);
			}
		}


		public void ChangeColor () {

			// Max shots
			if (shotCount >= maxShotCount)
				return;

			// Read row and column from the input field
			string rowString = "", columnString = "";
			string cellNumber = inputField.text;
			for (int k = 0; k < cellNumber.Length; k++) {
				if (cellNumber [k] >= '9') {
					rowString = cellNumber.Substring (0, k);
					columnString = cellNumber.Substring (k).ToUpper ();
					break;
				}
			}
			if (rowString == "") {
				Debug.Log ("Cell format is incorrect. Enter row followed by column letter without spaces or separators.");
				return;
			}

			// Convert row and column to integers
			int row, column;
			if (!int.TryParse (rowString, out row))
				return;
			// Remember to invert the row to match our distribution top->down
			row = grid.rowCount - row;
			column = columnString [0] - 'A';

			// Safety check to avoid referencing a non-existing cell
			if (row < 0 || row >= grid.rowCount || column < 0 || column >= grid.columnCount)
				return;

			// Get the cell index at that row / column and color it
			int cellIndex = grid.CellGetIndex (row, column);
			Color color = new Color (Random.value, Random.value, Random.value);
			grid.CellSetColor (cellIndex, color);

			// Increase and annotate shot count
			shotCount++;
			shotCountText.text = "Shots: " + shotCount;
		}

	}
}
