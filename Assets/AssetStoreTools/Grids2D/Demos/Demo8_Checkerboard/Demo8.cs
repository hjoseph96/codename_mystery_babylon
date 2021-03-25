using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{
	public class Demo8 : MonoBehaviour
	{

		Grid2D grid;
		GUIStyle labelStyle;
		GameObject piece;

		void Start ()
		{
			// setup GUI styles
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.black;

			// Get a reference to Grids 2D System's API
			grid = Grid2D.instance;

			// create checker board
			CreateBoard ();

			// create piece sprite and move it to center position
			CreatePiece ();

			// listen to click events
			grid.OnCellClick += (int cellIndex) => MovePiece (cellIndex);

		}

		void OnGUI ()
		{
			GUI.Label (new Rect (0, 5, Screen.width, 30), "Left mouse button = move piece to target cell.", labelStyle);
		}

		/// <summary>
		/// Creates a classic black and white checkers board
		/// </summary>
		void CreateBoard ()
		{
			Texture2D whiteCell = Resources.Load<Texture2D> ("Textures/cellWhite"); 
			Texture2D blackCell = Resources.Load<Texture2D> ("Textures/cellBlack"); 
			
			grid.gridTopology = GRID_TOPOLOGY.Box;
			grid.rowCount = 8;
			grid.columnCount = 8;
			
			bool even = false;
			for (int row=0; row<grid.rowCount; row++) {
				even = !even;
				for (int col=0; col<grid.columnCount; col++) {
					even = !even;
					Cell cell = grid.CellGetAtPosition (col, row);
					int cellIndex = grid.CellGetIndex (cell);
					if (even) {
						grid.CellToggle (cellIndex, true, whiteCell);
					} else {
						grid.CellToggle (cellIndex, true, blackCell);
					}
				}
			}
		}

		/// <summary>
		/// Creates the game object for the piece -> loads the texture, creates an sprite, assigns the texture and position the game object in world space over the center cell.
		/// </summary>
		void CreatePiece ()
		{
			// Creates the piece
			Sprite sprite = Resources.Load<Sprite> ("Sprites/piece");
			piece = new GameObject ("Piece");
			SpriteRenderer spriteRenderer = piece.AddComponent<SpriteRenderer> ();
			spriteRenderer.sprite = sprite;
			
			// Parents the piece to the grid and sets scale
			piece.transform.SetParent (grid.transform, false);
			piece.transform.localScale = Vector3.one * 0.25f;

			// Get central cell of checker board
			Cell centerCell = grid.CellGetAtPosition (4, 4);
			int centerCellIndex = grid.CellGetIndex (centerCell);
			grid.MoveTo(piece, centerCellIndex, 0);
		}


		/// <summary>
		/// Moves the piece to the center position of the cell specified by cell index.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		void MovePiece (int destinationCellIndex)
		{
			// Gets current cell under piece
			Cell currentCell = grid.CellGetAtPosition (piece.transform.position, true);
			int currentCellIndex = grid.CellGetIndex (currentCell);

			// Obtain a path from current cell to destination
			List<int> positions = grid.FindPath (currentCellIndex, destinationCellIndex);

			// Move along those positions
			grid.MoveTo (piece, positions, 0.5f, 0f).OnMoveEnd += (gameObject) => { Debug.Log("Piece moved to " + grid.cells[destinationCellIndex].coordinates); };

		}


	}
}
