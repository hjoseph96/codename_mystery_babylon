using UnityEngine;
using System.Collections;

namespace Grids2D {
	public class DemoOrtho : MonoBehaviour {

		Grid2D grid;
		GUIStyle labelStyle;

		void Start () {
			grid = Grid2D.instance;
		}

		void OnGUI () {
			if (labelStyle==null) {
				// setup GUI styles
				labelStyle = new GUIStyle ();
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.normal.textColor = Color.white;
			}
			GUI.Label (new Rect (0, 5, Screen.width, 30), "Click on any cell.", labelStyle);
		}

		void Update() {
			if (grid.cellHighlighted!=null) {
				if (Input.GetMouseButtonDown(0)) {
					Debug.Log ("Left clicked on cell #" + grid.CellGetIndex(grid.cellHighlighted));
				} else if (Input.GetMouseButtonDown(1)) {
					Debug.Log ("Right clicked on cell #" + grid.CellGetIndex(grid.cellHighlighted));
				}
			}
		}


	}
}
