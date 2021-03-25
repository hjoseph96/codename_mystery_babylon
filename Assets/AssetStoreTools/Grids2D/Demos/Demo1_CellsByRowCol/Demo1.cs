using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{
    public class Demo1 : MonoBehaviour
    {

        Grid2D grid;
        int currentRow, currentCol;

        void Start()
        {
            // Get a reference to Grids 2D System's API
            grid = Grid2D.instance;
            currentRow = 0;
            currentCol = 0;

            StartCoroutine(HighlightCell());

            //			// Benchmark test
            //			grid.rowCount = 50;
            //			grid.columnCount = 100;
            //			Debug.Log ("First draw Start: " + System.DateTime.Now.ToString("hh.mm.ss.ffffff"));
            //			for (int k=0;k<grid.cells.Count;k++) {
            //				grid.CellToggle(k, true, Color.red);
            //			}
            //			Debug.Log ("First draw End: " + System.DateTime.Now.ToString("hh.mm.ss.ffffff"));
            //			Debug.Log ("Second draw Start: " + System.DateTime.Now.ToString("hh.mm.ss.ffffff"));
            //			for (int k=0;k<grid.cells.Count;k++) {
            //				grid.CellToggle(k, true, Color.yellow);
            //			}
            //			Debug.Log ("Second draw End: " + System.DateTime.Now.ToString("hh.mm.ss.ffffff"));

        }

        // Highlight cells sequentially on each frame
        IEnumerator HighlightCell()
        {
            currentCol++;
            // run across the grid row by row
            if (currentCol >= grid.columnCount)
            {
                currentCol = 0;
                currentRow++;
                if (currentRow >= grid.rowCount)
                    currentRow = 0;
            }
            // get cell at current grid position and color it with fade out option
            Cell cell = grid.CellGetAtPosition(currentCol, currentRow);
            if (cell != null)
            {
                int cellIndex = grid.CellGetIndex(cell);
                float duration = Random.value * 2.5f + 0.5f;
                Color color = new Color(Random.value, Random.value, Random.value);
                grid.CellFadeOut(cellIndex, color, duration);
            }

            // trigger next iteration after this frame
            yield return new WaitForEndOfFrame();
            StartCoroutine(HighlightCell());
        }


    }
}
