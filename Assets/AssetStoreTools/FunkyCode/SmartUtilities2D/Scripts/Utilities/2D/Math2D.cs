using System.Collections.Generic;
using UnityEngine;
using System.Linq;


static public class Math2D {

	static public Vector2 ClosestPointOnLine(Vector2 vA, Vector2 vB) {
		float vx = (vB.x - vA.x);
		float vy = (vB.y - vA.y);
    
		float distance = Mathf.Sqrt(vx * vx + vy * vy);
		vx = vx / distance;
		vy = vy / distance;

		// Vector2.Dot(v, v1);
        float t = vx * -vA.x + vy * -vA.y;  
    
        if (t <= 0) {
            return vA;
        }

		float xs = vA.x - vB.x;
		float ys = vA.y - vB.y;

		bool dist = (t * t) >= (xs * xs + ys * ys);
		   
		// t >= Vector2.Distance(vA, vB)
        if (dist) { 
            return vB;
        }

		vA.x += vx * t;
		vA.y += vy * t;
        
        return vA;
    }

	static public Vector2 ClosestPointOnLine(Vector2 vPoint, Vector2 vA, Vector2 vB) {
		float vx = (vB.x - vA.x);
		float vy = (vB.y - vA.y);
    
		float distance = Mathf.Sqrt(vx * vx + vy * vy);
		vx = vx / distance;
		vy = vy / distance;
	
		float v1x = vPoint.x - vA.x;
		float v1y = vPoint.y - vA.y;

       	// Vector2.Dot(v, v1);
        float t = vx * v1x + vy * v1y;  
    
        if (t <= 0) {
            return vA;
        }
            
        float xs = vA.x - vB.x;
		float ys = vA.y - vB.y;

		bool dist = (t * t) >= (xs * xs + ys * ys);
		   
		// t >= Vector2.Distance(vA, vB)
        if (dist) { 
            return vB;
        }

		vA.x += vx * t;
		vA.y += vy * t;
        
        return vA;
    }

	public static Vector3 GetPitchYawRollRad(Quaternion rotation) {
		float roll = Mathf.Atan2(2*rotation.y*rotation.w - 2*rotation.x*rotation.z, 1 - 2*rotation.y*rotation.y - 2*rotation.z*rotation.z);
		float pitch = Mathf.Atan2(2*rotation.x*rotation.w - 2*rotation.y*rotation.z, 1 - 2*rotation.x*rotation.x - 2*rotation.z*rotation.z);
		float yaw = Mathf.Asin(2*rotation.x*rotation.y + 2*rotation.z*rotation.w);
		
		return new Vector3(pitch, roll, yaw);
	}

	public static Vector3 GetPitchYawRollDeg(Quaternion rotation) {
		Vector3 radResult = GetPitchYawRollRad(rotation);
		return new Vector3(radResult.x * Mathf.Rad2Deg, radResult.y * Mathf.Rad2Deg, radResult.z * Mathf.Rad2Deg);
	}

	public static Rect GetBounds(List<Vector2D> pointsList) {
		double rMinX = 1e+10f;
		double rMinY = 1e+10f;
		double rMaxX = -1e+10f;
		double rMaxY = -1e+10f;

		foreach (Vector2D id in pointsList) {
			rMinX = System.Math.Min (rMinX, id.x);
			rMinY = System.Math.Min (rMinY, id.y);
			rMaxX = System.Math.Max (rMaxX, id.x);
			rMaxY = System.Math.Max (rMaxY, id.y);
		}

		return(new Rect((float)rMinX, (float)rMinY, (float)System.Math.Abs(rMinX - rMaxX), (float)System.Math.Abs(rMinY - rMaxY))); 
	}

