using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D.Geom {
	class PointChain {
		public bool closed;
		public List<Point> pointList;

//		static int maxLen = 0;

		public PointChain(Segment s) {
			pointList = new List<Point>(50);
			pointList.Add(s.start);
			pointList.Add(s.end);
			closed = false;
		}
		
		// Links a segment to the pointChain
		public bool LinkSegment(Segment s) {
			Point front = pointList[0];
			Point back = pointList[pointList.Count - 1];
			
			if (Point.EqualsBoth(s.start, front)) {
				if (Point.EqualsBoth(s.end, back))
					closed = true;
				else
					pointList.Insert(0,s.end);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
				return true;
			} else if (Point.EqualsBoth(s.end, back)) {
				if (Point.EqualsBoth(s.start, front))
					closed = true;
				else
					pointList.Add(s.start);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
				return true;
			} else if (Point.EqualsBoth(s.end, front)) {
				if (Point.EqualsBoth(s.start, back))
					closed = true;
				else
					pointList.Insert (0,s.start);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
				return true;
			} else if (Point.EqualsBoth(s.start, back)) {
				if (Point.EqualsBoth(s.end, front))
					closed = true;
				else
					pointList.Add(s.end);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}

				return true;
			}
			
			return false;
		}
		
		// Links another pointChain onto this point chain.
		public bool LinkPointChain(PointChain chain) {
			Point firstPoint = pointList[0];
			Point lastPoint = pointList[pointList.Count - 1];
			
			Point chainFront = chain.pointList[0];
			Point chainBack = chain.pointList[chain.pointList.Count - 1];
			
			if (Point.EqualsBoth(chainFront, lastPoint)) {
				List<Point>temp = new List<Point>(chain.pointList.Count);
				for (int k=1;k<chain.pointList.Count;k++) temp.Add (chain.pointList[k]);
//				temp.RemoveAt(0);
				pointList.AddRange(temp);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
//				chain.pointList.Clear();
				return true;
			}
			
			if (Point.EqualsBoth(chainBack, firstPoint)) {
//				pointList.RemoveAt (0); // Remove the first element, and join this list to chain.pointList.
				List<Point>temp = new List<Point>(chain.pointList);
				temp.Capacity += pointList.Count;
				for (int k=1;k<pointList.Count;k++) temp.Add (pointList[k]);
//				temp.AddRange(pointList);
				pointList = temp;
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
//				chain.pointList.Clear();
				return true;
			}
			
			if (Point.EqualsBoth(chainFront, firstPoint)) {
//				pointList.RemoveAt (0); // Remove the first element, and join to reversed chain.pointList
				List<Point>temp = new List<Point>(chain.pointList);
				temp.Reverse();
				temp.Capacity += pointList.Count;
				for (int k=1;k<pointList.Count;k++) temp.Add (pointList[k]);
//				temp.AddRange(pointList);
				pointList = temp;
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
//				chain.pointList.Clear();
				return true;
			}
			
			if (Point.EqualsBoth(chainBack, lastPoint)) {
				pointList.RemoveAt (pointList.Count-1);
				List<Point>temp = new List<Point>(chain.pointList);
				temp.Reverse();
				pointList.AddRange(temp);
//				if (pointList.Count>maxLen) {
//					maxLen = pointList.Count;
//					Debug.Log (maxLen);
//				}
//				chain.pointList.Clear();
				return true;
			}
			return false;
		}
		
	}

}