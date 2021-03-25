using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Grids2D.PathFinding;

namespace Grids2D {

	public partial class Grid2D : MonoBehaviour {

		IPathFinder finder;
		bool needRefreshRouteMatrix;

		void ComputeRouteMatrix () {

			if (!needRefreshRouteMatrix)
				return;

			bool createFinder = finder == null || finder.GridX != _cellColumnCount || finder.GridY != _cellRowCount;
			if (createFinder) {
				if ((_cellColumnCount & (_cellColumnCount - 1)) == 0) {	// is power of two?
					finder = new PathFinderFast (cells.ToArray (), _cellColumnCount, _cellRowCount);
				} else {
					finder = new PathFinderFastNonSQR (cells.ToArray (), _cellColumnCount, _cellRowCount);
				}
			} else {
				finder.SetCalcMatrix (cells.ToArray ());
			}
			needRefreshRouteMatrix = false;
		}

		/// <summary>
		/// Used by FindRoute method to satisfy custom positions check
		/// </summary>
		float FindRoutePositionValidator (int location) {
			float cost = 1;
			if (OnPathFindingCrossCell != null) {
				cost = OnPathFindingCrossCell (location);
			}
			return cost;
		}

	}

}