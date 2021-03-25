using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D
{

	public delegate void OnMoveEvent(GameObject gameObject);

	public class Grid2DMove : MonoBehaviour
	{

		public Grid2D grid;
		public List<int> positions;
		public float duration;
		public float elevation;
		public event OnMoveEvent OnMoveEnd;

		float startTime;
		Vector3 startPosition, destination;
		int posIndex;

		void Start ()
		{
			posIndex = 0;
			ComputeNextDestination ();
		}

		void Update ()
		{
			float t = duration > 0 ? (Time.time - startTime) / duration : 1f;
			t = Mathf.Clamp01 (t);
			transform.position = Vector3.Lerp (startPosition, destination, t);
			if (t >= 1) {
				ComputeNextDestination();
			}
		}

		void ComputeNextDestination ()
		{
			startTime = Time.time;
			startPosition = transform.position;
			if (positions == null || posIndex >= positions.Count) {
				if (OnMoveEnd != null) OnMoveEnd(gameObject);
				Destroy (this);
			} else {
				destination = grid.CellGetPosition (positions [posIndex], elevation);
				posIndex++;
			}
		}
	}
}