using UnityEngine;
using System.Collections;

namespace Grids2D {
	public class Demo4 : MonoBehaviour {

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
			grid.OnCellClick += (int cellIndex) => toggleCellVisible(cellIndex);

			// assign a canvas texture
			grid.canvasTexture = Resources.Load<Texture2D>("Textures/worldMap");
		}

		void OnGUI () {
			GUI.Label (new Rect (0, 5, Screen.width, 30), "Click on any position to reveal part of the canvas texture.", labelStyle);
		}

		void toggleCellVisible(int cellIndex) {
			grid.CellToggle(cellIndex, true, Color.white, false, grid.canvasTexture);
		}



	}
}
