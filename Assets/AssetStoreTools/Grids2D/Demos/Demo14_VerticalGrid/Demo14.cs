using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{
	public class Demo14 : MonoBehaviour
	{
		
		Grid2D grid;
		public Texture2D[] fruits;
		GUIStyle labelStyle;

		void Start ()
		{
			// setup GUI - only for the demo
			GUIResizer.Init (800, 500); 
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;

			// Get a reference to Grids 2D System's API
			grid = Grid2D.instance;
			for (int k=0; k<grid.numCells; k++) {
				Texture2D fruitTexture = fruits [Random.Range (0, fruits.Length)];

				Vector2 textureOffset = Vector2.zero; 
				Vector2 textureScale = new Vector2 (1f, 0.6f); // to keep some aspect ratio
				float rotationDegrees = -90f;

				grid.CellToggle (k, true, Color.white, false, fruitTexture, textureScale, textureOffset, rotationDegrees, true);
			}

		}


	}
}