	public static Rect GetBounds(Pair2D pair) {
		double rMinX = 1e+10f;
		double rMinY = 1e+10f;
		double rMaxX = -1e+10f;
		double rMaxY = -1e+10f;

		Vector2D id = pair.A;
		rMinX = System.Math.Min (rMinX, id.x);
		rMinY = System.Math.Min (rMinY, id.y);
		rMaxX = System.Math.Max (rMaxX, id.x);
		rMaxY = System.Math.Max (rMaxY, id.y);
	
		id = pair.B;
		rMinX = System.Math.Min (rMinX, id.x);
		rMinY = System.Math.Min (rMinY, id.y);
		rMaxX = System.Math.Max (rMaxX, id.x);
		rMaxY = System.Math.Max (rMaxY, id.y);

		return(new Rect((float)rMinX, (float)rMinY, (float)System.Math.Abs(rMinX - rMaxX), (float)System.Math.Abs(rMinY - rMaxY))); 
	}


	public static bool PolyInPoly(Polygon2D polyA, Polygon2D polyB) {
		foreach (Pair2D p in Pair2D.GetList(polyB.pointsList)) {
			if (PointInPoly (p.A, polyA) == false) {
				return(false);
			}
		}

		if (PolyIntersectPoly (polyA, polyB) == true) {
			return(false);
		}
		
		return(true);
	}

	// Is it not finished?
	public static bool PolyCollidePoly(Polygon2D polyA, Polygon2D polyB) {
		if (PolyIntersectPoly (polyA, polyB) == true) {
			return(true);
		}

		if (PolyInPoly (polyA, polyB) == true) {
			return(true);
		}

		if (PolyInPoly (polyB, polyA) == true) {
			return(true);
		}
		
		return(false);
	}

	public static bool PolyIntersectPoly(Polygon2D polyA, Polygon2D polyB) {
		foreach (Pair2D a in Pair2D.GetList(polyA.pointsList)) {
			foreach (Pair2D b in Pair2D.GetList(polyB.pointsList)) {
				if (LineIntersectLine (a, b)) {
					return(true);
				}
			}
		}

		return(false);
	}

	public static bool SliceIntersectPoly(List <Vector2D> slice, Polygon2D poly) {
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D pointA in slice) {
			pairA.B = pointA;
			
			if (pairA.A != null && pairA.B != null) {
				Pair2D pairB = new Pair2D(new Vector2D(poly.pointsList.Last()),  null);
				foreach (Vector2D pointB in poly.pointsList) {
					pairB.B = pointB;

					if (LineIntersectLine (pairA, pairB)) {
						return(true);
					}

					pairB.A = pointB;
				}
			}

			pairA.A = pointA;
		}

