using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{
	public class Demo17 : MonoBehaviour
	{
		
		Grid2D grid;
		public Sprite sprite;
		
		void Start ()
		{
			// Get a reference to Grids 2D System's API
			grid = Grid2D.instance;
			for (int k=0;k<100;k++) {
				int cellIndex = Random.Range(0, grid.numCells);
				Color color = new Color(Random.value, Random.value, Random.value);
				grid.CellSetSprite (cellIndex, color, sprite);
			}

		}

	}
}
