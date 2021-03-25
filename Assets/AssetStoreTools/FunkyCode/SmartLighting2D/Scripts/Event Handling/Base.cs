using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventHandling {
	public class Base {
		public static Vector2 edgeLeft, edgeRight;
		public static Vector2 projectionLeft, projectionRight;
		public static Polygon2 eventPoly = null;

		static public Polygon2 GetPolygon() {
			if (eventPoly == null) {
				eventPoly = new Polygon2(4);
			}
			return(eventPoly);
		}
	}
}