		return(false);
	}

	public static bool SliceIntersectSlice(List <Vector2D> sliceA, List <Vector2D> sliceB) {
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D pointA in sliceA) {
			pairA.B = pointA;

			if (pairA.A != null && pairA.B != null) {

				Pair2D pairB = new Pair2D(null,  null);
				foreach (Vector2D pointB in sliceB) {
					pairB.B = pointB;

					if (pairB.A != null && pairB.B != null) {
						if (LineIntersectLine (pairA, pairB)) {
							return(true);
						}
					}

					pairB.A = pointB;
				}
			}

			pairA.A = pointA;
		}

		return(false);
	}
		
	public static bool LineIntersectPoly(Pair2D line, Polygon2D poly) {
		Pair2D pair = new Pair2D(new Vector2D(poly.pointsList.Last()), new Vector2D(Vector2.zero));
		foreach (Vector2D point in poly.pointsList) {
			pair.B = point;

			if (LineIntersectLine (line, pair)) {
				return(true);
			}
			
			pair.A = point;
		}
		
		return(false);
	}

	public static bool LineIntersectLine(Pair2D lineA, Pair2D lineB) {
		if (GetPointLineIntersectLine (lineA, lineB) != null) {
			return(true);
		}

		return(false);
	}

	public static bool SliceIntersectItself(List<Vector2D> slice) {
		Pair2D pairA = new Pair2D(null,  null);
		foreach (Vector2D va in slice) {
			pairA.B = va;

			if (pairA.A != null && pairA.B != null) {

				Pair2D pairB = new Pair2D(null,  null);
				foreach (Vector2D vb in slice) {
					pairB.B = vb;

					if (pairB.A != null && pairB.B != null) {
						if (GetPointLineIntersectLine (pairA, pairB) != null) {
							if (pairA.A != pairB.A && pairA.B != pairB.B && pairA.A != pairB.B && pairA.B != pairB.A) {
								return(true);
							}
						}
					}
					pairB.A = vb;
				}
			}
			
			pairA.A = va;
		}
		
		return(false);
	}

	public static Vector2D GetPointLineIntersectLine(Pair2D lineA, Pair2D lineB) {
		double ay_cy, ax_cx, px, py;
		double dx_cx = lineB.B.x - lineB.A.x;
		double dy_cy = lineB.B.y - lineB.A.y;
		double bx_ax = lineA.B.x - lineA.A.x;
		double by_ay = lineA.B.y - lineA.A.y;
		double de = bx_ax * dy_cy - by_ay * dx_cx;
		double tor = 1E-10;

		if (System.Math.Abs(de) < 0.0001d) {
			return(null);
		}	

		if (de > - tor && de < tor) {
			return(null);
		}

		ax_cx = lineA.A.x - lineB.A.x;
		ay_cy = lineA.A.y - lineB.A.y;

		double r = (ay_cy * dx_cx - ax_cx * dy_cy) / de;
		double s = (ay_cy * bx_ax - ax_cx * by_ay) / de;

		if ((r < 0) || (r > 1) || (s < 0)|| (s > 1)) {
			return(null);
		}

		px = lineA.A.x + r * bx_ax;
		py = lineA.A.y + r * by_ay;

		return(new Vector2D (px, py));
	}

	public static Vector2? GetPointLineIntersectLine2(Pair2D lineA, Pair2D lineB) {
		double ay_cy, ax_cx;
		double dx_cx = lineB.B.x - lineB.A.x;
		double dy_cy = lineB.B.y - lineB.A.y;
		double bx_ax = lineA.B.x - lineA.A.x;
		double by_ay = lineA.B.y - lineA.A.y;
		double de = bx_ax * dy_cy - by_ay * dx_cx;
		double tor = 1E-10;

		if (System.Math.Abs(de) < 0.0001d) {
			return(null);
		}	

		if (de > - tor && de < tor) {
			return(null);
		}

		ax_cx = lineA.A.x - lineB.A.x;
		ay_cy = lineA.A.y - lineB.A.y;

		double r = (ay_cy * dx_cx - ax_cx * dy_cy) / de;
		double s = (ay_cy * bx_ax - ax_cx * by_ay) / de;

		if ((r < 0) || (r > 1) || (s < 0)|| (s > 1)) {
			return(null);
		}

		float px = (float)(lineA.A.x + r * bx_ax);
		float py = (float)(lineA.A.y + r * by_ay);

		return(new Vector2 (px, py));
	}

	const double tor = 1E-10;

	public static Vector2? GetPointLineIntersectLine3(Vector2 a_a, Vector2 a_b, Vector2 b_a, Vector2 b_b) {
		double dx_cx = b_b.x - b_a.x;
		double dy_cy = b_b.y - b_a.y;
		double bx_ax = a_b.x - a_a.x;
		double by_ay = a_b.y - a_a.y;
		double de = bx_ax * dy_cy - by_ay * dx_cx;

		if (System.Math.Abs(de) < 0.0001d) {
			return(null);
		}	

		if (de > - tor && de < tor) {
			return(null);
		}

		double ax_cx = a_a.x - b_a.x;
		double ay_cy = a_a.y - b_a.y;

		double r = (ay_cy * dx_cx - ax_cx * dy_cy) / de;
		double s = (ay_cy * bx_ax - ax_cx * by_ay) / de;

		if ((r < 0) || (r > 1) || (s < 0)|| (s > 1)) {
			return(null);
		}

		return(new Vector2 ((float)(a_a.x + r * bx_ax),  (float)(a_a.y + r * by_ay)));
	}

	public static bool GetPointLineIntersectLine4(Vector2 a_a, Vector2 a_b, Vector2 b_a, Vector2 b_b) {
		double dx_cx = b_b.x - b_a.x;
		double dy_cy = b_b.y - b_a.y;
		double bx_ax = a_b.x - a_a.x;
		double by_ay = a_b.y - a_a.y;
		double de = bx_ax * dy_cy - by_ay * dx_cx;

		if (System.Math.Abs(de) < 0.0001d) {
			return(false);
		}	

		if (de > - tor && de < tor) {
			return(false);
		}

		double ax_cx = a_a.x - b_a.x;
		double ay_cy = a_a.y - b_a.y;

		double r = (ay_cy * dx_cx - ax_cx * dy_cy) / de;
		double s = (ay_cy * bx_ax - ax_cx * by_ay) / de;

		if ((r < 0) || (r > 1) || (s < 0)|| (s > 1)) {
			return(false);
		}

		return(true);
	}





	private static Pair2D pointInPoly_Pair2D = Pair2D.Zero();
	public static bool PointInPoly(Vector2D point, Polygon2D poly) {
		int count = poly.pointsList.Count;
		if (count < 3) {
			return(false);
		}

		int total = 0;
		int diff = 0;

		Pair2D id = pointInPoly_Pair2D;
		id.A = poly.pointsList[count - 1];

		Vector2D p;
		
		for(int i = 0; i < count; i++) {
			p = poly.pointsList[i];
			
			id.B = p;

			diff = (GetQuad (point, id.A) - GetQuad (point, id.B));

			switch (diff) {
				case -2: case 2:
					if ((id.B.x - (((id.B.y - point.y) * (id.A.x - id.B.x)) / (id.A.y - id.B.y))) < point.x)
						diff = -diff;

					break;

				case 3:
					diff = -1;
					break;

				case -3:
					diff = 1;
					break;

				default:
					break;   
			}

			total += diff;

			id.A = id.B;
		}

		return(System.Math.Abs(total) == 4);
	}

	private static int GetQuad(Vector2D axis, Vector2D vert) {
		if (vert.x < axis.x) {
			if (vert.y < axis.y) {
				return(1);
			}
			return(4);
		}
		if (vert.y < axis.y) {
			return(2);
		}
		return(3);
	}





	public static bool PointInPoly(Vector2 point, Polygon2 poly) {
		int count = poly.points.Length;
		if (count < 3) {
			return(false);
		}

		int total = 0;
		int diff = 0;

		Vector2 A = poly.points[count - 1];
		Vector2 B;

		for(int i = 0; i < count; i++) {
			B = poly.points[i];

			int vertARes = 0;
			int vertBRes = 0;

			if (A.x < point.x) {
				if (A.y < point.y) {
					vertARes = 1;
				} else {
					vertARes = 4;
				}
				
			} else {
				if (A.y < point.y) {
					vertARes = 2;
				} else {
					vertARes = 3;
				}
			}

			if (B.x < point.x) {
				if (B.y < point.y) {
					vertBRes = 1;
				} else {
					vertBRes = 4;
				}
				
			} else {
				if (B.y < point.y) {
					vertBRes = 2;
				} else {
					vertBRes = 3;
				}
			}

			diff = vertARes - vertBRes;

			switch (diff) {
				case -2: 
				case 2:
					if ((B.x - (((B.y - point.y) * (A.x - B.x)) / (A.y - B.y))) < point.x)
						diff = -diff;

					break;

				case 3:
					diff = -1;
					break;

				case -3:
					diff = 1;
					break;

				default:
					break;   
			}

			total += diff;

			A = B;
		}

		return(total == 4 || total == -4);
	}

	private static int GetQuad(Vector2 axis, Vector2 vertA, Vector2 vertB) {
		int vertARes = 0;
		int vertBRes = 0;

		if (vertA.x < axis.x) {
			if (vertA.y < axis.y) {
				vertARes = 1;
			} else {
				vertARes = 4;
			}
			
		} else {
			if (vertA.y < axis.y) {
				vertARes = 2;
			} else {
				vertARes = 3;
			}
		}

		if (vertB.x < axis.x) {
			if (vertB.y < axis.y) {
				vertBRes = 1;
			} else {
				vertBRes = 4;
			}
			
		} else {
			if (vertB.y < axis.y) {
				vertBRes = 2;
			} else {
				vertBRes = 3;
			}
		}

		return(vertARes - vertBRes);
		
	}

	/*
	private static int GetQuad(Vector2 axis, Vector2 vert) {
		if (vert.x < axis.x) {
			if (vert.y < axis.y) {
				return(1);
			}
			return(4);
		}
		if (vert.y < axis.y) {
			return(2);
		}
		return(3);
	}


	*/

	





























		
	// Getting List is Slower
	public static List <Vector2D> GetListLineIntersectPoly(Pair2D line, Polygon2D poly) {
		List <Vector2D> result = new List <Vector2D>() ;

		Pair2D pair = new Pair2D(new Vector2D(poly.pointsList.Last()),  null);
		foreach (Vector2D point in poly.pointsList) {
			pair.B = point;

			Vector2D intersection = GetPointLineIntersectLine (line, pair);
			if (intersection != null) {
				result.Add(intersection);
			}

			pair.A = point;
		}
		return(result);
	}

	public static List<Vector2D> GetListLineIntersectSlice(Pair2D pair, List<Vector2D> slice) {
		List<Vector2D> resultList = new List<Vector2D> ();
		
		Pair2D id = new Pair2D(null,  null);
		foreach (Vector2D point in slice) {
			id.B = point;

			if (id.A != null && id.B != null) {
				Vector2D result = GetPointLineIntersectLine(id, pair);
				if (result != null) {
					resultList.Add(result);
				}
			}

			id.A = point;
		}
		return(resultList);
	}

	public static Vector2 ReflectAngle(Vector2 v, float wallAngle) {
		//normal vector to the wall
		Vector2 n = new Vector2(Mathf.Cos(wallAngle + Mathf.PI / 2), Mathf.Sin(wallAngle + Mathf.PI / 2));

		// p is the projection of V onto the normal
		float dotproduct = v.x * n.x + v.y * n.y;

		// the velocity after hitting the wall is V - 2p, so just subtract 2*p from V
		return(new Vector2(v.x - 2f * (dotproduct * n.x), v.y - 2f * (dotproduct * n.y)));
	}

	static public bool PolygonIntersectCircle(Polygon2D poly, Vector2D circle, float radius) {
		foreach (Pair2D id in Pair2D.GetList(poly.pointsList)) {
			if (LineIntersectCircle(id, circle, radius) == true) {
				return(true);
			}
		}
		return(false);
	}
	
	static public bool SliceIntersectCircle(List<Vector2D> points, Vector2D circle, float radius) {
		foreach (Pair2D id in Pair2D.GetList(points, false)) {
			if (LineIntersectCircle(id, circle, radius) == true) {
				return(true);
			}
		}
		return(false);
	}

	static public bool LineIntersectCircle(Pair2D line, Vector2D circle, float radius) {
		double sx = line.B.x - line.A.x;
		double sy = line.B.y - line.A.y;

		double q = ((circle.x - line.A.x) * (line.B.x - line.A.x) + (circle.y - line.A.y) * (line.B.y - line.A.y)) / (sx * sx + sy * sy);
			
		if (q < 0.0f) {
			q = 0.0f;
		} else if (q > 1.0) {
			q = 1.0f;
		}

		double dx = circle.x - ((1.0f - q) * line.A.x + q * line.B.x);
		double dy = circle.y - ((1.0f - q) * line.A.y + q * line.B.y);

		if (dx * dx + dy * dy < radius * radius) {
			return(true);
		} else {
			return(false);
		}
	}


	public static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p) {
		return ((a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x));
	}

}
