using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Rendering.Light.Shadow {

    public static class LegacyGPU {

        public static Pair2 pair = Pair2.Zero();
        public static Vector2 edgeAWorld, edgeBWorld;

        public static void Draw(List<Polygon2> polygons, float distance, float translucency) {
            if (polygons == null) {
                return;
            }

            Light2D light = ShadowEngine.light;

            Vector2 position = ShadowEngine.lightOffset;
            position.x += ShadowEngine.objectOffset.x;
            position.y += ShadowEngine.objectOffset.y; 

            float outerAngle = Mathf.Deg2Rad * light.outerAngle;
            float shadowDistance = ShadowEngine.lightSize;
  
            int PolygonCount = polygons.Count;

            Vector2 draw = ShadowEngine.drawOffset;

            if (translucency > 0) {
                outerAngle = 0;
            }

            if (distance > 0) {
                shadowDistance = distance;
                outerAngle = 0;
            }

            for(int i = 0; i < PolygonCount; i++) {
                Vector2[] pointsList = polygons[i].points;
                int pointsCount = pointsList.Length;
            
                for(int x = 0; x < pointsCount; x++) {
                    int next = (x + 1) % pointsCount;
                    
                    pair.A = pointsList[x];
                    pair.B = pointsList[next];
 
                    edgeAWorld.x = pair.A.x + position.x;
                    edgeAWorld.y = pair.A.y + position.y;

                    edgeBWorld.x = pair.B.x + position.x;
                    edgeBWorld.y = pair.B.y + position.y;
    
                    GL.Vertex3(draw.x + edgeAWorld.x, draw.y + edgeAWorld.y, 0);
                    GL.Vertex3(draw.x + edgeBWorld.x, draw.y + edgeBWorld.y, 0);
                    GL.Vertex3(shadowDistance, outerAngle, translucency); 
                }
            }
        }
    }
}