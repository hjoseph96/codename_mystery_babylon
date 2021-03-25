using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Grids2D.Geom {
	public class VoronoiCell {
		public List <Segment> segments;
		public Point center;
		public List<Point>top, left, bottom, right; // for cropping
		static Connector connector;

		public VoronoiCell (Point center) {
			segments = new List<Segment> (16);
			this.center = center;
			left = new List<Point> ();
			top = new List<Point> ();
			bottom = new List<Point> ();
			right = new List<Point> ();
		}

		public void Init(Point center) {
			segments.Clear ();
			this.center = center;
			left.Clear ();
			top.Clear ();
			bottom.Clear ();
			right.Clear ();
		}

		public Polygon GetPolygon (int edgeSubdivisions, float curvature) {
			if (connector == null) {
				connector = new Connector ();
			} else {
				connector.Clear ();
			}
			int count = segments.Count;
			for (int k=0; k<count; k++) {
				Segment s = segments [k];
				if (!s.deleted) {
					if (edgeSubdivisions>1) {
						connector.AddRange (s.Subdivide(edgeSubdivisions, curvature));
					} else {
						connector.Add (s);
					}
				}
			}
			return connector.ToPolygonFromLargestLineStrip ();
		}

	
		public Point centroid {
			get {
				Point point = Point.zero;
				int count=0;
				int segmentsCount = segments.Count;
				for (int k=0;k<segmentsCount;k++) {
					Segment s = segments[k];
					if (!s.deleted) {
						point += segments[k].start;
						point += segments[k].end;
						count+=2;
					}
				}
				if (count>0) point /= count;
				return point;
			}
		}

	}

}
