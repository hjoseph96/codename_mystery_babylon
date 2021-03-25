using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Night {

	public static class Particle {
        static float sin, cos, angle;
		
        static public void DrawPass(Vector2 pos, Vector2 size, float angle) {
            angle = angle * Mathf.Deg2Rad + Mathf.PI;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            float cosx = size.x * cos;
            float sinx = size.x * sin;

            float cosy = size.y * cos;
            float siny = size.y * sin;

            GL.TexCoord3 (1, 1, 0);
            GL.Vertex3 (-cosx + siny + pos.x, -sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 1, 0);
            GL.Vertex3 (cosx + siny + pos.x, sinx - cosy + pos.y, 0);

            GL.TexCoord3 (0, 0, 0);
            GL.Vertex3 (cosx - siny + pos.x, sinx + cosy + pos.y, 0);

            GL.TexCoord3 (1, 0, 0);
            GL.Vertex3 (-cosx - siny + pos.x, -sinx + cosy + pos.y, 0);
		}
	}
}

/*
rot = rot * Mathf.Deg2Rad + Mathf.PI;

float rectAngle = Mathf.Atan2(size.y, size.x);
float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

v1.x = pos.x + Mathf.Cos(rectAngle + rot) * dist;
v1.y = pos.y + Mathf.Sin(rectAngle + rot) * dist;

v2.x = pos.x + Mathf.Cos(-rectAngle + rot) * dist;
v2.y = pos.y + Mathf.Sin(-rectAngle + rot) * dist;

v3.x = pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist;
v3.y = pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist;

v4.x = pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist;
v4.y = pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist;
*/

